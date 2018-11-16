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

public class Create3DRobot : MonoBehaviour
{
    public static Create3DRobot Instance;


    public const int specNum = 20; //生成模型时每次小循环生成的模型的数量


    #region 变量
    public GameObject oriGO;

    private Vector3 objPos;
    private Vector3 objAngle;
    private Vector3 objScale;

    List<string> typetemp;
    public Dictionary<string, GameObject> prefabgos = new Dictionary<string, GameObject>();
    private GameObject tempgo;

    public Dictionary<string, DPBOX> alldpbox = new Dictionary<string, DPBOX>();//舵盘和舵机主体旋转时需要的父物体模型--只用舵机才使用该属性

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
    private GameObject arrowCamera;    //显示旋转箭头的相机
    private GameObject arrowCameraPreb;



    public static string robotName;

    //已经生成的物体
    public List<GameObject> AddedAllGOs = new List<GameObject>();
    public Dictionary<string, GameObject> AddedAllGOsDic = new Dictionary<string, GameObject>();

    public List<GameObject> alldj = new List<GameObject>();  //所有的舵机物体-实物
    #endregion
    Dictionary<string, Texture> djIDTexture = new Dictionary<string, Texture>();

    public Vector3 oriPos;  //oriGO的默认坐标
    public Vector3 oriAngle;//oriGO的默认角度
    public Vector3 oriScale;//oriGO的默认尺寸


    GameObject mainProtect;//主场景
    GameObject loadingSprite; //加载动画

    float t1;
    List<string> storeParts = new List<string>();//内置的零件
    List<string> outParts = new List<string>();  //需要从后台下载加载的零件
    List<string> innerParts = new List<string>();  //模型需要的内置的零件

    Dictionary<string, string> wireJionts = new Dictionary<string, string>();     //<线的名称，joint后缀>
    static Dictionary<string, DJLianDongs> djld = new Dictionary<string, DJLianDongs>();//联动舵机
    Dictionary<string, DJLianDongs> gosld = new Dictionary<string, DJLianDongs>();//舵机引起的模型联动
    Dictionary<string, LianDongGOs> ldgos = new Dictionary<string, LianDongGOs>();

    public static Material matT;//默认使用的材质
    private string adsfilename = null;//广告动画名称

