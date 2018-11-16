using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
/// <summary>
/// Author:xj
/// FileName:PlayMove.cs
/// Description:
/// Time:2017/10/19 9:59:54
/// </summary>
public class PlayMove
{
    #region 公有属性
    #endregion

    #region 其他属性
    MoveSequence mNowPlayMove;
    Robot mRobot;
    int mNowPlayIndex = 0;
    ActionType mNowActionType;
    int mNowActionStep;
    Action mLastAction;
    float mPlayStartTime;
    long mTurnTimerIndex = -1;
    bool mWaitTurnFlag = false;
    int mWaitTurnCount = -1;
    public int WaitTurnCount
    {
        get { return mWaitTurnCount; }
        set { mWaitTurnCount = value; }
    }
    #endregion

    #region 公有函数
    public PlayMove(Robot robot)
    {
        mRobot = robot;
    }

    public bool IsPlaying()
    {
        if (null != mNowPlayMove)
        {
            return true;
        }
        return false;
    }

    public void Play(MoveSequence move)
    {
        try
        {
            mWaitTurnFlag = false;
            WaitTurnCount = -1;
            CannelTurnTimer();
            SingletonObject<MyTime>.GetInst().StopTime();
            if (null != mNowPlayMove && null != mRobot && mNowPlayMove.IsTurnMove())
            {
                mRobot.StopRunTurn();
            }
            if (null != mRobot && null != move)
            {
                mNowPlayMove = move;
                PlayMoveActionByActionType(ActionType.Head, PublicFunction.Default_Action_Step);
            }
            else
            {
                PlayFinished();
            }
        }
        catch (System.Exception ex)
        {
            PlayFail();
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    /*public void Stop()
    {
        CannelTurnTimer();
        SingletonObject<MyTime>.GetInst().StopTime();
        if (null != mNowPlayMove)
        {
            NetWork.GetInst().ClearCtrlAction();
            if (null != mRobot)
            {
                mRobot.StopAllTurn();
            }
            PlayFinished();
        }
    }*/

    public void StopPlay(CallUnityResult result = CallUnityResult.success)
    {
        CannelTurnTimer();
        SingletonObject<MyTime>.GetInst().StopTime();
        if (null != mNowPlayMove)
        {
            if (result == CallUnityResult.success)
            {
                PlayFinished();
            } else
            {
                mNowPlayMove = null;
                SingletonObject<LogicCtrl>.GetInst().PlayMotionCallBack(CallUnityResult.failure);
            }
        }
    }

    public void PlayTurnFinished()
    {
        if (mWaitTurnFlag)
        {
            mWaitTurnCount--;
            if (WaitTurnCount <= 0 && null != mNowPlayMove && mNowPlayMove.playArg == 0 && mNowPlayMove.IsTurnMove())
            {//记录计时器
                SingletonObject<LogicCtrl>.GetInst().PlayMotionCallBack(CallUnityResult.success);
            }
        }
    }
    #endregion

    #region 其他函数

    private void OpenTurnMoveTimer()
    {
        mPlayStartTime = Time.time;
        CannelTurnTimer();
        if (mNowPlayMove.playArg != 0)
        {
            if (mNowPlayMove.sequenceType == MoveSequenceType.time_byServo || mNowPlayMove.sequenceType == MoveSequenceType.time_byMotor)
            {
                mTurnTimerIndex = Timer.Add(mNowPlayMove.playArg, 1, 1, PlayTurnCallBack);
            }
            else if (mNowPlayMove.sequenceType == MoveSequenceType.degree)
            {//角度转换成时间
                mTurnTimerIndex = Timer.Add(AngleToTime((UInt32)mNowPlayMove.playArg, mNowPlayMove.playSpeed), 1, 1, PlayTurnCallBack);
            }
        }
    }

    private float AngleToTime(UInt32 angle, float speed)
    {
        float time = 2f ;
        if (PlatformMgr.Instance.PowerData.power > 0)
        {
            time *= 83f / PlatformMgr.Instance.PowerData.power;
        }
        time *= angle / 90.0f;
        return time / speed;
    }
    private void PlayMoveActionByActionType(ActionType actionType, int actionStep)
    {
        if (null != mNowPlayMove && null != mRobot)
        {
            do 
            {
                mNowActionStep = actionStep;
                mNowActionType = actionType;
                Dictionary<int, MoveAction> dict = mNowPlayMove.GetMoveDict(actionType);
                if (null != dict && dict.Count > 0)
                {
                    if (actionType == ActionType.Body && !mNowPlayMove.IsTurnMove())
                    {
                        if (mNowActionStep == dict.Count + 1 && mNowPlayMove.playArg == 0)
                        {//无步数限制的在此时回调
                            SingletonObject<LogicCtrl>.GetInst().PlayMotionCallBack(CallUnityResult.success);
                        }
                        int index = GetActionStepIndex(actionStep, dict);
                        if (dict.ContainsKey(index))
                        {
                            PlayMoveAction(dict[index]);
                        } else
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, string.Format("robot name = {0} moveType = {1} moveSpeed = {2} 不存在index = {3}的动作", mRobot.Name, mNowPlayMove.moveType.ToString(), mNowPlayMove.moveSpeed.ToString(), index));
                            //播放失败
                            PlayNextAction();
                        }
                    } else
                    {
                        PlayMoveAction(dict[actionStep]);
                        if (actionType == PublicFunction.Default_Action_Type && mNowPlayIndex == 0 && mNowPlayMove.IsTurnMove())
                        {//记录计时器
                            if (mNowPlayMove.playArg == 0)
                            {//无时间限制的此时回调
                                mWaitTurnFlag = true;
                            }
                            OpenTurnMoveTimer();
                        }
                    }
                    break;
                }
                //播放失败
                PlayNextAction();
            } while (false);
        }
        else
        {
            PlayFail();
        }
    }

