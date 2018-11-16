using System;
using System.Collections.Generic;

public class ColorOut
{
    
    List<EColorType> mColorList;
    int mNowIndex;

    public int Count
    {
        get { if (null != mColorList) return mColorList.Count; return 0; }
    }

    public ColorOut()
    {
        UnityEngine.Random.InitState((int)(DateTime.Now.Ticks / 100000) + UnityEngine.Random.Range(0, 1000));
    }

    /// <summary>
    /// 生成颜色列表
    /// </summary>
    /// <param name="trichromaticNum"></param>
    /// <param name="mixedColorNum"></param>
    public void CreateColor(int trichromaticNum, int mixedColorNum = 0)
    {
        mColorList = CreateAllColorList(trichromaticNum, mixedColorNum);
        mNowIndex = 0;
    }

    /// <summary>
    /// 取出一个颜色
    /// </summary>
    /// <returns></returns>
    public EColorType PopColor()
    {
        if (null != mColorList && mNowIndex < mColorList.Count)
        {
            return mColorList[mNowIndex++];
        }
        return EColorType.None;
    }
    /// <summary>
    /// 把下标复位
    /// </summary>
    public void ResetIndex()
    {
        mNowIndex = 0;
    }

    public EColorType GetNowColor()
    {
        if (null != mColorList && mNowIndex < mColorList.Count)
        {
            return mColorList[mNowIndex];
        }
        return EColorType.None;
    }

    public void MoveNextColor()
    {
        mNowIndex++;
    }

    /// <summary>
    /// 已取完最后一个颜色
    /// </summary>
    /// <returns></returns>
    public bool IsLastColor()
    {
        if (mNowIndex >= mColorList.Count)
        {
            return true;
        }
        return false;
    }

    public List<EColorType> GetOutList()
    {
        return mColorList;
    }

    /// <summary>
    /// 根据数量生成三基色列表
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    List<EColorType> CreateTrichromaticList(int num)
    {
        List<EColorType> list = new List<EColorType>();
        for (int i = 0; i < num; ++i)
        {
            list.Add(CreateTrichromatic());
        }
        return list;
    }

    /// <summary>
    /// 随机生成所有颜色的列表
    /// </summary>
    /// <param name="trichromaticNum"></param>
    /// <param name="mixedColorNum"></param>
    /// <returns></returns>
    List<EColorType> CreateAllColorList(int trichromaticNum, int mixedColorNum)
    {
        List<EColorType> list = new List<EColorType>();
        
        for (int i = 0, imax = trichromaticNum + mixedColorNum; i < imax; ++i)
        {
            int randomNum = UnityEngine.Random.Range(1, trichromaticNum + mixedColorNum + 1);
            if (randomNum <= trichromaticNum)
            {
                list.Add(CreateTrichromatic());
                --trichromaticNum;
            } else
            {
                list.Add(CreateMixedColor());
                --mixedColorNum;
            }
        }
        return list;
    }

    /// <summary>
    /// 随机生成三基色
    /// </summary>
    /// <returns></returns>
    EColorType CreateTrichromatic()
    {
        int index = UnityEngine.Random.Range(0, 3);
        return ColorUtils.Trichromatic[index];
    }

    /// <summary>
    /// 随机生成混合色
    /// </summary>
    /// <returns></returns>
    EColorType CreateMixedColor()
    {
        int index = UnityEngine.Random.Range(0, 3);
        return ColorUtils.MixedColor[index];
    }
}