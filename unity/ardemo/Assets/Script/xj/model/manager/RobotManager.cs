using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Resource;
using System.IO;
using System.Xml;
using LitJson;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:RobotManager.cs
/// Description:机器人管理
/// Time:2015/6/26 13:48:52
/// </summary>
public class RobotManager
{
    #region 公有属性
    public bool IsSetDeviceIDFlag
    {
        get { return isSetDeviceIDFlag; }
        set { isSetDeviceIDFlag = value; }
    }
    public bool IsCreateRobotFlag
    {
        get { return isCreateRobotFlag; }
        set { isCreateRobotFlag = value; }
    }
    #endregion

    #region 私有属性
    static RobotManager sInst = null;
    bool isSetDeviceIDFlag = false;
    Dictionary<string, Robot> mRobotDict = null;
    Dictionary<string, bool> mSetRobotDjDict = null;//记录有没有设置过舵机
    //Dictionary<string, List<Int2>> mRobotResetDict = null;//记录舵机初始角度
    Robot mCurrentRobot = null;
    const string SetRobotDjFileName = "RecordSetDj";
    //const string SetRobotResetFileName = "RobotReset";
    Robot mSetDeviceRobot = null;
    Robot mCreateRobot = null;
    bool isCreateRobotFlag = false;
    #endregion

    #region 公有函数
    public static RobotManager GetInst()
    {
        if (null == sInst)
        {
            sInst = new RobotManager();
        }
        return sInst;
    }

    public void CleanUp()
    {
        isCreateRobotFlag = false;
        isSetDeviceIDFlag = false;
        if (null != mRobotDict)
        {
            mRobotDict.Clear();
        }
    }

    /// <summary>
    /// 增加一个机器人
    /// </summary>
    /// <param name="id"></param>
    /// <param name="robot"></param>
    public void AddRobot(string id, Robot robot)
    {
        mRobotDict[id] = robot;
        //robot.InitActionSequence(ActionsManager.GetInst().GetRobotActions(id));
    }

    public void AddRobotActions(Robot robot, ActionSequence act)
    {
        if (mRobotDict.ContainsKey(robot.ID))
        {
            ActionsManager.GetInst().AddRobotActions(robot.ID, act);
        }
        //告诉应用添加了动作
        //AddRobotOrActions(robot.ID);
    }
    /// <summary>
    /// 创建了新的模型或者更新了模型须调用此接口
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    public void CreateOrUpdateRobot(string name, string id = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (null == id)
        {
            id = RobotMgr.Instance.FindRobotID(name);
        }
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        Robot robot = null;
        if (mRobotDict.ContainsKey(id))
        {
            Dictionary<string, int> dj = RobotMgr.Instance.FindDJData(name);
            if (null != dj)
            {
                robot = mRobotDict[id];
                foreach (KeyValuePair<string, int> kvp in dj)
                {
                    byte djId = (byte)RobotMgr.Instance.FinddjIDBydjNam(name, kvp.Key);//PublicFunction.GetDuoJiId(kvp.Key);
                    if (null == robot.GetAllDjData().GetDjData(djId))
                    {
                        robot.GetAllDjData().UpdateData(new DuoJiData(djId, PublicFunction.DuoJi_Start_Rota));
                    }
                }
            }
        }
        else
        {
            robot = new Robot(id);
            robot.SetRobotMacAndName(string.Empty, name);
            Dictionary<string, int> dj = RobotMgr.Instance.FindDJData(name);
            if (null != dj)
            {
                foreach (KeyValuePair<string, int> kvp in dj)
                {

                    int djId = RobotMgr.Instance.FinddjIDBydjNam(name, kvp.Key);//PublicFunction.GetDuoJiId(kvp.Key);
                    DuoJiData data = new DuoJiData((byte)djId, (short)PublicFunction.DuoJi_Start_Rota);
                    robot.GetAllDjData().UpdateData(data);
                }
            }
            List<byte> motors = RobotMgr.Instance.FindMotorIds(name);
            if (null != motors && motors.Count > 0)
            {
                robot.MotorsData = new MotorData(motors);
            }
            AddRobot(id, robot);
        }
        SingletonObject<ServosConManager>.GetInst().ReadServosConnection(robot);
        ActionsManager.GetInst().ReadActions(robot);
        SingletonObject<ActionAudioManager>.GetInst().ReadRobotAudio(robot);
        SingletonObject<MoveManager>.GetInst().ReadMoves(robot);
        //告诉应用添加了模型
        //AddRobotOrActions(id);
    }
    /// <summary>
    /// 删除机器人
    /// </summary>
    /// <param name="name"></param>
    public void DeleteRobot(string name)
    {
        string robId = string.Empty;
        foreach (KeyValuePair<string, Robot> kvp in mRobotDict)
        {
            if (0 == kvp.Value.Name.CompareTo(name))
            {
                robId = kvp.Key;
                break;
            }
        }
        if (!string.IsNullOrEmpty(robId))
        {
            mRobotDict.Remove(robId);
            ActionsManager.GetInst().DeleteRobotAllActions(robId);
        }
        //PlatformMgr.Instance.DelModel(name);
    }

