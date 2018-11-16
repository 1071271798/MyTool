using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:UICenterOnChildEx.cs
/// Description:
/// Time:2015/8/17 14:45:16
/// </summary>
[AddComponentMenu("NGUI/Interaction/Center Scroll View on ChildEx")]
public class UICenterOnChildEx : MonoBehaviour
{
    #region 公有属性
    public delegate void OnCenterCallback(ItemDataEx centeredObject);

    public float springStrength = 8;

    public float nextPageThreshold = 0f;

    public SpringPanel.OnFinished onFinished;

    public OnCenterCallback onCenter;

    UIScrollViewEx mScrollView;
    CustomGrid mGrid;
    ItemDataEx mCenteredObject;

    public ItemDataEx centeredObject { get { return mCenteredObject; } }
    #endregion

    #region 私有属性
    void CenterOn(ItemDataEx target, Vector3 panelCenter, bool immediately = false)
    {
        if (target != null && mGrid != null && mScrollView != null && mScrollView.panel != null)
        {
            Transform panelTrans = mScrollView.panel.cachedTransform;
            mCenteredObject = target;

            // Figure out the difference between the chosen child and the panel's center in local coordinates
            Vector3 cp = panelTrans.InverseTransformPoint(mGrid.GetItemPosition(target));
            Vector3 cc = panelTrans.InverseTransformPoint(panelCenter);
            Vector3 localOffset = cp - cc;

            // Offset shouldn't occur if blocked
            if (!mScrollView.canMoveHorizontally) localOffset.x = 0f;
            if (!mScrollView.canMoveVertically) localOffset.y = 0f;
            localOffset.z = 0f;

            // Spring the panel to this calculated position
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                panelTrans.localPosition = panelTrans.localPosition - localOffset;

                Vector4 co = mScrollView.panel.clipOffset;
                co.x += localOffset.x;
                co.y += localOffset.y;
                mScrollView.panel.clipOffset = co;
            }
            else
#endif
            {
                if (immediately)
                {
                    panelTrans.localPosition = panelTrans.localPosition - localOffset;

                    Vector4 co = mScrollView.panel.clipOffset;
                    co.x += localOffset.x;
                    co.y += localOffset.y;
                    mScrollView.panel.clipOffset = co;
                    OnFinished();
                }
                else
                {
                    SpringPanel.Begin(mScrollView.panel.cachedGameObject,
                    panelTrans.localPosition - localOffset, springStrength).onFinished = OnFinished;
                }

            }
        }
        else mCenteredObject = null;

        // Notify the listener
        if (onCenter != null) onCenter(mCenteredObject);
    }

    void OnFinished()
    {
        /*if (null != mCenteredObject)
        {
            mScrollViewEx.CenterOn(mCenteredObject);
        }*/
        if (null != onFinished)
        {
            onFinished();
        }
    }
    #endregion

    #region 公有函数

    public void CenterOn(ItemDataEx target, bool immediately = false)
    {
        if (mScrollView != null && mScrollView.panel != null)
        {
            Vector3[] corners = mScrollView.panel.worldCorners;
            Vector3 panelCenter = (corners[2] + corners[0]) * 0.5f;
            CenterOn(target, panelCenter, immediately);
        }
    }

    [ContextMenu("Execute")]
    public void Recenter()
    {
        if (mScrollView == null)
        {
            mScrollView = NGUITools.FindInParents<UIScrollViewEx>(gameObject);

            if (mScrollView == null)
            {
                Debuger.LogWarning(GetType() + " requires " + typeof(UIScrollView) + " on a parent object in order to work", this);
                enabled = false;
                return;
            }
            else
            {
                mScrollView.onDragFinished = OnDragFinished;

                if (mScrollView.horizontalScrollBar != null)
                    mScrollView.horizontalScrollBar.onDragFinished = OnDragFinished;

                if (mScrollView.verticalScrollBar != null)
                    mScrollView.verticalScrollBar.onDragFinished = OnDragFinished;
            }
        }
        
        if (mScrollView.panel == null) return;

        if (null == mGrid)
        {
            mGrid = NGUITools.FindInParents<CustomGrid>(gameObject);
            if (null == mGrid)
            {
                Debuger.LogWarning(GetType() + " requires " + typeof(CustomGrid) + " on a parent object in order to work", this);
                enabled = false;
                return;
            }
        }

        List<ItemDataEx> items = mGrid.DataList;
        if (null == items || items.Count == 0)
        {
            return;
        }
        //Transform trans = transform;

        // Calculate the panel's center in world coordinates
        Vector3[] corners = mScrollView.panel.worldCorners;
        Vector3 panelCenter = (corners[2] + corners[0]) * 0.5f;

        // Offset this value by the momentum
        Vector3 momentum = mScrollView.currentMomentum * mScrollView.momentumAmount;
        Vector3 moveDelta = NGUIMath.SpringDampen(ref momentum, 9f, 2f);
        Vector3 pickingPoint = panelCenter - moveDelta * 0.05f; // Magic number based on what "feels right"
        mScrollView.currentMomentum = Vector3.zero;

        float min = float.MaxValue;
        ItemDataEx closest = null;
        int index = 0;

        // Determine the closest child
        for (int i = 0, imax = items.Count; i < imax; ++i)
        {
            ItemDataEx t = items[i];
            if (!mGrid.ItemActiveInHierarchy(t)) continue;
            float sqrDist = Vector3.SqrMagnitude(mGrid.GetItemPosition(t) - pickingPoint);

            if (sqrDist < min)
            {
                min = sqrDist;
                closest = t;
                index = i;
            }
        }

        // If we have a touch in progress and the next page threshold set
        if (nextPageThreshold > 0f && UICamera.currentTouch != null)
        {
            // If we're still on the same object
            if (mCenteredObject != null && mCenteredObject == items[index])
            {
                Vector2 totalDelta = UICamera.currentTouch.totalDelta;

                float delta = 0f;

                switch (mScrollView.movement)
                {
                    case UIScrollViewEx.Movement.Horizontal:
                        {
                            delta = totalDelta.x;
                            break;
                        }
                    case UIScrollViewEx.Movement.Vertical:
                        {
                            delta = totalDelta.y;
                            break;
                        }
                    default:
                        {
                            delta = totalDelta.magnitude;
                            break;
                        }
                }

                if (delta > nextPageThreshold)
                {
                    // Next page
                    if (index > 0)
                        closest = items[index - 1];
                }
                else if (delta < -nextPageThreshold)
                {
                    // Previous page
                    if (index < items.Count - 1)
                        closest = items[index + 1];
                }
            }
        }

        CenterOn(closest, panelCenter);
    }

    #endregion

    #region 私有函数
    void OnEnable()
    {
        Recenter();
        if (mScrollView) mScrollView.onDragFinished = OnDragFinished;
    }

    void OnDisable() { if (mScrollView) mScrollView.onDragFinished -= OnDragFinished; }
    void OnDragFinished() { if (enabled) Recenter(); }

    void OnValidate() { nextPageThreshold = Mathf.Abs(nextPageThreshold); }
    #endregion
}