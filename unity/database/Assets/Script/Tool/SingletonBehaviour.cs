using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:SingletonBehaviour.cs
/// Description:MonoBehaviour的单例模版，不需要自己去挂脚本，会自动挂在Client上面
/// Time:2015/7/21 10:02:39
/// </summary>
public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    #region 公有属性
    public static T Inst
    {
        get { if (null == mSingleton) CreateComponet(); return mSingleton; }
    }
    #endregion

    #region 私有属性
    static T mSingleton;
    static GameObject sClientObj;
    #endregion

    #region 公有函数
    public static T GetInst()
    {
        if (null == mSingleton)
        {
            CreateComponet();
        }
        return mSingleton;
    }
    #endregion

    #region 私有函数
    static void CreateComponet()
    {
        if (null == sClientObj)
        {
            sClientObj = GameObject.Find("MainClient");
        }
        if (null != sClientObj)
        {
            mSingleton = sClientObj.GetComponent<T>();
            if (null == mSingleton)
            {
                mSingleton = sClientObj.AddComponent<T>();
            }
        }
    }
    #endregion
}