/// <summary>
/// 颜色
/// </summary>
public enum EColorType : byte
{
    None = 0,
    /// <summary>
    /// 红色
    /// </summary>
    R = 1,
    /// <summary>
    /// 绿色
    /// </summary>
    G = 2,
    /// <summary>
    /// 蓝色
    /// </summary>
    B = 4,
    /// <summary>
    /// 黄色
    /// </summary>
    Y = 3,
    /// <summary>
    /// 紫色
    /// </summary>
    M = 5,
    /// <summary>
    /// 青色
    /// </summary>
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
    Game_Show_Color,
    Game_Show_Color_Finished,
    Game_Open_Input,
    Game_Input,
    Game_Input_Finished,
    Game_Show_Reward,
    Game_Difficulity_Unlock,
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
/// <summary>
/// 游戏引导事件id，用于触发引导
/// </summary>
public enum EGameGuideEventID : byte
{
    None = 0,
    First_Recognition_Success,//首次识别成功
    Show_Color_Finished,//显示颜色完毕
    Input_Color_Finished,//输入完毕
    Unlock_Color_Y,//解锁黄色
    Unlock_Color_M,//解锁紫色
    Unlock_Color_C,//解锁青色
}
/// <summary>
/// 彩虹世界引导UI样式
/// </summary>
public enum ERainbowGuideStyle : byte
{
    Style_Hide = 0,//不显示UI
    Style_Only_Text,//纯文字
    Style_Text_And_BaseColor_Tap,//文字与三基色的点击组合
    Style_Text_And_MixedColor,//文字和混合颜色的介绍
    Style_Text_And_MixedColor_Tap,//文字与混合颜色的点击组合
}
/// <summary>
/// 指引中触发的事件
/// </summary>
public enum ERainbowGuideTriggerEvent : byte
{
    None = 0,
    Trigger_Guide_Show_Color,
    Trigger_Guide_Input_Color,
    Trigger_Start_Game,
    Trigger_Continue_Game,
}
/// <summary>
/// 事件触发时机
/// </summary>
public enum ERainbowGuideTriggerTime : byte
{
    None = 0,
    Guide_Start,
    Guide_End,
}