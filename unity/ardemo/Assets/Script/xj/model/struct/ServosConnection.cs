using Game.Platform;
using Game.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ServosConnection.cs
/// Description:舵机连接情况
/// Time:2016/4/5 9:29:44
/// </summary>
public class ServosConnection
{
    #region 公有属性

    #endregion

    #region 其他属性
    /// <summary>
    /// 舵机连接数据，key为主板连接口，List<int>舵机按数序连接
    /// </summary>
    Dictionary<int, List<int>> mServosDict = null;
    /// <summary>
    /// 拓扑图数据
    /// </summary>
    List<TopologyPartData> mPartData = null;
    /// <summary>
    /// 舵机模式
    /// </summary>
    Dictionary<byte, ServoModel> mServosModelDict = null;
    /// <summary>
    /// 连接情况
    /// </summary>
    Dictionary<string, string> mPortConnectionDict = null;
    /// <summary>
    /// 主控盒类型
    /// </summary>
    TopologyPartType mMainboardType = TopologyPartType.MainBoard;
    public TopologyPartType MainboardType
    {
        get { return mMainboardType; }
        set { mMainboardType = value; }
    }

    long mFileModifyTime = 0;
    public long FileModifyTime
    {
        get
        {
            return  mFileModifyTime;
        }
    }
    #endregion

