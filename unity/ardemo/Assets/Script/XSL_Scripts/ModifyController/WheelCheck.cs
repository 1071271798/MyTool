using UnityEngine;
using System.Collections;

/// <summary>
/// 检测轮模式改变频率是否太快  - 摇杆模式
/// 时间过滤类
/// </summary>
public class WheelCheck : MonoBehaviour {

    public JoyStickControl joyControl;

    float leftT;
    float rightT;
    float turnTime;
    /// <summary>
    /// 检测改变的频率是否太快
    /// </summary>
    void FixedUpdate()
    {
        if (UserdefControllerUI.isSetting)
            return;
        if (joyControl == null)
        {
            joyControl = ControllerManager.GetInst().widgetManager.joyStickManager.GetJoystickByID(gameObject.name);
            if (joyControl == null)
                return;
        }
        /*if (!joyControl.isReady)
            return;
        if (joyControl.leftFlag)
        {
            joyControl.leftFlag = false;  //时间拦截
            joyControl.leftOver = false;
            leftT = Time.time;
        }
        if (Time.time - leftT > 0.01f && !joyControl.leftOver)
            joyControl.LeftWheelTurn();

        if (joyControl.rightFlag)
        {
            joyControl.rightFlag = false;
            joyControl.rightOver = false;
            rightT = Time.time;
        }
        if (Time.time - rightT > 0.01f && !joyControl.rightOver)
            joyControl.RightWheelTurn();*/
        /*if (joyControl.turnFlag)
        {
            joyControl.turnFlag = false;
            joyControl.turnOver = false;
            turnTime = Time.time;
        }
        if (Time.time - turnTime > 0.1f && !joyControl.turnOver)
        {
            joyControl.SendWheelTurn();
        }*/
    }

}
