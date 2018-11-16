using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ConvertAngle.cs
/// Description:
/// Time:2015/7/17 9:42:18
/// </summary>
public class ConvertAngle
{
    #region 公有属性
    #endregion

    #region 私有属性
    #endregion

    #region 公有函数
    public static Vector3 ConvertToRightAngle(Vector3 euler)
    {
        Vector3 angle = euler;
        int yNum = (int)(euler.y / 90 + 0.5f);
        int zNum = (int)(euler.z / 90 + 0.5f);
        if (zNum % 2 == 1)
        {
            int flag = 1;
            if (zNum == 3) flag = -1;
            angle.x = (angle.x + angle.y * flag) % 360;
            if (angle.x < 0) angle.x += 360;
        }
        else if (euler.y == 180 || euler.z == 180)
        {
            angle.x = 180 - angle.x;
            angle.y = euler.y - 180;
            angle.z = euler.z - 180;
            if (angle.x < 0) angle.x += 360;
            if (angle.y < 0) angle.y += 360;
            if (angle.z < 0) angle.z += 360;
        }
        return angle;
    } 
    #endregion

    #region 私有函数
    #endregion
}