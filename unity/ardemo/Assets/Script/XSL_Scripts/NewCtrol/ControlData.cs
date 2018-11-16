//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
//#define PlayMode_new
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using MyData;
using MyMVC;
using Game.Event;
using Game.Platform;
using Game.Resource;

public class ControlData
{
    public string ActionDataPath;
    private static ControlData _instance;
    public static ControlData GetIns()
    {
        if (_instance == null)
        {
            _instance = new ControlData();
        }
        return _instance;
    }

    public static void ClearData()
    {
        _instance = null;
        ActionLogic.Clear();
    }

    private ControlData()
    {
        //ActionDataPath = ResourcesEx.persistentDataPath + "/" + RobotManager.GetInst().GetCurrentRobot().ID + NewCtrolview.ControllerID + ".xml";
        //UpdateActionData();  //唯一的robotid
    }

    public ActionXmlData curActionData;
    //    public List<string> curUseActionlist; 

    /// <summary>
    /// 遥控的删除功能
    /// </summary>
    /// <param name="actionNM"></param>
    public void RemoveControllerAction(int index)
    {
        if (curActionData.ActionList.Count > index)
        {
            if (curActionData.ActionList[index] != "")
            {
                curActionData.ActionList[index] = "";
                SaveToXml(curActionData);
            }
        }
    }

    public void RemoveCotrollerAllAction()
    { 
        //if(curActionData.ActionList.Count)
        for (int i = 0; i < curActionData.ActionList.Count; i++)
        {
            if (curActionData.ActionList.Count > i)
            {
                if (curActionData.ActionList[i] != "")
                {
                    curActionData.ActionList[i] = "";
                }
            }
        }
        SaveToXml(curActionData);
    }

    /// <summary>
    /// 删除动作表中的某个动作
    /// </summary>
    /// <param name="actionNm"></param>
    public void DeletaAction(string actionNm)
    {
    }

    #region // xml文件与数据类
    public void UpdateActionData() // 更新数据
    {
        if (!File.Exists(ActionDataPath)) //文件不存在
        {
            if (RecordContactInfo.Instance.openType != "playerdata") //official模型
            {
                CreateOfficalActionFile();
            }
            else
            {
                CreateEmptyActionFile();
            }
        }
        LoadFromXml(ref curActionData);
    }

    /// <summary>
    /// 生成动作文件
    /// </summary>
    public void CreateEmptyActionFile()
    {
        curActionData = new ActionXmlData();
        curActionData.RobotID = RobotManager.GetInst().GetCurrentRobot().ID;
        curActionData.RobotName = RobotManager.GetInst().GetCurrentRobot().Name;
        for (int i = 0; i < ActionXmlData.MaxActionAmount; i++)
        {
            curActionData.ActionList.Add("");
        }
        SaveToXml(curActionData);
    }
    /// <summary>
    /// 生成官方动作配置
    /// </summary>
    public void CreateOfficalActionFile()
    {
        curActionData = new ActionXmlData();
        curActionData.RobotID = RobotManager.GetInst().GetCurrentRobot().ID;
        curActionData.RobotName = RobotManager.GetInst().GetCurrentRobot().Name;
        string tempName;
        for (int i = 0; i < ActionXmlData.MaxActionAmount; i++)
        {
            tempName = GetDefaultControllerNameByIndex(i);
            curActionData.ActionList.Add(tempName);
        }
        SaveToXml(curActionData);
    }

    List<string> temList = new List<string>(); //临时存放已配置过的动作名称
    string GetDefaultControllerNameByIndex(int index)
    {
        string tempName = "";
        switch (index)
        { 
            case 0:
                tempName = "icon_top";
                break;
            case 1:
                tempName = "icon_left";
                break;
            case 2:
                tempName = "icon_right";
                break;
            case 3:
                tempName = "icon_getdown";
                break;
            default:
                /*for (int i = 0; i < RobotManager.GetInst().GetCurrentRobot().GetActionsNameList().Count; i++)
                {
                    if (!temList.Contains(RobotManager.GetInst().GetCurrentRobot().GetActionsNameList()[i]))
                    {
                        tempName = RobotManager.GetInst().GetCurrentRobot().GetActionsNameList()[i];
                        break;
                    }
                }*/
                temList.Add(tempName);
                return tempName;
        }
        //tempName = RobotManager.GetInst().GetCurrentRobot().GetActionNameForIcon(tempName);
        temList.Add(tempName);
        Debug.Log(tempName);
        return tempName;
    }

