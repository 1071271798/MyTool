using Game.Platform;
using UnityEngine;
/// <summary>
/// 彩虹世界游戏结束页面
/// </summary>
public class RainbowGameOverMsg : BasePopWin
{
    EGameScoreState mGameScoreState;
    EventDelegate.Callback mQuitCallback;
    EventDelegate.Callback mRestartCallback;
    int mGameScore;

    public RainbowGameOverMsg(EGameScoreState scoreState, int gameScore, EventDelegate.Callback quitCallback, EventDelegate.Callback restartCallback)
    {
        isSingle = true;
        mUIResPath = "ARGame/Msg/rainbowGameOverMsg";
        mGameScoreState = scoreState;
        mGameScore = gameScore;
        mQuitCallback = quitCallback;
        mRestartCallback = restartCallback;
    }

    public static void ShowGameOverMsg(EGameScoreState scoreState, int gameScore, EventDelegate.Callback quitCallback, EventDelegate.Callback restartCallback)
    {
        object[] args = new object[4];
        args[0] = scoreState;
        args[1] = gameScore;
        args[2] = quitCallback;
        args[3] = restartCallback;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(RainbowGameOverMsg), args);
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
                    InitContentUI(content, mGameScoreState);
                    GameHelper.SetPosition(content, UIWidget.Pivot.Center, new Vector2(0, 100));
                }
                Transform btn = mTrans.Find("btn");
                if (null != btn)
                {
                    GameHelper.SetPosition(btn, UIWidget.Pivot.Bottom, new Vector2(0, 150));
                    GameHelper.SetLabelText(btn.Find("Btn_quit/Label"), "Quit");
                    GameHelper.SetLabelText(btn.Find("Btn_restart/Label"), "Start Over");
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
            if (obj.name.Equals("Btn_quit"))
            {
                OnClose();
                if (null != mQuitCallback)
                {
                    mQuitCallback();
                }
            }
            else if (obj.name.Equals("Btn_restart"))
            {
                OnClose();
                if (null != mRestartCallback)
                {
                    mRestartCallback();
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void InitContentUI(Transform trans, EGameScoreState state)
    {
        string titleText;
        string desText;
        if (state == EGameScoreState.Score_Normal)
        {
            titleText = "Score:";
            desText = "Keep pratice! \n You can do better!";
        } else
        {
            titleText = "High Score:";
            desText = "Congratulations! \n You achieved a new high score!";
        }
        GameHelper.SetLabelText(trans.Find("Label_title"), titleText);
        GameHelper.SetLabelText(trans.Find("Label_score"), mGameScore.ToString());
        GameHelper.SetLabelText(trans.Find("Label_des"), desText);
    }
}
