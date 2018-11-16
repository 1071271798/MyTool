using Game.Platform;
using System;
using System.Collections.Generic;
using LitJson;
using System.Text;
using Game.Resource;
using Game.Scene;
using Game;
using UnityEngine;
using Game.Event;
/// <summary>
/// 逻辑编程
/// </summary>
public class LogicCtrl : SingletonObject<LogicCtrl>
{
    public delegate void ExceptionDelegate(bool confirmFlag);

    public delegate void LogicCmdCallBack(string result);


    delegate void CallUnityCmdDelegate(string[] args);

    class WaitCmdData
    {
        public string[] args;
        public int waitMs;
        long startTime;

        public WaitCmdData(int waitTime, string[] args)
        {
            this.args = args;
            this.waitMs = waitTime;
            startTime = PublicFunction.GetNowMillisecond();
        }

        public bool IsTimeOut()
        {
            if (PublicFunction.GetNowMillisecond() - startTime >= 0)
            {
                return true;
            }
            return false;
        }
    }

    public LogicCmdCallBack logicCmdCallBack = null;

    /// <summary>
    /// 是否是处于逻辑编程通讯
    /// </summary>
    bool isLogicProgramming = false;
    public bool IsLogicProgramming
    {
        get { return isLogicProgramming; }
        set { isLogicProgramming = value; }
    }
    /// <summary>
    /// 由逻辑编程打开了蓝牙连接
    /// </summary>
    bool isLogicOpenSearchFlag = false;
    public bool IsLogicOpenSearchFlag
    {
        get { return isLogicOpenSearchFlag; }
        set { isLogicOpenSearchFlag = value; }
    }
    Dictionary<string, CallUnityCmdDelegate> mCallCmdDict;
#if UNITY_ANDROID
    public readonly string Logic_Cmd_Start = "jimu://";
#else
    public readonly string Logic_Cmd_Start = "https://js.jimu.com/";
#endif

    /// <summary>
    /// 正在执行的命令
    /// </summary>
    Dictionary<string, string[]> mRuningCmdDict;
    /// <summary>
    /// 正在等待执行的命令
    /// </summary>
    Dictionary<string, WaitCmdData> mWaitCmdDict;

    Robot mRobot;

    float mShowTipsLastTime = 0;
    Dictionary<string, float> mShowTipsLastTimeDict = null;

    bool mServoSetCallBackFlag = false;
    bool mServoSetFinishedFlag = false;
    long mWaitServoSetIndex = -1;

    bool mChargeProtectedFlag = false;//充电保护提示

    ExceptionDelegate mExceptionCallBack = null;//异常处理
    bool mExceptionTipsFlag = false;
    bool mWaitExceptionRepairFlag = false;

    int mSetLEDCount = 0;
    int mSetUltrasonicCount = 0;
    int mSetEmojiCount = 0;
    int mSetDigitalTubeCount = 0;
    int mCtrlServoTurnCount = 0;
    int mSetRgbLightColorCount = 0;
    int mSetRgbLightEmojiCount = 0;
    public int CtrlServoTurnCount
    {
        get { return mCtrlServoTurnCount; }
        set { mCtrlServoTurnCount = value; }
    }

