using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.IO;
using Game.Resource;
using Game.Platform;

/// <summary>
/// author:   孙宇
/// describe: 遥控数据管理  controllerData负责控件的数据 widgetManager负责控件的逻辑 
/// </summary>
public class ControllerManager : SingletonObject<ControllerManager>
{
    //private string modeType = "/default/";
    private static string ControllerPath = Application.persistentDataPath + "/default/" + RobotManager.GetInst().GetCurrentRobot().Name.Substring(0, RobotManager.GetInst().GetCurrentRobot().Name.IndexOf("_")) + "/ControllerData"; //遥控数据存放路径
    private string ControllerID; //遥控名称
    private ControllerData preControllerData;  //配置前的遥控数据
    private ControllerData curController; //当前遥控数据
    public string ControllerName;  //遥控名字
    private WidgetManager curWidgets;
    public WidgetManager widgetManager  //外部不准写入
    {
        get
        {
            return curWidgets;
        }
    }
    public ControllerManager()
    {
        try
        {
            if (RecordContactInfo.Instance.openType == "default")
                ControllerPath = ResourcesEx.GetRobotCommonPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
            else
                ControllerPath = ResourcesEx.GetRobotPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";

            if (!Directory.Exists(ControllerPath))
            {
                Directory.CreateDirectory(ControllerPath);
            }
            if (IsControllersNull(RobotManager.GetInst().GetCurrentRobot().ID))
            {
                CreateNewController();
            }
            else
            {
                try
                {
                    CreateExistController();
                }
                catch (System.Exception ex)
                {
                    CreateNewController();
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "读取遥控器数据失败 - error = " + ex.ToString());
                }
                
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "遥控器数据初始化失败 - error = " + ex.ToString());
        }
        
    }
    public override void CleanUp()
    {
        base.CleanUp();
        Destroy();
    }
    /// <summary>
    /// 动作是否已经配置过
    /// </summary>
    /// <param name="actionName"></param>
    /// <returns></returns>
    public bool IsActionExist(string actionId)
    {
        return curController.actionData.Find((x) => x.actionId == actionId) != null;
    }
    /// <summary>
    /// 通过控件id获取对应的控件
    /// </summary>
    /// <param name="widgetID"></param>
    /// <returns></returns>
    public object GetWidgetdataByID(string widgetID)
    {
        object oo = new object();
        oo = curController.GetActiondataByID(widgetID);
        if (oo != null)
            return oo;
        oo = curController.GetJockdataByID(widgetID);
        if (oo != null)
            return oo;
        oo = curController.GetSliderdataByID(widgetID);
        if (oo != null)
            return oo;
        oo = curController.GetHSliderdataByID(widgetID);
        return oo;
    }
    /// <summary>
    /// 是否有遥控
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsControllersNull(string controllerID)
    {
        if (RecordContactInfo.Instance.openType == "default")
            ControllerPath = ResourcesEx.GetRobotCommonPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
        else
            ControllerPath = ResourcesEx.GetRobotPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
        controllerID = ControllerPath + "/" + controllerID;
        return !File.Exists(controllerID);
    }

    /// <summary>
    /// 新建遥控
    /// </summary>
    public void CreateNewController()
    {
        if (null != RobotManager.GetInst().GetCurrentRobot())
        {
            ControllerID = RobotManager.GetInst().GetCurrentRobot().ID;
            curController = new ControllerData(ControllerID);
            preControllerData = new ControllerData(ControllerID);
            curWidgets = new WidgetManager(curController);
        }
        
        
    }
    public void CreateExistController()
    { 
        ControllerID = RobotManager.GetInst().GetCurrentRobot().ID;
        curController = GetControllerByName(ControllerID);
        preControllerData = GetControllerByName(ControllerID);
        curWidgets = new WidgetManager(curController);
    }
    /// <summary>
    /// 删除遥控数据
    /// </summary>
    public static void DeletaController(string controllerID)
    {
        if (RecordContactInfo.Instance.openType == "default")
            ControllerPath = ResourcesEx.GetRobotCommonPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
        else
            ControllerPath = ResourcesEx.GetRobotPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
        string name = ControllerPath + "/" + controllerID;
        if (File.Exists(name))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Delete controller data file!!");

            File.Delete(name);
            PlatformMgr.Instance.OperateSyncFile(RobotManager.GetInst().GetCurrentRobot().Name, name, OperateFileType.Operate_File_Del);
        }
    }
    /// <summary>
    /// 新建摇杆 widgetID必须唯一
    /// </summary>
    public void NewJoystick(string widgetID, Vector2 pos)
    {
        JockstickData data = new JockstickData(widgetID, pos);
        //Debuger.Log(widgetID);
        curController.AddNewJoystickData(data);
        curWidgets.AddJoystickControl(data);
    }
    public void RemoveJoystick(string widgetID)
    {
        curController.RemoveJoystickData(widgetID);
        curWidgets.RemoveJoystickControl(widgetID);
    }
    /// <summary>
    /// 新建滑杆
    /// </summary>
    /// <param name="widgetID"></param>
    /// <param name="pos"></param>
    public void NewSliderBar(string widgetID, Vector2 pos)
    {
        SliderWidgetData data = new SliderWidgetData(widgetID, pos);
        curController.AddNewSliderData(data);
        curWidgets.AddVsliderControl(data);
    }
    public void RemoveSliderBar(string widgetID)
    {
        curController.RemoveSliderData(widgetID);
        curWidgets.RemoveVsliderControl(widgetID);
    }
    /// <summary>
    /// 新建横杆
    /// </summary>
    /// <param name="widgetID"></param>
    /// <param name="pos"></param>
    public void NewHSliderBar(string widgetID, Vector2 pos)
    {
        HSliderWidgetData data = new HSliderWidgetData(widgetID, pos);
        curController.AddNewHSliderData(data);
        curWidgets.AddHsliderControl(data);
    }
    public void RemoveHSliderBar(string widgetID)
    {
        curController.RemoveHSliderData(widgetID);
        curWidgets.RemoveHsliderControl(widgetID);
    }
    /// <summary>
    /// 新建动作
    /// </summary>
    /// <param name="widgetID"></param>
    /// <param name="pos"></param>
    public void NewAction(string widgetID, Vector2 pos)
    {
        ActionWidgetData data = new ActionWidgetData(widgetID, pos);
        curController.AddNewActionData(data);
        curWidgets.AddActionControl(data);
    }
    public void RemoveAction(string widgetID)
    {
        curController.RemoveActionData(widgetID);
        curWidgets.RemoveActionControl(widgetID);
    }
    /// <summary>
    /// 反序列化读取数据
    /// </summary>
    /// <param name="name"></param>
    public static ControllerData GetControllerByName(string name)
    {
        if (RecordContactInfo.Instance.openType == "default")
            ControllerPath = ResourcesEx.GetRobotCommonPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
        else
            ControllerPath = ResourcesEx.GetRobotPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
        name = ControllerPath + "/" + name;
        ControllerData data = new ControllerData();
        data = MyMVC.XmlHelper.XmlDeserializeFromFile<ControllerData>(name, System.Text.Encoding.UTF8);
        return data;
    }
    /// <summary>
    /// 序列化写入数据
    /// </summary>
    /// <param name="id"></param>
    public static void WriteControllerByID(ControllerData data)
    {
        if (RecordContactInfo.Instance.openType == "default")
            ControllerPath = ResourcesEx.GetRobotCommonPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
        else
            ControllerPath = ResourcesEx.GetRobotPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";

        string str = MyMVC.XmlHelper.XmlSerialize(data, System.Text.Encoding.UTF8);
        FileStream fs = new FileStream(ControllerPath + "/"+data.controllerID, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);
        fs.SetLength(0);//首先把文件清空了。
        sw.Write(str);//写你的字符串。
        sw.Close();
        fs.Close();
    }
    /// <summary>
    /// 保存遥控数据
    /// </summary>
    public void SaveCurController()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Save current controller data!!");

        curController.ScreenWidth = PublicFunction.GetWidth();
        curController.ScreenHeight = PublicFunction.GetHeight();
        curController.controllerID = RobotManager.GetInst().GetCurrentRobot().ID;
        if (RecordContactInfo.Instance.openType == "default")
            ControllerPath = ResourcesEx.GetRobotCommonPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";
        else
            ControllerPath = ResourcesEx.GetRobotPath(RobotManager.GetInst().GetCurrentRobot().Name) + "/ControllerData";

        string str = MyMVC.XmlHelper.XmlSerialize(curController, System.Text.Encoding.UTF8);
        string path = ControllerPath + "/" + RobotManager.GetInst().GetCurrentRobot().ID;
        bool addFileFlag = true;
        if (File.Exists(path))
        {
            addFileFlag = false;
        }
        File.WriteAllText(path, str);
        /*FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);
        fs.SetLength(0);//首先把文件清空了。
        sw.Write(str);//写你的字符串。
        sw.Flush();
        fs.Flush();
        sw.Close();
        fs.Close();
        sw.Dispose();
        fs.Dispose();*/
        if (ResourcesEx.GetRobotType(RobotManager.GetInst().GetCurrentRobot()) == ResFileType.Type_playerdata)
        {
            if (addFileFlag)
            {
                PlatformMgr.Instance.OperateSyncFile(RobotManager.GetInst().GetCurrentRobot().Name, path, OperateFileType.Operate_File_Add);
            }
            else
            {
                PlatformMgr.Instance.OperateSyncFile(RobotManager.GetInst().GetCurrentRobot().Name, path, OperateFileType.Operate_File_Change);
            }
        }
        //深拷贝
        string strr = MyMVC.XmlHelper.XmlSerialize(curController, System.Text.Encoding.UTF8);
        preControllerData = MyMVC.XmlHelper.XmlDeserialize<ControllerData>(strr, System.Text.Encoding.UTF8);

        //Debug.Log("now servoID is " + curController.GetHSliderList()[0].servoID);
        //Debug.Log("now min_angle is " + curController.GetHSliderList()[0].min_angle);
        //Debug.Log("now max_angle is " + curController.GetHSliderList()[0].max_angle);
    }
    /// <summary>
    /// 取消遥控修改
    /// </summary>
    public void CancelCurController()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Cancel change current controller data!!");

        //深拷贝
        string str = MyMVC.XmlHelper.XmlSerialize(preControllerData, System.Text.Encoding.UTF8);
        curController = MyMVC.XmlHelper.XmlDeserialize<ControllerData>(str, System.Text.Encoding.UTF8);
        curWidgets = new WidgetManager(curController); // 控件和数据更新
    }
    /// <summary>
    /// 获取当前遥控数据
    /// </summary>
    /// <returns></returns>
    public ControllerData GetCurControllerData()
    {
        return curController;
    }
    /// <summary>
    /// 转换遥控数据 可显示的数据结构
    /// </summary>
    /// <returns></returns>
    public List<WidgetShowType> TurnShowTypeList()
    {
        if (null == curController)
        {
            return null;
        }
        List<WidgetShowType> list = new List<WidgetShowType>();

        if (curController.actionData != null)
        {
            
            for (int i = 0; i < curController.actionData.Count; i++)
            {
                //Debug.Log("Now action is " + curController.actionData[i].widgetID);
                list.Add(new WidgetShowType(curController.actionData[i].widgetID, WidgetShowType.widgetType.action, curController.actionData[i].localPos));
            }
        }
        if (curController.sliderData != null)
        {
            for (int i = 0; i < curController.sliderData.Count; i++)
            {
                list.Add(new WidgetShowType(curController.sliderData[i].widgetId, WidgetShowType.widgetType.vSlider, curController.sliderData[i].localPos));
            }
        }
        if (curController.hsliderData != null)
        {
            for (int i = 0; i < curController.hsliderData.Count; i++)
            {
                list.Add(new WidgetShowType(curController.hsliderData[i].widgetId, WidgetShowType.widgetType.hSlider, curController.hsliderData[i].localPos));
            }
        }
        if (curController.jockstickData != null)
        {
            for (int i = 0; i < curController.jockstickData.Count; i++)
            {
                list.Add(new WidgetShowType(curController.jockstickData[i].widgetId, WidgetShowType.widgetType.joystick, curController.jockstickData[i].localPos));
                //Debug.Log(curController.jockstickData[i].localPos.x + " " + curController.jockstickData[i].localPos.y);
            }
        }
		if (list.Count == 0)
			return null;
        return list;
    }
    
    public JockstickData GetJockstickData()
    {
        if (null != curController && null != curController.jockstickData && curController.jockstickData.Count > 0)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, string.Format("left font id = {0} back id = {1} right font id = {2} back id = {3}", curController.jockstickData[0].leftUpID, curController.jockstickData[0].leftBottomID, curController.jockstickData[0].rightUpID, curController.jockstickData[0].rightBottomID));
            return curController.jockstickData[0];
        }
        return null;
    }
    /// <summary>
    /// 控件就绪
    /// </summary>
    public void ControllerReady()
    {
        if (null == curWidgets)
        {
            return;
        }
        if (null != curWidgets.joyStickManager)
        {
            curWidgets.joyStickManager.ReadyJockControl();
        }
        if (null != curWidgets.vSliderManager)
        {
            curWidgets.vSliderManager.ReadyVsliderControl();
        }
        if (null != curWidgets.hSliderManager)
        {
            curWidgets.hSliderManager.ReadyHsliderControl();
        }
    }
    /// <summary>
    /// 控件的物理结构
    /// </summary>
    public class WidgetShowType
    {
        public string widgetID;
        public Vector2 pos;
        public widgetType type;
        public WidgetShowType(string wid, widgetType ty, Vector2 po)
        {
            this.widgetID = wid;
            this.type = ty;
            this.pos = po;
        }
        public enum widgetType
        { 
            vSlider,
            hSlider,
            joystick,
            action,
        }
    }

    /// <summary>
    ///获取配置分辨率
    /// </summary>
    /// <returns></returns>
    public int GetCurControllerSceneWidth()
    {
        return curController.ScreenWidth;
    }
    public int GetCurControllerSceneHeight()
    {
        return curController.ScreenHeight;
    }
}
/// <summary>
/// 遥控数据
/// </summary>
public class ControllerData
{
    [XmlAttribute]
    public string controllerID;
    [XmlAttribute]
    public int ScreenWidth;
    [XmlAttribute]
    public int ScreenHeight;
    [XmlElement]
    public List<ActionWidgetData> actionData; //动作控件数据
    [XmlElement]
    public List<SliderWidgetData> sliderData; //滑杆控件数据
    [XmlElement]
    public List<HSliderWidgetData> hsliderData; //横杆控件数据
    [XmlElement]
    public List<JockstickData> jockstickData; //摇杆控件数据

