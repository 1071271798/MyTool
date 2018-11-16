using System;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;
using Game;
using Game.Event;

/// <summary>
/// Author:xj
/// FileName:OpenActionsMsg.cs
/// Description:
/// Time:2015/7/22 13:11:21
/// </summary>
public class OpenActionsMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 私有属性
    UILabel mTitleText;
    UIScrollView mScrollView;
    UIGrid mGrid;
    GameObject mFilePrefab;
    UIClickBody mCloseBtn = null;
    Dictionary<int, ActionSequence> mActionsDict;
    Robot mRobot;
    #endregion

    #region 公有函数
    public OpenActionsMsg()
    {
        mUIResPath = "Prefab/UI/ConnenctBluetoothMsg";
    }
    public override void UpdateUI()
    {
        base.UpdateUI();
        if (null != mRobot)
        {
            List<string> actIdList = mRobot.GetActionsIdList();
            for (int i = 0, icount = actIdList.Count; i < icount; ++i)
            {
                ActionSequence acts = mRobot.GetActionsForID(actIdList[i]);
                if (null != acts)
                {
                    AddFile(acts);
                }
            }
            mGrid.repositionNow = true;
        }
    }

    #endregion

    #region 保护函数
    protected override void AddEvent()
    {
        base.AddEvent();
        if (null == mTrans)
        {
            return;
        }
        Transform title = mTrans.Find("bluetoothLabel");
        if (null != title)
        {
            mTitleText = title.GetComponent<UILabel>();
            if (null != mTitleText)
            {
                mTitleText.text = string.Empty;
            }
        }
        mScrollView = mTrans.Find("List").GetComponent<UIScrollView>();
        mGrid = mScrollView.transform.Find("Grid").GetComponent<UIGrid>();
        mFilePrefab = mGrid.transform.Find("Blue").gameObject;
        mFilePrefab.SetActive(false);
        mCloseBtn = new UIClickBody(mTrans.Find("Button").gameObject);
        mCloseBtn.Pivot = UIWidget.Pivot.Center;
        mCloseBtn.Listener.onClick += OnCloseClick;
        mActionsDict = new Dictionary<int, ActionSequence>();
        mRobot = RobotManager.GetInst().GetCurrentRobot();
    }
    #endregion

    #region 私有函数
    void AddFile(ActionSequence acts)
    {
        if (null != acts && null != mFilePrefab)
        {
            GameObject obj = GameObject.Instantiate(mFilePrefab) as GameObject;
            obj.name = mActionsDict.Count.ToString();
            mActionsDict.Add(mActionsDict.Count, acts);
            obj.transform.parent = mGrid.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.SetActive(true);
            UIEventListener lis = obj.AddComponent<UIEventListener>();
            lis.onClick += OnFileClick;
            Transform label = obj.transform.Find("Label");
            if (null != label)
            {
                label.GetComponent<UILabel>().text = acts.Name;
            }            
        }
    }

    void OnCloseClick(GameObject obj)
    {
        OnClose();
    }

    void OnFileClick(GameObject obj)
    {
        if (PublicFunction.IsInteger(obj.name))
        {
            int key = int.Parse(obj.name);
            if (null != mActionsDict && mActionsDict.ContainsKey(key))
            {
                EventMgr.Inst.Fire(EventID.UI_Open_Actions, new EventArg(mActionsDict[key]));
                OnClose();
            }
        }
    }
    #endregion
}