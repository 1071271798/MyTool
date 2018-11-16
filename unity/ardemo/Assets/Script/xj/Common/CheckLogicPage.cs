using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Event;
/// <summary>
/// Author:xj
/// FileName:CheckLogicPage.cs
/// Description:
/// Time:2017/7/5 18:18:55
/// </summary>
/// 
public class CheckLogicPage : SingletonObject<CheckLogicPage>
{
    #region 公有属性
    public enum CheckType : byte
    {
        Blue_Search,
        Set_Servo
    }
    #endregion

    #region 其他属性
    long mCheckIndex = -1;
    bool isCheck = false;
    CheckType mType = CheckType.Blue_Search;
    #endregion

    #region 公有函数

    public void OpenCheck(CheckType type)
    {

        mType = type;
        StartCheck();
        if (isCheck)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "openCheck");
        }
    }

    public void ReCheck()
    {
        if (isCheck)
        {
            if (!PlatformMgr.Instance.IsInUnity)
            {
                CloseCheck();
            }
            else
            {
                StartCheck();
            }
        }
    }

    public void CloseCheck()
    {
        if (-1 != mCheckIndex)
        {
            Timer.Cancel(mCheckIndex);
        }
        isCheck = false;
        mCheckIndex = -1;
    }
    #endregion

    #region 其他函数
    void StartCheck()
    {
        isCheck = true;
        if (PopWinManager.GetInst().GetCount() < 1)
        {
            if (-1 != mCheckIndex)
            {
                Timer.Cancel(mCheckIndex);
            }
            mCheckIndex = Timer.Add(1, 1, 1, Check);
        }
    }
    void Check()
    {
        if (PlatformMgr.Instance.IsInUnity && PopWinManager.GetInst().GetCount() < 1)
        {
            if (mType == CheckType.Blue_Search)
            {
                SingletonObject<ConnectCtrl>.GetInst().ConnectFinishedFail();
                EventMgr.Inst.Fire(EventID.Blue_Connect_Finished);
            }
            else
            {
                SingletonObject<LogicCtrl>.GetInst().CloseSetServoModel();
            }
        }
        mCheckIndex = -1;
    }
    #endregion
}