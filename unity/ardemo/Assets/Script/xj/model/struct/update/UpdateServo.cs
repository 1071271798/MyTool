using Game;
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateServo.cs
/// Description:升级舵机
/// Time:2016/11/28 15:47:49
/// </summary>
public class UpdateServo : UpdateBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数

    public override ErrorCode CheckUpdate(ReadMotherboardDataMsgAck msg)
    {
        ErrorCode ret = ErrorCode.Can_Upgraded;
        do
        {
            if (msg.ids.Count > 0 && !string.IsNullOrEmpty(mVersion) && (!mVersion.Equals(msg.djVersion) || msg.errorVerIds.Count > 0))
            {
                if (mFilePath.EndsWith(".bytes") || File.Exists(mFilePath))
                {
                    if (ReadPowerMsgAck.isLithium && PlatformMgr.Instance.PowerData.power < PublicFunction.Update_System_Power_Min)
                    {//电量太低，不更新
                        ret = ErrorCode.Robot_Power_Low;
                        break;
                    }
                }
                else
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("升级文件不存在，Servo_Version = {0} Servo_FilePath = {1}", mVersion, mFilePath));
                    /*if (msg.errorVerIds.Count > 0)
                    {
                        ret = ErrorCode.Servo_Version_inconsistent;
                        break;
                    }*/
                    ret = ErrorCode.Result_OK;
                    break;
                }
            }
            else
            {
                /*if (msg.errorVerIds.Count > 0)
                {
                    ret = ErrorCode.Servo_Version_inconsistent;
                    break;
                }*/
                ret = ErrorCode.Result_OK;
                break;
            }
        } while (false);
        return ret;
    }

    public override bool UpdateStart(Robot robot, byte arg)
    {
        bool flag = base.UpdateStart(robot, arg);
        if (flag)
        {
            byte[] bytes = mUpdateFile;
            int mod = bytes.Length % 64;
            if (mod != 0)
            {
                mUpdateFile = new byte[bytes.Length + 64 - mod];
            }
            else
            {
                mUpdateFile = new byte[bytes.Length];
            }
            Array.Copy(bytes, mUpdateFile, bytes.Length);
            mFrameTotalNum = (ushort)(mUpdateFile.Length / 100);
            if (mUpdateFile.Length % 100 != 0)
            {
                mFrameTotalNum += 1;
            }
            ServoUpdateStartMsg msg = new ServoUpdateStartMsg();
            msg.id = arg;
            msg.frameCount = mFrameTotalNum;
            msg.crc32 = PublicFunction.GetCRC32Str(mUpdateFile);
            NetWork.GetInst().SendMsg(CMDCode.Servo_Update_Start, msg, mRobot.Mac);
        }
        return flag;
    }

    public override void SendFrame()
    {
        base.SendFrame();
        byte[] bytes = GetNextFrame();
        if (null != bytes)
        {
            ServoUpdateWriteMsg msg = new ServoUpdateWriteMsg();
            msg.frameNum = mSendFrameIndex;
            msg.bytes = bytes;
            if (mSendFrameIndex == mFrameTotalNum)
            {
                Timer.Add(0.02f, 0, 1, delegate ()
                {
                    NetWork.GetInst().SendMsg(CMDCode.Servo_Update_Finish, msg, mRobot.Mac);
                });
            }
            else
            {
                Timer.Add(0.02f, 0, 1, delegate ()
                {
                    NetWork.GetInst().SendMsg(CMDCode.Servo_Update_Write, msg, mRobot.Mac);
                });
            }
        }
    }
    #endregion

    #region 其他函数
    protected override void Init()
    {
        mPartType = TopologyPartType.Servo;
        base.Init();
    }
    #endregion
}