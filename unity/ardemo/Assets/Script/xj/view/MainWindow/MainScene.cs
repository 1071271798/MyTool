using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Scene;
using Game;
using Game.Resource;
using Game.Event;
using LitJson;

public class MainScene : BaseScene
{
    #region 公有属性
    #endregion

    #region 私有属性
    MenuUI mMenuUi;
    static MainMenuType sSceneType;
    public MainMenuType MenuType
    {
        get { return sSceneType; }
    }
    Robot mRobot;
    CamRotateAroundCircle mRobotCamRotate;
    bool mChoicePhotoFlag = false;
    UITexture mPhotoTexture;

    
    UISprite mConnectIcon;
    UIButton mConnectBtn;
    /*UITexture mModelIcon;
    UISprite mEffectIcon;
    bool mEffectFlag = false;
    int mEffectIndex = 0;
    float mEffectTime = 0;
    int Power_Charge_Num_Max = 35;
    int Power_Low_Num_Max = 19;
    string Charge_Start = "power_charge_{0}";
    string Low_Start = "power_low_{0}";*/
    int mNowStep = 7;
    int mTotalStep = 10;

    PowerShow mPowerShow;

    static MainScene sMainScene;
    static GameObject sRobotCamera;
    #endregion

    #region 公有函数
    public MainScene()
    {
        mResPath = "Prefab/UI/mainSceneUI";
        mUIList = new List<BaseUI>();
        mMenuUi = new MenuUI();
        mUIList.Add(mMenuUi);
        sMainScene = this;
    }

    static void ClearRobotCamera()
    {
        if (null != sRobotCamera)
        {
            Camera cam = sRobotCamera.GetComponent<Camera>();
            if (null != cam)
            {
                cam.cullingMask = 0;
            }
            CamRotateAroundCircle rotateCircle = sRobotCamera.GetComponent<CamRotateAroundCircle>();
            if (null != rotateCircle)
            {
                rotateCircle.enabled = false;
            }
        }
    }

    static void ShowRobotCamera()
    {
        if (null != sRobotCamera)
        {
            Camera cam = sRobotCamera.GetComponent<Camera>();
            int mast = LayerMask.NameToLayer("Robot");
            if (mast >= 0)
            {
                mast = 1 << mast;
                if (null != cam && cam.cullingMask != mast)
                {
                    cam.cullingMask = mast;
                }
            }
            CamRotateAroundCircle rotateCircle = sRobotCamera.GetComponent<CamRotateAroundCircle>();
            if (null != rotateCircle)
            {
                rotateCircle.enabled = true;
            }
        }
    }

    public static void GotoScene(MainMenuType sceneType = MainMenuType.Action_Menu)
    {
        sSceneType = sceneType;
        SceneMgr.EnterScene(SceneType.EmptyScene, typeof(MainScene));
    }

    public static void GotoLastScene()
    {
        GotoScene(sSceneType);
    }

