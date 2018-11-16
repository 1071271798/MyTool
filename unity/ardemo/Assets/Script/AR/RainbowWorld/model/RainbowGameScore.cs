/// <summary>
/// 彩虹世界分数
/// </summary>
public class RainbowGameScore
{
    int mScore;
    public int Score
    {
        get { return mScore; }
    }
    /// <summary>
    /// 增加前几个输入的分数
    /// </summary>
    /// <param name="difficulty"></param>
    /// <param name="color"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public int AddFrontInputScore(EGameDifficulty difficulty, EColorType color, int level)
    {
        int score = 0;
        switch(difficulty)
        {
            case EGameDifficulty.Easy:
            case EGameDifficulty.Normal:
                score = 100;
                break;
            case EGameDifficulty.Hard:
                score = 200;
                break;
        }
        mScore += score;
        return score;
    }
    /// <summary>
    /// 增加最后一种颜色的分数
    /// </summary>
    /// <param name="difficulty"></param>
    /// <param name="color"></param>
    /// <param name="level"></param>
    /// <param name="colorCount"></param>
    /// <returns></returns>
    public int AddLastInputScore(EGameDifficulty difficulty, EColorType color, int level, int colorCount)
    {
        int score = 0;
        switch (difficulty)
        {
            case EGameDifficulty.Easy:
            case EGameDifficulty.Normal:
                score = 100;
                break;
            case EGameDifficulty.Hard:
                score = 200;
                break;
        }
        score = score * colorCount * level - (colorCount - 1) * score;
        mScore += score;
        return score;
    }

    public void Reset()
    {
        mScore = 0;
    }
}