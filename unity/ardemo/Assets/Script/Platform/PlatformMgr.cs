//#define Test
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Scene;
using System.Text;
using System.IO;
using System;
using Game.Event;
using LitJson;
using Game.Resource;
using Game.UI;

namespace Game.Platform
{
    public class PlatformMgr : MonoBehaviour
    {
        private static PlatformMgr m_instance;
        public static PlatformMgr Instance { get { return m_instance; } }

        public delegate void CallUnityDelegate(object arg);

        public ReadPowerMsgAck PowerData = new ReadPowerMsgAck();

        //public bool NeedUpdateFlag = false;
        
        PlatformInterface mPlatInterface = null;

        BluetoothMgr m_blueMgr = new BluetoothMgr();

        /// <summary>
        /// 边充边玩,true表示可以边充边玩，false表示不可以
        /// </summary>
        bool isChargePlaying = true;

        string mLastUser = string.Empty;
        
        public bool IsCourseFlag
        {
            get { if (mCurrentModuleType == ModuleType.LessonBuild) return true; return false; }
        }

        ModuleType mCurrentModuleType = ModuleType.None;
        public Game.Platform.ModuleType CurrentModuleType
        {
            get { return mCurrentModuleType; }
            set { mCurrentModuleType = value; }
        }
        int courseId = 0;
        public int CourseId
        {
            get { return courseId; }
        }

        FromPlatform mFromType = FromPlatform.Self;
        public FromPlatform FromType
        {
            get { return mFromType; }
        }

        /// <summary>
        /// 逻辑编程参数
        /// </summary>
        private Dictionary<string, object> logicDict;
        public System.Collections.Generic.Dictionary<string, object> LogicDict
        {
            get { return logicDict; }
            set { logicDict = value; }
        }
        public bool IsChargeProtected
        {
            get { return GetBluetoothState() && !isChargePlaying && PowerData.isAdapter; }
        }
        /// <summary>
        /// 等待升级完成标志
        /// </summary>
        public bool IsWaitUpdateFlag = false;

        Dictionary<string, CallUnityDelegate> mUnityDelegateDict;

        string Pic_Path;

        string mConnectedSpeakerMac = string.Empty;
        string mConnectingSpeakerMac = string.Empty;

        bool isInUnity = false;
        public bool IsInUnity
        {
            get { return isInUnity; }
            set
            {
                isInUnity = value;
                if (value)
                {
                    LauguageTool.GetIns().SetCurLauguage();
                } else
                {
                    PopWinManager.GetInst().CloseAll();
                }
            }
        }
        //关于苹果demo版标志
        int actionCameraValidity = 1;

        public bool isAppleDemo
        {
            get { return actionCameraValidity == 0 ? true : false; }
        }

        bool mEditFlag = true;
        public bool EditFlag
        {
            get { return mEditFlag; }
        }

        public SetHardwareState NeedSetServoFlag = SetHardwareState.None;

        public bool GetBluetoothState()
        {
            //return true;
            /*#if UNITY_EDITOR
                        return true;
            #endif */    
            return SingletonObject<ConnectCtrl>.GetInst().GetBluetoothState();
        }
        
        public void CleanUpBlue()
        {
            m_blueMgr.ClearDevice();
        }