    public override void FirstOpen()
    {
        base.FirstOpen();
        try
        {
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            EventMgr.Inst.Regist(EventID.Read_Power_Msg_Ack, PowerCallBack);
            EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
            EventMgr.Inst.Regist(EventID.Photograph_Back, PhotographBack);
            EventMgr.Inst.Regist(EventID.Set_Choice_Robot, SetChoiceRobot);
            if (null == sRobotCamera)
            {
                GameObject loadCamera = Resources.Load<GameObject>("Prefab/MainCamera");
                if (null != loadCamera)
                {
                    sRobotCamera = (GameObject)GameObject.Instantiate(loadCamera);
                    sRobotCamera.name = "MainCamera";
                    GameObject.DontDestroyOnLoad(sRobotCamera);
                }
            } else
            {
                ShowRobotCamera();
                //sRobotCamera.SetActive(true);
            }
            if (null != mTrans)
            {
                Transform top = mTrans.Find("top");
                if (null != top)
                {
                    /*Vector2 pos = UIManager.GetWinPos(top, UIWidget.Pivot.Top, 0, PublicFunction.Back_Btn_Pos.y);
                    pos.x = 0;
                    top.localPosition = pos;*/

                    Transform topleft = top.Find("topleft");
                    if (null != topleft)
                    {
                        Vector2 topLeftPos = UIManager.GetWinPos(topleft, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        topleft.localPosition = new Vector2(topLeftPos.x - 300, topLeftPos.y);
                        GameHelper.PlayTweenPosition(topleft, topLeftPos, 0.6f);
                    }

                    Transform topright = top.Find("topright");
                    if (null != topright)
                    {
                        Vector2 rightPos = UIManager.GetWinPos(topright, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        topright.localPosition = new Vector2(rightPos.x + 300, rightPos.y);
                        GameHelper.PlayTweenPosition(topright, rightPos, 0.6f);

                        Transform btnConnect = topright.Find("btnConnect");
                        if (null != btnConnect)
                        {
                            /*mModelIcon = GameHelper.FindChildComponent<UITexture>(btnConnect, "model");
                            if (null != mModelIcon)
                            {
                                SingletonBehaviour<ResourcesLoad>.GetInst().Load(PlatformMgr.Instance.GetRobotIconPath(), typeof(Texture), delegate (object arg)
                                {
                                    if (null != arg && arg.GetType() == typeof(Texture2D))
                                    {
                                        mModelIcon.mainTexture = (Texture2D)arg;
                                    }
                                    if (null != mModelIcon.mainTexture)
                                    {
                                        PublicFunction.SetCircularTexture(mModelIcon, PublicFunction.Connect_Icon_Size);
                                    }
                                });
                            }*/
                            //mEffectIcon = GameHelper.FindChildComponent<UISprite>(btnConnect, "effect");
                            mConnectIcon = GameHelper.FindChildComponent<UISprite>(btnConnect, "Background");
                            mConnectBtn = btnConnect.GetComponent<UIButton>();
                        }

#if UNITY_EDITOR
                        if (null != mRobot && mRobot.GetRobotType() == ResFileType.Type_default)
                        {
                            Transform btnBuild = topright.Find("btnBuild");
                            if (null != btnBuild)
                            {
                                btnBuild.gameObject.SetActive(true);
                            }
                        }
#endif
                    }

                    Transform name = top.Find("name");
                    if (null != name)
                    {
                        name.localPosition = UIManager.GetWinPos(name, UIWidget.Pivot.Top, 0, PublicFunction.Back_Btn_Pos.y);
                        name.gameObject.SetActive(false);
                        /*if (null != mRobot && mRobot.GetRobotType() == ResFileType.Type_default)
                        {
                            name.gameObject.SetActive(false);
                        } else
                        {
                            UILabel label = name.GetComponent<UILabel>();
                            if (null != label)
                            {
                                if (sSceneType == MainMenuType.Action_Menu)
                                {
                                    label.text = LauguageTool.GetIns().GetText("动作设计");
                                } else if (sSceneType == MainMenuType.Program_Menu)
                                {
                                    label.text = LauguageTool.GetIns().GetText("程序开发");
                                }
                            }
                        }*/
                    }
                }
                Transform bottom = mTrans.Find("bottom");
                if (null != bottom)
                {
                    Transform bottomleft = bottom.Find("bottomleft");                                                                                        
                    if (null != bottomleft)
                    {
                        Transform btnReset = bottomleft.Find("btnReset");
                        Transform power = bottomleft.Find("power");
                        if (null != power)
                        {
                            mPowerShow = new PowerShow(power);
                        }
                        bool defaultFlag = false;
                        if (null != mRobot && mRobot.GetRobotType() == ResFileType.Type_default)
                        {
                            defaultFlag = true;
                        }
                        if (defaultFlag)
                        {//官方
                            if (null != btnReset)
                            {
                                GameHelper.SetPosition(btnReset, UIWidget.Pivot.BottomLeft, new Vector2(PublicFunction.Back_Btn_Pos.x, 35));
                            }
                            if (null != power)
                            {
                                GameHelper.SetPosition(power, UIWidget.Pivot.BottomLeft, new Vector2(PublicFunction.Back_Btn_Pos.x, 155));
                            }
                        } else
                        {//自建
                            if (null != btnReset)
                            {
                                btnReset.gameObject.SetActive(false);
                            }
                            if (null != power)
                            {
                                Vector2 leftPos;
                                if (PlatformMgr.Instance.EditFlag)
                                {
                                    leftPos = UIManager.GetWinPos(power, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x, 35);
                                }
                                else
                                {
                                    leftPos = UIManager.GetWinPos(power, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x, 110);
                                }
                                power.localPosition = new Vector2(leftPos.x - 300, leftPos.y);
                                GameHelper.PlayTweenPosition(power, leftPos);
                            }
                        }
                        if (null != mPowerShow)
                        {
                            mPowerShow.HidePower();
                        }
                    }

                    Transform browse = bottom.Find("browse");
                    if (null != browse)
                    {
                        if (PlatformMgr.Instance.EditFlag)
                        {
                            browse.gameObject.SetActive(false);
                        } else
                        {
                            GameHelper.SetPosition(browse, UIWidget.Pivot.Bottom, new Vector2(0, -1));
                            UISprite bg = GameHelper.FindChildComponent<UISprite>(browse, "bg");
                            if (null != bg)
                            {
                                bg.width = PublicFunction.GetExtendWidth();
                            }
                            Transform browseRight = browse.Find("right");
                            if (null != browseRight)
                            {
                                GameHelper.SetPosition(browseRight.Find("btnNext"), UIWidget.Pivot.Right, new Vector2(20 + PublicFunction.Iphonex_Add_Offset.x, 0));
                                GameHelper.SetPosition(browseRight.Find("num"), UIWidget.Pivot.Right, new Vector2(103 + PublicFunction.Iphonex_Add_Offset.x, -2));
                                GameHelper.SetPosition(browseRight.Find("btnPre"), UIWidget.Pivot.Right, new Vector2(205 + PublicFunction.Iphonex_Add_Offset.x, 0));
                                //browseRight.localPosition = UIManager.GetWinPos(browseRight, UIWidget.Pivot.Right, 80);
                            }
                        }
                    }
                }
                Transform photo = mTrans.Find("center/photo");
                if (null != photo)
                {
                    if (string.Equals(RecordContactInfo.Instance.openType, "playerdata"))
                    {
                        Transform btnPhoto = photo.Find("btnPhoto");
                        Vector2 photoPos = Vector2.zero;
                        if (null != btnPhoto)
                        {
                            photoPos = new Vector2(-PublicFunction.GetWidth() / 2 + 0.3015f * PublicFunction.GetWidth(), 0);
                            btnPhoto.localPosition = photoPos;
                        }
                        
                        mPhotoTexture = GameHelper.FindChildComponent<UITexture>(photo, "btnChange");
                        if (null != mPhotoTexture)
                        {
                            mPhotoTexture.transform.localPosition = photoPos;
                        }
                        //PlatformMgr.Instance.Pic_Path = "file://C:\\Users\\Administrator\\AppData\\LocalLow\\ubt\\JIMU\\data\\customize\\image\\1.jpg";
                        if (!string.IsNullOrEmpty(PlatformMgr.Instance.GetRobotTexturePath()))
                        {
                            if (null != btnPhoto)
                            {
                                btnPhoto.gameObject.SetActive(false);
                            }
                            SingletonBehaviour<ResourcesLoad>.GetInst().Load(PlatformMgr.Instance.GetRobotTexturePath(), typeof(Texture), delegate (object arg)
                            {
                                if (null != arg && arg.GetType() == typeof(Texture2D))
                                {
                                    if (null != mPhotoTexture)
                                    {
                                        mPhotoTexture.mainTexture = (Texture2D)arg;
                                    }
                                    
                                }
                                if (null != mPhotoTexture && null != mPhotoTexture.mainTexture)
                                {
                                    PublicFunction.SetTextureFullSize(mPhotoTexture, (int)(PublicFunction.GetWidth() * (1 - PublicFunction.Main_Scene_Right_Width)) + 2, PublicFunction.GetHeight() + 2);
                                    GameHelper.SetTransformAlpha(mPhotoTexture.transform, 0.001f);
                                    GameHelper.PlayTweenAlpha(mPhotoTexture.transform, 1);
                                } else
                                {
                                    if (null != btnPhoto && PlatformMgr.Instance.EditFlag)
                                    {
                                        btnPhoto.gameObject.SetActive(true);
                                        GameHelper.SetTransformAlpha(btnPhoto, 1);
                                    }
                                }
                            });
                        } else if (!PlatformMgr.Instance.EditFlag)
                        {
                            if (null != btnPhoto)
                            {
                                btnPhoto.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        photo.gameObject.SetActive(false);
                    }
                }

                Transform right = mTrans.Find("right");
                if (null != right)
                {
                    UISprite bg = GameHelper.FindChildComponent<UISprite>(right, "bg");
                    if (null != bg)
                    {
                        bg.width = (int)(PublicFunction.GetWidth() * 0.408f);
                        bg.height = PublicFunction.GetHeight();
                    }
                    right.localPosition = UIManager.GetWinPos(right, UIWidget.Pivot.Right);
                }

            }
            SetConnectState();
            if (null != mPowerShow)
            {
                mPowerShow.SetPowerState();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void UpdateScene()
    {
        base.UpdateScene();
        OpenSceneUI(sSceneType);
        /*if (PlatformMgr.Instance.IsChargeProtected)
        {
            PublicPrompt.ShowChargePrompt(null);
        }*/
    }

    public override void Update()
    {
        base.Update();
        if (null != mPowerShow)
        {
            mPowerShow.CharingUpdate();
        }
    }

    public override void Open()
    {
        base.Open();
    }

    /*public override void Update()
    {
        base.Update();
        if (isOpen)
        {
            if (mEffectFlag && null != mEffectIcon)
            {
                mEffectTime += Time.deltaTime;
                if (mEffectTime >= mEffectIndex * 0.125f)
                {
                    ++mEffectIndex;
                    if (PlatformMgr.Instance.PowerData.isAdapter)
                    {
                        mEffectIcon.spriteName = string.Format(Charge_Start, mEffectIndex.ToString().PadLeft(5, '0'));
                        if (mEffectIndex >= Power_Charge_Num_Max)
                        {
                            mEffectTime = -0.25f;
                            mEffectIndex = -1;
                        }
                    } else
                    {
                        mEffectIcon.spriteName = string.Format(Low_Start, mEffectIndex.ToString().PadLeft(5, '0'));
                        if (mEffectIndex >= Power_Low_Num_Max)
                        {
                            mEffectTime = -0.25f;
                            mEffectIndex = -1;
                        }
                    }
                }
            }
        }
    }*/

    public override void Close()
    {
        base.Close();
        sMainScene = null;
        EventMgr.Inst.UnRegist(EventID.Read_Power_Msg_Ack, PowerCallBack);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.UnRegist(EventID.Photograph_Back, PhotographBack);
        EventMgr.Inst.UnRegist(EventID.Set_Choice_Robot, SetChoiceRobot);
    }

    #endregion

    #region 私有函数

    protected override void OnButtonClick(GameObject obj)
    {
        try
        {
            base.OnButtonClick(obj);
            string name = obj.name;
            switch(name)
            {
                case "btnBack":
                    GoBack();
                    break;
                case "btnControl":
                    PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P7_3);
                    if (null == mRobot)
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("请重新选择或创建模型"), 1, CommonTipsColor.red);
                        return;
                    }
                    if (PlatformMgr.Instance.IsChargeProtected)
                    {
                        PublicPrompt.ShowChargePrompt(null);
                        return;
                    }
                    GoControl();
                    break;
                case "btnConnect":
                    PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P7_2);
                    if (null == mRobot)
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("请重新选择或创建模型"), 1, CommonTipsColor.red);
                        return;
                    }
                    PublicPrompt.ShowClickBlueBtnMsg(ConnectionTriggerPage.Action_Page);
                    break;
                case "btnPhoto":
                case "btnChange":
                    if (!mChoicePhotoFlag && PlatformMgr.Instance.EditFlag)
                    {
                        mChoicePhotoFlag = true;
                        Timer.Add(0.4f, 1, 1, delegate () { mChoicePhotoFlag = false; });
                        if (null != mRobot)
                        {
                            PlatformMgr.Instance.Photograph(RobotMgr.NameNoType(mRobot.Name), PlatformMgr.Instance.GetRobotTexturePath());
                        } else
                        {
                            Robot createRobot = RobotManager.GetInst().GetCreateRobot();
                            if (null != createRobot)
                            {
                                PlatformMgr.Instance.Photograph(RobotMgr.NameNoType(createRobot.Name), PlatformMgr.Instance.GetRobotTexturePath());
                            }
                        }
                    }
                    break;
                case "btnReset":
                    if (null == mRobotCamRotate)
                    {
                        GameObject mainCamera = GameObject.Find("MainCamera");
                        if (null != mainCamera)
                        {
                            mRobotCamRotate = mainCamera.GetComponent<CamRotateAroundCircle>();
                        }
                    }
                    if (null != mRobotCamRotate)
                    {
                        mRobotCamRotate.MobClickOriginalPosition();
                        mRobotCamRotate.ResetOriState();
                    }
                    break;
                case "btnPre":
                    DiyJumpStep(mNowStep - 1);
                    break;
                case "btnNext":
                    int targetStep = mNowStep + 1;
                    DiyJumpStep(targetStep);
                    break;
                case "btnBuild":
                    if (null == mRobot)
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("请重新选择或创建模型"), 1, CommonTipsColor.red);
                        return;
                    }
                    if (null != sRobotCamera)
                    {
                        GameObject.Destroy(sRobotCamera);
                        sRobotCamera = null;
                    }
                    GameObject mVCenter = GameObject.Find("MVCenter");
                    if (null != mVCenter)
                    {
                        GameObject.Destroy(mVCenter);
                    }
                    SceneMgr.EnterScene(SceneType.Assemble);
                    break;

            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    


    void OpenSceneUI(MainMenuType sceneType)
    {
        sSceneType = sceneType;
        if (!PlatformMgr.Instance.EditFlag && null != mTrans)
        {
            UILabel num = GameHelper.FindChildComponent<UILabel>(mTrans, "bottom/browse/right/num");
            if (null != num)
            {
                string json = PlatformMgr.Instance.GetData(PlatformDataType.diyStepData, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    Dictionary<string, object> dict = (Dictionary<string, object>)Json.Deserialize(json);
                    if (dict.ContainsKey("nowStep") && PublicFunction.IsInteger(dict["nowStep"].ToString()))
                    {
                        mNowStep = int.Parse(dict["nowStep"].ToString());
                    }
                    if (dict.ContainsKey("totalStep") && PublicFunction.IsInteger(dict["totalStep"].ToString()))
                    {
                        mTotalStep = int.Parse(dict["totalStep"].ToString());
                    }
                }
                
                string formatStr = "{0}/{1}";
                num.text = string.Format(formatStr, mNowStep, mTotalStep);
            }
            
        }
        if (null != mTrans)
        {
            Transform btnControl = mTrans.Find("top/topright/btnControl");
            if (null != btnControl)
            {
                if (sSceneType == MainMenuType.Program_Menu)
                {
                    btnControl.gameObject.SetActive(false);
                } else
                {
                    btnControl.gameObject.SetActive(true);
                }
            }
        }
         
        mMenuUi.SetMenuData(sceneType);
        mMenuUi.Open();
    }

    void GoBack()
    {
        //mMenuUi.HideUI();
        if (null != sRobotCamera)
        {
            GameObject.Destroy(sRobotCamera);
            sRobotCamera = null;
        }
        UnLoadTexture();
        if (ClientMain.Use_Third_App_Flag)
        {
            if (!PlatformMgr.Instance.EditFlag)
            {
                ExitDiyBrowsing();
            } else
            {
                PlatformMgr.Instance.BackThirdApp();
            }
        }
        else
        {
            SceneMgr.EnterScene(SceneType.MenuScene);
            try
            {
                RobotMgr.Instance.GoToCommunity();
            }
            catch (System.Exception ex)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
            }
        }
    }

    void GoControl()
    {
        if (PlatformMgr.Instance.IsChargeProtected)
        {
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("adpateProtected"), 1, CommonTipsColor.red);
            return;
        }
        /*GameObject oriGO = GameObject.Find("oriGO");
        if (oriGO != null)
        {
            if (null != MoveSecond.Instance)
            {
                MoveSecond.Instance.ResetParent();
                MoveSecond.Instance.ResetDJDPPA();
                MoveSecond.Instance.RestGOPA();
            }
            GameObject.DontDestroyOnLoad(oriGO);
        }*/
        /*if (sLoadFinished)
        {
            HideRobotCamera();
        }*/
        ClearRobotCamera();
        mMenuUi.HideUI();
        SceneMgrTest.Instance.LastScene = SceneType.MainWindow;
        UserdefControllerScene.GotoController(FromModuleType.From_Main_Scene);
        //SceneMgr.EnterScene(SceneType.Assemble);
    }

