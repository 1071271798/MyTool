public interface RainbowGameListener
{
    void CountdownCallback(int num);
    void GameShowColorCallback(EColorType color, bool lastColorFlag);
    void GameStateCallback(EGameState state, params object[] args);
    void GameLevelChangeCallback(int newLevel);
    void GameScoreChangeCallback(int changeScore, int totalScore);
}
