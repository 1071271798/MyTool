using UnityEngine;
using Game.Platform;

public class RainbowRewardMsg : BasePopWin
{
    RainbowPlayerData mPlayerData;
    public RainbowRewardMsg()
    {
        mUIResPath = "ARGame/Msg/rainbowRewardMsg";
        isSingle = true;
        mPlayerData = new RainbowPlayerData();
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
                GameHelper.SetPosition(mTrans.Find("Btn_close"), UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos);
                Transform title = mTrans.Find("Label_title");
                if (null != title)
                {
                    GameHelper.SetPosition(title, UIWidget.Pivot.Top, new Vector2(0, PublicFunction.Title_Margin));
                    GameHelper.SetLabelText(title, "My Reward");
                }
                Transform score = mTrans.Find("Label_score");
                if (null != score)
                {
                    GameHelper.SetPosition(score, UIWidget.Pivot.Center, new Vector2(0, 200));
                    GameHelper.SetLabelText(score, string.Format("High Score: {0}", mPlayerData.GetHighScore(EGameDifficulty.Easy)));
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
}