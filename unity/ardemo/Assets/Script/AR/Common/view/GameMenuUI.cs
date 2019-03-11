using Game.Platform;
using Game.Scene;
using UnityEngine;

public class GameMenuUI : BaseUI
{
    UILabel mDifficultyLabel;
    UIButton mDifficultyLeftBtn;
    UIButton mDifficultyRightBtn;
    RainbowPlayerData mPlayerData;
    EGameMenuType mMenuType;

    GameObject mMenuGameObject;
    GameObject mGreetingGameObject;

    public GameMenuUI(EGameMenuType menuType)
    {
        mUIResPath = "ARGame/UI/gameMenu";
        mPlayerData = new RainbowPlayerData();
        mMenuType = menuType;
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
                    GameHelper.SetPosition(content, UIWidget.Pivot.Center, new Vector2(120, 0));
                    Transform btn = content.Find("btn");
                    if (null != btn)
                    {
                        mMenuGameObject = btn.gameObject;
                    }
                    Transform greeting = content.Find("greeting");
                    if (null != greeting)
                    {
                        mGreetingGameObject = greeting.gameObject;
                    }
                }
                
                switch (mMenuType)
                {
                    case EGameMenuType.RainbowWorld:
                        {
                            if (mPlayerData.IsShowGreeting())
                            {
                                SetMenuGameObject(false);
                                SetGreetingGameObject(true);
                                InitGreetingUI();
                            } else
                            {
                                SetMenuGameObject(true);
                                SetGreetingGameObject(false);
                                InitDifficultyUI();
                            }
                        }
                        break;
                    case EGameMenuType.RainbowWorld_HowToPlay:
                        break;
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
            switch (name)
            {
                case "Btn_Help":
                    {

                    }
                    break;
                case "Btn_Reward":
                    {
                        ClickBtnReward();
                    }
                    break;
                case "Btn_Start":
                    {
                        ClickBtnStart();
                    }
                    break;
                case "Btn_left":
                    {
                        mPlayerData.SetSelectedDifficulty(EGameDifficulty.Easy);
                        SetDifficultyUI(EGameDifficulty.Easy);
                    }
                    break;
                case "Btn_right":
                    {
                        mPlayerData.SetSelectedDifficulty(EGameDifficulty.Hard);
                        SetDifficultyUI(EGameDifficulty.Hard);
                    }
                    break;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SetMenuGameObject(bool active)
    {
        if (null != mMenuGameObject)
        {
            mMenuGameObject.SetActive(active);
        }
    }

    void SetGreetingGameObject(bool active)
    {
        if (null != mGreetingGameObject)
        {
            mGreetingGameObject.SetActive(active);
        }
    }

    void ClickBtnStart()
    {
        if (mMenuType == EGameMenuType.RainbowWorld)
        {
            //SceneMgr.EnterScene(SceneType.EmptyScene, typeof(ARScene));
            SceneMgr.EnterScene(SceneType.EmptyScene, typeof(RainbowWorldScene));
        }
    }

    void ClickBtnReward()
    {
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(RainbowRewardMsg));
    }

    void InitDifficultyUI()
    {
        Transform difficulty = mTrans.Find("content/btn/difficulty");
        if (null != difficulty)
        {
            mDifficultyLeftBtn = GameHelper.FindChildComponent<UIButton>(difficulty, "Btn_left");
            mDifficultyRightBtn = GameHelper.FindChildComponent<UIButton>(difficulty, "Btn_right");
            mDifficultyLabel = GameHelper.FindChildComponent<UILabel>(difficulty, "Label");

            SetDifficultyUI(mPlayerData.GetSelectedDifficulty());
        }
    }

    void InitGreetingUI()
    {
        Transform panel = mTrans.Find("content/greeting/panel");
        if (null == panel)
        {
            return;
        }
        
        Transform grid = panel.Find("grid");
        if (null == grid)
        {
            return;
        }
        int height = 0;
        Transform title = grid.Find("Lab_title");
        if (null != title)
        {
            UILabel lb = title.GetComponent<UILabel>();
            if (null != lb)
            {
                lb.text = "Greetings Jimu Master！";
                Vector2 size = NGUIText.CalculatePrintedSize(lb.text, lb, 30);
                lb.height = Mathf.CeilToInt(size.y);
                height = lb.height;
                lb.transform.localPosition = new Vector3(0, -height / 2.0f, 0);
            }
        }
        Transform content = grid.Find("Lab_content");
        if (null != content)
        {
            UILabel lb = content.GetComponent<UILabel>();
            if (null != lb)
            {
                lb.text = "My name is Melody. \n\n Are you the owner of this proud UnicornBot? \n\n Lets have some fun and unlock some magical light shows featuring me and my fairy friends. \n\n This game is a fun, color and memorization game. \n\n Lets get started!";
                Vector2 size = NGUIText.CalculatePrintedSize(lb.text, lb, 46);
                lb.height = Mathf.CeilToInt(size.y);
                lb.transform.localPosition = new Vector3(0, -height - 25 - lb.height/2.0f , 0);
            }
        }
    }


    void SetDifficultyUI(EGameDifficulty difficulty)
    {
        if (mPlayerData.IsHardUnlockFlag())
        {
            if (difficulty == EGameDifficulty.Easy)
            {
                OnSleepLeftBtn();
                OnAwakeRightBtn();
            } else
            {
                OnAwakeLeftBtn();
                OnSleepRightBtn();
            }
        } else
        {
            OnSleepLeftBtn();
            OnSleepRightBtn();
        }
        SetDifficultyLabel(difficulty);
    }

    void SetDifficultyLabel(EGameDifficulty difficulty)
    {
        if (difficulty == EGameDifficulty.Easy)
        {
            if (null != mDifficultyLabel)
            {
                mDifficultyLabel.text = "Beginner";
            }
        } else
        {
            if (null != mDifficultyLabel)
            {
                mDifficultyLabel.text = "Advanced";
            }
        }
    }

    void OnSleepLeftBtn()
    {
        if (null != mDifficultyLeftBtn)
        {
            mDifficultyLeftBtn.OnSleep();
        }
    }

    void OnAwakeLeftBtn()
    {
        if (null != mDifficultyLeftBtn)
        {
            mDifficultyLeftBtn.OnAwake();
        }
    }

    void OnSleepRightBtn()
    {
        if (null != mDifficultyRightBtn)
        {
            mDifficultyRightBtn.OnSleep();
        }
    }

    void OnAwakeRightBtn()
    {
        if (null != mDifficultyRightBtn)
        {
            mDifficultyRightBtn.OnAwake();
        }
    }

}