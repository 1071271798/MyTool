using Game.Platform;
using UnityEngine;
/// <summary>
/// 显示游戏中获得的成就
/// </summary>
public class RainbowAchievementMsg : BasePopWin
{
    EventDelegate.Callback mCloseCallback;

    public RainbowAchievementMsg(EventDelegate.Callback callback)
    {
        isSingle = true;
        mCloseCallback = callback;
        mUIResPath = "ARGame/Msg/rainbowGameAchievementMsg";
    }

    public static void ShowAchievementMsg(EventDelegate.Callback callback)
    {
        object[] args = new object[1];
        args[0] = callback;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(RainbowAchievementMsg), args);
    }

    protected override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            if (null != mTrans)
            {
                Transform title = mTrans.Find("title");
                if (null != title)
                {
                    GameHelper.SetLabelText(title, "Item received!");
                    GameHelper.SetPosition(title, UIWidget.Pivot.Top, new Vector2(0, PublicFunction.Title_Margin_1));
                }
                GameHelper.SetLabelText(mTrans.Find("Btn_close/Label"), "Continue");
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
        try
        {
            base.OnButtonClick(obj);
            if (obj.name.Equals("Btn_close"))
            {
                OnClose();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void Close()
    {
        base.Close();
        if (null != mCloseCallback)
        {
            mCloseCallback();
        }
    }
}