using Game;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 游戏逻辑
/// </summary>
public class RainbowWorldLogic
{
    EGameState mGameState;
    RainbowGameLevel mGameLevel;
    RainbowGameScore mGameScore;
    
    ColorOut mColorOut;
    ColorInput mColorInput;
    int mFailureCount;

    GameShowColorCallBack mShowColorCallBack;
    GameStateCallBack mGameStateCallBack;
    IntParamCallBack  mGameLevelUpCallBack;
    GameScoreChangeCallBack mGameScoreChangeCallBack;
    IntParamCallBack mCountdownCallBack;

    long mShowColorIndex = -1;
    bool isPlaying = false;


    public int GameScore
    {
        get { return mGameScore.Score; }
    }

    public RainbowWorldLogic()
    {
        mGameState = EGameState.Game_Select;
    }
    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="difficulty"></param>
    public void GameStart(EGameDifficulty difficulty)
    {
        SetGameSate(EGameState.Game_Ready);
        mGameLevel = new RainbowGameLevel(difficulty);
        mColorOut = new ColorOut();
        mColorInput = new ColorInput();
        mGameScore = new RainbowGameScore();
        isPlaying = true;
        mFailureCount = 0;
    }
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        SetGameSate(EGameState.Game_Ready);
    }
    /// <summary>
    /// 设置游戏状态改变回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetGameStateCallBack(GameStateCallBack callback)
    {
        mGameStateCallBack = callback;
    }
    /// <summary>
    /// 设置颜色显示回调
    /// </summary>
    /// <param name="showColorCallback"></param>
    public void SetGameShowColorCallBack(GameShowColorCallBack showColorCallback)
    {
        mShowColorCallBack = showColorCallback;
    }
    /// <summary>
    /// 设置倒计时回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetCountdownCallback(IntParamCallBack callback)
    {
        mCountdownCallBack = callback;
    }
    /// <summary>
    /// 设置关卡等级提示回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetGameLevelUpCallback(IntParamCallBack callback)
    {
        mGameLevelUpCallBack = callback;
    }
    /// <summary>
    /// 设置分数改变回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetGameScoreChangeCallBack(GameScoreChangeCallBack callback)
    {
        mGameScoreChangeCallBack = callback;
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
                int addScore = 0;
                if (mColorOut.IsLastColor())
                {//输入完成
                    addScore = mGameScore.AddLastInputScore(mGameLevel.GameDifficulty, targetColor, mGameLevel.Level, mColorOut.Count);
                    if (addScore > 0 && null != mGameScoreChangeCallBack)
                    {
                        mGameScoreChangeCallBack(addScore, mGameScore.Score);
                    }
                    mGameLevel.InputFinished();
                    if (null != mGameLevelUpCallBack)
                    {
                        mGameLevelUpCallBack(mGameLevel.Level);
                    }
                    SetGameSate(EGameState.Game_Wait);
                    Timer.Add(1, 1, 1, StartShowColor);
                } else
                {
                    addScore = mGameScore.AddFrontInputScore(mGameLevel.GameDifficulty, targetColor, mGameLevel.Level);
                    if (addScore > 0 && null != mGameScoreChangeCallBack)
                    {
                        mGameScoreChangeCallBack(addScore, mGameScore.Score);
                    }
                }
                
            } else if (state == EColorInputState.Input_Error)
            {//输入错误，游戏结束
                if (mFailureCount <= RainbowGlobal.GameRemainderCountMax)
                {
                    GameFailure();
                } else
                {
                    GameOver();
                }
                ++mFailureCount;
            } else if (state == EColorInputState.Input_Wait)
            {//需等待下一颜色
            }
            return state;
        }
        return EColorInputState.Input_None;
    }
    /// <summary>
    /// 获取剩余游戏次数
    /// </summary>
    /// <returns></returns>
    public int GetGameRemainderCount()
    {
        return RainbowGlobal.GameRemainderCountMax - mFailureCount;
    }
    /// <summary>
    /// 放弃继续游戏
    /// </summary>
    public void GiveUpGame()
    {
        if (mGameState == EGameState.Game_Failure)
        {
            GameOver();
        }
    }
    /// <summary>
    /// 重新开始本关卡
    /// </summary>
    public void RestartNowLevel()
    {
        StartShowColor();
    }


    public void OnUpdate()
    {
        if (isPlaying)
        {
            switch (mGameState)
            {
                case EGameState.Game_Ready:
                    {
                        Countdown(3, mCountdownCallBack, StartGame);
                        mGameState = EGameState.Game_Countdown;
                    }
                    break;
            }
        }
    }

    public List<EColorType> GetColorOutList()
    {
        if (null != mColorOut)
        {
            return mColorOut.GetOutList();
        }
        return null;
    }
    /// <summary>
    /// 获取当前输出的颜色值
    /// </summary>
    /// <returns></returns>
    public EColorType GetNowColorOut()
    {
        return mColorOut.GetNowColor();
    }


    //////////////////////////////////////////////////////////////////////////

    void SetGameSate(EGameState state)
    {
        mGameState = state;
        if (null != mGameStateCallBack)
        {
            mGameStateCallBack(state);
        }
    }
    
    void StartGame()
    {
        StartShowColor();
        if (null != mGameLevelUpCallBack)
        {
            mGameLevelUpCallBack(mGameLevel.Level);
        }
    }

    void StartShowColor()
    {
        SetGameSate(EGameState.Game_Show_Color);
        mColorOut.CreateColor(mGameLevel.GetNowTrichromatic(), mGameLevel.GetNowMixedColor());
        if (-1 != mShowColorIndex)
        {
            Timer.Cancel(mShowColorIndex);
        }
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
        if (null != mShowColorCallBack)
        {
            mShowColorCallBack(color, lastColorFlag);
        }
        if (lastColorFlag)
        {
            OpenInput();
        }
    }
    /// <summary>
    /// 开启输入
    /// </summary>
    void OpenInput()
    {
        SetGameSate(EGameState.Game_Input);
        mColorOut.ResetIndex();
        mColorInput.Ready();
    }
    /// <summary>
    /// 游戏结束
    /// </summary>
    void GameOver()
    {
        SetGameSate(EGameState.Game_Over);
        //保存游戏分数

        mFailureCount = 0;
        mGameLevel.Reset();
        mGameScore.Reset();
    }
    /// <summary>
    /// 游戏失败
    /// </summary>
    void GameFailure()
    {
        SetGameSate(EGameState.Game_Failure);
    }

    /// <summary>
    /// 显示倒计时
    /// </summary>
    /// <param name="callback"></param>
    void Countdown(int num, IntParamCallBack callback, EventDelegate.Callback countdownFinished)
    {
        Timer.Add(1, 1, num + 1, delegate () {
            if (null != callback)
            {
                callback(num--);
            }
            if (num < 0 && null != countdownFinished)
            {
                countdownFinished();
            }
        });
    }

}

public delegate void GameShowColorCallBack(EColorType color, bool lastColorFlag);
public delegate void GameStateCallBack(EGameState state);
public delegate void IntParamCallBack(int newLevel);
public delegate void GameScoreChangeCallBack(int addScore, int totalScore);
