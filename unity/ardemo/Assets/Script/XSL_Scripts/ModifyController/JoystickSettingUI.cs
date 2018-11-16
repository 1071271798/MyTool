using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;
using Game.Event;
/// <summary>
/// author: 孙宇
/// describe:摇杆设置界面
/// time: 
/// </summary>
public class JoystickSettingUI :BaseUI
{
    public bool isFirstSetting;
    private UISprite selectServo;
    private int selectServoID;
    private int selectWheelID;
    private bool curSelectServo;
    private Transform tra;

    private JockstickData joyData; //临时数据
    public JockstickData JoyData
    {
        get
        {
            return joyData;
        }
    }
    string widgetID;
    public JoystickSettingUI(string widgetID)
    {
        mUIResPath = "Prefab/UI/control/joystickSettingUI";
        isFirstSetting = false;
        joyData = new JockstickData();
        CopyData(joyData,(JockstickData)ControllerManager.GetInst().GetWidgetdataByID(widgetID));
        if (joyData.type == JockstickData.JockType.none)
            isFirstSetting = true;
        this.widgetID = widgetID;
        //如果只能设置两轮的则不要去选择
        int turnCount = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count;
        int angleCount = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList().Count;
        int motorCount = 0;
        if (null != RobotManager.GetInst().GetCurrentRobot().MotorsData)
        {
            motorCount = RobotManager.GetInst().GetCurrentRobot().MotorsData.Count;
        }
        if (motorCount < 4 && angleCount + turnCount < 4)
        {
            isFirstSetting = false;
            joyData.type = JockstickData.JockType.twoServo;
        }
    }

    public void CopyData(JockstickData data, JockstickData copyD)
    {
        if (copyD != null)
        {
            data.type = copyD.type;
            data.motionType = copyD.motionType;
            data.leftUpID = copyD.leftUpID;
            data.leftBottomID = copyD.leftBottomID;
            data.rightBottomID = copyD.rightBottomID;
            data.rightUpID = copyD.rightUpID;            
        }
    }

    private GameObject settingUI2;
    private GameObject settingUI1;
    private UILabel Title;
    private UILabel Details;
    private Transform bottomTrans;   //底部的舵机面板
    private int wheelType = 0;
    private bool hasJoysickData;
    protected override void AddEvent()
    {
        base.AddEvent();
        curSelectServo = false;
        EventMgr.Inst.Regist(EventID.Change_Servo_Model, ChangeServoModel);

        // settingUI1
        Transform trans = mTrans.Find("settingUI1/Up/topLeft").transform;  //top - left
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);//UserdefControllerScene.upSpace);

