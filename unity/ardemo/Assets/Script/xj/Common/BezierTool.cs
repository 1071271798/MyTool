using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:BezierTool.cs
/// Description:
/// Time:2017/2/13 15:00:55
/// </summary>
public class BezierTool
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    /// <summary>
    /// 三阶Bezier曲线
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector2 ThirdOrderBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        t = Mathf.Clamp01(t);
        Vector2 target = Vector2.zero;
        target = (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2;
        return target;
    }
    #endregion

    #region 其他函数
    #endregion
}