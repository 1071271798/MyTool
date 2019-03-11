using Game;
using System.Collections.Generic;
/// <summary>
/// 彩虹世界引导
/// </summary>
public class RainbowLogicGuide
{
    EGameState mGameState;
    ColorOut mColorOut;
    ColorInput mColorInput;
    GameShowColorCallBack mShowColorCallback;
    GameStateCallBack mGameStateCallback;
    long mShowColorIndex = -1;

    public RainbowLogicGuide()
    {

    }

    public void GameStart(GameStateCallBack callback, GameShowColorCallBack showColorCallback)
    {
        mGameStateCallback = callback;
        mShowColorCallback = showColorCallback;
        mColorInput = new ColorInput(ClickOutTimeCallback);
        StartShowColor();
    }

    /// <summary>
    /// 设置输出颜色
    /// </summary>
    /// <param name="list"></param>
    public void SetColorOut(List<EColorType> list)
    {
        mColorOut = new ColorOut(list);
    }


    /// <summary>
    /// 开启输入
    /// </summary>
    public void OpenInput()
    {
        SetGameSate(EGameState.Game_Open_Input, mColorOut.Count);
        mColorOut.ResetIndex();
        mColorInput.Ready();
        SetGameSate(EGameState.Game_Input, mColorOut.GetNowColor());
    }
    /// <summary>
    /// 点击对应颜色按钮
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public EColorInputState ClickColor(EInputDirection direction, EColorType color)
    {
        if (mGameState == EGameState.Game_Input)
        {
            EColorType targetColor = mColorOut.GetNowColor();
            EColorInputState state;
            if (direction == EInputDirection.Input_Left)
            {
                state = mColorInput.LeftInput(color, targetColor);
            }
            else
            {
                state = mColorInput.RightInput(color, targetColor);
            }
            if (state == EColorInputState.Input_Success)
            {//输入正确
                mColorOut.MoveNextColor();
                if (mColorOut.IsLastColor())
                {//输入完成
                    SetGameSate(EGameState.Game_Input_Finished);
                }
                else
                {
                    SetGameSate(EGameState.Game_Input, mColorOut.GetNowColor());
                }
            }
            else if (state == EColorInputState.Input_Error)
            {//输入错误，重新开始
                ClickInputError();
            }
            else if (state == EColorInputState.Input_Wait)
            {//需等待下一颜色
            }
            return state;
        }
        return EColorInputState.Input_None;
    }

    /// <summary>
    /// 获取当前输出的颜色值
    /// </summary>
    /// <returns></returns>
    public EColorType GetNowColorOut()
    {
        return mColorOut.GetNowColor();
    }

    /// <summary>
    /// 输入超时
    /// </summary>
    void ClickOutTimeCallback()
    {
        ClickInputError();
    }

    /// <summary>
    /// 点击错误
    /// </summary>
    void ClickInputError()
    {
        OpenInput();
    }

    void SetGameSate(EGameState state, params object[] args)
    {
        mGameState = state;
        if (null != mGameStateCallback)
        {
            mGameStateCallback(state, args);
        }
    }

    void StartShowColor()
    {
        SetGameSate(EGameState.Game_Show_Color);
        mColorOut.ResetIndex();
        CancelShowColor();
        ShowColor();
    }

    void ShowColor()
    {
        mShowColorIndex = -1;
        EColorType color = mColorOut.PopColor();
        bool lastColorFlag = color == EColorType.None ? true : false;
        if (!lastColorFlag)
        {
            mShowColorIndex = Timer.Add(3, 1, 1, ShowColor);
        }
        if (null != mShowColorCallback)
        {
            mShowColorCallback(color, lastColorFlag);
        }
        if (lastColorFlag)
        {
            SetGameSate(EGameState.Game_Show_Color_Finished);
        }
    }

    

    void CancelShowColor()
    {
        if (-1 != mShowColorIndex)
        {
            Timer.Cancel(mShowColorIndex);
            mShowColorIndex = -1;
        }
    }
}