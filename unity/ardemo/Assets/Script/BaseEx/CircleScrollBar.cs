using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:CircleScrollBar.cs
/// Description:
/// Time:2015/10/22 11:14:07
/// </summary>
public class CircleScrollBar : UISlider
{
    #region 公有属性
    public delegate void OnDragChange(GameObject obj, bool finished);

    public OnDragChange onDragChange;

    /// <summary>
    /// 直角坐标系中的初始角度
    /// </summary>
    public float RealStartAngle
    {
        get { return mRealStartAngle; }
        set
        {
            mRealStartAngle = value;
        }
    }
    /// <summary>
    /// 角度范围
    /// </summary>
    public float AngleRange
    {
        get { return mAngleRange; }
        set
        {
            mAngleRange = value;
        }
    }
    /// <summary>
    /// 半径
    /// </summary>
    public float Radius
    {
        get { return mRadius; }
        set
        {
            mRadius = value;
            if (mRadius < 0)
            {
                mRadius = 0;
            }
            SetFGPos(this.value);
        }
    }
    #endregion

    #region 其他属性
    [SerializeField]
    float mRadius = 50f;
    [SerializeField]
    float mRealStartAngle = 0f;
    [SerializeField]
    float mAngleRange = 360f;


    float mLocalStartAngle = 1f;
    float mLocalAngle = 0f;

    float mMinRadius;
    float mMaxRadius;

    bool mValueUpdateFlag = false;
    float targetValue = 0;
    float startValue = 0;
    float mTotalTime = 0;
    float mDuration = 0;

    GameObject mGameObject;
    Vector2 mCachedTouchPos;
    GameObject mPressObject;
    #endregion

    #region 公有函数

    public void OnRefresh()
    {
        if (mRealStartAngle < 0 || mRealStartAngle >= 360)
        {
            mRealStartAngle %= 360;
            if (mRealStartAngle < 0)
            {
                mRealStartAngle += 360;
            }
        }
        if (mAngleRange < 0 || mAngleRange > 360)
        {
            mAngleRange %= 360;
            if (mAngleRange < 0)
            {
                mAngleRange += 360;
            }
        }
        if (mRadius < 0)
        {
            mRadius = 0;
        }
        CalculateRadius();
        SetFGPos(this.value);
    }
    #endregion

    #region 其他函数
    /// <summary>
    /// Make the scroll bar's foreground react to press events.
    /// </summary>

    protected override void OnStart()
    {
        base.OnStart();
        mGameObject = gameObject;
        if (mFG != null && mFG.gameObject != gameObject)
        {
            bool hasCollider = (mFG.GetComponent<Collider>() != null) || (mFG.GetComponent<Collider2D>() != null);
            if (!hasCollider) return;

            UIEventListener fgl = UIEventListener.Get(mFG.gameObject);
            fgl.onPress += OnPressForeground;
            fgl.onDrag += OnDragForeground;
            mFG.autoResizeBoxCollider = true;

            CalculateRadius();
        }
    }

