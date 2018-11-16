using Game.Platform;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuScene : BaseScene
{
    EGameMenuType mMenuType;
    GameMenuUI mMenuUI;
    public GameMenuScene(EGameMenuType menuType)
    {
        mUIList = new List<BaseUI>();
        mMenuType = menuType;
        mMenuUI = new GameMenuUI();
        mUIList.Add(mMenuUI);
    }

    public override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            mBtnDelegate = new ButtonDelegate();
            mBtnDelegate.onClick = OnButtonClick;
            mMenuUI.Open(mBtnDelegate);

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static void GotoGameMenu(EGameMenuType menuType)
    {
        object[] args = new object[1];
        args[0] = menuType;
        SceneMgr.EnterScene(SceneType.EmptyScene, typeof(GameMenuScene), args);
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

                    }
                    break;
                case "Btn_Difficulty":
                    {

                    }
                    break;
                case "Btn_Start":
                    {
                        ClickBtnStart();
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


    void ClickBtnStart()
    {
        if (mMenuType == EGameMenuType.RainbowWorld)
        {
            //SceneMgr.EnterScene(SceneType.EmptyScene, typeof(ARScene));
            SceneMgr.EnterScene(SceneType.EmptyScene, typeof(RainbowWorldScene));
        }
    }
}

public enum EGameMenuType : byte
{
    RainbowWorld = 0,
}