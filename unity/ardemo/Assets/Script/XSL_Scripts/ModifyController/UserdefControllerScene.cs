using UnityEngine;
using Game.Scene;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;

public class UserdefControllerScene : BaseScene {

    public static UserdefControllerScene Ins;
    static FromModuleType sFromModule;
    public FromModuleType FromModule
    {
        get { return sFromModule; }
    }
    //public static int leftSpace = 20;  //左边距
    //public static int rightSpace = 20; //右边距
    //public static int upSpace = 30;    //上边距 
    //public static int downSpace = 30;  //下边距 
    static string controllerName;
    string controllerUIpath;  //ui场景路径
    private UserdefControllerUI controllerUI;
    private UserCotrollerSettingUI setUI;
    private JoystickSettingUI joySettingUI;
    //private VsliderSettingUI vsliderSettingUI;
    //private HsliderSettingUI hsliderSettingUI;
    private SliderWidgetData vsliderServoID;
    private int hsliderServoID;
    private string selectWidgetID;
    
    public UserdefControllerScene()
    {
        mUIList = new System.Collections.Generic.List<BaseUI>();
        Ins = this;
    }

    public enum curControlT
    {
        joystick_w,
        vslider_sw,
        vslider_cw,
        hslider_sw,
        hslider_cw,
    }
    
    public static void GotoController(FromModuleType fromModule)
    {
        sFromModule = fromModule;
        SceneMgr.EnterScene(SceneType.ActionPlay, typeof(UserdefControllerScene));
    }

    public static void GoBack()
    {
        if (sFromModule == FromModuleType.From_App_Home)
        {
            PlatformMgr.Instance.BackThirdApp();
        } else
        {
            MainScene.GotoLastScene();
        }
    }

    /// <summary>
    /// 创建新遥控
    /// </summary>
    /// <param name="name"></param>
    public void CreateNewController(string name)
    {
        if (setUI != null)
            mUIList.Remove(setUI);
        if (controllerUI == null)
        {
            //Debug.Log("isFirstSetting is true");
            controllerUI = new UserdefControllerUI(true);
          //  controllerUI.IsSetting = true;
            controllerUI.isFirstSetting = true;
            mUIList.Add(controllerUI);
            
         //   controllerUI.IsSetting = true;
        }
        UpdateScene();
    }
    /// <summary>
    /// 打开摇杆设置页面
    /// </summary>
    /// <param name="id"></param>
    public void OpenJoystickSettingUI(string id)
    {
        joySettingUI = new JoystickSettingUI(id);
        if (joySettingUI.JoyData.type == JockstickData.JockType.none) //widget 为null
        {
            joySettingUI.isFirstSetting = true;
        }
        else  //widget不为null
        {
            joySettingUI.isFirstSetting = false;
        }
        mUIList.Add(joySettingUI);
        controllerUI.OnHide();
        UpdateScene();
    }
    /// <summary>
    /// 关闭摇杆设置页面
    /// </summary>
    public void CloseJoystickSettingUI()
    {
        mUIList.Remove(joySettingUI);
        joySettingUI.OnClose();
    }
    /// <summary>
    /// 打开滑杆设置页面
    /// </summary>
    /// <param name="id"></param>
    public void OpenVsliderSettingUI(string id)
    {
        /*vsliderSettingUI = new VsliderSettingUI(id);
        mUIList.Add(vsliderSettingUI);
        controllerUI.OnHide();
        UpdateScene();*/
        VsliderSettingUI.ShowMsg(id);
    }
    /// <summary>
    /// 关闭滑杆设置页面
    /// </summary>
    public void CloseVsliderSettingUI()
    {
        selectWidgetID = VsliderSettingUI.sliderData.widgetId;
        vsliderServoID = VsliderSettingUI.sliderData;
        /*mUIList.Remove(vsliderSettingUI);
        vsliderSettingUI.OnClose();*/
    }
    /// <summary>
    /// 打开横杆设置页面
    /// </summary>
    /// <param name="id"></param>
    public void OpenHsliderSettingUI(string id)
    {
        HsliderSettingUI.ShowMsg(id);
        //controllerUI.OnHide();
        //UpdateScene();
    }
    /// <summary>
    /// 关闭横杆设置页面
    /// </summary>
    public void CloseHsliderSettingUI()
    {
        selectWidgetID = HsliderSettingUI.hsliderData.widgetId;
        /*if (hsliderSettingUI.isSelectOtherServo)
            hsliderServoID = hsliderSettingUI.hsliderData.servoID;
        else
            hsliderServoID = 0;*/
        hsliderServoID = HsliderSettingUI.hsliderData.servoID;
        //mUIList.Remove(hsliderSettingUI);
        //hsliderSettingUI.OnClose();
    }

