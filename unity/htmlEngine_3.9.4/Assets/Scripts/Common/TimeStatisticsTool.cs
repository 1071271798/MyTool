using System.Collections.Generic;
using UnityEngine;

public class TimeStatisticsTool : SingletonObject<TimeStatisticsTool>
{
    public delegate void EventFinishedCallBack(long time);
    private Dictionary<string, long> mStatisticsDict;
    private Dictionary<string, List<long>> mTotalDict;

    public void EventStart(string eventStr)
    {
        if (null == mStatisticsDict)
        {
            mStatisticsDict = new Dictionary<string, long>();
        }
        mStatisticsDict[eventStr] = PublicFunction.GetNowMillisecond();
        //Debug.Log(string.Format("{0} start time : {1}", eventStr, mStatisticsDict[eventStr]));
    }

    public void EventFinished(string eventStr, EventFinishedCallBack callback)
    {
        if (null != mStatisticsDict && mStatisticsDict.ContainsKey(eventStr))
        {
            long time = PublicFunction.GetNowMillisecond() - mStatisticsDict[eventStr];
            if (null == mTotalDict)
            {
                mTotalDict = new Dictionary<string, List<long>>();
            }
            if (!mTotalDict.ContainsKey(eventStr))
            {
                List<long> list = new List<long>();
                mTotalDict[eventStr] = list;
            }
            mStatisticsDict.Remove(eventStr);
            mTotalDict[eventStr].Add(time);
            if (null != callback)
            {
                callback(time);
            }
            //Debug.Log(string.Format("{0}  duration time : {1}", eventStr, time));
        } else
        {
            if (null != callback)
            {
                callback(-1);
            }
        }
    }


    public void RemoveEvent(string eventStr)
    {
        if (null != mStatisticsDict)
        {
            mStatisticsDict.Remove(eventStr);
        }
    }
    public void CalculateAverage()
    {
        if (null != mTotalDict)
        {
            foreach (var kvp in mTotalDict)
            {
                List<long> list = kvp.Value;
                list.Sort();
                long average = 0;
                for (int i = 0, imax = list.Count; i < imax; ++i)
                {
                    average += list[i];
                }
                average /= list.Count;
                Debug.Log(string.Format("event = {0} count = {1} average = {2} max = {3} min = {4}", kvp.Key, list.Count, average, list[list.Count - 1], list[0]));
            }
        }
    }


    public override void CleanUp()
    {
        base.CleanUp();
        if (null != mStatisticsDict)
        {
            mStatisticsDict.Clear();
        }
        if (null != mTotalDict)
        {
            mTotalDict.Clear();
        }
    }

}

