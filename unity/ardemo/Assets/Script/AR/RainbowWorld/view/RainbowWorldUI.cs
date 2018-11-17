using UnityEngine;
using Game.Platform;
using Game;
using System.Collections.Generic;
using System.Collections;

public class RainbowWorldUI : BaseUI {
    Transform mRainbowTrans;
    UISprite mRainbowSprite;

    Transform mShowColorTrans;
    UISprite mShowColorSprite;

    UILabel mGameLevelNumLabel;
    UILabel mGameScoreNumLabel;
    UILabel mColorCountNumLabel;

    long mShowRainbowIndex = -1;
    long mShowColorIndex = -1;

    int mColorCount;

    Dictionary<EColorType, ColorBtnData> mInputBtnDict;

    Transform mReadyTrans;
    UILabel mReadyLabel;

    Transform mTopRightTrans;
    Transform mTopLeftTrans;
    Transform mColorCountTrans;

    GameObject mBottomLeftObj;
    GameObject mBottomRightObj;

    public RainbowWorldUI()
    {
        mUIResPath = "ARGame/UI/rainbowWorldUI";
    }

    public void StartGame()
    {
        SingletonObject<RainbowWorldCtrl>.GetInst().GameStart(EGameDifficulty.Easy, GameStateChangeCallBack, ShowColorCallBack, GameLevelUpCallBack, GameScoreChangeCallBack, ShowReadNumCallback);
    }

    protected override void Close()
    {
        base.Close();
        if (-1 != mShowRainbowIndex)
        {
            Timer.Cancel(mShowRainbowIndex);
        }
        if (-1 != mShowColorIndex)
        {
            Timer.Cancel(mShowColorIndex);
        }
    }

