using Game;
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ServoExceptionMgr.cs
/// Description:舵机异常管理
/// Time:2016/11/22 9:59:20
/// </summary>
public class ServoExceptionMgr : SingletonObject<ServoExceptionMgr>
{
    #region 公有属性
    #endregion

    #region 其他属性
    ServoExceptionType mExceptionType = ServoExceptionType.None;
    List<byte> mExceptionServoList;
    Robot mRobot;
    #endregion

    #region 公有函数
    public ServoExceptionMgr()
    {
        mExceptionServoList = new List<byte>();
    }
    /// <summary>
    /// 增加异常id，把新异常的id添加到以前的异常id里面
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="ids"></param>
    public void SetException(ServoExceptionType exception, List<byte> ids)
    {
        if (ids.Count > 0)
        {
            if (exception != mExceptionType)
            {
                mExceptionServoList = ids;
            }
            else
            {
                for (int i = 0, imax = ids.Count; i < imax; ++i)
                {
                    if (!mExceptionServoList.Contains(ids[i]))
                    {
                        mExceptionServoList.Add(ids[i]);
                    }
                }
            }
            mExceptionServoList.Sort();
            mExceptionType = exception;
        }
    }
    /// <summary>
    /// 更新一条异常数据，替换以前数据
    /// </summary>
    /// <param name="sensorType"></param>
    /// <param name="ids"></param>
    public void UpdateException(ServoExceptionType exception, List<byte> ids)
    {
        if (ids.Count > 0)
        {
            mExceptionServoList = ids;
            mExceptionServoList.Sort();
            mExceptionType = exception;
        }
    }

    public List<byte> GetServoExceptionIds(ServoExceptionType exception)
    {
        if (exception == mExceptionType)
        {
            return mExceptionServoList;
        }
        return null;
    }

    public ServoExceptionType GetServoException()
    {
        return mExceptionType;
    }


    public string GetExceptionTips()
    {
        if (mExceptionType != ServoExceptionType.None)
        {
            if (SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
            {
                switch (mExceptionType)
                {
                    case ServoExceptionType.turnProtect:
                        return string.Format(LogicLanguage.GetText("舵机转动异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.eProtect:
                        return string.Format(LogicLanguage.GetText("舵机电流异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.cProtect:
                        return string.Format(LogicLanguage.GetText("舵机温度异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.hfProtect:
                        return string.Format(LogicLanguage.GetText("舵机过压异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.lfProtect:
                        return string.Format(LogicLanguage.GetText("舵机欠压异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.otherProtect:
                        return string.Format(LogicLanguage.GetText("舵机其他异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.encryptProtect:
                        return string.Format(LogicLanguage.GetText("舵机已损坏，请更换该舵机"), PublicFunction.ListToString(mExceptionServoList));
                }
            }
            else
            {
                switch (mExceptionType)
                {
                    case ServoExceptionType.turnProtect:
                        return string.Format(LauguageTool.GetIns().GetText("舵机转动异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.eProtect:
                        return string.Format(LauguageTool.GetIns().GetText("舵机电流异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.cProtect:
                        return string.Format(LauguageTool.GetIns().GetText("舵机温度异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.hfProtect:
                        return string.Format(LauguageTool.GetIns().GetText("舵机过压异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.lfProtect:
                        return string.Format(LauguageTool.GetIns().GetText("舵机欠压异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.otherProtect:
                        return string.Format(LauguageTool.GetIns().GetText("舵机其他异常"), PublicFunction.ListToString(mExceptionServoList));
                    case ServoExceptionType.encryptProtect:
                        return string.Format(LauguageTool.GetIns().GetText("舵机已损坏，请更换该舵机"), PublicFunction.ListToString(mExceptionServoList));
                }
            }
        }
        return string.Empty;
    }
    public override void CleanUp()
    {
        base.CleanUp();
        mExceptionServoList.Clear();
        mExceptionType = ServoExceptionType.None;
    }

    public bool ShowExceptionTips(Robot robot)
    {
        mRobot = robot;
        if (mExceptionType != ServoExceptionType.None)
        {
            if (mExceptionType == ServoExceptionType.otherProtect || mExceptionType == ServoExceptionType.encryptProtect)
            {
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(GetExceptionTips(), SelfExceptionOnClick);
                }
                else if (PlatformMgr.Instance.IsInUnity)
                {
                    PromptMsg.CloseAll();
                    PromptMsg.ShowDoublePrompt(GetExceptionTips(), SelfCheckErrorOnClick);
                }
            }
            else
            {
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(GetExceptionTips(), SelfServoExceptionOnClick);
                }
                else if (PlatformMgr.Instance.IsInUnity)
                {
                    PromptMsg.CloseAll();
                    PromptMsg.ShowDoublePrompt(GetExceptionTips(), SelfServoCheckErrorOnClick);
                }
            }
            return true;
        }
        return false;
    }

    public bool CheckException(Robot robot)
    {
        mRobot = robot;
        if (mExceptionType != ServoExceptionType.None)
        {
            if (mExceptionType == ServoExceptionType.otherProtect || mExceptionType == ServoExceptionType.encryptProtect)
            {
                ReadMotherboard();
            }
            else
            {
                RepairServo();
            }
            return true;
        }
        return false;
    }
    #endregion

    #region 其他函数
    void SelfExceptionOnClick(bool confirmFlag)
    {
        if (confirmFlag)
        {
            ReadMotherboard();
        }
        else
        {
            //SingletonObject<ServoExceptionMgr>.GetInst().CleanUp();
            //mSelfCheckErrorFlag = false;
        }
    }

    void ReadMotherboard()
    {
        if (null != mRobot)
        {
            mRobot.HandShake();
            NetWaitMsg.ShowWait();
            //2秒以后读取初始角度
            Timer.Add(3, 0, 1, mRobot.ReadMotherboardData);
        }
    }

    void SelfServoCheckErrorOnClick(GameObject obj)
    {
        string btnName = obj.name;
        if (btnName.Equals(PromptMsg.LeftBtnName))
        {
            //SingletonObject<ServoExceptionMgr>.GetInst().CleanUp();
        }
        else if (btnName.Equals(PromptMsg.RightBtnName))
        {
            RepairServo();
        }
    }
    void RepairServo()
    {
        if (null != mRobot)
        {
            mRobot.RepairServoException();
        }
    }
    void SelfServoExceptionOnClick(bool confirmFlag)
    {
        if (confirmFlag)
        {
            RepairServo();
        }
        else
        {
        }
    }

    void SelfCheckErrorOnClick(GameObject obj)
    {
        string btnName = obj.name;
        if (btnName.Equals(PromptMsg.LeftBtnName))
        {//断开蓝牙
        }
        else if (btnName.Equals(PromptMsg.RightBtnName))
        {//握手命令
            ReadMotherboard();
        }
    }
    #endregion
}

/// <summary>
/// 舵机异常类型
/// </summary>
public enum ServoExceptionType : byte
{
    None = 0,
    turnProtect,//转动保护
    eProtect,//电流保护
    cProtect,//温度保护
    hfProtect,//过压保护
    lfProtect,//欠压保护
    otherProtect,//其它异常
    encryptProtect,//熔丝位或加密错误保护
}