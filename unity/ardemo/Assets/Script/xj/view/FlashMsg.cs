using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:FlashMsg.cs
/// Description:
/// Time:2015/12/3 15:04:45
/// </summary>
public class FlashMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 私有属性
    UILabel mTitleText;
    UIScrollView mScrollView;
    UIGrid mGrid;
    GameObject mFilePrefab;
    List<string> mActionsList;
    Robot mRobot;
    #endregion

    #region 公有函数
    public FlashMsg(List<string> actList)
    {
        mUIResPath = "Prefab/UI/ConnenctBluetoothMsg";
        mActionsList = actList;
    }

    public static void OpenFlashMsg(List<string> actList)
    {
        object[] args = new object[1];
        args[0] = actList;
        PopWinManager.GetInst().ShowPopWin(typeof(FlashMsg), args);
    }
    public override void UpdateUI()
    {
        base.UpdateUI();
        if (null != mRobot && null != mActionsList)
        {
            List<string> actIdList = mActionsList;
            for (int i = 0, icount = actIdList.Count; i < icount; ++i)
            {
                AddFile(actIdList[i]);
            }
            UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
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
                mTitleText.text = "动作列表";
            }
        }
        mScrollView = mTrans.Find("List").GetComponent<UIScrollView>();
        mGrid = mScrollView.transform.Find("Grid").GetComponent<UIGrid>();
        mFilePrefab = mGrid.transform.Find("Blue").gameObject;
        mFilePrefab.SetActive(false);
        
        mRobot = RobotManager.GetInst().GetCurrentRobot();
    }
    #endregion

    #region 私有函数
    void AddFile(string acts)
    {
        if (null != acts && null != mFilePrefab)
        {
            GameObject obj = GameObject.Instantiate(mFilePrefab) as GameObject;
            obj.name = acts;
            obj.transform.parent = mGrid.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.SetActive(true);
            Transform label = obj.transform.Find("Label");
            if (null != label)
            {
                label.GetComponent<UILabel>().text = acts;
            }            
        }
    }


    protected override void OnButtonClick(GameObject obj)
    {
        try
        {
            string name = obj.name;
            if (name.Equals("Button"))
            {
                OnClose();
            }
            else
            {
                if (null != mActionsList && mActionsList.Contains(name))
                {
                    if (null != mRobot)
                    {
                        mRobot.PlayFlash(name);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
        	
        }
    }
    void OnCloseClick(GameObject obj)
    {
        OnClose();
    }

    #endregion
}