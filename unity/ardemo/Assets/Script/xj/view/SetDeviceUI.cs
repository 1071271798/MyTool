using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Event;
using Game.Platform;
using Game.UI;
using Game.Scene;

/// <summary>
/// Author:xj
/// FileName:SetDeviceUI.cs
/// Description:
/// Time:2015/8/29 10:50:33
/// </summary>
public class SetDeviceUI : BaseUI
{
    #region 公有属性
    #endregion

    #region 其他属性
    GameObject mItem;
    byte mDeviceDjID;
    byte mOldId;
    UIInput mInput;
    UISprite mConnectIcon;
    UIButton mConnectBtn;
    bool mConnected;

    UIButton mSaveBtn;
    UIButton mConfirmBtn;
    UILabel deviceCurIdss;

    Color32 Normal_Color = new Color32(125, 56, 0, 255);

    TweenAlpha mTipsTweenAlpha;
    GameObject mSaveBtnObj;

    bool isOldConnected = false;

    public enum tipIndex
    {
        tip_first,
        tip_second,
        tip_third,
    }

    tipIndex tipCurIndex;
    Transform successDialog;
    Transform nextBtn, connectBtn;
    Transform img1, img2, img3;

    TopologyPartType mDevicePartType = TopologyPartType.Servo;
    #endregion

    #region 公有函数
    public SetDeviceUI()
    {
        mUIResPath = "Prefab/UI/SetDeviceID";
    }

    #endregion

    #region 其他函数

    protected override void AddEvent()
    {
        try
        {
            base.AddEvent();
            EventMgr.Inst.Regist(EventID.Set_Device_ID_ReadData_Result, ReadDeviceResult);
            EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
            EventMgr.Inst.Regist(EventID.Set_Device_ID_Msg_Ack, ChangeCallBack);
            EventMgr.Inst.Regist(EventID.Change_Sensor_ID_Msg_Ack, ChangeSensorCallBack);
            RobotManager.GetInst().IsSetDeviceIDFlag = true;

            if (null != mTrans)
            {
                Transform top = mTrans.Find("list/top");
                if (null != top)
                {
                    GameHelper.SetPosition(top, UIWidget.Pivot.Top, Vector2.zero);
                    GameHelper.SetPosition(top.Find("backBtn"), UIWidget.Pivot.Left, Vector2.zero + PublicFunction.Iphonex_Add_Offset);
                    AddScreenResizeEvent(top.Find("backBtn"), delegate () {
                        GameHelper.SetPosition(top.Find("backBtn"), UIWidget.Pivot.Left, Vector2.zero + PublicFunction.Iphonex_Add_Offset);
                    });
                }
                Transform title = mTrans.Find("list/top/title");
                if (null != title)
                {
                    //GameHelper.SetPosition(title, UIWidget.Pivot.Top, new Vector2(0, PublicFunction.Back_Btn_Pos.y));
                    UILabel lb = GameHelper.FindChildComponent<UILabel>(title, "Label");
                    if (null != lb)
                    {
                        lb.text = LauguageTool.GetIns().GetText("修改ID");
                    }
                }
                Transform titlebg = mTrans.Find("list/top/bg");
                if (null != titlebg)
                {
                    //GameHelper.SetPosition(titlebg, UIWidget.Pivot.Top, Vector2.zero);
                    UISprite bg = titlebg.GetComponent<UISprite>();
                    if (null != bg)
                    {
                        bg.width = PublicFunction.GetExtendWidth();
                        AddScreenResizeEvent(bg.transform, delegate ()
                        {
                            bg.width = PublicFunction.GetExtendWidth();
                        });
                    }
                }


                mItem = mTrans.Find("list/Item").gameObject;

                successDialog = mTrans.Find("successTips");
                if (null != successDialog)
                {
                    Transform diaTips = successDialog.Find("tips");
                    diaTips.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("修改ID成功提示");

                    mConfirmBtn = successDialog.Find("confirmBtn").GetComponent<UIButton>();

                    successDialog.gameObject.SetActive(false);
                }

                Transform saveBtn = mTrans.Find("list/saveBtn");
                if (null != saveBtn)
                {
                    mSaveBtn = saveBtn.GetComponent<UIButton>();
                    if (null != mSaveBtn)
                    {
                        saveBtn.localPosition = UIManager.GetWinPos(saveBtn, UIWidget.Pivot.Bottom, 0, PublicFunction.Bottom_Margin_x);
                        mSaveBtn.OnSleep();
                    }
                    
                    Transform label = saveBtn.Find("Label");
                    if (null != label)
                    {
                        UILabel lb = label.GetComponent<UILabel>();
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("确定");
                        }
                    }
                    mSaveBtnObj = saveBtn.gameObject;
                }
                nextBtn = mTrans.Find("list/nextBtn");
                if (null != nextBtn)
                {
                    nextBtn.gameObject.SetActive(true);
                    if (null != nextBtn.GetComponent<UIButton>())
                    {
                        nextBtn.localPosition = UIManager.GetWinPos(nextBtn, UIWidget.Pivot.Bottom, 0, PublicFunction.Bottom_Margin_x);
                        //mSaveBtn.OnSleep();
                    }

                    Transform label = nextBtn.Find("Label");
                    if (null != label)
                    {
                        UILabel lb = label.GetComponent<UILabel>();
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("下一步");
                        }
                    }
                }
                connectBtn = mTrans.Find("list/connectBtn");
                if (null != connectBtn)
                {
                    if (null != connectBtn.GetComponent<UIButton>())
                    {
                        connectBtn.localPosition = UIManager.GetWinPos(connectBtn, UIWidget.Pivot.Bottom, 0, PublicFunction.Bottom_Margin_x);
                        //mSaveBtn.OnSleep();
                    }

                    Transform label = connectBtn.Find("Label");
                    if (null != label)
                    {
                        UILabel lb = label.GetComponent<UILabel>();
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("连接");
                        }
                    }
                    connectBtn.gameObject.SetActive(false);
                }

