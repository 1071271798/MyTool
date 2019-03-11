using Game.Platform;
using UnityEngine;
/// <summary>
/// 显示游戏中获得的成就
/// </summary>
public class RainbowAchieveMsg : BasePopWin
{
    EventDelegate.Callback mCloseCallback;
    RainbowAchieveData mAchieveData;

    public RainbowAchieveMsg(RainbowAchieveData data, EventDelegate.Callback callback)
    {
        isSingle = true;
        mAchieveData = data;
        mCloseCallback = callback;
        mUIResPath = "ARGame/Msg/rainbowGameAchieveMsg";
    }

    public static void ShowAchieveMsg(RainbowAchieveData data, EventDelegate.Callback callback)
    {
        object[] args = new object[2];
        args[0] = data;
        args[1] = callback;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(RainbowAchieveMsg), args);
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
                Transform label_des = mTrans.Find("Label_des");
                if (null != label_des)
                {
                    string daimonName = string.Empty;
                    if (null != mAchieveData)
                    {
                        daimonName = mAchieveData.titleName;
                    }
                    GameHelper.SetLabelText(label_des, string.Format("Congratulations! \n You got a diamon {0}!", daimonName));
                    GameHelper.SetPosition(label_des, UIWidget.Pivot.Center, new Vector2(0, -100));
                }
                Transform btn_close = mTrans.Find("Btn_close");
                if (null != btn_close)
                {
                    GameHelper.SetLabelText(btn_close.Find("Label"), "Continue");
                    GameHelper.SetPosition(btn_close, UIWidget.Pivot.Bottom, new Vector2(0, PublicFunction.Bottom_Margin_x));
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