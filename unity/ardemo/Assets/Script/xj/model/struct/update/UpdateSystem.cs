using Game;
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateSystem.cs
/// Description:升级主板
/// Time:2016/11/24 10:36:14
/// </summary>
public class UpdateSystem : UpdateBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    public UpdateSystem(TopologyPartType partType) : base(partType)
    {
        
    }

    public override ErrorCode CheckUpdate(ReadMotherboardDataMsgAck msg)
    {
        ErrorCode ret = ErrorCode.Can_Upgraded;
        do
        {
            if (!string.IsNullOrEmpty(mVersion) && !mVersion.Equals(msg.mbVersion))
            {//检测到有升级文件，且版本不一致
                if (mFilePath.EndsWith(".bytes") || File.Exists(mFilePath))
                {
                    if (ReadPowerMsgAck.isLithium)
                    {
                        if (PlatformMgr.Instance.PowerData.power < PublicFunction.Update_System_Power_Min)
                        {//电量太低，不更新
                            ret = ErrorCode.Robot_Power_Low;
                            break;
                        }
                    }
                }
                else
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("升级文件不存在，Robot_System_Version = {0} Robot_System_FilePath = {1}", mVersion, mFilePath));
                    ret = ErrorCode.Result_OK;
                    break;
                }
            }
            else
            {
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
            RobotUpdateStartMsg msg = new RobotUpdateStartMsg();
            msg.fileName = mFileName;
            msg.frameCount = mFrameTotalNum;
            NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Start, msg, robot.Mac);
        }
        return flag;
    }

    public override void SendFrame()
    {
        base.SendFrame();
        byte[] bytes = GetNextFrame();
        if (null != bytes)
        {
            UpdateWriteMsg msg = new UpdateWriteMsg();
            msg.frameNum = mSendFrameIndex;
            msg.bytes = bytes;
            if (mSendFrameIndex == mFrameTotalNum)
            {
                Timer.Add(0.02f, 0, 1, delegate ()
                {
                    NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Finish, msg, mRobot.Mac);
                });
            }
            else
            {
                Timer.Add(0.02f, 0, 1, delegate ()
                {
                    NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Write, msg, mRobot.Mac);
                });
            }
        }
    }
    #endregion

    #region 其他函数

    #endregion
}