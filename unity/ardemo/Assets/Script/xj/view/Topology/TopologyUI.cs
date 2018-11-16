using Game;
using Game.Event;
using Game.Platform;
using Game.Resource;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:TopologyUI.cs
/// Description:拓扑图的UI，用于嵌套在其他界面
/// Time:2016/8/18 11:02:51
/// </summary>
/// 
public class TopologyUI : BaseUI
{
    #region 公有属性
    public delegate void ChoicePartActiveCallBack(bool activeFlag);
    public Transform mSpeakerTrans;
    #endregion

    #region 其他属性
    protected enum TopologyType
    {
        Topology_Default = 0,
        Topology_Playerdata,
    }

    eUICameraType mCameraType = eUICameraType.OrthographicTwo;

    protected TopologyType mTopologyType;
    protected Robot mRobot;
    protected ServosConnection mServosConnection;
    
    //需要保存的数据
    Dictionary<GameObject, TopologyPartData> mTopologyDict;
    Dictionary<GameObject, TopologyPartData> mIndependentQueueDict;
    Dictionary<GameObject, TopologyPartData> mLineDict;
    Dictionary<GameObject, GameObject> mPortConnectionDict;
    //可以根据保存信息生成数据
    Dictionary<GameObject, List<GameObject>> mPartLineDict;
    Dictionary<GameObject, List<PartPortData>> mPartPortDict;

    Dictionary<Transform, UpdateShowData> mUpdateDict;

    Dictionary<TopologyPartType, List<byte>> mAddNewSensorDict;

    Transform mPanelSelfTrans;
    Transform mPanelDefaultTrans;
    Transform mIndependentQueueTrans;
    Transform mQueuePanelTrans;
    Transform mQueueDragTrans;

    Transform mPin3Trans;
    Transform mPin4Trans;
    GameObject mChooseCloseBtnObj;

    Transform mMotherBoxTrans;

    UIScrollView mIndependentScrollView;
    UIScrollView mTopologyPanelScrollView;

    Vector2 mServoSize;
    Vector2 mSpaceSize = new Vector2(1, 1);
    Vector2 mAngleSize = new Vector2(4, 4);
    Vector3 mPinTransPos = Vector3.zero;
    Vector3 mTopologyStartPos = Vector3.zero;

    GameObject mBgDragObj;
    GameObject mServoItem;
    GameObject mLineItem;
    GameObject mArcItem;
    GameObject mAngleItem;
    GameObject mSensorItem;
    GameObject mAddItem;
    /// <summary>
    /// 选中的连接物体
    /// </summary>
    GameObject mSelectedConnectionObj;

    /// <summary>
    /// 在拖动的物体
    /// </summary>
    GameObject mDragGameObject;

    int mDepth = 1;

    ButtonDelegate.OnClick mOnClickDelegate;
    ChoicePartActiveCallBack mChoicePartActiveCallBack;
    bool isEdit = false;
    GameObject mSelectedServoObj;
    /// <summary>
    /// 个人拓扑图初始零件的位置
    /// </summary>
    Vector2[] mPlayerStartPos = new Vector2[] { new Vector2(250, -164), new Vector2(250, 0), new Vector2(250, 164), new Vector2(-250, 164), new Vector2(-250, 0), new Vector2(-250, -164)};
    Vector2[] mPlayerStartPos_low = new Vector2[] { new Vector2(250, 240), new Vector2(280, 80), new Vector2(280, -80), new Vector2(250, -240), new Vector2(-250, 240), new Vector2(-280, 80), new Vector2(-280, -80), new Vector2(-250, -240) };

    List<GameObject> mBlankList;
    int mLineWidth = 70;
    ServoModel mChoiceServoModel = ServoModel.Servo_Model_Angle;
    float mSwitchDragOffset;
    /// <summary>
    /// 显示的选择队列的类型
    /// </summary>
    PartPortType mShowChoicePortType = PartPortType.Port_Type_Pin_3;
    /// <summary>
    /// 是否打开了选择列表,true表示打开了
    /// </summary>
    bool mShowChoicePanelFlag = false;
    /// <summary>
    /// 记录选中的零件
    /// </summary>
    GameObject mSelectedPartGameObject;
    /// <summary>
    /// 选中物体的背景
    /// </summary>
    Transform mSelectedBgTrans;
    /// <summary>
    /// 连接图适配屏幕大小
    /// </summary>
    TransformAdaptation mTransformAdaptation;
    /// <summary>
    /// 主板类型
    /// </summary>
    TopologyPartType mMainboardType = TopologyPartType.None;


    Dictionary<TopologyPartType, List<byte>> conSensorDict = null;

    int QueuePanelTransBottomMargin = 210;
    Vector2 QueuePanelTransPos = Vector2.zero;
    #endregion

    #region 公有函数
    public TopologyUI(Transform parentTrans)
    {
        mUIResPath = "Prefab/UI/TopologyUI";
        mParentTrans = parentTrans;

        mTopologyDict = new Dictionary<GameObject, TopologyPartData>();
        mIndependentQueueDict = new Dictionary<GameObject, TopologyPartData>();
        mLineDict = new Dictionary<GameObject, TopologyPartData>();
        mPartLineDict = new Dictionary<GameObject, List<GameObject>>();
        mPortConnectionDict = new Dictionary<GameObject, GameObject>();
        mPartPortDict = new Dictionary<GameObject, List<PartPortData>>();
        mUpdateDict = new Dictionary<Transform, UpdateShowData>();
        mBlankList = new List<GameObject>();
        mAddNewSensorDict = new Dictionary<TopologyPartType, List<byte>>();
    }

    public TopologyUI(eUICameraType cameraType)
    {
        mUIResPath = "Prefab/UI/TopologyUI";
        mCameraType = cameraType;

        mTopologyDict = new Dictionary<GameObject, TopologyPartData>();
        mIndependentQueueDict = new Dictionary<GameObject, TopologyPartData>();
        mLineDict = new Dictionary<GameObject, TopologyPartData>();
        mPartLineDict = new Dictionary<GameObject, List<GameObject>>();
        mPortConnectionDict = new Dictionary<GameObject, GameObject>();
        mPartPortDict = new Dictionary<GameObject, List<PartPortData>>();
        mUpdateDict = new Dictionary<Transform, UpdateShowData>();
        mBlankList = new List<GameObject>();
        mAddNewSensorDict = new Dictionary<TopologyPartType, List<byte>>();
    }

    public override void Init()
    {
        base.Init();
        mUICameraType = mCameraType;
    }

    public override void LoadUI()
    {
        if (string.IsNullOrEmpty(mUIResPath))
        {
            return;
        }
        mState = eBaseUIState.Loding;
        GameObject obj = Resources.Load(mUIResPath, typeof(GameObject)) as GameObject;
        if (null != obj)
        {
            GameObject o = UnityEngine.Object.Instantiate(obj) as GameObject;
            o.name = obj.name;
            mTrans = o.transform;
            mPanel = o.GetComponent<UIPanel>();
            if (null == mPanel)
            {
                mPanel = o.AddComponent<UIPanel>();
            }
            if (null != mParentTrans)
            {
                mTrans.parent = mParentTrans;
                mTrans.localPosition = Vector3.zero;
                mTrans.localScale = Vector3.one;
                mTrans.localEulerAngles = Vector3.zero;
                PublicFunction.SetLayerRecursively(mTrans.gameObject, mParentTrans.gameObject.layer);
            }
            else
            {
                SingletonObject<UIManager>.Inst.InitUI(mSide, mUICameraType, mTrans);
            }
            mBtnDelegate = new ButtonDelegate();
            mBtnDelegate.onClick = OnButtonClick;
            mBtnDelegate.onDrag = OnButtonDrag;
            mBtnDelegate.onPress = OnButtonPress;
            mBtnDelegate.onDurationClick = OnDurationClick;
            mBtnDelegate.onDurationPress = OnDurationPress;

            if (null != mSceneDelegate)
            {
                UIManager.SetButtonEventDelegate(mTrans, mSceneDelegate);
            }
            else
            {
                UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
            }
        }
    }


    public override void Release()
    {
        base.Release();
        EventMgr.Inst.UnRegist(EventID.Item_Drag_Drop_Start, ItemDragDropStart);
        EventMgr.Inst.UnRegist(EventID.Item_Drag_Drop_End, ItemDragDropEnd);
    }

    public void ResetTopology()
    {
        RecalculateTopology();
        /*if (null != mPanelSelfTrans)
        {
            mPanelSelfTrans.localScale = Vector3.one;
        }
        if (null != mTopologyPanelScrollView)
        {
            SpringPanel springPanel = mTopologyPanelScrollView.gameObject.GetComponent<SpringPanel>();
            if (null == springPanel || !springPanel.enabled)
            {
                SpringPanel.Begin(mTopologyPanelScrollView.gameObject, mTopologyStartPos, 13f);
            }
        }*/
    }
    /// <summary>
    /// 重新计算连接图适配
    /// </summary>
    public void RecalculateTopology()
    {
        if (null != mTransformAdaptation)
        {
            float h = 0;
            float startH = 0;
            if (mIndependentQueueDict.Count > 0)
            {
                h = PublicFunction.GetHeight() - 360;
                startH = 80;
            }
            else
            {
                h = PublicFunction.GetHeight() - 180;
                startH = 0;
            }
            mTransformAdaptation.Recalculate(new Vector4(0, startH, PublicFunction.GetWidth() * 0.9f, h), delegate (Transform trans)
            {
                /*if (null != mIndependentQueueTrans)
                {
                    mIndependentQueueTrans.localScale = trans.localScale;
                }
                if (null != mQueuePanelTrans)
                {
                    mQueuePanelTrans.localPosition = QueuePanelTransPos - new Vector2(0, 110 * (1 - trans.localScale.x));
                }*/
            });
        }
    }
    /// <summary>
    /// 把拓扑图还原成原比例大小
    /// </summary>
    public void ResetNormalScale()
    {
        if (null != mTransformAdaptation)
        {
            float h = 0;
            float startH = 0;
            if (mIndependentQueueDict.Count > 0)
            {
                h = PublicFunction.GetHeight() - 360;
                startH = 80;
            }
            else
            {
                h = PublicFunction.GetHeight() - 180;
                startH = 0;
            }
            mTransformAdaptation.RemovePosition(new Vector4(0, startH, PublicFunction.GetWidth() * 0.9f, h), delegate (Transform trans)
            {
                /*if (null != mIndependentQueueTrans)
                {
                    mIndependentQueueTrans.localScale = trans.localScale;
                }*/
            });
        }
    }

    public void SetDepth(int depth)
    {
        mDepth = depth;
    }
    /// <summary>
    /// 设置零件状态
    /// </summary>
    /// <param name="mainResult"></param>
    /// <param name="servoUpdateList"></param>
    public void SetPartState(ErrorCode mainResult, List<byte> servoUpdateList, Dictionary<TopologyPartType, List<byte>> sensorUpdateList)
    {
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            CheckPartState(kvp.Key, kvp.Value, mainResult, servoUpdateList, sensorUpdateList);
        }
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
        {
            CheckPartState(kvp.Key, kvp.Value, mainResult, servoUpdateList, sensorUpdateList);
        }
    }

    public void OpenMainBoardUpdateAnim()
    {
        mUpdateDict.Clear();
        if (null != mMotherBoxTrans)
        {
            AddUpdateItem(mMotherBoxTrans);
        }
        OpenUpdateAnim();
    }

    public void OpenServoUpdateAnim(List<byte> servoUpdateList)
    {
        mUpdateDict.Clear();
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            if (kvp.Value.partType == TopologyPartType.Servo && servoUpdateList.Contains(kvp.Value.id))
            {
                AddUpdateItem(kvp.Key.transform);
            }
        }
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
        {
            if (kvp.Value.partType == TopologyPartType.Servo && servoUpdateList.Contains(kvp.Value.id))
            {
                AddUpdateItem(kvp.Key.transform);
            }
        }
        OpenUpdateAnim();
    }

    public void OpenSensorUpdateAnim(TopologyPartType sensorType, List<byte> sensorUpdateList)
    {
        mUpdateDict.Clear();
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            if (kvp.Value.partType == sensorType && sensorUpdateList.Contains(kvp.Value.id))
            {
                AddUpdateItem(kvp.Key.transform);
            }
        }
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
        {
            if (kvp.Value.partType == sensorType && sensorUpdateList.Contains(kvp.Value.id))
            {
                AddUpdateItem(kvp.Key.transform);
            }
        }
        OpenUpdateAnim();
    }

    public void UpdateFinishedAnim(bool state, bool instant)
    {
        foreach (KeyValuePair<Transform, UpdateShowData> kvp in mUpdateDict)
        {
            StateUpdateFinished(kvp.Key, state, instant);
            kvp.Value.UptateFinished();
        }
    }

    public void UpdateProgress(int progress)
    {
        foreach (KeyValuePair<Transform, UpdateShowData> kvp in mUpdateDict)
        {
            kvp.Value.UpdateProgress(progress);
        }
    }

    public void UpdateWait()
    {
        foreach (KeyValuePair<Transform, UpdateShowData> kvp in mUpdateDict)
        {
            kvp.Value.UpdateWait();
        }
    }

    public void RefreshIndependent()
    {
        mAddNewSensorDict.Clear();
        if (null != mIndependentQueueTrans)
        {
            List<GameObject> delList = new List<GameObject>();
            for (int i = 0, imax = mIndependentQueueTrans.childCount; i < imax; ++i)
            {
                GameObject obj = mIndependentQueueTrans.GetChild(i).gameObject;
                mPartPortDict.Remove(obj);
                delList.Add(obj);
            }
            for (int i = 0, imax = delList.Count; i < imax; ++i)
            {
                GameObject.Destroy(delList[i]);
            }
            mIndependentQueueDict.Clear();
            mIndependentQueueTrans.DetachChildren();
            InitIndependentQueueUI();
        }
        
    }
    /// <summary>
    /// 更新模型
    /// </summary>
    /// <param name="robot"></param>
    public void UpdateRobot(Robot robot)
    {
        mRobot = robot;
    }
    /// <summary>
    /// 隐藏零件状态
    /// </summary>
    public void HidePartState()
    {
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            HideState(kvp.Key.transform);
        }
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
        {
            HideState(kvp.Key.transform);
        }
    }
    /// <summary>
    /// 保存拓扑图数据
    /// </summary>
    public void SaveTopologyData()
    {
        if (null == mServosConnection)
        {
            mServosConnection = new ServosConnection();
            mServosConnection.MainboardType = mMainboardType;
        }
        else
        {
            mServosConnection.CleanUp();
        }
        try
        {
            //获取端口连接的舵机数据,适配老版本，新版本不需要此数据
            for (int i = 1; i <= 8; ++i)
            {
                Transform port = mMotherBoxTrans.Find(string.Format("port{0}", i));
                if (null != port)
                {
                    if (mPortConnectionDict.ContainsKey(port.gameObject))
                    {
                        int portNum = int.Parse(port.name.Substring("port".Length));
                        List<int> servosList = new List<int>();
                        GameObject nextPort = mPortConnectionDict[port.gameObject];
                        GameObject otherPort = null;
                        while (null != nextPort)
                        {
                            if (nextPort.name.StartsWith("s_"))
                            {//舵机
                                int id = GetPortId(nextPort.name);
                                if (0 != id)
                                {
                                    servosList.Add(id);
                                }
                            }
                            otherPort = GetPartOtherPort(nextPort);
                            if (mPortConnectionDict.ContainsKey(otherPort))
                            {
                                nextPort = mPortConnectionDict[otherPort];
                            }
                            else
                            {
                                nextPort = null;
                            }
                        }
                        if (servosList.Count > 0)
                        {
                            servosList.Sort();
                            mServosConnection.AddPortServos(portNum, servosList);
                        }
                    }
                }
            }
            //获取独立队列数据
            foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
            {
                mServosConnection.AddTopologyPartData(kvp.Value);
            }
            //获取拓扑图数据
            foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
            {
                if (!kvp.Key.name.Equals("blank"))
                {
                    mServosConnection.AddTopologyPartData(kvp.Value, kvp.Key.transform);
                }
            }
            //获取连线数据
            /*foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mLineDict)
            {
                mServosConnection.AddTopologyPartData(kvp.Value);
            }*/
            //获取连接数据
            foreach (KeyValuePair<GameObject, GameObject> kvp in mPortConnectionDict)
            {
                if (!kvp.Key.name.StartsWith("blank_") && !kvp.Value.name.StartsWith("blank_"))
                {
                    mServosConnection.AddPortConnection(kvp.Key.name, kvp.Value.name);
                }
            }
            if (null != mRobot)
            {
                //获取舵机模式
                List<byte> servosList = mRobot.GetAllDjData().GetIDList();
                for (int i = 0, imax = servosList.Count; i < imax; ++i)
                {
                    DuoJiData data = mRobot.GetAllDjData().GetDjData(servosList[i]);
                    if (null != data)
                    {
                        mServosConnection.UpdateServoModel(data.id, data.modelType);
                    }
                }
                mServosConnection.Save(mRobot);

                SingletonObject<ServosConManager>.GetInst().UpdateServosConnection(mRobot.ID, mServosConnection);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        

    }

    public void OpenEditTopology()
    {
        isEdit = true;
        if (mTopologyType == TopologyType.Topology_Playerdata)
        {
            if (null != mQueuePanelTrans)
            {
                mQueuePanelTrans.gameObject.SetActive(false);
            }
            if (null != mQueueDragTrans)
            {
                mQueueDragTrans.gameObject.SetActive(false);
            }
            CreateAllBlankSpace();
            InitChoicePartQueue();
            AutoSelectPartGameObject();
        }
        else
        {
            OpenIndependentDragDrop();
            OpenTopologyDictDragDrop();
        }
        ResetNormalScale();
        OpenTopologyMove();
    }

    public void CloseEditTopology()
    {
        isEdit = false;
        if (mTopologyType == TopologyType.Topology_Playerdata)
        {
            if (mIndependentQueueTrans.childCount > 0)
            {
                if (null != mQueuePanelTrans)
                {
                    mQueuePanelTrans.gameObject.SetActive(true);
                }
                if (null != mQueueDragTrans)
                {
                    mQueueDragTrans.gameObject.SetActive(true);
                }
                ResetIndependentQueuePosition(false);
            }
            RemoveChoicePartQueue();
        }
        else
        {
            CloseIndependentDragDrop();
            CloseTopologyDictDragDrop();
        }
        RecalculateTopology();
        CloseTopologyMove();
    }
    /// <summary>
    /// 检测拓扑图是否设置完成
    /// </summary>
    /// <returns></returns>
    public bool IsSettingFinished()
    {
        return true;
        //return IsPartSettingFinished(PartPortType.Port_Type_Pin_3) && IsPartSettingFinished(PartPortType.Port_Type_Pin_4);
    }
    /// <summary>
    /// 删除不连续的零件
    /// </summary>
    public void RemoveDisContinuousPart()
    {
        DelDiscontinuousPart();
        /*Dictionary<TopologyPartType, List<byte>> dict = new Dictionary<TopologyPartType, List<byte>>();
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
        {
            if (!dict.ContainsKey(kvp.Value.partType))
            {
                List<byte> list = new List<byte>();
                dict[kvp.Value.partType] = list;
            }
            dict[kvp.Value.partType].Add(kvp.Value.id);
        }
        TopologyPartType[] partTypes = new TopologyPartType[PublicFunction.Open_Topology_Part_Type.Length + 1];
        partTypes[0] = TopologyPartType.Servo;
        for (int i = 1, imax = partTypes.Length; i < imax; ++i)
        {
            partTypes[i] = PublicFunction.Open_Topology_Part_Type[i - 1];
        }
        StringBuilder tips = new StringBuilder();
        for (int i = 0, imax = partTypes.Length; i < imax; ++i)
        {
            if (dict.ContainsKey(partTypes[i]))
            {
                if (tips.Length > 0)
                {
                    tips.Append(PublicFunction.Separator_Comma);
                }
                dict[partTypes[i]].Sort();
                tips.Append(GetMissTips(partTypes[i], PublicFunction.ListToString<byte>(dict[partTypes[i]])));
            }
        }
        PromptMsg.ShowSinglePrompt(string.Format(LauguageTool.GetIns().GetText("零件缺失"), tips.ToString()));*/
    }

    /// <summary>
    /// 设置按钮委托
    /// </summary>
    /// <param name="dlgt"></param>
    public void SetOnClickDelegate(ButtonDelegate.OnClick dlgt)
    {
        mOnClickDelegate = dlgt;
    }
    /// <summary>
    /// 设置选择队列隐藏或者显示
    /// </summary>
    /// <param name="callBack"></param>
    public void SetChoicePartActiveCallBack(ChoicePartActiveCallBack callBack)
    {
        mChoicePartActiveCallBack = callBack;
    }
    /// <summary>
    /// 恢复拓扑图
    /// </summary>
    public void RecoverTopology()
    {
        //删除独立队列
        List<GameObject> delList = new List<GameObject>();
        for (int i = mIndependentQueueTrans.childCount - 1; i >= 0; --i)
        {
            delList.Add(mIndependentQueueTrans.GetChild(i).gameObject);
        }
        for (int i = 0 ,imax = delList.Count; i < imax; ++i)
        {
            GameObject.Destroy(delList[i]);
        }
        mIndependentQueueTrans.DetachChildren();
        mIndependentQueueDict.Clear();
        //删除连线
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mLineDict)
        {
            GameObject.Destroy(kvp.Key);
        }
        mLineDict.Clear();
        //删除拓扑图零件
        TopologyPartData mainData = null;
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            if (kvp.Value.partType != TopologyPartType.MainBoard && kvp.Value.partType != TopologyPartType.MainBoard_new_low)
            {
                GameObject.Destroy(kvp.Key);
            }
            else
            {
                mainData = kvp.Value;
            }
        }
        mTopologyDict.Clear();
        if (null != mainData)
        {
            mTopologyDict[mMotherBoxTrans.gameObject] = mainData;
        }
        mPortConnectionDict.Clear();
        mPartLineDict.Clear();

        List<PartPortData> list = null;
        if (mPartPortDict.ContainsKey(mMotherBoxTrans.gameObject))
        {
            list = mPartPortDict[mMotherBoxTrans.gameObject];
        }
        mPartPortDict.Clear();
        if (null != list)
        {
            mPartPortDict[mMotherBoxTrans.gameObject] = list;
        }
        if (null != mRobot)
        {//恢复舵机模式
            ModelDjData servosData = mRobot.GetAllDjData();
            List<byte> servoList = servosData.GetIDList();
            for (int i = 0, imax = servoList.Count; i < imax; ++i)
            {
                if (null != mServosConnection)
                {
                    servosData.UpdateServoModel(servoList[i], mServosConnection.GetServoModel(servoList[i]));
                }
                else
                {
                    servosData.UpdateServoModel(servoList[i], ServoModel.Servo_Model_Angle);
                }
            }
        }
        
        SetPartPortState(mMotherBoxTrans.gameObject);
        InitUI();
        HidePartState();
    }
    /// <summary>
    /// 隐藏选择列表
    /// </summary>
    /// <param name="instant"></param>
    public void HideChoicePartPanel(bool instant)
    {
        if (mShowChoicePanelFlag)
        {
            SetSelectedPart(null);
            HidePartQueue(instant);
            if (null != mChoicePartActiveCallBack)
            {
                mChoicePartActiveCallBack(false);
            }
        }
    }

    public void OpenTopologyMove()
    {
        if (null != mTopologyPanelScrollView)
        {
            mTopologyPanelScrollView.enabled = true;
        }
        if (null != mTrans)
        {
            TouchManager touchManager = GameHelper.FindChildComponent<TouchManager>(mTrans, "center");
            if (null != touchManager)
            {
                touchManager.enabled = true;
            }
        }
    }

    public void CloseTopologyMove()
    {
        if (null != mTopologyPanelScrollView)
        {
            mTopologyPanelScrollView.enabled = false;
        }
        if (null != mTrans)
        {
            TouchManager touchManager = GameHelper.FindChildComponent<TouchManager>(mTrans, "center");
            if (null != touchManager)
            {
                touchManager.enabled = false;
            }
        }
    }
    /// <summary>
    /// 保存传感器数据
    /// </summary>
    public void SaveTopologySensorData()
    {
        if (null != mRobot && null != mRobot.MotherboardData && null != mServosConnection)
        {
            bool changeFlag = false;
            foreach (var kvp in mIndependentQueueDict)
            {
                if (kvp.Value.partType < TopologyPartType.Infrared || kvp.Value.partType >= TopologyPartType.Motor)
                {
                    continue;
                }
                SensorData sensorData = mRobot.MotherboardData.GetSensorData(kvp.Value.partType);
                if (null == sensorData || !sensorData.ids.Contains(kvp.Value.id))
                {//删除不存在的传感器
                    mServosConnection.DelIndependentTopologyData(kvp.Value);
                    changeFlag = true;
                }
                else if (mAddNewSensorDict.ContainsKey(kvp.Value.partType) && mAddNewSensorDict[kvp.Value.partType].Contains(kvp.Value.id) && !sensorData.errorIds.Contains(kvp.Value.id))
                {
                    mServosConnection.AddTopologyPartData(kvp.Value);
                    changeFlag = true;
                }
            }
            if (changeFlag)
            {
                mServosConnection.Save(mRobot);
            }
        }
    }