        trans = mTrans.Find("settingUI1/Center/type1").transform; // center - left
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Center, -200);
        trans = mTrans.Find("settingUI1/Center/type2").transform; // center - right
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Center, 200);

        // settingUI2
        trans = mTrans.Find("settingUI2/Up/topLeft").transform;  //top - left
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
        trans = mTrans.Find("settingUI2/Up/topRight").transform;       //top - right
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);//, topR.GetChild(0).GetComponent<UIWidget>().width);

        trans = mTrans.Find("Title").transform;  // title
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Top, 0, PublicFunction.Title_Margin);
        Title = trans.GetComponent<UILabel>();
        Vector3 po2 = new Vector3(0, trans.localPosition.y - 40, 0);
        Details = mTrans.Find("settingUI2/Details").transform.GetComponent<UILabel>();
        Details.transform.localPosition = po2;
        trans = mTrans.Find("settingUI2/Bottom").transform.GetChild(0);  //bottom
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Bottom, 0, -trans.GetComponentInChildren<UIWidget>().height - 11f);//UIManager.GetWinPos(trans, UIWidget.Pivot.Bottom,0,-trans.GetComponentInChildren<UISprite>().height);
        bottomTrans = trans;

        settingUI2 = mTrans.Find("settingUI2").gameObject;
        settingUI1 = mTrans.Find("settingUI1").gameObject;
        Transform bottomBoard = settingUI2.transform.Find("Bottom/bottomBoard");
        if (null != bottomBoard)
        {
            UISprite bg = GameHelper.FindChildComponent<UISprite>(bottomBoard, "bg");
            if (null != bg)
            {
                bg.width = PublicFunction.GetWidth();
            }
            UISprite shadow = GameHelper.FindChildComponent<UISprite>(bottomBoard, "shadow");
            if (null != shadow)
            {
                shadow.width = PublicFunction.GetWidth();
            }
        }

        
        if (isFirstSetting) // 车子结构选择界面
        {
            hasJoysickData = false;
            settingUI2.SetActive(false);
            Title.text = LauguageTool.GetIns().GetText("选择车子类型");
            settingUI1.transform.GetChild(1).GetChild(0).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("two-wheels");
            settingUI1.transform.GetChild(1).GetChild(1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("four-wheels");
            Details.enabled = false;
            bottomTrans.gameObject.SetActive(false); //隐藏底部面板

            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Enter joystick setting UI1!!");
        }
        else    // 配置界面
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Enter joystick setting UI2!!");

            hasJoysickData = true;
            //settingUI1.SetActive(false);
            
            Title.text = LauguageTool.GetIns().GetText("设置轮子主标题");
            Details.enabled = true;
            string detailtext = LauguageTool.GetIns().GetText("设置轮子副标题");
            string djids = "1";
            Details.text = string.Format(detailtext, djids);
            tra = null;
            if (joyData.type == JockstickData.JockType.twoServo)
            {
                wheelType = 2;
                tra = settingUI2.transform.GetChild(1).GetChild(0);
            }
            else if (joyData.type == JockstickData.JockType.fourServo)
            {
                wheelType = 4;
                tra = settingUI2.transform.GetChild(1).GetChild(1);
            }
            else if (joyData.type == JockstickData.JockType.treeServo)
            {
                wheelType = 3;
                tra = settingUI2.transform.GetChild(1).GetChild(2);
            }
            else
            { 
            
            }

            ShowSetting_2();

            tra.gameObject.SetActive(true);
            for (int i = 0; i < tra.childCount; i++)
            {
                if (i == 0) //1号轮子
                {
                    if (joyData.leftUpID != 0)
                    {
                        //tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoAngleEnable";
                        //tra.GetChild(0).GetChild(2).GetComponent<UISprite>().enabled = true;
                        SetServo(tra.Find("wheel1/servo"), joyData.leftUpID, joyData.motionType);
                    }
                }
                else if (i == 1) // 2号轮子
                {
                    if (joyData.rightUpID != 0)
                    {
                        //tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoAngleEnable";
                        SetServo(tra.Find("wheel2/servo"), joyData.rightUpID, joyData.motionType);
                    }
                    
                }
                else if (i == 2)//3号轮子
                {
                    if (joyData.leftBottomID != 0)
                    {
                        //tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoAngleEnable";
                        SetServo(tra.Find("wheel3/servo"), joyData.leftBottomID, joyData.motionType);
                    }
                    
                }
                else if (i == 3)//4号轮子
                {
                    if (joyData.rightBottomID != 0)
                    {
                        //tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoAngleEnable";
                        SetServo(tra.Find("wheel4/servo"), joyData.rightBottomID, joyData.motionType);
                    }
                    
                }
            }
            //ShowDuojiList();
            //ShowServosState();
            //bottomTrans.gameObject.SetActive(false); //隐藏底部面板
        }   
    }

    protected override void Close()
    {
        EventMgr.Inst.UnRegist(EventID.Change_Servo_Model, ChangeServoModel);
    }

    void SetServo(Transform servo, int id, CtrlMotionType motionType)
    {
        if (null != servo)
        {
            servo.gameObject.SetActive(true);
            Transform text = servo.Find("text");
            Transform motorId = servo.Find("motorId");
            UISprite icon = servo.GetComponent<UISprite>();
            UILabel lb = null;
            if (motionType == CtrlMotionType.servo)
            {
                lb = text.GetComponent<UILabel>();
                text.gameObject.SetActive(true);
                motorId.gameObject.SetActive(false);
                icon.spriteName = "servo_angle";
            } else if (motionType == CtrlMotionType.motor)
            {
                lb = motorId.GetComponent<UILabel>();
                text.gameObject.SetActive(false);
                motorId.gameObject.SetActive(true);
                icon.spriteName = "motor";
            }
            if (null != lb)
            {
                lb.text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
            }
            //servo.parent.GetComponent<UISprite>().spriteName = "Clean";
        }
    }

    void HideServo(Transform servo)
    {
        if (null != servo)
        {
            servo.gameObject.SetActive(false);
            //servo.parent.GetComponent<UISprite>().spriteName = "servoBg2x";
        }
    }

    public override void LoadUI()
    {
        base.LoadUI();
    }

    Transform curSelectWheel;
    int wheelID = 0; //正在设置的轮子id
    int SetWheelID
    {
        set
        {
            if (wheelID != value)
            {
                if (curSelectWheel != null)  //切换设置的轮子时 上一个选中状态cancel
                    curSelectWheel.Find("selected").GetComponent<UISprite>().enabled = false;
            }
            wheelID = value;
            if (wheelID != 0) // wheelid 有设置时显示舵机列表
            {
                ShowDuojiList();
                ShowServosState(); // 对应的状态
            }
        }
    }
    void UpdateServoList()
    {
        Transform p = mTrans.Find("settingUI2/Bottom/bottomBoard/Sprite/grid").transform;

        List<byte> turnModelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList();
        List<byte> engelModelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList();

        if (p.childCount != 0)
        {
            for (int i = p.childCount - 1; i >= 0; i--)
                GameObject.Destroy(p.GetChild(i).gameObject);
        }

        UserdefControllerScene.InitServoList(p, OnButtonClick, wheelType);
    }
    /// <summary>
    /// 显示舵机列表
    /// </summary>
    void ShowDuojiList()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Display joystick servo list!!");

        //Debug.Log("hasJoysickData is " + hasJoysickData);
        bottomTrans.gameObject.SetActive(true);
        UserdefControllerUI.HideOrShowTrans(true, bottomTrans, UserdefControllerUI.directType.bottom);
        //
    }
    /// <summary>
    /// 显示舵机列表下的状态
    /// </summary>
    void ShowServosState()
    {
        if (joyData == null)
            return;
        List<byte> takenlist = new List<byte>();
        if (joyData.leftUpID != 0)
            takenlist.Add(joyData.leftUpID);
        if (joyData.rightUpID != 0)
            takenlist.Add(joyData.rightUpID);
        if (joyData.leftBottomID != 0)
            takenlist.Add(joyData.leftBottomID);
        if (joyData.rightBottomID != 0)
            takenlist.Add(joyData.rightBottomID);
        if (joyData.UpID != 0)
            takenlist.Add(joyData.UpID); 
        byte selected = 0;
        if (joyData.type == JockstickData.JockType.treeServo && wheelID == 3)
        {
            if (joyData.UpID != 0)
                selected = joyData.UpID;
        }
        else
        {
            if (wheelID == 1)
                selected = joyData.leftUpID;
            else if (wheelID == 2)
                selected = joyData.rightUpID;
            else if (wheelID == 3)
                selected = joyData.leftBottomID;
            else if (wheelID == 4)
                selected = joyData.rightBottomID;
        }
        ShowTakenState(takenlist, selected);
    }
    /// <summary>
    /// 显示被占用以及当前被选中的状态
    /// </summary>
    void ShowTakenState(List<byte> takenList, byte selected = 0)
    {
        if (selectServo != null)
            selectServo.enabled = false;

        if (takenList == null || takenList.Count == 0)
            return;
        Transform grid = bottomTrans.GetChild(1).GetChild(0);
        //Transform marks = GameObject.Find("UI Root/mark0").transform;
        byte b = 0;

        for (int i = 0; i < grid.childCount; i++)
        {
            CtrlMotionType motionType = CtrlMotionType.servo;
            if (grid.GetChild(i).name.StartsWith("motor_"))
            {
                motionType = CtrlMotionType.motor;
            }
            b = byte.Parse(grid.GetChild(i).name.Substring(6));
            /*string text = grid.GetChild(i).GetComponentInChildren<UILabel>().text;
            if (text.StartsWith(PublicFunction.ID_Format))
            {
                text = text.Substring(PublicFunction.ID_Format.Length);
            }
            byte.TryParse(text, out b);*/
            UISprite numIcon = grid.GetChild(i).Find("num").GetComponent<UISprite>();
            numIcon.enabled = false;
            UISprite selectState = grid.GetChild(i).Find("selectState").GetComponent<UISprite>();
            selectState.enabled = false;
            if (takenList.Contains(b) && selected != b && motionType == joyData.motionType) //被占用的状态
            {
                if (joyData.leftUpID != 0 && joyData.leftUpID == b)
                {
                    numIcon.enabled = true;
                    numIcon.spriteName = "1";
                }
                else if (joyData.rightUpID != 0 && joyData.rightUpID == b)
                {
                    numIcon.enabled = true;
                    numIcon.spriteName = "2";
                }
                else if (joyData.leftBottomID != 0 && joyData.leftBottomID == b)
                {
                    numIcon.enabled = true;
                    numIcon.spriteName = "3";
                }
                else if (joyData.rightBottomID != 0 && joyData.rightBottomID == b)
                {
                    numIcon.enabled = true;
                    numIcon.spriteName = "4";
                }
            }
            else if (selected == b && motionType == joyData.motionType)  //选中的状态 
            {
                //grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                if (joyData.leftUpID != 0 && joyData.leftUpID == b)
                {
                    selectServo = numIcon;
                    numIcon.spriteName = "1";
                    selectServo.enabled = true;
                }
                else if (joyData.rightUpID != 0 && joyData.rightUpID == b)
                {
                    selectServo = numIcon;
                    numIcon.spriteName = "2";
                    selectServo.enabled = true;
                }
                else if (joyData.leftBottomID != 0 && joyData.leftBottomID == b)
                {
                    selectServo = numIcon;
                    numIcon.spriteName = "3";
                    selectServo.enabled = true;
                }
                else if (joyData.rightBottomID != 0 && joyData.rightBottomID == b)
                {
                    selectServo = numIcon;
                    numIcon.spriteName = "4";
                    selectServo.enabled = true;
                }
                else
                {
                    selectServo = numIcon;
                    selectServo.enabled = false;
                }
                selectServoID = b;
            }
        }
    }

    void CheckServoTypes(int types)
    {
        int wheelServoID01 = 0;
        int wheelServoID02 = 0;
        int wheelServoID03 = 0;
        int wheelServoID04 = 0;

        if (types == 2) //双轮模式
        {
            wheelServoID01 = joyData.leftUpID;
            wheelServoID02 = joyData.rightUpID;
            wheelServoID03 = 0;
            wheelServoID04 = 0;
        }
        else if (types == 4) //四轮模式
        {
            wheelServoID01 = joyData.leftUpID;
            wheelServoID02 = joyData.rightUpID;
            wheelServoID03 = joyData.leftBottomID;
            wheelServoID04 = joyData.rightBottomID;
        }
        else
        {
            wheelServoID01 = 0;
            wheelServoID02 = 0;
            wheelServoID03 = 0;
            wheelServoID04 = 0;
        }

        if (joyData.motionType == CtrlMotionType.servo && ((wheelServoID01 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID01).modelType == ServoModel.Servo_Model_Angle) ||
            (wheelServoID02 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID02).modelType == ServoModel.Servo_Model_Angle) ||
            (wheelServoID03 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID03).modelType == ServoModel.Servo_Model_Angle) ||
            (wheelServoID04 != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)wheelServoID04).modelType == ServoModel.Servo_Model_Angle)))
        {
            //清除摇杆数据
            joyData.leftUpID = 0;
            joyData.rightUpID = 0;
            joyData.UpID = 0;
            joyData.leftBottomID = 0;
            joyData.rightBottomID = 0;
            joyData.type = JockstickData.JockType.none;

            CopyData((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(widgetID), joyData);
        }
    }

    bool isChange = false;
    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        string name = obj.name;
        if (name.Contains("backM"))  //返回 取消修改
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Cancel change joystick control data!!");

            CheckServoTypes(wheelType);
            mTrans.gameObject.SetActive(false);
            UserdefControllerScene.Ins.CloseJoystickSettingUI();
            UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.joystick_w);
            //UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("未完成配置提示"), AbandonSetting, isChange);
        }
        else if (name.Contains("backS"))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Back joystick setting UI1!!");
            //如果只能设置两轮的则不要去选择
            int turnCount = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count;
            int angleCount = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList().Count;
            int motorCount = 0;
            if (null != RobotManager.GetInst().GetCurrentRobot().MotorsData)
            {
                motorCount = RobotManager.GetInst().GetCurrentRobot().MotorsData.Count;
            }
            if (motorCount < 4 && angleCount + turnCount < 4)
            {
                CheckServoTypes(wheelType);
                mTrans.gameObject.SetActive(false);
                UserdefControllerScene.Ins.CloseJoystickSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.joystick_w);
            } else
            {
                ShowSetting_1();
            }
        }
        else if (name.Contains("save")) //保存数据
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Save current joystick control data!!");

            if (joyData.type == JockstickData.JockType.twoServo)
            {
                if (joyData.leftUpID == 0 || joyData.rightUpID == 0)
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("未完成设置"), 1, CommonTipsColor.red);
                else
                {
                    if (isChange)
                    {
                        CopyData((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(widgetID), joyData);  //确定修改， 
                        UserdefControllerUI.isTotalDataChange = true;
                    }
                    mTrans.gameObject.SetActive(false);
                    UserdefControllerScene.Ins.CloseJoystickSettingUI();
                    UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.joystick_w);
                }
                    //ShowSetting_1();
            }
            else if (joyData.type == JockstickData.JockType.fourServo)
            {
                if (joyData.leftUpID == 0 || joyData.rightUpID == 0 || joyData.leftBottomID == 0 || joyData.rightBottomID == 0)
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("未完成设置"), 1, CommonTipsColor.red);
                else
                {
                    if (isChange)
                    {
                        CopyData((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(widgetID), joyData);  //确定修改， 
                        UserdefControllerUI.isTotalDataChange = true;
                    }
                    mTrans.gameObject.SetActive(false);
                    UserdefControllerScene.Ins.CloseJoystickSettingUI();
                    UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.joystick_w);
                }
                    //ShowSetting_1();
            }
        }
        else if (name.Contains("type1"))  //双轮模式
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Select two-wheels model!!");

            selectWheelID = 1;
            //Debug.Log("two-wheels");
            wheelType = 2;
            /*if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count < 2)
            {
                //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("轮模式舵机数量不足"));
                if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count == 0)
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置轮模式提示"), ModifyServoTypeSetting, true);
                else
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置摇杆轮模式提示"), ModifyServoTypeSetting, true);
            }
            else*/
            {
                CheckServoTypes(wheelType);

                ShowSetting_2();
                if (joyData.type != JockstickData.JockType.twoServo)
                {
                    joyData.type = JockstickData.JockType.twoServo;
                    joyData.leftUpID = 0;
                    joyData.rightUpID = 0;
                    joyData.UpID = 0;
                    joyData.leftBottomID = 0;
                    joyData.rightBottomID = 0;

                    selectWheelID = 1;

                    Transform tra1 = settingUI2.transform.Find("Center/twoServo");

                    if (tra1 != null)
                    {
                        //tra1.GetChild(0).GetChild(2).GetComponent<UISprite>().enabled = true;
                        for (int i = 0; i < tra1.childCount; i++)
                        {
                            HideServo(tra1.GetChild(i).Find("servo"));
                        }
                        //tra1.GetChild(0).GetChild(2).gameObject.SetActive(true);
                    }

                    isChange = true;

                    Transform grid1 = bottomTrans.GetChild(1).GetChild(0);

                    if (grid1 != null)
                    {
                        for (int i = 0; i < grid1.childCount; i++)
                        {
                                grid1.GetChild(i).Find("num").GetComponent<UISprite>().enabled = false;
                                grid1.GetChild(i).Find("selectState").GetComponent<UISprite>().enabled = false;
                        }
                    }

                    if (selectServo != null)
                    {
                        selectServo.enabled = false;
                        selectServo = null;
                    }
                }
                else
                {
                    /*isChange = false;

                    Transform grid1p = bottomTrans.GetChild(1).GetChild(0);

                    if (grid1p != null)
                    {
                        for (int i = 0; i < grid1p.childCount; i++)
                        {
                            grid1p.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                            grid1p.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                            grid1p.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                            grid1p.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                            grid1p.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                        }
                    }*/

                    if (selectServo != null)
                    {
                        selectServo.enabled = true;
                    }
                    else
                    {
                        Transform grid1p = bottomTrans.GetChild(1).GetChild(0);
                        if (grid1p != null)
                        {
                            for (int i = 0; i < grid1p.childCount; i++)
                            {
                                grid1p.GetChild(i).Find("num").GetComponent<UISprite>().enabled = false;
                                grid1p.GetChild(i).Find("selectState").GetComponent<UISprite>().enabled = false;
                            }
                        }
                    }
                }
                settingUI2.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
            }
        }
        else if (name.Contains("type2")) //四轮模式
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Select four-wheels model!!");

            selectWheelID = 1;
            //Debug.Log("four-wheels");
            wheelType = 4;
            int motorCount = 0;
            if (null != RobotManager.GetInst().GetCurrentRobot().MotorsData)
            {
                motorCount = RobotManager.GetInst().GetCurrentRobot().MotorsData.Count;
            }
            if (motorCount < 4 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count < 4)
            {
                //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("轮模式舵机数量不足"));
                if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count == 0)
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置轮模式提示"), ModifyServoTypeSetting, true);
                else
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置摇杆轮模式提示"), ModifyServoTypeSetting, true);
            }
            else
            {
                CheckServoTypes(wheelType);

                ShowSetting_2();
                if (joyData.type != JockstickData.JockType.fourServo)
                {
                    joyData.type = JockstickData.JockType.fourServo;
                    joyData.leftUpID = 0;
                    joyData.rightUpID = 0;
                    joyData.UpID = 0;
                    joyData.leftBottomID = 0;
                    joyData.rightBottomID = 0;

                    selectWheelID = 1;

                    Transform tra2 = settingUI2.transform.Find("Center/fourServo");

                    if (tra2 != null)
                    {
                        //Debug.Log("四轮模式预配置！！");
                        //tra2.GetChild(0).GetChild(2).GetComponent<UISprite>().enabled = true;

                        for (int i = 0; i < tra2.childCount; i++)
                        {
                            HideServo(tra2.GetChild(i).Find("servo"));
                        }
                        //tra2.GetChild(0).GetChild(2).gameObject.SetActive(true);
                    }


                    isChange = true;

                    Transform grid2 = bottomTrans.GetChild(1).GetChild(0);

                    if (grid2 != null)
                    {
                        for (int i = 0; i < grid2.childCount; i++)
                        {
                            grid2.GetChild(i).Find("num").GetComponent<UISprite>().enabled = false;
                            grid2.GetChild(i).Find("selectState").GetComponent<UISprite>().enabled = false;
                        }
                    }

                    //ShowDuojiList(false);
                    if (selectServo != null)
                    {
                        selectServo.enabled = false;
                        selectServo = null;
                    }
                }
                else
                {
                    /*isChange = false;

                    Transform grid2p = bottomTrans.GetChild(1).GetChild(0);

                    if (grid2p != null)
                    {
                        for (int i = 0; i < grid2p.childCount; i++)
                        {
                            grid2p.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                            grid2p.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                            grid2p.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                            grid2p.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                            grid2p.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                        }
                    }*/

                    if (selectServo != null)
                    {
                        selectServo.enabled = true;
                    }

                    else
                    {
                        Transform grid2p = bottomTrans.GetChild(1).GetChild(0);
                        if (grid2p != null)
                        {
                            for (int i = 0; i < grid2p.childCount; i++)
                            {
                                grid2p.GetChild(i).Find("num").GetComponent<UISprite>().enabled = false;
                                grid2p.GetChild(i).Find("selectState").GetComponent<UISprite>().enabled = false;
                            }
                        }
                    }
                }
                settingUI2.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
            }
        }
        else if (name.Contains("type3"))  //三轮模式
        {
            ShowSetting_2();
            if (joyData.type != JockstickData.JockType.treeServo)
            {
                joyData.type = JockstickData.JockType.treeServo;
                joyData.leftUpID = 0;
                joyData.rightUpID = 0;
                joyData.UpID = 0;
                joyData.leftBottomID = 0;
                joyData.rightBottomID = 0;
                isChange = true;
            }
            settingUI2.transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
        }
        else if (name.Contains("wheel1"))   // 前左 点击后记录当前设置的轮子，同时背景选中状态
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "wheel1 is selected!!");

            SetWheelID = 1;
            selectWheelID = 1;
            Details.enabled = true;
            Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "1");
            curSelectWheel = obj.transform;
            curSelectWheel.Find("selected").GetComponent<UISprite>().enabled = true;
        }
        else if (name.Contains("wheel2"))  //前右
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "wheel2 is selected!!");

            SetWheelID = 2;
            selectWheelID = 2;
            Details.enabled = true;
            Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "2");
            curSelectWheel = obj.transform;
            curSelectWheel.Find("selected").GetComponent<UISprite>().enabled = true;
        }
        else if (name.Contains("wheel3"))   //后左 或第三个轮子
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "wheel3 is selected!!");

            SetWheelID = 3;
            selectWheelID = 3;
            Details.enabled = true;
            Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "3");
            curSelectWheel = obj.transform;
            curSelectWheel.Find("selected").GetComponent<UISprite>().enabled = true;
        }
        else if (name.Contains("wheel4"))   //后右
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "wheel4 is selected!!");

            SetWheelID = 4;
            selectWheelID = 4;
            Details.enabled = true;
            Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "4");
            curSelectWheel = obj.transform;
            curSelectWheel.Find("selected").GetComponent<UISprite>().enabled = true;
        }
        else if (name.StartsWith("servo_") || name.StartsWith("motor_"))  //选择对应的舵机 servo_num 
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "select this turn servo!!");
            CtrlMotionType motionType = CtrlMotionType.servo;
            if (name.StartsWith("motor_"))
            {
                motionType = CtrlMotionType.motor;
            }
            selectServoID = int.Parse(name.Substring(6));
            int motorCount = 0;
            if (null != RobotManager.GetInst().GetCurrentRobot().MotorsData)
            {
                motorCount += RobotManager.GetInst().GetCurrentRobot().MotorsData.Count;
            }
            int turnCount = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count;
            int angleCount = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList().Count;
            if (motionType == CtrlMotionType.motor && motorCount < wheelType)
            {//选择了马达，且马达数量不够
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("马达数量不足"), 1, CommonTipsColor.red);
                return;
            } else if (motionType == CtrlMotionType.servo)
            {
                if (turnCount + angleCount < wheelType)
                {//舵机不够
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("舵机数量不足"), 1, CommonTipsColor.red);
                    return;
                } else if (turnCount < wheelType)
                {
                    if (turnCount == 0)
                    {
                        UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置轮模式提示"), ModifyServoTypeSetting, true);
                    }
                    else
                    {
                        UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("配置摇杆轮模式提示"), ModifyServoTypeSetting, true);
                    }
                    return;
                } else
                {
                    DuoJiData servoData = RobotManager.GetInst().GetCurrentRobot().GetAnDjData(selectServoID);
                    if (null != servoData && servoData.modelType == ServoModel.Servo_Model_Angle)
                    {
                        UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("当前舵机无法360度旋转，需要设置吗"), ModifyServoTypeSetting, true);
                        return;
                    }
                }
            }
            if (null != joyData && joyData.motionType != motionType)
            {//切换了模型
                joyData.motionType = motionType;
                joyData.leftUpID = 0;
                joyData.rightUpID = 0;
                joyData.leftBottomID = 0;
                joyData.rightBottomID = 0;
                joyData.UpID = 0;
                Transform wheelTrans = null;
                if (wheelType == 2)
                {
                    wheelTrans = settingUI2.transform.Find("Center/twoServo");
                } else if (wheelType == 4)
                {
                    wheelTrans = settingUI2.transform.Find("Center/fourServo");
                }
                if (null != wheelTrans)
                {
                    for (int i = 0, imax = wheelTrans.childCount; i < imax; ++i)
                    {
                        HideServo(wheelTrans.GetChild(i).Find("servo"));
                    }
                }
            }
            UILabel text = obj.GetComponentInChildren<UILabel>();
            if (obj.transform.Find("num").GetComponent<UISprite>().enabled)
            {
                curSelectServo = true;
                //obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
            }
            else
            {
                curSelectServo = false;
            }

            if (curSelectServo)
            {
                curSelectServo = false;
                if (joyData.type == JockstickData.JockType.twoServo)
                {
                    tra = settingUI2.transform.Find("Center/twoServo");
                }
                else if (joyData.type == JockstickData.JockType.fourServo)
                {
                    tra = settingUI2.transform.Find("Center/fourServo");
                }
                if (joyData.leftUpID != 0 && joyData.leftUpID == selectServoID)
                {
                    HideServo(tra.GetChild(0).Find("servo"));
                    joyData.leftUpID = 0;
                }
                else if (joyData.rightUpID != 0 && joyData.rightUpID == selectServoID)
                {
                    HideServo(tra.GetChild(1).Find("servo"));
                    joyData.rightUpID = 0;
                }
                else if (joyData.leftBottomID != 0 && joyData.leftBottomID == selectServoID)
                {
                    HideServo(tra.GetChild(2).Find("servo"));
                    joyData.leftBottomID = 0;
                }
                else if (joyData.rightBottomID != 0 && joyData.rightBottomID == selectServoID)
                {
                    HideServo(tra.GetChild(3).Find("servo"));
                    joyData.rightBottomID = 0;
                }
            }
            selectServo = obj.transform.Find("num").GetComponent<UISprite>();
            selectServo.spriteName = selectWheelID.ToString();

            SetWheelServoID(selectServoID, motionType);
            if (selectServo != null)  //选中模式
            {
                selectServo.enabled = true;
            }

            ShowServosState();
            
            //selectServo = obj.transform.GetChild(2).GetComponent<UISprite>();            
        }
    }

    /// <summary>
    /// 修改舵机类型界面
    /// </summary>
    /// <param name="go"></param>
    public void ModifyServoTypeSetting(GameObject obj)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Modify servo type!!");

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
    /// 丢弃修改
    /// </summary>
    /// <param name="obj"></param>
    void AbandonSetting(GameObject obj)
    {
        try
        {
            if (obj == null) //没有改动时直接退出
            {
                mTrans.gameObject.SetActive(false);
                UserdefControllerScene.Ins.CloseJoystickSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.joystick_w);
                return;
            }
            string name = obj.name;
            if (name.Equals(PromptMsg.RightBtnName)) //确定丢弃修改
            {
                mTrans.gameObject.SetActive(false);
                UserdefControllerScene.Ins.CloseJoystickSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.joystick_w);
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
    }
    /// <summary>
    /// 通过结构和轮子设定对应的舵机id
    /// </summary>
    /// <param name="id"></param>
    void SetWheelServoID(int id, CtrlMotionType motionType)
    {
        if (wheelID == 1)
        {
            if (joyData.leftUpID != (byte)id)
            {
                isChange = true;
                joyData.leftUpID = (byte)id;
            }
        }
        else if (wheelID == 2)
        {
            if (joyData.rightUpID != (byte)id)
            {
                isChange = true;
                joyData.rightUpID = (byte)id;
            }
        }
        else if (wheelID == 3 && joyData.type == JockstickData.JockType.fourServo)
        {
            if (joyData.leftBottomID != (byte)id)
            {
                isChange = true;
                joyData.leftBottomID = (byte)id;
            }
        }
        else if (wheelID == 3 && joyData.type == JockstickData.JockType.treeServo)
        {
            if(joyData.UpID != (byte)id)
            {
                isChange = true;
                joyData.UpID = (byte)id;
            }
        }
        else if (wheelID == 4)
        {
            if (joyData.rightBottomID != (byte)id)
            {
                isChange = true;
                joyData.rightBottomID = (byte)id;
            }
        }
        if (curSelectWheel != null)
        {
            SetServo(curSelectWheel.Find("servo"), id, motionType);
        }
    }
    void ShowSetting_1()
    {
        for (int i = 0; i < settingUI2.transform.GetChild(1).childCount; i++)
        {
            settingUI2.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
        }
        settingUI2.SetActive(false);
        settingUI1.SetActive(true);
        Title.text = LauguageTool.GetIns().GetText("选择车子类型");
        settingUI1.transform.GetChild(1).GetChild(0).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("two-wheels");
        settingUI1.transform.GetChild(1).GetChild(1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("four-wheels");
    }
    void ShowSetting_2()
    {
        settingUI2.SetActive(true);
        settingUI1.SetActive(false);
        Title.text = LauguageTool.GetIns().GetText("设置轮子主标题");

        selectWheelID = 1;

        if (wheelType == 2)
        {
            Transform wheelOne = settingUI2.transform.GetChild(1).GetChild(0).GetChild(0);
            SetWheelID = 1;
            curSelectWheel = wheelOne;
            curSelectWheel.Find("selected").GetComponent<UISprite>().enabled = true;
        }
        else if (wheelType == 4)
        {
            Transform wheelTwo = settingUI2.transform.GetChild(1).GetChild(1).GetChild(0);
            SetWheelID = 1;
            curSelectWheel = wheelTwo;
            curSelectWheel.Find("selected").GetComponent<UISprite>().enabled = true;
        }
        
        ShowDuojiList();
        UpdateServoList();
        ShowServosState(); // 对应的状态
        Details.enabled = true;
        Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "1");
    }

    void ChangeServoModel(EventArg arg)
    {
        try
        {
            if (null != settingUI2 && settingUI2.activeSelf)
            {
                UpdateServoList();
                ShowServosState();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