    public ControllerData()
    {
    }
    public ControllerData(string ID)
    {
        this.controllerID = ID;
        this.ScreenWidth = PublicFunction.GetWidth();
        this.ScreenHeight = PublicFunction.GetHeight();
        actionData = new List<ActionWidgetData>();
        sliderData = new List<SliderWidgetData>();
        hsliderData = new List<HSliderWidgetData>();
        jockstickData = new List<JockstickData>();
    }
    public void ClearUp()
    {
        actionData = new List<ActionWidgetData>();
        sliderData = new List<SliderWidgetData>();
        hsliderData = new List<HSliderWidgetData>();
        jockstickData = new List<JockstickData>();
    }
    
    /// <summary>
    ///获取控件个数
    /// </summary>
    /// <returns></returns>
    public int GetWidgetMount()
    {
        return actionData.Count + sliderData.Count + hsliderData.Count + jockstickData.Count;
    }
    /// <summary>
    /// 获取控件通过id
    /// </summary>
    /// <param name="ID"></param>
    public ActionWidgetData GetActiondataByID(string ID)
    {
        return actionData.Find((x) => x.widgetID == ID);
    }
    public SliderWidgetData GetSliderdataByID(string ID)
    {
        return sliderData.Find((x) => x.widgetId == ID);
    }
    public HSliderWidgetData GetHSliderdataByID(string ID)
    {
        return hsliderData.Find((x) => x.widgetId == ID);
    }
    public JockstickData GetJockdataByID(string ID)
    {
        return jockstickData.Find((x) => x.widgetId == ID);
    }
    public List<JockstickData> GetJockList()
    {
        return jockstickData;
    }
    public List<SliderWidgetData> GetSliderList()
    {
        return sliderData;
    }
    public List<HSliderWidgetData> GetHSliderList()
    {
        return hsliderData;
    }

