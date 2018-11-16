using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:StayDuringEvent.cs
/// Description:
/// Time:2017/12/28 17:48:25
/// </summary>
public class StayDuringEvent
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<MobClickEventID, float> mEventDict;
    #endregion

    #region 公有函数
    public StayDuringEvent()
    {
        mEventDict = new Dictionary<MobClickEventID, float>();
    }

    public void EventStart(MobClickEventID eventId)
    {
        mEventDict[eventId] = Time.time;
    }

    public void EventEnd(MobClickEventID eventId)
    {
        if (mEventDict.ContainsKey(eventId))
        {
            EventFinished(eventId, mEventDict[eventId]);
            mEventDict.Remove(eventId);
        }
    }

    public void AllEventEnd()
    {
        foreach (var kvp in mEventDict)
        {
            //if (kvp.Key != MobClickEventID.Connection_DuringToSuccess)
            {
                EventFinished(kvp.Key, kvp.Value);
            }
        }
        mEventDict.Clear();
    }
    #endregion

    #region 其他函数
    void EventFinished(MobClickEventID eventId, float time)
    {
        float during = Time.time - time;
        if (false/*eventId == MobClickEventID.Connection_DuringToSuccess*/)
        {
            if (during <= 30)
            {
                PlatformMgr.Instance.MobClickEvent(eventId, PublicFunction.Rounding(during) + " s");
            }
            else
            {
                PlatformMgr.Instance.MobClickEvent(eventId, "30+ s");
            }
        }
        else
        {
            if (during < 300)
            {
                PlatformMgr.Instance.MobClickEvent(eventId, "< 5 min");
            } else if (during < 1200)
            {//20分钟以内，以5分钟递增
                PlatformMgr.Instance.MobClickEvent(eventId, PublicFunction.Rounding(during / 300) * 5 + " min");
            }
            else if (during < 3600)
            {//10分钟递增
                PlatformMgr.Instance.MobClickEvent(eventId, PublicFunction.Rounding(during / 600) * 10 + " min");
            } else
            {
                PlatformMgr.Instance.MobClickEvent(eventId, "60+ min");
            }
        }
    }
    #endregion
}