using Game;
using Game.Event;
using Game.Platform;
using System.Collections.Generic;
using UnityEngine;

public class ConnectCtrl : SingletonObject<ConnectCtrl>
{
    private Robot mRobot;
    private string AutoConnectSaveKey = "AutoConnect";
    private bool isAutoConnectFlag = true;
    private bool isReconnectFlag = false;
    private int mReconnectTime = 0;
    private long mReStartScanIndex = -1;
    private Robot mReconnectRobot;
    private ConnectDevice mConnectDevice;
    string mConnectingMac = string.Empty;
    string mLastConnectingMac = string.Empty;
    string mConnectedMac = string.Empty;
    float lastDicConnectedTime = 0;

    BlueConnectFailReason mConnectFailReason = BlueConnectFailReason.unknow;

    long mOutTimeIndex = -1;
    public float LastDicConnectedTime
    {
        get { return lastDicConnectedTime; }
    }
    public bool IsReconnectFlag
    {
        get { return isReconnectFlag; }
        set { this.isReconnectFlag = value; }
    }

    public ConnectCtrl()
    {
        if (PlayerPrefs.HasKey(AutoConnectSaveKey))
        {
            isAutoConnectFlag = PlayerPrefs.GetInt(AutoConnectSaveKey) == 1 ? true : false;
        }
        else
        {
            isAutoConnectFlag = true;
        }
    }

    public Robot NowRobot
    {
        get
        {
            if (null == mRobot)
            {
                mRobot = GetConnectRobot();
            }
            return mRobot;
        }
    }

    /// <summary>
    /// 获取连接状态
    /// </summary>
    /// <returns></returns>
    public bool GetBluetoothState()
    {
        if (string.IsNullOrEmpty(mConnectedMac))
        {
            return false;
        }
        return true;
    }

    public string GetLastConnectingMac()
    {
        return mLastConnectingMac;
    }
    /// <summary>
    /// 是否需要自动连接
    /// </summary>
    /// <param name="robotId"></param>
    /// <param name="mac"></param>
    /// <returns></returns>
    public bool IsAutoConnect(string robotId, string mac)
    {
        if (!isAutoConnectFlag)
        {
            return false;
        }
        if (PlayerPrefs.HasKey(robotId))
        {
            if (PlayerPrefs.GetString(robotId).Equals(mac))
            {
                if (null == mConnectDevice || mConnectDevice.ConnectTimes < 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void OpenConnectPage(Robot robot, EventDelegate.Callback callback = null)
    {
        isReconnectFlag = false;
        mRobot = robot;
        CreateConnectDevice(robot, callback);
        if (SingletonBehaviour<ClientMain>.GetInst().useThirdAppFlag)
        {
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.openBluetoothPage, string.Empty);
        } else
        {
            ConnectBluetoothMsg.ShowMsg();
        }
    }

    public void CloseConnectPage()
    {
        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            PlatformMgr.Instance.OnlyDisConnectBluetooth();
        }
        if (null != mConnectDevice)
        {
            mConnectDevice.CloseConnectPage();
        }
        mRobot = null;
        mConnectDevice = null;
    }

    public void OnConnect(string mac, string name)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "连接设备 mac =" + mac);
        if (!string.IsNullOrEmpty(mConnectedMac) && null != mConnectDevice)
        {
            mConnectDevice.Disconnect();
            mConnectedMac = string.Empty;
        }
#if UNITY_EDITOR
        if (null == mRobot)
        {
            CreateConnectDevice(NowRobot, null);
            mConnectDevice.UpdateDevice(mac, name);
        } else
        {
            CreateConnectDevice(mac, name);
        }
#endif

        CleanConnectData();
        mConnectingMac = mac;
        mLastConnectingMac = mac;
        mConnectDevice.OnConnect(mac, name);
        if (-1 != mOutTimeIndex)
        {
            Timer.Cancel(mOutTimeIndex);
        }
        mOutTimeIndex = Timer.Add(15f, 1, 1, ConnectOutTime, mac);
    }

