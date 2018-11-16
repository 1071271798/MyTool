using Game;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Platform;
using Game.Event;
using Game.Scene;

/// <summary>
/// Author:xj
/// FileName:PublicPrompt.cs
/// Description:
/// Time:2015/12/30 16:50:36
/// </summary>
public class PublicPrompt
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数

    /// <summary>
    /// 设置复位动作弹框
    /// </summary>
    public static void ShowResetPrompt()
    {
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot)
        {
            robot.RobotPowerDown();
        }
        PromptMsg.ShowDoublePrompt("WeiSheZhiFuWei", ResetPromptOnClick);
    }
    /// <summary>
    /// 点击蓝牙按钮显示页面
    /// </summary>
    public static void ShowClickBlueBtnMsg(ConnectionTriggerPage page)
    {
        if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCreateRobot());
        }
        else
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                TopologyBaseMsg.ShowInfoMsg(ConnectionTriggerPage.Disconnect);
            }
            else
            {
                SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot());
            }

        }
    }

    public static void ShowDisconnect()
    {
        PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("是否要断开与设备的蓝牙连接？"), ShowDisconnectOnClick);
    }

    public static void ShowDelateWin(string text,ButtonDelegate.OnClick onclick)
    {
        PromptMsg.ShowDoublePrompt(text, onclick);
    }

    static EventDelegate.Callback sShowChargeCallBack = null;
    static bool sOpenShowChargePrompt = false;
    /// <summary>
    /// 充电保护提示
    /// </summary>
    /// <param name="callBack">点击按钮执行的事情，若无事件传空</param>
    public static void ShowChargePrompt(EventDelegate.Callback callBack)
    {
        if (sOpenShowChargePrompt)
        {
            return;
        }
        sOpenShowChargePrompt = true;
        sShowChargeCallBack = callBack;
        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("禁止边充边玩"), ShowChargePromptOnClick);
    }
    #endregion

    #region 其他函数
    /// <summary>
    /// 设置复位动作的按钮事件
    /// </summary>
    /// <param name="obj"></param>
    static void ResetPromptOnClick(GameObject obj)
    {
        try
        {
            string name = obj.name;
            if (name.Equals(PromptMsg.RightBtnName))
            {
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                if (null != robot)
                {
#if UNITY_EDITOR
                    Action action = new Action();
                    robot.GetNowAction(action);
                    robot.CreateDefualtActions(action);
#else
                    //robot.ReadRobotStartState();
#endif
                    Game.Event.EventMgr.Inst.Fire(Game.Event.EventID.SetDefaultOver);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    /// <summary>
    /// 断开蓝牙按钮事件
    /// </summary>
    /// <param name="obj"></param>
    static void ShowDisconnectOnClick(GameObject obj)
    {
        try
        {
            string name = obj.name;
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "点击断开蓝牙弹框按钮 name=" + name);
            if (name.Equals(PromptMsg.RightBtnName))
            {
                if (null != RobotManager.GetInst().GetCurrentRobot())
                {//有轮子在转动
                    RobotManager.GetInst().GetCurrentRobot().StopRunTurn();
                }
                PlatformMgr.Instance.DisConnenctBuletooth();
                if (SingletonObject<PopWinManager>.GetInst().IsExist(typeof(TopologyBaseMsg)))
                {
                    EventMgr.Inst.Fire(EventID.Exit_Blue_Connect);
                    SingletonObject<PopWinManager>.GetInst().ClosePopWin(typeof(TopologyBaseMsg));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    static void ShowChargePromptOnClick(GameObject obj)
    {
        try
        {
            sOpenShowChargePrompt = false;
            string name = obj.name;
            if (name.Equals(PromptMsg.RightBtnName))
            {
                if (null != sShowChargeCallBack)
                {
                    sShowChargeCallBack();
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    #endregion
}