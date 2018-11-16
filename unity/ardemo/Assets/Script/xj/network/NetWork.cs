//#define Test
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

#if UNITY_EDITOR
using MyTest;
#endif

/// <summary>
/// Author:xj
/// FileName:NetWork.cs
/// Description:
/// Time:2015/7/1 18:04:10
/// </summary>
public class NetWork
{
    #region 公有属性
    #endregion

    #region 私有属性
    static NetWork mInst = null;
    //static UnityEngine.Object mLock = new UnityEngine.Object();
    //Queue<SendMsgData> mSendMsgList = null;
    List<SendMsgData> mSendMsgList = null;

    Dictionary<CMDCode, ExtendCMDCode> mLastSendExCmdDict;
    //Thread mSendThread;
#if UNITY_EDITOR
    const int Wait_Msg_Out_Time_Long = 1;
    const int Wait_Msg_Out_Time_Normal = 1;
#else
    const int Wait_Msg_Out_Time_Long = 10;
    const int Wait_Msg_Out_Time_Normal = 3;//新主控升级命令时间太久才由1s改成3s
#endif
    float mLastSendTime;
    /// <summary>
    /// 禁止边充边玩时可以发送的指令
    /// </summary>
    CMDCode[] mChargeCanSendCmd = new CMDCode[] { CMDCode.Hand_Shake, CMDCode.Read_System_Power, CMDCode.Read_Device_Type, CMDCode.Self_Check, CMDCode.Read_Motherboard_Data, CMDCode.Read_System_Version, CMDCode.Read_IC_Flash, CMDCode.Write_IC_Flash, CMDCode.Read_MCU_ID, CMDCode.Change_Name, CMDCode.Robot_Update_Start, CMDCode.Robot_Update_Finish, CMDCode.Robot_Update_Write, CMDCode.Robot_Update_Stop, CMDCode.Robot_Restart_Update_Start_Ack, CMDCode.Robot_Restart_Update_Finish_Ack, CMDCode.Servo_Update_Start, CMDCode.Servo_Update_Write, CMDCode.Servo_Update_Finish, CMDCode.Servo_Update_Stop, CMDCode.Change_ID, CMDCode.Change_Sensor_ID, CMDCode.Read_Sensor_Data, CMDCode.Read_Sensor_Data_Other, CMDCode.Sensor_Update_Start, CMDCode.Sensor_Update_Write, CMDCode.Sensor_Update_Stop, CMDCode.Sensor_Update_Finish };
    bool mChargePromptFlag = false;

    SendMsgData mSendedMsgData;
    #endregion

    #region 公有函数
    public static NetWork GetInst()
    {
        if (null == mInst)
        {
            mInst = new NetWork();
        }
        return mInst;
    }
    /// <summary>
    /// 发包
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="msg"></param>
    public void SendMsg(CMDCode cmd, CBaseMsg msg, string mac, ExtendCMDCode exCmd = ExtendCMDCode.Extend_Code_None)
    {
        //MyLog.Log("SendMsg start");
        if (!PlatformMgr.Instance.GetBluetoothState())
        {//防止出现无回调现象
            //ProtocolClient.GetInst().OnMsgDelegate(cmd, 0, mac, null, exCmd);
            return;
        }
        if (PlatformMgr.Instance.IsChargeProtected)
        {//禁止边充边玩
            bool returnFlag = true;
            for (int i = 0, imax = mChargeCanSendCmd.Length; i < imax; ++i)
            {
                if (cmd == mChargeCanSendCmd[i])
                {
                    returnFlag = false;
                    break;
                }
            }
            if (returnFlag)
            {
                NetWaitMsg.CloseWait();
                /*if (!mChargePromptFlag)
                {
                    mChargePromptFlag = true;
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("禁止边充边玩"));
                    //GuideViewBase.Ins.OnCloseGuide(null);
                }*/
                //防止出现无回调现象
                //ProtocolClient.GetInst().OnMsgDelegate(cmd, 0, mac, null, exCmd);
                return;
            }
        }
#if Test
        MemoryStream DataStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(DataStream);

        if (null != msg)
        {
            msg.Write(writer);
        }
        byte[] pMsg = DataStream.ToArray();

        writer.Close();
        DataStream.Close();
        PlatformMgr.Instance.SendMsg((byte)cmd, pMsg, pMsg.Length);
#else
        //lock (mLock)
        {
            //MyLog.Log("SendMsg start 1");
            SendMsgData data = new SendMsgData(cmd, msg, mac, exCmd);
            AddSendMsg(data);
            if (mSendMsgList.Count > 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0, imax = mSendMsgList.Count; i < imax; ++i)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(PublicFunction.Separator_Comma);
                    }
                    sb.Append(string.Format("cmd= {0} exCmd = {1}", mSendMsgList[i].cmd, mSendMsgList[i].extendCmd));
                }
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeDebug, string.Format("等待的命令数量={0},  {1}", mSendMsgList.Count, sb.ToString()));
            }
            if (mLastSendTime < 0.1f)
            {
                SendMsg();
            }
        }
        
