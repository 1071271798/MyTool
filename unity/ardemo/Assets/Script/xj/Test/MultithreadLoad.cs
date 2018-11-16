using Game.Event;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:MultithreadLoad.cs
/// Description:
/// Time:2016/6/30 17:53:58
/// </summary>
public class MultithreadLoad : BaseUI
{
    public MultithreadLoad()
    {
        mUIResPath = "Prefab/UI/loadImgView";
    }


    protected override void AddEvent()
    {
        base.AddEvent();
        if (null != mTrans)
        {
            Transform BtnCancel = mTrans.Find("BtnCancel");
            if (null != BtnCancel)
            {
                BtnCancel.localPosition = UIManager.GetWinPos(BtnCancel, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
            }
            Transform BtnLoad = mTrans.Find("BtnLoad");
            if (null != BtnLoad)
            {
                BtnLoad.localPosition = UIManager.GetWinPos(BtnLoad, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
            }
        }
    }




    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        string name = obj.name;
        if (name.Equals("BtnCancel"))
        {
            OnClose();
            EventMgr.Inst.Fire(EventID.Back_Test_Scene);
        }
        else if (name.Equals("BtnLoad"))
        {
            
        }
    }
}