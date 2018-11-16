using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:MoveSequence.cs
/// Description:
/// Time:2017/10/19 10:03:11
/// </summary>
public class MoveSequence
{
    #region 公有属性
    public float playSpeed = PublicFunction.Default_Play_Speed;
    public float playArg = 0;
    public MoveSequenceType sequenceType;
    //记录数据
    public MoveType moveType;
    public MoveSpeed moveSpeed;
    #endregion

    #region 其他属性
    Dictionary<ActionType, Dictionary<int, MoveAction>> moveDict;
    #endregion

    #region 公有函数
    public MoveSequence()
    {
        moveDict = new Dictionary<ActionType, Dictionary<int, MoveAction>>();
    }

    public MoveSequence(MoveSequence sequence)
    {//不完全拷贝
        moveDict = new Dictionary<ActionType, Dictionary<int, MoveAction>>();
        foreach (var kvp in sequence.moveDict)
        {
            moveDict[kvp.Key] = kvp.Value;
        }
        if (null != sequence && sequence.sequenceType != MoveSequenceType.undefined)
        {
            this.sequenceType = sequence.sequenceType;
        }
    }

    public void AddMoveAction(MoveAction action)
    {
        if (!moveDict.ContainsKey(action.actionType))
        {
            Dictionary<int, MoveAction> dict = new Dictionary<int, MoveAction>();
            moveDict[action.actionType] = dict;
        }
        moveDict[action.actionType][action.actionStep] = action;
        if (action.actionType == PublicFunction.Default_Action_Type)
        {
            sequenceType = action.sequenceType;
        }
    }

    public Dictionary<int, MoveAction> GetMoveDict(ActionType actionType)
    {
        if (moveDict.ContainsKey(actionType))
        {
            return moveDict[actionType];
        }
        return null;
    }

    public bool HaveActionType(ActionType actionType)
    {
        if (moveDict.ContainsKey(actionType))
        {
            return true;
        }
        return false;
    }

    public void AddMoveDict(ActionType actionType, Dictionary<int, MoveAction> dict)
    {
        moveDict[actionType] = dict;
    }

    public void CleanUp()
    {
        moveDict.Clear();
    }

    public int AciontCount
    {
        get { return moveDict.Count; }
    }
    public bool IsTurnMove()
    {
        if (sequenceType != MoveSequenceType.distance && sequenceType != MoveSequenceType.undefined)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region 其他函数
    #endregion
}

public enum MoveSequenceType : byte
{
    undefined = 0,
    degree,
    time_byServo,
    time_byMotor,
    distance,
}
