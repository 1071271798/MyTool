//#define USE_TEST
#if UNITY_EDITOR
using UnityEditor;
#endif


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
    
    #endregion

    #region 私有属性
    #endregion

    #region 公有函数
    

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
            
            //永不待机
            //Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //锁定30帧
            //Application.targetFrameRate = 30;
            //日志开关
            
            SingletonObject<UIManager>.GetInst().Init();

            SingletonObject<SceneManager>.GetInst().Init();

            SingletonObject<PopWinManager>.GetInst().Init();
            

            DontDestroyOnLoad(gameObject);

            SceneManager.GetInst().GotoScene(typeof(PartsLoadScene));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    void Update()
    {
        try
        {
            SingletonObject<SceneManager>.GetInst().Update();
            SingletonObject<PopWinManager>.GetInst().Update();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void OnDestroy()
    {
        try
        {
            SingletonObject<UIManager>.GetInst().Dispose();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    void OnApplicationQuit()
    {
        try
        {
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void OnApplicationFocus(bool focusStatus)
    {
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
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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