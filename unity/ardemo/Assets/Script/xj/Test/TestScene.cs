using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Scene;
using Game.Event;
using Game.Platform;
/// <summary>
/// Author:xj
/// FileName:TestScene.cs
/// Description:
/// Time:2016/6/30 16:18:01
/// </summary>
public class TestScene : BaseScene
{
    MultithreadLoad mLoadUI;
    InterfaceTestView mInterfaceUI;


    public TestScene()
    {
        mResPath = "Prefab/UI/Test";
        mUIList = new List<BaseUI>();
        mLoadUI = new MultithreadLoad();
        mUIList.Add(mLoadUI);
        mInterfaceUI = new InterfaceTestView();
        mUIList.Add(mInterfaceUI);
    }
    public override void UpdateScene()
    {
        base.UpdateScene();
        EventMgr.Inst.Regist(EventID.Back_Test_Scene, BackTestScene);
        try
        {
            RobotMgr.Instance.GoToCommunity();
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
        if (null != mTrans)
        {
            Transform BtnCancel = mTrans.Find("BtnBack");
            if (null != BtnCancel)
            {
                BtnCancel.localPosition = UIManager.GetWinPos(BtnCancel, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
            }
        }
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Release()
    {
        base.Release();
        EventMgr.Inst.UnRegist(EventID.Back_Test_Scene, BackTestScene);
    }




    protected override void OnButtonClick(GameObject obj)
    {
        try
        {
            string name = obj.name;
            if (name.Equals("BtnBack"))
            {
                ClientMain.GetInst().UseTestModelFlag = false;
                MainScene.GotoScene();
            }
            else if (name.Equals("btn1"))
            {
                mLoadUI.Open();
                mTrans.gameObject.SetActive(false);
            }
            else if (name.Equals("btn2"))
            {
                mInterfaceUI.Open();
                mTrans.gameObject.SetActive(false);
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    void BackTestScene(EventArg args)
    {
        mTrans.gameObject.SetActive(true);
    }
}