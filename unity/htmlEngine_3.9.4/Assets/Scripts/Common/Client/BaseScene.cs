using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:BaseScene.cs
/// Description:
/// Time:2015/7/21 14:17:28
/// </summary>
public abstract class BaseScene
{
    #region 公有属性
    
    #endregion

    #region 私有属性
    #endregion

    #region 保护属性
    protected bool isOpen = false;
    protected bool isFirstOpen = true;
    protected bool isInited = false;
    protected string mResPath;
    protected List<BaseUI> mUIList;
    protected Transform mTrans;
    protected ButtonDelegate mBtnDelegate;
    #endregion

    #region 公有函数
    public BaseScene(string resPath)
    {
        mResPath = resPath;
    }

    public BaseScene()
    {
        mResPath = string.Empty;
    }

    public virtual void Open()
    {
        Debug.Log("Open Scene = " + this.GetType().ToString());
        if (isOpen)
        {
            return;
        }
        isOpen = true;
        LoadScene();

        if (isFirstOpen)
        {
            FirstOpen();
        }
        isFirstOpen = false;
        isInited = false;
        UpdateScene();
    }

    public virtual void Close()
    {
        Debug.Log("Close Scene = " + this.GetType().ToString());
        if (!isOpen)
        {
            return;
        }
        isOpen = false;
        Release();
        if (null != mTrans)
        {
            UnityEngine.Object.Destroy(mTrans.gameObject);
            mTrans = null;
        }
        if (null != mUIList)
        {
            for (int i = 0, icount = mUIList.Count; i < icount; ++i)
            {
                SingletonObject<UIManager>.GetInst().CloseUI(mUIList[i]);
            }
            mUIList.Clear();
        }
    }

    public virtual void Release()
    {
        
    }

    public virtual void Update()
    {
        if (!isOpen)
        {
            return;
        }
        if (null != mUIList)
        {
            for (int i = 0, icount = mUIList.Count; i < icount; ++i)
            {
                mUIList[i].Update();
            }
        }
    }

    public virtual void LateUpdate()
    {
        if (!isOpen)
        {
            return;
        }
        if (null != mUIList)
        {
            for (int i = 0, icount = mUIList.Count; i < icount; ++i)
            {
                mUIList[i].LateUpdate();
            }
        }
    }

    public virtual void LoadScene()
    {
        if (string.IsNullOrEmpty(mResPath))
        {
            return;
        }
        GameObject obj = Resources.Load(mResPath, typeof(GameObject)) as GameObject;
        if (null != obj)
        {
            GameObject o = UnityEngine.Object.Instantiate(obj) as GameObject;
            o.name = obj.name;
            mTrans = o.transform;

            if (!string.IsNullOrEmpty(mResPath))
            {
                SetUICamera();

                mBtnDelegate = new ButtonDelegate();
                mBtnDelegate.onClick = OnButtonClick;
                mBtnDelegate.onDrag = OnButtonDrag;
                mBtnDelegate.onPress = OnButtonPress;
                UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
            }


        }
    }

    public virtual void FirstOpen()
    {
         
    }

    public virtual void UpdateScene()
    {
        if (!isOpen)
        {
            return;
        }
        if (null != mUIList)
        {
            for (int i = 0, icount = mUIList.Count; i < icount; ++i)
            {
                mUIList[i].UpdateUI();
            }
        }
    }
    #endregion

    #region 保护函数
    /// <summary>
    /// 如果场景预设为ui则可重载此函数，确定ui的锚点和摄像机
    /// </summary>
    protected virtual void SetUICamera()
    {
        SingletonObject<UIManager>.Inst.InitUI(UIAnchor.Side.Center, eUICameraType.OrthographicOne, mTrans);
    }

    protected virtual void OnButtonClick(GameObject obj)
    {

    }

    protected virtual void OnButtonDrag(GameObject obj, Vector2 delta)
    {

    }

    protected virtual void OnButtonPress(GameObject obj, bool press)
    {

    }
    #endregion

    #region 私有函数
    #endregion
}