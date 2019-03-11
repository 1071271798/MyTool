using System.Collections.Generic;
using UnityEngine;

public class RainbowWorldScene : ARScene
{
    RainbowWorldUI mRainbowUI;
    public RainbowWorldScene()
    {
        mUIList = new List<BaseUI>();
        mRainbowUI = new RainbowWorldUI();
        mUIList.Add(mRainbowUI);
    }

    public override void FirstOpen()
    {
        base.FirstOpen();
        
    }

    protected override void OnVuforiaStarted()
    {
        base.OnVuforiaStarted();
    }

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
    }

    protected override void BackGameMenu()
    {
        base.BackGameMenu();
        GameMenuScene.GotoGameMenu(EGameMenuType.RainbowWorld);
    }

    protected override void FirstTrackingFound()
    {
        base.FirstTrackingFound();
    }

    protected override void StartGame()
    {
        base.StartGame();
        SingletonObject<RainbowWorldCtrl>.GetInst().GuideEventListener(EGameGuideEventID.First_Recognition_Success, delegate(ERainbowGuideTriggerEvent eventId) {
            if (eventId == ERainbowGuideTriggerEvent.Trigger_Guide_Show_Color)
            {
                mRainbowUI.Open();
                mRainbowUI.StartGame();
            }
        });
        if (!SingletonObject<RainbowWorldCtrl>.GetInst().IsGuide())
        {
            mRainbowUI.Open();
            mRainbowUI.StartGame();
        }
       
    }

    protected override void LoadAR3DScene(Transform sceneTrans)
    {
        base.LoadAR3DScene(sceneTrans);
    }
}