    /// <summary>
    /// 新增或删除控件数据
    /// </summary>
    /// <param name="data"></param>
    public void AddNewJoystickData(JockstickData data)
    {
        jockstickData.Add(data);
    }
    public void AddNewActionData(ActionWidgetData data)
    {
        actionData.Add(data);
    }
    public void AddNewSliderData(SliderWidgetData data)
    {
        sliderData.Add(data);
    }
    public void AddNewHSliderData(HSliderWidgetData data)
    {
        hsliderData.Add(data);
    }
    public void RemoveJoystickData(string widgetID)
    {
        jockstickData.Remove(jockstickData.Find((x) => x.widgetId == widgetID));
    }
    public void RemoveSliderData(string widgetID)
    {
        sliderData.Remove(sliderData.Find((x) => x.widgetId == widgetID));
    }
    public void RemoveHSliderData(string widgetID)
    {
        hsliderData.Remove(hsliderData.Find((x) => x.widgetId == widgetID));
    }
    public void RemoveActionData(string widgetID)
    {
        actionData.Remove(actionData.Find((x) => x.widgetID == widgetID));
    }
}
/// <summary>
/// 动作控件数据
/// </summary>
public class ActionWidgetData
{
    [XmlAttribute]
    public string widgetID; //控件id
    [XmlElement]
    public string actionNm;
    [XmlElement]
    public string actionId;
    [XmlIgnore]
    public Vector2 localPos
    {
        get
        {
            return new Vector2(pos_x, pos_y);
        }
        set
        {
            pos_x = value.x;
            pos_y = value.y;
        }
    }
    [XmlElement]
    public float pos_x;
    [XmlElement]
    public float pos_y;
    public ActionWidgetData()
    { }
    public ActionWidgetData(string id,Vector2 pos)
    {
        this.widgetID = id;
        actionNm = "";
        actionId = "";
        localPos = pos;
    }
    public ActionWidgetData(string widgetid, string actionNm, string actionId, Vector3 pos)
    {
        this.widgetID = widgetid;
        this.actionNm = actionNm;
        this.actionId = actionId;
        this.localPos = pos;
    }
    public string GetActionName(string id)
    {
        if (id == "")
            return "";
        string actionNms = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionId).Name;
        //Debug.Log("Now action is " + actionNms);
        return actionNms;
        
    }
    public string GetActionId(string id)
    {
        if (id == "")
            return "";
        string actionIds = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionId).Id;
        //Debug.Log("Now action is " + actionIds);
        return actionIds;

    }
}

