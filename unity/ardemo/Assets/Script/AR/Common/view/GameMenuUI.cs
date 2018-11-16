using Game.Platform;
using UnityEngine;

public class GameMenuUI : BaseUI
{
    public GameMenuUI()
    {
        mUIResPath = "ARGame/UI/gameMenu";
    }


    protected override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            if (null != mTrans)
            {
                GameHelper.SetPosition(mTrans, UIWidget.Pivot.Right, Vector2.zero);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
    }
}