    /*public void AddRobotOrActions(string robotId)
    {
        if (mRobotDict.ContainsKey(robotId))
        {
            string modelName = mRobotDict[robotId].Name;

            Dictionary<string, ActionSequence> acts = ActionsManager.GetInst().GetRobotActions(robotId);
            List<Dictionary<string, string>> actionArray = new List<Dictionary<string, string>>();
            if (null != acts)
            {
                foreach (KeyValuePair<string, ActionSequence> kvp in acts)
                {
                    Dictionary<string, string> tmp = new Dictionary<string, string>();
                    tmp.Add("actionName", kvp.Value.Name);
                    actionArray.Add(tmp);
                }
            }

            //零件
            List<Dictionary<string, object>> gosList = new List<Dictionary<string, object>>();
            Dictionary<string, int> gos = RobotMgr.Instance.FindGONumbers(modelName);
            if (null != gos)
            {
                foreach (KeyValuePair<string, int> kvp in gos)
                {
                    if (kvp.Value > 0)
                    {
                        Dictionary<string, object> tmpDict = new Dictionary<string, object>();
                        tmpDict.Add("pid", kvp.Key);
                        tmpDict.Add("count", kvp.Value);
                        gosList.Add(tmpDict);
                    }
                }
            }
            
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("path", ResourcesEx.GetRootPath() + "/" + modelName);
            dic.Add("modelName", modelName);
            dic.Add("actionArray", actionArray/ *Json.Serialize(actionArray)* /);
            dic.Add("date", DateTime.Now.ToString("yyyy-MM-dd"));
            dic.Add("parts", gosList);
            string json_bill = Json.Serialize(dic);
            Debuger.Log(json_bill);
            PlatformMgr.Instance.SaveModelOrActions(json_bill);
        }
    }*/
    /// <summary>
    /// 通过id地址获取机器人数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Robot GetRobotForID(string id)
    {
        if (mRobotDict.ContainsKey(id))
        {
            return mRobotDict[id];
        }
        return null;
    }

    public Robot GetRobotForName(string name)
    {
        foreach (KeyValuePair<string, Robot> kvp in mRobotDict)
        {
            if (kvp.Value.Name.Equals(name))
            {
                return kvp.Value;
            }
        }
        return null;
    }
    /// <summary>
    /// 断开所有连接
    /// </summary>
    public void DisAllConnencted()
    {
        foreach (KeyValuePair<string, Robot> kvp in mRobotDict)
        {
            kvp.Value.Disconnect();
        }
    }

    /// <summary>
    /// 玩家选中了某个机器人
    /// </summary>
    /// <param name="id"></param>
    public void ChoiceRobotForID(string id)
    {
        Robot robot = null;
        if (mRobotDict.ContainsKey(id))
        {
            robot = mRobotDict[id];
        }
        if (null != robot)
        {
            ChoiceRobot(robot);
        }
    }
    /// <summary>
    /// 玩家选中了某个机器人
    /// </summary>
    /// <param name="name"></param>
    public void ChoiceRobotForName(string name)
    {
        Robot robot = null;
        foreach (KeyValuePair<string, Robot> kvp in mRobotDict)
        {
            if (kvp.Value.Name.Equals(name))
            {
                robot = kvp.Value;
                break;
            }
        }
        if (null != robot)
        {
            ChoiceRobot(robot);
        }
    }

    /// <summary>
    /// 设置当前机器人
    /// </summary>
    /// <param name="robot"></param>
    public void SetCurrentRobot(Robot robot)
    {
        
        if (null != robot)
        {
            mCurrentRobot = robot;
            ActionLogic.GetIns().RobotID = robot.ID;
            if (!mRobotDict.ContainsKey(robot.ID))
            {
                AddRobot(robot.ID, robot);
            }
        }
        else
        {
            mCurrentRobot = null;
            ActionLogic.GetIns().RobotID = "";
        }
    }
    /// <summary>
    /// 获取当前机器人
    /// </summary>
    /// <returns></returns>
    public Robot GetCurrentRobot()
    {
        return mCurrentRobot;
    }
    /// <summary>
    /// 获得设置设备Id的机器人
    /// </summary>
    /// <returns></returns>
    public Robot GetSetDeviceRobot()
    {
        if (null == mSetDeviceRobot)
        {
            mSetDeviceRobot = new Robot("setDeviceRobot");
        }
        return mSetDeviceRobot;
    }