#endif
    }

    public void ReceiveMsg(CMDCode cmd, int len, string mac, BinaryReader br)
    {
        bool finished = true;
        if (null != mSendedMsgData && (cmd == mSendedMsgData.cmd || mSendedMsgData.cmd == CMDCode.Read_Sensor_Data && cmd == CMDCode.Read_Sensor_Data_Other))
        {
            finished = ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, mSendedMsgData.extendCmd);
        }
        else
        {
            if (mLastSendExCmdDict.ContainsKey(cmd))
            {
                finished = ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, mLastSendExCmdDict[cmd]);
            }
            else
            {
                finished = ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, ExtendCMDCode.Extend_Code_None);
            }
        }
        if (cmd != CMDCode.Read_Sensor_Data_Other)
        {
            finished = true;
        }
        if (finished && null != mSendedMsgData && (cmd == mSendedMsgData.cmd || mSendedMsgData.cmd == CMDCode.Read_Sensor_Data && cmd == CMDCode.Read_Sensor_Data_Other))
        {
            if (mSendedMsgData.cmd == CMDCode.Read_Back)
            {//回读命令需特殊处理，等所有的舵机都回了以后在发送下一条指令
                ReadBackMsg msg = (ReadBackMsg)mSendedMsgData.msg;
                msg.needReadBackCount--;
                if (msg.needReadBackCount <= 0)
                {
                    mSendedMsgData = null;
                    mLastSendTime = 0;
                    if (mSendMsgList.Count > 0)
                    {
                        SendMsg();
                    }
                }
            }
            else
            {
                mLastSendTime = 0;
                mSendedMsgData = null;
                if (mSendMsgList.Count > 0)
                {
                    SendMsg();
                }
            }
        }        
    }


    public void Update()
    {
#if !Test
        if (mLastSendTime > 0.1f)
        {
            float time = Time.time;
            if (null != mSendedMsgData)
            {
                //读取传感器的超时时间设为1秒，防止逻辑编程发送速度太快出现数据丢失等待时间过长
                bool outTime = (mSendedMsgData.cmd == CMDCode.Hand_Shake || mSendedMsgData.cmd == CMDCode.Read_Motherboard_Data) ? time - mLastSendTime >= Wait_Msg_Out_Time_Long : time - mLastSendTime >= Wait_Msg_Out_Time_Normal;
                if (outTime)
                {
                    if (mSendedMsgData.cmd != CMDCode.Read_Back)
                    {
                        NetWaitMsg.CloseWait();
                    }
                    if (mSendedMsgData.cmd == CMDCode.Ctrl_Action && ExtendCMDCode.CtrlAction == mSendedMsgData.extendCmd ||  mSendedMsgData.cmd == CMDCode.DuoJi_Turn && ExtendCMDCode.TurnAction == mSendedMsgData.extendCmd)
                    {//如果动作命令超时了，则删除队列里面的动作命令
                        //ClearCtrlAction();
                    }
                    ReceiveMsg(mSendedMsgData.cmd, 0, mSendedMsgData.mac, null);
                }
            }
        }
#endif

    }

    public void ClearCtrlAction()
    {
        for (int i = 0, imax = mSendMsgList.Count; i < imax; ++i)
        {
            if (CMDCode.Ctrl_Action == mSendMsgList[i].cmd && ExtendCMDCode.CtrlAction == mSendMsgList[i].extendCmd 
                || CMDCode.DuoJi_Turn == mSendMsgList[i].cmd && ExtendCMDCode.TurnAction == mSendMsgList[i].extendCmd
                || CMDCode.Ctrl_Motor == mSendMsgList[i].cmd)
            {
                mSendMsgList.RemoveAt(i);
                --i;
                --imax;
            }
        }
    }
    public void ClearAllMsg()
    {
        //lock (mLock)
        {
            mSendMsgList.Clear();
        }
        mLastSendExCmdDict.Clear();
        mLastSendTime = 0;
        mChargePromptFlag = false;
        mSendedMsgData = null;
    }

    public void ClearCacheMsg()
    {
        mSendMsgList.Clear();
    }
    #endregion

    #region 私有函数
    NetWork()
    {
        mSendMsgList = new List<SendMsgData>();
        mLastSendTime = 0;
        mLastSendExCmdDict = new Dictionary<CMDCode, ExtendCMDCode>();
        /*mSendThread = new Thread(new ThreadStart(Test));
        mSendThread.Start();*/
    }
    /*List<long> list = new List<long>();
    void Test()
    {
        while (true)
        {
            list.Add(DateTime.Now.Millisecond / 10000);
            if (list.Count > 1000)
            {
                break;
            }
        }
    }*/

    void AddSendMsg(SendMsgData msg)
    {
        if (msg.cmd == CMDCode.Ctrl_Action && msg.extendCmd != ExtendCMDCode.CtrlAction 
            || msg.cmd == CMDCode.Read_Sensor_Data 
            || msg.cmd == CMDCode.Read_Sensor_Data_Other
            || msg.cmd == CMDCode.DuoJi_Turn
            || msg.cmd == CMDCode.Ctrl_Motor
            || msg.cmd == CMDCode.Stop_Motor)
        {
            List<SendMsgData> delList = null;
            SendMsgData tmpMsg = null;
            for (int i = 0, imax = mSendMsgList.Count; i < imax; ++i)
            {
                tmpMsg = mSendMsgList[i];
                if (msg.cmd == CMDCode.Ctrl_Motor && (tmpMsg.cmd == CMDCode.Ctrl_Motor || tmpMsg.cmd == CMDCode.Stop_Motor))
                {//清除之前的旋转命令和停止命令
                    CtrlMotorMsg ctrlMsg = (CtrlMotorMsg)msg.msg;
                    if (tmpMsg.cmd == CMDCode.Stop_Motor && ctrlMsg.ContainsID(((StopMotorMsg)tmpMsg.msg).ids)
                        || tmpMsg.cmd == CMDCode.Ctrl_Motor && ctrlMsg.ContainsID((CtrlMotorMsg)tmpMsg.msg))
                    {
                        if (null == delList)
                        {
                            delList = new List<SendMsgData>();
                        }
                        delList.Add(tmpMsg);
                    }
                }
                else if (msg.cmd == CMDCode.Stop_Motor && (tmpMsg.cmd == CMDCode.Stop_Motor || tmpMsg.cmd == CMDCode.Ctrl_Motor))
                {//清除之前的旋转的命令和停止命令
                    StopMotorMsg stopMsg = (StopMotorMsg)msg.msg;
                    if (tmpMsg.cmd == CMDCode.Stop_Motor && stopMsg.ContainsID(((StopMotorMsg)tmpMsg.msg).ids)
                        || tmpMsg.cmd == CMDCode.Ctrl_Motor && stopMsg.ContainsID((CtrlMotorMsg)tmpMsg.msg))
                    {
                        if (null == delList)
                        {
                            delList = new List<SendMsgData>();
                        }
                        delList.Add(tmpMsg);
                    }
                }
                else if (msg.cmd != CMDCode.DuoJi_Turn && msg.cmd == tmpMsg.cmd && msg.extendCmd == tmpMsg.extendCmd 
                    || msg.cmd == CMDCode.DuoJi_Turn && msg.msg.Equals(tmpMsg.msg))
                {//轮模式只要id相同则覆盖之前等待的命令,轮模式消息重载了Equals
                    mSendMsgList.Remove(tmpMsg);
                    break;
                }
            }
            if (null != delList)
            {
                /*PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "msg = " + msg.msg.ToString());
                for (int i = 0, imax = delList.Count; i < imax; ++i)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "del = " + delList[i].msg.ToString());
                }*/
                for (int i = 0, imax = delList.Count; i < imax; ++i)
                {
                    mSendMsgList.Remove(delList[i]);
                }
                delList = null;
            }
        }
        mSendMsgList.Add(msg);
    }

    SendMsgData GetSendMsg()
    {
        if (mSendMsgList.Count > 0)
        {
            SendMsgData msg = mSendMsgList[0];
            mSendMsgList.RemoveAt(0);
            return msg;
        }
        return null;
    }

    void SendMsg()
    {
        try
        {
            SendMsgData sendData = GetSendMsg();
            if (null == sendData)
            {
                return;
            }
            if (mSendedMsgData == sendData)
            {//防止回包里面发了包引起同一个包重复发送
                return;
            }
            mSendedMsgData = sendData;
            //SendMsgData sendData = mSendMsgList.Dequeue();
            if (null != sendData)
            {
                MemoryStream DataStream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(DataStream);
                CBaseMsg msg = sendData.msg;
                CMDCode cmd = sendData.cmd;
                if (null != msg)
                {
                    msg.Write(writer);
                }
                byte[] pMsg = DataStream.ToArray();

                writer.Close();
                DataStream.Close();
                if (cmd != CMDCode.Read_Sensor_Data_Other && cmd != CMDCode.Robot_Update_Write && cmd != CMDCode.Servo_Update_Write && cmd != CMDCode.Sensor_Update_Write)
                {
                    PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "send time =" + Time.time + ",msg cmd=" + cmd.ToString() + " length=" + pMsg.Length + "msg=" + PublicFunction.BytesToHexString(pMsg));
                }

                mLastSendExCmdDict[cmd] = sendData.extendCmd;
                /*if (sendData.cmd == CMDCode.DuoJi_Turn && null != RobotManager.GetInst().GetCurrentRobot())
                {//记录轮模式发送方向
                    DjTurnMsg djMsg = (DjTurnMsg)sendData.msg;
                    Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != djMsg.ids)
                    {
                        for (int i = 0, imax = djMsg.ids.Count; i < imax; ++i)
                        {
                            DuoJiData data = robot.GetAllDjData().GetDjData(djMsg.ids[i]);
                            if (null != data)
                            {
                                //记录上次数据
                                data.lastTurnData.turnDirection = (TurnDirection)djMsg.turnDirection;
                                data.lastTurnData.turnSpeed = djMsg.turnSpeed;
                            }
                            
                        }
                    }
                }*/
                PlatformMgr.Instance.SendMsg((byte)cmd, pMsg, pMsg.Length);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
        mLastSendTime = Time.time;

#if UNITY_EDITOR
        Game.Timer.Add(0.5f, 1, 1, delegate () {
            SingletonObject<MsgTestManager>.GetInst().ReceiveMsg(mSendedMsgData);
        });
#endif
    }

    
    #endregion
}


public class SendMsgData
{
    public string mac;
    public CMDCode cmd;
    public CBaseMsg msg;
    public ExtendCMDCode extendCmd;

    public SendMsgData(CMDCode cmd, CBaseMsg msg, string mac, ExtendCMDCode exCmd)
    {
        this.cmd = cmd;
        this.msg = msg;
        this.mac = mac;
        this.extendCmd = exCmd;
    }
}