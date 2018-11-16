using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:SpeakerData.cs
/// Description:
/// Time:2016/10/20 10:18:20
/// </summary>
public class SpeakerData : ReadSensorDataBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<byte, SpeakerInfoData> dataDict;
    #endregion

    #region 公有函数
    public SpeakerData()
    {
        dataDict = new Dictionary<byte, SpeakerInfoData>();
    }

    public SpeakerData(List<byte> ids) : base (ids)
    {
        dataDict = new Dictionary<byte, SpeakerInfoData>();
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            SpeakerInfoData tmp = new SpeakerInfoData();
            dataDict[ids[i]] = tmp;
        }
    }

    public SpeakerInfoData GetSpeakerData(byte id)
    {
        if (dataDict.ContainsKey(id))
        {
            return dataDict[id];
        }
        return null;
    }

    public SpeakerInfoData GetSpeakerData()
    {
        if (null != dataDict)
        {
            foreach (var kvp in dataDict)
            {
                return kvp.Value;
            }
        }
        return null;
    }

    public override string GetReadResult()
    {
        return base.GetReadResult();
    }

    public override void ReadCallBackMsg(BinaryReader br, int len)
    {
        try
        {
            base.ReadCallBackMsg(br, len);
            ReadSpeakerDataMsgAck msg = new ReadSpeakerDataMsgAck(len - 2);
            msg.Read(br);
            if (null != backIds)
            {
                for (int i = 0, imax = backIds.Count; i < imax; ++i)
                {
                    if (dataDict.ContainsKey(backIds[i]))
                    {
                        dataDict[backIds[i]] = msg.data;
                        //EventMgr.Inst.Fire(EventID.Read_Speaker_Data_Ack, new EventArg(backIds[i]));
                    }
                }
            }
            if (null != errIds)
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


public class SpeakerInfoData
{
    public string speakerName;
    public string speakerMac;

    public SpeakerInfoData()
    {
        speakerName = string.Empty;
        speakerMac = string.Empty;
    }

    public void CleanUp()
    {
        speakerName = string.Empty;
        speakerMac = string.Empty;
    }
}