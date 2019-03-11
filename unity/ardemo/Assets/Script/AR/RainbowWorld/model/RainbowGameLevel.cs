/// <summary>
/// 游戏关卡
/// </summary>
public class RainbowGameLevel
{
    int mLevel;
    public int Level
    {
        get { return mLevel; }
    }
    EGameDifficulty mGameDifficulty;
    public EGameDifficulty GameDifficulty
    {
        get { return mGameDifficulty; }
    }
    int mRepeatCount;

    public RainbowGameLevel(EGameDifficulty difficulty)
    {
        mGameDifficulty = difficulty;
        mLevel = 1;
        mRepeatCount = 0;
    }

    public int GetNowTrichromatic()
    {
        if (mGameDifficulty == EGameDifficulty.Easy)
        {
            return 2 + mLevel;
        } else if (mGameDifficulty == EGameDifficulty.Hard)
        {
            switch (mLevel)
            {
                case 1:
                    return 2;
                case 2:
                case 3:
                    return 3;
                case 4:
                case 5:
                    return 4;
                case 6:
                    return 5;
                default:
                    return mLevel - 1;
            }
        } else
        {

        }
        return 3;
    }


    public int GetNowMixedColor()
    {
        if (mGameDifficulty == EGameDifficulty.Easy)
        {
            return 0;
        }
        else if (mGameDifficulty == EGameDifficulty.Hard)
        {
            switch (mLevel)
            {
                case 1:
                    return 1;
                case 2:
                case 3:
                    return mLevel - 1;
                case 4:
                case 5:
                    return mLevel - 2;
                case 6:
                    return 3;
                default:
                    return mLevel - 4;
            }
        }
        else
        {

        }
        return 0;
    }
    /// <summary>
    /// 输入完成调用一次
    /// </summary>
    /// <returns>是否进入下一关，true进入下一关</returns>
    public bool InputFinished()
    {
        ++mRepeatCount;
        if (mRepeatCount >= 1)
        {
            ++mLevel;
            mRepeatCount = 0;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        mLevel = 1;
        mRepeatCount = 0;
    }
    
}