    protected override void AddEvent()
    {
        try
        {
            base.AddEvent();
            if (null != mTrans)
            {
                Transform bottomLeft = mTrans.Find("bottomLeft");
                Transform bottomRight = mTrans.Find("bottomRight");
                SetColorBtnLayout(bottomLeft, UIWidget.Pivot.BottomLeft);
                SetColorBtnLayout(bottomRight, UIWidget.Pivot.BottomRight);
                mInputBtnDict = new Dictionary<EColorType, ColorBtnData>();
                if (null != bottomLeft)
                {
                    mBottomLeftObj = bottomLeft.gameObject;
                    for (int i = 0, imax = ColorUtils.Trichromatic.Length; i < imax; ++i)
                    {
                        Transform btn = bottomLeft.Find(string.Format("Btn_L_{0}", ColorUtils.Trichromatic[i]));
                        if (null != btn)
                        {
                            ColorBtnData data = new ColorBtnData(EInputDirection.Input_Left, btn.gameObject);
                            mInputBtnDict[ColorUtils.Trichromatic[i]] = data;
                        }
                    }
                    mBottomLeftObj.SetActive(false);
                }
                if (null != bottomRight)
                {
                    mBottomRightObj = bottomRight.gameObject;
                    for (int i = 0, imax = ColorUtils.Trichromatic.Length; i < imax; ++i)
                    {
                        Transform btn = bottomRight.Find(string.Format("Btn_R_{0}", ColorUtils.Trichromatic[i]));
                        if (null != btn)
                        {
                            ColorBtnData data = new ColorBtnData(EInputDirection.Input_Right, btn.gameObject);
                            mInputBtnDict[ColorUtils.Trichromatic[i]] = data;
                        }
                    }
                    mBottomRightObj.SetActive(false);
                }
                mRainbowTrans = mTrans.Find("rainbow");
                if (null != mRainbowTrans)
                {
                    mRainbowSprite = mRainbowTrans.GetComponent<UISprite>();
                    mRainbowTrans.localPosition = new Vector3(-60, 240);
                    GameHelper.SetTransformAlpha(mRainbowTrans, 0);
                }
                mShowColorTrans = mTrans.Find("showColor");
                if (null != mShowColorTrans)
                {
                    mShowColorTrans.localPosition = UIManager.GetWinPos(mShowColorTrans, UIWidget.Pivot.Bottom, 0, PublicFunction.Bottom_Margin_x);
                    mShowColorSprite = GameHelper.FindChildComponent<UISprite>(mShowColorTrans, "icon");
                    GameHelper.SetTransformAlpha(mShowColorTrans, 0);
                }
                
                mTopLeftTrans = mTrans.Find("topLeft");
                if (null != mTopLeftTrans)
                {
                    GameHelper.SetPosition(mTopLeftTrans, UIWidget.Pivot.TopLeft, new Vector2(-300, PublicFunction.Margin_Normal.y));
                    mGameLevelNumLabel = GameHelper.FindChildComponent<UILabel>(mTopLeftTrans, "level/num");
                    mGameScoreNumLabel = GameHelper.FindChildComponent<UILabel>(mTopLeftTrans, "score/num");
                    SetGameLevel(0);
                    SetGameScore(0);
                }
                mTopRightTrans = mTrans.Find("topRight");
                if (null != mTopRightTrans)
                {
                    GameHelper.SetPosition(mTopRightTrans, UIWidget.Pivot.TopRight, new Vector2(-300, PublicFunction.Margin_Normal.y));
                }
                OnHideAllInputBtn();
                mReadyTrans = mTrans.Find("ready");
                if (null != mReadyTrans)
                {
                    mReadyLabel = mReadyTrans.GetComponent<UILabel>();
                }
                mColorCountTrans = mTrans.Find("colorCount");
                if (null != mColorCountTrans)
                {
                    GameHelper.SetPosition(mColorCountTrans, UIWidget.Pivot.Left, new Vector2(-300, 100));
                    mColorCountNumLabel = GameHelper.FindChildComponent<UILabel>(mColorCountTrans, "num");
                    SetColorCount(0);
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
            string name = obj.name;
            bool inputColorFlag = false;
            EInputDirection direction = EInputDirection.Input_Left;
            EColorType inputColor = EColorType.None;
            switch (name)
            {
                case "Btn_L_R":
                    inputColorFlag = true;
                    direction = EInputDirection.Input_Left;
                    inputColor = EColorType.R;
                    break;
                case "Btn_L_G":
                    inputColorFlag = true;
                    direction = EInputDirection.Input_Left;
                    inputColor = EColorType.G;
                    break;
                case "Btn_L_B":
                    inputColorFlag = true;
                    direction = EInputDirection.Input_Left;
                    inputColor = EColorType.B;
                    break;
                case "Btn_R_R":
                    inputColorFlag = true;
                    direction = EInputDirection.Input_Right;
                    inputColor = EColorType.R;
                    break;
                case "Btn_R_G":
                    inputColorFlag = true;
                    direction = EInputDirection.Input_Right;
                    inputColor = EColorType.G;
                    break;
                case "Btn_R_B":
                    inputColorFlag = true;
                    direction = EInputDirection.Input_Right;
                    inputColor = EColorType.B;
                    break;
            }
            if (inputColorFlag)
            {
                EColorType targetColor = SingletonObject<RainbowWorldCtrl>.GetInst().GetNowColorOut();
                EColorInputState state = SingletonObject<RainbowWorldCtrl>.GetInst().ClickColor(direction, inputColor);
                if (state == EColorInputState.Input_Success)
                {
                    ShowInputColor(targetColor);
                    SubColorCount();
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void Update()
    {
        base.Update();
        SingletonObject<RainbowWorldCtrl>.GetInst().OnUpdate();
    }

    /// <summary>
    /// 游戏状态改变回调
    /// </summary>
    /// <param name="state"></param>
    void GameStateChangeCallBack(EGameState state)
    {
        switch (state)
        {
            case EGameState.Game_Ready:
                OnHideGameUI();
                break;
            case EGameState.Game_Show_Color:
                mColorCount = 0;
                OnHideAllInputBtn();
                break;
            case EGameState.Game_Wait:
                OnHideAllInputBtn();
                SingletonObject<UIManager>.GetInst().CloseMultiTouch();
                break;
            case EGameState.Game_Input:
                OnShowAllInputBtn();
                SingletonObject<UIManager>.GetInst().OpenMultiTouch();
                break;
            case EGameState.Game_Over:
                GameOver();
                break;
        }
    }
    /// <summary>
    /// 游戏关卡提升
    /// </summary>
    /// <param name="newLevel"></param>
    void GameLevelUpCallBack(int newLevel)
    {
        SetGameLevel(newLevel);
    }
    /// <summary>
    /// 游戏分数改变
    /// </summary>
    /// <param name="addScore"></param>
    /// <param name="totalScore"></param>
    void GameScoreChangeCallBack(int addScore, int totalScore)
    {
        SetGameScore(totalScore);
    }
    /// <summary>
    /// 设置颜色按钮位置
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="pivot"></param>
    void SetColorBtnLayout(Transform trans, UIWidget.Pivot pivot)
    {
        if (null != trans)
        {
            trans.localPosition = UIManager.GetWinPos(trans, pivot, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
            Transform tmp = null;
            int r = 250;
            if (pivot == UIWidget.Pivot.BottomLeft)
            {
                for (int i = 0, imax = trans.childCount; i < imax; ++i)
                {
                    tmp = trans.GetChild(i);
                    tmp.localPosition = new Vector3(r * Mathf.Sin(i * Mathf.PI * 2 / 8), r * Mathf.Cos(i * Mathf.PI * 2 / 8));
                }
            } else if (pivot == UIWidget.Pivot.BottomRight)
            {
                for (int i = 0, imax = trans.childCount; i < imax; ++i)
                {
                    tmp = trans.GetChild(i);
                    tmp.localPosition = new Vector3(r * Mathf.Sin((8 - i) * Mathf.PI * 2 / 8), r * Mathf.Cos((8 - i) * Mathf.PI * 2 / 8));
                }
            }
        }
    }
    /// <summary>
    /// 显示彩虹颜色
    /// </summary>
    /// <param name="color"></param>
    void ShowRainbowColor(string color)
    {
        if (null != mRainbowSprite)
        {
            mRainbowSprite.spriteName = "rainbow_" + color;
        }
        if (null != mRainbowTrans)
        {
            GameHelper.SetTransformAlpha(mRainbowTrans, 0);
        }
        GameHelper.PlayTweenAlpha(mRainbowTrans, 1);
        if (-1 != mShowRainbowIndex)
        {
            Timer.Cancel(mShowRainbowIndex);
        }
        mShowRainbowIndex = Timer.Add(2, 0, 0, delegate () {
            GameHelper.PlayTweenAlpha(mRainbowTrans, 0);
        });
    }
    /// <summary>
    /// 显示按钮输入颜色
    /// </summary>
    /// <param name="colorType"></param>
    void ShowInputColor(EColorType colorType)
    {
        string color = colorType.ToString();
        if (null != mShowColorSprite)
        {
            mShowColorSprite.spriteName = color;
        }
        if (null != mShowColorTrans)
        {
            GameHelper.SetTransformAlpha(mShowColorTrans, 0);
        }
        GameHelper.PlayTweenAlpha(mShowColorTrans, 1);
        if (-1 != mShowColorIndex)
        {
            Timer.Cancel(mShowColorIndex);
        }
        mShowColorIndex = Timer.Add(1, 0, 0, delegate ()
        {
            mShowColorIndex = -1;
            GameHelper.PlayTweenAlpha(mShowColorTrans, 0);
        });
    }
    /// <summary>
    /// 隐藏颜色按钮
    /// </summary>
    void OnHideAllInputBtn()
    {
        if (null != mBottomLeftObj)
        {
            mBottomLeftObj.SetActive(false);
        }
        if (null != mBottomRightObj)
        {
            mBottomRightObj.SetActive(false);
        }
    }
    /// <summary>
    /// 显示颜色按钮
    /// </summary>
    void OnShowAllInputBtn()
    {
        if (null != mBottomLeftObj)
        {
            mBottomLeftObj.SetActive(true);
        }
        if (null != mBottomRightObj)
        {
            mBottomRightObj.SetActive(true);
        }
    }
    /// <summary>
    /// 隐藏游戏UI
    /// </summary>
    void OnHideGameUI()
    {
        if (null != mTopLeftTrans)
        {
            GameHelper.PlayTweenPosition(mTopLeftTrans, UIWidget.Pivot.TopLeft, new Vector2(-300, PublicFunction.Margin_Normal.y));
        }
        if (null != mTopRightTrans)
        {
            GameHelper.PlayTweenPosition(mTopRightTrans, UIWidget.Pivot.TopRight, new Vector2(-300, PublicFunction.Margin_Normal.y));
        }
        if (null != mColorCountTrans)
        {
            GameHelper.PlayTweenPosition(mColorCountTrans, UIWidget.Pivot.Left, new Vector2(-300, 100));
        }
    }
    /// <summary>
    /// 显示游戏UI
    /// </summary>
    void OnShowGameUI()
    {
        if (null != mTopLeftTrans)
        {
            GameHelper.PlayTweenPosition(mTopLeftTrans, UIWidget.Pivot.TopLeft, PublicFunction.Margin_Normal);
        }
        if (null != mTopRightTrans)
        {
            GameHelper.PlayTweenPosition(mTopRightTrans, UIWidget.Pivot.TopRight, PublicFunction.Margin_Normal);
        }
        if (null != mColorCountTrans)
        {
            GameHelper.PlayTweenPosition(mColorCountTrans, UIWidget.Pivot.Left, new Vector2(PublicFunction.Margin_Normal.x, 100));
        }
    }
    
    /// <summary>
    /// 显示倒计时
    /// </summary>
    /// <param name="num"></param>
    void ShowReadNumCallback(int num)
    {
        if (null != mReadyTrans && null != mReadyLabel)
        {
            if (num > 0)
            {
                if (!mReadyTrans.gameObject.activeSelf)
                {
                    mReadyTrans.gameObject.SetActive(true);
                }
                mReadyLabel.text = num.ToString();
                GameHelper.SetTransformAlpha(mReadyTrans, 1);
                mReadyTrans.localScale = Vector3.one;
                GameHelper.PlayTweenAlpha(mReadyTrans, 0, 1);
                GameHelper.PlayTweenScale(mReadyTrans, Vector2.one * 2, 1);
            }
            else
            {
                OnShowGameUI();
                mReadyTrans.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 设置显示关卡
    /// </summary>
    /// <param name="level"></param>
    void SetGameLevel(int level)
    {
        if (null != mGameLevelNumLabel)
        {
            mGameLevelNumLabel.text = level.ToString();
        }
    }
    /// <summary>
    /// 设置显示积分
    /// </summary>
    /// <param name="score"></param>
    void SetGameScore(int score)
    {
        if (null != mGameScoreNumLabel)
        {
            mGameScoreNumLabel.text = score.ToString();
        }
    }
    /// <summary>
    /// 增加计数
    /// </summary>
    void AddColorCount()
    {
        mColorCount++;
        SetColorCount(mColorCount);
    }
    /// <summary>
    /// 减少计数
    /// </summary>
    void SubColorCount()
    {
        --mColorCount;
        SetColorCount(mColorCount);
    }

    void SetColorCount(int count)
    {
        if (null != mColorCountNumLabel)
        {
            mColorCountNumLabel.text = count.ToString();
        }
    }

    /// <summary>
    /// 显示颜色
    /// </summary>
    /// <param name="color"></param>
    /// <param name="lastColorFlag"></param>
    void ShowColorCallBack(EColorType color, bool lastColorFlag)
    {
        if (color != EColorType.None)
        {
            ShowRainbowColor(color.ToString());
            AddColorCount();
        }
    }

    void GameOver()
    {
        OnHideAllInputBtn();
    }




    //////////////////////////////////////////////////////////////////////////
    class ColorBtnData
    {
        public EInputDirection direction;
        public GameObject btnGameObject;
        public UIButton button;

        public ColorBtnData(EInputDirection direction, GameObject obj)
        {
            this.direction = direction;
            this.btnGameObject = obj;
            button = obj.GetComponent<UIButton>();
        }
    }
}
