using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Game;
using Game.Event;
using Game.Scene;
using Game.Platform;
using Game.UI;
/// <summary>
/// 竖杆配置
/// </summary>
public class HsliderSettingUI : BasePopWin
{
    bool isChange = false;
    public static HSliderWidgetData hsliderData;  //滑杆数据
    private UISprite selectServo;

    Transform directionTrans;
    GameObject adjustObj;
    UILabel angleLabel;
    CircleScrollBar slider;
    MyTweenRotation tweenRota;

    UISprite rangeIcon;
    UISprite minHead;
    UISprite maxHead;

    Robot mRobot; 

    int angle = 120;
    float mLastSendDjAngle = 0;


    public static void ShowMsg(string widgetID)
    {
        object[] args = new object[1];
        args[0] = widgetID;
        PopWinManager.GetInst().ShowPopWin(typeof(HsliderSettingUI), args);
    }

    public HsliderSettingUI(string widgetID)
    {
        mUIResPath = "Prefabs/HsliderSetting";
        hsliderData = new HSliderWidgetData();
        mRobot = RobotManager.GetInst().GetCurrentRobot();
        CopyData(hsliderData, (HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(widgetID));
        hsliderData.servoID = (byte)UserdefControllerUI.curHsliderServoNum;
        isSingle = true;
    }


    Vector3 calminHeadPos(int angle, float radius)
    {
        float posX = 0;
        float posY = 0;

        if (angle < -90)
        {
            posX = (Mathf.Cos((angle * (-1) - 90 + 5) * Mathf.Deg2Rad)) * radius * (-1);
            posY = (Mathf.Sin((angle * (-1) - 90 + 6) * Mathf.Deg2Rad)) * radius * (-1);
        }
        else if (angle >= -90 && angle < 0)
        {
            posX = (Mathf.Cos((angle + 90 - 7) * Mathf.Deg2Rad)) * radius * (-1);
            posY = (Mathf.Sin((angle + 90 - 6) * Mathf.Deg2Rad)) * radius;
        }
        else if (angle >= 0 && angle < 90)
        {
            posX = (Mathf.Cos((90 - angle + 7) * Mathf.Deg2Rad)) * radius;
            posY = (Mathf.Sin((90 - angle + 6) * Mathf.Deg2Rad)) * radius;
        }
        else if (angle >= 90)
        {
            posX = (Mathf.Cos((angle - 90 - 7) * Mathf.Deg2Rad)) * radius;
            posY = (Mathf.Sin((angle - 90 - 6) * Mathf.Deg2Rad)) * radius * (-1);
        }

        return new Vector3(posX, posY, 0.0f);
    }

    Vector3 calmaxHeadPos(int angle, float radius)
    {
        float posX = 0;
        float posY = 0;

        if (angle < -90)
        {
            posX = (Mathf.Cos((angle * (-1) - 90 - 7) * Mathf.Deg2Rad)) * radius * (-1);
            posY = (Mathf.Sin((angle * (-1) - 90 - 6) * Mathf.Deg2Rad)) * radius * (-1);
        }
        else if (angle >= -90 && angle < 0)
        {
            posX = (Mathf.Cos((angle + 90 + 7) * Mathf.Deg2Rad)) * radius * (-1);
            posY = (Mathf.Sin((angle + 90 + 6) * Mathf.Deg2Rad)) * radius;
        }
        else if (angle >= 0 && angle < 90)
        {
            posX = (Mathf.Cos((90 - angle - 7) * Mathf.Deg2Rad)) * radius;
            posY = (Mathf.Sin((90 - angle - 6) * Mathf.Deg2Rad)) * radius;
        }
        else if (angle >= 90)
        {
            posX = (Mathf.Cos((angle - 90 + 5) * Mathf.Deg2Rad)) * radius;
            posY = (Mathf.Sin((angle - 90 + 6) * Mathf.Deg2Rad)) * radius * (-1);
        }

        return new Vector3(posX, posY, 0.0f);
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        selectServo = null;
        Transform trans = mTrans.Find("Cancel").transform;  //cancelBTN
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);

        Vector3 pos = trans.position;
        trans = mTrans.Find("mainLabel").transform;  //label
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Top,0, PublicFunction.Title_Margin);
        trans.GetComponent<UILabel>().text = string.Format(LauguageTool.GetIns().GetText("设置横杆主标题"),hsliderData.servoID);

