//#define Update_Test
//#define Update_Test1
using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:UpdateManager.cs
/// Description:升级管理
/// Time:2016/11/24 10:45:01
/// </summary>
public class UpdateManager : SingletonObject<UpdateManager>
{
    #region 公有属性

#if Update_Test
    public string Robot_System_Version = "Jimu_p1.30";
    public string Robot_System_FilePath = "Jimu2primary_P1.30";
    public string Robot_Servo_Version = "41161301";
    public string Robot_Servo_FilePath = "jimu2_app_41161301";
    public string Robot_Infrared_Version = "14161215";
    public string Robot_Infrared_FilePath = "Jimu2_infrared_sensor_14161215";
    public string Robot_Touch_Version = "14161215";
    public string Robot_Touch_FilePath = "Jimu2_touch_sensor_14161215";
    public string Robot_Gyro_Version = "13161215";
    public string Robot_Gyro_FilePath = "Jimu2_gyros_sensor_13161215";
    public string Robot_Speaker_Version = "01161215";
    public string Robot_Speaker_FilePath = "Jimu2_speaker_sensor_01161215";
    public string Robot_DigitalTube_Version = "02161215";
    public string Robot_DigitalTube_FilePath = "Jimu2_digital_sensor_02161215";
#elif Update_Test1
    public string Robot_System_Version = "Jimu_p1.37";
    public string Robot_System_FilePath = "Jimu2primary_p1.37";
    public string Robot_Servo_Version = "41165101";
    public string Robot_Servo_FilePath = "jimu2_app_41165101";
    public string Robot_Infrared_Version = "14161215";
    public string Robot_Infrared_FilePath = "Jimu2_infrared_sensor_14161215";
    public string Robot_Touch_Version = "14161215";
    public string Robot_Touch_FilePath = "Jimu2_touch_sensor_14161215";
    public string Robot_Gyro_Version = "13161215";
    public string Robot_Gyro_FilePath = "Jimu2_gyros_sensor_13161215";
    public string Robot_Speaker_Version = "01161215";
    public string Robot_Speaker_FilePath = "Jimu2_speaker_sensor_01161215";
    public string Robot_DigitalTube_Version = "02161215";
    public string Robot_DigitalTube_FilePath = "Jimu2_digital_sensor_02161215";
#else
    /// <summary>
    /// 系统主板版本
    /// </summary>
    /*public string Robot_System_Version = string.Empty;
    /// <summary>
    /// 系统主板程序路径
    /// </summary>
    public string Robot_System_FilePath = string.Empty;
    /// <summary>
    /// 系统舵机版本
    /// </summary>
    public string Robot_Servo_Version = "41165101";
    /// <summary>
    /// 系统舵机程序路径
    /// </summary>
    public string Robot_Servo_FilePath = "jimu2_app_41165101";
    /// <summary>
    /// 红外版本
    /// </summary>
    /*public string Robot_Infrared_Version = "14161215";
    /// <summary>
    /// 红外路径
    /// </summary>
    public string Robot_Infrared_FilePath = "Jimu2_infrared_sensor_14161215";
    /// <summary>
    /// 触碰版本
    /// </summary>
    public string Robot_Touch_Version = "14161215";
    /// <summary>
    /// 触碰路径
    /// </summary>
    public string Robot_Touch_FilePath = "Jimu2_touch_sensor_14161215";
    /// <summary>
    /// 陀螺仪版本
    /// </summary>
    public string Robot_Gyro_Version = "13161215";
    /// <summary>
    /// 陀螺仪路径
    /// </summary>
    public string Robot_Gyro_FilePath = "Jimu2_gyros_sensor_13161215";
    /// <summary>
    /// 蓝牙喇叭版本
    /// </summary>
    public string Robot_Speaker_Version = "01161215";
    /// <summary>
    /// 蓝牙喇叭路径
    /// </summary>
    public string Robot_Speaker_FilePath = "Jimu2_speaker_sensor_01161215";
    /// <summary>
    /// 数码管版本
    /// </summary>
    public string Robot_DigitalTube_Version = "02161215";
    /// <summary>
    /// 数码管路径
    /// </summary>
    public string Robot_DigitalTube_FilePath = "Jimu2_digital_sensor_02161215";*/
    /*public string Robot_Infrared_Version = string.Empty;
    public string Robot_Infrared_FilePath = string.Empty;
    public string Robot_Touch_Version = string.Empty;
    public string Robot_Touch_FilePath = string.Empty;
    public string Robot_Gyro_Version = string.Empty;
    public string Robot_Gyro_FilePath = string.Empty;
    public string Robot_Speaker_Version = string.Empty;
    public string Robot_Speaker_FilePath = string.Empty;
    public string Robot_DigitalTube_Version = string.Empty;
    public string Robot_DigitalTube_FilePath = string.Empty;
    public string Robot_Light_Version = string.Empty;
    public string Robot_Light_FilePath = string.Empty;
    public string Robot_Gravity_Version = string.Empty;
    public string Robot_Gravity_FilePath = string.Empty;
    public string Robot_Ultrasonic_Version = string.Empty;
    public string Robot_Ultrasonic_FilePath = string.Empty;*/
#endif
    #endregion

