using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:BaseUI.cs
/// Description:
/// Time:2015/7/21 13:09:08
/// </summary>
public abstract class BaseUI
{
    public enum eBaseUIState
    {
        Open,
        Close,
        Loding,
        NotLoad
    }

    #region 公有属性
    public bool IsOpen
    {
        get { return isOpen; }
    }
    public bool IsShow
    {
        get { return isShow; }
    }
    #endregion

    #region 保护属性
    protected bool isOpen = false;
    protected bool isShow = false;
    protected bool isFirstOpen = true;
    protected bool isInited = false;
    protected eBaseUIState mState = eBaseUIState.NotLoad;
    protected string mUIResPath;
    protected Transform mTrans;
    protected UIAnchor.Side mSide;
    protected eUICameraType mUICameraType;
    protected UIPanel mPanel;
    protected ButtonDelegate mBtnDelegate;
    protected ButtonDelegate mSceneDelegate;

    protected Dictionary<Transform, EventDelegate.Callback> mScreenResizeDict;

    protected Transform mParentTrans;
    #endregion

    #region 私有属性
    #endregion

    #region 公有函数
    public BaseUI(string resPath)
    {
        mUIResPath = resPath;
    }

    public BaseUI()
    {

    }
    /// <summary>
    /// 初始化参数，在加载之前调用
    /// </summary>
    public virtual void Init()
    {
        mUICameraType = eUICameraType.OrthographicOne;
        mSide = UIAnchor.Side.Center;
    }
    public virtual void Open()
    {
        isOpen = true;
        isShow = true;
        if (eBaseUIState.NotLoad == mState)
        {
            Init();
            LoadUI();
            DirectOpen();
        }
        else
        {
            DirectOpen();
        }
        SingletonObject<UIManager>.GetInst().OpenUI(this);
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent,  " Open UI name =" + this.GetType().ToString());
    }
    /// <summary>
    /// 用于把按钮事件传入所属Scene
    /// </summary>
    /// <param name="sceneDlgt"></param>
    public virtual void Open(ButtonDelegate sceneDlgt)
    {
        mSceneDelegate = sceneDlgt;
        Open();
    }

    public void OnClose()
    {
        if (!isOpen)
        {
            return;
        }
        isOpen = false;
        mState = eBaseUIState.NotLoad;
        isInited = false;
        Close();
        Release();
        if (null != mTrans)
        {
            SingletonObject<UIManager>.GetInst().AddDestroyObj(mTrans.gameObject);
            mTrans = null;
        }
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "OnClose UI name =" + this.GetType().ToString());
    }
    
    public virtual void Release()
    {

    }
    /// <summary>
    /// 更新UI，每次打开都会调用
    /// </summary>
    public virtual void UpdateUI()
    {

    }

    public virtual void Update()
    {

    }
    public virtual void LateUpdate()
    {

    }
    public virtual void LoadUI()
    {
        if (string.IsNullOrEmpty(mUIResPath))
        {
            return;
        }
        mState = eBaseUIState.Loding;
        GameObject obj = Resources.Load(mUIResPath, typeof(GameObject)) as GameObject;
        if (null != obj)
        {
            GameObject o = UnityEngine.Object.Instantiate(obj) as GameObject;
            o.name = obj.name;
            mTrans = o.transform;
            mPanel = o.GetComponent<UIPanel>();
            if (null == mPanel)
            {
                mPanel = o.AddComponent<UIPanel>();
            }
            if (null == mParentTrans)
            {
                mParentTrans = SingletonObject<UIManager>.Inst.InitUI(mSide, mUICameraType, mTrans);
                
            } else
            {
                mTrans.parent = mParentTrans;
                mTrans.localPosition = Vector3.zero;
                mTrans.localScale = Vector3.one;
                mTrans.localEulerAngles = Vector3.zero;
                PublicFunction.SetLayerRecursively(o, mParentTrans.gameObject.layer);
            }
            mBtnDelegate = new ButtonDelegate();
            mBtnDelegate.onClick = OnButtonClick;
            mBtnDelegate.onDrag = OnButtonDrag;
            mBtnDelegate.onPress = OnButtonPress;
            mBtnDelegate.onDurationClick = OnDurationClick;
            mBtnDelegate.onDurationPress = OnDurationPress;
            mBtnDelegate.onDragdropRelease = OnDragdropRelease;
            mBtnDelegate.onDragdropStart = OnDragdropStart;
            if (null != mSceneDelegate)
            {
                UIManager.SetButtonEventDelegate(mTrans, mSceneDelegate);
            }
            else
            {
                UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
            }
        }
    }
    /// <summary>
    /// 显示页面
    /// </summary>
    public virtual void OnShow()
    {
        isShow = true;
        if (null != mTrans)
        {
            mTrans.gameObject.SetActive(isShow);
        }
    }
    /// <summary>
    /// 隐藏页面
    /// </summary>
    public virtual void OnHide()
    {
        isShow = false;
        if (null != mTrans)
        {
            mTrans.gameObject.SetActive(isShow);
        }
    }

    public void AddScreenResizeEvent(Transform trans, EventDelegate.Callback callback)
    {
        if (null == mScreenResizeDict)
        {
            mScreenResizeDict = new Dictionary<Transform, EventDelegate.Callback>();
        }
        mScreenResizeDict[trans] = callback;
        if (null != callback)
        {
            callback();
        }
    }

    public void RemoveScreenResizeEvent(Transform trans)
    {
        if (null != mScreenResizeDict)
        {
            mScreenResizeDict.Remove(trans);
        }
    }

    public void OnScreenResize()
    {
        try
        {
            if (null != mScreenResizeDict)
            {
                foreach (var kvp in mScreenResizeDict)
                {
                    if (null != kvp.Value)
                    {
                        kvp.Value();
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, ex.ToString());
        }
        
    }
    #endregion

    #region 保护函数
    /// <summary>
    /// 初始化的时候调用，只调用一次
    /// </summary>
    protected virtual void AddEvent()
    {

    }
    /// <summary>
    /// 第一次打开界面的时候调用，只调用一次
    /// </summary>
    protected virtual void FirstOpen()
    {

    }

    protected virtual void Close()
    {
        
    }
    #endregion
    /// <summary>
    /// 按钮点击事件
    /// </summary>
    /// <param name="obj"></param>
    protected virtual void OnButtonClick(GameObject obj)
    {
        if (null != obj)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, this.GetType().ToString() + " OnClick name =" + obj.name);
        }
    }

    protected virtual void OnButtonDrag(GameObject obj, Vector2 delta)
    {

    }

    protected virtual void OnButtonPress(GameObject obj, bool press)
    {

    }
    /// <summary>
    /// 长按执行一次
    /// </summary>
    /// <param name="obj"></param>
    protected virtual void OnDurationClick(GameObject obj)
    {

    }
    /// <summary>
    /// 长按持续执行
    /// </summary>
    /// <param name="obj"></param>
    protected virtual void OnDurationPress(GameObject obj)
    {

    }
    /// <summary>
    /// 开始拖动
    /// </summary>
    /// <param name="obj"></param>
    protected virtual void OnDragdropStart(GameObject obj)
    { 
    }
    /// <summary>
    /// 拖动结束
    /// </summary>
    protected virtual void OnDragdropRelease(GameObject obj)
    { }

    #region 私有函数
    private void DirectOpen()
    {
        mState = eBaseUIState.Open;

        if (!isInited)
        {
            AddEvent();
        }
        if (isFirstOpen)
        {
            FirstOpen();
        }
        isFirstOpen = false;
        isInited = true;
        UpdateUI();
    }
    #endregion
}