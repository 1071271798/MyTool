using Game.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:TopologyPartData.cs
/// Description:拓扑图零件数据
/// Time:2016/7/19 10:36:55
/// </summary>
public class TopologyPartData
{
    #region 公有属性
    /// <summary>
    /// id
    /// </summary>
    public byte id;
    /// <summary>
    /// 零件类型
    /// </summary>
    public TopologyPartType partType;
    /// <summary>
    /// true表示未配置，处于独立队列
    /// </summary>
    public bool isIndependent;
    /// <summary>
    /// 相对位置
    /// </summary>
    public Vector2 localPosition;
    /// <summary>
    /// 记录旋转角度
    /// </summary>
    public Vector3 localEulerAngles;
    /// <summary>
    /// 零件宽度
    /// </summary>
    public int width;
    /// <summary>
    /// 零件长度
    /// </summary>
    public int height;
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    public TopologyPartData()
    {
        id = 0;
        partType = TopologyPartType.Servo;
        isIndependent = false;
        localPosition = Vector3.zero;
        localEulerAngles = Vector3.zero;
        width = 0;
        height = 0;
    }

    public TopologyPartData(TopologyPartData data)
    {
        this.id = data.id;
        this.partType = data.partType;
        this.isIndependent = data.isIndependent;
        this.localPosition = data.localPosition;
        this.localEulerAngles = data.localEulerAngles;
        this.width = data.width;
        this.height = data.height;
    }

    public TopologyPartData(string data)
    {
        try
        {
            string[] ary = data.Split(' ');
            for (int i = 0, imax = ary.Length; i < imax; ++i)
            {
                switch (i)
                {
                    case 0:
                        id = PublicFunction.IsInteger(ary[i]) ? byte.Parse(ary[i]) : (byte)0;
                        break;
                    case 1:
                        if (PublicFunction.IsInteger(ary[i]))
                        {
                            byte num = byte.Parse(ary[i]);
                            if (num >= 8)
                            {//兼容以前的数据
                                num += 1;
                            }
                            partType = (TopologyPartType)num;
                        }
                        else
                        {
                            try
                            {
                                partType = (TopologyPartType)Enum.Parse(typeof(TopologyPartType), ary[i]);
                            }
                            catch (System.Exception ex)
                            {
                                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "- TopologyPartData " + "- error = " + ex.ToString());
                            }
                        }
                        break;
                    case 2:
                        try
                        {
                            isIndependent = bool.Parse(ary[i]);
                        }
                        catch (System.Exception ex)
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "- TopologyPartData " +  "- error = " + ex.ToString());
                        }
                        
                        break;
                    case 3:
                        List<float> list = PublicFunction.StringToFloatList(ary[i]);
                        if (list.Count >= 2)
                        {
                            localPosition.x = list[0];
                            localPosition.y = list[1];
                        }
                        break;
                    case 4:
                        List<float> list1 = PublicFunction.StringToFloatList(ary[i]);
                        if (list1.Count >= 3)
                        {
                            localEulerAngles.x = list1[0];
                            localEulerAngles.y = list1[1];
                            localEulerAngles.z = list1[2];
                        }
                        break;
                    case 5:
                        try
                        {
                            width = int.Parse(ary[i]);
                        }
                        catch (System.Exception ex)
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "- TopologyPartData " + "- error = " + ex.ToString());
                        }
                        break;
                    case 6:
                        try
                        {
                            height = int.Parse(ary[i]);
                        }
                        catch (System.Exception ex)
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "- TopologyPartData " + "- error = " + ex.ToString());
                        }
                        break;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(id);
        sb.Append(' ');
        sb.Append(partType.ToString());
        sb.Append(' ');
        sb.Append(isIndependent);
        sb.Append(' ');
        sb.Append(localPosition.x);
        sb.Append(PublicFunction.Separator_Comma);
        sb.Append(localPosition.y);
        sb.Append(' ');
        sb.Append(localEulerAngles.x);
        sb.Append(PublicFunction.Separator_Comma);
        sb.Append(localEulerAngles.y);
        sb.Append(PublicFunction.Separator_Comma);
        sb.Append(localEulerAngles.z);
        sb.Append(' ');
        sb.Append(width);
        sb.Append(' ');
        sb.Append(height);
        return sb.ToString();
    }

    public static byte ConvertToSensorType(TopologyPartType partType)
    {
        switch (partType)
        {
            case TopologyPartType.Infrared://红外
                return 1;
            case TopologyPartType.Touch://触碰
                return 2;
            case TopologyPartType.Gyro://陀螺仪
                return 3;
            case TopologyPartType.Light://灯光
                return 4;
            case TopologyPartType.Gravity://重力
                return 5;
            case TopologyPartType.Ultrasonic://超声
                return 6;
            case TopologyPartType.DigitalTube://数码管
                return 7;
            case TopologyPartType.Speaker://喇叭
                return 8;
            case TopologyPartType.EnLight://光感
                return 12;
            case TopologyPartType.Atmosphere://大气压
                break;
            case TopologyPartType.Sound://声音
                return 13;
            case TopologyPartType.Temperature://温湿度
                return 11;
            case TopologyPartType.Color://颜色
                return 9;
            case TopologyPartType.Motor://马达
                return 10;
            case TopologyPartType.RgbLight://rgb灯
                return 14;
            default:
                return 0;
        }
        return 0;
    }
    
    
    public static TopologyPartType ConvertToPartType(byte sensorType)
    {
        switch (sensorType)
        {
            case 1://红外
                return TopologyPartType.Infrared;
            case 2://触碰
                return TopologyPartType.Touch;
            case 3://陀螺仪
                return TopologyPartType.Gyro;
            case 4://灯光
                return TopologyPartType.Light;
            case 5://重力
                return TopologyPartType.Gravity;
            case 6://超声
                return TopologyPartType.Ultrasonic;
            case 7://数码管
                return TopologyPartType.DigitalTube;
            case 8://喇叭
                return TopologyPartType.Speaker;
            case 12://光感
                return TopologyPartType.EnLight;
            /*case TopologyPartType.Atmosphere://大气压
                break;*/
            case 13://声音
                return TopologyPartType.Sound;
            case 11://温湿度
                return TopologyPartType.Temperature;
            case 9://颜色
                return TopologyPartType.Color;
            case 10://马达
                return TopologyPartType.Motor;
            case 14://rgb灯
                return TopologyPartType.RgbLight;
            default:
                return TopologyPartType.None;
        }
    }
    #endregion

    #region 其他函数
    #endregion
}
/// <summary>
/// 零件接口数据
/// </summary>
public class PartPortData
{
    public PartPortType portType;
    public GameObject portObj;

    public PartPortData()
    {

    }
}

/// <summary>
/// 拓扑图零件类型
/// </summary>
public enum TopologyPartType : byte
{
    None = 0,
    Infrared = 1,//红外
    Touch = 2,//触碰
    Gyro = 3,//陀螺仪
    Light = 4,//灯光
    Gravity = 5,//重力
    Ultrasonic = 6,//超声
    DigitalTube = 7,//数码管
    Speaker,//蓝牙喇叭
    EnLight,//光感
    Atmosphere,//大气压
    Sound,//声音
    Temperature,//温湿度
    Color,//颜色
    RgbLight,//RGB角灯
    Motor,//马达
    MainBoard,//主板
    MainBoard_new_low,//新主板
    Servo,//舵机
    Line,//直线
    Line_Angle,//拐角    
}

/// <summary>
/// 零件接口类型
/// </summary>
public enum PartPortType : byte
{
    Port_Type_Pin_3,
    Port_Type_Pin_4
}