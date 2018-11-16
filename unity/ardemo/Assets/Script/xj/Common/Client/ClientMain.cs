//#define USE_TEST
#if UNITY_EDITOR
using UnityEditor;
#endif
using Game.Platform;
using Game.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;
using System.Text;

/// <summary>
/// Author:xj
/// FileName:ClientMain.cs
/// Description:客户端主入口
/// Time:2015/7/21 9:59:17
/// </summary>
public class ClientMain : SingletonBehaviour<ClientMain>
{
    #region 公有属性
    public delegate void Time_CallBack();
    public static bool Exception_Log_Flag
    {
        get { return SingletonBehaviour<ClientMain>.GetInst().debugLogFlag; }
    }
    public static bool Use_Third_App_Flag
    {
        get { return SingletonBehaviour<ClientMain>.GetInst().useThirdAppFlag; }
    }
    public static bool Simulation_Use_Third_App_Flag
    {
        get { return SingletonBehaviour<ClientMain>.GetInst().simulationUseThirdAppFlag; }
    }

    public static bool Copy_Default_Flag
    {
        get { return SingletonBehaviour<ClientMain>.GetInst().copyDefaultFlag; }
    }

    public bool useThirdAppFlag = false;
    public bool simulationUseThirdAppFlag = false;
    public bool debugLogFlag = true;
    public bool copyDefaultFlag = true;

    private bool useTestModelFlag = false;
    public bool UseTestModelFlag
    {
        get
        {
            return useTestModelFlag;
        }
        set
        {
            if (value)
            {
                SceneMgr.EnterScene(SceneType.EmptyScene, typeof(TestScene));
            }
            else
            {
                MainScene.GotoScene();
            }
            useTestModelFlag = value;
        }
    }
    #endregion

    #region 私有属性
    #endregion

    #region 公有函数
    public void SetLogState(bool openFlag)
    {
        debugLogFlag = openFlag;
        Debuger.EnableLog = debugLogFlag;
    }

    public void WaitTimeInvoke(float sec, Time_CallBack callBack)
    {
        StartCoroutine(InvokeCallBack(sec, callBack));
    }

    public void WaitFrameInvoke(Time_CallBack callBack)
    {
        StartCoroutine(WaitFrameCallBack(callBack));
    }
    #endregion

