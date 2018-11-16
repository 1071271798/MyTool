using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Event;
using Game;

/// <summary>
/// Author:xj
/// FileName:PowerMsg.cs
/// Description:电量显示
/// Time:2016/4/12 9:50:03
/// </summary>
public class PowerMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    GameObject mCharging;
    UISprite mPowerSprite;
    Transform mRootObj;
    TweenPosition mTweenPosition;
    MyTweenAlpha mMyTweenAlpha;
    Vector2 mSpaceSize = new Vector2(30, 20);
    static PowerMsg mInst = null;
    bool isPromptFlag = false;
    long mResidenceTimeIndex = -1;
    #endregion

    #region 公有函数
    public PowerMsg()
    {
        mUIResPath = "Prefab/UI/PowerMsg";
        isSingle = true;
        mInst = this;
    }

    public override void Init()
    {
        base.Init();
        mAddBox = false;
        mCoverAlpha = 0;
        mDepth = 1;
    }

    public override void Release()
    {
        mInst = null;
        base.Release();
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        if (null != mRootObj)
        {
            string[] posAry = new string[2];
            posAry[0] = mRootObj.localPosition.x.ToString("0.000");
            posAry[1] = mRootObj.localPosition.y.ToString("0.000");
            string pos = string.Empty;
            for (int i = 0; i < 2; ++i)
            {
                if (!string.IsNullOrEmpty(pos))
                {
                    pos += PublicFunction.Separator_Comma;
                }
                pos += posAry[i];
            }
            PlayerPrefs.SetString("PowerPos", pos);
        }        
    }


    public static void OpenPower()
    {
        if (null == mInst)
        {
            SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(PowerMsg), null, 0, null, false);
        }
        else
        {
            ShowPower();
        }
    }
    public static void ShowPower()
    {
        if (null != mInst)
        {
            if (!mInst.IsShow)
            {
                GameHelper.PlayMyTweenAlpha(mInst.mMyTweenAlpha, 1f);
                Timer.Add(0.3f, 1, 1, mInst.OnShow);
                /*if (-1 != mResidenceTimeIndex)
                {
                    Timer.Cancel(mResidenceTimeIndex);
                }
                mResidenceTimeIndex = Timer.Add(10.3f, 1, 1, ResidenceTimeDeal);*/
            }
            mInst.SetPowerShow();
        }
    }

    public static void UpdatePower()
    {
        if (null != mInst)
        {
            mInst.SetPowerShow();
        }
    }


    public static void HidePower()
    {
        if (null != mInst)
        {
            if (mInst.IsShow)
            {
                GameHelper.PlayMyTweenAlpha(mInst.mMyTweenAlpha, 0.01f);
                Timer.Add(0.3f, 1, 1, mInst.OnHide);
            }
        }
    }
    #endregion

    #region 其他函数
    protected override void AddEvent()
    {
        base.AddEvent();
        try
        {
            EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
            if (null != mTrans)
            {
                mRootObj = mTrans.Find("obj");
                if (null != mRootObj)
                {
                    Transform charging = mRootObj.Find("charging");
                    if (null != charging)
                    {
                        mCharging = charging.gameObject;
                    }
                    mPowerSprite = GameHelper.FindChildComponent<UISprite>(mRootObj, "power");

                    mTweenPosition = mRootObj.GetComponent<TweenPosition>();
                    mMyTweenAlpha = mRootObj.GetComponent<MyTweenAlpha>();

                    if (PlayerPrefs.HasKey("PowerPos"))
                    {
                        string pos = PlayerPrefs.GetString("PowerPos");
                        string[] posAry = pos.Split(PublicFunction.Separator_Comma);
                        if (posAry.Length >= 2)
                        {
                            mRootObj.localPosition = new Vector3(float.Parse(posAry[0]), float.Parse(posAry[1]));
                        }
                        else
                        {
                            mRootObj.localPosition = UIManager.GetWinPos(mRootObj, UIWidget.Pivot.Left, mSpaceSize.x);
                        }
                    }
                    else
                    {
                        mRootObj.localPosition = UIManager.GetWinPos(mRootObj, UIWidget.Pivot.Left, mSpaceSize.x);
                    }                    

                    SetPowerShow();
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }




    protected override void OnButtonPress(GameObject obj, bool press)
    {
        if (obj.name.Equals("Button"))
        {
            if (press)
            {
                //GameHelper.PlayMyTweenAlpha(mMyTweenAlpha, 1, 0);
            }
            else
            {
                GameHelper.PlayTweenPosition(mTweenPosition, GetObjTargetPos());
                /*if (-1 != mResidenceTimeIndex)
                {
                    Timer.Cancel(mResidenceTimeIndex);
                }
                mResidenceTimeIndex = Timer.Add(10, 1, 1, ResidenceTimeDeal);*/
            }
            
        }
    }
    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
    }


    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 获取按钮目标停靠坐标
    /// </summary>
    /// <returns></returns>
    Vector2 GetObjTargetPos()
    {
        Vector2 pos = new Vector2(-PublicFunction.GetWidth() / 2, -PublicFunction.GetHeight() / 2);
        if (null != mRootObj)
        {
            float minx = Mathf.Min(Mathf.Abs(-PublicFunction.GetWidth() / 2 - mRootObj.localPosition.x), Mathf.Abs(PublicFunction.GetWidth() / 2 - mRootObj.localPosition.x));
            float miny = Mathf.Min(Mathf.Abs(-PublicFunction.GetHeight() / 2 - mRootObj.localPosition.y), Mathf.Abs(PublicFunction.GetHeight() / 2 - mRootObj.localPosition.y));
            Bounds b = NGUIMath.CalculateRelativeWidgetBounds(mRootObj);
            //求出边界值
            float targetMinx = -PublicFunction.GetWidth() / 2 + b.size.x / 2 + mSpaceSize.x;
            float targetMaxx = PublicFunction.GetWidth() / 2 - b.size.x / 2 - mSpaceSize.x;
            float targetMiny = -PublicFunction.GetHeight() / 2 + b.size.y / 2 + mSpaceSize.y;
            float targetMaxy = PublicFunction.GetHeight() / 2 - b.size.y / 2 - mSpaceSize.y;
            if (minx <= miny)
            {//y值不变
                if (mRootObj.localPosition.x > 0)
                {
                    pos.x = targetMaxx;
                }
                else
                {
                    pos.x = targetMinx;
                }
                //须保证y值在屏幕内，并保持间隙
                if (mRootObj.localPosition.y < targetMiny)
                {
                    pos.y = targetMiny;
                }
                else if (mRootObj.localPosition.y > targetMaxy)
                {
                    pos.y = targetMaxy;
                }
                else
                {
                    pos.y = mRootObj.localPosition.y;
                }
            }
            else
            {//x值不变
                if (mRootObj.localPosition.y > 0)
                {
                    pos.y = targetMaxy;
                }
                else
                {
                    pos.y = targetMiny;
                }
                //须保证x值在屏幕内，并保持间隙
                if (mRootObj.localPosition.x < targetMinx)
                {
                    pos.x = targetMinx;
                }
                else if (mRootObj.localPosition.x > targetMaxx)
                {
                    pos.x = targetMaxx;
                }
                else
                {
                    pos.x = mRootObj.localPosition.x;
                }
            }
        }
        return pos;
    }

    void SetPowerShow()
    {
        ReadPowerMsgAck data = PlatformMgr.Instance.PowerData;
        if (null != mCharging)
        {
            if (data.isAdapter)
            {
                if (data.isChargingFinished)
                {
                    mCharging.SetActive(false);
                }
                else
                {
                    mCharging.SetActive(true);
                }
            }
            else
            {
                mCharging.SetActive(false);
            }
        }
        float power = data.percentage / 100.0f;//(data.power - PublicFunction.Robot_Power_Empty) / (PublicFunction.Robot_Power_Max - PublicFunction.Robot_Power_Empty + 0.0f);
        power = Mathf.Clamp01(power);
        if (power < 0.04f)
        {
            power = 0.04f;
        }
        if (null != mPowerSprite)
        {
            mPowerSprite.fillAmount = power;
            if (data.isAdapter)
            {
                mPowerSprite.color = new Color32(50, 192, 0, 255);
            }
            else
            {
                if (power <= 0.1f)
                {
                    mPowerSprite.color = new Color32(226, 2, 0, 255);
                }
                else
                {
                    mPowerSprite.color = Color.white;
                }
            }
            
        }
        if (!data.isAdapter && power <= 0.2f && !isPromptFlag)
        {
            isPromptFlag = true;
        }
    }

    void OnBlueConnectResult(EventArg arg)
    {
        try
        {
            if (null != mInst && !PlatformMgr.Instance.GetBluetoothState())
            {
                HidePower();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    void ResidenceTimeDeal()
    {
        GameHelper.PlayMyTweenAlpha(mMyTweenAlpha, 0.5f);
        mResidenceTimeIndex = -1;
    }


    #endregion
}