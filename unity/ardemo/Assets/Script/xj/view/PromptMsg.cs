using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Platform;
using Game.Event;

/// <summary>
/// Author:xj
/// FileName:PromptMsg.cs
/// Description:普通提示弹框，支持单键和双键
/// Time:2015/8/5 10:34:47
/// </summary>
public class PromptMsg : BasePopWin
{
    #region 公有属性
    public const string LeftBtnName = "leftBtn";
    public const string RightBtnName = "rightBtn";

    public enum MsgBtnNum
    {
        MsgBtnNum_Single,
        MsgBtnNum_Double
    }
    #endregion

    #region 私有属性
    static Dictionary<string, PromptMsg> sShowMsg = null;
    string mTitleText;
    string mContentText;
    string mLeftBtnText;
    string mRightBtnText;

    UILabel mLeftBtnLabel;
    UILabel mRightBtnLabel;
    MsgBtnNum mMsgBtnNum;
    float mAddHeight;

    ButtonDelegate.OnClick onBtnClick;
    ButtonDelegate.OnClick onHelpBtnClick;
    #endregion

    #region 公有函数
    public PromptMsg(MsgBtnNum btnNum, string content, ButtonDelegate.OnClick onClick, ButtonDelegate.OnClick helpClick)
    {
        mUIResPath = "Prefab/UI/PromptMsg";
        if (null == sShowMsg)
        {
            sShowMsg = new Dictionary<string, PromptMsg>();
        }
        sShowMsg[content] = this;
        mTitleText = string.Empty;
        mLeftBtnText = LauguageTool.GetIns().GetText("取消");
        mRightBtnText = LauguageTool.GetIns().GetText("确定");
        mContentText = content;
        onBtnClick = onClick;
        onHelpBtnClick = helpClick;
        mMsgBtnNum = btnNum;
    }
    /// <summary>
    /// 普通提示消息
    /// </summary>
    /// <param name="text"></param>
    public static void ShowSinglePrompt(string text, PopWinManager.OpenWinFinished onFinished = null) 
    {
        if (IsExist(text))
        {
            sShowMsg[text].onBtnClick = null;
            sShowMsg[text].onHelpBtnClick = null;
            sShowMsg[text].SetHelpBtnActive(false);
            if (null != onFinished)
            {
                onFinished(sShowMsg[text]);
            }
            return;
        }
        object[] args = new object[4];
        args[0] = MsgBtnNum.MsgBtnNum_Single;
        args[1] = text;
        args[2] = null;
        args[3] = null;
        PopWinManager.GetInst().ShowPopWin(typeof(PromptMsg), args, PublicFunction.PopWin_Alpha, onFinished);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="btnOnClick"></param>
    public static void ShowSinglePrompt(string text, ButtonDelegate.OnClick btnOnClick, ButtonDelegate.OnClick helpClick = null, PopWinManager.OpenWinFinished onFinished = null)
    {
        if (IsExist(text))
        {
            sShowMsg[text].onBtnClick = btnOnClick;
            sShowMsg[text].onHelpBtnClick = helpClick;
            if (null != helpClick)
            {
                sShowMsg[text].SetHelpBtnActive(true);
            }
            else
            {
                sShowMsg[text].SetHelpBtnActive(false);
            }
            if (null != onFinished)
            {
                onFinished(sShowMsg[text]);
            }
            return;
        }
        object[] args = new object[4];
        args[0] = MsgBtnNum.MsgBtnNum_Single;
        args[1] = text;
        args[2] = btnOnClick;
        args[3] = helpClick;
        PopWinManager.GetInst().ShowPopWin(typeof(PromptMsg), args, PublicFunction.PopWin_Alpha, onFinished);
    }
    /// <summary>
    /// 弹出两个按钮的弹框
    /// </summary>
    /// <param name="text"></param>
    /// <param name="btnOnClick"></param>
    public static void ShowDoublePrompt(string text, ButtonDelegate.OnClick btnOnClick, ButtonDelegate.OnClick helpClick = null, PopWinManager.OpenWinFinished onFinished = null)
    {
        if (IsExist(text))
        {
            sShowMsg[text].onBtnClick = btnOnClick;
            sShowMsg[text].onHelpBtnClick = helpClick;
            if (null != helpClick)
            {
                sShowMsg[text].SetHelpBtnActive(true);
            }
            else
            {
                sShowMsg[text].SetHelpBtnActive(false);
            }
            if (null != onFinished)
            {
                onFinished(sShowMsg[text]);
            }
            return;
        }
        object[] args = new object[4];
        args[0] = MsgBtnNum.MsgBtnNum_Double;
        args[1] = text;
        args[2] = btnOnClick;
        args[3] = helpClick;
        PopWinManager.GetInst().ShowPopWin(typeof(PromptMsg), args, PublicFunction.PopWin_Alpha, onFinished);
    }

    public static void CloseAll()
    {
        if (null != sShowMsg)
        {
            List<PromptMsg> closeList = new List<PromptMsg>();
            foreach (var kvp in sShowMsg)
            {
                closeList.Add(kvp.Value);
            }
            for (int i = 0, imax = closeList.Count; i < imax; ++i)
            {
                closeList[i].OnClose();
            }
            sShowMsg = null;
        }
    }





    public void SetLeftBtnText(string text)
    {
        mLeftBtnText = text;
        if (null != mLeftBtnLabel)
        {
            mLeftBtnLabel.text = text;
        }
    }

    public void SetRightBtnText(string text)
    {
        mRightBtnText = text;
        if (null != mRightBtnLabel)
        {
            mRightBtnLabel.text = text;
        }
    }

    public void SetContentsPivot(UIWidget.Pivot pivot)
    {
        UILabel lb = GameHelper.FindChildComponent<UILabel>(mTrans, "contents/text/Label");
        if (null != lb)
        {
            lb.pivot = pivot;
        }
    }

    public override void Init()
    {
        base.Init();
        //mCoverColor = new Color(122f / 255f, 122f / 255f, 122f / 255, 200f / 255f);
    }

    #endregion


    protected override void AddEvent()
    {
        base.AddEvent();
        if (null != mTrans)
        {
            EventMgr.Inst.Fire(EventID.PopPromptMsg);
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
                SetContentsLabel(contents);
            }
            Transform btn = mTrans.Find("btn");
            if (null != btn)
            {
                SetBtn(btn);
            }
            if (null != onHelpBtnClick)
            {
                SetHelpBtnActive(true);
            }
            else
            {
                SetHelpBtnActive(false);
            }
        }
    }


    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        string name = obj.name;
        if (name.Equals(LeftBtnName) || name.Equals(RightBtnName))
        {
            try
            {
                if (null != onBtnClick)
                {
                    onBtnClick(obj);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            OnClose();
        }
        else if (name.Equals("btnHelp"))
        {
            if (null != onHelpBtnClick)
            {
                NetWaitMsg.ShowWait(1);
                try
                {
                    onHelpBtnClick(obj);
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
                
            }
        }
    }

    protected override void Close()
    {
        base.Close();
        if (null != sShowMsg)
        {
            sShowMsg.Remove(mContentText);
            if (sShowMsg.Count == 0)
            {
                sShowMsg = null;
                EventMgr.Inst.Fire(EventID.ClosePromptMsg);
            }
        }
        else
        {
            EventMgr.Inst.Fire(EventID.ClosePromptMsg);
        }
    }
    #region 私有函数

    void SetHelpBtnActive(bool activeFlag)
    {
        if (null != mTrans)
        {
            Transform btnHelp = mTrans.Find("btnHelp");
            if (null != btnHelp)
            {
                btnHelp.gameObject.SetActive(activeFlag);
            }
        }
    }

    static bool IsExist(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }
        if (null != sShowMsg && sShowMsg.ContainsKey(text))
        {
            return true;
        }
        return false;
    }

    void SetTitleText(string text)
    {
        mTitleText = text;
    }

    void SetContentsText(string text)
    {
        mContentText = text;
    }

    void SetBtnNum(MsgBtnNum num)
    {
        mMsgBtnNum = num;
    }

    


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

    void SetContentsLabel(Transform obj)
    {
        Transform bg = obj.Find("bg");
        Transform text = obj.Find("text");
        if (null != text)
        {
            Transform label = text.Find("Label");
            if (null != label)
            {
                UILabel lbtext = label.GetComponent<UILabel>();
                if (null != lbtext)
                {
                    //lbtext.color = Color.white;
                    lbtext.text = mContentText.Replace("\\n", "\n");
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "PromptMsg Show Text :" + lbtext.text);
                }
            }
        }
    }

    void SetBtn(Transform obj)
    {
        Transform leftBtn = obj.Find(LeftBtnName);

        Transform rightBtn = obj.Find(RightBtnName);
        
        if (null != rightBtn)
        {
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