using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:MyGrid.cs
/// Description:
/// Time:2016/7/22 14:26:32
/// </summary>
public class MyGrid : UIGrid
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    #endregion

    #region 其他函数
    protected override void ResetPosition(List<Transform> list)
    {
        mReposition = false;

        // Epic hack: Unparent all children so that we get to control the order in which they are re-added back in
        // EDIT: Turns out this does nothing.
        //for (int i = 0, imax = list.Count; i < imax; ++i)
        //	list[i].parent = null;

        int x = 0;
        int y = 0;
        int maxX = 0;
        int maxY = 0;
        Transform myTrans = transform;

        // Re-add the children in the same order we have them in and position them accordingly
        for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            Transform t = list[i];
            // See above
            //t.parent = myTrans;

            float depth = t.localPosition.z;
            Vector3 pos = (arrangement == Arrangement.Horizontal) ?
                new Vector3(cellWidth * x + cellWidth / 2, -cellHeight * y, depth) :
                new Vector3(cellWidth * y, -cellHeight * x + cellHeight / 2, depth);

            if (animateSmoothly && Application.isPlaying)
            {
                SpringPosition sp = SpringPosition.Begin(t.gameObject, pos, 8f);
                sp.updateScrollView = true;
                sp.ignoreTimeScale = true;
            }
            else t.localPosition = pos;

            maxX = Mathf.Max(maxX, x);
            maxY = Mathf.Max(maxY, y);

            if (++x >= maxPerLine && maxPerLine > 0)
            {
                x = 0;
                ++y;
            }
        }

        // Apply the origin offset
        if (pivot != UIWidget.Pivot.TopLeft)
        {
            Vector2 po = NGUIMath.GetPivotOffset(pivot);

            float fx, fy;

            if (arrangement == Arrangement.Horizontal)
            {
                fx = Mathf.Lerp(0f, maxX * cellWidth, po.x);
                fy = Mathf.Lerp(-maxY * cellHeight, 0f, po.y);
            }
            else
            {
                fx = Mathf.Lerp(0f, maxY * cellWidth, po.x);
                fy = Mathf.Lerp(-maxX * cellHeight, 0f, po.y);
            }

            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);
                SpringPosition sp = t.GetComponent<SpringPosition>();

                if (sp != null)
                {
                    sp.target.x -= fx;
                    sp.target.y -= fy;
                }
                else
                {
                    Vector3 pos = t.localPosition;
                    pos.x -= fx;
                    pos.y -= fy;
                    t.localPosition = pos;
                }
            }
        }
    }
    #endregion
}