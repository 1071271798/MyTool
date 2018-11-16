using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:UICenterOnClickEx.cs
/// Description:
/// Time:2015/8/18 16:15:35
/// </summary>
[AddComponentMenu("NGUI/Interaction/Center Scroll View on ClickEx")]
public class UICenterOnClickEx : MonoBehaviour
{
    #region 公有属性
    #endregion

    #region 私有属性
    #endregion

    #region 公有函数
    #endregion

    #region 私有函数
    void OnEnable()
    {
        
    }
    void OnClick ()
	{
        if (!enabled)
        {
            return;
        }
        UICenterOnChildEx center = NGUITools.FindInParents<UICenterOnChildEx>(gameObject);
        CustomGrid grid = NGUITools.FindInParents<CustomGrid>(gameObject);
		UIPanel panel = NGUITools.FindInParents<UIPanel>(gameObject);

        if (center != null && center.enabled && grid != null)
		{
            ItemDataEx data = grid.GetItemData(transform.name);
            if (null != data)
            {
                center.CenterOn(data);
            }
            
			
		}
		else if (panel != null && panel.clipping != UIDrawCall.Clipping.None)
		{
            UIScrollViewEx sv = panel.GetComponent<UIScrollViewEx>();
			Vector3 offset = -panel.cachedTransform.InverseTransformPoint(transform.position);
			if (!sv.canMoveHorizontally) offset.x = panel.cachedTransform.localPosition.x;
			if (!sv.canMoveVertically) offset.y = panel.cachedTransform.localPosition.y;
			SpringPanel.Begin(panel.cachedGameObject, offset, 6f);
		}
	}
    #endregion
}