    int gonameCount;//所有零件的个数
    int outStartN = 0;
    int officalGoN = 0;   //官方模型GO开始的数字
    void Awake()
    {
        Instance = this;

        oriGO = GameObject.Find("oriGO");

        if (oriGO == null)
        {
            oriScale = new Vector3(1, 1, 1);
            robotName = RobotMgr.Instance.rbtnametempt;
            t1 = Time.realtimeSinceStartup;

            TextureMgr textM = new TextureMgr();
            djIDTexture = textM.FindIDPic();  //查找舵机ID贴图

            matT = Resources.Load("Prefab/Test4/Materials/AO") as Material;
            Debug.Log("matT:"+matT.name);
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
                //if (SceneMgr.mCurrentSceneType == SceneType.MainWindow)
                //{
                //mainProtect = GameObject.Find("UIRootABL/Camera/Center/protect");
                //loadingSprite = Resources.Load("Prefab/Test4/MainScene/Loading") as GameObject;
                //loadingSprite = GameObject.Instantiate(loadingSprite, Vector3.zero, Quaternion.identity) as GameObject;
                //loadingSprite.transform.parent = mainProtect.transform;
                //}

                arrowCameraPreb = Resources.Load("Prefab/Test4/ArrowCamera") as GameObject;

                Init();

            }
        }
    }


    #region 加载广告动画

    //  "file:///"只有WWW需要加
    public string ReturnAnimPath(string prePath)
    {
        string nameNoType = RobotMgr.NameNoType(robotName);

        string filepathPre = ResourcesEx.persistentDataPath + "/default/" + nameNoType + "/prebanim";//"/parts";  
        string path1 = null;

        if (prePath == "load")
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {

                path1 = "file:///" + filepathPre + "/editor/" + adsfilename + ".assetbundle";
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
    WWW bundleAnim = null;
    IEnumerator GetAnimAssets(string path)
    {

        bundleAnim = new WWW(path);
        yield return bundleAnim;
        try
        {
            if (bundleAnim.isDone == true)
            {
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "add adsfilename:" + adsfilename);
                UnityEngine.Object t = bundleAnim.assetBundle.LoadAsset(adsfilename);

                GameObject tempgot = t as GameObject;

                GameObject prebgo = Instantiate(tempgot, Vector3.zero, Quaternion.identity) as GameObject;
                prebgo.transform.parent = GameObject.Find("MainUIRoot_new").transform;
                prebgo.transform.localScale = new Vector3(1, 1, 1);

                foreach (Transform animchild in prebgo.transform.GetComponentsInChildren<Transform>())
                {
                    if (animchild.name == "close")
                    {
                        UIEventListener.Get(animchild.gameObject).onClick += CloseAdvertAnim;
                        GameObject camTemp = GameObject.Find("MainUIRoot_new/Camera");
                        UISprite animTemp = animchild.GetComponent<UISprite>();

                        if (camTemp != null && animTemp != null)
                        {
                            animTemp.leftAnchor.Set(camTemp.transform, 0, 39);
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
        // PublicFunction.DelDirector(path1, true);


    }
    #endregion



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

                //StartCoroutine(GetInnerParts(innerParts[outN], 0.001f, outN));
                GetInnerParts(innerParts[outN], 0.001f, outN);
                // GetInnerParts(innerParts[outN], outN);
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
    //IEnumerator GetInnerParts(string tem, float t3, int num)
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
                //Debug.Log("time1:" + Time.realtimeSinceStartup);
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "don't neet AddOutParts");
                string robotType = RobotMgr.DataType(robotName);
                prefabgos = RobotMgr.Instance.prefabgos;
                if (robotType == "default")
                {
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
                StartCoroutine(GetOutParts(temp, path1));
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

        try
        {
            PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "add outparts assets:" + tem);
            UnityEngine.Object t = bundle1.assetBundle.mainAsset;

            tempgo = t as GameObject;

            if (RobotMgr.Instance.prefabgos.ContainsKey(tem) == false)
            {
                RobotMgr.Instance.prefabgos.Add(tem, tempgo);
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
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        bundle1.assetBundle.Unload(false);
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

        FindOriComponent(outStartN);


    }

    /// <summary>
    /// 添加相机脚本CamRotateAroundCircle
    /// </summary>
    public void AddCamRAC()
    {
        mainCamera = GameObject.Find("MainCamera");

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
            PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "CreateTheGO:");
            AddCamRAC();

            oriGO = GameObject.Find("oriGO1");

            string nameNoType = RobotMgr.NameNoType(robotname);
            string[] x = RecordContactInfo.Instance.FindPosModel(nameNoType);
            oriPos = SingletonObject<PublicTools>.GetInst().StringToVector(x[0]);

            oriAngle = SingletonObject<PublicTools>.GetInst().StringToVector(x[1]);
            oriScale = SingletonObject<PublicTools>.GetInst().StringToVector(x[2]);
            if (oriGO == null)
            {

                oriGO = new GameObject();
                oriGO.name = "oriGO1";
                oriGO.transform.position = oriPos;
                oriGO.transform.localEulerAngles = oriAngle;

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

                Debug.Log("ProduceModel:" + robotname + ";officalGoN:" + officalGoN);
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
        //Debug.Log("ProduceModel 01:" + num);
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
        //PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeInfo, "finish produce go");
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
                if (RobotMgr.Instance.rbt[robotName].gos[nameT].hidego == "true")
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
                    RobotMgr.Instance.ShowID(newt, djIDTemp, djIDTexture);
                }

                //显示传感器ID
                if (RobotMgr.Instance.rbt[robotName].gos[nameT].sensorID != null)
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
                    if (RobotMgr.Instance.hideGOs.Contains(newt) == false)
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

                goPosAll = RobotMgr.Instance.goPosAll;
                goAngleAll = RobotMgr.Instance.goAngleAll;
                goScaleAll = RobotMgr.Instance.goScaleAll;
                alldj = RobotMgr.Instance.alldj;

                CreateModelFinish();
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

        oriGO.transform.localScale = oriScale;

        #region 模型父子关系

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

                if (dpgo != null && RobotMgr.Instance.alldpbox.ContainsKey(djTemp.name) == false)
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
        
        #endregion

        if (loadingSprite != null)
        {
            loadingSprite.SetActive(false);  //关闭加载动画
        }

        StartCoroutine(Test());
        djld = RecordContactInfo.Instance.FindLDDJ(robotName);
        ldgos = GetLianDongData.Inst.FindLDGOsData();
        // gosld = RecordContactInfo.Instance.FindLDGOs(robotName);
    }

    /// <summary>
    /// 等待模型加载完毕后，关闭屏幕屏蔽物体
    /// </summary>
    /// <returns></returns>
    IEnumerator Test()
    {
        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "等待模型加载完毕后，关闭屏幕屏蔽物体 000");
        if (null != oriGO)
        {
            oriGO.name = "oriGO";
        }
        yield return new WaitForEndOfFrame();        
        JMSimulatorOnly.Instance.InitialModelAnims();   // 分割动画
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "Create3DRobot-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "Create3DRobot-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

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

}
