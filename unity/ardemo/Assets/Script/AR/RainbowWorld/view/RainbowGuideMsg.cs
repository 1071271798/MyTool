using Game.Platform;
using UnityEngine;

public class RainbowGuideMsg : BasePopWin
{
    RainbowGuideData mGuideData;
    UILabel mLabel_text;
    static RainbowGuideMsg sInst;
    public RainbowGuideMsg(RainbowGuideData data)
    {
        mGuideData = data;
        mUIResPath = "ARGame/Msg/rainbowGuideMsg";
        isSingle = true;
        sInst = this;
    }

    public static void ShowMsg(RainbowGuideData data)
    {
        if (null == sInst)
        {
            object[] args = new object[1];
            args[0] = data;
            PopWinManager.GetInst().ShowPopWin(typeof(RainbowGuideMsg), args);
        } else
        {
            sInst.MoveNext(data);
        }
    }

    public override void Init()
    {
        base.Init();
        mInitDepth = 990;
    }

    protected override void Close()
    {
        base.Close();
        sInst = null;
    }

    protected override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            if (null != mTrans)
            {
                Transform topLeft = mTrans.Find("topLeft");
                if (null != topLeft)
                {
                    GameHelper.SetPosition(topLeft, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos);
                    GameHelper.SetLabelText(topLeft.Find("Btn_skip/Label"), LauguageTool.GetIns().GetText("跳过"));
                }
                Transform bottom = mTrans.Find("bottom");
                if (null != bottom)
                {
                    GameHelper.SetPosition(bottom, UIWidget.Pivot.Bottom, new Vector2(0, 100));
                    GameHelper.SetLabelText(bottom.Find("Btn_next/Label"), LauguageTool.GetIns().GetText("确定"));
                }
                UIWidget fullClick = GameHelper.FindChildComponent<UIWidget>(mTrans, "Btn_next");
                if (null != fullClick)
                {
                    fullClick.width = PublicFunction.GetExtendWidth();
                    fullClick.height = PublicFunction.GetExtendHeight();
                }
                mLabel_text = GameHelper.FindChildComponent<UILabel>(mTrans, "Label_text");
                SetUIStyle();
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
            string name = obj.name;
            if (name.Equals("Btn_next"))
            {//下一步
                mGuideData = SingletonObject<RainbowWorldCtrl>.GetInst().GuideMoveNext();
                if (null == mGuideData)
                {
                    OnClose();
                }
            } else if (name.Equals("Btn_skip"))
            {//跳过
                mGuideData = SingletonObject<RainbowWorldCtrl>.GetInst().SkipGuide();
                if (null == mGuideData)
                {
                    OnClose();
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void MoveNext(RainbowGuideData data)
    {
        mGuideData = data;
        SetUIStyle();
    }

    void SetUIStyle()
    {
        if (null != mGuideData)
        {
            if (null != mLabel_text)
            {
                mLabel_text.text = mGuideData.text;
            }
            if (mGuideData.style == ERainbowGuideStyle.Style_Hide)
            {
                OnHide();
            } else
            {
                OnShow();
            }
        }
    }
}