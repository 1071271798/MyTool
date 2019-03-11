using Game.Resource;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
public class RainbowReward
{
    RainbowAchieve mAchieveData;
    string Achievement_File = "rainbowAchieveConfig.asset";

    public RainbowReward()
    {
        mAchieveData = ResourcesEx.Load<RainbowAchieve>(Achievement_File);
    }
    /// <summary>
    /// 获取新增成就,若无新增返回null
    /// </summary>
    /// <param name="score">当前分数</param>
    /// <param name="hard">游戏难度</param>
    /// <param name="achievedList">已获得成就</param>
    public RainbowAchieveData GetNewAchieve(int score, EGameDifficulty hard, List<int> achievedList)
    {
        if (null != mAchieveData && null != mAchieveData.dataList)
        {
            RainbowAchieveData data = null;
            for (int i = 0, imax = mAchieveData.dataList.Count; i < imax; ++i)
            {
                data = mAchieveData.dataList[i];
                if (data.needDifficulty == hard && score >= data.needScore && (null == achievedList || !achievedList.Contains(data.id)))
                {
                    return data;
                }
            }
        }
        return null;
    }

#if UNITY_EDITOR
    void SaveTest()
    {
        if (null == mAchieveData)
        {
            mAchieveData = new RainbowAchieve();
            RainbowAchieveData tmp = new RainbowAchieveData();
            tmp.id = 1;
            tmp.titleName = "宝石1";
            tmp.needScore = 1470;
            tmp.needDifficulty = EGameDifficulty.Easy;
            mAchieveData.dataList.Add(tmp);
            tmp = new RainbowAchieveData();
            tmp.id = 2;
            tmp.titleName = "宝石2";
            tmp.needScore = 3350;
            tmp.needDifficulty = EGameDifficulty.Easy;
            mAchieveData.dataList.Add(tmp);
            tmp = new RainbowAchieveData();
            tmp.id = 3;
            tmp.titleName = "宝石3";
            tmp.needScore = 6160;
            tmp.needDifficulty = EGameDifficulty.Easy;
            mAchieveData.dataList.Add(tmp);
            tmp = new RainbowAchieveData();
            tmp.id = 4;
            tmp.titleName = "宝石4";
            tmp.needScore = 9000;
            tmp.needDifficulty = EGameDifficulty.Easy;
            mAchieveData.dataList.Add(tmp);
            tmp = new RainbowAchieveData();
            tmp.id = 5;
            tmp.titleName = "宝石5";
            tmp.needScore = 3020;
            tmp.needDifficulty = EGameDifficulty.Hard;
            mAchieveData.dataList.Add(tmp);
            tmp = new RainbowAchieveData();
            tmp.id = 6;
            tmp.titleName = "宝石6";
            tmp.needScore = 8200;
            tmp.needDifficulty = EGameDifficulty.Hard;
            mAchieveData.dataList.Add(tmp);
            tmp = new RainbowAchieveData();
            tmp.id = 7;
            tmp.titleName = "宝石7";
            tmp.needScore = 15860;
            tmp.needDifficulty = EGameDifficulty.Hard;
            mAchieveData.dataList.Add(tmp);
            tmp = new RainbowAchieveData();
            tmp.id = 8;
            tmp.titleName = "宝石8";
            tmp.needScore = 18000;
            tmp.needDifficulty = EGameDifficulty.Hard;
            mAchieveData.dataList.Add(tmp);

            AssetDatabase.CreateAsset(mAchieveData, "Assets/Resources/" + Achievement_File);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
#endif

}