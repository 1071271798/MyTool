using Game;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:NetWaitMsg.cs
/// Description:
/// Time:2015/9/1 16:26:29
/// </summary>
public class NetWaitMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    int Point_Num = 12;
    float Point_R = 18.0f;
    float mTimes = 0;
    float mShowTime = 0;
    static NetWaitMsg sInst = null;
    //List<UISprite> mSpriteList = null;
    #endregion

    #region 公有函数
    /// <summary>
    /// 如何时间为0，则表示无时间限制
    /// </summary>
    /// <param name="showTime"></param>
    public NetWaitMsg(float showTime)
    {
        mUIResPath = "Prefab/UI/NetWaiting";
        mShowTime = showTime;
        sInst = this;
    }
    /// <summary>
    /// 显示等待
    /// </summary>
    /// <param name="showTime">显示时间，如果是默认参数则需自己关闭</param>
    public static void ShowWait(float showTime = 30)
    {
        if (null != sInst)
        {
            sInst.mShowTime = showTime;
            sInst.mTimes = 0;
        }
        else
        {
            object[] args = new object[1];
            args[0] = showTime;
            PopWinManager.GetInst().ShowPopWin(typeof(NetWaitMsg), args);
        }
    }
    /// <summary>
    /// 关闭等待效果
    /// </summary>
    public static void CloseWait()
    {
        if (null != sInst)
        {
            sInst.OnClose();
        }
    }
    #endregion

    #region 其他函数
    protected override void AddEvent()
    {
        base.AddEvent();
        if (null == mTrans)
        {
            return;
        }
        /*UILabel label = GameHelper.FindChildComponent<UILabel>(mTrans, "Label");
        if (null != label)
        {
            label.text = LauguageTool.GetIns().GetText("LianJieZhong");
        }

        Transform quan = mTrans.Find("quan");
        if (null != quan)
        {
            Transform dian = quan.Find("dian");
            if (null != dian)
            {
                //mSpriteList = new List<UISprite>();
                for (int i = 0; i < Point_Num; ++i)
                {
                    GameObject tmp = GameObject.Instantiate(dian.gameObject) as GameObject;
                    tmp.name = "point" + i;
                    tmp.SetActive(true);
                    tmp.transform.parent = quan;
                    float x = (float)(Math.Sin(i * Math.PI * 2 / Point_Num) * Point_R);
                    float y = (float)(Math.Cos(i * Math.PI * 2 / Point_Num) * Point_R);
                    float eulerZ = -(i + 0.0f) / Point_Num * 360; ;
                    tmp.transform.localScale = Vector3.one;
                    tmp.transform.localPosition = new Vector3(x, y, 0);
                    tmp.transform.localEulerAngles = new Vector3(0, 0, eulerZ);
                    TweenAlpha ta = tmp.GetComponent<TweenAlpha>();
                    ta.delay = (i + 0f) / Point_Num;
                    / *UISprite sp = tmp.GetComponent<UISprite>();
                    if (null != sp)
                    {
                        sp.alpha = (i + 1) / Point_Num;
                        mSpriteList.Add(sp);
                    }* /
                }
            }
        }*/
    }

    protected override void Close()
    {
        base.Close();
        sInst = null;
    }

    public override void Update()
    {
        base.Update();
        mTimes += Time.deltaTime;
        /*if (null != mSpriteList)
        {
            for (int i = 0, imax = mSpriteList.Count; i < imax; ++i)
            {
                float tmp = 0.01f + mSpriteList[i].alpha;
                if (tmp > 1)
                {
                    tmp -= 1;
                }
                mSpriteList[i].alpha = tmp;
            }
        }*/
        if (mShowTime > 0 && mTimes >= mShowTime)
        {
            OnClose();
        }
    }

    public override void Init()
    {
        base.Init();
        SetInitDepth(990);
    }
    #endregion
}