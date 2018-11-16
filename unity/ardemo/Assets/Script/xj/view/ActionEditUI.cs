using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Event;
using Game.Scene;
using Game.Platform;
using Game.UI;
using Game.Resource;

/// <summary>
/// Author:xj
/// FileName:ActionEditUI.cs
/// Description:编程界面UI
/// Time:2015/10/29 14:02:25
/// </summary>
public class ActionEditUI : BaseUI
{
    #region 公有属性
    public delegate void OnDragDjDelegate(DuoJi data, bool finished);
    public class DuoJi
    {
        public Transform trans;
        public int Angle
        {
            get { return angle; }
        }
        public int Id
        {
            get { return id; }
        }
        public string Name
        {
            get 
            {
                if (null != trans)
                {
                    return trans.name;
                }
                return string.Empty;
            }
        }
        public UIDragScrollView ScrollDrag
        {
            get
            {
                if (null == scrollDrag)
                {
                    if (null != trans)
                    {
                        scrollDrag = trans.GetComponent<UIDragScrollView>();
                        if (null == scrollDrag)
                        {
                            scrollDrag = trans.gameObject.AddComponent<UIDragScrollView>();
                        }
                    }
                }
                return scrollDrag;
            }
        }
        public DragScrollViewEx ScrollDragEx
        {
            get 
            {
                if (null == scrollDragEx)
                {
                    if (null != trans)
                    {
                        scrollDragEx = trans.GetComponent<DragScrollViewEx>();
                        if (null == scrollDragEx)
                        {
                            scrollDragEx = trans.gameObject.AddComponent<DragScrollViewEx>();
                        }
                    }
                }
                return scrollDragEx; 
            }
        }
        public UIDragObject DragObject
        {
            get 
            {
                if (null == dragObject)
                {
                    if (null != trans)
                    {
                        dragObject = trans.GetComponent<UIDragObject>();
                        if (null == dragObject)
                        {
                            dragObject = trans.gameObject.AddComponent<UIDragObject>();
                        }
                        dragObject.dragMovement = new Vector3(1, 1, 0);
                    }
                }
                return dragObject; 
            }
        }
        Transform directionTrans;
        GameObject adjustObj;
        UILabel idLabel;
        UILabel angleLabel;
        CircleScrollBar slider;
        UIDragScrollView scrollDrag;
        
        DragScrollViewEx scrollDragEx;
        
        UIDragObject dragObject;

        MyTweenRotation tweenRota;
        Color32 AngleNormalColor = new Color32(89, 111, 128, 255);
        UISprite mDianSprite = null;
        int angle;
        int id;

        public OnDragDjDelegate onDragDj;

        public DuoJi(Transform trans, int id)
        {
            Init(trans, id);
        }

