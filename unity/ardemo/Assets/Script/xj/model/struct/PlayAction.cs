using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:PlayAction.cs
/// Description:播放动作的类
/// Time:2017/10/13 10:36:25
/// </summary>
public class PlayAction
{
    #region 公有属性
    public delegate void PlayActionsDelegate(int index, bool finished);
    #endregion

    #region 其他属性
    ActionSequence mNowPlayActions;
    int mNowPlayIndex = 0;
    PlayActionsDelegate mPlayActionDlgt = null;
    Robot mRobot;
    #endregion

    #region 公有函数
    public PlayAction(Robot robot)
    {
        mRobot = robot;
    }

    public bool IsPlaying()
    {
        if (null != mNowPlayActions)
        {
            return true;
        }
        return false;
    }

    public void Play(ActionSequence actions, PlayActionsDelegate dlgt = null, int startIndex = 0)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("PlayActions name = {0}", actions.Name));
        SingletonObject<MyTime>.GetInst().StopTime();
        if (null != mRobot)
        {
            try
            {
                mPlayActionDlgt = dlgt;
                if (actions.Count > startIndex)
                {
                    mNowPlayActions = actions;
                    mNowPlayIndex = startIndex;
                    if (0 == startIndex)
                    {
                        mRobot.CtrlAction(actions[startIndex], true, actions.playSpeed);
                        if (!SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
                        {
                            SingletonObject<ActionAudioManager>.GetInst().PlayActionAudio(mRobot.ID, actions.Id);
                        }
                    }
                    else
                    {
                        mRobot.CtrlAction(actions[startIndex], actions[startIndex - 1], actions.playSpeed);
                    }
                    
                    int startTime = 0;
                    for (int i = startIndex, imax = actions.Count; i < imax; ++i)
                    {
                        startTime += PublicFunction.GetActionTime(actions[i].AllTime, actions.playSpeed);
                        MyTime.GetInst().Add(startTime / 1000.0f, PlayActionsCallBack, i);
                    }
                }                
            }
            catch (System.Exception ex)
            {
                PlayFail();
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        else
        {
            PlayFail();
        }
    }
    

    
    public void Pause()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "PauseActions");
        MyTime.GetInst().PauseTime();
        if (null == mRobot)
        {
            return;
        }
        if (SingletonObject<ServoExceptionMgr>.GetInst().ShowExceptionTips(mRobot))
        {
            return;
        }
        if (null != mNowPlayActions && mNowPlayActions.Count > 0)
        {
            int index = mNowPlayIndex;
            if (index < 0)
            {
                index = 0;
            }
            else if (index >= mNowPlayActions.Count)
            {
                index = mNowPlayActions.Count - 1;
            }
            mRobot.PauseTurnAction(mNowPlayActions[index]);
        }
    }

    public void Continue()
    {
        if (null == mRobot)
        {
            return;
        }
        if (SingletonObject<ServoExceptionMgr>.GetInst().ShowExceptionTips(mRobot))
        {
            return;
        }
        if (null != mNowPlayActions)
        {
            int index = mNowPlayIndex + 1;
            if (index < mNowPlayActions.Count)
            {
                MyTime.GetInst().ContinueTime();
            }
            else if (mNowPlayActions.IsTurnModel())
            {
                int lastIndex = mNowPlayActions.Count - 1;
                if (lastIndex >= 0)
                {
                    mRobot.TurnAction(mNowPlayActions[lastIndex], true, mNowPlayActions.playSpeed);
                    if (null != mNowPlayActions[lastIndex].motorDict && mNowPlayActions[lastIndex].motorDict.Count > 0)
                    {
                        mRobot.MotorAction(mNowPlayActions[lastIndex].motorDict, true, mNowPlayActions.playSpeed);
                    }
                }
            }
            else
            {
                mNowPlayActions = null;
            }
        }
    }

    public void StopNowPlayActions(CallUnityResult result = CallUnityResult.success)
    {
        mNowPlayIndex = 0;
        MyTime.GetInst().StopTime();
        NetWork.GetInst().ClearCtrlAction();
        if (null != mRobot)
        {
            mRobot.StopRunTurn();
        }
        if (null != mNowPlayActions)
        {
            EventMgr.Inst.Fire(EventID.Stop_Robot_Actions, new EventArg(mNowPlayActions));
            mNowPlayActions = null;
            if (result == CallUnityResult.failure)
            {
                SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.failure);
            }
        }
    }

    public bool HaveRunServo()
    {
        if (null != mNowPlayActions)
        {
            for (int i = 0, imax = mNowPlayActions.Count; i < imax; ++i)
            {
                if (mNowPlayActions[i].HaveTurnServo())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HaveRunMotor()
    {
        if (null != mNowPlayActions)
        {
            for (int i = 0, imax = mNowPlayActions.Count; i < imax; ++i)
            {
                if (mNowPlayActions[i].HaveMotor())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void StopControlerlPlayActions()
    {
        mNowPlayIndex = 0;
        MyTime.GetInst().StopTime();
        NetWork.GetInst().ClearCtrlAction();
        if (null != mNowPlayActions)
        {
            
            EventMgr.Inst.Fire(EventID.Stop_Robot_Actions, new EventArg(mNowPlayActions));
            mNowPlayActions = null;
        }
    }

    public void CleanUp()
    {
        mNowPlayIndex = 0;
    }
    #endregion

    #region 其他函数

    void PlayActionsCallBack(object[] obj)
    {
        int index = (int)obj[0];
        index++;
        if (null != mNowPlayActions)
        {
            if (index < mNowPlayActions.Count)
            {
                mNowPlayIndex = index;
                mRobot.CtrlAction(mNowPlayActions[index], mNowPlayActions[index - 1], mNowPlayActions.playSpeed);
                
                if (null != mPlayActionDlgt)
                {
                    mPlayActionDlgt(index, false);
                }
            }
            else if (!mNowPlayActions.IsTurnModel())
            {
                mNowPlayActions = null;
                if (null != mPlayActionDlgt)
                {
                    mPlayActionDlgt(index, true);
                }
                SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.success);
            }
            else
            {
                SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.success);
                if (null != mPlayActionDlgt)
                {
                    mPlayActionDlgt(index, false);
                }
            }
        }
        else
        {
            SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.success);
        }

    }

    private void PlayFinished()
    {

    }

    private void PlayFail()
    {

    }
    #endregion
}