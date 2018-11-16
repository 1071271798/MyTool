using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:MyTweenPosition.cs
/// Description:
/// Time:2015/12/4 14:07:48
/// </summary>
public class MyTweenPosition : MonoBehaviour
{
    public delegate void MoveFinished();
    #region 公有属性
    public Vector3 from;
    public Vector3 to;
    public float duration;
    public float delay;
    public MoveFinished onMoveFinished;
    #endregion

    #region 其他属性
    Transform mTrans;
    float mStartTime = 0f;
    float mDuration = 0f;
    bool playFlag = false;
    UIPanel mPanel;
    UIScrollView mDrag;
    UIScrollViewEx mDragEx;
    #endregion

    #region 公有函数
    public void Play()
    {
        playFlag = true;
        mStartTime = Time.time;
        mDuration = 0;
        enabled = true;
    }

    static public MyTweenPosition Begin(GameObject go, Vector3 pos, float duration)
    {
        MyTweenPosition sp = go.GetComponent<MyTweenPosition>();
        if (sp == null) sp = go.AddComponent<MyTweenPosition>();
        sp.from = go.transform.localPosition;
        sp.to = pos;
        sp.duration = duration;
        sp.Play();
        return sp;
    }

    public void MoveToFinished()
    {
        if (playFlag)
        {
            Vector3 before = mTrans.localPosition;
            Vector3 after = to;
            playFlag = false;
            mDuration = 0;
            enabled = false;
            if (null != onMoveFinished)
            {
                onMoveFinished();
            }
            mTrans.localPosition = after;
            Vector3 offset = after - before;
            Vector2 cr = mPanel.clipOffset;
            cr.x -= offset.x;
            cr.y -= offset.y;
            mPanel.clipOffset = cr;

            if (mDrag != null) mDrag.UpdateScrollbars(false);
            if (mDragEx != null) mDragEx.UpdateScrollbars(false);
        }
        
    }
    #endregion

    #region 其他函数
    void Start()
    {
        mTrans = transform;
        mStartTime = 0f;
        mDuration = 0f;
        mPanel = GetComponent<UIPanel>();
        mDrag = GetComponent<UIScrollView>();
        mDragEx = GetComponent<UIScrollViewEx>();
    }

    void Update()
    {
        if (playFlag)
        {
            if (Time.time - mStartTime >= delay)
            {
                mDuration += Time.deltaTime;
                float val = mDuration / duration;
                Vector3 before = mTrans.localPosition;
                Vector3 after = new Vector3(Mathf.Lerp(from.x, to.x, val), Mathf.Lerp(from.y, to.y, val), Mathf.Lerp(from.z, to.z, val));
                if (Vector3.Distance(after, to) <= 0.01f)
                {
                    after = to;
                    playFlag = false;
                    mDuration = 0;
                    enabled = false;
                    if (null != onMoveFinished)
                    {
                        onMoveFinished();
                    }
                }
                mTrans.localPosition = after;
                Vector3 offset = after - before;
                Vector2 cr = mPanel.clipOffset;
                cr.x -= offset.x;
                cr.y -= offset.y;
                mPanel.clipOffset = cr;

                if (mDrag != null) mDrag.UpdateScrollbars(false);
                if (mDragEx != null) mDragEx.UpdateScrollbars(false);
            }
        }
        else
        {
            enabled = false;
        }
    }
    #endregion
}