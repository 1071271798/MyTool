using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Scene;
using Game.Event;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:SetScene.cs
/// Description:设置界面
/// Time:2015/7/29 10:51:55
/// </summary>
public class SetScene : BaseScene
{
    #region 公有属性
    #endregion

    #region 私有属性
    Robot mRobot;
    //ShowRobotIDUI mShowRobotDjID;
    SetDeviceUI mSetDevice;
    //SetResetUI mSetReset;
    Transform mCenter;
    GameObject mRobotObj;
    Transform mBackBtn;
    SetSceneType mSceneType;
    bool mChangeDeviceIdFlag;
    List<Transform> mDjIdTransList;
    #endregion

    #region 公有函数
    public SetScene(SetSceneType type)
    {
        //mResPath = "Prefab/UI/SetDuoJi";
        mSceneType = type;
        mUIList = new List<BaseUI>();
        /*mShowRobotDjID = new ShowRobotIDUI(ShowRobotIDType.SetModelID);
        mSetMenu = new SetMenuUI();*/
        mSetDevice = new SetDeviceUI();
        /*mSetReset = new SetResetUI();
        mUIList.Add(mShowRobotDjID);
        mUIList.Add(mSetMenu);*/
        mUIList.Add(mSetDevice);
        //mUIList.Add(mSetReset);

       //mDjIdTransList = new List<Transform>();
    }


    public static void GotoSetScene(SetSceneType type)
    {
        /*if (SceneMgr.GetCurrentSceneType() != SceneType.EmptyScene)
        {
            SceneMgr.EnterScene(SceneType.EmptyScene);
        }*/
        object[] arg = new object[1];
        arg[0] = type;
        SceneMgr.EnterScene(SceneType.EmptyScene, typeof(SetScene), arg);
        //SceneManager.GetInst().GotoScene(typeof(SetScene), arg);
        /*GameObject obj = GameObject.Find("MainClient/UI Root/Camera");
        if (null != obj)
        {
            Camera camera = obj.GetComponent<Camera>();
            if (null != camera)
            {
                camera.backgroundColor = new Color(241 / 255.0f, 241 / 255.0f, 241 / 255.0f);
            }
        }*/
    }

    public override void UpdateScene()
    {
        base.UpdateScene();
        mChangeDeviceIdFlag = false;
        RobotEventListener.RegisterSetDJEvent();
        EventMgr.Inst.Regist(EventID.Select_Duoji_For_Robot, OnSelectDuoJi);
        EventMgr.Inst.Regist(EventID.Set_Change_DuoJi_Data, SwitchDuoJiDataDlgt);
        EventMgr.Inst.Regist(EventID.Set_DuoJI_Start_ID, SetDuoJiStartIDDlgt);
        EventMgr.Inst.Regist(EventID.Close_Set_Model_ID_UI, SetModelIDUIClose);
        EventMgr.Inst.Regist(EventID.Close_Set_Device_ID_UI, SetDeviceIDUIClose);
        EventMgr.Inst.Regist(EventID.Change_Device_ID, ChangeDeviceIDBack);
        if (null != mTrans)
        {
            
            mCenter = mTrans.Find("Center");
            if (null != mCenter)
            {
                mCenter.localPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            }
        }
        /*mRobotObj = GameObject.Find("Robot");
        if (null != mRobotObj)
        {
            MoveSecond ms = mRobotObj.GetComponent<MoveSecond>();
            if (null != ms)
            {
                List<string> list = ms.GetAllDjName();
                if (null != list)
                {
                    mDjIdTransList.Clear();
                    for (int i = 0, icount = list.Count; i < icount; ++i)
                    {
                        GameObject obj = ms.FindGOByName(list[i]);
                        if (null != obj)
                        {
                            mDjIdTransList.Add(obj.transform);
                        }
                    }
                }
                
            }
            mRobotObj.SetActive(false);
        }*/

        OpenSceneUI(mSceneType);
        mRobot = RobotManager.GetInst().GetCurrentRobot();

        /*if (null != mRobot)
        {//设置舵机起始id
            SetDuoJiStartIDMsg.ShowMsg(!RobotManager.GetInst().GetSetDuoJiFlag(mRobot.ID));
        }*/
    }

    public override void Close()
    {
        base.Close();
        /*GameObject obj = GameObject.Find("MainClient/UI Root/Camera");
        if (null != obj)
        {
            Camera camera = obj.GetComponent<Camera>();
            if (null != camera)
            {
                camera.backgroundColor = new Color(46 / 255.0f, 59 / 255.0f, 85 / 255.0f);
            }
        }*/
    }

    public override void Release()
    {
        base.Release();
        EventMgr.Inst.UnRegist(EventID.Select_Duoji_For_Robot, OnSelectDuoJi);
        EventMgr.Inst.UnRegist(EventID.Set_Change_DuoJi_Data, SwitchDuoJiDataDlgt);
        EventMgr.Inst.UnRegist(EventID.Set_DuoJI_Start_ID, SetDuoJiStartIDDlgt);
        EventMgr.Inst.UnRegist(EventID.Close_Set_Model_ID_UI, SetModelIDUIClose);
        EventMgr.Inst.UnRegist(EventID.Close_Set_Device_ID_UI, SetDeviceIDUIClose);
        EventMgr.Inst.UnRegist(EventID.Change_Device_ID, ChangeDeviceIDBack);
    }
    #endregion

