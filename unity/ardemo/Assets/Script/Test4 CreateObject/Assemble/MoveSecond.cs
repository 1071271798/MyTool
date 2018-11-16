//#define  Close_Robot
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Game.Event;
using Game.Scene;
using Game.Resource;
using System.IO;
using Game.Platform;

public class MoveSecond : MonoBehaviour
{
    public static MoveSecond Instance;

    public const int specNum = 20; //生成模型时每次小循环生成的模型的数量


    #region 变量
    public GameObject oriGO;

    public GameObject arrow;
    private Vector3 objPos;
    private Vector3 objAngle;
    private Vector3 objScale;

    List<string> typetemp;
    public Dictionary<string, GameObject> prefabgos = new Dictionary<string, GameObject>();
    // public Dictionary<string, int> gonamnum = new Dictionary<string, int>();
    private int nameNum = 1;
    private GameObject tempgo;

    public Dictionary<string, DPBOX> alldpbox = new Dictionary<string, DPBOX>();//舵盘和舵机主体旋转时需要的父物体模型--只用舵机才使用该属性
    Dictionary<string, DJClass> djIntGO = new Dictionary<string, DJClass>();   //舵机的名称，舵机类<内部模型名称，模型材质>

    //List<GameObject> lines = new List<GameObject>();  //所有线


    Dictionary<string, Vector3> goPosAll = new Dictionary<string, Vector3>();  //生成模型的所有坐标
    Dictionary<string, Vector3> goAngleAll = new Dictionary<string, Vector3>();  //生成模型的所有角度
    Dictionary<string, Vector3> goScaleAll = new Dictionary<string, Vector3>();  //生成模型的所有尺寸

    private string goPos;   //物体的位置
    private string goAngle;  //物体的角度
    private string goScale;  //物体的角度
    private GameObject newt;
    private string newtName;

    private List<string> goName = new List<string>();  //所有物体的名字

    public GameObject mainCamera;
    public GameObject camera1;
    private GameObject arrowCamera;    //显示旋转箭头的相机
    private GameObject arrowCameraPreb;

    public GameObject duojiGO;     //选中的物体为duoji物体
    public GameObject dpTemp;    //duoji上duopan的替代物，用于获取选择后dp的角度
    public GameObject firstobj;

    public List<GameObject> hideGOs=new List<GameObject>();//生成时需要隐藏的模型

    public Vector3 tempangle;
    public static string robotName;

    //已经生成的物体
    public List<GameObject> AddedAllGOs = new List<GameObject>();
    public Dictionary<string,GameObject> AddedAllGOsDic = new Dictionary<string,GameObject>();

    Dictionary<string, int> mDjInitRota = null;
    public List<GameObject> alldj = new List<GameObject>();  //所有的舵机物体-实物

    Dictionary<string, int> mDjEulerDict = new Dictionary<string, int>();

    Dictionary<string, float> mDjRotaDict = new Dictionary<string, float>();

    Dictionary<string, SelectDuoJiData> mSelectDjData = new Dictionary<string, SelectDuoJiData>();
    #endregion

    Dictionary<string, Texture> djIDTexture = new Dictionary<string, Texture>();

    public Vector3 oriPos;  //oriGO的默认坐标
    public Vector3 oriAngle;//oriGO的默认角度
    public Vector3 oriScale;//oriGO的默认尺寸

    public UILabel testlabel;

    GameObject mainUIRoot;//主场景
    GameObject loadingSprite; //加载动画

    float t1;
    List<string> storeParts = new List<string>();//内置的零件
    List<string> outParts = new List<string>();  //需要从后台下载加载的零件
    List<string> innerParts = new List<string>();  //模型需要的内置的零件
	
	Dictionary<string, string> wireJionts = new Dictionary<string, string>();     //<线的名称，joint后缀>
    static Dictionary<string, DJLianDongs> djld = new Dictionary<string, DJLianDongs>();//联动舵机
    Dictionary<string, DJLianDongs> gosld = new Dictionary<string, DJLianDongs>();//舵机引起的模型联动
    Dictionary<string, LianDongGOs> ldgos = new Dictionary<string, LianDongGOs>();

    public GameObject connect;   //mainScene中的connect按钮
    public GameObject Guanfang;//mainScene中的Guanfang按钮

   public static Material matT;//默认使用的材质
   private string adsfilename = null;//广告动画名称

    bool createStart = true;
    int gonameCount;//所有零件的个数
    int outStartN = 0;
    int officalGoN = 0;   //官方模型GO开始的数字
    void Awake()
    {
        Instance = this;
        oriScale = new Vector3(1, 1, 1);
        robotName = RobotMgr.Instance.rbtnametempt;
        t1 = Time.realtimeSinceStartup;

        TextureMgr textM = new TextureMgr();
        djIDTexture=textM.FindIDPic();  //查找舵机ID贴图
        matT = Resources.Load("Prefab/Test4/Materials/AO") as Material;
        // Debug.Log("matT:"+matT.name);
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null == robot)
        {
            return;
        }
        if ("playerdata" == RecordContactInfo.Instance.openType)
        {
           
        }
        else if ("default" == RecordContactInfo.Instance.openType)
        {
            string nameNoType = RobotMgr.NameNoType(robotName);
            wireJionts = RecordContactInfo.Instance.FindWiresData(nameNoType);
            MoveStart2nd(robotName);
            arrow = Resources.Load("Prefab/Test4/arrow") as GameObject;
            arrowCameraPreb = Resources.Load("Prefab/Test4/ArrowCamera") as GameObject;
            if (SceneMgrTest.Instance.LastScene == SceneType.StartScene)
            {
                Init();
            }
            else
            {
                CreateTheGO(robotName);
            }
        }
        
       
    }

    void Start()
    {
        adsfilename = RecordContactInfo.Instance.FindAds();
        string path = ReturnAnimPath("file");        
        if (File.Exists(path) == true&&adsfilename!=null)
        {
            PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "playAnim:" );
            playAnim();
        }

        RobotMgr.Instance.colliderCount = 0;
        if ("playerdata" == RecordContactInfo.Instance.openType)
        {
            PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "don't need 3D model");
            StartCoroutine(Test());
        }
    }

    #region 加载广告动画

    //  "file:///"只有WWW需要加
    public string ReturnAnimPath(string prePath)
    {
        string nameNoType = RobotMgr.NameNoType(robotName);

        string filepathPre = ResourcesEx.persistentDataPath + "/default/" + nameNoType + "/prebanim";//"/parts";  
        string path1= null;

        if (prePath == "load")
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {

                path1 = "file:///" + filepathPre+"/editor/" + adsfilename + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                path1 = "file:///" + filepathPre + "/ios/" + adsfilename + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                path1 = "file:///" + filepathPre + "/android/" + adsfilename + ".assetbundle";

            }
        }
        else
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path1 = filepathPre + "/editor/" + adsfilename + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                path1 = filepathPre + "/ios/" + adsfilename + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                path1 = filepathPre + "/android/" + adsfilename + ".assetbundle";

            }
        }

        return path1;
        
    }


    public void playAnim()
    {
        string path1 = ReturnAnimPath("load");
        StartCoroutine(GetAnimAssets(path1));
    }

    /// <summary>
    /// 动画文件
    /// </summary>
    /// <param name="tem">零件预设名称</param>
    /// <param name="path">加载路径</param>
    /// <returns></returns>
    /// 
    WWW bundleAnim=null;
    IEnumerator GetAnimAssets(string path)
    {

        bundleAnim = new WWW(path);
        yield return bundleAnim;
        try 
        { 
            if (bundleAnim.isDone==true)
            {
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "add adsfilename:"+adsfilename);
                UnityEngine.Object t = bundleAnim.assetBundle.LoadAsset(adsfilename);

                GameObject tempgot = t as GameObject;
        
                GameObject prebgo = Instantiate(tempgot, Vector3.zero, Quaternion.identity) as GameObject;
                prebgo.transform.parent = GameObject.Find("MainUIRoot_new").transform;
                prebgo.transform.localScale = new Vector3(1, 1, 1);

                foreach(Transform animchild in prebgo.transform.GetComponentsInChildren<Transform>())
                {
                    if(animchild.name=="close")
                    {
                       UIEventListener.Get(animchild.gameObject).onClick += CloseAdvertAnim;
                       GameObject camTemp = GameObject.Find("MainUIRoot_new/Camera");
                       UISprite animTemp=animchild.GetComponent<UISprite>();

                        if(camTemp!=null&&animTemp!=null)
                        {
                            animTemp.leftAnchor.Set(camTemp.transform,0,39);
                           animTemp.bottomAnchor.Set(camTemp.transform, 1, -126);
                           animTemp.rightAnchor.Set(camTemp.transform, 0, 129);
                           animTemp.topAnchor.Set(camTemp.transform, 1, -36);
                        }
                       
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());

        }
       
    }

    public void CloseAdvertAnim(GameObject go)
    {
        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "clear adsfilename assets:" + adsfilename);
        Destroy(go.transform.parent.gameObject);

        bundleAnim.assetBundle.Unload(false);

        string nameNoType = RobotMgr.NameNoType(robotName);
        string path1 = ReturnAnimPath("file");
        File.Delete(path1);
        
    }
