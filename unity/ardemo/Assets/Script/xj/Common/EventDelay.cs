using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:EventDelay.cs
/// Description:
/// Time:2017/9/29 10:37:00
/// </summary>
public class EventDelay
{
    public delegate void TimeCallBack(params object[] args);
    public class TimeData
    {
        public float time;
        float callTime;
        EventDelegate.Callback callBack;
        public bool isPlaying;
        
        public TimeData(float callTime, EventDelegate.Callback ack)
        {
            time = 0;
            this.callTime = callTime;
            this.callBack = ack;           
            isPlaying = true;
        }

        public bool TryCall()
        {
            if (time >= callTime)
            {
                if (null != callBack)
                {
                    callBack();
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

    public EventDelay()
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
                
            }
        }
    }

    
    public TimeData Add(float start, EventDelegate.Callback ack)
    {
        TimeData data = new TimeData(start, ack);
        if (null == mAddList)
        {
            mAddList = new List<TimeData>();
        }
        mAddList.Add(data);
        return data;
    }

    public void Remove(TimeData data)
    {
        if (null == data)
        {
            return;
        }
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