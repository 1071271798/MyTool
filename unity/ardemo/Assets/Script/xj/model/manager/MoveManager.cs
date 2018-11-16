using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Resource;
using System.IO;
using System.Text;
/// <summary>
/// Author:xj
/// FileName:MoveManager.cs
/// Description:
/// Time:2017/10/13 11:31:02
/// </summary>
public class MoveManager : SingletonObject<MoveManager>
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<string, MoveGroup> mAllMoveDict = null;
    static string[] ActionIconList = { "", "icon_1", "icon_2", "icon_3", "icon_4", "icon_3", "icon_4", "" };
    #endregion

    #region 公有函数
    public MoveManager()
    {
        mAllMoveDict = new Dictionary<string, MoveGroup>();
    }

    public void ReadMoves(Robot robot)
    {
        try
        {
            mAllMoveDict.Remove(robot.ID);
            List<string> files = ResourcesEx.GetRobotMovesPath(robot);
            if (null != files && files.Count > 0)
            {
                files.Sort(delegate (string a, string b)
                {
                    FileInfo aFileInfo = new FileInfo(a);
                    FileInfo bFileInfo = new FileInfo(b);
                    if (aFileInfo.CreationTimeUtc.Ticks > bFileInfo.CreationTimeUtc.Ticks)
                    {
                        return 1;
                    }
                    else if (aFileInfo.CreationTimeUtc.Ticks < bFileInfo.CreationTimeUtc.Ticks)
                    {
                        return -1;
                    }
                    return 0;
                });
                for (int i = 0, icount = files.Count; i < icount; ++i)
                {
                    MoveAction moves = ReadXml(files[i], robot);
                    if (null != moves && moves.moveType != MoveType.Undefined)
                    {
                        AddRobotMove(moves.RobotID, moves);
                    }
                    else
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, string.Format("运动动作有问题 path = {0}", files[i]));
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public MoveSequence GetMoveSequence(string robotId, MoveType moveType, MoveSpeed moveSpeed, float playArg)
    {
        if (mAllMoveDict.ContainsKey(robotId))
        {
            Robot robot = RobotManager.GetInst().GetRobotForID(robotId);
            return mAllMoveDict[robotId].GetPlayMove(moveType, moveSpeed, playArg, robot);
        }
        return null;
    }

    public List<MoveType> GetRobotMoveList(string robotId)
    {
        if (mAllMoveDict.ContainsKey(robotId))
        {
            return mAllMoveDict[robotId].GetMoveList();
        }
        return null;
    }

    public MoveSequenceType GetSequenceType(Robot robot, MoveType moveType)
    {
        if (mAllMoveDict.ContainsKey(robot.ID))
        {
            return mAllMoveDict[robot.ID].GetSequenceType(moveType, robot);
        }
        return MoveSequenceType.undefined;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        mAllMoveDict.Clear();
    }

    public static string GetMoveTypeName(MoveType moveType)
    {
        return LauguageTool.GetIns().GetText(((MoveType_CN)moveType).ToString());
    }

    public static string GetMoveIconName(MoveType moveType)
    {
        return ActionsManager.GetInst().GetActionIconName(ActionIconList[(byte)moveType]);
    }

    
    #endregion

    #region 其他函数
    private MoveAction ReadXml(string path, Robot robot)
    {
        try
        {
            ActionSequence actionSequence = ActionsManager.ReadXml(path, robot);
            MoveSequenceType seqType = MoveSequenceType.distance;
            if (robot.GetAllDjData().GetTurnList().Count >= 2)
            {
                seqType = MoveSequenceType.time_byServo;
            } else if (robot.MotorsData != null && robot.MotorsData.Count > 0)
            {
                seqType = MoveSequenceType.time_byMotor;
            }
            MoveAction moveAction = new MoveAction(actionSequence, robot, seqType);
            return moveAction;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return null;
    }

    private void AddRobotMove(string robotId, MoveAction move)
    {
        if (!mAllMoveDict.ContainsKey(robotId))
        {
            MoveGroup group = new MoveGroup();
            mAllMoveDict[robotId] = group;
        }
        mAllMoveDict[robotId].AddMoveAction(move);
    }
    #endregion
}