using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:HUDTextTips.cs
/// Description:
/// Time:2015/8/11 17:28:06
/// </summary>
public class HUDTextTips : BasePopWin
{
    #region 公有属性
    #endregion

    #region 私有属性
    HUDText mHudText;//提示组件实例
    float mTime = 0;
    const int DefaultStayTime = 1;
    const int Close_Time = 10;
    const int FontSize = 25;
    static HUDTextTips sInst;
    float mCloseTime;
    bool mStartCloseFlag;
    
    public static Color Color_Red = new Color(234.0f / 255, 57.0f / 255, 115.0f / 255);
    public static Color Color_Green = new Color(42.0f / 255, 233.0f / 255, 148.0f / 255);
    public static Color Color_Yellow = new Color(1, 183.0f / 255, 59.0f / 255);

    GameObject mItem;

    List<ItemData> mItemList;
    List<ItemData> mUseDataList;

    float Item_Open_Time = 0.3f;
    float Item_Close_Time = 0.3f;

    Vector2 Item_Size = new Vector2(0, 40);

    protected enum ItemState
    {
        State_None,
        State_Open,
        State_Stay,
        State_Close
    }
    protected class ItemData
    {
        public float time;          // Timestamp of when this entry was added
        public float stay = 0f;     // How long the text will appear to stay stationary on the screen
        public UILabel label;		// Label on the game object
        public UISprite bg;
        public Transform trans;
        public MyTweenAlpha tweenAlpha;
        public TweenPosition tweenPos;
        public ItemState state;
        public float movementStart { get { return time + stay; } }

        public ItemData(GameObject obj)
        {
            trans = obj.transform;
            label = GameHelper.FindChildComponent<UILabel>(trans, "Label");
            bg = GameHelper.FindChildComponent<UISprite>(trans, "bg");
            tweenAlpha = obj.GetComponent<MyTweenAlpha>();
            tweenPos = obj.GetComponent<TweenPosition>();
        }
    }
    #endregion

    #region 公有函数
    public HUDTextTips()
    {
        mUIResPath = "Prefab/UI/hudTextTips";
        sInst = this;
        mCloseTime = 0;
        mStartCloseFlag = false;
        mItemList = new List<ItemData>();
        mUseDataList = new List<ItemData>();
    }

    protected override void Close()
    {
        base.Close();
        sInst = null;
    }

    

    //intervalTime 间隔几秒弹出提示
    private static void ShowTextTip(string tiptext, Color color, float staytime, int fontSize, float intervalTime)
    {
        if (null == sInst)
        {
            PopWinManager.GetInst().ShowPopWin(typeof(HUDTextTips));
        }
        if (null != sInst)
        {
            if (sInst.mTime != 0 && Time.time - sInst.mTime < intervalTime)
            {
                return;
            }
            sInst.ShowTip(tiptext, color, staytime, fontSize);
        }
    }

    /// <summary>
    /// 显示文本提示,默认文本颜色为红色，停留时间为0
    /// </summary>
    /// <param name="tiptext">提示的内容</param>
    public static void ShowTextTip(string tiptext)
    {
        ShowTextTip(tiptext, Color_Red, DefaultStayTime, FontSize, 0);
    }
    /// <summary>
    /// 显示文本提示，默认文本颜色为红色，停留时间为0
    /// </summary>
    /// <param name="tiptext"></param>
    /// <param name="intervalTime">多少秒执行一次</param>
    public static void ShowTextTip(string tiptext, float intervalTime)
    {
        ShowTextTip(tiptext, Color_Red, DefaultStayTime, FontSize, intervalTime);
    }

    public static void ShowTextTip(string tiptext, Color color, float intervalTime)
    {
        ShowTextTip(tiptext, color, DefaultStayTime, FontSize, intervalTime);
    }
    /// <summary>
    /// 显示文本提示,停留时间为0
    /// </summary>
    /// <param name="tiptext"></param>
    /// <param name="color"></param>
    public static void ShowTextTip(string tiptext, Color color)
    {
        ShowTextTip(tiptext, color, DefaultStayTime, FontSize, 0);
    }

    
    #endregion
    protected override void AddEvent()
    {
        base.AddEvent();
        if (null == mTrans)
        {
            return;
        }
        Transform hudGameOjbect = mTrans.Find("tips/hudtext");
        if (null != hudGameOjbect)
        {
            mHudText = hudGameOjbect.GetComponent<HUDText>();
        }

        if (null != mPanel && null != hudGameOjbect)
        {
            Transform tips = mPanel.transform.Find("tips");
            if (null != tips)
            {
                //tips.localPosition += new Vector3(0, 23, 0);
                //float y = hudGameOjbect.localPosition.y - tips.localPosition.y + 26;
                //mPanel.clipping = UIDrawCall.Clipping.SoftClip;
                Vector4 rect = mPanel.finalClipRegion;
                rect.z = PublicFunction.GetExtendWidth();
                mPanel.baseClipRegion = rect;

                mItem = tips.Find("item").gameObject;
                UISprite sp = GameHelper.FindChildComponent<UISprite>(mItem.transform, "bg");
                if (null != sp)
                {
                    sp.width = PublicFunction.GetExtendWidth() + 8;
                    sp.height = (int)Item_Size.y;
                }
            }
            
        }
    }

