using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Event;
using System.Text;
using Game;
using LitJson;
/// <summary>
/// Author:xj
/// FileName:SetServoTypeMsg.cs
/// Description:设置舵机模式
/// Time:2017/1/11 10:15:05
/// </summary>
public class SetServoTypeMsg : BasePopWin
{
    #region 公有属性
    public enum SetServoTypeState : byte
    {
        Msg_Type_None = 0,
        Set_Ready,
        Set_Help,
        Set_Servo_Type,
    }

    #endregion

    #region 其他属性
    SetServoTypeState mMsgType = SetServoTypeState.Msg_Type_None;
    static SetServoTypeMsg sInst;
    GameObject mServoItem;
    Vector2 mServoSize;
    Robot mRobot;
    Transform mLeftGridTrans;
    Transform mRightGridTrans;

    GameObject mDragGameObject;

    int ServoBgWidth = 640;
    int ServoBgHeight = 508;

    Dictionary<byte, ServoModel> mServoDict = null;
    List<byte> mChangeList = null;

    GameObject mLeftTips;
    GameObject mRightTips;

    GameObject mLeftBgObj;
    GameObject mRightBgObj;

    GameObject mGuideGameObject;

    EventDelegate.Callback mOnCloseCallBack = null;

    int mNowStep = 3;
    int mTotalStep = 10;
    #endregion

    #region 公有函数
    public SetServoTypeMsg(SetServoTypeState msgType, EventDelegate.Callback closeBack)
    {
        mUIResPath = "Prefab/UI/SetServoModelMsg";
        mMsgType = msgType;
        isSingle = true;
        sInst = this;
        mServoDict = new Dictionary<byte, ServoModel>();
        mChangeList = new List<byte>();
        mOnCloseCallBack = closeBack;
    }

