using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
/// <summary>
/// Author:xj
/// FileName:PlayMotor.cs
/// Description:
/// Time:2017/12/18 19:31:10
/// </summary>
public class MyPlayMotor
{
    #region 公有属性
    #endregion

    #region 其他属性
    private Dictionary<byte, SendMotorData> mMotorDict;
    Robot mRobot;
    private List<SendMotorData> mSendList;
    int mSendCount;
    #endregion

    #region 公有函数
    public MyPlayMotor(Robot robot)
    {
        mRobot = robot;
        mMotorDict = new Dictionary<byte, SendMotorData>();
    }


    public bool HaveMotorTurn()
    {
        foreach (var kvp in mMotorDict)
        {
            if (kvp.Value.direction != TurnDirection.turnStop)
            {
                return true;
            }
        }
        return false;
    }

    public void CtrlMotor(List<SendMotorData> list)
    {
        List<byte> stopList = new List<byte>();
        List<SendMotorData> sendList = new List<SendMotorData>();
        /*for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            if (mMotorDict.ContainsKey(list[i].id) && mMotorDict[list[i].id].lastDirection != TurnDirection.turnStop && mMotorDict[list[i].id].lastDirection != list[i].direction)
            {
                stopList.Add(list[i].id);
            }
        }
        //转向需要先停止
        if (stopList.Count > 0)
        {
            StopMotor(stopList);
        }*/
        for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            if (!mMotorDict.ContainsKey(list[i].id) || mMotorDict[list[i].id].lastDirection != list[i].direction || mMotorDict[list[i].id].lastSpeed != list[i].speed || mMotorDict[list[i].id].time != list[i].time)
            {
                list[i].lastDirection = list[i].direction;
                list[i].lastSpeed = list[i].speed;
                sendList.Add(list[i]);
                mMotorDict[list[i].id] = list[i];
            }
        }
        mSendList = sendList;
        if (mSendList.Count > 0)
        {
            mSendCount = mSendList.Count;
            mRobot.SendMotorMsg(sendList);
        } else
        {
            SingletonObject<LogicCtrl>.GetInst().PlayMotorCallBack(CallUnityResult.success);
        }

    }

    public void StopMotor(List<byte> ids)
    {
        mRobot.StopMotorMsg(ids);
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            if (mMotorDict.ContainsKey(ids[i]))
            {
                mMotorDict[ids[i]].direction = TurnDirection.turnStop;
                mMotorDict[ids[i]].speed = 0;
                mMotorDict[ids[i]].lastDirection = TurnDirection.turnStop;
                mMotorDict[ids[i]].lastSpeed = 0;
            }
        }
    }

    public void StopMotor()
    {
        List<byte> stopList = new List<byte>();
        foreach (var kvp in mMotorDict)
        {
            if (kvp.Value.direction != TurnDirection.turnStop)
            {
                stopList.Add(kvp.Value.id);
            }
        }
        if (stopList.Count > 0)
        {
            StopMotor(stopList);
        }
    }

    public void CtrlMotorCallBack(CallUnityResult result)
    {
        bool finished = false;
        if (result == CallUnityResult.success)
        {
            if (null != mSendList)
            {
                for (int i = 0, imax = mSendList.Count; i < imax; ++i)
                {
                    SendMotorData item = mSendList[i];
                    //item.lastDirection = item.direction;
                    //item.lastSpeed = item.speed;
                    if (item.time < 65535)
                    {//马达停止了
                        //-0.1是防止block重复块未改变状态就返回了
                        float delayTime = item.time / 10.0f - 0.1f;
                        if (delayTime <= 0)
                        {
                            item.lastDirection = TurnDirection.turnStop;
                            item.lastSpeed = 0;
                            item.direction = TurnDirection.turnStop;
                            item.speed = 0;
                            mSendCount--;
                        }
                        else
                        {
                            Timer.Add(delayTime, 1, 1, delegate ()
                            {
                                item.lastDirection = TurnDirection.turnStop;
                                item.lastSpeed = 0;
                                item.direction = TurnDirection.turnStop;
                                item.speed = 0;
                                mSendCount--;
                            });
                        }

                        Timer.Add(item.time / 10.0f, 1, 1, delegate ()
                        {
                            if (mSendCount == 0)
                            {//完成
                                SingletonObject<LogicCtrl>.GetInst().PlayMotorCallBack(result);
                            }
                        });
                    }
                    else
                    {
                        finished = true;
                    }
                }
            }
        } else if (result == CallUnityResult.failure)
        {
            finished = true;
            if (null != mSendList)
            {
                foreach (var item in mSendList)
                {
                    item.lastDirection = TurnDirection.turnStop;
                    item.lastSpeed = 0;
                    item.direction = TurnDirection.turnStop;
                    item.speed = 0;
                }
            }
        }
        if (finished)
        {
            SingletonObject<LogicCtrl>.GetInst().PlayMotorCallBack(result);
        }
    }

    public void StopMotorCallBack(CallUnityResult result)
    {
        /*if (result == CallUnityResult.success)
        {
            foreach (var kvp in mMotorDict)
            {
                if (kvp.Value.direction == TurnDirection.turnStop)
                {
                    kvp.Value.lastDirection = TurnDirection.turnStop;
                    kvp.Value.speed = 0;
                }
            }
        }
        else if (result == CallUnityResult.failure)
        {
            foreach (var kvp in mMotorDict)
            {
                if (kvp.Value.direction == TurnDirection.turnStop)
                {
                    kvp.Value.direction = kvp.Value.lastDirection;
                    kvp.Value.speed = kvp.Value.lastSpeed;
                }
            }
        }*/
        SingletonObject<LogicCtrl>.GetInst().StopMotorCallBack(result);
    }


    public List<byte> GetSendIds()
    {
        if (null != mSendList)
        {
            List<byte> ids = new List<byte>();
            for (int i = 0, imax = mSendList.Count; i < imax; ++i)
            {
                ids.Add(mSendList[i].id);
            }
            return ids;
        }
        return null;
    }


    public void CleanUp()
    {
        if (null != mMotorDict)
        {
            mMotorDict.Clear();
        }
        if (null != mSendList)
        {
            mSendList.Clear();
        }
    }
    #endregion

    #region 其他函数
    #endregion
}