#endregion

    void OnEnable()
    {
#if !Close_Robot
        EventMgr.Inst.Regist(EventID.Ctrl_Robot_Action, CtrlAction);
        EventMgr.Inst.Regist(EventID.Read_Start_Rota_Ack, ReadStartRotaAck);
        EventMgr.Inst.Regist(EventID.Adjust_Angle_For_UI, AdjustAngleForUI);
        EventMgr.Inst.Regist(EventID.Read_Back_Msg_Ack_Success, OnReadBackAck);
#endif
    }
    void OnDisable()
    {
#if !Close_Robot
        EventMgr.Inst.UnRegist(EventID.Ctrl_Robot_Action, CtrlAction);
        EventMgr.Inst.UnRegist(EventID.Read_Start_Rota_Ack, ReadStartRotaAck);
        EventMgr.Inst.UnRegist(EventID.Adjust_Angle_For_UI, AdjustAngleForUI);
        EventMgr.Inst.UnRegist(EventID.Read_Back_Msg_Ack_Success, OnReadBackAck);
#endif
    }
    public void DestroyRobot()
    {
        if (null != oriGO)
        {
            //Destroy(oriGO);
        }
    }

    public string path;
    /// <summary>
    /// 递归加载内置在app中的零件预设
    /// </summary>
    /// <param name="outN">已添加的零件的数量</param>
    public void FindOriComponent(int outN)
    {
        if (outN < innerParts.Count)
        {
            if (RobotMgr.Instance.prefabgos.ContainsKey(innerParts[outN]) == false)
            {
                GetInnerParts(innerParts[outN], 0.001f, outN);
                //StartCoroutine(GetInnerParts(innerParts[outN], 0.001f, outN));
            }
            else
            {
                ++outN;
                FindOriComponent(outN);
            }
        }
    }

    /// <summary>
    /// 获取内置parts
    /// </summary>
    /// <param name="tem">预设名称</param>
    /// <param name="t3">加载间隔时间</param>
    /// <param name="num">已加载的数量</param>
    /// <returns></returns>
    void GetInnerParts(string tem, float t3, int num)
    {

        //yield return new WaitForSeconds(t3);
        tempgo = Resources.Load("Prefab/Test4/GOPrefabs/" + tem) as GameObject;
        //添加生成的预设
        if (RobotMgr.Instance.prefabgos.ContainsKey(tem) == false)
        {
            RobotMgr.Instance.prefabgos.Add(tem, tempgo);
        }

        if (RobotMgr.Instance.prefabgos.Count == innerParts.Count)
        {
            if (outParts.Count == 0) 
            {
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "don't neet AddOutParts");
                string robotType = RobotMgr.DataType(robotName);
                prefabgos = RobotMgr.Instance.prefabgos;
                if (robotType == "default")
                {
                    //SingletonObject<TimeStatisticsTool>.GetInst().EventFinished("load parts");
                    //当内部零件加载完毕并且无外部零件时，开始生成模型
                    CreateTheGO(robotName);
                }
            }
            else
            {
                //加载从外部零件
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "need AddOutParts");
                AddOutParts();
            }

        }

        num++;
        FindOriComponent(num);
    }
    
    /// <summary>
    /// 加载外部parts
    /// </summary>
    public void AddOutParts()
    {

        foreach (string temp in outParts)
        {
            if (RobotMgr.Instance.prefabgos.ContainsKey(temp) == false)
            {

                string path1 = "";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {

                    path1 = "file:///" + ResourcesEx.persistentDataPath + "/parts/editor/" + temp + ".assetbundle";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    path1 = "file:///" + ResourcesEx.persistentDataPath + "/parts/ios/" + temp + ".assetbundle";
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    path1 = "file:///" + ResourcesEx.persistentDataPath + "/parts/ios/" + temp + ".assetbundle";
                }

                else if (Application.platform == RuntimePlatform.Android)
                {
                    path1 = "file:///" + ResourcesEx.persistentDataPath + "/parts/android/" + temp + ".assetbundle";

                }
                try
                {
                    StartCoroutine(GetOutParts(temp, path1));
                }
                catch (System.Exception ex)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "生成模型失败- error = " + ex.ToString());
                }
                
            }
        }
    }
 
    /// <summary>
    /// 加载外部parts具体操作
    /// </summary>
    /// <param name="tem">零件预设名称</param>
    /// <param name="path">加载路径</param>
    /// <returns></returns>
    IEnumerator GetOutParts(string tem, string path)
    {

        WWW bundle1 = new WWW(path);
        yield return bundle1;
        if (bundle1.error == null)
        {
            try
            {
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "add outparts assets:" + tem);
                UnityEngine.Object t = bundle1.assetBundle.mainAsset;

                tempgo = t as GameObject;

                if (RobotMgr.Instance.prefabgos.ContainsKey(tem) == false)
                {
                    RobotMgr.Instance.prefabgos.Add(tem, tempgo);
                }
                bundle1.assetBundle.Unload(false);
            }
            catch (System.Exception ex)
            {
                if (RobotMgr.Instance.prefabgos.ContainsKey(tem) == false)
                {
                    RobotMgr.Instance.prefabgos.Add(tem, null);
                }
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }

            
        }
        else
        {
            if (RobotMgr.Instance.prefabgos.ContainsKey(tem) == false)
            {
                RobotMgr.Instance.prefabgos.Add(tem, null);
            }
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + bundle1.error);
        }
        //当零件预设加载完毕时，开始生成模型
        if (RobotMgr.Instance.prefabgos.Count == typetemp.Count)
        {
            string robotType = RobotMgr.DataType(robotName);
            prefabgos = RobotMgr.Instance.prefabgos;
            if (robotType == "default")
            {
                CreateTheGO(robotName);
            }
        }
    }

    /// <summary>
    /// 获取模型需要加载的零件预设的名称列表
    /// (innerParts:内置在app中的零件列表；outParts：放着在外部的零件列表)
    /// </summary>
    void Init()
    {

        typetemp = RobotMgr.Instance.FindAllGOTypes(robotName);

        storeParts = PartsDataRead.Instance.FindPartsType();

        foreach (string temp in typetemp)
        {
            if (storeParts.Contains(temp) == false && outParts.Contains(temp) == false)
            {
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "add outpartspic:" + temp);
                outParts.Add(temp);
                
            }

            else if (storeParts.Contains(temp) == true && innerParts.Contains(temp) == false)
            {
                //Debug.Log("add inner:"+temp);
                //PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "add innerpartspic:" + temp);
                innerParts.Add(temp);
                
            }
        }
        //SingletonObject<TimeStatisticsTool>.GetInst().EventStart("load parts");
        FindOriComponent(outStartN);


    }
    /// <summary>
    /// 获取舵机的初始数据
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, int> GetDjInitData()
    {
        try
        {
            if (null != mDjInitRota)
            {
                Dictionary<int, int> data = new Dictionary<int, int>();
                foreach (KeyValuePair<string, int> kvp in mDjInitRota)
                {
                    int id = RobotMgr.Instance.FinddjIDBydjNam(robotName, kvp.Key);// PublicFunction.GetDuoJiId(kvp.Key);
                    data[id] = (int)RobotMgr.Instance.rbt[robotName].gos[kvp.Key].oriDJAngleX; //PublicFunction.DuoJi_Start_Rota;
                }
                return data;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return null;
    }

    /// <summary>
    /// 添加相机脚本CamRotateAroundCircle
    /// </summary>
    public void AddCamRAC()
    {
        if (transform.GetComponent<CamRotateAroundCircle>() != null)
        {
            mainCamera = transform.gameObject;
        } else
        {
            mainCamera = GameObject.Find("MainCamera");

        }
        if (null != mainCamera)
        {
            mainCamera.transform.position = new Vector3(-3.0f, 1.0f, -0.6799996f);
            Vector3 rotatTemp = new Vector3(13.0f, 99.0f, 0);
            Quaternion tempquat = Quaternion.Euler(rotatTemp);
            mainCamera.transform.rotation = tempquat;
            Camera cam = mainCamera.GetComponent<Camera>();
            cam.farClipPlane = 2000;
            CamRotateAroundCircle rotaCricle = mainCamera.GetComponent<CamRotateAroundCircle>();
            if (null == rotaCricle)
            {
                rotaCricle = mainCamera.AddComponent<CamRotateAroundCircle>();
            }
        }

        bool needProduce = false;
        if (null != mainCamera)
        {
            foreach (Transform child in mainCamera.GetComponentInChildren<Transform>())
            {
                if (child.name == "Camera")
                {
                    needProduce = true;
                }
            }
        }
        if (needProduce == false)
        {
            arrowCamera = UnityEngine.GameObject.Instantiate(arrowCameraPreb, mainCamera.transform.position, Quaternion.identity) as GameObject;
            arrowCamera.name = "Camera";
            arrowCamera.transform.parent = mainCamera.transform;
            arrowCamera.transform.localEulerAngles = Vector3.zero;
        }
    }

    /// <summary>
    /// 创建机器人模型
    /// </summary>
    /// <param name="robotname">机器人名称</param>
    public void CreateTheGO(string robotname)
    {
        try
        {
            //SingletonObject<TimeStatisticsTool>.GetInst().EventStart("CreateModelGameObject");
            PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "CreateTheGO:");
            AddCamRAC();

            oriGO = GameObject.Find("oriGO");

            string nameNoType = RobotMgr.NameNoType(robotname);
            string[] x = RecordContactInfo.Instance.FindPosModel(nameNoType);
            oriPos = SingletonObject<PublicTools>.GetInst().StringToVector(x[0]);

            oriAngle = SingletonObject<PublicTools>.GetInst().StringToVector(x[1]);
            oriScale = SingletonObject<PublicTools>.GetInst().StringToVector(x[2]);
            
            if (oriGO == null)
            {

                oriGO = new GameObject();
                oriGO.name = "oriGO";
                oriGO.transform.position = oriPos;
                oriGO.transform.localEulerAngles = oriAngle;
                DontDestroyOnLoad(oriGO);
                goName = RobotMgr.Instance.FindAllGOName(robotname);
                gonameCount = goName.Count;

                m = gonameCount / specNum;
                n = gonameCount % specNum;

                if (n > 0)
                {
                    m++;
                }
                prefabgos = RobotMgr.Instance.prefabgos;
                RobotMgr.Instance.ClearBaseData();//清除部分数据
                StartCoroutine(ProduceModel(robotname, officalGoN, 0.001f));
            }
            else
            {
                oriGO.transform.position = oriPos;
                oriGO.transform.localEulerAngles = oriAngle;

                if (loadingSprite != null)
                {
                    loadingSprite.SetActive(false); //关闭加载动画
                }
                CreateModelFinishSec();
            }


        }
        catch (System.Exception ex)
        {
            StartCoroutine(Test());
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        if (RecordContactInfo.Instance.openType == "playerdata")
        {
            oriGO.SetActive(false);    //模型为自定义的模型时，隐藏模型
        }
    }

    //模型生成过程

    List<string> goNameTemp = new List<string>();   //每次小循环中需要生成的模型的名称的数组
    int m = 0;  //需要几次小循环
    int n = 0;  //最后一次小循环生成的模型的数量
    int rowN;    //确定每次小循环的次数  specNum=20
    int nu = 0;  //已生成的模型的数量
    IEnumerator ProduceModel(string robotname, int num, float t)   //List<string> goNameTemp,
    {
        yield return new WaitForSeconds(t);
        //Debug.Log("ProduceModel 01:"+num);
        if (num <= m - 1)
        {
            string robotid = RobotMgr.Instance.rbt[robotname].id;

            if (n != 0)
            {
                if (num < m - 1)
                {
                    rowN = specNum - 1;

                    for (int numT = nu; numT < nu + specNum; numT++)
                    {
                        goNameTemp.Add(goName[numT]);
                    }
                }
                else if (num == m - 1)
                {
                    rowN = n - 1;
                    for (int numT = nu; numT < nu + n; numT++)
                    {
                       
                        goNameTemp.Add(goName[numT]);
                    }
                }
            }
            else
            {
                if (num < m)
                {
                    rowN = specNum - 1;

                    for (int numT = nu; numT < nu + specNum; numT++)
                    {
                        goNameTemp.Add(goName[numT]);
                    }
                }
                else if (num == m)
                {
                    rowN = specNum - 1;
                    for (int numT = nu; numT < nu + n; numT++)
                    {
                        goNameTemp.Add(goName[numT]);
                    }
                }

            }
            //Debug.Log("rowN:"+rowN);
            for (int i = rowN; i >= 0; i--)
            {
                nu++;

                if (goNameTemp[i] != null)
                {
                    string goType = RobotMgr.Instance.rbt[robotname].gos[goNameTemp[i]].goType;

                    RobotMgr.Instance.FindGOPosAngle(robotname, goNameTemp[i], out goPos, out goAngle, out goScale);

                    objPos = StringToVector(goPos);
                    objAngle = StringToVector(goAngle);
                    objScale = StringToVector(goScale);
                    if (goPosAll.ContainsKey(goNameTemp[i]) == false)
                    {
                        RobotMgr.Instance.goPosAll.Add(goNameTemp[i], objPos);
                        RobotMgr.Instance.goAngleAll.Add(goNameTemp[i], objAngle);
                        RobotMgr.Instance.goScaleAll.Add(goNameTemp[i], objScale);
                    }


                    if (prefabgos.ContainsKey(goType))
                    {
                        //Debug.Log("prefab:"+goType);
                        ProduceOfficalGO(prefabgos[goType], objPos, out newt, goNameTemp[i], objAngle, num, objScale);
                    }
                   
                    goNameTemp.RemoveAt(i);

                }
            }

        }
    }



    /// <summary>
    /// 生成官方模型GO
    /// </summary>
    /// <param name="prefabgo"></param>
    /// <param name="oPos"></param>
    /// <param name="newt"></param>
    /// <param name="nameT"></param>
    /// <param name="oAngle"></param>
    /// <param name="num"></param>
    /// <param name="oScale"></param>
    public void ProduceOfficalGO(GameObject prefabgo, Vector3 oPos, out GameObject newt, string nameT, Vector3 oAngle, int num, Vector3 oScale)
    {
        try
        {
            if (null == prefabgo)
            {             
                num++;
                if (num < m && goNameTemp.Count == 1)
                {
                    StartCoroutine(ProduceModel(robotName, num, 0.001f));
                }
                else if (nu == goName.Count)
                {
                    AddedAllGOs = RobotMgr.Instance.AddedAllGOs;
                    AddedAllGOsDic = NormalTools.ListToDic<GameObject>(AddedAllGOs);
                    RobotMgr.Instance.alldj = RobotMgr.Instance.FindAllDJGO(AddedAllGOs);

                    goPosAll = RobotMgr.Instance.goPosAll;
                    goAngleAll = RobotMgr.Instance.goAngleAll;
                    goScaleAll = RobotMgr.Instance.goScaleAll;
                    alldj = RobotMgr.Instance.alldj;

                    CreateModelFinish();
                }
                newt = null;
                return;
            }
            newt = UnityEngine.Object.Instantiate(prefabgo, oPos, Quaternion.identity) as GameObject;

            if (newt != null)
            {
                newt.name = nameT;
                if (oriGO != null)
                {
                    newt.transform.parent = oriGO.transform;
                }

                newt.transform.localPosition = oPos;
                newt.transform.localEulerAngles = oAngle;
                newt.transform.localScale = oScale;

                newtName = nameT;

                #region normal Handle
                //给模型赋颜色
                SettingColorByOne(robotName, newt);

                //隐藏含hidego节点的物体
                if(RobotMgr.Instance.rbt[robotName].gos[nameT].hidego =="true")
                {
                    if (RobotMgr.Instance.hideGOs.Contains(newt) == false)
                    {
                        RobotMgr.Instance.hideGOs.Add(newt);
                    }
                    newt.SetActive(false);
                }
                

                //舵机连接处编号的显示
                if (prefabgo.name == "seivo")
                {
                    int djIDTemp = RobotMgr.Instance.rbt[robotName].gos[nameT].djID;
                    RobotMgr.Instance.ShowID(newt,djIDTemp, djIDTexture);
                }

                //显示传感器ID
                if(RobotMgr.Instance.rbt[robotName].gos[nameT].sensorID!=null)
                {
                    int sensorIDT = int.Parse(RobotMgr.Instance.rbt[robotName].gos[nameT].sensorID);
                    RobotMgr.Instance.ShowID(newt, sensorIDT, djIDTexture);
                }

                //显示马达ID
                if (prefabgo.name == "motor")
                {
                    int motorIDT = RobotMgr.Instance.rbt[robotName].gos[nameT].motorID;
                    RobotMgr.Instance.ShowID(newt, motorIDT, djIDTexture);
                }

                //模型的组模块隐藏
                if (prefabgo.name.Contains("GTemp"))
                {
                    if(RobotMgr.Instance.hideGOs.Contains(newt)==false)
                    {
                        RobotMgr.Instance.hideGOs.Add(newt);
                    }
                    newt.SetActive(false);
                }

                //对模型线的处理
                HandleWireJionts(nameT);

                if (RobotMgr.Instance.AddedAllGOs.Contains(newt) == false)
                {
                    RobotMgr.Instance.AddedAllGOs.Add(newt);
                }
                else
                {
                    Destroy(newt);
                }
                #endregion

            }

            num++;
            //Debug.Log("num:"+num);
            if (num < m && goNameTemp.Count == 1)
            {

                StartCoroutine(ProduceModel(robotName, num, 0.001f));
            }
            else if (nu == goName.Count)
            {
                AddedAllGOs = RobotMgr.Instance.AddedAllGOs;
                AddedAllGOsDic = NormalTools.ListToDic<GameObject>(AddedAllGOs);
                RobotMgr.Instance.alldj = RobotMgr.Instance.FindAllDJGO(AddedAllGOs);
        
                goPosAll=RobotMgr.Instance.goPosAll;
                goAngleAll=RobotMgr.Instance.goAngleAll;
                goScaleAll=RobotMgr.Instance.goScaleAll;
                alldj = RobotMgr.Instance.alldj;
               
                CreateModelFinish();
            }
        }
        catch (System.Exception ex)
        {
            newt = null;
            StartCoroutine(Test());
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 对模型线的处理
    /// </summary>
    /// <param name="nameT"></param>
    public void HandleWireJionts(string nameT)
    {
        if (wireJionts != null && wireJionts.Count > 0)
        {
            if (wireJionts.ContainsKey(nameT))
            {
                string numT = wireJionts[nameT];  //名称需要加的符号"_"/"-"
                string symbolType = RobotMgr.GOSymbolType(nameT);
                List<Transform> joints = new List<Transform>();

                foreach (Transform child in newt.transform.GetComponentsInChildren<Transform>())
                {
                    if (child.name.Contains("joint"))
                    {
                        joints.Add(child);
                    }
                }

                for (int i = joints.Count - 1; i >= 0; i--)
                {
                    joints[i].name += symbolType + numT;
                }
            }

        }
    }

    /// <summary>
    /// 当进入当前场景时，模型已存在，对模型的一些处理
    /// </summary>
    public void CreateModelFinishSec()
    {
        //SingletonObject<TimeStatisticsTool>.GetInst().EventFinished("CreateModelGameObject");
        AddedAllGOs = RobotMgr.Instance.AddedAllGOs;
        AddedAllGOsDic = NormalTools.ListToDic<GameObject>(AddedAllGOs);
        alldpbox = RobotMgr.Instance.alldpbox;
        alldj = RobotMgr.Instance.alldj;
        goPosAll = RobotMgr.Instance.goPosAll;
        goAngleAll = RobotMgr.Instance.goAngleAll;
        goScaleAll = RobotMgr.Instance.goScaleAll;
        oriGO.transform.localScale = oriScale;

        if (alldpbox != null && alldpbox.Count > 0)
        {
            RobotMgr.Instance.ChangeParent(robotName, alldj, AddedAllGOs, alldpbox);
        }

        if (loadingSprite != null)
        {
            loadingSprite.SetActive(false);
            //    GuideViewBase.IsModelLoadOver = true;
        }

       
        djld = RecordContactInfo.Instance.FindLDDJ(robotName);
        ldgos = GetLianDongData.Inst.FindLDGOsData();

        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "has 3D model");
        StartCoroutine(Test());
    }
 

    /// <summary>
    /// 当进入当前场景时，模型不存在，对新生成模型的一些处理
    /// </summary>
    public void CreateModelFinish()
    {
        //SingletonObject<TimeStatisticsTool>.GetInst().EventFinished("CreateModelGameObject");
        oriGO.transform.localScale = oriScale;
       
        #region 模型父子关系
        if (SceneMgr.GetCurrentSceneType() != SceneType.Assemble)
        {

            Vector3 pos = Vector3.zero;
            Transform trans = oriGO.transform;
            PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Robot"));

            //查找舵机位置图标
            RobotMgr.CloseDJPosShow(AddedAllGOs);


            //找到所有舵机的舵盘和主体旋转的父物体
            foreach (GameObject djTemp in alldj)
            {
                GameObject dpgo = null;
                GameObject boxgo = null;

                RobotMgr.Instance.FindDPBOXGO(robotName, djTemp, out dpgo, out boxgo);
  
                if (dpgo != null && RobotMgr.Instance.alldpbox.ContainsKey(djTemp.name)==false)
                {
                    DPBOX dpbox = new DPBOX();
                    dpbox.dp = dpgo;
                    dpbox.dplocalPos = dpgo.transform.localPosition;
                    dpbox.dplocalAngle = dpgo.transform.localEulerAngles;

                    dpbox.box = boxgo;
                    dpbox.boxlocalPos = boxgo.transform.localPosition;
                    dpbox.boxlocalAngle = boxgo.transform.localEulerAngles;

                    RobotMgr.Instance.alldpbox.Add(djTemp.name, dpbox);
                }
            }

            //变换父子关系
            alldpbox = RobotMgr.Instance.alldpbox;
            if (alldpbox != null && alldpbox.Count > 0)
            {
                RobotMgr.Instance.ChangeParent(robotName, alldj, AddedAllGOs, alldpbox);
            }
        }
        #endregion

        if (loadingSprite != null)
        {
            loadingSprite.SetActive(false);  //关闭加载动画
        }

        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "no model");
        StartCoroutine(Test());
        djld = RecordContactInfo.Instance.FindLDDJ(robotName);
        ldgos = GetLianDongData.Inst.FindLDGOsData();
        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "find djld ldgos");
       // gosld = RecordContactInfo.Instance.FindLDGOs(robotName);
    }

    public delegate void voidDelegate();
    public voidDelegate OnModelLoadOver;
    /// <summary>
    /// 等待模型加载完毕后，关闭屏幕屏蔽物体
    /// </summary>
    /// <returns></returns>
    IEnumerator Test()
    {
        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "等待模型加载完毕后，关闭屏幕屏蔽物体 000");
        yield return new WaitForEndOfFrame();
        
        if(OnModelLoadOver != null)
            OnModelLoadOver();
        AddScript();
        EventMgr.Inst.Fire(EventID.Main_Model_Load_Finished);
        PlatformMgr.Instance.CloseWaitingPage();
    }

    /// <summary>
    /// 给碰撞的物体添加脚本
    /// </summary>
    public void AddScript()
    {

        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "add ColliderGO script");
        bool colliderGO = RecordContactInfo.Instance.HasNode("ColliderGO");
        if (colliderGO)
        {
            Dictionary<string, string> cGOs = new Dictionary<string, string>();
            cGOs = RecordContactInfo.Instance.FindColliderGO();

            if (cGOs != null && cGOs.Count > 0)
            {
                foreach (string namT in cGOs.Keys)
                {
                    GameObject goTemp = GameObject.Find(namT);
                    if (goTemp != null)
                    {
                        ColliderControl.AddClass(goTemp, "ysjlsCollider");
                    }
                }
            }
        }
        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "AddScript finish");
    }

    /// <summary>
    /// 生成个人模型，只有舵机
    /// </summary>
    /// <param name="prefabgo"></param>
    /// <param name="oPos"></param>
    /// <param name="newt"></param>
    /// <param name="nameT"></param>
    /// <param name="oAngle"></param>
    public void ProduceGO(GameObject prefabgo, Vector3 oPos, out GameObject newt, string nameT, Vector3 oAngle)
    {
        try
        {
            newt = UnityEngine.Object.Instantiate(prefabgo, oPos, Quaternion.identity) as GameObject;

            newt.name = nameT;
            newt.transform.eulerAngles = oAngle;
            newt.transform.parent = oriGO.transform;
            newtName = nameT;

            if (AddedAllGOs.Contains(newt) == false)
            {
                AddedAllGOs.Add(newt);
            }
            else
            {
                Destroy(newt);
            }
        }
        catch (System.Exception ex)
        {
            newt = null;
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


 
    /// <summary>
    /// 创建旋转箭头
    /// </summary>
    GameObject arrowgo;
    public void CreateArrow(GameObject tgo)
    {
        try
        {
            string t1type = RobotMgr.Instance.rbt[robotName].gos[tgo.name].goType;

            if (t1type == "seivo")
            {
                arrowgo = UnityEngine.GameObject.Instantiate(arrow, tgo.transform.position, Quaternion.identity) as GameObject;
                arrowgo.transform.right = tgo.transform.right;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }


    /// <summary>
    /// 删除旋转箭头
    /// </summary>
    public void DestroyArrow()
    {
        try
        {
            if (null != arrowgo)
            {
                UnityEngine.GameObject.Destroy(arrowgo);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 将字符串转换为Vector3
    /// </summary>
    /// <param name="vect"></param>
    /// <returns></returns>
    public Vector3 StringToVector(string vect)
    {
        Vector3 newVect = Vector3.zero;
        try
        {
            string[] num = vect.Split(new char[] { '(', ',', ')' });

            //Convert.ToSingle()将字符转换为float
            newVect = new Vector3(Convert.ToSingle(num[1]), Convert.ToSingle(num[2]), Convert.ToSingle(num[3]));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return newVect;
    }


    /// <summary>
    /// 动作控制模块初始化时，需要的数据
    /// </summary>
    /// <param name="nam"></param>
    void MoveStart2nd(string nam)
    {
        try
        {
            mDjInitRota = RobotMgr.Instance.FindDJData(robotName);

            if (null != mDjInitRota)
            {
                mDjRotaDict.Clear();
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                foreach (KeyValuePair<string, int> kvp in mDjInitRota)
                {
                    mDjEulerDict[kvp.Key] = kvp.Value;
                    mDjRotaDict[kvp.Key] = RobotMgr.Instance.rbt[robotName].gos[kvp.Key].oriDJAngleX; //PublicFunction.DuoJi_Start_Rota;
                    if (null != robot)
                    {
                        byte id = (byte)RobotMgr.Instance.FinddjIDBydjNam(robotName, kvp.Key);
                        robot.SetStartRota(id, (int)mDjRotaDict[kvp.Key]);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    public List<string> GetAllDjName()
    {
        if (null != mDjInitRota)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, int> kvp in mDjInitRota)
            {
                list.Add(kvp.Key);
            }
            return list;
        }
        return null;
    }

    /// <summary>
    /// 通过名字查找物体
    /// </summary>
    /// <param name="nam"></param>
    /// <returns></returns>
    public GameObject FindGOByName(string nam)
    {

        for (int i = 0; i < AddedAllGOs.Count; i++)
        {
            if (AddedAllGOs[i].name == nam)
            {
                return AddedAllGOs[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 把舵机的角度转换成模型上的舵盘角度
    /// </summary>
    /// <param name="name"></param>
    /// <param name="djRota"></param>
    /// <returns></returns>
    float ConvertToEulerAngles(string name, float djRota)
    {
        if (null != mDjInitRota && mDjInitRota.ContainsKey(name))
        {
            float euler = djRota + mDjInitRota[name] - RobotMgr.Instance.rbt[robotName].gos[name].oriDJAngleX;//PublicFunction.DuoJi_Start_Rota;
            if (euler < 0)
            {
                euler += 360;
            }
            return euler;
        }
        return djRota;
    }
    /// <summary>
    /// 把模型上的舵盘角度转换成舵机的角度
    /// </summary>
    /// <param name="name"></param>
    /// <param name="eulerAngles"></param>
    /// <returns></returns>
    float ConvertToDjRota(string name, float eulerAngles)
    {
        if (null != mDjInitRota && mDjInitRota.ContainsKey(name))
        {
            if (eulerAngles < 0)
            {
                eulerAngles += 360;
            }
            float rota = eulerAngles + RobotMgr.Instance.rbt[robotName].gos[name].oriDJAngleX - mDjInitRota[name];
            if (rota < 0)
            {
                rota += 360;
            }
            return rota;
        }
        return eulerAngles;
    }
    void ReadStartRotaAck(EventArg arg)
    {
        try
        {
            DuoJiData data = (DuoJiData)arg[0];
            string name = RobotMgr.Instance.FindDJBydjID(robotName, data.id);//PublicFunction.GetDuoJiName(data.id);
            if (mDjRotaDict.ContainsKey(name))
            {
                mDjRotaDict[name] = data.rota;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    void CtrlAction(EventArg arg)
    {
        try
        {
            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null == robot)
            {
                return;
            }
            if (ResFileType.Type_default != ResourcesEx.GetRobotType(robot))
            {
                return;
            }
            Action action = (Action)arg[0];
            if (null == action)
            {
                return;
            }
            foreach (KeyValuePair<byte, short> kvp in action.rotas)
            {
                string name = RobotMgr.Instance.FindDJBydjID(robotName, kvp.Key);//PublicFunction.GetDuoJiName(kvp.Key);
                short rota = kvp.Value;
                if (rota < 0)
                {
                    rota = (short)(-rota);
                }
                DuoJiData data = robot.GetAllDjData().GetDjData(kvp.Key);
                if (null != data && data.modelType == ServoModel.Servo_Model_Angle)
                {
                    RotateTo(name, rota, action.sportTime);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    void AdjustAngleForUI(EventArg arg)
    {
        try
        {
            int id = (int)arg[0];
            int rota = (int)arg[1];
            string name = RobotMgr.Instance.FindDJBydjID(robotName, id);//PublicFunction.GetDuoJiName(kvp.Key);
            RotateTo(name, rota);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void OnReadBackAck(EventArg arg)
    {
        try
        {
            CtrlAction(arg);
            /*DuoJiData data = (DuoJiData)arg[0];
            if (null != data)
            {
                string name = RobotMgr.Instance.FindDJBydjID(robotName, data.id);//PublicFunction.GetDuoJiName(data.id);
                GameObject obj = FindGOByName(name);
                if (null != obj)
                {
                    DefaultZone(obj);
                    RotateTo(mSelectData.selectObj, data.rota, true);
                    ClearZones();
                }
            }*/
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public void RotateTo(string djName, int a, int duration = 0)
    {
        try
        {

            if (duration > 0)
            {
                if (!mSelectDjData.ContainsKey(djName))
                {
                    mSelectDjData[djName] = new SelectDuoJiData();
                }
                mSelectDjData[djName].djName = djName;
                mSelectDjData[djName].selectObj = FindRotateGO(djName);
                mSelectDjData[djName].duration = duration;
                mSelectDjData[djName].Play(mDjRotaDict[djName], a, duration / 1000.0f);
            }
            else
            {
                if (mDjRotaDict.ContainsKey(djName))
                {
                    float rota = a - mDjRotaDict[djName];
                    if (Math.Abs(rota) > 0.1f)
                    {
                        //Debug.Log("test01:"+djName);
                        Rotate(djName, FindRotateGO(djName), rota);
                        
                       LDRotate(djName,rota);
                    }
                }
            }

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }


    }

    /// <summary>
    /// 舵机联动
    /// </summary>
    /// <param name="djName"></param>
    /// <param name="rota"></param>
    public void LDRotate(string djName,float rota)
    {
        if(djld!=null&&djld.Count>0&&djld.ContainsKey(djName))
        {
            //Debug.Log("djld.Count02:"+djld.Count);
            foreach(string keyChild in djld[djName].djlds.Keys)
            {
                DJLianDong djldTemp = djld[djName].djlds[keyChild];
                if (djldTemp.difA == 0 && djldTemp.difB == 0)
                {
                    Rotate(keyChild, FindRotateGO(keyChild), MathTool.MathResult(0, djldTemp.symbol, rota));
                }
                else if (djldTemp.difA != 0 && djldTemp.difB == 0)
                {
                    Rotate(keyChild, FindRotateGO(keyChild), MathTool.MathResult(djldTemp.difA, djldTemp.symbol, rota));
                }
                else if (djldTemp.difA == 0 && djldTemp.difB != 0)
                {
                    Rotate(keyChild, FindRotateGO(keyChild), MathTool.MathResult(rota, djldTemp.symbol, djldTemp.difB));
                }
            }
                           
        }

        
    }

    /// <summary>
    /// 舵机带动的联动物体角度重置
    /// </summary>
    /// <param name="djName"></param>
    /// <param name="rota"></param>
    public void LDGOsAngle(string djName)
    {
        //Debug.Log("LDGOnfsfsfsS:");
        if (ldgos != null && ldgos.Count > 0)
        {
            GameObject dp = alldpbox[djName].dp;
            float angleX = dp.transform.eulerAngles.x;

            //Debug.Log("dp:" + dp.name + ";anglex:" + angleX);
            if (angleX > 180)
            {
                angleX -= 360;
            }
            
            int idTemp = 0;
            Dictionary<int, ldgo> ldgoT = ldgos[djName].ldgos;
            
            foreach (int idChild in ldgoT.Keys)
            {
                if ((ldgoT[idChild].start < angleX) && (ldgoT[idChild].end>=angleX))
                {
                    idTemp = idChild;
                    
                }
            }
            
            if (idTemp != 0)
            {
                Dictionary<string, Vector3> ldangle = ldgoT[idTemp].ldangle;
                if (ldangle!=null)
                {
                    foreach (string nameT in ldangle.Keys)
                    {
                        //Debug.Log("LDGOS:" + nameT + ";vector3:" + ldangle[nameT]);
                        AddedAllGOsDic[nameT].transform.eulerAngles = ldangle[nameT];
                    }
                }

                Dictionary<string, Vector3> ldpos = ldgoT[idTemp].ldpos;
                if (ldpos != null)
                {
                    foreach (string nameT in ldpos.Keys)
                    {
                        Debug.Log("LDGOS:" + nameT + ";vector3:" + ldpos[nameT]);
                        AddedAllGOsDic[nameT].transform.position = ldpos[nameT];
                    }
                }
            }
        }
    }

    /// <summary>
    /// 舵机带动的物体联动
    /// </summary>
    /// <param name="djName"></param>
    /// <param name="rota"></param>
    public void LDGOsRotate(string djName, float rota)
    {
        if (gosld != null && gosld.Count > 0 && gosld.ContainsKey(djName))
        {
            Debug.Log("rota:" + rota);
            foreach (string keyChild in gosld[djName].djlds.Keys)
            {
                DJLianDong djldTemp=gosld[djName].djlds[keyChild];
                if (djldTemp.difA == 0 && djldTemp.difB == 0&&AddedAllGOsDic.ContainsKey(keyChild))
                {
                    if (keyChild == "m48_0026" || keyChild == "m48_0027" || keyChild == "m48_0028" || keyChild == "m48_0023" || keyChild == "m48_0030" || keyChild == "m48_0029")
                    {
                        AddedAllGOsDic[keyChild].transform.Rotate(new Vector3(0, 0, MathTool.MathResult(0, djldTemp.symbol, rota)));
                    }
                    //else if (keyChild == "m48_0024")
                    //{
                    //    AddedAllGOsDic[keyChild].transform.Rotate(new Vector3(0, 0, MathTool.MathResult(0, djldTemp.symbol,rota)));
                    //}
                    //else
                    //{
                        AddedAllGOsDic[keyChild].transform.Rotate(new Vector3(0, 0, MathTool.MathResult(0, djldTemp.symbol, rota)));
                    //}
                   
                   
                }
                else if (djldTemp.difA != 0 && djldTemp.difB == 0)
                {
                    AddedAllGOsDic[keyChild].transform.Rotate(new Vector3(0, 0, MathTool.MathResult(djldTemp.difA, djldTemp.symbol, rota)));
                }
                else if (djldTemp.difA == 0 && djldTemp.difB != 0)
                {
                    AddedAllGOsDic[keyChild].transform.Rotate(new Vector3(0, 0, MathTool.MathResult(rota, djldTemp.symbol, djldTemp.difB)));
                }
            }

        }
    }


    /// <summary>
    /// 模型旋转
    /// </summary>
    /// <param name="djName"></param>
    /// <param name="tselected"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    public int Rotate(string djName, GameObject tselected, float a)
    {
        float euler = 0;
        try
        {
            if (null != tselected)
            {
                euler = mDjRotaDict[djName] + a;
                if (euler < PublicFunction.DuoJi_Min_Rota)
                {
                    euler = PublicFunction.DuoJi_Min_Rota;
                    a = euler - mDjRotaDict[djName];
                }
                else if (euler > PublicFunction.DuoJi_Max_Rota)
                {
                    euler = PublicFunction.DuoJi_Max_Rota;
                    a = euler - mDjRotaDict[djName];
                }

                if (RobotMgr.isDJDP(robotName, djName) == false)
                {

                    tselected.transform.Rotate(new Vector3(a, 0, 0));
                     //Debug.Log("djname:" + djName + ";djzhuti" + ";rota:"+a);
                }
                else
                {
                    tselected.transform.Rotate(new Vector3(-a, 0, 0));
                    //Debug.Log("djname:" + djName + ";duoji" + ";rota:"+a);
                }

                mDjRotaDict[djName] += a;
                mDjEulerDict[djName] = PublicFunction.Rounding(euler);

            }

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return PublicFunction.Rounding(euler);
    }
    /// <summary>
    /// 通过舵机id获取连动数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Dictionary<int, int> GetDJLianDongData(int id, int targetRota)
    {
        try
        {
            string servoName = RobotMgr.Instance.FindDJBydjID(robotName, id);
            if (null != djld && djld.ContainsKey(servoName) && null != djld[servoName].djlds)
            {
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                if (null != robot && robot.Name.Equals(robotName))
                {
                    int rota = targetRota - robot.GetStartRota((byte)id);
                    Dictionary<int, int> dict = new Dictionary<int, int>();
                    foreach (KeyValuePair<string, DJLianDong> kvp in djld[servoName].djlds)
                    {
                        int offset = 0;
                        if (kvp.Value.difA == 0 && kvp.Value.difB == 0)
                        {
                            offset = (int)MathTool.MathResult(0, kvp.Value.symbol, rota);
                        }
                        else if (kvp.Value.difA != 0 && kvp.Value.difB == 0)
                        {
                            offset = (int)MathTool.MathResult(kvp.Value.difA, kvp.Value.symbol, rota);
                        }
                        else if (kvp.Value.difA == 0 && kvp.Value.difB != 0)
                        {
                            offset = (int)MathTool.MathResult(rota, kvp.Value.symbol, kvp.Value.difB);
                        }
                        dict[kvp.Value.djid] = offset;
                    }
                    return dict;
                }
                /*if (mDjRotaDict.ContainsKey(servoName))
                {
                    float rota = targetRota - mDjRotaDict[servoName];
                    Dictionary<int, int> dict = new Dictionary<int, int>();
                    foreach (KeyValuePair<string, DJLianDong> kvp in djld[servoName].djlds)
                    {
                        int offset = 0;
                        if (kvp.Value.difA == 0 && kvp.Value.difB == 0)
                        {
                            offset = (int)MathTool.MathResult(0, kvp.Value.symbol, rota);
                        }
                        else if (kvp.Value.difA != 0 && kvp.Value.difB == 0)
                        {
                            offset = (int)MathTool.MathResult(kvp.Value.difA, kvp.Value.symbol, rota);
                        }
                        else if (kvp.Value.difA == 0 && kvp.Value.difB != 0)
                        {
                            offset = (int)MathTool.MathResult(rota, kvp.Value.symbol, kvp.Value.difB);
                        }
                        dict[kvp.Value.djid] = offset;
                    }
                    return dict;
                }*/
                
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "MoveSecond-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return null;
    }

    float angleXTemp=0;
    void Update()
    {
       
        if (ldgos != null && ldgos.Count > 0)
        {
            foreach(string nameT in ldgos.Keys)
            {
                if (alldpbox.ContainsKey(nameT))
                {
                    GameObject dp = alldpbox[nameT].dp;
                    float angleX = dp.transform.localEulerAngles.x;
                    if(angleX!=angleXTemp)
                    {
                        LDGOsAngle(nameT);
                        angleXTemp = angleX;
                    }
                }
                
                
            }
            
        }
        
        
        try
        {
            if (mSelectDjData.Count > 0)
            {
                List<string> dellist = null;
                foreach (KeyValuePair<string, SelectDuoJiData> kvp in mSelectDjData)
                {
                    if (kvp.Value.Update())
                    {
                        if (null == dellist)
                        {
                            dellist = new List<string>();
                        }
                        dellist.Add(kvp.Key);
                    }
                    if (mDjRotaDict.ContainsKey(kvp.Key))
                    {
                        float offset = kvp.Value.rota - mDjRotaDict[kvp.Key];
                        if (Mathf.Abs(offset) > 0.1f)
                        {
                            //Debug.Log("test02");
                            Rotate(kvp.Value.djName, kvp.Value.selectObj, offset);

                            LDRotate(kvp.Value.djName, offset);
      
                           // LDGOsRotate(kvp.Value.djName, offset);
                        }
                    }

                }
                if (null != dellist)
                {
                    for (int i = 0, imax = dellist.Count; i < imax; ++i)
                    {
                        mSelectDjData.Remove(dellist[i]);
                    }
                    dellist.Clear();
                }
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

    }


    List<string> childGO = new List<string>();
    public List<GameObject> posGO = new List<GameObject>();
    /// <summary>
    /// 设置模型颜色
    /// </summary>
    /// <param name="robotname"></param>
    /// <param name="goTemp"></param>
    public static void SettingColorByOne(string robotname, GameObject goTemp)
    {
        try
        {
            if (goTemp.activeInHierarchy)
            {

                string nameTemp = goTemp.name;
				if (nameTemp.Contains("lnA") == false && nameTemp.Contains("lnB") == false && nameTemp.Contains("line") == false && nameTemp.Contains("onoff") == false)
                {
                    string colorTempL = RobotMgr.Instance.rbt[robotname].gos[nameTemp].color;

                    if (colorTempL != "")
                    {
                        Color colorTemp = SingletonObject<PublicTools>.GetInst().StringToColor(colorTempL);
                        RobotMgr.SettingColor(goTemp, colorTemp);
                    }
                }
                else
                {
                    RobotMgr.SettingLineColor(goTemp);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "MoveSecond-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


    /// <summary>
    /// 确认生成物体后，使模型的shader为Diffuse
    /// </summary>
    /// <param name="t"></param>
    /// <param name="ori"></param>
    public static void ChangeDiffuse(GameObject t, Color ori)
    {
        try
        {
            if (t != null)
            {
                RobotMgr.ChangeDiffuse(t, ori);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "MoveSecond-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }



    #region djid

    /// <summary>
    /// 获取所有djid
    /// </summary>
    /// <param name="robotname"></param>
    /// <returns></returns>
    public List<int> FindDJID(string robotname)
    {
        List<int> dict = new List<int>();
        try
        {
            dict = RobotMgr.Instance.FindDJID(robotname);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return dict;
    }

    /// <summary>
    /// 通过duiji的name找duiji的id
    /// </summary>
    /// <param name="robotname"></param>
    /// <param name="djname"></param>
    /// <returns></returns>
    public int FinddjIDBydjNam(string robotname, string djname)
    {
        int djid = 0;
        try
        {
            djid = RobotMgr.Instance.FinddjIDBydjNam(robotname, djname);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return djid;
    }

    /// <summary>
    /// 通过duiji的id找duiji的name
    /// </summary>
    /// <param name="robotname"></param>
    /// <param name="djidTemp"></param>
    /// <returns></returns>
    public string FindDJBydjID(string robotname, int djidTemp)
    {
        string djname = string.Empty;
        try
        {
            RobotMgr.Instance.FindDJBydjID(robotname, djidTemp);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return djname;
    }

    /// <summary>
    /// 修改duiji的id
    /// </summary>
    /// <param name="robotname"></param>
    /// <param name="oriid"></param>
    /// <param name="newid"></param>
    public void ReviseDJID(string robotname, int oriid, int newid)
    {
        try
        {
            RobotMgr.Instance.ReviseDJID(robotname, oriid, newid);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    #endregion

    /// <summary>
    /// 旋转物体
    /// </summary>
    /// <param name="pickgo"></param>
    /// <returns></returns>
    public GameObject FindRotateGO(string pickgo)
    {
        GameObject rotateGo = null;
        try
        {
            int isdp = RobotMgr.Instance.rbt[robotName].gos[pickgo].isDP;
            if (isdp == 1)
            {
                rotateGo = alldpbox[pickgo].dp;
            }
            else
            {
                rotateGo = alldpbox[pickgo].box;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return rotateGo;
    }


    #region  创建无模型机器人
    /// <summary>
    ///  创建无模型机器人
    /// </summary>
    /// <param name="robotname"></param>
    public void CreateRobotWithoutGO(string robotname)
    {
        try
        {

            mainCamera = GameObject.Find("MainCamera");
            
            bool needProduce = false;
            foreach (Transform child in mainCamera.GetComponentInChildren<Transform>())
            {
                if (child.name == "Camera")
                {
                    needProduce = true;
                }
            }
            if (needProduce == false)
            {
                arrowCamera = UnityEngine.GameObject.Instantiate(arrowCameraPreb, mainCamera.transform.position, Quaternion.identity) as GameObject;
                arrowCamera.name = "Camera";
                arrowCamera.transform.parent = mainCamera.transform;
                arrowCamera.transform.localEulerAngles = Vector3.zero;
            }

            oriGO = GameObject.Find("oriGO");
            if (oriGO == null)
            {
                oriGO = new GameObject();
                oriGO.name = "oriGO";
            }
            oriGO.transform.position = Vector3.zero;
            oriGO.transform.localEulerAngles = Vector3.zero;

            // RecordContactInfo.Instance.FindAllGOName2nd(goName);
            goName = RobotMgr.Instance.FindAllGOName(robotname);

            if (goName != null && goName.Count > 0)
            {

                foreach (string goN in goName)
                {
                    //string goType = RecordContactInfo.Instance.FindPickGOType2nd(goN);
                    if (goN != null)
                    {
                        string goType = RobotMgr.Instance.rbt[robotname].gos[goN].goType;

                        RobotMgr.Instance.FindGOPosAngle(robotname, goN, out goPos, out goAngle, out goScale);
                        objPos = StringToVector(goPos);
                        objAngle = StringToVector(goAngle);
                        objScale = StringToVector(goScale);

                        ProduceGO(prefabgos["seivo"], objPos, out newt, goN, objAngle);
                    }
                }
            }


        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        oriGO.SetActive(false);
        //return oriGO;
    }
    #endregion

    
    /// <summary>
    /// 返回到主场景
    /// </summary>
    /// <param name="go"></param>
    void OnReturnClicked(GameObject go)
    {
        // DontDestroyOnLoad(uir);
        SceneMgr.EnterScene(SceneType.MainWindow);
    }

    
    /// <summary>
    /// 设置模型父子关系
    /// </summary>
    public void ChangeParent()
    {
        RobotMgr.Instance.ChangeParent(robotName, alldj, AddedAllGOs, alldpbox);
    }

    
    /// <summary>
    /// 重置模型的父子关系
    /// </summary>
    public void ResetParent()
    {
        RobotMgr.Instance.ResetParent(robotName, oriGO);

    }
    
    /// <summary>
    /// 恢复舵机或者舵盘的坐标和角度
    /// </summary>
    public void ResetDJDPPA()
    {
        RobotMgr.Instance.ResetDJDPPAOld();
    }

    
    /// <summary>
    /// 重置oriGO的位置和角度
    /// </summary>
    public void ResetOriGOPos()
    {
        oriGO.transform.position = oriPos;

        oriGO.transform.localEulerAngles = oriAngle;
    }

    /// <summary>
    /// 重置goPosAll，goAngleAll，存储相对位置坐标和角度坐标
    /// </summary>
    public void RestGOPA()
    {
        goPosAll.Clear();
        goAngleAll.Clear();

        if (AddedAllGOs != null)
        {
            foreach (GameObject keyGO in AddedAllGOs)
            {

                string keytemp = keyGO.name;
                Vector3 posTemp = keyGO.transform.localPosition;
                Vector3 angTemp = keyGO.transform.localEulerAngles;

                goPosAll.Add(keytemp, posTemp);
                goAngleAll.Add(keytemp, angTemp);
            }

        }
    }

}

/// <summary>
/// 选中舵机后返回的物体，和旋转的方向
/// </summary>
public class SelectDuoJiData
{
    public string djName;
    public float rota;
    public float fromRota;
    public int targetRota;
    public GameObject selectObj;
    public float duration;
    public float time;
    public bool isPlaying;

    public SelectDuoJiData()
    {

    }

    public void Play(float rota, int target, float duration)
    {
        this.rota = rota;
        this.fromRota = rota;
        this.targetRota = target;
        this.duration = duration;
        time = 0;
        isPlaying = true;
    }

    public bool Update()
    {
        if (isPlaying)
        {
            time += Time.deltaTime;
            float val = time / duration;
            val = Mathf.Clamp01(val);
            rota = Mathf.Lerp(fromRota, targetRota, val);
            if (Mathf.Abs(rota - targetRota) <= 0.01f)
            {
                rota = targetRota;
                isPlaying = false;
                return true;
            }
        }
        return false;
    }
}


