using System.Collections.Generic;
using UnityEngine;

public class ColorInput
{
    List<EColorType> mInputList;
    EColorInputState mInputState;

    public int Count
    {
        get { if (null != mInputList) return mInputList.Count; return 0; }
    }
    float mLastTime;

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
                if (nowTime - mLastTime < 0.1f && ColorUtils.IsTrichromatic(color))
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
                    mInputList.Add(inputColor);
                    mInputState = EColorInputState.Input_Error;
                }
            } else
            {
                mInputList.Add(inputColor);
                if (ColorUtils.IsTrichromatic(inputColor))
                {//目标色为混合色
                    if (ColorUtils.IsBasicColorForMix(inputColor, targetColor))
                    {
                        mInputState = EColorInputState.Input_Wait;
                    }
                    else
                    {
                        mInputState = EColorInputState.Input_Error;
                    }
                }
            }
        }
        mLastTime = nowTime;
        return mInputState;
    }
}