    #region 公有函数
    public ServosConnection()
    {
        mPartData = new List<TopologyPartData>();
        mServosModelDict = new Dictionary<byte, ServoModel>();
        mPortConnectionDict = new Dictionary<string, string>();
    }
    /// <summary>
    /// 通过主板接线口获取连接的舵机
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public List<int> GetServosForPort(int port)
    {
        if (null != mServosDict && mServosDict.ContainsKey(port))
        {
            return mServosDict[port];
        }
        return null;
    }
    /// <summary>
    /// 增加一条舵机数据
    /// </summary>
    /// <param name="port"></param>
    /// <param name="List"></param>
    /// <param name=""></param>
    /// <param name=""></param>
    public ErrorCode AddPortServos(int port, List<int> servos)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        if (null != mServosDict && mServosDict.ContainsKey(port))
        {//已存在的连接口
            ret = ErrorCode.Result_Port_Exist;
            return ret;
        }
        Dictionary<int, int> tmpServos = new Dictionary<int, int>();
        for (int i = 0, imax = servos.Count; i < imax; ++i)
        {
            if (tmpServos.ContainsKey(servos[i]))
            {//舵机id重复
                ret = ErrorCode.Result_DJ_ID_Repeat;
                return ret;
            }
            tmpServos.Add(servos[i], 1);
        }
        if (null == mServosDict)
        {
            mServosDict = new Dictionary<int, List<int>>();
        }
        else
        {
            foreach (KeyValuePair<int, List<int>> kvp in mServosDict)
            {
                for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
                {
                    if (tmpServos.ContainsKey(kvp.Value[i]))
                    {//舵机id重复
                        ret = ErrorCode.Result_DJ_ID_Repeat;
                        return ret;
                    }
                }
            }
        }
        mServosDict.Add(port, servos);
        return ret;
    }
    /// <summary>
    /// 删除某条连接
    /// </summary>
    /// <param name="port"></param>
    public void DelPortServos(int port)
    {
        if (null != mServosDict && mServosDict.ContainsKey(port))
        {
            mServosDict.Remove(port);
        }
    }
    /// <summary>
    /// 更新一条舵机数据
    /// </summary>
    /// <param name="port"></param>
    /// <param name="servos"></param>
    public void UpdatePortServos(int port, List<int> servos)
    {
        if (null == mServosDict)
        {
            mServosDict = new Dictionary<int, List<int>>();
        }
        mServosDict[port] = servos;
    }
    /// <summary>
    /// 获取主板接口
    /// </summary>
    /// <returns></returns>
    public List<int> GetPortList()
    {
        if (null != mServosDict)
        {
            List<int> list = new List<int>();
            foreach (int key in mServosDict.Keys)
            {
                list.Add(key);
            }
            return list;
        }
        return null;
    }

    public Dictionary<TopologyPartType, List<byte>> GetTopologySensor()
    {
        Dictionary<TopologyPartType, List<byte>> dict = new Dictionary<TopologyPartType, List<byte>>();
        if (null != mPartData)
        {
            for (int i = 0, imax = mPartData.Count; i < imax; ++i)
            {
                if (mPartData[i].partType >= TopologyPartType.Infrared && mPartData[i].partType < TopologyPartType.Motor)
                {
                    if (!dict.ContainsKey(mPartData[i].partType))
                    {
                        List<byte> ids = new List<byte>();
                        dict[mPartData[i].partType] = ids;
                    }
                    dict[mPartData[i].partType].Add(mPartData[i].id);
                }
            }
            foreach (var kvp in dict)
            {
                kvp.Value.Sort();
            }
        }
        return dict;
    }

    public bool PartExist(TopologyPartType partType, int id)
    {
        if (null != mPartData)
        {
            for (int i = 0, imax = mPartData.Count; i < imax; ++i)
            {
                if (mPartData[i].partType == partType && mPartData[i].id == id)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Save(Robot robot)
    {
        try
        {
            string path = PublicFunction.CombinePath(ResourcesEx.GetRobotPath(robot.Name), "servos");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = PublicFunction.CombinePath(path, "servos.txt");
            bool addFileFlag = true;
            if (File.Exists(path))
            {
                addFileFlag = false;
            }
            //FileInfo fileInfo = new FileInfo(path);
            List<string> list = new List<string>();
            //StreamWriter sw = new StreamWriter(fileInfo.Open(FileMode.OpenOrCreate));
            if (null != mServosDict)
            {
                foreach (KeyValuePair<int, List<int>> kvp in mServosDict)
                {
                    list.Add(kvp.Key + " " + PublicFunction.ListToString<int>(kvp.Value));
                    //sw.WriteLine(kvp.Key + " " + PublicFunction.ListToString<int>(kvp.Value));
                }
            }
            StringBuilder angleServos = new StringBuilder();
            StringBuilder turnServos = new StringBuilder();
            foreach (KeyValuePair<byte, ServoModel> kvp in mServosModelDict)
            {
                if (kvp.Value == ServoModel.Servo_Model_Angle)
                {
                    if (angleServos.Length > 0)
                    {
                        angleServos.Append(PublicFunction.Separator_Comma);
                    }
                    angleServos.Append(kvp.Key);
                }
                else
                {
                    if (turnServos.Length > 0)
                    {
                        turnServos.Append(PublicFunction.Separator_Comma);
                    }
                    turnServos.Append(kvp.Key);
                }
            }
            if (angleServos.Length > 0)
            {
                list.Add("angleServos:" + angleServos.ToString());
                //sw.WriteLine("angleServos:" + angleServos.ToString());
            }
            if (turnServos.Length > 0)
            {
                list.Add("turnServos:" + turnServos.ToString());
                //sw.WriteLine("turnServos:" + turnServos.ToString());
            }
            for (int i = 0, imax = mPartData.Count; i < imax; ++i)
            {
                list.Add("partData:" + mPartData[i].ToString());
                //sw.WriteLine("partData:" + mPartData[i].ToString());
            }
            if (mPortConnectionDict.Count > 0)
            {
                StringBuilder portString = new StringBuilder();
                foreach (KeyValuePair<string, string> kvp in mPortConnectionDict)
                {
                    if (portString.Length > 0)
                    {
                        portString.Append(PublicFunction.Separator_Or);
                    }
                    portString.Append(kvp.Key);
                    portString.Append(PublicFunction.Separator_Comma);
                    portString.Append(kvp.Value);
                }
                list.Add("portConnection:" + portString.ToString());
                //sw.WriteLine("portConnection:" + portString.ToString());
            }
            /*sw.Flush();
            sw.Dispose();
            sw.Close();*/
            File.WriteAllLines(path, list.ToArray());
            mFileModifyTime = DateTime.Now.Ticks;
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "保存连线图：" + path);
            if (ResourcesEx.GetRobotType(robot) == ResFileType.Type_playerdata)
            {
                if (addFileFlag)
                {
                    PlatformMgr.Instance.OperateSyncFile(robot.Name, path, OperateFileType.Operate_File_Add);
                }
                else
                {
                    PlatformMgr.Instance.OperateSyncFile(robot.Name, path, OperateFileType.Operate_File_Change);
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
    /// 
    /// </summary>
    /// <param name="robotFloder"></param>
    public void Save(string robotFloder)
    {
        try
        {
            string path = robotFloder + "/servos";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = PublicFunction.CombinePath(path, "servos.txt");
            FileInfo fileInfo = new FileInfo(path);
            StreamWriter sw = new StreamWriter(fileInfo.Open(FileMode.OpenOrCreate));
            if (null != mServosDict)
            {
                foreach (KeyValuePair<int, List<int>> kvp in mServosDict)
                {
                    sw.WriteLine(kvp.Key + " " + PublicFunction.ListToString<int>(kvp.Value));
                }
            }
            StringBuilder angleServos = new StringBuilder();
            StringBuilder turnServos = new StringBuilder();
            foreach (KeyValuePair<byte, ServoModel> kvp in mServosModelDict)
            {
                if (kvp.Value == ServoModel.Servo_Model_Angle)
                {
                    if (angleServos.Length > 0)
                    {
                        angleServos.Append(PublicFunction.Separator_Comma);
                    }
                    angleServos.Append(kvp.Key);
                }
                else
                {
                    if (turnServos.Length > 0)
                    {
                        turnServos.Append(PublicFunction.Separator_Comma);
                    }
                    turnServos.Append(kvp.Key);
                }
            }
            if (angleServos.Length > 0)
            {
                sw.WriteLine("angleServos:" + angleServos.ToString());
            }
            if (turnServos.Length > 0)
            {
                sw.WriteLine("turnServos:" + turnServos.ToString());
            }
            for (int i = 0, imax = mPartData.Count; i < imax; ++i)
            {
                sw.WriteLine("partData:" + mPartData[i].ToString());
            }
            if (mPortConnectionDict.Count > 0)
            {
                StringBuilder portString = new StringBuilder();
                foreach (KeyValuePair<string, string> kvp in mPortConnectionDict)
                {
                    if (portString.Length > 0)
                    {
                        portString.Append(PublicFunction.Separator_Or);
                    }
                    portString.Append(kvp.Key);
                    portString.Append(PublicFunction.Separator_Comma);
                    portString.Append(kvp.Value);
                }
                sw.WriteLine("portConnection:" + portString.ToString());
            }
            sw.Flush();
            sw.Dispose();
            sw.Close();
            mFileModifyTime = DateTime.Now.Ticks;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public void Read(string robotFloder)
    {
        try
        {
            string path = robotFloder + "/servos/servos.txt";
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                mFileModifyTime = fileInfo.LastWriteTimeUtc.Ticks;
                if (mFileModifyTime <= 0)
                {
                    mFileModifyTime = fileInfo.CreationTimeUtc.Ticks;
                }
                StreamReader sr = new StreamReader(fileInfo.Open(FileMode.Open));
                string readStr = null;
                mPartData.Clear();
                mServosModelDict.Clear();
                mPortConnectionDict.Clear();
                while (null != (readStr = sr.ReadLine()))
                {
                    if (readStr.StartsWith("partData:"))
                    {
                        TopologyPartData data = new TopologyPartData(readStr.Substring("partData:".Length));
                        if (data.partType == TopologyPartType.MainBoard)
                        {
                            mMainboardType = TopologyPartType.MainBoard;
                        } else if (data.partType == TopologyPartType.MainBoard_new_low)
                        {
                            mMainboardType = TopologyPartType.MainBoard_new_low;
                        }
                        mPartData.Add(data);
                    }
                    else if (readStr.StartsWith("angleServos:"))
                    {
                        List<byte> angleList = PublicFunction.StringToByteList(readStr.Substring("angleServos:".Length));
                        for (int i = 0, imax = angleList.Count; i < imax; ++i)
                        {
                            mServosModelDict[angleList[i]] = ServoModel.Servo_Model_Angle;
                        }
                    }
                    else if (readStr.StartsWith("turnServos:"))
                    {
                        List<byte> turnList = PublicFunction.StringToByteList(readStr.Substring("turnServos:".Length));
                        for (int i = 0, imax = turnList.Count; i < imax; ++i)
                        {
                            mServosModelDict[turnList[i]] = ServoModel.Servo_Model_Turn;
                        }
                    }
                    else if (readStr.StartsWith("portConnection:"))
                    {
                        string[] portList = readStr.Substring("portConnection:".Length).Split(PublicFunction.Separator_Or);
                        for (int i = 0, imax = portList.Length; i < imax; ++i)
                        {
                            if (!string.IsNullOrEmpty(portList[i]))
                            {
                                string[] tmpStr = portList[i].Split(PublicFunction.Separator_Comma);
                                if (2 == tmpStr.Length)
                                {
                                    mPortConnectionDict[tmpStr[0]] = tmpStr[1];
                                }
                            }
                            
                        }
                    }
                    string[] tmp = readStr.Split(' ');
                    if (null != tmp && 2 == tmp.Length)
                    {
                        if (null == mServosDict)
                        {
                            mServosDict = new Dictionary<int, List<int>>();
                        }
                        int port = int.Parse(tmp[0]);
                        List<int> servos = PublicFunction.StringToList(tmp[1]);
                        mServosDict[port] = servos;
                    }
                }
                sr.Dispose();
                sr.Close();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    /// <summary>
    /// 创建一个ServosConnection对象
    /// </summary>
    /// <param name="robotFloder"></param>
    /// <returns></returns>
    public static ServosConnection CreateServos(string robotFloder)
    {
        ServosConnection servosConnection = null;
        try
        {
            string path = robotFloder + "/servos/servos.txt";
            if (File.Exists(path))
            {
                servosConnection = new ServosConnection();
                servosConnection.Read(robotFloder);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return servosConnection;
    }
    /// <summary>
    /// 获取拓扑图数据
    /// </summary>
    /// <returns></returns>
    public List<TopologyPartData> GetTopologyData()
    {
        return mPartData;
    }
    /// <summary>
    /// 获取端口连接数据
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> GetPortConnectionData()
    {
        return mPortConnectionDict;
    }

    /// <summary>
    /// 获取独立队列的id
    /// </summary>
    /// <returns></returns>
    public Dictionary<TopologyPartType, List<byte>> GetIndependentQueue()
    {
        Dictionary<TopologyPartType, List<byte>> dataDict = null;
        if (null != mPartData)
        {
            TopologyPartData data = null;
            for (int i = 0, imax = mPartData.Count; i < imax; ++i)
            {
                data = mPartData[i];
                if (data.isIndependent)
                {
                    if (null == dataDict)
                    {
                        dataDict = new Dictionary<TopologyPartType, List<byte>>();
                    }
                    if (dataDict.ContainsKey(data.partType))
                    {
                        dataDict[data.partType].Add(data.id);
                    }
                    else
                    {
                        List<byte> list = new List<byte>();
                        list.Add(data.id);
                        dataDict[data.partType] = list;
                    }
                }
            }
            if (null != dataDict)
            {
                foreach (var kvp in dataDict)
                {
                    kvp.Value.Sort();
                }
            }
        }
        return dataDict;
    }
    
    /// <summary>
    /// 获取舵机模式
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ServoModel GetServoModel(byte id)
    {
        if (mServosModelDict.ContainsKey(id))
        {
            return mServosModelDict[id];
        }
        return ServoModel.Servo_Model_Angle;
    }

    public void UpdateServoModel(byte id, ServoModel modelType)
    {
        mServosModelDict[id] = modelType;
    }

    /// <summary>
    /// 增加一条连接数据
    /// </summary>
    /// <param name="portName"></param>
    /// <param name="portName1"></param>
    public void AddPortConnection(string portName, string otherPortName)
    {
        mPortConnectionDict[portName] = otherPortName;
    }

    public void AddTopologyPartData(TopologyPartData data)
    {
        mPartData.Add(new TopologyPartData(data));
    }

    public void DelIndependentTopologyData(TopologyPartData data)
    {
        if (data.isIndependent && null != mPartData)
        {
            for (int i = 0, imax = mPartData.Count; i < imax; ++i)
            {
                if (data.partType == mPartData[i].partType && data.id == mPartData[i].id && mPartData[i].isIndependent)
                {
                    mPartData.Remove(mPartData[i]);
                    return;
                }
            }
        }
    }

    public void UpdateSersorTopologyData(Robot robot)
    {
        if (null != mPartData && null != robot && null != robot.MotherboardData)
        {
            TopologyPartData data = null;
            List<TopologyPartData> delData = null;
            Dictionary<TopologyPartType, Dictionary<byte, TopologyPartData>> oldSensorData = null;
            bool changeFlag = false;
            for (int i = 0, imax = mPartData.Count; i < imax; ++i)
            {
                data = mPartData[i];
                if (data.partType < TopologyPartType.Infrared || data.partType >= TopologyPartType.Motor)
                {
                    continue;
                }
                if (data.isIndependent)
                {
                    SensorData sensorData = robot.MotherboardData.GetSensorData(data.partType);
                    if (null == sensorData || !sensorData.ids.Contains(data.id))
                    {//不存在次传感器，要删除
                        if (null == delData)
                        {
                            delData = new List<TopologyPartData>();
                        }
                        delData.Add(data);
                        continue;
                    }
                }
                if (null == oldSensorData)
                {
                    oldSensorData = new Dictionary<TopologyPartType, Dictionary<byte, TopologyPartData>>();
                }
                if (!oldSensorData.ContainsKey(data.partType))
                {
                    Dictionary<byte, TopologyPartData> dict = new Dictionary<byte, TopologyPartData>();
                    oldSensorData[data.partType] = dict;
                }
                oldSensorData[data.partType][data.id] = data;
            }
            if (null != delData)
            {//删除传感器
                for (int i = 0, imax = delData.Count; i < imax; ++i)
                {
                    mPartData.Remove(delData[i]);
                }
                delData.Clear();
                changeFlag = true;
            }
            TopologyPartType[] sensorType = PublicFunction.Open_Topology_Part_Type;
            for (int i = 0, imax = sensorType.Length; i < imax; ++i)
            {
                SensorData sensorData = robot.MotherboardData.GetSensorData(sensorType[i]);
                if (null != sensorData)
                {
                    for (int sensorIndex = 0, sensorMax = sensorData.ids.Count; sensorIndex < sensorMax; ++sensorIndex)
                    {
                        if (null == oldSensorData || !oldSensorData.ContainsKey(sensorType[i]) || !oldSensorData[sensorType[i]].ContainsKey(sensorData.ids[sensorIndex]))
                        {//以前不存在，需添加
                            TopologyPartData newData = new TopologyPartData();
                            newData.isIndependent = true;
                            newData.id = sensorData.ids[sensorIndex];
                            newData.partType = sensorType[i];
                            mPartData.Add(newData);
                            changeFlag = true;
                        }
                    }
                }
            }
            if (changeFlag)
            {
                Save(robot);
            }
        }
    }

    public void AddTopologyPartData(TopologyPartData data, Transform trans)
    {
        TopologyPartData newData = new TopologyPartData(data);
        newData.localPosition = trans.localPosition;
        newData.localEulerAngles = trans.localEulerAngles;
        mPartData.Add(newData);
    }

    public void CleanUp()
    {
        if (null != mServosDict)
        {
            mServosDict.Clear();
        }
        mPartData.Clear();
        mServosModelDict.Clear();
        mPortConnectionDict.Clear();
    }

    public bool IsNewSensor(TopologyPartType sensorType, byte id)
    {
        if (null != mPartData)
        {
            for (int i = 0, imax = mPartData.Count; i < imax; ++i)
            {
                if (mPartData[i].partType == sensorType && mPartData[i].id == id)
                {
                    return false;
                }
            }
        }
        return true;
    }

    #endregion

    #region 其他函数
    #endregion
}




/// <summary>
/// 舵机模式
/// </summary>
public enum ServoModel : byte
{
    Servo_Model_Angle = 1,
    Servo_Model_Turn,
}