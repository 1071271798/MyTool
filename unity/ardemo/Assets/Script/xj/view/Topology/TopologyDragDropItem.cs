using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Event;
/// <summary>
/// Author:xj
/// FileName:TopologyDragDropItem.cs
/// Description:拓扑图拖拽组件
/// Time:2016/7/22 10:20:55
/// </summary>
public class TopologyDragDropItem : UIDragDropItem
{
    #region 公有属性
    #endregion

    #region 其他属性
    protected ButtonEvent mBtnEvent;
    #endregion

    #region 公有函数
    #endregion

    #region 其他函数
    protected override void Start()
    {
        base.Start();
        mBtnEvent = GetComponent<ButtonEvent>();
    }
    protected override void OnDragDropStart()
    {
        // Automatically disable the scroll view
        if (mDragScrollView != null) mDragScrollView.enabled = false;

        // Disable the collider so that it doesn't intercept events
        if (null != mBtnEvent){ mBtnEvent.enabled = false; }
        if (mButton != null) mButton.isEnabled = false;
        //else if (mCollider != null) mCollider.enabled = false;

        mTouchID = UICamera.currentTouchID;
        mParent = mTrans.parent;
        mRoot = NGUITools.FindInParents<UIRoot>(mParent);
        /*mGrid = NGUITools.FindInParents<UIGrid>(mParent);
        mTable = NGUITools.FindInParents<UITable>(mParent);*/

        // Re-parent the item
        /*if (UIDragDropRoot.root != null)
            mTrans.parent = UIDragDropRoot.root;*/

        Vector3 pos = mTrans.localPosition;
        pos.z = 0f;
        mTrans.localPosition = pos;

        TweenPosition tp = GetComponent<TweenPosition>();
        if (tp != null) tp.enabled = false;

        SpringPosition sp = GetComponent<SpringPosition>();
        if (sp != null) sp.enabled = false;

        // Notify the widgets that the parent has changed
        NGUITools.MarkParentAsChanged(gameObject);

        //if (mTable != null) mTable.repositionNow = true;
        //if (mGrid != null) mGrid.repositionNow = true;
        EventMgr.Inst.Fire(EventID.Item_Drag_Drop_Start, new EventArg(gameObject));
    }

    protected override void OnDragDropRelease(GameObject surface)
    {
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

            this.restriction = UIDragDropItem.Restriction.None;
            // Is there a droppable container?
            /*UIDragDropContainer container = surface ? NGUITools.FindInParents<UIDragDropContainer>(surface) : null;

            if (container != null)
            {
                // Container found -- parent this object to the container
                mTrans.parent = (container.reparentTarget != null) ? container.reparentTarget : container.transform;

                Vector3 pos = mTrans.localPosition;
                pos.z = 0f;
                mTrans.localPosition = pos;
            }
            else
            {
                // No valid container under the mouse -- revert the item's parent
                mTrans.parent = mParent;
            }*/
            
            // Update the grid and table references
            mParent = mTrans.parent;
            //mGrid = NGUITools.FindInParents<UIGrid>(mParent);
            //mTable = NGUITools.FindInParents<UITable>(mParent);

            // Re-enable the drag scroll view script
            /*if (mDragScrollView != null)
                StartCoroutine(EnableDragScrollView());*/

            // Notify the widgets that the parent has changed
            NGUITools.MarkParentAsChanged(gameObject);

            //if (mTable != null) mTable.repositionNow = true;
            //if (mGrid != null) mGrid.repositionNow = true;
        }
        else NGUITools.Destroy(gameObject);
        EventMgr.Inst.Fire(EventID.Item_Drag_Drop_End, new EventArg(gameObject));
    }
    #endregion
}