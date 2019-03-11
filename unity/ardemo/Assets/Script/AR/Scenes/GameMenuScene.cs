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
        mMenuUI = new GameMenuUI(mMenuType);
        mUIList.Add(mMenuUI);
    }

    public override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            mMenuUI.Open();

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
            

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }


    void QuitGameScene()
    {
        
    }

}

public enum EGameMenuType : byte
{
    RainbowWorld = 0,
    RainbowWorld_HowToPlay,
}