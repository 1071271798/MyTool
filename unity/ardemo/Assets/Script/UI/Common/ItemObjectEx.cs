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
/// item对像的封装类Lzh_LoopItemObject，不要求具体的item类来继承它。
/// </summary>
//[System.Serializable]
public class ItemObjectEx
{
	/// <summary>
	/// 本item，在实际整个scrollview中的索引位置，
	/// 即对就数据，在数据列表中的索引
	/// </summary>
	public ItemDataEx data = null;


    public GameObject itemObj;

    public GameObject childObj;

    public GameObject childObj1;
}
