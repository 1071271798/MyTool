using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:BezierPosition.cs
/// Description:
/// Time:2017/2/13 15:29:47
/// </summary>
public class BezierPosition : UITweener
{
    #region 公有属性
    public Vector3 from;
    public Vector3 to;

    [HideInInspector]
    public bool worldSpace = false;

    public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

    /// <summary>
    /// Tween's current value.
    /// </summary>

    public Vector3 value
    {
        get
        {
            return worldSpace ? cachedTransform.position : cachedTransform.localPosition;
        }
        set
        {
            if (mRect == null || !mRect.isAnchored || worldSpace)
            {
                if (worldSpace) cachedTransform.position = value;
                else cachedTransform.localPosition = value;
            }
            else
            {
                value -= cachedTransform.localPosition;
                NGUIMath.MoveRect(mRect, value.x, value.y);
            }
        }
    }
    #endregion

    #region 其他属性
    List<Vector3> mid;
    Transform mTrans;
    UIRect mRect;
    #endregion

    #region 公有函数
    public static BezierPosition Begin(GameObject go, float duration, Vector3 from, Vector3 to, List<Vector3> mid)
    {
        BezierPosition comp = go.GetComponent<BezierPosition>();
        if (null == comp)
        {
            comp = UITweener.Begin<BezierPosition>(go, duration);
        }
        comp.from = from;
        comp.to = to;
        comp.mid = mid;
        comp.enabled = true;
        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }
    #endregion

    #region 其他函数
    
    void Awake() { mRect = GetComponent<UIRect>(); }
    protected override void OnUpdate(float factor, bool isFinished)
    {
        if (null == mid || mid.Count == 0)
        {
            value = from * (1f - factor) + to * factor;
        }
        else if (mid.Count == 1)
        {
            value = BezierTool.ThirdOrderBezier(from, mid[0], to, factor);
        }
        else
        {
            value = from * (1f - factor) + to * factor;
        }
    }
    #endregion
}