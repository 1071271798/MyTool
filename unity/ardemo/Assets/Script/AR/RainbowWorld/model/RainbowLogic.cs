using Game;
using System.Collections.Generic;
using UnityEngine;
using Game.Platform;
/// <summary>
/// 游戏逻辑
/// </summary>
public class RainbowLogic
{
    protected EGameState mGameState;
    protected RainbowGameLevel mGameLevel;
    protected RainbowGameScore mGameScore;
    protected RainbowPlayerData mPlayerData;
    protected RainbowReward mGameReward;

    protected RainbowAchieveData mNeedShowAchieve;
    protected bool mNeedUnlockHardFlag;

    protected ColorOut mColorOut;
    protected ColorInput mColorInput;

    protected int mFailureCount;
    protected int mAgainCount;

    protected GameShowColorCallBack mShowColorCallback;
    protected GameStateCallBack mGameStateCallback;
    protected IntParamCallBack mGameLevelUpCallback;
    protected GameScoreChangeCallBack mGameScoreChangeCallback;
    protected IntParamCallBack mCountdownCallback;
    protected IntParamCallBack mAgainCountChangeCallback;

    long mShowColorIndex = -1;
    long mStartNextIndex = -1;
    long mCountdownIndex = -1;
    protected bool isPlaying = false;


    public int GameScore
    {
        get { return mGameScore.Score; }
    }

    public RainbowLogic()
    {
        mGameState = EGameState.Game_Select;
    }
    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="difficulty"></param>
    public virtual void GameStart(EGameDifficulty difficulty)
    {
        SetGameSate(EGameState.Game_Ready);
        mGameLevel = new RainbowGameLevel(difficulty);
        mColorOut = new ColorOut();
        mColorInput = new ColorInput(ClickOutTimeCallback);
        mGameScore = new RainbowGameScore();
        mPlayerData = new RainbowPlayerData();
        mGameReward = new RainbowReward();
        isPlaying = true;
        mFailureCount = 0;
        mAgainCount = 0;
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
        mGameStateCallback = callback;
    }
    /// <summary>
    /// 设置颜色显示回调
    /// </summary>
    /// <param name="showColorCallback"></param>
    public void SetGameShowColorCallBack(GameShowColorCallBack showColorCallback)
    {
        mShowColorCallback = showColorCallback;
    }
    /// <summary>
    /// 设置倒计时回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetCountdownCallback(IntParamCallBack callback)
    {
        mCountdownCallback = callback;
    }
    /// <summary>
    /// 设置关卡等级提示回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetGameLevelUpCallback(IntParamCallBack callback)
    {
        mGameLevelUpCallback = callback;
    }
    /// <summary>
    /// 设置分数改变回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetGameScoreChangeCallback(GameScoreChangeCallBack callback)
    {
        mGameScoreChangeCallback = callback;
    }
    /// <summary>
    /// 设置重玩次数回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetAgainCountChangeCallback(IntParamCallBack callback)
    {
        mAgainCountChangeCallback = callback;
    }


