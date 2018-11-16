using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:MyTweenAlpha.cs
/// Description:
/// Time:2016/4/12 16:28:24
/// </summary>
public class MyTweenAlpha : UITweener
{
#if UNITY_3_5
	public float from = 1f;
	public float to = 1f;
#else
    [Range(0f, 1f)]
    public float from = 1f;
    [Range(0f, 1f)]
    public float to = 1f;
#endif

    UIWidget[] mWidgetAry = null;
    

    /// <summary>
    /// Tween's current value.
    /// </summary>

    public float value
    {
        get
        {
            if (null == mWidgetAry)
            {
                FindAllWidget();
            }
            if (null == mWidgetAry || mWidgetAry.Length < 1)
            {
                return 0;
            }
            else
            {
                return mWidgetAry[0].alpha;
            }
        }
        set
        {
            if (null == mWidgetAry)
            {
                FindAllWidget();
            }
            if (null != mWidgetAry)
            {
                for (int i = 0, imax = mWidgetAry.Length; i < imax; ++i)
                {
                    mWidgetAry[i].alpha = value;
                }
            }
        }
    }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate(float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenAlpha Begin(GameObject go, float duration, float alpha)
    {
        TweenAlpha comp = UITweener.Begin<TweenAlpha>(go, duration);
        comp.from = comp.value;
        comp.to = alpha;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }

    public override void SetStartToCurrentValue() { from = value; }
    public override void SetEndToCurrentValue() { to = value; }

    void FindAllWidget()
    {
        if (null != gameObject)
        {
            mWidgetAry = gameObject.GetComponentsInChildren<UIWidget>(true);
        }
    }
}