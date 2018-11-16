
using Game.Platform;
using UnityEngine;

public class PowerShow
{
    GameObject mPowerGameObject;
    UISprite mPowerBg;
    UISprite[] mPowerIcon;
    PowerState mPowerState = PowerState.None;
    int mPowerNum = -1;
    float mChargeTime;

    public PowerShow(Transform powerTrans)
    {
        mPowerGameObject = powerTrans.gameObject;
        mPowerBg = GameHelper.FindChildComponent<UISprite>(powerTrans, "powerbg");
        mPowerIcon = new UISprite[3];
        for (int i = 0; i < 3; ++i)
        {
            mPowerIcon[i] = GameHelper.FindChildComponent<UISprite>(powerTrans, "power" + (i + 1));
        }
    }

    public void ShowPower()
    {
        if (null != mPowerGameObject)
        {
            mPowerGameObject.SetActive(true);
        }
    }

    public void HidePower()
    {
        if (null != mPowerGameObject)
        {
            mPowerGameObject.SetActive(false);
        }
    }

    public void CharingUpdate()
    {
        if (mPowerState == PowerState.Power_Charging)
        {
            mChargeTime += Time.deltaTime;
            if (mChargeTime >= 0.25f)
            {
                mChargeTime = 0;
                mPowerNum += 1;
                if (mPowerNum >= 4)
                {
                    mPowerNum = 0;
                }
                SetPowerNum(mPowerNum);
            }
        }
    }

    public void SetPowerState()
    {
        PowerState state = PowerState.None;
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            if (PlatformMgr.Instance.PowerData.isAdapter)
            {
                if (PlatformMgr.Instance.PowerData.isChargingFinished)
                {
                    state = PowerState.Power_Charge_Finished;
                }
                else
                {
                    state = PowerState.Power_Charging;
                }
            }
            else if (PlatformMgr.Instance.PowerData.IsPowerLow())
            {
                state = PowerState.Power_Low;
            }
            else
            {
                state = PowerState.Power_Normal;
            }
        }
        else
        {
            mPowerNum = -1;
        }
        if (state != mPowerState)
        {
            PowerStateChange(state);
        }
        if (state != PowerState.Power_Charging && state != PowerState.None)
        {
            SetPowerPercentage();
        }
    }

    void SetPowerPercentage()
    {
        int num = 1;
        if (PlatformMgr.Instance.PowerData.percentage <= 20)
        {
            num = 0;
        }
        else if (PlatformMgr.Instance.PowerData.percentage < 50)
        {
            num = 1;
        }
        else if (PlatformMgr.Instance.PowerData.percentage < 75)
        {
            num = 2;
        }
        else
        {
            num = 3;
        }
        if (mPowerNum != num)
        {
            SetPowerNum(num);
        }

    }

    void SetPowerNum(int num)
    {
        mPowerNum = num;
        if (null != mPowerIcon)
        {
            for (int i = 0, imax = mPowerIcon.Length; i < imax; ++i)
            {
                if (i < num)
                {
                    mPowerIcon[i].alpha = 1;
                }
                else
                {
                    mPowerIcon[i].alpha = 0;
                }
            }
        }
    }

    void PowerStateChange(PowerState state)
    {
        mPowerState = state;
        Color32 color;
        switch (mPowerState)
        {
            case PowerState.Power_Normal:
                color = new Color32(0, 195, 200, 255);
                break;
            case PowerState.Power_Low:
                color = new Color32(254, 73, 121, 255);
                break;
            case PowerState.Power_Charging:
                color = new Color32(160, 221, 93, 255);
                mChargeTime = 0;
                break;
            case PowerState.Power_Charge_Finished:
                color = new Color32(160, 221, 93, 255);
                break;
            default:
                color = new Color32(0, 195, 200, 255);
                break;
        }
        if (mPowerState == PowerState.None)
        {
            HidePower();
        }
        else
        {
            ShowPower();
            if (null != mPowerBg)
            {
                mPowerBg.color = color;
            }
            if (null != mPowerIcon)
            {
                for (int i = 0, imax = mPowerIcon.Length; i < imax; ++i)
                {
                    if (null != mPowerIcon[i])
                    {
                        mPowerIcon[i].color = color;
                    }
                }
            }
        }
    }

    public enum PowerState : byte
    {
        None = 0,
        Power_Normal,
        Power_Low,
        Power_Charging,
        Power_Charge_Finished
    }
}
