using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:DeviceMsg.cs
/// Description:显示设备信息
/// Time:2016/8/26 15:03:34
/// </summary>
public class DeviceMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数

    public DeviceMsg()
    {
        mUIResPath = "Prefab/UI/PromptDevice";
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
                Transform contents = mTrans.Find("contents");
                if (null != contents)
                {
                    Transform left = contents.Find("left");
                    if (null != left)
                    {
                        string[] leftStr = new string[] { "ZhuBanBanBenHao", "DuoJiBanBenXinXi", "DianLiang" };
                        for (int i = 1; i <= 3; ++i)
                        {
                            UILabel label = GameHelper.FindChildComponent<UILabel>(left, string.Format("Label{0}", i));
                            if (null != label)
                            {
                                label.text = LauguageTool.GetIns().GetText(leftStr[i - 1]);
                            }
                        }
                    }
                    Transform right = contents.Find("right");
                    if (null != right)
                    {
                        string[] rightStr = null;
                        Robot robot = RobotManager.GetInst().GetCurrentRobot();
                        if (null != robot && null != robot.MotherboardData)
                        {
                            rightStr = new string[] { robot.MotherboardData.mbVersion, robot.MotherboardData.djVersion, string.Format("{0}%",PlatformMgr.Instance.PowerData.percentage)};
                        }
                        else
                        {
                            rightStr = new string[] { string.Empty, string.Empty, string.Empty };
                        }
                        for (int i = 1; i <= 3; ++i)
                        {
                            UILabel label = GameHelper.FindChildComponent<UILabel>(right, string.Format("Label{0}", i));
                            if (null != label)
                            {
                                label.text = rightStr[i - 1];
                            }
                        }
                    }
                }

                UILabel lb = GameHelper.FindChildComponent<UILabel>(mTrans, "btn/leftBtn/Label");
                if (null != lb)
                {
                    lb.text = LauguageTool.GetIns().GetText("确定");
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
        if (obj.name.Equals("leftBtn"))
        {
            OnClose();
        }
    }
    #endregion
}