/// <summary>
/// 滑杆 控制一个舵机
/// </summary>
public class SliderWidgetData
{
    [XmlAttribute]
    public string widgetId;
    [XmlElement]
    public byte servoID;
    [XmlEnum]
    public CtrlMotionType motionType = CtrlMotionType.servo;
    [XmlEnum]
    public sliderType sType;
    [XmlIgnore]
    public Vector2 localPos
    {
        get
        {
            return new Vector2(pos_x, pos_y);
        }
        set
        {
            pos_x = value.x;
            pos_y = value.y;
        }
    }
    [XmlElement]
    public float pos_x;
    [XmlElement]
    public float pos_y;
    [XmlElement]
    public bool directionDisclock = false;
    [XmlIgnore]
    public bool isOK
    {
        get
        {
            return servoID != 0;
        }
    }
    //protected string 
    public SliderWidgetData()
    {
        servoID = 0;
    }
    public SliderWidgetData(string widgetID,Vector2 pos)
    {
        this.widgetId = widgetID;
        localPos = pos;
        servoID = 0;
        sType = sliderType.v;
    }
    public SliderWidgetData(string widetID, byte servoID, Vector3 pos)
    {
        this.widgetId = widetID;
        this.servoID = servoID;
        this.localPos = pos;
    }

    public SliderWidgetData(SliderWidgetData data)
    {
        if (data != null)
        {
            this.widgetId = data.widgetId;
            this.pos_x = data.pos_x;
            this.pos_y = data.pos_y;
            this.servoID = data.servoID;
            this.motionType = data.motionType;
            this.sType = data.sType;
            this.directionDisclock = data.directionDisclock;
        }
    }

