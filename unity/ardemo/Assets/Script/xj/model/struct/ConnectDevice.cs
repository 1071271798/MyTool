using Game;
using Game.Event;
using Game.Platform;
using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ConnectDevice.cs
/// Description:连接设备的数据
/// Time:2016/12/8 10:05:11
/// </summary>
/// 
public class ConnectDevice
{
    #region 公有属性
    #endregion

    #region 其他属性
    ConnectState mState = ConnectState.Connect_None;
    byte mReadDeviceOutTimeCount = 0;
    byte mConnectTimes = 0;
    public byte ConnectTimes
    {
        get { return mConnectTimes; }
    }
    public ConnectState ConState
    {
        get { return mState; }
    }
    string mMac = string.Empty;
    public string Mac
    {
        get { return mMac; }
    }
    string mName = string.Empty;
    public string Name
    {
        get { return mName; }
    }

    long mReadDeviceTypeIndex = -1;
    long mReadMotherboardDataIndex = -1;
    Robot mRobot;
    private EventDelegate.Callback mCallBack;
    #endregion

    #region 公有函数
    public ConnectDevice()
    {
    }

    public void UpdateDevice(Robot robot, EventDelegate.Callback callback)
    {
        mRobot = robot;
        mCallBack = callback;
    }

    public void UpdateDevice(string mac, string name)
    {
        mMac = mac;
        mName = name;
    }

    public void UpdateDevice(Robot robot)
    {
        mRobot = robot;
    }
    /// <summary>
    /// 连接
    /// </summary>
    public void OnConnect(string mac, string name)
    {
        CleanUpTimer();
        mMac = mac;
        mName = name;
        mState = ConnectState.Connect_Ing;
        ++mConnectTimes;
        mReadDeviceOutTimeCount = 0;
    }

    public void ConnectFail(BlueConnectFailReason reason)
    {
        CleanUpTimer();
        mState = ConnectState.Connect_Fail;
        //ConnectFinishedFail();
    }

