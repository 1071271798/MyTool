using UnityEngine;
using System.Collections;
using Game.Scene;
using Game.Platform;

public class UserCotrollerSettingUI : BaseUI
{
    public UserCotrollerSettingUI()
    {
        mUIResPath = "Prefab/UI/control/userdefineControllerSetting";
    }

    protected override void FirstOpen()
    {
        base.FirstOpen();
    }

    protected override void Close()
    {
        base.Close();
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        Transform topL = mTrans.Find("Up/topLeft").transform;
        topL.GetChild(0).localPosition = UIManager.GetWinPos(topL, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);//, topL.GetChild(0).GetComponentInChildren<UIWidget>().width);
        Transform topR = mTrans.Find("Up/topright").transform;
        topR.GetChild(0).localPosition = UIManager.GetWinPos(topR, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);//, topR.GetChild(0).GetComponent<UIWidget>().width);
        if (!PlatformMgr.Instance.EditFlag)
        {
            topR.gameObject.SetActive(false);
        }
        /*Transform centerIcon = mTrans.FindChild("Center/settingTip").transform;
        centerIcon.localPosition = UIManager.GetWinPos(centerIcon, UIWidget.Pivot.Center, 0, 70);*/
        Transform centerTip = mTrans.Find("Center/settingTip2").transform;
        //centerTip.localPosition = UIManager.GetWinPos(centerTip, UIWidget.Pivot.Center, 0, -70);
        centerTip.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("初始化遥控提示");
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            if (obj.name.Contains("backM"))
            {
                GoBack();
            }
            else if (obj.name.Contains("settingC"))
            {
                //SetScene.GotoSetScene(SetSceneType.SetSceneTypeDevice);
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P9_1);
                DoSetting();
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
    }

    void GoBack()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Back model function scene!!");
        //RobotMgr.Instance.openActionList = true;
        mTrans.gameObject.SetActive(false);
        UserdefControllerScene.GoBack();
    }

    void DoSetting()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Enter controller setting UI!!");

        OnClose();
        UserdefControllerScene.Ins.CreateNewController("");
    }
}
