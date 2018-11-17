/// <summary>
/// 颜色
/// </summary>
public enum EColorType : byte
{
    None = 0,
    R = 1,
    G = 2,
    B = 4,
    Y = 3,
    M = 5,
    C = 6,
}
/// <summary>
/// 颜色输入状态
/// </summary>
public enum EColorInputState : byte
{
    Input_None = 0,
    Input_Wait,
    Input_Error,
    Input_Success,
}

/// <summary>
/// 游戏状态
/// </summary>
public enum EGameState : byte
{
    Game_Select = 0,
    Game_Ready,
    Game_Countdown,
    Game_Pause,
    //Game_Tutorial,
    Game_Show_Color,
    Game_Wait,
    Game_Input,
    Game_Failure,
    Game_Over,
}
/// <summary>
/// 游戏难度
/// </summary>
public enum EGameDifficulty : byte
{
    Easy = 0,
    Normal,
    Hard,
}
/// <summary>
/// 输入方向
/// </summary>
public enum EInputDirection : byte
{
    Input_Left,
    Input_Right,
}
/// <summary>
/// 游戏分数状态
/// </summary>
public enum EGameScoreState : byte
{
    Score_Normal,
    Score_High,
}