    public void ConnectSuccess(string mac = null, string name = null)
    {
        mState = ConnectState.Connect_Verify;
        if (!string.IsNullOrEmpty(mac))
        {
            mMac = mac;
        }
        if (string.IsNullOrEmpty(name))
        {
            mName = name;
        }
        mRobot.ConnectRobotResult(mMac, true);
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            if (PlayerPrefs.HasKey(mMac))
            {
                ReadDeviceResult(true);
            }
            else
            {
                mRobot.ReadDeviceType();
            }
        } else
        {
            ConnectingFail(LauguageTool.GetIns().GetText("连接失败"));
        }
        
        
        //mReadDeviceTypeIndex = Timer.Add(1, 1, 1, robot.ReadDeviceType);
    }

    public void ConnectCancel()
    {
        CleanUpTimer();
        mState = ConnectState.Connect_Cannel;
        //ConnectFinishedFail();
    }

    public void Disconnect()
    {
        CleanUpTimer();
        if (mState == ConnectState.Connect_Finished)
        {
            mConnectTimes = 0;
        } else if (mState == ConnectState.Connect_Verify || mState == ConnectState.Connect_ReadData)
        {
            ConnectingFail(LauguageTool.GetIns().GetText("连接失败"));
        } else if (mState == ConnectState.Connect_ReadData_Finished)
        {
            ConnectFinishedFail();
        }
        mState = ConnectState.Connect_Disconnect;
        
    }

    public void ReadDeviceResult(bool result)
    {
        mReadDeviceOutTimeCount = 0;
        if (result)
        {
            if (!PlayerPrefs.HasKey(mMac))
            {
                PlayerPrefs.SetInt(mMac, 1);
                PlayerPrefs.Save();
            }
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                mRobot.HandShake();
                //2秒以后读取初始角度
                mReadMotherboardDataIndex = Timer.Add(2, 1, 1, delegate () {
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {
                        mRobot.ReadMotherboardData();
                    } else
                    {
                        ConnectingFail(LauguageTool.GetIns().GetText("连接失败"));
                    }
                } );
                mState = ConnectState.Connect_ReadData;
            } else
            {
                ConnectingFail(LauguageTool.GetIns().GetText("连接失败"));
            }
        }
        else
        {
            mState = ConnectState.Connect_Fail;
            mRobot.HandShake();//加入握手命令是防止一直连不上必须重启主控盒的情况出现
            ConnectingFail(LauguageTool.GetIns().GetText("连接失败"));
            PlatformMgr.Instance.OnlyDisConnectBluetooth();
        }
    }

    public void ReadDeviceOutTime()
    {
        ++mReadDeviceOutTimeCount;
        if (mReadDeviceOutTimeCount > 4)
        {
            ReadDeviceResult(false);
        }
        else
        {
            mReadDeviceTypeIndex = Timer.Add(1, 1, 1, delegate ()
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("第{0}次读取设备类型", (mReadDeviceOutTimeCount + 1)));
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mRobot.ReadDeviceType();
                } else
                {
                    ReadDeviceResult(false);
                }
            });
        }
    }

    public void ReadDataOutTime()
    {
        ConnectingFail(LauguageTool.GetIns().GetText("HuoQuZhuBanXinXiShiBai"));
        mState = ConnectState.Connect_ReadData_OutTime;
    }

    public void ReadDataAck()
    {
        mState = ConnectState.Connect_ReadData_Finished;
    }

    public void ReadRobotAngle()
    {
        if (!PlatformMgr.Instance.IsChargeProtected && mRobot.GetAllDjData().GetAngleList().Count > 0 && !RobotManager.GetInst().IsSetDeviceIDFlag)
        {//回读角度
            mRobot.ReadConnectedAngle(delegate() {
                ConnectFinished(mRobot);
            });
        } else
        {
            ConnectFinished(mRobot);
        }
    }

    public void CloseConnectPage()
    {
        if (mState == ConnectState.Connect_Verify || mState == ConnectState.Connect_ReadData)
        {
            ConnectFinishedFail();
        } else if (null != mCallBack)
        {
            mCallBack();
        }
        mState = ConnectState.Connect_None;
    }

    /// <summary>
    /// 整个连接流程完成的失败
    /// </summary>
    public void ConnectFinishedFail()
    {
        ConnectEvent(false);
        ConnectCallBack(false, true, string.Empty);
        if (null != mCallBack)
        {
            mCallBack();
        }
    }

    /// <summary>
    /// 连接过程中的失败，不退出连接流程
    /// </summary>
    public void ConnectingFail(string msg)
    {
        mState = ConnectState.Connect_Fail;
        ConnectCallBack(false, false, msg);
    }

    private void ConnectCallBack(bool result, bool finished, string msg)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["result"] = result ? 1 : 0;
        dict["finished"] = finished ? 1 : 0;
        dict["msg"] = msg;
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.connectCallBack, Json.Serialize(dict));
    }

    #endregion

    #region 其他函数
    void ConnectFinished(Robot robot)
    {
        if (!RobotManager.GetInst().IsSetDeviceIDFlag)
        {
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("蓝牙连接成功"), 1, CommonTipsColor.green);
        }
        EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(true));
        mState = ConnectState.Connect_Finished;
        robot.StartReadSystemPower();
        robot.ReadMCUInfo();
        robot.SelfCheck(true);
        robot.canShowPowerFlag = true;
        TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
        for (int i = 0, imax = partType.Length; i < imax; ++i)
        {
            if (partType[i] == TopologyPartType.Speaker)
            {
                continue;
            }
            if (null != robot.MotherboardData)
            {
                SensorData sensorData = robot.MotherboardData.GetSensorData(partType[i]);
                if (null != sensorData && sensorData.ids.Count > 0)
                {
                    robot.SensorInit(sensorData.ids, partType[i]);
                }
            }
        }
        EventMgr.Inst.Fire(EventID.Blue_Connect_Finished);
        if (!PlatformMgr.Instance.IsChargeProtected)
        {
            if (robot.GetAllDjData().GetTurnList().Count > 0)
            {
                robot.StopAllTurn();
            }
        }
        PlatformMgr.Instance.SaveRobotLastConnectedData(robot.ID, robot.Mac);
        ConnectCallBack(true, true, string.Empty);
        ConnectEvent(true);
        bool isLogicSearch = SingletonObject<LogicCtrl>.GetInst().IsLogicOpenSearchFlag;
        if (null != mCallBack)
        {
            mCallBack();
        }
        if (!isLogicSearch)
        {
            SingletonObject<LogicCtrl>.GetInst().ConnectBLECallBack();
        }
    }

    void CleanUpTimer()
    {

        if (-1 != mReadDeviceTypeIndex)
        {
            Timer.Cancel(mReadDeviceTypeIndex);
            mReadDeviceTypeIndex = -1;
        }
        if (-1 != mReadMotherboardDataIndex)
        {
            Timer.Cancel(mReadMotherboardDataIndex);
            mReadMotherboardDataIndex = -1;
        }
    }

    void ConnectEvent(bool result)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        if (result)
        {
            dict["resultCode"] = PublicFunction.ResultCode_Success;
        }
        else
        {
            dict["resultCode"] = PublicFunction.ResultCode_Failure;
        }
        if (null != mRobot)
        {
            if (mRobot.MotherboardData != null)
            {
                dict["contorlboxVersion"] = mRobot.MotherboardData.mbVersion;
            } else
            {
                dict["contorlboxVersion"] = string.Empty;
            }
        } else
        {
            dict["contorlboxVersion"] = string.Empty;
        }
        dict["controlboxId"] = ConnectCtrl.GetInst().GetLastConnectingMac();
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.app_bluetooth_connect, dict);
    }
    #endregion
}

/// <summary>
/// 连接的状态
/// </summary>
public enum ConnectState : byte
{
    Connect_None,
    Connect_Ing,
    Connect_Cannel,
    Connect_Fail,
    Connect_Verify,
    Connect_ReadData,
    Connect_ReadData_OutTime,
    Connect_ReadData_Finished,
    Connect_Disconnect,
    Connect_Finished,
}