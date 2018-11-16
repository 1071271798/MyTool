using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.Event;

/// <summary>
/// Author:xj
/// FileName:RobotCtrl.cs
/// Description:
/// Time:2015/6/26 14:36:26
/// </summary>
public class RobotCtrl
{
    #region 公有属性
    #endregion

    #region 私有属性
    static RobotCtrl mInst = null;
    Dictionary<RobotEventID, RobotEventDelegate> mEventDlgt = null;
    #endregion

    #region 公有函数
    public static RobotCtrl GetInst()
    {
        if (null == mInst)
        {
            mInst = new RobotCtrl();
        }
        return mInst;
    }
    /// <summary>
    /// 出发事件
    /// </summary>
    /// <param name="mac"></param>
    /// <param name="id"></param>
    /// <param name="arg"></param>
    public void PostEvent(string mac, RobotEventID id, EventArg arg)
    {
        if (mEventDlgt.ContainsKey(id))
        {
            if (null != mEventDlgt[id])
            {
                mEventDlgt[id](mac, arg);
            }
        }
    }

    public void RegisterEvent(RobotEventID id, RobotEventDelegate dlgt)
    {
        if (!mEventDlgt.ContainsKey(id))
        {
            mEventDlgt.Add(id, new RobotEventDelegate(dlgt));
        }
        else
        {
            mEventDlgt[id] += new RobotEventDelegate(dlgt);
        }
    }

    public void RemoveEvent(RobotEventID id, RobotEventDelegate dlgt)
    {
        if (mEventDlgt.ContainsKey(id))
        {
            mEventDlgt[id] -= dlgt;
        }
    }

    #endregion

    #region 私有函数
    RobotCtrl()
    {
        mEventDlgt = new Dictionary<RobotEventID, RobotEventDelegate>();
    }
    #endregion
}

public delegate void RobotEventDelegate(string mac, EventArg arg);

public enum RobotEventID
{
    //Read_Back_Msg_Ack,//回读返回


}
