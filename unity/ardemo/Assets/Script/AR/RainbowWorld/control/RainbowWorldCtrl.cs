using Game;
using Game.Scene;
using System.Collections.Generic;
using System;
/// <summary>
/// 彩虹世界游戏控制
/// </summary>
public class RainbowWorldCtrl : SingletonObject<RainbowWorldCtrl>
{
    public delegate void RainbowGuideCallback(ERainbowGuideTriggerEvent triggerEvent);
    RainbowLogic mGameLogic;
    RainbowLogicGuide mGameLogicGuide;
    GameStateCallBack mGameStateCallback;
    RainbowGuideCallback mGuideTriggerEventCallback;
    RainbowWorldUI mGameUi;

    RainbowGuide mGameGuide;

    EGameDifficulty mGameDifficulty;


    public RainbowWorldCtrl()
    {
        mGameLogic = new RainbowLogic();
        mGameGuide = new RainbowGuide();
    }
    
    public void RegisterGameUI(RainbowWorldUI ui)
    {
        mGameUi = ui;
    }

    public void UnRegisterGameUI()
    {
        mGameUi = null;
    }
    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="difficulty">难度</param>
    /// <param name="stateCallback">游戏状态回调</param>
    /// <param name="showColorCallback">显示游戏回调</param>
    /// <param name="levelUpCallBack">关卡升级回调</param>
    /// <param name="scoreCallback">分数改变回调</param>
    /// <param name="countdownCallBack">倒计时回调</param>
    public void GameStart(EGameDifficulty difficulty, GameStateCallBack stateCallback, GameShowColorCallBack showColorCallback, IntParamCallBack levelUpCallBack, GameScoreChangeCallBack scoreCallback, IntParamCallBack countdownCallBack)
    {
        mGameDifficulty = difficulty;
        mGameStateCallback = stateCallback;
        mGameLogic.SetGameStateCallBack(GameStateCallback);
        mGameLogic.SetGameShowColorCallBack(showColorCallback);
        mGameLogic.SetGameLevelUpCallback(levelUpCallBack);
        mGameLogic.SetGameScoreChangeCallback(scoreCallback);
        mGameLogic.SetCountdownCallback(countdownCallBack);
        mGameLogic.SetAgainCountChangeCallback(AgainCountChangeCallback);
        if (mGameGuide.IsGuide())
        {
            if (null == mGameLogicGuide)
            {
                mGameLogicGuide = new RainbowLogicGuide();
            }
            if (null != mGameUi)
            {
                mGameUi.OnShowGameUI();
            }
            mGameLogicGuide.SetColorOut(mGameGuide.GetCurrentGuide().color);
            mGameLogicGuide.GameStart(GameStateCallback, showColorCallback);
            
        } else
        {
            mGameLogic.GameStart(difficulty);
        }
    }

    /// <summary>
    /// 点击颜色块
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="color"></param>
    public EColorInputState ClickColor(EInputDirection direction, EColorType color)
    {
        if (mGameGuide.IsGuide())
        {
            return mGameLogicGuide.ClickColor(direction, color);
        }
        return mGameLogic.ClickColor(direction, color);
    }
   
    public void OnUpdate()
    {
        if (!mGameGuide.IsGuide())
        {
            mGameLogic.OnUpdate();
        }
    }


    /// <summary>
    /// 指引事件监听
    /// </summary>
    /// <param name="eventId"></param>
    public void GuideEventListener(EGameGuideEventID eventId, RainbowGuideCallback callback)
    {
        mGuideTriggerEventCallback = callback;
        RainbowGuideData guideData = mGameGuide.ActiveGuide(eventId);
        if (null != guideData)
        {
            if (null == mGameLogicGuide)
            {
                mGameLogicGuide = new RainbowLogicGuide();
            }
            if (guideData.triggerTime == ERainbowGuideTriggerTime.Guide_Start)
            {
                GuideTriggerEvent(guideData);
            }
            RainbowGuideMsg.ShowMsg(guideData);
        }
    }
    /// <summary>
    /// 是否在指引
    /// </summary>
    /// <returns></returns>
    public bool IsGuide()
    {
        return mGameGuide.IsGuide();
    }
    /// <summary>
    /// 下一步指引
    /// </summary>
    /// <returns></returns>
    public RainbowGuideData GuideMoveNext()
    {
        if (null != mGameGuide.GetCurrentGuide() && mGameGuide.GetCurrentGuide().triggerTime == ERainbowGuideTriggerTime.Guide_End)
        {
            GuideTriggerEvent(mGameGuide.GetCurrentGuide());
        }
        RainbowGuideData guideData = mGameGuide.MoveNext();
        if (null != guideData)
        {
            RainbowGuideMsg.ShowMsg(guideData);
            if (guideData.triggerTime == ERainbowGuideTriggerTime.Guide_Start)
            {
                GuideTriggerEvent(guideData);
            }
        }
        return guideData;

    }
    /// <summary>
    /// 跳过指引
    /// </summary>
    /// <returns></returns>
    public RainbowGuideData SkipGuide()
    {
        if (null != mGameGuide.GetCurrentGuide() && mGameGuide.GetCurrentGuide().triggerTime == ERainbowGuideTriggerTime.Guide_End)
        {
            GuideTriggerEvent(mGameGuide.GetCurrentGuide());
        }
        RainbowGuideData guideData = mGameGuide.SkipGuide();
        if (null != guideData)
        {
            RainbowGuideMsg.ShowMsg(guideData);
            if (guideData.triggerTime == ERainbowGuideTriggerTime.Guide_Start)
            {
                GuideTriggerEvent(guideData);
            }
        }
        return guideData;
    }
    /// <summary>
    /// 重新玩当前关卡，不重新生成颜色
    /// </summary>
    public void AgainNowLevel()
    {
        mGameLogic.AgainNowLevel();
    }

