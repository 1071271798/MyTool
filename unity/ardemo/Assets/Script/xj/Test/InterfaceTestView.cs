using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Event;
using Game;
/// <summary>
/// Author:xj
/// FileName:InterfaceTestView.cs
/// Description:
/// Time:2016/7/1 12:19:46
/// </summary>
public class InterfaceTestView : BaseUI
{
    enum ViewState
    {
        Menu,
        Logic,
    }

    ViewState mViewState = ViewState.Menu;

    UIInput mLogicInput;
    GameObject mLogicObj;
    GameObject mMenuObj;

    List<string> mCmdList = new List<string>();
    int mCmdIndex = 0;
    bool repeatFlag = false;
    bool mQueryFlag = false;
    UILabel mQueryLabel = null;
    long mQueryIndex = -1;
    string mLastSendCmd = string.Empty;
    public InterfaceTestView()
    {
        mUIResPath = "Prefab/UI/InterfaceTestView";
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        SingletonObject<LogicCtrl>.GetInst().logicCmdCallBack = LogicCmdCallBack;
        if (null != mTrans)
        {
            Transform logic = mTrans.Find("logic");
            if (null != logic)
            {
                mLogicObj = logic.gameObject;
            }
            Transform grid = mTrans.Find("grid");
            if (null != grid)
            {
                mMenuObj = grid.gameObject;
            }
            Transform BtnCancel = mTrans.Find("BtnCancel");
            if (null != BtnCancel)
            {
                BtnCancel.localPosition = UIManager.GetWinPos(BtnCancel, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
            }
            mQueryLabel = GameHelper.FindChildComponent<UILabel>(mTrans, "logic/qureyBtn/Label");
            SetQueryLabel();
            mLogicInput = GameHelper.FindChildComponent<UIInput>(mTrans, "logic/Input");
        }
        
    }

    public override void Release()
    {
        base.Release();
        if (-1 != mQueryIndex)
        {
            Timer.Cancel(mQueryIndex);
        }
        SingletonObject<LogicCtrl>.GetInst().logicCmdCallBack = null;
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        string name = obj.name;
        if (name.Equals("btnlogic"))
        {
            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null != robot)
            {
                LogicCtrl.GetInst().OpenLogicForRobot(robot);
                SetViewState(ViewState.Logic);
            }
            else
            {
                Debuger.Log("未选中模型");
            }
        }
        else if (name.Equals("submitBtn"))
        {
            repeatFlag = false;
            ExecubteLogic();
        }
        else if (name.Equals("repeatBtn"))
        {//重复执行
            if (repeatFlag)
            {
                repeatFlag = false;
            }
            else
            {
                repeatFlag = true;
                ExecubteLogic();
            }
        }
        else if (name.Equals("qureyBtn"))
        {//查询所有传感器
            mQueryFlag = !mQueryFlag;
            SetQueryLabel();
            if (mQueryFlag)
            {
                if (-1 != mQueryIndex)
                {
                    Timer.Cancel(mQueryIndex);
                }
                mQueryIndex = Timer.Add(0.5f, 0.05f, 0, OpenQueryAllSensor);
            }
            else
            {
                if (-1 != mQueryIndex)
                {
                    Timer.Cancel(mQueryIndex);
                    mQueryIndex = -1;
                }
            }
        }
        else if (name.Equals("stopBtn"))
        {//停止
            repeatFlag = false;
            mQueryFlag = false;
            SetQueryLabel();
            if (-1 != mQueryIndex)
            {
                Timer.Cancel(mQueryIndex);
                mQueryIndex = -1;
            }
            LogicCtrl.GetInst().CallUnityCmd(SingletonObject<LogicCtrl>.GetInst().Logic_Cmd_Start + "stopRobot");
        }
        else if (name.Equals("BtnCancel"))
        {
            if (ViewState.Menu == mViewState)
            {
                OnClose();
                EventMgr.Inst.Fire(EventID.Back_Test_Scene);
            }
            else
            {
                if (mViewState == ViewState.Logic)
                {
                    LogicCtrl.GetInst().ExitLogic();
                }
                SetViewState(ViewState.Menu);
            }
        }
    }

    void ExecubteLogic()
    {
        if (null != mLogicInput)
        {
            string text = mLogicInput.value;
            if (!string.IsNullOrEmpty(text) && text.StartsWith(LogicCtrl.GetInst().Logic_Cmd_Start))
            {
                text = text.Replace("\r", "");
                string[] cmds = text.Split('\n');
                mCmdList.Clear();
                mCmdIndex = 0;
                for (int i = 0, imax = cmds.Length; i < imax; ++i)
                {
                    if (!string.IsNullOrEmpty(cmds[i]))
                    {
                        mCmdList.Add(cmds[i]);
                        //LogicCtrl.GetInst().CallUnityCmd(cmds[i]);
                    }
                }
                RunLogicCmd();
            }
        }
    }

    void RunLogicCmd()
    {
        if (mCmdIndex < mCmdList.Count)
        {
            string cmd = mCmdList[mCmdIndex];
            mCmdIndex++;
            mLastSendCmd = cmd.Split('|')[0];
            LogicCtrl.GetInst().CallUnityCmd(cmd);
        }
        else if (repeatFlag && mCmdList.Count > 0)
        {
            mCmdIndex = 0;
            RunLogicCmd();
        }
    }

    void LogicCmdCallBack(string result)
    {
        if (!string.IsNullOrEmpty(mLastSendCmd))
        {
            if (result.Contains(mLastSendCmd))
            {
                RunLogicCmd();
            }
        }
    }


    void SetQueryLabel()
    {
        if (null != mQueryLabel)
        {
            if (mQueryFlag)
            {
                mQueryLabel.text = "关闭传感器";
            }
            else
            {
                mQueryLabel.text = "开启传感器";
            }
        }
    }

    void OpenQueryAllSensor()
    {
        SingletonObject<LogicCtrl>.GetInst().CallUnityCmd(SingletonObject<LogicCtrl>.GetInst().Logic_Cmd_Start + "queryAllSensor");
    }

    void SetViewState(ViewState state)
    {
        if (state != mViewState)
        {
            SetViewActive(mViewState, false);
            SetViewActive(state, true);
            mViewState = state;
        }
    }

    void SetViewActive(ViewState state, bool active)
    {
        switch (state)
        {
            case ViewState.Menu:
                SetGameObjectActive(mMenuObj, active);
                break;
            case ViewState.Logic:
                SetGameObjectActive(mLogicObj, active);
                break;
        }
    }

    void SetGameObjectActive(GameObject obj, bool active)
    {
        if (null != obj)
        {
            obj.SetActive(active);
        }
    }
}