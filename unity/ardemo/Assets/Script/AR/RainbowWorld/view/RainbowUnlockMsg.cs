using Game.Platform;
using UnityEngine;
/// <summary>
/// 彩虹世界难度解锁
/// </summary>
public class RainbowUnlockMsg : BasePopWin
{
    EGameDifficulty mUnlockDifficulty;
    EventDelegate.Callback mCloseCallback;
    public RainbowUnlockMsg(EGameDifficulty diff, EventDelegate.Callback callback)
    {
        mUIResPath = "ARGame/Msg/rainbowDifficultyUnlock";
        isSingle = true;
        mUnlockDifficulty = diff;
        mCloseCallback = callback;
    }

    public static void ShowUnlockMsg(EGameDifficulty diff, EventDelegate.Callback callback)
    {
        object[] args = new object[2];
        args[0] = diff;
        args[1] = callback;
        PopWinManager.GetInst().ShowPopWin(typeof(RainbowUnlockMsg), args);
    }


    public override void Init()
    {
        base.Init();
        mCoverColor = PublicFunction.Connect_bg_Color;
        mCoverAlpha = 1;
    }

    protected override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            if (null != mTrans)
            {
                Transform title = mTrans.Find("Label_title");
                if (null != title)
                {
                    GameHelper.SetPosition(title, UIWidget.Pivot.Top, new Vector2(0, 100));
                    GameHelper.SetLabelText(title, "Difficulty Unlock");
                }
                Transform difficulty = mTrans.Find("Label_difficulty");
                if (null != difficulty)
                {
                    if (mUnlockDifficulty == EGameDifficulty.Hard)
                    {
                        GameHelper.SetLabelText(difficulty, "Advanced");
                    }
                }
                Transform des = mTrans.Find("Label_des");
                if (null != des)
                {
                    GameHelper.SetPosition(des, UIWidget.Pivot.Center, new Vector2(0, -100));
                    GameHelper.SetLabelText(des, "Let's try it next time.");
                }
                Transform btnClose = mTrans.Find("Btn_close");
                if (null != btnClose)
                {
                    GameHelper.SetPosition(btnClose, UIWidget.Pivot.Bottom, new Vector2(0, 100));
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
                if (null != mCloseCallback)
                {
                    mCloseCallback();
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