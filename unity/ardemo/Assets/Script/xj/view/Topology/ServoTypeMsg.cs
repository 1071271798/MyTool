using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ServoTypeMsg.cs
/// Description:设置舵机模式
/// Time:2016/8/30 14:05:43
/// </summary>
public class ServoTypeMsg : BasePopWin
{
    #region 公有属性
    public delegate void OnServoResult(ServoModel modelType);
    #endregion

    #region 其他属性
    ServoModel mServoType = ServoModel.Servo_Model_Angle;
    UISprite mAngleSelectIcon;
    UISprite mTurnSelectIcon;
    UIButton mAngleBtn;
    UIButton mTurnBtn;
    OnServoResult mResultDelegate;
    #endregion

    #region 公有函数
    public ServoTypeMsg(ServoModel modelType, OnServoResult resultDelegate)
    {
        mUIResPath = "Prefab/UI/ServoModelMsg";
        mResultDelegate = resultDelegate;
        mServoType = modelType;
    }

    public static void ShowMsg(ServoModel modelType, OnServoResult resultDelegate)
    {
        object[] args = new object[2];
        args[0] = modelType;
        args[1] = resultDelegate;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(ServoTypeMsg), args);
    }

    public override void Init()
    {
        base.Init();
        mSide = UIAnchor.Side.Bottom;
    }

    public override void Release()
    {
        base.Release();
        if (null != mResultDelegate)
        {
            mResultDelegate(mServoType);
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
                Transform bg = mTrans.Find("bg");
                if (null != bg)
                {
                    UISprite bg1 = GameHelper.FindChildComponent<UISprite>(bg, "bg1");
                    if (null != bg1)
                    {
                        bg1.width = PublicFunction.GetExtendWidth();
                    }
                    UISprite bg2 = GameHelper.FindChildComponent<UISprite>(bg, "bg2");
                    if (null != bg2)
                    {
                        bg2.width = PublicFunction.GetExtendWidth();
                    }
                }
                Transform angleBtn = mTrans.Find("angleBtn");
                if (null != angleBtn)
                {
                    mAngleBtn = angleBtn.GetComponent<UIButton>();
                    Vector3 pos = angleBtn.localPosition;
                    pos.x = UIManager.GetWinPos(angleBtn, UIWidget.Pivot.Left, 40).x;
                    angleBtn.localPosition = pos;
                    UILabel lb = GameHelper.FindChildComponent<UILabel>(angleBtn, "Label");
                    if (null != lb)
                    {
                        lb.text = LauguageTool.GetIns().GetText("角模式");
                    }
                    mAngleSelectIcon = GameHelper.FindChildComponent<UISprite>(angleBtn, "Background");
                }

                Transform turnBtn = mTrans.Find("turnBtn");
                if (null != turnBtn)
                {
                    mTurnBtn = turnBtn.GetComponent<UIButton>();
                    Vector3 pos = turnBtn.localPosition;
                    pos.x = UIManager.GetWinPos(turnBtn, UIWidget.Pivot.Right, 40).x;
                    turnBtn.localPosition = pos;
                    UILabel lb = GameHelper.FindChildComponent<UILabel>(turnBtn, "Label");
                    if (null != lb)
                    {
                        lb.text = LauguageTool.GetIns().GetText("轮模式");
                    }
                    mTurnSelectIcon = GameHelper.FindChildComponent<UISprite>(turnBtn, "Background");
                }
                SetServoMode(mServoType);
                UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
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
            string name = obj.name;
            if (name.Equals("angleBtn"))
            {
                if (mServoType != ServoModel.Servo_Model_Angle)
                {
                    SetServoMode(ServoModel.Servo_Model_Angle);
                }
                OnClose();
            }
            else if (name.Equals("turnBtn"))
            {
                if (mServoType != ServoModel.Servo_Model_Turn)
                {
                    PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("轮模式切换提示"), PopTurnTitleOnClick);
                    
                }
            }
            else
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

    void PopTurnTitleOnClick(GameObject obj)
    {
        string name = obj.name;
        if (name.Equals(PromptMsg.LeftBtnName))
        {

        }
        else if (name.Equals(PromptMsg.RightBtnName))
        {
            SetServoMode(ServoModel.Servo_Model_Turn);
            OnClose();
        }
    }

    void SetServoMode(ServoModel modelType)
    {
        string angleSprite = "notselected";
        string turnSprite = "notselected";
        if (modelType == ServoModel.Servo_Model_Angle)
        {
            angleSprite = "selected";
        }
        else
        {
            turnSprite = "selected";
        }
        if (null != mAngleSelectIcon)
        {
            mAngleSelectIcon.spriteName = angleSprite;
        }
        if (null != mAngleBtn)
        {
            mAngleBtn.normalSprite = angleSprite;
        }
        if (null != mTurnSelectIcon)
        {
            mTurnSelectIcon.spriteName = turnSprite;
        }
        if (null != mTurnBtn)
        {
            mTurnBtn.normalSprite = turnSprite;
        }
        mServoType = modelType;
    }
    #endregion
}