    public void BackControllerSettingUI(curControlT controlT)
    {
        if (controllerUI != null)
            controllerUI.OnShow();
        //Debug.Log("ReEnter is Controller");
        Transform gridPanelC = GameObject.Find("userdefineControllerUI/Center/gridPanel").transform;

        if (controlT == curControlT.vslider_sw || controlT == curControlT.hslider_sw)
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("配置成功提示"), 1, CommonTipsColor.green);

        // 竖杆操作
        if (gridPanelC != null && gridPanelC.childCount > 1 && controlT == curControlT.vslider_sw)
        {
            for (int i = 1; i < gridPanelC.childCount; i++)
            {
                if (gridPanelC.GetChild(i).tag.Contains("widget_vslider") && gridPanelC.GetChild(i).name == selectWidgetID)
                {
                    if (null != vsliderServoID)
                    {
                        if (vsliderServoID.motionType == CtrlMotionType.servo)
                        {
                            gridPanelC.GetChild(i).GetChild(4).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("舵机") + " " + vsliderServoID.servoID;
                        }
                        else if (vsliderServoID.motionType == CtrlMotionType.motor)
                        {
                            gridPanelC.GetChild(i).GetChild(4).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("马达") + " " + vsliderServoID.servoID;
                        }
                        if (((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(i).name)).directionDisclock)
                        {
                            gridPanelC.GetChild(i).GetChild(5).localEulerAngles = new Vector3(0, 180, 0);
                            gridPanelC.GetChild(i).GetChild(6).localEulerAngles = Vector3.zero;
                        }
                        else
                        {
                            gridPanelC.GetChild(i).GetChild(6).localEulerAngles = new Vector3(0, 180, 0);
                            gridPanelC.GetChild(i).GetChild(5).localEulerAngles = Vector3.zero;
                        }
                        for (int j = 1; j < gridPanelC.childCount; j++)
                        {
                            if (gridPanelC.GetChild(j).tag.Contains("widget_vslider") && gridPanelC.GetChild(j).name != selectWidgetID)
                            {
                                SliderWidgetData tmp = (SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name);
                                if (null != tmp && tmp.motionType == vsliderServoID.motionType && tmp.servoID == vsliderServoID.servoID)
                                {
                                    gridPanelC.GetChild(j).GetChild(4).GetComponent<UILabel>().text = "";
                                    tmp.servoID = 0;
                                    tmp.motionType = CtrlMotionType.servo;
                                    tmp.directionDisclock = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        // 横杆操作
        if (gridPanelC != null && gridPanelC.childCount > 1 && HsliderSettingUI.hsliderData != null && controlT == curControlT.hslider_sw)
        {
            for (int i = 1; i < gridPanelC.childCount; i++)
            {
                if (gridPanelC.GetChild(i).tag.Contains("widget_hslider") && gridPanelC.GetChild(i).name == selectWidgetID)
                {
                    gridPanelC.GetChild(i).GetChild(1).GetChild(0).transform.localPosition = Vector3.zero;
                    gridPanelC.GetChild(i).GetChild(4).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("舵机") + " " + hsliderServoID.ToString();
                    gridPanelC.GetChild(i).GetChild(5).GetComponent<UILabel>().text = HsliderSettingUI.hsliderData.min_angle.ToString() + "°";
                    gridPanelC.GetChild(i).GetChild(6).GetComponent<UILabel>().text = HsliderSettingUI.hsliderData.max_angle.ToString() + "°";
                    for (int j = 1; j < gridPanelC.childCount; j++)
                    {
                        if (gridPanelC.GetChild(j).tag.Contains("widget_hslider") && gridPanelC.GetChild(j).name != selectWidgetID && gridPanelC.GetChild(j).GetChild(4).GetComponent<UILabel>().text == gridPanelC.GetChild(i).GetChild(4).GetComponent<UILabel>().text)
                        {
                            gridPanelC.GetChild(j).GetChild(4).GetComponent<UILabel>().text = "";
                            gridPanelC.GetChild(j).GetChild(1).GetChild(0).transform.localPosition = Vector3.zero;
                            ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).servoID = 0;
                            ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).min_angle = -118;
                            ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).max_angle = 118;
                            gridPanelC.GetChild(j).GetChild(5).GetComponent<UILabel>().text = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).min_angle.ToString() + "°";
                            gridPanelC.GetChild(j).GetChild(6).GetComponent<UILabel>().text = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).max_angle.ToString() + "°";
                        }
                    }
                }
            }
        }
        UpdateScene();
    }

    /// <summary>
    /// 场景更新
    /// </summary>
    public override void UpdateScene()
    {
        base.UpdateScene();
        //if(controllerUI != null)
        //    controllerUI.Open();
        foreach (var tem in mUIList)
        {
            tem.Open();
        }
    }
    public override void Open()
    {
        //mUIList.Clear();
        if (ControllerManager.IsControllersNull(RobotManager.GetInst().GetCurrentRobot().ID) && PlatformMgr.Instance.EditFlag) //没有遥控的时候 设置界面
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Open Setting");
            //CreateNewController("");  //创建一个没命名的遥控
            setUI = new UserCotrollerSettingUI();
            if (setUI != null)
            {
                mUIList.Add(setUI);
            }   
        }
        else //进入遥控列表界面
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Restart Setting");
            controllerUI = new UserdefControllerUI(false);
            if (controllerUI != null)
            {
                mUIList.Add(controllerUI);
            }            
        }
        base.Open();
        // ControllerManager.WriteControllerByID();
    }
    public override void Close()
    {
        base.Close();
        //释放遥控数据 同时停止所有可能的动作
        ControllerManager.GetInst().CleanUp();
    }

    #region  public static
    /// <summary>
    /// 初始化舵机列表（摇杆）
    /// </summary>
    public static void InitServoList(Transform p, ButtonDelegate.OnClick call, int wheelType)
    {
        GameObject obj = Resources.Load<GameObject>("prefabs/servo_");
        List<byte> turnList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList();
        List<byte> engelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList();
        if (turnList.Count > 0)
        {
            for (int i = 0; i < turnList.Count; i++)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.name = "servo_" + turnList[i];
                oo.transform.SetParent(p);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = Vector3.zero;
                oo.transform.Find("num").GetComponent<UISprite>().enabled = false;
                oo.transform.Find("selectState").GetComponent<UISprite>().enabled = false;
                if (turnList.Count < wheelType)
                {
                    oo.GetComponent<UISprite>().spriteName = "servo_angle_upgrade";
                }
                oo.GetComponentInChildren<UILabel>().text = PublicFunction.ID_Format + turnList[i].ToString().PadLeft(2, '0');//djl[i].ToString();
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }
        if (engelList.Count > 0)
        {
            //Debug.Log("角度列表非空");
            for (int i = 0; i < engelList.Count; i++)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.name = "servo_" + engelList[i];
                oo.transform.SetParent(p);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = Vector3.zero;
                oo.transform.Find("num").GetComponent<UISprite>().enabled = false;
                oo.transform.Find("selectState").GetComponent<UISprite>().enabled = false;
                oo.GetComponentInChildren<UILabel>().text = PublicFunction.ID_Format + engelList[i].ToString().PadLeft(2, '0');//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servo_angle_upgrade";
                Transform turn = oo.transform.Find("turn");
                if (null != turn)
                {
                    turn.gameObject.SetActive(false);
                }
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "Clean";
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }
        MotorData motorData = RobotManager.GetInst().GetCurrentRobot().MotorsData;
        if (null != motorData && motorData.Count > 0)
        {
            for (int i = 0, imax = motorData.Count; i < imax; ++i)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.name = "motor_" + motorData.Get(i);
                SetItem(oo, motorData.Get(i), CtrlMotionType.motor, p);
                if (motorData.Count < wheelType)
                {
                    oo.GetComponent<UISprite>().spriteName = "motor_upgrade";
                }
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }
        p.GetComponent<UIGrid>().repositionNow = true;
    }
    /// <summary>
    /// 初始化舵机列表（竖杆）
    /// </summary>
    public static void InitServoListV(Transform p, SliderWidgetData curServo, ButtonDelegate.OnClick call = null)
    {
        GameObject obj = Resources.Load<GameObject>("prefabs/servo_");
        
        List<byte> turnList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList();
        //List<byte> engelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList();

        /*if (engelList.Count > 0)
        {
            for (int i = 0; i < engelList.Count; i++)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.transform.SetParent(p);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = Vector3.zero;
                oo.GetComponentInChildren<UILabel>().text = "Servo " + engelList[i].ToString();//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servoAngleNo";
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_jdN3x";
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = null;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
             }
        }*/
        

        if (turnList.Count > 0)
        {
            for (int i = 0; i < turnList.Count; i++)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.name = "servo_" + turnList[i];
                if (null != curServo)
                {
                    SetItem(oo, turnList[i], CtrlMotionType.servo, p,  curServo.servoID, curServo.motionType, ServoModel.Servo_Model_Turn);
                } else
                {
                    SetItem(oo, turnList[i], CtrlMotionType.servo, p, 0, CtrlMotionType.servo, ServoModel.Servo_Model_Turn);
                }
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }
        MotorData motorData = RobotManager.GetInst().GetCurrentRobot().MotorsData;
        if (null != motorData && motorData.Count > 0)
        {
            for (int i = 0, imax = motorData.Count; i < imax; ++i)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.name = "motor_" + motorData.Get(i);
                if (null != curServo)
                {
                    SetItem(oo, motorData.Get(i), CtrlMotionType.motor, p, curServo.servoID, curServo.motionType);
                } else
                {
                    SetItem(oo, motorData.Get(i), CtrlMotionType.motor, p);
                }
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }
        p.GetComponent<UIGrid>().repositionNow = true;
    }

    static void SetItem(GameObject item, int id, CtrlMotionType type, Transform parent, int curId = 0, CtrlMotionType curType = CtrlMotionType.servo, ServoModel servoModel = ServoModel.Servo_Model_Angle)
    {
        Transform trans = item.transform;
        trans.SetParent(parent);
        trans.localScale = Vector3.one;
        trans.localPosition = Vector3.zero;
        
        if (type == CtrlMotionType.servo)
        {
            item.GetComponent<UISprite>().spriteName = "servo_angle";
            trans.Find("text").GetComponent<UILabel>().text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
            if (servoModel == ServoModel.Servo_Model_Angle)
            {
                trans.Find("turn").gameObject.SetActive(false);
            } else
            {
                trans.Find("turn").gameObject.SetActive(true);
            }
        }
        else if (type == CtrlMotionType.motor)
        {
            item.GetComponent<UISprite>().spriteName = "motor";
            trans.Find("text").gameObject.SetActive(false);
            trans.Find("motorid").gameObject.SetActive(true);
            trans.Find("motorid").GetComponent<UILabel>().text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
            trans.Find("turn").gameObject.SetActive(false);
        }
        trans.Find("num").GetComponent<UISprite>().enabled = false;
        if (0 != curId && curType == type && id == curId)
        {
            trans.Find("selectState").GetComponent<UISprite>().enabled = true;
        }
        else
        {
            trans.Find("selectState").GetComponent<UISprite>().enabled = false;
        }


    }
    /// <summary>
    /// 初始化舵机列表（横杆）
    /// </summary>
    public static void InitServoListH(Transform p, int curServo, ButtonDelegate.OnClick call = null)
    {
        GameObject obj = Resources.Load<GameObject>("prefabs/servo_");
        List<byte> turnList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList();
        List<byte> engelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList();

        if (engelList.Count > 0)
        {
            //Debug.Log("角度列表非空");
            for (int i = 0; i < engelList.Count; i++)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.name = "servo_" + engelList[i];
                SetItem(oo, engelList[i], CtrlMotionType.servo, p, curServo, CtrlMotionType.servo, ServoModel.Servo_Model_Angle);
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }
        /*if (turnList.Count > 0)
        {
            //Debug.Log("轮列表非空");
            for (int i = 0; i < turnList.Count; i++)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.transform.SetParent(p);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = Vector3.zero;
                oo.GetComponentInChildren<UILabel>().text = "Servo " + turnList[i].ToString();//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servoTurnNo";
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_lN3x";
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = null;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }*/
        p.GetComponent<UIGrid>().repositionNow = true;
    }
    /// <summary>
    /// 确认取消的提示框 如果数据未发生改变时 不弹出提示框
    /// </summary>
    /// <param name="str"></param>
    /// <param name="onclick"></param>
    public static void PopWin(string str, ButtonDelegate.OnClick onclick,bool isChange)
    {
        if (isChange)
        {
            PublicPrompt.ShowDelateWin(str, onclick);
        }
        else
        {
            onclick(null);
        }
    }
    #endregion
}


public enum FromModuleType : byte
{
    From_App_Home = 0,
    From_Build,
    From_Main_Scene,
}
