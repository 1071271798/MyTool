using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class DragdropItemEX : UIDragDropItem {
    public delegate void voidDelegate(GameObject obj);
    public voidDelegate onDragdropStart;
    public voidDelegate onDragdropRelease;
    public voidDelegate onPress;
    //public bool PressSuccess;
    public UISprite leftKit;

    public enum ControllerType
    {
        None,
        ActionWidget,
        JoystickWidget,
        VsliderWidget,
        HsliderWidget,
    }

    public ControllerType controllerType = ControllerType.None;
    //public EventDelegate.Callback 

    protected override void OnDragDropStart()
    {
        // Automatically disable the scroll view
        if (mDragScrollView != null) mDragScrollView.enabled = false;

        // Disable the collider so that it doesn't intercept events
        //if (mButton != null) mButton.isEnabled = false;
        //else if (mCollider != null) mCollider.enabled = false;

        mTouchID = UICamera.currentTouchID;
        mParent = mTrans.parent;
        mRoot = NGUITools.FindInParents<UIRoot>(mParent);
        mGrid = NGUITools.FindInParents<UIGrid>(mParent);
        mTable = NGUITools.FindInParents<UITable>(mParent);

        // Re-parent the item
        if (UIDragDropRoot.root != null)
            mTrans.parent = UIDragDropRoot.root;

        Vector3 pos = mTrans.localPosition;
        pos.z = 0f;
        mTrans.localPosition = pos;

        TweenPosition tp = GetComponent<TweenPosition>();
        if (tp != null) tp.enabled = false;

        SpringPosition sp = GetComponent<SpringPosition>();
        if (sp != null) sp.enabled = false;

        // Notify the widgets that the parent has changed
        NGUITools.MarkParentAsChanged(gameObject);

        if (mTable != null) mTable.repositionNow = true;
        if (mGrid != null) mGrid.repositionNow = true;
        if (onDragdropStart != null)
        {
            onDragdropStart(gameObject);
        }
    }

    protected override void OnPress(bool isPressed)
    {
        if (isPressed)
        {
            if (restriction == Restriction.PressAndHold)
            {
                //Debug.Log("可以拖动");
                if (controllerType == ControllerType.ActionWidget || controllerType == ControllerType.JoystickWidget || controllerType == ControllerType.VsliderWidget || controllerType == ControllerType.HsliderWidget)
                {
                    if (leftKit != null)
                        leftKit.enabled = true;
                }
            }
        }
        else
        {
            if (restriction == Restriction.PressAndHold)
            {
                //Debug.Log("不可拖动");
                if (controllerType == ControllerType.ActionWidget || controllerType == ControllerType.JoystickWidget || controllerType == ControllerType.VsliderWidget || controllerType == ControllerType.HsliderWidget)
                {
                    if (leftKit != null)
                        leftKit.enabled = false;
                }
            }
        }
    }

    protected override void OnDragDropRelease(GameObject surface)
    {
        if (!cloneOnDrag)
        {
         //   base.OnDragDropStart();
        }
        //base.OnDragDropRelease(surface);
        else
        {

        }
        if (onDragdropRelease != null)
        {
            onDragdropRelease(gameObject);
        }
    }

    protected override void OnDragStart()
    {
        if (!enabled) return;//|| mTouchID != int.MinValue) return;
        // If we have a restriction, check to see if its condition has been met first
        if (restriction != Restriction.None)
        {
            if (restriction == Restriction.Horizontal)
            {
                Vector2 delta = UICamera.currentTouch.totalDelta;
                if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y)) return;
            }
            else if (restriction == Restriction.Vertical)
            {
                Vector2 delta = UICamera.currentTouch.totalDelta;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) return;
            }
            else if (restriction == Restriction.PressAndHold)
            {
                if (mPressTime + pressAndHoldDelay > RealTime.time)  // 拖动需要持续多长时间
                {
                    return;
                }
            }
        }
        if (cloneOnDrag)
        {
            //Debug.Log("正在拖动！！");
            GameObject clone = NGUITools.AddChild(transform.parent.gameObject, gameObject);
            clone.transform.localPosition = transform.localPosition;
            clone.transform.localRotation = transform.localRotation;
            clone.transform.localScale = transform.localScale;

            UIButtonColor bc = clone.GetComponent<UIButtonColor>();
            if (bc != null) bc.defaultColor = GetComponent<UIButtonColor>().defaultColor;

            UICamera.currentTouch.dragged = clone;

            DragdropItemEX item = clone.GetComponent<DragdropItemEX>();
            item.Start();
            if (item.GetComponent<ButtonEvent>() != null)   //事件关系
                item.GetComponent<ButtonEvent>().SetDelegate(this.GetComponent<ButtonEvent>().GetButtonDelegate());
            //if (onDragdropStart != null)
            //    item.onDragdropStart = this.onDragdropStart;
            //if (onDragdropRelease != null)
            //    item.onDragdropRelease = this.onDragdropRelease;
            item.OnDragDropStart();
        }
        else OnDragDropStart();
    }

    
}
