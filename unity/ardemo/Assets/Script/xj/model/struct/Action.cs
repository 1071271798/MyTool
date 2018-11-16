using Game.Platform;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:Action.cs
/// Description:动作
/// Time:2015/6/26 10:47:41
/// </summary>
public class Action
{
#region 公有属性
    /// <summary>
    /// 动作索引
    /// </summary>
    public int index;
    /// <summary>
    /// 动作运动时间
    /// </summary>
    public int sportTime;
    /// <summary>
    /// 补时，即停留时间
    /// </summary>
    public int waitTime;
    /// <summary>
    /// 动作舵机角度
    /// </summary>
    public Dictionary<byte, short> rotas = null;
    /// <summary>
    /// 处于旋转模式下的舵机
    /// </summary>
    public Dictionary<byte, TurnData> turnDict = null;
    /// <summary>
    /// 马达数据
    /// </summary>
    public Dictionary<byte, SendMotorData> motorDict = null;
    /// <summary>
    /// 最小的舵机id
    /// </summary>
    public byte idMin = 0;
    /// <summary>
    /// 最小舵机id对应的角度
    /// </summary>
    public short idMinRota;
    /// <summary>
    /// 需要显示的舵机列表
    /// </summary>
    private List<byte> showList;

    public int AllTime
    {
        get { return sportTime + waitTime; }
    }
#endregion

#region 私有属性
#endregion
    
#region 公有函数
    public Action()
    {
        Init(0, PublicFunction.Default_Actions_Time, 0);
    }
    public Action(int index)
    {
        Init(index, PublicFunction.Default_Actions_Time, 0);
    }

