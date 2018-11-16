using UnityEngine;
using System.Collections;

/// <summary>
/// 翻书的效果实现
/// spriteName1,spriteName2代表下方的点状态
/// 节点下面只需要挂页面内容就ok了 
/// </summary>
public class UIScrollview_likeBook : UIScrollView {

    public Transform indexsTrans;
    public Transform indexLines;
    private string spriteName1;
    private string spriteName2;
    private int Curindex = 1;
    
    void Start()
    {
        spriteName1 = "Rectangle 1";
        spriteName2 = "Rectangle 2";
       // PublicPrompt.ShowChargePrompt(AutoSaveActions);
        for (int i = 0; i < transform.childCount; i++)
        {
            GetTCompent.GetCompent<UIWidget>(transform.GetChild(i)).SetDimensions((int)GetComponent<UIPanel>().GetViewSize().x, (int)GetComponent<UIPanel>().GetViewSize().y);
            GetTCompent.GetCompent<UIWidget>(transform.GetChild(i)).autoResizeBoxCollider = true;
            GetTCompent.AddCompent<BoxCollider>(transform.GetChild(i));
            GetTCompent.AddCompent<UIDragScrollView>(transform.GetChild(i));
        }
       // UICenterOnChild.OnCenterCallback
        UICenterOnChild center = transform.GetComponent<UICenterOnChild>();
        if (center != null)
        {
            center.onCenter = new UICenterOnChild.OnCenterCallback(GetCenterChildName);
        }
      //  InitLinesSize();
    }
    /// <summary>
    /// center 回调
    /// </summary>
    /// <param name="obj"></param>
    void GetCenterChildName(GameObject obj)
    {
        indexsTrans.GetChild(Curindex - 1).GetComponent<UISprite>().spriteName = spriteName1;
        if(Curindex < 7 && Curindex > 1)
            indexLines.GetChild(Curindex -2).GetComponent<UISprite>().enabled = false;
        Curindex = int.Parse(obj.name);
        indexsTrans.GetChild(Curindex - 1).GetComponent<UISprite>().spriteName = spriteName2;
        if(Curindex < 7 && Curindex > 1)
            indexLines.GetChild(Curindex - 2).GetComponent<UISprite>().enabled = true;
    }

    public override void Drag()
    {
        if (closeEvent)
        {
            return;
        }
        if (enabled && NGUITools.GetActive(gameObject) && mShouldMove)
        {
            if (mDragID == -10) mDragID = UICamera.currentTouchID;
            UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

            // Prevents the drag "jump". Contributed by 'mixd' from the Tasharen forums.
            if (smoothDragStart && !mDragStarted)
            {
                mDragStarted = true;
                mDragStartOffset = UICamera.currentTouch.totalDelta;
                if (onDragStarted != null)
                {
                    onDragStarted();
                    //CamRotateAroundCircle._instance.canChangeView = false;
                }
            }

            Ray ray = smoothDragStart ?
                UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos - mDragStartOffset) :
                UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);

            float dist = 0f;

            if (mPlane.Raycast(ray, out dist))
            {
                Vector3 currentPos = ray.GetPoint(dist);
                Vector3 offset = currentPos - mLastPos;
                mLastPos = currentPos;

                if (offset.x != 0f || offset.y != 0f || offset.z != 0f)
                {
                    offset = mTrans.InverseTransformDirection(offset);

                    if (movement == Movement.Horizontal)
                    {
                        offset.y = 0f;
                        offset.z = 0f;
                    }
                    else if (movement == Movement.Vertical)
                    {
                        offset.x = 0f;
                        offset.z = 0f;
                    }
                    else if (movement == Movement.Unrestricted)
                    {
                        offset.z = 0f;
                    }
                    else
                    {
                        offset.Scale((Vector3)customMovement);
                    }
                    offset = mTrans.TransformDirection(offset);
                }
                if (openDragFactor)
                {
                    if (movement == Movement.Horizontal)
                    {
                        dragFactor = Mathf.Abs(offset.x) / mTrans.lossyScale.x * dragSpeed;
                    }
                    else if (movement == Movement.Vertical)
                    {
                        dragFactor = Mathf.Abs(offset.y) / mTrans.lossyScale.x * dragSpeed;
                    }
                    if (dragFactor > 1.0f)
                    {
                        if (dragFactor > 5.0f)
                        {
                            dragFactor = 5.0f;
                        }
                        offset *= dragFactor;
                    }
                }
                // Adjust the momentum
                if (dragEffect == DragEffect.None) mMomentum = Vector3.zero;
                else mMomentum = Vector3.Lerp(mMomentum, mMomentum + offset * (0.01f * momentumAmount), 0.67f);

                // Move the scroll view
                if (!iOSDragEmulation || dragEffect != DragEffect.MomentumAndSpring)
                {
                    MoveAbsolute(offset);
                }
                else
                {
                    Vector3 constraint = mPanel.CalculateConstrainOffset(bounds.min, bounds.max);

                    if (constraint.magnitude > 1f)
                    {
                        MoveAbsolute(offset * 0.5f);
                        mMomentum *= 0.5f;
                    }
                    else
                    {
                        MoveAbsolute(offset);
                    }
                }

                // We want to constrain the UI to be within bounds
                if (restrictWithinPanel &&
                    mPanel.clipping != UIDrawCall.Clipping.None &&
                    dragEffect != DragEffect.MomentumAndSpring)
                {
                    RestrictWithinBounds(true, canMoveHorizontally, canMoveVertically);
                }
            }
        }
        //base.Drag();
    }
}
