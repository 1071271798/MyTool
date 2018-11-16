using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.UI;
using Game.Scene;
using System;
using Game.Platform;
using Game.Event;
//using System.IO;

/// <summary>
/// author: 孙宇
/// describe:遥控主界面
/// time: 
/// </summary>
/// 
public class UserdefControllerUI : BaseUI
{
    #region  //保护属性
    private UserCotrollerSettingUI setnewUI;
    protected List<BaseUI> mnewUIList;
    protected GameObject sliderToogle;
    protected Transform leftTrans;
    protected Transform topTrans;
    protected Transform bottomTrans;
    protected Transform gridPanel;
    protected UISprite setBgSprite;
    protected Transform widgetsParent;
    protected Transform centerTip;
    protected Transform centerStartTip;
    protected Transform powerDownBtn;
    protected Transform connectBtn;
    protected Transform settingBtn;
    protected Transform powerTrans;
    protected UILabel editTitle;
    protected UILabel editDescribe;
    protected UISprite placedArea;
    protected Transform topRightIcons;
    protected Transform sliderBottoms;
    protected Transform grayBackgrounds;
    protected Transform tireBackgrounds;
    protected GameObject curDragControl;
    //protected Transform cleanBoxCollider;

    private SmartPlace smartPlace;  // 

    private Transform selectActionState;

    public static SliderWidgetData curVsliderServoNum;
    public static int curHsliderServoNum;
    private UISprite curVsliderSelectServo;
    private UISprite curHsliderSelectServo;
    private UISprite curVsliderShadow;
    private UISprite curHsliderShadow;

    private UICamera curCameraViews; // 当前相机
    private GameObject curSelectControllerWidget; // 当前选中的控件

    private int isExitCollision = 0; //是否碰撞，当拖动时 没有碰撞的情况下绘制可拖动面积
    public static bool isTotalDataChange = false;  // 检测数据是否发生改变
    private bool isActionDataChange = false;
    private bool isFirstController = false;
    public bool isFirstSetting = false; //默认进入设置界面
    private bool _isSetting;
    private bool isZeroArea;
    private bool isVsliderServo = false;
    private bool isHsliderServo = false;
    private bool isVsliderChange = false;
    private bool isHsliderChange = false;
    private bool isFirstEdit = false;
    private float positionXRatio = 1.0f;
    private float positionYRatio = 1.0f;
    private float scaleRatio = 1.0f;
    private string curVsliderName;
    private string curHsliderName;
    public static bool isSetting;
    private bool showPowerFlag = true;

    int Action_Icon_Size_Width = 70;
    int Action_Icon_Size_Height = 70;

    PowerShow mPowerShow;
    protected bool IsSetting  //遥控设置
    {
        get
        {
            return _isSetting;
        }
        set
        {
            isSetting = value;
            _isSetting = value;
            //    ShowOrHideLines(value);
            if (leftTrans != null)
                leftTrans.gameObject.SetActive(value);
            ShowOrHideTopBoard(value);
            ShowOrHideLeftboard(value);
            if (UICamera.current != null)
                UICamera.current.allowMultiTouch = !value;
            //     UICamera.current.allowMultiTouch = !value;
            
            if (sliderToogle != null)
                sliderToogle.SetActive(value);
            
            DragdropItemEX dragObj;
            for (int i = 0; i < gridPanel.childCount; i++)
            {
                dragObj = gridPanel.GetChild(i).GetComponent<DragdropItemEX>();
                if(dragObj != null)
                    dragObj.enabled = value;
            }
        }
    }

    private List<string> showActionList;

    /*UITexture mModelIcon;
    UISprite mEffectIcon;*/
    UISprite mConnectIcon;
    UIButton mConnectBtn;
    #endregion

