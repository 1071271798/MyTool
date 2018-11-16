using Game;
using Game.Platform;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:TouchData.cs
/// Description:触碰传感器
/// Time:2016/10/8 15:33:36
/// </summary>
public class TouchData : ReadSensorDataBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<byte, byte> dataDict;
    Dictionary<byte, byte> mTouchValueDict;
    Dictionary<byte, long> mResetValueIndexDict;
    #endregion

    #region 公有函数
    public TouchData()
    {
        dataDict = new Dictionary<byte, byte>();
        mTouchValueDict = new Dictionary<byte, byte>();
        mResetValueIndexDict = new Dictionary<byte, long>();
    }

    public TouchData(List<byte> ids) : base(ids)
    {
        dataDict = new Dictionary<byte, byte>();
        mTouchValueDict = new Dictionary<byte, byte>();
        mResetValueIndexDict = new Dictionary<byte, long>();
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            dataDict[ids[i]] = 0;
            mTouchValueDict[ids[i]] = 0;
        }
    }

    public override string GetReadResult()
    {
        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
        List<byte> ids = new List<byte>();
        if (readIds[0] == 0)
        {
            foreach (KeyValuePair<byte, byte> kvp in mTouchValueDict)
            {
                ids.Add(kvp.Key);
            }
        }
        else
        {
            ids = readIds;
        }
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            byte id = ids[i];
            if (mTouchValueDict.ContainsKey(id))
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["id"] = id;
                dict["result"] = mTouchValueDict[id];
                mTouchValueDict[id] = 0;
                if (mResetValueIndexDict.ContainsKey(id) && -1 != mResetValueIndexDict[id])
                {
                    Timer.Cancel(mResetValueIndexDict[id]);
                    mResetValueIndexDict[id] = -1;
                }
                result.Add(dict);
            }
        }
        return Json.Serialize(result);
    }

    public override object GetReadAllResult()
    {
        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
        if (null != readAllIds && readAllIds.Count > 0)
        {
            if (readAllIds[0] == 0)
            {
                foreach (KeyValuePair<byte, byte> kvp in dataDict)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict["id"] = kvp.Key;
                    dict["result"] = kvp.Value;
                    result.Add(dict);
                }
            }
            else
            {
                for (int i = 0, imax = readAllIds.Count; i < imax; ++i)
                {
                    if (dataDict.ContainsKey(readAllIds[i]))
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict["id"] = readAllIds[i];
                        dict["result"] = dataDict[readAllIds[i]];
                        result.Add(dict);
                    }
                }
            }
        }
        
        return result;
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
                    ReadTouchDataMsgAck msg = new ReadTouchDataMsgAck();
                    msg.Read(br);
                    byte id = backIds[i];
                    if (dataDict.ContainsKey(id))
                    {
                        dataDict[id] = msg.arg;
                    }
                    if (msg.arg > 0 && mTouchValueDict.ContainsKey(id))
                    {
                        mTouchValueDict[id] = msg.arg;
                        if (mResetValueIndexDict.ContainsKey(id) && -1 != mResetValueIndexDict[id])
                        {
                            Timer.Cancel(mResetValueIndexDict[id]);
                            mResetValueIndexDict[id] = -1;
                        }
                        mResetValueIndexDict[id] = Timer.Add(3, 1, 1, ResetTouchValue, id);
                    }
                }
            }
            if (null != errIds)
            {
                for (int i = 0, imax = errIds.Count; i < imax; ++i)
                {
                    byte id = errIds[i];
                    if (dataDict.ContainsKey(id))
                    {
                        dataDict[id] = 0;
                    }
                    if (mTouchValueDict.ContainsKey(id))
                    {
                        mTouchValueDict[id] = 0;
                        if (mResetValueIndexDict.ContainsKey(id) && -1 != mResetValueIndexDict[id])
                        {
                            Timer.Cancel(mResetValueIndexDict[id]);
                            mResetValueIndexDict[id] = -1;
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
    #endregion

    #region 其他函数
    void ResetTouchValue(params object[] arg)
    {
        byte id = (byte)arg[0];
        if (mTouchValueDict.ContainsKey(id))
        {
            mTouchValueDict[id] = 0;
        }
        if (mResetValueIndexDict.ContainsKey(id))
        {
            mResetValueIndexDict[id] = -1;
        }
    }
    #endregion
}