    /// <summary>
    /// xml序列化到对象
    /// </summary>
    /// <param name="cell"></param>
    public void SaveToXml(ActionXmlData actionData) //将数据保存到xml中
    {
        if (actionData == null)
            return;

        string str = XmlHelper.XmlSerialize(actionData, Encoding.UTF8);

        FileStream fs = new FileStream(ActionDataPath, FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);
        fs.SetLength(0);//首先把文件清空了。
        sw.Write(str);//写你的字符串。
        sw.Close();
    }

    /// <summary>
    /// xml反序列到对象
    /// </summary>
    /// <param name="actionData"></param>
    /// <param name="path"></param>
    void LoadFromXml(ref ActionXmlData actionData)
    {
        if (!File.Exists(ActionDataPath))
        {
            //"文件不存在")
            return;
        }
        actionData = XmlHelper.XmlDeserializeFromFile<ActionXmlData>(ActionDataPath, Encoding.UTF8);
    }
    #endregion
}

public class ActionLogic
{
    public GameObject actionItem;
    private GameObject CurOperateAnim;  // 当前正在播放的action
    private List<GameObject> playList; // 动作列表
    private static ActionLogic _instance;
    public static ActionLogic GetIns()
    {
        if (_instance == null)
            _instance = new ActionLogic();
        return _instance;
    }

    private ActionLogic()
    {
        //连接是否
        //_isConnect = 
        CurOperateAnim = null;
        CurRobot = RobotManager.GetInst().GetCurrentRobot();
        actionItem = Resources.Load("Prefabs/actionItem") as GameObject;
        playList = new List<GameObject>();
        CurPlayMode = ActionPlayModel.none;
    }

    public static void Clear()
    {
        _instance = null;
    }

    public void AddToPlayList(GameObject obj)
    {
        playList.Add(obj);
    }

    public void RemoveFromPlayList(GameObject obj)
    {
        playList.Remove(obj);
    }

    public void ClearPlayList()
    {
        playList.Clear();
    }

