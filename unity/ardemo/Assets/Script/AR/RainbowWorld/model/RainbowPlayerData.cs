using System;
using System.Collections.Generic;
//using System.Xml.Serialization;
using UnityEngine;

public class RainbowPlayerData
{
    /*static RainbowScoreManager sInst;
    [XmlElement]
    public List<SaveScoreRanking> ranking;*/
    int mEasyHighScore;
    int mHardHighScore;
    bool mShowGreetingFlag = true;
    List<int> mAchievedList;

    bool mHardUnlockFlag;
    EGameDifficulty mSelectDifficulty = EGameDifficulty.Easy;

    string Easy_High_Score_Key = "rainbowEasyHighScore";
    string Hard_High_Score_Key = "rainbowHardHighScore";
    string Greeting_Key = "rainbowGreeting";
    string Achieved_Key = "rainbowAchieved";
    string Difficulity_Hard_Unlock_Key = "rainbowHardUnlock";
    string Select_Difficulity_Key = "selectDifficulty";

    public RainbowPlayerData()
    {
        //ranking = new List<SaveScoreRanking>();
        mEasyHighScore = PlayerPrefs.GetInt(Easy_High_Score_Key);
        mHardHighScore = PlayerPrefs.GetInt(Hard_High_Score_Key);
        mHardUnlockFlag = PlayerPrefs.GetInt(Difficulity_Hard_Unlock_Key) == 1 ? true : false;
        mShowGreetingFlag = PlayerPrefs.GetInt(Greeting_Key) == 1 ? false : true;
        if (PlayerPrefs.HasKey(Achieved_Key))
        {
            mAchievedList = PublicFunction.StringToList(PlayerPrefs.GetString(Achieved_Key));
        }
        if (PlayerPrefs.HasKey(Select_Difficulity_Key))
        {
            mSelectDifficulty = (EGameDifficulty)(PlayerPrefs.GetInt(Select_Difficulity_Key));
        }
    }

    /*public static RainbowScoreManager GetInst()
    {
        if (null == sInst)
        {
            sInst = StateStorage.LoadData<RainbowScoreManager>("rainbowRanking");
            if (null == sInst)
            {
                sInst = new RainbowScoreManager();
            }
        }
        return sInst;
    }*/

    /*public void SaveScore(SaveScoreRanking data)
    {
        ranking.Add(data);
        if (ranking.Count >= 2)
        {
            ranking.Sort(new ScoreComparer());
        }
        StateStorage.SaveData<RainbowScoreManager>("rainbowRanking", sInst);
    }*/
    /// <summary>
    /// 保存游戏分数
    /// </summary>
    /// <param name="score"></param>
    public void SaveScore(int score, EGameDifficulty diff)
    {
        if (diff == EGameDifficulty.Easy)
        {
            if (score > mEasyHighScore)
            {
                mEasyHighScore = score;
                PlayerPrefs.SetInt(Easy_High_Score_Key, mEasyHighScore);
            }
        } else
        {
            if (score > mHardHighScore)
            {
                mHardHighScore = score;
                PlayerPrefs.SetInt(Hard_High_Score_Key, mHardHighScore);
            }
        }

    }
    /// <summary>
    /// 解锁难度
    /// </summary>
    public void UnlockHard()
    {
        mHardUnlockFlag = true;
        PlayerPrefs.SetInt(Difficulity_Hard_Unlock_Key, 1);
    }
    /// <summary>
    /// 获取最高分
    /// </summary>
    /// <returns></returns>
    public int GetHighScore(EGameDifficulty diff)
    {
        /*if (null != ranking && ranking.Count > 0)
        {
            return ranking[ranking.Count - 1].score;
        }*/
        if (diff == EGameDifficulty.Easy)
        {
            return mEasyHighScore;
        }
        return mHardHighScore;
    }
    /// <summary>
    /// 获取已获得成就
    /// </summary>
    /// <returns></returns>
    public List<int> GetAchievedList()
    {
        return mAchievedList;
    }
    /// <summary>
    /// 困难难度是否解锁
    /// </summary>
    /// <returns></returns>
    public bool IsHardUnlockFlag()
    {
        return mHardUnlockFlag;
    }
    /// <summary>
    /// 选择的难度
    /// </summary>
    /// <returns></returns>
    public EGameDifficulty GetSelectedDifficulty()
    {
        return mSelectDifficulty;
    }

    /// <summary>
    /// 获得成就
    /// </summary>
    /// <param name="id">成就id</param>
    public void AddAchieve(int id)
    {
        if (null == mAchievedList)
        {
            mAchievedList = new List<int>();
        }
        mAchievedList.Add(id);
        if (mAchievedList.Count > 1)
        {
            mAchievedList.Sort();
        }
        PlayerPrefs.SetString(Achieved_Key, PublicFunction.ListToString(mAchievedList));
    }
    /// <summary>
    /// 判断是否已经获取该成就
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool HaveAchieved(int id)
    {
        if (null == mAchievedList)
        {
            return false;
        }
        return mAchievedList.IndexOf(id) != -1 ? true : false;
    }
    /// <summary>
    /// 设置选择的难度
    /// </summary>
    /// <param name="selectedDifficulty"></param>
    public void SetSelectedDifficulty(EGameDifficulty selectedDifficulty)
    {
        mSelectDifficulty = selectedDifficulty;
        PlayerPrefs.SetInt(Select_Difficulity_Key, (byte)mSelectDifficulty);
    }
    /// <summary>
    /// 是否显示欢迎页面
    /// </summary>
    /// <returns></returns>
    public bool IsShowGreeting()
    {
        return mShowGreetingFlag;
    }
    /// <summary>
    /// 显示完了欢迎页面
    /// </summary>
    public void ShowGreetingFinished()
    {
        mShowGreetingFlag = false;
        PlayerPrefs.SetInt(Greeting_Key, 1);
    }

    //////////////////////////////////////////////////////////////////////////
    /*public class ScoreComparer : IComparer<SaveScoreRanking>
    {
        public int Compare(SaveScoreRanking x, SaveScoreRanking y)
        {
            return x.score - y.score;
        }
    }*/

}

/*public class SaveScoreRanking
{
    [XmlAttribute]
    public int score;

    public SaveScoreRanking()
    {

    }

    public SaveScoreRanking(int score)
    {
        this.score = score;
    }
}*/