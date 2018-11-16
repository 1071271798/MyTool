using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:MoveGroup.cs
/// Description:
/// Time:2017/10/18 15:35:15
/// </summary>
public class MoveGroup
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<MoveType, Dictionary<MoveSpeed, MoveSequence>> mActionDict = null;
    #endregion

    #region 公有函数
    public MoveGroup()
    {
        mActionDict = new Dictionary<MoveType, Dictionary<MoveSpeed, MoveSequence>>();
    }

    public void AddMoveAction(MoveAction action)
    {
        if (action.moveType != MoveType.Undefined)
        {
            if (!mActionDict.ContainsKey(action.moveType))
            {
                Dictionary<MoveSpeed, MoveSequence> dict = new Dictionary<MoveSpeed, MoveSequence>();
                mActionDict[action.moveType] = dict;
            }
            if (!mActionDict[action.moveType].ContainsKey(action.moveSpeed))
            {
                MoveSequence sequence = new MoveSequence();
                mActionDict[action.moveType][action.moveSpeed] = sequence;
            }
            mActionDict[action.moveType][action.moveSpeed].AddMoveAction(action);
        }
    }

    public MoveSequence GetPlayMove(MoveType moveType, MoveSpeed moveSpeed, float arg, Robot robot)
    {
        MoveSequence selectMoveSequence = null;
        MoveSequenceType seqType = GetSequenceType(moveType, robot);
        if (mActionDict.ContainsKey(moveType))
        {
            //判断是轮模式运动还是角度模式运动
            if (IsTurnModel())
            {//轮模式只需判断速度
                selectMoveSequence = GetMove(moveType, moveSpeed, seqType);
                if (null != selectMoveSequence)
                {
                    if (selectMoveSequence.sequenceType == MoveSequenceType.undefined)
                    {
                        selectMoveSequence.sequenceType = seqType;
                    }
                    selectMoveSequence.playSpeed = GetPlaySpeed(moveSpeed, selectMoveSequence.sequenceType);
                    selectMoveSequence.playArg = arg;
                    selectMoveSequence.moveType = moveType;
                    selectMoveSequence.moveSpeed = moveSpeed;
                }
            }
            else
            {//角度模式
                selectMoveSequence = GetAngleMove(moveType, moveSpeed, seqType);
                if (null != selectMoveSequence)
                {
                    if (selectMoveSequence.sequenceType == MoveSequenceType.undefined)
                    {
                        selectMoveSequence.sequenceType = seqType;
                    }
                    selectMoveSequence.playSpeed = GetPlaySpeed(moveSpeed, selectMoveSequence.sequenceType);
                    selectMoveSequence.playArg = arg;
                    selectMoveSequence.moveType = moveType;
                    selectMoveSequence.moveSpeed = moveSpeed;
                }
            }
        }
        return selectMoveSequence;
    }

    public List<MoveType> GetMoveList()
    {
        List<MoveType> list = null;
        foreach (var key in mActionDict.Keys)
        {
            if (key > MoveType.Undefined && key < MoveType.StandBy)
            {
                if (null == list)
                {
                    list = new List<MoveType>();
                }
                list.Add(key);
            }
        }
        if (null != list)
        {
            list.Sort();
        }
        return list;
    }
    public bool IsTurnModel()
    {
        if (mActionDict.ContainsKey(MoveType.TurnLeft) || mActionDict.ContainsKey(MoveType.TurnRight))
        {
            return true;
        }
        return false;
    }

    public MoveSequenceType GetSequenceType(MoveType moveType, Robot robot)
    {
        if (IsTurnModel())
        {
            /*if (moveType == MoveType.TurnLeft || moveType == MoveType.TurnRight)
            {
                return MoveSequenceType.degree;
            }
            else*/
            if (null != robot && robot.GetAllDjData().GetTurnList().Count >= 2)
            {
                return MoveSequenceType.time_byServo;
            } else
            {
                return MoveSequenceType.time_byMotor;
            }
        }
        return MoveSequenceType.distance;
    }

    public static float GetPlaySpeed(MoveSpeed moveSpeed, MoveSequenceType seqType)
    {
        if (seqType == MoveSequenceType.distance)
        {
            switch (moveSpeed)
            {
                case MoveSpeed.VS:
                    return 0.6667f;
                case MoveSpeed.S:
                    return 0.8f;
                case MoveSpeed.M:
                    return 1f;
                case MoveSpeed.F:
                    return 1.33333f;
                case MoveSpeed.VF:
                    return 2f;
                default:
                    return PublicFunction.Default_Play_Speed;
            }
        } else if (seqType == MoveSequenceType.time_byMotor)
        {
            switch (moveSpeed)
            {
                case MoveSpeed.VS:
                    return 0.40f;
                case MoveSpeed.S:
                    return 0.55f;
                case MoveSpeed.M:
                    return 0.7f;
                case MoveSpeed.F:
                    return 0.85f;
                case MoveSpeed.VF:
                    return 1f;
                default:
                    return PublicFunction.Default_Play_Speed;
            }
        } else
        {
            switch (moveSpeed)
            {
                case MoveSpeed.VS:
                    return 0.5f;
                case MoveSpeed.S:
                    return 0.75f;
                case MoveSpeed.M:
                    return 1f;
                case MoveSpeed.F:
                    return 1.25f;
                case MoveSpeed.VF:
                    return 1.5f;
                default:
                    return PublicFunction.Default_Play_Speed;
            }
        }
        
    }
    #endregion

    #region 其他函数
    MoveSequence GetAngleMove(MoveType moveType, MoveSpeed moveSpeed, MoveSequenceType seqType)
    {
        MoveSequence sequence = GetMove(moveType, moveSpeed, seqType);
        if (null != sequence)
        {
            if (!sequence.HaveActionType(ActionType.Head))
            {//没有起始动作
                //先取默认速度的起始动作
                Dictionary<int, MoveAction>  headAction = GetAction(moveType, PublicFunction.Default_Move_Speed, ActionType.Head);
                if (null == headAction)
                {
                    //取不到，取复位动作
                    headAction = GetStandBy(moveSpeed, ActionType.Head);
                }
                if (null != headAction)
                {
                    sequence.AddMoveDict(ActionType.Head, headAction);
                }
            }
            if (!sequence.HaveActionType(ActionType.Footer))
            {//没有结束动作
                //先取默认速度的结束动作
                Dictionary<int, MoveAction> footer = GetAction(moveType, PublicFunction.Default_Move_Speed, ActionType.Footer);
                if (null == footer)
                {
                    //取不到，取复位动作
                    footer = GetStandBy(moveSpeed, ActionType.Footer);
                }
                if (null != footer)
                {
                    sequence.AddMoveDict(ActionType.Footer, footer);
                }
            }
        }
        return sequence;
    }    

    MoveSequence GetMove(MoveType moveType, MoveSpeed moveSpeed, MoveSequenceType seqType)
    {
        if (mActionDict[moveType].ContainsKey(moveSpeed))
        {
            return new MoveSequence(mActionDict[moveType][moveSpeed]);
        } else if (seqType == MoveSequenceType.time_byMotor && mActionDict[moveType].ContainsKey(PublicFunction.Default_Motor_Speed))
        {
            return new MoveSequence(mActionDict[moveType][PublicFunction.Default_Motor_Speed]);
        }
        else if (mActionDict[moveType].ContainsKey(PublicFunction.Default_Move_Speed))
        {
            return new MoveSequence(mActionDict[moveType][PublicFunction.Default_Move_Speed]);
        }
        return null;
    }

    Dictionary<int, MoveAction> GetAction(MoveType moveType, MoveSpeed moveSpeed, ActionType actionType)
    {
        if (mActionDict[moveType].ContainsKey(moveSpeed))
        {
            return mActionDict[moveType][moveSpeed].GetMoveDict(actionType);
        }
        return null;
    }

    Dictionary<int, MoveAction> GetStandBy(MoveSpeed moveSpeed, ActionType actionType)
    {
        if (mActionDict.ContainsKey(MoveType.StandBy))
        {
            MoveSequence sequence = null;
            if (mActionDict[MoveType.StandBy].ContainsKey(moveSpeed))
            {
                sequence = mActionDict[MoveType.StandBy][moveSpeed];
            } else if (mActionDict[MoveType.StandBy].ContainsKey(PublicFunction.Default_Move_Speed))
            {
                sequence = mActionDict[MoveType.StandBy][PublicFunction.Default_Move_Speed];
            }
            if (null != sequence)
            {
                if (sequence.HaveActionType(actionType))
                {
                    return sequence.GetMoveDict(actionType);
                }
                return sequence.GetMoveDict(PublicFunction.Default_Action_Type);
            }
        }
        return null;
    }

    #endregion
}