    #region 公有函数
    public UserdefControllerUI(bool isFirstEnter)
    {
        mUIResPath = "Prefab/UI/control/userdefineControllerUI";
        isTotalDataChange = false;
        isActionDataChange = false;
        isFirstEdit = isFirstEnter;
        //editTitle.enabled = true;
        //Debug.Log("ReEnter is Controller");

        smartPlace = new SmartPlace();
    }
    #endregion
    #region 私有的
    /// <summary>
    /// 返回
    /// </summary>
    void OnBackbtnClicked()
    {
        //Debug.Log("MainWindow");
        Transform topL = mTrans.Find("Center/gridPanel").transform;
        if (topL != null && topL.childCount <= 1)
        {
            //Debug.Log("Delete Controller is " + RobotManager.GetInst().GetCurrentRobot().ID);
            ControllerManager.DeletaController(RobotManager.GetInst().GetCurrentRobot().ID);
        }
        
        mTrans.gameObject.SetActive(false);
        //RobotMgr.Instance.openActionList = true;
        //SceneMgr.EnterScene(SceneType.MainWindow);
        UserdefControllerScene.GoBack();
    }
    /// <summary>
    /// 修改舵机类型界面
    /// </summary>
    /// <param name="go"></param>
    public void ModifyServoTypeSetting(GameObject obj)
    {
        try
        {
            if (obj == null)
            {
                return;
            }
            string btnname = obj.name;
            if (btnname.Equals(PromptMsg.LeftBtnName))
            {
                return;
            }
            else if (btnname.Equals(PromptMsg.RightBtnName))
            {
                // 进入修改舵机轮模式界面
                SetServoTypeMsg.ShowMsg();
                //PublicPrompt.ShowClickBlueBtnMsg();
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
    }
    /// <summary>
    /// 修改取消
    /// </summary>
    /// <param name="go"></param>
    void OnSecondbackClicked(GameObject obj)
    {
        try
        {
            if (obj == null)
            {
                IsSetting = false;
                secBacks.gameObject.SetActive(false);
                return;
            }
            string btnname = obj.name;
            if (btnname.Equals(PromptMsg.LeftBtnName))
            {
                editTitle.enabled = false;
                editDescribe.enabled = false;
                ShowOrHideLines(false);
                secBacks.gameObject.SetActive(false);
                //Debug.Log("btnname is run");
                IsSetting = false;
                //取消修改
                if (isTotalDataChange)
                {
                    //Debug.Log("DataToWidget is run2");
                    ControllerManager.GetInst().CancelCurController();
                    if (gridPanel.childCount > 1)
                    {
                        for (int i = 1; i < gridPanel.childCount; i++)
                            GameObject.Destroy(gridPanel.GetChild(i).gameObject);
                    }
                    DataToWidget();
                }
            }
            else if (btnname.Equals(PromptMsg.RightBtnName))
            {
                editTitle.enabled = false;
                editDescribe.enabled = false;
                ShowOrHideLines(false);
                secBacks.gameObject.SetActive(false);
                if (isTotalDataChange)
                {
                    OnConfirmClicked();
                    //DataToWidget();
                }    
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
    }

    /// <summary>
    /// controldata数据显示出来
    /// </summary>
    void DataToWidget()
    {
        isZeroArea = false;

        smartPlace.Clear();
        smartPlace.SetBgBoard(new Vector4(-gridPanel.GetComponent<UIWidget>().width / 2.0f + (PublicFunction.Back_Btn_Pos.x * positionXRatio), gridPanel.GetComponent<UIWidget>().height / 2.0f, gridPanel.GetComponent<UIWidget>().width / 2.0f - (PublicFunction.Back_Btn_Pos.x * positionXRatio), -gridPanel.GetComponent<UIWidget>().height / 2.0f));
        //Debug.Log((-Screen.width / 2.0f + (UserdefControllerScene.leftSpace * Screen.width / 1334.0f) + " " + (Screen.height / 2.0f) + " " + (Screen.width / 2.0f - (UserdefControllerScene.leftSpace * Screen.width / 1334.0f)) + " " + (-Screen.height / 2.0f)));

        GameObject actionWidget = Resources.Load("Prefabs/actionWidget") as GameObject; 
        GameObject vsliderWidget = Resources.Load("Prefabs/vSlider") as GameObject;
        GameObject hsliderWidget = Resources.Load("Prefabs/hSlider") as GameObject;
        GameObject joystickWidget = Resources.Load("Prefabs/joystick") as GameObject;

        Transform leftItems = mTrans.Find("Left/leftBoard/ContainerLeft/EditScrollview/Grid").transform;

        if (leftItems != null)
        {
            //Debug.Log("leftItem is not null!!");
            leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(2).GetChild(4).GetComponent<UILabel>().enabled = false;
            leftItems.GetChild(2).GetChild(5).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(2).GetChild(6).GetComponent<UISprite>().enabled = false;
            //leftItems.GetChild(3).GetChild(1).GetChild(1).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(3).GetChild(3).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(3).GetChild(4).GetComponent<UILabel>().enabled = false;
        }
        
        //将遥控数据显示出来
        if (ControllerManager.GetInst().TurnShowTypeList() == null)
        {            
            centerTip.gameObject.SetActive(true);
            centerStartTip.gameObject.SetActive(true);
            connectBtn.gameObject.SetActive(false);
            powerDownBtn.gameObject.SetActive(false);
            if (null != mPowerShow)
            {
                mPowerShow.HidePower();
            }
            showPowerFlag = false;
        }
        else
        {
            centerTip.gameObject.SetActive(false);
            centerStartTip.gameObject.SetActive(false);
            connectBtn.gameObject.SetActive(true);
            powerDownBtn.gameObject.SetActive(true);
            showPowerFlag = true;
            if (PlatformMgr.Instance.GetBluetoothState() && null != mPowerShow)
            {
                mPowerShow.ShowPower();
            }
        }
        List<ControllerManager.WidgetShowType> widgetList = ControllerManager.GetInst().TurnShowTypeList();
        if (null != widgetList)
        {
            foreach (var tem in widgetList)
            {
                GameObject oo = null;
                if (tem.type == ControllerManager.WidgetShowType.widgetType.action)
                {
                    oo = GameObject.Instantiate(actionWidget) as GameObject;
                    oo.tag = "widget_action"; // 遥控面板的tag跟动作列表里的tag 响应的点击事件不一样
                    oo.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
                    /*if (!IsSetting)
                        oo.transform.GetComponent<UIButtonScale>().enabled = true;
                    else
                        oo.transform.GetComponent<UIButtonScale>().enabled = false;*/
                }
                else if (tem.type == ControllerManager.WidgetShowType.widgetType.vSlider)
                {
                    oo = GameObject.Instantiate(vsliderWidget) as GameObject;
                    oo.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                    oo.transform.GetChild(4).GetComponent<UILabel>().enabled = true;
                }
                else if (tem.type == ControllerManager.WidgetShowType.widgetType.hSlider)
                {
                    oo = GameObject.Instantiate(hsliderWidget) as GameObject;
                    //oo.transform.GetChild(1).GetChild(1).GetComponent<UISprite>().enabled = false;
                    oo.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                    oo.transform.GetChild(4).GetComponent<UILabel>().enabled = true;
                }
                else if (tem.type == ControllerManager.WidgetShowType.widgetType.joystick)
                {
                    oo = GameObject.Instantiate(joystickWidget) as GameObject;
                    oo.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                }

                if (oo != null)
                {
                    oo.transform.SetParent(gridPanel);
                    oo.transform.localScale = Vector3.one;
                    oo.transform.localPosition = new Vector3(tem.pos.x * positionXRatio, tem.pos.y * positionYRatio, 0.0f);
                    oo.name = tem.widgetID;
                    GetTCompent.GetCompent<DragdropItemEX>(oo.transform).enabled = false;
                    GetTCompent.AddCompent<BoxCollider>(oo.transform);
                    ButtonDelegate del = new ButtonDelegate();
                    //Debug.Log("onDragdropStart");
                    del.onDragdropStart = OnDragdropStart;
                    del.onDragdropRelease = OnDragdropRelease;
                    del.onDrag = OnButtonDrag;
                    del.onClick = OnButtonClick;
                    del.onPress = OnButtonPress;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);

                    if (tem.type == ControllerManager.WidgetShowType.widgetType.action)  //获取对应的动作图标
                    {
                        //string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).actionNm;// 
                        string actionId = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).actionId;
                        if (actionId != null && actionId != "")
                        {
                            //Debug.Log("actionName is " + actionName);
                            ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionId);
                            if (null == act)
                            {
                                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "add";
                            }
                            else
                            {
                                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
                            }
                            
                            string actionIcon = oo.transform.GetChild(0).GetComponent<UISprite>().spriteName;
                            if (RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionId) != null)
                                ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).actionNm = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionId).Name;
                            
                            //string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).actionNm;
                            //Debug.Log("Get actionIcon is " + actionIcon);
                            //Debug.Log("Get actionName is " + actionName);
                            /*if (actionId != "")
                            {
                                Debug.Log("Get actionId is " + RobotManager.GetInst().GetCurrentRobot().GetActionsIconForID(actionId));
                            }*/
                        }
                        oo.transform.GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
                    }

                    if (tem.type == ControllerManager.WidgetShowType.widgetType.vSlider)
                    {
                        //Debug.Log("obj name is " + oo.tag);
                        if (!((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).isOK)
                        {
                            oo.transform.GetChild(4).GetComponent<UILabel>().text = "";
                            curVsliderServoNum = null;
                        }
                        else
                        {
                            SliderWidgetData servoIDs = ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name));
                            if (null != servoIDs)
                            {
                                if (servoIDs.motionType == CtrlMotionType.servo)
                                {
                                    oo.transform.GetChild(4).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("舵机") + " " + servoIDs.servoID;
                                }
                                else
                                {
                                    oo.transform.GetChild(4).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("马达") + " " + servoIDs.servoID;
                                }
                            }
                            //curVsliderServoNum = servoIDs;
                        }
                        oo.transform.GetChild(5).GetComponent<UISprite>().enabled = true;
                        oo.transform.GetChild(6).GetComponent<UISprite>().enabled = true;
                        if (((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).directionDisclock)
                        {
                            oo.transform.GetChild(5).localEulerAngles = new Vector3(0, 180, 0);
                            oo.transform.GetChild(6).localEulerAngles = Vector3.zero;
                        }
                        else
                        {
                            oo.transform.GetChild(5).localEulerAngles = Vector3.zero;
                            oo.transform.GetChild(6).localEulerAngles = new Vector3(0, 180, 0);
                        }
                    }

                    if (tem.type == ControllerManager.WidgetShowType.widgetType.hSlider)
                    {
                        //Debug.Log("obj name is " + oo.tag);
                        //oo.transform.GetChild(1).GetChild(1).GetComponent<UISprite>().enabled = false;
                        if (!((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).isOK)
                        {
                            oo.transform.GetChild(4).GetComponent<UILabel>().text = "";
                            curHsliderServoNum = 0;
                        }
                        else
                        {
                            int servoIDs = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).servoID;
                            oo.transform.GetChild(4).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("舵机") + " " + servoIDs.ToString();
                            //curHsliderServoNum = servoIDs;
                        }
                        int minAngles = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).min_angle;
                        int maxAngles = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).max_angle;
                        oo.transform.GetChild(5).GetComponent<UILabel>().text = minAngles.ToString() + "°";
                        oo.transform.GetChild(6).GetComponent<UILabel>().text = maxAngles.ToString() + "°";
                    }
                    //Debug.Log("DataToWidget Add Board");
                    smartPlace.AddBoard(new SmartPlace.RectBoard(oo.name, TurnWidgetRect(oo.GetComponentInChildren<UIWidget>())));
                }
            }
        }
        
        ControllerManager.GetInst().ControllerReady();

        if (isFirstController)
        {
            isFirstController = false;
            //IsSetting = false;
            UICamera camGO = GameObject.Find("UIRoot(2D)(2)/UICamera").transform.GetComponent<UICamera>();
            if (camGO != null)
                camGO.allowMultiTouch = true;
            //ControllerManager.GetInst().CancelCurController();
            //UICamera.current.allowMultiTouch = true;
            //Debug.Log("第一次进入遥控更新数据！！" + UICamera.current.allowMultiTouch);
        }
        //Debug.Log("Controller is Ready!!");

        //curSelectControllerWidget = null;

        InitActionList();
    }
    Vector4 TurnWidgetRect(UIWidget widget)
    {
        if (widget == null)
            return Vector4.zero;
        return new Vector4(widget.transform.localPosition.x - widget.width / 2.0f, widget.transform.localPosition.y + widget.height / 2.0f, widget.transform.localPosition.x + widget.width / 2.0f, widget.transform.localPosition.y - widget.height / 2.0f);
    }
    /// <summary>
    /// 遥控修改确认
    /// </summary>
    /// <param name="go"></param>
    void OnConfirmClicked()
    {
        IsSetting = false;
        //发通知出去  本地遥控数据更新，动作列表更新
        if (isTotalDataChange)
        {
            ControllerManager.GetInst().SaveCurController();
            ControllerManager.GetInst().ControllerReady();
        }
        if (isActionDataChange)
        {
            //Debug.Log("isActionDataChange is " + isActionDataChange);
            //InitActionList();
        }
        Transform topL = mTrans.Find("Center/gridPanel").transform;
        if (topL != null && topL.childCount <= 1)
        {
            //Debug.Log("Delete Controller is " + RobotManager.GetInst().GetCurrentRobot().ID);
            ControllerManager.DeletaController(RobotManager.GetInst().GetCurrentRobot().ID);
            centerTip.gameObject.SetActive(true);
            centerStartTip.gameObject.SetActive(true);
            connectBtn.gameObject.SetActive(false);
            powerDownBtn.gameObject.SetActive(false);
            showPowerFlag = false;
            if (null != mPowerShow)
            {
                mPowerShow.HidePower();
                
            }
        }
        else
        {
            centerTip.gameObject.SetActive(false);
            centerStartTip.gameObject.SetActive(false);
            connectBtn.gameObject.SetActive(true);
            powerDownBtn.gameObject.SetActive(true);
            showPowerFlag = true;
            if (PlatformMgr.Instance.GetBluetoothState() && null != mPowerShow)
            {
                mPowerShow.ShowPower();
            }
        } 
    }
    /// <summary>
    /// 设置
    /// </summary>
    /// <param name="go"></param>
    void OnSettingbtnClicked()
    {
        IsSetting = true;
        isActionDataChange = false;
        isTotalDataChange = false;
    }

    /// <summary>
    /// 动作停止
    /// </summary>
    /// <param name="go"></param>
    void OnStopbtnClicked(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click stop action button!!");

        ActionLogic.GetIns().DoStopAction(go);
    }
    
    /// <summary>
    /// 伸缩左侧面板
    /// </summary>
    /// <param name="go"></param>
    void OnExplorebtnClicked()
    {     
        ShowOrHideLeftboard(!isLeftShow);
    }
    /// <summary>
    /// 网格显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    void ShowOrHideLines(bool isshow)
    {
        if (gridPanel.GetComponent<UISprite>() != null)
            gridPanel.GetComponent<UISprite>().enabled = isshow;
        if (null != setBgSprite)
        {
            setBgSprite.enabled = isshow;
        }
    }
    /// <summary>
    /// 左侧面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isLeftShow = false;
    void ShowOrHideLeftboard(bool isshow)
    {
        if (leftTrans == null || isshow == isLeftShow)
            return;
        isLeftShow = isshow;
        if (isshow)
        {
            //Debug.Log("now child num is " + leftItems.childCount);
            if (leftItems.childCount > 4)
            {
                for (int kii = 4; kii < leftItems.childCount; kii++)
                {
                    if (leftItems.GetChild(kii) != null)
                        GameObject.Destroy(leftItems.GetChild(kii).gameObject);
                }
            }
        }
        if (isshow && isBottomShow)  //底部如果存在则消失
        {
            ShowOrHideBottmBoard(false);
        }
        HideOrShowTrans(isshow, leftTrans, directType.left);
        sliderToogle.transform.GetChild(0).transform.Rotate(new Vector3(0, 180, 0));
       // ClientMain.GetInst().StartCoroutine(DelayOneFrame(isshow));
        if (secBacks != null && IsSetting)
            secBacks.gameObject.SetActive(!isshow);

    }
    /// <summary>
    /// 上侧面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isTopShow = false;
    void ShowOrHideTopBoard(bool isshow)
    {
        #region  //上边栏弹出 

        #endregion
        if (rightControl != null && rightSetting != null)
        {
            rightSetting.gameObject.SetActive(isshow);
            rightControl.gameObject.SetActive(!isshow);
        }
        //if (secBacks != null)
            //secBacks.gameObject.SetActive(isshow);
        topTrans.Find("topLeft").gameObject.SetActive(!isshow);
    }
    /// <summary>
    /// 底部面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isBottomShow = false;
    void ShowOrHideBottmBoard(bool isshow)
    {
        if (bottomTrans == null || isshow == isBottomShow)
            return;
        bottomTrans.gameObject.SetActive(true);
        isBottomShow = isshow;
        bottomTrans.GetChild(0).GetComponent<BoxCollider>().enabled = isBottomShow;

        if (curSelectControllerWidget != null)
        {
            ChangeDepthCustom(isshow, curSelectControllerWidget);
            //curSelectControllerWidget = null;
        }

        grayBackgrounds.gameObject.SetActive(isshow);
        
        if (isshow && isLeftShow)
            ShowOrHideLeftboard(false);
        

        HideOrShowTrans(isshow, bottomTrans, directType.bottom, 0.7f, OverHide);

        
        if (!isshow && curSettingAction != null)  //下弹出框消失时 取消动作选中状态
        {
            curSettingAction.GetComponent<UISprite>().spriteName = "ButtonBG";
            //Debug.Log("Cancel isBottomShow is " + isBottomShow);
        }
        else if (isshow && curSettingAction != null) //动作选中
        {
            //Debug.Log("动作选中状态");
            //curSettingAction.GetComponent<UISprite>().spriteName = "Button";
            //Debug.Log("Select isBottomShow is " + isBottomShow);
        }
    }
    /// <summary>
    /// 横竖杆配置左侧面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    //bool isSliderLeftShow = false;
    void SliderShowOrHideLeftboard(bool isshow)
    {
        //Debug.Log("now isshow is " + isshow);
        if (leftTrans == null || isshow == isLeftShow)
            return;
        isLeftShow = isshow;

        //grayBackgrounds.gameObject.SetActive(isshow);
        /*if (isshow && isSliderBottomShow)  //底部如果存在则消失
            SliderShowOrHideBottmboard(false);*/
        HideOrShowTrans(isshow, leftTrans, directType.left);
        sliderToogle.transform.GetChild(0).transform.Rotate(new Vector3(0, 180, 0));
        // ClientMain.GetInst().StartCoroutine(DelayOneFrame(isshow));
    }
    /// <summary>
    /// 横竖杆配置底部面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isSliderBottomShow = false;
    void SliderShowOrHideBottmboard(bool isshow)
    {
        //Debug.Log("now isSliderBottomShow is " + isSliderBottomShow);
        //Debug.Log("now bottom isshow is "+isshow);
        if (sliderBottoms == null || isshow == isSliderBottomShow)
            return;

        sliderBottoms.gameObject.SetActive(true);
        isSliderBottomShow = isshow;
        sliderBottoms.GetChild(0).GetComponent<BoxCollider>().enabled = isSliderBottomShow;

        if (curSelectControllerWidget != null)
        {
            ChangeDepthCustom(isshow, curSelectControllerWidget);
            //curSelectControllerWidget = null;
        }

        grayBackgrounds.gameObject.SetActive(isshow);
        /*if (isshow && isLeftShow)
            SliderShowOrHideLeftboard(false);*/
        if (isshow)
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Display slider servo list!!");
        else
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Hide slider servo list!!");

        float times;
        if (isshow)
            times = 1.0f;
        else
            times = 0.6f;

        HideOrShowTrans(isshow, sliderBottoms, directType.bottom, times, OverBottomHide);
        //HideOrShowTrans(isshow, bottomTrans, directType.bottom, 0.7f, OverHide);
    }
    void DelayShowSliderServo()
    {
        //cleanBoxCollider.gameObject.SetActive(false);
        //Debug.Log("show slider servo!!");
        SliderShowOrHideBottmboard(true);

        if(isVsliderServo && (!isHsliderServo))
            UserdefControllerScene.InitServoListV(mTrans.Find("sliderBottom/Sprite/grid").transform, curVsliderServoNum, OnVsliderButtonClick);
        else if(isHsliderServo && (!isVsliderServo))
            UserdefControllerScene.InitServoListH(mTrans.Find("sliderBottom/Sprite/grid").transform, curHsliderServoNum, OnHsliderButtonClick);
    }
    IEnumerator DelayHideSliderServo(float t)
    {
        //cleanBoxCollider.gameObject.SetActive(false); 
        yield return new WaitForSeconds(t);

        for (int l = 0; l < sliderBottoms.GetChild(1).GetChild(0).childCount; l++)
        {
            GameObject.Destroy(sliderBottoms.GetChild(1).GetChild(0).GetChild(l).gameObject);
        }

        sliderBottoms.gameObject.SetActive(false);

        editTitle.enabled = true;
        editDescribe.enabled = true;
        ShowOrHideLines(true);
        topRightIcons.gameObject.SetActive(true);

        if (isVsliderServo && (!isHsliderServo))
        {
            isVsliderServo = false;

            if (curVsliderShadow != null)
                curVsliderShadow.enabled = false;

            OpenVsliderSetting(curVsliderName);
        }
        else if (isHsliderServo && (!isVsliderServo))
        {
            isHsliderServo = false;

            if (curHsliderShadow != null)
                curHsliderShadow.enabled = false;

            OpenHsliderSetting(curHsliderName);
        }
    }
    IEnumerator DirectHideSliderServoList(float t)
    {
        //cleanBoxCollider.gameObject.SetActive(false); 
        yield return new WaitForSeconds(t);

        for (int l = 0; l < sliderBottoms.GetChild(1).GetChild(0).childCount; l++)
        {
            GameObject.Destroy(sliderBottoms.GetChild(1).GetChild(0).GetChild(l).gameObject);
        }

        sliderBottoms.gameObject.SetActive(false);

        //editTitle.enabled = true;
        //editDescribe.enabled = true;
        ShowOrHideLines(true);
        topRightIcons.gameObject.SetActive(true);

        if (isVsliderServo && (!isHsliderServo))
        {
            isVsliderServo = false;

            if (curVsliderShadow != null)
                curVsliderShadow.enabled = false;

            if (curVsliderServoNum != null)
                curVsliderServoNum = null;
            //OpenVsliderSetting(curVsliderName);
        }
        else if (isHsliderServo && (!isVsliderServo))
        {
            isHsliderServo = false;

            if (curHsliderShadow != null)
                curHsliderShadow.enabled = false;

            if (curHsliderServoNum != 0)
                curHsliderServoNum = 0;
            //OpenHsliderSetting(curHsliderName);
        }
    }
    void OverHide()
    {
        if (isActionDataChange)
        {
            Transform grid = mTrans.Find("Bottom/bottomBoard/Sprite/grid").transform;
            for (int i = 0; i < grid.childCount; i++)
            {
                GameObject.Destroy(grid.GetChild(i).gameObject);
            }
            GameObject obj = Resources.Load("Prefabs/newActionItem2") as GameObject;
            GameObject none = GameObject.Instantiate(obj) as GameObject;
            none.transform.SetParent(grid);
            none.transform.localScale = Vector3.one;
            ButtonDelegate del1 = new ButtonDelegate();
            del1.onClick = OnButtonClick;
            GetTCompent.GetCompent<ButtonEvent>(none.transform).SetDelegate(del1);
            if (showActionList != null && showActionList.Count > 0)
            {
                for (int i = 0; i < showActionList.Count; i++)
                {
                    GameObject oo = GameObject.Instantiate(obj) as GameObject;
                    oo.name = "newActionItem_" + showActionList[i];
                    oo.transform.SetParent(grid);
                    oo.transform.localScale = Vector3.one;
                    oo.transform.localPosition = Vector3.zero;

                    ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(showActionList[i]);
                    UISprite actionIcon = oo.transform.GetChild(0).GetComponent<UISprite>();
                    if (act == null)
                    {
                        actionIcon.spriteName = "add";
                        oo.transform.GetChild(1).GetComponent<UILabel>().text = string.Empty;
                        actionIcon.MakePixelPerfect();
                    }
                    else
                    {
                        actionIcon.spriteName = act.IconName;
                        oo.transform.GetChild(1).GetComponent<UILabel>().text = act.Name;
                        actionIcon.width = Action_Icon_Size_Width;
                        actionIcon.height = Action_Icon_Size_Height;
                    }
                    
                    //Debug.Log("action name is " + oo.transform.GetChild(0).GetComponent<UISprite>().spriteName);

                    ButtonDelegate del = new ButtonDelegate();
                    del.onClick = OnButtonClick;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
                }

                isActionDataChange = false;
            }
            grid.GetComponent<UIGrid>().repositionNow = true;
        }
        //else
            //InitActionList();
        //Debug.Log("OverHide isBottomShow is " + isBottomShow);
        if (!isBottomShow)
            bottomTrans.gameObject.SetActive(isBottomShow);
        bottomTrans.GetChild(0).GetComponent<BoxCollider>().enabled = isBottomShow;
        if (!isBottomShow && selectActionState != null)
        {
            selectActionState.GetComponent<UISprite>().enabled = false;
            selectActionState.gameObject.SetActive(false);
        }
    }
    void OverBottomHide()
    {
        Transform grid = mTrans.Find("sliderBottom/Sprite/grid").transform;
        grid.GetComponent<UIGrid>().repositionNow = true;
    }
    // 修改控件渲染深度
    public void ChangeDepthCustom(bool isAdd, GameObject obj)
    {
        if (obj.transform.GetComponent<UIWidget>() == null)
            return;
        else
        {
            if (isAdd)
            {
                obj.transform.GetComponent<UIWidget>().depth += 10;
                for (int i=0; i < obj.transform.childCount; i++)
                {
                    obj.transform.GetChild(i).GetComponent<UIWidget>().depth += 10;
                }
                if (obj.transform.GetChild(1).childCount != 0)
                    obj.transform.GetChild(1).GetChild(0).GetComponent<UIWidget>().depth += 10;
            }
            else
            {
                obj.transform.GetComponent<UIWidget>().depth -= 10;
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    obj.transform.GetChild(i).GetComponent<UIWidget>().depth -= 10;
                }
                if (obj.transform.GetChild(1).childCount != 0)
                    obj.transform.GetChild(1).GetChild(0).GetComponent<UIWidget>().depth -= 10;
            }
        }
    }
    protected void OnVsliderButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.StartsWith("servo_") || obj.name.StartsWith("motor_"))  //舵机被点击
        {
            byte id = byte.Parse(obj.name.Substring(6));
            CtrlMotionType motionType = CtrlMotionType.servo;
            if (obj.name.StartsWith("motor_"))
            {
                motionType = CtrlMotionType.motor;
            }
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click vslider servo icon!!");

            if (curVsliderSelectServo != null)
            {
                curVsliderSelectServo.enabled = false;
            }
            for (int p = 0; p < sliderBottoms.GetChild(1).GetChild(0).childCount; p++)
            {
                if (null != curVsliderServoNum && motionType == curVsliderServoNum.motionType && id == curVsliderServoNum.servoID)
                {
                    sliderBottoms.GetChild(1).GetChild(0).GetChild(p).Find("selectState").GetComponent<UISprite>().enabled = false;
                }
            }
            curVsliderSelectServo = obj.transform.Find("selectState").GetComponent<UISprite>();
            curVsliderSelectServo.enabled = true;
            
            if (null == curVsliderServoNum || motionType != curVsliderServoNum.motionType || id != curVsliderServoNum.servoID)
            {
                isVsliderChange = true;
                if (null == curVsliderServoNum)
                {
                    curVsliderServoNum = new SliderWidgetData();
                }
                curVsliderServoNum.servoID = id;
                curVsliderServoNum.motionType = motionType;
            }

            SliderShowOrHideBottmboard(false);

            //((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curVsliderName)).servoID = (byte)curVsliderServoNum;

            ClientMain.GetInst().StartCoroutine(DelayHideSliderServo(0.6f));
            //GameObject.Find("HsliderSetting/sliderWiget").transform.GetChild(1).GetComponent<UILabel>().text = "Servo " + hsliderData.servoID;
        }
    }
    protected void OnHsliderButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.StartsWith("servo_"))  //舵机被点击
        {
            if (curHsliderSelectServo != null)
            {
                curHsliderSelectServo.enabled = false;
            }
            for (int p = 0; p < sliderBottoms.GetChild(1).GetChild(0).childCount; p++)
            {
                if (sliderBottoms.GetChild(1).GetChild(0).GetChild(p).GetComponentInChildren<UILabel>().text == curHsliderServoNum.ToString())
                {
                    sliderBottoms.GetChild(1).GetChild(0).GetChild(p).Find("selectState").GetComponent<UISprite>().enabled = false;
                }
            }
            curHsliderSelectServo = obj.transform.Find("selectState").GetComponent<UISprite>();
            curHsliderSelectServo.enabled = true;
            UILabel text = obj.GetComponentInChildren<UILabel>();

            byte id = byte.Parse(text.text.Substring(PublicFunction.ID_Format.Length));
            if (id != curHsliderServoNum)
            {
                isHsliderChange = true;
                curHsliderServoNum = id;
            }

            SliderShowOrHideBottmboard(false);

            //((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curHsliderName)).servoID = (byte)curHsliderServoNum;

            ClientMain.GetInst().StartCoroutine(DelayHideSliderServo(0.6f));
            //GameObject.Find("HsliderSetting/sliderWiget").transform.GetChild(1).GetComponent<UILabel>().text = "Servo " + hsliderData.servoID;
        }
    }
    /// <summary>
    /// 面板伸缩
    /// </summary>
    /// <param name="isShow"></param>
    /// <param name="trans"></param>
    /// <param name="type"></param>
    /// <param name="time"></param>
    /// <param name="call"></param>
    public static void HideOrShowTrans(bool isShow, Transform trans, directType type, float time = 0.5f, EventDelegate.Callback call = null)
    {
        TweenPosition tp = null;
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.zero;

        //Debug.Log("Trans isshow is " + isShow);
        if (trans != null)
        {
            Transform nullTran = null;
            UIWidget temWidget = trans.GetComponentInChildren<UIWidget>();
            int hh = 0;
            int ww = 0;
            if (temWidget != null)
            {
                temWidget.SetAnchor(nullTran);
                hh = temWidget.height;
                ww = temWidget.width;
            }

            hh -= 40;
            ww -= 40;
            MyAnimtionCurve cur1 = new MyAnimtionCurve(MyAnimtionCurve.animationCurveType.position);
            if (trans.GetComponent<TweenPosition>() == null)
            {
                trans.gameObject.AddComponent<TweenPosition>();
                trans.GetComponent<TweenPosition>().from = trans.localPosition;
                if (type == directType.bottom)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x, trans.localPosition.y + hh, trans.localPosition.z);
                else if (type == directType.top)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x, trans.localPosition.y - hh, trans.localPosition.z);
                else if (type == directType.left)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x + ww, trans.localPosition.y, trans.localPosition.z);
                else if (type == directType.right)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x - ww, trans.localPosition.y, trans.localPosition.z);
                trans.GetComponent<TweenPosition>().animationCurve = cur1.animCurve;
            }
            tp = trans.GetComponent<TweenPosition>();
            tp.duration = time;

            if (isShow)
            {
                tp.PlayForward();
            }
            else
            {
                tp.PlayReverse();
            }
            if(tp != null && call != null)
                tp.AddOnFinished(call);
        }
    }
    /// <summary>
    /// 方向枚举
    /// </summary>
    public enum directType
    {
        left,
        right,
        top,
        bottom,
    }
    /// <summary>
    /// 操作枚举
    /// </summary>
    public enum dragType
    {
        dragStart,
        dragRelease,
    }
    /// <summary>
    /// 动作设置入口
    /// </summary>
    private GameObject curSettingAction;
    void OnActionSetting(GameObject obj)
    {
        if (!IsSetting)
        {
            //Debug.Log("IsSetting iees " + IsSetting);
        }
        else
        {
            //Debug.Log("Now IsSetting is " + IsSetting);
            curSettingAction = obj;
            ShowOrHideBottmBoard(true);
        }
    }
    /// <summary>
    /// 设置摇杆
    /// </summary>
    /// <param name="id"></param>
    void OpenJoystickSettingUI(string id)
    {
        UserdefControllerScene.Ins.OpenJoystickSettingUI(id);
     //   OnHide();
    }
    void OpenVsliderSetting(string id)
    {
        UserdefControllerScene.Ins.OpenVsliderSettingUI(id);
    }
    void OpenHsliderSetting(string id)
    {
        UserdefControllerScene.Ins.OpenHsliderSettingUI(id);
    }
    /// <summary>
    /// 初始化动作列表
    /// </summary>
    void  InitActionList()
    {
        Transform grid = bottomTrans.Find("Sprite/grid");
        if (grid != null)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Init action list in controller setting scene!!");

            List<string> actionsidd = RobotManager.GetInst().GetCurrentRobot().GetActionsIdList();
            
            //Debug.Log("actions count is " + actions.Count+"Time:"+Time.fixedDeltaTime);
            
            for (int i = 0; i < grid.childCount; i++)
            {
                GameObject.Destroy(grid.GetChild(i).gameObject);
            }

            GameObject obj = Resources.Load("Prefabs/newActionItem2") as GameObject;
            GameObject none = GameObject.Instantiate(obj) as GameObject;
            none.transform.SetParent(grid);
            none.transform.localScale = Vector3.one;
            ButtonDelegate del1 = new ButtonDelegate();
            del1.onClick = OnButtonClick;
            GetTCompent.GetCompent<ButtonEvent>(none.transform).SetDelegate(del1);

            for (int i = 0; i < actionsidd.Count; i++)
            {
                if (!ControllerManager.GetInst().IsActionExist(actionsidd[i]))
                {
                    //Debug.Log("IsActionExist is false");
                    if (!showActionList.Contains(actionsidd[i])) //不存在
                    {
                        //Debug.Log("actions is not exist!!");
                        showActionList.Add(actionsidd[i]);
                    }
                    GameObject oo = GameObject.Instantiate(obj) as GameObject;
                    oo.name = "newActionItem_" + actionsidd[i];
                    oo.transform.SetParent(grid);
                    oo.transform.localScale = Vector3.one;
                    //oo.tag = actionsidd[i];
                    oo.transform.GetChild(1).GetComponent<UILabel>().text = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionsidd[i]).Name;          //动作名称
                    
                    UISprite sp = oo.transform.GetChild(0).GetComponent<UISprite>();
                    if (null != sp)
                    {
                        ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionsidd[i]);
                        if (null == act)
                        {
                            sp.spriteName = "add";
                            sp.MakePixelPerfect();
                        }
                        else
                        {
                            sp.spriteName = act.IconName;
                            sp.width = Action_Icon_Size_Width;
                            sp.height = Action_Icon_Size_Height;
                        }
                        

                    }
                    ButtonDelegate del = new ButtonDelegate();
                    del.onClick = OnButtonClick;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
                }
            }
            grid.GetComponent<UIGrid>().repositionNow = true;
            UIManager.SetButtonEventDelegate(grid, mBtnDelegate);
        }
    }
    #endregion

    #region other
    private Transform rightSetting;
    private Transform rightControl;
    private Transform leftItems;
    private Transform secBacks;
    private dragType curDragStatus;

    protected override void AddEvent()
    {
        PlatformMgr.Instance.MobPageStart(MobClickEventID.P9);
        base.AddEvent();
        showActionList = new List<string>();
        //添加
        leftTrans = mTrans.Find("Left/leftBoard").transform;
        bottomTrans = mTrans.Find("Bottom/bottomBoard").transform;
        topTrans = mTrans.Find("Up").transform;
        topRightIcons = mTrans.Find("Up/topright_setting").transform;
        widgetsParent = mTrans.Find("Center/widgetsParent").transform;
        gridPanel = mTrans.Find("Center/gridPanel").transform;
        setBgSprite = GameHelper.FindChildComponent<UISprite>(mTrans, "Center/bg");
        sliderToogle = mTrans.Find("Left/leftBoard/ContainerLeft/Backdrop/sliderToogleBtn").gameObject;
        editTitle = mTrans.Find("Up/edittitle").GetComponent<UILabel>();
        editDescribe = mTrans.Find("Up/editdescribe").GetComponent<UILabel>();
        centerTip = mTrans.Find("StartIcon").transform;
        centerStartTip = mTrans.Find("StartTip").transform;
        powerDownBtn = mTrans.Find("Up/topright_control/powerDownBtn");
        connectBtn = mTrans.Find("Up/topright_control/btnConnect");
        settingBtn = mTrans.Find("Up/topright_control/settingBtn");
        powerTrans = mTrans.Find("Up/topright_control/power");
        if (null != powerTrans)
        {
            mPowerShow = new PowerShow(powerTrans);
        }
        grayBackgrounds = mTrans.Find("GrayBg").transform;
        tireBackgrounds = mTrans.Find("TiresBg").transform;
        //cleanBoxCollider = GameObject.Find("userdefineControllerUI/cleanBox").transform;
        if ((float)Screen.width / Screen.height > 2.16f)
        {
            sliderToogle.GetComponent<UISprite>().SetDimensions(120, 360);
        }
        #region 位置布局 
        Transform left = leftTrans.GetChild(0);
        Vector3 pos = UIManager.GetWinPos(left, UIWidget.Pivot.Left, -left.GetComponentInChildren<UIWidget>().width-34);
        left.localPosition = pos;
        Transform toplr = topTrans.Find("topLeft");
        pos = UIManager.GetWinPos(toplr, UIWidget.Pivot.TopLeft,PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
        toplr.localPosition = pos;
        rightSetting = topTrans.Find("topright_setting");
        pos = UIManager.GetWinPos(rightSetting.GetChild(0), UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
        rightSetting.localPosition = pos;
        rightControl = topTrans.Find("topright_control");
        pos = UIManager.GetWinPos(rightControl, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
        rightControl.localPosition = pos;
        if (!PlatformMgr.Instance.EditFlag)
        {
            if (null != settingBtn)
            {
                settingBtn.gameObject.SetActive(false);
            }
        }
        Transform btnConnect = rightControl.Find("btnConnect");
        if (null != btnConnect)
        {
            mConnectIcon = GameHelper.FindChildComponent<UISprite>(btnConnect, "Background");
            mConnectBtn = btnConnect.GetComponent<UIButton>();
        }
        pos = UIManager.GetWinPos(editTitle.transform, UIWidget.Pivot.Top, 0, PublicFunction.Title_Margin);
        editTitle.transform.localPosition = pos;
        //Debug.Log(pos.x+" "+pos.y+" "+pos.z);
        editTitle.text = LauguageTool.GetIns().GetText("设置遥控页面主标题");
        pos = UIManager.GetWinPos(editDescribe.transform, UIWidget.Pivot.Top, 0, PublicFunction.Title_Margin + 40);
        editDescribe.transform.localPosition = pos;
        editDescribe.text = LauguageTool.GetIns().GetText("设置遥控页面副标题");
        Transform bottom = bottomTrans.parent;
        Transform deleta = bottom.GetChild(1);
        pos = UIManager.GetWinPos(bottomTrans, UIWidget.Pivot.Bottom, 0, -bottomTrans.GetComponentInChildren<UIWidget>().height-11.0f);
        bottomTrans.localPosition = pos;
        
        pos = UIManager.GetWinPos(deleta, UIWidget.Pivot.BottomRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
        deleta.localPosition = pos;
        deleta.gameObject.SetActive(false);  //隐藏

        //centerTip.localPosition = UIManager.GetWinPos(centerTip, UIWidget.Pivot.Center, 0, 70);
        //centerStartTip.localPosition = UIManager.GetWinPos(centerStartTip, UIWidget.Pivot.Center, 0, -70);
        centerStartTip.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("初始化遥控提示");
       // topTrans.GetComponentInChildren<UISprite>().width = PublicFunction.GetWidth();
        bottomTrans.GetComponentInChildren<UISprite>().width = PublicFunction.GetWidth();
        bottomTrans.Find("bg_down/shader").GetComponent<UISprite>().width = PublicFunction.GetWidth();
        leftTrans.GetComponentInChildren<UISprite>().height = PublicFunction.GetHeight() + 40;
        leftTrans.Find("ContainerLeft/Backdrop/BG").GetComponent<UISprite>().height = PublicFunction.GetHeight() + 40;
        gridPanel.GetComponent<UIWidget>().width = PublicFunction.GetWidth();
        gridPanel.GetComponent<UIWidget>().height = PublicFunction.GetHeight();
        if (null != setBgSprite)
        {
            setBgSprite.width = PublicFunction.GetExtendWidth();
            setBgSprite.height = PublicFunction.GetExtendHeight();
        }
        placedArea = gridPanel.GetChild(0).GetComponent<UISprite>();
        placedArea.enabled = false;
        IsSetting = false;

        sliderBottoms = mTrans.Find("sliderBottom").transform;
        Vector3 posB;
        posB = UIManager.GetWinPos(sliderBottoms, UIWidget.Pivot.Bottom, 0, -sliderBottoms.GetComponentInChildren<UIWidget>().height - 11.0f);
        sliderBottoms.localPosition = posB;
        sliderBottoms.GetComponentInChildren<UISprite>().width = PublicFunction.GetWidth();
        sliderBottoms.Find("sliderbg_down/shader").GetComponent<UISprite>().width = PublicFunction.GetWidth();

        centerTip.gameObject.SetActive(false);
        centerStartTip.gameObject.SetActive(false);
        grayBackgrounds.gameObject.SetActive(false);
        tireBackgrounds.gameObject.SetActive(false);
        connectBtn.gameObject.SetActive(false);
        powerDownBtn.gameObject.SetActive(false);
        tireBackgrounds.GetComponent<ButtonEvent>().enabled = false;
        //cleanBoxCollider.gameObject.SetActive(false);

        curDragStatus = dragType.dragRelease;

        secBacks = mTrans.Find("Up/secBack").transform;
        Vector3 posSS;
        posSS = UIManager.GetWinPos(secBacks, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
        secBacks.localPosition = posSS;

        secBacks.gameObject.SetActive(false);
        //topRightIcons.gameObject.SetActive(false);

        curSelectControllerWidget = null;

        if (isFirstEdit)
        {
            isFirstEdit = false;
            ShowOrHideLines(true);
            editTitle.enabled = true;
            editDescribe.enabled = true;
        }
        else
        {
            ShowOrHideLines(false);
            editTitle.enabled = false;
            editDescribe.enabled = false;
        }

        // 计算控件位置和缩放比例
        float curScreenW = ControllerManager.GetInst().GetCurControllerSceneWidth() * 1.0f;
        float curScreenH = ControllerManager.GetInst().GetCurControllerSceneHeight() * 1.0f;
        if (curScreenW == 0)
            curScreenW = 1334.0f;
        if (curScreenH == 0)
            curScreenH = 750.0f;

        positionXRatio = PublicFunction.GetWidth() * 1.0f / curScreenW;
        positionYRatio = PublicFunction.GetHeight() * 1.0f / curScreenH;

        scaleRatio = Mathf.Min(positionXRatio, positionYRatio);

        leftItems = mTrans.Find("Left/leftBoard/ContainerLeft/EditScrollview/Grid").transform;

        if (leftItems != null)
        {
            //Debug.Log("leftItem is not null!!");
            leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(2).GetChild(4).GetComponent<UILabel>().enabled = false;
            leftItems.GetChild(2).GetChild(5).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(2).GetChild(6).GetComponent<UISprite>().enabled = false;
            //leftItems.GetChild(3).GetChild(1).GetChild(1).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(3).GetChild(3).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(3).GetChild(4).GetComponent<UILabel>().enabled = false;

            // 设置长按反应时间
            leftItems.GetChild(0).GetComponent<UIDragDropItem>().pressAndHoldDelay = 0.1f;
            leftItems.GetChild(1).GetComponent<UIDragDropItem>().pressAndHoldDelay = 0.1f;
            leftItems.GetChild(2).GetComponent<UIDragDropItem>().pressAndHoldDelay = 0.1f;
            leftItems.GetChild(3).GetComponent<UIDragDropItem>().pressAndHoldDelay = 0.1f;
        }

        isFirstController = true;

        isVsliderServo = false;
        isHsliderServo = false;

        sliderBottoms.gameObject.SetActive(false);

        InitActionList();
        bottomTrans.gameObject.SetActive(false);

        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Add event in controller setting scene!!");

        //Debug.Log("注册充电保护中！！");
        
        EventMgr.Inst.Regist(EventID.Read_Power_Msg_Ack, GetPowerState);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        SetConnectState();
        if (PlatformMgr.Instance.GetBluetoothState()) //蓝牙连接时 发送电量信息
        {
            GetPowerState(null);
        }
        
        #endregion
    }

    public override void Release()
    {
        UICamera camGO2 = GameObject.Find("UIRoot(2D)(2)/UICamera").transform.GetComponent<UICamera>();
        if (camGO2 != null)
            camGO2.allowMultiTouch = false;

        base.Release();
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.UnRegist(EventID.Read_Power_Msg_Ack, GetPowerState);
        PlatformMgr.Instance.MobPageEnd(MobClickEventID.P9);
    }


    void GetPowerState(EventArg arg)
    {
        try
        {
            if (PlatformMgr.Instance.IsChargeProtected)  //充电保护 11 => 10
            {
                PublicPrompt.ShowChargePrompt(null);
            }
            if (null != mPowerShow)
            {
                mPowerShow.SetPowerState();
            }
            if (showPowerFlag && PlatformMgr.Instance.GetBluetoothState())
            {
                mPowerShow.ShowPower();
            }
            else
            {
                mPowerShow.HidePower();
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
    }

    void OnBlueConnectResult(EventArg arg)
    {
        SetConnectState();
    }

    void GoHome()
    {
        UserdefControllerScene.GoBack();
        //Game.Scene.SceneMgr.EnterScene(Game.Scene.SceneType.MainWindow);
    }
    GameObject pressActionObj;
    protected override void OnButtonPress(GameObject obj, bool press)
    {
        if (!IsSetting)
        {
            base.OnButtonPress(obj, press);
            if (!obj.tag.Equals("Untagged") && PlatformMgr.Instance.IsChargeProtected)
            {
                PublicPrompt.ShowChargePrompt(null);
                return;
            }
            try
            {
                if (press)
                    ControlLogic.touchCount++;
                else
                    ControlLogic.touchCount--;
                if (obj.tag.Contains("widget_action")) //动作之间互斥 ，不可同时按下两个动作
                {
                    string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionNm;
                    string actionId = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionId;

                    if (!PlatformMgr.Instance.GetBluetoothState() && press)  //未连接的情况 模型运动 
                    {
                        SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot());
                        //SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("connectRobotTip"), 1, CommonTipsColor.red);
                    }     
                    else
                    {
                        if (string.IsNullOrEmpty(actionName) && press)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("未完成配置"), 1, CommonTipsColor.red);
                        }
                        else
                        {
                            if (press)
                            {
                                if (!ControlLogic.actionTouch) //动作被按下时 不响应其它动作
                                {
                                    pressActionObj = obj;
                                    ControlLogic.actionTouch = true;
                                    ControlLogic.GetIns().PlayAction(actionId);
                                    //Debug.Log("now action id is " + actionId);
                                }
                            }
                            else
                            {
                                if (pressActionObj == obj)
                                {
                                    pressActionObj = null;
                                    ControlLogic.actionTouch = false;
                                    ControlLogic.GetIns().CancelRePlay();
                                }
                            }
                        }                        
                    }
                }
                if (obj.tag.Contains("widget_vslider") && (!IsSetting)) //使用模式下点击竖杆提示连接机器人
                {
                    if (press)
                        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "operate vslider controller!!");

                    //string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionNm;
                    if(press)
                    {
                        if ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.transform.name) != null)
                            curVsliderServoNum = new SliderWidgetData(((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.transform.name)));

                        if (!PlatformMgr.Instance.GetBluetoothState())  //未连接的情况 模型运动 
                        {
                            SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot());
                            //SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("connectRobotTip"), 1, CommonTipsColor.red);
                        }
                        else if (!((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).isOK)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("未完成配置"), 1, CommonTipsColor.red);
                        }
                        else if (curVsliderServoNum != null && curVsliderServoNum.motionType == CtrlMotionType.servo && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData(curVsliderServoNum.servoID).modelType == ServoModel.Servo_Model_Angle)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机属性已更改，不可使用"), 1, CommonTipsColor.red);
                        }
                    }                       
                }
                if (obj.tag.Contains("widget_hslider") && (!IsSetting)) //使用模式下点击横杆提示连接机器人
                {
                    if (press)
                        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "operate hslider controller!!");

                    //string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionNm;
                    if (press)
                    {
                        if ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.transform.name) != null)
                            curHsliderServoNum = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.transform.name)).servoID;

                        if (!PlatformMgr.Instance.GetBluetoothState())  //未连接的情况 模型运动 
                        {
                            SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot());
                            //SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("connectRobotTip"), 1, CommonTipsColor.red);
                        }
                        else if (!((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).isOK)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("未完成配置"), 1, CommonTipsColor.red);
                        }
                        else if (curHsliderServoNum != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)curHsliderServoNum).modelType == ServoModel.Servo_Model_Turn)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机属性已更改，不可使用"), 1, CommonTipsColor.red);
                        }
                    }                    
                }
                if (obj.tag.Contains("widget_joystick") && (!IsSetting)) //使用模式下点击摇杆提示连接机器人
                {
                    if (press)
                        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "operate joystick controller!!");

                    //string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionNm;
                    if (press)
                    {
                        int wheelServoID01 = 0;
                        int wheelServoID02 = 0;
                        int wheelServoID03 = 0;
                        int wheelServoID04 = 0;
                        JockstickData jockstickData = (JockstickData)ControllerManager.GetInst().GetWidgetdataByID(obj.transform.name);
                        if (jockstickData != null && jockstickData.type == JockstickData.JockType.twoServo)
                        {
                            wheelServoID01 = jockstickData.leftUpID;
                            wheelServoID02 = jockstickData.rightUpID;
                            wheelServoID03 = 0;
                            wheelServoID04 = 0;
                        }
                        else if (jockstickData != null && jockstickData.type == JockstickData.JockType.fourServo)
                        {
                            wheelServoID01 = jockstickData.leftUpID;
                            wheelServoID02 = jockstickData.rightUpID;
                            wheelServoID03 = jockstickData.leftBottomID;
                            wheelServoID04 = jockstickData.rightBottomID;
                        }
                        
                        
                        if (!PlatformMgr.Instance.GetBluetoothState())  //未连接的情况 模型运动 
                        {
                            SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot());
                            //SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("connectRobotTip"), 1, CommonTipsColor.red);
                        }
                        else if (null != jockstickData && !jockstickData.isOK)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("未完成配置"), 1, CommonTipsColor.red);
                        }
                        else if (null != jockstickData && jockstickData.motionType == CtrlMotionType.servo && ((wheelServoID01 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID01).modelType == ServoModel.Servo_Model_Angle) ||
                                 (wheelServoID02 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID02).modelType == ServoModel.Servo_Model_Angle) ||
                                 (wheelServoID03 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID03).modelType == ServoModel.Servo_Model_Angle) ||
                                 (wheelServoID04 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID04).modelType == ServoModel.Servo_Model_Angle)))
                        {
                            jockstickData.leftBottomID = 0;
                            jockstickData.leftUpID = 0;
                            jockstickData.rightBottomID = 0;
                            jockstickData.rightUpID = 0;
                            jockstickData.type = JockstickData.JockType.none;
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("未完成配置"), 1, CommonTipsColor.red);
                        }
                    }
                }

                BoxCollider setBox = rightControl.GetChild(0).GetComponent<BoxCollider>();
                BoxCollider backBox = mTrans.Find("Up/topLeft/back").GetComponent<BoxCollider>();
                
                if (ControlLogic.touchCount == 1 || ControlLogic.touchCount == 0)  //遥控操作时 返回和设置按钮不响应
                {
                    //setBtn0.GetComponent<UIButton>().enabled = true;
                    setBox.enabled = true;
                    backBox.enabled = true;
                }
                else
                {
                    //setBtn0.GetComponent<UIButton>().enabled = false;
                    setBox.enabled = false;
                    backBox.enabled = false;
                }
            }
            catch (System.Exception ex)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
            }
        }
    }
    float lastClickTime = 0f;
    float clickDuration = 1f;
    //配置控件数据事件
    protected override void OnButtonClick(GameObject obj)
    {
        string name = obj.name;
        base.OnButtonClick(obj);
        try
        {
            
            if(name.Equals("back")) //返回主页
            {
                editTitle.enabled = false;
                ShowOrHideLines(false);
                ActionLogic.GetIns().DoStopAction(obj);
                editDescribe.enabled = false;
                
                OnBackbtnClicked();
            }
            else if (name.Equals("btnConnect"))
            {
                PublicPrompt.ShowClickBlueBtnMsg(ConnectionTriggerPage.Controls_Page);
            }
            else if(name.Equals("secBack")) //设置返回
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Exit controller setting UI!!");

                if (selectActionState != null)
                {
                    selectActionState.GetComponent<UISprite>().enabled = false;
                    selectActionState.gameObject.SetActive(false);
                }
                editTitle.enabled = false;
                ShowOrHideLines(false);
                editDescribe.enabled = false;
                //centerTip.gameObject.SetActive(false);
                Transform topL2 = mTrans.Find("Center/gridPanel").transform;
                if (!isTotalDataChange && topL2 != null && topL2.childCount <= 1)
                {
                    //Debug.Log("Delete Controller is " + RobotManager.GetInst().GetCurrentRobot().ID);
                    ControllerManager.DeletaController(RobotManager.GetInst().GetCurrentRobot().ID);
                    centerTip.gameObject.SetActive(true);
                    centerStartTip.gameObject.SetActive(true);
                    connectBtn.gameObject.SetActive(false);
                    powerDownBtn.gameObject.SetActive(false);
                    showPowerFlag = false;
                    if (null != mPowerShow)
                    {
                        mPowerShow.HidePower();
                    }
                }
                else
                {
                    centerTip.gameObject.SetActive(false);
                    centerStartTip.gameObject.SetActive(false);
                    connectBtn.gameObject.SetActive(true);
                    powerDownBtn.gameObject.SetActive(true);
                    showPowerFlag = true;
                    if (PlatformMgr.Instance.GetBluetoothState() && null != mPowerShow)
                    {
                        mPowerShow.ShowPower();
                    }
                }
                UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("保存遥控提示"), OnSecondbackClicked, isTotalDataChange);    
            }
            else if (name.Equals("stopBtn")) //动作停止
            {
                OnStopbtnClicked(obj);
                //Debug.Log("stop");
            }
            else if (name.Equals("powerDownBtn"))
            {//掉电
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != robot)
                    {
                        NetWaitMsg.ShowWait(1);
                        robot.RobotPowerDown();
                    }
                }
                ActionLogic.GetIns().DoStopAction(null);
            }
            else if (name.Equals("settingBtn")) //设置
            {
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P9_1);
                editTitle.enabled = true;
                editDescribe.enabled = true;
                ShowOrHideLines(true);
                centerTip.gameObject.SetActive(false);
                centerStartTip.gameObject.SetActive(false);
                OnSettingbtnClicked();
            }
            else if (name.Equals("confirm")) //确认修改
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Save modified controller data directly!!");

                if (selectActionState != null)
                {
                    selectActionState.GetComponent<UISprite>().enabled = false;
                    selectActionState.gameObject.SetActive(false);
                }
                editTitle.enabled = false;
                editDescribe.enabled = false;
                ShowOrHideLines(false);
                secBacks.gameObject.SetActive(false);
                OnConfirmClicked();              
            } else
            {
                if (lastClickTime == 0f)
                {
                    lastClickTime = Time.time - 10;
                }
                if (Time.time - lastClickTime < clickDuration)
                {
                    return;
                }
                lastClickTime = Time.time;
                if (name.Equals("sliderToogleBtn"))  //伸缩按钮
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Click left items toogle button!!");

                    OnExplorebtnClicked();
                }
                else if (name.Equals("bg_down"))
                {
                    ShowOrHideBottmBoard(false);
                }
                else if (name.Equals("sliderbg_down"))
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Hide slider servo list!!");

                    //isVsliderServo = false;
                    //isHsliderServo = false;

                    if (isVsliderChange)
                        isVsliderChange = false;
                    if (isHsliderChange)
                        isHsliderChange = false;

                    SliderShowOrHideBottmboard(false);

                    ClientMain.GetInst().StartCoroutine(DirectHideSliderServoList(0.5f));
                }
                else if (obj.name.Contains("newActionItem"))
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Click action icon when selected any action controller!!");

                    if (curSettingAction != null)
                    {
                        if (obj.transform.GetChild(0).GetComponent<UISprite>().spriteName == "add")
                            curSettingAction.transform.GetChild(0).GetComponent<UISprite>().spriteName = "add";
                        else
                            curSettingAction.transform.GetChild(0).GetComponent<UISprite>().spriteName = obj.transform.GetChild(0).GetComponent<UISprite>().spriteName;
                        curSettingAction.transform.GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
                        // controllerdata 数据修改
                        string name1 = obj.GetComponentInChildren<UILabel>().text;

                        if (name1 == "")
                        {
                            name1 = "-1";
                        }

                        string id0 = obj.name.Remove(0, 14);

                        string name2 = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionNm;

                        string id1;
                        if (name1 == "" || name1 == "-1")
                            id1 = "-1";
                        else
                            id1 = id0;

                        string id2 = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionId;

                        //((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionId = RobotManager.GetInst().GetCurrentRobot().GetActionsForName(name2).Id;
                        if (id2 != id1/* || name2 != name1*/)
                        {
                            ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionNm = name1;
                            ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionId = id1;
                            isActionDataChange = true;
                            isTotalDataChange = true;
                            if (id1 == "") //选择了一个空的 
                            {
                                if (!showActionList.Contains(id2))
                                    showActionList.Add(id2);
                            }
                            else
                            {
                                if (showActionList.Contains(id1))
                                    showActionList.Remove(id1);
                                if (id2 != "" && id2 != "-1" && !showActionList.Contains(id2))
                                    showActionList.Add(id2);
                            }
                        }
                        else  //同时为空动作
                        {
                            return;
                        }
                    }
                    ShowOrHideBottmBoard(false);
                }
                else if (obj.tag.Contains("widget_action")) //动作被点击
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Click action controller widget!!");

                    if (selectActionState != null)
                    {
                        selectActionState.GetComponent<UISprite>().enabled = false;
                        selectActionState.gameObject.SetActive(false);
                    }
                    //Debug.Log("IsSetting is "+IsSetting);
                    if (IsSetting && obj.transform.parent.name == "gridPanel")
                    {
                        editTitle.enabled = false;
                        editDescribe.enabled = false;

                        curSelectControllerWidget = obj;

                        selectActionState = obj.transform.GetChild(4);
                        selectActionState.gameObject.SetActive(true);
                        selectActionState.GetComponent<UISprite>().enabled = true;
                        //ShowOrHideLines(false);
                        OnActionSetting(obj);
                    }
                }
                else if (obj.tag.Contains("widget_vslider")) //竖杆被点击
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Click vslider controller widget!!");

                    if (selectActionState != null)
                    {
                        selectActionState.GetComponent<UISprite>().enabled = false;
                        selectActionState.gameObject.SetActive(false);
                    }
                    if (IsSetting && obj.transform.parent.name == "gridPanel")
                    {
                        editTitle.enabled = true;
                        editDescribe.enabled = true;
                        ShowOrHideLines(true);

                        do
                        {
                            if (PlatformMgr.Instance.isAppleDemo)
                            {
                                if (RecordContactInfo.Instance.openType == "default")
                                {
                                    editTitle.enabled = false;
                                    editDescribe.enabled = false;
                                    ShowOrHideLines(true);
                                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("官方模型不可更改配置"), 1, CommonTipsColor.red);
                                    break;
                                }
                            }
                            int count = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count;
                            if (null != RobotManager.GetInst().GetCurrentRobot().MotorsData)
                            {
                                count += RobotManager.GetInst().GetCurrentRobot().MotorsData.Count;
                            }
                            if (count < 1)
                            {
                                editTitle.enabled = false;
                                editDescribe.enabled = false;
                                isSliderBottomShow = false;
                                curVsliderServoNum = null;

                                if (RecordContactInfo.Instance.openType == "default")
                                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机和马达数量不足，无法使用垂直滑动条"), 1, CommonTipsColor.red);
                                else if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList().Count > 0)
                                {
                                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置轮模式提示"), ModifyServoTypeSetting, true);
                                }
                                else
                                {
                                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机和马达数量不足，无法使用垂直滑动条"), 1, CommonTipsColor.red);
                                }

                            }
                            else
                            {
                                //bool isVsliderLeft = isLeftShow;

                                isVsliderServo = true;

                                //cleanBoxCollider.gameObject.SetActive(true);
                                curVsliderName = obj.name;
                                SliderWidgetData tmpData = (SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.transform.name);
                                if (null != tmpData)
                                {
                                    curVsliderServoNum = new SliderWidgetData(tmpData);
                                }
                                else
                                {
                                    curVsliderServoNum = null;
                                }

                                curVsliderShadow = obj.transform.GetChild(3).GetComponent<UISprite>();
                                curVsliderShadow.enabled = true;


                                if (curVsliderServoNum == null || curVsliderServoNum.servoID == 0)
                                {
                                    editTitle.enabled = false;
                                    editDescribe.enabled = false;

                                    curSelectControllerWidget = obj;

                                    sliderBottoms.gameObject.SetActive(true);

                                    SliderShowOrHideLeftboard(false);

                                    DelayShowSliderServo();
                                }
                                else if (curVsliderServoNum.motionType == CtrlMotionType.servo && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData(curVsliderServoNum.servoID).modelType == ServoModel.Servo_Model_Angle)
                                {
                                    editTitle.enabled = false;
                                    editDescribe.enabled = false;

                                    curSelectControllerWidget = obj;

                                    sliderBottoms.gameObject.SetActive(true);

                                    SliderShowOrHideLeftboard(false);

                                    DelayShowSliderServo();
                                }
                                else
                                {
                                    editTitle.enabled = true;
                                    editDescribe.enabled = true;
                                    ShowOrHideLines(true);
                                    topRightIcons.gameObject.SetActive(true);

                                    sliderBottoms.gameObject.SetActive(false);

                                    isVsliderServo = false;

                                    if (curVsliderShadow != null)
                                        curVsliderShadow.enabled = false;

                                    OpenVsliderSetting(curVsliderName);
                                }
                            }
                        } while (false);

                    }
                }
                else if (obj.tag.Contains("widget_hslider")) //横杆被点击
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Click hslider controller widget!!");

                    if (selectActionState != null)
                    {
                        selectActionState.GetComponent<UISprite>().enabled = false;
                        selectActionState.gameObject.SetActive(false);
                    }
                    if (IsSetting && obj.transform.parent.name == "gridPanel")
                    {
                        editTitle.enabled = true;
                        editDescribe.enabled = true;
                        ShowOrHideLines(true);

                        if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList().Count < 1)
                        {
                            //editTitle.enabled = false;
                            //editDescribe.enabled = false;                     
                            isSliderBottomShow = false;

                            if (RecordContactInfo.Instance.openType == "default")
                                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("角模式舵机数量不足"), 1, CommonTipsColor.red);
                            else if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count < 1)
                            {
                                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机数量不足"), 1, CommonTipsColor.red);
                            }
                            else
                            {
                                UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置角度模式提示"), ModifyServoTypeSetting, true);
                            }
                            //obj.transform.GetChild(3).gameObject.SetActive(false);
                        }
                        else
                        {
                            isHsliderServo = true;

                            curHsliderName = obj.name;
                            curHsliderServoNum = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.transform.name)).servoID;

                            curHsliderShadow = obj.transform.GetChild(3).GetComponent<UISprite>();
                            curHsliderShadow.enabled = true;

                            if (curHsliderServoNum == 0)
                            {
                                editTitle.enabled = false;
                                editDescribe.enabled = false;

                                curSelectControllerWidget = obj;

                                sliderBottoms.gameObject.SetActive(true);

                                SliderShowOrHideLeftboard(false);

                                DelayShowSliderServo();
                            }
                            else if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)curHsliderServoNum).modelType == ServoModel.Servo_Model_Turn)
                            {
                                //editTitle.enabled = false;
                                //editDescribe.enabled = false;

                                curSelectControllerWidget = obj;

                                sliderBottoms.gameObject.SetActive(true);

                                SliderShowOrHideLeftboard(false);

                                DelayShowSliderServo();
                            }
                            else
                            {
                                editTitle.enabled = true;
                                editDescribe.enabled = true;
                                ShowOrHideLines(true);
                                topRightIcons.gameObject.SetActive(true);

                                sliderBottoms.gameObject.SetActive(false);

                                isHsliderServo = false;

                                if (curHsliderShadow != null)
                                    curHsliderShadow.enabled = false;

                                OpenHsliderSetting(curHsliderName);
                            }
                        }
                    }
                }
                else if (obj.tag.Contains("widget_joystick"))   //摇杆被点击
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Click joystick controller widget!!");

                    if (selectActionState != null)
                    {
                        selectActionState.GetComponent<UISprite>().enabled = false;
                        selectActionState.gameObject.SetActive(false);
                    }
                    if (IsSetting && obj.transform.parent.name == "gridPanel")
                    {
                        editTitle.enabled = true;
                        editDescribe.enabled = true;
                        ShowOrHideLines(true);
                        bool editJoystick = false;
#if UNITY_EDITOR
                        editJoystick = true;
#endif
                        if (RecordContactInfo.Instance.openType == "default" && !editJoystick)
                        {
                            editTitle.enabled = false;
                            editDescribe.enabled = false;
                            ShowOrHideLines(true);
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("官方模型不可更改配置"), 1, CommonTipsColor.red);
                        }
                        else
                        {
                            JockstickData jockstickData = (JockstickData)ControllerManager.GetInst().GetWidgetdataByID(obj.transform.name);
                            int wheelServoID01 = 0;
                            int wheelServoID02 = 0;
                            int wheelServoID03 = 0;
                            int wheelServoID04 = 0;

                            if (jockstickData != null && jockstickData.type == JockstickData.JockType.twoServo)
                            {
                                wheelServoID01 = jockstickData.leftUpID;
                                wheelServoID02 = jockstickData.rightUpID;
                                wheelServoID03 = 0;
                                wheelServoID04 = 0;
                            }
                            else if (jockstickData != null && jockstickData.type == JockstickData.JockType.fourServo)
                            {
                                wheelServoID01 = jockstickData.leftUpID;
                                wheelServoID02 = jockstickData.rightUpID;
                                wheelServoID03 = jockstickData.leftBottomID;
                                wheelServoID04 = jockstickData.rightBottomID;
                            }

                            int turnCount = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count;
                            int angleCount = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList().Count;
                            int motorCount = 0;
                            if (null != RobotManager.GetInst().GetCurrentRobot().MotorsData)
                            {
                                motorCount = RobotManager.GetInst().GetCurrentRobot().MotorsData.Count;
                            }
                            if (turnCount < 2 && motorCount < 2)
                            {
                                editTitle.enabled = false;
                                editDescribe.enabled = false;
                                //ShowOrHideLines(false);
                                //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("轮模式舵机数量不足"));
                                if (turnCount + angleCount == 0)
                                {//只有马达
                                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("马达数量不足，无法使用方向键"), 1, CommonTipsColor.red);
                                }
                                else if (motorCount == 0 && turnCount + angleCount < 2)
                                {
                                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机数量不足，无法使用方向键"), 1, CommonTipsColor.red);
                                }
                                else if (motorCount > 0 && turnCount + angleCount > 0 && turnCount + angleCount < 2)
                                {
                                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机和马达数量不足，无法使用方向键"), 1, CommonTipsColor.red);
                                }
                                else if (turnCount + angleCount >= 2)
                                {
                                    if (turnCount == 0)
                                    {
                                        UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置轮模式提示"), ModifyServoTypeSetting, true);
                                    }
                                    else
                                    {
                                        UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置摇杆轮模式提示"), ModifyServoTypeSetting, true);
                                    }
                                }
                            }
                            else if (null != jockstickData && jockstickData.motionType == CtrlMotionType.servo && ((wheelServoID01 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID01).modelType == ServoModel.Servo_Model_Angle) ||
                                     (wheelServoID02 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID02).modelType == ServoModel.Servo_Model_Angle) ||
                                     (wheelServoID03 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID03).modelType == ServoModel.Servo_Model_Angle) ||
                                     (wheelServoID04 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID04).modelType == ServoModel.Servo_Model_Angle)))
                            {
                                // 清空当前摇杆数据
                                jockstickData.leftBottomID = 0;
                                jockstickData.leftUpID = 0;
                                jockstickData.rightBottomID = 0;
                                jockstickData.rightUpID = 0;
                                jockstickData.type = JockstickData.JockType.none;

                                editTitle.enabled = true;
                                editDescribe.enabled = true;
                                ShowOrHideLines(true);
                                OpenJoystickSettingUI(obj.name);
                            }
                            else
                            {
                                editTitle.enabled = true;
                                editDescribe.enabled = true;
                                ShowOrHideLines(true);
                                OpenJoystickSettingUI(obj.name);
                            }
                        }
                    }
                }
            }
            
        }
        catch (Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
    }
    /// <summary>
    /// 负责设置状态下控件拖动事件
    /// </summary>
    bool flag1 = false; //拖动前左侧面板是否显示
    bool dragging = false;  //拖动ing 
    Transform dragingTransform; //正在被拖动的控件
    Vector3 finalPosition; //控件最终适应的位置
    bool isColliderCheck = false;
    bool isNewOperate = false;
    protected override void OnDragdropStart(GameObject obj)
    {
        if (curDragStatus == dragType.dragRelease)
        {
            curDragStatus = dragType.dragStart;

            editTitle.enabled = false;
            editDescribe.enabled = false;
            if (placedArea.enabled)
                placedArea.enabled = false;
            isZeroArea = false;
            //Debug.Log("dragdrop is start");
            base.OnDragdropStart(obj);
            obj.transform.localScale = Vector3.one;
            curDragControl = obj;
            if (IsSetting && obj.tag.Contains("widget"))
            {
                //Debug.Log("drag is start");
                placedArea.enabled = true;
                ShowOrHideLines(true);
                Vector4 currentSelect = TurnWidgetRect(obj.GetComponentInChildren<UIWidget>());
                if (smartPlace.IsPlaceable())
                    smartPlace.ChangeCurPos(currentSelect);

                isExitCollision = 0;
                dragging = true;
                dragingTransform = obj.transform;
                GetTCompent.GetCompent<Rigidbody>(obj.transform).useGravity = false;
                GetTCompent.GetCompent<TriggleCheck>(obj.transform).onTriggleEnter = OnEnterCollision;
                GetTCompent.GetCompent<TriggleCheck>(obj.transform).onTriggleExit = OnExitCollision;
                GetTCompent.GetCompent<BoxCollider>(obj.transform).isTrigger = true;

                //Transform leftSelect = GameObject.Find("userdefineControllerUI/Left/leftBoard/ContainerLeft/EditScrollview/Grid").transform;
                bottomTrans.parent.GetChild(1).gameObject.SetActive(true);  //拖动时垃圾桶出现

                if (obj.transform.parent != gridPanel)
                {
                    if (obj.tag.Contains("widget"))
                    {
                        for (int hu = 0; hu < 4; hu++)
                        {
                            leftItems.GetChild(hu).GetComponent<ButtonEvent>().enabled = false;
                            leftItems.GetChild(hu).GetComponent<DragdropItemEX>().enabled = false;
                        }
                    }
                }
                else
                {
                    for (int ku = 1; ku < gridPanel.childCount; ku++)
                    {
                        //Debug.Log(gridPanel.GetChild(ku).name);
                        if (gridPanel.GetChild(ku).name != obj.transform.name)
                        {
                            gridPanel.GetChild(ku).GetComponent<ButtonEvent>().enabled = false;
                            gridPanel.GetChild(ku).GetComponent<DragdropItemEX>().enabled = false;
                        }
                    }
                }

                if (isBottomShow)
                    ShowOrHideBottmBoard(false);
                if (isLeftShow)
                {
                    flag1 = true;
                    ShowOrHideLeftboard(false);

                    if (obj.tag.Contains("widget") && obj.transform.parent != gridPanel) // obj为控件库里的物体
                    {
                        //bottomTrans.parent.GetChild(1).gameObject.SetActive(false);
                        //Debug.Log("isNewOperate is true");
                        isNewOperate = true;
                        bottomTrans.parent.GetChild(1).gameObject.SetActive(false);
                        obj.transform.SetParent(gridPanel);
                        //Debug.Log(gridPanel);

                        if (obj.tag.Contains("widget_action"))
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Start build new action control widget!!");

                            obj.transform.GetChild(4).GetComponent<UISprite>().enabled = true;
                            leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = true;
                        }
                        else if (obj.tag.Contains("widget_joystick"))
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Start build new joystick control widget!!");

                            obj.transform.GetChild(3).GetComponent<UISprite>().enabled = true;
                            leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = true;
                        }
                        else if (obj.tag.Contains("widget_vslider"))
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Start build new vslider control widget!!");

                            obj.transform.GetChild(3).GetComponent<UISprite>().enabled = true;
                            leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = true;
                            leftItems.GetChild(2).GetChild(4).GetComponent<UILabel>().enabled = false;
                        }
                        else if (obj.tag.Contains("widget_hslider"))
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Start build new hslider control widget!!");

                            obj.transform.GetChild(3).GetComponent<UISprite>().enabled = true;
                            leftItems.GetChild(3).GetChild(3).GetComponent<UISprite>().enabled = true;
                            leftItems.GetChild(3).GetChild(4).GetComponent<UILabel>().enabled = false;
                        }

                        if (obj.GetComponent<UIDragScrollView>() != null)
                            GameObject.DestroyImmediate(obj.GetComponent<UIDragScrollView>());

                    }
                }
                else
                    flag1 = false;

                if (!isNewOperate)
                {
                    smartPlace.SetCurRect(obj.name);
                }

                if (isNewOperate)
                {
                    if (!(smartPlace.IsNewControl(currentSelect)))
                    {
                        //Debug.Log("IsNewControl is " + smartPlace.IsNewControl(currentSelect));
                        isZeroArea = true;
                        placedArea.enabled = false;
                        DrawCanPlacedArea(new Vector2(0, 0), 0, 0);
                    }
                    else
                    {
                        isZeroArea = false;
                        placedArea.enabled = true;
                        DrawCanPlacedArea(new Vector2((currentSelect.x + currentSelect.z) / 2.0f, (currentSelect.y + currentSelect.w) / 2.0f), (int)(currentSelect.z - currentSelect.x), (int)(currentSelect.y - currentSelect.w));
                    }

                }
                else
                {
                    isZeroArea = false;
                    placedArea.enabled = true;
                    DrawCanPlacedArea(new Vector2((currentSelect.x + currentSelect.z) / 2.0f, (currentSelect.y + currentSelect.w) / 2.0f), (int)(currentSelect.z - currentSelect.x), (int)(currentSelect.y - currentSelect.w));
                }

            }
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now has start drag the obj!!");
        }
        /*else
        {
            if (placedArea.enabled)
                placedArea.enabled = false;
        }*/
    }

    //松开选择的控件放置于界面上
    protected override void OnDragdropRelease(GameObject obj)
    {
        bool joystickFlag = false;
#if UNITY_EDITOR
        joystickFlag = true;
#endif
        if (curDragStatus == dragType.dragStart)
        {
            curDragStatus = dragType.dragRelease;

            base.OnDragdropRelease(obj);

            //ui状态更改
            //if (flag1 != isLeftShow)  //拖动前后左侧面板状态还原
            //ShowOrHideLeftboard(flag1);

            Vector4 curSelected = TurnWidgetRect(obj.GetComponentInChildren<UIWidget>());
            //拖动出界判定失败
            if (isNewOperate && smartPlace.IsNewControl(curSelected))
            {
                isZeroArea = false;
            }

            if (leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled)
                leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = false;
            if (leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled)
                leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
            if (leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled)
                leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = false;
            if (leftItems.GetChild(2).GetChild(4).GetComponent<UILabel>().enabled)
                leftItems.GetChild(2).GetChild(4).GetComponent<UILabel>().enabled = false;
            if (leftItems.GetChild(3).GetChild(3).GetComponent<UISprite>().enabled)
                leftItems.GetChild(3).GetChild(3).GetComponent<UISprite>().enabled = false;
            if (leftItems.GetChild(3).GetChild(4).GetComponent<UILabel>().enabled)
                leftItems.GetChild(3).GetChild(4).GetComponent<UILabel>().enabled = false;

            bottomTrans.parent.GetChild(1).gameObject.SetActive(false);  //拖动时垃圾桶出现
            tireBackgrounds.gameObject.SetActive(true);

            dragging = false;
            dragingTransform = null;

            for (int hu = 0; hu < 4; hu++)
            {
                leftItems.GetChild(hu).GetComponent<ButtonEvent>().enabled = true;
                leftItems.GetChild(hu).GetComponent<DragdropItemEX>().enabled = true;
            }

            if (obj != null && obj.tag.Contains("widget"))
            {
                obj.transform.GetComponent<ButtonEvent>().enabled = false;
                obj.transform.GetComponent<DragdropItemEX>().enabled = false;
            }

            /*for (int cv = 1; cv < gridPanel.childCount; cv++)
            {
                //Debug.Log(gridPanel.GetChild(ku).name);
                if (gridPanel.GetChild(cv).gameObject.tag.Contains("widget"))
                {
                    gridPanel.GetChild(cv).GetComponent<ButtonEvent>().enabled = true;
                    gridPanel.GetChild(cv).GetComponent<DragdropItemEX>().enabled = true;
                }            
            }*/

            //判断是否拖动成功
            if (!IsDragSuccess() || isZeroArea)
            {
                placedArea.enabled = false;
                if (isDeleta || isZeroArea)
                {
                    //Debug.Log("isDeleta is " + isDeleta);
                    if (obj.tag.Contains("widget_action") && ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)) != null)
                    {
                        string an = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionId;
                        if (an != "" && an != "-1")
                        {
                            //Debug.Log("now delete act icon is" + an);
                            if (!showActionList.Contains(an))
                                showActionList.Add(an);
                        }

                        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Delete action control data!!");

                        ControllerManager.GetInst().RemoveAction(obj.name);
                        isActionDataChange = true;
                    }
                    else if (obj.tag.Contains("widget_vslider"))
                    {
                        if (PlatformMgr.Instance.isAppleDemo)
                        {
                            if (RecordContactInfo.Instance.openType == "default")
                            {
                                //Debug.Log("官方模型不可删除");
                                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("官方模型不可更改配置"), 1, CommonTipsColor.red);
                            }
                            else
                            {
                                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Delete vslider control data!!");

                                ControllerManager.GetInst().RemoveSliderBar(obj.name);
                            }
                        }
                        else
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Delete vslider control data!!");

                            ControllerManager.GetInst().RemoveSliderBar(obj.name);
                        }
                        
                    }
                    else if (obj.tag.Contains("widget_hslider"))
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Delete hslider control data!!");

                        ControllerManager.GetInst().RemoveHSliderBar(obj.name);
                    }
                    else if (obj.tag.Contains("widget_joystick"))
                    {
                        if (RecordContactInfo.Instance.openType == "default" && !joystickFlag)
                        {
                            //Debug.Log("官方模型不可删除");
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("官方模型不可更改配置"), 1, CommonTipsColor.red);
                        }
                        else
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Delete joystick control data!!");

                            ControllerManager.GetInst().RemoveJoystick(obj.name);
                        }
                    }
                    if ((RecordContactInfo.Instance.openType == "default" && !(obj.tag.Contains("widget_joystick") && !joystickFlag || (obj.tag.Contains("widget_vslider") && PlatformMgr.Instance.isAppleDemo))) 
                        || (RecordContactInfo.Instance.openType != "default") 
                        || (RecordContactInfo.Instance.openType == "default" && (obj.tag.Contains("widget_joystick") || (obj.tag.Contains("widget_vslider") && PlatformMgr.Instance.isAppleDemo)) && isZeroArea))
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Delete current control widget!!");

                        isZeroArea = false;
                        GameObject.Destroy(obj);
                        isDeleta = false;
                        isTotalDataChange = true;
                    }
                    else if (RecordContactInfo.Instance.openType == "default" && obj.tag.Contains("widget_joystick") && (!isZeroArea))
                    {
                        isDeleta = false;
                        isTotalDataChange = true;
                        obj.transform.localPosition = finalPosition;
                        if (((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)) != null)
                            ((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;
                    }
                    else if (PlatformMgr.Instance.isAppleDemo && RecordContactInfo.Instance.openType == "default" && obj.tag.Contains("widget_vslider") && (!isZeroArea))
                    {
                        isDeleta = false;
                        isTotalDataChange = true;
                        obj.transform.localPosition = finalPosition;
                        if ((ControllerManager.GetInst().GetWidgetdataByID(obj.name)) != null)
                            ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;
                    }
                    if (isNewOperate)
                    {
                        isNewOperate = false;
                    }
                }
                //隐藏不可触控的区域
                ClientMain.GetInst().StartCoroutine(DelayDragOthers(0.2f));

                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now has release drag the obj!!");
                return;
            }
            isTotalDataChange = true;

            if (smartPlace.IsPlaceable())
            {
                //isZeroArea = false;

                //计算可放置区域大小
                placedArea.enabled = true;
                DrawCanPlacedArea(new Vector2((smartPlace.curRect.board.x + smartPlace.curRect.board.z) / 2.0f, (smartPlace.curRect.board.y + smartPlace.curRect.board.w) / 2.0f), (int)(smartPlace.curRect.board.z - smartPlace.curRect.board.x), (int)(smartPlace.curRect.board.y - smartPlace.curRect.board.w));//(new Vector2(smartPlace.curRect.board.x, smartPlace.curRect.board.y), (int)smartPlace.curRect.board.z, (int)smartPlace.curRect.board.w);
                //smartPlace.AddBoard(new SmartPlace.RectBoard(obj.name, TurnWidgetRect(obj.GetComponentInChildren<UISprite>())));
            }
            placedArea.enabled = false;
            obj.transform.localPosition = finalPosition;

            #region    //判断当前操作类型， 对应修改临时的controllerdata数据

            if (isNewOperate)  //新建控件
            {
                //Debug.Log("new control");
                //Debuger.Log(ControllerManager.GetInst().widgetManager.joyStickManager.JoystickNum());
                isNewOperate = false;
                obj.name = System.DateTime.Now.ToFileTime().ToString(); //对应的控件id 为创建的时间
                DragdropItemEX dragdropEx = obj.GetComponent<DragdropItemEX>();
                if (dragdropEx != null)    //释放完成后 更改克隆属性，不然会有问题
                {
                    dragdropEx.cloneOnDrag = false;
                    dragdropEx.restriction = UIDragDropItem.Restriction.None;
                }
                if (obj.tag.Contains("widget_action"))
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "New action control data successfully!!");

                    ControllerManager.GetInst().NewAction(obj.name, obj.transform.localPosition);
                    obj.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
                }
                else if (obj.tag.Contains("widget_vslider"))
                {
                    do 
                    {
                        if (PlatformMgr.Instance.isAppleDemo)
                        {
                            if (RecordContactInfo.Instance.openType == "default")
                            {
                                placedArea.enabled = false;
                                GameObject.Destroy(obj);
                                ControllerManager.GetInst().RemoveSliderBar(obj.name);
                                //isDeleta = false;
                                isTotalDataChange = true;
                                dragging = false;
                                dragingTransform = null;
                                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("官方模型不可更改配置"), 1, CommonTipsColor.red);                                //隐藏不可触控的区域
                                ClientMain.GetInst().StartCoroutine(DelayDragOthers(0.2f));
                                return;
                            }
                        }
                        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "New vslider control data successfully!!");

                        ControllerManager.GetInst().NewSliderBar(obj.name, obj.transform.localPosition);
                        obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                        obj.transform.GetChild(4).GetComponent<UILabel>().enabled = true;
                        obj.transform.GetChild(4).GetComponent<UILabel>().text = "";
                        obj.transform.GetChild(5).GetComponent<UISprite>().enabled = true;
                        obj.transform.GetChild(6).GetComponent<UISprite>().enabled = true;
                        obj.transform.GetChild(5).localEulerAngles = Vector3.zero;
                        obj.transform.GetChild(6).localEulerAngles = new Vector3(0, 180, 0);
                    } while (false);
                }
                else if (obj.tag.Contains("widget_hslider"))
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "New hslider control data successfully!!");

                    ControllerManager.GetInst().NewHSliderBar(obj.name, obj.transform.localPosition);
                    obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(4).GetComponent<UILabel>().enabled = true;
                    obj.transform.GetChild(4).GetComponent<UILabel>().text = "";
                    obj.transform.GetChild(5).GetComponent<UILabel>().text = "-118°";
                    obj.transform.GetChild(6).GetComponent<UILabel>().text = "118°";
                }
                else if (obj.tag.Contains("widget_joystick"))
                {
                    if (ControllerManager.GetInst().widgetManager.joyStickManager.JoystickNum() > 0 || RecordContactInfo.Instance.openType == "default" && !joystickFlag)
                    {
                        //Debug.Log("摇杆数量上限！！");
                        placedArea.enabled = false;
                        GameObject.Destroy(obj);
                        ControllerManager.GetInst().RemoveJoystick(obj.name);
                        //isDeleta = false;
                        isTotalDataChange = true;
                        dragging = false;
                        dragingTransform = null;

                        if (RecordContactInfo.Instance.openType == "default")
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("官方模型不可更改配置"), 1, CommonTipsColor.red);
                        else
                            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("无法增加摇杆"), 1, CommonTipsColor.red);

                        //隐藏不可触控的区域
                        ClientMain.GetInst().StartCoroutine(DelayDragOthers(0.2f));
                        return;
                    }
                    else
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "New joystick control data successfully!!");

                        ControllerManager.GetInst().NewJoystick(obj.name, obj.transform.localPosition);
                    }
                    obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                }
            }
            else  //拖动操作
            {
                //Debug.Log("Now Operate is Drag!!");
                if (obj.tag.Contains("widget_action"))
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Change action control widget position!!");
                    if (((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)) != null)
                        ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;//.NewAction(obj.name, obj.transform.localPosition);
                }
                else if (obj.tag.Contains("widget_vslider"))
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Change vslider control widget position!!");
                    if (((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)) != null)
                        ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;
                }
                else if (obj.tag.Contains("widget_hslider"))
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Change hslider control widget position!!");
                    if (((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)) != null)
                        ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;
                }
                else if (obj.tag.Contains("widget_joystick"))
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Change joystick control widget position!!");
                    if (((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)) != null)
                        ((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;
                }
            }

            smartPlace.AddBoard(new SmartPlace.RectBoard(obj.name, TurnWidgetRect(obj.GetComponentInChildren<UISprite>())));
            placedArea.enabled = false;
            #endregion

            if (obj.GetComponent<BoxCollider>() != null)
                obj.GetComponent<BoxCollider>().enabled = true;

            if (IsSetting && obj.tag.Contains("widget"))   //拖动结束 删掉碰撞检测功能
            {
                if (obj.GetComponent<Rigidbody>() != null)
                    GameObject.Destroy(obj.GetComponent<Rigidbody>());
                if (obj.GetComponent<TriggleCheck>() != null)
                    GameObject.Destroy(obj.GetComponent<TriggleCheck>());
                if (isExitCollision != 0)
                {
                    //   isExitCollision = 0;
                    //  obj.transform.localPosition = finalPosition;
                }
            }
            //隐藏不可触控的区域
            ClientMain.GetInst().StartCoroutine(DelayDragOthers(0.2f));

            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now has release drag the obj!!");
        }
        else
        {
            if (placedArea.enabled)
            {
                placedArea.enabled = false;
                obj.transform.localPosition = finalPosition;
            }
        }
    }
    IEnumerator DelayDragOthers(float t)
    {
        //cleanBoxCollider.gameObject.SetActive(false); 
        yield return new WaitForSeconds(t);

        //Debug.Log("now retry drag the control!!");

        tireBackgrounds.gameObject.SetActive(false);

        for (int cv = 1; cv < gridPanel.childCount; cv++)
        {
            //Debug.Log(gridPanel.GetChild(ku).name);
            if (gridPanel.GetChild(cv).gameObject.tag.Contains("widget"))
            {
                gridPanel.GetChild(cv).GetComponent<ButtonEvent>().enabled = true;
                gridPanel.GetChild(cv).GetComponent<DragdropItemEX>().enabled = true;
            }
        }
    }
    /// <summary>
    /// 判断是否拖动成功
    /// </summary>
    /// <returns></returns>
    bool IsDragSuccess()
    {
        if (isDeleta)
            return false;
        return true;
    }
    /// <summary>
    /// 初始化遥控，根据遥控ID初始化对应的遥控界面和遥控内容
    /// </summary>
    public override void Init()
    {
        base.Init();
        //curControllerData 设置界面
      //  DataToWidget();
    }
    public override void LoadUI()
    {
        base.LoadUI();
        if (!ControllerManager.IsControllersNull(RobotManager.GetInst().GetCurrentRobot().ID)) //有遥控数据
        {
            showPowerFlag = true;
            ClientMain.GetInst().StartCoroutine(DelayExcute());
        } else
        {
            showPowerFlag = false;
        }
    }
    IEnumerator DelayExcute()
    {
        //Debug.Log("DataToWidget is run1");
        yield return null;
        yield return new WaitForEndOfFrame();
        DataToWidget();
    }

    //跟随拖拽显示网格
    protected override void OnButtonDrag(GameObject obj, Vector2 delta)
    {
        base.OnButtonDrag(obj, delta);
        if (obj == null || obj != curDragControl)
        {
            dragging = false;
            return;
        }
        if (isSetting && obj.tag.Contains("widget") && dragging)
        {
            if (smartPlace.IsEmptyBgboard())
            {
                smartPlace.Clear();
                smartPlace.SetBgBoard(new Vector4(-gridPanel.GetComponent<UIWidget>().width / 2.0f + (PublicFunction.Back_Btn_Pos.x * positionXRatio), gridPanel.GetComponent<UIWidget>().height / 2.0f, gridPanel.GetComponent<UIWidget>().width / 2.0f - (PublicFunction.Back_Btn_Pos.x * positionXRatio), -gridPanel.GetComponent<UIWidget>().height / 2.0f));
            }

            Vector4 curSelect = TurnWidgetRect(obj.GetComponentInChildren<UIWidget>());
            smartPlace.ChangeCurPos(curSelect);

            if (smartPlace.IsNewControl(curSelect))
            {
                isZeroArea = false;
                //Debug.Log("IsEmptyable is true");
                placedArea.enabled = true;
                DrawCanPlacedArea(new Vector2((curSelect.x + curSelect.z) / 2.0f, (curSelect.y + curSelect.w) / 2.0f), (int)(curSelect.z - curSelect.x), (int)(curSelect.y - curSelect.w));
            }
            //smartPlace.SetCurRect(new SmartPlace.RectBoard(obj.name
            //smartPlace.SetCurRect(new Vector4(obj.transform.localPosition.x, obj.transform.localPosition.y, obj.GetComponentInChildren<UIWidget>().width, obj.GetComponentInChildren<UIWidget>().height));
            if (smartPlace.IsPlaceable())
            {
                isZeroArea = false;

                //PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now has space for obj!!");

                //计算可放置区域大小
                placedArea.enabled = true;
                DrawCanPlacedArea(new Vector2((smartPlace.curRect.board.x + smartPlace.curRect.board.z) / 2.0f, (smartPlace.curRect.board.y + smartPlace.curRect.board.w) / 2.0f), (int)(smartPlace.curRect.board.z - smartPlace.curRect.board.x), (int)(smartPlace.curRect.board.y - smartPlace.curRect.board.w));//(new Vector2(smartPlace.curRect.board.x, smartPlace.curRect.board.y), (int)smartPlace.curRect.board.z, (int)smartPlace.curRect.board.w);
                //smartPlace.AddBoard(new SmartPlace.RectBoard(obj.name, TurnWidgetRect(obj.GetComponentInChildren<UISprite>())));
            }
        }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        IsDragSuccess();
    }

    
    public override void Open()
    {
        base.Open();
        if (isFirstSetting)
        {
            //Debug.Log("StartCoroutine");
            ClientMain.GetInst().StartCoroutine(DelayOneFrame(OnFirstComing));
        }
    }
    /// <summary>
    /// 默认配置遥控进来
    /// </summary>
    void OnFirstComing()
    {
        //Debug.Log("IsSetting is true");
        IsSetting = true;
    }
    /// <summary>
    /// 进碰撞
    /// </summary>
    bool isDeleta = false;
    void OnEnterCollision(Collider collider)
    {
        isExitCollision++;
        if (collider.name == "deleta")
        {
            isDeleta = true;
        }
    }
    /// <summary>
    /// 出碰撞
    /// </summary>
    void OnExitCollision(Collider collider)
    {
        isExitCollision--;
        if (collider.name == "deleta")
        {
            isDeleta = false;
        }
    }
    /// <summary>
    /// 绘制可放置的区域
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void DrawCanPlacedArea(Vector2 pos, int width, int height)
    {
        placedArea.transform.localPosition = pos;
        placedArea.width = width;
        placedArea.height = height;
        finalPosition = pos;
    }
    /// <summary>
    /// 滞后一
    /// </summary>
    /// <param name="del"></param>
    /// <returns></returns>
    IEnumerator DelayOneFrame(EventDelegate.Callback del)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        del();
    }
    
    void SetConnectState()
    {
        string connectName = null;
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            connectName = "connect";
            if (null != connectBtn && null != powerTrans)
            {
                connectBtn.localPosition = powerTrans.localPosition - new Vector3(122, 0);
            }
        }
        else
        {
            connectName = "disconnect";
            if (null != connectBtn && null != powerTrans)
            {
                connectBtn.localPosition = powerTrans.localPosition;
            }
        }
        if (!string.IsNullOrEmpty(connectName))
        {
            if (null != mConnectIcon)
            {
                mConnectIcon.spriteName = connectName;
            }
            if (null != mConnectBtn)
            {
                mConnectBtn.normalSprite = connectName;
            }
        }
        if (null != mPowerShow)
        {
            mPowerShow.SetPowerState();
        }
    }
    #endregion
}