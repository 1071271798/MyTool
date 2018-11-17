using Game;
using Game.Scene;
using System.Collections.Generic;
/// <summary>
/// 彩虹世界游戏控制
/// </summary>
public class RainbowWorldCtrl : SingletonObject<RainbowWorldCtrl>
{
    RainbowWorldLogic mGameLogic;
    GameStateCallBack mGameStateCallback;

    public RainbowWorldCtrl()
    {
        mGameLogic = new RainbowWorldLogic();
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
        mGameStateCallback = stateCallback;
        mGameLogic.SetGameStateCallBack(GameStateCallback);
        mGameLogic.SetGameShowColorCallBack(showColorCallback);
        mGameLogic.SetGameLevelUpCallback(levelUpCallBack);
        mGameLogic.SetGameScoreChangeCallBack(scoreCallback);
        mGameLogic.SetCountdownCallback(countdownCallBack);
        mGameLogic.GameStart(difficulty);
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        mGameLogic.RestartGame();
    }

    /// <summary>
    /// 点击颜色块
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="color"></param>
    public EColorInputState ClickColor(EInputDirection direction, EColorType color)
    {
        return mGameLogic.ClickColor(direction, color);
    }
   
    public void OnUpdate()
    {
        mGameLogic.OnUpdate();
    }
    /// <summary>
    /// 获取颜色列表
    /// </summary>
    /// <returns></returns>
    public List<EColorType> GetColorOutList()
    {
        return mGameLogic.GetColorOutList();
    }

    public EColorType GetNowColorOut()
    {
        return mGameLogic.GetNowColorOut();
    }

    void GameStateCallback(EGameState state)
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
            case EGameState.Game_Wait:
                break;
            case EGameState.Game_Input:
                break;
            case EGameState.Game_Failure:
                RainbowGameFailureMsg.ShowGameFailureMsg(mGameLogic.GetGameRemainderCount(), GameFailureCancelCallback, GameFailureConfirmCallback);
                break;
            case EGameState.Game_Over:
                RainbowGameOverMsg.ShowGameOverMsg(EGameScoreState.Score_Normal, mGameLogic.GameScore, GameOverQuitCallback, GameOverRestartCallback);
                break;
            default:
                break;
        }
        if (null != mGameStateCallback)
        {
            mGameStateCallback(state);
        }
    }


    //////////////////////////////////////////////////////////////////////////
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
}