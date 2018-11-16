using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:MoveAction.cs
/// Description:
/// Time:2017/10/17 10:07:40
/// </summary>
public class MoveAction : ActionSequence
{
    #region 公有属性
    public MoveType moveType = MoveType.Undefined;
    public MoveSpeed moveSpeed = PublicFunction.Default_Move_Speed;
    public int actionStep = PublicFunction.Default_Action_Step;
    public ActionType actionType = PublicFunction.Default_Action_Type;
    public MoveSequenceType sequenceType = MoveSequenceType.undefined;

    public override string Name
    {
        get
        {
            return MoveManager.GetMoveTypeName(moveType);
        }
        set
        {
            base.Name = value;
        }
    }

    public override string IconName
    {
        get
        {
            return MoveManager.GetMoveIconName(moveType);
        }
    }
    #endregion

    #region 其他属性
    
    protected List<List<Action>> mStepActions;
    #endregion

    #region 公有函数
    public MoveAction(string robotId) : base(robotId)
    {
    }

    /*public MoveAction(string name, string robotId, string actionsId, string icon, string showName, bool isofficial) : base(name, robotId, actionsId, icon, showName, isofficial)
    {

    }*/

    public MoveAction(ActionSequence act, Robot robot, MoveSequenceType seqType) : base(act, robot)
    {
        sequenceType = seqType;
        if (sequenceType == MoveSequenceType.time_byMotor)
        {//用于兼容马达速度不能超过140的问题
            moveSpeed = PublicFunction.Default_Motor_Speed;
        }
        MoveInitByName();
    }
    #endregion

    #region 其他函数
    void MoveInitByName()
    {
        try
        {
            string[] ary = null;
            if (name.Contains("_"))
            {
                ary = name.Split('_');
            } else
            {
                ary = name.Split('-');
            }
            if (null == ary || ary.Length == 0)
            {
                return;
            }
            MoveType_CN cn = (MoveType_CN)Enum.Parse(typeof(MoveType_CN), ary[0]);
            moveType = (MoveType)cn;
            if (moveType != MoveType.Undefined && ary.Length > 1)
            {
                for (int i = 1, imax = ary.Length; i < imax; ++i)
                {
                    if (ary[i].StartsWith("b"))
                    {//动作主体
                        string num = ary[i].Substring(1);
                        if (PublicFunction.IsInteger(num))
                        {
                            actionStep = int.Parse(num);
                        }
                    }
                    else if (ary[i].StartsWith("h"))
                    {//动作开头
                        actionType = ActionType.Head;
                    }
                    else if (ary[i].StartsWith("f"))
                    {//动作结束
                        actionType = ActionType.Footer;
                        string num = ary[i].Substring(1);
                        if (PublicFunction.IsInteger(num))
                        {
                            actionStep = int.Parse(num);
                        }
                    }
                    else if (ary[i].StartsWith("s"))
                    {//速度
                        string num = ary[i].Substring(1);
                        if (PublicFunction.IsInteger(num))
                        {
                            SetMoveSpeed(int.Parse(num));
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            moveType = MoveType.Undefined;
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SetMoveSpeed(int speed)
    {
        switch (speed)
        {
            case 1:
                moveSpeed = MoveSpeed.VS;
                break;
            case 2:
                moveSpeed = MoveSpeed.S;
                break;
            case 3:
                moveSpeed = MoveSpeed.M;
                break;
            case 4:
                moveSpeed = MoveSpeed.F;
                break;
            case 5:
                moveSpeed = MoveSpeed.VF;
                break;
            default:
                moveSpeed = PublicFunction.Default_Move_Speed;
                break;
        }
    }

    
    #endregion
}

/// <summary>
/// 运动类型
/// </summary>
public enum MoveType : byte
{
    Undefined = 0,
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    TurnLeft,
    TurnRight,
    StandBy,
}

public enum MoveType_CN : byte
{
    Undefined = 0,
    前进,
    后退,
    左移,
    右移,
    左转,
    右转,
    复位
}

/// <summary>
/// 运动速度
/// </summary>
public enum MoveSpeed : byte
{
    VS = 0,
    S,
    M,
    F,
    VF,
}

public enum ActionType : byte
{
    Head = 0,
    Body,
    Footer
}