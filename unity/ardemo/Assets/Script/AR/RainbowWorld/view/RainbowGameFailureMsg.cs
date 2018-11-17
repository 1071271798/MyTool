using Game.Platform;
using UnityEngine;
/// <summary>
/// 游戏失败界面
/// </summary>
public class RainbowGameFailureMsg : BasePopWin
{
    EventDelegate.Callback mCancelCallback;
    EventDelegate.Callback mConfirmCallback;
    int mGameCount;

    public RainbowGameFailureMsg(int gameCount, EventDelegate.Callback cancelCallback, EventDelegate.Callback confirmCallback)
    {
        isSingle = true;
        mUIResPath = "ARGame/Msg/rainbowGameFailureMsg";
        this.mGameCount = gameCount;
        this.mCancelCallback = cancelCallback;
        this.mConfirmCallback = confirmCallback;
    }


    public static void ShowGameFailureMsg(int gameCount, EventDelegate.Callback cancelCallback, EventDelegate.Callback confirmCallback)
    {
        object[] args = new object[3];
        args[0] = gameCount;
        args[1] = cancelCallback;
        args[2] = confirmCallback;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(RainbowGameFailureMsg), args);
    }

    protected override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            if (null != mTrans)
            {
                Transform content = mTrans.Find("content");
                if (null != content)
                {
                    GameHelper.SetLabelText(content, string.Format("You have {0} changes. \n Do you wanna try again?", mGameCount));
                    GameHelper.SetPosition(content, UIWidget.Pivot.Center, new Vector2(0, 100));
                }
                Transform btn = mTrans.Find("btn");
                if (null != btn)
                {
                    GameHelper.SetPosition(btn, UIWidget.Pivot.Bottom, new Vector2(0, 150));
                    GameHelper.SetLabelText(btn.Find("Btn_cancel/Label"), "No");
                    GameHelper.SetLabelText(btn.Find("Btn_confirm/Label"), "Yes");
                }
                
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
            if (obj.name.Equals("Btn_confirm"))
            {
                OnClose();
                if (null != mConfirmCallback)
                {
                    mConfirmCallback();
                }
            } else if (obj.name.Equals("Btn_cancel"))
            {
                OnClose();
                if (null != mCancelCallback)
                {
                    mCancelCallback();
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}