        public void SetActive(bool active)
        {
            if (null != trans)
            {
                trans.gameObject.SetActive(active);
            }
            if (null != tweenRota && !active)
            {
                tweenRota.enabled = false;
            }
        }
        public static string GetFrameDjName(int index, int id)
        {
            return "fd_" + index + "_" + id;
        }
        void Init(Transform trans, int id)
        {
            this.trans = trans;
            this.id = id;
            try
            {
                if (null != trans)
                {
                    directionTrans = trans.Find("direction");
                    if (null != directionTrans)
                    {
                        tweenRota = directionTrans.GetComponent<MyTweenRotation>();
                        idLabel = GameHelper.FindChildComponent<UILabel>(directionTrans, "id");
                    }
                    
                    SetID(id);
                    Transform adjust = trans.Find("adjust");
                    if (null != adjust)
                    {
                        UILabel label = GameHelper.FindChildComponent<UILabel>(adjust, "Label");
                        if (null != label)
                        {
                            label.text = LauguageTool.GetIns().GetText("角度");
                        }
                        adjustObj = adjust.gameObject;
                        angleLabel = GameHelper.FindChildComponent<UILabel>(adjust, "angle");
                        slider = GameHelper.FindChildComponent<CircleScrollBar>(adjust, "angleSlider");
                        if (null != slider)
                        {
                            slider.onDragChange = OnDragChange;
                        }
                        ButtonEvent btn = GameHelper.FindChildComponent<ButtonEvent>(adjust, "angleSlider/dian");
                        if (null != btn)
                        {
                            GameObject.Destroy(btn);
                        }
                        mDianSprite = GameHelper.FindChildComponent<UISprite>(adjust, "angleSlider/dian");
                        ButtonEvent btn1 = GameHelper.FindChildComponent<ButtonEvent>(adjust, "angleSlider/bg");
                        if (null != btn1)
                        {
                            GameObject.Destroy(btn1);
                        }
                    }
                    
                    scrollDrag = trans.GetComponent<UIDragScrollView>();
                    scrollDragEx = trans.GetComponent<DragScrollViewEx>();
                    dragObject = trans.GetComponent<UIDragObject>();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void SetAngle(int angle, bool instant = true, float time = 0)
        {
            if (angle < PublicFunction.DuoJi_Min_Show_Rota)
            {
                angle = PublicFunction.DuoJi_Min_Show_Rota;
            }
            else if (angle > PublicFunction.DuoJi_Max_Show_Rota)
            {
                angle = PublicFunction.DuoJi_Max_Show_Rota;
            }
            this.angle = angle;
            if (null != slider)
            {
                if (angle <= PublicFunction.DuoJi_Min_Show_Rota)
                {
                    slider.value = 1;
                }
                else if (angle >= PublicFunction.DuoJi_Max_Show_Rota)
                {
                    slider.value = 0;
                }
                else
                {
                    slider.value = 1 - angle / slider.AngleRange;
                }
                

            }
            SetAngleShow(instant, time);
        }
        /// <summary>
        /// 可设置超出正常范围的角度
        /// </summary>
        /// <param name="angle"></param>
        public void SetErrorAngle(int angle)
        {
            if (angle > PublicFunction.DuoJi_Max_Rota)
            {
                angle = PublicFunction.DuoJi_Max_Rota;
            }
            else if (angle < PublicFunction.DuoJi_Min_Rota)
            {
                angle = PublicFunction.DuoJi_Min_Rota - 1;
            }
            this.angle = angle;
            
            if (null != slider)
            {
                if (angle <= PublicFunction.DuoJi_Min_Show_Rota)
                {
                    slider.value = 1;
                }
                else if (angle >= PublicFunction.DuoJi_Max_Show_Rota)
                {
                    slider.value = 0;
                }
                else
                {
                    slider.value = 1 - angle / slider.AngleRange;
                }
                
            }
            SetAngleShow();
        }
        public void SetID(int id)
        {
            this.id = id;
            if (null != idLabel)
            {
                idLabel.text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
            }
        }

        public void SetAdjustActive(bool active)
        {
            if (null != adjustObj)
            {
                adjustObj.SetActive(active);
            }
        }

        public void SetFrameData(int index, int id, int rota)
        {
            if (null != trans)
            {
                trans.name = GetFrameDjName(index, id);
                SetID(id);
                SetAngle(rota);
            }
        }
        public void SetScrollDragActive(bool active)
        {
            if (null != ScrollDrag)
            {
                ScrollDrag.enabled = active;
            }
        }
        public void SetScrollDragExActive(bool active)
        {
            if (null != ScrollDragEx)
            {
                ScrollDragEx.enabled = active;
            }
        }
        public void DeleteUIPanel()
        {
            if (null != trans)
            {
                UIPanel panel = trans.GetComponent<UIPanel>();
                if (null != panel)
                {
                    UnityEngine.Object.DestroyImmediate(panel);
                }
                Rigidbody rb = trans.GetComponent<Rigidbody>();
                if (null != rb)
                {
                    UnityEngine.Object.DestroyImmediate(rb);
                }
            }
        }

        public void AddUIPanel(int depth)
        {
            if (null != trans)
            {
                UIPanel panel = trans.GetComponent<UIPanel>();
                if (null == panel)
                {
                    panel = trans.gameObject.AddComponent<UIPanel>();
                }
                panel.depth = depth;
            }
        }
        public void SetDragObjectActive(bool active)
        {
            if (null != DragObject)
            {
                DragObject.enabled = active;
            }
        }

        void SetAngleShow(bool instant = true, float time = 0)
        {
            if (null != angleLabel)
            {
                if (angle < 120)
                {
                    angleLabel.text = (angle - 120).ToString() + "°";
                }
                else
                {
                    angleLabel.text = (angle - 120).ToString() + "°";
                }
                if (PublicFunction.IsShowNormalRota(angle))
                {
                    angleLabel.color = AngleNormalColor;
                    /*if (null != mDianSprite)
                    {
                        mDianSprite.color = Color.white;
                    }*/
                    if (null != idLabel)
                    {
                        idLabel.color = new Color32(35, 24, 21, 255);
                    }
                }
                else
                {
                    angleLabel.color = Color.red;
                    /*if (null != mDianSprite)
                    {
                        mDianSprite.color = Color.red;
                    }*/
                    if (null != idLabel)
                    {
                        idLabel.color = Color.red;
                    }
                }
            }
            /*Vector3 to = new Vector3(0, 0, 120 - angle);
            if (instant)
            {
                if (null != directionTrans)
                {
                    directionTrans.localEulerAngles = to;
                }
            }
            else
            {
                if (null != tweenRota)
                {
                    tweenRota.from = tweenRota.transform.localEulerAngles;
                    tweenRota.to = to;
                    tweenRota.delay = 0;
                    tweenRota.duration = time;
                    tweenRota.Play();
                }
            }*///屏蔽旋转
        }

        void OnDragChange(GameObject obj, bool finished)
        {
            angle = (int)((1 - slider.value) * slider.AngleRange);
            if (angle < PublicFunction.DuoJi_Min_Show_Rota)
            {
                angle = PublicFunction.DuoJi_Min_Show_Rota;
            }
            else if (angle > PublicFunction.DuoJi_Max_Show_Rota)
            {
                angle = PublicFunction.DuoJi_Max_Show_Rota;
            }
            SetAngleShow();
            if (null != onDragDj)
            {
                onDragDj(this, finished);
            }
        }
    }
    #endregion

    #region 其他属性
    bool isEditState;
    TweenPosition mFixTweenPosition;
    TweenPosition mPlaybtnTweenPosition;
    TweenPosition mDjTweenPosition;
    TweenPosition mTimeBarPosition;
    TweenPosition mTitlePosition;
    TweenPosition mFrameListPosition;
    TweenPosition mFrameScrollBarTweenPos;
    TweenScale mFrameGridTweenScale;
    TweenRotation mStretchRotation;
    long mDelayScaleIndex = -1;
    int Fix_Offset_Y = -230;
    Vector2 mFixEditPos;
    //Vector2 mDjTransSize = Vector2.zero;

    Vector2 mDjGameObjectEditPos;
    GameObject mDjGameObject;
    GameObject mDuojiPrefab;
    GameObject mFixRightObject;
    GameObject mTitleGameObject;
    Transform mDjParentTrans;
    GameObject mTimeObj;
    GameObject mTimeBoxObj;
    Vector3 mTimeShowPos;
    Vector3 mTimeHidePos;
    UIScrollBar mTimeBar;
    UILabel mTimeLabel;
    //UILabel mConnectLabel;
    UIButton mConnectBtn;
    UISprite mConnectIcon;
    Transform mTimeMin;
    Transform mTimeMax;
    int mNowTime = PublicFunction.Default_Actions_Time;
    //UIGrid mDjlistGrid;

    GameObject mStartObj;

    Transform mPowerDownBtnTrans;//回读按钮trans
    UIButton mPowerDownBtn;
    //float mPowerDownBtnWidth;
    //Vector3 mPowerDownBtnStartPos;
    GameObject mReadBackObj;
    Transform mReadBackBtnTrans;
    TweenAlpha mReadBackTipsTweenAlpha;
    TweenPosition mReadBackTipsTweenPosition;
    //TweenPosition mReadBackBtnPosition;
    TweenScale mReadBackBtnTweenScale;
    TweenRotation mReadBackBtnTweenRotation;
    TweenScale mBigEffectTweenScale;
    TweenScale mSmallEffectTweenScale;

    Robot mRobot;
    Dictionary<string, DuoJi> mDuojiDict;

    DuoJi mSelectDragDj;
    bool mDragStartFlag;
    CustomGrid mFrameCustomGrid;
    UIScrollViewEx mFrameScrollViewEx;
    SpringPanel mFrameSpringPanel;
    UIScrollView mDjScrollView;
    Transform mFrameListTrans;
    Transform mFrameGridTrans;
    List<ItemDataEx> mFrameList;
    int mFrameIndex;
    FrameDataEx mNewFrameData;
    FrameDataEx mAddToFrameData;
    FrameDataEx mSelectFrameData;
    FrameDataEx mCopyFrameData;
    DuoJi mSelectDuoJi;
    Vector4 mFrameListStartClip;
    UIPanel mFrameListPanel;

    ActionSequence mSimulationActs = null;
    int mNowPlayObjectIndex = 0;
    int mNowPlayDataIndex = 0;
    bool isPlaying = false;//正在播放
    bool isPause = false;//暂停
    UISprite mPlayBtnIcon = null;
    UIButton mPlayBtn = null;

    bool mSwitchEditFlag = false;
    //static ActionSequence mOpenActions = null;

    GameObject mFramePrefab;

    BoxCollider mFixBgBoxCollider;
    TriggerEvent mBgTriggerEvent;
    List<TriggerEvent> mFrameTriggerList;
    TriggerDelegate mTriggerDelegate;
    List<TweenScale> mFrameDjScaleList;

    List<DuoJi> mCacheDjList;
    Dictionary<string, DuoJi> mFrameUseDjDict;

    UIButton mDelBtn;
    UIButton mPasteBtn;
    UIButton mCopyBtn;

    UIButton mSaveBtn;
    UIButton mSaveAsBtn;


    Vector2 mFixLeftSize;
    Vector2 mFixRightSize;
    UISprite mFixBgSprite;

    //动作帧的进度条
    Transform mFrameScrollBarTrans;
    UIScrollBar mFrameScrollBar;
    UISprite mFrameScrollBarBg;
    UISprite mFrameScrollBarBgBg;
    UISprite mFrameScrollBarFg;

    AnimationCurve mAnimationCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(3, 0.3f, 0, 0), new Keyframe(5, 1, 0, 0));
    float mStartTime = 0;
    int mSpeedFactor = 1;//速度因子:按住速度变化的因子数
    int maxFactor = 30;//每秒最大增加的角度

    int Time_Min = 80;
    int Time_Max = 5000;

    int Edit_Frame_Width = 312;
    int Edit_Frame_Height = 162;

    int Edit_DuoJi_Width = 210;
    int Edit_DuoJi_Not_Open_Width = 140;

    int Small_Frame_Width = 130;
    int Small_Frame_Height = 150;

    int Small_DuoJi_Width = 130;

    Vector2 Edit_Min = Vector2.zero;
    Vector2 Edit_Max = Vector2.zero;

    string Frame_Select_Bg_Sprite = "bg_frame_sel";
    string Frame_Normal_Bg_Sprite = "bg_smll";

    float mLastSendDjRota = 0;

    ActionSequence mActions = null;
    UISprite mActionIcon = null;
    UILabel mActionNameLabel = null;

    bool isOfficial = false;

    int mLastFrameTime = 0;

    CamRotateAroundCircle mCamRotate;
    #endregion

    #region 公有函数
    public ActionEditUI()
    {
        mUIResPath = "Prefab/UI/editFrameUI";
    }

    public void OpenActions(string id)
    {
        try
        {
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            if (null != mRobot)
            {
                ActionSequence act = mRobot.GetActionsForID(id);
                if (null != act)
                {
                    InitItems(new ActionSequence(act, mRobot));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public void CreateActions(string name, string iconName)
    {
        try
        {
            if (null == mActions)
            {
                string robotID = string.Empty;
                mRobot = RobotManager.GetInst().GetCurrentRobot();
                if (null != mRobot)
                {
                    robotID = mRobot.ID;
                }
                mActions = new ActionSequence(robotID);
            }
            mActions.Name = name;
            mActions.IconID = iconName;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }
    #endregion

    #region 其他函数
    protected override void AddEvent()
    {
        try
        {
            PlatformMgr.Instance.MobPageStart(MobClickEventID.P8);
            base.AddEvent();
            
            EventMgr.Inst.Regist(EventID.UI_Save_Actions, OnSaveActions);
            EventMgr.Inst.Regist(EventID.Quit_Edit_Actions_Scene, QuitEditScene);
            EventMgr.Inst.Regist(EventID.Read_Back_Msg_Ack_Success, OnReadBackSuccess);
            EventMgr.Inst.Regist(EventID.Read_Back_Msg_Ack_Failed, OnReadBackFailed);
            EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
            EventMgr.Inst.Regist(EventID.Stop_Robot_Actions, StopPlayActions);
            EventMgr.Inst.Regist(EventID.Read_Power_Msg_Ack, OnReadPowerResult);
            //EventMgr.Inst.Regist(EventID.Read_Back_Msg_Ack, OnReadBackAck);
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            isEditState = true;
            mDragStartFlag = false;
            isPlaying = false;
            isPause = false;
            mLastFrameTime = PublicFunction.Default_Actions_Time;
            SetEventActive(true);
            mSwitchEditFlag = false;
            mTriggerDelegate = new TriggerDelegate();
            mTriggerDelegate.onEnter = OnTriggerEnter;
            mTriggerDelegate.onExit = OnTriggerExit;
            mTriggerDelegate.onStay = OnTriggerStay;
            mCacheDjList = new List<DuoJi>();
            if (null == mFrameList)
            {
                mFrameList = new List<ItemDataEx>();
            }
            mFrameIndex = mFrameList.Count + 1;
            mFrameUseDjDict = new Dictionary<string, DuoJi>();
            if (null != mActions && (mActions.IsOfficial()))
            {//官方动作，不能编辑
                isOfficial = true;
            }
            else
            {
                isOfficial = false;
            }
            if (null != mTrans)
            {
                Transform tmpTrans = mTrans.Find("readback");
                if (null != tmpTrans)
                {
                    mReadBackObj = tmpTrans.gameObject;
                    Transform readbackbg = tmpTrans.Find("readbackbg");
                    if (null != readbackbg)
                    {
                        UISprite readBg = readbackbg.GetComponent<UISprite>();
                        if (null != readBg)
                        {
                            readBg.width = PublicFunction.GetExtendWidth();
                            readBg.height = PublicFunction.GetExtendHeight();
                            readBg.color = PublicFunction.PopWinColor;
                            readBg.alpha = PublicFunction.PopWin_Alpha;
                        }
                        BoxCollider box = readbackbg.GetComponent<BoxCollider>();
                        if (null != box)
                        {
                            box.size = new Vector2(PublicFunction.GetExtendWidth(), PublicFunction.GetExtendHeight());
                        }
                    }
                   
                    mReadBackBtnTrans = tmpTrans.Find("readbackbtn");
                    if (null != mReadBackBtnTrans)
                    {
                        mReadBackBtnTweenRotation = mReadBackBtnTrans.GetComponent<TweenRotation>();
                        mReadBackBtnTweenScale = mReadBackBtnTrans.GetComponent<TweenScale>();
                        //mReadBackBtnPosition = mReadBackBtnTrans.GetComponent<TweenPosition>();
                        mBigEffectTweenScale = GameHelper.FindChildComponent<TweenScale>(mReadBackBtnTrans, "effect/bg1");
                        mSmallEffectTweenScale = GameHelper.FindChildComponent<TweenScale>(mReadBackBtnTrans, "effect/bg2");
                    }
                    Transform tips = tmpTrans.Find("tips");
                    if (null != tips)
                    {
                        mReadBackTipsTweenPosition = tips.GetComponent<TweenPosition>();
                        mReadBackTipsTweenAlpha = tips.GetComponent<TweenAlpha>();
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(tips, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("记录机器人的姿势");
                        }
                    }
                }
                Transform title = mTrans.Find("title");
                if (null != title)
                {
                    mTitlePosition = title.GetComponent<TweenPosition>();
                    mTitleGameObject = title.gameObject;
                    Transform topLeft = title.Find("TopLeft");
                    if (null != topLeft)
                    {
                        Transform savebtn = topLeft.Find("savebtn");
                        if (null != savebtn)
                        {
                            mSaveBtn = savebtn.GetComponent<UIButton>();
                            if (!PlatformMgr.Instance.EditFlag)
                            {
                                savebtn.gameObject.SetActive(false);
                                Transform backbtn = topLeft.Find("backbtn");
                                if (null != backbtn)
                                {
                                    backbtn.localPosition = Vector3.zero;
                                }
                            }
                        }
                        TweenPosition tween = topLeft.GetComponent<TweenPosition>();
                        if (null != tween)
                        {
                            Vector3 pos = UIManager.GetWinPos(topLeft, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                            topLeft.localPosition = pos - new Vector3(300, 0);
                            GameHelper.PlayTweenPosition(tween, pos, 0.6f);
                        }
                        else
                        {
                            topLeft.localPosition = UIManager.GetWinPos(topLeft, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        }
                    }
                   
                    Transform topRight = title.Find("TopRight");
                    if (null != topRight)
                    {
                        TweenPosition tween = topRight.GetComponent<TweenPosition>();
                        if (null != tween)
                        {
                            Vector3 pos = UIManager.GetWinPos(topRight, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                            topRight.localPosition = pos + new Vector3(300, 0);
                            GameHelper.PlayTweenPosition(tween, pos, 0.6f);
                        }
                        else
                        {
                            topRight.localPosition = UIManager.GetWinPos(topRight, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        }
                        //mConnectBtn = GameHelper.FindChildComponent<UIButton>(topRight, "connectbtn");
                        //mConnectIcon = GameHelper.FindChildComponent<UISprite>(topRight, "connectbtn/bg");
                        //mConnectLabel = GameHelper.FindChildComponent<UILabel>(topRight, "connectbtn/Label");
                        //SetConnectState();
                        mDelBtn = GameHelper.FindChildComponent<UIButton>(topRight, "delbtn");
                        SetBtnState(mDelBtn, false);
                        mCopyBtn = GameHelper.FindChildComponent<UIButton>(topRight, "copybtn");
                        SetBtnState(mCopyBtn, false);
                        mPasteBtn = GameHelper.FindChildComponent<UIButton>(topRight, "pastebtn");
                        SetBtnState(mPasteBtn, false);
                    }
                }
                Transform dj = mTrans.Find("dj");
                if (null != dj)
                {
                    mDjGameObject = dj.gameObject;
                    UISprite bg = GameHelper.FindChildComponent<UISprite>(dj, "bg");
                    if (null != bg)
                    {
                        bg.width = PublicFunction.GetExtendWidth();
                        UISprite shadow = GameHelper.FindChildComponent<UISprite>(bg.transform, "shadow");
                        if (null != shadow)
                        {
                            shadow.width = bg.width;
                        }
                    }
                    mDjGameObjectEditPos = UIManager.GetWinPos(dj, UIWidget.Pivot.Bottom);
                    
                    //mDjTransSize = NGUIMath.CalculateRelativeWidgetBounds(dj).size;
                    mDjTweenPosition = dj.GetComponent<TweenPosition>();
                    Transform djlist = dj.Find("djlist");
                    if (null != djlist)
                    {
                        djlist.gameObject.SetActive(true);
                    }
                    //舵机滑动条
                    mDjParentTrans = dj.Find("djlist/grid");
                    if (null != mDjParentTrans)
                    {
                        int widthSpace = 90;
                        mDjScrollView = mDjParentTrans.GetComponent<UIScrollView>();
                        
                        UIPanel panel = mDjParentTrans.GetComponent<UIPanel>();
                        //mDjlistGrid = mDjParentTrans.GetComponent<UIGrid>();
                        Vector3 pos = mDjParentTrans.localPosition;
                        pos.x = -PublicFunction.GetWidth() / 2 + widthSpace;
                        /*float cellWidth = 0;
                        if (null != mDjlistGrid)
                        {
                            cellWidth = mDjlistGrid.cellWidth / 2;
                            pos.x += cellWidth;
                        }*/
                        mDjParentTrans.localPosition = pos;
                        //适配裁剪区域

                        Vector4 rect = panel.finalClipRegion;
                        rect.z = PublicFunction.GetWidth() - (widthSpace * 2);
                        rect.x = rect.z / 2;
                        panel.baseClipRegion = rect;
                    }
                    mDjGameObject.SetActive(isEditState);
                    
                }
                Transform fix = mTrans.Find("fix");
                if (null != fix)
                {
                    mFixTweenPosition = fix.GetComponent<TweenPosition>();
                    fix.localPosition = UIManager.GetWinPos(fix, UIWidget.Pivot.Bottom, 0, -1);
                    mFixEditPos = fix.localPosition;
                    Transform left = fix.Find("left");
                    if (null != left)
                    {
                        Vector3 leftPos = left.localPosition;
                        mFixLeftSize = NGUIMath.CalculateRelativeWidgetBounds(left).size;
                        leftPos.x = UIManager.GetWinPos(left, UIWidget.Pivot.Left, PublicFunction.Back_Btn_Pos.x).x;
                        left.localPosition = leftPos;
                        Transform playbtn = left.Find("playbtn");
                        if (null != playbtn)
                        {
                            mPlayBtn = playbtn.GetComponent<UIButton>();
                            mPlayBtnIcon = GameHelper.FindChildComponent<UISprite>(playbtn, "Background");
                            mPlaybtnTweenPosition = playbtn.GetComponent<TweenPosition>();
                        }
                        //mStretchRotation = GameHelper.FindChildComponent<TweenRotation>(left, "stretchbtn");
                        mPowerDownBtnTrans = left.Find("powerdownbtn");
                        if (null != mPowerDownBtnTrans)
                        {
                            mPowerDownBtn = mPowerDownBtnTrans.GetComponent<UIButton>();
                            if (null != mPowerDownBtn)
                            {
                                if (PlatformMgr.Instance.GetBluetoothState() && null != mRobot && mRobot.GetAllDjData().GetAngleList().Count > 0)
                                {
                                    mPowerDownBtn.OnAwake();
                                }
                                else
                                {
                                    mPowerDownBtn.OnSleep();
                                }
                            }
                        }
                    }

                    mStretchRotation = GameHelper.FindChildComponent<TweenRotation>(fix, "stretchbtn");
                    if (null != mStretchRotation)
                    {
                        mStretchRotation.transform.localPosition = new Vector3(0, 260, 0);
                    }

                    mFrameListTrans = fix.Find("framelist");
                    if (null != mFrameListTrans)
                    {
                        Vector3 listPos = mFrameListTrans.localPosition;
                        listPos.y = 60;
                        listPos.x = (mFixLeftSize.x + PublicFunction.Back_Btn_Pos.x) / 2;
                        mFrameListTrans.localPosition = listPos;
                        mFrameScrollViewEx = mFrameListTrans.GetComponent<UIScrollViewEx>();
                        if (null != mFrameScrollViewEx)
                        {
                            mFrameScrollViewEx.onStoppedMoving = OnFrameMoveStop;
                        }
                        mFrameSpringPanel = mFrameListTrans.GetComponent<SpringPanel>();
                        mFrameListPanel = mFrameListTrans.GetComponent<UIPanel>();
                        mFrameListStartClip = mFrameListPanel.finalClipRegion;
                        mFrameListStartClip.z = PublicFunction.GetWidth() - mFixLeftSize.x - PublicFunction.Back_Btn_Pos.x - PublicFunction.Iphonex_Add_Offset.x/* + 40*/;
                        mFrameListStartClip.x -= PublicFunction.Iphonex_Add_Offset.x / 2;
                        //mFrameListPanel.baseClipRegion = mFrameListStartClip;
                        //mFrameListPanel.clipOffset = Vector2.zero;
                        mFrameListPosition = mFrameListTrans.GetComponent<TweenPosition>();
                        mFrameGridTrans = mFrameListTrans.Find("grid");
                        if (null != mFrameGridTrans)
                        {
                            Vector3 pos = mFrameGridTrans.localPosition;
                            pos.x = -mFrameListStartClip.z / 2 + mFrameListPanel.clipSoftness.x + 1 - PublicFunction.Iphonex_Add_Offset.x / 2;
                            mFrameGridTrans.localPosition = pos;
                            mFrameCustomGrid = mFrameGridTrans.GetComponent<CustomGrid>();
                            mFrameCustomGrid.StartPos = mFrameListTrans.localPosition;
                            mFrameGridTweenScale = mFrameGridTrans.GetComponent<TweenScale>();
                        }
                    }

                    Transform start = fix.Find("start");
                    if (null != start)
                    {
                        mStartObj = start.gameObject;
                        if (null != mFrameListTrans)
                        {
                            start.localPosition = mFrameListTrans.localPosition;
                        }
                        UISprite sp = GameHelper.FindChildComponent<UISprite>(start, "bg");
                        if (null != sp)
                        {
                            sp.width = (int)mFrameListStartClip.z - 100;
                            SetDottedBox(sp.transform, sp.width, sp.height);
                        }
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(start, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("请将舵机拖拽到这里");
                        }
                    }
                    
                    Transform bg = fix.Find("bg");
                    if (null != bg)
                    {
                        
                        Transform bgTop = bg.Find("bgTop");
                        if (null != bgTop)
                        {
                            bgTop.gameObject.SetActive(true);
                        }
                        mFixBgSprite = bg.GetComponent<UISprite>();
                        if (null != mFixBgSprite)
                        {
                            mFixBgSprite.width = PublicFunction.GetExtendWidth();
                            /*UISprite bgbg = GameHelper.FindChildComponent<UISprite>(mFixBgSprite.transform, "bg");
                            if (null != bgbg)
                            {
                                bgbg.width = mFixBgSprite.width;
                            }*/
                        }
                        mFixBgBoxCollider = bg.GetComponent<BoxCollider>();
                        mBgTriggerEvent = bg.GetComponent<TriggerEvent>();
                        if (null != mBgTriggerEvent)
                        {
                            mBgTriggerEvent.triggerdlgt = mTriggerDelegate;
                            mBgTriggerEvent.enabled = false;
                        }
                        Vector3 pos = UIManager.GetWinPos(mFixTweenPosition.transform, UIWidget.Pivot.Bottom/*, 0, mDjTransSize.y*/) + bg.localPosition;
                        Edit_Min = new Vector2(-PublicFunction.GetWidth() / 2 + mFixLeftSize.x, -mFixBgSprite.height / 2 + pos.y + 100);
                        Edit_Max = new Vector2(PublicFunction.GetWidth() / 2, mFixBgSprite.height / 2 + pos.y - 20);
                    }
                                       

                    mFrameScrollBarTrans = fix.Find("framescrollbar");
                    if (null != mFrameScrollBarTrans)
                    {
                        mFrameScrollBar = mFrameScrollBarTrans.GetComponent<UIScrollBar>();
                        mFrameScrollBarTweenPos = mFrameScrollBarTrans.GetComponent<TweenPosition>();
                        mFrameScrollBarBg = GameHelper.FindChildComponent<UISprite>(mFrameScrollBarTrans, "Background");
                        if (null != mFrameScrollBarBg)
                        {
                            //mFrameScrollBarBgBg = GameHelper.FindChildComponent<UISprite>(mFrameScrollBarBg.transform, "bg");
                            ButtonEvent btn1 = mFrameScrollBarBg.GetComponent<ButtonEvent>();
                            if (null!= btn1)
                            {
                                GameObject.Destroy(btn1);
                            }
                            mFrameScrollBarBg.alpha = 0;
                        }
                        mFrameScrollBarFg = GameHelper.FindChildComponent<UISprite>(mFrameScrollBarTrans, "Foreground");
                        if (null != mFrameScrollBarFg)
                        {
                            ButtonEvent btn1 = mFrameScrollBarFg.GetComponent<ButtonEvent>();
                            if (null != btn1)
                            {
                                GameObject.Destroy(btn1);
                            }
                            mFrameScrollBarFg.alpha = 0;
                        }
                        int frameWidth = (int)mFrameListStartClip.z - 60;
                        if (null != mFrameScrollBarBg)
                        {
                            mFrameScrollBarBg.width = frameWidth;
                            /*if (null != mFrameScrollBarBgBg)
                            {
                                mFrameScrollBarBgBg.width = mFrameScrollBarBg.width;
                            }*/
                        }
                        if (null != mFrameScrollBarFg)
                        {
                            mFrameScrollBarFg.width = frameWidth;
                        }
                        if (null != mFrameScrollBarTrans)
                        {
                            Vector3 pos = mFrameScrollBarTrans.localPosition;
                            pos.x = PublicFunction.GetWidth() / 2 - frameWidth / 2 - 30;
                            mFrameScrollBarTrans.localPosition = pos;
                        }
                    }
                }
                Transform duoji = mTrans.Find("duoji");
                if (null != duoji)
                {
                    mDuojiPrefab = duoji.gameObject;
                }
                Transform frame = mTrans.Find("frame");
                if (null != frame)
                {
                    mFramePrefab = frame.gameObject;
                }
                Transform time = mTrans.Find("time");
                if (null != time)
                {
                    mTimeObj = time.gameObject;
                    UISprite timebg = GameHelper.FindChildComponent<UISprite>(time, "timebg");
                    float height = NGUIMath.CalculateRelativeWidgetBounds(mTimeObj.transform).size.y;
                    if (null != timebg)
                    {
                        timebg.width = PublicFunction.GetExtendWidth();
                        UISprite shadow = GameHelper.FindChildComponent<UISprite>(timebg.transform, "shadow");
                        if (null != shadow)
                        {
                            shadow.width = timebg.width;
                        }
                        height = timebg.height;
                        UISprite timeShdow = GameHelper.FindChildComponent<UISprite>(timebg.transform, "bg");
                        if (null != timeShdow)
                        {
                            timeShdow.width = timebg.width;
                        }
                    }
                    mTimeShowPos = UIManager.GetWinPos(time, UIWidget.Pivot.Bottom);
                    mTimeHidePos = mTimeShowPos - new Vector3(0, height);
                    time.localPosition = mTimeHidePos;
                    mTimeBoxObj = time.Find("time").gameObject;
                    UITexture tex = UIManager.AddUITexture(mTimeBoxObj, 0.01f, Color.white);
                    mTimeBoxObj.GetComponent<BoxCollider>().size = new Vector2(tex.width, tex.height);
                    Camera cam = NGUITools.FindInParents<Camera>(mTrans);
                    if (null != cam)
                    {
                        mTimeBoxObj.transform.position = cam.transform.position;
                    }
                    else
                    {
                        mTimeBoxObj.transform.localPosition = Vector3.zero;
                    }
                    
                    
                    mTimeBarPosition = time.GetComponent<TweenPosition>();

                    Transform subtimebtn = time.Find("subtimebtn");
                    if (null != subtimebtn)
                    {
                        Vector3 pos = subtimebtn.localPosition;
                        pos.x = UIManager.GetWinPos(subtimebtn, UIWidget.Pivot.Left, PublicFunction.Margin_Normal.x).x;
                        subtimebtn.localPosition = pos;
                    }
                    Transform addtimebtn = time.Find("addtimebtn");
                    if (null != addtimebtn)
                    {
                        Vector3 pos = addtimebtn.localPosition;
                        pos.x = UIManager.GetWinPos(addtimebtn, UIWidget.Pivot.Right, PublicFunction.Margin_Normal.x).x;
                        addtimebtn.localPosition = pos;
                    }
                    mTimeBar = GameHelper.FindChildComponent<UIScrollBar>(time, "timebar");
                    int barWidth = PublicFunction.GetWidth() - 240 - (int)(PublicFunction.Iphonex_Add_Offset.x * 2);
                    float barFgWidth = 50.0f;
                    if (null != mTimeBar)
                    {
                        mTimeBar.barSize = barFgWidth / barWidth;
                        UISprite bg = GameHelper.FindChildComponent<UISprite>(mTimeBar.transform, "Background");
                        if (null != bg)
                        {
                            //删除按钮监听
                            ButtonEvent btnEvent = bg.transform.GetComponent<ButtonEvent>();
                            if (null != btnEvent)
                            {
                                GameObject.Destroy(btnEvent);
                            }
                            bg.width = barWidth;
                            UISprite bgbg = GameHelper.FindChildComponent<UISprite>(bg.transform, "bg");
                            if (null != bgbg)
                            {
                                bgbg.width = barWidth;
                            }
                            UISprite bgline = GameHelper.FindChildComponent<UISprite>(bg.transform, "bgline");
                            if (null != bgline)
                            {
                                bgline.width = barWidth - 100;
                            }
                            BoxCollider box = bg.transform.GetComponent<BoxCollider>();
                            if (null != box)
                            {
                                Vector3 boxSize = box.size;
                                boxSize.x = barWidth;
                                box.size = boxSize;
                            }
                        }
                        
                        UISprite fg = GameHelper.FindChildComponent<UISprite>(mTimeBar.transform, "Foreground");
                        if (null != fg)
                        {
                            //删除按钮监听
                            ButtonEvent btnEvent = fg.transform.GetComponent<ButtonEvent>();
                            if (null != btnEvent)
                            {
                                GameObject.Destroy(btnEvent);
                            }
                            fg.width = barWidth;
                        }

                        EventDelegate.Add(mTimeBar.onChange, TimeBarOnChange);
                        mTimeBar.onDragFinished = OnTimeBarDragFinished;
                        //mTimeBarBox = GameHelper.FindChildComponent<BoxCollider>(mTimeBar.transform, "Foreground");
                    }
                    mTimeObj.SetActive(false);
                    /* UILabel mintime = GameHelper.FindChildComponent<UILabel>(time, "mintime");
                     if (null != mintime)
                     {
                         mTimeMin = mintime.transform;
                         Vector3 pos = mTimeMin.localPosition;
                         pos.x = -(barWidth - barFgWidth) / 2;
                         mTimeMin.localPosition = pos;
                         mintime.text = Time_Min + "ms";
                     }
                     UILabel maxtime = GameHelper.FindChildComponent<UILabel>(time, "maxtime");
                     if (null != maxtime)
                     {
                         mTimeMax = maxtime.transform;
                         Vector3 pos = mTimeMax.localPosition;
                         pos.x = (barWidth - barFgWidth) / 2;
                         mTimeMax.localPosition = pos;
                         maxtime.text = Time_Max + "ms";
                     }
                     mTimeLabel = GameHelper.FindChildComponent<UILabel>(time, "nowtime");*/
                }
                InitDjList();
                ShowEditAction(true, true);
                //SetFrameListEdit(isEditState, true);
                if (null != mFrameCustomGrid)
                {
                    mFrameCustomGrid.Init(mFrameList, InitFrameEx, CreateItemEx);
                    UpdateScrollViewEnabled();
                    UpdateScrollBar();
                    
                    if (mFrameList.Count > 0)
                    {
                        if (null != mStartObj && mStartObj.activeSelf)
                        {
                            mStartObj.SetActive(false);
                        }
                    }
                }
                //UpdatePowerDownPos();
                ChangeSaveBtnState();
                //Timer.Add(10, 1, 1, DelayInit);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void DelayInit()
    {
        ShowEditAction(true, true);
    }

    public override void Release()
    {
        base.Release();
        if (null != mTimeBar)
        {
            EventDelegate.Remove(mTimeBar.onChange, TimeBarOnChange);
        }
        EventMgr.Inst.UnRegist(EventID.UI_Save_Actions, OnSaveActions);
        EventMgr.Inst.UnRegist(EventID.Quit_Edit_Actions_Scene, QuitEditScene);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.UnRegist(EventID.Read_Back_Msg_Ack_Success, OnReadBackSuccess);
        EventMgr.Inst.UnRegist(EventID.Read_Back_Msg_Ack_Failed, OnReadBackFailed);
        EventMgr.Inst.UnRegist(EventID.Stop_Robot_Actions, StopPlayActions);
        EventMgr.Inst.UnRegist(EventID.Read_Power_Msg_Ack, OnReadPowerResult);
        //EventMgr.Inst.UnRegist(EventID.Read_Back_Msg_Ack, OnReadBackAck);
        PlatformMgr.Instance.MobPageEnd(MobClickEventID.P8);
    }

    protected override void OnButtonClick(GameObject obj)
    {
        try
        {
            if (mSwitchEditFlag || null != mFrameSpringPanel && mFrameSpringPanel.enabled)
            {//是否该提示当前状态不能操作
                return;
            }
            bool cancelDj = true;
            bool canelFrame = true;
            base.OnButtonClick(obj);
            string name = obj.name;
            if (name.Equals("savebtn") || name.Equals("savebtn1"))
            {//保存
                if (null != mRobot)
                {
                    if (mFrameList.Count < 1)
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("DongZuoWeiKong"), 1, CommonTipsColor.red);
                    }
                    else
                    {
                        CreateActionsUI.ShowMsg(CreateActionsUI.ActionsMsgType.Actions_Msg_Save, mRobot.ID, mActions, OnQuitEdit);
                    }
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("未选择模型"));
                }
            }
            else if (name.Equals("saveasbtn"))
            {
                if (null != mRobot)
                {
                    if (mFrameList.Count < 1)
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("DongZuoWeiKong"), 1, CommonTipsColor.red);
                    }
                    else
                    {
                        CreateActionsUI.ShowMsg(CreateActionsUI.ActionsMsgType.Actions_Msg_SaveAs, mRobot.ID, mActions);
                    }
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("未选择模型"));
                }
            }
            else if (name.Equals("backbtn"))
            {//返回
                QuitEditScene(null);
            }
            else if (name.Equals("playbtn"))
            {//仿真
                if (isPlaying)
                {
                    isPause = true;
                    isPlaying = false;
                }
                else if (null == mSimulationActs)
                {
                    if (null != mFrameList && mFrameList.Count > 0)
                    {
                        //canelFrame = false;
                        Simulation(mSelectFrameData);
                    }
                }
                else
                {
                    isPlaying = true;
                    if (isPause)
                    {//正在暂停,取消暂停
                        isPause = false;
                    }
                    else
                    {//暂停完毕了
                        ContinueAction();
                    }
                }
                SetPlayIconState(isPlaying);
            }
            else if (isPlaying)
            {
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("请先停止播放"), 1.5f, CommonTipsColor.red);
            }
            else if (name.StartsWith("frame_") && isEditState)
            {//点中了动作帧
                FrameDataEx data = (FrameDataEx)mFrameCustomGrid.GetItemData(name);
                if (null != data)
                {
                    //展开动作帧,选中动作帧
                    SetSelectFrame(data, true);
                    if (null != data.showList && data.showList.Count == 1)
                    {//单个舵机的帧，选中相当于选中舵机
                        try
                        {
                            Transform dj = obj.transform.Find("djlist").GetChild(0);
                            if (null != dj && mFrameUseDjDict.ContainsKey(dj.name))
                            {
                                SetSelectDuoJi(mFrameUseDjDict[dj.name]);
                                cancelDj = false;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                        }
                    }
                    canelFrame = false;
                }
            }
            else if (name.StartsWith("fd_") && isEditState)
            {//点中了帧上的舵机
                string frameName = GetFrameForDj(name);
                FrameDataEx data = (FrameDataEx)mFrameCustomGrid.GetItemData(frameName);
                if (null != data)
                {
                    if (/*isEditState && */(data.isOpen || null != data.showList && data.showList.Count <= 1))
                    {
                        if (mFrameUseDjDict.ContainsKey(name))
                        {
                            SetSelectDuoJi(mFrameUseDjDict[name]);
                            cancelDj = false;
                        }
                    }
                    //展开动作帧,选中动作帧
                    SetSelectFrame(data, true);
                    canelFrame = false;
                }
            }
            else if (name.Equals("powerdownbtn"))
            {//掉电
                if (null != mRobot)
                {
                    mRobot.RobotPowerDown();
                }
                OpenReadBack();
            }
            else if (name.Equals("readbackbg"))
            {//取消回读
                CloseReadBack();
            }
            else if (name.Equals("readbackbtn"))
            {//回读
                if (null != mRobot)
                {
                    mRobot.ReadBack(ExtendCMDCode.ReadBack);
                }
            }
            else if (name.Equals("subBtn"))
            {
                cancelDj = false;
                canelFrame = false;
                ChangeDuoJiAngle(-1, obj.transform.parent.parent);
            }
            else if (name.Equals("addBtn"))
            {
                cancelDj = false;
                canelFrame = false;
                ChangeDuoJiAngle(1, obj.transform.parent.parent);
            }
            else if (name.Equals("delbtn"))
            {
                if (null != mSelectDuoJi)
                {//删除舵机
                    string frameName = GetFrameForDj(mSelectDuoJi.Name);
                    if (null != mFrameCustomGrid)
                    {
                        FrameDataEx data = (FrameDataEx)mFrameCustomGrid.GetItemData(frameName);
                        if (null != data)
                        {
                            if (null != data.showList && data.showList.Count > 1)
                            {
                                DelActionDuoJi(data, mSelectDuoJi);
                            }
                            else
                            {//删除帧
                                DelActionFrame(data);
                                mSelectFrameData = null;
                            }
                            mSelectDuoJi = null;
                        }
                    }
                }
                else if (null != mSelectFrameData)
                {//删除帧
                    DelActionFrame(mSelectFrameData);
                    mSelectFrameData = null;
                }
            }
            else if (name.Equals("copybtn"))
            {
                if (null != mSelectFrameData)
                {//可复制
                    canelFrame = false;
                    mCopyFrameData = mSelectFrameData.OnCopy(mRobot);
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("复制成功"), 1.5f, CommonTipsColor.green);
                }
                if (null != mSelectDuoJi)
                {
                    cancelDj = false;
                }
            }
            else if (name.Equals("pastebtn"))
            {
                if (null != mSelectFrameData && null != mCopyFrameData)
                {//粘贴
                    canelFrame = false;
                    bool selectDjFlag = PasteFrame();
                    if (selectDjFlag)
                    {
                        cancelDj = false;
                    }
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("粘贴成功"), 1.5f, CommonTipsColor.green);
                }
            }
            else if (name.Equals("timeedit"))
            {
                if (mTimeObj.activeSelf)
                {
                    return;
                }
                Transform frame = obj.transform.parent;
                if (null != frame)
                {
                    FrameDataEx data = (FrameDataEx)mFrameCustomGrid.GetItemData(frame.name);
                    //SetSelectDuoJi(null);
                    SetSelectFrame(data);
                    canelFrame = false;
                    if (null != mSelectDuoJi)
                    {
                        cancelDj = false;
                    }
                    if (null != mTimeObj)
                    {
                        SetTimeBarActive(!mTimeObj.activeSelf);
                    }
                }
            }
            else if (name.Equals("time"))
            {
                if (!mTimeObj.activeSelf)
                {
                    return;
                }
                if (null != mSelectFrameData)
                {
                    canelFrame = false;
                    if (null != mSelectDuoJi)
                    {
                        cancelDj = false;
                    }
                    mSelectFrameData.action.sportTime = BarValueToTime();
                    mLastFrameTime = mSelectFrameData.action.sportTime;
                    if (null != mFrameCustomGrid)
                    {
                        ItemObjectEx item = mFrameCustomGrid.GetItemObject(mSelectFrameData);
                        if (null != item)
                        {
                            SetFrameTime(item.itemObj.transform, mSelectFrameData.action.sportTime);
                        }
                    }
                }
                SetTimeBarActive(false);
            }
            else if (name.Equals("subtimebtn"))
            {
                canelFrame = false;
                if (null != mSelectDuoJi)
                {
                    cancelDj = false;
                }
                SetTimeBarTime(mNowTime - 20);
            }
            else if (name.Equals("addtimebtn"))
            {
                canelFrame = false;
                if (null != mSelectDuoJi)
                {
                    cancelDj = false;
                }
                SetTimeBarTime(mNowTime + 20);
            }
            else if (name.Equals("timebg"))
            {
                canelFrame = false;
                if (null != mSelectDuoJi)
                {
                    cancelDj = false;
                }
            }
            else if (name.Equals("stretchbtn"))
            {//切换编辑状态
                ShowEditAction(!isEditState);
            }
            /*else if (name.Equals("connectbtn"))
            {//连接
                PublicPrompt.ShowClickBlueBtnMsg();
            }*/
            else if (name.Equals("resetbtn"))
            {//复位
                if (null == mCamRotate)
                {
                    GameObject tmpCam = GameObject.Find("MainCamera");
                    if (null != tmpCam)
                    {
                        mCamRotate = tmpCam.GetComponent<CamRotateAroundCircle>();
                    }
                }
                if (null != mCamRotate)
                {
                    mCamRotate.ResetOriState();
                }
            }
            else if (name.Equals("readsnbtn"))
            {
                if (null != mRobot)
                {
                    mRobot.ReadSnInfo();
                }
            }
            else if (name.Equals("writesnbtn"))
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("isSuccess", "1");
                dict.Add("reason", "0");
                dict.Add("sn", System.Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16));
                PlatformMgr.Instance.RegisterRobotResult(dict);
            }
            else if (name.Equals("flushbtn"))
            {
                if (null != mRobot)
                {
                    mRobot.FlushFlashInfo();
                }
            }
            if (cancelDj)
            {
                SetSelectDuoJi(null);
            }
            if (canelFrame)
            {
                SetSelectFrame(null);
            }
            if (cancelDj && canelFrame)
            {
                SetBtnState(mDelBtn, false);
                SetBtnState(mCopyBtn, false);
                SetBtnState(mPasteBtn, false);
            }
            else
            {
                SetBtnState(mDelBtn, true);
                if (null != mSelectFrameData)
                {
                    SetBtnState(mCopyBtn, true);
                    if (null != mCopyFrameData)
                    {
                        SetBtnState(mPasteBtn, true);
                    }
                    else
                    {
                        SetBtnState(mPasteBtn, false);
                    }
                }
                else
                {
                    SetBtnState(mCopyBtn, false);
                    SetBtnState(mPasteBtn, false);
                }
            }

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void UpdateSpeedFactor()
    {
        float cur = mAnimationCurve.Evaluate(Time.time - mStartTime);
        mSpeedFactor = (int)(cur * maxFactor);
        mSpeedFactor = (mSpeedFactor < 1 ? 1 : mSpeedFactor);
    }

    protected override void OnDurationPress(GameObject obj)
    {
        base.OnDurationPress(obj);
        if (isPlaying || mSwitchEditFlag)
        {
            return;
        }
        string name = obj.name;
        if (name.Equals("subBtn"))
        {
            UpdateSpeedFactor();
            ChangeDuoJiAngle(-mSpeedFactor, obj.transform.parent.parent);
        }
        else if (name.Equals("addBtn"))
        {
            UpdateSpeedFactor();
            ChangeDuoJiAngle(mSpeedFactor, obj.transform.parent.parent);
        }
        else if (name.Equals("subtimebtn"))
        {
            UpdateSpeedFactor();
            SetTimeBarTime(mNowTime - mSpeedFactor * 20);
        }
        else if (name.Equals("addtimebtn"))
        {
            UpdateSpeedFactor();
            SetTimeBarTime(mNowTime + mSpeedFactor * 20);
        }
    }

    protected override void OnButtonPress(GameObject obj, bool press)
    {
        base.OnButtonPress(obj, press);
        if (isPlaying || mSwitchEditFlag)
        {
            return;
        }
        try
        {
            if (press)
            {
                mDragStartFlag = true;
                mStartTime = Time.time;
            }
            else
            {
                mDragStartFlag = false;
                mStartTime = 0;
                mSpeedFactor = 1;
                if (null != mSelectDragDj)
                {
                    SetFrameTriggerEnabled(false);
                    if (null != mBgTriggerEvent)
                    {
                        mBgTriggerEvent.enabled = false;
                    }
                    do 
                    {
                        if (null != mAddToFrameData)
                        {
                            string name = mSelectDragDj.Name.Substring("drag_".Length);
                            if (mDuojiDict.ContainsKey(name))
                            {
                                mAddToFrameData.AddDj(mDuojiDict[name].Id);
                                /*if (mAddToFrameData.isOpen)
                                {
                                    mAddToFrameData.width = GetOpenFrameWidth(mAddToFrameData.showList.Count);
                                }
                                else
                                {
                                    mAddToFrameData.width = GetCloseFrameWidth(mAddToFrameData.showList.Count);
                                }*/

                                DuoJi data = mSelectDragDj;//new DuoJi(mSelectDragDj.transform, 0);
                                data.DeleteUIPanel();
                                data.SetScrollDragExActive(true);
                                data.SetDragObjectActive(false);
                                do
                                {
                                    if (null != mFrameGridTrans)
                                    {
                                        Transform frame = mFrameGridTrans.Find(mAddToFrameData.name);
                                        if (null != frame)
                                        {
                                            Transform djlist = frame.Find("djlist");
                                            TriggerEvent te = frame.GetComponent<TriggerEvent>();
                                            if (null != djlist)
                                            {//新增舵机
                                                te.enabled = true;
                                                mAddToFrameData.alphaFlag = false;
                                                SetFrameAddState(frame, mAddToFrameData);
                                                data.SetFrameData(mAddToFrameData.action.index, mDuojiDict[name].Id, mDuojiDict[name].Angle);
                                                data.trans.parent = djlist;
                                                data.trans.localEulerAngles = Vector3.zero;
                                                data.trans.localScale = Vector3.one;
                                                data.SetActive(false);
                                                
                                                FrameDuoJiGrid(mAddToFrameData, frame, false);
                                                //SpringPosition.Begin(data.trans.gameObject, Vector3.zero, 15);
                                                //TweenPosition tp = TweenPosition.Begin(data.trans.gameObject, 0.3f, Vector3.zero);
                                                //data.trans.localPosition = Vector3.zero;
                                                mFrameUseDjDict[data.trans.name] = data;
                                                if (mAddToFrameData != mNewFrameData)
                                                {
                                                    //ClientMain.GetInst().WaitTimeInvoke(0.15f, OnFinished);
                                                    Timer.Add(0.15f, 1, 1, OnFinished);
                                                    /*RemoveFrame(mNewFrameData);
                                                    mNewFrameData = null;*/
                                                }
                                                else
                                                {
                                                    mFrameCustomGrid.MoveToEnd(false);
                                                    mNewFrameData = null;
                                                }
                                                SetActionSaveFlag(true);
                                                EventMgr.Inst.Fire(EventID.UI_Set_Save_Actions_Btn_State, new EventArg(true));
                                                /*if (GuideViewBase.OpenOrCloseGuide)
                                                {
                                                    EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().DragDuojiStep, true));
                                                }*/
                                                break;
                                            }
                                        }
                                        
                                    }
                                    RemoveFrame(mNewFrameData);
                                    mNewFrameData = null;
                                    //RemoveFrameDj(mSelectDragDj.Name);
                                    mSelectDragDj.SetActive(false);
                                    mCacheDjList.Add(data);
                                } while (false);
                                break;
                            }
                        }
                        //RemoveFrameDj(mSelectDragDj.Name);
                        mSelectDragDj.SetActive(false);
                        mCacheDjList.Add(mSelectDragDj);
                        if (null != mNewFrameData)
                        {
                            RemoveFrame(mNewFrameData);
                            mNewFrameData = null;
                        }
                        /*if (StepManager.GetIns().OpenOrCloseGuide)
                        {
                            EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().DragDuojiStep, false));
                        }*/
                    } while (false);
                    mSelectDragDj = null;
                    mAddToFrameData = null;
                }
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 设置舵机角度
    /// </summary>
    /// <param name="rota">改变的角度</param>
    /// <param name="dj"></param>
    void ChangeDuoJiAngle(int rota, Transform dj)
    {
        if (null != dj)
        {
            Int2 ids = GetIDFordjName(dj.name);
            string frameName = GetFrameName(ids.num1);
            FrameDataEx data = (FrameDataEx)mFrameCustomGrid.GetItemData(frameName);
            if (null != data)
            {
                if (mFrameUseDjDict.ContainsKey(dj.name))
                {
                    DuoJi djdata = mFrameUseDjDict[dj.name];
                    djdata.SetAngle(djdata.Angle + rota);
                    data.action.UpdateRota((byte)ids.num2, (short)djdata.Angle);
                    SetDuoJiAngle(ids.num2, djdata.Angle);
                    if (null != mRobot)
                    {
                        mRobot.GetAllDjData().UpdateData((byte)ids.num2, (short)djdata.Angle);
                        SendRota(ids.num2, djdata.Angle, true);
                        Dictionary<int, int> dict = MoveSecond.GetDJLianDongData(ids.num2, djdata.Angle);
                        if (null != dict)
                        {
                            foreach (var kvp in dict)
                            {
                                DuoJiData otherData = mRobot.GetAnDjData(kvp.Key);
                                if (null != otherData)
                                {
                                    SetDuoJiAngle(otherData.id, otherData.rota);
                                    data.action.UpdateRota((byte)(otherData.id), (short)(otherData.rota));
                                }
                            }
                        }

                    }
                    EventMgr.Inst.Fire(EventID.UI_Set_Save_Actions_Btn_State, new EventArg(true));
                    SetActionSaveFlag(true);
                }
            }
        }
    }

    /// <summary>
    /// 通过滑动调节舵机角度
    /// </summary>
    /// <param name="data"></param>
    void DragSetDuoJiAngle(DuoJi data, bool finished)
    {
        Int2 ids = GetIDFordjName(data.Name);
        string frameName = GetFrameName(ids.num1);
        FrameDataEx frameData = (FrameDataEx)mFrameCustomGrid.GetItemData(frameName);
        if (null != frameData)
        {
            frameData.action.UpdateRota((byte)data.Id, (short)data.Angle);
            if (null != mRobot)
            {
                SetDuoJiAngle(data.Id, data.Angle);
                mRobot.GetAllDjData().UpdateData((byte)data.Id, (short)data.Angle);
                SendRota(data.Id, data.Angle, finished);
                Dictionary<int, int> dict = MoveSecond.GetDJLianDongData(data.Id, data.Angle);
                if (null != dict)
                {
                    foreach (var kvp in dict)
                    {
                        DuoJiData otherData = mRobot.GetAnDjData(kvp.Key);
                        if (null != otherData)
                        {
                            SetDuoJiAngle(otherData.id, otherData.rota);
                            frameData.action.UpdateRota((byte)(otherData.id), (short)(otherData.rota));
                        }
                    }
                }

            }
            EventMgr.Inst.Fire(EventID.UI_Set_Save_Actions_Btn_State, new EventArg(true));
            SetActionSaveFlag(true);
        }
        //Debug.Log(data.Angle);
        //if (Math.Abs(data.Angle - PublicFunction.DuoJi_Start_Rota) >= 30)
        //{
        //    if (StepManager.GetIns().OpenOrCloseGuide)
        //    {
        //        EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().TurnAroundDuojiStep, true));
        //    }
        //}
        /*if (StepManager.GetIns().OpenOrCloseGuide)
        {
            if (StepManager.GetIns().GetCurStepID() == 15 && data.Angle <= 30)
                EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().TurnAroundDuojiStep, true));
            else if (StepManager.GetIns().GetCurStepID() == 18 && data.Angle >= 120)
                EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().TurnAroundDuojiStep, true));
        }*/
    }

    void FrameDuoJiGrid(FrameDataEx data, Transform frame, bool instant, bool needUpdateFrame = true)
    {
        if (null == data.showList || data.showList.Count == 0)
        {
            return;
        }
        Transform djlist = frame.Find("djlist");
        Transform djNum = frame.Find("djNum");
        if (null != djlist)
        {
            if (null != data.showList && data.showList.Count == 1)
            {
                if (null != djNum)
                {
                    djNum.gameObject.SetActive(false);
                }
                for (int i = 0, imax = djlist.childCount; i < imax; ++i)
                {
                    if (i == 0)
                    {
                        Transform tmp = djlist.GetChild(i);
                        tmp.gameObject.SetActive(true);
                        //tmp.localScale = Vector3.one;
                        if (instant)
                        {
                            tmp.localPosition = Vector3.zero;
                        }
                        else
                        {
                            SpringPosition.Begin(tmp.gameObject, Vector3.zero, 15).onFinished = DjMoveFinished;
                        }
                    }
                    else
                    {
                        djlist.GetChild(i).gameObject.SetActive(false);
                    }
                }
                if (needUpdateFrame)
                {
                    UpdateFrameWidthForDjCount(data, frame);
                }
                
                return;
            }
            /*if (isEditState)
            {
                if (data.isOpen)
                {
                    width = Edit_DuoJi_Width;
                }
                else
                {
                    width = Edit_DuoJi_Not_Open_Width;
                }
            }
            else
            {
                width = Small_DuoJi_Width;
            }*/
            int space = 20;
            int count = 3;
            if (data.isOpen)
            {
                count = data.showList == null ? 0 : data.showList.Count;
            }
            else
            {
                count = count > djlist.childCount ? djlist.childCount : count;
            }
            if (null != djNum && (null == data.showList ||data.showList.Count <= 3))
            {
                djNum.gameObject.SetActive(false);
            }
            float allWidth = data.width;
            Vector3 pos;
            if (needUpdateFrame)
            {
                if (data.isOpen)
                {
                    allWidth = GetOpenFrameWidth(count, space);
                }
                else
                {
                    space = 10;
                    allWidth = GetCloseFrameWidth(count, space);
                }
            }

            int width = (int)(allWidth / count);
            pos = new Vector3(-allWidth / 2 + width / 2, 0, 0);
            for (int i = djlist.childCount - 1, imax = djlist.childCount - 1; i >= 0; --i)
            {
                if (imax - i < count)
                {
                    if (!data.isOpen && 3 == count && data.showList.Count > 3 && imax - i == count - 1)
                    {
                        Transform tmp = djlist.GetChild(i);
                        tmp.gameObject.SetActive(false);
                        if (null != djNum)
                        {
                            djNum.gameObject.SetActive(true);
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(djNum, "Label");
                            if (null != lb)
                            {
                                lb.text = "+" + (data.showList.Count - 2);
                            }
                            pos += new Vector3(width, 0, 0);
                            if (instant)
                            {
                                djNum.localPosition = pos;
                            }
                            else
                            {
                                SpringPosition.Begin(djNum.gameObject, pos, 15);
                            }
                        }
                    }
                    else
                    {
                        Transform tmp = djlist.GetChild(i);
                        tmp.gameObject.SetActive(true);
                        tmp.localScale = Vector3.one;
                        if (i < imax)
                        {
                            pos += new Vector3(width, 0, 0);
                        }
                        if (instant)
                        {
                            tmp.localPosition = pos;
                        }
                        else
                        {
                            SpringPosition.Begin(tmp.gameObject, pos, 15);
                        }
                    }
                }
                else
                {
                    djlist.GetChild(i).gameObject.SetActive(false);
                }
                
            }
            if (needUpdateFrame)
            {
                UpdateFrameWidthForDjCount(data, frame);
            }
        }
    }

    void UpdateFrameWidthForDjCount(FrameDataEx data, Transform frameTrans)
    {
        if (null == data.showList)
        {
            return;
        }
        if (null != mFrameCustomGrid)
        {
            Vector2 oldSize = new Vector2(data.width, data.height);
            int width = 0;
            if (data.isOpen)
            {
                width = GetOpenFrameWidth(data.showList.Count);
                if (isEditState)
                {
                    if (mFrameList[0] == data && null != data.showList && data.showList.Count > 1)
                    {
                        width += 100;
                    }
                    else if (mFrameList[mFrameList.Count - 1] == data && null != data.showList)
                    {
                        if (data.showList.Count > 1)
                        {
                            width += 140;
                        }
                        else
                        {
                            width += 40;
                        }
                    }
                }
            }
            else
            {
                width = GetCloseFrameWidth(data.showList.Count);
            }
            data.width = width;
            ItemObjectEx item = mFrameCustomGrid.Reposition(data, oldSize, false, true);
        }
        UpdateScrollViewEnabled();
        //UpdatePowerDownPos();
        UpdateScrollBar();
    }

    void DjMoveFinished()
    {
        Timer.Add(0.01f, 1, 1, DelayDjMoveFinished);
    }

    void DelayDjMoveFinished()
    {
        /*if (StepManager.GetIns().OpenOrCloseGuide)
        {
            EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().DragDuojiStep, true));
        }*/
    }

    void OnFinished()
    {
        try
        {
            RemoveFrame(mNewFrameData);
            mNewFrameData = null;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void OnButtonDrag(GameObject obj, Vector2 delta)
    {
        try
        {
            base.OnButtonDrag(obj, delta);
            if (isPlaying || mSwitchEditFlag)
            {
                return;
            }
            if (null != mSelectDragDj)
            {
                if (IsInEdit(mSelectDragDj.trans))
                {
                    if (null == mAddToFrameData)
                    {//无添加目标帧，如果在编辑区域内，则加在最后
                        if (null != mNewFrameData)
                        {
                            mNewFrameData.alphaFlag = true;
                            ItemObjectEx item = mFrameCustomGrid.GetItemObject(mNewFrameData);
                            if (null != item)
                            {
                                SetFrameAddState(item.itemObj.transform, mNewFrameData);
                            }
                            mAddToFrameData = mNewFrameData;
                        }
                    }
                }
                else if (null != mAddToFrameData && mAddToFrameData == mNewFrameData)
                {
                    mNewFrameData.alphaFlag = false;
                    ItemObjectEx item = mFrameCustomGrid.GetItemObject(mNewFrameData);
                    if (null != item)
                    {
                        SetFrameAddState(item.itemObj.transform, mNewFrameData);
                    }
                    mAddToFrameData = null;
                }
                
            }
            else if (mDragStartFlag && (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) || (null != mDjScrollView && !mDjScrollView.shouldMoveHorizontally)) && obj.name.StartsWith("dj_"))
            {
                int id = int.Parse(obj.name.Substring("dj_".Length));
                if (null != mRobot)
                {
                    DuoJiData dj = mRobot.GetAnDjData(id);
                    if (null != dj)
                    {
                        mSelectDragDj = CreateDragDj(id, dj.rota, obj.transform.position);
                        UICamera.currentTouch.dragged = mSelectDragDj.trans.gameObject;
                        mSelectDragDj.SetScrollDragActive(false);
                        mSelectDragDj.SetDragObjectActive(true);
                        mSelectDragDj.DragObject.OnPress(true);

                        SetFrameTriggerEnabled(true);
                        bool toEnd = mSelectFrameData == null ? true : false;
                        mNewFrameData = AddFrame(toEnd);
                        mAddToFrameData = null;
                        mSelectDragDj.AddUIPanel(10);

                        if (null != mSelectFrameData)
                        {
                            SetSelectFrame(null);
                        }
                        if (null != mSelectDuoJi)
                        {
                            SetSelectDuoJi(null);
                        }
                    }
                }

            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
        mDragStartFlag = false;
    }

    /// <summary>
    /// 进入触发器
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(GameObject own, GameObject other)
    {
        string name = own.name;
        if (name.StartsWith("frame_"))
        {//进入动作帧
            if (null != mSelectDragDj && other == mSelectDragDj.trans.gameObject)
            {
                CalculateDisMinFrame(own);
            }
        }
        //Debuger.Log("OnTriggerEnter own name = " + own.name + ",other name = " + other.name);
    }
    /// <summary>
    /// 退出触发器
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(GameObject own, GameObject other)
    {
        string name = own.name;
        if (null != mAddToFrameData && name.StartsWith("frame_"))
        {//退出动作帧
            if (null != mSelectDragDj && other == mSelectDragDj.trans.gameObject && null != mFrameCustomGrid)
            {
                FrameDataEx data = (FrameDataEx)mFrameCustomGrid.GetItemData(own.name);
                if (null != data && data == mAddToFrameData && mAddToFrameData != mNewFrameData)
                {
                    mAddToFrameData.alphaFlag = false;
                    SetFrameAddState(own.transform, mAddToFrameData);
                    mAddToFrameData = null;
                }
            }
        }
        //Debuger.Log("OnTriggerExit own name = " + own.name + ",other name = " + other.name);
    }
    /// <summary>
    /// 逗留在触发器
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(GameObject own, GameObject other)
    {
        string name = own.name;
        if (name.StartsWith("frame_"))
        {//停留在动作帧
            if (null != mSelectDragDj && other == mSelectDragDj.trans.gameObject)
            {
                CalculateDisMinFrame(own);
            }
        }
        //Debuger.Log("OnTriggerStay own name = " + own.name + ",other name = " + other.name);
    }
    /// <summary>
    /// 设置选中的舵机
    /// </summary>
    /// <param name="dj"></param>
    void SetSelectDuoJi(DuoJi dj)
    {
        if (dj != mSelectDuoJi)
        {
            Transform oldFrame = null;
            Transform nowFrame = null;
            if (null != mSelectDuoJi)
            {
                if (mFrameUseDjDict.ContainsKey(mSelectDuoJi.Name))
                {//上一次选中的舵机处于显示状态则关闭调节
                    mSelectDuoJi.SetAdjustActive(false);
                    oldFrame = mSelectDuoJi.trans.parent.parent;
                }
            }
            mSelectDuoJi = dj;
            if (null != mSelectDuoJi)
            {
                if (mFrameUseDjDict.ContainsKey(mSelectDuoJi.Name))
                {
                    mSelectDuoJi.SetAdjustActive(true);
                    nowFrame = mSelectDuoJi.trans.parent.parent;
                }
            }
            if (oldFrame != nowFrame)
            {
                if (null != oldFrame)
                {
                    Transform timeedit = oldFrame.Find("timeedit");
                    if (null != timeedit)
                    {
                        timeedit.gameObject.SetActive(true);
                    }
                }
                if (null != nowFrame)
                {
                    Transform timeedit = nowFrame.Find("timeedit");
                    if (null != timeedit)
                    {
                        timeedit.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 设置选中的帧
    /// </summary>
    /// <param name="frame"></param>
    void SetSelectFrame(FrameDataEx frame, bool ctrlModelFlag = false)
    {
        if (frame != mSelectFrameData)
        {
            if (null != mSelectFrameData)
            {
                /*if (null != mFrameCustomGrid)
                {
                    ItemObjectEx obj = mFrameCustomGrid.GetItemObject(mSelectFrameData);
                    if (null != obj && null != obj.childObj)
                    {
                        obj.childObj.SetActive(true);
                    }
                }*/
                
                if (null != mSelectFrameData.showList)
                {
                    SetFrameOpenState(mSelectFrameData, false);
                }
            }
            mSelectFrameData = frame;
            if (null != mSelectFrameData)
            {
                if (null != mSelectFrameData.showList)
                {
                    SetFrameOpenState(mSelectFrameData, true);
                }
                SetDuoJiAngle(mSelectFrameData.action);
                if (ctrlModelFlag)
                {
                    if (null != mRobot)
                    {
                        mRobot.CtrlAction(mSelectFrameData.action);
                    }
                }
                EventMgr.Inst.Fire(EventID.Ctrl_Robot_Action, new EventArg(mSelectFrameData.action));
            }
            
        }
    }
    

    /// <summary>
    /// 求出mAddToFrameData帧
    /// </summary>
    /// <param name="other"></param>
    void CalculateDisMinFrame(GameObject other)
    {
        try
        {
            if (null == mFrameCustomGrid || null == mSelectDragDj)
            {
                return;
            }
            byte id = byte.Parse(mSelectDragDj.Name.Substring("drag_dj_".Length));
            FrameDataEx data = (FrameDataEx)mFrameCustomGrid.GetItemData(other.name);
            if (null != data)
            {
                if (null != data.showList && data.showList.Contains(id))
                {
                    MoveFramePanel(other);
                    return;
                }
                if (null == mAddToFrameData)
                {//无添加目标帧
                    mAddToFrameData = (FrameDataEx)data;
                    mAddToFrameData.alphaFlag = true;
                    SetFrameAddState(other.transform, mAddToFrameData);
                    MoveFramePanel(other);
                }
                else if (data != mAddToFrameData)
                {//已有目标帧，且不是自己
                    Transform addFrame = mFrameGridTrans.Find(mAddToFrameData.name);
                    if (null == addFrame)
                    {
                        mAddToFrameData.alphaFlag = false;
                        data.alphaFlag = true;
                        SetFrameAddState(other.transform, data);
                        mAddToFrameData = data;
                        MoveFramePanel(other);
                    }
                    else
                    {
                        if (Mathf.Abs(Vector3.Distance(mSelectDragDj.trans.localPosition, addFrame.localPosition)) > Mathf.Abs(Vector3.Distance(mSelectDragDj.trans.localPosition, other.transform.localPosition)))
                        {//改变了mAddToFrameData
                            mAddToFrameData.alphaFlag = false;
                            SetFrameAddState(addFrame, mAddToFrameData);
                            data.alphaFlag = true;
                            SetFrameAddState(other.transform, data);
                            mAddToFrameData = data;
                            MoveFramePanel(other);
                        }
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

    bool IsInEdit(Transform obj)
    {
        Vector3 pos = obj.localPosition;
        if (pos.x >= Edit_Min.x && pos.x <= Edit_Max.x && pos.y >= Edit_Min.y && pos.y <= Edit_Max.y)
        {
            return true;
        }
        return false;
    }

    void MoveFramePanel(GameObject selectObj)
    {
        if (null != mFrameGridTrans && null != mFrameCustomGrid)
        {
            UIPanel panel = NGUITools.FindInParents<UIPanel>(mFrameGridTrans);
            if (null != panel)
            {
                Bounds bs = NGUIMath.CalculateRelativeWidgetBounds(panel.transform, selectObj.transform);
                int ret = panel.GetInPanelArea(bs, true, false);
                if (0 == ret)
                {
                    return;
                }
                Vector3 offset = new Vector3(-300 * ret, 0, 0);
                mFrameCustomGrid.MoveWithinBounds(offset, false);
            }
        }
    }
    /// <summary>
    /// 移动某个ui
    /// </summary>
    /// <param name="tweens"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    public static void PlayTweenPosition(TweenPosition tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localPosition;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tweens"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    public static void PlayTweenScale(TweenScale tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localScale;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tweens"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    public static void PlayTweenRota(TweenRotation tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localEulerAngles;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 是否显示编辑区域
    /// </summary>
    /// <param name="activeFlag"></param>
    void ShowEditAction(bool activeFlag, bool instant = false)
    {
        try
        {
            isEditState = activeFlag;
            if (instant)
            {
            }
            else
            {
                mSwitchEditFlag = true;
                SetEventActive(false);
            }
            Vector3 fixTo = Vector3.zero;
            Vector3 playTo = Vector3.zero;
            Vector3 powerdownbtnTo = Vector3.zero;
            Vector3 djTo = Vector3.zero;
            Vector3 rota = Vector3.zero;
            
            if (isEditState)
            {
                if (null != mFixTweenPosition)
                {
                    fixTo = mFixEditPos;// UIManager.GetWinPos(mFixTweenPosition.transform, UIWidget.Pivot.Bottom/*, 0, mDjTransSize.y*/);
                }
                if (null != mDjGameObject)
                {
                    mDjGameObject.SetActive(true);
                    djTo = mDjGameObjectEditPos;
                }
                playTo = new Vector3(0, 106, 0);
                powerdownbtnTo = new Vector3(0, -36);
                if (instant)
                {
                    OpenEdit();
                }
                else
                {
                    Timer.Add(0.3f, 1, 1, OpenEdit);
                }
                if (mFrameList.Count < 1 && null != mStartObj && !mStartObj.activeSelf)
                {
                    mStartObj.SetActive(true);
                }
            }
            else
            {
                rota.z = 180;
                Vector3 offset = Vector2.zero;
                if (null != mFixTweenPosition)
                {
                    fixTo = new Vector2(0, mFixEditPos.y + Fix_Offset_Y);//UIManager.GetWinPos(mFixTweenPosition.transform, UIWidget.Pivot.Bottom, 0, Fix_Offset_Y);
                    offset = fixTo - mFixTweenPosition.transform.localPosition;
                    offset.z = 0;
                }
                if (null != mDjGameObject)
                {
                    djTo = mDjGameObject.transform.localPosition + offset;
                }
                if (instant)
                {
                    CloseEdit();
                }
                else
                {
                    Timer.Add(0.3f, 1, 1, CloseEdit);
                }
                
                playTo = new Vector3(0, 116, 0);
                powerdownbtnTo = new Vector3(0, -120);
                if (null != mBgTriggerEvent)
                {
                    mBgTriggerEvent.enabled = false;
                }
                if (null != mStartObj && mStartObj.activeSelf)
                {
                    mStartObj.SetActive(false);
                }
            }
            if (instant)
            {
                if (null != mFixTweenPosition)
                {
                    mFixTweenPosition.transform.localPosition = fixTo;
                }
                if (null != mPlaybtnTweenPosition)
                {
                    mPlaybtnTweenPosition.transform.localPosition = playTo;
                }
                if (null != mDjTweenPosition)
                {
                    mDjTweenPosition.transform.localPosition = djTo;
                }
                if (null != mStretchRotation)
                {
                    mStretchRotation.transform.localEulerAngles = rota;
                }
                if (null != mPowerDownBtnTrans)
                {
                    mPowerDownBtnTrans.localPosition = powerdownbtnTo;
                }
            }
            else
            {
                PlayTweenPosition(mFixTweenPosition, fixTo);
                PlayTweenPosition(mPlaybtnTweenPosition, playTo);
                GameHelper.PlayTweenPosition(mPowerDownBtnTrans, powerdownbtnTo);
                PlayTweenPosition(mDjTweenPosition, djTo);
                PlayTweenRota(mStretchRotation, rota);
            }
            SetFrameListEdit(isEditState, instant);
        }
        catch (System.Exception ex)
        {
            mSwitchEditFlag = false;
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 设置动作帧为编辑状态和非编辑
    /// </summary>
    /// <param name="edit">true表示进入编辑状态</param>
    /// <param name="instant">true表示立即改变位置和大小</param>
    void SetFrameListEdit(bool edit, bool instant)
    {
        if (null == mFrameListTrans || null == mFrameGridTrans)
        {
            return;
        }
        Vector3 frameBar = Vector3.zero;
        Vector4 rect = mFrameListStartClip;
        Vector3 to = mFrameListTrans.localPosition;
        //bool readFlag = false;
        if (-1 != mDelayScaleIndex)
        {
            Timer.Cancel(mDelayScaleIndex);
            mDelayScaleIndex = -1;
        }
        int fixBgWidth = 0;
        SetSelectDuoJi(null);
        SetSelectFrame(null);
        if (edit)
        {
            to.y = 60;
            mFrameCustomGrid.showNum = (int)(mFrameListStartClip.z / Edit_Frame_Width) + 1;
            fixBgWidth = (int)(PublicFunction.GetWidth() - mFixLeftSize.x - mFixRightSize.x);
            if (null != mFixBgBoxCollider)
            {//编辑时扩大区域
                mFixBgBoxCollider.size = new Vector2(PublicFunction.GetWidth() + 500, PublicFunction.GetHeight() + 500);
            }
            if (null != mFrameScrollBarTrans)
            {
                frameBar = mFrameScrollBarTrans.localPosition;
                frameBar.y = -84;
            }
        }
        else
        {
            to.y = 75;
            mFrameCustomGrid.showNum = (int)(mFrameListStartClip.z / Small_Frame_Width) + 1;
            rect.w = rect.w / 2 + 40;            
            fixBgWidth = (int)(PublicFunction.GetWidth() - mFixLeftSize.x);
            if (null != mFrameScrollBarTrans)
            {
                frameBar = mFrameScrollBarTrans.localPosition;
                frameBar.y = 10;
            }
        }
        if (instant)
        {
            SetFrameListClip(rect);
            SetAllFrameEditState(edit);
            UpdateScrollBar();
            if (null != mFrameCustomGrid)
            {
                mFrameCustomGrid.UpdateShow();
            }
            mFrameScrollBarTrans.localPosition = frameBar;
        }
        else
        {
            PlayTweenPosition(mFrameScrollBarTweenPos, frameBar);
            mDelayScaleIndex = Timer.Add(0.35f, 0, 1, delegate()//ClientMain.GetInst().WaitTimeInvoke(0.3f, delegate()
            {
                SetFrameListClip(rect);/*panel.baseClipRegion = rect;*/
                SetAllFrameEditState(edit);
                /*if (readFlag)
                {
                    UpdatePowerDownPos();
                }*/
                UpdateScrollBar();
                if (null != mFrameCustomGrid)
                {
                    mFrameCustomGrid.UpdateShow();
                }
                if (null != mFixRightObject)
                {
                    mFixRightObject.SetActive(!edit);
                    mFixRightObject.SetActive(edit);
                }
            });
        }
        if (null != mFixRightObject)
        {
            mFixRightObject.SetActive(edit);
        }
        if (null != mFixBgSprite)
        {
            if (null != mFixBgBoxCollider)
            {//编辑时扩大区域
                if (edit)
                {
                    mFixBgBoxCollider.size = new Vector3(PublicFunction.GetWidth() + 500, PublicFunction.GetHeight() + 500);
                }
                else
                {
                    int tmpHeight = mFixBgSprite.height;
                    mFixBgBoxCollider.size = new Vector3(fixBgWidth, tmpHeight);
                }
                
            }
        }
        mFrameCustomGrid.StartPos = new Vector3(mFrameCustomGrid.StartPos.x, to.y, mFrameCustomGrid.StartPos.z);
        /*if (null != mPowerDownBtnTrans)
        {
            mPowerDownBtnTrans.gameObject.SetActive(readFlag);
        }*/
        if (instant)
        {
            mFrameListTrans.localPosition = to;
        }
        else
        {
            PlayTweenPosition(mFrameListPosition, to);
        }
    }

    void SetFrameSoftness()
    {
        if (mFrameCustomGrid.GetAllWidth() < mFrameListPanel.baseClipRegion.z)
        {
            mFrameListPanel.clipSoftness = Vector2.zero;
        }
        else
        {
            mFrameListPanel.clipSoftness = new Vector2(35, 0);
        }
    }
    /// <summary>
    /// 设置panel的裁剪区域
    /// </summary>
    /// <param name="rect"></param>
    void SetFrameListClip(Vector4 rect)
    {
        if (rect.z != mFrameListStartClip.z)
        {//宽度改变了，须移动mFrameGridTrans
            rect.x = (rect.z - mFrameListStartClip.z) / 2;
        }
        if (null != mFrameListPanel)
        {
            mFrameListPanel.baseClipRegion = rect;
            mFrameListPanel.clipOffset = new Vector2(mFrameCustomGrid.StartPos.x - mFrameListPanel.transform.localPosition.x, 0);
        }
        UpdateScrollViewEnabled();
    }

    void UpdateScrollBar()
    {
        if (null != mFrameScrollViewEx)
        {
            mFrameScrollViewEx.UpdateScrollbars();
            mFrameScrollViewEx.UpdateScrollbarActive();
        }
    }

    void UpdateScrollViewEnabled()
    {
        if (null != mFrameCustomGrid && null != mFrameScrollViewEx && null != mFrameListPanel)
        {
            if (mFrameCustomGrid.GetAllWidth() < mFrameListPanel.baseClipRegion.z)
            {
                mFrameScrollViewEx.enabled = false;
            }
            else
            {
                mFrameScrollViewEx.enabled = true;
            }
            SetFrameSoftness();
            //mFrameCustomGrid.MoveWithinBounds(Vector2.zero, true);
        }
    }

    void SetTimeBarActive(bool active)
    {
        mTimeObj.SetActive(true);
        Vector3 to;
        if (active)
        {
            if (null != mSelectFrameData)
            {
                SetTimeBarTime(mSelectFrameData.action.sportTime);
            }
            to = mTimeShowPos;
            mTimeBoxObj.SetActive(true);
            /*BoxCollider box = GameHelper.FindChildComponent<BoxCollider>(mTimeBar.transform, "Background");
            if (null != box)
            {
                Vector3 size = box.size;
                UISprite bg = box.GetComponent<UISprite>();
                if (null != bg)
                {
                    size.y = bg.height + 100;
                }
                else
                {
                    size.y = 150;
                }
                box.size = size;
            }*/
        }
        else
        {
            to = mTimeHidePos;
            //ClientMain.GetInst().WaitTimeInvoke(0.35f, HideTimeBar);
            Timer.Add(0.3f, 1, 1, HideTimeBar);
        }
        PlayTweenPosition(mTimeBarPosition, to);
    }

    void HideTimeBar()
    {
        if (null != mTimeObj)
        {
            mTimeObj.SetActive(false);
        }
        mTimeBoxObj.SetActive(false);
    }

    void OnFrameMoveStop()
    {
        if (null != mFrameListPanel && null != mFrameCustomGrid)
        {
            mFrameListPanel.clipOffset = new Vector2(mFrameCustomGrid.StartPos.x - mFrameListPanel.transform.localPosition.x, 0);
            /*if (mSwitchEditFlag && null != mFrameCustomGrid)
            {
                mFrameCustomGrid.UpdateShow();
            }*/
        }
    }
    void OpenEdit()
    {
        mSwitchEditFlag = false;
        SetEventActive(true);
        if (null != mFrameListPanel && null != mFrameCustomGrid)
        {
            mFrameListPanel.clipOffset = new Vector2(mFrameCustomGrid.StartPos.x - mFrameListPanel.transform.localPosition.x, 0);
        }
    }
    void CloseEdit()
    {
        if (null != mDjGameObject)
        {
            mDjGameObject.SetActive(false);
        }
        mSwitchEditFlag = false;
        SetEventActive(true);
        if (null != mFrameListPanel && null != mFrameCustomGrid)
        {
            mFrameListPanel.clipOffset = new Vector2(mFrameCustomGrid.StartPos.x - mFrameListPanel.transform.localPosition.x, 0);
        }
    }
    /// <summary>
    /// 设置点击事件状态
    /// </summary>
    /// <param name="activeFlag"></param>
    void SetEventActive(bool activeFlag)
    {
        if (null != mDjScrollView)
        {
            mDjScrollView.closeEvent = !activeFlag;
        }
        if (null != mFrameScrollViewEx)
        {
            mFrameScrollViewEx.closeEvent = !activeFlag;
        }
        if (null != mFrameScrollBar)
        {
            mFrameScrollBar.closeEvent = !activeFlag;
        }
    }

    void SetFrameTriggerEnabled(bool enabled)
    {
        if (null != mFrameTriggerList)
        {
            for (int i = 0, imax = mFrameTriggerList.Count; i < imax; ++i)
            {
                mFrameTriggerList[i].enabled = enabled;
            }
        }
    }


    /// <summary>
    /// 初始化舵机列表
    /// </summary>
    void InitDjList()
    {
        try
        {
            if (null == mDuojiPrefab || null == mDjParentTrans)
            {
                return;
            }
            List<byte> list = null;
            if (null != mRobot)
            {
                list = mRobot.GetAllDjData().GetAngleList();
            }
            if (null != list)
            {
                mDuojiDict = new Dictionary<string, DuoJi>();
                UIPanel panel = mDjParentTrans.GetComponent<UIPanel>();
                int cellWidth = 140;
                int allWidth = list.Count * cellWidth;
                if (panel.finalClipRegion.z > allWidth)
                {
                    panel.clipSoftness = Vector2.zero;
                }
                
                Vector3 startPos = new Vector3(panel.clipSoftness.x + cellWidth / 2, 0);
                for (int i = 0, imax = list.Count; i < imax; ++i)
                {
                    GameObject obj = UnityEngine.Object.Instantiate(mDuojiPrefab) as GameObject;
                    if (null != obj)
                    {
                        obj.name = "dj_" + list[i];
                        obj.SetActive(true);
                        obj.transform.parent = mDjParentTrans;
                        obj.transform.localPosition = startPos + new Vector3(cellWidth * i, 0);
                        obj.transform.localScale = Vector3.one;
                        obj.transform.localEulerAngles = Vector3.zero;
                        DuoJi duoji = new DuoJi(obj.transform, list[i]);
                        mDuojiDict.Add(obj.name, duoji);
                        DuoJiData dj = mRobot.GetAnDjData(list[i]);
                        if (null != dj)
                        {
                            duoji.SetAngle(dj.rota);
                        }
                    }
                }
                
                /*if (null != mDjlistGrid)
                {
                    mDjlistGrid.repositionNow = true;
                }*/
                UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    DuoJi CreateDragDj(int id, int rota, Vector3 pos)
    {
        DuoJi data = null;
        if (null != mCacheDjList && mCacheDjList.Count > 0)
        {
            data = mCacheDjList[0];
            mCacheDjList.RemoveAt(0);
        }
        else if (null != mDuojiPrefab)
        {
            GameObject obj = UnityEngine.Object.Instantiate(mDuojiPrefab) as GameObject;
            UIManager.SetButtonEventDelegate(obj.transform, mBtnDelegate);
            data = new DuoJi(obj.transform, id);
            data.onDragDj = DragSetDuoJiAngle;
        }
        if (null != data)
        {
            data.trans.name = "drag_dj_" + id;
            data.trans.parent = mTrans;
            data.trans.position = pos;
            data.trans.localScale = Vector3.one;
            data.SetActive(true);
            data.SetAngle(rota);
            data.SetID(id);
            data.SetAdjustActive(false);
        }
        return data;
    }
    DuoJi CreateFrameDj(int index, int id, int rota)
    {
        DuoJi data = null;
        if (null != mCacheDjList && mCacheDjList.Count > 0)
        {
            data = mCacheDjList[0];
            data.trans.localScale = Vector3.one;
            mCacheDjList.RemoveAt(0);
        }
        else if (null != mDuojiPrefab)
        {
            GameObject obj = UnityEngine.Object.Instantiate(mDuojiPrefab) as GameObject;
            UIManager.SetButtonEventDelegate(obj.transform, mBtnDelegate);
            data = new DuoJi(obj.transform, id);
            data.onDragDj = DragSetDuoJiAngle;
        }
        if (null != data)
        {
            InitFrameDj(data, index, id, rota);
            /*data.SetScrollDragActive(false);
            data.SetScrollDragExActive(true);
            data.SetDragObjectActive(false);
            data.SetFrameData(index, id, rota);
            data.SetAdjustActive(false);
            if (null != mFrameUseDjDict && mFrameUseDjDict.ContainsKey(data.trans.name))
            {
                RemoveFrameDj(data.trans.name);
                Debuger.LogError("逻辑创建逻辑混乱" + data.trans.name);
            }
            if (null == mFrameUseDjDict)
            {
                mFrameUseDjDict = new Dictionary<string, DuoJi>();
            }
            mFrameUseDjDict[data.trans.name] = data;*/
        }
        return data;
    }

    void InitFrameDj(DuoJi data, int index, int id, int rota)
    {
        data.SetScrollDragActive(false);
        data.SetScrollDragExActive(true);
        data.SetDragObjectActive(false);
        data.SetFrameData(index, id, rota);
        data.SetAdjustActive(false);
        if (null != mFrameUseDjDict && mFrameUseDjDict.ContainsKey(data.trans.name))
        {
            RemoveFrameDj(data.trans.name);
        }
        if (null == mFrameUseDjDict)
        {
            mFrameUseDjDict = new Dictionary<string, DuoJi>();
        }
        mFrameUseDjDict[data.trans.name] = data;
    }

    void SetDottedBox(Transform trans, int width, int height)
    {
        if (null != trans)
        {
            UISprite bg = GameHelper.FindChildComponent<UISprite>(trans, "bg");
            if (null != bg)
            {
                bg.width = width;
                bg.height = height;
            }
            SetDotted(trans.Find("top"), UIWidget.Pivot.Top, width, height);
            SetDotted(trans.Find("bottom"), UIWidget.Pivot.Bottom, width, height);
            SetDotted(trans.Find("left"), UIWidget.Pivot.Left, width, height);
            SetDotted(trans.Find("right"), UIWidget.Pivot.Right, width, height);
        }
    }

    void SetDotted(Transform trans, UIWidget.Pivot pivot, int width, int height)
    {
        if (null != trans)
        {
            int spWidth = 0;
            Vector2 pos = Vector2.zero;
            switch (pivot)
            {
                case UIWidget.Pivot.Top:
                    spWidth = width - 72;
                    pos = new Vector2(0, height / 2.0f - 0.5f);
                    break;
                case UIWidget.Pivot.Bottom:
                    spWidth = width - 72;
                    pos = new Vector2(0, -height / 2.0f + 0.5f);
                    break;
                case UIWidget.Pivot.Left:
                    spWidth = height - 68;
                    pos = new Vector2(-width / 2.0f + 0.5f, 0);
                    break;
                case UIWidget.Pivot.Right:
                    spWidth = height - 68;
                    pos = new Vector2(width / 2.0f - 0.5f, 0);
                    break;
            }
            trans.localPosition = pos;
            UISprite sp = trans.GetComponent<UISprite>();
            if (null != sp)
            {
                sp.width = spWidth;
            }
        }
    }

    void RemoveFrameDj(string name)
    {
        if (mFrameUseDjDict.ContainsKey(name))
        {
            DuoJi data = mFrameUseDjDict[name];
            mFrameUseDjDict.Remove(name);
            if (null != data.trans)
            {
                data.trans.parent = mTrans;
                data.SetAdjustActive(false);
                data.SetActive(false);
            }
            mCacheDjList.Add(data);
            
        }
    }
    int GetOpenFrameWidth(int count, int space = 20)
    {
        int width = 0;
        if (isEditState)
        {
            width = count * Edit_DuoJi_Width + (count - 1) * space;
            if (width < Edit_Frame_Width)
            {
                width = Edit_Frame_Width;
            }
        }
        else
        {
            width = count * Small_DuoJi_Width + (count - 1) * space;
            if (width < Small_Frame_Width)
            {
                width = Small_Frame_Width;
            }
        }
        return width;
    }

    int GetCloseFrameWidth(int count, int space = 10)
    {
        count = count > 3 ? 3 : count;
        int width = count * Edit_DuoJi_Not_Open_Width + (count - 1) * space;
        if (width < Edit_Frame_Width)
        {
            width = Edit_Frame_Width;
        }
        return width;
    }
    void SetFrameOpenState(FrameDataEx data, bool isOpen)
    {
        Vector2 oldSize = new Vector2(data.width, data.height);
        if (isOpen)
        {
            data.width = GetOpenFrameWidth(data.showList.Count);
            if (mFrameList[0] == data && null != data.showList && data.showList.Count > 1)
            {//第一帧
                data.width += 100;
            }
            else if (mFrameList[mFrameList.Count - 1] == data && null != data.showList)
            {
                if (data.showList.Count > 1)
                {
                    data.width += 140;
                }
                else
                {
                    data.width += 40;
                }
            }
        }
        else
        {
            data.width = GetCloseFrameWidth(data.showList.Count);
        }
        data.isOpen = isOpen;
        if (null != mFrameCustomGrid)
        {
            ItemObjectEx item = mFrameCustomGrid.Reposition(data, oldSize, true, true);
            if (null != data.showList && data.showList.Count > 3 && null != item && null != item.childObj1)
            {
                if (isOpen)
                {
                    item.childObj1.SetActive(false);
                }
                else
                {
                    item.childObj1.SetActive(true);
                }
            }
            if (null != item && null != item.childObj)
            {
                if (null == mSelectDuoJi)
                {
                    item.childObj.SetActive(true);
                }
            }

        }
        UpdateScrollViewEnabled();
        //UpdatePowerDownPos();
        UpdateScrollBar();
    }

    void SetAllFrameEditState(bool isEdit)
    {
        if (isEdit)
        {
            FrameDataEx data;
            for (int i = 0, imax = mFrameList.Count; i < imax; ++i)
            {
                data = (FrameDataEx)mFrameList[i];
                data.width = GetCloseFrameWidth(data.showList == null ? 0 : data.showList.Count);
                //data.width = Edit_Frame_Width;
                data.height = Edit_Frame_Height;
                data.isOpen = false;
            }
        }
        else
        {
            FrameDataEx data;
            for (int i = 0, imax = mFrameList.Count; i < imax; ++i)
            {
                data = (FrameDataEx)mFrameList[i];
                int count = 1;
                if (null != data.showList)
                {
                    count = data.showList.Count;
                }
                data.width = GetOpenFrameWidth(count);
                data.height = Small_Frame_Height;
                data.isOpen = true;
            }
        }
        if (null != mFrameCustomGrid)
        {
            if (null != mFrameCustomGrid.m_cellList)
            {
                for (int i = 0, imax = mFrameCustomGrid.m_cellList.Length; i < imax; ++i)
                {
                    if (null != mFrameCustomGrid.m_cellList[i].childObj)
                    {
                        if (isEdit)
                        {
                            mFrameCustomGrid.m_cellList[i].childObj.SetActive(true);
                        }
                        else
                        {
                            mFrameCustomGrid.m_cellList[i].childObj.SetActive(false);
                        }
                    }
                }
            }
            mFrameCustomGrid.Reposition();
        }
        //UpdatePowerDownPos();
        UpdateScrollViewEnabled();
    }

    /// <summary>
    /// 设置编辑状态的所有帧为打开状态
    /// </summary>
    /// <param name="isOpen"></param>
    void SetEditAllFrameOpenState(bool isOpen)
    {
        if (null != mFrameScrollViewEx)
        {
            mFrameScrollViewEx.MoveToFinished();
        }
        if (isOpen)
        {
            FrameDataEx data;
            for (int i = 0, imax = mFrameList.Count; i < imax; ++i)
            {
                data = (FrameDataEx)mFrameList[i];
                int count = 1;
                if (null != data.showList)
                {
                    count = data.showList.Count;
                }
                data.width = GetOpenFrameWidth(count);
                data.isOpen = isOpen;
            }
        }
        else
        {
            FrameDataEx data;
            for (int i = 0, imax = mFrameList.Count; i < imax; ++i)
            {
                data = (FrameDataEx)mFrameList[i];
                data.width = GetCloseFrameWidth(data.showList == null ? 0 : data.showList.Count);
                data.isOpen = isOpen;
            }
        }
        if (null != mFrameCustomGrid)
        {
            mFrameCustomGrid.Reposition();
        }
        //UpdatePowerDownPos();
    }
    FrameDataEx CreateFrameData()
    {
        FrameDataEx data = new FrameDataEx(mFrameIndex, "frame_" + mFrameIndex, Edit_Frame_Width, Edit_Frame_Height, mLastFrameTime);
        ++mFrameIndex;
        return data;
    }
    FrameDataEx AddFrame(bool toEnd)
    {
        if (null != mFrameCustomGrid)
        {
            FrameDataEx data = CreateFrameData();
            if (null != mRobot)
            {
                mRobot.GetNowAction(data.action);
            }
            mFrameCustomGrid.AddItem(data, toEnd);
            //UpdatePowerDownPos();
            UpdateScrollViewEnabled();
            SetActionSaveFlag(true);

            if (null != mStartObj && mStartObj.activeSelf)
            {
                mStartObj.SetActive(false);
            }
            return data;
        }
        return null;
    }

    void RemoveFrame(FrameDataEx data)
    {
        if (null != mFrameCustomGrid)
        {
            if (null != mSimulationActs)
            {//处于播放暂停状态，如何删除的帧在播放帧之前
                int index = mFrameList.IndexOf(data);
                if (-1 != index && index <= mNowPlayDataIndex)
                {
                    --mNowPlayDataIndex;
                }
            }
            mFrameCustomGrid.RemoveItem(data);
            //UpdatePowerDownPos();
            UpdateScrollBar();
            UpdateScrollViewEnabled();
            if (null != mNewFrameData && data == mNewFrameData)
            {
                --mFrameIndex;
            }
            if (mFrameList.Count > 0)
            {
                EventMgr.Inst.Fire(EventID.UI_Set_Save_Actions_Btn_State, new EventArg(true));
                SetActionSaveFlag(true);
            }
            else
            {
                EventMgr.Inst.Fire(EventID.UI_Set_Save_Actions_Btn_State, new EventArg(false));
                SetActionSaveFlag(false);
                if (null != mStartObj && !mStartObj.activeSelf)
                {
                    mStartObj.SetActive(true);
                }
            }
            
        }
    }
    /// <summary>
    /// 粘贴动作帧
    /// </summary>
    bool PasteFrame()
    {
        int index = mFrameList.IndexOf(mSelectFrameData);
        if (-1 != index)
        {
            FrameDataEx data = CreateFrameData();
            data.action.Copy(mCopyFrameData.action, mRobot);
            if (mFrameCustomGrid.IsEnd(mSelectFrameData))
            {//加在最后
                mFrameCustomGrid.AddItem(data);
            }
            else
            {//中间插入
                mFrameCustomGrid.InsertItem(index + 1, data);
                if (null != mSimulationActs && index + 1 < mNowPlayDataIndex)
                {//动作处于暂停状态，在暂停位置之前插入
                    ++mNowPlayDataIndex;
                }
            }
            SetSelectFrame(data);
            //UpdatePowerDownPos();
            UpdateScrollViewEnabled();
            SetActionSaveFlag(true);
            if (null != data.showList && data.showList.Count == 1)
            {//单个舵机的帧，选中相当于选中舵机
                try
                {
                    ItemObjectEx obj = mFrameCustomGrid.GetItemObject(data);
                    if (null != obj)
                    {
                        Transform dj = obj.itemObj.transform.Find("djlist").GetChild(0);
                        if (null != dj && mFrameUseDjDict.ContainsKey(dj.name))
                        {
                            SetSelectDuoJi(mFrameUseDjDict[dj.name]);
                            return true;
                        }
                    }
                    
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
            if (null != mStartObj && mStartObj.activeSelf)
            {
                mStartObj.SetActive(false);
            }
        }
        return false;
    }

    /*void UpdatePowerDownPos(int offset = 0)
    {
        if (null == mPowerDownBtnTrans/ * || !PlatformMgr.Instance.GetBluetoothState()* /)
        {
            return;
        }
        if (0 == offset)
        {
            Vector3 pos = mPowerDownBtnStartPos;
            if (null != mFrameCustomGrid)
            {
                float showWidth = mFrameCustomGrid.GetShowWidth();
                float allWidth = mFrameCustomGrid.GetAllWidth();
                if (allWidth > showWidth)
                {//显示满了，在最后
                    pos.x += mFrameCustomGrid.GetShowWidth();
                }
                else
                {
                    pos.x += mFrameCustomGrid.GetShowWidth() + 30;
                }

            }
            mPowerDownBtnTrans.localPosition = pos;
        }
        else
        {
            mPowerDownBtnTrans.localPosition += new Vector3(offset, 0);
        }

    }*/


    void UpdateFrameDj(GameObject obj, FrameDataEx data, bool needUpdateFlag)
    {
        try
        {
            if (null != obj)
            {
                Transform djlist = obj.transform.Find("djlist");
                if (null != djlist)
                {
                    List<Transform> needChangeList = null;
                    List<int> sameList = null;
                    for (int i = 0, imax = djlist.childCount; i < imax; ++i)
                    {
                        Transform tmp = djlist.GetChild(i);
                        string[] ary = tmp.name.Split('_');
                        do 
                        {
                            if (null != ary && ary.Length == 3)
                            {
                                if (null != data.showList && int.Parse(ary[1]) == data.action.index && data.showList.Contains(byte.Parse(ary[2])))
                                {
                                    if (null == sameList)
                                    {
                                        sameList = new List<int>();
                                    }
                                    sameList.Add(int.Parse(ary[2]));
                                    break;
                                }
                            }
                            if (null == needChangeList)
                            {
                                needChangeList = new List<Transform>();
                            }
                            needChangeList.Add(tmp);
                        } while (false);
                    }
                    if (null == data.showList && null != needChangeList)
                    {//删除舵机
                        for (int i = 0, imax = needChangeList.Count; i < imax; ++i)
                        {
                            RemoveFrameDj(needChangeList[i].name);
                        }
                    }
                    else if (null != data.showList)
                    {
                        int useNeedCount = 0;
                        foreach (int djId in data.showList)
                        {
                            do 
                            {
                                if (null != needChangeList && useNeedCount < needChangeList.Count)
                                {//改变舵机
                                    string name = needChangeList[useNeedCount].name;
                                    DuoJi tmpdj = null;
                                    if (mFrameUseDjDict.ContainsKey(name))
                                    {
                                        tmpdj = mFrameUseDjDict[name];
                                        mFrameUseDjDict.Remove(name);
                                    }
                                    else
                                    {
                                        tmpdj = new DuoJi(needChangeList[useNeedCount], djId);
                                        tmpdj.onDragDj = DragSetDuoJiAngle;
                                    }
                                    InitFrameDj(tmpdj, data.action.index, djId, data.GetRota(djId));
                                    /*tmpdj.SetFrameData(data.action.index, djId, data.GetRota(djId));
                                    tmpdj.SetAdjustActive(false);
                                    mFrameUseDjDict[tmpdj.trans.name] = tmpdj;*/
                                    ++useNeedCount;
                                    break;
                                }
                                //生成舵机
                                string djName = DuoJi.GetFrameDjName(data.action.index, djId);
                                if (null != mFrameUseDjDict && mFrameUseDjDict.ContainsKey(djName))
                                {//要生成的舵机在其他帧下面，移过来
                                    mFrameUseDjDict[djName].trans.parent = djlist;
                                    break;
                                }
                                DuoJi newDj = CreateFrameDj(data.action.index, djId, data.GetRota(djId));
                                if (null != newDj)
                                {
                                    newDj.trans.parent = djlist;
                                    newDj.SetActive(false);
                                    newDj.SetActive(true);
                                    //newDj.trans.localPosition = Vector3.zero;
                                    newDj.trans.localScale = Vector3.one;
                                    newDj.trans.localEulerAngles = Vector3.zero;
                                }
                            } while (false);
                        }
                        
                        if (null != needChangeList && useNeedCount < needChangeList.Count)
                        {//需要删除舵机
                            for (int i = useNeedCount, imax = needChangeList.Count; i < imax; ++i)
                            {
                                RemoveFrameDj(needChangeList[i].name);
                            }
                        }
                        if (data.showList.Count > 0)
                        {
                            FrameDuoJiGrid(data, obj.transform, true, needUpdateFlag);
                        }
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
    void SetFrameAddState(Transform frame, FrameDataEx data)
    {
        if (null != frame && null != data)
        {
            TweenAlpha tweens = frame.GetComponent<TweenAlpha>();
            if (null != tweens)
            {
                tweens.enabled = data.alphaFlag;
                if (!tweens.enabled)
                {
                    tweens.value = 1;
                }
            }
            if (!data.alphaFlag)
            {
                UISprite bg = GameHelper.FindChildComponent<UISprite>(frame, "bg");
                if (null != bg)
                {
                    if (null != data.showList && data.showList.Count == 1)
                    {
                        bg.alpha = 0;
                    }
                    else
                    {
                        bg.alpha = 1;
                    }
                }
            }
            
        }
    }

    void SetFrameTime(Transform frame, int time)
    {
        UILabel label = GameHelper.FindChildComponent<UILabel>(frame, "timeedit/Label");
        if (null != label)
        {
            label.text = "[596f80]" + time + " [93a7b6]ms[-][-]";
        }
    }

    void SetFrameIndex(Transform frame, int index)
    {
        UILabel label = GameHelper.FindChildComponent<UILabel>(frame, "timeedit/indexlabel");
        if (null != label)
        {
            label.text = index.ToString();
        }
    }

    void TimeBarOnChange()
    {
        int time = Time_Min;
        if (null != mTimeBar)
        {
            time += PublicFunction.Rounding(mTimeBar.value * (Time_Max - Time_Min));
        }
        int remainder = time % 20;
        if (remainder != 0)
        {
            if (remainder < 10)
            {
                time -= remainder;
            }
            else
            {
                time += 20 - remainder;
            }
        }
        mNowTime = time;
        SetNowTimeLabel(time);
        if (null != mSelectFrameData && null != mSelectFrameData.action)
        {
            mSelectFrameData.action.sportTime = time;
            if (null != mFrameCustomGrid)
            {
                ItemObjectEx item = mFrameCustomGrid.GetItemObject(mSelectFrameData);
                if (null != item)
                {
                    SetFrameTime(item.itemObj.transform, mSelectFrameData.action.sportTime);
                }
            }
        }
        
        EventMgr.Inst.Fire(EventID.UI_Set_Save_Actions_Btn_State, new EventArg(true));
        SetActionSaveFlag(true);
    }

    void OnTimeBarDragFinished()
    {
        BarValueToTime();
    }

    void SetTimeBarTime(int time)
    {
        if (time < Time_Min)
        {
            time = Time_Min;
        }
        else if (time > Time_Max)
        {
            time = Time_Max;
        }
        if (null != mTimeBar)
        {
            mTimeBar.value = (time - Time_Min + 0.0f) / (Time_Max - Time_Min);
        }
        mNowTime = time;
    }

    void SetNowTimeLabel(int time)
    {
        if (null != mTimeLabel)
        {
            mTimeLabel.text = "[596f80]" + time + "[93a7b6]ms[-][-]";
            if (null != mTimeMin && null != mTimeMax && null != mTimeBar)
            {
                Vector3 pos = mTimeLabel.transform.localPosition;
                pos.x = mTimeMin.localPosition.x + (mTimeMax.localPosition.x - mTimeMin.localPosition.x) * mTimeBar.value;
                mTimeLabel.transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
            }
        }
    }
    int BarValueToTime()
    {
        int time = Time_Min;
        if (null != mTimeBar)
        {
            time += PublicFunction.Rounding(mTimeBar.value * (Time_Max - Time_Min));
            int remainder = time % 20;
            if (remainder != 0)
            {
                if (remainder < 10)
                {
                    time -= remainder;
                }
                else
                {
                    time += 20 - remainder;
                }
                mTimeBar.value = (time - Time_Min + 0.0f) / (Time_Max - Time_Min);
            }
        }
        return time;
    }
    /// <summary>
    /// 创建一个帧
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    ItemObjectEx CreateItemEx(GameObject obj)
    {
        ItemObjectEx objEx = new ItemObjectEx();
        objEx.itemObj = obj;
        objEx.childObj = obj.transform.Find("timeedit").gameObject;
        objEx.childObj1 = obj.transform.Find("djNum").gameObject;
        UIManager.SetButtonEventDelegate(objEx.itemObj.transform, mBtnDelegate);
        TriggerEvent te = obj.GetComponent<TriggerEvent>();
        if (null != te)
        {
            te.triggerdlgt = mTriggerDelegate;
            if (null == mFrameTriggerList)
            {
                mFrameTriggerList = new List<TriggerEvent>();
            }
            mFrameTriggerList.Add(te);
            te.enabled = false;
        }
        TweenScale scale = GameHelper.FindChildComponent<TweenScale>(obj.transform, "djlist");
        if (null != scale)
        {
            if (null == mFrameDjScaleList)
            {
                mFrameDjScaleList = new List<TweenScale>();
            }
            mFrameDjScaleList.Add(scale);
        }
        return objEx;
    }

    void InitFrameEx(ItemObjectEx item, ItemDataEx data, params object[] args)
    {
        try
        {
            bool updateDjFlag = (bool)args[0];
            bool updateFrameFlag = (bool)args[1];
            if (null != item.itemObj)
            {
                FrameDataEx dataEx = (FrameDataEx)data;
                BoxCollider frameBox = item.itemObj.GetComponent<BoxCollider>();
                if (null != frameBox)
                {
                    frameBox.size = new Vector3(data.width, data.height + 50, 0);
                    frameBox.center = new Vector3(0, 5, 0);
                }
                UISprite bg = GameHelper.FindChildComponent<UISprite>(item.itemObj.transform, "bg");
                if (null != bg)
                {
                    bg.width = (int)dataEx.width;
                    bg.height = (int)dataEx.height;
                    SetDottedBox(bg.transform, (int)dataEx.width, (int)dataEx.height);
                    if (null != dataEx.showList && dataEx.showList.Count == 1)
                    {
                        bg.alpha = 0;
                    }
                    else
                    {
                        bg.alpha = 1;
                    }
                }
                TweenAlpha tweenAlpha = item.itemObj.GetComponent<TweenAlpha>();
                if (null != tweenAlpha)
                {
                    tweenAlpha.enabled = dataEx.alphaFlag;
                }
                if (null == mSelectDuoJi && null != item.childObj)
                {
                    if (isEditState)
                    {
                        item.childObj.SetActive(true);
                    }
                }
                SetFrameTime(item.itemObj.transform, dataEx.action.sportTime);
                SetFrameIndex(item.itemObj.transform, dataEx.action.index);
                if (null != dataEx.showList && dataEx.showList.Count > 3)
                {
                    if (dataEx.isOpen)
                    {
                        if (null != item.childObj1)
                        {
                            item.childObj1.SetActive(false);
                        }
                    }
                    else
                    {
                        if (null != item.childObj1)
                        {
                            item.childObj1.SetActive(true);
                        }
                    }
                }
                else
                {
                    if (null != item.childObj1)
                    {
                        item.childObj1.SetActive(false);
                    }
                }
                if (updateDjFlag)
                {
                    UpdateFrameDj(item.itemObj, dataEx, updateFrameFlag);
                }
                
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 通过舵机的名字获取它所在帧的名字
    /// </summary>
    /// <param name="djName"></param>
    /// <returns></returns>
    string GetFrameForDj(string djName)
    {
        try
        {
            string[] str = djName.Split('_');
            if (null != str && 3 == str.Length)
            {
                return "frame_" + str[1];
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return string.Empty;
    }
    string GetFrameName(int index)
    {
        return "frame_" + index;
    }
    /// <summary>
    /// 通过帧上舵机的名字获取帧id和舵机id
    /// </summary>
    /// <param name="djName"></param>
    /// <returns></returns>
    Int2 GetIDFordjName(string djName)
    {
        Int2 data = new Int2();
        try
        {
            string[] str = djName.Split('_');
            if (null != str && 3 == str.Length)
            {
                data.num1 = int.Parse(str[1]);
                data.num2 = int.Parse(str[2]);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return data;
    }
    /// <summary>
    /// 设置按钮为激活状态还是sleep
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="active"></param>
    void SetBtnState(UIButton btn, bool active)
    {
        if (null != btn)
        {
            if (active)
            {
                btn.OnAwake();
            }
            else
            {
                btn.OnSleep();
            }
        }
    }
    /// <summary>
    /// 删除动作帧的舵机
    /// </summary>
    void DelActionDuoJi(FrameDataEx data, DuoJi dj)
    {
        int index = mFrameList.IndexOf(data);
        data.RomoveDj(dj.Id);
        if (index > 0)
        {
            FrameDataEx forntData = (FrameDataEx)mFrameList[index - 1];
            data.action.UpdateRota((byte)dj.Id, (short)forntData.GetRota(dj.Id));
        }
        else
        {
            data.action.UpdateRota((byte)dj.Id, PublicFunction.DuoJi_Start_Rota);
        }
        RemoveFrameDj(dj.Name);
        SetFrameOpenState(data, true);
        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("删除成功"), 1.5f, CommonTipsColor.green);
    }
    /// <summary>
    /// 删除动作的帧
    /// </summary>
    void DelActionFrame(FrameDataEx data)
    {
        /*ItemObjectEx item = mFrameCustomGrid.GetItemObject(data);
        if (null != item)
        {
            Transform djlist = item.itemObj.transform.Find("djlist");
            if (null != djlist)
            {
                for (int i = 0, imax = djlist.childCount; i < imax; ++i)
                {
                    Transform tmp = djlist.GetChild(i);
                    RemoveFrameDj(tmp.name);
                }
            }
        }*/
        RemoveFrame(data);
        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("删除成功"), 1.5f, CommonTipsColor.green);
    }
    void SetConnectState()
    {
        string iconName;
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            iconName = "btn_connect";
        }
        else
        {
            iconName = "btn_disconnect";
        }
        if (null != mConnectBtn)
        {
            mConnectBtn.normalSprite = iconName;
        }
        if (null != mConnectIcon)
        {
            mConnectIcon.spriteName = iconName;
            mConnectIcon.MakePixelPerfect();
        }
    }
    /// <summary>
    /// 设置下面一排舵机的角度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    void SetDuoJiAngle(int id, int rota)
    {
        try
        {
            string name = "dj_" + id.ToString();
            if (null != mDuojiDict && mDuojiDict.ContainsKey(name))
            {
                mDuojiDict[name].SetAngle(rota);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 设置下面一排舵机的角度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    void SetDuoJiAngle(Action action)
    {
        try
        {
            foreach (KeyValuePair<byte, short> kvp in action.rotas)
            {
                string name = "dj_" + kvp.Key.ToString();
                if (null != mDuojiDict && mDuojiDict.ContainsKey(name))
                {
                    mDuojiDict[name].SetAngle(kvp.Value);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /*void CleanUp()
    {
        try
        {
            if (null != mFrameCustomGrid)
            {
                mFrameCustomGrid.CleanUp();
            }
            mFrameIndex = 1;
            mNewFrameData = null;
            mAddToFrameData = null;
            mSelectFrameData = null;
            mCopyFrameData = null;
            mSelectDuoJi = null;
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }*/

    

    void InitItems(ActionSequence actions)
    {
        try
        {
            //CleanUp();
            mActions = actions;
            if (null != mActions)
            {
                if (null == mFrameList)
                {
                    mFrameList = new List<ItemDataEx>();
                }
                mFrameList.Clear();
                List<Action> list = mActions.GetActions();
                mFrameIndex = mFrameList.Count + 1;
                if (null != list)
                {
                    FrameDataEx frame;
                    for (int i = 0, imax = list.Count; i < imax; ++i)
                    {
                        frame = CreateFrameData();
                        frame.action.Copy(list[i], mRobot);
                        if (null != frame.action.GetShowID() && frame.action.GetShowID().Count > 0)
                        {
                            mFrameList.Add(frame);
                        }
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
    /// <summary>
    /// 像机器人发送角度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    void SendRota(int id, int rota, bool finished)
    {
        if (null == mRobot)
        {
            return;
        }
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            if (finished)
            {
                mLastSendDjRota = 0;
                mRobot.CtrlActionForDjId(id, rota);
            }
            else if (Time.time - mLastSendDjRota >= 0.1f)
            {
                mRobot.CtrlActionForDjId(id, rota);
                mLastSendDjRota = Time.time;
            }
        }
        else
        {
            Dictionary<int, int> dict = MoveSecond.GetDJLianDongData(id, rota);
            if (null != dict)
            {
                foreach (var kvp in dict)
                {
                    DuoJiData otherData = mRobot.GetAnDjData(kvp.Key);
                    if (null != otherData)
                    {
                        mRobot.GetAllDjData().UpdateData((byte)(otherData.id), (short)(otherData.startRota + kvp.Value));
                    }
                }
            }
        }
        EventMgr.Inst.Fire(EventID.Adjust_Angle_For_UI, new EventArg(id, rota));
    }

    void OpenReadBack()
    {
        if (null != mReadBackObj)
        {
            mReadBackObj.SetActive(true);
        }
        if (null != mReadBackBtnTrans)
        {
            //mReadBackBtnTrans.localPosition = mPowerDownBtnTrans.localPosition + mPowerDownBtnTrans.parent.localPosition + mPowerDownBtnTrans.parent.parent.localPosition;
            //PlayTweenPosition(mReadBackBtnPosition, Vector3.zero);
            mReadBackBtnTrans.localScale = Vector3.zero;
            mReadBackBtnTrans.localEulerAngles = new Vector3(0, 0, 270);
            GameHelper.PlayTweenRota(mReadBackBtnTweenRotation, Vector3.zero);
            GameHelper.PlayTweenScale(mReadBackBtnTweenScale, Vector3.one);
            if (null != mBigEffectTweenScale)
            {
                Timer.Add(0.25f, 1, 1, StartEffectTweenScale, mBigEffectTweenScale);
                /*mBigEffectTweenScale.transform.localScale = Vector3.one * 0.7f;
                GameHelper.PlayTweenScale(mBigEffectTweenScale, Vector3.one, 0.8f);*/
            }
            if (null != mSmallEffectTweenScale)
            {
                Timer.Add(0.2f, 1, 1, StartEffectTweenScale, mSmallEffectTweenScale);
                /*mSmallEffectTweenScale.transform.localScale = Vector3.one * 0.7f;
                GameHelper.PlayTweenScale(mSmallEffectTweenScale, Vector3.one, 0.8f);*/
            }
            if (null != mReadBackTipsTweenPosition)
            {
                Vector3 pos = UIManager.GetWinPos(mReadBackTipsTweenPosition.transform, UIWidget.Pivot.Bottom, 0, 120);
                mReadBackTipsTweenPosition.transform.localPosition = pos - new Vector3(0, 150);
                GameHelper.PlayTweenPosition(mReadBackTipsTweenPosition, pos);
            }
            if (null != mReadBackTipsTweenAlpha)
            {
                mReadBackTipsTweenAlpha.value = 0;
                GameHelper.PlayTweenAlpha(mReadBackTipsTweenAlpha, 1);
            }
            
        }
        //mPowerDownBtnTrans.gameObject.SetActive(false);
    }

    void StartEffectTweenScale(params object[] args)
    {
        TweenScale tweenScale = (TweenScale)args[0];
        tweenScale.transform.localScale = Vector3.one * 0.7f;
        GameHelper.PlayTweenScale(tweenScale, Vector3.one, 0.8f);
    }



    void CloseReadBack()
    {
        if (null == mReadBackObj || !mReadBackObj.activeSelf)
        {
            return;
        }
        GameHelper.PlayTweenRota(mReadBackBtnTweenRotation, new Vector3(0, 0, 270));
        GameHelper.PlayTweenScale(mReadBackBtnTweenScale, Vector3.zero);
        if (null != mBigEffectTweenScale)
        {
            mBigEffectTweenScale.ResetToBeginning();
            mBigEffectTweenScale.enabled = false;
        }
        if (null != mSmallEffectTweenScale)
        {
            mSmallEffectTweenScale.ResetToBeginning();
            mSmallEffectTweenScale.enabled = false;
        }
        if (null != mReadBackTipsTweenPosition)
        {
            Vector3 pos = UIManager.GetWinPos(mReadBackTipsTweenPosition.transform, UIWidget.Pivot.Center);
            GameHelper.PlayTweenPosition(mReadBackTipsTweenPosition, pos);
        }
        if (null != mReadBackTipsTweenAlpha)
        {
            GameHelper.PlayTweenAlpha(mReadBackTipsTweenAlpha, 0);
        }
        Timer.Add(0.3f, 1, 1, CloseReadBackOnFinished);
    }

    void CloseReadBackOnFinished()
    {
        if (null != mReadBackObj)
        {
            mReadBackObj.SetActive(false);
        }
        /*if (PlatformMgr.Instance.GetBluetoothState() && isEditState)
        {
            mPowerDownBtnTrans.gameObject.SetActive(true);
        }*/
    }
    /// <summary>
    /// 设置动作图标
    /// </summary>
    /// <param name="iconName"></param>
    void SetActionIcon(string iconName)
    {
        if (null != mActionIcon)
        {
            mActionIcon.spriteName = iconName;
            mActionIcon.MakePixelPerfect();
        }
    }
    /// <summary>
    /// 设置动作名字
    /// </summary>
    /// <param name="name"></param>
    void SetActionNameLabel(string name)
    {
        if (null != mActionNameLabel)
        {
            mActionNameLabel.text = name;
        }
    }

    void Simulation(FrameDataEx selectItem = null)
    {
        try
        {
            isPlaying = true;
            SetEventActive(false);
            if (null == mSimulationActs)
            {
                string robotId = string.Empty;
                if (null != mRobot)
                {
                    robotId = mRobot.ID;
                }
                mSimulationActs = new ActionSequence(robotId);
            }
            int startIndex = 0;
            if (null != selectItem)
            {
                startIndex = mFrameList.IndexOf(selectItem);
                if (-1 == startIndex)
                {
                    startIndex = 0;
                }
            }
            mSimulationActs.ClearActions();
            FrameDataEx data;
            for (int i = 0, icount = mFrameList.Count; i < icount; ++i)
            {
                data = (FrameDataEx)mFrameList[i];
                mSimulationActs.AddAction(data.action);
            }
            StartPlay(startIndex);
            if (null != mRobot)
            {
                mRobot.PlayActions(mSimulationActs, PlayActionsDelegate, startIndex);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    //开始播放
    void StartPlay(int startIndex)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "StartPlay startIndex =" + startIndex);
        if (isEditState)
        {//编辑状态播放全部展开
            SetEditAllFrameOpenState(true);
        }
        //移到开始位置
        mFrameCustomGrid.FrameMoveToFront(startIndex, true);

        if (mFrameList.Count > 0)
        {
            mNowPlayDataIndex = startIndex;
            mNowPlayObjectIndex = mFrameCustomGrid.GetItemIndex(mFrameList[startIndex]);
            PlayFrame(mNowPlayObjectIndex, mNowPlayDataIndex);
        }
    }
    //暂停播放
    void PauseAction()
    {
        MyTime.GetInst().PauseTime();
        if (isEditState)
        {//编辑状态播放全部展开
            SetEditAllFrameOpenState(false);
        }
        SetEventActive(true);
    }
    //继续播放
    void ContinueAction()
    {
        if (mNowPlayDataIndex >= mFrameList.Count)
        {//播放完毕了，重新开始
            Simulation(null);
        }
        else
        {
            Simulation((FrameDataEx)mFrameList[mNowPlayDataIndex]);
        }
        /*mSimulationActs.ClearActions();
        FrameDataEx data;
        for (int i = 0, icount = mFrameList.Count; i < icount; ++i)
        {
            data = (FrameDataEx)mFrameList[i];
            mSimulationActs.AddAction(data.action);
        }
        if (isEditState)
        {//编辑状态播放全部展开
            SetEditAllFrameOpenState(true);
        }
        //移到开始位置
        mFrameCustomGrid.FrameMoveToFront(mNowPlayDataIndex, true);
        isPlaying = true;
        SetEventActive(false);
        MyTime.GetInst().ContinueTime();*/
    }
    /// <summary>
    /// 停止播放
    /// </summary>
    void StopAction()
    {

    }

    void PlayFrame(int itemIndex, int dataIndex)
    {
        if (-1 != dataIndex)
        {
            FrameDataEx data = null;
            Action frontAct = null;
            if (dataIndex < mFrameList.Count)
            {
                data = (FrameDataEx)mFrameList[dataIndex];
                if (dataIndex > 0)
                {
                    frontAct = ((FrameDataEx)mFrameList[dataIndex - 1]).action;
                }
                else
                {
                    frontAct = new Action();
                }
            }
            if (null != frontAct)
            {//把角度设置成前一帧的角度
                SetFrameAngle(data, frontAct, true);
            }
            if (null != data)
            {
                SetFrameAngle(data, data.action, false);
                //运动
                if (null != mFrameCustomGrid)
                {
                    mFrameCustomGrid.MoveWithinBounds(new Vector3(-data.width, 0), false, data.action.AllTime);
                }
            }
        }
    }

    void SetFrameAngle(FrameDataEx data, Action action, bool instant)
    {
        if (null != data.showList)
        {
            for (int i = 0, imax = data.showList.Count; i < imax; ++i)
            {
                string name = DuoJi.GetFrameDjName(data.action.index, data.showList[i]);
                if (null != mFrameUseDjDict && mFrameUseDjDict.ContainsKey(name))
                {
                    mFrameUseDjDict[name].SetAngle(action.GetRota(data.showList[i]), instant, action.sportTime / 1000.0f);
                }
            }

        }
    }
    void PlayActionsDelegate(int index, bool finished)
    {
        try
        {
            if (finished)
            {
                StopPlayActions(null);
            }
            else
            {
                ++mNowPlayDataIndex;
                if (isPlaying)
                {
                    PlayFrame(mNowPlayObjectIndex, mNowPlayDataIndex);
                    EventMgr.Inst.Fire(EventID.Ctrl_Robot_Action, new EventArg(mSimulationActs[mNowPlayDataIndex]));
                }
                else if (isPause)
                {//暂停了
                    PauseAction();
                }
                isPause = false;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            isPause = false;
        }

    }

    void StopPlayActions(EventArg arg)
    {
        try
        {
            mSimulationActs = null;
            isPlaying = false;
            SetEventActive(true);
            SetPlayIconState(isPlaying);
            if (isEditState)
            {
                SetEditAllFrameOpenState(false);
                SetSelectFrame(null);
            }
            EventMgr.Inst.Fire(EventID.UI_Post_Robot_Select_ID);
            isPause = false;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SetActionSaveFlag(bool saveFlag)
    {
        if (null != mActions)
        {
            mActions.NeedSave = saveFlag;
        }
        ChangeSaveBtnState();
    }

    void ChangeSaveBtnState()
    {
        if (null == mSaveBtn || null == mSaveAsBtn)
        {
            return;
        }
        do 
        {
            if (isOfficial)
            {
                mSaveBtn.OnSleep();
            }
            else
            {
                if (mFrameList.Count > 0)
                {
                    mSaveBtn.OnAwake();
                }
                else
                {
                    mSaveBtn.OnSleep();
                }
            }
            if (mFrameList.Count > 0 && null != mActions)
            {
                mSaveAsBtn.OnAwake();
            }
            else
            {
                mSaveAsBtn.OnSleep();
            }
        } while (false);
        
    }

    void OnSaveActions(EventArg arg)
    {
        try
        {
            if (mFrameList.Count < 1)
            {
                return;
            }
            string name = (string)arg[0];
            string icon = (string)arg[1];
            bool saveAsFlag = (bool)arg[2];
            ActionSequence actions = null;
            if (saveAsFlag)
            {
                string robotID = string.Empty;
                if (null != mRobot)
                {
                    robotID = mRobot.ID;
                }
                actions = new ActionSequence(robotID);
                actions.Name = name;
                actions.IconID = icon;
                mActions = actions;
                
            }
            else
            {
                CreateActions(name, icon);
                mActions.ClearActions();
                actions = mActions;
                /*SetActionIcon(ActionsManager.GetInst().GetActionIconName(icon));
                SetActionNameLabel(name);*/
            }
            SetActionIcon(actions.IconName);
            SetActionNameLabel(actions.Name);
            for (int i = 0, imax = mFrameList.Count; i < imax; ++i)
            {
                FrameDataEx dataEx = (FrameDataEx)mFrameList[i];
                if (null != dataEx)
                {
                    actions.AddAction(dataEx.action);
                }
            }
            /*ActionSequence tmp = ActionsManager.GetInst().GetAction(mActions.RobotID, mActions.Id);
            if (null != tmp)
            {//已存在的动作
                if (!tmp.Name.Equals(mActions.Name))
                {//修改了名字需重置ID
                    mActions.ReCreateID();
                }
            }*/
            actions.Save();
            if (null != mRobot)
            {
                ActionSequence acopy = new ActionSequence(actions, mRobot);
                acopy.SetOfficial(false);
                RobotManager.GetInst().AddRobotActions(mRobot, acopy);
                /*if (ResourcesEx.GetRobotType(mRobot) == ResFileType.Type_playerdata)
                {
                    PlatformMgr.Instance.MobClickEvent(MobClickEventID.DIYRobot_actionQuantityPerRobot, mRobot.GetActionsIdList().Count);
                }*/
            }
            SetActionSaveFlag(false);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    bool CheckNeedSave()
    {
        try
        {
            if (mFrameList.Count < 1)
            {
                return false;
            }
            if (null == mActions)
            {
                if (mFrameList.Count > 0)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return mActions.NeedSave;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return false;
    }

    void SetPlayIconState(bool playFlag)
    {
        if (null != mPlayBtnIcon && null != mPlayBtn)
        {
            if (playFlag)
            {
                mPlayBtnIcon.spriteName = "icon_stop";
                mPlayBtnIcon.transform.localPosition = Vector3.zero;
            }
            else
            {
                mPlayBtnIcon.spriteName = "icon_play";
                mPlayBtnIcon.transform.localPosition = Vector3.zero;
            }
            mPlayBtn.normalSprite = mPlayBtnIcon.spriteName;
            /*if (null != mPlayBtn)
            {
                mPlayBtn.normalSprite = mPlayBtnIcon.spriteName;
            }*/
            //mPlayBtnIcon.MakePixelPerfect();
        }
    }
    void QuitEditScene(EventArg arg)
    {
        try
        {
            
           
            if (CheckNeedSave() && PlatformMgr.Instance.EditFlag)
            {//需要保存
                PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("您还未保存该动作，是否需要保存？"), PromptSaveActionsMsgOnClick, null, delegate(BasePopWin popMsg) {
                    if (popMsg is PromptMsg)
                    {
                        PromptMsg msg = (PromptMsg)popMsg;
                        msg.SetLeftBtnText(LauguageTool.GetIns().GetText("否"));
                        msg.SetRightBtnText(LauguageTool.GetIns().GetText("是"));
                    }
                });
                
                /*if (null != mRobot)
                {
                    CreateActionsUI.ShowMsg(CreateActionsUI.ActionsMsgType.Actions_Msg_Save, mRobot.ID, mActions, OnQuitEdit);
                }*/
            }
            else
            {
                OnQuitEdit();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void OnQuitEdit()
    {
        OnQuitEditMain(false);
    }

    void OnQuitEditMain(bool toMainFlag)
    {
        try
        {
            SceneMgrTest.Instance.LastScene = SceneType.EditAction;
            
            if (null != mRobot)
            {
                mRobot.StopNowPlayActions();
            }
            SingletonObject<PublicTools>.GetInst().ResetCameraPos(false);
            MainScene.GotoScene(MainMenuType.Action_Menu);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void PromptSaveActionsMsgOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.LeftBtnName))
        {
            OnQuitEdit();
        }
        else if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            if (null != mRobot)
            {
                CreateActionsUI.ShowMsg(CreateActionsUI.ActionsMsgType.Actions_Msg_Save, mRobot.ID, mActions, OnQuitEdit);
            }
            else
            {
                OnQuitEdit();
            }
        }
    }

    void OnBlueConnectResult(EventArg arg)
    {
        try
        {
            bool flag = (bool)arg[0];
            SetConnectState();
            if (!flag)
            {
                CloseReadBack();
            }
            if (flag/* && !mPowerDownBtnTrans.gameObject.activeSelf*/)
            {//需要显示回读按钮
             //Vector4 rect = mFrameListStartClip;
             //rect.z -= mPowerDownBtnWidth;
             //SetFrameListClip(rect);
             //UpdatePowerDownPos();
                if (null != mPowerDownBtn)
                {
                    mPowerDownBtn.OnAwake();
                }
                //mPowerDownBtnTrans.gameObject.SetActive(true);
                //UpdateScrollBar();
            }
            else if (!flag/* && mPowerDownBtnTrans.gameObject.activeSelf*/)
            {//需要隐藏回读按钮
                if (null != mPowerDownBtn)
                {
                    mPowerDownBtn.OnSleep();
                }
                /*SetFrameListClip(mFrameListStartClip);
                mPowerDownBtnTrans.gameObject.SetActive(false);
                UpdateScrollBar();*/
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void OnReadBackSuccess(EventArg arg)
    {
        try
        {//回读成功
            if (null != mReadBackObj && mReadBackObj.activeSelf)
            {
                CloseReadBack();
                Action action = (Action)arg[0];
                SetDuoJiAngle(action);
                if (null != mRobot)
                {
                    List<byte> angleList = mRobot.GetAllDjData().GetAngleList();
                    if (angleList.Count < 1)
                    {
                        return;
                    }
                    List<byte> showList = mRobot.GetReadBackShowList();
                    if (showList.Count == 0)
                    {//未改变角度
                        if (mFrameList.Count == 0)
                        {//第一帧显示所有舵机
                            showList = angleList;
                        }
                        else
                        {//显示最后一帧的舵机数
                            showList = ((FrameDataEx)mFrameList[mFrameList.Count - 1]).showList;
                        }
                    }
                    FrameDataEx data = AddFrame(true);
                    mRobot.GetNowAction(data.action);
                    if (null != showList)
                    {
                        for (int i = 0, imax = showList.Count; i < imax; ++i)
                        {
                            data.AddDj(showList[i]);
                        }
                    }
                    ItemObjectEx item = mFrameCustomGrid.GetItemObject(data);
                    if (null != item)
                    {
                        InitFrameEx(item, data, true, true);
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

    void OnReadBackFailed(EventArg arg)
    {
        try
        {
            CloseReadBack();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void OnReadBackAck(EventArg arg)
    {
        try
        {
            CloseReadBack();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void OnReadPowerResult(EventArg arg)
    {
        if (PlatformMgr.Instance.IsChargeProtected)
        {
            PublicPrompt.ShowChargePrompt(AutoSaveActions);
        }
    }

    void AutoSaveActions()
    {
        if (!isOfficial && null != mRobot && mFrameList.Count > 0)
        {
            if (null == mActions)
            {
                string iconId = ActionsManager.GetInst().GetNewActionsIconID(mRobot.ID);
                if (!string.IsNullOrEmpty(iconId))
                {
                    string createName = string.Empty;
                    int count = 1;
                    do
                    {
                        createName = string.Format("Action{0}", count);
                        ErrorCode ret = ActionsManager.GetInst().CheckActionsName(mRobot.ID, createName, string.Empty);
                        if (ErrorCode.Result_OK == ret)
                        {
                            break;
                        }
                        count++;
                    } while (true);
                    OnSaveActions(new EventArg(createName, iconId, false));
                }
            }
            else
            {
                OnSaveActions(new EventArg(mActions.Name, mActions.IconID, false));
            }
        }
        OnQuitEditMain(true);
    }
    
    #endregion

    public class FrameDataEx : ItemDataEx
    {
        public Action action;
        public bool alphaFlag;
        public bool isOpen;


        public List<byte> showList
        {
            get { return action.GetShowID(); }
        }

        
        public FrameDataEx(int index, string name, float width, float height, int sportTime)
            : base(name, width, height)
        {
            action = new Action(index, sportTime);
            alphaFlag = false;
            isOpen = false;
        }

        public int GetRota(int id)
        {
            return action.GetRota((byte)id);
        }

        public void AddDj(int id)
        {
            action.AddShowID(id);
        }

        public void RomoveDj(int id)
        {
            action.RomoveShowID(id);
        }
        /// <summary>
        /// 用于复制
        /// </summary>
        /// <returns></returns>
        public FrameDataEx OnCopy(Robot robot)
        {
            FrameDataEx data = new FrameDataEx(action.index, name, width, height, action.sportTime);
            data.action.Copy(action, robot);
            data.alphaFlag = false;
            data.isOpen = false;
            return data;
        }
    }
}