    //////////////////////////////////////////////////////////////////////////
    void GuideTriggerEvent(RainbowGuideData data)
    {
        if (null != data && data.triggerEvent != ERainbowGuideTriggerEvent.None)
        {
            if (null != mGuideTriggerEventCallback)
            {
                mGuideTriggerEventCallback(data.triggerEvent);
            }
            switch (data.triggerEvent)
            {
                case ERainbowGuideTriggerEvent.Trigger_Guide_Show_Color:
                    break;
                case ERainbowGuideTriggerEvent.Trigger_Guide_Input_Color:
                    if (null != mGameLogicGuide)
                    {
                        mGameLogicGuide.OpenInput();
                    }
                    break;
                case ERainbowGuideTriggerEvent.Trigger_Start_Game:
                    mGameLogic.GameStart(mGameDifficulty);
                    break;
                case ERainbowGuideTriggerEvent.Trigger_Continue_Game:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 游戏失败，点击取消按钮，游戏结束
    /// </summary>
    void GameFailureCancelCallback()
    {
        mGameLogic.GiveUpGame();
    }
    /// <summary>
    /// 游戏失败，点击确认按钮，游戏继续
    /// </summary>
    void GameFailureConfirmCallback()
    {
        mGameLogic.RestartNowLevel();
    }

    /// <summary>
    /// 游戏结束，点击退出按钮，返回游戏菜单
    /// </summary>
    void GameOverQuitCallback()
    {
        GameMenuScene.GotoGameMenu(EGameMenuType.RainbowWorld);
    }
    /// <summary>
    /// 游戏结束，点击重玩按钮，游戏重新开始
    /// </summary>
    void GameOverRestartCallback()
    {
        mGameLogic.RestartGame();
    }

    void GameStateCallback(EGameState state, params object[] args)
    {
        switch (state)
        {
            case EGameState.Game_Select:
                break;
            case EGameState.Game_Ready:
                break;
            case EGameState.Game_Countdown:
                break;
            case EGameState.Game_Pause:
                break;
            case EGameState.Game_Show_Color:
                break;
            case EGameState.Game_Show_Color_Finished:
                if (mGameGuide.IsGuide() && null != mGameLogicGuide)
                {
                    GuideMoveNext();
                }
                break;
            case EGameState.Game_Input_Finished:
                if (mGameGuide.IsGuide() && null != mGameLogicGuide)
                {
                    GuideMoveNext();
                }
                break;
            case EGameState.Game_Open_Input:
                if (mGameGuide.IsGuide() && null != mGameUi && null != args && args.Length > 0)
                {
                    mGameUi.SetColorCount((int)args[0]);
                }
                break;
            case EGameState.Game_Input:
                if (null != args && args.Length > 0)
                {
                    EColorType color = (EColorType)args[0];
                    if (mGameGuide.IsGuide() && null != mGameUi)
                    {
                        mGameUi.ShowLightForInputBtn(color);
                    }
                }
                break;
            case EGameState.Game_Show_Reward:
                {//显示成就奖励
                    RainbowAchieveData data = null;
                    if (null != args && args.Length > 0 && args[0] is RainbowAchieveData)
                    {
                        data = (RainbowAchieveData)args[0];
                    }
                    RainbowAchieveMsg.ShowAchieveMsg(data, mGameLogic.ContinueNextLevel);
                }
                break;
            case EGameState.Game_Difficulity_Unlock:
                {//解锁难度
                    EGameDifficulty diff = EGameDifficulty.Hard;
                    if (null != args && args.Length > 0)
                    {
                        diff = (EGameDifficulty)args[0];
                    }
                    RainbowUnlockMsg.ShowUnlockMsg(diff, mGameLogic.ContinueNextLevel);
                }
                break;
            case EGameState.Game_Failure:
                {
                    int remainderCount = 0;
                    if (null != args && args.Length > 0 && args[0] is int)
                    {
                        remainderCount = (int)args[0];
                    }
                    RainbowGameFailureMsg.ShowGameFailureMsg(remainderCount, GameFailureCancelCallback, GameFailureConfirmCallback);
                }
                break;
            case EGameState.Game_Over:
                {
                    EGameScoreState scoreState = EGameScoreState.Score_Normal;
                    int score = 0;
                    if (null != args && args.Length > 1)
                    {
                        if (args[0] is EGameScoreState)
                        {
                            scoreState = (EGameScoreState)args[0];
                        }
                        if (args[1] is int)
                        {
                            score = (int)args[1];
                        }
                    }
                    RainbowGameOverMsg.ShowGameOverMsg(scoreState, score, GameOverQuitCallback, GameOverRestartCallback);
                }
                break;
            default:
                break;
        }
        if (null != mGameStateCallback)
        {
            mGameStateCallback(state, args);
        }
    }

    void AgainCountChangeCallback(int count)
    {
        if (null != mGameUi)
        {
            mGameUi.SetAgainCount(count);
        }
    }
}