    void OnDisable()
    {
        if (mValueUpdateFlag)
        {
            mValueUpdateFlag = false;
            mDuration = 0;
            if (null != onDragChange && null != mGameObject)
            {
                onDragChange(mGameObject, true);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        if (mValueUpdateFlag)
        {
            float before = this.value;
            mDuration += Time.deltaTime;
            float tmpValue = Mathf.Lerp(startValue, targetValue, mDuration / mTotalTime);
            this.value = tmpValue;
            bool finished = false;
            if (Math.Abs(value - targetValue) < 0.01f)
            {
                mValueUpdateFlag = false;
                value = targetValue;
                tmpValue = targetValue;
                mDuration = 0;
                finished = true;
            }
            SetFGPos(tmpValue);
            if (before != this.value && null != onDragChange && null != mGameObject)
            {
                onDragChange(mGameObject, finished);
            }
        }
    }

    protected override void OnPressBackground(GameObject go, bool isPressed)
    {
        if (IsInBoxCollider() && IsRange(GetAngleForPos(mCachedTouchPos)))
        {
            if (isPressed)
            {
                mPressObject = go;
            }
            else
            {
                mPressObject = null;
            }
            base.OnPressBackground(go, isPressed);
            if (null != onDragChange && null != mGameObject)
            {
                onDragChange(mGameObject, false);
            }
        }
        else
        {
            mPressObject = null;
        }
    }

    protected override void OnDragBackground(GameObject go, Vector2 delta)
    {
        if (IsInBoxCollider() && IsRange(GetAngleForPos(mCachedTouchPos)) && mPressObject == go)
        {
            float before = this.value;
            base.OnDragBackground(go, delta);
            if (before != this.value && null != onDragChange && null != mGameObject)
            {
                onDragChange(mGameObject, false);
            }
        }
        else
        {
            mPressObject = null;
        }
    }

    protected override void OnPressForeground(GameObject go, bool isPressed)
    {
        if (IsInBoxCollider())
        {
            if (isPressed)
            {
                mPressObject = go;
            }
            else
            {
                mPressObject = null;
            }
            base.OnPressForeground(go, isPressed);
            if (null != onDragChange && null != mGameObject)
            {
                onDragChange(mGameObject, false);
            }
        }
        else
        {
            mPressObject = null;
        }
    }

    protected override void OnDragForeground(GameObject go, Vector2 delta)
    {
        if (/*IsInBoxCollider() && */mPressObject == go)
        {
            float before = this.value;
            base.OnDragForeground(go, delta);
            if (before != this.value && null != onDragChange && null != mGameObject)
            {
                onDragChange(mGameObject, false);
            }
        }
        else
        {
            mPressObject = null;
        }
    }

    /// <summary>
    /// Move the scroll bar to be centered on the specified position.
    /// </summary>

    protected override float LocalToValue(Vector2 localPos)
    {
        if (mFG != null)
        {
            float dis = Vector2.Distance(localPos, Vector2.zero);
            if (dis < mMinRadius/* || dis > mMaxRadius*/)
            {
                return value;
            }
            float localAngle = GetAngleForPos(localPos);
            targetValue = AngleToValue(localAngle);
            value = targetValue;
            /*startValue = value;
            mTotalTime = Math.Abs(targetValue - value) * 0.1f;
            mValueUpdateFlag = true;
            mDuration = 0f;*/
            return value;
        }
        return base.LocalToValue(localPos);
    }

    /// <summary>
    /// Update the value of the scroll bar.
    /// </summary>

    public override void ForceUpdate()
    {
        if (null != mFG)
        {
            mIsDirty = false;
            SetFGPos(this.value);
        }
        else base.ForceUpdate();
    }

    bool IsRange(float angle)
    {
        float maxAngle = mRealStartAngle + mAngleRange;
        if (maxAngle > 360)
        {
            float tmp = maxAngle - 360;
            if (angle < mRealStartAngle && angle > tmp)
            {
                return false;
            }
        }
        else if (angle < mRealStartAngle || angle > maxAngle)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// 把角度转换成value
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    float AngleToValue(float angle)
    {
        float maxAngle = mRealStartAngle + mAngleRange;
        if (maxAngle > 360)
        {
            float tmp = maxAngle - 360;
            if (angle < mRealStartAngle && angle > tmp)
            {
                angle = this.value > 1 - this.value ? tmp : mRealStartAngle;
            }
        }
        else
        {
            if (angle < mRealStartAngle)
            {
                angle = mRealStartAngle;
            }
            else if (angle > maxAngle)
            {
                angle = maxAngle;
            }
        }
        float valAngle = angle - mRealStartAngle;
        if (valAngle < 0 || valAngle > 360)
        {
            valAngle %= 360;
            if (valAngle < 0)
            {
                valAngle += 360;
            }
        }
        
        float val = valAngle / mAngleRange;
        if (val < 0)
        {
            val = 0;
        }
        else if (val > 1)
        {
            val = 1;
        }
        return val;
    }
    /// <summary>
    /// 通过坐标，求角度
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    float GetAngleForPos(Vector3 pos)
    {
        float angle = 0;
        if (pos.x == 0)
        {
            if (pos.y == 0)
            {
                angle = 0;
            }
            else if (pos.y > 0)
            {
                angle = 90;
            }
            else
            {
                angle = 270;
            }
        }
        else if (pos.x < 0)
        {
            if (pos.y == 0)
            {
                angle = 180;
            }
            else if (pos.y < 0)
            {//第三象限
                angle = (Mathf.Atan(pos.y / pos.x) * 180 / Mathf.PI) + 180;
            }
            else
            {//第二象限
                angle = 180 - (Mathf.Atan(-pos.y / pos.x) * 180 / Mathf.PI);
            }
        }
        else
        {
            if (pos.y == 0)
            {
                angle = 0;
            }
            else if (pos.y < 0)
            {//第四象限
                angle = 360 - (Mathf.Atan(-pos.y / pos.x) * 180 / Mathf.PI);
            }
            else
            {//第一象限
                angle = (Mathf.Atan(pos.y / pos.x) * 180 / Mathf.PI);
            }
        }
        if (float.IsNaN(angle))
        {
            angle = 0;
            Debuger.LogError("GetAngleForPos angle float.IsNaN");
        }
        return angle;
    }
    void SetFGPos(float value)
    {
        if (null != mFG)
        {
            float angle = mRealStartAngle + mAngleRange * value;
            Vector3 pos = mFG.transform.localPosition;
            pos.x = mRadius * Mathf.Cos(angle * Mathf.PI / 180);
            pos.y = mRadius * Mathf.Sin(angle * Mathf.PI / 180);
            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
            {
                mFG.transform.localPosition = pos;
            }
        }
    }

    void CalculateRadius()
    {
        if (null != mFG)
        {
            Vector3[] corners = mFG.localCorners;
            Vector3 size = (corners[2] - corners[0]);
            float fgRadius = Mathf.Sqrt(size.x * size.x + size.y * size.y) / 2;
            mMinRadius = mRadius - fgRadius;
            mMaxRadius = mRadius + fgRadius;
        }
    }

    bool IsInBoxCollider()
    {
        // Create a plane
        Transform trans = cachedTransform;
        Plane plane = new Plane(trans.rotation * Vector3.back, trans.position);
        // If the ray doesn't hit the plane, do nothing
        float dist;
        Ray ray = cachedCamera.ScreenPointToRay(UICamera.lastTouchPosition);
        if (!plane.Raycast(ray, out dist)) return false;
        // Transform the point from world space to local space
        mCachedTouchPos = trans.InverseTransformPoint(ray.GetPoint(dist));
        float dis = Vector2.Distance(mCachedTouchPos, Vector2.zero);
        if (dis < mMinRadius || dis > mMaxRadius)
        {
            return false;
        }
        return true;
    }
    #endregion
}