    #region 其他属性
    /// <summary>
    /// 舵机版本
    /// </summary>
    string Robot_Servo_Version = "41165101";
    /// <summary>
    /// 舵机程序路径
    /// </summary>
    string Robot_Servo_FilePath = "jimu2_app_41165101";
    TopologyPartType mUpdateDeviceType = TopologyPartType.None;
    TopologyPartType mLastSuccesUpdateDeviceType = TopologyPartType.None;
    UpdateState mUpdateState = UpdateState.State_Start;
    Dictionary<TopologyPartType, UpdateBase> mUpdateDataDict = null;
#endregion

#region 公有函数
    public UpdateManager()
    {
        mUpdateDataDict = new Dictionary<TopologyPartType, UpdateBase>();
    }

    /// <summary>
    /// 检查升级
    /// </summary>
    /// <param name="deviceType">升级的硬件类型</param>
    /// <param name="msg">主板信息</param>
    /// <returns></returns>
    public ErrorCode CheckUpdate(TopologyPartType deviceType, ReadMotherboardDataMsgAck msg)
    {
        CreateUpdate(deviceType);
        mLastSuccesUpdateDeviceType = TopologyPartType.None;
        if (mUpdateDataDict.ContainsKey(deviceType))
        {
            return mUpdateDataDict[deviceType].CheckUpdate(msg);
        }
        return ErrorCode.Result_OK;
    }

