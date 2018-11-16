//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using Game.Event;
using System;
using Game.Platform;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(ClientMain), true)]
public class ClientMainEditor : UIWidgetContainerEditor
{
    int eventIdIndex = 0;
    string callUnityArg = string.Empty;
    int callUnityIndex = 0;
    string[] callUnityIdAry = null;
    string[] callUnityCache = null;
    string[] eventIdAry = null;
    string[] eventIdCache = null;
    string findCallUnity = string.Empty;
    string findEventId = string.Empty;
    string sendMsgName = string.Empty;
    string sendMsgArg = string.Empty;
    Color32 collectColor = Color.white;
    public override void OnInspectorGUI ()
	{
		serializedObject.Update();
        if (null == callUnityIdAry)
        {
            Array arr = Enum.GetValues(typeof(CallUnityFuncID));
            callUnityIdAry = new string[arr.Length];
            callUnityCache = new string[arr.Length];
            for (int i = 0, imax = arr.Length; i < imax; ++i)
            {
                callUnityIdAry[i] = arr.GetValue(i).ToString();
                callUnityCache[i] = arr.GetValue(i).ToString();
            }
        }
        if (null == eventIdAry)
        {
            Array arr = Enum.GetValues(typeof(EventID));
            eventIdAry = new string[arr.Length];
            eventIdCache = new string[arr.Length];
            for (int i = 0, imax = arr.Length; i < imax; ++i)
            {
                eventIdAry[i] = arr.GetValue(i).ToString();
                eventIdCache[i] = arr.GetValue(i).ToString();
            }
        }
        NGUIEditorTools.DrawProperty("使用社区", serializedObject, "useThirdAppFlag");
        NGUIEditorTools.DrawProperty("模拟使用社区", serializedObject, "simulationUseThirdAppFlag");
        if (GUILayout.Button("编辑指引", GUILayout.Width(100), GUILayout.Height(24)))
        {
            SingletonObject<GuideManager>.GetInst().OpenGuideEdit();
        }
        if (GUILayout.Button("清除指引标记", GUILayout.Width(100), GUILayout.Height(24)))
        {
            SingletonObject<GuideManager>.GetInst().ClearPlayerGuide();
        }
        if (GUILayout.Button("清除PlayerPrefs", GUILayout.Width(100), GUILayout.Height(24)))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
        if (GUILayout.Button("显示高斯模糊", GUILayout.Width(100), GUILayout.Height(24)))
        {
            //RapidBlurEffect.ShowRapidBlurEffect();
            //GaussianBlurRenderer.ShowGaussianBlur();
            BlurEffect.ShowEffect();
        }
        //NGUIEditorTools.DrawProperty("复制默认文件", serializedObject, "copyDefaultFlag");
        ClientMain clientMain = target as ClientMain;
        if (null != clientMain)
        {
            if (clientMain.debugLogFlag)
            {
                if (GUILayout.Button("关闭日志", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    clientMain.SetLogState(false);
                }
            }
            else
            {
                if (GUILayout.Button("打开日志", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    clientMain.SetLogState(true);
                }
            }
            if (clientMain.UseTestModelFlag)
            {
                if (GUILayout.Button("退出测试模式", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    clientMain.UseTestModelFlag = false;
                }
            }
            else
            {
                if (GUILayout.Button("进入测试模式", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    clientMain.UseTestModelFlag = true;
                }
            }
			if (GUILayout.Button("打开测试标志", GUILayout.Width(100), GUILayout.Height(24)))
			{
#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "USE_TEST");
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, "USE_TEST");
#endif
			}
        }
        if (PlatformMgr.Instance != null && RobotManager.GetInst().GetCurrentRobot() != null)
        {
            if (!PlatformMgr.Instance.GetBluetoothState())
            {
                if (GUILayout.Button("连接蓝牙", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    PlatformMgr.Instance.ConnenctBluetooth("test", "test");
                    //SingletonObject<ConnectCtrl>.GetInst().OnConnectResult("test", true);
                }
            }
            else
            {
                if (GUILayout.Button("断开蓝牙", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    SingletonObject<ConnectCtrl>.GetInst().DisconnectNotify();
                }
            }
        }
        
        if (SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
        {
            if (GUILayout.Button("关闭逻辑编程", GUILayout.Width(100), GUILayout.Height(24)))
            {
                SingletonObject<LogicCtrl>.GetInst().TestExitLogic();
            }
        }
        else
        {
            if (GUILayout.Button("打开逻辑编程", GUILayout.Width(100), GUILayout.Height(24)))
            {
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                if (null != robot)
                {
                    SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(robot);
                }
            }
        }
        GUILayout.BeginHorizontal();
        collectColor = EditorGUILayout.ColorField("collectColor", collectColor);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("收集颜色", GUILayout.Width(100), GUILayout.Height(24)))
        {
            PublicFunction.ConverRgtToColor(collectColor.r, collectColor.g, collectColor.b);
        }
        if (GUILayout.Button("输出统计时间", GUILayout.Width(100), GUILayout.Height(24)))
        {
            SingletonObject<TimeStatisticsTool>.GetInst().CalculateAverage();
            SingletonObject<TimeStatisticsTool>.GetInst().CleanUp();
        }
        LauguageType lgType = LauguageTool.GetIns().CurLauguage;
        LauguageType tagType = lgType + 1;
        if (tagType == LauguageType.Ohter)
        {
            tagType = LauguageType.Chinese;
        }
        else if (tagType == LauguageType.Korean)
        {
            tagType = LauguageType.German;
        }
        if (GUILayout.Button("切换成" + tagType.ToString(), GUILayout.Width(100), GUILayout.Height(24)))
        {
            LauguageTool.GetIns().CurLauguage = tagType;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("EventID", GUILayout.Width(100));
        findEventId = EditorGUILayout.TextField(findEventId, GUILayout.Width(60));
        if (GUILayout.Button("搜索", GUILayout.Width(40)))
        {
            List<string> cmdList = new List<string>();
            for (int i = 0, imax = eventIdAry.Length; i < imax; ++i)
            {
                string cmd = eventIdAry[i].ToString();
                if (string.IsNullOrEmpty(findEventId) || cmd.Contains(findEventId) || findEventId.Contains(cmd))
                {
                    cmdList.Add(cmd);
                }
            }
            eventIdCache = new string[cmdList.Count];
            eventIdIndex = 0;
            for (int i = 0, imax = cmdList.Count; i < imax; ++i)
            {
                eventIdCache[i] = cmdList[i];
            }
        }
        eventIdIndex = EditorGUILayout.Popup(eventIdIndex, eventIdCache/*, GUILayout.Width(200)*/);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("执行", GUILayout.Width(100), GUILayout.Height(24)))
        {
            if (-1 != eventIdIndex)
            {
                EventID id = (EventID)Enum.Parse(typeof(EventID), eventIdCache[eventIdIndex]);
                if (id == EventID.BLUETOOTH_MATCH_RESULT)
                {
                    EventMgr.Inst.Fire(id, new EventArg(!PlatformMgr.Instance.GetBluetoothState()));
                }
                else
                {
                    EventMgr.Inst.Fire(id);
                }
            }
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("CallUnityFuncID", GUILayout.Width(100));
        findCallUnity = EditorGUILayout.TextField(findCallUnity, GUILayout.Width(60));
        if (GUILayout.Button("搜索", GUILayout.Width(40)))
        {
            List<string> cmdList = new List<string>();
            for (int i = 0, imax = callUnityIdAry.Length; i < imax; ++i)
            {
                string cmd = callUnityIdAry[i].ToString();
                if (string.IsNullOrEmpty(findCallUnity) || cmd.Contains(findCallUnity) || findCallUnity.Contains(cmd))
                {
                    cmdList.Add(cmd);
                }
            }
            callUnityCache = new string[cmdList.Count];
            callUnityIndex = 0;
            for (int i = 0, imax = cmdList.Count; i < imax; ++i)
            {
                callUnityCache[i] = cmdList[i];
            }
        }
        callUnityIndex = EditorGUILayout.Popup(callUnityIndex, callUnityCache/*, GUILayout.Width(200)*/);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("参数", GUILayout.Width(100));
        
        callUnityArg = EditorGUILayout.TextField(callUnityArg, /*GUILayout.Width(300), */GUILayout.Height(20));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("执行", GUILayout.Width(100), GUILayout.Height(24)))
        {
            if (callUnityIndex >= 0)
            {
                CallUnityFuncID id = (CallUnityFuncID)Enum.Parse(typeof(CallUnityFuncID), callUnityCache[callUnityIndex]);
                PlatformMgr.Instance.TestCallUnityFunc(id, callUnityArg);
            }
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("方法名", GUILayout.Width(100));
        sendMsgName = EditorGUILayout.TextField(sendMsgName, GUILayout.Width(100), GUILayout.Height(20));
        GUILayout.Label("参数", GUILayout.Width(60));
        sendMsgArg = EditorGUILayout.TextField(sendMsgArg, GUILayout.Width(100), GUILayout.Height(20));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("执行", GUILayout.Width(100), GUILayout.Height(24)))
        {
            if (!string.IsNullOrEmpty(sendMsgName))
            {
                PlatformMgr.Instance.SendMessage(sendMsgName, sendMsgArg.Replace("\\n","\n"));
            }
        }

        serializedObject.ApplyModifiedProperties();
	}
}
