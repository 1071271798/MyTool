using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ConnectSpeakerHelpMsg.cs
/// Description:连接蓝牙音响帮助页面
/// Time:2017/4/19 10:48:08
/// </summary>
public class ConnectSpeakerHelpMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    SpeakerInfoData mSpeakerData;
    #endregion

    #region 公有函数
    public ConnectSpeakerHelpMsg(SpeakerInfoData speaker)
    {
        mUIResPath = "Prefab/UI/connectSpeakerHelpMsg";
        isSingle = true;
        mSpeakerData = speaker;
    }

    public static void ShowMsg(SpeakerInfoData speaker)
    {
        object[] args = new object[1];
        args[0] = speaker;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(ConnectSpeakerHelpMsg), args, 1, null, false);
    }

    public override void Init()
    {
        base.Init();
        mCoverAlpha = 1;
        mCoverColor = PublicFunction.Connect_bg_Color;
    }
    #endregion

    #region 其他函数
    protected override void AddEvent()
    {
        base.AddEvent();
        try
        {
            if (null != mTrans)
            {
                Transform top = mTrans.Find("top");
                if (null != top)
                {
                    Transform title = top.Find("title");
                    if (null != title)
                    {
                        SetTitlePosition(title);
                        SetTitleLabel(title);
                    }
                    Transform btnBack = top.Find("btnBack");
                    if (null != btnBack)
                    {
                        SetBackBtnPosition(btnBack);
                    }
                }
                Transform center = mTrans.Find("center");
                if (null != center)
                {
                    SetContentPosition(center);
                    SetContentLabel(center);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            if (obj.name.Equals("btnBack"))
            {
                OnClose();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SetTitlePosition(Transform title)
    {
        GameHelper.SetPosition(title, UIWidget.Pivot.Top, new Vector2(0, PublicFunction.Title_Margin_1));
        Transform subtitle = title.Find("subtitle");
        if (null != subtitle)
        {
            subtitle.localPosition = new Vector3(0, -80);
        }
    }

    void SetTitleLabel(Transform title)
    {
        GameHelper.SetLabelText(title.Find("maintitle"), LauguageTool.GetIns().GetText("连接蓝牙speaker主标题"));
        string subText = LauguageTool.GetIns().GetText("连接蓝牙speaker副标题");
        /*if (null != mSpeakerData)
        {
            subText = string.Format(subText, mSpeakerData.speakerName);
        }
        else
        {
            subText = string.Format(subText, "Jimuspk_XXXX");
        }*/
        GameHelper.SetLabelText(title.Find("subtitle"), subText);
    }

    void SetContentPosition(Transform content)
    {
        Transform left = content.Find("left");
        Transform center = content.Find("center");
        Transform right = content.Find("right");
        if (null != left)
        {
            Vector3 pos = UIManager.GetWinPos(left, UIWidget.Pivot.Left, 90);
            TweenPosition tp = left.GetComponent<TweenPosition>();
            if (null != tp)
            {
                left.localPosition = pos - new Vector3(500, 0);
                GameHelper.PlayTweenPosition(tp, pos, 0.6f);
            }
            else
            {
                left.localPosition = pos;
            }
        }
        if (null != center)
        {
            TweenPosition tp = center.GetComponent<TweenPosition>();
            if (null != tp)
            {
                Vector3 pos = UIManager.GetWinPos(center, UIWidget.Pivot.Bottom);
                center.localPosition = pos - new Vector3(0, 500);
                GameHelper.PlayTweenPosition(tp, Vector3.zero, 0.6f);
            }
            else
            {
                center.localPosition = Vector3.zero;
            }
        }
        if (null != right)
        {
            Vector3 pos = UIManager.GetWinPos(right, UIWidget.Pivot.Right, 90);
            TweenPosition tp = right.GetComponent<TweenPosition>();
            if (null != tp)
            {
                right.localPosition = pos + new Vector3(500, 0);
                GameHelper.PlayTweenPosition(tp, pos, 0.6f);
            }
            else
            {
                right.localPosition = pos;
            }
        }
    }

    void SetContentLabel(Transform content)
    {
        GameHelper.SetLabelText(content.Find("left/Label"), LauguageTool.GetIns().GetText("蓝牙speaker连接步骤1"));
        GameHelper.SetLabelText(content.Find("center/Label"), LauguageTool.GetIns().GetText("蓝牙speaker连接步骤2"));
        string contentText = LauguageTool.GetIns().GetText("蓝牙speaker连接步骤3");
        if (null != mSpeakerData)
        {
            contentText = string.Format(contentText, mSpeakerData.speakerName);
        }
        else
        {
            contentText = string.Format(contentText, "Jimuspk_XXXX");
        }
        GameHelper.SetLabelText(content.Find("right/Label"), contentText);
    }

    void SetBackBtnPosition(Transform btnBack)
    {
        Vector3 pos = UIManager.GetWinPos(btnBack, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
        TweenPosition tp = btnBack.GetComponent<TweenPosition>();
        if (null != tp)
        {
            btnBack.localPosition = pos - new Vector3(300, 0);
            GameHelper.PlayTweenPosition(tp, pos, 0.6f);
        }
        else
        {
            btnBack.localPosition = pos;
        }
    }
    #endregion
}