        public string GetRobotTexturePath()
        {
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                if (!string.IsNullOrEmpty(Pic_Path))
                {
                    return "file:///" + Pic_Path;
                }
                return string.Empty;
            } else if (RobotManager.GetInst().IsSetDeviceIDFlag)
            {
                return string.Empty;
            } else
            {
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                if (null != robot)
                {
                    if (ResourcesEx.GetRobotType(robot) == ResFileType.Type_default)
                    {
                        string robotname = RobotMgr.Instance.rbtnametempt;
                        string robotNameNoType = RobotMgr.NameNoType(robotname);
                        return "file:///" + ResourcesEx.persistentDataPath + "/default/" + robotNameNoType + "/" + robotNameNoType + ".png";
                    }
                }
                if (!string.IsNullOrEmpty(Pic_Path))
                {
                    return "file:///" + Pic_Path;
                }
                return string.Empty;
            }
        }

        
        public string GetRobotIconPath()
        {
            string path = GetRobotTexturePath();
            /*if (string.IsNullOrEmpty(path))
            {
                path = "file:///" + Application.streamingAssetsPath + "/model.png";
            }*/
            return path;
        }
        /// <summary>
        /// 保存最后连接的蓝牙
        /// </summary>
        /// <param name="mac"></param>
        public void SaveLastConnectedData(string mac)
        {
            PlayerPrefs.SetString(PublicFunction.Last_Connected_Bluetooth, mac);
            PlayerPrefs.Save();
        }
        /// <summary>
        /// 保存模型连接的mac地址
        /// </summary>
        /// <param name="mac"></param>
        public void SaveRobotLastConnectedData(string robotId, string mac)
        {
            PlayerPrefs.SetString(robotId, mac);
            PlayerPrefs.Save();
        }
        /// <summary>
        /// 保存是jimu的mac地址，下次连接不用判断是否是jimu
        /// </summary>
        /// <param name="mac"></param>
        public void SaveJimuMac(string mac)
        {
            if (!string.IsNullOrEmpty(mac))
            {
                PlayerPrefs.SetInt(mac, 1);
                PlayerPrefs.Save();
            }
        }
        /// <summary>
        /// 通过mac地址获取主板名字
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public string GetNameForMac(string mac)
        {
            if (PlayerPrefs.HasKey(mac))
            {
                return PlayerPrefs.GetString(mac);
            }
            if (GetBluetoothState())
            {
                return SingletonObject<ConnectCtrl>.GetInst().GetConnectedName();
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取模型的mac地址
        /// </summary>
        /// <param name="robotId"></param>
        /// <returns></returns>
        public string GetRobotConnectedMac(string robotId)
        {
            if (GetBluetoothState())
            {
                return SingletonObject<ConnectCtrl>.GetInst().GetConnectedMac();
            }
            else if (PlayerPrefs.HasKey(robotId))
            {
                return PlayerPrefs.GetString(robotId);
            }
            return string.Empty;
        }

        public string GetLastConnectedMac()
        {
            if (PlayerPrefs.HasKey(PublicFunction.Last_Connected_Bluetooth))
            {
                return PlayerPrefs.GetString(PublicFunction.Last_Connected_Bluetooth);
            }
            return string.Empty;
        }

        public void RegesiterCallUnityDelegate(CallUnityFuncID id, CallUnityDelegate dlgt)
        {
            if (null == mUnityDelegateDict)
            {
                mUnityDelegateDict = new Dictionary<string, CallUnityDelegate>();
            }
            mUnityDelegateDict[id.ToString()] = dlgt;
        }

        public void RemoveCallUnityDelegate(CallUnityFuncID id)
        {
            if (null != mUnityDelegateDict)
            {
                mUnityDelegateDict.Remove(id.ToString());
            }
        }
        void Awake()
        {
            m_instance = this;
#if UNITY_EDITOR
            mPlatInterface = new PlatformInterface();
#elif UNITY_ANDROID
            mPlatInterface = new AndroidInterface();
#elif UNITY_IPHONE
            mPlatInterface = new IosInterface();
#endif
            //DontDestroyOnLoad(this.gameObject);
            RegesiterCallUnityDelegate(CallUnityFuncID.RegisterRobotResult, RegisterRobotResult);
            RegesiterCallUnityDelegate(CallUnityFuncID.UnitySetupSteeringEngineID, ModifyServoId);
            //RegesiterCallUnityDelegate(CallUnityFuncID.MainboardProgramVersion, MainboardProgramVersion);
            //RegesiterCallUnityDelegate(CallUnityFuncID.SteeringEngineProgramVersion, SteeringEngineProgramVersion);
            RegesiterCallUnityDelegate(CallUnityFuncID.ChargeProtection, ChargeProtectionCallBack);
            RegesiterCallUnityDelegate(CallUnityFuncID.LogicCMD, LogicCMDCallUnity);
            RegesiterCallUnityDelegate(CallUnityFuncID.ExitLogicView, ExitLogicView);
            RegesiterCallUnityDelegate(CallUnityFuncID.ConnectBLE, OpenBlueSearch);
            RegesiterCallUnityDelegate(CallUnityFuncID.JsShowExceptionCallback, JsShowExceptionCallback);
            RegesiterCallUnityDelegate(CallUnityFuncID.Destroy, QuitUnity);
            RegesiterCallUnityDelegate(CallUnityFuncID.autoConnect, BlueAutoConnect);
            RegesiterCallUnityDelegate(CallUnityFuncID.Screenshots, Screenshots);
            //RegesiterCallUnityDelegate(CallUnityFuncID.SensorProgramVersion, SensorProgramVersion);
            RegesiterCallUnityDelegate(CallUnityFuncID.setServoMode, SetServoModel);
            RegesiterCallUnityDelegate(CallUnityFuncID.gotoModule, GotoModule);
            RegesiterCallUnityDelegate(CallUnityFuncID.DisConnect, DisConnect);
            RegesiterCallUnityDelegate(CallUnityFuncID.hardwareProgramVersion, HardwareProgramVersion);
            RegesiterCallUnityDelegate(CallUnityFuncID.clearHardwareProgramVersion, ClearHardwareProgramVersion);
            RegesiterCallUnityDelegate(CallUnityFuncID.setHardware, SetHardware);
            RegesiterCallUnityDelegate(CallUnityFuncID.downloadFilesNotify, downloadFilesNotify);
            RegesiterCallUnityDelegate(CallUnityFuncID.diyServoSetting, DiyServoSetting);

            if (PlayerPrefs.HasKey("isChargePlaying"))
            {
                int result = PlayerPrefs.GetInt("isChargePlaying");
                if (result >= 1)
                {
                    isChargePlaying = true;
                }
                else
                {
                    isChargePlaying = false;
                }
            }
           
        }
        
        
        void OnDestroy()
        {
        }
        void FixedUpdate()
        {
            try
            {
                SingletonObject<MyTime>.GetInst().Update();
                SingletonObject<PopWinManager>.GetInst().FixedUpdate();
                Timer.Update();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

        public void TestCallUnityFunc(CallUnityFuncID id, string arg)
        {
            if (null != mUnityDelegateDict && mUnityDelegateDict.ContainsKey(id.ToString()) && null != mUnityDelegateDict[id.ToString()])
            {
                if (!string.IsNullOrEmpty(arg) && (arg.StartsWith("{") || arg.StartsWith("[")))
                {
                    mUnityDelegateDict[id.ToString()](Json.Deserialize(arg));
                } else
                {
                    mUnityDelegateDict[id.ToString()](arg);
                }
            }
        }


        


#region Unity调用Platform

        public void PlatformInit()
        {
            mPlatInterface.PlatformInit();
        }

        public void PlatformDestroy()
        {
            mPlatInterface.PlatformDestroy();
        }
        //打开蓝牙
        public void OpenBluetooth()
        {
            try
            {
                mPlatInterface.OpenBluetooth();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }

        }


        public void CloseBluetooth()
        {
            try
            {
                mPlatInterface.CloseBluetooth();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }

        }
        /// <summary>
        /// 蓝牙是否开启
        /// </summary>
        /// <returns></returns>
        public bool IsOpenBluetooth()
        {
            try
            {
                return mPlatInterface.IsOpenBluetooth();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            return false;
        }
        
        public void StartScan()
        {
            try
            {
                SingletonObject<ConnectCtrl>.GetInst().CleanConnectData();
                m_blueMgr.ClearDevice();
                mPlatInterface.StartScan();
                Log(MyLogType.LogTypeEvent, "开启蓝牙搜索");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void StopScan()
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "停止蓝牙搜索");
                mPlatInterface.StopScan();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        //连接蓝牙设备
        public void ConnenctBluetooth(string mac, string name)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, string.Format("ConnenctBluetooth mac = {0} name = {1}", mac, name));
                SingletonObject<ConnectCtrl>.GetInst().OnConnect(mac, name);
                mPlatInterface.ConnenctBluetooth(mac);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }

        }
        /// <summary>
        ///  断开蓝牙连接
        /// </summary>
        public void DisConnenctBuletooth()
        {
            if (GetBluetoothState())
            {
                PowStatusChange(3);
            }
            SingletonObject<ConnectCtrl>.GetInst().OnDisconnect();
            //NeedUpdateFlag = false;
            OnlyDisConnectBluetooth();

        }

        public void OnlyDisConnectBluetooth()
        {
            try
            {
                m_blueMgr.ClearDevice();
                DisConnectSpeaker();
                SingletonObject<ConnectCtrl>.GetInst().CleanConnectData();
                mPlatInterface.DisConnenctBuletooth();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void DisDeviceBluetooth()
        {
            try
            {
                mPlatInterface.DisConnenctBuletooth();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        /// <summary>
        /// 取消蓝牙连接
        /// </summary>
        public void CannelConnectBluetooth()
        {
            try
            {
                SingletonObject<ConnectCtrl>.GetInst().CancelConnecting();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        public void SendMsg(byte cmd, byte[] param, int len)
        {
            try
            {
                mPlatInterface.SendMsg(cmd, param, len);
            }
            catch (System.Exception ex)
            {
                //MyLog.Log("platform SendMsg Exception" + ex.ToString());
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }

        }

        public void SaveModelOrActions(string contents)
        {
            try
            {
                mPlatInterface.SaveModelOrActions(contents);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void DelModel(string contents)
        {
            try
            {
                mPlatInterface.DelModel(contents);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void QuitUnityCleanData(EventDelegate.Callback callback = null)
        {
            try
            {
                IsInUnity = false;
                mEditFlag = true;
                mFromType = FromPlatform.Self;
                ShowTexture.ClearTexture2D();
                SingletonBehaviour<ResourcesLoad>.GetInst().UnLoad(typeof(Texture));
                SceneMgr.EnterScene(SceneType.EmptyScene, callback);
                try
                {
                    RobotMgr.Instance.GoToCommunity();
                }
                catch (System.Exception ex)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void BackThirdApp()
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "BackThirdApp");
                QuitUnityCleanData(delegate() {
                    SingletonBehaviour<ClientMain>.GetInst().WaitFrameInvoke(delegate ()
                    {
                        mPlatInterface.BackThirdApp();
                    });
                });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void BackToFromApp()
        {
            try
            {
                FromPlatform from = mFromType;
                QuitUnityCleanData(delegate() {
                    SingletonBehaviour<ClientMain>.GetInst().WaitFrameInvoke(delegate ()
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict["from"] = (byte)from;
                        CallPlatformFunc(CallPlatformFuncID.backTo, Json.Serialize(dict));
                    });
                });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void BackCourseMenu(bool useBlueState = true)
        {
            QuitUnityCleanData(delegate() {
                mCurrentModuleType = ModuleType.None;
                Dictionary<string, object> dict = LogicDict;
                if (null == dict)
                {
                    dict = new Dictionary<string, object>();
                }
                if (useBlueState)
                {
                    dict["blueState"] = (PlatformMgr.Instance.GetBluetoothState() ? 1 : 0).ToString();
                }
                else
                {
                    dict["blueState"] = "0";
                }
                string jsonbill = Json.Serialize(dict);
                PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.backCourseMenu, jsonbill);
                if (PlatformMgr.Instance.IsChargeProtected)
                {
                    LogicCtrl.GetInst().ChargeProtectedCallBack();
                }
            });
            
        }
        public void BackThirdAppImmediately()
        {
            try
            {
                IsInUnity = false;
                mEditFlag = true;
                Log(MyLogType.LogTypeEvent, "BackThirdAppImmediately");
                mPlatInterface.BackThirdApp();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="picPath"></param>
        public void Photograph(string modelName, string picPath)
        {
            mPlatInterface.Photograph(modelName, picPath);
        }
        /// <summary>
        /// 保存模型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void SaveModel(string name, ResFileType type = ResFileType.Type_playerdata)
        {
            mPlatInterface.SaveModel(name, type);
        }
        /// <summary>
        /// 发布模型
        /// </summary>
        /// <param name="name"></param>
        public void PublishModel(string name)
        {
            Log(MyLogType.LogTypeEvent, "发布模型 name =" + name);
            //SceneMgr.EnterScene(SceneType.EmptyScene);
            mPlatInterface.PublishModel(name);
        }
        /// <summary>
        /// 激活机器人
        /// </summary>
        /// <param name="mcuId"></param>
        /// <param name="sn"></param>
        public void ActivationRobot(string mcuId, string sn)
        {
            Log(MyLogType.LogTypeEvent, string.Format("激活设备 mcu = {0}  sn = {1}", mcuId, sn));
            mPlatInterface.ActivationRobot(mcuId, sn);
        }
        /// <summary>
        /// 调用各平台函数
        /// </summary>
        /// <param name="funcId"></param>
        /// <param name="arg"></param>
        public void CallPlatformFunc(CallPlatformFuncID funcId, string arg)
        {
            if (string.IsNullOrEmpty(arg) || !arg.Contains("queryAllSensor"))
            {
                Log(MyLogType.LogTypeEvent, "CallPlatformFuncID = " + funcId + " arg = " + arg);
            }
            mPlatInterface.CallPlatformFunc(funcId.ToString(), arg);
        }
        /// <summary>
        /// 设置心跳开关
        /// </summary>
        /// <param name="state"></param>
        public void SetSendXTState(bool state)
        {
            Log(MyLogType.LogTypeEvent, string.Format("设置心跳开关 state = {0}", state.ToString()));
            mPlatInterface.SetSendXTState(state);
            IsWaitUpdateFlag = !state;
        }

        void OpenSendXT()
        {
            SetSendXTState(true);
        }
        /// <summary>
        /// 获取用户数据
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string GetUserData(UserDataType dataType = UserDataType.userId)
        {
            return mPlatInterface.GetPlatformData(dataType.ToString());
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public string GetData(PlatformDataType dataType, string jsonString)
        {
            return mPlatInterface.GetData(dataType.ToString(), jsonString);
        }
        /// <summary>
        /// 友盟统计事件
        /// </summary>
        /// <param name="id"></param>
        public void MobClickEvent(MobClickEventID id)
        {
            MobClickEvent(id, string.Empty);
        }
        /// <summary>
        /// 友盟统计事件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="arg"></param>
        public void MobClickEvent(MobClickEventID id, object arg)
        {
            MobClickEvent(MobEventType.record_event, id, arg);
        }

        public void MobPageStart(MobClickEventID id)
        {
            MobClickEvent(MobEventType.measure_page_time_start, id, string.Empty);
        }

        public void MobPageEnd(MobClickEventID id)
        {
            MobClickEvent(MobEventType.measure_page_time_end, id, string.Empty);
        }

        public void MobClickEvent(MobEventType eventType, MobClickEventID id, object arg)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["eventName"] = id.ToString();
            dict["params"] = arg;
            dict["type"] = eventType.ToString();
            string result = Json.Serialize(dict);
            CallPlatformFunc(CallPlatformFuncID.MobClickEvent, result);
        }
        /// <summary>
        /// 统计持续时间的事件开始
        /// </summary>
        /// <param name="id"></param>
        public void DurationEventStart(MobClickEventID id)
        {
            try
            {
                SingletonObject<TimeStatisticsTool>.GetInst().EventStart(id.ToString());
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void DurationEventEnd(MobClickEventID id)
        {
            try
            {
                SingletonObject<TimeStatisticsTool>.GetInst().EventFinished(id.ToString(), delegate(long time) {
                    if (time > 0)
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict["duration"] = time;
                        if (id == MobClickEventID.app_building_complete)
                        {
                            if (null != RobotManager.GetInst().GetCurrentRobot())
                            {
                                dict["modelId"] = RobotManager.GetInst().GetCurrentRobot().GetModelID();
                            }
                        }
                        //PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, string.Format("id = {0} params = {1}", id, Json.Serialize(dict)));
                        MobClickEvent(id, dict);
                    }
                });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }


        public void RemoveDurationEvent(MobClickEventID id)
        {
            SingletonObject<TimeStatisticsTool>.GetInst().RemoveEvent(id.ToString());
        }
        public bool IsConnectedSpeaker(string speaker)
        {
            return mPlatInterface.IsConnectedSpeaker(speaker);
        }

        public void ConnectSpeaker(string speaker)
        {
            if (!string.IsNullOrEmpty(mConnectingSpeakerMac))
            {
                return;
            }
            mConnectingSpeakerMac = speaker;
            mPlatInterface.ConnectSpeaker(speaker);
        }

        public void DisConnectSpeaker()
        {
            mPlatInterface.DisConnectSpeaker(mConnectedSpeakerMac);
            mConnectedSpeakerMac = string.Empty;
            mConnectingSpeakerMac = string.Empty;
        }
        /// <summary>
        /// 同步文件
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="modelType"></param>
        /// <param name="filePath"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        public bool OperateSyncFile(string modelId, ResFileType modelType, string filePath, OperateFileType operateType)
        {
            Log(MyLogType.LogTypeEvent, string.Format("同步文件 modelId = {0} modelType = {1} operateType = {2} filePath = {3}", modelId, modelType.ToString(), operateType.ToString(), filePath));
            return mPlatInterface.OperateSyncFile(modelId, modelType, filePath, operateType);
        }

        /// <summary>
        /// 同步文件
        /// </summary>
        /// <param name="robotName"></param>
        /// <param name="filePath"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        public bool OperateSyncFile(string robotName, string filePath, OperateFileType operateType)
        {
            string[] ary = robotName.Split('_');
            if (null != ary && ary.Length == 2)
            {
                return mPlatInterface.OperateSyncFile(ary[0], ResourcesEx.GetResFileType(ary[1]), filePath, operateType);
            }
            return false;
        }

        public void Log(MyLogType logType, string text)
        {
            switch (logType)
            {
                case MyLogType.LogTypeInfo:
                    mPlatInterface.LogInfo(text);
                    BuglyAgent.PrintLog(LogSeverity.LogInfo, text);
                    break;
                case MyLogType.LogTypeDebug:
                    mPlatInterface.LogDebug(text);
                    BuglyAgent.PrintLog(LogSeverity.LogDebug, text);
                    break;
                case MyLogType.LogTypeEvent:
                    mPlatInterface.LogEvent(text);
                    BuglyAgent.PrintLog(LogSeverity.LogInfo, text);
                    break;
            }
        }

        public void PopWebErrorType(ConnectionErrorType errorType)
        {
            CallPlatformFunc(CallPlatformFuncID.NotificationNameConnectionError, ((byte)errorType).ToString());
        }

        public void CloseWaitingPage()
        {
            BaseScene baseScene = SceneManager.GetInst().GetCurrentScene();
            if (baseScene != null && baseScene.GetType() == typeof(MainScene))
            {
                if (((MainScene)baseScene).MenuType == MainMenuType.Action_Menu)
                {
                    DurationEventEnd(MobClickEventID.app_action_loadhomepage);
                    //SingletonObject<GuideManager>.GetInst().ActiveGuide(GuideTriggerEvent.Action_CloseWaitingPage);
                } else if (((MainScene)baseScene).MenuType == MainMenuType.Program_Menu)
                {
                    //SingletonObject<GuideManager>.GetInst().ActiveGuide(GuideTriggerEvent.Program_CloseWaitingPage);
                }
            }/* else if (!JMSimulatorOnly.IsLessonBuild)
            {
                //SingletonObject<GuideManager>.GetInst().ActiveGuide(GuideTriggerEvent.closeWaitingPage);
            }*/
            else if (SceneMgr.GetCurrentSceneType() == SceneType.Assemble && !JMSimulatorOnly.IsLessonBuild)
            {
                DurationEventEnd(MobClickEventID.app_building_loadhomepage);
            }
            CallPlatformFunc(CallPlatformFuncID.closeWaitingPage, string.Empty);
        }

        public void SetHardwareBack()
        {
            IsInUnity = false;
            mEditFlag = true;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (NeedSetServoFlag == SetHardwareState.None || NeedSetServoFlag == SetHardwareState.SetSuccess)
            {
                dict["result"] = 1;
            } else
            {
                dict["result"] = 0;
            }
            dict["blueState"] = PlatformMgr.Instance.GetBluetoothState() ? 1 : 0;
            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null != robot)
            {
                if (null != robot.MotherboardData)
                {
                    dict["boardType"] = robot.MotherboardData.GetMainboardType();
                }
                dict["servo"] = robot.GetAllDjData().GetIDList();
                if (null != robot.MotorsData && robot.MotorsData.Count > 0)
                {
                    dict["motor"] = robot.MotorsData.GetIds();
                } else
                {
                    dict["motor"] = new List<byte>();
                }
                if (null != robot.MotherboardData)
                {
                    TopologyPartType[] parts = PublicFunction.Open_Topology_Part_Type;
                    for (int i = 0, imax = parts.Length; i < imax; ++i)
                    {
                        SensorData sensorData = robot.MotherboardData.GetSensorData(parts[i]);
                        if (null != sensorData && sensorData.ids.Count > 0)
                        {
                            dict[parts[i].ToString()] = sensorData.ids;
                        }
                    }
                }
            }
            NeedSetServoFlag = SetHardwareState.None;
            CallPlatformFunc(CallPlatformFuncID.setHardwareBack, Json.Serialize(dict));
        }
#endregion

        #region Platform回调Unity
        //蓝牙连接结果
        public void ConnenctCallBack(string str)
        {
            Log(MyLogType.LogTypeEvent, string.Format("ConnenctCallBack = {0}", str));
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    SingletonObject<ConnectCtrl>.GetInst().OnConnectResult(str, false);
                }
                else
                {
                    string name = m_blueMgr.GetNameForMac(str);
                    SingletonObject<ConnectCtrl>.GetInst().OnConnectResult(str, true, name);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

        /// <summary>
        /// 蓝牙音响连接结果
        /// </summary>
        /// <param name="str"></param>
        public void ConnenctSpeakerCallBack(string str)
        {
            mConnectingSpeakerMac = string.Empty;
            if (string.IsNullOrEmpty(str))
            {//连接失败
                Robot robot = null;
                if (RobotManager.GetInst().IsCreateRobotFlag)
                {
                    robot = RobotManager.GetInst().GetCreateRobot();
                }
                else
                {
                    robot = RobotManager.GetInst().GetCurrentRobot();
                }
                if (null != robot && null != robot.MotherboardData)
                {
                    SpeakerData speakerData = (SpeakerData)robot.GetReadSensorData(TopologyPartType.Speaker);
                    if (null != speakerData)
                    {
                        SpeakerInfoData infoData = speakerData.GetSpeakerData();
                        if (null != infoData)
                        {
                            PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("检测到蓝牙音响需要连接"), infoData.speakerName), PopSpeakerOnClick);
                        }
                    }
                }
                
            }
            else
            {
                mConnectedSpeakerMac = str;
            }
        }

        void PopSpeakerOnClick(GameObject obj)
        {
            if (PromptMsg.RightBtnName.Equals(obj.name))
            {
                PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.OpenAndroidBLESetting, string.Empty);
            }
        }

        //当发现蓝牙未匹配过的设备
        public void OnFoundDevice(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {       
                    if (!name.ToLower().Contains("jimu"))
                    {
                        return;
                    }
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("发现的设备 name = {0}", name));
                    m_blueMgr.NewFound(name);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        /// <summary>
        /// SendMsg回包
        /// </summary>
        /// <param name="msg">len + cmd + param</param>
        public void OnMsgAck(string msg)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(msg); //Encoding.UTF8.GetBytes(msg);
                MemoryStream readStream = new MemoryStream(bytes);
                BinaryReader br = new BinaryReader(readStream, Encoding.ASCII);
                int macLen = br.ReadByte();
                byte[] mac = br.ReadBytes(macLen);
                string macStr = Encoding.UTF8.GetString(mac);
                int len = br.ReadByte();
                CMDCode cmd = (CMDCode)br.ReadByte();
                if (cmd != CMDCode.Read_Sensor_Data_Other && cmd != CMDCode.Robot_Update_Write && cmd != CMDCode.Servo_Update_Write && cmd != CMDCode.Sensor_Update_Write)
                {
                    byte[] paramAry = new byte[bytes.Length - 3 - macLen];
                    Array.Copy(bytes, 3 + macLen, paramAry, 0, paramAry.Length);
                    /*for (int i = 0, imax = paramAry.Length; i < imax; ++i)
                    {
                        paramAry[i] = bytes[3 + macLen + i];
                    }*/
                    Log(MyLogType.LogTypeEvent, "mac=" + macStr + ";len=" + len + ";cmd=" + cmd.ToString() + " param = " + PublicFunction.BytesToHexString(paramAry));
                }
#if !Test
                NetWork.GetInst().ReceiveMsg(cmd, len, macStr, br);
#endif
                readStream.Flush();
                readStream.Close();
                br.Close();

            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

        /// <summary>
        /// 进入unity场景
        /// </summary>
        /// <param name="msg"></param>
        public void GotoScene(string msg)
        {
            if (mCurrentModuleType == ModuleType.LessonBuild)
            {
                SingletonObject<LogicCtrl>.GetInst().CleanUp();
            }
            mCurrentModuleType = ModuleType.None;
            if (IsInUnity)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，不能调用gotoScene");
                CloseWaitingPage();
                return;
            }
            DurationEventStart(MobClickEventID.app_action_loadhomepage);
            IsInUnity = true;
            try
            {
                Log(MyLogType.LogTypeEvent, "GotoScene msg = " + msg);
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(msg);
                if (null != data)
                {
                    string modelId = null;
                    string modelName = null;
                    string picPath = null;
                    int modelType = 0;
                    if (data.ContainsKey("modelID"))
                    {
                        modelId = data["modelID"].ToString();
                    }
                    if (data.ContainsKey("modelName"))
                    {
                        modelName = data["modelName"].ToString();
                    }
                    if (data.ContainsKey("picPath"))
                    {
                        picPath = data["picPath"].ToString();
                        Pic_Path = picPath;
                    } else
                    {
                        Pic_Path = string.Empty;
                    }
                    if (data.ContainsKey("edit"))
                    {
                        bool.TryParse(data["edit"].ToString(), out mEditFlag);
                    } else
                    {
                        mEditFlag = true;
                    }
                    if (data.ContainsKey("modelType") && PublicFunction.IsInteger(data["modelType"].ToString()))
                    {
                        modelType = int.Parse(data["modelType"].ToString());
                    }
                    if (data.ContainsKey("actionCameraValidity") && PublicFunction.IsInteger(data["actionCameraValidity"].ToString()))
                    {
                        actionCameraValidity = int.Parse(data["actionCameraValidity"].ToString());
                    }
                    Robot robot = LoadModelData(modelType, modelId, modelName);
                    ResFileType fileType = (ResFileType)modelType;
                    if (null == robot && fileType == ResFileType.Type_playerdata)
                    {//不存在，新建
                        Log(MyLogType.LogTypeEvent, "不存在模型，走新建流程");
                        string namewithtype = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(fileType));
                        RobotManager.GetInst().SetCurrentRobot(null);
                        RobotMgr.Instance.rbtnametempt = RobotMgr.NameWithType(modelId, "playerdata");
                        //RobotManager.GetInst().IsCreateRobotFlag = true;                        
                        if (PlatformMgr.Instance.GetBluetoothState())
                        {
                            PlatformMgr.Instance.DisConnenctBuletooth();
                        }
#if UNITY_IPHONE
                        OpenBluetooth();
#endif
                        //Timer.Add(0.1f, 1, 1, ConnectBluetoothMsg.ShowMsg);
                    }
                }
                MainScene.GotoScene(MainMenuType.Action_Menu);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

        /// <summary>
        /// 拍照返回
        /// </summary>
        /// <param name="msg">图片路径</param>
        public void PhotographBack(string msg)
        {
            Log(MyLogType.LogTypeEvent, "拍照返回 msg = " + msg);
            try
            {
                string oldImg = GetRobotTexturePath();
                Pic_Path = msg;
                EventMgr.Inst.Fire(EventID.Photograph_Back, new EventArg(oldImg));
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        /// <summary>
        /// 下载了新的模型
        /// </summary>
        /// <param name="msg"></param>
        public void DownloadModel(string msg)
        {
            try
            {
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(msg);
                if (null != data)
                {
                    string modelId = null;
                    int modelType = 0;
                    if (data.ContainsKey("modelID"))
                    {
                        modelId = data["modelID"].ToString();
                    }
                    if (data.ContainsKey("modelType") && PublicFunction.IsInteger(data["modelType"].ToString()))
                    {
                        modelType = int.Parse(data["modelType"].ToString());
                    }
                    
                    ResFileType fileType = (ResFileType)modelType;
                    if (fileType == ResFileType.Type_default)
                    {
                        RecordContactInfo.Instance.openType = "default";
                        SingletonBehaviour<GetRobotData>.GetInst().AddMoreFile(fileType, modelId);
                        /*if (!string.IsNullOrEmpty(robotName))
                        {//增加官方动作
                            Robot tmpRobot = RobotManager.GetInst().GetRobotForName(robotName);
                            if (null != tmpRobot)
                            {
                                List<string> actionList = ActionsManager.GetInst().GetActionsIDList(tmpRobot.ID);
                                for (int i = 0, imax = actionList.Count; i < imax; ++i)
                                {
                                    ActionSequence actions = ActionsManager.GetInst().GetActionForID(tmpRobot.ID, actionList[i]);
                                    if (null != actions && actions.IsOfficial())
                                    {
                                        ActionsManager.GetInst().AddOfficial(actionList[i]);
                                    }
                                }
                                ActionsManager.GetInst().SaveOfficialActions();
                            }
                        }*/
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void ChangeRobotShowName(string msg)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "修改模型名字返回" + msg);
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                if (null != robot && ResourcesEx.GetRobotType(robot) == ResFileType.Type_playerdata)
                {
                    robot.ShowName = msg;
                }
                else
                {
                    return;
                }
                /*EventMgr.Inst.Fire(EventID.Change_Robot_Name_Back, new EventArg(msg));
                if (SceneMgr.GetCurrentSceneType() != SceneType.MainWindow)
                {
                    SceneMgr.EnterScene(SceneType.MainWindow);
                }*/
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        /// <summary>
        /// 删除了模型
        /// </summary>
        /// <param name="msg"></param>
        public void DeleteModel(string msg)
        {
            try
            {
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(msg);
                if (null != data)
                {
                    string modelId = null;
                    int modelType = 0;
                    if (data.ContainsKey("modelID"))
                    {
                        modelId = data["modelID"].ToString();
                    }
                    if (data.ContainsKey("modelType") && PublicFunction.IsInteger(data["modelType"].ToString()))
                    {
                        modelType = int.Parse(data["modelType"].ToString());
                    }
                    ResFileType fileType = (ResFileType)modelType;
                    string modelName = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(fileType));
                    Robot robot = RobotManager.GetInst().GetRobotForName(modelName);
                    if (null != robot)
                    {
                        //删除遥控数据
                        ControllerManager.DeletaController(robot.ID);
                    }
                    SingletonBehaviour<GetRobotData>.GetInst().DeleteRobotData(modelName);

                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        /// <summary>
        /// 调用unity里面的方法
        /// </summary>
        /// <param name="msg"></param>
        public void CallUnityFunc(string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(msg) || !msg.Contains("queryAllSensor"))
                {
                    Log(MyLogType.LogTypeDebug, msg);
                }
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(msg);
                if (null != data)
                {
                    string funcName = null;
                    object arg = null;
                    if (data.ContainsKey("funcName"))
                    {
                        funcName = data["funcName"].ToString();
                    }
                    if (data.ContainsKey("arg"))
                    {
                        arg = data["arg"];
                    }
                    if (null != mUnityDelegateDict && mUnityDelegateDict.ContainsKey(funcName) && null != mUnityDelegateDict[funcName])
                    {
                        mUnityDelegateDict[funcName](arg);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        int lastStatus = -1;
        public void PowStatusChange(int status)
        {
            // 电池 状态 0 蓝牙连接中未充电 1 充电中 2电量不足 3 蓝牙断开
            if (lastStatus != status)
            {
                lastStatus = status;
                CallPlatformFunc(CallPlatformFuncID.PowerStatusChange, status.ToString());
            }
        }
        /// <summary>
        /// 断开连接回调
        /// </summary>
        /// <param name="mac"></param>
        public void OnDisConnenct(string mac)
        {
            try
            {
                PowStatusChange(3);
                SingletonObject<ConnectCtrl>.GetInst().DisconnectNotify();
                m_blueMgr.ClearDevice();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        private void downloadFilesNotify(object arg)
        {
            try
            {
                if (null != arg && IsInUnity)
                {
                    Dictionary<string, object> data = (Dictionary<string, object>)arg;
                    string modelId = null;
                    int modelType = 1;
                    int fileType = 1;
                    string modelName = null;
                    if (data.ContainsKey("modelID"))
                    {
                        modelId = data["modelID"].ToString();
                    }
                    if (data.ContainsKey("modelName"))
                    {
                        modelName = data["modelName"].ToString();
                    }
                    if (data.ContainsKey("modelType") && PublicFunction.IsInteger(data["modelType"].ToString()))
                    {
                        modelType = int.Parse(data["modelType"].ToString());
                    }
                    if (data.ContainsKey("fileType") && PublicFunction.IsInteger(data["fileType"].ToString()))
                    {
                        fileType = int.Parse(data["fileType"].ToString());
                    }
                    ResFileType robotType = (ResFileType)modelType;
                    switch (fileType)
                    {
                        case 1://模型
                            if (null == RobotManager.GetInst().GetCurrentRobot())
                            {
                                LoadModelData(modelType, modelId, modelName);//重新加载模型
                                EventMgr.Inst.Fire(EventID.Set_Choice_Robot);
                            }
                            break;
                        case 2://动作
                            {
                                string namewithtype = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(robotType));
                                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                                if (null != robot && string.Equals(namewithtype, robot.Name))
                                {
                                    ActionsManager.GetInst().ReadActions(robot);
                                    EventMgr.Inst.Fire(EventID.Refresh_List);
                                }
                            }
                            break;
                        case 3://程序
                            {
                                string namewithtype = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(robotType));
                                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                                if (null != robot && string.Equals(namewithtype, robot.Name))
                                {
                                    EventMgr.Inst.Fire(EventID.Refresh_List);
                                }
                            }
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
#endregion

        public void RegisterRobotResult(object arg)
        {
            try
            {
                if (null != arg)
                {
                    Dictionary<string, object> dict = (Dictionary<string, object>)arg;
                    int result = 0;
                    string sn = string.Empty;
                    do
                    {
                        if (dict.ContainsKey("sn"))
                        {
                            sn = dict["sn"].ToString();
                        }
                        if (dict.ContainsKey("isSuccess"))
                        {
                            result = PublicFunction.IsInteger(dict["isSuccess"].ToString()) ? int.Parse(dict["isSuccess"].ToString()) : 0;
                            if (1 == result)
                            {
                                break;
                            }
                        }
                        if (dict.ContainsKey("reason"))
                        {
                            int reason = PublicFunction.IsInteger(dict["reason"].ToString()) ? int.Parse(dict["reason"].ToString()) : 0;
                            result = (reason == 2002 ? 1 : 0);
                        }
                    } while (false);
                    if (1 == result)
                    {
                        Robot robot = RobotManager.GetInst().GetCurrentRobot();
                        if (null != robot)
                        {
                            robot.ActivationRobotSuccess();
                            if (!string.IsNullOrEmpty(sn))
                            {
                                robot.WriteSn(sn);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

        private void ModifyServoId(object arg)
        {
            if (IsInUnity)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用ModifyServoId");
                CloseWaitingPage();
                return;
            }
            IsInUnity = true;
            if (SceneManager.GetInst().GetCurrentScene() == null || SceneManager.GetInst().GetCurrentScene().GetType() != typeof(SetScene))
            {
                Timer.Add(0.01f, 1, 1, delegate ()
                {
                    //延迟是为了解决屏幕宽高获取错误的问题
                    SetScene.GotoSetScene(SetSceneType.SetSceneTypeDevice);

                });
            }
            CloseWaitingPage();
        }
       

        private void HardwareProgramVersion(object arg)
        {
            if (null != arg)
            {
                JsonData jsonData = new JsonData(arg);
                bool saveFlag = false;
                for (int i = 0, imax = jsonData.Count; i < imax; ++i)
                {
                    Dictionary<string, object> dict = (Dictionary<string, object>)jsonData[i].Dictionary;
                    string versionName = string.Empty;
                    string versionPath = string.Empty;
                    int versionType = 0;
                    do
                    {
                        if (!dict.ContainsKey("versionType") || !dict.ContainsKey("versionName") || !dict.ContainsKey("versionPath"))
                        {
                            break;
                        }
                        versionType = PublicFunction.IsInteger(dict["versionType"].ToString()) ? int.Parse(dict["versionType"].ToString()) : 0;
                        versionName = dict["versionName"].ToString();
                        versionPath = dict["versionPath"].ToString();
                        TopologyPartType partType = TopologyPartType.None;
                        switch (versionType)
                        {
                            case 3:
                            case 31://老主板
                                partType = TopologyPartType.MainBoard;
                                break;
                            case 4:
                            case 41://舵机
                                partType = TopologyPartType.Servo;
                                break;
                            case 5:
                            case 51://新主板
                                partType = TopologyPartType.MainBoard_new_low;
                                break;
                            case 101:
                            case 102:
                            case 103:
                            case 104:
                            case 105:
                            case 106:
                            case 107:
                            case 108: //传感器
                                partType = TopologyPartData.ConvertToPartType((byte)(versionType % 100));
                                break;                 
                        }
                        if (partType != TopologyPartType.None)
                        {
                            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Name + partType.ToString(), versionName);
#if UNITY_ANDROID
                            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Path + partType.ToString(), versionPath);
#else
                            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Path + partType.ToString(), ResourcesEx.persistentDataPath + "/" + versionPath);
#endif
                            Log(MyLogType.LogTypeEvent, string.Format("set versionType={0} versionName={1}  versionPath={2}", versionType, versionName, versionPath));
                            saveFlag = true;
                        }
                    } while (false);
                }
                if (saveFlag)
                {
                    PlayerPrefs.Save();
                }
            }
            
        }

        private void ClearHardwareProgramVersion(object arg)
        {
            string versionName = null;
            string versionPath = null;
            bool saveFlag = false;
            for (TopologyPartType part = TopologyPartType.Infrared; part < TopologyPartType.Line; ++part)
            {
                versionName = PublicFunction.Hardware_Version_Name + part.ToString();
                versionPath = PublicFunction.Hardware_Version_Path + part.ToString();
                if (PlayerPrefs.HasKey(versionName))
                {
                    PlayerPrefs.DeleteKey(versionName);
                    saveFlag = true;
                }
                if (PlayerPrefs.HasKey(versionPath))
                {
                    PlayerPrefs.DeleteKey(versionPath);
                    saveFlag = true;
                }
            }
            if (saveFlag)
            {
                PlayerPrefs.Save();
            }
            SingletonObject<UpdateManager>.GetInst().ClearDefaultData();
        }

        private void LogicCMDCallUnity(object arg)
        {
            try
            {
                string str = (string)arg;
                if (string.IsNullOrEmpty(str) || !str.Contains("queryAllSensor"))
                {
                    Log(MyLogType.LogTypeEvent, "逻辑编程执行命令 = " + (string)arg);
                }
                LogicCtrl.GetInst().CallUnityCmd(str);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

        private void ExitLogicView(object arg)
        {
            Log(MyLogType.LogTypeEvent, "退出逻辑编程");
            LogicCtrl.GetInst().ExitLogic();
        }
        private void DisConnect(object arg)
        {
            if (isInUnity)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用DisConnect");
                CloseWaitingPage();
                return;
            }
            CloseWaitingPage();
            //DisConnectMsg.ShowMsg(false);
            TopologyBaseMsg.ClearCloserCallback();
            TopologyBaseMsg.ShowInfoMsg(ConnectionTriggerPage.Disconnect, delegate() {
                PlatformMgr.Instance.BackThirdAppImmediately();
            });
        }
        private void OpenBlueSearch(object arg)
        {
            if (IsInUnity)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用OpenBlueSearch");
                CloseWaitingPage();
                return;
            }
            if (arg != null)
            {
                string msg = arg.ToString();
                Log(MyLogType.LogTypeEvent, "OpenBlueSearchg = " + arg);
                if (!string.IsNullOrEmpty(msg))
                {
                    Dictionary<string, object> data = (Dictionary<string, object>)arg;
                    if (null != data)
                    {
                        string modelId = null;
                        string modelName = null;
                        string picPath = null;
                        int modelType = 0;
                        if (data.ContainsKey("modelID"))
                        {
                            modelId = data["modelID"].ToString();
                        }
                        if (data.ContainsKey("modelName"))
                        {
                            modelName = data["modelName"].ToString();
                        }
                        if (data.ContainsKey("picPath"))
                        {
                            picPath = data["picPath"].ToString();
                            Pic_Path = picPath;
                        } else
                        {
                            Pic_Path = string.Empty;
                        }
                        if (data.ContainsKey("modelType") && PublicFunction.IsInteger(data["modelType"].ToString()))
                        {
                            modelType = int.Parse(data["modelType"].ToString());
                        }
                        if (data.ContainsKey("actionCameraValidity") && PublicFunction.IsInteger(data["actionCameraValidity"].ToString()))
                        {
                            actionCameraValidity = int.Parse(data["actionCameraValidity"].ToString());
                        }
                        LoadModelData(modelType, modelId, modelName);
                    }
                }
            }
            Log(MyLogType.LogTypeEvent, "打开蓝牙搜索页面");
#if UNITY_IPHONE
            OpenBluetooth();          
#endif
            Timer.Add(0.15f, 1, 1, delegate() {
                LogicCtrl.GetInst().OpenBlueSearch();
                CloseWaitingPage();
            } );
        }

        private void SetServoModel(object arg)
        {
            
            if (IsInUnity)
            {
                CloseWaitingPage();
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用SetServoModel");
                return;
            }
            Log(MyLogType.LogTypeEvent, "设置舵机模式");
            Timer.Add(0.1f, 1, 1, delegate ()
            {
                CloseWaitingPage();
                SingletonObject<LogicCtrl>.GetInst().OpenSetServoModel();
            });
            
        }

        private void DiyServoSetting(object arg)
        {
            
            if (IsInUnity)
            {
                CloseWaitingPage();
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，不能调用DiyServoSetting");
                return;
            }
            IsInUnity = true;
            try
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)arg;
                if (null != dict)
                {
                    string modelId = null;
                    string modelName = null;
                    if (dict.ContainsKey("modelID"))
                    {
                        modelId = dict["modelID"].ToString();
                    }
                    if (dict.ContainsKey("modelName"))
                    {
                        modelName = dict["modelName"].ToString();
                    }
                    if (dict.ContainsKey("picPath"))
                    {
                        Pic_Path = dict["picPath"].ToString();
                    }
                    else
                    {
                        Pic_Path = string.Empty;
                    }
                    if (dict.ContainsKey("edit"))
                    {
                        bool.TryParse(dict["edit"].ToString(), out mEditFlag);
                    }
                    else
                    {
                        mEditFlag = true;
                    }
                    LoadModelData((int)ResFileType.Type_playerdata, modelId, modelName);
                    NeedSetServoFlag = SetHardwareState.NeedSetServo;
                    Timer.Add(0.1f, 1, 1, delegate () {
                        SetServoTypeMsg.ShowMsg(DiyServoSettingBack);
                        CloseWaitingPage();
                    });
                }
            }
            catch (System.Exception ex)
            {
                CloseWaitingPage();
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        private void DiyServoSettingBack()
        {
            IsInUnity = false;
            mEditFlag = true;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (NeedSetServoFlag == SetHardwareState.SetSuccess)
            {
                dict["result"] = 1;
            } else
            {
                dict["result"] = 0;
            }
            NeedSetServoFlag = SetHardwareState.None;
            CallPlatformFunc(CallPlatformFuncID.diyServoSettingBack, Json.Serialize(dict));
            
        }


        private void ChargeProtectionCallBack(object arg)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "设置充电保护 result = " + arg.ToString());
                int result = int.Parse(arg.ToString());
                if (result >= 1)
                {
                    isChargePlaying = false;
                }
                else
                {
                    isChargePlaying = true;
                }
                PlayerPrefs.SetInt("isChargePlaying", isChargePlaying ? 1 : 0);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        //js异常提示点击按钮返回
        private void JsShowExceptionCallback(object arg)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "js异常提示点击按钮返回" + arg.ToString());
                int result = int.Parse(arg.ToString());
                LogicCtrl.GetInst().JsExceptionOnClick(1 == result ? true : false);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        private void QuitUnity(object arg)
        {
            try
            {
                IsInUnity = false;
                Log(MyLogType.LogTypeEvent, "退出unity");
                SceneMgr.EnterScene(SceneType.EmptyScene);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString()); 
            }
        }

        void BlueAutoConnect(object arg)
        {
            Log(MyLogType.LogTypeEvent, "设置自动连接" + arg.ToString());
            try
            {
                int result = int.Parse(arg.ToString());
                SingletonObject<ConnectCtrl>.GetInst().SetAutoConnectFlag(result == 1 ? true : false);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

        void Screenshots(object arg)
        {
            try
            {
                SingletonBehaviour<Screenshots>.GetInst().SaveScreenshots();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        void GotoModule(object arg)
        {
            try
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)arg;
                if (null != dict)
                {
                    string modelId = null;
                    string modelName = null;
                    string picPath = null;
                    int modelType = 0;
                    ModuleType moduleType = ModuleType.LessonBuild;
                    if (dict.ContainsKey("modelID"))
                    {
                        modelId = dict["modelID"].ToString();
                    }
                    if (dict.ContainsKey("modelName"))
                    {
                        modelName = dict["modelName"].ToString();
                    }
                    if (dict.ContainsKey("picPath"))
                    {
                        picPath = dict["picPath"].ToString();
                        Pic_Path = picPath;
                    } else
                    {
                        Pic_Path = string.Empty;
                    }
                    if (dict.ContainsKey("edit"))
                    {
                        bool.TryParse(dict["edit"].ToString(), out mEditFlag);
                    } else
                    {
                        mEditFlag = true;
                    }
                    if (dict.ContainsKey("modelType") && PublicFunction.IsInteger(dict["modelType"].ToString()))
                    {
                        modelType = int.Parse(dict["modelType"].ToString());
                    }
                    if (dict.ContainsKey("moduleType") && PublicFunction.IsInteger(dict["moduleType"].ToString()))
                    {
                        moduleType = (ModuleType)int.Parse(dict["moduleType"].ToString());
                    }
                    if (dict.ContainsKey("courseId") && PublicFunction.IsInteger(dict["courseId"].ToString()))
                    {
                        courseId = int.Parse(dict["courseId"].ToString());
                    }
                    else
                    {
                        courseId = -1;
                    }
#if UNITY_IPHONE
                        OpenBluetooth();
#endif
                    //ClientMain.GetInst().LoadGameBgTexture();
                    mCurrentModuleType = moduleType;
                    if (moduleType == ModuleType.LessonBuild)
                    {
                        LogicDict = dict;
                        if (IsInUnity)
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用GotoModuleBuild");
                            CloseWaitingPage();
                            return;
                        }
                        JMSimulatorOnly.IsLessonBuild = true;
                        GotoModuleBuild(modelType, modelId, modelName);
                    }
                    else if (moduleType == ModuleType.LessonLogicProgramming)
                    {
                        LogicDict = dict;
                        if (isInUnity)
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用GotoModuleLogicProgramming");
                            CloseWaitingPage();
                            return;
                        }
                        GotoModuleLogicProgramming(modelType, modelId, modelName);
                    }
                    else if (moduleType == ModuleType.Build)
                    {
                        LogicDict = null;
                        if (dict.ContainsKey("from") && PublicFunction.IsInteger(dict["from"].ToString()))
                        {
                            mFromType = (FromPlatform)byte.Parse(dict["from"].ToString());
                        }
                        if (isInUnity)
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用GotoModuleBuild");
                            CloseWaitingPage();
                            return;
                        }
                        DurationEventStart(MobClickEventID.app_building_loadhomepage);
                        GotoModuleBuild(modelType, modelId, modelName);
                    }
                    else if (moduleType == ModuleType.LogicProgramming)
                    {
                        LogicDict = dict;
                        if (IsInUnity)
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用GotoModuleLogicProgramming");
                            CloseWaitingPage();
                            return;
                        }
                        IsInUnity = true;
                        GotoModuleLogicProgramming(modelType, modelId, modelName);
                    } else if (moduleType == ModuleType.Controller)
                    {
                        LogicDict = null;
                        if (isInUnity)
                        {
                            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，无法调用GotoModuleBuild");
                            CloseWaitingPage();
                            return;
                        }
                        GotoModuleController(modelType, modelId, modelName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void BuildTest()
        {
            JMSimulatorOnly.IsLessonBuild = true;
            GotoModuleBuild(0, "TankbotPro", "TankbotPro");
            //PlatformMgr.Instance.OpenBlueSearch("{\"modelID\":\"TankbotPro\",\"modelName\":\"TankbotPro\",\"modelType\":0}");
        }

        Robot LoadModelData(int modelType, string modelId, string modelName)
        {
            DealUserData();
            ResFileType fileType = (ResFileType)modelType;
            if (fileType == ResFileType.Type_playerdata)
            {
                RecordContactInfo.Instance.openType = "playerdata";
            }
            else
            {
                mEditFlag = true;
                RecordContactInfo.Instance.openType = "default";
            }
            if (string.IsNullOrEmpty(modelId))
            {
                RobotManager.GetInst().SetCurrentRobot(null);
                Robot createRobot = RobotManager.GetInst().GetCreateRobot();
                if (null != createRobot)
                {
                    createRobot.ShowName = modelName;
                }
                return null;
            }
            string namewithtype = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(fileType));
            //RobotMgr.Instance.rbtnametempt = namewithtype;
            Robot robot = RobotManager.GetInst().GetRobotForName(namewithtype);
            if (null == robot)
            {
                string robotPath = string.Empty;
                if (fileType == ResFileType.Type_default)
                {
                    robotPath = ResourcesEx.GetCommonPathForNoTypeName(modelId);
                    GetRobotData.GetInst().ReadOneRobot(namewithtype);
                    robot = RobotManager.GetInst().GetRobotForName(namewithtype);
                }
                else
                {
                    robotPath = ResourcesEx.GetRobotPathForNoTypeName(modelId);
                    if (!Directory.Exists(robotPath))
                    {
                        Directory.CreateDirectory(robotPath);
                        PlatformMgr.Instance.SaveModel(modelId);
                    }
                    else
                    {
                        GetRobotData.GetInst().ReadOneRobot(namewithtype);
                        robot = RobotManager.GetInst().GetRobotForName(namewithtype);
                    }
                }
            }
            if (null != robot)
            {
                RobotManager.GetInst().IsCreateRobotFlag = false;
                robot.ShowName = modelName;
                if (fileType == ResFileType.Type_default)
                {
                    SingletonBehaviour<GetRobotData>.GetInst().SelectRobotDefault(modelId);
                }
                else if (fileType == ResFileType.Type_playerdata)
                {
                    SingletonBehaviour<GetRobotData>.GetInst().SelectRobotPlayer(modelId);
                }
                else
                {
                    SingletonBehaviour<GetRobotData>.GetInst().SelectRobotDownload(modelId);
                }
                RobotManager.GetInst().ChoiceRobotForID(robot.ID);
            }
            else
            {
                Robot createRobot = RobotManager.GetInst().GetCreateRobot();
                if (null != createRobot)
                {
                    createRobot.SetRobotMacAndName(createRobot.Mac, namewithtype);
                    createRobot.ShowName = modelName;
                }
                RobotManager.GetInst().SetCurrentRobot(null);
            }
            return robot;
        }

        void GotoModuleBuild(int modelType, string modelId, string modelName)
        {
            IsInUnity = true;
            LogicCtrl.GetInst().CleanUp();
            LoadModelData(modelType, modelId, modelName);
            SceneMgr.EnterScene(SceneType.Assemble);
        }

        void GotoModuleController(int modelType, string modelId, string modelName)
        {
            IsInUnity = true;
            LogicCtrl.GetInst().CleanUp();
            LoadModelData(modelType, modelId, modelName);
            UserdefControllerScene.GotoController(FromModuleType.From_App_Home);
            CloseWaitingPage();
        }

        void GotoModuleLogicProgramming(int modelType, string modelId, string modelName)
        {
            IsInUnity = false;
            LoadModelData(modelType, modelId, modelName);
            SceneMgr.EnterScene(SceneType.EmptyScene);
            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null != robot)
            {
                SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(robot);
            }
            else
            {
                SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(RobotManager.GetInst().GetCreateRobot());
            }
            CloseWaitingPage();
        }

        void SetHardware(object arg)
        {
            if (IsInUnity)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "已处于unity，不能调用SetHardware");
                CloseWaitingPage();
                return;
            }
            IsInUnity = true;
            try
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)arg;
                if (null != dict)
                {
                    string modelId = null;
                    string modelName = null;
                    if (dict.ContainsKey("modelID"))
                    {
                        modelId = dict["modelID"].ToString();
                    }
                    if (dict.ContainsKey("modelName"))
                    {
                        modelName = dict["modelName"].ToString();
                    }
                    if (dict.ContainsKey("picPath"))
                    {
                        Pic_Path = dict["picPath"].ToString();
                    }
                    else
                    {
                        Pic_Path = string.Empty;
                    }
                    if (dict.ContainsKey("edit"))
                    {
                        bool.TryParse(dict["edit"].ToString(), out mEditFlag);
                    } else
                    {
                        mEditFlag = true;
                    }
                    Robot robot = LoadModelData((int)ResFileType.Type_playerdata, modelId, modelName);
                    if (null == robot)
                    {
                        string namewithtype = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(ResFileType.Type_playerdata));
                        RobotManager.GetInst().SetCurrentRobot(null);
                        RecordContactInfo.Instance.openType = "playerdata";
                        RobotMgr.Instance.rbtnametempt = RobotMgr.NameWithType(modelId, "playerdata");
                        RobotManager.GetInst().IsCreateRobotFlag = true;
                        Robot newRobot = RobotManager.GetInst().GetCreateRobot();
                        newRobot.SetRobotMacAndName(newRobot.Mac, namewithtype);
                        newRobot.ShowName = modelName;
                        if (PlatformMgr.Instance.GetBluetoothState())
                        {
                            PlatformMgr.Instance.DisConnenctBuletooth();
                        }
                        robot = newRobot;
                    }
                    NeedSetServoFlag = SetHardwareState.NeedSetServo;
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {
                        TopologyBaseMsg.ClearCloserCallback();
                        TopologyBaseMsg.ShowInfoMsg(ConnectionTriggerPage.SetHardware, SetHardwareBack);
                    } else
                    {
#if UNITY_IPHONE
                        OpenBluetooth();
#endif
                        Timer.Add(0.2f, 1, 1, delegate ()
                        {
                            CloseWaitingPage();
                            SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(robot, SetHardwareBack);
                            /*ConnectBluetoothMsg.ClearCloserCallback();
                            ConnectBluetoothMsg.ShowMsg(SetHardwareBack);*/
                        });
                    }
                }
            }
            catch (System.Exception ex)
            {
                CloseWaitingPage();
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }


        void DealUserData()
        {
            string userId = GetUserData();
            if (!string.IsNullOrEmpty(mLastUser) && !userId.Equals(mLastUser))
            {//切换了账号，清除模型数据
                Log(MyLogType.LogTypeDebug, "切换账号，清除数据");
                CleanUserData();
            }
            mLastUser = userId;
        }

        void CleanUserData()
        {
            try
            {
                if (null != RobotMgr.Instance.rbt)
                {
                    RobotMgr.Instance.rbt.Clear();
                }
                ActionsManager.GetInst().CleanUp();
                RobotManager.GetInst().CleanUp();
                ServosConManager.GetInst().CleanUp();
                SingletonObject<MoveManager>.GetInst().CleanUp();
            }
            catch (System.Exception ex)
            {
                Log(MyLogType.LogTypeEvent, "切换账号时清除数据 error = " + ex.ToString());
            }
        }
    }

    /// <summary>
    /// android或ios平台调用unity函数
    /// </summary>
    public enum CallUnityFuncID
    {
        RegisterRobotResult = 0,//注册机器人结果
        UnitySetupSteeringEngineID = 1,//修改舵机id
        MainboardProgramVersion = 2,//主板升级
        SteeringEngineProgramVersion = 3,//舵机升级
        ChargeProtection = 14,//充电保护
        LogicCMD = 20,//逻辑编程总接口
        ExitLogicView = 21,//退出逻辑编程
        ConnectBLE = 22,//连接蓝牙
        JsShowExceptionCallback = 23,//JS异常提示
        Destroy,//销毁unity
        GiveupVideoState,//放弃视频
        CurrentStartPlaying,//当前状态是开始录制
        //CurrentStopPlaying,//当前状态是停止录制
        CurrentSaveVideo,//当前状态可以保存视频
        ReplayVideoState,//重拍当前预览的视频
        autoConnect,//自动连接开关
        SensorProgramVersion,//传感器版本
        Screenshots,//截图
        setServoMode,//设置舵机模式
        diyServoSetting,//diy舵机设置
        gotoModule,//打开unity模块
        DisConnect,//断开蓝牙
        hardwareProgramVersion,//设置硬件升级文件数据
        clearHardwareProgramVersion,//清除硬件升级文件数据
        playAction,//播放动作
        pauseAction,//暂停动作
        stopAction,//停止动作
        openActionEdit,//打开动作编辑
        setHardware,//硬件设置
        downloadFilesNotify,//下载文件通知
        cancelConnect,//取消连接
    }

    /// <summary>
    /// unity调用各平台函数
    /// </summary>
    public enum CallPlatformFuncID
    {
        ExitSetupSteeringEngineID = 0,//设置舵机界面返回
        OpenLogicProgramming = 1,//打开逻辑编程(arg:模型数据，动作列表)
        LogicCMDResult = 20,//逻辑编程返回总接口
        ConnectBLECallBack = 21,//连接蓝牙返回
        BLEDisconnectNotity,
        CommonTips,
        JsShowException,//js异常信息
        ChargeProtected,//充电保护
        MobClickEvent,//友盟统计
        GetCameraCurrentState,//进入拍摄界面
        GiveupCurrentVideo,//放弃录制的视频
        StartPlayingVideo,//开始录制视频
        StopPlayingVideo,//停止录制视频
        SavedCurrentVideo,//保存当前视频
        GetBluetoothCurrentState,//进入蓝牙连接
        ExitBluetoothCurrentState,//退出蓝牙连接
        JsExceptionWaitResult,//异常等待返回
        ExitPlayVideoMode,//充电保护下注销拍摄视频模式
        OpenAndroidBLESetting,//打开android蓝牙设置
        NotificationNameConnectionError,
        refreshAllServo,//设置舵机返回
        diyServoSettingBack,//diy设置舵机返回
        playActionAudio,//播放动作音效
        backCourseMenu,//返回课程菜单
        closeWaitingPage,//关闭等待页面
        DisConnectBLECallBack,//关闭蓝牙返回
        PowerStatusChange,//电池状态改变
        initRobotData,//初始化模型数据
        setHardwareBack,//硬件设置返回
        deleteProgram,//删除程序
        diyJumpStep,//diy切换步骤
        exitDiyBrowsing,//退出diy浏览模式
        guideFinished,//完成指引
        skipGuide,//跳过指引
        backTo,//返回alphamini
        openBluetoothPage,//打开蓝牙连接页面
        openMatchPage,//打开连线图
        connectCallBack,//连接返回
    }
    /// <summary>
    /// 用户数据
    /// </summary>
    public enum UserDataType : byte
    {
        userId = 0,
        language,
    }

    public enum PlatformDataType : byte
    {
        program = 0,
        diyStepData,//diy步数数据
        guideState,//指引状态
    }
    /// <summary>
    /// 友盟统计事件
    /// </summary>
    public enum MobClickEventID : byte
    {
        /*Connection_TriggerTimes = 0,//蓝牙连接点击量
        RobotPage_ResetRobotPositionButtonTapped,
        RobotPage_EntryTapped,
        RobotPage_TappedActionButton,
        RobotPage_TappedCodingButton,
        RobotPage_TappedControlButton,
        Connection_Times,//蓝牙连接次数
        ModelPage_CreateActionCount,    //新建模型动作数量
        BuildPage_StayDuring,//单次搭建停留时间
        Action_CreatActionTimes,//点击新建动作入口次数
        Action_actionTappedTimes,//官方模型动作点击总量，参数为模型名称_动作名称
        Action_actionSavedTimes,//保存动作次数
        Action_actionCopyTimes,//复制动作帧次数
        Action_actionPasteTimes,//粘贴动作帧次数
        Action_actionRecordButtonTappedTimes,//点击回读按钮次数
        Action_actionPreviewButtonTappedTimes,//点击预览动作次数
        Action_ationQuitWithoutSavingTimes,//不保存动作退出次数
        Action_ationPageStayDuring,//编程页面停留时长
        Action_ationCameraButtonTappedTimes,//点击动作相机按钮次数      
        blockly_blocklyButtonTappedTimesPerRobot,//某个官方模型编程按钮点击次数，参数为模型名称
        Console_setConsoleTimes,//设置遥控器次数
        Console_pageStayDuring,//遥控器页面停留时间
        Connection_DuringToSuccess,//蓝牙连接成功时长
        Connection_TriggerPage,//蓝牙连接位置
        Connection_DuringWhenUsing,//单次蓝牙连接时长
        //修改id改为TopologyPartType_ModifyID_Times
        ControlBox_Upgrad,
        Sensor_ErrorStatu,//传感器异常状态
        //升级改为TopologyPartType_Upgrad
        DIYRobot_actionQuantityPerRobot,*/
        app_action_loadhomepage,//加载动作列表主页
        app_building_loadhomepage,//加载搭建主页
        app_building_complete,//完成模型搭建
        app_building_downloadmodel,//进入搭建时上报官方模型的加载结果
        app_bluetooth_connect,//连接蓝牙结果
        P6,//搭建页面
        click_P6_1,//U3D模型滑动按钮
        click_P6_2,//进度条拖控按钮
        click_P6_3,//前进按钮
        click_P6_4,//后退按钮
        P7,//动作页面
        click_P7_1,//添加动作按钮
        click_P7_2,//蓝牙连接按钮
        click_P7_3,//遥控器按钮
        P8,//动作设计页面
        P9,//遥控器页面
        click_P9_1,//遥控器-设置按钮
    }

    /// <summary>
    /// 统计事件类型
    /// </summary>
    public enum MobEventType : byte
    {
        measure_page_time_start = 0,//进入页面
        measure_page_time_end,//退出页面
        record_event,//事件统计
    }

    /// <summary>
    /// 蓝牙连接失败原因
    /// </summary>
    public enum BlueConnectFailReason : byte
    {
        BluetoothFail = 0,   //连接蓝牙失败；
        ReadDeviceFail,  //读取设备类型失败；
        ReadControlboxInfoFail,  //读取主板信息失败；
        ModelInfoIncorrect,  //模型数据不匹配；
        ModelInfoIncorrect_differentID,  //ID不一致
        ModelInfoIncorrect_servoAmount,  //舵机数量不一致
        ModelInfoIncorrect_repeatedID,   //ID重复
        MotorIncorrect_repeatedID,//马达id重复
        SensorIncorrect_repeatedID,//传感器ID重复
        MotorIncorrect_amount,//马达数量不一致
        MotorIncorrect_differentID,//马达id不匹配
        UnableUpgrade,   //因各种原因不能升级而导致失败；
        Cancel,  //用户取消连接；
        Disconnect,  //蓝牙异常断开导致的失败(主板不回消息)
        LowPower,    //低电量
        unknow  //未知异常
    }

    /// <summary>
    /// 蓝牙连接页面
    /// </summary>
    public enum ConnectionTriggerPage : byte
    {
        None = 0,
        Build_Page,//搭建
        Model_Page,//模型页
        Action_Page,//动作页
        Action_Coding_Page,//动作编程
        Coding_Page,//逻辑编程页
        Controls_Page,//遥控页
        ModifyID_Page,//修改舵机id
        CreateModel_Page,//创建模型
        Camera_Page,//动作相机
        Disconnect,//断开连接
        SetHardware,//硬件设置
    }

    public enum UpgradFailReson : byte
    {
        Sending_Receive_Failed,//发送过程中主板返回失败，返回01
        Sending_Write_Failed,//发送过程中写入失败
        Sending_Space_NotEnough,//主板空间不足
        Sending_FileName_Error,//文件名过长
        Sending_Timeout,//发送过程中未回包
        Upgrade_Timeout,//升级过程中未回包
        Upgrade_new_Failed,//新主板升级失败
        Upgrade_Start_Failed,//开始升级失败，返回01
        Upgrade_Start_Revert_Failed,//开始升级还原失败，返回EE
        Upgrade_End_Failed,//结束升级失败，返回01
        Upgrade_End_Revert_Failed,//结束升级还原失败,返回EE
        Upgrade_Failed,//舵机、传感器升级失败
        Switch_Background//切换到后台
    }

    public enum MyLogType : byte
    {
        LogTypeEvent = 0,//重要日志
        LogTypeInfo,//一般日志
        LogTypeDebug,//调试日志
    }
    /// <summary>
    /// 连接异常
    /// </summary>
    public enum ConnectionErrorType : byte
    {
        ConnectionUnknowErrorType = 0,
        ConnectionSearchJimuType = 1,
        ConnectionServoIdRepeatType = 2,
        ConnectionServoVSLineType = 3,
        ConnectionServoNumVsLineType = 4,
        ConnectionServoLineErrorType = 5,
        ConnectionFirmwareUpdateErrorType = 6,
        ConnectionSystemHelp = 7,
        ConnectionFailed = 8,
    }
    /// <summary>
    /// 模块名字
    /// </summary>
    public enum ModuleType : byte
    {
        LessonBuild = 0,
        LessonLogicProgramming,
        Build,
        LogicProgramming,
        Controller,

        None = 99,
    }


    public enum SetHardwareState : byte
    {
        None = 0,
        NeedSetServo,
        SetSuccess,
        SetFailure,
    }

    public enum FromPlatform : byte
    {
        Self = 0,
        Alpha_mini = 1,
    }
}

