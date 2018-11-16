using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:PromptDevice.cs
/// Description:
/// Time:2017/2/14 10:37:55
/// </summary>
public class PromptDevice : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    TopologyPartType mDeviceType;
    #endregion

    #region 公有函数
    
    public PromptDevice(TopologyPartType deviceType)
    {
        mUIResPath = "Prefab/UI/PromptDevice";
        isSingle = true;
        mDeviceType = deviceType;
    }

    public override void Init()
    {
        base.Init();
        mAddBox = true;
    }

    public static void ShowMainboardInfo(ReadMotherboardDataMsgAck boardData)
    {
        object[] args = new object[1];
        args[0] = boardData.GetMainboardType();
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(PromptDevice), args, PublicFunction.PopWin_Alpha, delegate(BasePopWin popMsg) {
            if (popMsg is PromptDevice)
            {
                PromptDevice msg = (PromptDevice)popMsg;
                msg.SetMainTitle(LauguageTool.GetIns().GetText("主控盒"));
                List<string> left = new List<string>();
                List<string> right = new List<string>();
                left.Add(LauguageTool.GetIns().GetText("蓝牙名称"));
                left.Add(LauguageTool.GetIns().GetText("版本号"));
                right.Add(SingletonObject<ConnectCtrl>.GetInst().GetConnectedName());
                right.Add(boardData.mbVersion);
                left.Add(LauguageTool.GetIns().GetText("DianLiang"));
                right.Add(string.Format("{0}%", PlatformMgr.Instance.PowerData.percentage));
                msg.SetContents(left, right);
            }
        });
        
    }

    public static void ShowAngleServoInfo(int id, string version)
    {
        object[] args = new object[1];
        args[0] = TopologyPartType.Servo;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(PromptDevice), args, PublicFunction.PopWin_Alpha, delegate(BasePopWin popMsg) {
            if (popMsg is PromptDevice)
            {
                PromptDevice msg = (PromptDevice)popMsg;
                msg.SetMainTitle(LauguageTool.GetIns().GetText("角度模式舵机"));
                msg.SetSubTitle(LauguageTool.GetIns().GetText("只能在-118°~118°之间转动"));
                List<string> left = new List<string>();
                List<string> right = new List<string>();
                left.Add("ID:");
                left.Add(LauguageTool.GetIns().GetText("版本号"));
                right.Add(id.ToString());
                right.Add(version);
                msg.SetContents(left, right);
            }
        });
        
    }

    public static void ShowTurnServoInfo(int id, string version)
    {
        object[] args = new object[1];
        args[0] = TopologyPartType.Servo;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(PromptDevice), args, PublicFunction.PopWin_Alpha, delegate(BasePopWin popMsg) {
            if (popMsg is PromptDevice)
            {
                PromptDevice msg = (PromptDevice)popMsg;
                msg.SetMainTitle(LauguageTool.GetIns().GetText("轮模式舵机"));
                msg.SetSubTitle(LauguageTool.GetIns().GetText("可以360°转动"));
                List<string> left = new List<string>();
                List<string> right = new List<string>();
                left.Add("ID:");
                left.Add(LauguageTool.GetIns().GetText("版本号"));
                right.Add(id.ToString());
                right.Add(version);
                msg.SetContents(left, right);
            }
        });
        
    }

    public static void ShowSensorInfo(TopologyPartType sensorType, int id, string version)
    {
        object[] args = new object[1];
        args[0] = sensorType;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(PromptDevice), args, PublicFunction.PopWin_Alpha, delegate(BasePopWin popMsg) {
            if (popMsg is PromptDevice)
            {
                PromptDevice msg = (PromptDevice)popMsg;
                switch (sensorType)
                {
                    case TopologyPartType.Infrared:
                        msg.SetMainTitle(LauguageTool.GetIns().GetText("红外传感器"));
                        break;
                    case TopologyPartType.Gyro:
                        msg.SetMainTitle(string.Format(LauguageTool.GetIns().GetText("陀螺仪传感器ID"), string.Empty));
                        break;
                    case TopologyPartType.Touch:
                        msg.SetMainTitle(LauguageTool.GetIns().GetText("触碰传感器"));
                        break;
                    case TopologyPartType.Light:
                        msg.SetMainTitle(LauguageTool.GetIns().GetText("Led灯"));
                        break;
                    case TopologyPartType.Speaker:
                        msg.SetMainTitle(LauguageTool.GetIns().GetText("蓝牙音响"));
                        break;
                    case TopologyPartType.DigitalTube:
                        msg.SetMainTitle(LauguageTool.GetIns().GetText("数码管"));
                        break;
                    case TopologyPartType.Ultrasonic:
                        msg.SetMainTitle(string.Format(LauguageTool.GetIns().GetText("超声传感器ID"), string.Empty));
                        break;
                    case TopologyPartType.Color:
                        msg.SetMainTitle(LauguageTool.GetIns().GetText("颜色传感器"));
                        break;
                    case TopologyPartType.RgbLight:
                        msg.SetMainTitle(LauguageTool.GetIns().GetText("独角兽灯"));
                        break;
                }

                List<string> left = new List<string>();
                List<string> right = new List<string>();
                left.Add("ID:");
                left.Add(LauguageTool.GetIns().GetText("版本号"));
                right.Add(id.ToString());
                right.Add(version);
                msg.SetContents(left, right);
            }
        });
    }
    
    
    public static void ShowMotorInfo(int id, string version)
    {
        object[] args = new object[1];
        args[0] = TopologyPartType.Motor;
        SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(PromptDevice), args, PublicFunction.PopWin_Alpha, delegate(BasePopWin popMsg) {
            if (popMsg is PromptDevice)
            {
                PromptDevice msg = (PromptDevice)popMsg;
                msg.SetMainTitle(LauguageTool.GetIns().GetText("马达"));
                List<string> left = new List<string>();
                List<string> right = new List<string>();
                left.Add("ID:");
                left.Add(LauguageTool.GetIns().GetText("版本号"));
                right.Add(id.ToString());
                right.Add(version);
                msg.SetContents(left, right);
            }
        });
        
    }
    #endregion

    #region 其他函数
    protected override void AddEvent()
    {
        base.AddEvent();
        try
        {
            if (null != mTrans)
            {
                Transform title = mTrans.Find("title");
                if (null != title)
                {
                    Transform maintitle = title.Find("maintitle");
                    Transform subtitle = title.Find("subtitle");
                    if (TopologyPartType.Servo != mDeviceType)
                    {
                        maintitle.localPosition = Vector3.zero;
                        subtitle.gameObject.SetActive(false);
                    }
                }
                Transform contents = mTrans.Find("contents");
                if (null != contents)
                {
                    Transform left = contents.Find("left");
                    Transform right = contents.Find("right");
                    if (mDeviceType != TopologyPartType.MainBoard && mDeviceType != TopologyPartType.MainBoard_new_low)
                    {
                        left.Find("Label3").gameObject.SetActive(false);
                        right.Find("Label3").gameObject.SetActive(false);
                        contents.localPosition -= new Vector3(0, 20);
                    }
                }

                UIWidget widget = GameHelper.FindChildComponent<UIWidget>(mTrans, "closeBtn");
                if (null != widget)
                {
                    widget.width = PublicFunction.GetExtendWidth();
                    widget.height = PublicFunction.GetExtendHeight();
                }
            }
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
        if (!obj.name.Equals("bg"))
        {
            OnClose();
        }
    }

    void SetMainTitle(string text)
    {
        if (null != mTrans)
        {
            GameHelper.SetLabelText(mTrans.Find("title/maintitle"), text);
        }
    }

    void SetSubTitle(string text)
    {
        if (null != mTrans)
        {
            GameHelper.SetLabelText(mTrans.Find("title/subtitle"), text);
        }
    }

    void SetContents(List<string> leftList, List<string> rightList)
    {
        if (null != mTrans)
        {
            Transform contents = mTrans.Find("contents");
            if (null != contents)
            {
                int imax = leftList.Count >= rightList.Count ? rightList.Count : leftList.Count;
                for (int i = 0; i < imax; ++i)
                {
                    GameHelper.SetLabelText(contents.Find("left/Label" + (i + 1)), leftList[i]);
                    GameHelper.SetLabelText(contents.Find("right/Label" + (i + 1)), rightList[i]);
                }
            }
        }
        
        
    }
    #endregion
}