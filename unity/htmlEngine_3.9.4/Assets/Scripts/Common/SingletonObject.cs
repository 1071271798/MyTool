using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:SingletonObject.cs
/// Description:单例模版
/// Time:2015/7/21 10:33:54
/// </summary>
public class SingletonObject<T> where T : new()
{
    #region 公有属性
    public static T Inst
    {
        get 
        {
            if (null == sInst)
            {
                sInst = new T();
            }
            return sInst;
        }
    }
    #endregion

    #region 私有属性
    static T sInst;
    #endregion

    #region 公有函数
    public static T GetInst()
    {
        if (null == sInst)
        {
            sInst = new T();
        }
        return sInst;
    }
    /// <summary>
    /// 清空数据
    /// </summary>
    public virtual void CleanUp()
    {

    }

    public virtual void Destroy()
    {
        sInst = default(T);
    }
    #endregion

    #region 私有函数
    #endregion
}