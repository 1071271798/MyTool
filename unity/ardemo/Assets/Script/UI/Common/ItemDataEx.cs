/*
 * 描术：
 * 
 * 作者：AnYuanLzh
 * 公司：lexun
 * 时间：2014-xx-xx
 */
using UnityEngine;
using System.Collections;

/// <summary>
/// 与item对关联的数据类，具体的item的数据类一定继承它
/// </summary>
public class ItemDataEx
{
	// ***
    /// <summary>
    /// item的名字，唯一性
    /// </summary>
    public string name;

    public float width;

    public float height;


    /// <summary>
    /// item的位置
    /// </summary>
    public Vector3 localPosition;

    public ItemDataEx(string name, float width, float height)
    {
        this.name = name;
        this.width = width;
        this.height = height;
    }
}