    /// <summary>
    /// 点击对应颜色按钮
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public virtual EColorInputState ClickColor(EInputDirection direction, EColorType color)
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
                    if (addScore > 0 && null != mGameScoreChangeCallback)
                    {
                        mGameScoreChangeCallback(addScore, mGameScore.Score);
                    }
                    int oldLevel = mGameLevel.Level;
                    mGameLevel.InputFinished();
                    if (null != mGameLevelUpCallback && mGameLevel.Level - oldLevel > 0)
                    {
                        mGameLevelUpCallback(mGameLevel.Level);
                    }
                    if (mGameLevel.Level > RainbowGlobal.GameUnlockHardLevel && !mPlayerData.IsHardUnlockFlag())
                    {//解锁难度
                        mNeedUnlockHardFlag = true;
                    }
                    //判断是否获得成就
                    mNeedShowAchieve = mGameReward.GetNewAchieve(mGameScore.Score, mGameLevel.GameDifficulty, mPlayerData.GetAchievedList());
                    if (null == mNeedShowAchieve)
                    {//未获得成就
                        if (mNeedUnlockHardFlag)
                        {//解锁难度
                            mPlayerData.UnlockHard();
                            SetGameSate(EGameState.Game_Difficulity_Unlock, EGameDifficulty.Hard);
                        } else
                        {
                            SetGameSate(EGameState.Game_Input_Finished);
                            CancelStartNext();
                            mStartNextIndex = Timer.Add(1, 1, 1, StartShowColor);
                        }
                    }
                    else
                    {//获得成就
                        SetGameSate(EGameState.Game_Show_Reward, mNeedShowAchieve);
                        mPlayerData.AddAchieve(mNeedShowAchieve.id);
                        mNeedShowAchieve = null;
                    }
                } else
                {
                    SetGameSate(EGameState.Game_Input, mColorOut.GetNowColor());
                    addScore = mGameScore.AddFrontInputScore(mGameLevel.GameDifficulty, targetColor, mGameLevel.Level);
                    if (addScore > 0 && null != mGameScoreChangeCallback)
                    {
                        mGameScoreChangeCallback(addScore, mGameScore.Score);
                    }
                }
                
            } else if (state == EColorInputState.Input_Error)
            {//输入错误，游戏结束
                ClickInputError();
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
        int subScore = mGameScore.SubFrontInputScore(mGameLevel.GameDifficulty, mColorInput.GetInputSuccessList(), mGameLevel.Level);
        if (subScore < 0 && null != mGameScoreChangeCallback)
        {
            mGameScoreChangeCallback(subScore, mGameScore.Score);
        }
        StartShowColor();
    }
    /// <summary>
    /// 再次玩本关卡，不会重新生成颜色
    /// </summary>
    public void AgainNowLevel()
    {
        if (mAgainCount >= RainbowGlobal.GameResetLevelCountMax)
        {
            return;
        }
        if (mGameState == EGameState.Game_Show_Color)
        {
            CancelShowColor();
            AgainShowColor();
        }
        else if (mGameState == EGameState.Game_Input || mGameState == EGameState.Game_Input_Finished)
        {
            int subScore = mGameScore.SubFrontInputScore(mGameLevel.GameDifficulty, mColorInput.GetInputSuccessList(), mGameLevel.Level);
            if (subScore < 0 && null != mGameScoreChangeCallback)
            {
                mGameScoreChangeCallback(subScore, mGameScore.Score);
            }
            AgainShowColor();
        }
        mAgainCount++;
        if (null != mAgainCountChangeCallback)
        {
            mAgainCountChangeCallback(RainbowGlobal.GameResetLevelCountMax - mAgainCount);
        }
    }

    /// <summary>
    /// 继续下一关游戏
    /// </summary>
    public void ContinueNextLevel()
    {
        if (mNeedUnlockHardFlag)
        {
            mPlayerData.UnlockHard();
            SetGameSate(EGameState.Game_Difficulity_Unlock, EGameDifficulty.Hard);
            mNeedUnlockHardFlag = false;
        } else
        {
            StartShowColor();
        }
    }

    public void OnUpdate()
    {
        if (isPlaying)
        {
            switch (mGameState)
            {
                case EGameState.Game_Ready:
                    {
                        Countdown(3, mCountdownCallback, StartGame);
                        mGameState = EGameState.Game_Countdown;
                    }
                    break;
            }
        }
    }



    //////////////////////////////////////////////////////////////////////////

    protected void SetGameSate(EGameState state, params object[] args)
    {
        mGameState = state;
        if (null != mGameStateCallback)
        {
            mGameStateCallback(state, args);
        }
    }
    
    protected void StartGame()
    {
        StartShowColor();
        if (null != mGameLevelUpCallback)
        {
            mGameLevelUpCallback(mGameLevel.Level);
        }
        if (null != mAgainCountChangeCallback)
        {
            mAgainCountChangeCallback(RainbowGlobal.GameResetLevelCountMax - mAgainCount);
        }
    }

    void StartShowColor()
    {
        mStartNextIndex = -1;
        SetGameSate(EGameState.Game_Show_Color);
        mColorOut.CreateColor(mGameLevel.GetNowTrichromatic(), mGameLevel.GetNowMixedColor());
        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, PublicFunction.ListToString(mColorOut.GetOutList()));
        CancelShowColor();
        ShowColor();
    }

    void AgainShowColor()
    {
        mStartNextIndex = -1;
        SetGameSate(EGameState.Game_Show_Color);
        mColorOut.ResetIndex();
        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, PublicFunction.ListToString(mColorOut.GetOutList()));
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
            OpenInput();
        }
    }
    /// <summary>
    /// 开启输入
    /// </summary>
    protected virtual void OpenInput()
    {
        SetGameSate(EGameState.Game_Open_Input, mColorOut.Count);
        mColorOut.ResetIndex();
        mColorInput.Ready();
        SetGameSate(EGameState.Game_Input, mColorOut.GetNowColor());
    }
    /// <summary>
    /// 点击错误
    /// </summary>
    void ClickInputError()
    {
        if (mFailureCount < RainbowGlobal.GameRemainderCountMax)
        {
            GameFailure();
        }
        else
        {
            GameOver();
        }
        ++mFailureCount;
    }
    /// <summary>
    /// 输入超时
    /// </summary>
    protected virtual void ClickOutTimeCallback()
    {
        ClickInputError();
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    void GameOver()
    {
        EGameScoreState scoreState = EGameScoreState.Score_Normal;
        if (mGameScore.Score > mPlayerData.GetHighScore(mGameLevel.GameDifficulty))
        {
            scoreState = EGameScoreState.Score_High;
        }
        SetGameSate(EGameState.Game_Over, scoreState, mGameScore.Score);
        //保存游戏分数
        mPlayerData.SaveScore(mGameScore.Score, mGameLevel.GameDifficulty);
        PlayerPrefs.Save();
        mFailureCount = 0;
        mGameLevel.Reset();
        mGameScore.Reset();
        mAgainCount = 0;
    }
    /// <summary>
    /// 游戏失败
    /// </summary>
    void GameFailure()
    {
        SetGameSate(EGameState.Game_Failure, GetGameRemainderCount());
    }

    /// <summary>
    /// 显示倒计时
    /// </summary>
    /// <param name="callback"></param>
    void Countdown(int num, IntParamCallBack callback, EventDelegate.Callback countdownFinished)
    {
        CancelCountdown();
        mCountdownIndex = Timer.Add(1, 1, num + 1, delegate () {
            if (null != callback)
            {
                callback(num--);
            }
            if (num < 0 && null != countdownFinished)
            {
                mCountdownIndex = -1;
                countdownFinished();
            }
        });
    }

    void CancelShowColor()
    {
        if (-1 != mShowColorIndex)
        {
            Timer.Cancel(mShowColorIndex);
            mShowColorIndex = -1;
        }
    }

    void CancelStartNext()
    {
        if (-1 != mStartNextIndex)
        {
            Timer.Cancel(mStartNextIndex);
            mStartNextIndex = -1;
        }
    }

    void CancelCountdown()
    {
        if (-1 != mCountdownIndex)
        {
            Timer.Cancel(mCountdownIndex);
            mCountdownIndex = -1;
        }
    }

}

public delegate void GameShowColorCallBack(EColorType color, bool lastColorFlag);
public delegate void GameStateCallBack(EGameState state, params object[] args);
public delegate void IntParamCallBack(int newLevel);
public delegate void GameScoreChangeCallBack(int changeScore, int totalScore);
