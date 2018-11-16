using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ProtocolClient.cs
/// Description:
/// Time:2015/7/1 17:26:06
/// </summary>
public class ProtocolClient
{
    #region 公有属性
    #endregion

    #region 私有属性
    static ProtocolClient mInst = null;
    Dictionary<string, Dictionary<CMDCode, OnRobotDelegate>> mNetCallBack = null;
    Dictionary<string, Dictionary<CMDCode, Dictionary<ExtendCMDCode, OnRobotDelegate>>> mClientCallBack = null;
    #endregion

    #region 公有函数
    public static ProtocolClient GetInst()
    {
        if (null == mInst)
        {
            mInst = new ProtocolClient();
        }
        return mInst;
    }
    /// <summary>
    /// 注册消息回调
    /// </summary>
    /// <param name="mac">机器人的mac地址</param>
    /// <param name="cmd"></param>
    /// <param name="callBack"></param>
    public void Register(string mac, CMDCode cmd, OnRobotDelegate delg)
    {
        if (!mNetCallBack.ContainsKey(mac))
        {
            Dictionary<CMDCode, OnRobotDelegate> tmp = new Dictionary<CMDCode, OnRobotDelegate>();
            mNetCallBack.Add(mac, tmp);
        }
        mNetCallBack[mac][cmd] = delg;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mac"></param>
    /// <param name="cmd"></param>
    /// <param name="exCmd"></param>
    /// <param name="dlgt"></param>
    public void Register(string mac, CMDCode cmd, ExtendCMDCode exCmd, OnRobotDelegate dlgt)
    {
        if (null == mClientCallBack)
        {
            mClientCallBack = new Dictionary<string, Dictionary<CMDCode, Dictionary<ExtendCMDCode, OnRobotDelegate>>>();
        }
        if (!mClientCallBack.ContainsKey(mac))
        {
            Dictionary<CMDCode, Dictionary<ExtendCMDCode, OnRobotDelegate>> dict = new Dictionary<CMDCode, Dictionary<ExtendCMDCode, OnRobotDelegate>>();
            mClientCallBack[mac] = dict;
        }
        if (!mClientCallBack[mac].ContainsKey(cmd))
        {
            Dictionary<ExtendCMDCode, OnRobotDelegate> dict = new Dictionary<ExtendCMDCode, OnRobotDelegate>();
            mClientCallBack[mac][cmd] = dict;
        }
        mClientCallBack[mac][cmd][exCmd] = dlgt;
    }
    /// <summary>
    /// 移除某个机器人的所有回调
    /// </summary>
    /// <param name="mac"></param>
    public void RemoveRobotDlgt(string mac)
    {
        mNetCallBack.Remove(mac);
        if (null != mClientCallBack)
        {
            mClientCallBack.Remove(mac);
        }
    }
    /// <summary>
    /// 处理回调，之所以要传入长度是因为有些消息需要通过长度来判断是否成功
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="len"></param>
    /// <param name="br"></param>
    public bool OnMsgDelegate(CMDCode cmd, int len, string mac, BinaryReader br, ExtendCMDCode exCmd)
    {
        if (mNetCallBack.ContainsKey(mac))
        {
            if (mNetCallBack[mac].ContainsKey(cmd) && null != mNetCallBack[mac][cmd])
            {
                return mNetCallBack[mac][cmd](len, br, exCmd);
            }
        }
        if (null != mClientCallBack && mClientCallBack.ContainsKey(mac) && mClientCallBack[mac].ContainsKey(cmd) && mClientCallBack[mac][cmd].ContainsKey(exCmd))
        {
            return mClientCallBack[mac][cmd][exCmd](len, br, exCmd);
        }
        return true;
    }
    #endregion

    #region 私有函数
    ProtocolClient()
    {
        mNetCallBack = new Dictionary<string, Dictionary<CMDCode, OnRobotDelegate>>();
    }
    #endregion
}

public delegate bool OnRobotDelegate(int len, BinaryReader br, ExtendCMDCode exCmd);
/// <summary>
/// 命令
/// </summary>
public enum CMDCode : byte
{
    /// <summary>
    /// 握手命令,可让主板重新获取机器信息
    /// </summary>
    Hand_Shake = (byte)0x01,
    /// <summary>
    /// 改名
    /// </summary>
    Change_Name = (byte)0x02,
    /// <summary>
    /// 自检
    /// </summary>
    Self_Check = (byte)0x05,
    /// <summary>
    /// 控制舵机连续旋转
    /// </summary>
    DuoJi_Turn = (byte)0x07,
    /// <summary>
    /// 读取主板信息
    /// </summary>
    Read_Motherboard_Data = (byte)0x08,
    /// <summary>
    /// 发送动作命令
    /// </summary>
    Ctrl_Action = (byte)0x09,
    /// <summary>
    /// 角度回读命令
    /// </summary>
    Read_Back = (byte)0x0B,
    /// <summary>
    /// 修改id
    /// </summary>
    Change_ID = (byte)0x0C,
    /// <summary>
    /// 进入写外部Flash模式
    /// </summary>
    Flash_Start = (byte)0x10,
    /// <summary>
    /// 写入动作
    /// </summary>
    Flash_Write = (byte)0x11,
    /// <summary>
    /// 停止写入
    /// </summary>
    Flash_Stop = (byte)0x12,
    /// <summary>
    /// 写入完成
    /// </summary>
    Flash_End = (byte)0x13,
    /// <summary>
    /// 执行主板的动作
    /// </summary>
    Play_Flash = (byte)0x14,
    /// <summary>
    /// 暂停动作
    /// </summary>
    Pause_Action = (byte)0x15,
    /// <summary>
    /// 继续动作
    /// </summary>
    Continue_Action = (byte)0x16,
    /// <summary>
    /// 停止动作
    /// </summary>
    Stop_Action = (byte)0x17,
    /// <summary>
    /// 进入主板在线升级模式
    /// </summary>
    Robot_Update_Start = (byte)0x1A,
    /// <summary>
    /// 完成主板在线升级模式
    /// </summary>
    Robot_Update_Finish = (byte)0x1B,
    /// <summary>
    /// 主板在线升级指令
    /// </summary>
    Robot_Update_Write = (byte)0x1C,
    /// <summary>
    /// 取消主板在线升级
    /// </summary>
    Robot_Update_Stop = (byte)0x1D,
    /// <summary>
    /// 主板重启后开始升级回复命令
    /// </summary>
    Robot_Restart_Update_Start_Ack = (byte)0x1E,
    /// <summary>
    /// 主板重启后升级结束回复命令
    /// </summary>
    Robot_Restart_Update_Finish_Ack = (byte)0x1F,
    /// <summary>
    /// 读取系统版本号
    /// </summary>
    Read_System_Version = (byte)0x22,
    /// <summary>
    /// 舵机升级开始
    /// </summary>
    Servo_Update_Start = (byte)0x23,
    /// <summary>
    /// 舵机升级写入
    /// </summary>
    Servo_Update_Write = (byte)0x24,
    /// <summary>
    /// 停止舵机升级
    /// </summary>
    Servo_Update_Stop = (byte)0x25,
    /// <summary>
    /// 舵机写入完成
    /// </summary>
    Servo_Update_Finish = (byte)0x26,
    /// <summary>
    /// 读取所有动作列表
    /// </summary>
    Read_All_Flash = (byte)0x28,
    /// <summary>
    /// 循环播放所有动作
    /// </summary>
    While_All_Action = (byte)0x29,
    /// <summary>
    /// 写配置参数到主IC Flash
    /// </summary>
    Write_IC_Flash = (byte)0x2A,
    /// <summary>
    /// 从主IC Flash中读配置参数
    /// </summary>
    Read_IC_Flash = (byte)0x2B,
    /// <summary>
    /// 读取MCU芯片的ID号
    /// </summary>
    Read_MCU_ID = (byte)0x2C,
    /// <summary>
    /// 擦除主IC Flash中所有的配置参数
    /// </summary>
    Flush_IC_Flash = (byte)0x2D,
    /// <summary>
    /// 读取电量
    /// </summary>
    Read_System_Power = (byte)0x27,
    /// <summary>
    /// 读取设备类型，是不是jimu主板
    /// </summary>
    Read_Device_Type = (byte)0x36,
    /// <summary>
    /// 控制舵机运动360度
    /// </summary>
    Ctrl_Servos_Move = (byte)0x37,
    /// <summary>
    /// 弱锁位指令
    /// </summary>
    Weak_Latches = (byte)0x38,
    /// <summary>
    /// 打开或者关闭蓝牙通讯超时机制
    /// </summary>
    Set_BLE_OutTime = (byte)0x3A,
    /// <summary>
    /// 自动修复舵机异常提示
    /// </summary>
    Repair_Servo_Exception = (byte)0x3B,
    /// <summary>
    /// 开启或者关闭传感器传输功能
    /// </summary>
    Set_Sensor_IO_State = (byte)0x71,
    /// <summary>
    /// 获取传感器数据
    /// </summary>
    Read_Sensor_Data = (byte)0x72,
    /// <summary>
    /// 修改传感器id
    /// </summary>
    Change_Sensor_ID = (byte)0x74,
    /// <summary>
    /// 读取传感器id，用于判断该传感器是否存在
    /// </summary>
    Read_sensor_ID = (byte)0x75,
    /// <summary>
    /// 发送数据到传感器
    /// </summary>
    Send_Sensor_Data = (byte)0x78,
    /// <summary>
    /// 发送灯光数据
    /// </summary>
    Send_Light_Data = (byte)0x79,
    /// <summary>
    /// RGB灯珠显示
    /// </summary>
    Send_RgbLight_Data = (byte)0x7C,
    /// <summary>
    /// 读取传感器数据信息（可以同时读取多个数据）
    /// </summary>
    Read_Sensor_Data_Other = (byte)0x7E,
    /// <summary>
    /// 控制传感器的LED显示
    /// </summary>
    Ctrl_Sensor_LED = (byte)0x7F,
    /// <summary>
    /// 开始传感器升级
    /// </summary>
    Sensor_Update_Start = (byte)0x80,
    /// <summary>
    /// 传感器升级文件写入
    /// </summary>
    Sensor_Update_Write = (byte)0x81,
    /// <summary>
    /// 取消传感器升级
    /// </summary>
    Sensor_Update_Stop = (byte)0x82,
    /// <summary>
    /// 传感器升级完成
    /// </summary>
    Sensor_Update_Finish = (byte)0x83,
    /// <summary>
    /// 控制马达
    /// </summary>
    Ctrl_Motor = (byte)0x90,
    /// <summary>
    /// 停止马达
    /// </summary>
    Stop_Motor = (byte)0x91,
    /// <summary>
    /// 修复马达异常提示
    /// </summary>
    Repair_Motor_Exception = (byte)0x92,
}

public enum ExtendCMDCode : byte
{
    Extend_Code_None,
    CtrlServoMove,
    CtrlActionForDjId,
    CtrlAction,
    CtrlActionPowerOn,
    CtrlServoTurn,
    CtrlServoStop,
    TurnAction,
    ServoPowerDown,
    ServoPowerOn,
    ReadBack,
    RobotPowerDown,
    ReadConnectedAngle,
    LogicGetPosture,
    ReadAllSensorData,
    ReadInfraredData,
    ReadTouchData,
    ReadGyroData,
    ReadLightData,
    ReadGravityData,
    ReadUltrasonicData,
    ReadDigitalTubeData,
    ReadSpeakerData,
    SendEmojiData,
    SendLedsData,
    SendUltrasonicData,
    SendDigitalTubeData,
    Set_Sensor_IO_State,
    Check_Sensor_ID,
    SendRgbLight_Color,
    SendRgbLight_Emoji,
}

/// <summary>
/// 错误码
/// </summary>
public enum ErrorCode
{
    Result_OK               = 0,//正确
    Result_DJ_ID_Error      = -1,//错误的舵机id
    Result_DJ_ID_Discontinuous = -2,//id不连续
    Result_DJ_ID_Repeat = -3,//舵机ID重复
    Result_Servo_Num_Inconsistent = -4,//舵机数量不一致
    Result_Servo_ID_Inconsistent = -5,//舵机id不一致
    Result_Sensor_Exception = -6,//传感器异常
    Servo_Model_Type_Error = -7,  //舵机模式错误
    Servo_Version_inconsistent = -8,//舵机版本不一致

    Result_Motor_Num_Inconsistent = -9,//马达数量不一致
    Result_Motor_ID_Inconsistent = -10,//马达id不匹配
    Result_Motor_ID_Repeat = -11,//马达id重复
    Result_Sensor_ID_Repeat = -12,//传感器id重复
    Result_Sensor_Have_Speaker = -13,//连接了蓝牙音箱

    Result_Name_Empty       = -100,//名字为空
    Result_Name_Exist       = -101,//名字已存在

    Result_Port_Exist       = -200,//已存在的连接口

    Can_Upgraded            = -300,//能够升级
    Robot_Power_Low         = -301,//电量过低
    Robot_Adapter_Close_Protect = -302,//在充电且关闭了充电保护
    Robot_Adapter_Open_Protect  = -303,//在充电且打开了充电保护
    Robot_Sensor_ID_Repeat  = -304,//传感器id重复

    Result_Blue_Dis         = -1000,//蓝牙已断开
    Parameter_Error         = -1001,//参数错误
    Unknown_Error           = -1002,//未知异常
    Blue_Back_Data_Error    = -1003,//蓝牙返回数据错误
    Blue_Back_Time_Out      = -1004,//通讯超时
    Result_Action_Not_Exist = -1005,//动作不存在
}