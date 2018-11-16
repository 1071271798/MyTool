using UnityEngine;
using System.Collections;
using Game.Event;

public class UIDragDropItem_ex : UIDragDropItem {

    bool _isSetting;
    bool IsSetting
    {
        set
        {
            _isSetting = value;
            enabled = value;
            if (enabled)
            {
                if (transform.parent.name == "Grid")
                {
                    restriction = Restriction.Vertical;
                    UISprite sp = GetComponent<UISprite>();
                    if (sp != null)
                    {
                       // GetComponent<UISprite>().spriteName = "btn_circles";
                        GetComponentInChildren<UILabel>().enabled = true;
                       // sp.SetDimensions(100,100);
                        sp.spriteName = "Button";
                        sp.SetDimensions(100, 100);
                       // sp.MakePixelPerfect();
                    }
                    transform.GetComponent<UIDragScollview_ex>().scrollView = transform.GetComponentInParent<UIScrollView>();
                }
                else
                {
                    UISprite sp = GetComponent<UISprite>();
                    if (sp != null)
                    {
                       // GetComponent<UISprite>().spriteName = "Button";
                        GetComponentInChildren<UILabel>().enabled = false;
                        //sp.SetDimensions(180,180);
                        sp.spriteName = "180";
                        sp.SetDimensions(180, 180);
                        //sp.MakePixelPerfect();
                    }
                    restriction = Restriction.None;
                }
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        IsSetting = true;
        EventMgr.Inst.Regist(EventID.SwitchEdit, OnSettingCallback);
    }

    public static UIDragDropContainer tranP;

    protected override void OnDragDropStart()
    {
        transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
        tranP = NGUITools.FindInParents<UIDragDropContainer>(transform);
        //bg sprite enable
        transform.GetChild(transform.childCount - 2).GetComponent<UISprite>().enabled = true;
        base.OnDragDropStart();
    }

    protected override void OnDragDropMove(Vector3 delta)
    {
        base.OnDragDropMove(delta);
    }

    protected override void OnDragStart()
    {
        base.OnDragStart();
    }

    protected override void OnDragDropRelease(GameObject surface)
    {
        if (!cloneOnDrag)
        {
            mTouchID = int.MinValue;

            // Re-enable the collider
            if (mButton != null) mButton.isEnabled = true;
            else if (mCollider != null) mCollider.enabled = true;

            // Is there a droppable container?
            UIDragDropContainer container = surface ? NGUITools.FindInParents<UIDragDropContainer>(surface) : null;
            
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
            }

            // Update the grid and table references
            mParent = mTrans.parent;
            mGrid = NGUITools.FindInParents<UIGrid>(mParent);
            mTable = NGUITools.FindInParents<UITable>(mParent);

            // Re-enable the drag scroll view script
            if (mDragScrollView != null)
                StartCoroutine(EnableDragScrollView());

            // Notify the widgets that the parent has changed
            NGUITools.MarkParentAsChanged(gameObject);

            if (mTable != null) mTable.repositionNow = true;
            if (mGrid != null) mGrid.repositionNow = true;
            if (mGrid == null && mTable == null)  //没有自动排序时
            {
            }

            if (surface != null && surface.name.Contains("newAction")&&surface.transform.parent.name.Contains("_control"))  //交换
            {
                UISprite sp1 = surface.GetComponent<UISprite>();
                UISprite sp2 = GetComponent<UISprite>();
                if (sp1 != null && sp2 != null)
                {
                    string temName = sp1.spriteName;
                    Vector2 tvec = new Vector2(sp1.width, sp1.height);
                    sp1.spriteName = sp2.spriteName;
                    sp2.spriteName = temName;
                    sp1.SetDimensions(sp2.width, sp2.height);
                    sp2.SetDimensions((int)tvec.x, (int)tvec.y);
                    //sp1.MakePixelPerfect();
                    //sp2.MakePixelPerfect();
                 //   Debug.Log(sp1.spriteName + "uuu"+sp2.spriteName);
                    if (GetComponentInChildren<UILabel>() != null && GetComponentInChildren<UILabel>().enabled && surface.GetComponentInChildren<UILabel>() != null)
                    {
                        surface.GetComponentInChildren<UILabel>().enabled = true;
                    }
                }
                //bg sprite enable disable change
                bool ff = transform.GetChild(transform.childCount - 2).GetComponent<UISprite>().enabled;
                transform.GetChild(transform.childCount - 2).GetComponent<UISprite>().enabled = surface.transform.GetChild(transform.childCount -2).GetComponent<UISprite>().enabled;
                surface.transform.GetChild(transform.childCount - 2).GetComponent<UISprite>().enabled = ff;
            }
        }
        else NGUITools.Destroy(gameObject);
        transform.localScale = Vector3.one;
        if (transform.parent.name == "Grid")   //判断是否有grid
        {
            restriction = Restriction.Vertical;
            UISprite sp = GetComponent<UISprite>();
            if (sp != null)
            {
                //GetComponent<UISprite>().spriteName = "btn_circles";
                GetComponentInChildren<UILabel>().enabled = true;
                sp.spriteName = "Button";
                sp.SetDimensions(100,100);
                //sp.MakePixelPerfect();
            }
            transform.GetComponent<UIDragScollview_ex>().scrollView = transform.GetComponentInParent<UIScrollView>();
            //bg sprite enable
            transform.GetChild(transform.childCount - 2).GetComponent<UISprite>().enabled = true;
        }
        else
        {
            restriction = Restriction.None;
            UISprite sp = GetComponent<UISprite>();
            if (sp != null)
            {
              //  GetComponent<UISprite>().spriteName = "circular";
                GetComponentInChildren<UILabel>().enabled = false;
               // sp.SetDimensions(180, 180);
                sp.spriteName = "180";
                sp.SetDimensions(180, 180);
            }
            transform.GetComponent<UIDragScollview_ex>().scrollView = null;

            //bg sprite disable
            transform.GetChild(transform.childCount - 2).GetComponent<UISprite>().enabled = false;
        }
    }

    void OnDestroy()
    {
        EventMgr.Inst.UnRegist(EventID.SwitchEdit, OnSettingCallback);
    }

    void OnSettingCallback(EventArg arg)
    {
        IsSetting = (bool)arg[0];
    }
	
}
