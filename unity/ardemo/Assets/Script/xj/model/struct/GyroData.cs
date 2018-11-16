using Game.Platform;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:GyroData.cs
/// Description:陀螺仪传感器
/// Time:2016/10/8 15:34:10
/// </summary>
public class GyroData : ReadSensorDataBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<byte, GyroBackData> dataDict;
    #endregion

    #region 公有函数
    public GyroData()
    {
        dataDict = new Dictionary<byte, GyroBackData>();
    }

    public GyroData(List<byte> ids) : base (ids)
    {
        dataDict = new Dictionary<byte, GyroBackData>();
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            GyroBackData tmp = new GyroBackData();
            dataDict[ids[i]] = tmp;
        }
    }

    public override string GetReadResult()
    {
        List<Dictionary<string, object>> dict = new List<Dictionary<string, object>>();
        if (readIds[0] == 0)
        {
            foreach (KeyValuePair<byte, GyroBackData> kvp in dataDict)
            {
                Dictionary<string, object> tmp = new Dictionary<string, object>();
                tmp["id"] = kvp.Key;
                tmp["x"] = kvp.Value.direction.x;
                tmp["y"] = kvp.Value.direction.y;
                tmp["z"] = kvp.Value.direction.z;
                dict.Add(tmp);
            }
        }
        else
        {
            for (int i = 0, imax = readIds.Count; i < imax; ++i)
            {
                Dictionary<string, object> tmp = new Dictionary<string, object>();
                tmp["id"] = readIds[i];
                if (dataDict.ContainsKey(readIds[i]))
                {
                    tmp["x"] = dataDict[readIds[i]].direction.x;
                    tmp["y"] = dataDict[readIds[i]].direction.y;
                    tmp["z"] = dataDict[readIds[i]].direction.z;
                }
                else
                {
                    tmp["x"] = -1;
                    tmp["y"] = -1;
                    tmp["z"] = -1;
                }
                dict.Add(tmp);
            }
        }
        return Json.Serialize(dict);
    }

    public override object GetReadAllResult()
    {
        List<Dictionary<string, object>> dict = new List<Dictionary<string, object>>();
        if (null != readAllIds && readAllIds.Count > 0)
        {
            if (readAllIds[0] == 0)
            {
                foreach (KeyValuePair<byte, GyroBackData> kvp in dataDict)
                {
                    Dictionary<string, object> tmp = new Dictionary<string, object>();
                    tmp["id"] = kvp.Key;
                    tmp["x"] = kvp.Value.direction.x;
                    tmp["y"] = kvp.Value.direction.y;
                    tmp["z"] = kvp.Value.direction.z;
                    dict.Add(tmp);
                }
            }
            else
            {
                for (int i = 0, imax = readAllIds.Count; i < imax; ++i)
                {
                    Dictionary<string, object> tmp = new Dictionary<string, object>();
                    tmp["id"] = readAllIds[i];
                    if (dataDict.ContainsKey(readAllIds[i]))
                    {
                        tmp["x"] = dataDict[readAllIds[i]].direction.x;
                        tmp["y"] = dataDict[readAllIds[i]].direction.y;
                        tmp["z"] = dataDict[readAllIds[i]].direction.z;
                    }
                    else
                    {
                        tmp["x"] = -1;
                        tmp["y"] = -1;
                        tmp["z"] = -1;
                    }
                    dict.Add(tmp);
                }
            }
        }
        
        return dict;
    }

    public override void ReadCallBackMsg(BinaryReader br, int len)
    {
        try
        {
            base.ReadCallBackMsg(br, len);
            if (null != backIds)
            {
                for (int i = 0, imax = backIds.Count; i < imax; ++i)
                {
                    ReadGyroDataMsgAck msg = new ReadGyroDataMsgAck();
                    msg.Read(br);
                    if (dataDict.ContainsKey(backIds[i]))
                    {
                        dataDict[backIds[i]] = msg.gyroData;
                    }
                }
            }
            if (null != errIds && errIds.Count > 0)
            {
                for (int i = 0, imax = errIds.Count; i < imax; ++i)
                {
                    if (dataDict.ContainsKey(errIds[i]))
                    {
                        dataDict[errIds[i]].CleanUp();
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
    #endregion

    #region 其他函数
    #endregion
}

public class GyroVector
{
    public Int16 x;
    public Int16 y;
    public Int16 z;

    public GyroVector()
    {
        CleanUp();
    }

    public void CleanUp()
    {
        x = 0;
        y = 0;
        z = 0;
    }

}

public class GyroBackData
{
    public GyroVector direction;
    public GyroVector acceleration;
    public GyroVector gyro;

    public GyroBackData()
    {
        direction = new GyroVector();
        acceleration = new GyroVector();
        gyro = new GyroVector();
    }

    public void CleanUp()
    {
        if (null != direction)
        {
            direction.CleanUp();
        }
        if (null != acceleration)
        {
            acceleration.CleanUp();
        }
        if (null != gyro)
        {
            gyro.CleanUp();
        }
    }
}