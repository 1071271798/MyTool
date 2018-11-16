using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:TriggerEvent.cs
/// Description:触发器事件
/// Time:2015/11/3 14:02:44
/// </summary>
public class TriggerEvent : MonoBehaviour
{
    #region 公有属性
    public TriggerDelegate triggerdlgt;
    #endregion

    #region 其他属性
    GameObject mGameObject;
    #endregion

    #region 公有函数
    #endregion

    #region 其他函数
    void Start()
    {
        mGameObject = gameObject;
    }
    /// <summary>
    /// 进入触发器
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (enabled && null != triggerdlgt && null != triggerdlgt.onEnter)
        {
            triggerdlgt.onEnter(mGameObject, other.gameObject);
        }
    }
    /// <summary>
    /// 退出触发器
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if (enabled && null != triggerdlgt && null != triggerdlgt.onExit)
        {
            triggerdlgt.onExit(mGameObject, other.gameObject);
        }
    }
    /// <summary>
    /// 逗留在触发器
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        if (enabled && null != triggerdlgt && null != triggerdlgt.onStay)
        {
            triggerdlgt.onStay(mGameObject, other.gameObject);
        }
    }
    
    #endregion
}

public class TriggerDelegate
{
    public delegate void OnTriggerEnter(GameObject own, GameObject other);
    public delegate void OnTriggerExit(GameObject own, GameObject other);
    public delegate void OnTriggerStay(GameObject own, GameObject other);


    public OnTriggerEnter onEnter;
    public OnTriggerExit onExit;
    public OnTriggerStay onStay;
}