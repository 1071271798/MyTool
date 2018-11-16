using Game;
using Game.Platform;
using System.Collections.Generic;
using UnityEngine;

public class PopInputMsg : BasePopWin
{
    #region 公有属性
    public ButtonDelegate.OnClick onBtnClick;
    public const string LeftBtnName = "leftBtn";
    public const string RightBtnName = "rightBtn";


    public UIButton mLeftBtn;
    public UIButton mRightBtn;
    public UIInput mInput;
    #endregion

    #region 私有属性
    string mTitleText;
    string mLeftBtnText;
    string mRightBtnText;
    string mDefaultText;
    string mInputText;
    UISprite mInputBg;

    UIInput.OnSelectDelegate mSelectDlgt;
    #endregion

    #region 公有函数
    public PopInputMsg(string titleText, string defaultText, string inputText, ButtonDelegate.OnClick onClick)
    {
        mUIResPath = "Prefab/UI/PromptRenameMsg";
        mTitleText = titleText;
        mDefaultText = defaultText;
        mInputText = inputText;
        mLeftBtnText = LauguageTool.GetIns().GetText("取消");
        mRightBtnText = LauguageTool.GetIns().GetText("确定");
        onBtnClick = onClick;
        isSingle = true;
    }
    
    
    /// <summary>
    /// 弹出两个按钮的弹框
    /// </summary>
    /// <param name="text"></param>
    /// <param name="btnOnClick"></param>
    public static void ShowPopInputMsg(string titleText, ButtonDelegate.OnClick btnOnClick)
    {
        object[] args = new object[4];
        args[0] = titleText;
        args[1] = string.Empty;
        args[2] = string.Empty;
        args[3] = btnOnClick;
        PopWinManager.GetInst().ShowPopWin(typeof(PopInputMsg), args);
    }

    public static void ShowPopInputMsg(string titleText, string defaultText, string inputText, ButtonDelegate.OnClick btnOnClick)
    {
        object[] args = new object[4];
        args[0] = titleText;
        args[1] = defaultText;
        args[2] = inputText;
        args[3] = btnOnClick;
        PopWinManager.GetInst().ShowPopWin(typeof(PopInputMsg), args);
    }

    public override void Init()
    {
        base.Init();
    }

    public void SetOnValidate(UIInput.OnValidate dlgt)
    {
        if (null != mInput)
        {
            mInput.onValidate = dlgt;
        }
    }

    public void SetOnSelect(UIInput.OnSelectDelegate dlgt)
    {
        mSelectDlgt = dlgt;
    }
    #endregion


    protected override void AddEvent()
    {
        base.AddEvent();
        if (null != mTrans)
        {
            Transform bgbg = mTrans.Find("bgbg");
            if (null != bgbg)
            {
                //UITexture mohu1 = GameHelper.FindChildComponent<UITexture>(bgbg, "mohu1");
                UITexture mohu2 = GameHelper.FindChildComponent<UITexture>(bgbg, "mohu2");
                if (/*null != mohu1 && */null != mohu2)
                {
                    //mohu1.width = PublicFunction.GetExtendWidth();
                    //mohu1.height = PublicFunction.GetExtendHeight();
                    mohu2.width = PublicFunction.GetExtendWidth();
                    mohu2.height = PublicFunction.GetExtendHeight();
#if UNITY_ANDROID
                    mohu2.gameObject.SetActive(false);
#endif
                }
            }
            Transform bg = mTrans.Find("bg");
            Transform title = mTrans.Find("title");
            if (null != title)
            {
                SetTitleLabel(title);
            }
            Transform contents = mTrans.Find("contents");
            if (null != contents)
            {
                mInput = GameHelper.FindChildComponent<UIInput>(contents, "Input");
                if (null != mInput)
                {
                    mInputBg = GameHelper.FindChildComponent<UISprite>(mInput.transform, "Background");
                    mInput.defaultText = mDefaultText;
                    mInput.value = mInputText;
                    mInput.onSelect = OnInputSelect;
                }
            }
            Transform btn = mTrans.Find("btn");
            if (null != btn)
            {
                SetBtn(btn);
            }
        }
    }


    protected override void OnButtonClick(GameObject obj)
    {
        if (obj.name.Equals(LeftBtnName) || obj.name.Equals(RightBtnName))
        {
            if (null != onBtnClick)
            {
                onBtnClick(obj);
            }
            OnClose();
        }
        
    }
    #region 私有函数

    void SetTitleText(string text)
    {
        mTitleText = text;
    }

    
    void SetLeftBtnText(string text)
    {
        mLeftBtnText = text;
    }

    void SetRightBtnText(string text)
    {
        mRightBtnText = text;
    }


    /// <summary>
    /// 设置标题
    /// </summary>
    /// <param name="obj"></param>
    void SetTitleLabel(Transform obj)
    {
        Transform label = obj.Find("Label");
        if (null != label)
        {
            UILabel lbText = label.GetComponent<UILabel>();
            if (null != lbText)
            {
                lbText.text = mTitleText;
            }
        }
    }

    void SetBtn(Transform obj)
    {
        Transform leftBtn = obj.Find(LeftBtnName);

        Transform rightBtn = obj.Find(RightBtnName);

        if (null != rightBtn)
        {
            mRightBtn = rightBtn.GetComponent<UIButton>();
            Transform label = rightBtn.Find("Label");
            if (null != label)
            {
                UILabel lbText = label.GetComponent<UILabel>();
                if (null != lbText)
                {
                    lbText.text = mRightBtnText;
                }
            }
        }
        if (null != leftBtn)
        {
            mLeftBtn = leftBtn.GetComponent<UIButton>();
            Transform label = leftBtn.Find("Label");
            if (null != label)
            {
                UILabel lbText = label.GetComponent<UILabel>();
                if (null != lbText)
                {
                    lbText.text = mLeftBtnText;
                }
            }
        }
    }

    void OnInputSelect(bool isSelect, GameObject obj)
    {
        try
        {
            if (!isSelect)
            {
                if (null != mInputBg)
                {
                    mInputBg.color = Color.white;
                }
            }
            else
            {
                if (null != mInputBg)
                {
                    mInputBg.color = new Color32(57, 197, 233, 255);
                }
            }
            if (null != mSelectDlgt)
            {
                mSelectDlgt(isSelect, obj);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    #endregion


}
