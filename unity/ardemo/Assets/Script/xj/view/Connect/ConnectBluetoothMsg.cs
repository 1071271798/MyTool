using Game;
using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Resource;
/// <summary>
/// Author:xj
/// FileName:ConnectBluetoothMsg.cs
/// Description:连接蓝牙页面
/// Time:2016/12/2 11:21:36
/// </summary>
public class ConnectBluetoothMsg : BasePopWin
{
    #region 公有属性

    enum ConnectMsgType : byte
    {
        Connect_Msg_None,
        Connect_Msg_Search_Ready,
        Connect_Msg_Search,
        Connect_Msg_Search_Fail,
        Connect_Msg_Select,
        Connect_Msg_Into_Select_Ing,
        Connect_Msg_Connecting,
        Connect_Msg_Connect_Fail,
    }

    EventDelegate.Callback  onCloseCallBack;
    EventDelegate.Callback onHelpCallback;
    #endregion

    #region 其他属性
    static ConnectBluetoothMsg sInst;
    ConnectMsgType mMsgType = ConnectMsgType.Connect_Msg_None;
    Vector3 mPhonePosition = Vector3.zero;
    UITexture[] mSearchTexture = null;
    bool mSearchUpdateFlag = false;
    float mSearchTime = 0;

    long mSearchBlueIndex = -1;
    long mCheckSearchIndex = -1;
    long mCheckConnectIndex = -1;

    List<DeviceInfo> mCacheDeviceList = new List<DeviceInfo>();
    Dictionary<GameObject, DeviceInfo> mDeviceDict = new Dictionary<GameObject, DeviceInfo>();
    Dictionary<string, GameObject> mDeviceForMacDict = new Dictionary<string, GameObject>();

    Vector3 mSelectBluePosition = Vector3.zero;

    GameObject mBlueItem = null;
    Transform mBlueGridTrans = null;
    Vector2 mBlueItemSize = Vector2.one;
    GameObject mConnectingObj;

    Dictionary<Transform, EventDelay.TimeData> mCallBackDict = new Dictionary<Transform, EventDelay.TimeData>();
    EventDelay mEventDelay = new EventDelay();


    int Point_Num = 8;
    float Point_R = 18.0f;
    List<UISprite> mSpriteList = null;
    GameObject mLoadingObj = null;
    bool mLoadingUpdateFlag = false;
    float mLoadingUpdateTime = 0;

    bool mSearchFlag = false;//加这个标记是因为android还没开始调扫描的接口就自动扫描了
    #endregion

    #region 公有函数

    public ConnectBluetoothMsg()
    {
        mUIResPath = "Prefab/UI/ConnectBluetoothMsg";
        sInst = this;
        isSingle = true;
        SingletonObject<ConnectCtrl>.GetInst().IsReconnectFlag = false;
    }

    public ConnectBluetoothMsg(EventDelegate.Callback callBack)
    {
        mUIResPath = "Prefab/UI/ConnectBluetoothMsg";
        sInst = this;
        onCloseCallBack = callBack;
        SingletonObject<ConnectCtrl>.GetInst().IsReconnectFlag = false;
    }