        Transform trans2 = mTrans.Find("subLabel").transform;  //label
        trans2.localPosition = new Vector3(0, trans.localPosition.y - 40, 0);
        trans2.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("设置横杆副标题");
        //trans = GameObject.Find("HsliderSetting/Confirm").transform; // confirmBTN
        //trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopRight, UserdefControllerScene.rightSpace, UserdefControllerScene.upSpace);

        trans = mTrans.Find("Confirm").transform;
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Bottom, 0, 40);
        //trans.GetComponent<UISprite>().width = PublicFunction.GetWidth();
        trans.GetChild(0).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("确定");

        trans = mTrans.Find("sliderShow").transform;
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Center, 0, 0);
        //trans.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("上推竖杆");

        adjustObj = mTrans.Find("sliderShow/duoji/adjust").gameObject;
        angleLabel = mTrans.Find("sliderShow/duoji/adjust/angle").transform.GetComponent<UILabel>();
        slider = mTrans.Find("sliderShow/duoji/adjust/angleSlider").transform.GetComponent<CircleScrollBar>();
        directionTrans = mTrans.Find("sliderShow/duoji/direction").transform;
        rangeIcon = mTrans.Find("sliderShow/duoji/adjust/angleSlider/range").transform.GetComponent<UISprite>();
        minHead = mTrans.Find("sliderShow/duoji/adjust/angleSlider/minhead").transform.GetComponent<UISprite>();
        maxHead = mTrans.Find("sliderShow/duoji/adjust/angleSlider/maxhead").transform.GetComponent<UISprite>();

        UILabel minTips = mTrans.Find("sliderShow/duoji/adjust/minTip").transform.GetComponent<UILabel>();
        minTips.text = LauguageTool.GetIns().GetText("设为最小值");

        UILabel maxTips = mTrans.Find("sliderShow/duoji/adjust/maxTip").transform.GetComponent<UILabel>();
        maxTips.text = LauguageTool.GetIns().GetText("设为最大值");

        directionTrans.GetChild(0).GetComponent<UILabel>().text = PublicFunction.ID_Format + hsliderData.servoID.ToString().PadLeft(2, '0');

        if (null != slider)
        {
            slider.onDragChange = OnDragChange;
        }
        /*if (null != angleLabel)
        {
            angleLabel.text = "0°";
        }*/

        if (hsliderData.servoID != 0) //已配置过的
        {
            rangeIcon.enabled = true;
            minHead.enabled = true;
            maxHead.enabled = true;

            rangeIcon.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (-180 + 118 - hsliderData.max_angle)*1.0f);

            // 描绘角度范围
            rangeIcon.fillAmount = (hsliderData.max_angle - hsliderData.min_angle + 68) / 360.0f;

            float hsliderR = (rangeIcon.width - minHead.width) / 2.0f;     

            minHead.transform.localPosition = calminHeadPos(hsliderData.min_angle, hsliderR);
            maxHead.transform.localPosition = calmaxHeadPos(hsliderData.max_angle, hsliderR);

            // 显示舵机角度范围
            angleLabel.text = getAngleRangeText(hsliderData.min_angle, hsliderData.max_angle);
        }
        else
        {
            rangeIcon.enabled = true;
            minHead.enabled = true;
            maxHead.enabled = true;

            hsliderData.min_angle = -118;
            hsliderData.max_angle = 118;

            angleLabel.text = getAngleRangeText(-118, 118);
        }

        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Init hslider control setting UI!!");
    }

    string getAngleRangeText(int minAngle, int maxAngle)
    {
        return "[596f80]" + minAngle.ToString() + "° [93a7b6]" + LauguageTool.GetIns().GetText("到") + "[-] " + maxAngle.ToString() + "°[-]";
    }

    string getAngleText(int angle)
    {
        return "[596f80]" + angle  +  "°[-]";
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.Contains("Confirm"))
        {
            mTrans.gameObject.SetActive(false);
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Confirm save hslider control data!!");
            
            CopyData((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(hsliderData.widgetId), hsliderData);  //确定修改， 
            UserdefControllerUI.isTotalDataChange = true;
            
            UserdefControllerScene.Ins.CloseHsliderSettingUI();
            UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.hslider_sw);
            OnClose();
        }
        else if (obj.name.Contains("Cancel"))
        {
            mTrans.gameObject.SetActive(false);
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Cancel current hslider control data!!");

            UserdefControllerScene.Ins.CloseHsliderSettingUI();
            UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.hslider_cw);
            OnClose();
        }
        else if (obj.name.Equals("minBtn"))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Set min angle!!");

            SetMinDuoJiAngle(obj.transform.parent.parent);
        }
        else if (obj.name.Equals("maxBtn"))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Set max angle!!");

            SetMaxDuoJiAngle(obj.transform.parent.parent);
        }
    }
    /// <summary>
    /// 设置舵机角度
    /// </summary>
    /// <param name="rota">改变的角度</param>
    /// <param name="dj"></param>
    void SetMinDuoJiAngle(Transform dj)
    {
        if (null != dj)
        {
            if (hsliderData.max_angle <= (angle - 120))
            {
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("无法设为最小值"), 1, CommonTipsColor.red);
            }
            else
            {
                isChange = true;
                hsliderData.min_angle = angle - 120;
                //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("配置成功提示"), HUDTextTips.Color_Green);

                // 显示最小角度范围
                rangeIcon.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (-180 + 118 - hsliderData.max_angle) * 1.0f);

                rangeIcon.enabled = true;
                minHead.enabled = true;
                maxHead.enabled = true;

                rangeIcon.fillAmount = (hsliderData.max_angle - hsliderData.min_angle + 68) / 360.0f;

                float hsliderR = (rangeIcon.width - minHead.width) / 2.0f;

                minHead.transform.localPosition = calminHeadPos(hsliderData.min_angle, hsliderR);
                maxHead.transform.localPosition = calmaxHeadPos(hsliderData.max_angle, hsliderR);

                // 显示舵机角度范围
                angleLabel.text = getAngleRangeText(hsliderData.min_angle, hsliderData.max_angle); 
            }
        }
    }
    void SetMaxDuoJiAngle(Transform dj)
    {
        if (null != dj)
        {
            if (hsliderData.min_angle >= (angle - 120))
            {
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("无法设为最大值"), 1, CommonTipsColor.red);
            }
            else
            {
                isChange = true;
                hsliderData.max_angle = angle - 120;
                //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("配置成功提示"), HUDTextTips.Color_Green);

                rangeIcon.enabled = true;
                minHead.enabled = true;
                maxHead.enabled = true;

                // 显示最大角度范围
                rangeIcon.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (-180 + 118 - hsliderData.max_angle) * 1.0f);

                rangeIcon.fillAmount = (hsliderData.max_angle - hsliderData.min_angle + 68) / 360.0f;

                float hsliderR = (rangeIcon.width - maxHead.width) / 2.0f;

                minHead.transform.localPosition = calminHeadPos(hsliderData.min_angle, hsliderR);
                maxHead.transform.localPosition = calmaxHeadPos(hsliderData.max_angle, hsliderR);

                // 显示舵机角度范围
                angleLabel.text = getAngleRangeText(hsliderData.min_angle, hsliderData.max_angle);
            }
        }
    }
    /// <summary>
    /// 可设置超出正常范围的角度
    /// </summary>
    /// <param name="angle"></param>
    public void SetErrorAngle(int angle)
    {
        if (angle > PublicFunction.DuoJi_Max_Rota)
        {
            angle = PublicFunction.DuoJi_Max_Rota;
        }
        else if (angle < PublicFunction.DuoJi_Min_Rota)
        {
            angle = PublicFunction.DuoJi_Min_Rota - 1;
        }
        this.angle = angle;

        if (null != slider)
        {
            if (angle <= PublicFunction.DuoJi_Min_Show_Rota)
            {
                slider.value = 1;
            }
            else if (angle >= PublicFunction.DuoJi_Max_Show_Rota)
            {
                slider.value = 0;
            }
            else
            {
                slider.value = 1 - angle / slider.AngleRange;
            }

        }
        SetAngleShow();
    }
    /// <summary>
    /// 拖动角度
    /// </summary>
    void OnDragChange(GameObject obj, bool finished)
    {
        angle = (int)((1 - slider.value) * slider.AngleRange);
        if (angle < PublicFunction.DuoJi_Min_Show_Rota)
        {
            angle = PublicFunction.DuoJi_Min_Show_Rota;
        }
        else if (angle > PublicFunction.DuoJi_Max_Show_Rota)
        {
            angle = PublicFunction.DuoJi_Max_Show_Rota;
        }
        SetAngleShow();

        SendCurAngle(hsliderData.servoID, angle, finished);
    }
    /// <summary>
    /// 角度计算
    /// </summary>
    public void SetAngle(int angle, bool instant = true, float time = 0)
    {
        if (angle < PublicFunction.DuoJi_Min_Show_Rota)
        {
            angle = PublicFunction.DuoJi_Min_Show_Rota;
        }
        else if (angle > PublicFunction.DuoJi_Max_Show_Rota)
        {
            angle = PublicFunction.DuoJi_Max_Show_Rota;
        }
        this.angle = angle;
        if (null != slider)
        {
            if (angle <= PublicFunction.DuoJi_Min_Show_Rota)
            {
                slider.value = 1;
            }
            else if (angle >= PublicFunction.DuoJi_Max_Show_Rota)
            {
                slider.value = 0;
            }
            else
            {
                slider.value = 1 - angle / slider.AngleRange;
            }


        }
        SetAngleShow(instant, time);
    }
    /// <summary>
    /// 角度显示
    /// </summary>
    void SetAngleShow(bool instant = true, float time = 0)
    {
        if (null != angleLabel)
        {
            if (angle < 120)
            {
                angleLabel.text = getAngleText(angle - 120);
            }
            else
            {
                angleLabel.text = getAngleText(angle - 120);
            }
        }
        /*Vector3 to = new Vector3(0, 0, 120 - angle);
        if (instant)
        {
            if (null != directionTrans)
            {
                directionTrans.localEulerAngles = to;
            }
        }
        else
        {
            if (null != tweenRota)
            {
                tweenRota.from = tweenRota.transform.localEulerAngles;
                tweenRota.to = to;
                tweenRota.delay = 0;
                tweenRota.duration = time;
                tweenRota.Play();
            }
        }*/
    }
    /// <summary>
    /// 向机器人发送角度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    void SendCurAngle(int id, int rota, bool finished)
    {
        if (null == mRobot)
        {
            return;
        }
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            if (finished)
            {
                mLastSendDjAngle = 0;
                mRobot.CtrlActionForDjId(id, rota);
            }
            else if (Time.time - mLastSendDjAngle >= 0.1f)
            {
                mRobot.CtrlActionForDjId(id, rota);
                mLastSendDjAngle = Time.time;
            }
        }
        else
        {
            //PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Connect robot is not exist!!");
        }
    }
    /// <summary>
    /// 取消保存
    /// </summary>
    /*void DoCancel(GameObject obj)
    {
        try
        {
            if (obj == null)
            {
                UserdefControllerScene.Ins.CloseHsliderSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
                return;
            }
            string name = obj.name;
            if (name.Equals(PromptMsg.LeftBtnName))
            {
                UserdefControllerScene.Ins.CloseHsliderSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
            }

                if (isChange)
                {
                    CopyData((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(hsliderData.widgetId), hsliderData);  //确定修改， 
                    UserdefControllerUI.isTotalDataChange = true;
                }
                UserdefControllerScene.Ins.CloseHsliderSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
            }
        }
        catch (System.Exception ex)
        { }
    }*/
    /// <summary>
    /// 确定保存
    /// </summary>
    void DoOK()
    {
        CopyData((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(hsliderData.widgetId), hsliderData);
    }
    /// <summary>
    /// 选对应的舵机
    /// </summary>
    /// <param name="obj"></param>
    void DoSelect(GameObject obj)
    {

    }
    /// <summary>
    /// 复制数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="copyData"></param>
    void CopyData(HSliderWidgetData data, HSliderWidgetData copyData)
    {
        if (data != null && copyData != null)
        {
            data.widgetId = copyData.widgetId;
            data.pos_x = copyData.pos_x;
            data.pos_y = copyData.pos_y;
            data.servoID = copyData.servoID;
            data.sType = copyData.sType;
            data.min_angle = copyData.min_angle;
            data.max_angle = copyData.max_angle;
            //data.directionDisclock = copyData.directionDisclock;
        }
    }
}
