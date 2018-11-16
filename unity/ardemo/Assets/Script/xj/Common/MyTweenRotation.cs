using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:MyTweenRotation.cs
/// Description:匀速旋转
/// Time:2015/11/25 15:35:40
/// </summary>
public class MyTweenRotation : MonoBehaviour
{
    #region 公有属性
    public Vector3 from;
    public Vector3 to;
    public float duration;
    public float delay;
    #endregion

    #region 其他属性
    Transform mTrans;
    float mStartTime = 0f;
    float mDuration = 0f;
    bool playFlag = false;
    #endregion

    #region 公有函数
    public void Play()
    {
        playFlag = true;
        mStartTime = Time.time;
        enabled = true;
        from = CheckRota(from);
        to = CheckRota(to);
    }
    #endregion

    #region 其他函数
    void Awake()
    {
        mTrans = transform;
        mStartTime = 0f;
        mDuration = 0f;
    }


    Vector3 CheckRota(Vector3 rota)
    {
        rota.x = CheckRota(rota.x);
        rota.y = CheckRota(rota.y);
        rota.z = CheckRota(rota.z);
        return rota;
    }

    float CheckRota(float rota)
    {
        if (rota < 0)
        {
            rota %= 360;
            if (rota < 0)
            {
                rota += 360;
            }
        }
        return rota;
    }

    float GetUpdateRota(float rota)
    {
        if (rota >= 180 && rota < 360)
        {
            return rota - 360;
        }
        return rota;
    }

    Vector3 GetUpdateRota(Vector3 rota)
    {
        rota.x = GetUpdateRota(rota.x);
        rota.y = GetUpdateRota(rota.y);
        rota.z = GetUpdateRota(rota.z);
        return rota;
    }
    void Update()
    {
        if (playFlag)
        {
            if (Time.time - mStartTime >= delay)
            {
                mDuration += Time.deltaTime;
                float val = mDuration / duration;
                float x;
                float y;
                float z;
                Vector3 tmpFrom = GetUpdateRota(from);
                Vector3 tmpTo = GetUpdateRota(to);
                if (Mathf.Abs(from.x - to.x) < 0.001f)
                {
                    x = to.x;
                }
                else
                {
                    x = Mathf.Lerp(tmpFrom.x, tmpTo.x, val);
                }
                if (Mathf.Abs(from.y - to.y) < 0.001f)
                {
                    y = to.y;
                }
                else
                {
                    y = Mathf.Lerp(tmpFrom.y, tmpTo.y, val);
                }
                if (Mathf.Abs(from.z - to.z) < 0.001f)
                {
                    z = to.z;
                }
                else
                {
                    z = Mathf.Lerp(tmpFrom.z, tmpTo.z, val);
                }
                mTrans.localEulerAngles = new Vector3(x, y, z);
                if (Vector3.Distance(mTrans.localEulerAngles, to) <= 0.001f)
                {
                    mTrans.localEulerAngles = to;
                    playFlag = false;
                    mDuration = 0;
                    enabled = false;
                }
            }
        }
        else
        {
            enabled = false;
        }
    }
    #endregion
}