    #region 保护函数
    

    protected override void OnButtonClick(GameObject obj)
    {
        try
        {
            if (obj.name.Equals("Button"))
            {
                if (null != mRobot)
                {
                    RobotManager.GetInst().RecordSetDjOver(mRobot.ID);
                }
                SceneMgr.EnterScene(SceneType.EditAction, typeof(ActionEditScene));
            }
            else if (obj.name.Equals("MenuBtn1"))
            {
                if (null != mRobotObj)
                {
                    mRobotObj.SetActive(true);
                }
                mBackBtn.gameObject.SetActive(false);
                //mSetMenu.OnClose();
                //mShowRobotDjID.InitDuoJi(mDjIdTransList);
                //mShowRobotDjID.Open();
            }
            else if (obj.name.Equals("MenuBtn2"))
            {
                mBackBtn.gameObject.SetActive(false);
                //mSetMenu.OnClose();
                mSetDevice.Open();
            }
            else if (obj.name.Equals("MenuBtn3"))
            {
                mBackBtn.gameObject.SetActive(false);
                if (mChangeDeviceIdFlag && PlatformMgr.Instance.GetBluetoothState())
                {//有改id，且当前处于连接状态则断开蓝牙
                    PlatformMgr.Instance.DisConnenctBuletooth();
                    mChangeDeviceIdFlag = false;
                }
                //mSetMenu.OnClose();
                //mSetReset.Open();
            }
            else if (obj.name.Equals("MenuBtn4"))
            {
                if (null != mRobot)
                {
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {
                        mRobot.SelfCheck(!mRobot.SelfCheckFlag);
                        //mSetMenu.SetSelfBtn();
                    }
                    else
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("QingLianJieSheBei"), 1, CommonTipsColor.red);
                    }
                }
                else
                {
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("QingLianJieSheBei"), 1, CommonTipsColor.red);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    #endregion

    #region 私有函数
    void OpenSceneUI(SetSceneType type)
    {
        try
        {
            RobotMgr.Instance.GoToCommunity();
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
        mSceneType = type;
        switch (mSceneType)
        {
            case SetSceneType.SetSceneTypeMenu:
                //mSetMenu.Open(mBtnDelegate);
                break;
            case SetSceneType.SetSceneTypeModel:
                if (null != mRobotObj)
                {
                    mRobotObj.SetActive(true);
                }
                if (null != mBackBtn)
                {
                    mBackBtn.gameObject.SetActive(false);
                }
                //mShowRobotDjID.InitDuoJi(mDjIdTransList);
                //mShowRobotDjID.Open();
                break;
            case SetSceneType.SetSceneTypeDevice:
                if (null != mBackBtn)
                {
                    mBackBtn.gameObject.SetActive(false);
                }
                mSetDevice.Open();
                PlatformMgr.Instance.CloseWaitingPage();
                break;
        }
    }
    void OnSelectDuoJi(EventArg args)
    {
        try
        {
            int id = (int)args[0];
            if (null != mRobot)
            {
                /*DuoJiData data = mRobot.GetAnDjData(id);
                if (null != data)
                {
                    SetDuoJiMsg.ShowSetDuoJiMsg(mRobot, data);
                }*/
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SwitchDuoJiDataDlgt(EventArg args)
    {
        try
        {
            if (null != mRobot)
            {
                mRobot.SwitchDuoJiId((int)args[0], (int)args[1]);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void SetDuoJiStartIDDlgt(EventArg args)
    {
        try
        {
            int id = (int)args[0];
            if (null != mRobot)
            {
                mRobot.SetStartDuoJiID(id);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SetModelIDUIClose(EventArg args)
    {
        try
        {
            if (null != mRobot)
            {
                if (mRobot.SaveAllActions())
                {
                    RobotManager.GetInst().RecordSetDjOver(mRobot.ID);
                    RobotDataMgr.Instance.ReviseDJid(mRobot.Name);
                }
                
            }
            //mSetMenu.Open(mBtnDelegate);
            if (null != mRobotObj)
            {
                mRobotObj.SetActive(false);
            }
            if (null != mBackBtn)
            {
                mBackBtn.gameObject.SetActive(true);
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    void SetDeviceIDUIClose(EventArg args)
    {
        try
        {
            /*if (null != mSetMenu && null != mBtnDelegate)
            {
                mSetMenu.Open(mBtnDelegate);
            }*/
            if (null != mBackBtn)
            {
                mBackBtn.gameObject.SetActive(true);
            }

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void ChangeDeviceIDBack(EventArg arg)
    {
        mChangeDeviceIdFlag = true;
    }
    #endregion
}

public enum SetSceneType
{
    SetSceneTypeMenu,
    SetSceneTypeModel,
    SetSceneTypeDevice,
}