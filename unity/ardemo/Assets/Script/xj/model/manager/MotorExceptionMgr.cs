using Game;
using Game.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MotorExceptionMgr : SingletonObject<MotorExceptionMgr>
{
    #region 公有属性
    #endregion

    #region 其他属性
    MotorExceptionType mExceptionType = MotorExceptionType.None;
    List<byte> mExceptionMotorList;
    Robot mRobot;
    #endregion

    #region 公有函数
    public MotorExceptionMgr()
    {
        mExceptionMotorList = new List<byte>();
    }
    /// <summary>
    /// 增加异常id，把新异常的id添加到以前的异常id里面
    /// </summary>
    /// <param name="ids"></param>
    public void SetException(MotorExceptionType exception, List<byte> ids)
    {
        if (ids.Count > 0)
        {
            if (exception != mExceptionType)
            {
                mExceptionMotorList = ids;
            }
            else
            {
                for (int i = 0, imax = ids.Count; i < imax; ++i)
                {
                    if (!mExceptionMotorList.Contains(ids[i]))
                    {
                        mExceptionMotorList.Add(ids[i]);
                    }
                }
            }
            mExceptionMotorList.Sort();
            mExceptionType = exception;
        }
    }
    /// <summary>
    /// 更新一条异常数据，替换以前数据
    /// </summary>
    /// <param name="ids"></param>
    public void UpdateException(MotorExceptionType exception, List<byte> ids)
    {
        if (ids.Count > 0)
        {
            mExceptionMotorList = ids;
            mExceptionMotorList.Sort();
            mExceptionType = exception;
        }
    }

    public List<byte> GetExceptionIds(MotorExceptionType exception)
    {
        if (exception == mExceptionType)
        {
            return mExceptionMotorList;
        }
        return null;
    }
    public MotorExceptionType GetServoException()
    {
        return mExceptionType;
    }

    public string GetExceptionTips()
    {
        if (mExceptionType != MotorExceptionType.None)
        {
            if (SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
            {
                switch(mExceptionType)
                {
                    case MotorExceptionType.disconnect:
                        return string.Format(LogicLanguage.GetText("马达无法控制"), PublicFunction.ListToString(mExceptionMotorList));
                    case MotorExceptionType.turnProtect:
                        return string.Format(LogicLanguage.GetText("马达转动异常"), PublicFunction.ListToString(mExceptionMotorList)); ;
                }
            }
            else
            {
                switch(mExceptionType)
                {
                    case MotorExceptionType.disconnect:
                        return string.Format(LauguageTool.GetIns().GetText("马达无法控制"), PublicFunction.ListToString(mExceptionMotorList));
                    case MotorExceptionType.turnProtect:
                        return string.Format(LauguageTool.GetIns().GetText("马达转动异常"), PublicFunction.ListToString(mExceptionMotorList)); ;
                }
            }
        }
        return string.Empty;
    }
    public override void CleanUp()
    {
        base.CleanUp();
        mExceptionMotorList.Clear();
        mExceptionType = MotorExceptionType.None;
    }

    public bool ShowExceptionTips(Robot robot)
    {
        mRobot = robot;
        if (mExceptionType != MotorExceptionType.None)
        {
            if (mExceptionMotorList.Count > 0)
            {
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(GetExceptionTips(), SelfExceptionOnClick);
                }
                else if (PlatformMgr.Instance.IsInUnity)
                {
                    PromptMsg.ShowDoublePrompt(GetExceptionTips(), SelfCheckErrorOnClick);
                }
            }
            return true;
        }
        
        return false;
    }

    public bool CheckException(Robot robot)
    {
        mRobot = robot;
        if (mExceptionType != MotorExceptionType.None)
        {
            if (mExceptionMotorList.Count > 0)
            {
                if (mExceptionType == MotorExceptionType.disconnect)
                {
                    ReadMotherboard();
                } else
                {
                    RepairMotor();
                }
                return true;
            }
        }
        return false;
    }
    #endregion

    #region 其他函数
    void SelfExceptionOnClick(bool confirmFlag)
    {
        if (confirmFlag)
        {
            if (mExceptionType == MotorExceptionType.disconnect)
            {
                ReadMotherboard();
            } else
            {
                RepairMotor();
            }
        }
        else
        {
            //SingletonObject<ServoExceptionMgr>.GetInst().CleanUp();
            //mSelfCheckErrorFlag = false;
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
            if (mExceptionType == MotorExceptionType.disconnect)
            {
                ReadMotherboard();
            }
            else
            {
                RepairMotor();
            }
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

    void RepairMotor()
    {
        if (null != mRobot)
        {
            mRobot.RepairMotorException();
        }
    }
    #endregion
}

/// <summary>
/// 马达异常类型
/// </summary>
public enum MotorExceptionType : byte
{
    None = 0,
    turnProtect,//堵转保护
    disconnect,//马达断开
    eProtect,//电流保护
    cProtect,//温度保护
    hfProtect,//过压保护
    lfProtect,//欠压保护
    encryptProtect,//熔丝位或加密错误保护
}