    public bool IsPlayListNull()
    {
        if (playList != null)
        {
            foreach(var tem in playList)
            {
                if (tem.GetComponentInChildren<UILabel>().text != "")
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    //  private bool _isConnect;
    public bool IsConnect
    {
        get { return PlatformMgr.Instance.GetBluetoothState(); }
    }
    Robot CurRobot;
    string _robotID = "";
    public string RobotID
    {
        get { return _robotID; }
        set
        {
            _robotID = value;
            if (_robotID != "")
                CurRobot = RobotManager.GetInst().GetRobotForID(_robotID);
        }
    }


    #region  //界面显示

    #endregion

    #region //动作逻辑控制
    public bool IsIdExist(string id)
    {
        bool f = false;
        foreach (var tem in playList)
        {
            if (tem.name == id)
            {
                f = true;
                break;
            }
        }
        return f;
    }
    public string GetCurActId()
    {
        string str = "";
        if (CurOperateAnim != null)
        {
            str = CurOperateAnim.name;
        }
        return str;
    }

    public string GetNowPlayingActionId()
    {
        return nowPlayingActionId;
        //if (runState == RunState.Run)
        //{
        //    return nowPlayingActionName;
        //}
        //else
        //    return "";
    }

    public string GetNowPauseActionId()
    {
        return nowPauseActionId;
        //if (runState == RunState.Pause)
        //{
        //    return nowPauseActionName;
        //}
        //else
        //    return "";
    }

    /// <summary>
    /// 删除某个动作
    /// </summary>
    public void DeletaAction()
    {
        if (CurOperateAnim == null)
            return;
        string actionNM = CurOperateAnim.name;//CurOperateAnim.transform.GetChild(0).GetComponent<UILabel>().text;
        RobotManager.GetInst().GetCurrentRobot().DeleteActionsForID(actionNM);
    }

    /// <summary>
    /// 当前动作是否为空
    /// </summary>
    /// <returns></returns>
    public bool IsNullOfCuraction()
    {
        return CurOperateAnim == null;
    }

    public RunState GetCurActionState()
    {
        if (CurOperateAnim == null)
            return RunState.none;
        return runState;
    }

    /// <summary>
    /// 选中一个动作item
    /// </summary>
    /// <param name="go"></param>
    public void DoSelectItem(GameObject go) // 点击了动作单元 用于模型的首页时的控制
    {
        if (go == null)
        {
            if (CurOperateAnim != null)
                CurOperateAnim.GetComponent<UISprite>().SetDimensions(100, 100);
            CurOperateAnim = null;
            return;
        }
        if (CurOperateAnim != null && go != CurOperateAnim) //切换
        {
            if (nowPlayingActionId == go.name)
            {
                runState = RunState.Run;
            }
            else if (nowPauseActionId == go.name)
            {
                runState = RunState.Pause;
            }
            else
                runState = RunState.notRun;

            //
            CurOperateAnim.GetComponent<UISprite>().SetDimensions(100, 100);
        }
        CurOperateAnim = go;
        go.GetComponent<UISprite>().width = 117;
        go.GetComponent<UISprite>().height = 117;
        ActionId = go.name;
    }

    void PlayOfficialEvent(string actid)
    {
        if (CurRobot != null)
        {
            if (CurRobot.IsOfficialForId(actid))
            {
                ActionSequence actions = CurRobot.GetActionsForID(actid);
                if (null != actions)
                {
                    string arg = RobotMgr.NameNoType(CurRobot.Name) + actions.ActionName;
                }
            }
        }
    }
    string ActionId = "";
    /// <summary>
    /// 播放按钮被按下
    /// </summary>
    /// <param name="go"></param>
    public void OnPlayBtnClicked(GameObject go)
    {
        string actionNM = ActionId;
//#if UNITY_EDITOR
//        if (CurOperateAnim == null)
//            return;
//#else
        if (CurOperateAnim == null)
            return;
        else if(!IsConnect || CurRobot == null)  //未连接的情况 模型运动 
        {
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("connectRobotTip"), 1, CommonTipsColor.red);
            //  PlayActionOnlyMode(actionNM);
            CurRobot.PlayActionsForID(actionNM);
            PlayOfficialEvent(actionNM);
            return;
        }
//#endif
        //string actionNM = ActionName;//CurOperateAnim.transform.GetChild(0).GetComponent<UILabel>().text;
        switch (runState)
        {
            case RunState.notRun:
                DoPlay(actionNM, go.transform.GetChild(0).GetComponent<UISprite>(), "icon_stop", "icon_play");
                PlayOfficialEvent(actionNM);
                break;
            case RunState.Pause:
                DoPlayContinue(actionNM,go.transform.GetChild(0).GetComponent<UISprite>(),"icon_play");   //继续播放
                //DoPlayContinue()
                if (go.transform.GetChild(0).GetComponent<UISprite>() != null)
                    go.transform.GetChild(0).GetComponent<UISprite>().spriteName = "icon_stop";
                break;
            case RunState.Run:
                DoPause(actionNM); //暂停
                if (go.transform.GetChild(0).GetComponent<UISprite>() != null)
                    go.transform.GetChild(0).GetComponent<UISprite>().spriteName = "icon_play";
                break;
            case RunState.none:
                DoImitatePlay(actionNM);
                break;
            default:
                break;
        }
    }

    private RunState runState;
    /// <summary>
    /// 遥控界面的动作控制，按下时 状态改变，时间到之后状态切换正常
    /// </summary>
    /// <param name="go"></param>
    public void DoTouchItem_New(GameObject go)
    {
        if (!IsConnect)
        {
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("connectRobotTip"), 1, CommonTipsColor.red);
            return;
        }
        if (CurOperateAnim != null && RunState.Run == runState) //正在播放
        {
#if PlayMode_new
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("请先停止播放"), 1, CommonTipsColor.red);
            return;
#else
            #region  //动作正在播放 , 切换动作时

            if (go == CurOperateAnim) //当前的正在播放
            {
                return;
            }
            else//其它的动作正在播放
            {
               // CurOperateAnim.transform.GetComponent<UISprite>().spriteName = "icon_control";   //上一个状态回归正常
                runState = RunState.notRun;
                CurOperateAnim = go;
                RobotManager.GetInst().GetCurrentRobot().StopNowPlayActions(); //停止当前动作
            }
            #endregion
#endif
        }
        else if (runState == RunState.notRun)
        {
            CurOperateAnim = go;
        }
        string actionNM = go.transform.GetChild(1).GetComponent<UILabel>().text;
#if PlayMode_new
    //    PlayAction(actionNM);
        DoPlay(actionNM, null, "icon_control_sel", "icon_control"); //播放前后sprite改变
#else
        //播放前后的图标切换
        DoPlay(actionNM, null,"icon_control_sel","icon_control"); //播放前后sprite改变
#endif
    }