                Transform tips = mTrans.Find("list/tips");
                if (null != tips)
                {
                    mTipsTweenAlpha = tips.GetComponent<TweenAlpha>();
                    Transform Label = tips.Find("Label");
                    if (null != Label)
                    {
                        int width = 0;
                        UILabel lb = Label.GetComponent<UILabel>();
                        if (null != lb)
                        {
                            lb.width = PublicFunction.GetWidth() - 128;
                            width = lb.width;
                            lb.text = LauguageTool.GetIns().GetText("修改舵机ID提示语");
                        }
                        Label.localPosition = new Vector3(-width / 2, (0.5f - 0.25f) * PublicFunction.GetHeight());
                    }

                    Transform img = tips.Find("img");
                    if (null != img)
                    {
                        tipCurIndex = tipIndex.tip_first;
                        img.localPosition = new Vector3(0, (0.5f - 0.6f) * PublicFunction.GetHeight());
                        img1 = img.Find("img1");
                        if (null != img1)
                        {
                            //img1.localPosition = new Vector3(UIManager.GetWinPos(img1, UIWidget.Pivot.Left, 160).x, 0);
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(img1, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("1.请先找到重复ID的舵机");
                            }
                        }
                        img2 = img.Find("img2");
                        if (null != img2)
                        {
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(img2, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("2.保留一个舵机连接主板");
                            }
                        }
                        img3 = img.Find("img3");
                        if (null != img3)
                        {
                            //img3.localPosition = new Vector3(UIManager.GetWinPos(img3, UIWidget.Pivot.Right, 160).x, 0);
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(img3, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("3.手机连接蓝牙修改ID");
                            }
                        }
                        img1.gameObject.SetActive(true);
                        img2.gameObject.SetActive(false);
                        img3.gameObject.SetActive(false);
                    }
                }               
            }

            if (PlatformMgr.Instance.GetBluetoothState())
                PlatformMgr.Instance.DisConnenctBuletooth();

            mSaveBtnObj.SetActive(false);

            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null != robot && robot.GetDjNum() == 1 && PlatformMgr.Instance.GetBluetoothState())
            {
                NetWork.GetInst().ClearAllMsg();
                Robot setRobot = RobotManager.GetInst().GetSetDeviceRobot();
                if (null != setRobot)
                {
                    string mac = robot.Mac;
                    setRobot.ConnectRobotResult(mac, true);
                    setRobot.HandShake();
                    NetWaitMsg.ShowWait();
                    //2秒以后读取初始角度
                    Timer.Add(2, 0, 1, robot.ReadMotherboardData);
                    isOldConnected = true;
                }
                if (null != mTipsTweenAlpha)
                {
                    mTipsTweenAlpha.gameObject.SetActive(false);
                }
            }
            else
            {
                PlatformMgr.Instance.DisConnenctBuletooth();
                mSaveBtnObj.SetActive(false);
            }
            SetConnectState();
            /*ReadMotherboardDataMsgAck msg = new ReadMotherboardDataMsgAck(1);
            msg.motors = new List<byte>();
            msg.motors.Add(2);
            EventMgr.Inst.Fire(EventID.Set_Device_ID_ReadData_Result, new EventArg(msg));*/
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }       
    }

    void InitItem()
    {
        if (null != mItem)
        {
            mItem.SetActive(true);
            SetItem(mDeviceDjID, mItem);
        }
    }

    void SetItem(int id, GameObject obj)
    {
        Transform input = obj.transform.Find("Input");
        if (null != input)
        {
            mInput = input.GetComponent<UIInput>();
            if (null != mInput)
            {
                mInput.value = id.ToString();
                //SetInputColor(tmpInput, Normal_Color);
                if (null == mInput.onSelect)
                {
                    mInput.onSelect = OnInputSelect;
                    mInput.onValidate = OnValidate;
                }
#if UNITY_IPHONE
                mInput.isSelected = true;
#endif

            }
        }
        //UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
        //if (null != lb)
        //{
        //    lb.text = LauguageTool.GetIns().GetText("设备ID:");
        //}
        if (mDevicePartType == TopologyPartType.Servo)
        {
            deviceCurIdss = GameHelper.FindChildComponent<UILabel>(obj.transform, "duoji/id");
            if (null != deviceCurIdss)
            {
                deviceCurIdss.text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
            }
        }
        else
        {
            Transform duoji = obj.transform.Find("duoji");
            if (null != duoji)
            {
                duoji.gameObject.SetActive(false);
            }
            Transform sensor = obj.transform.Find("sensor");
            if (null != sensor)
            {
                sensor.gameObject.SetActive(true);
                deviceCurIdss = GameHelper.FindChildComponent<UILabel>(sensor, "Label");
                if (null != deviceCurIdss)
                {
                    deviceCurIdss.text = PublicFunction.ID_Format + id.ToString().PadLeft(2, '0');
                }
                TopologyUI.SetSensorBg(sensor.gameObject, mDevicePartType);
            }
        }
        
    }

    void SetInputColor(UIInput input, Color color)
    {
        input.activeTextColor = color;
        UILabel lb = GameHelper.FindChildComponent<UILabel>(input.transform, "Label");
        if (null != lb)
        {
            lb.color = color;
        }
    }
    protected override void Close()
    {
        base.Close();
        EventMgr.Inst.UnRegist(EventID.Set_Device_ID_ReadData_Result, ReadDeviceResult);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
        EventMgr.Inst.UnRegist(EventID.Set_Device_ID_Msg_Ack, ChangeCallBack);
        EventMgr.Inst.UnRegist(EventID.Change_Sensor_ID_Msg_Ack, ChangeSensorCallBack);
        RobotManager.GetInst().IsSetDeviceIDFlag = false;
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            PlatformMgr.Instance.DisConnenctBuletooth();
        }
        /*Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot && robot.Connected)
        {
            robot.ConnectRobotResult(robot.Mac, true);
        }*/
        /*if (!mOldConnected)
        {
            EventMgr.Inst.Fire(EventID.Change_Device_ID);
        }*/
    }

    IEnumerator DelayShowSuccessTips(float t)
    {
        yield return new WaitForSeconds(t);
        successDialog.gameObject.SetActive(true);
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.Equals("saveBtn"))
        {
            //bool checkResults = CheckID();
            //Debug.Log("now check Result is " + checkResults);
            if (CheckID())
            {//可以发包了
                if (mConnected && mOldId!= 0 && mDeviceDjID > 0)
                {
                    //mDeviceDjIDList.Sort();
                    Robot robot = RobotManager.GetInst().GetSetDeviceRobot();
                    if (null != robot)
                    {
                        NetWaitMsg.ShowWait(10);
                        if (mDevicePartType == TopologyPartType.Servo)
                        {
                            robot.ChangeDeviceId(mOldId, mDeviceDjID);
                        }
                        else
                        {
                            robot.ChangeSensorID(mDevicePartType, mOldId, mDeviceDjID);
                        }
                    }
                }
            }
        }
        if (obj.name.Equals("confirmBtn"))
        {
            if (null != successDialog)
                successDialog.gameObject.SetActive(false);
        }
        else if (obj.name.Equals("nextBtn"))
        {
            if (tipCurIndex == tipIndex.tip_first)
            {
                img1.gameObject.SetActive(false);
                img2.gameObject.SetActive(true);
                img3.gameObject.SetActive(false);
                tipCurIndex = tipIndex.tip_second;
            }
            else if (tipCurIndex == tipIndex.tip_second)
            {
                img1.gameObject.SetActive(false);
                img2.gameObject.SetActive(false);
                img3.gameObject.SetActive(true);
                tipCurIndex = tipIndex.tip_third;
                nextBtn.gameObject.SetActive(false);
                connectBtn.gameObject.SetActive(true);
            }
        }
        else if (obj.name.Equals("backBtn"))
        {
            if (tipCurIndex == tipIndex.tip_first)
            {
                OnClose();
                SceneMgr.EnterScene(SceneType.EmptyScene);
                SingletonBehaviour<ClientMain>.GetInst().WaitFrameInvoke(BackApp);
            }
            else if (tipCurIndex == tipIndex.tip_second)
            {
                img1.gameObject.SetActive(true);
                img2.gameObject.SetActive(false);
                img3.gameObject.SetActive(false);
                tipCurIndex = tipIndex.tip_first;
            }
            else if (tipCurIndex == tipIndex.tip_third)
            {
                img1.gameObject.SetActive(false);
                img2.gameObject.SetActive(true);
                img3.gameObject.SetActive(false);
                tipCurIndex = tipIndex.tip_second;
                nextBtn.gameObject.SetActive(true);
                connectBtn.gameObject.SetActive(false);
            }
        }
        else if (obj.name.Equals("connectBtn"))
        {
            tipCurIndex = tipIndex.tip_first;
            /*ReadMotherboardDataMsgAck msg = new ReadMotherboardDataMsgAck();
            msg.ids.Add(1);
            ReadDeviceResult(new EventArg(msg));
            connectBtn.gameObject.SetActive(false);
            return;*/
            //ReadDeviceResult(null);
            
            if (mConnected)
            {
                PlatformMgr.Instance.DisConnenctBuletooth();
            }
            else
            {
                SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetSetDeviceRobot());
            }
        }
    }

    void BackApp()
    {
        PlatformMgr.Instance.IsInUnity = false;
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ExitSetupSteeringEngineID, string.Empty);
    }

    void OnInputSelect(bool isSelect, GameObject obj)
    {
        try
        {
            if (!isSelect)
            {
                if (!PublicFunction.IsInteger(mInput.value))
                {
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("DuoJiIDZhengShu"), 1, CommonTipsColor.red);
                    mInput.value = mDeviceDjID.ToString();
                    return;
                }
                if (mInput.value.StartsWith("0"))
                {
                    mInput.value = mInput.value.TrimStart('0');
                }
                int newId = 0;
                if (!string.IsNullOrEmpty(mInput.value))
                {
                    newId = int.Parse(mInput.value);
                }
                if (mDevicePartType == TopologyPartType.Servo)
                {
                    if (newId < PublicFunction.DuoJi_Id_Min || newId > PublicFunction.DuoJi_Id_Max)
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(string.Format(LogicLanguage.GetText("DuoJiIDFanWei"), PublicFunction.DuoJi_Id_Min, PublicFunction.DuoJi_Id_Max), 1, CommonTipsColor.red);
                        mInput.value = mDeviceDjID.ToString();
                        return;
                    }
                }
                else if (mDevicePartType == TopologyPartType.Gyro)
                {
                    if (newId < 1 || newId > 2)
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(string.Format(LogicLanguage.GetText("DuoJiIDFanWei"), 1, 2), 1, CommonTipsColor.red);
                        mInput.value = mDeviceDjID.ToString();
                        return;
                    }
                }
                else if (mDevicePartType == TopologyPartType.Speaker)
                {
                    // 蓝牙音箱不可修改ID
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("禁止修改ID提示"), 1, CommonTipsColor.red);
                    return;
                }
                else
                {
                    if (newId < PublicFunction.Sensor_ID_Min || newId > PublicFunction.Sensor_ID_Max)
                    {
                        SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(string.Format(LogicLanguage.GetText("DuoJiIDFanWei"), PublicFunction.Sensor_ID_Min, PublicFunction.Sensor_ID_Max), 1, CommonTipsColor.red);
                        mInput.value = mDeviceDjID.ToString();
                        return;
                    }
                }

                mDeviceDjID = (byte)newId;
                CheckBtnState();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    byte tmpLenght = 0;
    char OnValidate(string text, int charIndex, char addedChar)
    {
        if (charIndex == 0)
        {
            tmpLenght = 0;
        }
        if (mDevicePartType == TopologyPartType.Servo)
        {
            if (tmpLenght >= 2)
            {//限制长度
                return (char)0;
            }
        }
        else
        {
            if (tmpLenght >= 1)
            {//限制长度
                return (char)0;
            }
        }
        
        if (addedChar >= '0' && addedChar <= '9')
        {
            ++tmpLenght;
        }
        else
        {
            return (char)0;
        }
        return addedChar;
    }

    void SetConnectState()
    {
        string iconName;
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            iconName = "connect";
        }
        else
        {
            iconName = "disconnect";
        }
        /*if (null != mConnectBtn)
        {
            mConnectBtn.normalSprite = iconName;
        }*/
        if (null != mConnectIcon)
        {
            mConnectIcon.spriteName = iconName;
            mConnectIcon.MyMakePixelPerfect();
        }
    }

    bool CheckID()
    {
        do 
        {
            if (mDevicePartType == TopologyPartType.Servo)
            {
                if (mDeviceDjID < PublicFunction.DuoJi_Id_Min || mDeviceDjID > PublicFunction.DuoJi_Id_Max)
                {
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(string.Format(LogicLanguage.GetText("DuoJiIDFanWei"), PublicFunction.DuoJi_Id_Min, PublicFunction.DuoJi_Id_Max), 1, CommonTipsColor.red);
                    return false;
                }
            }
            else if (mDevicePartType == TopologyPartType.Gyro)
            {
                if ((mDeviceDjID < 1) || (mDeviceDjID > 2))
                {
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(string.Format(LogicLanguage.GetText("DuoJiIDFanWei"), 1, 2), 1, CommonTipsColor.red);
                    return false;
                }
            }
            else if (mDevicePartType == TopologyPartType.Speaker)
            {
                // 蓝牙音箱不可修改ID
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("禁止修改ID提示"), 1, CommonTipsColor.red);
                return false;
            }
            else
            {
                if ((mDeviceDjID < PublicFunction.Sensor_ID_Min) || (mDeviceDjID > PublicFunction.Sensor_ID_Max))
                {
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(string.Format(LogicLanguage.GetText("DuoJiIDFanWei"), PublicFunction.Sensor_ID_Min, PublicFunction.Sensor_ID_Max), 1, CommonTipsColor.red);
                    return false;
                }
            }
        } while (false);
        return true;
    }

    void CleanUp()
    {
        
        mDeviceDjID = 0;
        mInput = null;
        mOldId = 0;
        if (null != mItem)
        {
            mItem.SetActive(false);
        }
        isOldConnected = false;
    }

    void ReadDeviceResult(EventArg arg)
    {
        try
        {
            mConnected = true;
            SetConnectState();
            ReadMotherboardDataMsgAck msg = (ReadMotherboardDataMsgAck)arg[0];
            if (null != msg)
            {
                CleanUp();
                if (msg.ids.Count == 1)
                {
                    mDeviceDjID = msg.ids[0];
                    mOldId = msg.ids[0];
                    mDevicePartType = TopologyPartType.Servo;
                } else if (null != msg.motors && msg.motors.Count > 0)
                {
                    mDeviceDjID = msg.motors[0];
                    mOldId = msg.motors[0];
                    mDevicePartType = TopologyPartType.Motor;
                }
                else
                {
                    TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
                    for (int i = 0, imax = partType.Length; i < imax; ++i)
                    {
                        SensorData data = msg.GetSensorData(partType[i]);
                        if (null != data && data.ids.Count == 1)
                        {
                            mDeviceDjID = data.ids[0];
                            mOldId = data.ids[0];
                            mDevicePartType = partType[i];
                            break;
                        }
                    }
                }
                /*if (PlatformMgr.Instance.GetBluetoothState())
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("连接成功修改舵机提示"), 1, CommonTipsColor.green);*/
                InitItem();
                if (null != mSaveBtnObj)
                {
                    mSaveBtnObj.SetActive(true);
                    CheckBtnState();
                }
                GameHelper.PlayTweenAlpha(mTipsTweenAlpha, 0);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void OnConnenctResult(EventArg arg)
    {
        try
        {
            bool result = (bool)arg[0];
            if (!result)
            {
                mConnected = false;
                CleanUp();
            }
            else
            {
                connectBtn.gameObject.SetActive(false);
            }
            SetConnectState();
            isOldConnected = false;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void DelayShowTips()
    {
        NetWaitMsg.CloseWait();
        if (null != mInput)
        {
            if (mDevicePartType == TopologyPartType.Servo)
            {
                UILabel lb = GameHelper.FindChildComponent<UILabel>(mInput.transform.parent, "duoji/id");
                if (null != lb)
                {
                    lb.text = PublicFunction.ID_Format + mInput.value.PadLeft(2, '0');
                }
            }
            else
            {
                UILabel lb = GameHelper.FindChildComponent<UILabel>(mInput.transform.parent, "sensor/Label");
                if (null != lb)
                {
                    lb.text = PublicFunction.ID_Format + mInput.value.PadLeft(2, '0');
                }
            }
            
        }
        if (mDevicePartType == TopologyPartType.Servo)
        {
            //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机ID修改成功，请及时更换舵机的ID标签！"));
            //curModifyId = servoIdInput.value;
            if (successDialog != null)
                successDialog.gameObject.SetActive(true);
        }
        else
        {
            //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("ID修改成功，请及时更换设备的ID标签！"));
            //curModifyId = servoIdInput.value;
            if (successDialog != null)
                successDialog.gameObject.SetActive(true);
        }
    }
    void ChangeCallBack(EventArg arg)
    {
        /*OnClose();
        EventMgr.Inst.Fire(EventID.Close_Set_Device_ID_UI);*/
        try
        {
            CommonMsgAck msg = (CommonMsgAck)arg[0];
            if (msg.result == (byte)ErrorCode.Result_OK)
            {
                mOldId = mDeviceDjID;
                Timer.Add(2, 1, 1, DelayShowTips);
            }
            else
            {
                mDeviceDjID = mOldId;
                if (null != mInput)
                {
                    mInput.value = mOldId.ToString();
                }
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("修改失败"), 1, CommonTipsColor.red);
                if (msg.result == 255)
                {
                } else
                {
                }
            }
            CheckBtnState();
        }
        catch (System.Exception ex)
        {
            NetWaitMsg.CloseWait();
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void ChangeSensorCallBack(EventArg arg)
    {
        try
        {
            ChangeSensorIDMsgAck msg = (ChangeSensorIDMsgAck)arg[0];
            if (msg.result == (byte)ErrorCode.Result_OK)
            {
                mOldId = mDeviceDjID;
                Timer.Add(2, 1, 1, DelayShowTips);
            }
            else
            {
                mDeviceDjID = mOldId;
                if (null != mInput)
                {
                    mInput.value = mOldId.ToString();
                }
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("修改失败"), 1, CommonTipsColor.red);
                if (msg.result == 255)
                {
                }
                else
                {
                }
            }
            CheckBtnState();
        }
        catch (System.Exception ex)
        {
            NetWaitMsg.CloseWait();
        }
    }

    void CheckBtnState()
    {
        bool changeFlag = false;
        if (mDeviceDjID != mOldId)
        {
            changeFlag = true;
        }
        if (null != mSaveBtn)
        {
            if (changeFlag)
            {
                mSaveBtn.OnAwake();
            }
            else
            {
                mSaveBtn.OnSleep();
            }
        }
    }
    #endregion
}