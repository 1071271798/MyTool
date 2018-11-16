/*
 * 注释： 只有组装功能时使用
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using UnityEditor;
using Game.Scene;
using Game.Platform;

using Game.UI;
using System.IO;
using Game.Event;
using Game.Resource;


public class JMSimulatorOnly : MonoBehaviour
{
    #region static
    public static JMSimulatorOnly Instance;
    #endregion


    #region public
    public Dictionary<string, GameObject> btns = new Dictionary<string, GameObject>();
    public string userID = null;
    #endregion

    #region private
    private string robotname;
    private List<string> goName = new List<string>();  //所有物体的名字
    private GameObject oriGO;
    private Vector3 oriGOPos;
    private Vector3 oriGOAngle;
    private Vector3 oriGOScale;
    private bool isFinished = false;
    private List<string> btnNam = new List<string>();//所有按钮的名称包括Label

    private GameObject slider;     //进度条
    private UISlider tempslider;   //进度条上的UISlider组件
    private float tempValue;
    private float stepTemp;

    private bool onlyShowDJ = true;   //只显示舵机
    private Dictionary<string, string> nots = new Dictionary<string, string>();//提示
    private GameObject oriGOPrefab = null;
    private int oriStep;
    private float oriValue;
    private bool mRecordBuildDurationFlag = false;
    private List<string> innerSprites = new List<string>();//内置在app中的贴图资源
    private List<string> allSprites = new List<string>();
    private List<string> jubuSpritesName = new List<string>();//局部图名称
    private Texture textureTT;
    private List<string> outAddSprites = new List<string>();   //需要从外部加载的模型
    private Dictionary<string, Texture> outPics = new Dictionary<string, Texture>();//从外部加载的图片资源

    private Dictionary<string, Texture> jubuPics = new Dictionary<string, Texture>();//外部加载的局部图资源

    private Dictionary<string, UILabel> labels = new Dictionary<string, UILabel>();
    private Dictionary<string, UILabel> inputlabels = new Dictionary<string, UILabel>();

    private bool start = true;
    private bool hasFinished = false;

    public Dictionary<string, AnimData> rbtAnim = new Dictionary<string, AnimData>();   //<机器人名字，机器人类动画>
    private Dictionary<string, AnimationClip> anims = new Dictionary<string, AnimationClip>();  //存储动画片段
    private AnimationClip clipTemp;

    private Animation animationTemp;//模型上的animation组件

    private string robotNameNoType;//播放的动画名称的前缀
    private int clipCount = 0;//播放的动画的后缀名称

    private string addedClipName;   //Animation中已添加的动画

    private List<string> animtype = new List<string>();  //动画的source

    private Transform DJNotice;
    private GameObject partsPanel;
    private GameObject partsScrollView;
    private GameObject startBtn;
    private GameObject partsStep;

    private GameObject juBuStep;     //局部图显示区
    private GameObject partsControl;
    private GameObject finishAnim;//搭建完成的动画顶点
    private GameObject finishAnimMini;//搭建完mini小车的动画顶点
    private UITexture finishAnimBg;
    private GameObject fsNotice;
    private GameObject fsnormal;//搭建完成时显示的提示语
    private GameObject fslesson;//课程模式下搭建完成显示的提示语
    private GameObject llBack;
    private GameObject llForward;

    private GameObject createPList;

    private GameObject iconGO;
    private UISprite goicon;
    private UISprite jubuIcon;     //jubuBtn下的主Icon
    private UITexture jubuTexture;     //显示单步局部图的UITexture
    private UITexture jubuBGTexture;     //显示单步局部图模型的阴影
    private AnimationClip shaderAnim;  //主控盒数字编号动画
    private GameObject textureGO;

    public GameObject lackParts;//零件缺少时，提示
    private UITexture goTexture;
    private UILabel goLabel;
    private bool noParts = false;// 不显示单步零件的图片和名称
    private bool noJuBu = false;

    private GameObject littlepanel = null;
    private GameObject uicenter = null;
    private GameObject uiright = null;
    private GameObject uileft = null;
    private GameObject uibottom = null;
    private GameObject thumbT;

    private List<string> inplabel = new List<string>();
    private List<string> notelabel = new List<string>();

    private List<string> ids = new List<string>();

    public bool isActionList = false;//点击的是进入动作列表按钮
    public bool isControl = false;//点击的是进入遥控场景按钮
    public bool isExitBT = false;//是否已退出蓝牙链接界面
    public static bool IsLessonBuild = false;
    private GameObject uiCamera;
    private int _myStepCount; // add by xsl

    private string path1;
    private string robotIDTemp;

    private GameObject oriTemp;//oriGO临时父物体
    private Transform mUIRootABL;

    public GameObject lessonParts;//课程模式下课程控件
    private Dictionary<int, string> ecgnotice;//课程模式鼓励提示
    private GameObject ecggo;//课程模式提示物体
    private UILabel ecglabel1;//课程模式提示文字控件
    private UILabel ecglabel2;//课程模式提示文字控件
    

    private Dictionary<string, DJClass> djIntGO = new Dictionary<string, DJClass>();   //舵机的名称，舵机类<内部模型名称，模型材质>
    private Dictionary<string, Texture> djposTexture = new Dictionary<string, Texture>();//舵机位置贴图
    private Dictionary<string, GameObject> zkhNums = new Dictionary<string, GameObject>();   //主控盒数字标签
    private Dictionary<string, GameObject> AddedAllGOs = new Dictionary<string, GameObject>();      //已经生成的物体

    private FromPlatform fromType = FromPlatform.Self;

    private int stepCount
    {
        get
        {
            return _myStepCount;
        }
        set
        {
            _myStepCount = value;

        }
    }

    public GameObject lessonTex;   //课程模式背景
    #endregion

    void GuideShutdownCallback(EventArg arg)
    {

        btns["Return"].GetComponent<UIButtonColor>().state = UIButtonColor.State.Normal;
        btns["Return"].GetComponent<BoxCollider>().enabled = true;
    }
    /// <summary>
    /// 退出蓝牙
    /// </summary>
    /// <param name="go"></param>
    public void ExitBTConnect(EventArg arg)
    {
        if (isActionList)
        {
            SceneMgrTest.Instance.LastScene = SceneType.MainWindow;
            /*RobotMgr.Instance.openActionList = true;
            SceneMgr.EnterScene(SceneType.MainWindow);    */
            MainScene.GotoScene(MainMenuType.Action_Menu);
        }
        else if (isControl)
        {
            SceneMgrTest.Instance.LastScene = SceneType.ActionPlay;
            UserdefControllerScene.GotoController(FromModuleType.From_Build);
        }
    }

    void Awake()
    {
        Instance = this;
    }

    public void AdaptIphoneX()
    {
        uiCamera.GetComponent<Camera>().rect = new Rect(0.033f, 0, 0.92f, 1);
        StartCoroutine(DelayResetCamera());
    }

    IEnumerator DelayResetCamera()
    {
        yield return new WaitForSeconds(0.1f);
        GameObject goNull = null;
        littlepanel.GetComponent<UISprite>().SetAnchor(goNull);
        uiCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
        GameObject uitxr = GameObject.Find("MainCamera/AssemblePanel/UI Root/Camera/gamebg/tex");
        if(uitxr != null)
       {
           FindChildGO(lessonParts, "notices/yuhangyuannotice1/bg1").GetComponent<UITexture>().width = uitxr.GetComponent<UITexture>().width;
           FindChildGO(lessonParts, "notices/yuhangyuannotice1/bg1").GetComponent<UITexture>().height = uitxr.GetComponent<UITexture>().height;
        }
        
        
        GameObject lsSprite = null;
        for (int i = 0; i <= 3; i++)
        {
            string childName = null;
            float posX = btns["Return"].transform.localPosition.x-30;
            if (i == 0) 
            {
               childName = "notices/yuhangyuannotice1/Sprite";
            }
            else if(i == 1)
            {
                childName = "notices/tancechenotice1/Sprite";
            }
            else if(i == 2)
            {
                childName = "notices/TankbotPronotice1/Sprite";
            }
            else
            {
                posX = btns["Forward"].transform.localPosition.x+150;
                childName = "notices/TankbotPronotice1/Sprite2";
                
            }

            lsSprite = FindChildGO(lessonParts, childName);
            lsSprite.GetComponent<UIWidget>().SetAnchor(goNull);
            Vector3 localPos = lsSprite.transform.localPosition;
            lsSprite.transform.localPosition = new Vector3(posX, localPos.y, localPos.z);

        }

        

    }

    void Start()
    {
        //测试打开修改ID的UI
        //SetDeviceUI mSetDevice = new SetDeviceUI();
        //mSetDevice.Open();
        PlatformMgr.Instance.MobPageStart(MobClickEventID.P6);
        littlepanel = GameObject.Find("UIRootABL/Camera/Center/LittePanel");
        uiright = GameObject.Find("UIRootABL/Camera/Center/LittePanel/Right");
        uibottom = GameObject.Find("UIRootABL/Camera/Center/LittePanel/Bottom");
        uicenter = GameObject.Find("UIRootABL/Camera/Center/LittePanel/Center");
        uileft = GameObject.Find("UIRootABL/Camera/Center/LittePanel/Left");
        uiCamera = GameObject.Find("UIRootABL/Camera");



        try
        {
            try
            {
                EventMgr.Inst.Regist(EventID.Blue_Connect_Finished, ExitBTConnect);
                EventMgr.Inst.Regist(EventID.GuideShutdown, GuideShutdownCallback);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                ReturnScene(null);
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("JMSimulatorOnly-start-EventMgr.Inst.Regist"), 1, CommonTipsColor.red);
                return;
            }
            robotname = RobotMgr.Instance.rbtnametempt;

            try
            {
                robotNameNoType = RobotMgr.NameNoType(robotname);
                string robotid = RobotMgr.Instance.rbt[robotname].id;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                ReturnScene(null);
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("JMSimulatorOnly-start-RobotMgr.NameNoType"), 1, CommonTipsColor.red);
                return;
            }

            
            fromType = PlatformMgr.Instance.FromType;
            //fromType = FromPlatform.Alpha_mini;

            #region 翻译处理
            TranslateHandle();

            #endregion
            
            oriValue = RecordContactInfo.Instance.FindOriValue("Step");   //查找保存的步数
            
            shaderAnim = Resources.Load("Prefab/Test4/CommonClips/GreenColor") as AnimationClip;
            GameObject uiRootABLObj = GameObject.Find("UIRootABL");

            if (null != uiRootABLObj)
            { 
                mUIRootABL = uiRootABLObj.transform;
            }

           //IsLessonBuild = true;
            if (IsLessonBuild)
            {
                oriValue = 0;   //课程模式下退出时不记录步数
            }
            
            try
            {
                lessonParts = FindChildGO(uicenter, "LessonParts"); 
                lessonParts.SetActive(false);
                if (IsLessonBuild)
                {
                    
                    ecggo = lessonParts.transform.Find("encourage").gameObject;
                    ecglabel1 = ecggo.transform.Find("Label1").GetComponent<UILabel>();
                    ecglabel2 = ecggo.transform.Find("Label2").GetComponent<UILabel>();
                    lessonTex = GameObject.Find("MainCamera").transform.Find("AssemblePanel").gameObject;
                    lessonTex.SetActive(true);
                    lessonParts.SetActive(true);
                    StartCoroutine(LessonAssemble.Instance().ChangeCamBG(robotNameNoType));
                    foreach (Transform child in lessonParts.GetComponentInChildren<Transform>())
                    {
                        if (child.name != "notices" && child.name != "lessonbg")
                        {
                            child.gameObject.SetActive(false);
                        }

                    }
                    LessonAssemble.Instance().StartHandle(robotNameNoType, lessonParts);

                    StartCoroutine(DelayCmd());
                }

                if (AnimReadData.Instance != null)
                {
                    AnimReadData.Instance.Dispose();
                }

                TextureMgr textM = new TextureMgr();
                djposTexture = textM.FindPosPic();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                ReturnScene(null);
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("lessonParts 操作失败"), 1, CommonTipsColor.red);
                return;
            }
            #region Initial Handle
           
            
            #endregion

            #region 找组件
            try
            {
                FindComponents();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                ReturnScene(null);
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("FindComponents 操作失败"), 1, CommonTipsColor.red);
                DownloadModelResult(false);
                return;
            }
            #endregion


            //AdaptIphoneX();

            #if UNITY_IPHONE  //iphonex适配
                if (Screen.width == 2436 && Screen.height == 1125)
                {
                    AdaptIphoneX();
                }
            #endif

            //零件库信息
            try
            {
                PartListData();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                ReturnScene(null);
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText(" PartListData 操作失败"), 1, CommonTipsColor.red);
                DownloadModelResult(false);
                return;
            };

            //创建模型，分割动画
            oriGO = GameObject.Find("oriGO");
            if (oriGO != null)
            {
                try
                {
                    InitialModelAnims();
                }
                catch (System.Exception ex)
                {
                    DownloadModelResult(false);
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "JMSimulatorOnly InitialModelAnims 异常- error = " + ex.ToString());
                }

            }


            #region 零件缺少提示
             try
             {
                if (IsLessonBuild == false)
                {
                    userID = PlatformMgr.Instance.GetUserData();

                    string pathfile = ResourcesEx.persistentDataPath + "/data/partsImport/" + userID + ".json";

                    if (File.Exists(pathfile))
                    {
                        texTest texTestTemp = new texTest();
                        texTestTemp.delegateMethod += new texTest.DelegateMethod(texTestTemp.CreateLackPicsList);
                        texTestTemp.RunDelegateMethods();
                    }
                    else
                    {
                        lackParts.SetActive(false);
                    }

                }
                else
                {

                    lackParts.SetActive(false);

                }
             }
             catch (System.Exception ex)
             {
                 System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                 PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                 ReturnScene(null);
                 SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("零件缺少提示 操作失败"), 1, CommonTipsColor.red);
                return;
            }
            #endregion


            //初始化按钮
            EnterSceneHide();

            //添加事件
            try
            {
                AddEvents();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                ReturnScene(null);
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("AddEvents 操作失败"), 1, CommonTipsColor.red);
                return;
            }

            goName = RobotMgr.Instance.FindAllGOName(robotname);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            ReturnScene(null);
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("操作失败"), 1, CommonTipsColor.red);
            DownloadModelResult(false);
            return;
        }
        
    }

    void DownloadModelResult(bool successFlag)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        if (successFlag)
        {
            dict["resultCode"] = PublicFunction.ResultCode_Success;
        } else
        {
            dict["resultCode"] = PublicFunction.ResultCode_Failure;
        }
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.app_building_downloadmodel, dict);
    }


    /// <summary>
    /// 翻译处理
    /// </summary>
    public void TranslateHandle()
    {
        Dictionary<string, string> notString = NormalStringData.TranslateData();
        foreach(string child in notString.Keys)
        {
            nots.Add(child, RobotMgr.Instance.FindString(notString[child]));
        }

    }


    /// <summary>
    /// 查找组件
    /// </summary>
    public void FindComponents()
    {
        GameObject camera = GameObject.Find("UIRootABL/Camera");


        foreach (Transform child in camera.transform.GetComponentsInChildren<Transform>())
        {

            if (child.name == "Return" ||child.name == "Confirm" || child.name == "Forward" || child.name == "Back" || child.name == "Refresh")
            {

                btns.Add(child.name, child.gameObject);
            }
            else if (child.name == "TestLabel")   //child.name == "TotalStepsTitle" || child.name == "NowStepTitle" || 
            {

                labels.Add(child.name, FindLabelComponent(child));    //固定显示的label
            }
            else if (child.name == "TotalSteps" || child.name == "NowStep" || child.name == "djnotice" || child.name == "djnotice2" || child.name == "fsName" || child.name == "fsLabel1" || child.name == "fsStay" || child.name == "fsStayMini" || child.name == "fsNameMini")
            {
                inputlabels.Add(child.name, FindLabelComponent(child));    //不断变换内容的label
            }

        }

        

        slider = FindChildGO(uibottom, "StepSlider"); 
        thumbT = slider.transform.Find("Thumb").gameObject;
        tempslider = slider.transform.GetComponent<UISlider>();
        tempslider.value = 0;
        tempValue = tempslider.value;
        stepTemp = stepCount;

        partsControl = FindChildGO(uiright, "NoticeBG/PartsControl");
        partsStep = FindChildGO(uiright, "PartsStep");
        createPList = FindChildGO(uiright, "CreatePList");


        juBuStep = FindChildGO(uileft, "JuBuStep");

        fsNotice = FindChildGO(uibottom, "notice/fsNotice");
        fsnormal =fsNotice.transform.Find("notice/fsnormal").gameObject;
        fslesson = fsNotice.transform.Find("notice/fslesson").gameObject;
        if (IsLessonBuild)
        {
            fsnormal.SetActive(false);
        }
        else
        {
            fslesson.SetActive(false);
        }

        DJNotice = FindChildGO(uibottom, "notice/DJNotice").transform; 
        startBtn = FindChildGO(uibottom, "Start");

        finishAnim = FindChildGO(uicenter, "FinishAnim");
        
        finishAnimMini = FindChildGO(uicenter, "FinishAnimMini");
        
        if (null != finishAnim || null != finishAnimMini)
        {
            Transform fTransform = null;
            if (fromType == FromPlatform.Alpha_mini)
            {
                fTransform = finishAnimMini.transform;
            }
            else
            {
                fTransform = finishAnim.transform;
            }
            finishAnimBg = GameHelper.FindChildComponent<UITexture>(fTransform, "fsbg");
            if (null != finishAnimBg)
            {
                finishAnimBg.width = PublicFunction.GetWidth();
                finishAnimBg.height = PublicFunction.GetHeight();
                finishAnimBg.color = PublicFunction.Blur_Effect_Texture_Color;
            }
        }
       

        partsPanel = FindChildGO(uicenter, "PartsPanel");
        partsPanel.SetActive(true);
        partsScrollView = FindChildGO(partsPanel, "PartsScrollView"); 
        llBack = FindChildGO(partsPanel, "BtnPanel/llBack");
        llForward = FindChildGO(partsPanel, "BtnPanel/llForward");
        lackParts = FindChildGO(uicenter, "LackParts");
        lackParts.SetActive(true);

        inputlabels["fsStay"].text = nots["not7"];     //FindChildGO(finishAnim, "fsStay")
        inputlabels["fsLabel1"].text = nots["not6"];
        inputlabels["fsStayMini"].text = nots["not7"];
        inputlabels["fsNameMini"].text = nots["not27"];
        //FindLabel(FindChildGO(fsNotice, "notice/fsConfirm")).text = nots["not8"];
        FindLabel(FindChildGO(finishAnim, "fsAction")).text = nots["not10"];
        FindLabel(FindChildGO(finishAnimMini, "fsAction")).text = nots["not28"];
        FindLabel(FindChildGO(finishAnim, "fsControl")).text = nots["not11"];
        FindLabel(FindChildGO(uicenter, "LackParts/lpnotice1")).text = nots["not19"];
        FindLabel(FindChildGO(uicenter, "LackParts/lpnotice2")).text = nots["not20"];
        FindLabel(FindChildGO(uicenter, "AnZhuangAnim2/Panel/Label_1")).text = nots["not21"];
        FindLabel(FindChildGO(uicenter, "AnZhuangAnim2/Panel/Label_2")).text = nots["not22"];
       // FindLabel(FindChildGO(uicenter, "DryCellNotice/close")).text = nots["not8"];

        FindLabel(FindChildGO(fsNotice, "notice/fsnormal/finishNotice")).text = nots["not6"];
        FindLabel(FindChildGO(fsNotice, "notice/fsnormal/finishNotice2")).text = nots["not4"];

        string noticels = RecordContactInfo.Instance.FindLessonfsNotice();
        if(noticels!=null)
        {
            FindLabel(FindChildGO(fsNotice, "notice/fslesson/finishNotice")).text = LauguageTool.GetIns().GetText(noticels);
        }
        else
        {
            FindLabel(FindChildGO(fsNotice, "notice/fslesson/finishNotice")).text = nots["lessonFinish"];
        }

        FindLabel(FindChildGO(uicenter, "LackParts/lpConfirm")).text = nots["not23"];
 
        FindLabel(startBtn).text = nots["not12"];

        


        DisableBtnClick(btns["Refresh"]);
        DisableBtnClick(partsControl);
        DisableBtnClick(llBack);

        llBack.SetActive(false);
        llForward.SetActive(false);

        finishAnim.SetActive(false);
        finishAnimMini.SetActive(false);
        FindChildGO(uicenter, "AnZhuangAnim").SetActive(false);
        FindChildGO(uicenter, "AnZhuangAnim2").SetActive(false);
        FindChildGO(uicenter, "DryCellNotice").SetActive(false);
        fsNotice.SetActive(false);



        FindChildGO(partsControl, "icon").transform.localEulerAngles = new Vector3(180.0f, 0, 0);
        partsStep.GetComponent<UISprite>().color = new Color(1.0f, 1.0f, 1.0f, 0);
        juBuStep.GetComponent<UISprite>().color = new Color(1.0f, 1.0f, 1.0f, 0);
        jubuIcon = FindChildGO(uileft, "JuBu").transform.GetChild(0).GetComponent<UISprite>();
        jubuIcon.spriteName = "btn@2x";
        jubuTexture = FindChildGO(juBuStep, "gotexture").GetComponent<UITexture>();
        jubuBGTexture = FindChildGO(juBuStep, "gobgtexture").GetComponent<UITexture>();

        foreach (string temp in btns.Keys)
        {
            btnNam.Add(temp);
        }
        

    }



    /// <summary>
    /// 给按钮添加事件
    /// </summary>
    public void AddEvents()
    {
        UIEventListener.Get(btns["Forward"]).onClick += CreateForward;
        UIEventListener.Get(btns["Back"]).onClick += CreateBack;

        UIEventListener.Get(btns["Return"]).onClick += ReturnScene;
        UIEventListener.Get(FindChildGO(finishAnim, "fsReturn")).onClick += GoToStay;
        UIEventListener.Get(FindChildGO(finishAnim, "fsAction")).onClick += GoToActionList;
        UIEventListener.Get(FindChildGO(finishAnim, "fsControl")).onClick += GoToControl;
        UIEventListener.Get(FindChildGO(finishAnim, "fsStay")).onClick += GoToStay;
        UIEventListener.Get(FindChildGO(finishAnimMini, "fsAction")).onClick += GoToMiniApp;
        UIEventListener.Get(FindChildGO(finishAnimMini, "fsReturn")).onClick += GoToStay;
        UIEventListener.Get(FindChildGO(finishAnimMini, "fsStayMini")).onClick += GoToStay;

        UIEventListener.Get(FindChildGO(uileft, "JuBu")).onClick += jubustepSHide;
        UIEventListener.Get(partsControl).onClick += pstepSHide;
        UIEventListener.Get(startBtn).onClick += HidepPanel;

        UIEventListener.Get(btns["Confirm"]).onClick += ShowStepBtns;

        UIEventListener.Get(FindChildGO(fsNotice, "notice/fsConfirm")).onClick += ConfirmFinish;
        UIEventListener.Get(createPList).onClick += CreatePList; //ListNextPage
        UIEventListener.Get(llForward).onClick += ListNextPage;
        UIEventListener.Get(llBack).onClick += ListLastPage;

        UIEventListener.Get(FindChildGO(uicenter, "AnZhuangAnim/close")).onClick += CloseAnZhuangAnim;
        UIEventListener.Get(FindChildGO(uicenter, "AnZhuangAnim2/close")).onClick += CloseAnZhuangAnim2;
        UIEventListener.Get(FindChildGO(uicenter, "DryCellNotice/close")).onClick += CloseDryCellNotice;
        UIEventListener.Get(FindChildGO(uicenter, "LackParts/lpConfirm")).onClick += CloseLackParts;
        tempslider.onDragFinished += DragFinished; //进度条拖拽结束后的操作
    }

    /// <summary>
    /// 创建模型，分割动画
    /// </summary>
    public void InitialModelAnims()
    {
        oriGO = GameObject.Find("oriGO");
        /*if (null == oriGO)
        {
            oriGO = GameObject.Find("oriTemp");
        }*/
        if (oriGO != null)
        {
            RobotMgr.Instance.ResetParent(robotname, oriGO);
            RobotMgr.Instance.ResetDJDPPAOld();
            foreach (Transform child in oriGO.transform.GetComponentInChildren<Transform>())
            {
                if (AddedAllGOs.ContainsKey(child.name) == false)
                {
                    AddedAllGOs.Add(child.name, child.gameObject);
                    string gotemp = RobotMgr.GoType(child.name);
                    if (gotemp == "seivo")
                    {
                        if (djIntGO.ContainsKey(child.name) == false)
                        {
                            DJClass djc = new DJClass();
                            djc = RobotMgr.Instance.FindDJPosGO(child);
                            djIntGO.Add(child.name, djc);
                        }

                    }

                    //记录主控盒数字编号
                    if (child.name.Contains("mc"))
                    {
                        List<string> numT = NormalStringData.zkhNumbers();
                        //Debug.Log("fdfdg numT:" + numT.Count);
                        Shader shaderT = Shader.Find("Jimu/bbbb");
                        foreach (Transform childT in child.transform.GetComponentsInChildren<Transform>())
                        {
                            if (numT.Contains(childT.name))
                            {
                                childT.GetComponent<Renderer>().material.shader = shaderT;
                                if (zkhNums.ContainsKey(childT.name) == false)
                                {
                                    zkhNums.Add(childT.name, childT.gameObject);
                                }
                            }
                        }
                    }
                }

            }

            inputlabels["NowStep"].text = "0";
            FindOriClip();
        }
    }

    /// <summary>
    /// 零件库信息
    /// </summary>
    public void PartListData()
    {
        //找到内置的图片
        innerSprites = GetInnerTexList.Instance.FindPicType();
        foreach (Transform child in partsStep.GetComponent<Transform>())
        {
            if (child.name == "goLabel") goLabel = child.GetComponent<UILabel>();
            if (child.name == "goicon") { goicon = child.GetComponent<UISprite>(); iconGO = child.gameObject; }
            if (child.name == "gotexture") { goTexture = child.GetComponent<UITexture>(); textureGO = child.gameObject; }
        }


        string pathT = ResourcesEx.GetCommonPathForNoTypeName(robotNameNoType) + "/partsData";

        if (System.IO.Directory.Exists(pathT) != false)
        {

            startBtn.SetActive(false);
            partsPanel.SetActive(false);
        }
        else
        {
            startBtn.SetActive(false);
            partsPanel.SetActive(false);
            partsControl.SetActive(false);
            partsStep.SetActive(false);

            createPList.SetActive(false);
            noParts = true;
        }

        string pathTT = ResourcesEx.GetCommonPathForNoTypeName(robotNameNoType) + "/JuBuPic";
        if (System.IO.Directory.Exists(pathTT) == false)
        {
            juBuStep.SetActive(false);
            FindChildGO(uibottom, "JuBu").SetActive(false);
            noJuBu = true;
        }

        goLabel.text = null;
        goicon.spriteName = "Clean";

    }

    /// <summary>
    /// 关闭缺少零件提示框
    /// </summary>
    /// <param name="go"></param>
    public void CloseLackParts(GameObject go)
    {
        lackParts.SetActive(false);
    }

    /// <summary>
    /// 关闭安装时插入提示的动画
    /// </summary>
    /// <param name="go"></param>
    public void CloseAnZhuangAnim(GameObject go)
    {
        FindChildGO(uicenter, "AnZhuangAnim/Panel").transform.GetComponent<Animation>().Stop();

        StartCoroutine(DelayPlay("AnZhuangAnim"));
        
    }

    public void CloseAnZhuangAnim2(GameObject go)
    {
        FindChildGO(uicenter, "AnZhuangAnim2/Panel").transform.GetComponent<Animation>().Stop();

        StartCoroutine(DelayPlay("AnZhuangAnim2"));

    }

    public void CloseDryCellNotice(GameObject go)
    {
        FindChildGO(uicenter, "DryCellNotice").SetActive(false);
        ShowBtns();
    }

    IEnumerator DelayPlay(string nameT)
    {
        yield return new WaitForSeconds(0.1f);
        FindChildGO(uicenter, nameT).SetActive(false);
        ShowBtns();
    }


    #region 加载动画
    /// <summary>
    /// 加载动画数据及一些基本处理
    /// </summary>
    public void FindOriClip()
    {

        string[] x = RecordContactInfo.Instance.FindPosModel(robotNameNoType);
        oriGOPos = SingletonObject<PublicTools>.GetInst().StringToVector(x[0]);
        oriGOAngle = SingletonObject<PublicTools>.GetInst().StringToVector(x[1]);

        oriTemp = CreateUnityGO.CreateEmptyGO();
        oriTemp.name = "oriTemp";
        oriTemp.transform.position = oriGOPos;
        oriTemp.transform.eulerAngles = oriGOAngle;
        oriGO.transform.parent = oriTemp.transform;

        robotIDTemp = RobotMgr.Instance.FindRobotID(robotname);
        animationTemp = oriGO.GetComponent<Animation>();
        if (animationTemp == null)
        {

            oriGO.AddComponent<Animation>();
        }
        else
        {
            animationTemp.enabled = true;
        }

        animationTemp = oriGO.GetComponent<Animation>();

        //动画数据
        if (rbtAnim.ContainsKey(robotIDTemp) == false)
        {
           // RobotMgr.Instance.CreateAnim(robotIDTemp);
            AnimData animDataT = new AnimData();
            AnimBase animb;
            Dictionary<string, string[]> animdata = new Dictionary<string, string[]>();
            animdata = AnimReadData.Instance.FindAnimData();
            ecgnotice = AnimReadData.Instance.FindEcgNotice();

            for (int i = 1; i <= animdata.Count; i++)
            {
                animb = new AnimBase();
                string id = i.ToString();
                animb.id = animdata[id][0];
                animb.source = animdata[id][1];
                animb.start = animdata[id][2];
                animb.end = animdata[id][3];
                animb.step = animdata[id][4];
                animb.name = animdata[id][5];
                animb.parts = animdata[id][6];

                animb.djid = animdata[id][7];
                animb.shape = animdata[id][8];
                animb.line = animdata[id][9];
                animb.type = animdata[id][10];

                animb.goname = animdata[id][11];
                animb.pic = animdata[id][12];
                animb.firstPic = animdata[id][13];
                animb.lvdaiNum =animdata[id][14];
                animb.sensorID = animdata[id][15];
                if (animDataT.anims.ContainsKey(id) == false)
                {
                    animDataT.anims.Add(id, animb);
                }

                if (animtype.Contains(animb.type) == false)
                {
                    animtype.Add(animb.type);
                }

                if(allSprites.Contains(animdata[id][6])==false)
                {
                    allSprites.Add(animdata[id][6]);
                    
                }

                if(jubuSpritesName.Contains(animdata[id][12])==false)
                {
                
                    jubuSpritesName.Add(animdata[id][12]);
                }
                
                
            }
            rbtAnim.Add(robotIDTemp, animDataT);


            if (jubuSpritesName != null && jubuSpritesName.Count > 0)
            {
                jubuSpritesName.Add("background");
            }

            clipCount = rbtAnim[robotIDTemp].anims.Count;

            foreach(string temp in allSprites)
            {
                if (innerSprites.Contains(temp) == false && temp != "")
                {
                    if (temp.IndexOf('-') > 0)    //pic-WHT + 色号
                    {
                        string[] picStr = temp.Split('-');
                        Dictionary<string,string> colorType = SingletonObject<PublicTools>.GetInst().PartsPicColor();
                        string picNameTemp = picStr[0] + "-WHT";
                        if (!colorType.ContainsKey(picStr[1])) 
                        {
                            if (outAddSprites.Contains(temp) == false)
                            {
                                outAddSprites.Add(temp);
                            }
                        }
                    }
                    else
                    {
                        if (outAddSprites.Contains(temp) == false)
                        {
                            outAddSprites.Add(temp);
                        }
                    }
                }

               
            }

            textureTT = Resources.Load("Prefab/Test4/UI/Clean") as Texture;



            if (noJuBu == false)
            {
                StartCoroutine(AddJuBuPic(jubuSpritesName, jubuPCount));
            }
            else
            {
                if (outAddSprites != null && outAddSprites.Count > 0)
                {
                    StartCoroutine(AddOtherPic(outAddSprites, outPCount));
                }
                else
                {
                    AddAnimAssets();
                }
            }
        }

    }


    int jubuPCount = 0;
    /// <summary>
    /// 局部图的处理
    /// </summary>
    /// <param name="pics"></param>
    /// <param name="pcount"></param>
    /// <returns></returns>
    IEnumerator AddJuBuPic(List<string> pics,int pcount)
    {
       
        if(pics.Count>pcount)
        {

            //零件图片
            string pathTemp = "file:///" + ResourcesEx.GetCommonPathForNoTypeName(robotNameNoType) + "//JuBuPic//" + pics[pcount] + ".png";
            //Debug.Log("dfsfdsf:" + pathTemp);
            WWW www = new WWW(pathTemp);

            Texture textureT = null;
            yield return www;
            try
            {
                if (www != null && string.IsNullOrEmpty(www.error))
                {
                    //获取Texture
                    textureT = www.texture;
                    //更多操作...    
                    if (textureT != null && jubuPics.ContainsKey(pics[pcount]) == false)
                    {

                        //Debug.Log("dfsfdsf:"+pics[jubuPCount]);
                        jubuPics.Add(pics[pcount], textureT);
                    }

                }
                else
                {
                    if (textureTT != null && jubuPics.ContainsKey(pics[pcount]) == false)
                    {
                        jubuPics.Add(pics[pcount], textureTT);
                    }
                }
                }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                //EnableBtnClick(btns["Return"],Color.white);
                openReturnBtn = true;
                RecordContactInfo.Instance.CloseRootABL();
                string notFind = LauguageTool.GetIns().GetText("模型数据读取失败");
                DownloadModelResult(false);
                PromptMsg.ShowSinglePrompt(notFind);
            }

            pcount++;
            //Debug.Log("pics.Count:" + pics.Count + ";jubuPics.Count:" + jubuPics.Count);
            if (pics.Count == jubuPics.Count)
            {

                if (outAddSprites != null && outAddSprites.Count > 0)
                {
                    StartCoroutine(AddOtherPic(outAddSprites, outPCount));
                }
                else
                {
                    AddAnimAssets();
                }

            }
            else if (pcount <= pics.Count)
            {
                //Debug.Log("outNameT:"+outNamT[i]+";00outPics.Count:" + outPics.Count + ";00outNamT.Count:" + outNamT.Count);
                StartCoroutine(AddJuBuPic(pics, pcount));
            }
        }
    }


    int outPCount = 0;
    /// <summary>
    /// 添加其他图片
    /// </summary>
    /// <param name="outNamT"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    IEnumerator AddOtherPic(List<string> outNamT, int i)
    {

        if (outNamT.Count > i)
        {

            //零件图片
            //string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "//partsPic//" + outNamT[i] + ".png";
            string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "/default/" + robotNameNoType + "/partsPic/"+outNamT[i] + ".png";

            WWW www = new WWW(pathTemp);

            Texture textureT = null;
            yield return www;
            try
            {

                if (www != null && string.IsNullOrEmpty(www.error))
                {
                    //获取Texture
                    textureT = www.texture;
                    //更多操作...    
                    if (textureT != null && outPics.ContainsKey(outNamT[i]) == false)
                    {
                        outPics.Add(outNamT[i], textureT);
                    }

                }
                else
                {
                    if (textureTT != null)
                    {
                        //Debug.Log("sfff:" + outNamT[i]);
                        outPics.Add(outNamT[i], textureTT);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                //EnableBtnClick(btns["Return"],Color.white);
                openReturnBtn = true;
            }
            i++;

            //Debug.Log("outPics.Count:" + outPics.Count + ";outNamT.Count:" + outNamT.Count);
            if (outPics.Count == outNamT.Count)
            {
                AddAnimAssets();
            }
            else if (i <= outNamT.Count)
            {
                //Debug.Log("outNameT:"+outNamT[i]+";00outPics.Count:" + outPics.Count + ";00outNamT.Count:" + outNamT.Count);
                StartCoroutine(AddOtherPic(outNamT, i));
            }

        }
    }

    /// <summary>
    /// 动画资源的处理
    /// </summary>
    public void AddAnimAssets()
    {
        List<string> RtID = NormalStringData.DefaultRtID();
        if (RtID.Contains(robotIDTemp))   //目前只有起落杆的动画
        {

            string opentype = RecordContactInfo.Instance.openType;

           // Debug.Log("name:" + animtype[0]);

            string path = "Prefab/Test4/Anims/" + animtype[0];
            
          try{
            clipTemp = Resources.Load(path) as AnimationClip;
            clipTemp.name = animtype[0];

            if (clipTemp != null)
            {
                //Debug.Log("start press:" + clipTemp.name);
                anims.Add(clipTemp.name, clipTemp);
            }

            clipCount = rbtAnim[robotIDTemp].anims.Count;

          }
        catch (System.Exception ex)
        {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                RecordContactInfo.Instance.CloseRootABL();
                string notFind = LauguageTool.GetIns().GetText("模型数据读取失败");
                PromptMsg.ShowSinglePrompt(notFind);
                DownloadModelResult(false);
         }
            SimulateStart(oriGO);
            oriGO.name = robotNameNoType;
            
        }
        else
        {
            string opentype = RecordContactInfo.Instance.openType;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path1 = "file:///" + ResourcesEx.persistentDataPath + "/" + opentype + "/" + robotNameNoType + "/clip/editor/" + robotIDTemp + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path1 = "file:///" + ResourcesEx.persistentDataPath + "/" + opentype + "/" + robotNameNoType + "/clip/ios/" + robotIDTemp + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                path1 = "file:///" + ResourcesEx.persistentDataPath + "/" + opentype + "/" + robotNameNoType + "/clip/ios/" + robotIDTemp + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                path1 = "file:///" + ResourcesEx.persistentDataPath + "/" + opentype + "/" + robotNameNoType + "/clip/android/" + robotIDTemp + ".assetbundle";
            }

            StartCoroutine(GetClip(robotIDTemp));
        }
    }

    WWW bundle1 = null;
    /// <summary>
    /// 加载外部动画资源
    /// </summary>
    /// <param name="idtemp"></param>
    /// <returns></returns>
    /// 
    bool openReturnBtn = false;
    IEnumerator GetClip(string idtemp)
    {
        clipTemp = null;
        
        if (bundle1 != null)
        {
            bundle1.Dispose();

        }

        bundle1 = new WWW(path1);

        yield return bundle1;
        if (bundle1.error == null)
        {
            PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "加载外部模型动画clip资源");
            try
            {
                foreach (string temp in animtype)
                {
                    if (anims.ContainsKey(temp) == false)
                    {
                        UnityEngine.Object t = bundle1.assetBundle.LoadAsset(temp);

                        clipTemp = GameObject.Instantiate(t) as AnimationClip;
                        clipTemp.name = temp;
                        if (clipTemp != null)
                        {
                            anims.Add(temp, clipTemp);
                        }
                        bundle1.assetBundle.Unload(true);
                    }
                }


            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                //EnableBtnClick(btns["Return"],Color.white);
                openReturnBtn = true;
                RecordContactInfo.Instance.CloseRootABL();
                string notFind = LauguageTool.GetIns().GetText("模型数据读取失败");
                PromptMsg.ShowSinglePrompt("模型数据读取失败，请确认手机读写权限已打开");
                DownloadModelResult(false);
            }
        }
        else
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + bundle1.error);
            openReturnBtn = true;
            RecordContactInfo.Instance.CloseRootABL();
            string notFind = LauguageTool.GetIns().GetText("模型数据读取失败");
            PromptMsg.ShowSinglePrompt("模型数据读取失败，请确认手机读写权限已打开");
            DownloadModelResult(false);
        }
        

        if (jubuPics.ContainsKey("background"))
        {
            jubuBGTexture.mainTexture = jubuPics["background"];
        }

        clipCount = rbtAnim[robotIDTemp].anims.Count;
        
        SimulateStart(oriGO);
        oriGO.name = robotNameNoType;
        
    }


    //IEnumerator FindOriClip(float t)
    //{
    //    yield return new WaitForSeconds(t);

    //    if (oriGO != null)
    //    {
    //        foreach (Transform child in oriGO.transform.GetComponentInChildren<Transform>())
    //        {
    //            if (AddedAllGOs.ContainsKey(child.name) == false)
    //            {

    //                AddedAllGOs.Add(child.name, child.gameObject);

    //                string gotemp = RobotMgr.GoType(child.name);
    //                if (gotemp == "seivo")
    //                {
    //                    if (djIntGO.ContainsKey(child.name) == false)
    //                    {
    //                        DJClass djc = new DJClass();
    //                        djc = RobotMgr.Instance.FindDJPosGO(child);
    //                        djIntGO.Add(child.name, djc);
    //                    }

    //                }
    //            }

    //        }
    //    }

    //    FindOriClip();
    //}

    #endregion



   #region 创建零件菜单列表
    PartsSprites pSprite;
    bool createP = false;
    /// <summary>
    /// 创建零件列表
    /// </summary>
    /// <param name="t"></param>
    public void CreatePList(GameObject t)
    {
        createPList.GetComponent<BoxCollider>().enabled = false;
        if(createP==false)
        {
            //Debug.Log("Create:");
            startBtn.SetActive(true);
            partsPanel.SetActive(true);
           
            string pathT = ResourcesEx.GetCommonPathForNoTypeName(robotNameNoType) + "/partsData";

            if (System.IO.Directory.Exists(pathT) != false)
            {
                pSprite = FindChildGO(partsScrollView, "PartsUIWrap").AddComponent<PartsSprites>();
                pageNum = GetpSpritesData.Ins.FindPanels().Count;


                noParts = false;
            }

            createP = true;
        }
        else
        {
            //Debug.Log("reteate:");
            if(pSprite!=null)
            {
                Destroy(pSprite);
            }
            partsScrollView.transform.localPosition=Vector3.zero;
            partsScrollView.GetComponent<UIScrollView>().ResetPosition();
            startBtn.SetActive(true);
            partsPanel.SetActive(true);
        }

    }

    
    Vector3 pScrollPos;
    int pageNum;
    int countPage = 0;
    bool startPage = false;
    public float pswWidth;
    /// <summary>
    /// 点击查看下一页零件
    /// </summary>
    /// <param name="t"></param>
    public void ListNextPage(GameObject t)  
    {

        //countPage++;
        //if(countPage<pageNum)
        //{
        //    pScrollPos = partsScrollView.transform.localPosition;
        //    Vector3 posT = new Vector3(pScrollPos.x - pswWidth, 0, 0);
        //    SpringPanel.Begin(partsScrollView, posT, 6f); //spT=partsScrollView.GetComponent<SpringPanel>();

        //    if (llBack.GetComponent<BoxCollider>().enabled == false)
        //    {
        //        EnableBtnClick(llBack, Color.white);
        //    }
        //    if(countPage==pageNum-1)
        //    {
        //        DisableBtnClick(llForward);
        //    }
        //}

    }

    
    /// <summary>
    /// 点击查看上一页零件
    /// </summary>
    /// <param name="t"></param>
    public void ListLastPage(GameObject t)
    {
        //countPage--;
        //if (countPage >=0)
        //{

        //    //Debug.Log("countPage last:" + countPage);
        //    pScrollPos = partsScrollView.transform.localPosition;
        //    Vector3 posT = new Vector3(pScrollPos.x + pswWidth, 0, 0);
        //    SpringPanel.Begin(partsScrollView, posT, 6f);

        //    if (llForward.GetComponent<BoxCollider>().enabled == false)
        //    {
        //        EnableBtnClick(llForward, Color.white);
        //    }

        //    if (countPage ==0)
        //    {
        //        DisableBtnClick(llBack);
        //    }
        //}

    }

    
    /// <summary>
    /// 打开完成恭喜页面
    /// </summary>
    /// <param name="t"></param>
    public void ConfirmFinish(GameObject t)
    {
        
        stepTemp--;
        if (IsLessonBuild)
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                PlatformMgr.Instance.BackCourseMenu();
            }
            else
            {
                SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot(), delegate() {
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {
                        PlatformMgr.Instance.BackCourseMenu();
                    }
                });
            }
        }
        else
        {
            if (fromType == FromPlatform.Alpha_mini)
            {
                finishAnimMini.SetActive(true);
            }
            else
            {
                finishAnim.SetActive(true);
            }
        }
       
    }

    /// <summary>
    /// 查找模型的子物体
    /// </summary>
    /// <param name="paGO"></param>
    /// <param name="nameT"></param>
    /// <returns></returns>
    private GameObject FindChildGO(GameObject paGO, string nameT)
    {
        GameObject btnTemp = paGO.transform.Find(nameT).gameObject;
        return btnTemp;
    }
