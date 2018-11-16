using Game.Event;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
/// <summary>
/// Author:xj
/// FileName:ServoDragDropItem.cs
/// Description:舵机拖动组件
/// Time:2017/1/13 15:38:39
/// </summary>
public class ServoDragDropItem : UIDragDropItem
{
    #region 公有属性
    #endregion

    #region 其他属性
    protected ButtonEvent mBtnEvent;
    long mHoldExeIndex = -1;
    bool mHoldFlag = false;
    bool mMoveFlag = false;
    #endregion

    #region 公有函数
    #endregion

    #region 其他函数
    protected override void Start()
    {
        base.Start();
        mBtnEvent = GetComponent<ButtonEvent>();
        pressAndHoldDelay = 0.2f;
        mHoldFlag = false;
        mMoveFlag = false;
    }

    protected override void OnPress(bool isPressed)
    {
        base.OnPress(isPressed);
        EventMgr.Inst.Fire(EventID.Servo_Press);
        if (isPressed && !mHoldFlag)
        {
            if (-1 != mHoldExeIndex)
            {
                Timer.Cancel(mHoldExeIndex);
            }
            mHoldFlag = true;
            mMoveFlag = false;
            mHoldExeIndex = Timer.Add(pressAndHoldDelay, 1, 1, delegate () {
                mHoldExeIndex = -1;
                EventMgr.Inst.Fire(EventID.Servo_Press_Hold, new EventArg(gameObject));
            });
        }
        else
        {
            if (-1 == mHoldExeIndex && !mMoveFlag)
            {
                EventMgr.Inst.Fire(EventID.Servo_Press_Hold_Recover, new EventArg(gameObject));
            }
            mMoveFlag = false;
            mHoldFlag = false;
            if (-1 != mHoldExeIndex)
            {
                Timer.Cancel(mHoldExeIndex);
                mHoldExeIndex = -1;
            }
        }
    }

    protected override void OnDragDropStart()
    {
        mMoveFlag = true;
        // Automatically disable the scroll view
        if (mDragScrollView != null) mDragScrollView.enabled = false;

        // Disable the collider so that it doesn't intercept events
        if (null != mBtnEvent) { mBtnEvent.enabled = false; }
        if (mButton != null) mButton.isEnabled = false;
        //else if (mCollider != null) mCollider.enabled = false;

        mTouchID = UICamera.currentTouchID;
        mParent = mTrans.parent;
        mRoot = NGUITools.FindInParents<UIRoot>(mParent);
        
        // Re-parent the item
        Vector3 pos = mTrans.localPosition;
        pos.z = 0f;
        mTrans.localPosition = pos;

        TweenPosition tp = GetComponent<TweenPosition>();
        if (tp != null) tp.enabled = false;

        SpringPosition sp = GetComponent<SpringPosition>();
        if (sp != null) sp.enabled = false;

        // Notify the widgets that the parent has changed
        NGUITools.MarkParentAsChanged(gameObject);

        EventMgr.Inst.Fire(EventID.Servo_Drag_Drop_Start, new EventArg(gameObject));
    }

    protected override void OnDragDropRelease(GameObject surface)
    {
        mHoldFlag = false;
        if (!cloneOnDrag)
        {
            mTouchID = int.MinValue;

            if (null != mBtnEvent)
            {
                mBtnEvent.enabled = true;
            }
            // Re-enable the collider
            if (mButton != null) mButton.isEnabled = true;
            //else if (mCollider != null) mCollider.enabled = true;
            
            // Update the grid and table references
            mParent = mTrans.parent;
            
            // Notify the widgets that the parent has changed
            NGUITools.MarkParentAsChanged(gameObject);
        }
        else NGUITools.Destroy(gameObject);
        EventMgr.Inst.Fire(EventID.Servo_Drag_Drop_End, new EventArg(gameObject));
    }
    #endregion
}