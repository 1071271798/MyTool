using Game.Platform;
using LitJson;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 颜色传感器
/// </summary>
public class ColorData : ReadSensorDataBase
{

    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<byte, ColorSensorAckData> dataDict;
    #endregion

    #region 公有函数
    public ColorData()
    {
        dataDict = new Dictionary<byte, ColorSensorAckData>();
    }

    public ColorData(List<byte> ids) : base (ids)
    {
        dataDict = new Dictionary<byte, ColorSensorAckData>();
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            ColorSensorAckData tmp = new ColorSensorAckData();
            dataDict[ids[i]] = tmp;
        }
    }

    public override string GetReadResult()
    {
        List<Dictionary<string, object>> dict = new List<Dictionary<string, object>>();
        if (readIds[0] == 0)
        {
            foreach (var kvp in dataDict)
            {
                Dictionary<string, object> tmp = new Dictionary<string, object>();
                tmp["id"] = kvp.Key;
                tmp["color"] = PublicFunction.ConverRgtToColor(kvp.Value.color_r, kvp.Value.color_g, kvp.Value.color_b);
                tmp["realColor"] = PublicFunction.RGBToHexString(kvp.Value.color_r, kvp.Value.color_g, kvp.Value.color_b);
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
                    tmp["color"] = PublicFunction.ConverRgtToColor(dataDict[readIds[i]].color_r, dataDict[readIds[i]].color_g, dataDict[readIds[i]].color_b);
                    tmp["realColor"] = PublicFunction.RGBToHexString(dataDict[readAllIds[i]].color_r, dataDict[readAllIds[i]].color_g, dataDict[readAllIds[i]].color_b);
                }
                else
                {
                    tmp["color"] = string.Empty;
                    tmp["realColor"] = string.Empty;
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
                foreach (var kvp in dataDict)
                {
                    Dictionary<string, object> tmp = new Dictionary<string, object>();
                    tmp["id"] = kvp.Key;
                    tmp["color"] = PublicFunction.ConverRgtToColor(kvp.Value.color_r, kvp.Value.color_g, kvp.Value.color_b);
                    tmp["realColor"] = PublicFunction.RGBToHexString(kvp.Value.color_r, kvp.Value.color_g, kvp.Value.color_b);
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
                        tmp["color"] = PublicFunction.ConverRgtToColor(dataDict[readAllIds[i]].color_r, dataDict[readAllIds[i]].color_g, dataDict[readAllIds[i]].color_b);
                        tmp["realColor"] = PublicFunction.RGBToHexString(dataDict[readAllIds[i]].color_r, dataDict[readAllIds[i]].color_g, dataDict[readAllIds[i]].color_b);
                    }
                    else
                    {
                        tmp["color"] = string.Empty;
                        tmp["realColor"] = string.Empty;
                    }
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "id = " + readAllIds[i] + " r = " + dataDict[readAllIds[i]].color_r + " g = " + dataDict[readAllIds[i]].color_g + " b = " + dataDict[readAllIds[i]].color_b + " color = " + tmp["color"] + " realColor = " + tmp["realColor"]);
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
                    ReadColorDataMsgAck msg = new ReadColorDataMsgAck();
                    msg.Read(br);
                    if (dataDict.ContainsKey(backIds[i]))
                    {
                        dataDict[backIds[i]] = msg.colorData;
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
            //GetReadAllResult();
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

//////////////////////////////////////////////////////////////////////////
public class ColorSensorAckData
{
    public byte color_r;
    public byte color_g;
    public byte color_b;
    public byte AD1_H;
    public byte AD1_L;
    public byte AD2_H;
    public byte AD2_L;
    public byte AD3_H;
    public byte AD3_L;
    public byte AD4_H;
    public byte AD4_L;

    public ColorSensorAckData()
    {

    }

    public void CleanUp()
    {
        color_r = 0;
        color_g = 0;
        color_b = 0;
        AD1_H = 0;
        AD1_L = 0;
        AD2_H = 0;
        AD2_L = 0;
        AD3_H = 0;
        AD3_L = 0;
        AD4_H = 0;
        AD4_L = 0;
    }

}