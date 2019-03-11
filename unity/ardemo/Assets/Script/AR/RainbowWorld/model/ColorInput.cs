using System.Collections.Generic;
using UnityEngine;
using Game;

public class ColorInput
{
    List<EColorType> mInputList;
    EColorInputState mInputState;
    long mWaitNextIndex = -1;
    float Wait_Out_Time = 0.1f;
    EventDelegate.Callback mInputOutTimeCallback;

    public int Count
    {
        get { if (null != mInputList) return mInputList.Count; return 0; }
    }
    float mLastTime;

    public ColorInput(EventDelegate.Callback inputOutTimeCallback)
    {
        this.mInputOutTimeCallback = inputOutTimeCallback;
    }

    public void Ready()
    {
        if (null == mInputList)
        {
            mInputList = new List<EColorType>();
        } else
        {
            mInputList.Clear();
        }
        mLastTime = 0;
        mInputState = EColorInputState.Input_None;
    }

    public EColorInputState LeftInput(EColorType color, EColorType targetColor)
    {
        return AddInput(color, Time.time, targetColor);
    }

    public EColorInputState RightInput(EColorType color, EColorType targetColor)
    {
        return AddInput(color, Time.time, targetColor);
    }

    EColorInputState AddInput(EColorType color, float nowTime, EColorType targetColor)
    {
        EColorType inputColor = color;
        if (ColorUtils.IsTrichromatic(targetColor))
        {//目标颜色为三基色
            mInputList.Add(inputColor);
            if (inputColor == targetColor)
            {
                //输入成功
                mInputState = EColorInputState.Input_Success;
            } else
            {
                //输入错误
                mInputState = EColorInputState.Input_Error;
            }
        } else
        {//目标颜色为混合色
            if (mInputState == EColorInputState.Input_Wait)
            {//需要合并颜色
                CancelWaitNext();
                if (nowTime - mLastTime <= Wait_Out_Time && ColorUtils.IsTrichromatic(color))
                {//与上次输入合并
                    inputColor = ColorUtils.MixColor(color, mInputList[mInputList.Count - 1]);
                    mInputList[mInputList.Count - 1] = inputColor;
                    if (inputColor == targetColor)
                    {
                        //输入成功
                        mInputState = EColorInputState.Input_Success;
                    }
                    else
                    {
                        //输入错误
                        mInputState = EColorInputState.Input_Error;
                    }
                } else
                {//输入错误
                    mInputState = EColorInputState.Input_Error;
                }
            } else
            {
                mInputList.Add(inputColor);
                if (ColorUtils.IsTrichromatic(inputColor))
                {
                    if (ColorUtils.IsBasicColorForMix(inputColor, targetColor))
                    {
                        mInputState = EColorInputState.Input_Wait;
                        StartNextClickCheck();
                    }
                    else
                    {
                        mInputState = EColorInputState.Input_Error;
                    }
                } else if (inputColor != targetColor)
                {
                    mInputState = EColorInputState.Input_Error;
                } else
                {//输入成功
                    mInputState = EColorInputState.Input_Success;
                }
            }
        }
        mLastTime = nowTime;
        return mInputState;
    }
    /// <summary>
    /// 获取输入成功的列表
    /// </summary>
    /// <returns></returns>
    public List<EColorType> GetInputSuccessList()
    {
        if (mInputList == null)
        {
            return null;
        }
        List<EColorType> list = new List<EColorType>();
        int count = 0;
        if (mInputState == EColorInputState.Input_Success)
        {
            count = mInputList.Count;
        } else
        {
            count = mInputList.Count - 1;
        }
        for (int i = 0; i < count; ++i)
        {
            list.Add(mInputList[i]);
        }
        return list;
    }

    void StartNextClickCheck()
    {
        CancelWaitNext();
        mWaitNextIndex = Timer.Add(Wait_Out_Time, 1, 1, InputOutTimeCallback);
    }

    void InputOutTimeCallback()
    {
        mWaitNextIndex = -1;
        mInputState = EColorInputState.Input_Error;
        if (null != mInputOutTimeCallback)
        {
            mInputOutTimeCallback();
        }
    }

    void CancelWaitNext()
    {
        if (-1 != mWaitNextIndex)
        {
            Timer.Cancel(mWaitNextIndex);
            mWaitNextIndex = -1;
        }
    }
}