    public override void Init()
    {
        base.Init();
        mAddBox = false;
        mCoverAlpha = 0;
        mSide = UIAnchor.Side.Top;
        SetInitDepth(999);
    }

    public override void Update()
    {
        base.Update();
        if (mStartCloseFlag)
        {
            mCloseTime += Time.deltaTime;
            if (mCloseTime > Close_Time)
            {
                OnClose();
            }
        }

        for (int i = 0, imax = mItemList.Count; i < imax; ++i)
        {
            mItemList[i].time += Time.deltaTime;
            if (mItemList[i].state == ItemState.State_Open)
            {
                if (mItemList[i].time >= Item_Open_Time)
                {
                    mItemList[i].state = ItemState.State_Stay;
                }
            }
            else if (mItemList[i].state == ItemState.State_Stay)
            {
                if (mItemList[i].time >= mItemList[i].stay + Item_Open_Time)
                {
                    mItemList[i].state = ItemState.State_Close;
                    GameHelper.PlayMyTweenAlpha(mItemList[i].tweenAlpha, 0, Item_Close_Time);
                    GameHelper.PlayTweenPosition(mItemList[i].tweenPos, new Vector3(0, Item_Size.y / 2), Item_Close_Time);
                }
            }
            else if (mItemList[i].state == ItemState.State_Close)
            {
                if (mItemList[i].time >= mItemList[i].stay + Item_Open_Time + Item_Close_Time)
                {
                    mItemList[i].state = ItemState.State_None;
                    Delete(mItemList[i]);
                    --i;
                    --imax;
                }
            }
            
        }
    }

    ItemData Create()
    {
        // See if an unused entry can be reused
        if (mUseDataList.Count > 0)
        {
            ItemData ent = mUseDataList[mUseDataList.Count - 1];
            mUseDataList.RemoveAt(mUseDataList.Count - 1);
            ent.time = 0;
            ent.bg.depth = NGUITools.CalculateNextDepth(mTrans.gameObject);
            ent.label.depth = ent.bg.depth + 1;
            ent.trans.gameObject.SetActive(true);
            mItemList.Add(ent);
            return ent;
        }

        // New entry
        GameObject obj = GameObject.Instantiate(mItem) as GameObject;
        ItemData ne = new ItemData(obj);
        ne.time = 0;
        ne.bg.depth = NGUITools.CalculateNextDepth(mTrans.gameObject);
        ne.label.depth = ne.bg.depth + 1;
        ne.trans.parent = mItem.transform.parent;
        ne.trans.localScale = Vector3.one;
        ne.trans.gameObject.SetActive(true);

        mItemList.Add(ne);
        return ne;
    }

    void Delete(ItemData ent)
    {
        mItemList.Remove(ent);
        mUseDataList.Add(ent);
        ent.trans.gameObject.SetActive(false);

        if (mItemList.Count < 1)
        {
            OnFinish();
        }
    }

    void Add(string text, Color c, float stayDuration)
    {
        if (null == mTrans || !mTrans.gameObject.activeSelf) return;
        mStartCloseFlag = false;
        // Create a new entry
        ItemData ne = Create();
        ne.state = ItemState.State_Open;
        ne.stay = stayDuration;
        ne.label.color = Color.white;
        ne.label.alpha = 0f;
        ne.bg.color = c;
        ne.bg.alpha = 0f;
        ne.label.text = text;
        ne.trans.localPosition = new Vector3(0, Item_Size.y / 2);
        ne.time = 0;

        GameHelper.PlayTweenPosition(ne.tweenPos, new Vector3(0, -Item_Size.y / 2 + 3), Item_Open_Time);
        GameHelper.PlayMyTweenAlpha(ne.tweenAlpha, 1, Item_Open_Time);
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "HUDTextTips Show Text :" + text);
    }

    #region 私有函数
    /// <summary>
    /// 显示文本提示
    /// </summary>
    /// <param name="tiptext">提示的内容</param>
    /// <param name="color">提示文本的颜色</param>
    /// <param name="staytime">提示停留时间(该停留时间表示文本scale 变大后，停留时间，并非整个过程的时间)</param>
    void ShowTip(string tiptext, Color color, float staytime, int fontSize)
    {
        mTime = Time.time;
        /*if (null != mHudText)
        {
            mHudText.fontSize = FontSize;
            mHudText.Add(tiptext, color, staytime);
            mHudText.onShowFinish = OnFinish;
        }*/
        Add(tiptext, color, staytime);
    }

    void OnFinish()
    {
        mStartCloseFlag = true;
        mCloseTime = 0;
    }

    #endregion
}