using System.Collections.Generic;
/// <summary>
/// 彩虹世界游戏控制
/// </summary>
public class RainbowWorldCtrl : SingletonObject<RainbowWorldCtrl>
{
    RainbowWorldLogic mGameLogic;

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
        mGameLogic.SetGameStateCallBack(stateCallback);
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
}