    public Action(XmlElement xe, Robot robot)
    {
        rotas = new Dictionary<byte, short>();
        turnDict = new Dictionary<byte, TurnData>();
        try
        {
            int.TryParse(xe.GetAttribute("index"), out index);
            int.TryParse(xe.GetAttribute("sportTime"), out sportTime);
            int.TryParse(xe.GetAttribute("waitTime"), out waitTime);
            byte.TryParse(xe.GetAttribute("idMin"), out idMin);
            short.TryParse(xe.GetAttribute("idMinRota"), out idMinRota);
            string str = xe.GetAttribute("rotas");
            string[] ary = str.Split(';');
            if (null != ary && ary.Length > 0)
            {
                for (int i = 0, icount = ary.Length; i < icount; ++i)
                {
                    string[] tmp = ary[i].Split('$');
                    if (null != tmp && 2 == tmp.Length)
                    {
                        rotas[byte.Parse(tmp[0])] = short.Parse(tmp[1]);
                    }
                }
            }
            string str1 = xe.GetAttribute("turns");
            if (!string.IsNullOrEmpty(str1))
            {
                string[] ary1 = str1.Split(';');
                if (null != ary1 && ary1.Length > 0)
                {
                    for (int i = 0, icount = ary1.Length; i < icount; ++i)
                    {
                        string[] tmp1 = ary1[i].Split('$');
                        if (null != tmp1 && 2 == tmp1.Length)
                        {
                            string[] tmp2 = tmp1[1].Split(',');
                            if (null != tmp2 && 2 == tmp2.Length)
                            {
                                TurnData data;
                                data.turnDirection = (TurnDirection)(byte.Parse(tmp2[0]));
                                data.turnSpeed = ushort.Parse(tmp2[1]);
                                turnDict[byte.Parse(tmp1[0])] = data;
                            }
                        }
                    }
                }
            }

            string str2 = xe.GetAttribute("show");
            if (!string.IsNullOrEmpty(str2))
            {
                string[] ary2 = str2.Split(';');
                if (null != ary2 && ary2.Length > 0)
                {
                    showList = new List<byte>();
                    for (int i = 0, imax = ary2.Length; i < imax; ++i)
                    {
                        byte id = byte.Parse(ary2[i]);
                        DuoJiData data = robot.GetAnDjData(id);
                        if (null != data && data.modelType == ServoModel.Servo_Model_Angle)
                        {
                            showList.Add(id);
                        }
                    }
                }
            }
            /*else
            {
                showList = new List<byte>();
                foreach (KeyValuePair<byte, short> kvp in rotas)
                {
                    showList.Add(kvp.Key);
                }
            }*/

            string str3 = xe.GetAttribute("motors");
            if (!string.IsNullOrEmpty(str3))
            {
                string[] ary1 = str3.Split(';');
                if (null != ary1 && ary1.Length > 0)
                {
                    for (int i = 0, icount = ary1.Length; i < icount; ++i)
                    {
                        string[] tmp1 = ary1[i].Split('$');
                        if (null != tmp1 && tmp1.Length == 2)
                        {
                            string[] tmp2 = tmp1[1].Split(',');
                            if (null != tmp2 && tmp2.Length >= 2)
                            {
                                if (null == motorDict)
                                {
                                    motorDict = new Dictionary<byte, SendMotorData>();
                                }
                                SendMotorData data = new SendMotorData();
                                data.id = byte.Parse(tmp1[0]);
                                data.direction = (TurnDirection)(byte.Parse(tmp2[0]));
                                data.speed = short.Parse(tmp2[1]);
                                if (tmp2.Length > 2)
                                {
                                    data.time = (ushort)(int.Parse(tmp2[2]) / 100);
                                } else
                                {
                                    data.time = 65535;
                                }
                                motorDict[data.id] = data;
                            }
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


    public Action(int index, int sportTime)
    {
        Init(index, sportTime, 0);
    }

    public int GetRota(byte id)
    {
        if (rotas.ContainsKey(id))
        {
            return rotas[id];
        }
        return PublicFunction.DuoJi_Start_Rota;
    }
    public void UpdateData(int index, byte id, int sportTime, int waitTime)
    {
        Init(index, sportTime, waitTime);
    }
    /// <summary>
    /// 修改或添加一个舵机的角度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    public void UpdateRota(byte id, short rota)
    {
        if (turnDict.ContainsKey(id))
        {
            turnDict.Remove(id);
        }
        rotas[id] = rota;
         if (idMin == 0 || id < idMin)
         {
             idMin = id;
             idMinRota = rota;
         }
    }
    /// <summary>
    /// 增加或更新连续转动舵机的数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public void UpdateTurn(byte id, TurnData data)
    {
        if (rotas.ContainsKey(id))
        {
            rotas.Remove(id);
        }
        turnDict[id] = data;
        if (idMin == 0 || id < idMin)
        {
            idMin = id;
        }
    }
    /// <summary>
    /// 增加或更新连续转动舵机的数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dir"></param>
    /// <param name="speed"></param>
    public void UpdateTurn(byte id, TurnDirection dir, int speed)
    {//未修改最小id
        if (rotas.ContainsKey(id))
        {
            rotas.Remove(id);
        }
        TurnData data;
        data.turnDirection = dir;
        data.turnSpeed = (ushort)speed;
        turnDict[id] = data;
    }
    /// <summary>
    /// 修改舵机id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    public void SwitchDuoJiId(byte id, byte targetId)
    {
        if (rotas.ContainsKey(id))
        {
            if (rotas.ContainsKey(targetId))
            {
                short rota = rotas[id];
                rotas[id] = rotas[targetId];
                rotas[targetId] = rota;
            }
            else if (turnDict.ContainsKey(targetId))
            {
                turnDict[id] = turnDict[targetId];
                turnDict.Remove(targetId);
                rotas[targetId] = rotas[id];
                rotas.Remove(id);
            }
            else
            {
                rotas[targetId] = rotas[id];
                rotas.Remove(id);
            }
        }
        else if (turnDict.ContainsKey(id))
        {
            if (turnDict.ContainsKey(targetId))
            {
                TurnData tmpData = turnDict[id];
                turnDict[id] = turnDict[targetId];
                turnDict[targetId] = tmpData;
            }
            else if (rotas.ContainsKey(targetId))
            {
                turnDict[targetId] = turnDict[id];
                turnDict.Remove(id);
                rotas[id] = rotas[targetId];
                rotas.Remove(targetId);
            }
            else
            {
                turnDict[targetId] = turnDict[id];
                turnDict.Remove(id);
            }
        }
        if (targetId < idMin)
        {
            idMin = targetId;
        }
        else if (id == idMin && targetId > idMin)
        {
            idMin = byte.MaxValue;
            foreach (KeyValuePair<byte, short> kvp in rotas)
            {
                if (kvp.Key < idMin)
                {
                    idMin = kvp.Key;
                }
            }
            foreach (KeyValuePair<byte, TurnData> kvp in turnDict)
            {
                if (kvp.Key < idMin)
                {
                    idMin = kvp.Key;
                }
            }
        }
    }

    public void Copy(Action action, Robot robot)
    {
        sportTime = action.sportTime;
        waitTime = action.waitTime;
        idMin = action.idMin;
        idMinRota = action.idMinRota;
        rotas.Clear();
        foreach (KeyValuePair<byte, short> kvp in action.rotas)
        {
            UpdateRota(kvp.Key, kvp.Value);
        }
        turnDict.Clear();
        foreach (KeyValuePair<byte, TurnData> kvp in action.turnDict)
        {
            turnDict[kvp.Key] = kvp.Value;
        }
        if (null != action.motorDict)
        {
            motorDict = new Dictionary<byte, SendMotorData>();
            foreach (var kvp in action.motorDict)
            {
                SendMotorData data = new SendMotorData();
                data.id = kvp.Value.id;
                data.direction = kvp.Value.direction;
                data.speed = kvp.Value.speed;
                data.time = kvp.Value.time;
                data.lastDirection = kvp.Value.lastDirection;
                data.lastSpeed = kvp.Value.lastSpeed;
                motorDict[data.id] = data;
            }
        } else
        {
            motorDict = null;
        }
        if (null != action.showList)
        {
            for (int i = 0, imax = action.showList.Count; i < imax; ++i)
            {
                if (null != robot && null != robot.GetAnDjData(action.showList[i]) && robot.GetAnDjData(action.showList[i]).modelType == ServoModel.Servo_Model_Angle)
                {
                    if (null == showList)
                    {
                        showList = new List<byte>();
                    }
                    showList.Add(action.showList[i]);
                }
            }
            if (showList != null && showList.Count == 0)
            {
                showList = null;
            }
        }
        else
        {
            showList = null;
        }
    }
    /// <summary>
    /// 判断动作是否有轮模式
    /// </summary>
    /// <returns></returns>

    public bool IsTrunModel()
    {
        if (null != motorDict && motorDict.Count > 0)
        {
            foreach (KeyValuePair<byte, SendMotorData> kvp in motorDict)
            {
                if (kvp.Value.direction != TurnDirection.turnStop)
                {
                    return true;
                }
            }
        }
        if (turnDict.Count > 0)
        {
            foreach (KeyValuePair<byte, TurnData> kvp in turnDict)
            {
                if (kvp.Value.turnDirection != TurnDirection.turnStop)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HaveMotor()
    {
        if (null != motorDict && motorDict.Count > 0)
        {
            return true;
        }
        return false;
    }

    public bool HaveTurnServo()
    {
        if (null != turnDict && turnDict.Count > 0)
        {
            return true;
        }
        return false;
    }

    public void AddShowID(int id)
    {
        if (null == showList)
        {
            showList = new List<byte>();
        }
        showList.Add((byte)id);
    }

    public void RomoveShowID(int id)
    {
        if (null != showList)
        {
            showList.Remove((byte)id);
        }
    }

    public List<byte> GetShowID()
    {
        return showList;
    }

    public void CleanUp()
    {
        idMin = 0;
        idMinRota = 0;
        rotas.Clear();
        turnDict.Clear();
        showList.Clear();
        showList = null;
    }
    /// <summary>
    /// 把数据转换成xml节点
    /// </summary>
    /// <param name="xe"></param>
    /// <returns></returns>
    public XmlElement ConvertToNode(XmlElement xe)
    {
        if (null != xe)
        {
            xe.SetAttribute("index", index.ToString());
            xe.SetAttribute("sportTime", sportTime.ToString());
            xe.SetAttribute("waitTime", waitTime.ToString());
            xe.SetAttribute("idMin", idMin.ToString());
            xe.SetAttribute("idMinRota", idMinRota.ToString());
            xe.SetAttribute("rotas", RotasToString());
            xe.SetAttribute("turns", TurnDataToString());
            xe.SetAttribute("motors", MotorDataToString());
            xe.SetAttribute("show", ShowListToString());
        }
        return xe;
    }
    

#endregion
    


#region 私有函数
    private void Init(int index, int sportTime, int waitTime)
    {
        this.index = index;
        this.sportTime = sportTime;
        this.waitTime = waitTime;
        rotas = new Dictionary<byte, short>();
        turnDict = new Dictionary<byte, TurnData>();
    }

    private string RotasToString()
    {
        string str = string.Empty;
        foreach (KeyValuePair<byte, short> kvp in rotas)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str += ";";
            }
            str += kvp.Key + "$" + kvp.Value;
        }
        return str;
    }

    private string TurnDataToString()
    {
        string str = string.Empty;
        foreach (KeyValuePair<byte, TurnData> kvp in turnDict)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str += ";";
            }
            str += kvp.Key + "$" + (byte)kvp.Value.turnDirection + "," + kvp.Value.turnSpeed;
        }
        return str;
    }

    private string MotorDataToString()
    {
        string str = string.Empty;
        if (null != motorDict)
        {
            foreach (KeyValuePair<byte, SendMotorData> kvp in motorDict)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str += ";";
                }
                str += kvp.Key + "$" + (byte)kvp.Value.direction + "," + kvp.Value.speed + "," + kvp.Value.time * 100;
            }
        }
        
        return str;
    }


    private string ShowListToString()
    {
        string str = string.Empty;
        if (null != showList)
        {
            for (int i = 0, imax = showList.Count; i < imax; ++i)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str += ";";
                }
                str += showList[i];
            }
        }
        return str;
    }
#endregion

    
}