#endregion

    
    /// <summary>
    /// 隐藏步骤提示按钮
    /// </summary>
    /// <param name="t"></param>
    public void ShowStepBtns(GameObject t)
    {

        DJNotice.GetComponent<TweenPosition>().PlayReverse();
        DJNotice.GetComponent<TweenAlpha>().PlayReverse();

        StartCoroutine(HideDJNotice(0.35f));
    }

    /// <summary>
    /// 隐藏舵机提示
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    IEnumerator HideDJNotice(float t)
    {
        yield return new WaitForSeconds(t);
        DJNotice.gameObject.SetActive(false);
        ShowBtns();
    }

    public void ShowBtns()
    {
        EnableBtnClick(btns["Forward"], Color.white);
        EnableBtnClick(btns["Back"], Color.white);
        EnableBtnClick(btns["Return"], Color.white);
        EnableBtnClick(createPList, Color.white);
    }


    /// <summary>
    /// 隐藏零件panel
    /// </summary>
    /// <param name="t"></param>
    public void HidepPanel(GameObject t)
    {
        createPList.GetComponent<BoxCollider>().enabled = true;
        partsPanel.SetActive(false);
        startBtn.SetActive(false);
    }

    
    bool jubustepshow = true;
    /// <summary>
    /// 单步显示局部图信息时，隐藏与显示
    /// </summary>
    /// <param name="t"></param>
    public void jubustepSHide(GameObject t)
    {
        
        if (jubustepshow)
        {
            juBuStep.GetComponent<TweenAlpha>().PlayReverse();

            jubuIcon.spriteName = "btn_press@2x";
            jubustepshow = false;
        }
        else
        {
            juBuStep.GetComponent<TweenAlpha>().PlayForward();

            jubuIcon.spriteName = "btn@2x";
            jubustepshow = true;

        }

    }


    bool pstepshow = true;
    /// <summary>
    /// 单步显示零件信息时，隐藏与显示
    /// </summary>
    /// <param name="t"></param>
    public void pstepSHide(GameObject t)
    {
        if (pstepshow)
        {
            partsStep.GetComponent<TweenAlpha>().PlayForward();
            FindChildGO(partsControl, "icon").GetComponent<TweenRotation>().PlayForward();
            pstepshow = false;
        }
        else
        {
            partsStep.GetComponent<TweenAlpha>().PlayReverse();
            FindChildGO(partsControl, "icon").GetComponent<TweenRotation>().PlayReverse();
            pstepshow = true;
        }

    }

    void OnDestroy()
    {
        IsLessonBuild = false;
        EventMgr.Inst.UnRegist(EventID.GuideShutdown, GuideShutdownCallback);
        EventMgr.Inst.UnRegist(EventID.Blue_Connect_Finished, ExitBTConnect);
        PlatformMgr.Instance.RemoveDurationEvent(MobClickEventID.app_building_complete);
        PlatformMgr.Instance.MobPageEnd(MobClickEventID.P6);
    }

    void ResizeSliderLines()
    {
        try 
        {
            if (null == mUIRootABL)
            {
                GameObject obj = GameObject.Find("UIRootABL");
                if (null == obj)
                {
                    return;
                }
                mUIRootABL = obj.transform;
            }
            //Transform lines = mUIRootABL.FindChild("Camera/Bottom/StepSlider/lines");
            //if (null == lines)
            //{
            //    return;
            //}
            //int width = slider.GetComponent<UISprite>().width;

            //if (width > 880)
            //{
            //    for (int i = 0; i < lines.childCount; i++)
            //    {
            //        Transform line = lines.GetChild(i);
            //        int w = (i + 1) * width / 23 - width / 2;
            //        line.transform.localPosition = new Vector3(w, 0, 0);
            //    } 
            //}
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "JMSimulatorOnly ResizeSliderLines 异常- error = " + ex.ToString());
            RecordContactInfo.Instance.CloseRootABL();
            string notFind = LauguageTool.GetIns().GetText("模型数据读取失败");
            PromptMsg.ShowSinglePrompt(notFind);
            DownloadModelResult(false);
        }
    }


    void SetThumb()
    {
        try
        {
            GameObject tempAnchor = null;
            thumbT.GetComponent<UISprite>().SetAnchor(tempAnchor);
            if (!IsLessonBuild)
            {
                tempValue = tempslider.value;
                float xTemp = thumbT.transform.localPosition.x;

                bool hasThumb = RecordContactInfo.Instance.HasNode("ThumbX");
                if (hasThumb)
                {
                    xTemp = RecordContactInfo.Instance.FindOriValue("ThumbX");

                }
                thumbT.transform.localPosition = new Vector3(xTemp, 0, 0);
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            RecordContactInfo.Instance.CloseRootABL();
            string notFind = LauguageTool.GetIns().GetText("模型数据读取失败");
            PromptMsg.ShowSinglePrompt(notFind);
            DownloadModelResult(false);
        }
    }
    bool openStart = false;
    void Update()
    {

        //当动画文件加载完成后，打开start按钮
        //if (openStart == false)
        //{
        //    Debug.Log("openStart");
        //    if (AddedAllGOs.Count == goName.Count)
        //    {
        //        openStart = true;
        //        Debug.Log("openStart 000");
                
        //        Invoke("SetThumb", 0.3f);
                
        //    }
        //}
        

        #region 进度条搭建时的数据处理
        if (null != tempslider && tempslider.value != tempValue && hasFinished)
        {
            
            if (tempValue < tempslider.value)
            {

                tempValue = tempslider.value;
                stepCount = Mathf.RoundToInt(tempslider.value * clipCount);
                UISprite backSprite = btns["Back"].transform.GetComponent<UISprite>();
             
                if (stepTemp != stepCount)
                {
                    stepTemp = stepCount;

                    //Debug.Log("slider  1111:"+stepCount);
                    AnimNormal(stepCount);
                }
                
                isFinished = false;
            }
            else if (tempValue > tempslider.value)
            {
                tempValue = tempslider.value;
                stepCount = Mathf.RoundToInt(tempslider.value * clipCount);
            
                if (stepTemp != stepCount && stepCount >= 0)
                {
                    stepTemp = stepCount;
                    //Debug.Log("slider  2222:" + stepCount);
                    AnimNormal(stepCount);
                }

                isFinished = false;
            }
        }
        #endregion
    }


   
    /// <summary>
    /// 只显示舵机，以观察舵机ID
    /// </summary>
    /// <param name="go"></param>
    public void DJID(GameObject go)
    {
        if (onlyShowDJ == true)
        {
            for (int i = goName.Count - 1; i >= 0; i--)
            {
                string nameT = goName[i];
                if (nameT.Contains("seivo") == false)
                {
                    AddedAllGOs[nameT].SetActive(false);
                }
            }
            onlyShowDJ = !onlyShowDJ;
        }
        else
        {
            DJIDFalse();
        }
    }

    /// <summary>
    /// 进度条拖拽完成后的操作.
    /// </summary>
    public void DragFinished()
    {
        tempValue = tempslider.value;
        stepCount = Mathf.RoundToInt(tempslider.value * clipCount);
        UISprite backSprite = btns["Back"].transform.GetComponent<UISprite>();

        if (stepTemp != stepCount)
        {
            stepTemp = stepCount;
            AnimNormal(stepCount);
        }
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P6_2);
    } 
    /// <summary>
    /// 如果是onlyShowDJ=false，把隐藏的非舵机模型显示
    /// </summary>
    public void DJIDFalse()
    {
        if (onlyShowDJ == false)
        {
            for (int i = goName.Count - 1; i >= 0; i--)
            {
                string nameT = goName[i];
                if (nameT.Contains("seivo") == false)
                {
                    AddedAllGOs[nameT].SetActive(true);
                }
            }
            onlyShowDJ = !onlyShowDJ;
        }
    }

    
    /// <summary>
    /// 退出场景处理
    /// </summary>
    public void QuitHandle()
    {

        if (bundle1 != null && bundle1.assetBundle != null)
        {
            bundle1.assetBundle.Unload(true);
        }

        DontDestroyOnLoad(oriGO);

    }

    /// <summary>
    /// 退出场景时的处理
    /// </summary>
    /// <param name="go"></param>
    public void ReturnScene(GameObject go)
    {

        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            RecordContactInfo.Instance.RecordStepNum(tempslider.value);
            float thumbTX = thumbT.transform.localPosition.x;
            RecordContactInfo.Instance.RecordThumbX(thumbTX);
        }

        QuitHandle();


        SceneMgrTest.Instance.LastScene = SceneType.Assemble;
        try
        {

            RobotMgr.Instance.GoToCommunity();
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }

        StopAllCoroutines();
        if (fromType == FromPlatform.Alpha_mini) //返回到mini App
        {
            Debug.Log("return mini");
            PlatformMgr.Instance.BackToFromApp();
        }
        else
        {
            if (ClientMain.Use_Third_App_Flag)
            {
            
                if (IsLessonBuild)
                {
                    PlatformMgr.Instance.BackCourseMenu(false);
                }
                else
                {
                    PlatformMgr.Instance.BackThirdApp();
                }
            }
            else
            {
                SceneMgr.EnterScene(SceneType.MenuScene);
            }
       }
    }

    /// <summary>
    /// 进入动作编程界面
    /// </summary>
    /// <param name="go"></param>
    public void GoToActionList(GameObject go)
    {
        isControl = false;
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            RecordContactInfo.Instance.RecordStepNum(0);
            tempslider.value = 0;
            float thumbTX = thumbT.transform.localPosition.x;
            RecordContactInfo.Instance.RecordThumbX(thumbTX);
        }

        QuitHandle();

        if (!PlatformMgr.Instance.GetBluetoothState()) //未连接的情况
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                isActionList = true;
                SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot());
            }
            else
            {
                if (Application.platform != RuntimePlatform.WindowsEditor)
                {
                    RecordContactInfo.Instance.RecordStepNum(tempslider.value);
                    float thumbTX = thumbT.transform.localPosition.x;
                    RecordContactInfo.Instance.RecordThumbX(thumbTX);
                }

                SceneMgrTest.Instance.LastScene = SceneType.MainWindow;
                /*RobotMgr.Instance.openActionList = true;
                SceneMgr.EnterScene(SceneType.MainWindow);*/
                MainScene.GotoScene(MainMenuType.Action_Menu);
            }
        }
        else
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                RecordContactInfo.Instance.RecordStepNum(tempslider.value);
                float thumbTX = thumbT.transform.localPosition.x;
                RecordContactInfo.Instance.RecordThumbX(thumbTX);
            }

            SceneMgrTest.Instance.LastScene = SceneType.MainWindow;
            /*RobotMgr.Instance.openActionList = true;
            SceneMgr.EnterScene(SceneType.MainWindow); */
            MainScene.GotoScene(MainMenuType.Action_Menu);
        }
        
    }

    /// <summary>
    /// 进入miniApp
    /// </summary>
    /// <param name="go"></param>
    public void GoToMiniApp(GameObject go)
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            RecordContactInfo.Instance.RecordStepNum(tempslider.value);
            float thumbTX = thumbT.transform.localPosition.x;
            RecordContactInfo.Instance.RecordThumbX(thumbTX);
        }
        PlatformMgr.Instance.BackToFromApp();
    }
    
    /// <summary>
    /// 进入遥控界面
    /// </summary>
    /// <param name="go"></param>
    public void GoToControl(GameObject go)
    {
        isActionList = false;
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            RecordContactInfo.Instance.RecordStepNum(0);

            tempslider.value = 0;
            float thumbTX = thumbT.transform.localPosition.x;
            RecordContactInfo.Instance.RecordThumbX(thumbTX);
        }
  
        QuitHandle();
        PublicFunction.SetLayerRecursively(oriGOPrefab, LayerMask.NameToLayer("Arrow"));
        

        if (!PlatformMgr.Instance.GetBluetoothState()) //未连接的情况
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                isControl = true;
                SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot());
            }
            else
            {
                if (Application.platform != RuntimePlatform.WindowsEditor)
                {
                    RecordContactInfo.Instance.RecordStepNum(tempslider.value);
                    float thumbTX = thumbT.transform.localPosition.x;
                    RecordContactInfo.Instance.RecordThumbX(thumbTX);
                }
                SceneMgrTest.Instance.LastScene = SceneType.ActionPlay;
                UserdefControllerScene.GotoController(FromModuleType.From_Build);
            }
        }
        else
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                RecordContactInfo.Instance.RecordStepNum(tempslider.value);
                float thumbTX = thumbT.transform.localPosition.x;
                RecordContactInfo.Instance.RecordThumbX(thumbTX);
            }
            SceneMgrTest.Instance.LastScene = SceneType.ActionPlay;
            UserdefControllerScene.GotoController(FromModuleType.From_Build);
        }
        
    }

    
    /// <summary>
    /// 继续进行搭建
    /// </summary>
    /// <param name="go"></param>
    public void GoToStay(GameObject go)
    {
        stepCount = clipCount;
        CreateBack(oriGO);
        if (fromType == FromPlatform.Alpha_mini)
        {
            finishAnimMini.SetActive(false);
        }
        else
        {
            finishAnim.SetActive(false);
        }
        fsNotice.SetActive(false);
    }

    #region 隐藏
    /// <summary>
    /// 刚进入场景时隐藏相应控件
    /// </summary>
    public void EnterSceneHide()
    {
        for (int i = 0; i < btnNam.Count; i++)
        {
            if (btnNam[i] != "Return")
            {
                btns[btnNam[i]].SetActive(false);
            }

        }
        DJNotice.gameObject.SetActive(false);
        FindChildGO(uiright, "NoticeBG").SetActive(false);
        foreach (string temp in inputlabels.Keys)
        {
            inplabel.Add(temp);
        }
        for (int i = 0; i < inplabel.Count; i++)
        {
            inputlabels[inplabel[i]].enabled = false;

        }

        foreach (string temp in labels.Keys)
        {
            notelabel.Add(temp);
        }
        for (int i = 0; i < notelabel.Count; i++)
        {
            //Debug.Log("labelname:" + notelabel[i]);
            labels[notelabel[i]].gameObject.SetActive(false);
        }


        slider.SetActive(false);
    }

    public void PressStart()
    {
        for (int i = 0; i < btnNam.Count; i++)
        {
            if (btnNam[i] != "Connect")
            {

                btns[btnNam[i]].SetActive(true);
            }
            else
            {
                btns[btnNam[i]].SetActive(false);
            }

        }

        for (int i = 0; i < inplabel.Count; i++)
        {
            inputlabels[inplabel[i]].enabled = true;
        }

        for (int i = 0; i < notelabel.Count; i++)
        {
            labels[notelabel[i]].gameObject.SetActive(true);
        }
        FindChildGO(uiright, "NoticeBG").SetActive(true);

        slider.SetActive(true);
 
    }

    #endregion

    #region ButtonHandle

    /// <summary>
    /// 按钮点击无效
    /// </summary>
    /// <param name="btn"></param>
    public void DisableBtnClick(GameObject btn)
    {
        btn.GetComponent<BoxCollider>().enabled = false;

        if (btn.GetComponent<UIButtonColor>() != null && btn.GetComponent<UIButtonColor>().enabled == true)
        {
            btn.GetComponent<UIButtonColor>().enabled = false;
        }

        btn.transform.GetComponent<UISprite>().color = new Color(1,1,1,0.7f);
        foreach(Transform child in btn.GetComponent<Transform>())
        {
            if(child.GetComponent<UISprite>() !=null)
            {
               child.transform.GetComponent<UISprite>().color = new Color(1,1,1,0.7f);
            }
        }
    }
    
    /// <summary>
    /// 按钮点击有效
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="newColor"></param>
    public void EnableBtnClick(GameObject btn, Color newColor)
    {
        btn.GetComponent<BoxCollider>().enabled = true;

        if (btn.GetComponent<UIButtonColor>() != null && btn.GetComponent<UIButtonColor>().enabled == false)
        {
            btn.GetComponent<UIButtonColor>().enabled = true;
        }


        btn.transform.GetComponent<UISprite>().color = new Color(1, 1, 1, 1);
        foreach (Transform child in btn.GetComponent<Transform>())
        {
            if (child.GetComponent<UISprite>() != null)
            {
                child.transform.GetComponent<UISprite>().color = new Color(1, 1, 1, 1);
            }
        }
    }
    #endregion

    

    /// <summary>
    /// 获得当前步数
    /// </summary>
    /// <param name="stepN"></param>
    /// <returns></returns>
    public string GetNowStep(int stepN)
    {
        string nowstepTemp = stepN.ToString();

        string nowstep = AnimReadData.Instance.FindStep(nowstepTemp);
        return nowstep;
    }

    /// <summary>
    /// 搭建开始
    /// </summary>
    /// <param name="go"></param>
    public void SimulateStart(GameObject go)
    {
        DownloadModelResult(true);
        EventMgr.Inst.Fire(EventID.ModelLoadFinish, null);
        PressStart();

        DisableBtnClick(btns["Back"]);

        GameObject goT = FindChildGO(uicenter, "protect");

        if (goT != null && goT.GetComponent<BoxCollider>())
        {
            goT.GetComponent<BoxCollider>().enabled = false;
        }

        RobotMgr.OpenDJPosShowSec(AddedAllGOs);

        string[] x = RecordContactInfo.Instance.FindPosAnim(robotNameNoType);
        oriGOPos = SingletonObject<PublicTools>.GetInst().StringToVector(x[0]);
        oriGOAngle = SingletonObject<PublicTools>.GetInst().StringToVector(x[1]);
        oriGOScale = SingletonObject<PublicTools>.GetInst().StringToVector(x[2]);
        oriTemp.transform.position = oriGOPos;      
        oriTemp.transform.eulerAngles = oriGOAngle;
        oriTemp.transform.localScale = oriGOScale;

        string robotid = RobotMgr.Instance.rbt[robotname].id;
        if (rbtAnim.ContainsKey(robotid))
        {
            clipCount = rbtAnim[robotid].anims.Count;
            string allstep = clipCount.ToString();
            inputlabels["TotalSteps"].text = GetNowStep(clipCount);
        }

        oriStep = Mathf.RoundToInt(oriValue * clipCount);
        if (oriStep > 0)
        {
            stepCount = oriStep - 1;
            mRecordBuildDurationFlag = false;
        }
        else
        {
            stepCount = 0;
            mRecordBuildDurationFlag = true;
        }
       
        string stepTempL = stepCount.ToString();
        if (inputlabels["NowStep"].text != stepTempL)
        {
            inputlabels["NowStep"].text = GetNowStep(stepCount);
        }

        //重置进度条的值
        tempslider.value =oriValue;
        tempValue = tempslider.value;
        

        oriGO.SetActive(false);
        hasFinished = true;

        PlatformMgr.Instance.CloseWaitingPage();

        
       CreateForward(null);
       Invoke("SetThumb", 0.5f);
       
       if (RobotMgr.Instance.hideGOs != null && RobotMgr.Instance.hideGOs.Count > 0)
       {
           for (int i = RobotMgr.Instance.hideGOs.Count - 1; i >= 0; i--)
           {
               RobotMgr.Instance.hideGOs[i].SetActive(true);
           }
       }
    }

    IEnumerator DelayCmd()
    {
        yield return new WaitForSeconds(0f);
        PlatformMgr.Instance.CloseWaitingPage();
    }

    /// <summary>
    /// 单步分割动画
    /// </summary>
    /// <param name="tempanim"></param>
    /// <param name="id"></param>
    public void AddClips(Animation tempanim, string id)
    {
        if (null == RobotMgr.Instance.rbt || !RobotMgr.Instance.rbt.ContainsKey(robotname))
        {
            return;
        }
        string robotid = RobotMgr.Instance.rbt[robotname].id;
        if (null == rbtAnim || !rbtAnim.ContainsKey(robotid) || null == rbtAnim[robotid].anims || !rbtAnim[robotid].anims.ContainsKey(id))
        {
            return;
        }
        string source = rbtAnim[robotid].anims[id].source;
        string start = rbtAnim[robotid].anims[id].start;
        int startframe = PublicFunction.IsInteger(start) ? Int32.Parse(start) : 0;
        string end = rbtAnim[robotid].anims[id].end;
        int endframe = PublicFunction.IsInteger(end) ? Int32.Parse(end) : 0;
        string name = rbtAnim[robotid].anims[id].name;
        string type = rbtAnim[robotid].anims[id].type;

        if (tempanim.GetClip(name) == null)
        {
            //Debug.Log("name:"+name+";type:"+type);
            if (anims.ContainsKey(type))
            {
                tempanim.AddClip(anims[type], name, startframe, endframe);
                addedClipName = name;
            }
        }
    }



    #region new 不需要变换父子关系----点击一步一步执行
    private Dictionary<string, Vector3> backPoses = new Dictionary<string, Vector3>();
    private string startPos;  //物体的角度
    private Vector3 objStPos;

    string djname;
    string pos;
    Component djposanim;
    string lineIndex;

    Component uvTrans;
    Animation saTrans;

    bool isFirstStart=true;    //第二次进入时，避免出错
    /// <summary>
    /// 动画公用
    /// </summary>
    /// <param name="stepTemp"></param>
    public void AnimNormal(int stepTempT)
    {
        JustButtonsEnabled(stepTempT);

        if (!openReturnBtn)
        {
            AnimBaseHandle(stepTempT);
        }
        

        if (stepTempT == 0)
        {
            oriGO.SetActive(false);

            goLabel.text = null;
            goicon.spriteName = "Clean";
        }
        else
        {
            if(oriGO.activeInHierarchy==false)
            {
                oriGO.SetActive(true);
            }

            string id = stepTempT.ToString();
            string robotid = RobotMgr.Instance.rbt[robotname].id;
     
            string source = rbtAnim[robotid].anims[id].source;

            //勿删     删除animation中的动画
            if (animationTemp.GetClip(addedClipName) != null)
            {
                animationTemp.RemoveClip(addedClipName);
                Resources.UnloadUnusedAssets();
            }
            
            if (animationTemp.GetClipCount() <= clipCount)
            {
                AddClips(animationTemp, id);
            }
            animationTemp.Stop();
            string namel = rbtAnim[robotid].anims[id].name;

            if (animationTemp.GetClip(namel))
            {
                animationTemp.Play(namel);
            }

            //1.提示舵机ID
            TSDJID(source,robotid,id);
            
            //2.主控盒插孔编号
            TSZKHBH(source,robotid,id);

            //3.舵机舵盘装配位形状
            DPZPXZ(source, robotid, id);

            //4.零件图,局部图
             LJJUT(robotid, id);

            //5.是否打开局部图显示
            isOpenJBT(robotid,id);

            //6.履带动画
            LDNotice(source, robotid, id);

            //7.打开安装确认动画
            AZNotice(source);

            //8.课程鼓励
            if (IsLessonBuild)
            {
                KCGLNotice(stepTempT);
            }

        }

        string stepTempL = GetNowStep(stepTempT);
        if (inputlabels["NowStep"].text != stepTempL)
        {
            inputlabels["NowStep"].text = stepTempL;
        }
        //重置进度条的值
        tempslider.value = stepTempT * 1.0f / clipCount;
        tempValue = tempslider.value;
    }

    /// <summary>
    /// 1.提示舵机ID
    /// </summary>
    /// <param name="robotid"></param>
    /// <param name="id"></param>
    public void TSDJID(string source, string robotid, string id)
    {
        if (source == "djid")
        {
            DJNotice.gameObject.SetActive(true);
            DJNotice.Find("notice/Sprite").gameObject.SetActive(true);
            DJNotice.Find("notice/others").gameObject.SetActive(false);
            DJNotice.GetComponent<TweenPosition>().PlayForward();
            DJNotice.GetComponent<TweenAlpha>().PlayForward();

            DJNotice.Find("notice/Sprite/SpriteC1").gameObject.SetActive(true);
            DJNotice.Find("notice/Sprite/SpriteC2").gameObject.SetActive(false);
            
            string djid = rbtAnim[robotid].anims[id].djid;

            int idtemp = int.Parse(djid);

            DJNotice.Find("notice/Sprite/SpriteB/servoID").GetComponent<UILabel>().text = ReturnID(idtemp);
            inputlabels["djnotice"].text = string.Format(nots["not1"], ReturnID(idtemp));

            inputlabels["djnotice2"].text = nots["not2"];
        }

        if (source == "sensorID")
        {
            PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "sensor");
            DJNotice.gameObject.SetActive(true);
            DJNotice.Find("notice/Sprite").gameObject.SetActive(false);
            DJNotice.Find("notice/others").gameObject.SetActive(true);
            DJNotice.GetComponent<TweenPosition>().PlayForward();
            DJNotice.GetComponent<TweenAlpha>().PlayForward();
            Debug.Log("id:" + id + ";dsfse:" + rbtAnim[robotid].anims[id].sensorID);
            string sensorIDTemp = rbtAnim[robotid].anims[id].sensorID;
            string[] sensorDataTemp = sensorIDTemp.Split(';');
            string sensorName = LauguageTool.GetIns().GetText(sensorDataTemp[0]);
            DJNotice.Find("notice/others/otnotice").GetComponent<UILabel>().text = string.Format(nots["not25"], sensorName, sensorDataTemp[1]);

            DJNotice.Find("notice/others/otnotice2").GetComponent<UILabel>().text = string.Format(nots["not26"], sensorName);
        } 
    }

    /// <summary>
    /// 2.主控盒插孔编号
    /// </summary>
    /// <param name="robotid"></param>
    /// <param name="id"></param>
    public void TSZKHBH(string source,string robotid, string id)
    {
        if (lineIndex != null)
            {
                if (zkhNums.ContainsKey(lineIndex))
                {
                    zkhNums[lineIndex].GetComponent<Renderer>().material.color = Color.white;
                    //Debug.Log("test2:" + lineIndex);
                    Destroy(uvTrans);
                    Destroy(saTrans);
                }
            }

            lineIndex = null;
            if (source == "line")
            {
                DJNotice.gameObject.SetActive(true);
                DJNotice.GetComponent<TweenPosition>().PlayForward();
                DJNotice.GetComponent<TweenAlpha>().PlayForward();

                DJNotice.Find("notice/Sprite").gameObject.SetActive(false);
                DJNotice.Find("notice/others").gameObject.SetActive(true);

                lineIndex = rbtAnim[robotid].anims[id].line;

                if (zkhNums.ContainsKey(lineIndex))
                {
                    //Debug.Log("dfsfdsf122323:"+lineIndex);
                    uvTrans = zkhNums[lineIndex].AddComponent<UV>();
                    saTrans = zkhNums[lineIndex].AddComponent<Animation>() as Animation;

                    //saTrans.clip = shaderAnim;
                    saTrans.AddClip(shaderAnim, "test", 0, (int)(shaderAnim.length * 60));

                    saTrans.Play("test");
                }

                DJNotice.Find("notice/others/otnotice").GetComponent<UILabel>().text = nots["not13"];
                DJNotice.Find("notice/others/otnotice2").GetComponent<UILabel>().text = nots["not3"];
            }
            else if (source == "lineNoNotice")
            {
                DJNotice.gameObject.SetActive(false);
                lineIndex = rbtAnim[robotid].anims[id].line;

                if (zkhNums.ContainsKey(lineIndex))
                {
                    //Debug.Log("dfsfdsf122323:"+lineIndex);
                    uvTrans = zkhNums[lineIndex].AddComponent<UV>();
                    saTrans = zkhNums[lineIndex].AddComponent<Animation>() as Animation;

                    //saTrans.clip = shaderAnim;
                    saTrans.AddClip(shaderAnim, "test", 0, (int)(shaderAnim.length * 60));

                    saTrans.Play("test");
                }
            }
    }

    /// <summary>
    /// 3.舵机舵盘装配位形状
    /// </summary>
    /// <param name="source"></param>
    /// <param name="robotid"></param>
    /// <param name="id"></param>
    public void DPZPXZ(string source, string robotid, string id)
    {
        if (djname != "" && djname != null)
        {
            Destroy(djposanim);
            djIntGO[djname].trans[pos].GetComponent<Renderer>().material.mainTexture = djposTexture[pos];
            djIntGO[djname].trans[pos].GetComponent<Renderer>().material.mainTextureScale = new Vector2(1, 1);
            djIntGO[djname].trans[pos].GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0));

            djname = "";
            djposanim = null;
            pos = "";
        }

        djname = null;
        if (source == "shape")
        {

            DJNotice.gameObject.SetActive(true);
			
			DJNotice.Find("notice/Sprite").gameObject.SetActive(true);
            DJNotice.Find("notice/others").gameObject.SetActive(false);
			
            DJNotice.GetComponent<TweenPosition>().PlayForward();
            DJNotice.GetComponent<TweenAlpha>().PlayForward();
            DJNotice.Find("notice/Sprite/SpriteC1").gameObject.SetActive(false);
            DJNotice.Find("notice/Sprite/SpriteC2").gameObject.SetActive(true);

            string shape = rbtAnim[robotid].anims[id].shape;

            djname = RobotMgr.FindDJName(shape, ":");
            pos = RobotMgr.FindDJShape(shape, ":");

            int djid = RobotMgr.Instance.rbt[robotname].gos[djname].djID;
            DJNotice.Find("notice/Sprite/SpriteB/servoID").GetComponent<UILabel>().text = ReturnID(djid);
            inputlabels["djnotice"].text = string.Format(nots["not9"], djid);

            inputlabels["djnotice2"].text = nots["not3"];

            string dtpos = "Sec-" + pos;
            djIntGO[djname].trans[pos].GetComponent<Renderer>().material.mainTexture = djposTexture[dtpos];
            djposanim = djIntGO[djname].trans[pos].gameObject.AddComponent<TextureAnim>();

        }
    }

    /// <summary>
    /// 4.零件图,局部图
    /// </summary>
    /// <param name="robotid"></param>
    /// <param name="id"></param>
    public void LJJUT(string robotid, string id)
    {
        
        if (noParts == true)
        {

            goLabel.text = null;
            goicon.spriteName = "Clean";
        }
        else
        {

            string partsT = rbtAnim[robotid].anims[id].parts;
            string namT = RobotMgr.PartPicType(partsT);
            goLabel.text = namT;


            //Debug.Log("yes:" + partsT + ";goTexture:" + goTexture.name);
            if (outPics.ContainsKey(partsT) == false)
            {
                if (textureGO.activeInHierarchy == true)
                {
                    textureGO.SetActive(false);
                    iconGO.SetActive(true);
                }

                //Debug.Log("shou inner parts pic:" + rbtAnim[robotid].anims[id].parts);
                

                if (innerSprites.Contains(partsT))
                {
                    goicon.spriteName = partsT;
                    goicon.color = new Color(1,1,1,1);
                }
                else 
                {
                    if(partsT != null && partsT != "")
                    {
                        string[] picStr = partsT.Split('-');
                        Dictionary<string, string> colorType = SingletonObject<PublicTools>.GetInst().PartsPicColor();
                        string picNameTemp = picStr[0] + "-WHT";
                        goicon.spriteName = picNameTemp;
                        Color colorTemp = SingletonObject<PublicTools>.GetInst().StringToColor1(colorType[picStr[1]]);
                        goicon.color = colorTemp;
                    }
                    else
                    {
                        goicon.spriteName = "";
                    }
                }
            }
            else
            {
               // Debug.Log("show out partpic:" + rbtAnim[robotid].anims[id].parts);
                if (textureGO.activeInHierarchy == false)
                {
                    textureGO.SetActive(true);
                    iconGO.SetActive(false);
                }
                goTexture.mainTexture = outPics[partsT];
            }

            if (noJuBu == false)
            {
                //Debug.Log("step01");
                string jubuT = rbtAnim[robotid].anims[id].pic;
                if (jubuPics.ContainsKey(jubuT))
                {
                    //Debug.Log("step02");
                    jubuTexture.mainTexture = jubuPics[jubuT];
                }
            }
        }
    }

    /// <summary>
    /// 5.是否打开局部图显示
    /// </summary>
    /// <param name="robotid"></param>
    /// <param name="id"></param>
    public void isOpenJBT(string robotid, string id)
    {
        string fstPic = rbtAnim[robotid].anims[id].firstPic;
        if (fstPic == "true")
        {
            //Debug.Log("open");
            juBuStep.GetComponent<TweenAlpha>().PlayReverse();

            //Debug.Log("pstepshow=false");
            jubuIcon.spriteName = "btn_press@2x";
            jubustepshow = false;
        }
    }


    /// <summary>
    /// 6.履带提示
    /// </summary>
    /// <param name="robotid"></param>
    /// <param name="id"></param>
    public void LDNotice(string source, string robotid, string id)
    {
        if (source == "lvdai")
        {
            DJNotice.gameObject.SetActive(true);
			DJNotice.Find("notice/Sprite").gameObject.SetActive(false);
            DJNotice.Find("notice/others").gameObject.SetActive(true);
            DJNotice.GetComponent<TweenPosition>().PlayForward();
            DJNotice.GetComponent<TweenAlpha>().PlayForward();
            string lvdaiNumt = rbtAnim[robotid].anims[id].lvdaiNum;
            int lvdaiNum = int.Parse(lvdaiNumt);
			
			
			DJNotice.Find("notice/others/otnotice").GetComponent<UILabel>().text = string.Format(nots["not24"], lvdaiNum,"P87");
            DJNotice.Find("notice/others/otnotice2").GetComponent<UILabel>().text = "";
           
        }
    }

    /// <summary>
    /// 7.打开安装确认动画
    /// </summary>
    /// <param name="source"></param>
    public void AZNotice(string source)
    {
        if (source == "pa")
        {
            FindChildGO(uicenter, "AnZhuangAnim").SetActive(true);
        }
        if (source == "pa2")
        {
            FindChildGO(uicenter, "AnZhuangAnim2").SetActive(true);
        }
        if(source == "dry_cell")
        {
            FindChildGO(uicenter, "DryCellNotice").SetActive(true);
        }
    }

    /// <summary>
    ///  //8.课程鼓励
    /// </summary>
    /// <param name="idTemp"></param>
    public void KCGLNotice(int idTemp)
    {
        if(ecgnotice.ContainsKey(idTemp))
        {
            ecggo.SetActive(true);
            string nott=LauguageTool.GetIns().GetText(ecgnotice[idTemp]);
            string[] notTemp = nott.Split('\\');
            ecglabel1.text = notTemp[0];
            if(notTemp.Length >1){
                ecglabel2.text = notTemp[1];
            }

        }
        else
        {
            if(ecggo.activeInHierarchy)
            {
                ecggo.SetActive(false);
            }
            
        }
    }

    /// <summary>
    /// 播放动画时，一些基本数据的处理
    /// </summary>
    public void AnimBaseHandle(int stepTempT)
    {
        //关闭打开相应按钮
        if (stepTempT != 0)  
        {

            string id = stepTempT.ToString();
            string robotid = RobotMgr.Instance.rbt[robotname].id;

            string source = rbtAnim[robotid].anims[id].source;

            if (source == "djid" || source == "shape" || source == "line" || source == "pa" || source == "pa2" || source == "lvdai" || source == "sensorID" || source == "dry_cell")
            {
                DisableBtnClick(btns["Forward"]);
                DisableBtnClick(btns["Back"]);
                DisableBtnClick(btns["Return"]);  //createPList
                DisableBtnClick(createPList); 
            }
            else
            {
                EnableBtnClick(btns["Return"], Color.white);
                EnableBtnClick(createPList, Color.white); 
            }
        }

        //刚进入搭建场景时，对相机的处理
        if (isFirstStart == false)
        {
            CamRotateAroundCircle._instance.ResetCam(oriGO);
        }
        else
        {
            isFirstStart = false;
        }

        //恢复局部图按钮的状态
        if (juBuStep.GetComponent<UISprite>().color.a != 0)
        {
            juBuStep.GetComponent<UISprite>().color = new Color(1.0f, 1.0f, 1.0f, 0);
            jubustepshow = true;
            jubuIcon.spriteName = "btn@2x";
        }

        //当步数为0时，处理相应控件的状态
        if (start == true)
        {
            start = false;
            isFinished = false;
            oriGO.SetActive(true);

            EnableBtnClick(btns["Refresh"], Color.white);
            EnableBtnClick(partsControl, Color.white);

            FindChildGO(partsControl, "icon").GetComponent<TweenRotation>().PlayReverse();
            partsStep.GetComponent<UISprite>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        //删除舵机pos动画
        if (djposanim != null)
        {
            Destroy(djposanim);
        }

        //单步零件提示控件的处理
        if (pstepshow)
        {
            partsStep.GetComponent<UISprite>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        //舵机提示处理
        if (DJNotice.gameObject.activeInHierarchy)
        {
            DJNotice.GetComponent<TweenPosition>().PlayReverse();
            DJNotice.GetComponent<TweenAlpha>().PlayReverse();
            DJNotice.gameObject.SetActive(false);
        }

        if (FindChildGO(uicenter, "AnZhuangAnim").activeInHierarchy)
        {
            CloseAnZhuangAnim(oriGO);
        }
        if (FindChildGO(uicenter, "AnZhuangAnim2").activeInHierarchy)
        {
            CloseAnZhuangAnim2(oriGO);
        }
        if (FindChildGO(uicenter, "DryCellNotice").activeInHierarchy)
        {
            FindChildGO(uicenter, "DryCellNotice").SetActive(false);
            ShowBtns();
        }
    }



    /// <summary>
    /// 前进，后退按钮是否可用--进度条处理时
    /// </summary>
    public void JustButtonsEnabled(int stepTempT)
    {
        if (stepTempT < clipCount && stepTempT > 0)//搭建过程中
        {
            EnableBtnClick(btns["Back"], Color.white);
            EnableBtnClick(btns["Forward"], Color.white);

            labels["TestLabel"].text = " ";

            if (fsNotice.activeInHierarchy == true)
            {
                fsNotice.GetComponent<TweenAlpha>().PlayReverse();
                fsNotice.GetComponent<TweenPosition>().PlayReverse();
                fsNotice.SetActive(false);
            }
        }
        else if (stepTempT >= clipCount)   //搭建完成
        {
            PlatformMgr.Instance.DurationEventEnd(MobClickEventID.app_building_complete);
            //TopologyBaseMsg.ShowInfoMsg();
            EnableBtnClick(btns["Back"], Color.white);
            DisableBtnClick(btns["Forward"]);

            partsStep.GetComponent<UISprite>().color = new Color(1.0f, 1.0f, 1.0f, 0);

            fsNotice.SetActive(true);

            

            fsNotice.GetComponent<TweenAlpha>().PlayForward();
            fsNotice.GetComponent<TweenPosition>().PlayForward();

            //完成时，提示搭建模型的名称
            string nameNoTypeT;
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                nameNoTypeT = RobotManager.GetInst().GetCreateRobot().ShowName;
            }
            else
            {
                nameNoTypeT = RobotManager.GetInst().GetCurrentRobot().ShowName;
            }
            inputlabels["fsName"].text = string.Format(nots["not5"], nameNoTypeT);

            isFinished = false;
        }
        else if (stepTempT == 0)
        {
            EnableBtnClick(btns["Forward"], Color.white);
            DisableBtnClick(btns["Back"]);

            labels["TestLabel"].text = "";

        }
    }


    int tempn1 = 1;
    int tempn2 = 1;
    /// <summary>
    /// 下一步
    /// </summary>
    /// <param name="go"></param>
    public void CreateForward(GameObject go)
    {
        if (null != go)
        {
            PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P6_3);
        }
        if (animationTemp.isPlaying == false)
        {
            isFinished = false;
        }

        btns["Forward"].transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        btns["Forward"].GetComponent<TweenScale>().PlayReverse();
        if (stepCount < clipCount)
        {
            if (mRecordBuildDurationFlag && stepCount == 1)
            {
                PlatformMgr.Instance.DurationEventStart(MobClickEventID.app_building_complete);
                mRecordBuildDurationFlag = false;
            }
            stepCount++;

            AnimNormal(stepCount);
        }
    }


    /// <summary>
    /// 上一步
    /// </summary>
    /// <param name="go"></param>
    public void CreateBack(GameObject go)
    {
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P6_4);
        btns["Back"].GetComponent<TweenScale>().PlayForward();
        //影藏物体
        if (animationTemp.isPlaying == false)
        {
            isFinished = false;
        }
        if (stepCount == clipCount)
        {
            EnableBtnClick(btns["Forward"], Color.white);

        }
   

        if (stepCount > 0)
        {
            stepCount--;

            AnimNormal(stepCount);

            if (stepCount == 0)
            {
                LastOneNew();
            }


        }
    }

   
    /// <summary>
    /// 退到最后一步时的处理
    /// </summary>
    public void LastOneNew()
    {
        start = true;

        stepCount = 0;

        tempslider.value = 0;

        inputlabels["NowStep"].text = "0";
    }


    #region 自动播放
    bool autoplaynew = false;
    /// <summary>
    /// 自动播放
    /// </summary>
    /// <param name="go"></param>
    public void AutoPlay(GameObject go)
    {
        autoplaynew = !autoplaynew;
        if (autoplaynew == true && isFinished == false)
        {
            isFinished = true;

            if (start == true)
            {
                start = false;
                isFinished = false;

                EnableBtnClick(btns["Back"], Color.white);

                oriGO.SetActive(true);
            }

            InvokeRepeating("DelayPlayNew", 0.5f, 1.0f);

        }

        if (autoplaynew == false)
        {

            CloseAutoPlayNew();
        }

    }

    
    /// <summary>
    /// 自动播放的具体操作
    /// </summary>
    void DelayPlayNew()
    {

        if (stepCount < clipCount)
        {
            stepCount++;
            AnimNormal(stepCount);

            if (stepCount == 0)
            {
                LastOneNew();
            }

        }
        else
        {
            tempslider.value = 1;
        }

        if (stepCount > clipCount)
        {
            CancelInvoke();
            stepCount = goName.Count;

            if(IsLessonBuild )
            {
                labels["TestLabel"].text = nots["lessonFinish"];
            }
            else
            {
                labels["TestLabel"].text = nots["not4"];
            }
            

            isFinished = false;
        }
    }

    
    /// <summary>
    /// 关闭自动播放
    /// </summary>
    public void CloseAutoPlayNew()
    {
        animationTemp.Stop();
        CancelInvoke();
        isFinished = false;
    }
    #endregion
    #endregion

    #region common
    /// <summary>
    /// 从子物体中查找label物体
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public static UILabel FindLabel(GameObject button)
    {
        UILabel labelTemp = null;
        UIEventListener uButton = button.GetComponent<UIEventListener>();
        if (uButton == null)
        {
            labelTemp =button.GetComponent<UILabel>();
        }
        else
        {
            foreach (Transform m in button.transform.GetComponentInChildren<Transform>())
            {
                if (m.name == "Label")
                {
                    labelTemp = m.GetComponent<UILabel>();
                }
            }
        }
        return labelTemp;
    }

   
    /// <summary>
    /// 找到物体的UILabel组件
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    public UILabel FindLabelComponent(Transform m)
    {
        UILabel labelTemp = null;
        labelTemp = m.GetComponent<UILabel>();
        return labelTemp;
    }

    
    /// <summary>
    /// 输出ID显示字符
    /// </summary>
    /// <param name="idNum"></param>
    /// <returns></returns>
    public string ReturnID(int idTemp)
    {
        string idStr = "";
        if (idTemp <= 9)
        {
            idStr = "ID-0" + idTemp;
        }else
        {
            idStr = "ID-" + idTemp;
        }

        return idStr;
    }
    #endregion


}