    public void SetConnectRobot(Robot robot)
    {
        mRobot = robot;
        if (null != mConnectDevice)
        {
            mConnectDevice.UpdateDevice(robot);
        }
    }

    public void CancelConnecting()
    {
        if (-1 != mOutTimeIndex)
        {
            Timer.Cancel(mOutTimeIndex);
            mOutTimeIndex = -1;
        }
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "用户主动取消了连接");
        NowRobot.CancelConnect();
        if (string.IsNullOrEmpty(mConnectingMac))
        {
            if (!string.IsNullOrEmpty(mConnectedMac))
            {
                if (null != mConnectDevice)
                {
                    mConnectDevice.ConnectCancel();
                }
                PlatformMgr.Instance.OnlyDisConnectBluetooth();
            }
        } else
        {
            if (null != mConnectDevice)
            {
                mConnectDevice.ConnectCancel();
            }
            mConnectingMac = string.Empty;
        }
    }

    public void OnConnectResult(string mac, bool result, string name = null)
    {
        if (string.IsNullOrEmpty(mac))
        {//连接失败
            ConnectResultFail();
        } else
        {//连接成功
            CreateConnectDevice(mac, name);
            mLastConnectingMac = mac;
            ConnectResultSuccess(mac, name);
        }
    }    

    public void ReadDeviceResult(bool result)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "读取设备类型result = " + result.ToString());

        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            if (null != mConnectDevice)
            {
                mConnectDevice.ReadDeviceResult(result);
            }
        }
    }

    public void ReadDeviceOutTime()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "读取设备类型超时未回复");
        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            if (null != mConnectDevice)
            {
                mConnectDevice.ReadDeviceOutTime();
            }
        }
    }

    public void RobotMatchFinished()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "机器人匹配成功");
        if (null != mConnectDevice)
        {
            if (string.IsNullOrEmpty(mConnectedMac))
            {
                mConnectDevice.ConnectingFail(LauguageTool.GetIns().GetText("连接失败"));
            } else
            {
                mConnectDevice.ReadRobotAngle();
            }
        }
    }

    public void ConnectingFail(string msg)
    {
        if (null != mConnectDevice)
        {
            mConnectDevice.ConnectingFail(msg);
        }
        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            PlatformMgr.Instance.OnlyDisConnectBluetooth();
        }
    }

    public void ConnectFinishedFail()
    {
        if (null != mConnectDevice)
        {
            mConnectDevice.ConnectFinishedFail();
        }
        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            PlatformMgr.Instance.OnlyDisConnectBluetooth();
        }
    }

    public void ReadMotherboardAck()
    {
        if (null != mConnectDevice)
        {
            mConnectDevice.ReadDataAck();
        }
    }

    public void ReadMotherboardDataOutTime()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "读取主板信息超时");
        if (null != mConnectDevice)
        {
            mConnectDevice.ReadDataOutTime();
            if (!string.IsNullOrEmpty(mConnectedMac))
            {
                PlatformMgr.Instance.OnlyDisConnectBluetooth();
            }
            if (isReconnectFlag)
            {
                ReconnectFail();
            }
        }
    }

    public bool Reconnect(Robot robot)
    {
        if (string.IsNullOrEmpty(robot.Mac))
        {
            return false;
        }
        isReconnectFlag = true;
        mReconnectRobot = robot;
        mRobot = robot;
        mReconnectTime = 3;
        ReStarScan();
        return true;
    }

    void ReStarScan()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "ReStarScan");
        if (-1 != mReStartScanIndex)
        {
            Timer.Cancel(mReStartScanIndex);
        }
        if (!isReconnectFlag)
        {
            return;
        }
        PlatformMgr.Instance.StartScan();
        mReStartScanIndex = Timer.Add(30, 1, 1, ReconnectFail);
    }

    public void FoundDevice(DeviceInfo device)
    {
        if (null != mReconnectRobot)
        {
            if (string.Equals(device.Mac, mReconnectRobot.Mac))
            {
                if (-1 != mReStartScanIndex)
                {
                    Timer.Cancel(mReStartScanIndex);
                    mReStartScanIndex = -1;
                }
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "重连设备 mac = " + device.Mac + " name = " + device.Name + " reconnect mac = " + mReconnectRobot.Mac);
                PlatformMgr.Instance.ConnenctBluetooth(device.Mac, device.Name);
            }
        }
        else
        {
            ReconnectFail();
            PlatformMgr.Instance.StopScan();
        }
    }

    public void ReconnectFail()
    {
        mReconnectTime--;
        if (mReconnectTime > 0)
        {
            ReStarScan();
        }
        else
        {
            isReconnectFlag = false;
            mReconnectRobot = null;
        }
    }

    public void CannelReconnect()
    {
        if (-1 != mReStartScanIndex)
        {
            PlatformMgr.Instance.StopScan();
            Timer.Cancel(mReStartScanIndex);
            mReStartScanIndex = -1;
        }
        isReconnectFlag = false;
        mReconnectRobot = null;
    }

    public void ReconnectSuccess(ReadMotherboardDataMsgAck msg)
    {
        if (SingletonObject<PopWinManager>.GetInst().IsExist(typeof(TopologyBaseMsg)))
        {
            TopologyBaseMsg.ShowMsg(msg, TopologyBaseMsg.GetCloseCallback());
        }
        isReconnectFlag = false;
    }
    /// <summary>
    /// 主动断开连接
    /// </summary>
    public void OnDisconnect()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "主动断开蓝牙连接 mConnectedMac =" + mConnectedMac);
        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            lastDicConnectedTime = Time.time;
            if (null != mConnectDevice)
            {
                mConnectDevice.Disconnect();
            }
            CleanConnectData();
            if (!isReconnectFlag)
            {
                EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(false));
            }
        }
        else
        {
            CleanConnectData();
        }
    }
    /// <summary>
    /// 收到断开的通知
    /// </summary>
    public void DisconnectNotify()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "收到断开蓝牙的通知 mConnectedMac =" + mConnectedMac);
        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            if (null != mConnectDevice)
            {
                ConnectState oldState = mConnectDevice.ConState;
                mConnectDevice.Disconnect();
                if (oldState == ConnectState.Connect_Finished)
                {
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("蓝牙断开"), 1, CommonTipsColor.red);
                    LogicCtrl.GetInst().NotifyLogicDicBlue();
                }
            }
            CleanConnectData();
            EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(false));
        }
        else
        {
            CleanConnectData();
        }
        if (null != NowRobot)
        {
            NowRobot.CancelConnect();
        }
    }

    public string GetConnectedMac()
    {
        return mConnectedMac;
    }

    public string GetConnectedName()
    {
        if (null != mConnectDevice)
        {
            return mConnectDevice.Name;
        }
        return string.Empty;
    }
    /// <summary>
    /// 是否可以跳过连接图
    /// </summary>
    /// <param name="robot"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsSkipTopology(Robot robot, ReadMotherboardDataMsgAck data)
    {
        if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            return false;
        }
        if (PlatformMgr.Instance.NeedSetServoFlag == SetHardwareState.NeedSetServo)
        {
            return false;
        }
        ErrorCode ret = CheckCompareModel(robot, data);
        if (ErrorCode.Result_OK == ret)
        {
            return true;
        }
        return false;
    }

    public void SetAutoConnectFlag(bool openFlag)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "设置自动连接开关 state = " + openFlag);
        PlayerPrefs.SetInt(AutoConnectSaveKey, openFlag ? 1 : 0);
        isAutoConnectFlag = openFlag;
    }


    public ErrorCode CheckCompareModel(Robot robot, ReadMotherboardDataMsgAck data)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do
        {
            if (null == data || null == robot)
            {
                break;
            }
            if (data.errorIds.Count > 0)
            {
                ret = ErrorCode.Result_DJ_ID_Repeat;
                break;
            }
            if (null != data.motorErrorIds && data.motorErrorIds.Count > 0)
            {
                ret = ErrorCode.Result_Motor_ID_Repeat;
                break;
            }
            ret = CompareServoData(robot, data);
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            ret = CompareMotorData(robot, data);
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            ret = CompareSensorData(robot, data);
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            TopologyPartType mainPart = data.GetMainboardType();
            if (mainPart != TopologyPartType.None)
            {
                ret = SingletonObject<UpdateManager>.GetInst().CheckUpdate(mainPart, data);
            }
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            ret = SingletonObject<UpdateManager>.GetInst().CheckUpdate(TopologyPartType.Servo, data);
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            TopologyPartType[] partAry = PublicFunction.Open_Topology_Part_Type;
            for (int i = 0, imax = partAry.Length; i < imax; ++i)
            {
                ret = SingletonObject<UpdateManager>.GetInst().CheckUpdate(partAry[i], data);
                if (ErrorCode.Result_OK != ret)
                {
                    return ret;
                }
            }
        } while (false);
        return ret;
    }

    /// <summary>
    /// 比较实物与模型舵机信息是否匹配
    /// </summary>
    /// <param name="robot"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public ErrorCode CompareServoData(Robot robot, ReadMotherboardDataMsgAck data)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do
        {
            List<byte> list = robot.GetAllDjData().GetIDList();
            if (list.Count != data.ids.Count)
            {
                ret = ErrorCode.Result_Servo_Num_Inconsistent;
                break;
            }
            for (int i = 0, icount = list.Count; i < icount; ++i)
            {
                if (list[i] != data.ids[i])
                {
                    ret = ErrorCode.Result_Servo_ID_Inconsistent;
                    break;
                }
            }
        } while (false);
        return ret;
    }


    public ErrorCode CompareMotorData(Robot robot, ReadMotherboardDataMsgAck data)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        if (null != robot.MotorsData)
        {
            ret = robot.MotorsData.EqualsID(data.motors);
        }
        return ret;
    }

    public ErrorCode CompareSensorData(Robot robot, ReadMotherboardDataMsgAck mainData)
    {
        TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
        ServosConnection con = ServosConManager.GetInst().GetServosConnection(robot.ID);
        Dictionary<TopologyPartType, List<byte>> sensorDict = null;
        if (null != con)
        {
            sensorDict = con.GetTopologySensor();
        }
        for (int i = 0, imax = partType.Length; i < imax; ++i)
        {
            SensorData data = mainData.GetSensorData(partType[i]);
            if (null != data)
            {
                if (TopologyPartType.Speaker == partType[i] || TopologyPartType.RgbLight == partType[i])
                {
                    if (data.ids.Count == 1)
                    {
                        if (null == sensorDict || !sensorDict.ContainsKey(partType[i]))
                        {
                            return ErrorCode.Result_Sensor_Exception;
                        }
                        if (TopologyPartType.Speaker == partType[i])
                        {
                            return ErrorCode.Result_Sensor_Have_Speaker;
                        }
                    }
                    else if (data.ids.Count > 1 || data.errorIds.Count > 0)
                    {
                        return ErrorCode.Result_Sensor_ID_Repeat;
                    }
                    else if (data.ids.Count == 0 && null != sensorDict && sensorDict.ContainsKey(partType[i]) && sensorDict[partType[i]].Count > 0)
                    {
                        return ErrorCode.Result_Sensor_Exception;
                    }
                }
                else if (data.errorIds.Count > 0)
                {
                    return ErrorCode.Result_Sensor_ID_Repeat;
                }
                else if (null == sensorDict || !sensorDict.ContainsKey(partType[i]))
                {
                    if (data.ids.Count > 0)
                    {
                        return ErrorCode.Result_Sensor_Exception;
                    }
                }
                else if (null != sensorDict && sensorDict.ContainsKey(partType[i]))
                {
                    List<byte> sensorList = sensorDict[partType[i]];
                    if (sensorList.Count != data.ids.Count)
                    {
                        return ErrorCode.Result_Sensor_Exception;
                    }
                    else
                    {
                        for (int sensorIndex = 0, sensorMax = sensorList.Count; sensorIndex < sensorMax; ++sensorIndex)
                        {
                            if (sensorList[sensorIndex] != data.ids[sensorIndex])
                            {
                                return ErrorCode.Result_Sensor_Exception;
                            }
                        }
                    }
                }
            }
            else if (null != sensorDict && sensorDict.ContainsKey(partType[i]) && sensorDict[partType[i]].Count > 0)
            {
                return ErrorCode.Result_Sensor_Exception;
            }
        }
        return ErrorCode.Result_OK;
    }

    public ConnectState GetDeviceConnectState()
    {
        if (null != mConnectDevice)
        {
            return mConnectDevice.ConState;
        }
        return ConnectState.Connect_None;
    }

    public Robot GetConnectRobot()
    {
        Robot robot = null;
        if (RobotManager.GetInst().IsSetDeviceIDFlag)
        {
            robot = RobotManager.GetInst().GetSetDeviceRobot();
        }
        else if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            robot = RobotManager.GetInst().GetCreateRobot();
        }
        else
        {
            robot = RobotManager.GetInst().GetCurrentRobot();
        }
        return robot;
    }

    public override void CleanUp()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "ConnectCtrl CleanUp");
        CleanConnectData();
        mConnectDevice = null;
        mRobot = null;
    }
    public void CleanConnectData()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "ConnectCtrl CleanConnectData");
        mConnectedMac = string.Empty;
        mConnectingMac = string.Empty;
        mConnectFailReason = BlueConnectFailReason.unknow;
        RobotManager.GetInst().DisAllConnencted();
        NetWork.GetInst().ClearAllMsg();
        //SingletonObject<UpdateManager>.GetInst().CleanUp();
        PlatformMgr.Instance.PowerData.isAdapter = false;
        PlatformMgr.Instance.PowerData.isChargingFinished = false;
        ReadPowerMsgAck.isLithium = true;
    }


    //////////////////////////////////////////////////////////////////////////
    private void CreateConnectDevice(string mac, string name)
    {
        if (null == mConnectDevice)
        {
            mConnectDevice = new ConnectDevice();
        }
        else if (!string.IsNullOrEmpty(mConnectedMac) && null != mConnectDevice)
        {
            mConnectDevice.Disconnect();
            mConnectedMac = string.Empty;
        }
        mConnectDevice.UpdateDevice(mac, name);
        
    }

    private void CreateConnectDevice(Robot robot, EventDelegate.Callback callback)
    {
        if(null == mConnectDevice)
        {
            mConnectDevice = new ConnectDevice();
        }
        else if (!string.IsNullOrEmpty(mConnectedMac) && null != mConnectDevice)
        {
            mConnectDevice.Disconnect();
            mConnectedMac = string.Empty;
        }
        mConnectDevice.UpdateDevice(robot, callback);
    }

    private void ConnectOutTime(params object[] args)
    {
        string mac = (string)args[0];
        if (null != mConnectDevice && mConnectDevice.ConState == ConnectState.Connect_Ing)
        {
            OnConnectResult(string.Empty, false);
        }
    }

    private void ConnectResultSuccess(string mac, string name)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "连接成功 mac =" + mac + "mConnectingMac = " + mConnectingMac);
        if (null != mConnectDevice)
        {
            if (-1 != mOutTimeIndex)
            {
                Timer.Cancel(mOutTimeIndex);
                mOutTimeIndex = -1;
            }
            mConnectedMac = mac;
            mConnectDevice.ConnectSuccess(mac, name);
            mConnectingMac = string.Empty;
        }
        else
        {
            PlatformMgr.Instance.DisDeviceBluetooth();
        }
    }

    private void ConnectResultFail()
    {
        if (-1 != mOutTimeIndex)
        {
            Timer.Cancel(mOutTimeIndex);
            mOutTimeIndex = -1;
        }
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("ConnectFail mConnectingMac = {0}", mConnectingMac));
        if (!string.IsNullOrEmpty(mConnectingMac) && null != mConnectDevice)
        {
            mConnectDevice.ConnectFail(BlueConnectFailReason.BluetoothFail);
            if (isReconnectFlag)
            {
                ReconnectFail();
            }
            else
            {
                EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(false));
            }
        }
        mConnectingMac = string.Empty;
    }
}