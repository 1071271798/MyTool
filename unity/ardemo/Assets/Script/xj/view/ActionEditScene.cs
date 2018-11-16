using Game.Event;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Scene;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:ActionEditScene.cs
/// Description:
/// Time:2015/7/22 11:19:43
/// </summary>
public class ActionEditScene : BaseScene
{
    #region 公有属性
    #endregion

    #region 私有属性
    static string sOpenActionsName = null;
    static string sOpenActionsIcon = null;
    static string sOpenActionsId = null;
    //ShowRobotIDUI mShowRobotDjID;
    ActionEditUI mActionEditUi;
    #endregion

    #region 公有函数
    public ActionEditScene()
    {
        mResPath = string.Empty;
        mUIList = new List<BaseUI>();
        //mShowRobotDjID = new ShowRobotIDUI(ShowRobotIDType.ShowModelID);
        mActionEditUi = new ActionEditUI();
        mUIList.Add(mActionEditUi);
        //mUIList.Add(mShowRobotDjID);
    }


    public static void OpenActions(string id)
    {
        sOpenActionsId = id;
        sOpenActionsIcon = null;
        sOpenActionsName = null;
        SceneMgr.EnterScene(SceneType.EditAction, typeof(ActionEditScene));
    }

    public static void CreateActions(string name, string iconId)
    {
        sOpenActionsId = null;
        sOpenActionsName = name;
        sOpenActionsIcon = iconId;
        SceneMgr.EnterScene(SceneType.EditAction, typeof(ActionEditScene));
    }
    public override void UpdateScene()
    {
        base.UpdateScene();
        //mEditOperateUI.Open();
        if (!string.IsNullOrEmpty(sOpenActionsIcon))
        {
            mActionEditUi.CreateActions(sOpenActionsName, sOpenActionsIcon);
            sOpenActionsName = null;
            sOpenActionsIcon = null;
        }
        else if (!string.IsNullOrEmpty(sOpenActionsId))
        {
            mActionEditUi.OpenActions(sOpenActionsId);
            sOpenActionsId = null;
        }
        mActionEditUi.Open();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    #endregion

    #region 私有函数
    
    #endregion
}