#if UNITY_EDITOR
    public void AddIndependentSensor(byte id, TopologyPartType sensorType)
    {
        GameObject obj = CreateSensorGameObject(id, sensorType, mIndependentQueueTrans, false);
        if (null != obj)
        {
            //obj.transform.localPosition = new Vector3(mServoSize.x / 2 + i * (mServoSize.x + 46), 0);
            TopologyPartData sensorData = new TopologyPartData();
            sensorData.id = id;
            sensorData.isIndependent = true;
            sensorData.partType = sensorType;
            sensorData.width = (int)mServoSize.x;
            sensorData.height = (int)mServoSize.y;
            mIndependentQueueDict[obj] = sensorData;
            ResetIndependentQueuePosition(false);
            UIManager.SetButtonEventDelegate(mIndependentQueueTrans, mBtnDelegate);
            if (null != mQueuePanelTrans)
            {
                mQueuePanelTrans.gameObject.SetActive(true);
            }
            if (null != mQueueDragTrans)
            {
                mQueueDragTrans.gameObject.SetActive(true);
            }
        }
    }

    public bool IsEdit()
    {
        return isEdit;
    }
#endif
    #endregion

    #region 其他函数

    protected override void AddEvent()
    {
        base.AddEvent();
        EventMgr.Inst.Regist(EventID.Item_Drag_Drop_Start, ItemDragDropStart);
        EventMgr.Inst.Regist(EventID.Item_Drag_Drop_End, ItemDragDropEnd);

        if (RecordContactInfo.Instance.openType == "default")
        {
            mTopologyType = TopologyType.Topology_Default;
        }
        else
        {
            mTopologyType = TopologyType.Topology_Playerdata;
        }
        if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            mRobot = RobotManager.GetInst().GetCreateRobot();
            if (null != mRobot && null != mRobot.MotherboardData)
            {
                mMainboardType = mRobot.MotherboardData.GetMainboardType();
            }
        }
        else
        {
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            if (null != mRobot)
            {
                mServosConnection = ServosConManager.GetInst().GetServosConnection(mRobot.ID);
                if (null != mServosConnection)
                {
                    mMainboardType = mServosConnection.MainboardType;
                } else if (null != mRobot.MotherboardData)
                {
                    mMainboardType = mRobot.MotherboardData.GetMainboardType();
                } else
                {
                    mMainboardType = TopologyPartType.MainBoard_new_low;
                }
            }
        }
        if (mMainboardType == TopologyPartType.None)
        {
            mMainboardType = TopologyPartType.MainBoard_new_low;
        }
        if (null != mTrans)
        {
            UIPanel rootPanel = mTrans.GetComponent<UIPanel>();
            if (null != rootPanel)
            {
                rootPanel.depth = mDepth + 1;
            }
            Transform center = mTrans.Find("center");
            if (null != center)
            {
                Transform servo = center.Find("servo");
                if (null != servo)
                {
                    mServoItem = servo.gameObject;
                    mServoSize = NGUIMath.CalculateRelativeWidgetBounds(servo).size;
                    mServoItem.SetActive(false);
                }

                Transform sensor = center.Find("sensor");
                if (null != sensor)
                {
                    mSensorItem = sensor.gameObject;
                }

                Transform addItem = center.Find("addItem");
                if (null != addItem)
                {
                    mAddItem = addItem.gameObject;
                }
                mSelectedBgTrans = center.Find("selectedBg");

                Transform line = center.Find("line");
                if (null != line)
                {
                    mLineItem = line.Find("line").gameObject;
                    mArcItem = line.Find("arc").gameObject;
                    mAngleItem = line.Find("angle").gameObject;
                }

                Transform topologyPanel = center.Find("topologyPanel");
                if (null != topologyPanel)
                {
                    mTopologyStartPos = UIManager.GetWinPos(topologyPanel, UIWidget.Pivot.Top, 0, 168);
                    topologyPanel.localPosition = mTopologyStartPos;
                    mTopologyPanelScrollView = topologyPanel.GetComponent<UIScrollView>();
                    UIPanel panel = topologyPanel.GetComponent<UIPanel>();
                    if (null != panel)
                    {
                        panel.depth = mDepth - 1;
                        Vector4 rect = panel.finalClipRegion;
                        rect.y = -mTopologyStartPos.y;
                        rect.z = PublicFunction.GetWidth();
                        rect.w = PublicFunction.GetHeight();
                        panel.baseClipRegion = rect;
                    }
                    mPanelSelfTrans = topologyPanel.Find("panelSelf");
                    if (null != mPanelSelfTrans)
                    {
                        mTransformAdaptation = mPanelSelfTrans.GetComponent<TransformAdaptation>();
                        Transform m1 = mPanelSelfTrans.Find("MotherBox");
                        Transform m2 = mPanelSelfTrans.Find("MotherBox_new_low");
                        if (mMainboardType == TopologyPartType.MainBoard)
                        {
                            mMotherBoxTrans = m1;
                            if (null != m2)
                            {
                                m2.gameObject.SetActive(false);
                            }
                        } else if (mMainboardType == TopologyPartType.MainBoard_new_low)
                        {
                            mMotherBoxTrans = m2;
                            if (null != m1)
                            {
                                m1.gameObject.SetActive(false);
                            }
                        }
                        
                        if (null != mMotherBoxTrans)
                        {
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(mMotherBoxTrans, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("主控盒");
                            }
                            TopologyPartData motherData = new TopologyPartData();
                            motherData.partType = mMainboardType;
                            Vector2 motherSize = NGUIMath.CalculateRelativeWidgetBounds(mMotherBoxTrans).size;
                            motherData.width = (int)motherSize.x;
                            motherData.height = (int)motherSize.y;
                            mTopologyDict[mMotherBoxTrans.gameObject] = motherData;
                            List<PartPortData> portList = new List<PartPortData>();
                            if (mMainboardType == TopologyPartType.MainBoard)
                            {
                                for (int i = 1; i <= 6; ++i)
                                {
                                    Transform port = mMotherBoxTrans.Find(string.Format("port{0}", i));
                                    if (null != port)
                                    {
                                        PartPortData data = new PartPortData();
                                        if (i < 6)
                                        {
                                            data.portType = PartPortType.Port_Type_Pin_3;
                                        }
                                        else
                                        {
                                            data.portType = PartPortType.Port_Type_Pin_4;
                                        }
                                        data.portObj = port.gameObject;
                                        portList.Add(data);
                                    }
                                }
                            } else if (mMainboardType == TopologyPartType.MainBoard_new_low)
                            {
                                for (int i = 1; i <= 8; ++i)
                                {
                                    Transform port = mMotherBoxTrans.Find(string.Format("port{0}", i));
                                    if (null != port)
                                    {
                                        PartPortData data = new PartPortData();
                                        data.portType = PartPortType.Port_Type_Pin_3;
                                        data.portObj = port.gameObject;
                                        portList.Add(data);
                                    }
                                }
                            }
                            
                            mPartPortDict[mMotherBoxTrans.gameObject] = portList;
                        }
                    }
                    mPanelDefaultTrans = topologyPanel.Find("panelDefault");
                }
                mQueuePanelTrans = center.Find("queuePanel");
                if (null != mQueuePanelTrans)
                {
                    mIndependentScrollView = mQueuePanelTrans.GetComponent<UIScrollView>();
                    Vector3 pos = mQueuePanelTrans.localPosition;
                    pos.y = UIManager.GetWinPos(mQueuePanelTrans, UIWidget.Pivot.Bottom, 0, QueuePanelTransBottomMargin).y;
                    pos.x = -PublicFunction.GetWidth() / 2 + 120;
                    mQueuePanelTrans.localPosition = pos;
                    QueuePanelTransPos = pos;
                    UIPanel panel = mQueuePanelTrans.GetComponent<UIPanel>();
                    if (null != panel)
                    {
                        panel.depth = mDepth;
                        Vector4 rect = panel.finalClipRegion;
                        rect.z = PublicFunction.GetWidth() - 240;
                        rect.x = rect.z / 2;
                        panel.baseClipRegion = rect;
                    }
                    mIndependentQueueTrans = mQueuePanelTrans.Find("grid");
                }
                mQueueDragTrans = center.Find("queueDrag");
                if (null != mQueueDragTrans)
                {
                    if (null != mQueuePanelTrans)
                    {
                        mQueueDragTrans.localPosition = new Vector3(0, mQueuePanelTrans.localPosition.y);
                    }
                    UIWidget widget = GameHelper.FindChildComponent<UIWidget>(mQueueDragTrans, "widget");
                    if (null != widget)
                    {
                        widget.width = (int)(PublicFunction.GetWidth() - PublicFunction.Back_Btn_Pos.x * 2);
                    }
                    UIPanel panel = mQueueDragTrans.GetComponent<UIPanel>();
                    if (null != panel)
                    {
                        panel.depth = mDepth - 2;
                    }
                }

                Transform bgDrag = center.Find("bgDrag");
                if (null != bgDrag)
                {
                    UIWidget widget = GameHelper.FindChildComponent<UIWidget>(bgDrag, "dragwidget");
                    if (null != widget)
                    {
                        mBgDragObj = widget.gameObject;
                        widget.width = PublicFunction.GetWidth();
                        widget.height = PublicFunction.GetHeight();
                    }
                    UIPanel uiPanel = bgDrag.GetComponent<UIPanel>();
                    if (null != uiPanel)
                    {
                        uiPanel.depth = mDepth - 2;
                    }

                }

                mPin3Trans = center.Find("pin3trans");
                if (null != mPin3Trans)
                {
                    mPinTransPos = new Vector3(0, -PublicFunction.GetHeight() / 2 + 54);
                    mPin3Trans.localPosition = mPinTransPos - new Vector3(0, 300);
                    Transform bg = mPin3Trans.Find("bg");
                    if (null != bg)
                    {
                        UISprite sp = bg.GetComponent<UISprite>();
                        if (null != sp)
                        {
                            sp.width = PublicFunction.GetWidth();
                        }
                        UISprite sp1 = GameHelper.FindChildComponent<UISprite>(bg, "bg");
                        if (null != sp1)
                        {
                            sp1.width = PublicFunction.GetWidth();
                        }
                    }

                    Transform delBtn = mPin3Trans.Find("delBtn");
                    if (null != delBtn)
                    {
                        Vector3 pos = delBtn.localPosition;
                        pos.x = UIManager.GetWinPos(delBtn, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x).x;
                        delBtn.localPosition = pos;

                        UILabel lb = GameHelper.FindChildComponent<UILabel>(delBtn, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("ShanChu");
                        }
                    }

                    Transform switchBtn = mPin3Trans.Find("switchBtn");
                    if (null != switchBtn)
                    {
                        Vector3 pos = switchBtn.localPosition;
                        pos.x = UIManager.GetWinPos(switchBtn, UIWidget.Pivot.Right, PublicFunction.Back_Btn_Pos.x).x;
                        switchBtn.localPosition = pos;
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(switchBtn, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("轮模式");
                        }
                    }

                    Transform panel = mPin3Trans.Find("panel");
                    if (null != panel)
                    {
                        Vector3 pos = panel.localPosition;
                        pos.x = -PublicFunction.GetWidth() / 2 + PublicFunction.Back_Btn_Pos.x + 120;
                        panel.localPosition = pos;
                        UIPanel uiPanel = panel.GetComponent<UIPanel>();
                        if (null != uiPanel)
                        {
                            uiPanel.depth = mDepth + 2;
                            Vector4 rect = uiPanel.finalClipRegion;
                            rect.z = PublicFunction.GetWidth() - PublicFunction.Back_Btn_Pos.x * 2 - 120 - uiPanel.clipSoftness.x * 2;
                            rect.x = rect.z / 2 + uiPanel.clipSoftness.x;
                            uiPanel.baseClipRegion = rect;
                        }
                    }
                    mPin3Trans.gameObject.SetActive(false);
                }
                mPin4Trans = center.Find("pin4trans");
                if (null != mPin4Trans)
                {
                    mPin4Trans.localPosition = mPin3Trans.localPosition;
                    Transform bg = mPin4Trans.Find("bg");
                    if (null != bg)
                    {
                        UISprite sp = bg.GetComponent<UISprite>();
                        if (null != sp)
                        {
                            sp.width = PublicFunction.GetWidth();
                        }
                        UISprite sp1 = GameHelper.FindChildComponent<UISprite>(bg, "bg");
                        if (null != sp1)
                        {
                            sp1.width = PublicFunction.GetWidth();
                        }
                    }

                    Transform delBtn = mPin4Trans.Find("delBtn");
                    if (null != delBtn)
                    {
                        Vector3 pos = delBtn.localPosition;
                        pos.x = UIManager.GetWinPos(delBtn, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x).x;
                        delBtn.localPosition = pos;

                        UILabel lb = GameHelper.FindChildComponent<UILabel>(delBtn, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("ShanChu");
                        }
                    }

                    Transform panel = mPin4Trans.Find("panel");
                    if (null != panel)
                    {
                        Vector3 pos = panel.localPosition;
                        pos.x = -PublicFunction.GetWidth() / 2 + PublicFunction.Back_Btn_Pos.x + 120;
                        panel.localPosition = pos;
                        UIPanel uiPanel = panel.GetComponent<UIPanel>();
                        if (null != uiPanel)
                        {
                            uiPanel.depth = mDepth + 2;
                            Vector4 rect = uiPanel.finalClipRegion;
                            rect.z = PublicFunction.GetWidth() - PublicFunction.Back_Btn_Pos.x - 120 - uiPanel.clipSoftness.x * 2;
                            rect.x = rect.z / 2 + uiPanel.clipSoftness.x;
                            uiPanel.baseClipRegion = rect;
                        }
                    }
                    mPin4Trans.gameObject.SetActive(false);
                }
                Transform chooseCloseBtn = center.Find("chooseCloseBtn");
                if (null != chooseCloseBtn)
                {
                    mChooseCloseBtnObj = chooseCloseBtn.gameObject;
                    UIPanel panel = mChooseCloseBtnObj.GetComponent<UIPanel>();
                    if (null != panel)
                    {
                        panel.depth = mDepth;
                    }
                    Transform closeBtn = chooseCloseBtn.Find("closeBtn");
                    if (null != closeBtn)
                    {
                        UIWidget widget = closeBtn.GetComponent<UIWidget>();
                        if (null != widget)
                        {
                            widget.width = PublicFunction.GetExtendWidth();
                            widget.height = PublicFunction.GetExtendHeight();
                        }
                    }
                    
                    mChooseCloseBtnObj.SetActive(false);
                }
                TouchManager touchMgr = center.GetComponent<TouchManager>();
                if (null != touchMgr)
                {
                    /*touchMgr.onFirstTouchBegan += onFirstTouchBegan;
                    touchMgr.onFirstTouchMoved += onFirstTouchMoved;
                    touchMgr.onFirstTouchEnded += onFirstTouchEnded;*/
                    touchMgr.onTwoTouchMoved += onTwoTouchMoved;
                }
            }
            InitUI();
        }
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            string name = obj.name;
            if (isEdit)
            {
                EditOnClick(obj);
            }
            if (null != mOnClickDelegate)
            {
                mOnClickDelegate(obj);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


    protected override void OnButtonPress(GameObject obj, bool press)
    {
        try
        {
            base.OnButtonPress(obj, press);
            if (!isEdit)
            {
                return;
            }
            mSwitchDragOffset = 0;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void OnButtonDrag(GameObject obj, Vector2 delta)
    {
        try
        {
            base.OnButtonDrag(obj, delta);
            if (!isEdit)
            {
                return;
            }
            string name = obj.name;
            if (name.Equals("switchBtn"))
            {
                mSwitchDragOffset += delta.x;
                if (mChoiceServoModel == ServoModel.Servo_Model_Angle)
                {
                    if (mSwitchDragOffset >= 30 || delta.x >= 10)
                    {
                        mSwitchDragOffset = 0;
                        SwitchChoiceServoModel(ServoModel.Servo_Model_Turn);
                    }
                }
                else
                {
                    if (mSwitchDragOffset <= -30 || delta.x <= -10)
                    {
                        mSwitchDragOffset = 0;
                        SwitchChoiceServoModel(ServoModel.Servo_Model_Angle);
                    }
                }
            }
            /*else if (!name.Equals("delBtn") && !name.Equals("bg") && !name.StartsWith("c_"))
            {
                HideChoicePartPanel(false);
            }*/
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 编辑状态下的点击
    /// </summary>
    /// <param name="obj"></param>
    void EditOnClick(GameObject obj)
    {
        string name = obj.name;
        if (name.StartsWith("port"))
        {
            DealSelectedConnectionItem(obj);
        }
        else if (name.Contains("_left_"))
        {
            DealSelectedConnectionItem(obj);
        }
        else if (name.Contains("_right_"))
        {
            DealSelectedConnectionItem(obj);
        }
        else if (name.Equals("bg"))
        {
            return;
        }
        else if (name.Equals("blank"))
        {
            if (obj == mSelectedPartGameObject)
            {
                return;
            }
            if (mPartPortDict.ContainsKey(obj))
            {
                List<PartPortData> list = mPartPortDict[obj];
                if (list.Count > 0)
                {
                    /*if (mShowChoicePanelFlag)
                    {
                        //HidePartQueue(true);
                    }
                    else
                    {
                        if (null != mChoicePartActiveCallBack)
                        {
                            mChoicePartActiveCallBack(true);
                        }
                    }*/
                    if (mShowChoicePortType != list[0].portType)
                    {
                        if (mShowChoicePanelFlag)
                        {
                            HidePartQueue(true);
                        }
                    }
                    SetSelectedPart(obj);
                    ShowPartQueue(list[0].portType, ServoModel.Servo_Model_Angle);
                    return;
                }
            }
            
        }
        else if (name.Equals("switchBtn"))
        {
            if (null != mSelectedPartGameObject && mSelectedPartGameObject.name.StartsWith("servo_"))
            {
                if (mChoiceServoModel == ServoModel.Servo_Model_Angle)
                {
                    SwitchChoiceServoModel(ServoModel.Servo_Model_Turn);
                }
                else
                {
                    SwitchChoiceServoModel(ServoModel.Servo_Model_Angle);
                }
                
            }
            else
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("请先选择舵机"));
            }
            return;
        }
        else if (name.Equals("closeBtn"))
        {
            /*if (null != mSelectedPartGameObject && mSelectedPartGameObject.name.StartsWith("servo_"))
            {
                SetPlayerServoModel(mSelectedPartGameObject, mChoiceServoModel);
            }*/
            //HideChoicePartPanel(false);
            return;
        }
        else if (name.Equals("delBtn"))
        {//删除零件
            if (null != mSelectedPartGameObject && !mSelectedPartGameObject.name.Equals("blank"))
            {
                /*if (mSelectedPartGameObject.name.StartsWith("servo_"))
                {
                    SetPlayerServoModel(mSelectedPartGameObject, ServoModel.Servo_Model_Angle);
                }*/
                GameObject delObj = mSelectedPartGameObject;
                SetSelectedPart(null);
                GameObject blank = DelTopologyPart(delObj);
                SetSelectedPart(blank);
                //HidePartQueue();
            }
            return;
        }
        else if (name.StartsWith("c_"))
        {//选择了零件
            GameObject topoObj = FindTopoObjForChoicePart(obj);
            if (null != topoObj && !mTopologyDict.ContainsKey(topoObj))
            {
                if (null != mSelectedPartGameObject)
                {
                    GameObject targetObj = mSelectedPartGameObject;
                    SetSelectedPart(null);
                    if (topoObj.name.StartsWith("servo_"))
                    {
                        byte id = byte.Parse(topoObj.name.Substring("servo_".Length));
                        DuoJiData data = mRobot.GetAllDjData().GetDjData(id);
                        if (null != data)
                        {
                            SetChoiceServoModel(data.modelType);
                        }
                    }
                    if (targetObj.name.Equals("blank"))
                    {
                        AddTopologyPart(topoObj, targetObj);
                    }
                    else
                    {
                        Transform oldChoicePart = FindChoiceObjForTopoPart(targetObj);
                        if (null != oldChoicePart)
                        {
                            SetChoiceQueuePartState(oldChoicePart, false);
                        }
                        SwitchGameObject(targetObj, topoObj);
                    }
                    SetSelectedPart(topoObj);
                    //HidePartQueue();
                    SetChoiceQueuePartState(obj.transform, true);
                }
                else
                {
                    //HideChoicePartPanel(false);
                }
            }
            return;
        }
        else if (mTopologyDict.ContainsKey(obj))
        {
            if (mTopologyType == TopologyType.Topology_Default)
            {
                /*if (name.StartsWith("servo_"))
                {
                    mSelectedServoObj = obj;
                    byte id = byte.Parse(name.Substring("servo_".Length));
                    if (null != mRobot)
                    {
                        DuoJiData data = mRobot.GetAllDjData().GetDjData(id);
                        if (null != data)
                        {
                            ServoTypeMsg.ShowMsg(data.modelType, SetServoModelResult);
                        }
                    }
                }*/
            }
            else
            {
                if (obj == mSelectedPartGameObject)
                {
                    return;
                }
                if (mTopologyDict[obj].partType > TopologyPartType.None && mTopologyDict[obj].partType < TopologyPartType.MainBoard || mTopologyDict[obj].partType == TopologyPartType.Servo)
                {
                    PartPortType portType = GetPartPortType(mTopologyDict[obj].partType);
                    if (portType == PartPortType.Port_Type_Pin_4)
                    {
                        /*if (mShowChoicePanelFlag)
                        {
                            //HidePartQueue(true);
                        }
                        else
                        {
                            if (null != mChoicePartActiveCallBack)
                            {
                                mChoicePartActiveCallBack(true);
                            }
                        }*/
                        if (mShowChoicePortType != PartPortType.Port_Type_Pin_4)
                        {
                            if (mShowChoicePanelFlag)
                            {
                                HidePartQueue(true);
                            }
                        }
                        SetSelectedPart(obj);
                        ShowPartQueue(PartPortType.Port_Type_Pin_4);
                    }
                    else
                    {
                        /*if (mShowChoicePanelFlag)
                        {
                            //HidePartQueue(true);
                        }
                        else
                        {
                            if (null != mChoicePartActiveCallBack)
                            {
                                mChoicePartActiveCallBack(true);
                            }
                        }*/
                        if (mShowChoicePortType != PartPortType.Port_Type_Pin_3)
                        {
                            if (mShowChoicePanelFlag)
                            {
                                HidePartQueue(true);
                            }
                        }
                        ServoModel modelType = ServoModel.Servo_Model_Angle;
                        if (mTopologyDict[obj].partType == TopologyPartType.Servo)
                        {
                            byte id = byte.Parse(name.Substring("servo_".Length));
                            if (null != mRobot)
                            {
                                DuoJiData data = mRobot.GetAllDjData().GetDjData(id);
                                if (null != data)
                                {
                                    modelType = data.modelType;
                                }
                            }
                        }
                        SetSelectedPart(obj);
                        ShowPartQueue(PartPortType.Port_Type_Pin_3, modelType);
                    }
                }
                else
                {
                    //HideChoicePartPanel(false);
                }
            }
            return;
        }
        //HideChoicePartPanel(false);
    }
    void SetServoModelResult(ServoModel modelType)
    {
        if (null != mSelectedServoObj)
        {
            byte id = byte.Parse(mSelectedServoObj.name.Substring("servo_".Length));
            DuoJiData data = mRobot.GetAllDjData().GetDjData(id);
            if (null != data)
            {
                data.modelType = modelType;
            }
            SetServoModelIcon(mSelectedServoObj, modelType);
        }
    }

    void DealSelectedConnectionItem(GameObject obj)
    {
        if (mTopologyType == TopologyType.Topology_Playerdata)
        {
            return;
        }
        if (mPortConnectionDict.ContainsKey(obj))
        {
            if (null != mSelectedConnectionObj)
            {
                SetSelectPortState(mSelectedConnectionObj, false);
                mSelectedConnectionObj = null;
            }
            return;
        }
        if (null == mSelectedConnectionObj)
        {
            if (obj.transform.parent.parent == mIndependentQueueTrans)
            {
                return;
            }
            mSelectedConnectionObj = obj;
            SetSelectPortState(mSelectedConnectionObj, true);
        }
        else if (obj != mSelectedConnectionObj)
        {
            Transform selectPart = mSelectedConnectionObj.transform.parent;
            Transform nowPart = obj.transform.parent;
            if (nowPart == selectPart || nowPart.parent == mIndependentQueueTrans)
            {
                SetSelectPortState(mSelectedConnectionObj, false);
                mSelectedConnectionObj = null;
                return;
            }
            GameObject selectLastPort = GetPortLastPort(mSelectedConnectionObj);
            GameObject lastPort = GetPortLastPort(obj);
            if (selectLastPort == obj || lastPort == mSelectedConnectionObj || nowPart == selectLastPort.transform.parent || selectPart == lastPort.transform.parent || selectLastPort.transform.parent == lastPort.transform.parent)
            {//防止形成回路
                SetSelectPortState(mSelectedConnectionObj, false);
                mSelectedConnectionObj = null;
                return;
            }
            PartPortType selectPortType = GetPortType(mSelectedConnectionObj);
            PartPortType nowPortType = GetPortType(obj);
            if (nowPortType != selectPortType)
            {
                SetSelectPortState(mSelectedConnectionObj, false);
                mSelectedConnectionObj = null;
                return;
            }
            CreateConnectionLine(mSelectedConnectionObj, obj);
            mPortConnectionDict[mSelectedConnectionObj] = obj;
            mPortConnectionDict[obj] = mSelectedConnectionObj;
            mSelectedConnectionObj = null;
            SetPartPortState(obj.transform.parent.gameObject);
        }
    }

    #region 缩放
    protected virtual void onTwoTouchMoved(TouchEventArgs args)
    {
        if (null == UICamera.hoveredObject || mBgDragObj == UICamera.hoveredObject)
        {
            if (null != mPanelSelfTrans && args.twoMvDis != 0)
            {
                SetTransformScale(mPanelSelfTrans, args.twoMvDis);
            }
        }
    }

    protected void SetTransformScale(Transform trans, float change)
    {
        Vector3 scale = trans.localScale + new Vector3(change, change);
        if (scale.x < 0.5f)
        {
            scale.x = 0.5f;
            scale.y = 0.5f;
        }
        else if (scale.x > 1.5f)
        {
            scale.x = 1.5f;
            scale.y = 1.5f;
        }
        trans.localScale = scale;
    }
    #endregion

    //////////////////////////////////////////////////////////////////////////
    
    

    /// <summary>
    /// 通过一个连接端口获取零件的另一个连接端口
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    GameObject GetPartOtherPort(GameObject port)
    {
        string portName = port.name;
        string otherName = string.Empty;
        if (portName.Contains("left"))
        {
            otherName = portName.Replace("left", "right");
        }
        else if (portName.Contains("right"))
        {
            otherName = portName.Replace("right", "left");
        }
        if (null != port.transform.parent && !string.IsNullOrEmpty(otherName))
        {
            Transform otherPort = port.transform.parent.Find(otherName);
            if (null != otherPort)
            {
                return otherPort.gameObject;
            }
        }
        return null;
    }
    /// <summary>
    /// 获取一条线路的最终连接点
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    GameObject GetPortLastPort(GameObject port)
    {
        GameObject lastPort = port;
        GameObject otherObj = GetPartOtherPort(port);
        while (null != otherObj)
        {
            lastPort = otherObj;
            if (mPortConnectionDict.ContainsKey(otherObj))
            {
                lastPort = mPortConnectionDict[otherObj];
                otherObj = GetPartOtherPort(mPortConnectionDict[otherObj]);
            }
            else
            {
                break;
            }
        }
        return lastPort;
    }
    /// <summary>
    /// 获取零件所连的主板端口，未取到返回空
    /// </summary>
    /// <param name="part"></param>
    /// <returns></returns>
    GameObject GetPartMainPort(GameObject part)
    {
        if (mPartPortDict.ContainsKey(part))
        {
            List<PartPortData> list = mPartPortDict[part];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                GameObject lastPort = null;
                if (mPortConnectionDict.ContainsKey(list[i].portObj))
                {
                    lastPort = mPortConnectionDict[list[i].portObj];
                }
                while (null != lastPort)
                {
                    if (lastPort.name.StartsWith("port"))
                    {
                        return lastPort;
                    }
                    lastPort = GetPartOtherPort(lastPort);
                    if (mPortConnectionDict.ContainsKey(lastPort))
                    {
                        lastPort = mPortConnectionDict[lastPort];
                    }
                    else
                    {
                        lastPort = null;
                    }
                }
            }
        }
        return null;
    }

    
    GameObject CreateServoGameObject(byte id, Transform parentTrans, GameObject oldObj, bool isTopology)
    {
        try
        {
            if (null != mServoItem)
            {
                GameObject obj = null;
                if (null == oldObj)
                {
                    obj = GameObject.Instantiate(mServoItem) as GameObject;
                }
                else
                {
                    obj = oldObj;
                }
                int oldId = 0;
                if (null != oldObj)
                {
                    oldId = int.Parse(obj.name.Substring("servo_".Length));
                }
                obj.name = string.Format("servo_{0}", id);
                if (null == oldObj)
                {
                    obj.transform.parent = parentTrans;
                }
                obj.transform.localEulerAngles = Vector3.zero;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
                if (null != lb)
                {
                    lb.text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
                }
                List<PartPortData> list = new List<PartPortData>();
                Transform left = null;
                if (null == oldObj)
                {
                    left = obj.transform.Find("left");
                }
                else
                {
                    left = obj.transform.Find(string.Format("s_left_{0}", oldId));
                }
                if (null != left)
                {
                    left.name = string.Format("s_left_{0}", id);
                    PartPortData data = new PartPortData();
                    data.portObj = left.gameObject;
                    data.portType = PartPortType.Port_Type_Pin_3;
                    list.Add(data);
                    if (isTopology)
                    {
                        left.gameObject.SetActive(true);
                    }
                }
                Transform right = null;
                if (null == oldObj)
                {
                    right = obj.transform.Find("right");
                }
                else
                {
                    right = obj.transform.Find(string.Format("s_right_{0}", oldId));
                }
                if (null != right)
                {
                    right.name = string.Format("s_right_{0}", id);
                    PartPortData data = new PartPortData();
                    data.portObj = right.gameObject;
                    data.portType = PartPortType.Port_Type_Pin_3;
                    list.Add(data);
                    if (isTopology)
                    {
                        right.gameObject.SetActive(true);
                    }
                }
                ServoModel servoModel = ServoModel.Servo_Model_Angle;
                if (null != mServosConnection)
                {
                    servoModel = mServosConnection.GetServoModel(id);
                }
                SetServoModelIcon(obj, servoModel);
                mPartPortDict[obj] = list;
                obj.SetActive(true);
                return obj;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
        return null;
    }

    GameObject CreateChoiceServo(TopologyPartData data, Transform parentTrans)
    {
        if (null != mServoItem)
        {
            GameObject obj = GameObject.Instantiate(mServoItem) as GameObject;
            obj.name = string.Format("c_servo_{0}", data.id);
            obj.transform.parent = parentTrans;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
            if (null != lb)
            {
                lb.text = PublicFunction.ID_Format + data.id.ToString().PadLeft(2, '0');
            }
            Transform left = obj.transform.Find("left"); ;
            if (null != left)
            {
                left.gameObject.SetActive(false);
            }
            Transform right = obj.transform.Find("right");
            if (null != right)
            {
                right.gameObject.SetActive(false);
            }
            Transform state = obj.transform.Find("state");
            if (null != state)
            {
                UISprite bg = GameHelper.FindChildComponent<UISprite>(state, "bg");
                if (null != bg)
                {
                    bg.spriteName = "success";
                }
                UISprite icon = GameHelper.FindChildComponent<UISprite>(state, "icon");
                if (null != icon)
                {
                    icon.spriteName = "yes";
                    icon.MakePixelPerfect();
                }
                state.gameObject.SetActive(false);
            }
            ServoModel servoModel = ServoModel.Servo_Model_Angle;
            if (null != mRobot)
            {
                DuoJiData servoData = mRobot.GetAllDjData().GetDjData(data.id);
                if (null != servoData)
                {
                    servoModel = servoData.modelType;
                }
            }
            SetServoModelIcon(obj, servoModel);
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    void SetServoModelIcon(GameObject servo, ServoModel modelType)
    {
        UISprite icon = GameHelper.FindChildComponent<UISprite>(servo.transform, "bg");
        if (null != icon)
        {
            if (modelType == ServoModel.Servo_Model_Angle)
            {
                icon.transform.parent.Find("turnModel").gameObject.SetActive(false);
            }
            else
            {
                icon.transform.parent.Find("turnModel").gameObject.SetActive(true);
            }
        }
    }

    GameObject CreateMotorGameObject(byte id, Transform parentTrans, GameObject oldObj, bool isTopology)
    {
        try
        {
            if (null != mSensorItem)
            {
                GameObject obj = null;
                if (null == oldObj)
                {
                    obj = GameObject.Instantiate(mSensorItem) as GameObject;
                }
                else
                {
                    obj = oldObj;
                }
                int oldId = 0;
                if (null != oldObj)
                {
                    oldId = int.Parse(obj.name.Substring("motor_".Length));
                }
                obj.name = string.Format("motor_{0}", id);
                if (null == oldObj)
                {
                    obj.transform.parent = parentTrans;
                }
                obj.transform.localEulerAngles = Vector3.zero;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
                if (null != lb)
                {
                    lb.text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
                }
                List<PartPortData> list = new List<PartPortData>();
                Transform left = null;
                if (null == oldObj)
                {
                    left = obj.transform.Find("left");
                }
                else
                {
                    left = obj.transform.Find(string.Format("m_left_{0}", oldId));
                }
                if (null != left)
                {
                    left.name = string.Format("m_left_{0}", id);
                    PartPortData data = new PartPortData();
                    data.portObj = left.gameObject;
                    data.portType = PartPortType.Port_Type_Pin_3;
                    list.Add(data);
                    if (isTopology)
                    {
                        left.gameObject.SetActive(true);
                    }
                }
                Transform right = null;
                if (null == oldObj)
                {
                    right = obj.transform.Find("right");
                }
                else
                {
                    right = obj.transform.Find(string.Format("m_right_{0}", oldId));
                }
                if (null != right)
                {
                    right.name = string.Format("m_right_{0}", id);
                    PartPortData data = new PartPortData();
                    data.portObj = right.gameObject;
                    data.portType = PartPortType.Port_Type_Pin_3;
                    list.Add(data);
                    if (isTopology)
                    {
                        right.gameObject.SetActive(true);
                    }
                }
                UISprite motorBg = GameHelper.FindChildComponent<UISprite>(obj.transform, "bg");
                if (null != motorBg)
                {
                    motorBg.spriteName = "motor";
                }
                mPartPortDict[obj] = list;
                obj.SetActive(true);
                return obj;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return null;
    }

    GameObject CreateChoiceMotor(TopologyPartData data, Transform parentTrans)
    {
        if (null != mSensorItem)
        {
            GameObject obj = GameObject.Instantiate(mSensorItem) as GameObject;
            obj.name = string.Format("c_motor_{0}", data.id);
            obj.transform.parent = parentTrans;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
            if (null != lb)
            {
                lb.text = PublicFunction.ID_Format + data.id.ToString().PadLeft(2, '0');
            }
            Transform left = obj.transform.Find("left"); ;
            if (null != left)
            {
                left.gameObject.SetActive(false);
            }
            Transform right = obj.transform.Find("right");
            if (null != right)
            {
                right.gameObject.SetActive(false);
            }
            Transform state = obj.transform.Find("state");
            if (null != state)
            {
                UISprite bg = GameHelper.FindChildComponent<UISprite>(state, "bg");
                if (null != bg)
                {
                    bg.spriteName = "success";
                }
                UISprite icon = GameHelper.FindChildComponent<UISprite>(state, "icon");
                if (null != icon)
                {
                    icon.spriteName = "yes";
                    icon.MakePixelPerfect();
                }
                state.gameObject.SetActive(false);
            }
            UISprite motorBg = GameHelper.FindChildComponent<UISprite>(obj.transform, "bg");
            if (null != motorBg)
            {
                motorBg.spriteName = "motor";
            }
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    GameObject CreateSensorGameObject(byte id, TopologyPartType partType, Transform parentTrans, bool isTopology)
    {
        if (null != mSensorItem)
        {
            GameObject obj = GameObject.Instantiate(mSensorItem) as GameObject;
            string portNameFont = partType.ToString();
            
            obj.name = string.Format("sensor_{0}_{1}", partType.ToString(), id);
            obj.transform.parent = parentTrans;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
            if (null != lb)
            {
                lb.text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
            }
            SetSensorBg(obj, partType);
            SetSensorUpdateImg(obj, partType);
            if (partType == TopologyPartType.Speaker)
            {
                mSpeakerTrans = obj.transform;
            }
            List<PartPortData> list = new List<PartPortData>();
            Transform left = obj.transform.Find("left");
            if (null != left)
            {
                left.name = string.Format("{0}_left_{1}", portNameFont, id);
                PartPortData data = new PartPortData();
                data.portObj = left.gameObject;
                data.portType = GetPartPortType(partType);//PartPortType.Port_Type_Pin_3;
                list.Add(data);
                if (isTopology)
                {
                    left.gameObject.SetActive(true);
                }
            }
            Transform right = obj.transform.Find("right");
            if (null != right)
            {
                right.name = string.Format("{0}_right_{1}", portNameFont, id);
                PartPortData data = new PartPortData();
                data.portObj = right.gameObject;
                data.portType = GetPartPortType(partType);//PartPortType.Port_Type_Pin_3;
                list.Add(data);
                if (isTopology)
                {
                    right.gameObject.SetActive(true);
                }
            }
            mPartPortDict[obj] = list;
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    GameObject CreateChoiceSensor(TopologyPartData data, Transform parentTrans)
    {
        if (null != mSensorItem)
        {
            GameObject obj = GameObject.Instantiate(mSensorItem) as GameObject;
            obj.name = string.Format("c_sensor_{0}_{1}", data.partType.ToString(), data.id);
            obj.transform.parent = parentTrans;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
            if (null != lb)
            {
                lb.text = PublicFunction.ID_Format + data.id.ToString().PadLeft(2, '0');
            }
            Transform state = obj.transform.Find("state");
            if (null != state)
            {
                UISprite bg = GameHelper.FindChildComponent<UISprite>(state, "bg");
                if (null != bg)
                {
                    bg.spriteName = "success";
                }
                UISprite icon = GameHelper.FindChildComponent<UISprite>(state, "icon");
                if (null != icon)
                {
                    icon.spriteName = "yes";
                    icon.MakePixelPerfect();
                }
                state.gameObject.SetActive(false);
            }
            SetSensorBg(obj, data.partType);
            Transform left = obj.transform.Find("left");
            if (null != left)
            {
                left.gameObject.SetActive(false);
            }
            Transform right = obj.transform.Find("right");
            if (null != right)
            {
                right.gameObject.SetActive(false);
            }
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public static void SetSensorBg(GameObject obj, TopologyPartType partType)
    {
        UISprite icon = GameHelper.FindChildComponent<UISprite>(obj.transform, "bg");
        if (null != icon)
        {
            switch (partType)
            {
                case TopologyPartType.Infrared:
                    icon.spriteName = "sensor_infrared";
                    break;
                case TopologyPartType.Gyro:
                    icon.spriteName = "sensor_gyro";
                    break;
                case TopologyPartType.Touch:
                    icon.spriteName = "sensor_touch";
                    break;
                case TopologyPartType.Light:
                    icon.spriteName = "sensor_light";
                    break;
                case TopologyPartType.Speaker:
                    icon.spriteName = "sensor_speaker";
                    break;
                case TopologyPartType.DigitalTube:
                    icon.spriteName = "sensor_digitaltube";
                    break;
                case TopologyPartType.Ultrasonic:
                    icon.spriteName = "sensor_ultrasonic";
                    break;
                case TopologyPartType.Motor:
                    icon.spriteName = "motor";
                    break;
                case TopologyPartType.Color:
                    icon.spriteName = "sensor_color";
                    break;
                case TopologyPartType.RgbLight:
                    icon.spriteName = "sensor_rgblight";
                    break;
                default:
                    icon.spriteName = "sensor_infrared";
                    break;
            }
        }
    }


    public static void SetSensorUpdateImg(GameObject obj, TopologyPartType partType)
    {
        UISprite icon = GameHelper.FindChildComponent<UISprite>(obj.transform, "update/bg");
        UISprite icon1 = GameHelper.FindChildComponent<UISprite>(obj.transform, "update/bg1");
        if (null != icon && null != icon1)
        {
            switch (partType)
            {
                case TopologyPartType.Infrared:
                    icon.spriteName = "sensor_infrared_upgrade";
                    icon1.spriteName = "sensor_infrared_upgrading";
                    break;
                case TopologyPartType.Gyro:
                    icon.spriteName = "sensor_gyro_upgrade";
                    icon1.spriteName = "sensor_common_upgrading";
                    break;
                case TopologyPartType.Touch:
                    icon.spriteName = "sensor_touch_upgrade";
                    icon1.spriteName = "sensor_common_upgrading";
                    break;
                case TopologyPartType.Light:
                    icon.spriteName = "sensor_light_upgrade";
                    icon1.spriteName = "sensor_light_upgrading";
                    break;
                case TopologyPartType.Speaker:
                    icon.spriteName = "sensor_speaker_upgrade";
                    icon1.spriteName = "sensor_common_upgrading";
                    break;
                case TopologyPartType.DigitalTube:
                    icon.spriteName = "sensor_digitaltube_upgrade";
                    icon1.spriteName = "sensor_common_upgrading";
                    break;
                default:
                    icon.spriteName = "sensor_infrared_upgrade";
                    icon1.spriteName = "sensor_infrared_upgrading";
                    break;
            }
        }
    }

    GameObject CreateBlankGameObject(PartPortType portType)
    {
        if (null != mAddItem)
        {
            GameObject obj = GameObject.Instantiate(mAddItem) as GameObject;
            obj.name = "blank";
            obj.transform.parent = mPanelSelfTrans;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            
            List<PartPortData> list = new List<PartPortData>();
            Transform left = obj.transform.Find("left");
            if (null != left)
            {
                left.name = "blank_left";
                PartPortData data = new PartPortData();
                data.portObj = left.gameObject;
                data.portType = portType;
                list.Add(data);
            }
            Transform right = obj.transform.Find("right");
            if (null != right)
            {
                right.name = "blank_right";
                PartPortData data = new PartPortData();
                data.portObj = right.gameObject;
                data.portType = portType;
                list.Add(data);
            }
            mPartPortDict[obj] = list;
            obj.SetActive(true);
            mBlankList.Add(obj);
            return obj;
        }
        return null;
    }



    /// <summary>
    /// 通过零件类型获取接口类型
    /// </summary>
    /// <param name="partType"></param>
    /// <returns></returns>
    PartPortType GetPartPortType(TopologyPartType partType)
    {
        switch (partType)
        {
            case TopologyPartType.Gyro:
                return PartPortType.Port_Type_Pin_4;
        }
        return PartPortType.Port_Type_Pin_3;
    }
    /// <summary>
    /// 通过端口名字获取零件id
    /// </summary>
    /// <param name="portName"></param>
    /// <returns></returns>
    int GetPortId(string portName)
    {
        string num = portName.Substring(portName.LastIndexOf('_') + 1);
        if (PublicFunction.IsInteger(num))
        {
            return int.Parse(num);
        }
        return 0;
    }

    private enum StateType : byte
    {
        normal,
        disConnect,
        exception,
    }

    void SetState(Transform trans, StateType state)
    {
        Transform stateTrans = trans.Find("state");
        if (null != stateTrans)
        {
            stateTrans.gameObject.SetActive(true);
            UISprite bgSp = GameHelper.FindChildComponent<UISprite>(stateTrans, "bg");
            UISprite iconSp = GameHelper.FindChildComponent<UISprite>(stateTrans, "icon");
            if (null != bgSp)
            {
                if (state == StateType.normal)
                {
                    bgSp.spriteName = "success";
                }
                else
                {
                    bgSp.spriteName = "unsuccessful";
                }
                bgSp.MakePixelPerfect();
            }
            if (null != iconSp)
            {
                switch (state)
                {
                    case StateType.normal:
                        iconSp.spriteName = "yes";
                        break;
                    case StateType.disConnect:
                        iconSp.spriteName = "no";
                        break;
                    case StateType.exception:
                        iconSp.spriteName = "exception";
                        break;
                }
                iconSp.MakePixelPerfect();
            }
        }
    }

    void HideState(Transform trans)
    {
        Transform stateTrans = trans.Find("state");
        if (null != stateTrans)
        {
            stateTrans.gameObject.SetActive(false);
        }
    }

    void CheckPartState(GameObject obj, TopologyPartData data, ErrorCode mainResult, List<byte> servoUpdateList, Dictionary<TopologyPartType, List<byte>> sensorUpdateList)
    {
        switch (data.partType)
        {
            case TopologyPartType.MainBoard:
            case TopologyPartType.MainBoard_new_low:
                if (mainResult == ErrorCode.Result_OK)
                {
                    SetState(obj.transform, StateType.normal);
                }
                else
                {
                    SetState(obj.transform, StateType.exception);
                }
                break;
            case TopologyPartType.Servo:
                {
                    StateType state = StateType.normal;
                    if (null != mRobot && null != mRobot.MotherboardData)
                    {
                        if (mRobot.MotherboardData.errorIds.Contains(data.id))
                        {
                            state = StateType.exception;
                        }
                        /*else if (mRobot.MotherboardData.errorVerIds.Contains(data.id))
                        {
                            state = StateType.exception;
                        }*/
                        else if (!mRobot.MotherboardData.ids.Contains(data.id))
                        {
                            state = StateType.disConnect;
                        }
                        else if (!RobotManager.GetInst().IsCreateRobotFlag && null == mRobot.GetAnDjData(data.id))
                        {//多余的舵机
                            state = StateType.exception;
                        }
                        else if (servoUpdateList.Contains(data.id))
                        {
                            state = StateType.exception;
                        }
                    }
                    else
                    {
                        state = StateType.disConnect;
                    }
                    SetState(obj.transform, state);
                }
                break;
            case TopologyPartType.Motor:
                {
                    StateType state = StateType.normal;
                    if (null != mRobot && null != mRobot.MotherboardData)
                    {
                        if (null != mRobot.MotherboardData.motorErrorIds &&  mRobot.MotherboardData.motorErrorIds.Contains(data.id))
                        {
                            state = StateType.exception;
                        } else if (null == mRobot.MotherboardData.motors || !mRobot.MotherboardData.motors.Contains(data.id))
                        {
                            state = StateType.disConnect;
                        } else if (!RobotManager.GetInst().IsCreateRobotFlag && (null == mRobot.MotorsData || !mRobot.MotorsData.Contains(data.id)))
                        {//多余的马达
                            state = StateType.exception;
                        }
                    }
                    else
                    {
                        state = StateType.disConnect;
                    }
                    SetState(obj.transform, state);
                }
                break;
            case TopologyPartType.Speaker:
            case TopologyPartType.RgbLight:
                {
                    StateType state = StateType.normal;
                    if (null != mRobot && null != mRobot.MotherboardData)
                    {
                        SensorData sensorData = mRobot.MotherboardData.GetSensorData(data.partType);
                        if (null != sensorData)
                        {
                            if (sensorData.errorIds.Contains(data.id))
                            {
                                state = StateType.exception;
                            }
                            else if (sensorData.ids.Count > 1)
                            {
                                state = StateType.exception;
                            }
                            else if (!sensorData.ids.Contains(data.id))
                            {
                                state = StateType.disConnect;
                            }
                            else if (sensorUpdateList.ContainsKey(data.partType) && sensorUpdateList[data.partType].Contains(data.id))
                            {
                                state = StateType.exception;
                            }
                            else if (!RobotManager.GetInst().IsCreateRobotFlag && (null == conSensorDict || !conSensorDict.ContainsKey(data.partType) || !conSensorDict[data.partType].Contains(data.id)))
                            {//多余的传感器
                                state = StateType.exception;
                            }
                        }
                        else
                        {
                            state = StateType.disConnect;
                        }
                    }
                    else
                    {
                        state = StateType.disConnect;
                    }
                    SetState(obj.transform, state);
                }
                break;
            default:
                {
                    StateType state = StateType.normal;
                    if (null != mRobot && null != mRobot.MotherboardData)
                    {
                        if (null == conSensorDict)
                        {
                            ServosConnection con = ServosConManager.GetInst().GetServosConnection(mRobot.ID);
                            if (null != con)
                            {
                                conSensorDict = con.GetTopologySensor();
                            }
                        }
                        SensorData sensorData = mRobot.MotherboardData.GetSensorData(data.partType);
                        if (null != sensorData)
                        {
                            
                            if (sensorData.errorIds.Contains(data.id))
                            {
                                state = StateType.exception;
                            }
                            /*else if (sensorData.errorVerIds.Contains(data.id))
                            {
                                state = StateType.exception;
                            }*/
                            else if (!sensorData.ids.Contains(data.id))
                            {
                                state = StateType.disConnect;
                            }
                            else if (sensorUpdateList.ContainsKey(data.partType) && sensorUpdateList[data.partType].Contains(data.id))
                            {
                                state = StateType.exception;
                            } else if (!RobotManager.GetInst().IsCreateRobotFlag && (null == conSensorDict || !conSensorDict.ContainsKey(data.partType) || !conSensorDict[data.partType].Contains(data.id)))
                            {//多余的传感器
                                state = StateType.exception;
                            }
                        }
                        else
                        {
                            state = StateType.disConnect;
                        }
                    }
                    else
                    {
                        state = StateType.disConnect;
                    }
                    SetState(obj.transform, state);
                    break;
                }
        }
    }

    void AddUpdateItem(Transform trans)
    {
        /*Transform state = trans.Find("state");
        if (null != state)
        {
            UILabel lb = GameHelper.FindChildComponent<UILabel>(state, "Label");
            if (null != lb)
            {
                mUpdateDict[state] = lb;
            }
        }*/
        Transform update = trans.Find("update");
        if (null != update)
        {
            mUpdateDict[trans] = new UpdateShowData(update);
        }
    }
    void InitUI()
    {
        InitIndependentQueueUI();
        InitTopologyData();
        //OpenIndependentDragDrop();
        //OpenTopologyDictDragDrop();
    }
    /// <summary>
    /// 初始化独立队列的ui，包括舵机和传感器
    /// </summary>
    void InitIndependentQueueUI()
    {
        List<byte> servoList = new List<byte>();
        List<byte> motorList = new List<byte>();
        TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
        Dictionary<TopologyPartType, List<byte>> sensorDict = new Dictionary<TopologyPartType, List<byte>>();
        if (RobotManager.GetInst().IsCreateRobotFlag && null != mRobot && null != mRobot.MotherboardData)
        {
            if (mRobot.MotherboardData.errorIds.Count > 0)
            {
                servoList.AddRange(mRobot.MotherboardData.errorIds);
            }
            if (mRobot.MotherboardData.ids.Count > 0)
            {
                servoList.AddRange(mRobot.MotherboardData.ids);
            }
            if (mRobot.MotherboardData.motorErrorIds != null && mRobot.MotherboardData.motorErrorIds.Count > 0)
            {
                motorList.AddRange(mRobot.MotherboardData.motorErrorIds);
            }
            if (mRobot.MotherboardData.motors != null && mRobot.MotherboardData.motors.Count > 0)
            {
                motorList.AddRange(mRobot.MotherboardData.motors);
            }
            for (int i = 0, imax = partType.Length; i < imax; ++i)
            {
                SensorData data = mRobot.MotherboardData.GetSensorData(partType[i]);
                if (null != data && data.errorIds.Count + data.ids.Count > 0)
                {
                    List<byte> list = new List<byte>();
                    if (data.errorIds.Count > 0)
                    {
                        list.AddRange(data.errorIds);
                    }
                    if (data.ids.Count > 0)
                    {
                        list.AddRange(data.ids);
                    }
                    sensorDict[partType[i]] = list;
                }
            }
        }
        else
        {
            if (null != mServosConnection)
            {
                Dictionary<TopologyPartType, List<byte>> dataDict = mServosConnection.GetIndependentQueue();
                if (null != dataDict)
                {
                    if (dataDict.ContainsKey(TopologyPartType.Servo))
                    {
                        servoList.AddRange(dataDict[TopologyPartType.Servo]);
                    }
                    if (dataDict.ContainsKey(TopologyPartType.Motor))
                    {
                        motorList.AddRange(dataDict[TopologyPartType.Motor]);
                    }
                    for (int i = 0, imax = partType.Length; i < imax; ++i)
                    {
                        if (dataDict.ContainsKey(partType[i]) && dataDict[partType[i]].Count > 0)
                        {
                            sensorDict[partType[i]] = dataDict[partType[i]];
                        }
                        AddNewSensor(partType[i], sensorDict);
                    }
                }
                else
                {
                    for (int i = 0, imax = partType.Length; i < imax; ++i)
                    {
                        AddNewSensor(partType[i], sensorDict);
                    }
                }
                
                
#if UNITY_EDITOR
                if (null == dataDict && mTopologyType == TopologyType.Topology_Default && mServosConnection.GetTopologyData().Count < 1)
                {
                    servoList.AddRange(mRobot.GetAllDjData().GetIDList());
                    if (null != mRobot.MotorsData)
                    {
                        motorList.AddRange(mRobot.MotorsData.GetIds());
                    }
                }
#endif
            }
            else if (null != mRobot)
            {
                servoList.AddRange(mRobot.GetAllDjData().GetIDList());
                if (null != mRobot.MotorsData)
                {
                    motorList.AddRange(mRobot.MotorsData.GetIds());
                }
                if (null != mRobot.MotherboardData)
                {
                    for (int i = 0, imax = partType.Length; i < imax; ++i)
                    {
                        SensorData data = mRobot.MotherboardData.GetSensorData(partType[i]);
                        if (null != data && data.errorIds.Count + data.ids.Count > 0)
                        {
                            List<byte> list = new List<byte>();
                            if (data.errorIds.Count > 0)
                            {
                                list.AddRange(data.errorIds);
                            }
                            if (data.ids.Count > 0)
                            {
                                list.AddRange(data.ids);
                            }
                            sensorDict[partType[i]] = list;
                        }
                    }
                }
            }
        }
        if (!RobotManager.GetInst().IsCreateRobotFlag && mRobot != null && mRobot.MotherboardData != null)
        {
            //加入多接的舵机
            List<byte> servos = mRobot.GetAllDjData().GetIDList();
            for (int i = 0, icount = mRobot.MotherboardData.ids.Count; i < icount; ++i)
            {
                if (!servos.Contains(mRobot.MotherboardData.ids[i]) && !servoList.Contains(mRobot.MotherboardData.ids[i]))
                {
                    servoList.Add(mRobot.MotherboardData.ids[i]);
                }
            }
            if (mRobot.MotherboardData.errorIds.Count > 0)
            {
                for (int i = 0, icount = mRobot.MotherboardData.errorIds.Count; i < icount; ++i)
                {
                    if (!servos.Contains(mRobot.MotherboardData.errorIds[i]) && !servoList.Contains(mRobot.MotherboardData.errorIds[i]))
                    {
                        servoList.Add(mRobot.MotherboardData.errorIds[i]);
                    }
                }
            }
            //加入多接的马达
            List<byte> motors = null;
            if (null != mRobot.MotorsData && mRobot.MotorsData.Count > 0)
            {
                motors = mRobot.MotorsData.GetIds();
            }
            if (null != mRobot.MotherboardData.motors && mRobot.MotherboardData.motors.Count > 0)
            {
                for (int i = 0, imax = mRobot.MotherboardData.motors.Count; i < imax; ++i)
                {
                    if ((null == motors || !motors.Contains(mRobot.MotherboardData.motors[i])) && !motorList.Contains(mRobot.MotherboardData.motors[i]))
                    {
                        motorList.Add(mRobot.MotherboardData.motors[i]);
                    }
                }
            }
            if (null != mRobot.MotherboardData.motorErrorIds && mRobot.MotherboardData.motorErrorIds.Count > 0)
            {
                for (int i = 0, imax = mRobot.MotherboardData.motorErrorIds.Count; i < imax; ++i)
                {
                    if ((null == motors || !motors.Contains(mRobot.MotherboardData.motorErrorIds [i])) && !motorList.Contains(mRobot.MotherboardData.motorErrorIds[i]))
                    {
                        motorList.Add(mRobot.MotherboardData.motorErrorIds[i]);
                    }
                }
            }
        }
        if (sensorDict.ContainsKey(TopologyPartType.Speaker) && sensorDict[TopologyPartType.Speaker].Count > 0)
        {//音响显示在前面
            CreateIndependentSensorQueue(sensorDict[TopologyPartType.Speaker], TopologyPartType.Speaker);
        }
        if (servoList.Count > 0)
        {
            CreateIndependentServoQueue(servoList);
        }
        if (motorList.Count > 0)
        {
            CreateIndependentMotorQueue(motorList);
        }
        foreach (KeyValuePair<TopologyPartType, List<byte>> kvp in sensorDict)
        {
            if (TopologyPartType.Speaker != kvp.Key &&kvp.Value.Count > 0)
            {
                CreateIndependentSensorQueue(kvp.Value, kvp.Key);
            }
        }
        
        if (mIndependentQueueTrans.childCount > 0)
        {
            ResetIndependentQueuePosition(false);
            UIManager.SetButtonEventDelegate(mIndependentQueueTrans, mBtnDelegate);
            if (null != mQueuePanelTrans)
            {
                mQueuePanelTrans.gameObject.SetActive(true);
            }
            if (null != mQueueDragTrans)
            {
                mQueueDragTrans.gameObject.SetActive(true);
            }
        }
        else
        {
            if (null != mQueuePanelTrans)
            {
                mQueuePanelTrans.gameObject.SetActive(false);
            }
            if (null != mQueueDragTrans)
            {
                mQueueDragTrans.gameObject.SetActive(false);
            }
        }
    }

    void AddNewSensor(TopologyPartType sensorType, Dictionary<TopologyPartType, List<byte>> sensorDict)
    {
        //加入新增传感器
        if (null != mRobot && null != mRobot.MotherboardData)
        {
            SensorData sensorData = mRobot.MotherboardData.GetSensorData(sensorType);
            if (null != sensorData && sensorData.ids.Count + sensorData.errorIds.Count > 0)
            {
                for (int sensorIndex = 0, sensorMax = sensorData.ids.Count; sensorIndex < sensorMax; ++sensorIndex)
                {
                    bool isNewFlag = false;
                    if (sensorDict.ContainsKey(sensorType))
                    {
                        if (!sensorDict[sensorType].Contains(sensorData.ids[sensorIndex]))
                        {
                            if (null == mServosConnection || mServosConnection.IsNewSensor(sensorType, sensorData.ids[sensorIndex]))
                            {
                                isNewFlag = true;
                            }
                        }
                    }
                    else
                    {
                        if (null == mServosConnection || mServosConnection.IsNewSensor(sensorType, sensorData.ids[sensorIndex]))
                        {
                            isNewFlag = true;
                        }
                    }
                    if (isNewFlag)
                    {
                        if (!sensorDict.ContainsKey(sensorType))
                        {
                            List<byte> list = new List<byte>();
                            sensorDict[sensorType] = list;
                        }
                        sensorDict[sensorType].Add(sensorData.ids[sensorIndex]);
                        if (mAddNewSensorDict.ContainsKey(sensorType))
                        {
                            mAddNewSensorDict[sensorType].Add(sensorData.ids[sensorIndex]);
                        }
                        else
                        {
                            List<byte> list = new List<byte>();
                            list.Add(sensorData.ids[sensorIndex]);
                            mAddNewSensorDict[sensorType] = list;
                        }
                    }
                }

                for (int sensorIndex = 0, sensorMax = sensorData.errorIds.Count; sensorIndex < sensorMax; ++sensorIndex)
                {
                    bool isNewFlag = false;
                    if (sensorDict.ContainsKey(sensorType))
                    {
                        if (!sensorDict[sensorType].Contains(sensorData.errorIds[sensorIndex]))
                        {
                            if (null == mServosConnection || mServosConnection.IsNewSensor(sensorType, sensorData.errorIds[sensorIndex]))
                            {
                                isNewFlag = true;
                            }
                        }
                    }
                    else
                    {
                        if (null == mServosConnection || mServosConnection.IsNewSensor(sensorType, sensorData.errorIds[sensorIndex]))
                        {
                            isNewFlag = true;
                        }
                    }
                    if (isNewFlag)
                    {
                        if (!sensorDict.ContainsKey(sensorType))
                        {
                            List<byte> list = new List<byte>();
                            sensorDict[sensorType] = list;
                        }
                        sensorDict[sensorType].Add(sensorData.errorIds[sensorIndex]);
                        if (mAddNewSensorDict.ContainsKey(sensorType))
                        {
                            mAddNewSensorDict[sensorType].Add(sensorData.errorIds[sensorIndex]);
                        }
                        else
                        {
                            List<byte> list = new List<byte>();
                            list.Add(sensorData.errorIds[sensorIndex]);
                            mAddNewSensorDict[sensorType] = list;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 生成舵机列表
    /// </summary>
    /// <param name="servoList"></param>
    void CreateIndependentServoQueue(List<byte> servoList)
    {
        if (null != mIndependentQueueTrans)
        {
            int oldCount = mIndependentQueueTrans.childCount;
            int index = 0;
            for (int i = 0, imax = servoList.Count; i < imax; ++i)
            {
                GameObject tmp = null;
                if (index < oldCount)
                {
                    tmp = mIndependentQueueTrans.GetChild(index).gameObject;
                    ++index;
                    while (!tmp.name.StartsWith("servo_") && index < oldCount)
                    {
                        tmp = mIndependentQueueTrans.GetChild(index).gameObject;
                        ++index;
                    }
                    if (!tmp.name.StartsWith("servo_"))
                    {
                        tmp = null;
                    }
                }
                GameObject obj = CreateServoGameObject(servoList[i], mIndependentQueueTrans, tmp, false);
                if (null != obj)
                {
                    //obj.transform.localPosition = new Vector3(mServoSize.x / 2 + i * (mServoSize.x + 46), 0);
                    TopologyPartData servoData = new TopologyPartData();
                    servoData.id = servoList[i];
                    servoData.isIndependent = true;
                    servoData.partType = TopologyPartType.Servo;
                    servoData.width = (int)mServoSize.x;
                    servoData.height = (int)mServoSize.y;
                    mIndependentQueueDict[obj] = servoData;
                }
            }
        }
    }
    /// <summary>
    /// 生成马达列表
    /// </summary>
    /// <param name="motorList"></param>
    void CreateIndependentMotorQueue(List<byte> motorList)
    {
        if (null != mIndependentQueueTrans)
        {
            int oldCount = mIndependentQueueTrans.childCount;
            int index = 0;
            for (int i = 0, imax = motorList.Count; i < imax; ++i)
            {
                GameObject tmp = null;
                if (index < oldCount)
                {
                    tmp = mIndependentQueueTrans.GetChild(index).gameObject;
                    ++index;
                    while (!tmp.name.StartsWith("motor_") && index < oldCount)
                    {
                        tmp = mIndependentQueueTrans.GetChild(index).gameObject;
                        ++index;
                    }
                    if (!tmp.name.StartsWith("motor_"))
                    {
                        tmp = null;
                    }
                }
                GameObject obj = CreateMotorGameObject(motorList[i], mIndependentQueueTrans, tmp, false);
                if (null != obj)
                {
                    //obj.transform.localPosition = new Vector3(mServoSize.x / 2 + i * (mServoSize.x + 46), 0);
                    TopologyPartData motorData = new TopologyPartData();
                    motorData.id = motorList[i];
                    motorData.isIndependent = true;
                    motorData.partType = TopologyPartType.Motor;
                    motorData.width = (int)mServoSize.x;
                    motorData.height = (int)mServoSize.y;
                    mIndependentQueueDict[obj] = motorData;
                }
            }
        }
    }
    /// <summary>
    /// 生成传感器列表
    /// </summary>
    /// <param name="sensorList"></param>
    /// <param name="partType"></param>
    void CreateIndependentSensorQueue(List<byte> sensorList, TopologyPartType partType)
    {
        if (null != mIndependentQueueTrans)
        {
            for (int i = 0, imax = sensorList.Count; i < imax; ++i)
            {
                GameObject obj = CreateSensorGameObject(sensorList[i], partType, mIndependentQueueTrans, false);
                if (null != obj)
                {
                    //obj.transform.localPosition = new Vector3(mServoSize.x / 2 + i * (mServoSize.x + 46), 0);
                    TopologyPartData sensorData = new TopologyPartData();
                    sensorData.id = sensorList[i];
                    sensorData.isIndependent = true;
                    sensorData.partType = partType;
                    sensorData.width = (int)mServoSize.x;
                    sensorData.height = (int)mServoSize.y;
                    mIndependentQueueDict[obj] = sensorData;
                }
            }
        }
    }

    void InitTopologyData()
    {
        if (null != mServosConnection)
        {
            List<TopologyPartData> topoData = mServosConnection.GetTopologyData();
            Dictionary<string, GameObject> portDict = new Dictionary<string, GameObject>();
            for (int i = 0, imax = topoData.Count; i < imax; ++i)
            {
                TopologyPartData data = topoData[i];
                if (!data.isIndependent)
                {//独立队列不在此处生成
                    GameObject obj = null;
                    switch (data.partType)
                    {
                        case TopologyPartType.Servo:
                            obj = CreateServoGameObject(data.id, mPanelSelfTrans, null, true);
                            break;
                        case TopologyPartType.Motor:
                            obj = CreateMotorGameObject(data.id, mPanelSelfTrans, null, true);
                            break;
                        case TopologyPartType.Infrared://红外
                        case TopologyPartType.Touch://触碰
                        case TopologyPartType.Gyro://陀螺仪
                        case TopologyPartType.Light://灯光
                        case TopologyPartType.Gravity://重力
                        case TopologyPartType.Ultrasonic://超声
                        case TopologyPartType.DigitalTube://数码管
                        case TopologyPartType.Speaker://喇叭
                        case TopologyPartType.Color://颜色传感器
                        case TopologyPartType.RgbLight://独角兽灯
                            obj = CreateSensorGameObject(data.id, data.partType, mPanelSelfTrans, true);
                            break;
                        case TopologyPartType.MainBoard://主板
                        case TopologyPartType.MainBoard_new_low:
                            if (null != mMotherBoxTrans)
                            {//获取主板端口
                                GameObject motherObj = mMotherBoxTrans.gameObject;
                                mMotherBoxTrans.localPosition = data.localPosition;
                                mMotherBoxTrans.localEulerAngles = data.localEulerAngles;
                                if (mPartPortDict.ContainsKey(motherObj))
                                {
                                    List<PartPortData> list = mPartPortDict[motherObj];
                                    for (int portIndex = 0, portMax = list.Count; portIndex < portMax; ++portIndex)
                                    {
                                        PartPortData portData = list[portIndex];
                                        portDict[portData.portObj.name] = portData.portObj;
                                    }
                                }
                            }
                            break;
                    }
                    if (null != obj)
                    {
                        TopologyDragDropItem dropItem = obj.GetComponent<TopologyDragDropItem>();
                        if (null != dropItem)
                        {
                            dropItem.restriction = UIDragDropItem.Restriction.None;
                        }
                        obj.transform.localPosition = data.localPosition;
                        obj.transform.localEulerAngles = data.localEulerAngles;
                        mTopologyDict[obj] = new TopologyPartData(data);
                        if (mPartPortDict.ContainsKey(obj))
                        {
                            List<PartPortData> list = mPartPortDict[obj];
                            for (int portIndex = 0, portMax = list.Count; portIndex < portMax; ++portIndex)
                            {
                                PartPortData portData = list[portIndex];
                                portDict[portData.portObj.name] = portData.portObj;
                            }
                        }
                    }
                }
            }
            
            
            //生成连接数据
            Dictionary<string, string> connectionData = mServosConnection.GetPortConnectionData();
            foreach (KeyValuePair<string, string> kvp in connectionData)
            {
                if (portDict.ContainsKey(kvp.Key) && portDict.ContainsKey(kvp.Value))
                {
                    mPortConnectionDict[portDict[kvp.Key]] = portDict[kvp.Value];
                }
            }
            //生成连线
            foreach (KeyValuePair<GameObject, GameObject> kvp in mPortConnectionDict)
            {
                if (!mLineDict.ContainsKey(kvp.Key) && !mLineDict.ContainsKey(kvp.Value))
                {
                    CreateConnectionLine(kvp.Key, kvp.Value);
                    SetPartPortState(kvp.Key.transform.parent.gameObject);
                }
            }
            if (mTopologyDict.Count > 1)
            {
                RecalculateTopology();
            }
            UIManager.SetButtonEventDelegate(mPanelSelfTrans, mBtnDelegate);
        }
        
    }

    void ObjToTopology(GameObject obj)
    {
        if (mIndependentQueueDict.ContainsKey(obj))
        {
            TopologyDragDropItem itemDrop = obj.GetComponent<TopologyDragDropItem>();
            if (null != itemDrop)
            {
                itemDrop.restriction = UIDragDropItem.Restriction.None;
            }
            SpringPosition springPosition = obj.GetComponent<SpringPosition>();
            if (null != springPosition)
            {
                springPosition.enabled = false;
            }
            mIndependentQueueDict[obj].isIndependent = false;
            mTopologyDict[obj] = mIndependentQueueDict[obj];
            mIndependentQueueDict.Remove(obj);
        }
        else if (obj.name.Equals("blank"))
        {
            TopologyPartData data = new TopologyPartData();
            data.localPosition = obj.transform.localPosition;
            data.partType = TopologyPartType.None;
            mTopologyDict[obj] = data;
        }
        
        if (mPartPortDict.ContainsKey(obj))
        {
            List<PartPortData> list = mPartPortDict[obj];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                list[i].portObj.SetActive(true);
            }

        }

    }
    /// <summary>
    /// 重置独立队列位置
    /// </summary>
    void ResetIndependentQueuePosition(bool withinBounds)
    {
        ResetQueuePosition(mIndependentQueueTrans, (int)mServoSize.x, 36, withinBounds, mIndependentScrollView);
    }
    /// <summary>
    /// 重置某个队列的位置
    /// </summary>
    /// <param name="queueTrans"></param>
    /// <param name="cellWidth"></param>
    /// <param name="space"></param>
    /// <param name="withinBounds"></param>
    /// <param name="scrollView"></param>
    void ResetQueuePosition(Transform queueTrans, int cellWidth, int space, bool withinBounds = false, UIScrollView scrollView = null)
    {
        if (null != queueTrans)
        {
            int index = 0;
            for (int i = 0, imax = queueTrans.childCount; i < imax; ++i)
            {
                GameObject obj = queueTrans.GetChild(i).gameObject;
                if (obj.activeSelf)
                {
                    Vector3 targetPos = new Vector3(cellWidth / 2 + index * (cellWidth + space), 0);
                    SpringPosition.Begin(obj, targetPos, 8);
                    ++index;
                }
            }
            if (withinBounds && null != scrollView)
            {
                scrollView.RestrictWithinBounds(false);
            }
        }
    }
    /// <summary>
    /// 物体开始被拖动
    /// </summary>
    /// <param name="arg"></param>
    void ItemDragDropStart(EventArg args)
    {
        GameObject obj = (GameObject)args[0];
        if (null != obj)
        {
            mDragGameObject = obj;
            Transform temp = obj.transform;
            Transform parent = temp.parent;
            if (mIndependentQueueTrans == parent)
            {
                temp.parent = mQueuePanelTrans.parent;
                ResetIndependentQueuePosition(true);
            }
            RemovePartLine(obj);
        }
    }

    void ItemDragDropEnd(EventArg args)
    {
        GameObject obj = (GameObject)args[0];
        if (null != obj)
        {
            Transform temp = obj.transform;
            if (temp.parent != mPanelSelfTrans)
            {
                temp.parent = mPanelSelfTrans;
                ObjToTopology(obj);
                temp.localScale = Vector3.one;
                NGUITools.MarkParentAsChanged(obj);
            }
            Vector3 targetPos = GetDragItemTargetPos(obj);
            if (mTopologyDict.ContainsKey(obj))
            {
                mTopologyDict[obj].localPosition = obj.transform.localPosition;
                mTopologyDict[obj].localEulerAngles = obj.transform.localEulerAngles;
            }
            if (!targetPos.Equals(temp.localPosition))
            {
                SpringPosition.Begin(obj, targetPos, 15).onFinished = delegate () {
                    RecoverPartLine(obj);
                    SetPartPortState(obj);
                };
            }
            else
            {
                RecoverPartLine(obj);
                SetPartPortState(obj);
            }
        }
    }

    void SetGameObjectColor(GameObject obj, Color color)
    {
        if (null != obj)
        {
            UISprite sp = GameHelper.FindChildComponent<UISprite>(obj.transform, "bg");
            if (null != sp)
            {
                sp.color = color;
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////
    //拖拽相关
    /// <summary>
    /// 打开拖拽功能
    /// </summary>
    /// <param name="obj"></param>
    void OpenDragDropItem(GameObject obj)
    {
        TopologyDragDropItem drag = obj.GetComponent<TopologyDragDropItem>();
        if (null != drag)
        {
            drag.enabled = true;
        }
    }
    /// <summary>
    /// 关闭拖拽功能
    /// </summary>
    /// <param name="obj"></param>
    void CloseDragDropItem(GameObject obj)
    {
        TopologyDragDropItem drag = obj.GetComponent<TopologyDragDropItem>();
        if (null != drag)
        {
            drag.enabled = false;
        }
    }
    /// <summary>
    /// 打开独立队列物体的拖拽功能
    /// </summary>
    void OpenIndependentDragDrop()
    {
        if (null != mIndependentQueueDict)
        {
            foreach (GameObject item in mIndependentQueueDict.Keys)
            {
                OpenDragDropItem(item);
            }
        }
    }
    /// <summary>
    /// 关闭独立队列物体的拖拽功能
    /// </summary>
    void CloseIndependentDragDrop()
    {
        if (null != mIndependentQueueDict)
        {
            foreach (GameObject item in mIndependentQueueDict.Keys)
            {
                CloseDragDropItem(item);
            }
        }
    }
    /// <summary>
    /// 打开拓扑图物体的拖拽功能
    /// </summary>
    void OpenTopologyDictDragDrop()
    {
        foreach (GameObject item in mTopologyDict.Keys)
        {
            UIDragScrollView drag = item.GetComponent<UIDragScrollView>();
            if (null != drag)
            {
                drag.enabled = false;
            }
            TopologyDragDropItem topoDrop = item.GetComponent<TopologyDragDropItem>();
            if (null != topoDrop)
            {
                topoDrop.enabled = true;
            }
        }
    }
    /// <summary>
    /// 关闭拓扑图的拖拽功能
    /// </summary>
    void CloseTopologyDictDragDrop()
    {
        foreach (GameObject item in mTopologyDict.Keys)
        {
            UIDragScrollView drag = item.GetComponent<UIDragScrollView>();
            if (null != drag)
            {
                drag.enabled = true;
            }
            TopologyDragDropItem topoDrop = item.GetComponent<TopologyDragDropItem>();
            if (null != topoDrop)
            {
                topoDrop.enabled = false;
            }
        }
    }
    //////////////////////////////////////////////////////////////////////////
    //拖动物体位置计算

    /// <summary>
    /// 获取拖动物体的目标位置，防止重叠
    /// </summary>
    /// <param name="dragObj"></param>
    /// <returns></returns>
    Vector3 GetDragItemTargetPos(GameObject dragObj)
    {
        Vector3 targetPos = dragObj.transform.localPosition;
        TopologyPartData dragPartData = null;
        if (mTopologyDict.ContainsKey(dragObj))
        {
            dragPartData = mTopologyDict[dragObj];
        }
        else if (mIndependentQueueDict.ContainsKey(dragObj))
        {
            dragPartData = mIndependentQueueDict[dragObj];
        }
        if (null != dragPartData)
        {
            if (!IsLegitimatePosition(dragObj, targetPos, dragPartData))
            {//有重叠
                List<GameObject> nearList = GetNearGameObject(dragObj, dragPartData);
                for (int nearIndex = 0, nearMax = nearList.Count; nearIndex < nearMax; ++nearIndex)
                {
                    Vector3 pos = GetLegitimatePosition(dragObj, dragPartData, nearList[nearIndex]);
                    if (!pos.Equals(targetPos))
                    {
                        targetPos = pos;
                        break;
                    }
                }
            }
        }
        return targetPos;
    }

    Vector3 GetLegitimatePosition(GameObject dragObj, TopologyPartData dragData, GameObject nearObj)
    {
        if (!mTopologyDict.ContainsKey(nearObj))
        {
            return dragObj.transform.localPosition;
        }
        TopologyPartData nearData = mTopologyDict[nearObj];
        Vector2 dragPos = dragObj.transform.localPosition;
        Vector2 nearPos = nearObj.transform.localPosition;

        Vector2 targetPos = dragPos;
        float spaceX = Mathf.Abs(dragPos.x - nearPos.x) - (dragData.width + nearData.width) / 2;
        float spaceY = Mathf.Abs(dragPos.y - nearPos.y) - (dragData.height + nearData.height) / 2;
        float offsetX = Mathf.Abs(spaceX);
        float offsetY = Mathf.Abs(spaceY);
        bool isOverlap = false;//dragObj与nearPos是否有重叠
        if (spaceX <= 0 && spaceY <= 0)
        {
            isOverlap = true;
        }
        if (isOverlap)
        {
            if (offsetX <= offsetY)
            {//改变x移动小
                if (dragPos.x >= nearPos.x)
                {//拖动物体在目标物体右边，往右移
                    targetPos = GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Right, true, true);
                }
                else
                {//往左移
                    targetPos = GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Left, true, true);
                }
            }
            else
            {//改变y移动小
                if (dragPos.y >= nearPos.y)
                {//拖动物体在目标物体上边，往上移动
                    targetPos = GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Top, true, true);
                }
                else
                {//往下移动
                    targetPos = GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Bottom, true, true);
                }
            }
        }
        else
        {
            float disX = Mathf.Abs(dragPos.x - nearPos.x);
            float disY = Mathf.Abs(dragPos.y - nearPos.y);
            if (disX >= disY)
            {
                if (dragPos.x >= nearPos.x)
                {
                    targetPos = GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Left, true, true);
                }
                else
                {
                    targetPos = GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Right, true, true);
                }

            }
            else
            {
                if (dragPos.y >= nearPos.y)
                {
                    targetPos = GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Top, true, true);
                }
                else
                {
                    targetPos = GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Bottom, true, true);
                }
            }

        }

        return targetPos;
    }
    /// <summary>
    /// dragObj与near重合了，求dragObj位置
    /// </summary>
    /// <param name="dragObj"></param>
    /// <param name="dragPos"></param>
    /// <param name="nearPos"></param>
    /// <param name="dragData"></param>
    /// <param name="nearData"></param>
    /// <param name="dir"></param>
    /// <param name="dirFlag"></param>
    /// <param name="minFlag"></param>
    /// <returns></returns>
    Vector3 GetOverlapTargetPosition(GameObject dragObj, Vector3 dragPos, Vector3 nearPos, TopologyPartData dragData, TopologyPartData nearData, UIWidget.Pivot dir, bool dirFlag, bool minFlag)
    {
        float offsetX = Mathf.Abs((dragData.width + nearData.width) / 2 - Mathf.Abs(dragPos.x - nearPos.x));
        float offsetY = Mathf.Abs((dragData.height + nearData.height) / 2 - Mathf.Abs(dragPos.y - nearPos.y));
        Vector3 targetPos = dragPos;
        switch (dir)
        {
            case UIWidget.Pivot.Left:
                if (dirFlag)
                {
                    targetPos.x -= offsetX + mSpaceSize.x;
                }
                else
                {
                    targetPos.x = nearPos.x - (dragData.width + nearData.width) / 2 - mSpaceSize.x;
                }
                break;
            case UIWidget.Pivot.Right:
                if (dirFlag)
                {
                    targetPos.x += offsetX + mSpaceSize.x;
                }
                else
                {
                    targetPos.x = nearPos.x + (dragData.width + nearData.width) / 2 + mSpaceSize.x;
                }
                break;
            case UIWidget.Pivot.Top:
                if (dirFlag)
                {
                    targetPos.y += offsetY + mSpaceSize.y;
                }
                else
                {
                    targetPos.y = nearPos.y + (dragData.height + nearData.height) / 2 + mSpaceSize.y;
                }
                break;
            case UIWidget.Pivot.Bottom:
                if (dirFlag)
                {
                    targetPos.y -= offsetY + mSpaceSize.y;
                }
                else
                {
                    targetPos.y = nearPos.y - (dragData.height + nearData.height) / 2 - mSpaceSize.y;
                }
                break;
        }
        /*if (GetLegitimatePositionDir(dragObj, ref targetPos, dragData, dir))
        {
            return targetPos;
        }*/
        if (IsLegitimatePosition(dragObj, targetPos, dragData))
        {
            return targetPos;
        }
        switch (dir)
        {
            case UIWidget.Pivot.Left:
            case UIWidget.Pivot.Right:
                if (dirFlag)
                {//上一次是往最近的方向移动
                    if (minFlag)
                    {//上一次移动的是小距离的，下一次移动y值的小距离
                        if (dragPos.y >= nearPos.y)
                        {//拖动物体在目标物体上边，往上移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Top, false, true);
                        }
                        else
                        {//往下移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Bottom, false, true);
                        }
                    }
                    else
                    {//上一次移动的是大距离的，小一次移动反方向的大距离
                        if (dragPos.y >= nearPos.y)
                        {//拖动物体在目标物体上边，往下移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Bottom, false, false);
                        }
                        else
                        {//往上移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Top, false, false);
                        }
                    }
                }
                else
                {//上一次不是往最近的方向移动
                    if (minFlag)
                    {//上一次移动的是小距离的，下一次正方向的大距离
                        if (dragPos.y >= nearPos.y)
                        {//拖动物体在目标物体上边，往下移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Bottom, true, false);
                        }
                        else
                        {//拖动物体在目标物体下边，往上移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Top, true, false);
                        }
                    }
                    else
                    {//反方向大距离，表示未找到合适位置

                    }
                }
                break;
            case UIWidget.Pivot.Top:
            case UIWidget.Pivot.Bottom:
                if (dirFlag)
                {//上一次是往最近的方向移动
                    if (minFlag)
                    {//上一次移动的是小距离的，下一次移动x值的小距离
                        if (dragPos.x >= nearPos.x)
                        {//拖动物体在目标物体右边，往右移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Right, false, true);
                        }
                        else
                        {//往右移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Left, false, true);
                        }
                    }
                    else
                    {//上一次移动的是大距离的，下一次移动反方向的大距离
                        if (dragPos.x >= nearPos.x)
                        {//拖动物体在目标物体右边，往左移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Left, false, false);
                        }
                        else
                        {//往右移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Right, false, false);
                        }
                    }
                }
                else
                {//上一次不是往最近的方向移动
                    if (minFlag)
                    {//上一次移动的是小距离的，下一次正方向的大距离
                        if (dragPos.x >= nearPos.x)
                        {//拖动物体在目标物体右边，往左移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Left, true, false);
                        }
                        else
                        {//拖动物体在目标物体左边，往右移动
                            return GetOverlapTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Right, true, false);
                        }
                    }
                    else
                    {//反方向大距离，表示未找到合适位置

                    }
                }
                break;
        }
        return dragPos;
    }

    Vector3 GetSeparateTargetPosition(GameObject dragObj, Vector3 dragPos, Vector3 nearPos, TopologyPartData dragData, TopologyPartData nearData, UIWidget.Pivot dir, bool dirFlag, bool minFlag)
    {
        Vector3 targetPos = dragPos;
        float spaceX = Mathf.Abs(dragPos.x - nearPos.x);
        float spaceY = Mathf.Abs(dragPos.y - nearPos.y);
        float offsetX = (dragData.width + nearData.width) / 2 + mSpaceSize.x;
        float offsetY = (dragData.height + nearData.height) / 2 + mSpaceSize.y;
        switch (dir)
        {
            case UIWidget.Pivot.Left:
                targetPos.x = nearPos.x + offsetX;
                break;
            case UIWidget.Pivot.Right:
                targetPos.x = nearPos.x - offsetX;
                break;
            case UIWidget.Pivot.Top:
                targetPos.y = nearPos.y + offsetY;
                break;
            case UIWidget.Pivot.Bottom:
                targetPos.y = nearPos.y - offsetY;
                break;
        }
        if (IsLegitimatePosition(dragObj, targetPos, dragData))
        {
            return targetPos;
        }
        switch (dir)
        {
            case UIWidget.Pivot.Left:
            case UIWidget.Pivot.Right:
                if (dirFlag)
                {//上一次是往最近的方向移动
                    if (minFlag)
                    {//上一次移动的是小距离的，下一次移动y值的小距离
                        if (dragPos.y >= nearPos.y)
                        {//拖动物体在目标物体上边，往上移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Top, false, true);
                        }
                        else
                        {//往下移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Bottom, false, true);
                        }
                    }
                    else
                    {//上一次移动的是大距离的，小一次移动反方向的大距离
                        if (dragPos.y >= nearPos.y)
                        {//拖动物体在目标物体上边，往下移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Bottom, false, false);
                        }
                        else
                        {//往上移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Top, false, false);
                        }
                    }
                }
                else
                {//上一次不是往最近的方向移动
                    if (minFlag)
                    {//上一次移动的是小距离的，下一次正方向的大距离
                        if (dragPos.y >= nearPos.y)
                        {//拖动物体在目标物体上边，往下移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Bottom, true, false);
                        }
                        else
                        {//拖动物体在目标物体下边，往上移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Top, true, false);
                        }
                    }
                    else
                    {//反方向大距离，表示未找到合适位置

                    }
                }
                break;
            case UIWidget.Pivot.Top:
            case UIWidget.Pivot.Bottom:
                if (dirFlag)
                {//上一次是往最近的方向移动
                    if (minFlag)
                    {//上一次移动的是小距离的，下一次移动x值的小距离
                        if (dragPos.x >= nearPos.x)
                        {//拖动物体在目标物体右边，往右移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Right, false, true);
                        }
                        else
                        {//往右移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Left, false, true);
                        }
                    }
                    else
                    {//上一次移动的是大距离的，下一次移动反方向的大距离
                        if (dragPos.x >= nearPos.x)
                        {//拖动物体在目标物体右边，往左移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Left, false, false);
                        }
                        else
                        {//往右移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Right, false, false);
                        }
                    }
                }
                else
                {//上一次不是往最近的方向移动
                    if (minFlag)
                    {//上一次移动的是小距离的，下一次正方向的大距离
                        if (dragPos.x >= nearPos.x)
                        {//拖动物体在目标物体右边，往左移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Left, true, false);
                        }
                        else
                        {//拖动物体在目标物体左边，往右移动
                            return GetSeparateTargetPosition(dragObj, dragPos, nearPos, dragData, nearData, UIWidget.Pivot.Right, true, false);
                        }
                    }
                    else
                    {//反方向大距离，表示未找到合适位置

                    }
                }
                break;
        }
        return dragPos;
    }

    bool IsLegitimatePosition(GameObject obj, Vector3 targetPos, TopologyPartData data)
    {
        bool result = true;
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            if (kvp.Key != obj)
            {//排除掉自己
                Vector3 itemPos = kvp.Key.transform.localPosition;
                if (Mathf.Abs(targetPos.x - itemPos.x) <= (data.width + kvp.Value.width) / 2 && Mathf.Abs(targetPos.y - itemPos.y) <= (data.height + kvp.Value.height) / 2)
                {//重叠了
                    result = false;
                    break;
                }
            }
        }
        return result;
    }


    bool GetLegitimatePositionDir(GameObject dragObj, ref Vector3 dragPos, TopologyPartData dragData, UIWidget.Pivot lastDir)
    {
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            if (kvp.Key != dragObj)
            {//排除掉自己
                Vector3 itemPos = kvp.Key.transform.localPosition;
                float offsetX = Mathf.Abs(dragPos.x - itemPos.x) - (dragData.width + kvp.Value.width) / 2;
                float offsetY = Mathf.Abs(dragPos.y - itemPos.y) - (dragData.height + kvp.Value.height) / 2;
                if (offsetX <= 0 && offsetY <= 0)
                {
                    switch (lastDir)
                    {
                        case UIWidget.Pivot.Left:
                        case UIWidget.Pivot.Right:
                            if (dragPos.y >= itemPos.y)
                            {
                                dragPos.y += offsetY;
                            }
                            else
                            {
                                dragPos.y -= offsetY;
                            }
                            break;
                        case UIWidget.Pivot.Top:
                        case UIWidget.Pivot.Bottom:
                            if (dragPos.x >= itemPos.x)
                            {
                                dragPos.x += offsetX;
                            }
                            else
                            {
                                dragPos.x -= offsetX;
                            }
                            break;
                    }
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 获得离obj按距离升序排列的队列，list[0]最近
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    List<GameObject> GetNearGameObject(GameObject obj, TopologyPartData data)
    {
        Dictionary<GameObject, float> dict = new Dictionary<GameObject, float>();
        List<GameObject> list = new List<GameObject>();
        List<GameObject> coincideList = new List<GameObject>();
        Vector3 pos = obj.transform.localPosition;
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            if (kvp.Key != obj)
            {
                Vector3 itemPos = kvp.Key.transform.localPosition;
                if (Math.Abs(pos.x - itemPos.x) <= (data.width + kvp.Value.width) / 2 && Math.Abs(pos.y - itemPos.y) <= (data.height + kvp.Value.height) / 2)
                {//重叠了
                    coincideList.Add(kvp.Key);
                }
                dict[kvp.Key] = Vector2.Distance(itemPos, pos);
                list.Add(kvp.Key);
            }
        }
        NearDistanceSort sort = new NearDistanceSort(dict);
        list.Sort(sort);
        if (!coincideList.Contains(list[0]))
        {
            for (int i = 0, imax = coincideList.Count; i < imax; ++i)
            {
                list.Remove(coincideList[i]);
                list.Insert(i, coincideList[i]);
            }
        }
        return list;
    }


    //////////////////////////////////////////////////////////////////////////
    //连线

    void CreateConnectionLine(GameObject obj, GameObject otherObj)
    {
        Vector2 pos = obj.transform.localPosition + obj.transform.parent.localPosition;
        Vector2 otherPos = otherObj.transform.localPosition + otherObj.transform.parent.localPosition;

        UIWidget.Pivot objDir = GetLineDir(obj, pos, otherObj, otherPos);
        UIWidget.Pivot otherDir = GetLineDir(otherObj, otherPos, obj, pos);

        if (obj.name.StartsWith("port"))
        {
            CreateConnectionLine(pos, objDir, otherPos, otherDir, obj, otherObj);
        }
        else if (otherObj.name.StartsWith("port"))
        {
            CreateConnectionLine(otherPos, otherDir, pos, objDir, otherObj, obj);
        }
        else
        {
            float offset = mMotherBoxTrans.localPosition.x - pos.x;
            float otherOffset = mMotherBoxTrans.localPosition.x - otherPos.x;
            //保证连接方向是从主板向两端发散的
            if (offset <= 0 && otherOffset <= 0)
            {
                if (offset > otherOffset)
                {
                    CreateConnectionLine(pos, objDir, otherPos, otherDir, obj, otherObj);
                }
                else
                {
                    CreateConnectionLine(otherPos, otherDir, pos, objDir, otherObj, obj);
                }
            }
            else
            {
                if (offset > otherOffset)
                {
                    CreateConnectionLine(otherPos, otherDir, pos, objDir, otherObj, obj);
                }
                else
                {
                    CreateConnectionLine(pos, objDir, otherPos, otherDir, obj, otherObj);
                }
            }
        }
    }

    void CreateConnectionLine(Vector2 pos, UIWidget.Pivot pivot, Vector2 otherPos, UIWidget.Pivot otherPivot, GameObject obj, GameObject otherObj)
    {
        float offsetX = Mathf.Abs(pos.x - otherPos.x);
        float offsetY = Mathf.Abs(pos.y - otherPos.y);

        List<TopologyPartData> list = new List<TopologyPartData>();
        if (Mathf.Abs(pos.x - otherPos.x) <= mAngleSize.x
            && (pivot == UIWidget.Pivot.Top && otherPivot == UIWidget.Pivot.Bottom
            || pivot == UIWidget.Pivot.Bottom && otherPivot == UIWidget.Pivot.Top))
        {//一条竖线连接
            TopologyPartData data = new TopologyPartData();
            data.partType = TopologyPartType.Line;
            data.localEulerAngles = new Vector3(0, 0, 90);
            data.width = (int)Mathf.Abs(pos.y - otherPos.y);
            data.localPosition = new Vector2((pos.x + otherPos.x) / 2, (pos.y + otherPos.y) / 2);
            list.Add(data);
        }
        else if (Mathf.Abs(pos.y - otherPos.y) <= mAngleSize.y
            && (pivot == UIWidget.Pivot.Left && otherPivot == UIWidget.Pivot.Right
            || pivot == UIWidget.Pivot.Right && otherPivot == UIWidget.Pivot.Left))
        {//一条横线连接
            TopologyPartData data = new TopologyPartData();
            data.partType = TopologyPartType.Line;
            data.width = (int)Mathf.Abs(pos.x - otherPos.x);
            data.localPosition = new Vector2((pos.x + otherPos.x) / 2, (pos.y + otherPos.y) / 2);
            list.Add(data);
        }
        else
        {
            /*int width = 20;
            Vector2 newPos = GetNewPosition(pos, pivot, width);
            Vector2 newOtherPos = GetNewPosition(otherPos, otherPivot, width);*/
            float modulus = 0;
            if (pivot == UIWidget.Pivot.Left || pivot == UIWidget.Pivot.Right)
            {
                modulus = 1;
            }
            if (obj.name.StartsWith("port"))
            {
                int width = 20;
                Vector2 newPos = GetNewPosition(pos, pivot, width);
                List<TopologyPartData> lineList = CreateTwoConnections(newPos, pivot, otherPos, otherPivot, modulus);
                AddStartOrEndLine(lineList, true, pivot, pos, width);
                list.AddRange(lineList);
            }
            else
            {
                List<TopologyPartData> lineList = CreateTwoConnections(pos, pivot, otherPos, otherPivot, modulus);
                list.AddRange(lineList);
            }
            
            //AddStartOrEndLine(lineList, true, pivot, pos, width);
            //AddStartOrEndLine(lineList, false, otherPivot, otherPos, width);
            
        }
        for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            GameObject tmp = null;
            if (list[i].partType == TopologyPartType.Line)
            {
                tmp = CreateLineItem(list[i]);
            }
            else if (list[i].partType == TopologyPartType.Line_Angle)
            {
                tmp = CreateAngleItem(list[i]);
            }
            if (null != tmp)
            {
                mLineDict[tmp] = list[i];
                if (!mPartLineDict.ContainsKey(obj))
                {
                    List<GameObject> objList = new List<GameObject>();
                    mPartLineDict[obj] = objList;
                }
                mPartLineDict[obj].Add(tmp);
                if (!mPartLineDict.ContainsKey(otherObj))
                {
                    List<GameObject> objList = new List<GameObject>();
                    mPartLineDict[otherObj] = objList;
                }
                mPartLineDict[otherObj].Add(tmp);
            }
        }
    }


    List<TopologyPartData> CreateTwoConnections(Vector2 pos, UIWidget.Pivot pivot, Vector2 targetPos, UIWidget.Pivot targetPivot, float modulus)
    {
        List<TopologyPartData> list = new List<TopologyPartData>();
        if (modulus >= 0.01f)
        {//先x后y
            Vector2 nowTargetPos = pos;
            if (pivot == UIWidget.Pivot.Right && pos.x + mAngleSize.x > targetPos.x || pivot == UIWidget.Pivot.Left && pos.x - mAngleSize.x < targetPos.x)
            {//超过了，需转回去
                if (Mathf.Abs(nowTargetPos.x - targetPos.x) < mAngleSize.x)
                {//加横线线保证可以放下两个角
                    TopologyPartData line = new TopologyPartData();
                    line.partType = TopologyPartType.Line;
                    line.width = (int)(mAngleSize.x - Mathf.Abs(nowTargetPos.x - targetPos.x));
                    if (pivot == UIWidget.Pivot.Right)
                    {
                        line.localPosition = pos + new Vector2(line.width / 2, 0);
                        nowTargetPos = line.localPosition + new Vector2(line.width / 2, 0);
                    }
                    else
                    {
                        line.localPosition = pos - new Vector2(line.width / 2, 0);
                        nowTargetPos = line.localPosition - new Vector2(line.width / 2, 0);
                    }
                    list.Add(line);
                }
                TopologyPartData angle = new TopologyPartData();
                angle.partType = TopologyPartType.Line_Angle;
                nowTargetPos = SetAngleLineData(angle, nowTargetPos, targetPos, pivot);
                list.Add(angle);
                if (targetPos.y > pos.y)
                {
                    pivot = UIWidget.Pivot.Top;
                }
                else
                {
                    pivot = UIWidget.Pivot.Bottom;
                }
                bool needTwoAngle = true;
                if (nowTargetPos.x > targetPos.x && targetPivot == UIWidget.Pivot.Right || nowTargetPos.x < targetPos.x && targetPivot == UIWidget.Pivot.Left)
                {
                    needTwoAngle = false;
                }
                //加纵向
                TopologyPartData vtLine = new TopologyPartData();
                vtLine.partType = TopologyPartType.Line;
                vtLine.localEulerAngles = new Vector3(0, 0, 90);
                if (needTwoAngle)
                {
                    vtLine.width = (int)(Mathf.Abs(nowTargetPos.y - targetPos.y) - mAngleSize.y * 2);
                }
                else
                {
                    vtLine.width = (int)(Mathf.Abs(nowTargetPos.y - targetPos.y) - mAngleSize.y);
                }
                if (targetPos.y > nowTargetPos.y)
                {
                    vtLine.localPosition = new Vector2(nowTargetPos.x, nowTargetPos.y + vtLine.width / 2);
                    nowTargetPos = vtLine.localPosition + new Vector2(0, vtLine.width / 2);
                }
                else
                {
                    vtLine.localPosition = new Vector2(nowTargetPos.x, nowTargetPos.y - vtLine.width / 2);
                    nowTargetPos = vtLine.localPosition - new Vector2(0, vtLine.width / 2);
                }
                list.Add(vtLine);
                //加角
                TopologyPartData angle1 = new TopologyPartData();
                angle1.partType = TopologyPartType.Line_Angle;
                nowTargetPos = SetAngleLineData(angle1, nowTargetPos, targetPos, pivot);
                list.Add(angle1);
                if (targetPos.x > nowTargetPos.x)
                {
                    pivot = UIWidget.Pivot.Right;
                }
                else
                {
                    pivot = UIWidget.Pivot.Left;
                }
                if (!needTwoAngle || Mathf.Abs(targetPos.x - nowTargetPos.x) > mAngleSize.x)
                {
                    //加横线
                    TopologyPartData hzLine = new TopologyPartData();
                    hzLine.partType = TopologyPartType.Line;
                    if (needTwoAngle)
                    {
                        hzLine.width = (int)(Mathf.Abs(targetPos.x - nowTargetPos.x) - mAngleSize.x);

                    }
                    else
                    {
                        hzLine.width = (int)Mathf.Abs(targetPos.x - nowTargetPos.x);
                    }
                    if (pivot == UIWidget.Pivot.Right)
                    {
                        hzLine.localPosition = nowTargetPos + new Vector2(hzLine.width / 2, 0);
                        nowTargetPos = hzLine.localPosition + new Vector2(hzLine.width / 2, 0);
                    }
                    else
                    {
                        hzLine.localPosition = nowTargetPos - new Vector2(hzLine.width / 2, 0);
                        nowTargetPos = hzLine.localPosition - new Vector2(hzLine.width / 2, 0);
                    }
                    list.Add(hzLine);
                }

                if (needTwoAngle)
                {
                    TopologyPartData angle2 = new TopologyPartData();
                    angle2.partType = TopologyPartType.Line_Angle;
                    nowTargetPos = SetAngleLineData(angle2, nowTargetPos, targetPos, pivot);
                    list.Add(angle2);
                }

            }
            else
            {
                bool needOneAngle = targetPos.y > pos.y && targetPivot == UIWidget.Pivot.Bottom || targetPos.y < pos.y && targetPivot == UIWidget.Pivot.Top;
                if (pivot == UIWidget.Pivot.Top || pivot == UIWidget.Pivot.Bottom)
                {//需要加个角连接
                    TopologyPartData angle1 = new TopologyPartData();
                    angle1.partType = TopologyPartType.Line_Angle;
                    nowTargetPos = SetAngleLineData(angle1, pos, targetPos, pivot);
                    list.Add(angle1);
                }
                //加横线
                float offsetX = Mathf.Abs(nowTargetPos.x - targetPos.x);
                if (needOneAngle)
                {//结尾时不需要拐角,只有中间一个拐角
                    offsetX -= mAngleSize.x;
                }
                else
                {//需要两个拐角
                    offsetX -= 2 * mAngleSize.x;
                }
                TopologyPartData hzLine = new TopologyPartData();
                hzLine.partType = TopologyPartType.Line;
                hzLine.width = (int)(offsetX * modulus);
                if (targetPos.x > pos.x)
                {
                    hzLine.localPosition = nowTargetPos + new Vector2(hzLine.width / 2, 0);
                    nowTargetPos += new Vector2(hzLine.width, 0);
                }
                else
                {
                    hzLine.localPosition = nowTargetPos - new Vector2(hzLine.width / 2, 0);
                    nowTargetPos -= new Vector2(hzLine.width, 0);
                }
                list.Add(hzLine);
                //加拐角
                TopologyPartData angle2 = new TopologyPartData();
                angle2.partType = TopologyPartType.Line_Angle;

                if (targetPos.x > pos.x)
                {
                    nowTargetPos = SetAngleLineData(angle2, nowTargetPos, targetPos, UIWidget.Pivot.Right);
                }
                else
                {
                    nowTargetPos = SetAngleLineData(angle2, nowTargetPos, targetPos, UIWidget.Pivot.Left);
                }
                list.Add(angle2);
                //加竖线
                TopologyPartData vtLine = new TopologyPartData();
                vtLine.partType = TopologyPartType.Line;
                vtLine.localEulerAngles = new Vector3(0, 0, 90);

                if (needOneAngle)
                {//结尾时不需要拐角,只有中间一个拐角
                    vtLine.width = (int)Mathf.Abs(targetPos.y - nowTargetPos.y);
                }
                else
                {//需要两个拐角
                    vtLine.width = (int)(Mathf.Abs(targetPos.y - nowTargetPos.y) - mAngleSize.y);
                }
                if (targetPos.y > pos.y)
                {
                    vtLine.localPosition = new Vector2(nowTargetPos.x, nowTargetPos.y + vtLine.width / 2);
                    nowTargetPos += new Vector2(0, vtLine.width);
                }
                else
                {
                    vtLine.localPosition = new Vector2(nowTargetPos.x, nowTargetPos.y - vtLine.width / 2);
                    nowTargetPos -= new Vector2(0, vtLine.width);
                }
                list.Add(vtLine);
                if (!needOneAngle)
                {//需要加一个拐角
                    TopologyPartData angle3 = new TopologyPartData();
                    angle3.partType = TopologyPartType.Line_Angle;
                    if (targetPos.y > pos.y)
                    {
                        nowTargetPos = SetAngleLineData(angle3, nowTargetPos, targetPos, UIWidget.Pivot.Top);
                    }
                    else
                    {
                        nowTargetPos = SetAngleLineData(angle3, nowTargetPos, targetPos, UIWidget.Pivot.Bottom);
                    }

                    list.Add(angle3);
                }

                if (modulus < 0.999f)
                {//还未完成,再加横线
                    TopologyPartData hzLine1 = new TopologyPartData();
                    hzLine1.partType = TopologyPartType.Line;
                    hzLine1.width = (int)Mathf.Abs(nowTargetPos.x - targetPos.x);
                    if (targetPos.x > pos.x)
                    {
                        hzLine1.localPosition = new Vector2(nowTargetPos.x + hzLine1.width / 2, nowTargetPos.y);
                    }
                    else
                    {
                        hzLine1.localPosition = new Vector2(nowTargetPos.x - hzLine1.width / 2, nowTargetPos.y);
                    }
                    list.Add(hzLine1);
                }
            }
        }
        else
        {
            Vector2 nowTargetPos = pos;
            if (pivot == UIWidget.Pivot.Top && pos.y + mAngleSize.y > targetPos.y || pivot == UIWidget.Pivot.Bottom && pos.y - mAngleSize.y < targetPos.y)
            {//超过了，需转回去
                if (Mathf.Abs(nowTargetPos.y - targetPos.y) < mAngleSize.y)
                {//加竖线保证可以放下两个角
                    TopologyPartData line = new TopologyPartData();
                    line.partType = TopologyPartType.Line;
                    line.localEulerAngles = new Vector3(0, 0, 90);
                    line.width = (int)(mAngleSize.y - Mathf.Abs(nowTargetPos.y - targetPos.y));
                    if (pivot == UIWidget.Pivot.Top)
                    {
                        line.localPosition = pos + new Vector2(0, line.width / 2);
                        nowTargetPos = line.localPosition + new Vector2(0, line.width / 2);
                    }
                    else
                    {
                        line.localPosition = pos - new Vector2(0, line.width / 2);
                        nowTargetPos = line.localPosition - new Vector2(0, line.width / 2);
                    }
                    list.Add(line);
                }
                TopologyPartData angle = new TopologyPartData();
                angle.partType = TopologyPartType.Line_Angle;
                nowTargetPos = SetAngleLineData(angle, nowTargetPos, targetPos, pivot);
                list.Add(angle);
                if (targetPos.x > pos.x)
                {
                    pivot = UIWidget.Pivot.Right;
                }
                else
                {
                    pivot = UIWidget.Pivot.Left;
                }
                bool needTwoAngle = true;
                if (nowTargetPos.y > targetPos.y && targetPivot == UIWidget.Pivot.Top || nowTargetPos.y < targetPos.y && targetPivot == UIWidget.Pivot.Bottom)
                {
                    needTwoAngle = false;
                }
                //加横向
                TopologyPartData hzLine = new TopologyPartData();
                hzLine.partType = TopologyPartType.Line;
                if (needTwoAngle)
                {
                    hzLine.width = (int)(Mathf.Abs(nowTargetPos.x - targetPos.x) - mAngleSize.x * 2);
                }
                else
                {
                    hzLine.width = (int)(Mathf.Abs(nowTargetPos.x - targetPos.x) - mAngleSize.x);
                }
                if (targetPos.x > nowTargetPos.x)
                {
                    hzLine.localPosition = new Vector2(nowTargetPos.x + hzLine.width / 2, nowTargetPos.y);
                    nowTargetPos = hzLine.localPosition + new Vector2(hzLine.width / 2, 0);
                }
                else
                {
                    hzLine.localPosition = new Vector2(nowTargetPos.x - hzLine.width / 2, nowTargetPos.y);
                    nowTargetPos = hzLine.localPosition - new Vector2(hzLine.width / 2, 0);
                }
                list.Add(hzLine);
                //加角
                TopologyPartData angle1 = new TopologyPartData();
                angle1.partType = TopologyPartType.Line_Angle;
                nowTargetPos = SetAngleLineData(angle1, nowTargetPos, targetPos, pivot);
                list.Add(angle1);
                if (targetPos.y > nowTargetPos.y)
                {
                    pivot = UIWidget.Pivot.Top;
                }
                else
                {
                    pivot = UIWidget.Pivot.Bottom;
                }
                if (!needTwoAngle || Mathf.Abs(targetPos.y - nowTargetPos.y) > mAngleSize.y)
                {
                    //加竖线
                    TopologyPartData vtLine = new TopologyPartData();
                    vtLine.partType = TopologyPartType.Line;
                    vtLine.localEulerAngles = new Vector3(0, 0, 90);
                    if (needTwoAngle)
                    {
                        vtLine.width = (int)(Mathf.Abs(targetPos.y - nowTargetPos.y) - mAngleSize.y);

                    }
                    else
                    {
                        vtLine.width = (int)Mathf.Abs(targetPos.y - nowTargetPos.y);
                    }
                    if (pivot == UIWidget.Pivot.Top)
                    {
                        vtLine.localPosition = nowTargetPos + new Vector2(0, vtLine.width / 2);
                        nowTargetPos = vtLine.localPosition + new Vector2(0, vtLine.width / 2);
                    }
                    else
                    {
                        vtLine.localPosition = nowTargetPos - new Vector2(0, vtLine.width / 2);
                        nowTargetPos = vtLine.localPosition - new Vector2(0, vtLine.width / 2);
                    }
                    list.Add(vtLine);
                }

                if (needTwoAngle)
                {
                    TopologyPartData angle2 = new TopologyPartData();
                    angle2.partType = TopologyPartType.Line_Angle;
                    nowTargetPos = SetAngleLineData(angle2, nowTargetPos, targetPos, pivot);
                    list.Add(angle2);
                }


            }
            else
            {
                if (pivot == UIWidget.Pivot.Left || pivot == UIWidget.Pivot.Right)
                {//需要加个角连接
                    TopologyPartData angle1 = new TopologyPartData();
                    angle1.partType = TopologyPartType.Line_Angle;
                    nowTargetPos = SetAngleLineData(angle1, pos, targetPos, pivot);
                    list.Add(angle1);
                    if (targetPos.x > pos.x)
                    {
                        pivot = UIWidget.Pivot.Right;
                    }
                    else
                    {
                        pivot = UIWidget.Pivot.Left;
                    }
                }
                bool needOneAngle = targetPos.x > pos.x && targetPivot == UIWidget.Pivot.Left || targetPos.x < pos.x && targetPivot == UIWidget.Pivot.Right;


                //加竖线
                TopologyPartData vtLine = new TopologyPartData();
                vtLine.partType = TopologyPartType.Line;
                vtLine.localEulerAngles = new Vector3(0, 0, 90);
                if (needOneAngle)
                {//结尾时不需要拐角,只有中间一个拐角
                    vtLine.width = (int)(Mathf.Abs(targetPos.y - nowTargetPos.y) - mAngleSize.y);
                }
                else
                {//需要两个拐角
                    vtLine.width = (int)(Mathf.Abs(targetPos.y - nowTargetPos.y) - mAngleSize.y * 2);
                }
                if (targetPos.y > pos.y)
                {
                    vtLine.localPosition = new Vector2(nowTargetPos.x, nowTargetPos.y + vtLine.width / 2);
                    nowTargetPos += new Vector2(0, vtLine.width);
                }
                else
                {
                    vtLine.localPosition = new Vector2(nowTargetPos.x, nowTargetPos.y - vtLine.width / 2);
                    nowTargetPos -= new Vector2(0, vtLine.width);
                }
                list.Add(vtLine);

                TopologyPartData angle2 = new TopologyPartData();
                angle2.partType = TopologyPartType.Line_Angle;
                if (targetPos.y > pos.y)
                {
                    nowTargetPos = SetAngleLineData(angle2, nowTargetPos, targetPos, UIWidget.Pivot.Top);
                }
                else
                {
                    nowTargetPos = SetAngleLineData(angle2, nowTargetPos, targetPos, UIWidget.Pivot.Bottom);
                }
                list.Add(angle2);

                TopologyPartData hzLine1 = new TopologyPartData();
                hzLine1.partType = TopologyPartType.Line;
                hzLine1.width = (int)Mathf.Abs(nowTargetPos.x - targetPos.x);
                if (!needOneAngle)
                {
                    hzLine1.width -= (int)mAngleSize.x;
                }
                if (targetPos.x > pos.x)
                {
                    hzLine1.localPosition = new Vector2(nowTargetPos.x + hzLine1.width / 2, nowTargetPos.y);
                }
                else
                {
                    hzLine1.localPosition = new Vector2(nowTargetPos.x - hzLine1.width / 2, nowTargetPos.y);
                }
                list.Add(hzLine1);

                if (!needOneAngle)
                {
                    TopologyPartData angle3 = new TopologyPartData();
                    angle3.partType = TopologyPartType.Line_Angle;
                    if (targetPos.y > pos.y)
                    {
                        nowTargetPos = SetAngleLineData(angle3, nowTargetPos, targetPos, UIWidget.Pivot.Top);
                    }
                    else
                    {
                        nowTargetPos = SetAngleLineData(angle3, nowTargetPos, targetPos, UIWidget.Pivot.Bottom);
                    }
                    list.Add(angle3);
                }
            }

        }
        return list;
    }

    /// <summary>
    /// 加入两个点开头或者结束时的线段
    /// </summary>
    /// <param name="lineList"></param>
    /// <param name="startFlag"></param>
    /// <param name="pivot"></param>
    /// <param name="pos"></param>
    /// <param name="width"></param>
    void AddStartOrEndLine(List<TopologyPartData> lineList, bool startFlag, UIWidget.Pivot pivot, Vector2 pos, int width)
    {
        if (startFlag && lineList.Count > 0 && lineList[0].partType == TopologyPartType.Line || !startFlag && lineList.Count > 0 && lineList[lineList.Count - 1].partType == TopologyPartType.Line)
        {
            TopologyPartData data = null;
            if (startFlag)
            {
                data = lineList[0];
            }
            else
            {
                data = lineList[lineList.Count - 1];
            }
            data.width += width;
            switch (pivot)
            {
                case UIWidget.Pivot.Left:
                    data.localPosition.x += width / 2;
                    break;
                case UIWidget.Pivot.Right:
                    data.localPosition.x -= width / 2;
                    break;
                case UIWidget.Pivot.Top:
                    data.localPosition.y -= width / 2;
                    break;
                case UIWidget.Pivot.Bottom:
                    data.localPosition.y += width / 2;
                    break;
            }
        }
        else
        {
            TopologyPartData lineData = new TopologyPartData();
            lineData.partType = TopologyPartType.Line;
            lineData.width = width;
            switch (pivot)
            {
                case UIWidget.Pivot.Left:
                    lineData.localPosition = new Vector3(pos.x - width / 2, pos.y);
                    break;
                case UIWidget.Pivot.Right:
                    lineData.localPosition = new Vector3(pos.x + width / 2, pos.y);
                    break;
                case UIWidget.Pivot.Top:
                    lineData.localPosition = new Vector3(pos.x, pos.y + width / 2);
                    lineData.localEulerAngles = new Vector3(0, 0, 90);
                    break;
                case UIWidget.Pivot.Bottom:
                    lineData.localPosition = new Vector3(pos.x, pos.y - width / 2);
                    lineData.localEulerAngles = new Vector3(0, 0, 90);
                    break;
            }
            if (startFlag)
            {
                lineList.Insert(0, lineData);
            }
            else
            {
                lineList.Add(lineData);
            }
        }
    }
    UIWidget.Pivot GetLineDir(GameObject obj, Vector2 pos, GameObject targetObj, Vector2 targetPos)
    {
        UIWidget.Pivot dir = UIWidget.Pivot.Center;
        string name = obj.name;
        string targetName = targetObj.name;
        int rightCount = 3;
        if (mMainboardType == TopologyPartType.MainBoard_new_low)
        {
            rightCount = 4;
        }
        if (name.StartsWith("port"))
        {
            int port = int.Parse(name.Substring("port".Length));
            if (port <= rightCount)
            {
                dir = UIWidget.Pivot.Right;
            }
            else
            {
                dir = UIWidget.Pivot.Left;
            }
        }
        else if (name.Contains("_left_"))
        {
            if (targetName.StartsWith("port"))
            {//与主板相连
                int num = int.Parse(targetName.Substring("port".Length));
                if (num > rightCount)
                {
                    if (pos.x + mAngleSize.x < targetPos.x - 20)
                    {
                        if ((int)targetPos.y >= (int)pos.y)
                        {
                            dir = UIWidget.Pivot.Top;
                        }
                        else
                        {
                            dir = UIWidget.Pivot.Bottom;
                        }
                    }
                    else
                    {
                        dir = UIWidget.Pivot.Left;
                    }
                }
                else
                {
                    if (Mathf.Abs(pos.y - targetPos.y) < mAngleSize.x)
                    {//横线
                        dir = UIWidget.Pivot.Left;
                    }
                    else
                    {
                        if ((int)targetPos.y >= (int)pos.y)
                        {
                            dir = UIWidget.Pivot.Top;
                        }
                        else
                        {
                            dir = UIWidget.Pivot.Bottom;
                        }
                    }
                }
            }
            else
            {
                float dis = Vector2.Distance(mMotherBoxTrans.localPosition, pos);
                float targetDis = Vector2.Distance(mMotherBoxTrans.localPosition, targetPos);
                if (Mathf.Abs(pos.x - targetPos.x) < mAngleSize.x)
                {//竖线
                    if ((int)targetPos.y >= (int)pos.y)
                    {
                        dir = UIWidget.Pivot.Top;
                    }
                    else
                    {
                        dir = UIWidget.Pivot.Bottom;
                    }
                }
                else if (Mathf.Abs(pos.y - targetPos.y) < mAngleSize.y)
                {//横线
                    dir = UIWidget.Pivot.Left;
                }
                else
                {
                    if (dis < targetDis)
                    {
                        if (pos.x - mAngleSize.x > targetPos.x)
                        {
                            dir = UIWidget.Pivot.Left;
                        }
                        else
                        {
                            if ((int)targetPos.y >= (int)pos.y)
                            {
                                dir = UIWidget.Pivot.Top;
                            }
                            else
                            {
                                dir = UIWidget.Pivot.Bottom;
                            }
                        }
                    }
                    else
                    {
                        if (targetName.Contains("_right_"))
                        {
                            if (pos.x + mAngleSize.x < targetPos.x)
                            {
                                dir = UIWidget.Pivot.Left;
                            }
                            else
                            {
                                if ((int)targetPos.y >= (int)pos.y)
                                {
                                    dir = UIWidget.Pivot.Top;
                                }
                                else
                                {
                                    dir = UIWidget.Pivot.Bottom;
                                }
                            }
                        }
                        else
                        {
                            if (pos.x + mAngleSize.x < targetPos.x)
                            {
                                if ((int)targetPos.y >= (int)pos.y)
                                {
                                    dir = UIWidget.Pivot.Top;
                                }
                                else
                                {
                                    dir = UIWidget.Pivot.Bottom;
                                }
                            }
                            else
                            {
                                dir = UIWidget.Pivot.Left;
                            }
                        }
                    }
                }
            }
        }
        else if (name.Contains("_right_"))
        {
            if (targetName.StartsWith("port"))
            {//与主板相连
                int num = int.Parse(targetName.Substring("port".Length));
                if (num <= rightCount)
                {
                    if (pos.x + mAngleSize.x > targetPos.x + 20)
                    {
                        if ((int)targetPos.y >= (int)pos.y)
                        {
                            dir = UIWidget.Pivot.Top;
                        }
                        else
                        {
                            dir = UIWidget.Pivot.Bottom;
                        }
                    }
                    else
                    {
                        dir = UIWidget.Pivot.Right;
                    }
                }
                else
                {
                    if (Mathf.Abs(pos.y - targetPos.y) < mAngleSize.x)
                    {//横线
                        dir = UIWidget.Pivot.Right;
                    }
                    else
                    {
                        if ((int)targetPos.y >= (int)pos.y)
                        {
                            dir = UIWidget.Pivot.Top;
                        }
                        else
                        {
                            dir = UIWidget.Pivot.Bottom;
                        }
                    }
                }
            }
            else
            {
                float dis = Vector2.Distance(mMotherBoxTrans.localPosition, pos);
                float targetDis = Vector2.Distance(mMotherBoxTrans.localPosition, targetPos);
                if (Mathf.Abs(pos.x - targetPos.x) < mAngleSize.x)
                {//竖线
                    if ((int)targetPos.y >= (int)pos.y)
                    {
                        dir = UIWidget.Pivot.Top;
                    }
                    else
                    {
                        dir = UIWidget.Pivot.Bottom;
                    }
                }
                else if (Mathf.Abs(pos.y - targetPos.y) < mAngleSize.y)
                {//横线
                    dir = UIWidget.Pivot.Right;
                }
                else
                {
                    if (dis < targetDis)
                    {
                        if (pos.x + mAngleSize.x < targetPos.x)
                        {
                            dir = UIWidget.Pivot.Right;
                        }
                        else
                        {
                            if ((int)targetPos.y >= (int)pos.y)
                            {
                                dir = UIWidget.Pivot.Top;
                            }
                            else
                            {
                                dir = UIWidget.Pivot.Bottom;
                            }
                        }
                    }
                    else
                    {
                        if (targetName.Contains("_left_"))
                        {
                            if (pos.x - mAngleSize.x > targetPos.x)
                            {
                                dir = UIWidget.Pivot.Right;
                            }
                            else
                            {
                                if ((int)targetPos.y >= (int)pos.y)
                                {
                                    dir = UIWidget.Pivot.Top;
                                }
                                else
                                {
                                    dir = UIWidget.Pivot.Bottom;
                                }
                            }
                        }
                        else
                        {
                            if (pos.x - mAngleSize.x > targetPos.x)
                            {
                                if ((int)targetPos.y >= (int)pos.y)
                                {
                                    dir = UIWidget.Pivot.Top;
                                }
                                else
                                {
                                    dir = UIWidget.Pivot.Bottom;
                                }
                            }
                            else
                            {
                                dir = UIWidget.Pivot.Right;
                            }
                        }
                    }
                }
            }
            
        }
        else if (name.Equals("blank_left"))
        {
            dir = UIWidget.Pivot.Left;
        }
        else if (name.Equals("blank_right"))
        {
            dir = UIWidget.Pivot.Right;
        }
        return dir;
    }

    Vector2 GetNewPosition(Vector2 oldPos, UIWidget.Pivot dir, int width)
    {
        switch (dir)
        {
            case UIWidget.Pivot.Left:
                oldPos.x -= width;
                break;
            case UIWidget.Pivot.Right:
                oldPos.x += width;
                break;
            case UIWidget.Pivot.Top:
                oldPos.y += width;
                break;
            case UIWidget.Pivot.Bottom:
                oldPos.y -= width;
                break;
        }
        return oldPos;
    }

    /// <summary>
    /// 设置连接线的拐角的属性
    /// </summary>
    /// <param name="data"></param>
    /// <param name="pos">起始位置</param>
    /// <param name="targetPos">目标位置</param>
    /// <param name="pivot">原线段是的方向</param>
    Vector2 SetAngleLineData(TopologyPartData data, Vector2 pos, Vector2 targetPos, UIWidget.Pivot pivot)
    {
        Vector2 nowPos = pos;
        float offset = 0.5f;
        switch (pivot)
        {
            case UIWidget.Pivot.Left:
                if (targetPos.y > pos.y)
                {
                    data.localEulerAngles = new Vector3(0, 0, 180);
                    data.localPosition = new Vector2(pos.x - mAngleSize.x, pos.y - offset);
                    nowPos = data.localPosition + new Vector2(0, mAngleSize.y);
                }
                else
                {
                    data.localEulerAngles = new Vector3(0, 0, 90);
                    data.localPosition = new Vector2(pos.x - mAngleSize.x, pos.y + offset);
                    nowPos = data.localPosition + new Vector2(0, -mAngleSize.y);
                }
                break;
            case UIWidget.Pivot.Right:
                if (targetPos.y > pos.y)
                {
                    data.localEulerAngles = new Vector3(0, 0, 270);
                    data.localPosition = new Vector2(pos.x + mAngleSize.x, pos.y - offset);
                    nowPos = data.localPosition + new Vector2(0, mAngleSize.y);
                }
                else
                {
                    data.localEulerAngles = Vector3.zero;
                    data.localPosition = new Vector2(pos.x + mAngleSize.x, pos.y + offset);
                    nowPos = data.localPosition + new Vector2(0, -mAngleSize.y);
                }
                break;
            case UIWidget.Pivot.Top:
                if (targetPos.x > pos.x)
                {
                    data.localEulerAngles = new Vector3(0, 0, 90);
                    data.localPosition = new Vector2(pos.x - offset, pos.y + mAngleSize.y);
                    nowPos = data.localPosition + new Vector2(mAngleSize.x, 0);
                }
                else
                {
                    data.localEulerAngles = Vector3.zero;
                    data.localPosition = new Vector2(pos.x + offset, pos.y + mAngleSize.y);
                    nowPos = data.localPosition + new Vector2(-mAngleSize.x, 0);
                }
                break;
            case UIWidget.Pivot.Bottom:
                if (targetPos.x > pos.x)
                {
                    data.localEulerAngles = new Vector3(0, 0, 180);
                    data.localPosition = new Vector2(pos.x - offset, pos.y - mAngleSize.y);
                    nowPos = data.localPosition + new Vector2(mAngleSize.x, 0);
                }
                else
                {
                    data.localEulerAngles = new Vector3(0, 0, 270);
                    data.localPosition = new Vector2(pos.x + offset, pos.y - mAngleSize.y);
                    nowPos = data.localPosition + new Vector2(-mAngleSize.x, 0);
                }
                break;
        }
        return nowPos;
    }
    /// <summary>
    /// 生成一条直线
    /// </summary>
    /// <returns></returns>
    GameObject CreateLineItem(TopologyPartData data)
    {
        if (null != mLineItem)
        {
            GameObject line = GameObject.Instantiate(mLineItem) as GameObject;
            line.transform.parent = mPanelSelfTrans;
            line.transform.localScale = Vector2.one;
            line.transform.localEulerAngles = data.localEulerAngles;
            UISprite sp = line.GetComponent<UISprite>();
            if (null != sp)
            {
                sp.width = data.width;
            }
            line.transform.localPosition = data.localPosition;
            line.SetActive(true);
            return line;
        }
        return null;
    }

    /// <summary>
    /// 生成连线的拐角
    /// </summary>
    /// <returns></returns>
    GameObject CreateAngleItem(TopologyPartData data)
    {
        if (null != mAngleItem)
        {
            GameObject angle = GameObject.Instantiate(mAngleItem) as GameObject;
            angle.transform.parent = mPanelSelfTrans;
            angle.transform.localScale = Vector2.one;
            angle.transform.localEulerAngles = data.localEulerAngles;
            angle.transform.localPosition = data.localPosition;
            angle.SetActive(true);
            return angle;
        }
        return null;
    }
    /// <summary>
    /// 获取接口类型
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    PartPortType GetPortType(GameObject port)
    {
        PartPortType portType = PartPortType.Port_Type_Pin_3;
        GameObject part = port.transform.parent.gameObject;
        if (mPartPortDict.ContainsKey(part))
        {
            List<PartPortData> list = mPartPortDict[part];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                if (list[i].portObj == port)
                {
                    portType = list[i].portType;
                    break;
                }
            }
        }
        return portType;
    }


    void SetPartPortState(GameObject part)
    {
        if (mPartPortDict.ContainsKey(part))
        {
            List<PartPortData> list = mPartPortDict[part];
            PartPortData data = null;
            TopologyPartData partData = null;
            if (mTopologyDict.ContainsKey(part))
            {
                partData = mTopologyDict[part];
            }
            if (null != partData)
            {
                for (int i = 0, imax = list.Count; i < imax; ++i)
                {
                    data = list[i];
                    if (mPortConnectionDict.ContainsKey(data.portObj))
                    {
                        SetPortState(data.portObj, partData.partType, true);
                        TopologyPartData otherData = null;
                        GameObject otherPart = mPortConnectionDict[data.portObj].transform.parent.gameObject;
                        if (mTopologyDict.ContainsKey(otherPart))
                        {
                            otherData = mTopologyDict[otherPart];
                        }
                        if (null != otherData)
                        {
                            SetPortState(mPortConnectionDict[data.portObj], otherData.partType, true);
                        }
                        
                    }
                    else
                    {
                        SetPortState(data.portObj, partData.partType, false);
                    }
                }
            }
            
        }
    }

    /// <summary>
    /// 设置连接点图片
    /// </summary>
    /// <param name="port"></param>
    /// <param name="partType"></param>
    /// <param name="connected"></param>
    void SetPortState(GameObject port, TopologyPartType partType, bool connected)
    {
        string spriteName = string.Empty;
        switch (partType)
        {
            case TopologyPartType.None:
            case TopologyPartType.Servo:
            case TopologyPartType.Motor:
                if (connected)
                {
                    spriteName = "dian_hui";
                }
                else
                {
                    spriteName = "dian_hui";
                }
                break;
            case TopologyPartType.MainBoard:
            case TopologyPartType.MainBoard_new_low:
                if (port.name.Equals("port6") && partType == TopologyPartType.MainBoard)
                {
                    if (connected)
                    {
                        spriteName = "san_hui";
                    }
                    else
                    {
                        spriteName = "san_hui";
                    }
                }
                else
                {
                    if (mPortConnectionDict.ContainsKey(port))
                    {
                        string portName = mPortConnectionDict[port].name;
                        if (portName.StartsWith("s_") || portName.StartsWith("m_") || portName.StartsWith("blank_"))
                        {
                            if (connected)
                            {
                                spriteName = "dian_hui";
                            }
                            else
                            {
                                spriteName = "dian_hui";
                            }
                        }
                        else
                        {
                            if (connected)
                            {
                                spriteName = "dian_hui";
                            }
                            else
                            {
                                spriteName = "dian_hui";
                            }
                        }
                    }
                    else
                    {
                        if (connected)
                        {
                            spriteName = "dian_hui";
                        }
                        else
                        {
                            spriteName = "dian_hui";
                        }
                    }
                }
                break;
            case TopologyPartType.Gyro:
                if (connected)
                {
                    spriteName = "san_hui";
                }
                else
                {
                    spriteName = "san_hui";
                }
                break;
            default:
                if (connected)
                {
                    spriteName = "dian_hui";
                }
                else
                {
                    spriteName = "dian_hui";
                }
                break;
        }
        UIButton btn = port.GetComponent<UIButton>();
        if (null != btn)
        {
            btn.normalSprite = spriteName;
        }
        UISprite sp = port.GetComponentInChildren<UISprite>();
        if (null != sp && !string.IsNullOrEmpty(spriteName))
        {
            sp.spriteName = spriteName;
        }
    }

    void SetSelectPortState(GameObject port, bool selectFlag)
    {
        GameObject parentObj = port.transform.parent.gameObject;
        if (mTopologyDict.ContainsKey(parentObj))
        {
            SetPortState(port, mTopologyDict[parentObj].partType, selectFlag);
        }
    }

    /// <summary>
    /// 移除拖动物体连接的线
    /// </summary>
    /// <param name="dragObj"></param>
    void RemovePartLine(GameObject dragObj)
    {
        if (mPartPortDict.ContainsKey(dragObj))
        {
            List<PartPortData> list = mPartPortDict[dragObj];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                RemovePortLine(list[i].portObj);
            }
        }

    }
    /// <summary>
    /// 移除掉某个连接口线
    /// </summary>
    /// <param name="port"></param>
    void RemovePortLine(GameObject port)
    {
        if (mPartLineDict.ContainsKey(port))
        {
            for (int i = 0, imax = mPartLineDict[port].Count; i < imax; ++i)
            {
                if (mLineDict.ContainsKey(mPartLineDict[port][i]))
                {
                    mLineDict.Remove(mPartLineDict[port][i]);
                }
                GameObject.Destroy(mPartLineDict[port][i]);
            }
            mPartLineDict.Remove(port);
            if (mPortConnectionDict.ContainsKey(port))
            {
                if (mPartLineDict.ContainsKey(mPortConnectionDict[port]))
                {
                    mPartLineDict.Remove(mPortConnectionDict[port]);
                }
            }
        }
    }
    /// <summary>
    /// 恢复零件连线
    /// </summary>
    /// <param name="dragObj"></param>
    void RecoverPartLine(GameObject dragObj)
    {
        if (mPartPortDict.ContainsKey(dragObj))
        {
            List<PartPortData> list = mPartPortDict[dragObj];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                RecoverPortLine(list[i].portObj);
            }
        }
    }
    /// <summary>
    /// 恢复端口连线
    /// </summary>
    /// <param name="port"></param>
    void RecoverPortLine(GameObject port)
    {
        if (mPortConnectionDict.ContainsKey(port) && !mPartLineDict.ContainsKey(port))
        {
            CreateConnectionLine(port, mPortConnectionDict[port]);
        }
    }



    //////////////////////////////////////////////////////////////////////////
    //升级
    void OpenUpdateAnim()
    {
        foreach (KeyValuePair<Transform, UpdateShowData> kvp in mUpdateDict)
        {
            StateUpdateStart(kvp.Key);
            kvp.Value.StartUpdate();
        }
    }

    void StateUpdateStart(Transform trans)
    {
        Transform bg = trans.Find("bg");
        if (null != bg)
        {
            bg.gameObject.SetActive(false);
        }
        Transform state = trans.Find("state");
        if (null != state)
        {
            state.gameObject.SetActive(false);
        }
    }

    void StateUpdateFinished(Transform trans, bool state, bool instant)
    {
        Transform partBg = trans.Find("bg");
        if (null != partBg)
        {
            partBg.gameObject.SetActive(true);
        }
        Transform stateTrans = trans.Find("state");
        if (null != stateTrans)
        {
            stateTrans.gameObject.SetActive(true);
        }
        UISprite bg = GameHelper.FindChildComponent<UISprite>(stateTrans, "bg");
        if (null != bg)
        {
            if (state)
            {
                bg.spriteName = "success";
            }
            else
            {
                bg.spriteName = "unsuccessful";
            }
            bg.MakePixelPerfect();
            /*TweenRotation tweenRota = bg.gameObject.GetComponent<TweenRotation>();
            if (null != tweenRota)
            {
                tweenRota.enabled = false;
            }*/
        }
        /*Transform label = stateTrans.Find("Label");
        if (null != label)
        {
            label.gameObject.SetActive(false);
        }*/
        Transform icon = stateTrans.Find("icon");
        if (null != icon)
        {
            icon.gameObject.SetActive(true);
            UISprite sp = icon.GetComponent<UISprite>();
            if (null != sp)
            {
                if (state)
                {
                    sp.spriteName = "yes";
                    sp.MakePixelPerfect();
                }
                else
                {
                    sp.spriteName = "exception";
                    sp.MakePixelPerfect();
                }
            }
            icon.localScale = Vector3.zero;
            if (instant)
            {
                icon.localScale = Vector3.one;
            }
            else
            {
                TweenScale tweenScale = icon.GetComponent<TweenScale>();
                GameHelper.PlayTweenScale(tweenScale, Vector3.one, 1);
            }
            
        }
    }

    //////////////////////////////////////////////////////////////////////////
    //个人拓扑图

    /// <summary>
    /// 生成所有的空白格子
    /// </summary>
    void CreateAllBlankSpace()
    {
        if (null != mMotherBoxTrans && mPartPortDict.ContainsKey(mMotherBoxTrans.gameObject))
        {
            bool havePin3Flag = false;
            bool havePin4Flag = false;
            foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
            {
                if (GetPartPortType(kvp.Value.partType) == PartPortType.Port_Type_Pin_3)
                {
                    havePin3Flag = true;
                }
                else
                {
                    havePin4Flag = true;
                }
                if (havePin3Flag && havePin4Flag)
                {
                    break;
                }
            }
            List<PartPortData> list = mPartPortDict[mMotherBoxTrans.gameObject];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                if (list[i].portType == PartPortType.Port_Type_Pin_3 && havePin3Flag)
                {
                    CreatePortBlankSpace(list[i].portObj);
                }
                else if (list[i].portType == PartPortType.Port_Type_Pin_4 && havePin4Flag)
                {
                    CreatePortBlankSpace(list[i].portObj);
                }
            }
        }
    }

    /// <summary>
    /// 生成主板上某个端口的空白格
    /// </summary>
    /// <param name="portNum"></param>
    void CreatePortBlankSpace(GameObject port)
    {
        GameObject lastPort = port;
        GameObject otherObj = port;
        while (null != otherObj)
        {
            lastPort = otherObj;
            if (mPortConnectionDict.ContainsKey(otherObj))
            {
                lastPort = mPortConnectionDict[otherObj];
                if (lastPort.transform.parent.name.Equals("blank"))
                {
                    lastPort = null;
                    otherObj = null;
                    break;
                }
                otherObj = GetPartOtherPort(mPortConnectionDict[otherObj]);
            }
            else
            {
                break;
            }
        }
        if (null != lastPort)
        {//生成空白格与此端口连接
            PartPortType portType = PartPortType.Port_Type_Pin_3;
            int portId = int.Parse(port.name.Substring("port".Length));
            if (mMainboardType == TopologyPartType.MainBoard)
            {
                if (portId == 6)
                {
                    portType = PartPortType.Port_Type_Pin_4;
                }
            }

            GameObject obj = CreateBlankGameObject(portType);
            if (null != obj)
            {
                int rightPortCount = 3;
                if (mMainboardType == TopologyPartType.MainBoard_new_low)
                {
                    rightPortCount = 4;
                }
                if (lastPort == port)
                {
                    if (mMainboardType == TopologyPartType.MainBoard)
                    {
                        obj.transform.localPosition = mPlayerStartPos[portId - 1];
                    } else
                    {
                        obj.transform.localPosition = mPlayerStartPos_low[portId - 1];  
                    }
                }
                else
                {
                    Vector3 lastPortPos = lastPort.transform.localPosition + lastPort.transform.parent.localPosition;
                    
                    if (portId <= rightPortCount)
                    {
                        obj.transform.localPosition = lastPortPos + new Vector3(mLineWidth + mServoSize.x / 2, 0);
                    }
                    else
                    {
                        obj.transform.localPosition = lastPortPos - new Vector3(mLineWidth + mServoSize.x / 2, 0);
                    }
                }
                GameObject conPort = null;
                if (mPartPortDict.ContainsKey(obj))
                {
                    List<PartPortData> list = mPartPortDict[obj];
                    for (int i = 0, imax = list.Count; i < imax; ++i)
                    {
                        if (portId <= rightPortCount && list[i].portObj.name.Equals("blank_left"))
                        {
                            conPort = list[i].portObj;
                            break;
                        }
                        else if (portId > rightPortCount && list[i].portObj.name.Equals("blank_right"))
                        {
                            conPort = list[i].portObj;
                            break;
                        }
                    }
                }
                TopologyPartData data = new TopologyPartData();
                data.localPosition = obj.transform.localPosition;
                data.partType = TopologyPartType.None;
                mTopologyDict[obj] = data;
                if (null != conPort)
                {
                    CreateConnectionLine(lastPort, conPort);
                    mPortConnectionDict[lastPort] = conPort;
                    mPortConnectionDict[conPort] = lastPort;
                    SetPartPortState(conPort.transform.parent.gameObject);
                }
                UIManager.SetButtonEventDelegate(obj.transform, mBtnDelegate);
            }
        }
    }
    /// <summary>
    /// 删除最后的空白格
    /// </summary>
    /// <param name="portType"></param>
    void DelLastBlank(PartPortType portType)
    {
        for (int i = 0, imax = mBlankList.Count; i < imax; ++i)
        {
            if (mPartPortDict.ContainsKey(mBlankList[i]))
            {
                List<PartPortData> list = mPartPortDict[mBlankList[i]];
                if (list.Count > 0)
                {
                    if (portType == list[0].portType)
                    {
                        DelBlank(mBlankList[i]);
                        --i;
                        --imax;
                    }
                }
            }
        }
    }
    /// <summary>
    /// 判断是否有某种接口空白格 
    /// </summary>
    /// <param name="portType"></param>
    /// <returns></returns>
    bool HavePortTypeBlank(PartPortType portType)
    {
        for (int i = 0, imax = mBlankList.Count; i < imax; ++i)
        {
            if (mPartPortDict.ContainsKey(mBlankList[i]))
            {
                List<PartPortData> list = mPartPortDict[mBlankList[i]];
                if (list.Count > 0)
                {
                    if (portType == list[0].portType)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    /// <summary>
    /// 删除空白格
    /// </summary>
    /// <param name="blank"></param>
    void DelBlank(GameObject blank)
    {
        RemovePartLine(blank);
        if (mPartPortDict.ContainsKey(blank))
        {
            List<PartPortData> list = mPartPortDict[blank];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                if (mPortConnectionDict.ContainsKey(list[i].portObj))
                {
                    GameObject conport = mPortConnectionDict[list[i].portObj];
                    if (mTopologyDict.ContainsKey(conport.transform.parent.gameObject))
                    {
                        SetPortState(conport, mTopologyDict[conport.transform.parent.gameObject].partType, false);
                    }

                    if (mPortConnectionDict.ContainsKey(conport))
                    {
                        mPortConnectionDict.Remove(conport);
                    }
                    mPortConnectionDict.Remove(list[i].portObj);
                    
                }
            }
        }
        mPartPortDict.Remove(blank);
        mTopologyDict.Remove(blank);
        mBlankList.Remove(blank);
        GameObject.Destroy(blank);
    }
    /// <summary>
    /// 移除掉某个零件
    /// </summary>
    /// <param name="part"></param>
    void RemovePart(GameObject part)
    {
        Transform choicePart = FindChoiceObjForTopoPart(part);
        if (null != choicePart)
        {
            SetChoiceQueuePartState(choicePart, false);
        }
        RemovePartLine(part);
        if (mPartPortDict.ContainsKey(part))
        {
            List<PartPortData> list = mPartPortDict[part];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                if (mPortConnectionDict.ContainsKey(list[i].portObj))
                {
                    GameObject conport = mPortConnectionDict[list[i].portObj];
                    if (mTopologyDict.ContainsKey(conport.transform.parent.gameObject))
                    {
                        SetPortState(conport, mTopologyDict[conport.transform.parent.gameObject].partType, false);
                    }

                    if (mPortConnectionDict.ContainsKey(conport))
                    {
                        mPortConnectionDict.Remove(conport);
                    }
                    mPortConnectionDict.Remove(list[i].portObj);

                }
            }
        }
        mTopologyDict[part].isIndependent = true;
        part.transform.parent = mIndependentQueueTrans;
        part.transform.localPosition = Vector3.zero;
        part.transform.localScale = Vector3.one;
        mTopologyDict[part].localPosition = Vector3.zero;
        mIndependentQueueDict[part] = mTopologyDict[part];
        mTopologyDict.Remove(part);
    }
    /// <summary>
    /// 恢复某种接口类型的空白格
    /// </summary>
    /// <param name="portType"></param>
    void RecoverPortTypeBlank(PartPortType portType)
    {
        if (null != mMotherBoxTrans)
        {
            List<PartPortData> list = mPartPortDict[mMotherBoxTrans.gameObject];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                if (list[i].portType == portType)
                {
                    CreatePortBlankSpace(list[i].portObj);
                }
            }
        }
    }
    
    /// <summary>
    /// 删除不连续的零件
    /// </summary>
    void DelDiscontinuousPart()
    {
        if (null != mMotherBoxTrans && mPartPortDict.ContainsKey(mMotherBoxTrans.gameObject))
        {
            List<PartPortData> list = mPartPortDict[mMotherBoxTrans.gameObject];
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                DelDisconMainPort(list[i].portObj);
            }
        }
    }

    /// <summary>
    /// 删除某条连线上面不连续的零件
    /// </summary>
    /// <param name="port"></param>
    void DelDisconMainPort(GameObject port)
    {
        GameObject delPort = GetDisContinuousForMainPort(port);
        if (null != delPort)
        {
            GameObject otherObj = GetPartOtherPort(delPort);
            while (mPortConnectionDict.ContainsKey(otherObj))
            {
                GameObject removeObj = otherObj.transform.parent.gameObject;
                otherObj = GetPartOtherPort(mPortConnectionDict[otherObj]);
                RemoveTopologyPart(removeObj);
            }
            RemoveTopologyPart(otherObj.transform.parent.gameObject);
        }
        //ResetIndependentQueuePosition(true);
    }
    /// <summary>
    /// 移除掉拓扑图上的某个零件
    /// </summary>
    /// <param name="part"></param>
    void RemoveTopologyPart(GameObject part)
    {
        if (part.name.Equals("blank"))
        {
            DelBlank(part);
        }
        else
        {
            RemovePart(part);
        }
    }

    void InitChoicePartQueue()
    {
        if (null == mPin3Trans || null == mPin4Trans)
        {
            return;
        }
        List<GameObject> servoList = null;
        List<GameObject> motorList = null;
        List<GameObject> pin4List = null;
        List<GameObject> sensorList = null;
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
        {
            if (kvp.Value.partType == TopologyPartType.Servo)
            {
                if (null == servoList)
                {
                    servoList = new List<GameObject>();
                }
                servoList.Add(kvp.Key);
            } else if (kvp.Value.partType == TopologyPartType.Motor)
            {
                if (null == motorList)
                {
                    motorList = new List<GameObject>();
                }
                motorList.Add(kvp.Key);
            } else if (GetPartPortType(kvp.Value.partType) == PartPortType.Port_Type_Pin_4)
            {
                if (null == pin4List)
                {
                    pin4List = new List<GameObject>();
                }
                pin4List.Add(kvp.Key);
            }
            else
            {
                if (null == sensorList)
                {
                    sensorList = new List<GameObject>();
                }
                sensorList.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mTopologyDict)
        {
            if (kvp.Value.partType == TopologyPartType.MainBoard || kvp.Value.partType == TopologyPartType.MainBoard_new_low || kvp.Value.partType == TopologyPartType.None)
            {
                continue;
            }
            if (kvp.Value.partType == TopologyPartType.Servo)
            {
                if (null == servoList)
                {
                    servoList = new List<GameObject>();
                }
                servoList.Add(kvp.Key);
            } else if (kvp.Value.partType == TopologyPartType.Motor)
            {
                if (null == motorList)
                {
                    motorList = new List<GameObject>();
                }
                motorList.Add(kvp.Key);
            } else if (GetPartPortType(kvp.Value.partType) == PartPortType.Port_Type_Pin_4)
            {
                if (null == pin4List)
                {
                    pin4List = new List<GameObject>();
                }
                pin4List.Add(kvp.Key);
            } else
            {
                if (null == sensorList)
                {
                    sensorList = new List<GameObject>();
                }
                sensorList.Add(kvp.Key);
            }
        }

        if (null != servoList || null != motorList || null != sensorList)
        {
            List<GameObject> pin3List = new List<GameObject>();
            if (null != servoList)
            {
                servoList.Sort(delegate (GameObject a, GameObject b) 
                {
                    TopologyPartData aData = null;
                    TopologyPartData bData = null;
                    if (mIndependentQueueDict.ContainsKey(a))
                    {
                        aData = mIndependentQueueDict[a];
                    }
                    else
                    {
                        aData = mTopologyDict[a];
                    }
                    if (mIndependentQueueDict.ContainsKey(b))
                    {
                        bData = mIndependentQueueDict[b];
                    }
                    else
                    {
                        bData = mTopologyDict[b];
                    }
                    return (aData.id - bData.id);
                });
                pin3List.AddRange(servoList);
            }
            if (null != motorList)
            {
                motorList.Sort(delegate (GameObject a, GameObject b)
                {
                    TopologyPartData aData = null;
                    TopologyPartData bData = null;
                    if (mIndependentQueueDict.ContainsKey(a))
                    {
                        aData = mIndependentQueueDict[a];
                    }
                    else
                    {
                        aData = mTopologyDict[a];
                    }
                    if (mIndependentQueueDict.ContainsKey(b))
                    {
                        bData = mIndependentQueueDict[b];
                    }
                    else
                    {
                        bData = mTopologyDict[b];
                    }
                    return (aData.id - bData.id);
                });
                pin3List.AddRange(motorList);
            }
            if (null != sensorList)
            {
                sensorList.Sort(delegate (GameObject a, GameObject b) {
                    TopologyPartData aData = null;
                    TopologyPartData bData = null;
                    if (mIndependentQueueDict.ContainsKey(a))
                    {
                        aData = mIndependentQueueDict[a];
                    }
                    else
                    {
                        aData = mTopologyDict[a];
                    }
                    if (mIndependentQueueDict.ContainsKey(b))
                    {
                        bData = mIndependentQueueDict[b];
                    }
                    else
                    {
                        bData = mTopologyDict[b];
                    }
                    if (aData.partType != bData.partType)
                    {
                        return ((int)aData.partType - (int)bData.partType);
                    }
                    return (aData.id - bData.id);
                });
                pin3List.AddRange(sensorList);
            }
            CreateChoicePartQueue(pin3List, PartPortType.Port_Type_Pin_3);
        }
        if (null != pin4List)
        {
            pin4List.Sort(delegate (GameObject a, GameObject b) 
            {
                TopologyPartData aData = null;
                TopologyPartData bData = null;
                if (mIndependentQueueDict.ContainsKey(a))
                {
                    aData = mIndependentQueueDict[a];
                }
                else
                {
                    aData = mTopologyDict[a];
                }
                if (mIndependentQueueDict.ContainsKey(b))
                {
                    bData = mIndependentQueueDict[b];
                }
                else
                {
                    bData = mTopologyDict[b];
                }
                return (aData.id - bData.id);
            });
            CreateChoicePartQueue(pin4List, PartPortType.Port_Type_Pin_4);
        }
    }

    void CreateChoicePartQueue(List<GameObject> partList, PartPortType portType)
    {
        
        Transform panel = null;
        if (portType == PartPortType.Port_Type_Pin_3)
        {
            panel = mPin3Trans.Find("panel");
        }
        else
        {
            panel = mPin4Trans.Find("panel");
        }
        if (null == panel)
        {
            return;
        }
        UIScrollView scrollView = panel.GetComponent<UIScrollView>();
        Transform parentTrans = panel.Find("grid");
        for (int i = 0, imax = partList.Count; i < imax; ++i)
        {
            TopologyPartData data = null;
            if (mIndependentQueueDict.ContainsKey(partList[i]))
            {
                data = mIndependentQueueDict[partList[i]];
            }
            else
            {
                data = mTopologyDict[partList[i]];
            }
            GameObject obj = null;
            if (data.partType == TopologyPartType.Servo)
            {
                obj = CreateChoiceServo(data, parentTrans);
            } else if (data.partType == TopologyPartType.Motor)
            {
                obj = CreateChoiceMotor(data, parentTrans);
            } else
            {
                obj = CreateChoiceSensor(data, parentTrans);
            }
            if (mTopologyDict.ContainsKey(partList[i]))
            {
                if (null != obj)
                {
                    SetChoiceQueuePartState(obj.transform, true);
                }
            }
        }
        UIManager.SetButtonEventDelegate(parentTrans, mBtnDelegate);
        ResetQueuePosition(parentTrans, (int)mServoSize.x, 5, true, scrollView);
    }

    void RemoveChoicePartQueue()
    {
        if (null == mPin3Trans || null == mPin4Trans)
        {
            return;
        }
        Transform pin3Trans = mPin3Trans.Find("panel/grid");
        if (null != pin3Trans)
        {
            DestroyTransformChild(pin3Trans);
        }
        Transform pin4Trans = mPin4Trans.Find("panel/grid");
        if (null != pin4Trans)
        {
            DestroyTransformChild(pin4Trans);
        }
    }

    void DestroyTransformChild(Transform trans)
    {
        for (int i = trans.childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(trans.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 显示零件队列
    /// </summary>
    /// <param name="portType"></param>
    void ShowPartQueue(PartPortType portType, ServoModel modelType = ServoModel.Servo_Model_Angle)
    {
        if (!mShowChoicePanelFlag)
        {
            if (null != mChoicePartActiveCallBack)
            {
                mChoicePartActiveCallBack(true);
            }
        }
        Transform trans = null;
        if (portType == PartPortType.Port_Type_Pin_3)
        {
            trans = mPin3Trans;
            SetChoiceServoModel(modelType);
        }
        else
        {
            trans = mPin4Trans;
        }
        if (null != trans)
        {
            mShowChoicePanelFlag = true;
            mShowChoicePortType = portType;
            /*if (null != mChooseCloseBtnObj)
            {
                mChooseCloseBtnObj.SetActive(true);
            }*/
            trans.gameObject.SetActive(true);
            SetTransPosition(trans, mPinTransPos, false, false);
        }
    }
    /// <summary>
    /// 隐藏零件队列
    /// </summary>
    void HidePartQueue(bool instant)
    {
        Transform trans = null;
        if (mShowChoicePortType == PartPortType.Port_Type_Pin_3)
        {
            trans = mPin3Trans;
        }
        else
        {
            trans = mPin4Trans;
        }
        if (null != trans)
        {
            mShowChoicePanelFlag = false;
            /*if (null != mChooseCloseBtnObj)
            {
                mChooseCloseBtnObj.SetActive(false);
            }*/
            if (trans.gameObject.activeSelf)
            {
                SetTransPosition(trans, mPinTransPos - new Vector3(0, 300), true, instant);
            }
        }
    }

    void SetTransPosition(Transform trans, Vector3 pos, bool hideFlag, bool instant)
    {
        if (null != trans)
        {
            TweenPosition tweenPosition = trans.GetComponent<TweenPosition>();
            if (null != tweenPosition && !instant)
            {
                GameHelper.PlayTweenPosition(tweenPosition, pos, 0.6f);
                if (hideFlag)
                {
                    tweenPosition.SetOnFinished(delegate ()
                    {
                        trans.gameObject.SetActive(false);
                    });
                }
                else
                {
                    if (null != tweenPosition.onFinished)
                    {
                        tweenPosition.onFinished.Clear();
                    }
                }
            }
            else
            {
                trans.localPosition = pos;
                if (hideFlag)
                {
                    trans.gameObject.SetActive(false);
                }
                else
                {
                    if (null != tweenPosition && null != tweenPosition.onFinished)
                    {
                        tweenPosition.onFinished.Clear();
                    }
                }
            }
            

        }
    }

    void SwitchChoiceServoModel(ServoModel modelType)
    {
        if (null != mServosConnection)
        {
            if (!SingletonObject<PopWinManager>.GetInst().IsExist(typeof(PromptMsg)))
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机模式不可修改"));
            }
            return;
        }
        if (modelType == ServoModel.Servo_Model_Angle)
        {
            SetChoiceServoModel(modelType);
            if (null != mSelectedPartGameObject && mSelectedPartGameObject.name.StartsWith("servo_"))
            {
                SetPlayerServoModel(mSelectedPartGameObject, modelType);
            }
        }
        else
        {
            if (!SingletonObject<PopWinManager>.GetInst().IsExist(typeof(PromptMsg)))
            {
                PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("轮模式切换提示"), PopTurnTitleOnClick);
            }
        }
    }

    void PopTurnTitleOnClick(GameObject obj)
    {
        string name = obj.name;
        if (name.Equals(PromptMsg.LeftBtnName))
        {

        }
        else if (name.Equals(PromptMsg.RightBtnName))
        {
            SetChoiceServoModel(ServoModel.Servo_Model_Turn);
            if (null != mSelectedPartGameObject && mSelectedPartGameObject.name.StartsWith("servo_"))
            {
                SetPlayerServoModel(mSelectedPartGameObject, ServoModel.Servo_Model_Turn);
            }
        }
    }
    void SetChoiceServoModel(ServoModel modelType)
    {
        if (null != mPin3Trans)
        {
            Transform switchBtn = mPin3Trans.Find("switchBtn");
            if (null != switchBtn)
            {
                UIButton btn = switchBtn.GetComponent<UIButton>();
                if (null != btn)
                {
                    if (modelType == ServoModel.Servo_Model_Angle)
                    {
                        btn.normalSprite = "close";
                    }
                    else
                    {
                        btn.normalSprite = "open";
                    }
                    UISprite bg = btn.tweenTarget.GetComponent<UISprite>();
                    if (null != bg)
                    {
                        bg.spriteName = btn.normalSprite;
                    }
                }
                UILabel lb = GameHelper.FindChildComponent<UILabel>(switchBtn, "Label");
                if (null != lb)
                {
                    lb.text = LauguageTool.GetIns().GetText("轮模式");
                    /*if (modelType == ServoModel.Servo_Model_Angle)
                    {
                        lb.text = LauguageTool.GetIns().GetText("角模式");
                    }
                    else
                    {
                        lb.text = LauguageTool.GetIns().GetText("轮模式");
                    }*/
                }
            }
            
        }
        mChoiceServoModel = modelType;
    }

    /// <summary>
    /// 设置选择队列的零件状态
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="chooseFlag"></param>
    void SetChoiceQueuePartState(Transform part, bool chooseFlag)
    {
        Transform state = part.Find("state");
        if (null != state)
        {
            state.gameObject.SetActive(chooseFlag);
        }
    }
    /// <summary>
    /// 选中了某个零件
    /// </summary>
    /// <param name="part"></param>
    void SetSelectedPart(GameObject part)
    {
        if (null != part)
        {
            if (null != mSelectedBgTrans)
            {
                mSelectedBgTrans.parent = part.transform;
                mSelectedBgTrans.localPosition = Vector3.zero;
                mSelectedBgTrans.localScale = Vector3.one;
                mSelectedBgTrans.localEulerAngles = Vector3.zero;
                mSelectedBgTrans.gameObject.SetActive(true);
            }
        }
        else
        {
            if (null != mSelectedBgTrans)
            {
                mSelectedBgTrans.parent = mTrans;
                mSelectedBgTrans.gameObject.SetActive(false);
            }
        }
        mSelectedPartGameObject = part;
    }
    /// <summary>
    /// 通过选择列表中的零件获取拓扑图中的该零件
    /// </summary>
    /// <param name="part"></param>
    /// <returns></returns>
    GameObject FindTopoObjForChoicePart(GameObject part)
    {
        string name = part.name;
        if (name.StartsWith("c_"))
        {
            name = name.Substring("c_".Length);
            GameObject obj = null;
            if (null != mPanelSelfTrans)
            {
                Transform tmp = mPanelSelfTrans.Find(name);
                if (null != tmp)
                {
                    obj = tmp.gameObject;
                }
            }
            if (null == obj && null != mIndependentQueueTrans)
            {
                Transform tmp = mIndependentQueueTrans.Find(name);
                if (null != tmp)
                {
                    obj = tmp.gameObject;
                }
            }
            return obj;
        }
        return null;
    }
    /// <summary>
    /// 通过拓扑图上的零件获取选择列表对应的零件
    /// </summary>
    /// <param name="topoPart"></param>
    /// <returns></returns>
    Transform FindChoiceObjForTopoPart(GameObject topoPart)
    {
        string choiceName = "c_" + topoPart.name;
        Transform choicePart = null;
        PartPortType portType = PartPortType.Port_Type_Pin_3;
        if (mTopologyDict.ContainsKey(topoPart))
        {
            portType = GetPartPortType(mTopologyDict[topoPart].partType);
        }
        if (portType == PartPortType.Port_Type_Pin_3)
        {
            choicePart = mPin3Trans.Find("panel/grid/" + choiceName);
        }
        else
        {
            choicePart = mPin4Trans.Find("panel/grid/" + choiceName);
        }
        return choicePart;
    }
    /// <summary>
    /// 往拓扑图上添加零件
    /// </summary>
    /// <param name="addPart"></param>
    /// <param name="targetObj"></param>
    void AddTopologyPart(GameObject addPart, GameObject targetObj)
    {
        SwitchGameObject(targetObj, addPart);
        GameObject port = GetPartMainPort(addPart);
        if (null != port)
        {
            PartPortType portType = PartPortType.Port_Type_Pin_3;
            if (TopologyPartType.MainBoard == mMainboardType && port.name.Contains("6"))
            {
                portType = PartPortType.Port_Type_Pin_4;
            }
            bool finished = IsPartSettingFinished(portType);
            if (finished)
            {
                DelLastBlank(portType);
            }
            else
            {
                CreatePortBlankSpace(port);
            }
        }
    }
    /// <summary>
    /// 删除零件
    /// </summary>
    /// <param name="delPart"></param>
    GameObject DelTopologyPart(GameObject delPart)
    {
        PartPortType portType = PartPortType.Port_Type_Pin_3;
        if (mTopologyDict.ContainsKey(delPart))
        {
            portType = GetPartPortType(mTopologyDict[delPart].partType);
        }
        Transform choicePart = FindChoiceObjForTopoPart(delPart);
        if (null != choicePart)
        {
            SetChoiceQueuePartState(choicePart, false);
        }
        bool needRecoverBlank = true;
        if (HavePortTypeBlank(portType))
        {
            needRecoverBlank = false;
        }
        GameObject blank = CreateBlankGameObject(portType);
        if (null != blank)
        {
            SwitchGameObject(delPart, blank);
            if (needRecoverBlank)
            {//恢复空白格
                RecoverPortTypeBlank(portType);
            }
            UIManager.SetButtonEventDelegate(blank.transform, mBtnDelegate);
        }
        return blank;
    }

    void SwitchGameObject(GameObject oldTopoObj, GameObject otherObj)
    {
        otherObj.transform.parent = mPanelSelfTrans;
        ObjToTopology(otherObj);
        NGUITools.MarkParentAsChanged(otherObj);
        otherObj.transform.localPosition = oldTopoObj.transform.localPosition;
        otherObj.transform.localScale = Vector3.one;
        mTopologyDict[otherObj].localPosition = otherObj.transform.localPosition;
        List<PartPortData> oldPortList = null;
        List<PartPortData> otherPortList = null;
        if (mPartPortDict.ContainsKey(oldTopoObj))
        {
            oldPortList = mPartPortDict[oldTopoObj];
        }
        if (mPartPortDict.ContainsKey(otherObj))
        {
            otherPortList = mPartPortDict[otherObj];
        }
        bool delOldFlag = false;
        if (oldTopoObj.name.Equals("blank"))
        {//空白格
            delOldFlag = true;
        }
        else
        {
            oldTopoObj.transform.parent = mIndependentQueueTrans;
            oldTopoObj.transform.localScale = Vector3.one;
            if (mTopologyDict.ContainsKey(oldTopoObj))
            {
                mTopologyDict[oldTopoObj].isIndependent = true;
                mIndependentQueueDict[oldTopoObj] = mTopologyDict[oldTopoObj];
                mTopologyDict.Remove(oldTopoObj);
            }
        }
        //删除掉原来的线
        RemovePartLine(oldTopoObj);
        //建立新的连线
        if (null != oldPortList && null != otherPortList)
        {
            for (int i = 0, imax = oldPortList.Count; i < imax; ++i)
            {
                if (mPortConnectionDict.ContainsKey(oldPortList[i].portObj))
                {
                    string dirName = string.Empty;
                    if (oldPortList[i].portObj.name.Contains("left"))
                    {
                        dirName = "left";
                    }
                    else if (oldPortList[i].portObj.name.Contains("right"))
                    {
                        dirName = "right";
                    }
                    GameObject otherPort = null;
                    if (!string.IsNullOrEmpty(dirName))
                    {
                        for (int j = 0, jmax = otherPortList.Count; j < jmax; ++j)
                        {
                            if (otherPortList[j].portObj.name.Contains(dirName))
                            {
                                otherPort = otherPortList[j].portObj;
                                break;
                            }
                        }
                    }
                    if (null != otherPort)
                    {//替换端点
                        GameObject conPort = mPortConnectionDict[oldPortList[i].portObj];
                        //移除原来的点
                        mPortConnectionDict.Remove(conPort);
                        mPortConnectionDict.Remove(oldPortList[i].portObj);
                        //连接新的
                        CreateConnectionLine(otherPort, conPort);
                        mPortConnectionDict[otherPort] = conPort;
                        mPortConnectionDict[conPort] = otherPort;
                        SetPartPortState(otherObj);
                    }
                }
                
            }
        }
        if (delOldFlag)
        {
            mPartPortDict.Remove(oldTopoObj);
            mTopologyDict.Remove(oldTopoObj);
            mBlankList.Remove(oldTopoObj);
            GameObject.Destroy(oldTopoObj);
        }
    }
    /// <summary>
    /// 设置个人模型的舵机类型
    /// </summary>
    /// <param name="servo"></param>
    /// <param name="modelType"></param>
    void SetPlayerServoModel(GameObject servo, ServoModel modelType)
    {
        byte id = byte.Parse(servo.name.Substring("servo_".Length));
        if (null != mRobot)
        {
            mRobot.GetAllDjData().UpdateServoModel(id, modelType);
        }
        SetServoModelIcon(servo, modelType);
        Transform choicePart = FindChoiceObjForTopoPart(servo);
        if (null != choicePart)
        {
            SetServoModelIcon(choicePart.gameObject, modelType);
        }
    }
    /// <summary>
    /// 判断某种类型的端口零件是否设置完毕
    /// </summary>
    /// <param name="portType"></param>
    /// <returns></returns>
    bool IsPartSettingFinished(PartPortType portType)
    {
        bool finishedFlag = true;
        foreach (KeyValuePair<GameObject, TopologyPartData> kvp in mIndependentQueueDict)
        {
            if (GetPartPortType(kvp.Value.partType) == portType)
            {
                finishedFlag = false;
                break;
            }
        }
        if (finishedFlag)
        {
            if (null != mMotherBoxTrans)
            {
                if (portType == PartPortType.Port_Type_Pin_3)
                {
                    int count = mMainboardType == TopologyPartType.MainBoard ? 5 : 8;
                    for (int i = 1; i <= count; ++i)
                    {
                        Transform port = mMotherBoxTrans.Find(string.Format("port{0}", i));
                        if (null != port)
                        {
                            finishedFlag = !IsDisContinuousForMainPort(port.gameObject);
                            if (!finishedFlag)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Transform port = mMotherBoxTrans.Find("port6");
                    if (null != port)
                    {
                        finishedFlag = !IsDisContinuousForMainPort(port.gameObject);
                    }
                }
            }
        }
        return finishedFlag;
    }

    /// <summary>
    /// 通过主板端口获取该端口的断开的起始位置
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    GameObject GetDisContinuousForMainPort(GameObject port)
    {
        GameObject startPort = null;
        if (mPortConnectionDict.ContainsKey(port))
        {
            GameObject otherPort = GetPartOtherPort(mPortConnectionDict[port]);
            if (null != otherPort)
            {
                while (mPortConnectionDict.ContainsKey(otherPort))
                {
                    if (otherPort.name.StartsWith("blank_"))
                    {
                        startPort = mPortConnectionDict[otherPort];
                        break;
                    }
                    otherPort = GetPartOtherPort(mPortConnectionDict[otherPort]);
                }
            }
        }
        return startPort;
    }
    
    /// <summary>
    /// 通过主板端口获取该端口的连接是否是断开的
    /// </summary>
    /// <param name="port"></param>
    /// <returns>true表示断开</returns>
    bool IsDisContinuousForMainPort(GameObject port)
    {
        if (mPortConnectionDict.ContainsKey(port))
        {
            GameObject otherPort = GetPartOtherPort(mPortConnectionDict[port]);
            if (null != otherPort)
            {
                while (mPortConnectionDict.ContainsKey(otherPort))
                {
                    if (otherPort.name.StartsWith("blank_") && !mPortConnectionDict[otherPort].name.StartsWith("blank_"))
                    {
                        return true;
                    }
                    otherPort = GetPartOtherPort(mPortConnectionDict[otherPort]);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 自动选择零件
    /// </summary>
    void AutoSelectPartGameObject()
    {
        if (null == mSelectedPartGameObject)
        {
            List<PartPortData> list = mPartPortDict[mMotherBoxTrans.gameObject];
            if (null != list)
            {
                for (int i = 0, imax = list.Count; i < imax; ++i)
                {
                    if (list[i].portObj.name.Equals("port1"))
                    {
                        if (null != mPortConnectionDict && mPortConnectionDict.ContainsKey(list[i].portObj))
                        {
                            GameObject otherPort = mPortConnectionDict[list[i].portObj];
                            SetSelectedPart(otherPort.transform.parent.gameObject);
                            ShowPartQueue(PartPortType.Port_Type_Pin_3, ServoModel.Servo_Model_Angle);
                        }
                        break;
                    }
                }
            }
        }
        else
        {

        }
    }
    #endregion
}



//////////////////////////////////////////////////////////////////////////
//距离排序
public class NearDistanceSort : IComparer<GameObject>
{


    Dictionary<GameObject, float> topologDict;

    public NearDistanceSort(Dictionary<GameObject, float> dict)
    {
        topologDict = dict;
    }

    public int Compare(GameObject x, GameObject y)
    {
        if (!topologDict.ContainsKey(x))
        {
            return 1;
        }
        if (!topologDict.ContainsKey(y))
        {
            return -1;
        }
        return (int)(topologDict[x] - topologDict[y]);
    }
}

//////////////////////////////////////////////////////////////////////////
//升级显示
public class UpdateShowData
{
    Transform updateTrans;
    UISprite bg;
    UILabel label;
    Transform waitTrans;
    Transform turnModelTrans;
    bool turnModelActive;

    public UpdateShowData(Transform trans)
    {
        updateTrans = trans;
        bg = GameHelper.FindChildComponent<UISprite>(trans, "bg");
        label = GameHelper.FindChildComponent<UILabel>(trans, "Label");
        waitTrans = trans.Find("wait");
        turnModelTrans = trans.parent.Find("turnModel");
        if (null != turnModelTrans)
        {
            turnModelActive = turnModelTrans.gameObject.activeSelf;
        }
    }

    public void UpdateProgress(int progress)
    {
        if (null != bg)
        {
            bg.fillAmount = (100 - progress) / 100.0f;
        }
        if (null != label)
        {
            label.text = string.Format("{0}%", progress);
        }
    }

    public void StartUpdate()
    {
        if (null != updateTrans)
        {
            updateTrans.gameObject.SetActive(true);
        }
        if (null != waitTrans)
        {
            waitTrans.gameObject.SetActive(false);
        }
        if (null != turnModelTrans && turnModelActive)
        {
            turnModelTrans.gameObject.SetActive(false);
        }
        UpdateProgress(0);
    }

    public void UpdateWait()
    {
        if (null != label)
        {
            label.text = string.Empty;
        }
        if (null != waitTrans)
        {
            waitTrans.gameObject.SetActive(true);
        }
    }

    public void UptateFinished()
    {
        if (null != updateTrans)
        {
            updateTrans.gameObject.SetActive(false);
        }
        if (null != waitTrans)
        {
            waitTrans.gameObject.SetActive(false);
        }
        if (null != turnModelTrans && turnModelActive)
        {
            turnModelTrans.gameObject.SetActive(turnModelActive);
        }
    }
}