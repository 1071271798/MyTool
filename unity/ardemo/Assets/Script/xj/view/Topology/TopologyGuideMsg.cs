using Game;
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:TopologyGuideMsg.cs
/// Description:
/// Time:2016/9/26 16:04:14
/// </summary>
public class TopologyGuideMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    Transform mGridTrans;

    GameObject mServoItem;
    GameObject mSensorItem;

    Vector3 mServoSize;

    Robot mRobot;

    int mShowSize = 5;
    #endregion

    #region 公有函数
    public TopologyGuideMsg()
    {
        mUIResPath = "Prefab/UI/TopologyGuideMsg";
    }
    #endregion

    public override void Init()
    {
        base.Init();
        mCoverAlpha = 0.95f;
    }
    #region 其他函数
    protected override void AddEvent()
    {
        base.AddEvent();
        try
        {
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            if (null == mTrans)
            {
                return;
            }

            GameHelper.SetPosition(mTrans.Find("btnClose"), UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos);
            mServoItem = mTrans.Find("servo").gameObject;
            mServoSize = NGUIMath.CalculateRelativeWidgetBounds(mServoItem.transform).size;
            mServoItem.gameObject.SetActive(false);
            mSensorItem = mTrans.Find("sensor").gameObject;

            Transform topologyPanel = mTrans.Find("topologyPanel");
            if (null != topologyPanel)
            {
                topologyPanel.localPosition = UIManager.GetWinPos(topologyPanel, UIWidget.Pivot.Top, 0, 140);
                UILabel lb = GameHelper.FindChildComponent<UILabel>(topologyPanel, "tips/Label");
                if (null != lb)
                {
                    lb.text = LauguageTool.GetIns().GetText("点击选择你的控制器端口");
                }
            }
            

            Transform pin3Trans = mTrans.Find("pin3trans");
            if (null != pin3Trans)
            {
                pin3Trans.localPosition = new Vector3(0, -PublicFunction.GetHeight() / 2 + 54);
                Transform bg = pin3Trans.Find("bg");
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

                Transform delBtn = pin3Trans.Find("delBtn");
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
                
                Transform panel = pin3Trans.Find("panel");
                if (null != panel)
                {
                    mGridTrans = panel.Find("grid");
                    Vector3 pos = panel.localPosition;
                    pos.x = -PublicFunction.GetWidth() / 2 + PublicFunction.Back_Btn_Pos.x + 120;
                    panel.localPosition = pos;
                    UIPanel uiPanel = panel.GetComponent<UIPanel>();
                    if (null != uiPanel)
                    {
                        uiPanel.depth = mDepth + 1;
                        Vector4 rect = uiPanel.finalClipRegion;
                        rect.z = PublicFunction.GetWidth() - PublicFunction.Back_Btn_Pos.x * 2 - 120 - uiPanel.clipSoftness.x * 2;
                        mShowSize = (int)(rect.z / (mServoSize.x ));
                        rect.x = rect.z / 2 + uiPanel.clipSoftness.x;
                        uiPanel.baseClipRegion = rect;
                    }
                }
            }
            InitChoicePartQueue();
            SetTips2Data();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.Equals("btnClose"))
        {
            OnClose();
        }
    }


    void InitChoicePartQueue()
    {
        if (null == mGridTrans)
        {
            return;
        }
        ServosConnection servosConnection = null;
        if (null != mRobot)
        {
            servosConnection = ServosConManager.GetInst().GetServosConnection(mRobot.ID);
        }
        List<byte> servoList = null;
        List<byte> motorList = null;
        Dictionary<TopologyPartType, List<byte>> sensorDict = null;
        TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
        if (null == servosConnection)
        {
            if (null == mRobot)
            {
                return;
            }
            servoList = mRobot.GetAllDjData().GetIDList();
            if (null != mRobot.MotherboardData)
            {
                for (int i = 0, imax = partType.Length; i < imax; ++i)
                {
                    SensorData sensorData = mRobot.MotherboardData.GetSensorData(partType[i]);
                    if (null != sensorData)
                    {
                        if (null == sensorDict)
                        {
                            sensorDict = new Dictionary<TopologyPartType, List<byte>>();
                        }
                        sensorDict[partType[i]] = sensorData.ids;
                    }
                }
            }
            if (null != mRobot.MotorsData)
            {
                motorList = mRobot.MotorsData.GetIds();
            }
        }
        else
        {
            List<TopologyPartData> topoData = servosConnection.GetTopologyData();
            if (null != topoData)
            {
                for (int i = 0, imax = topoData.Count; i < imax; ++i)
                {
                    if (topoData[i].partType == TopologyPartType.MainBoard || topoData[i].partType == TopologyPartType.MainBoard_new_low || topoData[i].partType == TopologyPartType.None)
                    {
                        continue;
                    }
                    if (topoData[i].partType == TopologyPartType.Servo)
                    {
                        if (null == servoList)
                        {
                            servoList = new List<byte>();
                        }
                        servoList.Add(topoData[i].id);
                    }
                    else if (topoData[i].partType == TopologyPartType.Motor)
                    {
                        if (null == motorList)
                        {
                            motorList = new List<byte>();
                        }
                        motorList.Add(topoData[i].id);
                    }
                    else if (topoData[i].partType != TopologyPartType.Gyro)
                    {
                        if (null == sensorDict)
                        {
                            sensorDict = new Dictionary<TopologyPartType, List<byte>>();
                        }
                        if (!sensorDict.ContainsKey(topoData[i].partType))
                        {
                            List<byte> list = new List<byte>();
                            sensorDict[topoData[i].partType] = list;
                        }
                        sensorDict[topoData[i].partType].Add(topoData[i].id);
                    }
                }
            }
        }
        if (null != servoList)
        {
            servoList.Sort();
            CreatePartQueue(servoList, TopologyPartType.Servo);
        }
        if (null != motorList)
        {
            motorList.Sort();
            CreatePartQueue(motorList, TopologyPartType.Motor);
        }
        if (null != sensorDict)
        {
            for (int i = 0, imax = partType.Length; i < imax; ++i)
            {
                if (sensorDict.ContainsKey(partType[i]))
                {
                    sensorDict[partType[i]].Sort();
                    CreatePartQueue(sensorDict[partType[i]], partType[i]);
                }
            }
        }
        UIScrollView scrollView = NGUITools.FindInParents<UIScrollView>(mGridTrans);
        ResetQueuePosition(mGridTrans, (int)mServoSize.x, 10, true, scrollView);
    }




    void CreatePartQueue(List<byte> partList, TopologyPartType partType)
    {
        if (null == mGridTrans)
        {
            return;
        }
        for (int i = 0, imax = partList.Count; i < imax; ++i)
        {
            GameObject obj = null;
            if (partType == TopologyPartType.Servo)
            {
                obj = CreateChoiceServo(partList[i], mGridTrans);
            }
            else
            {
                obj = CreateChoiceSensor(partList[i], partType, mGridTrans);
            }
        }
    }
    GameObject CreateChoiceServo(byte id, Transform parentTrans)
    {
        if (null != mServoItem)
        {
            GameObject obj = GameObject.Instantiate(mServoItem) as GameObject;
            obj.name = string.Format("servo_{0}", id);
            obj.transform.parent = parentTrans;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
            if (null != lb)
            {
                lb.text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
            }
            
            ServoModel servoModel = ServoModel.Servo_Model_Angle;
            if (null != mRobot)
            {
                DuoJiData servoData = mRobot.GetAllDjData().GetDjData(id);
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

    GameObject CreateChoiceSensor(byte id, TopologyPartType partType, Transform parentTrans)
    {
        if (null != mSensorItem)
        {
            GameObject obj = GameObject.Instantiate(mSensorItem) as GameObject;
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
            TopologyUI.SetSensorBg(obj, partType);
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
                    if (withinBounds)
                    {
                        obj.transform.localPosition = targetPos;
                    }
                    else
                    {
                        SpringPosition.Begin(obj, targetPos, 8);
                    }
                    
                    ++index;
                }
            }
            if (withinBounds && null != scrollView)
            {
                scrollView.RestrictWithinBounds(true);
            }
        }
    }

    void SetTips2Data()
    {
        Transform tips2 = mTrans.Find("tips2");
        Vector2 pos = Vector2.zero;
        if (null != mGridTrans && mGridTrans.childCount > 0)
        {
            if (mGridTrans.childCount >= 5)
            {
                pos = mGridTrans.GetChild(4).position;
            }
            else
            {
                pos = mGridTrans.GetChild(mGridTrans.childCount - 1).position;
            }
        } else
        {
            if (null != tips2)
            {
                tips2.gameObject.SetActive(false);
            }
        }
        
        if (null != tips2)
        {
            tips2.position = pos;
            Vector2 tipsSize = NGUIMath.CalculateRelativeWidgetBounds(tips2).size;
            tips2.localPosition += new Vector3(tipsSize.x / 2 - 110, mServoSize.y / 2 + tipsSize.y / 2);
            UILabel lb = GameHelper.FindChildComponent<UILabel>(tips2, "Label");
            if (null != lb)
            {
                lb.text = LauguageTool.GetIns().GetText("选择端口连接的舵机或传感器");
            }
        }
    }
    #endregion
}