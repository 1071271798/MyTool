using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ButtonEvent.cs
/// Description:按钮事件
/// Time:2015/7/29 14:36:19
/// </summary>
public class ButtonEvent : MonoBehaviour
{
    #region 公有属性
    public enum PressType
    {
        None,
        PressStart,
        Pressing,
    }

    /// <summary>
    /// 长按的时间
    /// </summary>
    public float pressingTime = 0.5f;
    /// <summary>
    /// 持续按住按钮调用事件的间隔时间
    /// </summary>
    public float intervalTime = 0.1f;
    #endregion

    #region 私有属性
    ButtonDelegate mDelegate;
    PressType mPressType;
    float mNextPressTime;
    GameObject pressObj;
    #endregion

    #region 公有函数
    public void SetDelegate(ButtonDelegate del) 
    {
        mDelegate = del;
        DragdropItemEX dragEX = gameObject.GetComponent<DragdropItemEX>();
        if (dragEX != null)
        {
            dragEX.onDragdropRelease = mDelegate.onDragdropRelease;
            dragEX.onDragdropStart = mDelegate.onDragdropStart;
        }
    }
    public ButtonDelegate GetButtonDelegate()
    {
        return mDelegate;
    }
    #endregion

    #region 私有函数
    void OnEnable()
    {
        
    }

    void Update()
    {
        if (mPressType == PressType.PressStart)
        {
            mNextPressTime -= RealTime.deltaTime;
            if (mNextPressTime <= 0)
            {
                mPressType = PressType.Pressing;
                mNextPressTime = intervalTime;
                if (pressObj != null && mDelegate != null && UICamera.hoveredObject == pressObj)
                {
                    if (mDelegate.onDurationClick != null)
                    {
                        mDelegate.onDurationClick(pressObj);
                    }
                    if (mDelegate.onDurationPress != null)
                    {
                        mDelegate.onDurationPress(pressObj);
                    }
                }
            }
        }
        else if (mPressType == PressType.Pressing)
        {
            mNextPressTime -= RealTime.deltaTime;
            if (mNextPressTime <= 0)
            {
                if (pressObj != null && mDelegate != null && mDelegate.onDurationPress != null && UICamera.hoveredObject == pressObj)
                {
                    mDelegate.onDurationPress(pressObj);
                }
                mNextPressTime = intervalTime;
            }
        }
    }
    void OnPress(bool pressed)
    {
        if (pressed)
        {
            mPressType = PressType.PressStart;
            mNextPressTime = pressingTime;
            pressObj = UICamera.currentTouch.pressed;
        }
        else
        {
            if (mPressType != PressType.Pressing)
            {
                mPressType = PressType.None;
            }
            pressObj = null;
        }
        if (mDelegate != null && mDelegate.onPress != null)
        {
            mDelegate.onPress(this.gameObject, pressed);
        }
    }

    void OnClick()
    {
        if (mDelegate != null && mDelegate.onClick != null)
        {
            mDelegate.onClick(this.gameObject);
        }
        mPressType = PressType.None;
    }

    void OnDrag(Vector2 delta)
    {
        if (mDelegate != null && mDelegate.onDrag != null)
        {
            mDelegate.onDrag(this.gameObject, delta);
        }
    }

    #endregion
}

public class ButtonDelegate
{
    public delegate void OnPress(GameObject obj, bool pressed);
    public delegate void OnClick(GameObject obj);
    public delegate void OnDrag(GameObject obj, Vector2 delta);
    public delegate void OnDurationClick(GameObject obj);
    public delegate void OnDurationPress(GameObject obj);

    public OnPress onPress;
    public OnClick onClick;
    public OnDrag onDrag;
    public OnDurationClick onDurationClick;
    public OnDurationPress onDurationPress;
    public DragdropItemEX.voidDelegate onDragdropRelease;
    public DragdropItemEX.voidDelegate onDragdropStart;
}