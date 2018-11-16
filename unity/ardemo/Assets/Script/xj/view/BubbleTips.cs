using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:BubbleTips.cs
/// Description:气泡提示
/// Time:2017/5/11 19:58:34
/// </summary>
public class BubbleTips : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    string mContentText;
    string mBtnText;
    UIWidget.Pivot mPivot;
    Vector2 mTipsPos;
    GameObject mBgObj;
    GameObject mBtnObj;
    bool isLoadOver = false;
    SpeakerInfoData mSpeakerData;
    #endregion

    #region 公有函数

    public static void ShowMsg(string text, string btnText, Transform trans, SpeakerInfoData speakerData, int depth)
    {
        int width = 400;//背景的宽度
        int height = 240;//背景高
        int spaceY = -20;//距离音响的y坐标偏移
        int spaceX = 82;//距离音响的x坐标偏移
        int topHeight = PublicFunction.GetHeight() / 2;
        int bottomHeight = -PublicFunction.GetHeight() / 2;
        int leftWidth = -PublicFunction.GetWidth() / 2;
        int rightWidth = PublicFunction.GetWidth() / 2;
        Vector2 pos = Vector2.zero;
        UIWidget.Pivot pivot = UIWidget.Pivot.Top;
        Camera camera = NGUITools.FindInParents<Camera>(trans);
        if (null != camera)
        {
            Bounds transBs = NGUIMath.CalculateRelativeWidgetBounds(camera.transform, trans);
            pos = new Vector2(transBs.center.x, transBs.center.y);
            if (transBs.center.y + transBs.size.y / 2 + height < topHeight)
            {
                if (transBs.center.x + width < rightWidth)
                {
                    pivot = UIWidget.Pivot.TopRight;
                    pos += new Vector2(spaceX, transBs.size.y / 2 + height / 2 + spaceY);
                }
                else
                {
                    pivot = UIWidget.Pivot.TopLeft;
                    pos += new Vector2(-spaceX, transBs.size.y / 2 + height / 2 + spaceY);
                }

            }
            else
            {
                if (transBs.center.x + width < rightWidth)
                {
                    pivot = UIWidget.Pivot.BottomRight;
                    pos -= new Vector2(-spaceX, transBs.size.y / 2 + height / 2 + spaceY);
                }
                else
                {
                    pivot = UIWidget.Pivot.BottomLeft;
                    pos -= new Vector2(spaceX, transBs.size.y / 2 + height / 2 + spaceY);
                }
            }
        }
        
        object[] args = new object[6];
        args[0] = text;
        args[1] = btnText;
        args[2] = pivot;
        args[3] = pos;
        args[4] = speakerData;
        args[5] = depth;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(BubbleTips), args, 0, null, false);
    }
    



    public BubbleTips(string text, string btnText, UIWidget.Pivot pivot, Vector2 pos, SpeakerInfoData data, int depth)
    {
        mUIResPath = "Prefab/UI/bubbleTips";
        mContentText = text;
        mBtnText = btnText;
        mPivot = pivot;
        mTipsPos = pos;
        mSpeakerData = data;
        isSingle = true;
        SetInitDepth(depth);
    }

    public override void Init()
    {
        base.Init();
        mAddBox = false;
        mCoverAlpha = 0;
    }


    public override void Update()
    {
        base.Update();
        if (isOpen && isLoadOver)
        {
#if UNITY_EDITOR
            MouseCtrl();
#else
            TouchCtrl();
#endif
        }
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
                SetTransPos(mTipsPos);
                SetBg(mPivot);
                SetBtn(mBtnText, mPivot);
                SetContent(mContentText, mPivot);
                OpenAnim();
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
            if (obj.name.Equals("btn"))
            {
                if (null != mSpeakerData)
                {
                    ConnectSpeakerHelpMsg.ShowMsg(mSpeakerData);
                    CloseAnim();
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


    void OpenAnim()
    {
        mTrans.localScale = Vector2.zero;
        TweenScale ts = GameHelper.PlayTweenScale(mTrans, Vector3.one);
        if (null != ts)
        {
            ts.SetOnFinished(delegate () { isLoadOver = true; });
        }
        else
        {
            isLoadOver = true;
        }
    }

    void CloseAnim()
    {
        OnClose();return;
        /*TweenScale ts = GameHelper.PlayTweenScale(mTrans, Vector3.zero);
        if (null != ts)
        {
            ts.SetOnFinished(delegate ()
            {
                OnClose();
            });
        }
        else
        {
            OnClose();
        }*/
    }

    void SetTransPos(Vector2 pos)
    {
        mTrans.localPosition = pos;
    }

    void SetBg(UIWidget.Pivot pivot)
    {
        Transform bg = mTrans.Find("bg");
        if (null != bg)
        {
            mBgObj = bg.gameObject;
            UISprite sp = GameHelper.FindChildComponent<UISprite>(mTrans, "bg");
            if (null != sp)
            {
                switch (pivot)
                {
                    case UIWidget.Pivot.Top:
                        bg.localEulerAngles = Vector3.zero;
                        break;
                    case UIWidget.Pivot.Bottom:
                        bg.localEulerAngles = new Vector3(0, 0, 180);
                        break;
                    case UIWidget.Pivot.TopLeft:
                        bg.localEulerAngles = new Vector3(0, 180);
                        break;
                    case UIWidget.Pivot.TopRight:
                        bg.localEulerAngles = Vector3.zero;
                        break;
                    case UIWidget.Pivot.BottomRight:
                        bg.localEulerAngles = new Vector3(180, 0, 0);
                        break;
                    case UIWidget.Pivot.BottomLeft:
                        bg.localEulerAngles = new Vector3(0, 0, 180);
                        break;
                }
            }
        }
    }

    void SetContent(string text, UIWidget.Pivot pivot)
    {
        Transform contents = mTrans.Find("contents");
        if (null != contents)
        {
            GameHelper.SetLabelText(contents.Find("text"), text);
            if (pivot == UIWidget.Pivot.Bottom || pivot == UIWidget.Pivot.BottomLeft || pivot == UIWidget.Pivot.BottomRight)
            {
                contents.localPosition -= new Vector3(0, 20);
            }
        }
    }

    void SetBtn(string btnText, UIWidget.Pivot pivot)
    {
        Transform btn = mTrans.Find("btn");
        if (null != btn)
        {
            mBtnObj = btn.gameObject;
            if (string.IsNullOrEmpty(btnText))
            {
                btn.gameObject.SetActive(false);
            }
            else
            {
                GameHelper.SetLabelText(btn.Find("Label"), btnText);
                if (pivot == UIWidget.Pivot.Bottom || pivot == UIWidget.Pivot.BottomLeft || pivot == UIWidget.Pivot.BottomRight)
                {
                    btn.localPosition -= new Vector3(0, 20);
                }
            }
        }
    }


    void MouseCtrl()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("mouse up");
            if (UICamera.hoveredObject == null)
            {
                Debug.Log("hoverd object null");
            }
            else
            {
                Debug.Log("hoverd object name = " + UICamera.hoveredObject.name);
            }
            if (UICamera.hoveredObject == null || UICamera.hoveredObject != mBgObj && UICamera.hoveredObject != mBtnObj)
            {
                CloseAnim();
            }
        }
    }

    void TouchCtrl()
    {
        for (int i = 0, icount = Input.touchCount; i < icount; ++i)
        {
            Touch tmpTouch = Input.GetTouch(i);
            TouchPhase phase = tmpTouch.phase;
            if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
            {
                if (UICamera.hoveredObject == null || UICamera.hoveredObject != mBgObj && UICamera.hoveredObject != mBtnObj)
                {
                    CloseAnim();
                    return;
                }
            }
        }
    }
    #endregion
}