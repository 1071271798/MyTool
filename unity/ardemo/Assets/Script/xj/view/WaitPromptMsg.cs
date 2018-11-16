using System;
using System.Collections.Generic;
using UnityEngine;
using Game;

/// <summary>
/// Author:xj
/// FileName:PromptMsg.cs
/// Description:等待提示框，可改变文字
/// Time:2015/8/5 10:34:47
/// </summary>
public class WaitPromptMsg : BasePopWin
{
    #region 公有属性
    public ButtonDelegate.OnClick onBtnClick;
    public const string LeftBtnName = "leftBtn";
    public const string RightBtnName = "rightBtn";

    public enum MsgBtnNum
    {
        MsgBtnNum_Single,
        MsgBtnNum_Double
    }
    #endregion

    #region 私有属性
    string mTitleText;
    string mContentText;
    string mLeftBtnText;
    string mRightBtnText;
    MsgBtnNum mMsgBtnNum;
    float mAddHeight;

    UILabel mContentLabel;
    UILabel mLeftBtnLabel;
    UILabel mRightBtnLabel;
    UIButton mLeftBtn;
    UIButton mRightBtn;

    static WaitPromptMsg mInst;
    #endregion

    #region 公有函数
    public WaitPromptMsg(MsgBtnNum btnNum, string content, ButtonDelegate.OnClick onClick)
    {
        mUIResPath = "Prefab/UI/PromptMsg";
        mTitleText = string.Empty;
        mLeftBtnText = LauguageTool.GetIns().GetText("取消");
        mRightBtnText = LauguageTool.GetIns().GetText("确定");
        mContentText = content;
        onBtnClick = onClick;
        mMsgBtnNum = btnNum;
        isSingle = true;
        mInst = this;
    }
    /// <summary>
    /// 普通提示消息
    /// </summary>
    /// <param name="text"></param>
    public static void ShowPrompt(string text)
    {
        object[] args = new object[3];
        args[0] = MsgBtnNum.MsgBtnNum_Single;
        args[1] = text;
        args[2] = null;
        PopWinManager.GetInst().ShowPopWin(typeof(WaitPromptMsg), args);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="btnOnClick"></param>
    public static void ShowPrompt(string text, ButtonDelegate.OnClick btnOnClick)
    {
        object[] args = new object[3];
        args[0] = MsgBtnNum.MsgBtnNum_Single;
        args[1] = text;
        args[2] = btnOnClick;
        PopWinManager.GetInst().ShowPopWin(typeof(WaitPromptMsg), args);
    }
    

    public static void UpdateText(string text)
    {
        if (null != mInst)
        {
            mInst.mContentText = text;
            mInst.SetContentsLabel();
        }
        
    }
    public static void UpdateText(string text, ButtonDelegate.OnClick btnOnClick)
    {
        if (null != mInst)
        {
            mInst.mContentText = text;
            mInst.SetContentsLabel();
            mInst.onBtnClick = btnOnClick;
        }

    }

    public static void UpdateText(string text, string btnText, ButtonDelegate.OnClick btnOnClick)
    {
        if (null != mInst)
        {
            mInst.mContentText = text;
            mInst.mRightBtnText = btnText;
            mInst.SetContentsLabel();
            if (null != mInst.mRightBtnLabel)
            {
                mInst.mRightBtnLabel.text = btnText;
            }
            mInst.onBtnClick = btnOnClick;
        }
    }

    public static void SetRightBtnText(string text)
    {
        if (null != mInst && null != mInst.mRightBtnLabel)
        {
            mInst.mRightBtnText = text;
            mInst.mRightBtnLabel.text = text;
        }
    }

    public static void OnSleepRightBtn()
    {
        if (null != mInst && null != mInst.mRightBtn)
        {
            mInst.mRightBtn.OnSleep();
        }
    }

    public static void OnWakeRightBtn()
    {
        if (null != mInst && null != mInst.mRightBtn)
        {
            mInst.mRightBtn.OnAwake();
        }
    }

    #endregion


    protected override void AddEvent()
    {
        base.AddEvent();
        if (null != mTrans)
        {
            Transform bgbg = mTrans.Find("bgbg");
            if (null != bgbg)
            {
                //UITexture mohu1 = GameHelper.FindChildComponent<UITexture>(bgbg, "mohu1");
                UITexture mohu2 = GameHelper.FindChildComponent<UITexture>(bgbg, "mohu2");
                if (/*null != mohu1 && */null != mohu2)
                {
                    //mohu1.width = PublicFunction.GetExtendWidth();
                    //mohu1.height = PublicFunction.GetExtendHeight();
                    mohu2.width = PublicFunction.GetExtendWidth();
                    mohu2.height = PublicFunction.GetExtendHeight();
#if UNITY_ANDROID
                    mohu2.gameObject.SetActive(false);
#endif
                }
            }

            Transform bg = mTrans.Find("bg");
            Transform title = mTrans.Find("title");
            if (null != title)
            {
                SetTitleLabel(title);
            }
            Transform contents = mTrans.Find("contents");
            if (null != contents)
            {
                mContentLabel = GameHelper.FindChildComponent<UILabel>(contents, "text/Label");
                SetContentsLabel();
            }
            Transform btn = mTrans.Find("btn");
            if (null != btn)
            {
                SetBtn(btn);
            }
        }
    }


    protected override void OnButtonClick(GameObject obj)
    {
        string name = obj.name;
        if (name.Equals(LeftBtnName) || name.Equals(RightBtnName))
        {
            if (null != onBtnClick)
            {
                onBtnClick(obj);
            }
            OnClose();
        }
    }

    protected override void Close()
    {
        base.Close();
        mInst = null;
    }

    public override void Init()
    {
        base.Init();
        SetInitDepth(900);
    }
    #region 私有函数

    
    /// <summary>
    /// 设置标题
    /// </summary>
    /// <param name="obj"></param>
    void SetTitleLabel(Transform obj)
    {
        Transform label = obj.Find("Label");
        if (null != label)
        {
            UILabel lbText = label.GetComponent<UILabel>();
            if (null != lbText)
            {
                lbText.text = mTitleText;
            }
        }
    }

    void SetContentsLabel()
    {
        if (null != mContentLabel)
        {
            //lbtext.color = Color.white;
            mContentLabel.text = mContentText.Replace("\\n", "\n");
        }
    }

    void SetBtn(Transform obj)
    {
        Transform leftBtn = obj.Find(LeftBtnName);

        Transform rightBtn = obj.Find(RightBtnName);

        if (null != rightBtn)
        {
            mRightBtn = rightBtn.GetComponent<UIButton>();
            if (MsgBtnNum.MsgBtnNum_Single == mMsgBtnNum)
            {
                rightBtn.localPosition = Vector3.zero;
            }
            Transform label = rightBtn.Find("Label");
            if (null != label)
            {
                mRightBtnLabel = label.GetComponent<UILabel>();
                if (null != mRightBtnLabel)
                {
                    mRightBtnLabel.text = mRightBtnText;
                }
            }
        }
        if (null != leftBtn)
        {
            mLeftBtn = leftBtn.GetComponent<UIButton>();
            if (MsgBtnNum.MsgBtnNum_Single == mMsgBtnNum)
            {
                leftBtn.gameObject.SetActive(false);
            }
            else
            {
                Transform label = leftBtn.Find("Label");
                if (null != label)
                {
                    mLeftBtnLabel = label.GetComponent<UILabel>();
                    if (null != mLeftBtnLabel)
                    {
                        mLeftBtnLabel.text = mLeftBtnText;
                    }
                }
            }

        }
    }

    #endregion
}