    private void PlayMoveAction(MoveAction action)
    {
        mNowPlayIndex = 0;
        try
        {
            if (action.Count > mNowPlayIndex)
            {
                float playSpeed = action.playSpeed;
                if (mNowPlayMove.moveSpeed != action.moveSpeed)
                {
                    playSpeed = mNowPlayMove.playSpeed;
                }
                if (null == mLastAction)
                {//需发送所有舵机
                    mRobot.CtrlAction(action[mNowPlayIndex], true, playSpeed);
                }
                else
                {
                    mRobot.CtrlAction(action[mNowPlayIndex], mLastAction, playSpeed);
                }
                mLastAction = action[mNowPlayIndex];
                int startTime = 0;
                for (int i = mNowPlayIndex, imax = action.Count; i < imax; ++i)
                {
                    startTime += PublicFunction.GetActionTime(action[i].AllTime, playSpeed);
                    MyTime.GetInst().Add(startTime / 1000.0f, PlayMoveActionCallBack, i);
                }
            }
            else
            {
                PlayNextAction();
            }
        }
        catch (System.Exception ex)
        {
            PlayFail();
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    private void PlayMoveActionCallBack(object[] obj)
    {
        try
        {
            int index = (int)obj[0];
            index++;
            if (null == mNowPlayMove)
            {//动作还未播放完，时间到了
                return;
            }
            Dictionary<int, MoveAction> dict = mNowPlayMove.GetMoveDict(mNowActionType);
            if (null != mRobot && null != dict && dict.Count > 0)
            {
                int step = GetActionStepIndex(mNowActionStep, dict);
                if (dict.ContainsKey(step))
                {
                    if (index < dict[step].Count)
                    {
                        mNowPlayIndex = index;
                        float playSpeed = dict[step].playSpeed;
                        if (mNowPlayMove.moveSpeed != dict[step].moveSpeed)
                        {
                            playSpeed = mNowPlayMove.playSpeed;
                        }
                        mRobot.CtrlAction(dict[step][index], mLastAction, playSpeed);
                        mLastAction = dict[step][index];
                    }
                    else if (!dict[step].IsTurnModel() || !mNowPlayMove.IsTurnMove() || mNowActionType != PublicFunction.Default_Action_Type)
                    {
                        PlayNextAction();
                    }
                    else
                    {//轮模式，由定时器来结束

                    }
                }
                else
                {
                    PlayNextAction();
                }
            }
            else
            {
                PlayFail();
            }
        }
        catch (System.Exception ex)
        {
            PlayFail();
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    private void PlayTurnCallBack()
    {
        MyTime.GetInst().StopTime();//停止其他动作帧，防止时间过短，动作帧未播完，时间到了
        PlayNextAction();
    }
    private void PlayNextAction()
    {
        if (mNowActionType == ActionType.Head)
        {
            PlayMoveActionByActionType(ActionType.Body, PublicFunction.Default_Action_Step);
        }
        else if (mNowActionType == ActionType.Body)
        {
            Dictionary<int, MoveAction> dict = mNowPlayMove.GetMoveDict(ActionType.Body);
            if (null == dict || mNowPlayMove.IsTurnMove() || mNowActionStep >= mNowPlayMove.playArg && mNowPlayMove.playArg > 0)
            {
                int index = PublicFunction.Default_Action_Step;
                if (null != dict && dict.Count > 0)
                {
                    index = GetActionStepIndex(mNowActionStep, dict);
                }
                Dictionary<int, MoveAction> footer = mNowPlayMove.GetMoveDict(ActionType.Footer);
                if (null != footer && footer.ContainsKey(index))
                {
                    PlayMoveActionByActionType(ActionType.Footer, index);
                } else
                {
                    PlayMoveActionByActionType(ActionType.Footer, PublicFunction.Default_Action_Step);
                }
            }
            else
            {
                PlayMoveActionByActionType(ActionType.Body, mNowActionStep + 1);
            }
        }
        else
        {
            PlayFinished();
        }
    }

    private void CannelTurnTimer()
    {
        if (-1 != mTurnTimerIndex)
        {
            Timer.Cancel(mTurnTimerIndex);
            mTurnTimerIndex = -1;
        }
    }

    private int GetActionStepIndex(int actionStep, Dictionary<int, MoveAction> dict)
    {
        int index = actionStep % dict.Count == 0 ? dict.Count : actionStep % dict.Count;
        return index;
    }


    private void PlayFinished()
    {
        if (null != mNowPlayMove)
        {
            if (mNowPlayMove.IsTurnMove())
            {
                mRobot.StopRunTurn();
            }
            mNowPlayMove = null;
            SingletonObject<LogicCtrl>.GetInst().PlayMotionCallBack(CallUnityResult.success);
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "播放成功");
        }
    }

    private void PlayFail()
    {
        CannelTurnTimer();
        SingletonObject<MyTime>.GetInst().StopTime();
        if (null != mNowPlayMove)
        {
            NetWork.GetInst().ClearCtrlAction();
            if (null != mRobot)
            {
                mRobot.StopAllTurn();
            }
        }
        SingletonObject<LogicCtrl>.GetInst().PlayMotionCallBack(CallUnityResult.failure);
        PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "播放失败");
    }
    #endregion
}