    /// <summary>
    /// 开始升级
    /// </summary>
    /// <param name="deviceType">升级的硬件类型</param>
    /// <param name="robot"></param>
    /// <param name="arg">升级参数</param>
    /// <returns>返回true表示能升级</returns>
    public bool UpdateStart(TopologyPartType deviceType, Robot robot, byte arg = 0)
    {
        CreateUpdate(deviceType);
        if (mUpdateDataDict.ContainsKey(deviceType))
        {
            mUpdateDeviceType = deviceType;
            mUpdateState = UpdateState.State_Start;
            return mUpdateDataDict[deviceType].UpdateStart(robot, arg);
        }
        return false;
    }
    /// <summary>
    /// 发送升级数据
    /// </summary>
    public void WriteFrame()
    {
        if (mUpdateDataDict.ContainsKey(mUpdateDeviceType))
        {
            mUpdateDataDict[mUpdateDeviceType].SendFrame();
            mUpdateDataDict[mUpdateDeviceType].UpdateProgress();
            mUpdateState = UpdateState.State_Write;
        }
    }
    /// <summary>
    /// 升级异常
    /// </summary>
    public void UpdateError(UpgradFailReson failReson)
    {
        mUpdateState = UpdateState.State_Fail;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(mUpdateDeviceType));
            if (mUpdateDeviceType == TopologyPartType.MainBoard || mUpdateDeviceType == TopologyPartType.MainBoard_new_low)
            {
            } else
            {
            }
            CleanUpdate();
        }
    }
    /// <summary>
    /// 发送升级文件完成，等待升级
    /// </summary>
    public void UpdateWait()
    {
        mUpdateState = UpdateState.State_Wait;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            EventMgr.Inst.Fire(EventID.Update_Wait, new EventArg(mUpdateDeviceType));
        }
    }
    /// <summary>
    /// 升级失败
    /// </summary>
    /// <param name="obj"></param>
    public void UpdateFail(List<byte> list)
    {
        mUpdateState = UpdateState.State_Fail;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            EventMgr.Inst.Fire(EventID.Update_Fail, new EventArg(mUpdateDeviceType, list));
            if (mUpdateDeviceType == TopologyPartType.MainBoard || mUpdateDeviceType == TopologyPartType.MainBoard_new_low)
            {
            }
            else
            {
            }
            CleanUpdate();
        }
    }
    /// <summary>
    /// 升级成功
    /// </summary>
    public void UpdateSucces()
    {
        mUpdateState = UpdateState.State_Success;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            mLastSuccesUpdateDeviceType = mUpdateDeviceType;
            EventMgr.Inst.Fire(EventID.Update_Finished, new EventArg(mUpdateDeviceType));
            if (mUpdateDeviceType == TopologyPartType.MainBoard || mUpdateDeviceType == TopologyPartType.MainBoard_new_low)
            {
            }
            else
            {
            }
            CleanUpdate();
        }
    }
    /// <summary>
    /// 升级超时
    /// </summary>
    public void UpdateOutTime()
    {
        mUpdateState = UpdateState.State_Fail;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(mUpdateDeviceType));
            if (mUpdateDeviceType == TopologyPartType.MainBoard || mUpdateDeviceType == TopologyPartType.MainBoard_new_low)
            {
            }
            else
            {
            }
            CleanUpdate();
        }
    }

    public void CannelUpdate()
    {
        if (TopologyPartType.None != mUpdateDeviceType)
        {
            if (mUpdateDeviceType == TopologyPartType.MainBoard_new_low)
            {
                SingletonObject<ConnectCtrl>.GetInst().CannelReconnect();
            }
            EventMgr.Inst.Fire(EventID.Update_Cannel, new EventArg(mUpdateDeviceType));
            if (mUpdateDeviceType == TopologyPartType.MainBoard || mUpdateDeviceType == TopologyPartType.MainBoard_new_low)
            {
            }
            else
            {
            }
            CleanUpdate();
            
        }
    }

    public bool IsSystemUpdateSucces()
    {
        if ((TopologyPartType.MainBoard == mLastSuccesUpdateDeviceType || TopologyPartType.MainBoard_new_low == mLastSuccesUpdateDeviceType) && UpdateState.State_Success == mUpdateState)
        {
            return true;
        }
        return false;
    }

    public UpdateState GetUpdateState()
    {
        return mUpdateState;
    }

    public TopologyPartType GetUpdateDeviceType()
    {
        return mUpdateDeviceType;
    }


    public string GetUpdateVersion()
    {
        return GetHardwareVersion(mUpdateDeviceType);
    }


    public string GetHardwareVersion(TopologyPartType partType)
    {
        string version = string.Empty;
        string key = PublicFunction.Hardware_Version_Name + partType.ToString();
        if (PlayerPrefs.HasKey(key))
        {
            version = PlayerPrefs.GetString(key);
        } else if (partType == TopologyPartType.Servo)
        {
            version = Robot_Servo_Version;
        }
        return version;
    }

    public string GetHardwarePath(TopologyPartType partType)
    {
        string path = string.Empty;
        string key = PublicFunction.Hardware_Version_Path + partType.ToString();
        if (PlayerPrefs.HasKey(key))
        {
            path = PlayerPrefs.GetString(key);
        } else if (partType == TopologyPartType.Servo)
        {
            path = Robot_Servo_FilePath;
        }
        return path;
    }

    public bool EqualsVersion(TopologyPartType partType, string version)
    {
        string saveVersion = GetHardwareVersion(partType);
        if (!string.IsNullOrEmpty(saveVersion) && !saveVersion.Equals(version))
        {
            return false;
        }
        return true;
    }

    public bool EqualsMainBoardVersion(ReadMotherboardDataMsgAck data)
    {
        if (null != data)
        {
            TopologyPartType mainPart = data.GetMainboardType();
            if (mainPart != TopologyPartType.None)
            {
                return EqualsVersion(mainPart, data.mbVersion);
            }
        }
        return true;
    }


    public override void CleanUp()
    {
        base.CleanUp();
        mUpdateDeviceType = TopologyPartType.None;
        mLastSuccesUpdateDeviceType = TopologyPartType.None;
        mUpdateState = UpdateState.State_Start;
        mUpdateDataDict.Clear();
    }


    public void ClearDefaultData()
    {
        Robot_Servo_FilePath = string.Empty;
        Robot_Servo_Version = string.Empty;
    }
    #endregion

    #region 其他函数

    void CreateUpdate(TopologyPartType deviceType)
    {
        if (!mUpdateDataDict.ContainsKey(deviceType))
        {
            switch (deviceType)
            {
                case TopologyPartType.MainBoard:
                case TopologyPartType.MainBoard_new_low:
                    UpdateSystem system = new UpdateSystem(deviceType);
                    mUpdateDataDict[deviceType] = system;
                    break;
                case TopologyPartType.Servo:
                    UpdateServo servo = new UpdateServo();
                    mUpdateDataDict[deviceType] = servo;
                    break;
                case TopologyPartType.Infrared:
                case TopologyPartType.Touch:
                case TopologyPartType.Gyro:
                case TopologyPartType.Light:
                case TopologyPartType.DigitalTube:
                case TopologyPartType.Speaker:
                    UpdateSensor sensor = new UpdateSensor(deviceType);
                    mUpdateDataDict[deviceType] = sensor;
                    break;
            }
        }
    }

    void CleanUpdate()
    {
        if (mUpdateDataDict.ContainsKey(mUpdateDeviceType))
        {
            mUpdateDataDict[mUpdateDeviceType].CleanUp();
            mUpdateDataDict.Remove(mUpdateDeviceType);
            mUpdateDeviceType = TopologyPartType.None;
        }
    }
#endregion
}


public enum UpdateState : byte
{
    State_Start,
    State_Write,
    State_Wait,
    State_Success,
    State_Fail,
}