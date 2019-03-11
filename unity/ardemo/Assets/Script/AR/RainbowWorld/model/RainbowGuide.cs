using Game.Resource;
using System.Collections.Generic;
using UnityEngine;

public class RainbowGuide
{
    RainbowGuideConfig mGuideConfig;
    string Config_Name = "rainbowGuideConfig.asset";

    /// <summary>
    /// 已完成指引的id
    /// </summary>
    List<int> mCompletedGuide;
    /// <summary>
    /// 是否已经完成所有引导，若后续有新增引导，需清空此标记
    /// </summary>
    bool mCompletedAllGuideFlag;

    string Completed_Guide_Key = "rainbowCompletedGuide";
    string Completed_All_Guide = "rainbowCompletedAllGuide";

    RainbowGuideData mCurrentGuideData;

    public RainbowGuide()
    {
        mCompletedAllGuideFlag = PlayerPrefs.GetInt(Completed_Guide_Key) == 1 ? true : false;
        if (!mCompletedAllGuideFlag)
        {
            mCompletedGuide = PublicFunction.StringToList(PlayerPrefs.GetString(Completed_Guide_Key));
            InitGuideConfig();
        }
    }
    /// <summary>
    /// 是否完成引导
    /// </summary>
    /// <returns></returns>
    public bool IsCompletedGuide()
    {
        return mCompletedAllGuideFlag;
    }
    /// <summary>
    /// 是否处于指引状态
    /// </summary>
    /// <returns></returns>
    public bool IsGuide()
    {
        return mCurrentGuideData != null ? true : false;
    }
    /// <summary>
    /// 获取当前指引
    /// </summary>
    /// <returns></returns>
    public RainbowGuideData GetCurrentGuide()
    {
        return mCurrentGuideData;
    }

    /// <summary>
    /// 出发引导
    /// </summary>
    /// <param name="eventId"></param>
    /// <returns></returns>
    public RainbowGuideData ActiveGuide(EGameGuideEventID eventId)
    {
        if (!mCompletedAllGuideFlag && null != mGuideConfig && null != mGuideConfig.guideList)
        {
            RainbowGuideData data = null;
            for (int i = 0, imax = mGuideConfig.guideList.Count; i < imax; ++i)
            {
                data = mGuideConfig.guideList[i];
                if (data.activeEvent == eventId && (null == mCompletedGuide || !mCompletedGuide.Contains(data.id)))
                {
                    mCurrentGuideData = data;
                    return data;
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 前往下一步
    /// </summary>
    /// <returns></returns>
    public RainbowGuideData MoveNext()
    {
        if (null != mCurrentGuideData)
        {
            CompletedCurrentGuide();
            if (mCurrentGuideData.nextId > 0 && null != mGuideConfig && null != mGuideConfig.guideList)
            {
                RainbowGuideData data = null;
                for (int i = 0, imax = mGuideConfig.guideList.Count; i < imax; ++i)
                {
                    data = mGuideConfig.guideList[i];
                    if (data.id == mCurrentGuideData.nextId)
                    {
                        mCurrentGuideData = data;
                        return data;
                    }
                }
            }
            mCurrentGuideData = null;
            SaveGuideData();
            CheckCompletedAllGuide();
        }
        return null;
    }

    /// <summary>
    /// 跳过指引
    /// </summary>
    /// <returns></returns>
    public RainbowGuideData SkipGuide()
    {
        if (null != mCurrentGuideData)
        {
            CompletedCurrentGuide();
            SaveGuideData();
            if (mCurrentGuideData.skipId > 0 && null != mGuideConfig && null != mGuideConfig.guideList)
            {
                RainbowGuideData data = null;
                for (int i = 0, imax = mGuideConfig.guideList.Count; i < imax; ++i)
                {
                    data = mGuideConfig.guideList[i];
                    if (data.id == mCurrentGuideData.skipId)
                    {
                        mCurrentGuideData = data;
                        return data;
                    }
                }
            }
            mCurrentGuideData = null;
            CheckCompletedAllGuide();
        }
        return null;
    }
    /// <summary>
    /// 保存引导进度
    /// </summary>
    void SaveGuideData()
    {
        if (null != mCompletedGuide && mCompletedGuide.Count > 0)
        {
            PlayerPrefs.SetString(Completed_Guide_Key, PublicFunction.ListToString(mCompletedGuide));
        }
    }

    void CheckCompletedAllGuide()
    {
        if (null != mGuideConfig && null != mGuideConfig.guideList)
        {
            RainbowGuideData data = null;
            for (int i = 0, imax = mGuideConfig.guideList.Count; i < imax; ++i)
            {
                data = mGuideConfig.guideList[i];
                if (data.activeEvent != EGameGuideEventID.None && (null == mCompletedGuide || !mCompletedGuide.Contains(data.id)))
                {
                    return;
                }
            }
            //全部完成
            mCompletedAllGuideFlag = true;
            PlayerPrefs.SetInt(Completed_All_Guide, 1);
        }
    }

    void CompletedCurrentGuide()
    {
        if (mCurrentGuideData.activeEvent != EGameGuideEventID.None)
        {
            if (null == mCompletedGuide)
            {
                mCompletedGuide = new List<int>();
            }
            mCompletedGuide.Add(mCurrentGuideData.id);
        }
    }

    void InitGuideConfig()
    {
        if (null == mGuideConfig)
        {
            mGuideConfig = ResourcesEx.Load<RainbowGuideConfig>(Config_Name);
        }
    }
}