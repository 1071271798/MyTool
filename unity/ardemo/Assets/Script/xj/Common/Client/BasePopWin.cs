using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:BasePopWin.cs
/// Description:弹出窗口基类
/// Time:2015/7/27 9:41:43
/// </summary>
public abstract class BasePopWin : BaseUI
{
    #region 公有属性
    public PopWinBtnDelegate btnClick;
    public float mCoverAlpha;
    public Color mCoverColor;
    public bool mAddBox;
    public bool isSingle;//只可以创建一个实例
    public int mDepth;
    public bool isCoverAddPanel;//是否需要给Cover添加panel
    #endregion

    #region 私有属性
    float mShowTime;
    protected int mInitDepth;
    #endregion

    #region 公有函数
    public BasePopWin()
    {
        btnClick = null;
        mCoverAlpha = 0;
        mAddBox = true;
        mDepth = 0;
        isSingle = false;
        mInitDepth = 0;
        isCoverAddPanel = false;
        mCoverColor = PublicFunction.PopWinColor_Default;
    }
    public override void Init()
    {
        base.Init();
        mSide = UIAnchor.Side.Center;
        mUICameraType = eUICameraType.OrthographicTwo;
        mCoverAlpha = PublicFunction.PopWin_Alpha;
        mAddBox = true;
    }

    public virtual void FixedUpdate()
    {

    }

    public override void LoadUI()
    {
        base.LoadUI();
        if (null != mTrans)
        {
            if (mCoverAlpha > 0.01f)
            {//添加Cover
                UIManager.AddCover(mTrans, mCoverAlpha, mCoverColor, isCoverAddPanel, this);
            }
            if (mAddBox)
            {//添加
                UIManager.AddBox(mTrans, this);
            }
            if (null != mPanel)
            {
                if (0 != mInitDepth)
                {
                    mPanel.depth = mInitDepth;
                }
                else
                {
                    mPanel.depth = mDepth;
                }
                if (isCoverAddPanel)
                {
                    UIPanel panel = GameHelper.FindChildComponent<UIPanel>(mTrans, "Cover");
                    if (null != panel)
                    {
                        panel.depth = mPanel.depth - 2;
                    }
                }
            }
        }
        
    }

    protected override void Close()
    {
        base.Close();
        PopWinManager.Inst.RemovePopWin(this);
    }

    public void SetDepth(int depth)
    {
        mDepth = depth;
    }

    protected void SetInitDepth(int depth)
    {
        mInitDepth = depth;
    }


    #endregion

    #region 私有函数
    #endregion
}
/// <summary>
/// 按钮点击委托
/// </summary>
/// <param name="btnType"></param>
public delegate void PopWinBtnDelegate(PopBtnType btnType);

public enum PopBtnType
{
    Pop_Btn_First,
    Pop_Btn_Second,
    Pop_Btn_Third
}