    /// <summary>
    /// 播放前后 sprite改变
    /// </summary>
    /// <param name="actionNm"></param>
    /// <param name="sprite"></param>
    /// <param name="preNm"></param>
    /// <param name="finalNm"></param>
    string nowPlayingActionId = "";
    string nowPauseActionId = "";
    void DoPlay(string actionNm, UISprite sprite,string preNm, string finalNm)
    {
#if UNITY_EDITOR
        PlayAction(actionNm);

        // 变成暂停的图标 表示可以点击暂停 播放完之后 图标变为三角形
        if(sprite != null)
            sprite.spriteName = preNm;
        ClientMain.GetInst().StartCoroutine(DoDuration(actionTimeDuration / 1000.0f, sprite, finalNm));
#else
        if (IsConnect && CurRobot != null)
        {
            PlayAction(actionNm);

            // 变成暂停的图标 表示可以点击暂停 播放完之后 图标变为三角形
            if(sprite != null)
                sprite.spriteName = preNm;
            ClientMain.GetInst().StartCoroutine(DoDuration(actionTimeDuration / 1000.0f,sprite ,finalNm));
        }
        else
        {
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("connectRobotTip"), 1, CommonTipsColor.red);
        }
#endif
    }

    /// <summary>
    /// 直接点击curOperateAnim的播放模式
    /// </summary>
    float actionTimeDuration = 0f;  // 一个动作需要的时间
    void DoPlay(string actionNm)
    {
        if (IsConnect && CurRobot != null)
        {
            CurRobot.PlayActionsForID(actionNm);
            runState = RunState.Run;
            // 变成暂停的图标 表示可以点击暂停 播放完之后 图标变为三角形
            Transform temp = CurOperateAnim.transform.GetChild(2); //UI图标切换
            if (temp != null)
            {
                if (temp.GetComponent<UISprite>() != null)
                    temp.GetComponent<UISprite>().spriteName = "icon_pause";
            }
            PlayAction(actionNm);
            ClientMain.GetInst().StartCoroutine(DoDuration(actionTimeDuration / 1000.0f));
        }
        else
        {
            SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("connectRobotTip"), 1, CommonTipsColor.red);
        }
    }

    /// <summary>
    /// 播放模型动作
    /// </summary>
    /// <param name="actionNm"></param>
    void PlayActionOnlyMode(string actionNm)
    {
        ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(ActionId);
        EventMgr.Inst.Fire(EventID.Ctrl_Robot_Action, new EventArg(act));
    }

    void PlayAction(string actionNm)
    {
#if UNITY_EDITOR
        runState = RunState.Run;

        nowPauseActionId = "";
        nowPlayingActionId = actionNm;
        actionTimeDuration = 8000f;
        CurRobot.PlayActionsForID(actionNm);
#else
        CurRobot.PlayActionsForID(actionNm);
        runState = RunState.Run;

        nowPauseActionId = "";
        nowPlayingActionId = actionNm;
        //获取播放的时间 时间结束之后就
        float duration = CurRobot.GetActionsTimeForID(actionNm);
        //如果最后一帧有轮模式，则当前时间定为一个小时，
        if (CurRobot.IsTurnModelForID(actionNm))
            duration = 3600000f;
        actionTimeDuration = duration;
#endif
    }

    /// <summary>
    /// 执行某个动作的时长，多长时间后动作播放结束时图标切换回来 
    /// </summary>
    float timeDuration = 0;
    int coroutineKey = 0;
    IEnumerator DoDuration(float time)
    {
        timeDuration = Time.time; //当前动作开始执行的时间
        int myKey;
        UISprite temSprite = CurOperateAnim.transform.GetChild(2).GetComponent<UISprite>();
        /*
            mykey： 协程内部钥匙
            coroutinekey: 协程配对的唯一钥匙
            要求：每次只执行匹配正确后的内容
            同样的协程被开启多次时，唯一的钥匙时刻被改变，而如果在任意一个协程等待期间，唯一钥匙被其它协
            程改变时，将会导致匹配不对的协程内容不再执行
            从而保证了多个同样的协程不相互影响， 尼玛想了很久才得到的天才设计
        */
        myKey = ++coroutineKey;
        yield return new WaitForSeconds(time);
        if (coroutineKey == myKey && runState == RunState.Run)
        {
            temSprite.spriteName = "icon_play";
            runState = RunState.notRun;
            nowPauseActionId = "";
            nowPlayingActionId = "";

            if (GetNextAction() != null)
                ClientMain.GetInst().StartCoroutine(DoNextFrame());
                //DoTouchItem_New(GetNextAction());
        }
        if (coroutineKey == myKey)  //表示最新执行的动作结束
        {
            nowPauseActionId = "";
            nowPlayingActionId = "";
        }
    }
    IEnumerator DoNextFrame()
    {
        yield return null;
        DoTouchItem_New(GetNextAction());
    }
    /// <summary>
    /// 按钮的颜色状态改变
    /// </summary>
    /// <param name="time"></param>
    /// <param name="shiftSprite"></param>
    /// <returns></returns>
    IEnumerator DoDuration(float time, UISprite shiftSprite)
    {
        timeDuration = Time.time; //当前动作开始执行的时间
        int myKey;
        /*
            mykey： 协程内部钥匙
            coroutinekey: 协程配对的唯一钥匙
            要求：每次只执行匹配正确后的内容
            同样的协程被开启多次时，唯一的钥匙时刻被改变，而如果在任意一个协程等待期间，唯一钥匙被其它协
            程改变时，将会导致匹配不对的协程内容不再执行
            从而保证了多个同样的协程不相互影响， 尼玛想了很久才得到的天才设计
        */
        myKey = ++coroutineKey;
        yield return new WaitForSeconds(time);
        if (coroutineKey == myKey && runState == RunState.Run)
        {
            if (shiftSprite.GetComponent<UIButtonColor>() == null)
                shiftSprite.gameObject.AddComponent<UIButtonColor>();
            shiftSprite.GetComponent<UIButtonColor>().state = UIButtonColor.State.Normal;
            runState = RunState.notRun;
            nowPauseActionId = "";
            nowPlayingActionId = "";

            if (GetNextAction() != null)
                ClientMain.GetInst().StartCoroutine(DoNextFrame());
               // DoTouchItem_New(GetNextAction());
        }
        if (coroutineKey == myKey)  //表示最新执行的动作结束
        {
            nowPauseActionId = "";
            nowPlayingActionId = "";
        }
    }
    /// <summary>
    /// 按钮图标改变
    /// </summary>
    /// <param name="time"></param>
    /// <param name="shiftSprite"></param>
    /// <param name="finalName"></param>
    /// <returns></returns>
    IEnumerator DoDuration(float time,UISprite shiftSprite, string finalName)
    {
        timeDuration = Time.time; //当前动作开始执行的时间
        int myKey;
        /*
            mykey： 协程内部钥匙
            coroutinekey: 协程配对的唯一钥匙
            要求：每次只执行匹配正确后的内容
            同样的协程被开启多次时，唯一的钥匙时刻被改变，而如果在任意一个协程等待期间，唯一钥匙被其它协
            程改变时，将会导致匹配不对的协程内容不再执行
            从而保证了多个同样的协程不相互影响， 尼玛想了很久才得到的天才设计
        */
        myKey = ++coroutineKey;
        yield return new WaitForSeconds(time);
        if (coroutineKey == myKey && runState == RunState.Run)
        {
            if(shiftSprite != null)
                shiftSprite.spriteName = finalName;
            runState = RunState.notRun;

            nowPlayingActionId = "";
            nowPauseActionId = "";

            if(GetNextAction() != null)
                ClientMain.GetInst().StartCoroutine(DoNextFrame());
                //DoTouchItem_New(GetNextAction());
        }
        if (coroutineKey == myKey)  //表示最新执行的动作结束
        {
            nowPauseActionId = "";
            nowPlayingActionId = "";
        }
    }

    void DoPause(string actionNm)     //暂停某个动作
    {
#if UNITY_EDITOR
        runState = RunState.Pause;
        nowPauseActionId = actionNm;
        nowPlayingActionId = "";

   //     CurRobot.PauseActionsForName(actionNm);   //暂停
        timeDuration = Time.time - timeDuration; // 从开始执行到暂停所花费的时间
#else
        if (IsConnect && CurRobot != null)
        {
            runState = RunState.Pause;
            nowPauseActionId = actionNm;
            nowPlayingActionId = "";

            CurRobot.PauseActionsForID(actionNm);   //暂停
            timeDuration = Time.time - timeDuration; // 从开始执行到暂停所花费的时间
        }
#endif
    }


    void DoPlayContinue(string actionNm,UISprite shiftSprite,string finalName)   //继续播放某个动作
    {
#if UNITY_EDITOR
        runState = RunState.Run;  // 播放状态
    //    CurRobot.ContinueActionsForName(actionNm); //继续播放

        nowPlayingActionId = actionNm;
        nowPauseActionId = "";

        actionTimeDuration -= timeDuration * 1000;
        ClientMain.GetInst().StartCoroutine(DoDuration(actionTimeDuration / 1000.0f, shiftSprite, finalName));
#else
        if (IsConnect && CurRobot != null)
        {
            runState = RunState.Run;  // 播放状态
            CurRobot.ContinueActionsForID(actionNm); //继续播放

            nowPlayingActionId = actionNm;
            nowPauseActionId = "";

            actionTimeDuration -= timeDuration * 1000;
            ClientMain.GetInst().StartCoroutine(DoDuration(actionTimeDuration / 1000.0f, shiftSprite, finalName));
            //ClientMain.GetInst().StartCoroutine(DoDuration(actionTimeDuration / 1000.0f));   //继续那么长时间后 直到播放完毕
        }
#endif
    }

    /// <summary>
    /// 动作停止
    /// </summary>
    /// <param name="actionNm"></param>
    public void DoStopAction(GameObject go)
    {
        if (CurRobot == null)
            return;
        if(!IsConnect)  //未连接的情况 模型运动 
        {
            CurRobot.StopNowPlayActions();
            return;
        }
        if (IsConnect)
        {
            StopAction();
            return;
        }
    }

    public void StopAction()
    {
        runState = RunState.notRun;
#if UNITY_EDITOR
#else
        CurRobot.StopNowPlayActions();
#endif
        nowPauseActionId = "";
        nowPlayingActionId = "";
    }

    public void ActionExceptionStop()
    {
        runState = RunState.notRun;
        nowPauseActionId = "";
        nowPlayingActionId = "";
    }

    void DoImitatePlay(string actionNm)  //模拟播放某个动作
    {
        if (CurRobot != null)
        {
            //...

        }
    }
    #endregion

    #region //播放模式
    public ActionPlayModel CurPlayMode;

    /// <summary>
    /// 获取下一个要播放的动作
    /// 前置条件播放列表里的动作都要有效
    /// </summary>
    /// <returns></returns>
    public GameObject GetNextAction()
    {
        GameObject objAction = null;
        switch (CurPlayMode)
        { 
            case ActionPlayModel.random:
                objAction = playList[Random.Range(0, playList.Count)];
                break;
            case ActionPlayModel.sequence:
                if (playList.Contains(CurOperateAnim))
                {
                    int index = playList.IndexOf(CurOperateAnim);
                    if (index == playList.Count - 1)
                        objAction = null;
                    else
                        objAction = playList[index + 1];
                }
                else objAction = null;
                break;
            case ActionPlayModel.sequenceCircle:
                if (playList.Contains(CurOperateAnim))
                {
                    int index = playList.IndexOf(CurOperateAnim);
                    objAction = playList[++index % playList.Count];
                }
                else objAction = null;
                break;
            case ActionPlayModel.singleCircle:
                if (playList.Contains(CurOperateAnim))
                    objAction = CurOperateAnim;
                else
                    objAction = null;
                break;
            default:
                objAction = null;
                break;
        }
        return objAction;
    }

    //public void SetActionPlayMode()
    //{
    //    if(model ==)
    //    curPlayMode = model;
    //}
    #endregion
}

public enum WindowType
{
    swap,
    neww,
    other
}
//运行状态枚举
public enum RunState
{
    notRun,
    Run,
    Pause,
    none
}
public enum ActionPlayModel
{ 
    singleCircle,
    sequenceCircle,
    sequence,
    random,
    none,
}
