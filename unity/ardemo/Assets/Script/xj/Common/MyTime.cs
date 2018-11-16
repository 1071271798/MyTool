using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.Linq;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:MyTime.cs
/// Description:
/// Time:2015/12/2 10:40:13
/// </summary>

public class MyTime : SingletonObject<MyTime>
{
    public delegate void TimeCallBack(params object[] args);
    public class TimeData
    {
        public float time;
        float callTime;
        TimeCallBack callBack;
        public bool isPlaying;
        object[] args;
        public TimeData(float callTime, TimeCallBack ack, params object[] args)
        {
            time = 0;
            this.callTime = callTime;
            this.callBack = ack;
            this.args = args;
            isPlaying = true;
        }

        public bool TryCall()
        {
            if (time >= callTime)
            {
                if (null != callBack)
                {
                    callBack(args);
                }
				isPlaying = false;
                return true;
            }
            return false;
        }
    }

    List<TimeData> mTimeList = null;
    List<TimeData> mDelList = null;
    List<TimeData> mAddList = null;

    public MyTime()
    {
        mDelList = new List<TimeData>();
    }

    public void Update()
    {
        if (null != mAddList)
        {
            for (int i = 0, imax = mAddList.Count; i < imax; ++i)
            {
                if (null == mTimeList)
                {
                    mTimeList = new List<TimeData>();
                }
                mTimeList.Add(mAddList[i]);
            }
            mAddList = null;
        }
        if (null != mDelList && null != mTimeList && mDelList.Count > 0)
        {
            for (int i = 0, imax = mDelList.Count; i < imax; ++i)
            {
                mTimeList.Remove(mDelList[i]);
            }
            mDelList = null;
        }
        if (null != mTimeList && mTimeList.Count > 0)
        {
            for (int i = 0, imax = mTimeList.Count; i < imax; ++i)
            {
                if (mTimeList[i].isPlaying)
                {
                    mTimeList[i].time += Time.fixedDeltaTime;
                    if (mTimeList[i].TryCall())
                    {
                        Remove(mTimeList[i]);
                    }
                }
                if (imax != mTimeList.Count)
                {
                    Debug.LogError("数量不一致");
                }
            }
        }
    }

    public void PauseTime()
    {
        if (null != mTimeList)
        {
            for (int i = 0, imax = mTimeList.Count; i < imax; ++i)
            {
                mTimeList[i].isPlaying = false;
            }
        }
    }

    public void ContinueTime()
    {
        if (null != mTimeList)
        {
            for (int i = 0, imax = mTimeList.Count; i < imax; ++i)
            {
                mTimeList[i].isPlaying = true;
            }
        }
    }


    public void StopTime()
    {
        if (null != mTimeList)
        {
            for (int i = 0, imax = mTimeList.Count; i < imax; ++i)
            {
                Remove(mTimeList[i]);
            }
        }
    }



    public void Add(float start, TimeCallBack ack, params object [] args)
    {
        TimeData data = new TimeData(start, ack, args);
        if (null == mAddList)
        {
            mAddList = new List<TimeData>();
        }
        mAddList.Add(data);
    }

    void Remove(TimeData data)
    {
        if (null == mDelList)
        {
            mDelList = new List<TimeData>();
        }
        if (!mDelList.Contains(data))
        {
            mDelList.Add(data);
        }
    }
}