    public Robot GetCreateRobot()
    {
        if (null == mCreateRobot)
        {
            mCreateRobot = new Robot("CreateRobot");
        }
        return mCreateRobot;
    }

    /// <summary>
    /// 获得所有机器人
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllRobot()
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, Robot> kvp in mRobotDict)
        {
            list.Add(kvp.Key);
        }
        return list;
    }

    public Robot CreateNewRobot()
    {
        Robot robot = new Robot(CreateID.CreateRobotID());
        AddRobot(robot.ID, robot);
        return robot;
    }
    /// <summary>
    /// 记录机器人是否已经设置过舵机id
    /// </summary>
    /// <param name="robotId"></param>
    public void RecordSetDjOver(string robotId)
    {
        mSetRobotDjDict[robotId] = true;
        SaveSetDuoJiInfo();
    }
    /// <summary>
    /// 有没有设置过复位
    /// </summary>
    /// <param name="robotId"></param>
    /// <returns></returns>
    /*public bool HaveSetReset(string robotId)
    {
        if (mRobotResetDict.ContainsKey(robotId))
        {
            return true;
        }
        return false;
    }*/
    /// <summary>
    /// 设置机器人复位状态
    /// </summary>
    /// <param name="robotId"></param>
    /*public void SetRobotResetData(string robotId)
    {
        if (mRobotDict.ContainsKey(robotId))
        {
            ModelDjData datas = mRobotDict[robotId].GetAllDjData();
            if (null != datas)
            {
                List<Int2> list = new List<Int2>();
                Dictionary<byte, DuoJiData> djs = datas.GetAllData();
                foreach (KeyValuePair<byte, DuoJiData> kvp in djs)
                {
                    Int2 int2 = new Int2();
                    int2.num1 = kvp.Key;
                    int2.num2 = kvp.Value.startRota;
                    list.Add(int2);
                }
                mRobotResetDict[robotId] = list;
                SaveResetInfo();
            }
        }
    }*/
    /// <summary>
    /// 获取是否设置过舵机id
    /// </summary>
    /// <param name="robotId"></param>
    /// <returns></returns>
    public bool GetSetDuoJiFlag(string robotId)
    {
        if (mSetRobotDjDict.ContainsKey(robotId))
        {
            return mSetRobotDjDict[robotId];
        }
        return false;
    }

    #endregion

    #region 私有函数
    string orirobotname;
    private RobotManager()
    {
        mRobotDict = new Dictionary<string, Robot>();
        mSetRobotDjDict = new Dictionary<string, bool>();
        //mRobotResetDict = new Dictionary<string, List<Int2>>();

       // List<string> list = RecordContactInfo.Instance.XMLExist(RecordContactInfo.Instance.openType);  //
        /*List<string> list = RobotMgr.Instance.ExistRobotsNam();
        if (null != list)
        {
            for (int i = 0, icount = list.Count; i < icount; ++i)
            {
                if (string.IsNullOrEmpty(list[i]))
                {
                    continue;
                }


                string id = RobotMgr.Instance.FindRobotID(list[i]);

                if (!string.IsNullOrEmpty(id))
                {
                    Dictionary<string, int> dj = RobotMgr.Instance.FindDJData(list[i]);//RecordContactInfo.Instance.FindDJData(list[i]);
                    if (null != dj)
                    {
                        Robot robot = new Robot(id);
                        robot.SetRobotMacAndName(string.Empty, list[i]);
                        foreach (KeyValuePair<string, int> kvp in dj)
                        {
                            //string namewithtype = RobotMgr.NameWithType(list[i], RecordContactInfo.Instance.openType);  //给名字添加类型后缀

                            int djId = RobotMgr.Instance.FinddjIDBydjNam(list[i], kvp.Key);//PublicFunction.GetDuoJiId(kvp.Key);
                            DuoJiData data = new DuoJiData((byte)djId, (short)PublicFunction.DuoJi_Start_Rota);
                            robot.GetAllDjData().UpdateData(data);
                        }
                        AddRobot(id, robot);
                    }
                }
                
            }
        }*/
        ReadSetDuoJiInfo();
        //ReadResetInfo();
    }
    string GetSetDjFilePath()
    {
        return ResourcesEx.persistentDataPath + "/" + SetRobotDjFileName;
    }

    /*string GetResetFilePath()
    {
        return ResourcesEx.persistentDataPath + "/" + SetRobotResetFileName;
    }*/

    void ChoiceRobot(Robot robot)
    {
        if (PlatformMgr.Instance.GetBluetoothState())
        {//蓝牙是连接的
            if (null != mCurrentRobot && mCurrentRobot.ID != robot.ID)
            {//换了模型要断开蓝牙
                PlatformMgr.Instance.DisConnenctBuletooth();
            }
            else if (null != mCurrentRobot && mCurrentRobot != robot)
            {
                robot = mCurrentRobot;
            }
        }
        RobotManager.GetInst().SetCurrentRobot(robot);
    }
    void ReadSetDuoJiInfo()
    {
        try
        {
            string path = GetSetDjFilePath();
            if (File.Exists(path))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                XmlElement xmlRoot = xmlDoc.DocumentElement;
                XmlNode node = xmlRoot.FirstChild;
                mSetRobotDjDict.Clear();
                if (null == node)
                {
                    return;
                }

                while (null != node)
                {
                    if (!(node is XmlElement))
                    {
                        continue;
                    }
                    XmlElement xe = (XmlElement)node;
                    string robid = xe.GetAttribute("robotId");
                    int num = int.Parse(xe.GetAttribute("flag"));
                    bool flag = num > 0 ? true : false;
                    mSetRobotDjDict[robid] = flag;
                    node = node.NextSibling;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void SaveSetDuoJiInfo()
    {
        if (null != mSetRobotDjDict && mSetRobotDjDict.Count > 0)
        {
            //新建xml实例
            XmlDocument xmlDoc = new XmlDocument();
            //创建根节点，最上层节点
            XmlElement roots = xmlDoc.CreateElement("Roots");
            xmlDoc.AppendChild(roots);

            foreach (KeyValuePair<string, bool> kvp in mSetRobotDjDict)
            {
                XmlElement node = xmlDoc.CreateElement("Root");
                node.SetAttribute("robotId", kvp.Key);
                node.SetAttribute("flag", (kvp.Value ? 1 : 0).ToString());
                roots.AppendChild(node);
            }

            //将xml文件保存到本地
            xmlDoc.Save(GetSetDjFilePath());
        }
       
    }

    /*void ReadResetInfo()
    {
        string path = GetResetFilePath();
        if (File.Exists(path))
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                XmlElement xmlRoot = xmlDoc.DocumentElement;
                XmlNode node = xmlRoot.FirstChild;
                mRobotResetDict.Clear();
                if (null == node)
                {
                    return;
                }
                while (null != node)
                {
                    if (!(node is XmlElement))
                    {
                        continue;
                    }
                    XmlElement xe = (XmlElement)node;
                    string robid = xe.GetAttribute("robotId");
                    string rotas = xe.GetAttribute("rotas");
                    if (!string.IsNullOrEmpty(rotas))
                    {
                        if (mRobotDict.ContainsKey(robid))
                        {
                            string[] str = rotas.Split(';');
                            if (null != str && str.Length > 0)
                            {
                                List<Int2> list = new List<Int2>();
                                for (int i = 0, imax = str.Length; i < imax; ++i)
                                {
                                    string[] tmp = str[i].Split('$');
                                    if (null != tmp && tmp.Length == 2)
                                    {
                                        Int2 int2 = new Int2();
                                        int2.num1 = int.Parse(tmp[0]);
                                        int2.num2 = int.Parse(tmp[1]);
                                        mRobotDict[robid].SetStartRota(int2.num1, int2.num2);
                                        list.Add(int2);
                                    }
                                }
                                mRobotResetDict[robid] = list;
                            }
                            
                        }
                    }

                    node = node.NextSibling;
                }
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
            
        }
    }*/

    /*void SaveResetInfo()
    {
        try
        {
            if (null != mRobotResetDict && mRobotResetDict.Count > 0)
            {
                string path = GetResetFilePath();
                //新建xml实例
                XmlDocument xmlDoc = new XmlDocument();
                //创建根节点，最上层节点
                XmlElement roots = xmlDoc.CreateElement("Roots");
                xmlDoc.AppendChild(roots);
                
                foreach (KeyValuePair<string, List<Int2>> kvp in mRobotResetDict)
                {
                    string str = string.Empty;
                    for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            str += ";";
                        }
                        str += kvp.Value[i].num1 + "$" + kvp.Value[i].num2;
                    }
                    XmlElement node1 = xmlDoc.CreateElement("Root");
                    node1.SetAttribute("robotId", kvp.Key);
                    node1.SetAttribute("rotas", str);
                    roots.AppendChild(node1);
                }
                
                //将xml文件保存到本地
                xmlDoc.Save(path);
            }
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
    #endregion
}