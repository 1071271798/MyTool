using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 成就
/// </summary>
public class RainbowAchieve : ScriptableObject
{
    public List<RainbowAchieveData> dataList;

    public RainbowAchieve()
    {
        dataList = new List<RainbowAchieveData>();
    }
}
[System.Serializable]
public class RainbowAchieveData
{
    public int id;
    public string titleName;
    public int needScore;
    public EGameDifficulty needDifficulty;
}