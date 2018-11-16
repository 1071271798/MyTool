using Game;
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateSensor.cs
/// Description:升级传感器
/// Time:2016/12/16 13:49:11
/// </summary>
public class UpdateSensor : UpdateBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数

    public UpdateSensor(TopologyPartType partType) :base(partType)
    {
    }

    public override ErrorCode CheckUpdate(ReadMotherboardDataMsgAck msg)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do
        {
            SensorData sensorData = msg.GetSensorData(mPartType);
            if (null != sensorData)
            {
                if (sensorData.ids.Count > 0 && sensorData.errorIds.Count < 1 && !string.IsNullOrEmpty(mVersion) && (!mVersion.Equals(sensorData.version) || sensorData.errorVerIds.Count > 0))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(mFileName);
                    if (null != bytes && bytes.Length <= 50)
                    {
                        if (mFilePath.EndsWith(".bytes") || File.Exists(mFilePath))
                        {
                            if (ReadPowerMsgAck.isLithium && PlatformMgr.Instance.PowerData.power < PublicFunction.Update_System_Power_Min)
                            {//电量太低，不更新
                                ret = ErrorCode.Robot_Power_Low;
                                break;
                            }
                            else
                            {
                                ret = ErrorCode.Can_Upgraded;
                                break;
                            }
                        }
                        else
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("升级文件不存在，Sensor_Version = {0} Sensor_FilePath = {1}", mVersion, mFilePath));
                            break;
                        }
                    }
                }
                else
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, mPartType.ToString() + "不需要升级");
                }
            }
        } while (false);
        return ret;
    }

    public override bool UpdateStart(Robot robot, byte arg)
    {
        bool flag = base.UpdateStart(robot, arg);
        if (flag)
        {
            SensorUpdateStartMsg msg = new SensorUpdateStartMsg();
            msg.sensorType = TopologyPartData.ConvertToSensorType(mPartType);
            msg.ids=arg;
            msg.fileName = mFileName;
            msg.frameCount = mFrameTotalNum;
            NetWork.GetInst().SendMsg(CMDCode.Sensor_Update_Start, msg, robot.Mac);
        }
        return flag;
    }

    public override void SendFrame()
    {
        base.SendFrame();
        byte[] bytes = GetNextFrame();
        if (null != bytes)
        {
            SensorUpdateWriteMsg msg = new SensorUpdateWriteMsg();
            msg.sensorType = mPartType;
            msg.frameNum = mSendFrameIndex;
            msg.bytes = bytes;
            if (mSendFrameIndex == mFrameTotalNum)
            {
                Timer.Add(0.02f, 0, 1, delegate ()
                {
                    NetWork.GetInst().SendMsg(CMDCode.Sensor_Update_Finish, msg, mRobot.Mac);
                });
            }
            else
            {
                Timer.Add(0.02f, 0, 1, delegate ()
                {
                    NetWork.GetInst().SendMsg(CMDCode.Sensor_Update_Write, msg, mRobot.Mac);
                });
            }
        }
    }
    #endregion

    #region 其他函数
    #endregion
}