    public enum sliderType
    { 
        h,
        v
    }
}

public enum CtrlMotionType : byte
{
    servo = 0,
    motor,
}
/// <summary>
/// 横杆 控制一个舵机
/// </summary>
public class HSliderWidgetData
{
    [XmlAttribute]
    public string widgetId;
    [XmlElement]
    public byte servoID;
    [XmlEnum]
    public hsliderType sType;
    [XmlIgnore]
    public Vector2 localPos
    {
        get
        {
            return new Vector2(pos_x, pos_y);
        }
        set
        {
            pos_x = value.x;
            pos_y = value.y;
        }
    }
    [XmlElement]
    public float pos_x;
    [XmlElement]
    public float pos_y;
    [XmlElement]
    public int min_angle;
    [XmlElement]
    public int max_angle;
    [XmlIgnore]
    public bool isOK
    {
        get
        {
            return servoID != 0;
        }
    }
    //protected string 
    public HSliderWidgetData()
    {
        servoID = 0;
        max_angle = 118;
        min_angle = -118;
    }
    public HSliderWidgetData(string widgetID, Vector2 pos)
    {
        this.widgetId = widgetID;
        localPos = pos;
        servoID = 0;
        sType = hsliderType.h;
        max_angle = 118;
        min_angle = -118;
    }
    public HSliderWidgetData(string widetID, byte servoID, Vector3 pos, int maxAngle, int minAngle)
    {
        this.widgetId = widetID;
        this.servoID = servoID;
        this.localPos = pos;
        this.max_angle = maxAngle;
        this.min_angle = minAngle;
    }

