//----------------------------------------------
//            积木2: xiongsonglin
//            数据格式
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

/// <summary>
/// grid_ex 每个grid下只有<=1 个obj
/// </summary>
public class Grid_ex : UIGrid {

    protected override void Init()
    {
        base.Init();
    }

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// 最是那一低头的温柔，
    /// 像一朵水莲花
    /// 不剩凉风的娇羞
    /// 道一声 珍重
    /// 那一声珍重里有甜蜜的忧愁
    /// 沙扬娜拉
    /// </summary>
    public override void Reposition()
    {
        if (transform.childCount > 1)
        {
            if (UIDragDropItem_ex.tranP != null)
            {
                transform.GetChild(0).parent = UIDragDropItem_ex.tranP.GetComponentInChildren<UIGrid>().transform;
              //  UIDragDropItem_ex.tranP.GetComponentInChildren<UIGrid>().sorting = Sorting.None;
                UIDragDropItem_ex.tranP.GetComponentInChildren<UIGrid>().repositionNow = true;
            }
            //DestroyImmediate(transform.GetChild(0).gameObject);  //需要交换的
        }
        base.Reposition();  
    }

    protected override void Update()
    {
        base.Update();
    }
}
