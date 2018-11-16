using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:TransformAdaptation.cs
/// Description:
/// Time:2016/9/22 15:19:30
/// </summary>
public class TransformAdaptation : MonoBehaviour
{
    #region 公有属性
    public float ScalingFactor = 0.9f;
    public bool isRecalculate = false;
    public delegate void RecalculateFinished(Transform trans);
    #endregion

    #region 其他属性
    Transform mTrans;
    Vector4 mRect;
    bool isChangePosFlag = false;
    RecalculateFinished recalculateFinished;
    #endregion

    #region 公有函数
    public void Recalculate(Vector4 rect, RecalculateFinished finished)
    {
        isRecalculate = true;
        mRect = rect;
        isChangePosFlag = false;
        this.recalculateFinished = finished;
    }

    public void RemovePosition(Vector4 rect, RecalculateFinished finished)
    {
        isRecalculate = true;
        mRect = rect;
        isChangePosFlag = true;
        this.recalculateFinished = finished;
    }
    #endregion

    #region 其他函数
    void Awake()
    {
        mTrans = transform;
        mRect = new Vector4(0, 0, PublicFunction.GetWidth() * ScalingFactor, PublicFunction.GetHeight() * ScalingFactor);
    }

    void Update()
    {
        if (isRecalculate)
        {
            isRecalculate = false;
            Camera camera = NGUITools.FindInParents<Camera>(mTrans);
            if (null != camera)
            {
                if (isChangePosFlag)
                {
                    isChangePosFlag = false;
                    PublicFunction.MoveToCenter(mTrans, camera.transform, 1, mRect, true, null);
                    if (null != recalculateFinished)
                    {
                        recalculateFinished(mTrans);
                    }
                }
                else
                {
                    PublicFunction.MoveToCenter(mTrans, camera.transform, ScalingFactor, mRect, false,null);
                    if (null != recalculateFinished)
                    {
                        recalculateFinished(mTrans);
                    }
                }
                
            }
            
        }
    }
    #endregion
}