    void DiyJumpStep(int targetStep)
    {
        try
        {
            if (null != sRobotCamera)
            {
                GameObject.Destroy(sRobotCamera);
                sRobotCamera = null;
            }
            UnLoadTexture();
            PlatformMgr.Instance.QuitUnityCleanData(delegate() {
                SingletonBehaviour<ClientMain>.GetInst().WaitFrameInvoke(delegate () {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict["targetStep"] = targetStep;
                    dict["totalStep"] = mTotalStep;
                    PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.diyJumpStep, Json.Serialize(dict));
                });
            });
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void ExitDiyBrowsing()
    {
        try
        {
            if (null != sRobotCamera)
            {
                GameObject.Destroy(sRobotCamera);
                sRobotCamera = null;
            }
            UnLoadTexture();
            PlatformMgr.Instance.QuitUnityCleanData(delegate() {
                SingletonBehaviour<ClientMain>.GetInst().WaitFrameInvoke(delegate () {
                    PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.exitDiyBrowsing, string.Empty);
                });
            });
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SetConnectState()
    {
        //string effectIconName = null;
        string connectName = null;
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            /*if (PlatformMgr.Instance.PowerData.isAdapter)
            {
                mEffectFlag = true;
                mEffectIndex = 0;
                mEffectTime = 0;
                effectIconName = string.Format(Charge_Start, "00000");
            } else if (PlatformMgr.Instance.PowerData.IsPowerLow())
            {
                mEffectFlag = true;
                mEffectIndex = 0;
                mEffectTime = 0;
                effectIconName = string.Format(Low_Start, "00000");
            } else
            {
                mEffectFlag = false;
            }*/
            connectName = "connect";
        } else
        {
            //mEffectFlag = false;
            connectName = "disconnect";
        }
        if (!string.IsNullOrEmpty(connectName))
        {
            if (null != mConnectIcon)
            {
                mConnectIcon.spriteName = connectName;
            }
            if (null != mConnectBtn)
            {
                mConnectBtn.normalSprite = connectName;
            }
        }
        /*if (mEffectFlag)
        {
            if (null != mModelIcon)
            {
                mModelIcon.color = new Color32(151, 151, 151, 255);
            }
        } else
        {
            if (null != mModelIcon)
            {
                mModelIcon.color = Color.white;
            }
        }
        if (null != mEffectIcon)
        {
            if (effectIconName != null)
            {
                mEffectIcon.alpha = 1;
                mEffectIcon.spriteName = effectIconName;
            } else
            {
                mEffectIcon.alpha = 0;
            }
        }*/

    }



    

    void PowerCallBack(EventArg arg)
    {
        if (null != mPowerShow)
        {
            mPowerShow.SetPowerState();
        }
    }

    void OnBlueConnectResult(EventArg arg)
    {
        SetConnectState();
        if (null != mPowerShow)
        {
            mPowerShow.SetPowerState();
        }
    }

    void PhotographBack(EventArg arg)
    {
        if (null != mPhotoTexture && null != mTrans)
        {
            Transform btnPhoto = mTrans.Find("center/photo/btnPhoto");
            if (string.IsNullOrEmpty(PlatformMgr.Instance.GetRobotTexturePath()))
            {
                mPhotoTexture.mainTexture = null;
                if (null != btnPhoto)
                {
                    btnPhoto.gameObject.SetActive(true);
                    GameHelper.SetTransformAlpha(btnPhoto, 0.001f);
                    GameHelper.PlayTweenAlpha(btnPhoto, 1);
                }
                /*if (null != mModelIcon)
                {
                    mModelIcon.mainTexture = null;
                }*/
                if (null != arg)
                {
                    string path = (string)arg[0];
                    SingletonBehaviour<ResourcesLoad>.GetInst().UnLoad(typeof(Texture), path);
                }
            } else
            {
                if (null != arg)
                {//先把之前的图片删除了，防止切换图片时名字未改不加载新图片
                    string path = (string)arg[0];
                    SingletonBehaviour<ResourcesLoad>.GetInst().UnLoad(typeof(Texture), path);
                }
                bool hidePhoto = false;
                if (null != mPhotoTexture.mainTexture)
                {
                    GameHelper.PlayTweenAlpha(mPhotoTexture.transform, 0.001f);
                }
                SingletonBehaviour<ResourcesLoad>.GetInst().Load(PlatformMgr.Instance.GetRobotTexturePath(), typeof(Texture), delegate (object tex)
                {
                    if (null != tex && tex.GetType() == typeof(Texture2D))
                    {
                        mPhotoTexture.mainTexture = (Texture2D)tex;
                    }
                    if (null != mPhotoTexture.mainTexture)
                    {
                        PublicFunction.SetTextureFullSize(mPhotoTexture, (int)(PublicFunction.GetWidth() * (1 - PublicFunction.Main_Scene_Right_Width)) + 2, PublicFunction.GetHeight() + 2);
                        GameHelper.PlayTweenAlpha(mPhotoTexture.transform, 1);
                        /*mModelIcon.mainTexture = mPhotoTexture.mainTexture;
                        if (null != mModelIcon.mainTexture)
                        {
                            PublicFunction.SetCircularTexture(mModelIcon, PublicFunction.Connect_Icon_Size);
                        }*/
                        hidePhoto = true;
                    }
                    else
                    {
                        /*if (null != mModelIcon)
                        {
                            mModelIcon.mainTexture = null;
                        }*/
                    }
                    if (null != btnPhoto)
                    {
                        if (hidePhoto)
                        {
                            GameHelper.PlayTweenAlpha(btnPhoto, 0.001f);
                            Timer.Add(0.35f, 1, 1, delegate ()
                            {
                                btnPhoto.gameObject.SetActive(false);
                            });
                        }
                        else
                        {
                            btnPhoto.gameObject.SetActive(true);
                            GameHelper.SetTransformAlpha(btnPhoto, 0.001f);
                            GameHelper.PlayTweenAlpha(btnPhoto, 1);
                        }
                    }
                });
                Resources.UnloadUnusedAssets();
            }
        }
    }
    
    void SetChoiceRobot(EventArg arg)
    {
        mRobot = RobotManager.GetInst().GetCurrentRobot();
    }

    void UnLoadTexture()
    {
        /*if (null != mModelIcon)
        {
            mModelIcon.mainTexture = null;
        }*/
        if (null != mPhotoTexture)
        {
            mPhotoTexture.mainTexture = null;
        }
        SingletonBehaviour<ResourcesLoad>.GetInst().UnLoad(typeof(Texture));
    }

    
    #endregion
}

