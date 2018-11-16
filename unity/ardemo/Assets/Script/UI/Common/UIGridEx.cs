using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIGridEx:UIGrid
{
    public int distance = 0;

    public int CacheItems = 10;

    public bool repositionNow 
    { 
        set 
        { 
            if (value) 
            {
                mReposition = true; 
                enabled = true;
                m_items = GetChildList();
            }
        }
    }

    UIPanel m_panel;
    UIScrollView m_scroll;

    List<Transform> m_items=new List<Transform>();

    void Start()
    {
        base.Start();
        m_panel = NGUITools.FindInParents<UIPanel>(gameObject);
        m_scroll=m_panel.GetComponent<UIScrollView>();
        //m_scroll.onDragFinished += On_Panel_Move;
        m_panel.onClipMove += On_Panel_Move;
    }

    public override void Reposition()
    {
        base.Reposition();
        List<Transform> list = GetChildList();
        ResetPosition(list);
    }

    public void ResetPosition(List<Transform> list)
    {
        float lastPos=0;
        for (int i = 0; i < list.Count; i++)
        {
            if (i == 0)
            {
                list[i].transform.localPosition = Vector3.zero;
            }
            else
            {
                Vector3 vec = (distance > 0) ? NGUIMath.CalculateRelativeWidgetBounds(list[i - 1]).size : Vector3.zero;
                Vector3 pos = (arrangement == Arrangement.Horizontal) ?
                                    new Vector3(lastPos + vec.x + cellWidth, list[i].transform.localPosition.y, list[i].transform.localPosition.z) :
                                    new Vector3(list[i].transform.localPosition.x, lastPos + vec.x + cellWidth, list[i].transform.localPosition.z);
                lastPos = (arrangement == Arrangement.Horizontal) ? pos.x : pos.y;
                list[i].transform.localPosition = pos;
            }
        }
    }

    bool IsVisable(UIPanel panel,Transform trans)
    {

        Vector3[] vec=trans.GetComponentInChildren<UISprite>().worldCorners;
            bool active=panel.IsVisible(vec[0], vec[1], vec[2], vec[3]);
        return active;
    }

    void On_Panel_Move(UIPanel panel)
    {
        
    }

}