    public static void ShowMsg(EventDelegate.Callback closeBack = null)
    {
        if (null == sInst)
        {
            object[] args = new object[2];
            if (PlatformMgr.Instance.EditFlag)
            {
                args[0] = SetServoTypeState.Set_Ready;
            }
            else
            {
                args[0] = SetServoTypeState.Set_Servo_Type;
            }
            args[1] = closeBack;
            SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(SetServoTypeMsg), args, 1, null, false);
        }
        else
        {
            sInst.OnShow();
        }
    }

    public override void Init()
    {
        base.Init();
        mCoverAlpha = 1;
        mCoverColor = PublicFunction.Connect_bg_Color;
    }

    public override void Release()
    {
        base.Release();
        EventMgr.Inst.UnRegist(EventID.Servo_Drag_Drop_Start, ItemDragDropStart);
        EventMgr.Inst.UnRegist(EventID.Servo_Drag_Drop_End, ItemDragDropEnd);
        EventMgr.Inst.UnRegist(EventID.Servo_Press_Hold, ItemPressHold);
        EventMgr.Inst.UnRegist(EventID.Servo_Press_Hold_Recover, ItemPressHoldRecover);
        EventMgr.Inst.UnRegist(EventID.Servo_Press, ServoPress);
        sInst = null;
        if (null != mOnCloseCallBack)
        {
            mOnCloseCallBack();
        }
    }
    #endregion

    #region 其他函数

    protected override void AddEvent()
    {
        base.AddEvent();
        try
        {
            EventMgr.Inst.Regist(EventID.Servo_Drag_Drop_Start, ItemDragDropStart);
            EventMgr.Inst.Regist(EventID.Servo_Drag_Drop_End, ItemDragDropEnd);
            EventMgr.Inst.Regist(EventID.Servo_Press_Hold, ItemPressHold);
            EventMgr.Inst.Regist(EventID.Servo_Press_Hold_Recover, ItemPressHoldRecover);
            EventMgr.Inst.Regist(EventID.Servo_Press, ServoPress);
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            ServoBgHeight = PublicFunction.GetHeight() - 220;
            if (null != mTrans)
            {
                Transform top = mTrans.Find("top");
                if (null != top)
                {
                    GameHelper.SetPosition(top.Find("title"), UIWidget.Pivot.Top, new Vector2(0, PublicFunction.Title_Margin_1));
                    GameHelper.SetPosition(top.Find("btnBack"), UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos);
                    Transform subtitle = top.Find("title/subtitle");
                    if (null != subtitle)
                    {
                        subtitle.localPosition = new Vector3(0, -40);
                    }
                    Transform btnHelp = top.Find("btnHelp");
                    if (null != btnHelp)
                    {
                        GameHelper.SetPosition(btnHelp, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos);
                        btnHelp.gameObject.SetActive(false);
                    }
                }
                Transform bottom = mTrans.Find("bottom");
                if (null != bottom)
                {
                    Transform btnSetting = bottom.Find("setting/btnSetting");
                    if (null != btnSetting)
                    {
                        GameHelper.SetPosition(btnSetting, UIWidget.Pivot.Bottom, new Vector2(0, 20));
                        GameHelper.SetLabelText(btnSetting.Find("Label"), LauguageTool.GetIns().GetText("开始设置"));
                        //SetBottomBtn(btnSetting);
                    }
                    Transform btnFinished = bottom.Find("btnFinished");
                    if (null != btnFinished)
                    {
                        GameHelper.SetLabelText(btnFinished.Find("Label"), LauguageTool.GetIns().GetText("确定"));
                        UISprite bg = GameHelper.FindChildComponent<UISprite>(btnFinished, "Background");
                        if (null != bg)
                        {
                            bg.width = PublicFunction.GetExtendWidth();
                        }
                        GameHelper.SetPosition(btnFinished, UIWidget.Pivot.Bottom, Vector2.zero);
                        btnFinished.gameObject.SetActive(false);
                    }
                    /*Transform browse = bottom.FindChild("browse");
                    if (null != browse)
                    {
                        GameHelper.SetPosition(browse, UIWidget.Pivot.Bottom, new Vector2(0, -1));
                        UISprite bg = GameHelper.FindChildComponent<UISprite>(browse, "bg");
                        if (null != bg)
                        {
                            bg.width = PublicFunction.GetExtendWidth();
                        }
                        Transform btnFinished = browse.FindChild("btnFinished");
                        GameHelper.SetPosition(btnFinished, UIWidget.Pivot.Right, new Vector2(PublicFunction.GetWidth() * 0.015f, 0));
                    }*/
                }

                Transform servo = mTrans.Find("servo");
                if (null != servo)
                {
                    mServoItem = servo.gameObject;
                    mServoSize = NGUIMath.CalculateRelativeWidgetBounds(servo).size;
                    mServoItem.gameObject.SetActive(false);
                }

                Transform center = mTrans.Find("center");
                if (null != center)
                {
                    int space = 50;
                    Transform setting = center.Find("setting");
                    if (null != setting)
                    {
                        Transform left = setting.Find("left");
                        if (null != left)
                        {
                            left.localPosition = new Vector3(-PublicFunction.GetWidth() / 4, 0);
                            UIPanel uiPanel = GameHelper.FindChildComponent<UIPanel>(left, "list");
                            if (null != uiPanel)
                            {
                                uiPanel.depth = mDepth + 1;
                                Vector4 rect = uiPanel.finalClipRegion;
                                rect.w = ServoBgHeight - space * 2;
                                uiPanel.baseClipRegion = rect;
                            }
                            Transform title = left.Find("title");
                            if (null != title)
                            {
                                GameHelper.SetLabelText(title, LauguageTool.GetIns().GetText("角模式"));
                                title.localPosition = new Vector3(0, ServoBgHeight / 2 - space - 4);
                            }
                            mLeftGridTrans = left.Find("list/grid");
                            if (null != mLeftGridTrans)
                            {
                                Vector3 pos = mLeftGridTrans.localPosition;
                                pos.y = ServoBgHeight / 2 - space * 2;
                                mLeftGridTrans.localPosition = pos;
                            }
                            Transform tips = left.Find("tips");
                            if (null != tips)
                            {
                                mLeftTips = tips.gameObject;
                                GameHelper.SetLabelText(tips.Find("Label"), LauguageTool.GetIns().GetText("请将需要设置角度模式的舵机拖拽至此"));
                            }
                            Transform bg = left.Find("bg");
                            if (null != bg)
                            {
                                UISprite sprite = bg.GetComponent<UISprite>();
                                if (null != sprite)
                                {
                                    sprite.height = ServoBgHeight;
                                    UISprite bgShadow = GameHelper.FindChildComponent<UISprite>(bg, "shadow");
                                    if (null != bgShadow)
                                    {
                                        bgShadow.height = ServoBgHeight;
                                    }
                                }
                                mLeftBgObj = bg.gameObject;
                            }
                        }
                        Transform right = setting.Find("right");
                        if (null != right)
                        {
                            right.localPosition = new Vector3(PublicFunction.GetWidth() / 4, 0);
                            UIPanel uiPanel = GameHelper.FindChildComponent<UIPanel>(right, "list");
                            if (null != uiPanel)
                            {
                                uiPanel.depth = mDepth + 1;
                                Vector4 rect = uiPanel.finalClipRegion;
                                rect.w = ServoBgHeight - space * 2;
                                uiPanel.baseClipRegion = rect;
                            }
                            Transform title = right.Find("title");
                            if (null != title)
                            {
                                GameHelper.SetLabelText(title, LauguageTool.GetIns().GetText("轮模式"));
                                title.localPosition = new Vector3(0, ServoBgHeight / 2 - space - 4);
                            }
                            mRightGridTrans = right.Find("list/grid");
                            if (null != mRightGridTrans)
                            {
                                Vector3 pos = mRightGridTrans.localPosition;
                                pos.y = ServoBgHeight / 2 - space * 2;
                                mRightGridTrans.localPosition = pos;
                            }
                            Transform tips = right.Find("tips");
                            if (null != tips)
                            {
                                mRightTips = tips.gameObject;
                                GameHelper.SetLabelText(tips.Find("Label"), LauguageTool.GetIns().GetText("请将需要设置轮模式的舵机拖拽至此"));
                            }
                            Transform bg = right.Find("bg");
                            if (null != bg)
                            {
                                UISprite sprite = bg.GetComponent<UISprite>();
                                if (null != sprite)
                                {
                                    sprite.height = ServoBgHeight;
                                }
                                UISprite bgShadow = GameHelper.FindChildComponent<UISprite>(bg, "shadow");
                                if (null != bgShadow)
                                {
                                    bgShadow.height = ServoBgHeight;
                                }
                                mRightBgObj = bg.gameObject;
                            }
                        }
                        setting.localPosition = UIManager.GetWinPos(setting, UIWidget.Pivot.Bottom, 0, 96);
                        setting.gameObject.SetActive(false);
                    }

                    Transform help = center.Find("help");
                    if (null != help)
                    {
                        GameHelper.SetLabelText(help.Find("left/Label1"), LauguageTool.GetIns().GetText("角模式"));
                        GameHelper.SetLabelText(help.Find("left/Label2"), LauguageTool.GetIns().GetText("角度模式提示"));
                        GameHelper.SetLabelText(help.Find("right/Label1"), LauguageTool.GetIns().GetText("轮模式"));
                        GameHelper.SetLabelText(help.Find("right/Label2"), LauguageTool.GetIns().GetText("轮模式提示"));
                    }
                    Transform guide = center.Find("setting/guide");
                    if (null != guide)
                    {
                        UIPanel uiPanel = guide.GetComponent<UIPanel>();
                        if (null != uiPanel)
                        {
                            uiPanel.depth = mDepth + 2;
                        }
                        mGuideGameObject = guide.gameObject;
                    }
                }
                SetMsgType(mMsgType);
            }
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
            CloseGuide();
            string name = obj.name;
            if (name.Equals("btnBack"))
            {
                if (mMsgType == SetServoTypeState.Set_Help)
                {
                    SetMsgType(SetServoTypeState.Set_Servo_Type);
                }
                else
                {
                    OnClose();
                }
            }
            else if (name.Equals("btnSetting"))
            {
                SetMsgType(SetServoTypeState.Set_Servo_Type);
            }
            else if (name.Equals("btnFinished"))
            {
                if (mChangeList.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(LauguageTool.GetIns().GetText("舵机模式修改后"));
                    sb.Append('\n');
                    sb.Append('\n');
                    sb.Append(LauguageTool.GetIns().GetText("1.动作编程"));
                    sb.Append('\n');
                    sb.Append(LauguageTool.GetIns().GetText("2.遥控"));
                    sb.Append('\n');
                    sb.Append(LauguageTool.GetIns().GetText("3.逻辑编程"));
                    sb.Append('\n');
                    sb.Append('\n');
                    sb.Append(LauguageTool.GetIns().GetText("你确定要修改吗"));
                    PromptMsg.ShowDoublePrompt(sb.ToString(), PopFinishedOnClick,null, delegate(BasePopWin popMsg) {
                        if (popMsg is PromptMsg)
                        {
                            PromptMsg msg = (PromptMsg)popMsg;
                            msg.SetContentsPivot(UIWidget.Pivot.Left);
                        }
                    });
                    
                }
                else
                {
                    if (PlatformMgr.Instance.NeedSetServoFlag == SetHardwareState.NeedSetServo)
                    {
                        PlatformMgr.Instance.NeedSetServoFlag = SetHardwareState.SetSuccess;
                    }
                    OnClose();
                }
            }
            else if (name.Equals("btnHelp"))
            {
                SetMsgType(SetServoTypeState.Set_Help);
            } else if (name.Equals("btnPre"))
            {
                DiyJumpStep(mNowStep - 1);
            } else if (name.Equals("btnNext"))
            {
                int targetStep = mNowStep + 1;
                if (targetStep >= mTotalStep)
                {
                    ExitDiyBrowsing();
                }
                else
                {
                    DiyJumpStep(targetStep);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void DiyJumpStep(int targetStep)
    {
        try
        {
            if (null != mOnCloseCallBack)
            {
                mOnCloseCallBack = null;
            }
            OnClose();
            SingletonBehaviour<ClientMain>.GetInst().WaitFrameInvoke(delegate () {
                PlatformMgr.Instance.QuitUnityCleanData();
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["targetStep"] = targetStep;
                dict["totalStep"] = mTotalStep;
                PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.diyJumpStep, Json.Serialize(dict));
            });
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void ExitDiyBrowsing()
    {
        try
        {
            if (null != mOnCloseCallBack)
            {
                mOnCloseCallBack = null;
            }
            OnClose();
            SingletonBehaviour<ClientMain>.GetInst().WaitFrameInvoke(delegate () {
                PlatformMgr.Instance.QuitUnityCleanData();
                PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.exitDiyBrowsing, string.Empty);
            });
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void PopFinishedOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            if (PlatformMgr.Instance.NeedSetServoFlag == SetHardwareState.NeedSetServo)
            {
                PlatformMgr.Instance.NeedSetServoFlag = SetHardwareState.SetSuccess;
            }
            if (null != mRobot)
            {
                ServosConnection connection = SingletonObject<ServosConManager>.GetInst().GetServosConnection(mRobot.ID);
                if (null == connection)
                {
                    EventMgr.Inst.Fire(EventID.Save_Topology_Data);
                    connection = SingletonObject<ServosConManager>.GetInst().GetServosConnection(mRobot.ID);
                    if (null == connection)
                    {
                        connection = SingletonObject<ServosConManager>.GetInst().CreateRobotConnection(mRobot);
                    }
                }
                if (null != connection)
                {
                    ModelDjData servosData = mRobot.GetAllDjData();
                    foreach (var kvp in mServoDict)
                    {
                        connection.UpdateServoModel(kvp.Key, kvp.Value);
                        servosData.UpdateServoModel(kvp.Key, kvp.Value);
                    }
                    connection.Save(mRobot);
                    SingletonObject<ServosConManager>.GetInst().UpdateServosConnection(mRobot.ID, connection);
                }
            }
            OnClose();
            EventMgr.Inst.Fire(EventID.Change_Servo_Model);
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机模式修改成功"), 1, CommonTipsColor.green);
        }
    }

    void SetMsgType(SetServoTypeState state)
    {
        if (null == mTrans)
        {
            return;
        }
        SetTitle(state);
        Transform setting = mTrans.Find("bottom/setting");
        Transform btnHelp = mTrans.Find("top/btnHelp");
        Transform browse = mTrans.Find("bottom/btnFinished");
        UISprite backSprite = GameHelper.FindChildComponent<UISprite>(mTrans, "top/btnBack");
        switch (state)
        {
            case SetServoTypeState.Set_Ready:
                if (null != browse)
                {
                    browse.gameObject.SetActive(false);
                }
                ShowBottomBtn(setting);
                SetContentsActive(mTrans.Find("center/help"), true);
                if (null != backSprite)
                {
                    backSprite.spriteName = "backButton";
                }
                break;
            case SetServoTypeState.Set_Help:
                if (null != btnHelp)
                {
                    Vector3 pos = UIManager.GetWinPos(btnHelp, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                    SetBtnActive(btnHelp, pos + new Vector3(300, 0), false);
                }
                if (null != browse)
                {
                    browse.gameObject.SetActive(false);
                }
                SetContentsActive(mTrans.Find("center/help"), true);
                SetContentsActive(mTrans.Find("center/setting"), false);
                if (null != backSprite)
                {
                    backSprite.spriteName = "btn_cannel";
                }
                break;
            case SetServoTypeState.Set_Servo_Type:
                HideBottomBtn(setting);
                if (null != btnHelp && PlatformMgr.Instance.EditFlag)
                {
                    btnHelp.gameObject.SetActive(true);
                    Vector3 pos = UIManager.GetWinPos(btnHelp, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x + 124, PublicFunction.Back_Btn_Pos.y);
                    SetBtnActive(btnHelp, pos, true);
                }
                if (null != browse)
                {
                    browse.gameObject.SetActive(true);
                }
                if (null != backSprite)
                {
                    backSprite.spriteName = "backButton";
                }
                SetContentsActive(mTrans.Find("center/help"), false);
                SetContentsActive(mTrans.Find("center/setting"), true, delegate() {
                    if (PlatformMgr.Instance.EditFlag)
                    {
                        if (null != mRobot && mRobot.GetAllDjData().GetIDList().Count > 0)
                        {
                            string id = string.Format("setServo_{0}", mRobot.ID);
                            if (!PlayerPrefs.HasKey(id))
                            {
                                OpenGuide();
                                PlayerPrefs.SetInt(id, 1);
                            }
                        }
                    }
                });
                InitServo();
                break;
        }
        mMsgType = state;
    }

    void HideBottomBtn(Transform btn)
    {
        if (null != btn)
        {
            TweenPosition tp = btn.GetComponent<TweenPosition>();
            if (null != tp && btn.gameObject.activeSelf)
            {
                GameHelper.PlayTweenPosition(btn, new Vector3(0, -200), 0.6f);
                tp.SetOnFinished(delegate ()
                {
                    btn.gameObject.SetActive(false);
                });
            }
            else
            {
                btn.localPosition = new Vector3(0, -200);
                btn.gameObject.SetActive(false);
            }
        }
    }

    void ShowBottomBtn(Transform btn)
    {
        if (null != btn)
        {
            btn.gameObject.SetActive(true);
            TweenPosition tp = btn.GetComponent<TweenPosition>();
            if (null != tp)
            {
                GameHelper.PlayTweenPosition(btn, Vector3.zero, 0.6f);
                tp.onFinished.Clear();
            }
            else
            {
                btn.localPosition = Vector3.zero;
            }
        }
    }

    void SetBtnActive(Transform btn, Vector3 pos, bool activeFlag)
    {
        if (null != btn)
        {
            TweenPosition tp = btn.GetComponent<TweenPosition>();
            if (null != tp)
            {
                GameHelper.PlayTweenPosition(tp, pos, 0.6f);
                if (activeFlag)
                {
                    tp.onFinished.Clear();
                }
                else
                {
                    tp.SetOnFinished(delegate ()
                    {
                        btn.gameObject.SetActive(false);
                    });
                }
            }
            else
            {
                //btn.localPosition = pos;
                btn.gameObject.SetActive(activeFlag);
            }
        }
    }

    void SetBottomBtn(Transform btn)
    {
        int width = PublicFunction.GetExtendWidth();
        BoxCollider box = btn.GetComponent<BoxCollider>();
        if (null != box)
        {
            Vector3 boxSize = box.size;
            boxSize.x = width;
            box.size = boxSize;
        }
        UISprite bg = GameHelper.FindChildComponent<UISprite>(btn, "Background");
        if (null != bg)
        {
            bg.width = width;
        }
        UILabel lb = GameHelper.FindChildComponent<UILabel>(btn, "Label");
        if (null != lb)
        {
            lb.width = width;
        }
    }

    void SetTitle(SetServoTypeState state)
    {
        if (null != mTrans)
        {
            Transform maintitle = mTrans.Find("top/title/maintitle");
            Transform subtitle = mTrans.Find("top/title/subtitle");
            if (null != maintitle && null != subtitle)
            {
                switch (state)
                {
                    case SetServoTypeState.Set_Ready:
                        GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("舵机模式"));
                        break;
                    case SetServoTypeState.Set_Help:
                        //GameHelper.PlayTweenPosition(maintitle, Vector3.zero, 0.6f);
                        GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("舵机模式"));
                        TweenAlpha ta = GameHelper.PlayTweenAlpha(subtitle, 0.01f, 0.6f);
                        if (null != ta)
                        {
                            ta.SetOnFinished(delegate () {
                                subtitle.gameObject.SetActive(false);
                            });
                        }
                        break;
                    case SetServoTypeState.Set_Servo_Type:
                        //Vector3 pos = new Vector3(0, -subtitle.localPosition.y);
                        //GameHelper.PlayTweenPosition(maintitle, pos, 0.6f);
                        GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("舵机模式"));
                        GameHelper.SetLabelText(subtitle, LauguageTool.GetIns().GetText("拖拽舵机至相应的模式下"));
                        subtitle.gameObject.SetActive(true);
                        GameHelper.SetTransformAlpha(subtitle, 0.01f);
                        TweenAlpha ta1 = GameHelper.PlayTweenAlpha(subtitle, 1, 0.6f);
                        if (null != ta1)
                        {
                            ta1.onFinished.Clear();
                        }
                        break;
                }
            }
        }
    }


    void SetContentsActive(Transform contents, bool activeFlag, EventDelegate.Callback callBack = null)
    {
        if (null != contents)
        {
            Transform left = contents.Find("left");
            Transform right = contents.Find("right");
            if (activeFlag)
            {
                contents.gameObject.SetActive(true);
                if (null != left)
                {
                    left.localPosition = new Vector3(-PublicFunction.GetWidth(), 0);
                    TweenPosition tp = GameHelper.PlayTweenPosition(left, new Vector3(-PublicFunction.GetWidth() / 4, 0), 0.6f);
                    if (null != tp)
                    {
                        tp.onFinished.Clear();
                        if (null != callBack)
                        {
                            tp.SetOnFinished(callBack);
                        }
                    }
                    else if (null != callBack)
                    {
                        callBack();
                    }
                }
                if (null != right)
                {
                    right.localPosition = new Vector3(PublicFunction.GetWidth(), 0);
                    GameHelper.PlayTweenPosition(right, new Vector3(PublicFunction.GetWidth() / 4, 0), 0.6f);
                }
            }
            else
            {
                if (null != left)
                {
                    TweenPosition tp = GameHelper.PlayTweenPosition(left, new Vector3(-PublicFunction.GetWidth(), 0), 0.6f);
                    if (null != tp)
                    {
                        tp.onFinished.Clear();
                        tp.SetOnFinished(delegate () {
                            contents.gameObject.SetActive(false);
                            if (null != callBack)
                            {
                                callBack();
                            }
                        });
                    }
                    else
                    {
                        contents.gameObject.SetActive(false);
                        if (null != callBack)
                        {
                            callBack();
                        }
                    }
                }
                if (null != right)
                {
                    GameHelper.PlayTweenPosition(right, new Vector3(PublicFunction.GetWidth(), 0), 0.6f);
                }
            }
        }
    }

    void InitServo()
    {
        if (null != mLeftGridTrans && mLeftGridTrans.childCount > 0 || null != mRightGridTrans && mRightGridTrans.childCount > 0)
        {
            return;
        }
        if (null != mRobot)
        {
            List<byte> angleList = mRobot.GetAllDjData().GetAngleList();
            if (angleList.Count > 0)
            {
                CreateAngleList(angleList);
            }
            else
            {
                if (null != mLeftTips)
                {
                    mLeftTips.SetActive(true);
                }
                /*if (null != mLeftBgObj)
                {
                    mLeftBgObj.SetActive(false);
                }*/
            }
            List<byte> turnList = mRobot.GetAllDjData().GetTurnList();
            if (turnList.Count > 0)
            {
                CreateTurnList(turnList);
            }
            else
            {
                if (null != mRightTips)
                {
                    mRightTips.SetActive(true);
                }
                /*if (null != mRightBgObj)
                {
                    mRightBgObj.SetActive(false);
                }*/
            }
        }
    }

    void CreateAngleList(List<byte> list)
    {
        Transform grid = mTrans.Find("center/setting/left/list/grid");
        if (null != grid)
        {
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                CreateServo(grid, list[i], ServoModel.Servo_Model_Angle);
                mServoDict[list[i]] = ServoModel.Servo_Model_Angle;
            }
            ResetQueuePosition(grid, true);
        }
    }

    void CreateTurnList(List<byte> list)
    {
        Transform grid = mTrans.Find("center/setting/right/list/grid");
        if (null != grid)
        {
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                CreateServo(grid, list[i], ServoModel.Servo_Model_Turn);
                mServoDict[list[i]] = ServoModel.Servo_Model_Turn;
            }
            ResetQueuePosition(grid, true);
        }
    }

    void CreateServo(Transform trans, byte id, ServoModel modelType)
    {
        if (null != mServoItem && null != trans)
        {
            GameObject obj = GameObject.Instantiate(mServoItem) as GameObject;
            obj.name = string.Format("servo_{0}", id);
            obj.SetActive(true);
            Transform tmp = obj.transform;
            tmp.parent = trans;
            tmp.localPosition = Vector3.zero;
            tmp.localScale = Vector3.one;
            tmp.localEulerAngles = Vector3.zero;
            GameHelper.SetLabelText(tmp.Find("id"), PublicFunction.ID_Format + id.ToString().PadLeft(2, '0'));
            SetServoIcon(tmp, modelType);
        }
    }

    void ChangeServoModel(Transform servo, ServoModel modelType)
    {
        TweenScale ts = GameHelper.PlayTweenScale(servo, Vector3.zero);
        if (null != ts)
        {
            GameHelper.PlayTweenRota(servo, new Vector3(0, 0, 270));
            Timer.Add(0.3f, 1, 1, delegate () {
                SetServoIcon(servo, modelType);
                GameHelper.PlayTweenScale(servo, Vector3.one);
                GameHelper.PlayTweenRota(servo, Vector3.zero);
            });
        }
        else
        {
            SetServoIcon(servo, modelType);
        }
        
    }

    void SetGameObjectActive(GameObject obj, bool activeFlag)
    {
        if (obj.activeSelf == activeFlag)
        {
            return;
        }
        if (activeFlag)
        {
            obj.SetActive(true);
            GameHelper.SetTransformAlpha(obj.transform, 0.01f);
            TweenAlpha ta = GameHelper.PlayTweenAlpha(obj.transform, 1);
            if (null != ta)
            {
                ta.onFinished.Clear();
            }
        }
        else
        {
            TweenAlpha ta = GameHelper.PlayTweenAlpha(obj.transform, 0.01f);
            if (null != ta)
            {
                ta.SetOnFinished(delegate ()
                {
                    obj.SetActive(false);
                });
            }
            else
            {
                obj.SetActive(false);
            }
        }
    }

    void SetServoIcon(Transform servo, ServoModel modelType)
    {
        if (ServoModel.Servo_Model_Angle == modelType)
        {
            servo.transform.Find("turnModel").gameObject.SetActive(false);
        }
        else
        {
            servo.transform.Find("turnModel").gameObject.SetActive(true);
        }
    }

    void SwitchServoModel(Transform servo, ServoModel modelType)
    {
        ChangeServoModel(servo, modelType);
        byte id = byte.Parse(servo.name.Substring("servo_".Length));
        mServoDict[id] = modelType;
        if (mChangeList.Contains(id))
        {
            mChangeList.Remove(id);
        }
        else
        {
            mChangeList.Add(id);
        }
    }

    bool IsPlace(Transform dragTrans, Transform targetTrans)
    {
        Vector3 dragPos = dragTrans.parent.localPosition + dragTrans.localPosition;
        Vector3 minPos = targetTrans.localPosition - new Vector3(ServoBgWidth / 2, ServoBgHeight / 2);
        Vector3 maxPos = targetTrans.localPosition + new Vector3(ServoBgWidth / 2, ServoBgHeight / 2);
        if (dragPos.x >= minPos.x && dragPos.x <= maxPos.x && dragPos.y >= minPos.y && dragPos.y <= maxPos.y)
        {
            return true;
        }
        return false;
    }

    void ResetQueuePosition(Transform queueTrans, bool instant)
    {
        if (null != queueTrans && queueTrans.childCount > 0)
        {
            List<Transform> list = new List<Transform>();
            for (int i = 0, imax = queueTrans.childCount; i < imax; ++i)
            {
                list.Add(queueTrans.GetChild(i));
            }
            list.Sort(delegate (Transform a, Transform b) {
                int aid = int.Parse(a.name.Substring("servo_".Length));
                int bid = int.Parse(b.name.Substring("servo_".Length));
                return aid - bid;
            });
            int col = 4;
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                Vector3 targetPos = new Vector3(mServoSize.x / 2 + (i % col) * (mServoSize.x + 2), -mServoSize.y / 2 - (i / col) * (mServoSize.y + 10));
                Transform tmp = list[i];
                if (instant)
                {
                    tmp.localPosition = targetPos;
                }
                else
                {
                    TweenPosition tp = GameHelper.PlayTweenPosition(tmp, targetPos);
                    if (null != tp)
                    {
                        tp.onFinished.Clear();
                        tp.SetOnFinished(delegate () {
                            UIDragScrollView drag = tmp.GetComponent<UIDragScrollView>();
                            if (null != drag)
                            {
                                drag.scrollView = NGUITools.FindInParents<UIScrollView>(tmp);
                                drag.enabled = true;
                            }
                        });
                    }
                    //SpringPosition.Begin(list[i].gameObject, targetPos, 8);
                }
            }
            UIScrollView scrollView = NGUITools.FindInParents<UIScrollView>(queueTrans);
            if (null != scrollView)
            {
                Timer.Add(0.35f, 1, 1, delegate () {
                    scrollView.RestrictWithinBounds(false);
                });
            }
        }
    }

    void SetSelectIconActive(Transform servo, bool activeFlag)
    {
        Transform select = servo.Find("select");
        if (null != select)
        {
            select.gameObject.SetActive(activeFlag);
        }
    }

    /// <summary>
    /// 物体开始被拖动
    /// </summary>
    /// <param name="arg"></param>
    void ItemDragDropStart(EventArg args)
    {
        if (!PlatformMgr.Instance.EditFlag)
        {
            return;
        }
        CloseGuide();
        GameObject obj = (GameObject)args[0];
        if (null != obj)
        {
            mDragGameObject = obj;
            Transform temp = obj.transform;
            Transform parent = temp.parent;
            if (mLeftGridTrans == parent)
            {//角度模式的舵机
                SetSelectIconActive(temp, true);
                temp.parent = mLeftGridTrans.parent.parent;
                NGUITools.MarkParentAsChanged(obj);
                ResetQueuePosition(mLeftGridTrans, false);
            }
            else if (mRightGridTrans == parent)
            {//轮模式的舵机
                SetSelectIconActive(temp, true);
                temp.parent = mRightGridTrans.parent.parent;
                NGUITools.MarkParentAsChanged(obj);
                ResetQueuePosition(mRightGridTrans, false);
            }
        }
    }

    void ItemDragDropEnd(EventArg args)
    {
        mDragGameObject = null;
        GameObject obj = (GameObject)args[0];
        if (null != obj && obj.name.StartsWith("servo_"))
        {
            Transform temp = obj.transform;
            Transform parent = null;
            if (temp.parent == mLeftGridTrans.parent.parent)
            {
                if (IsPlace(temp, mRightGridTrans.parent.parent))
                {
                    parent = mRightGridTrans;
                    SwitchServoModel(temp, ServoModel.Servo_Model_Turn);
                }
                else
                {
                    parent = mLeftGridTrans;
                    GameHelper.PlayTweenScale(temp, Vector3.one);
                }
            }
            else if (temp.parent == mRightGridTrans.parent.parent)
            {
                if (IsPlace(temp, mLeftGridTrans.parent.parent))
                {
                    parent = mLeftGridTrans;
                    SwitchServoModel(temp, ServoModel.Servo_Model_Angle);
                }
                else
                {
                    parent = mRightGridTrans;
                    GameHelper.PlayTweenScale(temp, Vector3.one);
                }
            }
            if (null != parent)
            {
                SetSelectIconActive(temp, false);
                temp.parent = parent;
                NGUITools.MarkParentAsChanged(obj);

                ResetQueuePosition(parent, false);
                if (null != mLeftTips)
                {
                    SetGameObjectActive(mLeftTips, mLeftGridTrans.childCount <= 0);
                }
                /*if (null != mLeftBgObj)
                {
                    SetGameObjectActive(mLeftBgObj, mLeftGridTrans.childCount > 0);
                }*/
                if (null != mRightTips)
                {
                    SetGameObjectActive(mRightTips, mRightGridTrans.childCount <= 0);
                }
                /*if (null != mRightBgObj)
                {
                    SetGameObjectActive(mRightBgObj, mRightGridTrans.childCount > 0);
                }*/
            }
        }
    }

    void ItemPressHold(EventArg args)
    {
        if (!PlatformMgr.Instance.EditFlag)
        {
            return;
        }
        GameObject obj = (GameObject)args[0];
        if (null != obj)
        {
            GameHelper.PlayTweenScale(obj.transform, new Vector3(1.2f, 1.2f, 1));
        }
    }

    void ItemPressHoldRecover(EventArg args)
    {
        GameObject obj = (GameObject)args[0];
        if (null != obj)
        {
            GameHelper.PlayTweenScale(obj.transform, Vector3.one);
        }
    }

    void ServoPress(EventArg args)
    {
        CloseGuide();
    }

    void OpenGuide()
    {
        if (null != mGuideGameObject)
        {
            mGuideGameObject.SetActive(true);
            GameHelper.SetTransformAlpha(mGuideGameObject.transform, 1);
            Transform left = mTrans.Find("center/setting/left");
            Transform right = mTrans.Find("center/setting/right");
            Vector3 from = new Vector3(-PublicFunction.GetWidth() / 4, 100);
            Vector2 to = new Vector3(PublicFunction.GetWidth() / 4, 100);
            if (null != left)
            {
                from = left.localPosition + new Vector3(0, 100);
            }
            if (null != right)
            {
                to = right.localPosition + new Vector3(0, 100);
            }
            mGuideGameObject.transform.localPosition = from;
            mGuideGameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1);
            TweenScale tweenScale = GameHelper.PlayTweenScale(mGuideGameObject.transform, Vector3.one);
            tweenScale.onFinished.Clear();
            tweenScale.SetOnFinished(delegate () {
                
                List<Vector3> list = new List<Vector3>();
                list.Add(new Vector3(0, (from.y + to.y) / 2 + 200));
                BezierPosition bp = BezierPosition.Begin(mGuideGameObject, 1f, from, to, list);
                bp.ResetToBeginning();
                bp.onFinished.Clear();
                bp.SetOnFinished(delegate ()
                {
                    TweenScale ts = GameHelper.PlayTweenScale(mGuideGameObject.transform, new Vector3(1.2f, 1.2f, 1f));
                    ts.onFinished.Clear();
                    TweenAlpha ta = GameHelper.PlayTweenAlpha(mGuideGameObject.transform, 0.01f);
                    ta.onFinished.Clear();
                    ta.SetOnFinished(delegate ()
                    {
                        OpenGuide();
                    });
                });
            });
            
        }
    }

    void CloseGuide()
    {
        if (null != mGuideGameObject && mGuideGameObject.activeSelf)
        {
            mGuideGameObject.SetActive(false);
            TweenScale ts = mGuideGameObject.GetComponent<TweenScale>();
            ts.ResetToBeginning();
            ts.enabled = false;
            TweenAlpha ta = mGuideGameObject.GetComponent<TweenAlpha>();
            ta.ResetToBeginning();
            ta.enabled = false;
            BezierPosition tp = mGuideGameObject.GetComponent<BezierPosition>();
            tp.ResetToBeginning();
            tp.enabled = false;
        }
    }
    #endregion
}