    public LogicCtrl()
    {
        mCallCmdDict = new Dictionary<string, CallUnityCmdDelegate>();
        mCallCmdDict[LogicCmd.query.ToString()] = LogicQuery;
        mCallCmdDict[LogicCmd.action.ToString()] = LogicAction;
        mCallCmdDict[LogicCmd.servoPowerOn.ToString()] = LogicServoPowerOn;
        mCallCmdDict[LogicCmd.servoPowerOff.ToString()] = LogicServoPowerOff;
        mCallCmdDict[LogicCmd.servoSet.ToString()] = LogicServoSet;
        mCallCmdDict[LogicCmd.servoSetbySpeed.ToString()] = LogicServoSetbySpeed;
        mCallCmdDict[LogicCmd.stopServo.ToString()] = LogicStopServo;
        mCallCmdDict[LogicCmd.getPosture.ToString()] = LogicGetPosture;
        mCallCmdDict[LogicCmd.stopRobot.ToString()] = LogicStopRobot;
        mCallCmdDict[LogicCmd.adjustServo.ToString()] = LogicAdjustServo;
        mCallCmdDict[LogicCmd.DisconnectBLE.ToString()] = LogicDisConnenct;
        mCallCmdDict[LogicCmd.queryInfrared.ToString()] = LogicQueryInfrared;
        mCallCmdDict[LogicCmd.queryTouchStatus.ToString()] = LogicQueryTouchStatue;
        mCallCmdDict[LogicCmd.queryGyroscope.ToString()] = LogicQueryGyroscope;
        mCallCmdDict[LogicCmd.setLEDs.ToString()] = LogicSetLEDs;
        mCallCmdDict[LogicCmd.setGroupLEDs.ToString()] = LogicSetGroupLEDs;
        mCallCmdDict[LogicCmd.setGroupEmoji.ToString()] = LogicsetGroupEmoji;
        mCallCmdDict[LogicCmd.setEmoji.ToString()] = LogicSetEmoji;
        mCallCmdDict[LogicCmd.setDigitalTube.ToString()] = LogicSetDigitalTube;
        mCallCmdDict[LogicCmd.queryAllSensor.ToString()] = LogicQueryAllSensor;
        mCallCmdDict[LogicCmd.setSensorLED.ToString()] = LogicCtrlSensorLED;
        mCallCmdDict[LogicCmd.playAction.ToString()] = LogicPlayAction;
        mCallCmdDict[LogicCmd.stopMotion.ToString()] = LogicStopMotion;
        mCallCmdDict[LogicCmd.setMotor.ToString()] = LogicSetMotor;
        mCallCmdDict[LogicCmd.stopMotor.ToString()] = LogicStopMotor;
        mCallCmdDict[LogicCmd.setUltrasonic.ToString()] = LogicSetUltrasonic;
        mCallCmdDict[LogicCmd.queryRobotStatus.ToString()] = LogicQueryRobotStatus;
        mCallCmdDict[LogicCmd.setUnicornLightOn.ToString()] = LogicSetUnicornLightOn;
        mCallCmdDict[LogicCmd.setUnicornEmoji.ToString()] = LogicSetUnicornEmoji;
    }
    public void CallUnityCmd(string argStr)
    {
        do 
        {
            if (!mWaitExceptionRepairFlag && argStr.StartsWith(Logic_Cmd_Start))
            {
                argStr = argStr.Substring(Logic_Cmd_Start.Length);
                string[] args = argStr.Split('|');
                if (null != args)
                {
                    string cmd = args[0];
                    if (mCallCmdDict.ContainsKey(cmd) && null != mCallCmdDict[cmd])
                    {
                        if (null == mRuningCmdDict)
                        {
                            mRuningCmdDict = new Dictionary<string, string[]>();
                        }
                        if (mRuningCmdDict.ContainsKey(cmd) && (null == mWaitCmdDict || !mWaitCmdDict.ContainsKey(cmd) || !mWaitCmdDict[cmd].IsTimeOut()))
                        {//同个执行需等待
                            if (null == mWaitCmdDict)
                            {
                                mWaitCmdDict = new Dictionary<string, WaitCmdData>();
                            }
                            //若有同种命令等待，则已最新的状态更新
                            int waitTime = 0;
                            if (cmd.Equals(LogicCmd.stopRobot.ToString()))
                            {
                                waitTime = 1;
                            }
                            else if (cmd.Equals(LogicCmd.queryAllSensor.ToString()) || cmd.Equals(LogicCmd.queryTouchStatus.ToString()) || cmd.Equals(LogicCmd.queryInfrared.ToString()) || cmd.Equals(LogicCmd.queryGyroscope.ToString()))
                            {
                                waitTime = 1000;
                            }
                            WaitCmdData waitData = new WaitCmdData(waitTime, args);
                            mWaitCmdDict[cmd] = waitData;
                        }
                        else
                        {
                            if (null != mWaitCmdDict && mWaitCmdDict.ContainsKey(cmd))
                            {
                                mWaitCmdDict.Remove(cmd);
                            }
                            mRuningCmdDict[cmd] = args;
                            mCallCmdDict[cmd](args);
                        }
                        return;
                    }
                }
            }
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["result"] = CallUnityResult.failure.ToString();
            dict["cmd"] = argStr;
            string jsonbill = Json.Serialize(dict);
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.LogicCMDResult, jsonbill);
        } while (false);
    }


    /// <summary>
    /// 打开逻辑编程
    /// </summary>
    public void OpenLogicForRobot(Robot robot, Dictionary<string, object> program = null)
    {
        try
        {
            CleanUp();
            PlatformMgr.Instance.IsInUnity = false;
            IsLogicProgramming = true;
            mRobot = robot;
            
            string servoList = PublicFunction.ListToString<byte>(robot.GetAllDjData().GetAngleList());
            string circleServos = PublicFunction.ListToString<byte>(robot.GetAllDjData().GetTurnList());

            Dictionary<string, object> dict = new Dictionary<string, object>();
            AddActionData(ref dict);
            AddMotionData(ref dict);
            ResFileType type = ResourcesEx.GetResFileType(RobotMgr.DataType(robot.Name));
            dict["modelID"] = RobotMgr.NameNoType(mRobot.Name);
            dict["modelType"] = ((int)type).ToString();
            dict["servo"] = servoList;
            dict["circleServos"] = circleServos;
            dict["motor"] = robot.MotorsData == null ? string.Empty : PublicFunction.ListToString(robot.MotorsData.GetIds());
            dict["isLithium"] = ReadPowerMsgAck.isLithium ? 1 : 0;
            AddSensorData(ref dict);
            if (null != PlatformMgr.Instance.LogicDict)
            {
                foreach (var kvp in PlatformMgr.Instance.LogicDict)
                {
                    if (!dict.ContainsKey(kvp.Key))
                    {
                        dict[kvp.Key] = kvp.Value;
                    }
                }
            }
            dict["program"] = program;
            dict["edit"] = PlatformMgr.Instance.EditFlag;
            string jsonbill = Json.Serialize(dict);
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.OpenLogicProgramming, jsonbill);

            if (PlatformMgr.Instance.IsChargeProtected)
            {
                Timer.Add(3, 1, 1, delegate () {
                    if (PlatformMgr.Instance.IsChargeProtected)
                    {
                        LogicCtrl.GetInst().ChargeProtectedCallBack();
                    }
                });
            }
            else
            {
                //PlatformMgr.Instance.DurationEventStart(MobClickEventID.Action_ationPageStayDuring);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void AddSensorData(ref Dictionary<string, object> dict)
    {
        string infraredId = string.Empty;
        string touchId = string.Empty;
        string gyroscopeId = string.Empty;
        string lights = string.Empty;
        string ultrasonic = string.Empty;
        string colorId = string.Empty;
        string unicornId = string.Empty;
        if (null != mRobot && null != mRobot.MotherboardData)
        {
            if (null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Infrared))
            {
                infraredId = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Infrared).ids);
            }
            if (null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Touch))
            {
                touchId = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Touch).ids);
            }
            if (null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Gyro))
            {
                gyroscopeId = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Gyro).ids);
            }
            if (null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Light))
            {
                lights = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Light).ids);
            }
            if (null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Ultrasonic))
            {
                ultrasonic = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Ultrasonic).ids);
            }
            if (null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Color))
            {
                colorId = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Color).ids);
            }
            if (null != mRobot.MotherboardData.GetSensorData(TopologyPartType.RgbLight))
            {
                unicornId = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.RgbLight).ids);
            }
        }
        dict["infraredId"] = infraredId;
        dict["touchId"] = touchId;
        dict["gyroscopeId"] = gyroscopeId;
        dict["lights"] = lights;
        dict["ultrasonicId"] = ultrasonic;
        dict["colorId"] = colorId;
        dict["unicornId"] = unicornId;
    }

    void AddActionData(ref Dictionary<string, object> dict)
    {
        StringBuilder sb = new StringBuilder();
        if (null != mRobot)
        {
            List<string> actList = mRobot.GetActionsIdList();
            for (int i = 0, imax = actList.Count; i < imax; ++i)
            {
                ActionSequence actions = mRobot.GetActionsForID(actList[i]);
                if (null != actions)
                {
                    if ((PlatformMgr.Instance.CurrentModuleType == ModuleType.LessonLogicProgramming) && !actions.IsOfficial())
                    {
                        continue;
                    }
                    if (sb.Length > 0)
                    {
                        sb.Append(PublicFunction.Separator_Or);
                    }
                    sb.Append(actions.Id);
                    sb.Append(PublicFunction.Separator_Comma);
                    if (actions.IsOfficial())
                    {
                        sb.Append(actions.Name.ToLower());
                    }
                    else
                    {
                        sb.Append(actions.Name);
                    }
                    
                    sb.Append(PublicFunction.Separator_Comma);
                    sb.Append(actions.IconName);
                    sb.Append(PublicFunction.Separator_Comma);
                    if (!string.IsNullOrEmpty(actions.ActionName) && actions.IsOfficial())
                    {
                        if (PublicFunction.CheckStrChinessReg(actions.ActionName))
                        {
                            int num = 0;
                            byte[] bytes = Encoding.UTF8.GetBytes(actions.ActionName);
                            if (null != bytes)
                            {
                                foreach (byte item in bytes)
                                {
                                    num += item;
                                }
                            }
                            sb.Append(PublicFunction.GetPinyin(actions.ActionName).ToLower() + num);
                        }
                        else
                        {
                            sb.Append(actions.ActionName);
                        }
                    } else
                    {
                        sb.Append(actions.Id);
                    }
                }
            }
        }
        
        dict["action"] = sb.ToString();
    }

    void AddMotionData(ref Dictionary<string, object> dict)
    {
        Dictionary<String, List<Dictionary<string, string>>> list = new Dictionary<String, List<Dictionary<string, string>>>();
        if (null != mRobot)
        {
            List<MoveType> moves = SingletonObject<MoveManager>.GetInst().GetRobotMoveList(mRobot.ID);
            if (null != moves)
            {
                for (int i = 0, imax = moves.Count; i < imax; ++i)
                {
                    Dictionary<string, string> motion = new Dictionary<string, string>();
                    motion["id"] = moves[i].ToString();
                    motion["name"] = MoveManager.GetMoveTypeName(moves[i]).ToLower();
                    motion["icon"] = MoveManager.GetMoveIconName(moves[i]);
                    MoveSequenceType seqType = SingletonObject<MoveManager>.GetInst().GetSequenceType(mRobot, moves[i]);
                    if (seqType != MoveSequenceType.undefined)
                    {
                        string seqStr = seqType.ToString();
                        if (seqType == MoveSequenceType.time_byMotor || seqType == MoveSequenceType.time_byServo)
                        {
                            seqStr = "time";
                        }
                        if (!list.ContainsKey(seqStr))
                        {
                            List<Dictionary<string, string>> tmp = new List<Dictionary<string, string>>();
                            list[seqStr] = tmp;
                        }
                        list[seqStr].Add(motion);
                    }
                }              
            }
        }
        dict["motion"] = Json.Serialize(list);
    }

    public void ExitLogic()
    {
        CleanUp();
        /*if (!PlatformMgr.Instance.IsCourseFlag)
        {
            PlatformMgr.Instance.IsInUnity = true;
            MainScene.GotoScene(MainMenuType.Program_Menu);
        }*/
        SingletonObject<ColorCollectTool>.GetInst().ApplicationQuit();
    }

    public void TestExitLogic()
    {
        CleanUp();
    }

    public void OpenBlueSearch()
    {
        if (null != mRuningCmdDict)
        {
            mRuningCmdDict.Clear();
        }
        if (null != mWaitCmdDict)
        {
            mWaitCmdDict.Clear();
        }
        isLogicOpenSearchFlag = true;
        PlatformMgr.Instance.IsInUnity = true;
        mRobot = RobotManager.GetInst().GetCurrentRobot();
        SingletonObject<CheckLogicPage>.GetInst().OpenCheck(CheckLogicPage.CheckType.Blue_Search);
        ConnectBluetoothMsg.ShowMsg();
        SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(mRobot, CloseBlueSearch);
    }

    public void CloseBlueSearch()
    {
        SingletonObject<CheckLogicPage>.GetInst().CloseCheck();
        PopWinManager.GetInst().CloseAll();
        if (!PopWinManager.GetInst().IsExist(typeof(TopologyBaseMsg)) && !PopWinManager.GetInst().IsExist(typeof(ConnectBluetoothMsg)))
        {
            EventMgr.Inst.Fire(EventID.Exit_Blue_Connect);
        }
        //防止在unity连接模型未切换模型数据
        mRobot = RobotManager.GetInst().GetCurrentRobot();
        do 
        {
            if (isLogicOpenSearchFlag)
            {
                PlatformMgr.Instance.IsInUnity = false;
                isLogicOpenSearchFlag = false;
                ShowTexture.ClearTexture2D();
                SingletonBehaviour<ResourcesLoad>.GetInst().UnLoad(typeof(Texture));
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    NetWaitMsg.ShowWait(1);
                    Timer.Add(1, 1, 1, delegate ()
                    {//防止逻辑编程立即点运行，造成消息队列无法及时处理
                        ConnectBLECallBack();
                        if (PlatformMgr.Instance.IsChargeProtected)
                        {
                            LogicCtrl.GetInst().ChargeProtectedCallBack();
                        }
                    });
                } else
                {
                    ConnectBLECallBack();
                    if (PlatformMgr.Instance.IsChargeProtected)
                    {
                        LogicCtrl.GetInst().ChargeProtectedCallBack();
                    }
                }
                break;
            }
            else if (PlatformMgr.Instance.IsCourseFlag)
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    PlatformMgr.Instance.BackCourseMenu();
                }
            }
        } while (false);
        
    }

    public void ConnectBLECallBack()
    {
        if (null == mRobot)
        {
            mRobot = RobotManager.GetInst().GetCurrentRobot();
        }
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["blueState"] = (PlatformMgr.Instance.GetBluetoothState() ? 1 : 0).ToString();
        dict["isLithium"] = ReadPowerMsgAck.isLithium ? 1 : 0;
        AddSensorData(ref dict);
        string jsonbill = Json.Serialize(dict);
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ConnectBLECallBack, jsonbill);
    }


    public void OpenSetServoModel()
    {
        PlatformMgr.Instance.IsInUnity = true;
        SetServoTypeMsg.ShowMsg(CloseSetServoModel);
        SingletonObject<CheckLogicPage>.GetInst().OpenCheck(CheckLogicPage.CheckType.Set_Servo);
    }

    public void CloseSetServoModel()
    {
        SingletonObject<CheckLogicPage>.GetInst().CloseCheck();
        PlatformMgr.Instance.IsInUnity = false;
        string servoList = string.Empty;
        if (null != mRobot)
        {
            servoList = PublicFunction.ListToString<byte>(mRobot.GetAllDjData().GetAngleList(), PublicFunction.Separator_Or);
        }
        string circleServos = string.Empty;
        if (null != mRobot)
        {
            circleServos = PublicFunction.ListToString<byte>(mRobot.GetAllDjData().GetTurnList(), PublicFunction.Separator_Or);
        }
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict["commonServo"] = servoList;
        dict["circleServo"] = circleServos;
        string jsonbill = Json.Serialize(dict);
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.refreshAllServo, jsonbill);
        /*if (PlatformMgr.Instance.IsChargeProtected)
        {
            LogicCtrl.GetInst().ChargeProtectedCallBack();
        }*/
    }

    public void NotifyLogicDicBlue()
    {
        mChargeProtectedFlag = false;
        mWaitExceptionRepairFlag = false;
        if (IsLogicProgramming)
        {
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.BLEDisconnectNotity, LogicLanguage.GetText("蓝牙断开"));
        }
    }
    /// <summary>
    /// 异常信息返回
    /// </summary>
    /// <param name="exceptionString"></param>
    public void ExceptionCallBack(string exceptionString, ExceptionDelegate exDlgt)
    {
        if (IsLogicProgramming && !mExceptionTipsFlag)
        {
            mExceptionTipsFlag = true;
            mWaitExceptionRepairFlag = true;
            mExceptionCallBack = exDlgt;
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.JsShowException, exceptionString);
            if (!SingletonBehaviour<ClientMain>.GetInst().useThirdAppFlag)
            {
                PromptMsg.ShowDoublePrompt(exceptionString, SelfCheckErrorOnClick);
            }
        }
    }

    void SelfCheckErrorOnClick(GameObject obj)
    {
        string btnName = obj.name;
        if (btnName.Equals(PromptMsg.LeftBtnName))
        {
            JsExceptionOnClick(false);
        }
        else if (btnName.Equals(PromptMsg.RightBtnName))
        {
            JsExceptionOnClick(true);
        }
    }

    public void JsExceptionOnClick(bool confirmFlag)
    {
        mExceptionTipsFlag = false;
        if (null != mExceptionCallBack)
        {
            if (!confirmFlag)
            {
                mWaitExceptionRepairFlag = false;
            }
            mExceptionCallBack(confirmFlag);
            mExceptionCallBack = null;
        }
    }
    /// <summary>
    /// 普通提示，不做其他处理
    /// </summary>
    /// <param name="tips"></param>
    public void CommonTipsCallBack(string tips, float intervalTime, CommonTipsColor color)
    {
        if (!ClientMain.Use_Third_App_Flag)
        {
            switch (color)
            {
                case CommonTipsColor.red:
                    HUDTextTips.ShowTextTip(tips, HUDTextTips.Color_Red, intervalTime);
                    break;
                case CommonTipsColor.green:
                    HUDTextTips.ShowTextTip(tips, HUDTextTips.Color_Green, intervalTime);
                    break;
                case CommonTipsColor.yellow:
                    HUDTextTips.ShowTextTip(tips, HUDTextTips.Color_Yellow, intervalTime);
                    break;
            }
        }
        if (null != mShowTipsLastTimeDict && mShowTipsLastTimeDict.ContainsKey(tips) && Time.time - mShowTipsLastTimeDict[tips] < intervalTime)
        {
            return;
        }
        /*if (0 != mShowTipsLastTime && Time.time - mShowTipsLastTime < intervalTime)
        {
            return;
        }*/
        //mShowTipsLastTime = Time.time;
        if (null == mShowTipsLastTimeDict)
        {
            mShowTipsLastTimeDict = new Dictionary<string, float>();
        }
        mShowTipsLastTimeDict[tips] = Time.time;
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["msg"] = tips;
        dict["level"] = (byte)color;
        string jsonbill = Json.Serialize(dict);
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.CommonTips, jsonbill);
    }
    /// <summary>
    /// 充电保护
    /// </summary>
    public void ChargeProtectedCallBack()
    {
        if (IsLogicProgramming && !isLogicOpenSearchFlag)
        {
            if (!mChargeProtectedFlag)
            {
                mChargeProtectedFlag = true;
                PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ChargeProtected, LogicLanguage.GetText("禁止边充边玩"));
            }
        }
    }


    public override void CleanUp()
    {
        base.CleanUp();
        mShowTipsLastTime = 0;
        if (null != mShowTipsLastTimeDict)
        {
            mShowTipsLastTimeDict.Clear();
        }
        IsLogicProgramming = false;
        mExceptionTipsFlag = false;
        mWaitExceptionRepairFlag = false;
        mExceptionCallBack = null;
        if (null != mRuningCmdDict)
        {
            mRuningCmdDict.Clear();
        }
        if (null != mWaitCmdDict)
        {
            mWaitCmdDict.Clear();
        }
        mChargeProtectedFlag = false;
    }

    public void CleanMsg()
    {
        if (isLogicProgramming)
        {
            if (null != mRuningCmdDict)
            {
                mRuningCmdDict.Clear();
            }
            if (null != mWaitCmdDict)
            {
                mWaitCmdDict.Clear();
            }
            NetWork.GetInst().ClearCacheMsg();
        }
    }

    #region 逻辑编程调用unity命令
    /// <summary>
    /// 查询命令
    /// </summary>
    /// <param name="args">Model/Servo</param>
    void LogicQuery(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                if (!PlatformMgr.Instance.GetBluetoothState())
                {
                    result["state"] = CallUnityResult.failure;
                    result["result"] = 0;
                }
                else
                {
                    Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(args[1]);
                    do
                    {
                        result["state"] = CallUnityResult.success;
                        result["result"] = 1;
                        if (null == data)
                        {
                            result["result"] = 0;
                            break;
                        }
                        if (data.ContainsKey("model"))
                        {
                            if (data["model"].ToString().Equals("true") && !PlatformMgr.Instance.PowerData.IsPowerLow())
                            {
                                result["result"] = 0;
                            }
                        }
                        if ((int)result["result"] == 1 && data.ContainsKey("servos"))
                        {//查询舵机掉电，如果舵机为空则查询所有舵机上电
                            string servos = data["servos"].ToString();
                            if (string.IsNullOrEmpty(servos))
                            {//查询所有舵机上电
                                ModelDjData servoData = mRobot.GetAllDjData();
                                foreach (KeyValuePair<byte, DuoJiData> kvp in servoData.GetAllData())
                                {
                                    if (kvp.Value.isPowerDown)
                                    {
                                        result["result"] = 0;
                                        break;
                                    }
                                }
                            }
                            else
                            {//查询舵机掉电
                                List<int> list = PublicFunction.StringToList(servos);
                                for (int i = 0, imax = list.Count; i < imax; ++i)
                                {
                                    DuoJiData servoData = mRobot.GetAnDjData(list[i]);
                                    if (!servoData.isPowerDown)
                                    {
                                        result["result"] = 0;
                                        break;
                                    }
                                }
                            }
                        }
                    } while (false);
                }
                string resultStr = Json.Serialize(result);
                CmdMoreCallBack(args[0], resultStr, CallUnityErrorCode.Result_None);
                /*if (args[1].Equals("Model"))
                {
                    GetPowerState();
                }
                else if (args[1].Equals("Servo"))
                {
                    GetServoPowerState();
                }*/
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_OK);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    /// <summary>
    /// 获取电量状态
    /// </summary>
    void GetPowerState()
    {
        CallUnityErrorCode cmdResult = CallUnityErrorCode.Result_OK;
        if (!PlatformMgr.Instance.GetBluetoothState())
        {
            cmdResult = CallUnityErrorCode.Blue_DisConnect;
        }
        CallUnityResult result;
        if (PlatformMgr.Instance.PowerData.isAdapter)
        {//接了适配器
            if (PlatformMgr.Instance.PowerData.power > PublicFunction.Robot_Power_Max)
            {
                result = CallUnityResult.highPower;
            }
            else
            {
                result = CallUnityResult.highPower;
            }
        }
        else
        {
            if (PlatformMgr.Instance.PowerData.power < PublicFunction.Robot_Power_Min)
            {
                result = CallUnityResult.lowPower;
            }
            else
            {
                result = CallUnityResult.highPower;
            }
        }
        CmdCallBack(LogicCmd.query.ToString(), result, cmdResult);
    }

    /// <summary>
    /// 获取舵机掉电状态
    /// </summary>
    void GetServoPowerState()
    {
        CallUnityErrorCode cmdResult = CallUnityErrorCode.Result_OK;
        if (!PlatformMgr.Instance.GetBluetoothState())
        {
            cmdResult = CallUnityErrorCode.Blue_DisConnect;
        }
        CallUnityResult result = CallUnityResult.powerOff;
        ModelDjData servoData = mRobot.GetAllDjData();
        foreach (KeyValuePair<byte, DuoJiData> kvp in servoData.GetAllData())
        {
            if (!kvp.Value.isPowerDown)
            {
                result = CallUnityResult.powerOn;
                break;
            }
        }
        CmdCallBack(LogicCmd.query.ToString(), result, cmdResult);
    }

    /// <summary>
    /// 执行动作命令
    /// </summary>
    /// <param name="args">动作id</param>
    void LogicAction(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                string actId = args[1];
                CallUnityErrorCode cmdResult = CallUnityErrorCode.Result_OK;
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    ErrorCode ret = mRobot.PlayActionsForID(actId);
                    if (ErrorCode.Result_Action_Not_Exist == ret)
                    {
                        cmdResult = CallUnityErrorCode.Actions_Not_Exist;
                    }
                }
                else
                {
                    cmdResult = CallUnityErrorCode.Blue_DisConnect;
                }
                if (cmdResult != CallUnityErrorCode.Result_OK)
                {
                    CmdCallBack(args[0], CallUnityResult.failure, CallUnityErrorCode.Result_None);
                }
            }
            
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 设置舵机上电
    /// </summary>
    /// <param name="arg"></param>
    void LogicServoPowerOn(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    if (string.IsNullOrEmpty(args[1]) || args[1].Equals("0"))
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                    else
                    {
                        List<byte> servos = PublicFunction.StringToByteList(args[1]);
                        mRobot.ServoPowerOn(servos);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
                
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 设置舵机掉电
    /// </summary>
    /// <param name="args"></param>
    void LogicServoPowerOff(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    if (string.IsNullOrEmpty(args[1]) || args[1].Equals("0"))
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                    else
                    {
                        List<byte> servos = PublicFunction.StringToByteList(args[1]);
                        mRobot.ServoPowerDown(servos);
                    }
                        
                    //CmdCallBack(args[0], CallUnityResult.success);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }

            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 调节舵机角度，用于coding界面时看效果
    /// </summary>
    /// <param name="args"></param>
    void LogicAdjustServo(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                RunCmdFinished(args[0]);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    Dictionary<byte, byte> rotas = new Dictionary<byte, byte>();
                    int time = -1;
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            if (-1 == time && dict.ContainsKey("ms") && PublicFunction.IsInteger(dict["ms"].ToString()))
                            {
                                time = int.Parse(dict["ms"].ToString());
                            }
                            byte servo = PublicFunction.IsInteger(dict["servo"].ToString()) ? byte.Parse(dict["servo"].ToString()) : (byte)1;
                            int angle = PublicFunction.IsInteger(dict["degree"].ToString()) ? int.Parse(dict["degree"].ToString()) + PublicFunction.DuoJi_Start_Rota : PublicFunction.DuoJi_Start_Rota;
                            if (angle < PublicFunction.DuoJi_Min_Show_Rota)
                            {
                                angle = PublicFunction.DuoJi_Min_Show_Rota;
                            }
                            else if (angle > PublicFunction.DuoJi_Max_Show_Rota)
                            {
                                angle = PublicFunction.DuoJi_Max_Show_Rota;
                            }
                            rotas[servo] = (byte)angle;
                            ErrorCode ret = mRobot.CtrlActionForDjId(servo, angle);
                            if (ret != ErrorCode.Result_OK)
                            {
                                RunCmdFinished(args[0]);
                            }
                        }
                    }
                    
                }
                else
                {
                    RunCmdFinished(args[0]);
                }
            }
        }
        catch (System.Exception ex)
        {
            RunCmdFinished(args[0]);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 控制舵机转动
    /// </summary>
    /// <param name="arg"></param>
    void LogicServoSet(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    Dictionary<byte, byte> rotas = new Dictionary<byte, byte>();
                    int time = -1;
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            if (-1 == time && dict.ContainsKey("ms") && PublicFunction.IsInteger(dict["ms"].ToString()))
                            {
                                time = int.Parse(dict["ms"].ToString());
                            }
                            byte servo = PublicFunction.IsInteger(dict["servo"].ToString()) ? byte.Parse(dict["servo"].ToString()) : (byte)1;
                            int angle = PublicFunction.IsInteger(dict["degree"].ToString()) ? int.Parse(dict["degree"].ToString()) + PublicFunction.DuoJi_Start_Rota : PublicFunction.DuoJi_Start_Rota;
                           
                            if (angle < PublicFunction.DuoJi_Min_Show_Rota)
                            {
                                angle = PublicFunction.DuoJi_Min_Show_Rota;
                            }
                            else if (angle > PublicFunction.DuoJi_Max_Show_Rota)
                            {
                                angle = PublicFunction.DuoJi_Max_Show_Rota;
                            }
                            rotas[servo] = (byte)angle;
                        }
                        
                        
                    }
                    ErrorCode ret = mRobot.CtrlServoMove(rotas, time);
                    if (ret == ErrorCode.Result_OK)
                    {
                        mServoSetCallBackFlag = false;
                        mServoSetFinishedFlag = false;
                        mWaitServoSetIndex = Timer.Add(time / 1000.0f, 1, 1, ServoSetFinished);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
            
            
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 控制舵机轮模式
    /// </summary>
    /// <param name="args"></param>
    void LogicServoSetbySpeed(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    Dictionary<byte, TurnData> rotas = new Dictionary<byte, TurnData>();
                    int[] speedArray = new int[] { 0x0080, 0x00EA, 0x0154, 0x020E, 0x0292 };
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            TurnData turnData = new TurnData();
                            byte servo = PublicFunction.IsInteger(dict["servo"].ToString()) ? byte.Parse(dict["servo"].ToString()) : (byte)1;
                            string speedStr = string.Empty;
                            if (dict.ContainsKey("speed"))
                            {
                                speedStr = dict["speed"].ToString();
                            }
                            int speed = 0;
                            try
                            {
                                if (!string.IsNullOrEmpty(speedStr))
                                {
                                    MoveSpeed moveSpeed = (MoveSpeed)Enum.Parse(typeof(MoveSpeed), speedStr);
                                    speed = (byte)moveSpeed;
                                }
                            }
                            catch (System.Exception ex)
                            {
                                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-速度值错误 speed = " + speedStr + "- error = " + ex.ToString());
                            }                            
                            speed = speedArray[speed];
                            int tmp = PublicFunction.IsInteger(dict["direction"].ToString()) ? int.Parse(dict["direction"].ToString()) : 0;
                            if (tmp == 1)
                            {
                                turnData.turnDirection = TurnDirection.turnByClock;
                            }
                            else if (tmp == 2)
                            {
                                turnData.turnDirection = TurnDirection.turnByDisclock;
                            }
                            else
                            {
                                turnData.turnDirection = TurnDirection.turnStop;
                            }
                            turnData.turnSpeed = (ushort)speed;
                            rotas[servo] = turnData;
                        }
                    }
                    if (rotas.Count > 0)
                    {
                        mCtrlServoTurnCount = 0;
                        mRobot.CtrlServoTurn(rotas, ExtendCMDCode.CtrlServoTurn);
                    } else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                    //CmdCallBack(args[0], CallUnityResult.success);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }


        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 停止舵机轮模式
    /// </summary>
    /// <param name="args"></param>
    void LogicStopServo(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    Dictionary<byte, TurnData> rotas = new Dictionary<byte, TurnData>();
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            TurnData turnData = new TurnData();
                            byte servo = PublicFunction.IsInteger(dict["servo"].ToString()) ? byte.Parse(dict["servo"].ToString()) : (byte)1;
                            turnData.turnDirection = TurnDirection.turnStop;
                            turnData.turnSpeed = 0;
                            rotas[servo] = turnData;
                        }
                    }
                    if (rotas.Count > 0)
                    {
                        mCtrlServoTurnCount = 0;
                        mRobot.CtrlServoTurn(rotas, ExtendCMDCode.CtrlServoStop);
                    } else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                    
                    //CmdCallBack(args[0], CallUnityResult.success);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }


        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 回读
    /// </summary>
    /// <param name="args"></param>
    void LogicGetPosture(string[] args)
    {
        try
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                bool isPowerFlag = false;
                List<byte> angleList = mRobot.GetAllDjData().GetAngleList();
                for (int i = 0, imax = angleList.Count; i < imax; ++i)
                {
                    DuoJiData data = mRobot.GetAnDjData(angleList[i]);
                    if (null != data && data.modelType == ServoModel.Servo_Model_Angle && data.isPowerDown)
                    {
                        isPowerFlag = true;
                        break;
                    }
                }
                if (isPowerFlag)
                {
                    mRobot.ReadBack(ExtendCMDCode.LogicGetPosture);
                }
                else
                {
                    Action ac = new Action();
                    mRobot.GetNowAction(ac);
                    GetPostureCallBack(CallUnityResult.success, ac);
                }
                /*Action ac = new Action();
                ac.UpdateRota(1, 20);
                ac.UpdateRota(2, 30);
                GetPostureCallBack(CallUnityResult.success, ac);*/
            }
            else
            {
                CmdCallBack(args[0], CallUnityResult.failure);
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 停止机器人
    /// </summary>
    /// <param name="args"></param>
    void LogicStopRobot(string[] args)
    {
        try
        {
            if (null != mRuningCmdDict)
            {
                mRuningCmdDict.Clear();
            }
            if (null != mWaitCmdDict)
            {
                mWaitCmdDict.Clear();
            }
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                mRobot.StopNowPlayActions();
                if (mRobot.GetAllDjData().GetTurnList().Count > 0)
                {
                    mRobot.StopAllTurn();
                }
                SingletonObject<LightStatusManager>.GetInst().SendCloseLight(mRobot);
            }
            /*else
            {
                RunCmdFinished(args[0]);
            }*/
        }
        catch (System.Exception ex)
        {
            //RunCmdFinished(args[0]);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicDisConnenct(string[] args)
    {
        try
        {
            PlatformMgr.Instance.DisConnenctBuletooth();
            mWaitExceptionRepairFlag = false;
            mChargeProtectedFlag = false;
            RunCmdFinished(args[0]);
        }
        catch (System.Exception ex)
        {
            RunCmdFinished(args[0]);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicQueryAllSensor(string[] args)
    {
        try
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {

                /*if (null != mRobot.GetReadSensorData(TopologyPartType.Gyro) && null != mRobot.GetReadSensorData(TopologyPartType.Gyro).ids && mRobot.GetReadSensorData(TopologyPartType.Gyro).ids.Count > 0)
                {
                    mNeedWaitSensorCallBack = 2;
                    mRobot.ReadAllSensorData();
                    mRobot.ReadSensorData(mRobot.GetReadSensorData(TopologyPartType.Gyro).ids, TopologyPartType.Gyro, true);
                }
                else*/
                {
                    mRobot.ReadAllSensorData();
                }
            }
            else
            {
                CmdCallBack(args[0], CallUnityResult.failure);
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 查询红外数据
    /// </summary>
    /// <param name="args"></param>
    void LogicQueryInfrared(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    List<byte> ids = PublicFunction.StringToByteList(args[1]);
                    if (null != mRobot.GetReadSensorData(TopologyPartType.Infrared) && ids.Count > 0)
                    {
                        mRobot.ReadSensorData(ids, TopologyPartType.Infrared, false);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 查询触碰数据
    /// </summary>
    /// <param name="args"></param>
    void LogicQueryTouchStatue(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    List<byte> ids = PublicFunction.StringToByteList(args[1]);
                    if (null != mRobot.GetReadSensorData(TopologyPartType.Touch) && ids.Count > 0)
                    {
                        mRobot.ReadSensorData(ids, TopologyPartType.Touch, false);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 查询陀螺仪数据
    /// </summary>
    /// <param name="args"></param>
    void LogicQueryGyroscope(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    List<byte> ids = PublicFunction.StringToByteList(args[1]);
                    if (null != mRobot.GetReadSensorData(TopologyPartType.Gyro) && ids.Count > 0)
                    {
                        mRobot.ReadSensorData(ids, TopologyPartType.Gyro, false);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 设置Led数据
    /// </summary>
    /// <param name="args"></param>
    void LogicSetLEDs(string[] args)
    {
        try
        {
            if (args.Length < 3)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetLEDCount = 0;
                    UInt32 duration = 255000;
                    if (PublicFunction.IsInteger(args[2]))
                    {
                        duration = UInt32.Parse(args[2]);
                    } else if (null != args[2] && args[2].Contains("type"))
                    {
                        JsonData timeData = new JsonData(Json.Deserialize(args[2]));
                        Dictionary<string, object> timeDict = (Dictionary<string, object>)timeData.Dictionary;
                        if (null != timeDict && timeDict.ContainsKey("type") && timeDict["type"].ToString() == "time" && timeDict.ContainsKey("value"))
                        {
                            if (PublicFunction.IsInteger(timeDict["value"].ToString()))
                            {
                                duration = UInt32.Parse(timeDict["value"].ToString());
                            }
                        }
                    }
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    Dictionary<string, Dictionary<UInt32, List<byte>>> ledData = GetLEDDatas(data);
                    if (null != ledData && ledData.Count > 0)
                    {
                        foreach (var colorPair in ledData)
                        {
                            List<LightShowData> color = lightsToColorData(colorPair.Key);
                            if (null != color && color.Count > 0)
                            {
                                foreach (var timePair in colorPair.Value)
                                {
                                    if (null != timePair.Value && timePair.Value.Count > 0)
                                    {
                                        ++mSetLEDCount;
                                        mRobot.SendLight(timePair.Value, color, duration);
                                    }
                                }
                            }
                        }
                        if (mSetLEDCount == 0)
                        {
                            CmdCallBack(args[0], CallUnityResult.success);
                        }
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    
    void LogicSetUltrasonic(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetUltrasonicCount = 0;
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    Dictionary<string, Dictionary<byte, Dictionary<byte, Dictionary<UInt32, List<byte>>>>> sendData = new Dictionary<string, Dictionary<byte, Dictionary<byte, Dictionary<UInt32, List<byte>>>>>();
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            string color = string.Empty;
                            byte id = 0;
                            byte mode = 1;
                            byte speed = 0;
                            UInt32 time = 0;
                            if (dict.ContainsKey("color"))
                            {
                                color = dict["color"].ToString();
                            }
                            if (dict.ContainsKey("id") && PublicFunction.IsInteger(dict["id"].ToString()))
                            {
                                id = byte.Parse(dict["id"].ToString());
                            }
                            if (dict.ContainsKey("mode") && PublicFunction.IsInteger(dict["mode"].ToString()))
                            {
                                mode = byte.Parse(dict["mode"].ToString());
                            }
                            if (dict.ContainsKey("speed") && PublicFunction.IsInteger(dict["speed"].ToString()))
                            {
                                speed = (byte)PublicFunction.Rounding(UInt16.Parse(dict["speed"].ToString()) / 100.0f);
                            }
                            if (dict.ContainsKey("time") && PublicFunction.IsInteger(dict["time"].ToString()))
                            {
                                time = UInt32.Parse(dict["time"].ToString());
                                if (time == 0)
                                {
                                    mode = 0;
                                }
                            }
                            if (id > 0)
                            {
                                if (!sendData.ContainsKey(color))
                                {
                                    sendData[color] = new Dictionary<byte, Dictionary<byte, Dictionary<UInt32, List<byte>>>>();
                                }
                                if (!sendData[color].ContainsKey(mode))
                                {
                                    sendData[color][mode] = new Dictionary<byte, Dictionary<UInt32, List<byte>>>();
                                }
                                if (!sendData[color][mode].ContainsKey(speed))
                                {
                                    sendData[color][mode][speed] = new Dictionary<UInt32, List<byte>>();
                                }
                                if (!sendData[color][mode][speed].ContainsKey(time))
                                {
                                    sendData[color][mode][speed][time] = new List<byte>();
                                }
                                sendData[color][mode][speed][time].Add(id);
                            }
                        }
                    }
                    if (sendData.Count > 0)
                    {
                        foreach (var colorKvp in sendData)
                        {
                            foreach (var modeKvp in colorKvp.Value)
                            {
                                foreach (var speedKvp in modeKvp.Value)
                                {
                                    foreach (var timeKvp in speedKvp.Value)
                                    {
                                        SendUltrasonicDataMsg msg = new SendUltrasonicDataMsg();
                                        msg.sensorData.sensorType = TopologyPartData.ConvertToSensorType(TopologyPartType.Ultrasonic);
                                        msg.sensorData.ids = timeKvp.Value;
                                        msg.rgb = colorKvp.Key;
                                        msg.mode = modeKvp.Key;
                                        msg.speed = speedKvp.Key;
                                        msg.time = timeKvp.Key;
                                        ++mSetUltrasonicCount;
                                        mRobot.SendUltrasonic(msg);
                                    }
                                }
                            }
                            
                        }
                    } else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
  
    /// <summary>
    /// 设置表情数据
    /// </summary>
    /// <param name="args"></param>
    void LogicSetEmoji(string[] args)
    {
        try
        {
            if (args.Length < 3)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetEmojiCount = 0;
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    UInt16 times = UInt16.Parse(args[2]);
                    //UInt16 duration = UInt16.Parse(args[3]);
                    Dictionary<byte, Dictionary<string, List<byte>>> emojiDict = new Dictionary<byte, Dictionary<string, List<byte>>>();
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            byte id = 0;
                            byte lightType = 0;
                            string color = "#000000";
                            if (dict.ContainsKey("id") && PublicFunction.IsInteger(dict["id"].ToString()))
                            {
                                id = byte.Parse(dict["id"].ToString());
                            }
                            if (dict.ContainsKey("emotionIndex"))
                            {
                                lightType = PublicFunction.IsInteger(dict["emotionIndex"].ToString()) ? (byte)byte.Parse(dict["emotionIndex"].ToString()) : (byte)1;
                            }
                            if (dict.ContainsKey("color"))
                            {
                                color = dict["color"].ToString();
                            }
                            if (!emojiDict.ContainsKey(lightType))
                            {
                                Dictionary<string, List<byte>> colorDict = new Dictionary<string, List<byte>>();
                                emojiDict[lightType] = colorDict;
                            }
                            if (!emojiDict[lightType].ContainsKey(color))
                            {
                                List<byte> ids = new List<byte>();
                                emojiDict[lightType][color] = ids;
                            }
                            if (id > 0)
                            {
                                emojiDict[lightType][color].Add(id);
                            }
                            
                        }
                    }
                    if (emojiDict.Count <= 0)
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                    else
                    {
                        foreach (var kvp in emojiDict)
                        {
                            foreach (var tmp in kvp.Value)
                            {
                                if (tmp.Value.Count > 0)
                                {
                                    ++mSetEmojiCount;
                                    mRobot.SendEmoji(tmp.Value, kvp.Key, tmp.Key, times);
                                }
                            }
                        }
                        if (0 == mSetEmojiCount)
                        {
                            CmdCallBack(args[0], CallUnityResult.success);
                        }
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


    void LogicsetGroupEmoji(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetEmojiCount = 0;
                    JsonData data = new JsonData(Json.Deserialize(args[1]));                   
                    Dictionary<ushort, Dictionary<byte, Dictionary<string, List<byte>>>> emojiDict = new Dictionary<ushort, Dictionary<byte, Dictionary<string, List<byte>>>>();
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            byte id = 0;
                            byte lightType = 0;
                            string color = "#000000";
                            ushort times = 0;
                            if (dict.ContainsKey("times") && PublicFunction.IsInteger(dict["times"].ToString()))
                            {
                                times = ushort.Parse(dict["times"].ToString());
                            }
                            if (dict.ContainsKey("id") && PublicFunction.IsInteger(dict["id"].ToString()))
                            {
                                id = byte.Parse(dict["id"].ToString());
                            }
                            if (dict.ContainsKey("emotionIndex"))
                            {
                                lightType = PublicFunction.IsInteger(dict["emotionIndex"].ToString()) ? (byte)byte.Parse(dict["emotionIndex"].ToString()) : (byte)1;
                            }
                            if (dict.ContainsKey("color"))
                            {
                                color = dict["color"].ToString();
                            }
                            if (times >= 0 && id > 0)
                            {
                                if (!emojiDict.ContainsKey(times))
                                {
                                    Dictionary<byte, Dictionary<string, List<byte>>> tmp = new Dictionary<byte, Dictionary<string, List<byte>>>();
                                    emojiDict[times] = tmp;
                                }
                                if (!emojiDict[times].ContainsKey(lightType))
                                {
                                    Dictionary<string, List<byte>> colorDict = new Dictionary<string, List<byte>>();
                                    emojiDict[times][lightType] = colorDict;
                                }
                                if (!emojiDict[times][lightType].ContainsKey(color))
                                {
                                    List<byte> ids = new List<byte>();
                                    emojiDict[times][lightType][color] = ids;
                                }
                                emojiDict[times][lightType][color].Add(id);
                            }
                        }
                    }
                    if (emojiDict.Count > 0)
                    {
                        foreach (var tkvp in emojiDict)
                        {
                            foreach (var kvp in tkvp.Value)
                            {
                                foreach (var tmp in kvp.Value)
                                {
                                    if (tmp.Value.Count > 0)
                                    {
                                        ++mSetEmojiCount;
                                        mRobot.SendEmoji(tmp.Value, kvp.Key, tmp.Key, tkvp.Key);
                                    }
                                }
                            }
                        }
                        if (0 == mSetEmojiCount)
                        {
                            CmdCallBack(args[0], CallUnityResult.success);
                        }
                    } else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicSetGroupLEDs(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetLEDCount = 0;
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    Dictionary<string, Dictionary<UInt32, List<byte>>> ledData = GetLEDDatas(data);
                    if (null != ledData && ledData.Count > 0)
                    {
                        foreach (var colorPair in ledData)
                        {
                            List<LightShowData> color = lightsToColorData(colorPair.Key);
                            if (null != color && color.Count > 0)
                            {
                                foreach (var timePair in colorPair.Value)
                                {
                                    if (null != timePair.Value && timePair.Value.Count > 0)
                                    {
                                        ++mSetLEDCount;
                                        mRobot.SendLight(timePair.Value, color, timePair.Key);
                                    }
                                }
                            }
                        }
                        if (mSetLEDCount == 0)
                        {
                            CmdCallBack(args[0], CallUnityResult.success);
                        }
                    } else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


    Dictionary<string, Dictionary<UInt32, List<byte>>> GetLEDDatas(JsonData data)
    {
        if (null != data)
        {
            Dictionary<string, Dictionary<UInt32, List<byte>>> ledData = new Dictionary<string, Dictionary<UInt32, List<byte>>>();
            for (int i = 0, imax = data.Count; i < imax; ++i)
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                if (null != dict)
                {
                    byte id = 0;
                    if (dict.ContainsKey("id") && PublicFunction.IsInteger(dict["id"].ToString()))
                    {
                        id = byte.Parse(dict["id"].ToString());
                    }
                    string lights = string.Empty;
                    if (dict.ContainsKey("lights"))
                    {
                        lights = Json.Serialize(dict["lights"]);
                    }
                    UInt32 duration = 0;
                    if (dict.ContainsKey("time") && PublicFunction.IsInteger(dict["time"].ToString()))
                    {
                        duration = UInt32.Parse(dict["time"].ToString());
                    }
                    if (id != 0 && !string.IsNullOrEmpty(lights))
                    {
                        if (!ledData.ContainsKey(lights))
                        {
                            Dictionary<UInt32, List<byte>> tmp = new Dictionary<UInt32, List<byte>>();
                            ledData[lights] = tmp;
                        }
                        if (!ledData[lights].ContainsKey(duration))
                        {
                            List<byte> tmp = new List<byte>();
                            ledData[lights][duration] = tmp;
                        }
                        if (!ledData[lights][duration].Contains(id))
                        {
                            ledData[lights][duration].Add(id);
                        }
                    }
                }
            }
            return ledData;
        }
        return null;
    }

    List<LightShowData> lightsToColorData(string lights)
    {
        Dictionary<string, List<byte>> showColor = new Dictionary<string, List<byte>>();
        JsonData lightsData  = new JsonData(Json.Deserialize(lights));
        for (int lightIndex = 0, lightMax = lightsData.Count; lightIndex < lightMax; ++lightIndex)
        {
            string color = lightsData[lightIndex].ToString();
            if (!showColor.ContainsKey(color))
            {
                List<byte> list = new List<byte>();
                showColor[color] = list;
            }
            showColor[color].Add((byte)(lightIndex + 1));
        }
        if (showColor.Count > 0)
        {
            List<LightShowData> showData = new List<LightShowData>();
            foreach (var kvp in showColor)
            {
                LightShowData light = new LightShowData();
                light.ids = kvp.Value;
                light.rgb = kvp.Key;
                showData.Add(light);
            }
            return showData;
        }
        return null;
    }
    /// <summary>
    /// 设置数码管
    /// </summary>
    /// <param name="args">setDigitalTube|id1,2|控制类型0|需显示的数字位数1,2,3,4|需要显示的点的位数1,2,3,4|是否显示冒号0|是否是负数0|闪烁的次数10|闪烁或数值变化的频率300|起始值0|结束值1</param>
    void LogicSetDigitalTube(string[] args)
    {
        try
        {
            if (args.Length < 10)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetDigitalTubeCount = 0;
                    List<byte> ids = PublicFunction.StringToByteList(args[1]);
                    byte controlType = byte.Parse(args[2]);
                    List<byte> showNum = PublicFunction.StringToByteList(args[3]);
                    List<byte> showSubPoint = PublicFunction.StringToByteList(args[4]);
                    bool showColon = args[5].Equals("1");
                    bool isNegativeNum = args[6].Equals("1");
                    byte flickerTimes = byte.Parse(args[7]);
                    UInt32 flickerTimeout = UInt32.Parse(args[8]);
                    UInt32 startValue = UInt32.Parse(args[9]);
                    UInt32 endValue = UInt32.Parse(args[10]);
                    ++mSetDigitalTubeCount;
                    mRobot.SendDigitalTube(ids, controlType, showNum, showSubPoint, showColon, isNegativeNum, flickerTimes, flickerTimeout, startValue, endValue);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicCtrlSensorLED(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    TopologyPartType partType = TopologyPartType.Infrared;
                    byte id = 0;
                    CtrlSensorLEDMsg.ControlType controlType = CtrlSensorLEDMsg.ControlType.Single_Flash;
                    UInt16 duration = 300;
                    byte times = 0;
                    Dictionary<string, object> dict = (Dictionary<string, object>)data.Dictionary;
                    if (dict.ContainsKey("sensorType"))
                    {
                        partType = (TopologyPartType)Enum.Parse(typeof(TopologyPartType), dict["sensorType"].ToString());
                    }
                    if (dict.ContainsKey("id") && PublicFunction.IsInteger(dict["id"].ToString()))
                    {
                        id = byte.Parse(dict["id"].ToString());
                    }
                    if (dict.ContainsKey("controlType"))
                    {
                        controlType = (CtrlSensorLEDMsg.ControlType)byte.Parse(dict["controlType"].ToString());
                    }
                    if (dict.ContainsKey("duration"))
                    {
                        duration = UInt16.Parse(dict["duration"].ToString());
                    }
                    if (dict.ContainsKey("times") && PublicFunction.IsInteger(dict["times"].ToString()))
                    {
                        times = byte.Parse(dict["times"].ToString());
                    }

                    mRobot.CtrlSensorLED(id, partType, controlType, duration, times);
                    CmdCallBack(args[0], CallUnityResult.success);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    
    void LogicPlayAction(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            } else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));                  
                    Dictionary<string, object> dict = (Dictionary<string, object>)data.Dictionary;
                    if (null != dict)
                    {
                        MoveType moveType = MoveType.Undefined;
                        MoveSpeed moveSpeed = PublicFunction.Default_Move_Speed;
                        float playArg = 0;
                        if (dict.ContainsKey("action_id"))
                        {
                            try
                            {
                                moveType = (MoveType)Enum.Parse(typeof(MoveType), dict["action_id"].ToString());
                            }
                            catch (System.Exception ex)
                            {
                                CatchException(args[0], CallUnityErrorCode.Result_None);
                                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                                return;
                            }
                        }
                        
                        if (dict.ContainsKey("speed"))
                        {
                            try
                            {
                                moveSpeed = (MoveSpeed)Enum.Parse(typeof(MoveSpeed), dict["speed"].ToString());
                            }
                            catch (System.Exception ex)
                            {
                                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-速度值错误 speed = " + dict["speed"].ToString()  + "- error = " + ex.ToString());
                            }
                            
                        }
                        MoveSequenceType seqType = SingletonObject<MoveManager>.GetInst().GetSequenceType(mRobot, moveType);
                        switch (seqType)
                        {
                            case MoveSequenceType.degree:
                                if (dict.ContainsKey("degree") && PublicFunction.IsInteger(dict["degree"].ToString()))
                                {
                                    playArg = UInt32.Parse(dict["degree"].ToString());
                                }
                                break;
                            case MoveSequenceType.distance:
                                if (dict.ContainsKey("distance") && PublicFunction.IsInteger(dict["distance"].ToString()))
                                {
                                    playArg = UInt32.Parse(dict["distance"].ToString());
                                }
                                break;
                            case MoveSequenceType.time_byMotor:
                            case MoveSequenceType.time_byServo:
                                if (dict.ContainsKey("time") && PublicFunction.IsFloat(dict["time"].ToString()))
                                {
                                    playArg = float.Parse(dict["time"].ToString());
                                }
                                break;
                        }
                        mRobot.PlayMove(moveType, moveSpeed, playArg);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    
    void LogicStopMotion(string[] args)
    {
        try
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                mRobot.StopNowPlayActions();
            }
            Timer.Add(0.1f, 1, 1, delegate () { CmdCallBack(args[0], CallUnityResult.success); });
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicSetMotor(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    //byte[] speedArray = new byte[] { 150, 176, 202, 228, 255 };
                    List<SendMotorData> list = new List<SendMotorData>();
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            SendMotorData motorData = new SendMotorData();
                            motorData.id = PublicFunction.IsInteger(dict["id"].ToString()) ? byte.Parse(dict["id"].ToString()) : (byte)1;
                            string speedStr = string.Empty;
                            if (dict.ContainsKey("speed"))
                            {
                                speedStr = dict["speed"].ToString();
                            }
                            byte speed = 0;
                            try
                            {
                                if (!string.IsNullOrEmpty(speedStr) && PublicFunction.IsInteger(speedStr))
                                {
                                    //MoveSpeed moveSpeed = (MoveSpeed)Enum.Parse(typeof(MoveSpeed), speedStr);
                                    speed = byte.Parse(speedStr);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-速度值错误 speed = " + speedStr + "- error = " + ex.ToString());
                            }
                            //speed = speedArray[speed];
                            if (dict.ContainsKey("time"))
                            {
                                motorData.time = PublicFunction.IsInteger(dict["time"].ToString()) ? (ushort)(int.Parse(dict["time"].ToString()) / 100): (ushort)1;
                            }
                            if (dict.ContainsKey("direction"))
                            {
                                int tmp = PublicFunction.IsInteger(dict["direction"].ToString()) ? int.Parse(dict["direction"].ToString()) : 0;
                                if (tmp == 1)
                                {
                                    motorData.direction = TurnDirection.turnByClock;
                                }
                                else if (tmp == 2)
                                {
                                    motorData.direction = TurnDirection.turnByDisclock;
                                }
                                motorData.speed = speed;
                                list.Add(motorData);
                            }
                        }
                    }
                    if (list.Count > 0)
                    {
                        if (null == mRobot)
                        {
                            mRobot = RobotManager.GetInst().GetCurrentRobot();
                        }
                        mRobot.PlayMotor(list);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicStopMotor(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    List<byte> stopList = new List<byte>();
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (dict.ContainsKey("id"))
                        {
                            byte id = PublicFunction.IsInteger(dict["id"].ToString()) ? byte.Parse(dict["id"].ToString()) : (byte)0;
                            if (id != 0)
                            {
                                stopList.Add(id);
                            }
                        }
                    }
                    if (stopList.Count > 0)
                    {
                        mRobot.StopMotor(stopList);
                    } else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicQueryRobotStatus(string[] args)
    {
        try
        {
            if (args.Length < 1)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                int state = 0;//0表示正常状态，1表示马达或者轮模式舵机在旋转，2表示灯光在亮，3表示马达/舵机和灯光都在工作
                //是否在播放分步动作,是否有轮模式舵机在运动,判断灯光状态
                state = mRobot.IsPlayMove() || mRobot.GetAllDjData().HaveServoTurn() || mRobot.HaveMotorTurn() ? 1 : 0;
                if (state > 0)
                {
                    state = SingletonObject<LightStatusManager>.GetInst().IsLight() ? 3 : state;
                } else
                {
                    state = SingletonObject<LightStatusManager>.GetInst().IsLight() ? 2 : 0;
                }
                dict["state"] = state;
                CmdMoreCallBack(args[0], Json.Serialize(dict));
                /*Timer.Add(0.05f, 1, 1, delegate () {
                    CmdMoreCallBack(args[0], Json.Serialize(dict));
                });*/
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicSetUnicornLightOn(string[] args)
    {
        try
        {
            if (args.Length < 3)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetRgbLightColorCount = 0;
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    JsonData data1 = new JsonData(Json.Deserialize(args[2]));
                    Dictionary<string, object> timeDict = (Dictionary<string, object>)data1.Dictionary;
                    int time = 0;
                    int number = 0;
                    if (null != timeDict)
                    {
                        if (timeDict.ContainsKey("time") && PublicFunction.IsInteger(timeDict["time"].ToString()))
                        {
                            if (!int.TryParse(timeDict["time"].ToString(), out time) || time > 65535)
                            {
                                time = 65535;
                            }
                            if (time < 0)
                            {
                                time = 0;
                            }
                        }
                        if (timeDict.ContainsKey("number") && PublicFunction.IsInteger(timeDict["number"].ToString()))
                        {
                            if (!int.TryParse(timeDict["number"].ToString(), out number))
                            {
                                number = 0;
                            }
                            if (number > 1000)
                            {
                                number = 1000;
                            }
                            if (number < 0)
                            {
                                number = 0;
                            }
                        }
                    }
                    List<SendRgbLightDataMsg> list = null;
                    if (args[1].Contains("color"))
                    {
                        list = ConvertRgbLightMsg_ShowColor(data, (UInt16)time, (UInt16)number);
                    } else
                    {
                        if (time >= 65535)
                        {
                            time = 65530;
                        }
                        list = ConvertRgbLightMsg_ChangeColor(data, (UInt16)time, (UInt16)number);
                    }
                    if (null == list || list.Count < 1)
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    } else
                    {
                        for (int i = 0, imax = list.Count; i < imax; ++i)
                        {
                            ++mSetRgbLightColorCount;
                            mRobot.SendRgbLight_Color(list[i]);
                        }
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    List<SendRgbLightDataMsg> ConvertRgbLightMsg_ShowColor(JsonData data, UInt16 time, UInt16 number)
    {
        if (null != data)
        {
            Dictionary<string, List<byte>> dict = new Dictionary<string, List<byte>>();
            for (int i = 0, imax = data.Count; i < imax; ++i)
            {
                Dictionary<string, object> colorDict = (Dictionary<string, object>)data[i].Dictionary;
                if (null != colorDict)
                {
                    string color = null;
                    byte id = 0;
                    if (colorDict.ContainsKey("color"))
                    {
                        color = colorDict["color"].ToString();
                    }
                    if (colorDict.ContainsKey("id") && PublicFunction.IsInteger(colorDict["id"].ToString()))
                    {
                        id = byte.Parse(colorDict["id"].ToString());
                    }
                    if (id > 0 && color != null)
                    {
                        if (!dict.ContainsKey(color))
                        {
                            dict[color] = new List<byte>();
                        }
                        dict[color].Add(id);
                    }

                }
            }
            if (dict.Count > 0)
            {
                List<SendRgbLightDataMsg> list = new List<SendRgbLightDataMsg>();
                foreach (var kvp in dict)
                {
                    SendRgbLightDataMsg msg = new SendRgbLightDataMsg();
                    msg.sensorData.ids = kvp.Value;
                    msg.controlType = RgbLightControlType.Show_Color;
                    msg.rgb = kvp.Key;
                    if (PublicFunction.IsBlackColor(msg.rgb))
                    {
                        msg.time = 0;
                        msg.flash = 0;
                    } else
                    {
                        msg.time = time;
                        msg.flash = number;
                    }
                    list.Add(msg);
                }
                return list;
            }
        }
        return null;
    }

    List<SendRgbLightDataMsg> ConvertRgbLightMsg_ChangeColor(JsonData data, UInt16 time, UInt16 number)
    {
        if (null != data)
        {
            Dictionary<string, Dictionary<string, List<byte>>> dict = new Dictionary<string, Dictionary<string, List<byte>>>();
            for (int i = 0, imax = data.Count; i < imax; ++i)
            {
                Dictionary<string, object> colorDict = (Dictionary<string, object>)data[i].Dictionary;
                if (null != colorDict)
                {
                    string from = null;
                    string to = null;
                    byte id = 0;
                    if (colorDict.ContainsKey("from"))
                    {
                        from = colorDict["from"].ToString();
                    }
                    if (colorDict.ContainsKey("to"))
                    {
                        to = colorDict["to"].ToString();
                    }
                    if (colorDict.ContainsKey("id") && PublicFunction.IsInteger(colorDict["id"].ToString()))
                    {
                        id = byte.Parse(colorDict["id"].ToString());
                    }
                    if (id > 0 && from != null && to != null)
                    {
                        if (!dict.ContainsKey(from))
                        {
                            dict[from] = new Dictionary<string, List<byte>>();
                        }
                        if (!dict[from].ContainsKey(to))
                        {
                            dict[from][to] = new List<byte>();
                        }
                        dict[from][to].Add(id);
                    }

                }
            }
            if (dict.Count > 0)
            {
                List<SendRgbLightDataMsg> list = new List<SendRgbLightDataMsg>();
                foreach (var fromKvp in dict)
                {
                    foreach (var toKvp in fromKvp.Value)
                    {
                        SendRgbLightDataMsg msg = new SendRgbLightDataMsg();
                        msg.sensorData.ids = toKvp.Value;
                        msg.controlType = RgbLightControlType.Change_Color;
                        msg.rgb = fromKvp.Key;
                        msg.targetRgb = toKvp.Key;
                        if (PublicFunction.IsBlackColor(msg.rgb) && PublicFunction.IsBlackColor(msg.targetRgb))
                        {
                            msg.time = 0;
                            msg.flash = 0;
                        }
                        else
                        {
                            msg.time = time;
                            msg.flash = number;
                        }
                        list.Add(msg);
                    }
                    
                }
                return list;
            }
        }
        return null;
    }

    void LogicSetUnicornEmoji(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetRgbLightEmojiCount = 0;
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    Dictionary<int, List<byte>> msgDict = new Dictionary<int, List<byte>>();
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            byte id = 0;
                            int emotionIndex = -1;
                            if (dict.ContainsKey("id"))
                            {
                                byte.TryParse(dict["id"].ToString(), out id);
                            }
                            if (dict.ContainsKey("emotionIndex"))
                            {
                                if (!int.TryParse(dict["emotionIndex"].ToString(), out emotionIndex))
                                {
                                    emotionIndex = -1;
                                }
                            }
                            if (id > 0 && emotionIndex >= 0)
                            {
                                if (!msgDict.ContainsKey(emotionIndex))
                                {
                                    msgDict[emotionIndex] = new List<byte>();
                                }
                                msgDict[emotionIndex].Add(id);
                            }
                        }
                    }
                    if (msgDict.Count > 0)
                    {
                        foreach (var kvp in msgDict)
                        {
                            SendRgbLightDataMsg msg = new SendRgbLightDataMsg();
                            msg.sensorData.ids = kvp.Value;
                            msg.controlType = RgbLightControlType.Show_Emoji;
                            msg.emojiIndex = (ushort)kvp.Key;
                            ++mSetRgbLightEmojiCount;
                            mRobot.SendRgbLight_Emoji(msg);
                        }
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.success);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    #endregion

    #region 各种命令返回结果
    public void PlayActionCallBack(CallUnityResult result)
    {
        if (IsLogicProgramming)
        {
            CmdCallBack(LogicCmd.action.ToString(), result);
        }
    }

    public void AdjustServoCallBack()
    {
        if (IsLogicProgramming)
        {
            RunCmdFinished(LogicCmd.adjustServo.ToString());
        }
    }

    public void ServoSetCallBack(CallUnityResult result)
    {
        if (IsLogicProgramming)
        {
            mServoSetCallBackFlag = true;
            if (result == CallUnityResult.success)
            {
                if (mServoSetFinishedFlag)
                {
                    CmdCallBack(LogicCmd.servoSet.ToString(), result);
                }
            }
            else
            {
                if (-1 != mWaitServoSetIndex)
                {
                    Timer.Cancel(mWaitServoSetIndex);
                    mWaitServoSetIndex = -1;
                }
                CmdCallBack(LogicCmd.servoSet.ToString(), result);
            }
        }
        else
        {
            if (-1 != mWaitServoSetIndex)
            {
                Timer.Cancel(mWaitServoSetIndex);
                mWaitServoSetIndex = -1;
            }
        }
        
    }
    /// <summary>
    /// 回读返回
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action"></param>
    public void GetPostureCallBack(CallUnityResult result, Action action)
    {
        if (IsLogicProgramming)
        {
            if (result == CallUnityResult.success && null != action)
            {
                List<Dictionary<string, int>> list = new List<Dictionary<string, int>>();
                List<byte> angleList = mRobot.GetAllDjData().GetAngleList();
                for (int i = 0, imax = angleList.Count; i < imax; ++i)
                {
                    Dictionary<string, int> dict = new Dictionary<string, int>();
                    dict["servo"] = angleList[i];
                    dict["degree"] = action.GetRota(angleList[i]) - PublicFunction.DuoJi_Start_Rota;
                    list.Add(dict);
                }
                string jsonbill = CallUnityResult.success.ToString() + PublicFunction.Separator_Or + Json.Serialize(list);
                CmdMoreCallBack(LogicCmd.getPosture.ToString(), jsonbill);
            }
            else
            {
                CmdCallBack(LogicCmd.getPosture.ToString(), CallUnityResult.failure);
            }
            
        }
    }

    public void ServoPowerOnCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            CmdCallBack(LogicCmd.servoPowerOn.ToString(), result);
        }
    }

    public void ServoPowerOffCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            CmdCallBack(LogicCmd.servoPowerOff.ToString(), result);
        }
    }

    public void QueryAllSensorCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            string jsonbill = string.Empty;
            TopologyPartType[] sensorTypes = PublicFunction.Read_All_Sensor_Type;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int i = 0, imax = sensorTypes.Length; i < imax; ++i)
            {
                if (null != mRobot.GetReadSensorData(sensorTypes[i]))
                {
                    dict[sensorTypes[i].ToString()] = mRobot.GetReadSensorData(sensorTypes[i]).GetReadAllResult();
                }
            }
            jsonbill = result.ToString() + PublicFunction.Separator_Or + Json.Serialize(dict);
            CmdMoreCallBack(LogicCmd.queryAllSensor.ToString(), jsonbill);
        }
    }

    /// <summary>
    /// 查询红外返回
    /// </summary>
    /// <param name="result"></param>
    public void QueryInfraredCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            string jsonbill = string.Empty;
            if (result == CallUnityResult.success && null != mRobot.GetReadSensorData(TopologyPartType.Infrared))
            {
                jsonbill = CallUnityResult.success.ToString() + PublicFunction.Separator_Or + mRobot.GetReadSensorData(TopologyPartType.Infrared).GetReadResult();
            }
            else
            {
                jsonbill = CallUnityResult.failure.ToString();
            }
            CmdMoreCallBack(LogicCmd.queryInfrared.ToString(), jsonbill);
        }
    }
    /// <summary>
    /// 查询触碰返回
    /// </summary>
    /// <param name="result"></param>
    public void QueryTouchStatusCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            string jsonbill = string.Empty;
            if (result == CallUnityResult.success && null != mRobot.GetReadSensorData(TopologyPartType.Touch))
            {
                jsonbill = CallUnityResult.success.ToString() + PublicFunction.Separator_Or + mRobot.GetReadSensorData(TopologyPartType.Touch).GetReadResult();
            }
            else
            {
                jsonbill = CallUnityResult.failure.ToString();
            }
            CmdMoreCallBack(LogicCmd.queryTouchStatus.ToString(), jsonbill);
        }
    }
    /// <summary>
    /// 查询陀螺仪返回
    /// </summary>
    /// <param name="result"></param>
    public void QueryGyroscopeCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            string jsonbill = string.Empty;
            if (result == CallUnityResult.success && null != mRobot.GetReadSensorData(TopologyPartType.Gyro))
            {
                jsonbill = CallUnityResult.success.ToString() + PublicFunction.Separator_Or + mRobot.GetReadSensorData(TopologyPartType.Gyro).GetReadResult();
            }
            else
            {
                jsonbill = CallUnityResult.failure.ToString();
            }
            CmdMoreCallBack(LogicCmd.queryGyroscope.ToString(), jsonbill);
        }
    }

    public void SetLEDsCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetLEDCount;
            if (mSetLEDCount <= 0)
            {
                if (null != mRuningCmdDict)
                {
                    if (mRuningCmdDict.ContainsKey(LogicCmd.setGroupLEDs.ToString()))
                    {
                        CmdMoreCallBack(LogicCmd.setGroupLEDs.ToString(), result.ToString());
                    } else if (mRuningCmdDict.ContainsKey(LogicCmd.setLEDs.ToString()))
                    {
                        CmdMoreCallBack(LogicCmd.setLEDs.ToString(), result.ToString());
                    }
                }
            }
        }
    }

    public void SetUltrasonicCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetUltrasonicCount;
            if (mSetUltrasonicCount <= 0)
            {
                if (null != mRuningCmdDict)
                {
                    if (mRuningCmdDict.ContainsKey(LogicCmd.setUltrasonic.ToString()))
                    {
                        CmdMoreCallBack(LogicCmd.setUltrasonic.ToString(), result.ToString());
                    }
                }
            }
        }
    }


    public void SetEmojiCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetEmojiCount;
            if (mSetEmojiCount <= 0)
            {
                if (null != mRuningCmdDict && mRuningCmdDict.ContainsKey(LogicCmd.setGroupEmoji.ToString()))
                {
                    CmdMoreCallBack(LogicCmd.setGroupEmoji.ToString(), result.ToString());
                }
                else
                {
                    CmdMoreCallBack(LogicCmd.setEmoji.ToString(), result.ToString());
                }
            }
        }
    }

    public void SetDigitalTubeCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetDigitalTubeCount;
            if (mSetDigitalTubeCount <= 0)
            {
                CmdMoreCallBack(LogicCmd.setDigitalTube.ToString(), result.ToString());
            }
        }
    }

    public void ExceptionRepairResult()
    {
        if (mWaitExceptionRepairFlag)
        {
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.JsExceptionWaitResult, string.Empty);
            mWaitExceptionRepairFlag = false;
        }
    }

    public void PlayMotionCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            CmdMoreCallBack(LogicCmd.playAction.ToString(), result.ToString());
        }
    }

    public void CtrlServoTurnCallBack(CallUnityResult result, ExtendCMDCode exCmd)
    {
        if (isLogicProgramming)
        {
            if (result == CallUnityResult.failure)
            {
                if (exCmd == ExtendCMDCode.CtrlServoTurn)
                {
                    CmdCallBack(LogicCmd.servoSetbySpeed.ToString(), result);
                }
                else if (exCmd == ExtendCMDCode.CtrlServoStop)
                {
                    CmdCallBack(LogicCmd.stopServo.ToString(), result);
                }
            } else
            {
                mCtrlServoTurnCount--;
                if (mCtrlServoTurnCount <= 0)
                {
                    if (exCmd == ExtendCMDCode.CtrlServoTurn)
                    {
                        CmdCallBack(LogicCmd.servoSetbySpeed.ToString(), result);
                    }
                    else if (exCmd == ExtendCMDCode.CtrlServoStop)
                    {
                        CmdCallBack(LogicCmd.stopServo.ToString(), result);
                    }

                }
            }
        }
    }

    public void PlayMotorCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            CmdMoreCallBack(LogicCmd.setMotor.ToString(), result.ToString());
        }
    }


    public void StopMotorCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            CmdMoreCallBack(LogicCmd.stopMotor.ToString(), result.ToString());
        }
    }

    public void SetUnicornLightCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetRgbLightColorCount;
            if (mSetRgbLightColorCount <= 0)
            {
                if (null != mRuningCmdDict)
                {
                    if (mRuningCmdDict.ContainsKey(LogicCmd.setUnicornLightOn.ToString()))
                    {
                        CmdMoreCallBack(LogicCmd.setUnicornLightOn.ToString(), result.ToString());
                    }
                }
            }
        }
    }

    public void SetUnicornEmojiCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetRgbLightEmojiCount;
            if (mSetRgbLightEmojiCount <= 0)
            {
                if (null != mRuningCmdDict)
                {
                    if (mRuningCmdDict.ContainsKey(LogicCmd.setUnicornEmoji.ToString()))
                    {
                        CmdMoreCallBack(LogicCmd.setUnicornEmoji.ToString(), result.ToString());
                    }
                }
            }
        }
    }

    #endregion
    /// <summary>
    /// 执行完了某条命令
    /// </summary>
    /// <param name="cmd"></param>
    ///
    void RunCmdFinished(string cmd)
    {
        if (null == mRuningCmdDict)
        {
            return;
        }
        mRuningCmdDict.Remove(cmd);
        if (!mWaitExceptionRepairFlag && null != mWaitCmdDict && mWaitCmdDict.ContainsKey(cmd))
        {
            mRuningCmdDict[cmd] = mWaitCmdDict[cmd].args;
            mWaitCmdDict.Remove(cmd);
            mCallCmdDict[cmd](mRuningCmdDict[cmd]);
        }
    }
    /// <summary>
    /// 参数错误
    /// </summary>
    /// <param name="cmd"></param>
    void ParameterError(string cmd, CallUnityErrorCode errorCode)
    {
        CmdCallBack(cmd, CallUnityResult.failure, errorCode);
    }
    /// <summary>
    /// 出现异常
    /// </summary>
    /// <param name="cmd"></param>
    void CatchException(string cmd, CallUnityErrorCode errorCode)
    {
        CmdCallBack(cmd, CallUnityResult.failure, errorCode);
    }

    void CmdCallBack(string cmd, CallUnityResult result, CallUnityErrorCode errorCode = CallUnityErrorCode.Result_None)
    {
        CmdMoreCallBack(cmd, result.ToString(), errorCode);
    }

    void CmdMoreCallBack(string cmd, string result, CallUnityErrorCode errorCode = CallUnityErrorCode.Result_None)
    {
        if (null == mRuningCmdDict || !mRuningCmdDict.ContainsKey(cmd))
        {
            return;
        }
        Dictionary<string, string> dict = new Dictionary<string, string>();
        if (CallUnityErrorCode.Result_None != errorCode)
        {
            dict["result"] = ((byte)errorCode).ToString() + PublicFunction.Separator_Or + result;
        }
        else
        {
            dict["result"] = result;
        }
        dict["cmd"] = CmdArgsToString(mRuningCmdDict[cmd]).ToString();
        string jsonbill = Json.Serialize(dict);
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.LogicCMDResult, jsonbill);
        if (null != logicCmdCallBack)
        {
            logicCmdCallBack(jsonbill);
        }
        RunCmdFinished(cmd);
    }

    string CmdArgsToString(string[] args)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0, imax = args.Length; i < imax; ++i)
        {
            if (sb.Length == 0)
            {
                sb.Append(Logic_Cmd_Start);

            }
            else
            {
                sb.Append(PublicFunction.Separator_Or);
            }
            sb.Append(args[i]);
        }
        return sb.ToString();
    }

    Dictionary<string, string> StringToDict(string str)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        try
        {
            str = str.TrimStart('{').TrimEnd('}');
            string[] args = str.Split(',');
            for (int i = 0, imax = args.Length; i < imax; ++i)
            {
                string[] tmpArg = args[i].Split(':');
                if (2 == tmpArg.Length)
                {
                    dict[tmpArg[0]] = tmpArg[1];
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return dict;
    }


    //////////////////////////////////////////////////////////////////////////

    void ServoSetFinished()
    {
        mServoSetFinishedFlag = true;
        mWaitServoSetIndex = -1;
        if (mServoSetCallBackFlag)
        {
            CmdCallBack(LogicCmd.servoSet.ToString(), CallUnityResult.success);
        }
    }
}

/// <summary>
/// 逻辑编程命令
/// </summary>
public enum LogicCmd : byte
{
    query,
    action,
    servoSet,
    servoSetbySpeed,
    servoPowerOn,
    servoPowerOff,
    getPosture,
    stopServo,
    adjustServo,
    stopRobot,
    DisconnectBLE,
    queryAllSensor,
    queryInfrared,
    queryTouchStatus,
    queryGyroscope,
    setLEDs,
    setUltrasonic,
    setEmoji,
    setDigitalTube,
    setSensorLED,
    playAction,
    stopMotion,
    setGroupEmoji,
    setGroupLEDs,
    setMotor,
    stopMotor,
    queryRobotStatus,
    setUnicornLightOn,
    setUnicornEmoji,
}

/// <summary>
/// 逻辑编程调用unity接口事件结果返回
/// </summary>
public enum CallUnityResult : byte
{
    lowPower = 0,
    highPower,
    powerOn,
    powerOff,
    success,
    failure,
}
/// <summary>
/// 逻辑编程调用Unity接口错误码
/// </summary>
public enum CallUnityErrorCode : byte
{
    Result_None = 0,
    Result_OK = 1,
    Blue_DisConnect = 2,
    Actions_Not_Exist = 3,
}
/// <summary>
/// 提示颜色
/// </summary>
public enum CommonTipsColor : byte
{
    yellow = 0,
    green = 1,
    red = 2,
}
