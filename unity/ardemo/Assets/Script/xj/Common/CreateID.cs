using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:CreateID.cs
/// Description:生成一个id
/// Time:2015/7/4 15:23:09
/// </summary>
public class CreateID
{
    #region 公有属性
    public readonly static string Robot_Font = "robot_";
    public readonly static string Actions_Font = "actions_";
    #endregion

    #region 私有属性
    
    #endregion

    #region 公有函数
    /// <summary>
    /// 创建一个机器人id
    /// </summary>
    /// <returns></returns>
    public static string CreateRobotID()
    {
        return Robot_Font + CreateGuid();
    }
    /// <summary>
    /// 创建一套动作的id
    /// </summary>
    /// <returns></returns>
    public static string CreateActionsID()
    {
        return Actions_Font + CreateGuid();
    }
    /// <summary>
    /// 生成一个guid
    /// </summary>
    /// <returns></returns>
    public static string CreateGuid()
    {
        return DateTime.Now.Ticks.ToString();
        return System.Guid.NewGuid().ToString();
    }
    #endregion

    #region 私有函数
    #endregion
}