    public enum hsliderType
    {
        h,
        v
    }
}
/// <summary>
/// 摇杆数据
/// </summary>
public class JockstickData
{
    [XmlAttribute]
    public string widgetId;
    [XmlIgnore]
    public Vector2 localPos
    {
        get
        {
            return new Vector2(pos_x, pos_y);
        }
        set
        {
            pos_x = value.x;
            pos_y = value.y;
        }
    }
    [XmlElement]
    public float pos_x;
    [XmlElement]
    public float pos_y;
    [XmlEnum]
    public JockType type;
    [XmlEnum]
    public CtrlMotionType motionType = CtrlMotionType.servo;
    [XmlElement]
    public byte leftUpID;               //左前轮
    [XmlElement]
    public byte rightUpID;              //右前轮
    [XmlElement]
    public byte leftBottomID;           //左后轮
    [XmlElement]
    public byte rightBottomID;          //右后轮
    [XmlElement]
    public byte UpID;                   //前轮

    [XmlIgnore]
    public bool isOK //是否设置ok
    {
        get
        {
            if (type == JockType.none)
                return false;
            else if (type == JockType.twoServo)
                return leftUpID != 0 && rightUpID != 0;
            else if (type == JockType.fourServo)
                return leftUpID != 0 && rightUpID != 0 && leftBottomID != 0 && rightBottomID != 0;
            else
                return false;
        }
    }

    public JockstickData()
    {
        type = JockType.none;
    }
    public JockstickData(string widgetID, Vector2 pos)
    {
        this.widgetId = widgetID;
        this.localPos = pos;
        type = JockType.none;
    }

    public enum JockType
    { 
        twoServo,
        treeServo,
        fourServo,
        none,
    }
    
}