    public static void ShowMsg()
    {
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "未断开连接进入蓝牙连接页面");
            PlatformMgr.Instance.DisConnenctBuletooth();
        }
        if (null == sInst)
        {
            SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(ConnectBluetoothMsg), null, 1, null, false);
        }
        else
        {
            sInst.OnShow();
        }
    }

    public static void ShowMsg(EventDelegate.Callback closeCallBack)
    {
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "未断开连接进入蓝牙连接页面");
            PlatformMgr.Instance.DisConnenctBuletooth();
        }
        if (null == sInst)
        {
            object[] args = new object[1];
            args[0] = closeCallBack;
            SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(ConnectBluetoothMsg), args, 1, null, false);
        }
        else
        {
            sInst.OnShow();
        }
    }

    public static EventDelegate.Callback GetCloseCallback()
    {
        if (null != sInst && null != sInst.onCloseCallBack)
        {
            return sInst.onCloseCallBack;
        }
        return null;
    }

    public static void ClearCloserCallback()
    {
        if (null != sInst && null != sInst.onCloseCallBack)
        {
            sInst.onCloseCallBack = null;
        }
    }

    public static void CloseMsg()
    {
        if (null != sInst)
        {
            sInst.OnClose();
        }
    }

    public static void ConnectFail(string text)
    {
        if (null != sInst)
        {
            if (-1 != sInst.mCheckConnectIndex)
            {
                Timer.Cancel(sInst.mCheckConnectIndex);
                sInst.mCheckConnectIndex = -1;
            }
            sInst.mConnectingObj = null;
            if (sInst.mMsgType == ConnectMsgType.Connect_Msg_Connecting)
            {
                sInst.IntoMsgState(ConnectMsgType.Connect_Msg_Connect_Fail);
                if (RobotManager.GetInst().IsSetDeviceIDFlag)
                {
                    PromptMsg.ShowSinglePrompt(text, sInst.ConnectOnClick,null, delegate(BasePopWin popMsg) {
                        if (popMsg is PromptMsg)
                        {
                            PromptMsg msg = (PromptMsg)popMsg;
                            msg.SetRightBtnText(LauguageTool.GetIns().GetText("重试"));
                        }
                    });
                    
                } else
                {
                    sInst.ShowHelpBtn(delegate ()
                    {
                        if (SingletonBehaviour<ClientMain>.GetInst().useThirdAppFlag)
                        {
                            PlatformMgr.Instance.PopWebErrorType(ConnectionErrorType.ConnectionFailed);
                        }
                        else
                        {
                            PromptMsg.ShowSinglePrompt(text, null, null, delegate (BasePopWin popMsg)
                            {
                                if (popMsg is PromptMsg)
                                {
                                    PromptMsg msg = (PromptMsg)popMsg;
                                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("重试"));
                                }
                            });
                        }
                    });
                }
            }
        }
    }

    public override void Release()
    {
        base.Release();
        if (-1 != mCheckSearchIndex)
        {
            Timer.Cancel(mCheckSearchIndex);
            mCheckSearchIndex = -1;
        }
        if (-1 != mSearchBlueIndex)
        {
            Timer.Cancel(mSearchBlueIndex);
            mSearchBlueIndex = -1;
        }
        if (-1 != mCheckConnectIndex)
        {
            Timer.Cancel(mCheckConnectIndex);
            mCheckConnectIndex = -1;
        }
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_ON_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_ON_MATCHED_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnConnectResult);
        sInst = null;
        mCallBackDict.Clear();
        mCallBackDict = null;
        if (null != onCloseCallBack)
        {
            onCloseCallBack();
            onCloseCallBack = null;
        }
    }

    public override void Update()
    {
        base.Update();
        if (isShow)
        {
            if (mSearchUpdateFlag && null != mSearchTexture)
            {
                mSearchTime += Time.deltaTime;
                for (int i = 0, imax = mSearchTexture.Length; i < imax; ++i)
                {
                    if (mSearchTime >= i)
                    {
                        float size = 1 + 1.8f * ((mSearchTime - i) % 5);
                        mSearchTexture[i].width = (int)(160 * size);
                        mSearchTexture[i].height = (int)(160 * size);
                        mSearchTexture[i].alpha = 0.8f - ((mSearchTime - i) % 5) / 5;
                    }
                }
            }
            if (mLoadingUpdateFlag && mMsgType == ConnectMsgType.Connect_Msg_Select && null != mSpriteList)
            {
                mLoadingUpdateTime += Time.deltaTime;
                for (int i = 0, imax = mSpriteList.Count; i < imax; ++i)
                {
                    float tmp = Time.deltaTime + mSpriteList[i].alpha;
                    if (tmp > 1)
                    {
                        tmp -= 1;
                    }
                    mSpriteList[i].alpha = tmp;
                }
                if (mLoadingUpdateTime > 20)
                {
                    mLoadingUpdateFlag = false;
                    mLoadingUpdateTime = 0;
                    StopSearchBlue();
                    if (null != mLoadingObj)
                    {
                        mLoadingObj.SetActive(false);
                    }
                }
            }
        }
    }


    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isShow)
        {
            mEventDelay.Update();
        }
    }

    public override void Init()
    {
        base.Init();
        mCoverAlpha = 1;
        mCoverColor = PublicFunction.Connect_bg_Color;
    }
    #endregion

    #region 其他函数

    protected override void AddEvent()
    {
        base.AddEvent();
        //永不待机
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        EventMgr.Inst.Regist(EventID.BLUETOOTH_ON_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_ON_MATCHED_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnConnectResult);
        try
        {
            if (null != mTrans)
            {
                Transform top = mTrans.Find("top");
                if (null != top)
                {
                    GameHelper.SetPosition(top.Find("btnBack"), UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos);
                    
                    Transform title = top.Find("title");
                    if (null != title)
                    {
                        GameHelper.SetPosition(title, UIWidget.Pivot.Top, new Vector2(0, PublicFunction.Title_Margin_1));
                        Transform subtitle = title.Find("subtitle");
                        if (null != subtitle)
                        {
                            subtitle.localPosition = new Vector3(0, title.localPosition.y - 40, 0);
                        }
                    }

                    UITexture icon = mTrans.Find("center/icon/icon").gameObject.GetComponent<UITexture>();
                    if (null != icon)
                    {
                        if (string.IsNullOrEmpty(PlatformMgr.Instance.GetRobotTexturePath()))
                        {
                            mTrans.Find("center/icon/Sprite").gameObject.SetActive(true);
                        }
                        else
                        {
                            SingletonBehaviour<ResourcesLoad>.GetInst().Load(PlatformMgr.Instance.GetRobotTexturePath(), typeof(Texture), delegate (object arg)
                            {
                                if (null != arg && arg.GetType() == typeof(Texture2D))
                                {
                                    icon.mainTexture = (Texture2D)arg;
                                }
                                if (null != icon.mainTexture)
                                {
                                    PublicFunction.SetCircularTexture(icon, PublicFunction.Search_Model_Icon_Size);
                                }
                            });
                        }
                    }
                    Transform btnRefresh = top.Find("btnRefresh");
                    if (null != btnRefresh)
                    {
                        GameHelper.SetPosition(btnRefresh, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos);
                        btnRefresh.gameObject.SetActive(false);
                    }
                    Transform btnHelp = top.Find("btnHelp");
                    if (null != btnHelp)
                    {
                        GameHelper.SetPosition(btnHelp, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos);
                        btnHelp.gameObject.SetActive(false);
                    }
                }

                Transform center = mTrans.Find("center");
                if (null != center)
                {
                    Transform circle = center.Find("circle");
                    if (null != circle)
                    {
                        mSearchTexture = new UITexture[5];
                        for (int i = 1; i <= 5; ++i)
                        {
                            mSearchTexture[i-1] = circle.Find("circle" + i).GetComponent<UITexture>();
                        }
                    }                    
                }

                Transform bottom = mTrans.Find("bottom");
                if (null != bottom)
                {
                    
                    Transform phone = bottom.Find("phone");
                    if (null != phone)
                    {
                        mPhonePosition = UIManager.GetWinPos(phone, UIWidget.Pivot.Bottom);
                        phone.gameObject.SetActive(false);
                    }

                    Transform searchBtn = bottom.Find("searchBtn");
                    if (null != searchBtn)
                    {
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(searchBtn, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("点击搜索");
                        }
                        searchBtn.localPosition = UIManager.GetWinPos(searchBtn, UIWidget.Pivot.Bottom, 0, PublicFunction.Bottom_Margin_x);
                        //GameHelper.SetButtonCentered(searchBtn);
                        searchBtn.gameObject.SetActive(false);
                    }

                    Transform blueList = bottom.Find("blueList");
                    if (null != blueList)
                    {
                        Transform loading = blueList.Find("loading");
                        if (null != loading)
                        {
                            mLoadingObj = loading.gameObject;
                            mSpriteList = new List<UISprite>();
                            for (int i = 0; i < Point_Num; ++i)
                            {
                                Transform icon = loading.Find("icon" + i);
                                if (null != icon)
                                {
                                    float x = (float)(Math.Sin(i * Math.PI * 2 / Point_Num) * Point_R);
                                    float y = (float)(Math.Cos(i * Math.PI * 2 / Point_Num) * Point_R);
                                    icon.localPosition = new Vector3(x, y);
                                    UISprite sp = icon.GetComponent<UISprite>();
                                    if (null != sp)
                                    {
                                        sp.alpha = (Point_Num - i + 0.0f) / Point_Num;
                                        mSpriteList.Add(sp);
                                    }
                                }
                            }
                        }

                        Transform Label = blueList.Find("Label");
                        GameHelper.SetLabelText(Label, LauguageTool.GetIns().GetText("查看控制器背后的编号并点击下方"));
                        mSelectBluePosition = UIManager.GetWinPos(blueList, UIWidget.Pivot.Bottom,0,35);
                        if (mSelectBluePosition.y > -150f)
                        {
                            mSelectBluePosition.y = -150f;
                        }
                        blueList.localPosition = mSelectBluePosition - new Vector3(0, 300);
                        

                        Transform blueItem = blueList.Find("item");
                        if (null != blueItem)
                        {
                            mBlueItem = blueItem.gameObject;
                            mBlueItemSize = NGUIMath.CalculateRelativeWidgetBounds(blueItem).size;
                            mBlueItem.gameObject.SetActive(false);
                        }

                        Transform panel = blueList.Find("panel");
                        if (null != panel)
                        {
                            UIPanel uiPanel = panel.GetComponent<UIPanel>();
                            if (null != uiPanel)
                            {
                                uiPanel.depth = mDepth + 1;
                                Vector4 rect = uiPanel.finalClipRegion;
                                rect.z = PublicFunction.GetWidth();
                                uiPanel.baseClipRegion = rect;
                            }
                            mBlueGridTrans = panel.Find("grid");
                        }
                        blueList.gameObject.SetActive(false);

                    }
                }
                IntoSearchMsg();
            }
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "连接页面加载完毕");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            string name = obj.name;
            if (name.Equals("btnBack"))
            {
                if (mMsgType == ConnectMsgType.Connect_Msg_Connecting)
                {
                    if (-1 != mCheckConnectIndex)
                    {
                        Timer.Cancel(mCheckConnectIndex);
                        mCheckConnectIndex = -1;
                    }
                    if (null != mConnectingObj)
                    {
                        mConnectingObj = null;
                        PlatformMgr.Instance.CannelConnectBluetooth();
                    }
                    ClearAllDevice();
                    IntoSearchMsg();
                } else
                {
                    if (mMsgType == ConnectMsgType.Connect_Msg_Select || mMsgType == ConnectMsgType.Connect_Msg_Into_Select_Ing)
                    {
                        StopSearchBlue();
                    }
                    OnClose();
                    SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                }
            }
            else if (name.Equals("searchBtn"))
            {
                IntoSearchMsg();
            }
            else if (name.Equals("btnRefresh"))
            {
                StopSearchBlue();
                ClearAllDevice();
                IntoSearchMsg();
            }
            else if (name.Equals("btnHelp"))
            {
                if (null != onHelpCallback)
                {
                    onHelpCallback();
                }
            }
            else if (name.StartsWith("blue_"))
            {//选中了蓝牙
                if (mDeviceDict.ContainsKey(obj))
                {
                    if (-1 != mCheckConnectIndex)
                    {
                        Timer.Cancel(mCheckConnectIndex);
                    }
                    StopSearchBlue();
                    mCheckConnectIndex = Timer.Add(45, 1, 1, OpenConnectOutTimeCheck);
                    mConnectingObj = obj;
                    IntoMsgState(ConnectMsgType.Connect_Msg_Connecting);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void IntoSearchMsg()
    {
        mConnectingObj = null;
        if (PlatformMgr.Instance.IsOpenBluetooth())
        {
            IntoMsgState(ConnectMsgType.Connect_Msg_Search);
        }
        else
        {
#if UNITY_ANDROID
            PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("此应用需要使用蓝牙功能"), OpenBlueOnClick, null, delegate (BasePopWin popMsg)
            {
                if (popMsg is PromptMsg)
                {
                    PromptMsg msg = (PromptMsg)popMsg;
                    msg.SetLeftBtnText(LauguageTool.GetIns().GetText("拒绝"));
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("允许"));
                }
            });
            
#else
            PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙未打开").Replace('-', '>'), OpenIosBlueOnClick);
#endif
            IntoMsgState(ConnectMsgType.Connect_Msg_Search_Ready);
        }
    }

    void IntoMsgState(ConnectMsgType msgType)
    {
        if (mMsgType == msgType)
        {
            return;
        }
        try
        {
            SetTitle(msgType);
            SetSearchBtnState(msgType);
            switch (mMsgType)
            {
                case ConnectMsgType.Connect_Msg_Search_Ready:
                case ConnectMsgType.Connect_Msg_Search_Fail:
                    {
                        Transform btnHelp = mTrans.Find("top/btnHelp");
                        if (null != btnHelp)
                        {
                            btnHelp.gameObject.SetActive(false);
                        }
                    }
                    break;
                case ConnectMsgType.Connect_Msg_Search:
                    SetSearchPhoneActive(false);
                    if (msgType == ConnectMsgType.Connect_Msg_Select)
                    {
                        mTrans.Find("center/icon").gameObject.SetActive(false);
                    }
                    break;
                case ConnectMsgType.Connect_Msg_Select:
                case ConnectMsgType.Connect_Msg_Into_Select_Ing:
                    SetRefreshActive(false);
                    SetSelectBlueListActive(false);
                    if (msgType == ConnectMsgType.Connect_Msg_Search)
                    {
                        mTrans.Find("center/icon").gameObject.SetActive(true);
                    }
                    break;
                case ConnectMsgType.Connect_Msg_Connecting:
                    if (mMsgType != ConnectMsgType.Connect_Msg_Search)
                    {
                        SetConnectingPhoneActive(false);
                    }
                    mTrans.Find("center/icon/connecting").gameObject.SetActive(false);
                    break;
                case ConnectMsgType.Connect_Msg_Connect_Fail:
                    {
                        mTrans.Find("center/icon/state").gameObject.SetActive(false);
                        Transform btnHelp = mTrans.Find("top/btnHelp");
                        if (null != btnHelp)
                        {
                            btnHelp.gameObject.SetActive(false);
                        }
                    }
                    break;
            }
            if (msgType == ConnectMsgType.Connect_Msg_Select)
            {
                mMsgType = ConnectMsgType.Connect_Msg_Into_Select_Ing;
            }
            else
            {
                mMsgType = msgType;
            }
            switch (msgType)
            {
                case ConnectMsgType.Connect_Msg_Search_Ready:
                case ConnectMsgType.Connect_Msg_Search_Fail:
                    mTrans.Find("center/icon").gameObject.SetActive(true);
                    break;
                case ConnectMsgType.Connect_Msg_Search:
                    SetSearchPhoneActive(true);
                    break;
                case ConnectMsgType.Connect_Msg_Select:
                    SetRefreshActive(true);
                    SetSelectBlueListActive(true);
                    break;
                case ConnectMsgType.Connect_Msg_Connecting:
                    SetConnectingPhoneActive(true);
                    mTrans.Find("center/icon").gameObject.SetActive(true);
                    mTrans.Find("center/icon/connecting").gameObject.SetActive(true);
                    break;
                case ConnectMsgType.Connect_Msg_Connect_Fail:
                    ClearAllDevice();
                    mTrans.Find("center/icon/state").gameObject.SetActive(true);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void SetTitle(ConnectMsgType msgType)
    {
        if (null == mTrans)
        {
            return;
        }        
        Transform title = mTrans.Find("top/title");
        if (null != title)
        {
            Transform maintitle = title.Find("maintitle");
            
            if (null != maintitle)
            {
                TweenAlpha tweenAlpha = maintitle.GetComponent<TweenAlpha>();
                if (null != tweenAlpha)
                {
                    if (ConnectMsgType.Connect_Msg_None != mMsgType)
                    {
                        GameHelper.PlayTweenAlpha(tweenAlpha, 0.01f);
                        tweenAlpha.SetOnFinished(delegate () {
                            SetMainTitleText(maintitle, msgType);
                            tweenAlpha.onFinished.Clear();
                            GameHelper.PlayTweenAlpha(tweenAlpha, 1);
                        });
                    }
                    else
                    {
                        SetMainTitleText(maintitle, msgType);
                    }
                }
                else
                {
                    SetMainTitleText(maintitle, msgType);
                }
            }
        }
    }
    void SetMainTitleText(Transform maintitle, ConnectMsgType msgType)
    {
        switch (msgType)
        {         
            case ConnectMsgType.Connect_Msg_Search_Ready:
                {
                    GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("搜索Jimu机器人"));
                }
                break;
            case ConnectMsgType.Connect_Msg_Search:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("搜索Jimu中..."));
                break;
            case ConnectMsgType.Connect_Msg_Search_Fail:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("未搜索到Jimu机器人"));
                break;
            case ConnectMsgType.Connect_Msg_Connecting:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("连接Jimu中..."));
                break;
            case ConnectMsgType.Connect_Msg_Select:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("选择你要连接的Jimu"));
                break;
            case ConnectMsgType.Connect_Msg_Connect_Fail:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("LianJieShiBai"));
                break;
        }
    }

    void SetSearchBtnState(ConnectMsgType mstType)
    {
        Transform searchBtn = mTrans.Find("bottom/searchBtn");
        if (null != searchBtn)
        {
            bool activeFlag = false;
            string btnText = null;
            //string iconName = null;
            switch (mstType)
            {
                case ConnectMsgType.Connect_Msg_Search_Ready:
                case ConnectMsgType.Connect_Msg_Search_Fail:
                    activeFlag = true;
                    btnText = LauguageTool.GetIns().GetText("点击搜索");
                    //iconName = "search_btn_icon";
                    break;
                case ConnectMsgType.Connect_Msg_Connect_Fail:
                    activeFlag = true;
                    btnText = LauguageTool.GetIns().GetText("重新连接");
                    //iconName = "tryagain_btn_icon";
                    break;
            }
            searchBtn.gameObject.SetActive(activeFlag);
            if (activeFlag)
            {
                UILabel lb = GameHelper.FindChildComponent<UILabel>(searchBtn, "Label");
                if (null != lb)
                {
                    lb.text = btnText;
                }
                /*UISprite sp = GameHelper.FindChildComponent<UISprite>(searchBtn, "sprite");
                if (null != sp)
                {
                    sp.spriteName = iconName;
                }*/
            }
        }
    }

    void SetSearchPhoneActive(bool activeFlag)
    {
        Transform circle = mTrans.Find("center/circle");
        if (activeFlag)
        {
            if (null != circle)
            {
                for (int i = 0; i < circle.childCount; i++)
                {
                    Transform trans = circle.GetChild(i);
                    trans.localScale = Vector3.one;
                }
            }
            if (null != mSearchTexture)
            {
                for (int i = 0, imax = mSearchTexture.Length; i < imax; ++i)
                {
                    mSearchTexture[i].width = 0;
                    mSearchTexture[i].height = 0;
                }
            }
            mSearchUpdateFlag = true;
        }
        else
        {
            mSearchUpdateFlag = false;
        }
        mSearchTime = 0;
        if (null != circle)
        {
            circle.gameObject.SetActive(activeFlag);
        }
        Transform phone = mTrans.Find("bottom/phone");
        SetTransformPositionActive(phone, activeFlag, mPhonePosition, mPhonePosition - new Vector3(0, 300), delegate () {
            if (activeFlag)
            {
                if (-1 != mSearchBlueIndex)
                {
                    Timer.Cancel(mSearchBlueIndex);
                    mSearchBlueIndex = -1;
                }
                OpenSearchBlue(false);
            }
        });
    }

    void SetConnectingPhoneActive(bool activeFlag)
    {
        Transform phone = mTrans.Find("bottom/phone");
        SetTransformPositionActive(phone, activeFlag, mPhonePosition, mPhonePosition - new Vector3(0, 300), delegate () {
            if (activeFlag)
            {
                if (null != mConnectingObj && mDeviceDict.ContainsKey(mConnectingObj))
                {
                    PlatformMgr.Instance.ConnenctBluetooth(mDeviceDict[mConnectingObj].Mac, mDeviceDict[mConnectingObj].Name);
                }
                else
                {
                    IntoSearchMsg();
                }
            }
        });
    }


    void SetSelectBlueListActive(bool activeFlag)
    {
        Transform blueList = mTrans.Find("bottom/blueList");
        if (!activeFlag)
        {
            if (null != mLoadingObj)
            {
                mLoadingObj.SetActive(activeFlag);
            }
            mLoadingUpdateFlag = false;
            mLoadingUpdateTime = 0;
        }
        SetTransformPositionActive(blueList, activeFlag, mSelectBluePosition, mSelectBluePosition - new Vector3(0, 500), delegate () {
            if (activeFlag)
            {
                CreateCacheDevice();
                mMsgType = ConnectMsgType.Connect_Msg_Select;
                if (null != mLoadingObj)
                {
                    mLoadingObj.SetActive(activeFlag);
                }
                mLoadingUpdateFlag = true;
                mLoadingUpdateTime = 0;
            }
            else
            {
                if (null != blueList)
                {
                    Transform panel = blueList.Find("panel");
                    if (null != panel)
                    {
                        panel.localPosition = Vector3.zero;
                        UIPanel uiPanel = panel.GetComponent<UIPanel>();
                        if (null != uiPanel)
                        {
                            Vector2 offset = uiPanel.clipOffset;
                            offset.y = 0;
                            uiPanel.clipOffset = offset;
                        }
                    }
                }
            }
            
        });
    }

    void SetRefreshActive(bool activeFlag)
    {
        Transform btnRefresh = mTrans.Find("top/btnRefresh");
        btnRefresh.gameObject.SetActive(activeFlag);
        Transform btnHelp = mTrans.Find("top/btnHelp");
        btnHelp.gameObject.SetActive(false);
        //SetTransformPositionActive(btnRefresh, activeFlag, mRefreshBtnPosition, mRefreshBtnPosition + new Vector3(300, 0));
    }
  

    void SetTransformAlphaActive(Transform trans, bool activeFlag, EventDelegate.Callback onFinished = null)
    {
        if (null != trans)
        {
            TweenAlpha tweenAlpha = trans.GetComponent<TweenAlpha>();
            if (null != tweenAlpha)
            {
                if (activeFlag)
                {
                    trans.gameObject.SetActive(true);
                    GameHelper.SetTransformAlpha(trans, 0);
                    GameHelper.PlayTweenAlpha(tweenAlpha, 1, 0.6f);
                    tweenAlpha.SetOnFinished(delegate () {
                        tweenAlpha.onFinished.Clear();
                        if (null != onFinished)
                        {
                            onFinished();
                        }
                    });
                }
                else
                {
                    GameHelper.PlayTweenAlpha(tweenAlpha, 0, 0.6f);
                    tweenAlpha.SetOnFinished(delegate ()
                    {
                        trans.gameObject.SetActive(false);
                        tweenAlpha.onFinished.Clear();
                        if (null != onFinished)
                        {
                            onFinished();
                        }
                    });
                }
            }
            else
            {
                if (activeFlag)
                {
                    trans.gameObject.SetActive(activeFlag);
                    GameHelper.SetTransformAlpha(trans, 1);
                }
                else
                {
                    GameHelper.SetTransformAlpha(trans, 0);
                    trans.gameObject.SetActive(activeFlag);
                }
                if (null != onFinished)
                {
                    onFinished();
                }
            }
        }
    }

    void SetTransformPositionActive(Transform trans, bool activeFlag, Vector3 pos, Vector3 hidePos, EventDelegate.Callback onFinished = null)
    {
        if (null != trans)
        {
            TweenPosition tweenPosition = trans.GetComponent<TweenPosition>();
            if (null != tweenPosition)
            {
                if (activeFlag)
                {
                    trans.gameObject.SetActive(true);
                    trans.localPosition = hidePos;
                    GameHelper.PlayTweenPosition(tweenPosition, pos, 0.6f);
                    try
                    {
                        if (mCallBackDict.ContainsKey(trans))
                        {
                            mEventDelay.Remove(mCallBackDict[trans]);
                        }
                        if (null != onFinished)
                        {
                            EventDelay.TimeData timeData = mEventDelay.Add(0.6f, delegate ()
                            {
                                if (null != mCallBackDict && mCallBackDict.ContainsKey(trans) && null != mCallBackDict[trans])
                                {
                                    onFinished();
                                    mCallBackDict.Remove(trans);                                   
                                }
                            });
                            mCallBackDict[trans] = timeData;
                        }
                        
                    }
                    catch (System.Exception ex)
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, ex.ToString());
                    }
                }
                else
                {
                    GameHelper.PlayTweenPosition(tweenPosition, hidePos, 0.6f);
                    try
                    {
                        if (mCallBackDict.ContainsKey(trans))
                        {
                            mEventDelay.Remove(mCallBackDict[trans]);
                        }
                        if (null != onFinished)
                        {
                            EventDelay.TimeData timeData = mEventDelay.Add(0.6f, delegate ()
                            {
                                if (null != mCallBackDict && mCallBackDict.ContainsKey(trans) && null != mCallBackDict[trans])
                                {
                                    trans.gameObject.SetActive(false);
                                    onFinished();
                                    mCallBackDict.Remove(trans);
                                }
                            });
                            mCallBackDict[trans] = timeData;
                        }

                    }
                    catch (System.Exception ex)
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, ex.ToString());
                    }                 
                }
            }
            else
            {
                trans.gameObject.SetActive(activeFlag);
                if (activeFlag)
                {
                    trans.localPosition = pos;
                }
                else
                {
                    trans.localPosition = hidePos;
                }
                if (null != onFinished)
                {
                    onFinished();
                }
            }
        }
    }

    
    void OpenSearchBlue(bool instant)
    {
        if (instant)
        {
            SearchBlue();
        } else
        {
            float waitTime = Time.time - SingletonObject<ConnectCtrl>.GetInst().LastDicConnectedTime;
            if (waitTime >= 3)
            {
                mSearchBlueIndex = Timer.Add(1, 1, 1, SearchBlue);
            }
            else
            {
                float wt = 4f - waitTime;
                mSearchBlueIndex = Timer.Add(wt, 1, 1, SearchBlue);
            }
        }
        
    }
    void SearchBlue()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "SearchBlue");
        mSearchBlueIndex = -1;
        if (mMsgType == ConnectMsgType.Connect_Msg_Connecting)
        {
            return;
        }
        mSearchFlag = true;
        PlatformMgr.Instance.StartScan();
        if (-1 != mCheckSearchIndex)
        {
            Timer.Cancel(mCheckSearchIndex);
            mCheckSearchIndex = -1;
        }
        if (-1 != mCheckConnectIndex)
        {
            Timer.Cancel(mCheckConnectIndex);
            mCheckConnectIndex = -1;
        }
        mCheckSearchIndex = Timer.Add(20, 1, 1, CheckSearchDevice);
    }

    void StopSearchBlue()
    {
        if (-1 != mSearchBlueIndex)
        {
            Timer.Cancel(mSearchBlueIndex);
            mSearchBlueIndex = -1;
        }
        if (-1 != mCheckSearchIndex)
        {
            Timer.Cancel(mCheckSearchIndex);
            mCheckSearchIndex = -1;
        }
        mSearchFlag = false;
        PlatformMgr.Instance.StopScan();
    }

    void CheckSearchDevice()
    {
        mCheckSearchIndex = -1;
        if (null != mConnectingObj)
        {
            return;
        }
        if (mDeviceDict.Count < 1)
        {
            IntoMsgState(ConnectMsgType.Connect_Msg_Search_Fail);
            ShowHelpBtn(delegate () {
                if (SingletonBehaviour<ClientMain>.GetInst().useThirdAppFlag)
                {
                    PlatformMgr.Instance.PopWebErrorType(ConnectionErrorType.ConnectionSearchJimuType);
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("搜索不到蓝牙设备"), RefreshBlueOnClick, null, delegate(BasePopWin popMsg) {
                        if (popMsg is PromptMsg)
                        {
                            PromptMsg msg = (PromptMsg)popMsg;
                            msg.SetRightBtnText(LauguageTool.GetIns().GetText("刷新"));
                        }
                    });
                }
            });
        }
        /*if (mDeviceDict.Count == 1)
        {
            foreach (GameObject obj in mDeviceDict.Keys)
            {
                mConnectingObj = obj;
                PlatformMgr.Instance.StopScan();
                IntoMsgState(ConnectMsgType.Connect_Msg_Connecting);
            }      
        }*/
    }

    void ShowHelpBtn(EventDelegate.Callback callback)
    {
        onHelpCallback = callback;
        if (null != onHelpCallback)
        {
            onHelpCallback();
        }
        Transform btnHelp = mTrans.Find("top/btnHelp");
        if (null != btnHelp)
        {
            btnHelp.gameObject.SetActive(true);
        }
    }

    void RefreshBlueOnClick(GameObject obj)
    {
        try
        {
            IntoSearchMsg();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    GameObject AddDevice(DeviceInfo device)
    {
        if (null != mBlueItem && null != mBlueGridTrans && isShow)
        {
            //PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "发现设备:mac = " + device.Mac + " name=" + device.Name + " rssi =" + device.RSSI);
            GameObject tmp = GameObject.Instantiate(mBlueItem) as GameObject;
            tmp.name = string.Format("blue_{0}", device.RSSI);
            tmp.SetActive(true);
            mDeviceDict[tmp] = device;
            mDeviceForMacDict[device.Mac] = tmp;
            Transform trans = tmp.transform;
            trans.parent = mBlueGridTrans;
            trans.localScale = new Vector3(1, 0, 1);
            trans.localEulerAngles = Vector3.zero;
            SetDeviceSignal(trans, device.RSSI);
            BlueListReposition(trans);
            UILabel label = GameHelper.FindChildComponent<UILabel>(trans, "Label");
            if (null != label)
            {
                string mac = string.Empty;
                if (RobotManager.GetInst().IsCreateRobotFlag || RobotManager.GetInst().IsSetDeviceIDFlag)
                {
                    mac = PlatformMgr.Instance.GetLastConnectedMac();
                }
                else
                {
                    Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != robot)
                    {
                        mac = PlatformMgr.Instance.GetRobotConnectedMac(robot.ID);
                    }
                }
                string rename = PlatformMgr.Instance.GetNameForMac(device.Mac);
                Color selectColor = new Color32(0, 169, 255, 255);
                if (string.IsNullOrEmpty(rename))
                {
                    if (device.Name.StartsWith("Jimu_") || device.Name.StartsWith("JIMU_"))
                    {
                        if (device.Mac.Equals(mac))
                        {
                            label.text = device.Name;
                            label.color = selectColor;
                        }
                        else
                        {
                            label.text = string.Format("[596f80]{0}[-][-]", device.Name.Replace("_", "_[fe497c]"));
                        }
                    } else if (device.Name.StartsWith("My_Jimu_"))
                    {
                        if (device.Mac.Equals(mac))
                        {
                            label.text = device.Name;
                            label.color = selectColor;
                        }
                        else
                        {
                            label.text = string.Format("[596f80]{0}[-][-]", device.Name.Replace("My_Jimu_", "My_Jimu_[fe497c]"));
                        }
                    }
                    else
                    {
                        label.text = device.Name;
                        if (device.Mac.Equals(mac))
                        {
                            label.color = selectColor;
                        }
                        else
                        {
                            label.color = PublicFunction.GreyColor;
                        }
                        
                    }
                }
                else
                {
                    label.text = rename;
                    if (device.Mac.Equals(mac))
                    {
                        label.color = selectColor;
                    }
                    else
                    {
                        label.color = PublicFunction.GreyColor;
                    }
                }
                //label.text = label.text + string.Format(" [777b7c]信号强度{0}[-]", device.RSSI.ToString());
            }
            UIManager.SetButtonEventDelegate(trans, mBtnDelegate);
            return tmp;
        }
        return null;
    }

    void SetDeviceSignal(Transform tmp, int rssi)
    {
        UISprite sprite = GameHelper.FindChildComponent<UISprite>(tmp, "signal");
        if (null != sprite)
        {
            sprite.spriteName = GetSignalIconName(rssi);
            //sprite.MakePixelPerfect();
        }
    }

    string GetSignalIconName(int rssi)
    {
        if (rssi < -90)
        {
            return "signal1";
        }
        else if (rssi < -80)
        {
            return "signal2";
        }
        else if (rssi < -70)
        {
            return "signal3";
        }
        else
        {
            return "signal4";
        }
    }

    void BlueListReposition(Transform item)
    {
        List<Transform> list = new List<Transform>();
        for (int i = 0, imax = mBlueGridTrans.childCount; i < imax; ++i)
        {
            list.Add(mBlueGridTrans.GetChild(i));
        }
        list.Sort(delegate (Transform a, Transform b) {
            int num1 = int.Parse(a.name.Substring("blue_".Length));
            int num2 = int.Parse(b.name.Substring("blue_".Length));
            return num2 - num1;
        });
        for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            Transform trans = list[i];
            Vector2 pos = new Vector2(0, -mBlueItemSize.y / 2 - mBlueItemSize.y * i - 14 * (i + 1));
            if (item == trans)
            {
                trans.localPosition = pos;
                TweenScale tweenScale = GameHelper.PlayTweenScale(trans, Vector3.one);
                if (null != tweenScale)
                {
                    tweenScale.SetOnFinished(delegate () {
                        UIButtonScale btnScale = trans.GetComponent<UIButtonScale>();
                        if (null != btnScale)
                        {
                            btnScale.enabled = true;
                        }
                    });
                }
                else
                {
                    UIButtonScale btnScale = trans.GetComponent<UIButtonScale>();
                    if (null != btnScale)
                    {
                        btnScale.enabled = true;
                    }
                }
            }
            else
            {
                if (!pos.Equals(trans.localPosition))
                {
                    GameHelper.PlayTweenPosition(trans, pos);
                }
            }
        }
    }

    void CreateCacheDevice()
    {
        for (int i = 0, imax = mCacheDeviceList.Count; i < imax; ++i)
        {
            if (!mDeviceForMacDict.ContainsKey(mCacheDeviceList[i].Mac))
            {
                AddDevice(mCacheDeviceList[i]);
            }
        }
        mCacheDeviceList.Clear();
    }

    void ClearAllDevice()
    {
        mCacheDeviceList.Clear();
        foreach (var kvp in mDeviceForMacDict)
        {
            kvp.Value.transform.parent = null;
            GameObject.Destroy(kvp.Value);
        }
        mDeviceForMacDict.Clear();
        mDeviceDict.Clear();
        if (null != mBlueGridTrans)
        {
            mBlueGridTrans.DetachChildren();
        }
    }


    void OnFoundDevice(EventArg arg)
    {
        try
        {
            DeviceInfo info = arg[0] as DeviceInfo;
            if (info == null) return;
            if (info.Name.StartsWith("Jimuspk_"))
            {
                return;
            }
            if (!mSearchFlag)
            {
                return;
            }
            if (info.Name.StartsWith("JIMU") || info.Name.StartsWith("Jimu") || info.Name.StartsWith("My_Jimu_") || info.Name.StartsWith("jimu"))
            {
                if (info.RSSI == 127)
                {
                    info.RSSI = -127;
                }
                //if (-1 != mCheckSearchIndex)
                //{
                //    Timer.Cancel(mCheckSearchIndex);
                //    mCheckSearchIndex = -1;
                //}
                Robot robot = SingletonObject<ConnectCtrl>.GetInst().GetConnectRobot();
                if (null != robot)
                {
                    if (null == mConnectingObj && SingletonObject<ConnectCtrl>.GetInst().IsAutoConnect(robot.ID, info.Mac))
                    {
                        if (-1 != mCheckConnectIndex)
                        {
                            Timer.Cancel(mCheckConnectIndex);
                        }
                        StopSearchBlue();
                        mCheckConnectIndex = Timer.Add(45, 1, 1, OpenConnectOutTimeCheck);
                        mConnectingObj = AddDevice(info);
                        mCacheDeviceList.Clear();
                        IntoMsgState(ConnectMsgType.Connect_Msg_Connecting);
                        return;
                    }
                }
                if (!mDeviceForMacDict.ContainsKey(info.Mac))
                {
                    if (mMsgType == ConnectMsgType.Connect_Msg_Select)
                    {
                        AddDevice(info);
                    }
                    else if (mMsgType == ConnectMsgType.Connect_Msg_Search)
                    {
                        mCacheDeviceList.Add(info);
                        IntoMsgState(ConnectMsgType.Connect_Msg_Select);
                    }
                    else if (mMsgType == ConnectMsgType.Connect_Msg_Into_Select_Ing)
                    {
                        mCacheDeviceList.Add(info);
                    }
                }
            }

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    void OnConnectResult(EventArg arg)
    {
        try
        {
            bool result = (bool)arg[0];
            if (result)
            {
            }
            else
            {
                Robot robot = SingletonObject<ConnectCtrl>.GetInst().GetConnectRobot();
                if (-1 != mCheckConnectIndex)
                {
                    Timer.Cancel(mCheckConnectIndex);
                    mCheckConnectIndex = -1;
                }
                if (null != mConnectingObj && mMsgType == ConnectMsgType.Connect_Msg_Connecting)
                {
                    ConnectFail(LauguageTool.GetIns().GetText("连接失败"));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void OpenConnectOutTimeCheck()
    {
        try
        {
            mCheckConnectIndex = -1;
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "OpenConnectOutTimeCheck mMsgType = " + mMsgType.ToString() + " mConnectingObj = " + mConnectingObj);
            if (isShow && null != mConnectingObj && mMsgType == ConnectMsgType.Connect_Msg_Connecting)
            {
                PlatformMgr.Instance.OnlyDisConnectBluetooth();
                ConnectFail(LauguageTool.GetIns().GetText("连接失败"));
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void ConnectOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            ClearAllDevice();
            IntoSearchMsg();
        }
    }

    void OpenBlueOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.LeftBtnName))
        {
        }
        else if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            PlatformMgr.Instance.OpenBluetooth();
            NetWaitMsg.ShowWait(1);
            Timer.Add(1f, 1, 1, delegate () { 
                if (PlatformMgr.Instance.IsOpenBluetooth())
                {
                    IntoMsgState(ConnectMsgType.Connect_Msg_Search);
                }
            });
        }
    }

    void OpenIosBlueOnClick(GameObject obj)
    {
        if (PlatformMgr.Instance.IsOpenBluetooth())
        {
            IntoMsgState(ConnectMsgType.Connect_Msg_Search);
        }
    }

#endregion
}