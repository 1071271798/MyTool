using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:SensorExceptionMgr.cs
/// Description:传感器异常管理
/// Time:2016/11/16 10:08:04
/// </summary>
public class SensorExceptionMgr : SingletonObject<SensorExceptionMgr>
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<TopologyPartType, List<byte>> mExceptionData;
    int mNeedWaitCount = 0;
    #endregion

    public SensorExceptionMgr()
    {
        mExceptionData = new Dictionary<TopologyPartType, List<byte>>();
    }

    #region 公有函数
    /// <summary>
    /// 增加异常id，把新异常的id添加到以前的异常id里面
    /// </summary>
    /// <param name="sensorType"></param>
    /// <param name="ids"></param>
    public void AddException(TopologyPartType sensorType, List<byte> ids)
    {
        if (ids.Count > 0)
        {
            if (mExceptionData.ContainsKey(sensorType))
            {
                for (int i = 0, imax = ids.Count; i < imax; ++i)
                {
                    if (!mExceptionData[sensorType].Contains(ids[i]))
                    {
                        mExceptionData[sensorType].Add(ids[i]);
                    }
                }
            }
            else
            {
                mExceptionData[sensorType] = ids;
            }
            mExceptionData[sensorType].Sort();
        }
    }
    /// <summary>
    /// 更新一条异常数据，替换以前数据
    /// </summary>
    /// <param name="sensorType"></param>
    /// <param name="ids"></param>
    public void UpdateException(TopologyPartType sensorType, List<byte> ids)
    {
        if (ids.Count > 0)
        {
            mExceptionData[sensorType] = ids;
            mExceptionData[sensorType].Sort();
        }
    }

    public void RemoveException(TopologyPartType sensorType)
    {
        mExceptionData.Remove(sensorType);
    }

    public List<byte> GetSensorException(TopologyPartType sensorType)
    {
        if (null != mExceptionData && mExceptionData.ContainsKey(sensorType))
        {
            return mExceptionData[sensorType];
        }
        return null;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        if (null != mExceptionData)
        {
            mExceptionData.Clear();
        }
        mNeedWaitCount = 0;
    }

    public void CleanCount()
    {
        mNeedWaitCount = 0;
    }

    public int GetWaitCount()
    {
        return mNeedWaitCount;
    }

    public void AddCount()
    {
        ++mNeedWaitCount;
    }
    public void SubCount()
    {
        --mNeedWaitCount;
    }
    #endregion

    #region 其他函数
    #endregion
}