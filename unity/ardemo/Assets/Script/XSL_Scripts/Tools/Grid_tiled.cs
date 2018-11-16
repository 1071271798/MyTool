using UnityEngine;
using System.Collections;

public class Grid_tiled : UIGrid {

    protected override void Start()
    {
        fixedColumn = 4;
        StartCoroutine(WaitEndFrame());
    }
    public int fixedColumn;
    IEnumerator WaitEndFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        if (GetComponent<UIWidget>() != null && transform.childCount == fixedColumn)
        {
            cellHeight = GetComponent<UIWidget>().height / fixedColumn;
            
            for (int i = 0; i <fixedColumn ; i++)
            {
              //  GetComponent<UIWidget>().SetDimensions(120, (int)cellHeight);

                if (transform.GetChild(i).GetComponent<UIWidget>() != null)
                {
                    transform.GetChild(i).GetComponent<UIWidget>().height = (int)cellHeight-1;
                }
            }
        }
        base.Start();
    }

    protected override void Init()
    {
        base.Init();
    }

    public override void Reposition()
    {
        base.Reposition();
    }

    protected override void Update()
    {
        base.Update();
    }
}