    #region 私有函数
    void Start()
    {
        try
        {
            /*StringBuilder systemInfo = new StringBuilder();
            systemInfo.AppendFormat("deviceModel : {0}", SystemInfo.deviceModel);
            systemInfo.AppendFormat("deviceName : {0}", SystemInfo.deviceName);
            systemInfo.AppendFormat("deviceType : {0}", SystemInfo.deviceType.ToString());
            systemInfo.AppendFormat("deviceUniqueIdentifier : {0}", SystemInfo.deviceUniqueIdentifier);
            systemInfo.AppendFormat("graphicsDeviceID : {0}", SystemInfo.graphicsDeviceID);
            systemInfo.AppendFormat("graphicsDeviceName : {0}", SystemInfo.graphicsDeviceName);
            systemInfo.AppendFormat("graphicsMemorySize : {0}", SystemInfo.graphicsMemorySize);
            systemInfo.AppendFormat("operatingSystem : {0}", SystemInfo.operatingSystem);
            systemInfo.AppendFormat("systemMemorySize : {0}", SystemInfo.systemMemorySize);
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, systemInfo.ToString());*/
            //永不待机
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //锁定30帧
            Application.targetFrameRate = 30;
            //日志开关
            Debuger.EnableLog = debugLogFlag;
            //BuglyAgent.ConfigAutoQuitApplication(true);
            // 􀭏􀞐SDK􁌱􀷭􀮪􀲑􀜦􀒅􀝎􀫲􁇇􀹜􁧗􀛓􀮠􀙉􁳮
            //BuglyAgent.ConfigDebugMode(true);
#if UNITY_IPHONE || UNITY_IOS
            //BuglyAgent.InitWithAppId ("29e4f7309a");
#elif UNITY_ANDROID
            //BuglyAgent.InitWithAppId("3981030d46");
#endif
#if !UNITY_EDITOR
            BuglyAgent.RegisterLogCallback(LogCallback);
#endif
           
            BuglyAgent.EnableExceptionHandler();
            //BuglyAgent.ReportException(new NullReferenceException(), "测试");
            SingletonObject<UIManager>.GetInst().Init();

            SingletonObject<SceneManager>.GetInst().Init();

            SingletonObject<PopWinManager>.GetInst().Init();
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "ui框架初始化完毕");
            PlatformMgr plat = gameObject.GetComponent<PlatformMgr>();
            if (null == plat)
            {
                plat = gameObject.AddComponent<PlatformMgr>();
            }
            if (Use_Third_App_Flag)
            {
#if !UNITY_IPHONE
                plat.OpenBluetooth();
#endif
            }
            else
            {
                plat.OpenBluetooth();
            }

            DontDestroyOnLoad(gameObject);
            if (useThirdAppFlag)
            {
                CopyDefaultModelFineshed(true);
            }
            else
            {
                DefaultModelCopy defaultCopy = new DefaultModelCopy();
                defaultCopy.CheckDefaultCopy(CopyDefaultModelFineshed);
            }
            /*#if !UNITY_ANDROID
                        DefaultModelCopy defaultCopy = new DefaultModelCopy();
                        defaultCopy.CheckDefaultCopy(CopyDefaultModelFineshed);
            #else
                        if (useThirdAppFlag)
                        {
                            CopyDefaultModelFineshed(true);
                        }
                        else
                        {
                            DefaultModelCopy defaultCopy = new DefaultModelCopy();
                            defaultCopy.CheckDefaultCopy(CopyDefaultModelFineshed);
                        }
            #endif*/
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            BuglyAgent.PrintLog(LogSeverity.LogWarning, "condition = {0}   stackTrace = {1}", condition, stackTrace);
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, string.Format("condition = {0}   stackTrace = {1}", condition, stackTrace));
        }
    }
    void CopyDefaultModelFineshed(bool result)
    {
        if (!Use_Third_App_Flag)
        {
            LauguageTool.GetIns().SetCurLauguage();
            //SceneMgr.EnterScene(SceneType.MenuScene);
            GameMenuScene.GotoGameMenu(EGameMenuType.RainbowWorld);
        }
        else
        {
#if !UNITY_ANDROID
            SceneMgr.EnterScene(SceneType.EmptyScene);
#endif
        }
        PlatformMgr.Instance.PlatformInit();
        if (Simulation_Use_Third_App_Flag)
        {
            PlatformMgr.Instance.GotoScene("{\"modelID\":\"Baby\",\"modelName\":\"Baby\",\"picPath\":\"E:/RobotUnity3D/Assets/StreamingAssets/defaultFiles/data/customize/image/BABY.jpg\",\"modelType\":0}");
        }
    }

    void Update()
    {
        try
        {
            SingletonObject<SceneManager>.GetInst().Update();
            SingletonObject<PopWinManager>.GetInst().Update();
            NetWork.GetInst().Update();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    void LateUpdate()
    {
        try
        {
            SingletonObject<SceneManager>.GetInst().LateUpdate();
            SingletonObject<PopWinManager>.GetInst().LateUpdate();
            SingletonObject<UIManager>.GetInst().CheckScreenSize();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void OnDestroy()
    {
        try
        {
            SingletonObject<UIManager>.GetInst().Dispose();
            PlatformMgr.Instance.PlatformDestroy();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    void OnApplicationQuit()
    {
        try
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                Robot robot = SingletonObject<ConnectCtrl>.GetInst().GetConnectRobot();
                
                if (SingletonObject<UpdateManager>.GetInst().GetUpdateDeviceType() != TopologyPartType.None)
                {
                    SingletonObject<UpdateManager>.GetInst().CannelUpdate();
                }
                PlatformMgr.Instance.DisConnenctBuletooth();
            }
            /*if (PlatformMgr.Instance.IsOpenBluetooth())
            {
                PlatformMgr.Instance.CloseBluetooth();
            }*/
            MyLog.CloseMyLog();
#if UNITY_EDITOR
            Material material = AssetDatabase.LoadMainAssetAtPath("Assets/shader/Materials/Circular.mat") as Material;
            if (null != material)
            {
                material.SetFloat("_TexWidth", 1);
                material.SetFloat("_TexHeight", 1);
            }
#endif
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void OnApplicationFocus(bool focusStatus)
    {
#if !UNITY_EDITOR
        if (null != PlatformMgr.Instance && PlatformMgr.Instance.GetBluetoothState())
        {
            Robot robot = SingletonObject<ConnectCtrl>.GetInst().GetConnectRobot();
            if (!focusStatus)
            {
                PlatformMgr.Instance.SetSendXTState(false);
                if (SingletonObject<UpdateManager>.GetInst().GetUpdateDeviceType() == TopologyPartType.None)
                {
                    if (null != robot)
                    {
                        robot.SetBLEOutTimeState(false);
                    }
                }
                else
                {
                    SingletonObject<UpdateManager>.GetInst().CannelUpdate();
                }
            }
            else
            {
                //永不待机
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                PlatformMgr.Instance.SetSendXTState(true);
                if (null != robot)
                {
                    robot.SetBLEOutTimeState(true);
                }
            }
        }
#endif
        if (focusStatus)
        {
            SingletonObject<UIManager>.GetInst().CheckCameraDepth(eUICameraType.OrthographicOne);
            SingletonObject<UIManager>.GetInst().CheckCameraDepth(eUICameraType.OrthographicTwo);
        }
    }

    IEnumerator InvokeCallBack(float sec, Time_CallBack callBack)
    {
        yield return new WaitForSeconds(sec);
        try
        {
            if (null != callBack)
            {
                callBack();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    IEnumerator WaitFrameCallBack(Time_CallBack callBack)
    {
        yield return new WaitForEndOfFrame();
        try
        {
            if (null != callBack)
            {
                callBack();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public float ActionSpeed = 1;

#if USE_TEST
    string[] moveList = null;
    int selectMoveIndex = 0;
    string[] speedList = { "VS","S", "M", "F"};
    int selectSpeedIndex = 0;
    string playArg = "0";
    void OnGUI()
    {
        GUI.skin.label.normal.textColor = new Color(0, 0, 0, 1);
        // 后面的color为 RGBA的格式，支持alpha，取值范围为浮点数： 0 - 1.0.
        GUILayout.BeginVertical();
        GUILayout.Space(PublicFunction.GetHeight()*0.18f);
        GUILayout.EndVertical();
        if (GUILayout.Button("进入测试模式", GUILayout.Width(100), GUILayout.Height(60)))
        {
            UseTestModelFlag = true;
        }
        if (GUILayout.Button(ActionSpeed.ToString(), GUILayout.Width(60), GUILayout.Height(40)))
        {
            ActionSpeed -= 0.2f;
            if (ActionSpeed <= 0.01f)
            {
                ActionSpeed = 1;
            }
        }
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot)
        {          
            if (null == moveList)
            {
                List<MoveType> list = SingletonObject<MoveManager>.GetInst().GetRobotMoveList(robot.ID);
                if (null != list)
                {
                    moveList = new string[list.Count];
                    for (int i = 0, imax = list.Count; i < imax; ++i)
                    {
                        moveList[i] = list[i].ToString();
                    }
                }
            }
            
            if (null != moveList)
            {
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("动作", GUILayout.Width(60), GUILayout.Height(40));
                selectMoveIndex = GUILayout.SelectionGrid(selectMoveIndex, moveList, 4, GUILayout.Height(40));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("速度", GUILayout.Width(60), GUILayout.Height(40));
                selectSpeedIndex = GUILayout.SelectionGrid(selectSpeedIndex, speedList, 5, GUILayout.Height(40));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("参数", GUILayout.Width(60), GUILayout.Height(40));
                playArg = GUILayout.TextField(playArg, GUILayout.Width(100), GUILayout.Height(40));
                GUILayout.EndHorizontal();
                if (GUILayout.Button("执行", GUILayout.Width(100), GUILayout.Height(40)))
                {
                    /*if (!SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming && PlatformMgr.Instance.GetBluetoothState())
                    {
                        SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(robot);
                    }*/
                    if (selectMoveIndex == -1)
                    {
                        HUDTextTips.ShowTextTip("请选择动作");
                        return;
                    }
                    if (selectSpeedIndex == -1)
                    {
                        HUDTextTips.ShowTextTip("请选择速度");
                        return;
                    }
                    if (string.IsNullOrEmpty(playArg) || !PublicFunction.IsInteger(playArg))
                    {
                        HUDTextTips.ShowTextTip("请输入合法参数");
                        return;
                    }
                    MoveType moveType = (MoveType)Enum.Parse(typeof(MoveType), moveList[selectMoveIndex]);
                    MoveSpeed moveSpeed = (MoveSpeed)Enum.Parse(typeof(MoveSpeed), speedList[selectSpeedIndex]);
                    /*Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict["action_id"] = moveType.ToString();
                    dict["speed"] = (byte)moveSpeed;
                    dict["time"] = playArg;
                    PlatformMgr.Instance.TestCallUnityFunc(CallUnityFuncID.LogicCMD, SingletonObject<LogicCtrl>.GetInst().Logic_Cmd_Start + "playAction|" + LitJson.Json.Serialize(dict));*/
                    robot.PlayMove(moveType, moveSpeed, UInt32.Parse(playArg));
                }
            }
        }
    }
#endif
#if USE_TEST
    string[] motorIdsList;
    int selectMoveIndex = 0;
    string[] speedList = { "VS","S", "M", "F", "VF"};
    int selectSpeedIndex = 0;
    string[] direction = { "1", "2", "停止" };
    int selectDirectionIndex = 0;
    string playArg = "0";
    void OnGUI()
    {
        GUI.skin.label.normal.textColor = new Color(0, 0, 0, 1);
        // 后面的color为 RGBA的格式，支持alpha，取值范围为浮点数： 0 - 1.0.
        GUILayout.BeginVertical();
        GUILayout.Space(PublicFunction.GetHeight()*0.18f);
        GUILayout.EndVertical();
        if (GUILayout.Button("进入测试模式", GUILayout.Width(100), GUILayout.Height(60)))
        {
            UseTestModelFlag = true;
        }
        
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot && null != robot.MotorsData && robot.MotorsData.GetIds() != null)
        {          
            if (null == motorIdsList)
            {
                List<byte> list = robot.MotorsData.GetIds();
                if (null != list)
                {
                    motorIdsList = new string[list.Count + 1];
                    for (int i = 0, imax = list.Count; i < imax; ++i)
                    {
                        motorIdsList[i] = list[i].ToString();
                    }
                    motorIdsList[list.Count] = "all";
                }
            }
            
            if (null != motorIdsList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("马达", GUILayout.Width(60), GUILayout.Height(40));
                selectMoveIndex = GUILayout.SelectionGrid(selectMoveIndex, motorIdsList, 4, GUILayout.Height(40));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("速度", GUILayout.Width(60), GUILayout.Height(40));
                selectSpeedIndex = GUILayout.SelectionGrid(selectSpeedIndex, speedList, 5, GUILayout.Height(40));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("方向", GUILayout.Width(60), GUILayout.Height(40));
                selectDirectionIndex = GUILayout.SelectionGrid(selectDirectionIndex, direction, 3, GUILayout.Height(40));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("参数", GUILayout.Width(60), GUILayout.Height(40));
                playArg = GUILayout.TextField(playArg, GUILayout.Width(100), GUILayout.Height(40));
                GUILayout.EndHorizontal();
                if (GUILayout.Button("执行", GUILayout.Width(100), GUILayout.Height(40)))
                {
                    if (selectMoveIndex == -1)
                    {
                        HUDTextTips.ShowTextTip("请选择马达");
                        return;
                    }
                    if (selectSpeedIndex == -1)
                    {
                        HUDTextTips.ShowTextTip("请选择速度");
                        return;
                    }
                    if (selectDirectionIndex == -1)
                    {
                        HUDTextTips.ShowTextTip("请选择方向");
                        return;
                    }
                    if (string.IsNullOrEmpty(playArg) || !PublicFunction.IsInteger(playArg))
                    {
                        HUDTextTips.ShowTextTip("请输入合法参数");
                        return;
                    }
                    if (direction[selectDirectionIndex].Equals("停止"))
                    {
                        List<byte> stopList = new List<byte>();
                        if (motorIdsList[selectMoveIndex].Equals("all"))
                        {
                            foreach (string item in motorIdsList)
                            {
                                if (PublicFunction.IsInteger(item))
                                {
                                    stopList.Add(byte.Parse(item));
                                }
                            }
                        } else
                        {
                            stopList.Add(byte.Parse(motorIdsList[selectMoveIndex]));
                        }
                        SingletonObject<LogicCtrl>.GetInst().CallUnityCmd(LogicCtrl.GetInst().Logic_Cmd_Start + LogicCmd.stopMotor + "|" + Json.Serialize(stopList));
                    } else
                    {
                        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                        if (motorIdsList[selectMoveIndex].Equals("all"))
                        {
                            foreach (string item in motorIdsList)
                            {
                                if (PublicFunction.IsInteger(item))
                                {
                                    TestMotorData data = new TestMotorData();
                                    data.id = byte.Parse(item);
                                    data.direction = byte.Parse(direction[selectDirectionIndex]);
                                    data.speed = speedList[selectSpeedIndex];
                                    data.time = int.Parse(playArg);
                                    list.Add(data.GetDict());
                                }
                            }
                        }
                        else
                        {
                            TestMotorData data = new TestMotorData();
                            data.id = byte.Parse(motorIdsList[selectMoveIndex]);
                            data.direction = byte.Parse(direction[selectDirectionIndex]);
                            data.speed = speedList[selectSpeedIndex];
                            data.time = int.Parse(playArg);
                            list.Add(data.GetDict());
                        }
                        SingletonObject<LogicCtrl>.GetInst().CallUnityCmd(LogicCtrl.GetInst().Logic_Cmd_Start + LogicCmd.setMotor + "|" + Json.Serialize(list));
                    }                    
                }
            }
        }
    }
    
    public class TestMotorData
    {
        public byte id;
        public byte direction;
        public string speed;
        public int time;
        public TestMotorData()
        {

        }

        public Dictionary<string, object> GetDict()
        {
            Dictionary<string, object> info = new Dictionary<string, object>();
            info.Add("id", id);
            info.Add("direction", direction);
            info.Add("speed", speed);
            info.Add("time", time);
            return info;
        }
    }
#endif
    //颜色采集
    #if USE_TEST
    int selectColorIndex = 0;
    string[] colorAry = new string[] { "红", "橙", "黄", "绿", "青", "蓝", "紫", "白", "黑", "灰色" };
    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(PublicFunction.GetHeight() * 0.2f);
        GUILayout.EndVertical();
        selectColorIndex = GUILayout.SelectionGrid(selectColorIndex, colorAry, 4, GUILayout.Height(60));
        if (GUILayout.Button("开始采集", GUILayout.Width(100), GUILayout.Height(60)))
        {
            HUDTextTips.ShowTextTip("开始采集");
            SingletonObject<ColorCollectTool>.GetInst().StartCollect(PublicFunction.colorAry[selectColorIndex]);
        }
        if (GUILayout.Button("结束采集", GUILayout.Width(100), GUILayout.Height(60)))
        {
            HUDTextTips.ShowTextTip("结束采集");
            SingletonObject<ColorCollectTool>.GetInst().StopCollect();
        }
    }

#endif
    #endregion
}