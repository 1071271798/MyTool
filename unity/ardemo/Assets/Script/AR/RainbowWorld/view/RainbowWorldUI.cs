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
    UILabel mAgainCountLabel;

    long mShowRainbowIndex = -1;
    long mShowColorIndex = -1;

    int mColorCount;

    Dictionary<EColorType, DoubleColorBtnData> mInputBtnDict;

    Transform mReadyTrans;
    UILabel mReadyLabel;

    Transform mTopRightTrans;
    Transform mTopLeftTrans;
    Transform mColorCountTrans;

    GameObject mBottomLeftObj;
    GameObject mBottomRightObj;

    EColorType mTargetColor;

    UIButton mAgainButton;

    public RainbowWorldUI()
    {
        mUIResPath = "ARGame/UI/rainbowWorldUI";
    }

    public void StartGame()
    {
        SingletonObject<RainbowWorldCtrl>.GetInst().GameStart(EGameDifficulty.Easy, GameStateChangeCallBack, ShowColorCallBack, GameLevelUpCallBack, GameScoreChangeCallBack, ShowReadNumCallback);
    }
    /// <summary>
    /// 显示颜色按钮灯光特效
    /// </summary>
    /// <param name="color"></param>
    public void ShowLightForInputBtn(EColorType color)
    {
        if (ColorUtils.IsTrichromatic(color))
        {
            if (mInputBtnDict.ContainsKey(color))
            {
                mInputBtnDict[color].ShowEffect();
            }
        }
        else
        {
            List<EColorType> list = ColorUtils.GetDecomposeColor(color);
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                if (mInputBtnDict.ContainsKey(list[i]))
                {
                    mInputBtnDict[list[i]].ShowEffect();
                }
            }
        }
    }
    /// <summary>
    /// 设置颜色数量
    /// </summary>
    /// <param name="count"></param>
    public void SetColorCount(int count)
    {
        mColorCount = count;
        SetColorCountText(count);
    }
    /// <summary>
    /// 设置重玩次数
    /// </summary>
    /// <param name="count"></param>
    public void SetAgainCount(int count)
    {
        if (null != mAgainCountLabel)
        {
            mAgainCountLabel.text = count.ToString();
        }
        if (null != mAgainButton)
        {
            if (count <= 0)
            {
                mAgainButton.OnSleep();
            } else if (count == RainbowGlobal.GameResetLevelCountMax)
            {
                mAgainButton.OnAwake();
            }
        }
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
        SingletonObject<RainbowWorldCtrl>.GetInst().UnRegisterGameUI();
    }

    protected override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            SingletonObject<RainbowWorldCtrl>.GetInst().RegisterGameUI(this);
            if (null != mTrans)
            {
                Transform bottomLeft = mTrans.Find("bottomLeft");
                Transform bottomRight = mTrans.Find("bottomRight");
                SetColorBtnLayout(bottomLeft, UIWidget.Pivot.BottomLeft);
                SetColorBtnLayout(bottomRight, UIWidget.Pivot.BottomRight);
                mInputBtnDict = new Dictionary<EColorType, DoubleColorBtnData>();
                if (null != bottomLeft)
                {
                    mBottomLeftObj = bottomLeft.gameObject;
                    for (int i = 0, imax = ColorUtils.Trichromatic.Length; i < imax; ++i)
                    {
                        Transform btn = bottomLeft.Find(string.Format("Btn_L_{0}", ColorUtils.Trichromatic[i]));
                        if (null != btn)
                        {
                            DoubleColorBtnData data = new DoubleColorBtnData();
                            data.InitBtnData(EInputDirection.Input_Left, btn.gameObject);
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
                            if (mInputBtnDict.ContainsKey(ColorUtils.Trichromatic[i]))
                            {
                                mInputBtnDict[ColorUtils.Trichromatic[i]].InitBtnData(EInputDirection.Input_Right, btn.gameObject);
                            } else
                            {
                                DoubleColorBtnData data = new DoubleColorBtnData();
                                data.InitBtnData(EInputDirection.Input_Right, btn.gameObject);
                                mInputBtnDict[ColorUtils.Trichromatic[i]] = data;
                            }
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
                    GameHelper.SetPosition(mTopRightTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos);
                    mAgainButton = GameHelper.FindChildComponent<UIButton>(mTopRightTrans, "Btn_reset");
                    mAgainCountLabel = GameHelper.FindChildComponent<UILabel>(mTopRightTrans, "Btn_reset/Label");
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
                case "Btn_reset":
                    {
                        SingletonObject<RainbowWorldCtrl>.GetInst().AgainNowLevel();
                    }
                    return;
                case "Btn_pause":
                    return;
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
                if (EColorType.None != mTargetColor)
                {
                    HideLightForInputBtn(mTargetColor);
                }
                EColorInputState state = SingletonObject<RainbowWorldCtrl>.GetInst().ClickColor(direction, inputColor);
                if (state == EColorInputState.Input_Success)
                {
                    ShowInputColor(mTargetColor);
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
    void GameStateChangeCallBack(EGameState state, params object[] args)
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
            case EGameState.Game_Input_Finished:
                OnHideAllInputBtn();
                SingletonObject<UIManager>.GetInst().CloseMultiTouch();
                break;
            case EGameState.Game_Open_Input:
                OnShowAllInputBtn();
                SingletonObject<UIManager>.GetInst().OpenMultiTouch();
                break;
            case EGameState.Game_Input:
                if (null != args && args.Length > 0)
                {
                    mTargetColor = (EColorType)args[0];
                }
                break;
            case EGameState.Game_Failure:
                SingletonObject<UIManager>.GetInst().CloseMultiTouch();
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
    /// <param name="changeScore"></param>
    /// <param name="totalScore"></param>
    void GameScoreChangeCallBack(int changeScore, int totalScore)
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
    public void OnHideGameUI()
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
    public void OnShowGameUI()
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
        SetColorCountText(mColorCount);
    }
    /// <summary>
    /// 减少计数
    /// </summary>
    void SubColorCount()
    {
        --mColorCount;
        SetColorCountText(mColorCount);
    }

    void SetColorCountText(int count)
    {
        if (null != mColorCountNumLabel)
        {
            mColorCountNumLabel.text = count.ToString();
        }
    }

    void HideLightForInputBtn(EColorType color)
    {
        if (null == mInputBtnDict || mInputBtnDict.Count < 1)
        {
            return;
        }
        if (ColorUtils.IsTrichromatic(color))
        {
            if (mInputBtnDict.ContainsKey(color))
            {
                mInputBtnDict[color].HideEffect();
            }
        } else
        {
            List<EColorType> list = ColorUtils.GetDecomposeColor(color);
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                if (mInputBtnDict.ContainsKey(list[i]))
                {
                    mInputBtnDict[list[i]].HideEffect();
                }
            }
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
    class DoubleColorBtnData
    {
        public ColorBtnData leftBtn;
        public ColorBtnData rightBtn;

        public DoubleColorBtnData()
        {

        }

        public void InitBtnData(EInputDirection direction, GameObject obj)
        {
            if (direction == EInputDirection.Input_Left)
            {
                leftBtn = new ColorBtnData(direction, obj);
            } else
            {
                rightBtn = new ColorBtnData(direction, obj);
            }
        }

        public void ShowEffect()
        {
            if (null != leftBtn)
            {
                leftBtn.ShowEffect();
            }
            if (null != rightBtn)
            {
                rightBtn.ShowEffect();
            }
        }

        public void HideEffect()
        {
            if (null != leftBtn)
            {
                leftBtn.HideEffect();
            }
            if (null != rightBtn)
            {
                rightBtn.HideEffect();
            }
        }
    }

    class ColorBtnData
    {
        public EInputDirection direction;
        public GameObject btnGameObject;
        public GameObject effectGameObject;
        public UIButton button;

        public ColorBtnData(EInputDirection direction, GameObject obj)
        {
            this.direction = direction;
            this.btnGameObject = obj;
            Transform effect = obj.transform.Find("effect");
            {
                effectGameObject = effect.gameObject;
            }
            button = obj.GetComponent<UIButton>();
        }

        public void ShowEffect()
        {
            if (null != effectGameObject)
            {
                effectGameObject.SetActive(true);
            }
        }

        public void HideEffect()
        {
            if (null != effectGameObject)
            {
                effectGameObject.SetActive(false);
            }
        }
    }

    
}
