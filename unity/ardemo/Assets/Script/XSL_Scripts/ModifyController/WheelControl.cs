using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;
using Game.Event;
/// <summary>
/// author: xiongsonglin
/// describe:控件管理类
/// time: 
/// </summary>
public class WidgetManager
{
    public JoyStickContrlManager joyStickManager;
    public VSliderControlManager vSliderManager;
    public HSliderControlManager hSliderManager;
    public static ushort[] speedArray = new ushort[] { 0x0080, 0x00EA, 0x0154, 0x01BE, 0x0228, 0x0292 };  //速度档位
    //public static ushort[] speedArray = new ushort[] { 0x0080, 0x0154, 0x0208, 0x0292 ,0x320, 0x3DB};  //速度档位
    public static byte[] motorSpeedArray = new byte[] { 0, 60, 80, 100, 120, 140};//马达速度档位

    public WidgetManager(ControllerData data)
    {
        if (data != null)
        {
            joyStickManager = new JoyStickContrlManager(data.GetJockList());
            vSliderManager = new VSliderControlManager(data.GetSliderList());
            hSliderManager = new HSliderControlManager(data.GetHSliderList());
        }
    }

    public void ClearUp()
    {
        joyStickManager.ClearUp();
        vSliderManager.ClearUp();
        hSliderManager.ClearUp();
    }

    /// <summary>
    /// 新增摇杆控件
    /// </summary>
    /// <param name="data"></param>
    public void AddJoystickControl(JockstickData data)
    {
        joyStickManager.AddNewStickcontrol(data);
    }
    /// <summary>
    /// 删除摇杆控件
    /// </summary>
    /// <param name="widgetID"></param>
    public void RemoveJoystickControl(string widgetID)
    {
        joyStickManager.RemoveStickcontrol(widgetID);
    }
    /// <summary>
    /// 新增滑杆控件
    /// </summary>
    /// <param name="widgetID"></param>
    public void AddVsliderControl(SliderWidgetData data)
    {
        vSliderManager.AddNewStickcontrol(data);
    }
    /// <summary>
    /// 删除滑杆控件
    /// </summary>
    /// <param name="widgetID"></param>
    public void RemoveVsliderControl(string widgetID)
    {
        vSliderManager.RemoveStickcontrol(widgetID);
    }
    /// <summary>
    /// 新增横杆控件
    /// </summary>
    /// <param name="widgetID"></param>
    public void AddHsliderControl(HSliderWidgetData data)
    {
        hSliderManager.AddNewHStickcontrol(data);
    }
    /// <summary>
    /// 删除横杆控件
    /// </summary>
    /// <param name="widgetID"></param>
    public void RemoveHsliderControl(string widgetID)
    {
        hSliderManager.RemoveHStickcontrol(widgetID);
    }
    /// <summary>
    /// 新增动作数据
    /// </summary>
    /// <param name="data"></param>
    public void AddActionControl(ActionWidgetData data)
    {
        
    }
    public void RemoveActionControl(string widgetID)
    { }
  //  public void Init()
}

/// <summary>
/// 摇杆控制管理类
/// </summary>
public class JoyStickContrlManager
{
    List<JoyStickControl> joystickList;

    public JoyStickControl GetJoystickByID(string ID)
    {
        if (joystickList != null)
            return joystickList.Find((x) => x.ID == ID);
        else
            return null;
    }

    public void ClearUp()
    {
        joystickList = new List<JoyStickControl>();
    }

    public int JoystickNum()
    {
        return joystickList.Count;
    }

    public JoyStickContrlManager(List<JockstickData> jockdata)
    {
        joystickList = new List<JoyStickControl>(jockdata.Count);
        for (int i = 0; i < jockdata.Count; i++)
        {
            joystickList.Add(new JoyStickControl(jockdata[i]));
        }
    }
    /// <summary>
    /// 新增摇杆控件
    /// </summary>
    /// <param name="data"></param>
    public void AddNewStickcontrol(JockstickData data)
    {
        joystickList.Add(new JoyStickControl(data));
    }
    /// <summary>
    /// 删除摇杆控件
    /// </summary>
    /// <param name="widgetid"></param>
    public void RemoveStickcontrol(string widgetid)
    {
        joystickList.Remove(GetJoystickByID(widgetid));
    }
    /// <summary>
    /// 准备就绪
    /// </summary>
    public void ReadyJockControl()
    {
        if (joystickList != null)
        {
            foreach (var tem in joystickList)
            {
                if (tem.joyData != null)
                    tem.isRightSetting = tem.joyData.isOK;
                else
                    tem.isRightSetting = false;
            }
        }
    }
}

/// <summary>
/// 摇杆控制类
/// author:xsl
/// description:
/// </summary>
public class JoyStickControl 
{
    public bool isReady;  //摇杆控件是否可以控制 蓝牙ok，配置ok
    public string ID;  //控件的id 
    public JockstickData joyData;
    private TurnData preLeftWheel;
    private TurnData preRightWheel;
    private TurnData leftWheel;
    private TurnData rightWheel;
    public bool isRightSetting;
    
    private int _leftWheelSpeed1;
    private int _leftWheelSpeed2;
    private int _rightWheelSpeed1;
    private int _rightWheelSpeed2;
    
    // 速度频繁改变时 不予处理
    float leftTime;
    float rightTime;
    //public static bool leftFlag = false;     
    //public static bool rightFlag = false;
    public bool leftFlag = false;     //两个控件的时间过滤不能相互影响 所以要非静态
    public bool rightFlag = false;

    float turnTime;
    public bool turnFlag = false;
    public bool turnOver = false;

    private void SetWheelSpeed1(int leftSpeed, int rightSpeed)
    {
        //PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, string.Format("leftSpeed = {0} rightSpeed = {1}", leftSpeed, rightSpeed));
        if (joyData.type != JockstickData.JockType.treeServo)  //非三轮模式
        {
            if (leftSpeed != _leftWheelSpeed1 && leftSpeed != 0) //发生改变 发送命令
            {
                if (leftSpeed > 0)
                    leftWheel.turnDirection = TurnDirection.turnByClock;
                else
                    leftWheel.turnDirection = TurnDirection.turnByDisclock;
                if (joyData.motionType == CtrlMotionType.servo)
                {
                    leftWheel.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(leftSpeed)];
                }
                else if (joyData.motionType == CtrlMotionType.motor)
                {
                    leftWheel.turnSpeed = (ushort)WidgetManager.motorSpeedArray[Mathf.Abs(leftSpeed)];
                }
                turnFlag = true;
            }
            else if (leftSpeed != 0) //不发送指令
            {

            }
            else //发送停止指令
            {
                leftWheel.turnSpeed = 0;
                leftWheel.turnDirection = TurnDirection.turnStop;
                turnFlag = true;
            }
        }
        _leftWheelSpeed1 = leftSpeed;
        if (joyData.type != JockstickData.JockType.treeServo) //非三轮模式
        {
            if (rightSpeed != _rightWheelSpeed1 && rightSpeed != 0) //发生改变
            {
                if (rightSpeed > 0)
                    rightWheel.turnDirection = TurnDirection.turnByClock;
                else
                    rightWheel.turnDirection = TurnDirection.turnByDisclock;
                if (joyData.motionType == CtrlMotionType.servo)
                {
                    rightWheel.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(rightSpeed)];
                }
                else if (joyData.motionType == CtrlMotionType.motor)
                {
                    rightWheel.turnSpeed = (ushort)WidgetManager.motorSpeedArray[Mathf.Abs(rightSpeed)];
                }
                turnFlag = true;
            }
            else if (rightSpeed != 0) //不发送指令
            {
            }
            else  //发送停止指令
            {
                rightWheel.turnSpeed = 0;
                rightWheel.turnDirection = TurnDirection.turnStop;
                turnFlag = true;
            }
        }
        _rightWheelSpeed1 = rightSpeed;
        if (isReady && turnFlag)
        {
            SendWheelTurn();
        }
    }

    private int leftWheelSpeed1
    {
        get
        {
            return _leftWheelSpeed1;
        }
        set
        {
            if (joyData.type != JockstickData.JockType.treeServo)  //非三轮模式
            {
                if (value != _leftWheelSpeed1 && value != 0) //发生改变 发送命令
                {
                    if (value > 0)
                        leftWheel.turnDirection = TurnDirection.turnByClock;
                    else
                        leftWheel.turnDirection = TurnDirection.turnByDisclock;
                    if (joyData.motionType == CtrlMotionType.servo)
                    {
                        leftWheel.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(value)];
                    }
                    else if (joyData.motionType == CtrlMotionType.motor)
                    {
                        leftWheel.turnSpeed = (ushort)WidgetManager.motorSpeedArray[Mathf.Abs(value)];
                    }
                    leftFlag = true;
                }
                else if (value != 0) //不发送指令
                {

                }
                else //发送停止指令
                {
                    leftWheel.turnSpeed = 0;
                    leftWheel.turnDirection = TurnDirection.turnStop;

                    leftFlag = true;
                }
            }
            _leftWheelSpeed1 = value;
            if (isReady)
            {
                LeftWheelTurn();
            }
        }
    }
    private int rightWheelSpeed1
    {
        get
        {
            return _rightWheelSpeed1;
        }
        set
        {
            if (joyData.type != JockstickData.JockType.treeServo) //非三轮模式
            {
                if (value != _rightWheelSpeed1 && value != 0) //发生改变
                {
                    if (value > 0)
                        rightWheel.turnDirection = TurnDirection.turnByClock;
                    else
                        rightWheel.turnDirection = TurnDirection.turnByDisclock;
                    if (joyData.motionType == CtrlMotionType.servo)
                    {
                        rightWheel.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(value)];
                    }
                    else if (joyData.motionType == CtrlMotionType.motor)
                    {
                        rightWheel.turnSpeed = (ushort)WidgetManager.motorSpeedArray[Mathf.Abs(value)];
                    }

                    rightFlag = true;
                }
                else if (value != 0) //不发送指令
                {
                    //   Debug.Log("右轮速度保持不变！");
                }
                else  //发送停止指令
                {
                    rightWheel.turnSpeed = 0;
                    rightWheel.turnDirection = TurnDirection.turnStop;

                    rightFlag = true;
                }
            }
            _rightWheelSpeed1 = value;
            if (isReady)
            {
                RightWheelTurn();
            }
        }
    }
    
    
    /// <summary>
    /// 默认左轮id为1，右轮id为2
    /// </summary>
    public JoyStickControl(JockstickData data)
    {
        this.joyData = data;
        this.ID = data.widgetId;
    }

    public void TurnWhellSpeed(float x, float y, int r)
    {
        float leftSpeed = 0;
        float rightSpeed = 0;
        int leftDir = 0;
        int rightDir = 0;
        //确定方向
        if (y > x)
        {
            rightDir = 1;
        } else if (y < x)
        {
            rightDir = -1;
        } else
        {
            rightDir = 0;
        }
        if (y > -x)
        {
            leftDir = -1;
        } else if (y < -x)
        {
            leftDir = 1;
        } else
        {
            leftDir = 0;
        }
        //确定速度
        if (y >= 0 && x >= 0 || y <= 0 && x <= 0)
        {
            leftSpeed = Mathf.Sqrt(x * x + y * y) / r;
            rightSpeed = Mathf.Abs(x - y) / r;
        } else if (y >= 0 && x <= 0 || y <= 0 && x >= 0)
        {
            leftSpeed = Mathf.Abs(x + y) / r;
            rightSpeed = Mathf.Sqrt(x * x + y * y) / r;
        }/* else if (y <= 0 && x <= 0 )
        {
            leftSpeed = Mathf.Sqrt(x * x + y * y) / r;
            rightSpeed = Mathf.Abs(x - y) / r;
        } else if (y <= 0 && x >= 0)
        {
            leftSpeed = Mathf.Abs(x + y) / r;
            rightSpeed = Mathf.Sqrt(x * x + y * y) / r;
        }*/
        int leftSpeedTarget = FloatToIndex(leftSpeed);
        int rightSpeedTarget = FloatToIndex(rightSpeed);
        leftSpeedTarget *= leftDir;
        rightSpeedTarget *= rightDir;
        Debug.Log("leftSpeedTarget = " + leftSpeedTarget + " rightSpeedTarget = " + rightSpeedTarget);
        SetWheelSpeed1(leftSpeedTarget, rightSpeedTarget);
    }

    int FloatToIndex(float speed)
    {
        if (speed < 0.1f)
        {
            return 0;
        }
        else if (speed < 0.3f)
        {
            return 1;
        }
        else if (speed < 0.5f)
        {
            return 2;
        }
        else if (speed < 0.7f)
        {
            return 3;
        }
        else if (speed < 0.9f)
        {
            return 4;
        }
        else
        {
            return 5;
        }
    }
    /*public void TurnWhellSpeed(float x, float y, int r)
    {
        int leftSpeed = 0;
        int rightSpeed = 0;
        float lefta = (Mathf.Abs(x) - x - 2 * y) / (2 * r);
        float righta = (2 * y - x - Mathf.Abs(x)) / (2 * r);
        if (lefta < 0.1f && lefta > -0.1f)
        {
            leftSpeed = 0;
        } else
        {
            int zf = lefta > 0 ? 1 : -1;
            float aa = Mathf.Abs(lefta);
            if (aa < 0.3f)
                leftSpeed = 1 * zf;
            else if (aa < 0.5f)
                leftSpeed = 2 * zf;
            else if (aa < 0.7f)
                leftSpeed = 3 * zf;
            else if (aa < 0.9f)
                leftSpeed = 4 * zf;
            else
                leftSpeed = 5 * zf;
        }
        
        if (righta < 0.1f && righta > -0.1f)
        {
            rightSpeed = 0;
        } else
        {
            int zf = righta > 0 ? 1 : -1;
            float aa = Mathf.Abs(righta);
            if (aa < 0.3f)
                rightSpeed = 1 * zf;
            else if (aa < 0.5f)
                rightSpeed = 2 * zf;
            else if (aa < 0.7f)
                rightSpeed = 3 * zf;
            else if (aa < 0.9f)
                rightSpeed = 4 * zf;
            else
                rightSpeed = 5 * zf;
        }
        SetWheelSpeed1(leftSpeed, rightSpeed);
    }*/

    /// <summary>
    /// 左轮的档位值
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="r"></param>
    public void TurnLeftwheelSpeed(float x, float y, int r)
    {
        float a = (Mathf.Abs(x) - x - 2 * y)/(2*r);
        if (a < 0.1f && a > -0.1f)
        {
            leftWheelSpeed1 = 0;
            return;
        }
        int zf = a > 0 ? 1 : -1;
        float aa = Mathf.Abs(a);
        if (aa < 0.3f)
            leftWheelSpeed1 = 1*zf;
        else if (aa < 0.5f)
            leftWheelSpeed1 = 2*zf;
        else if (aa < 0.7f)
            leftWheelSpeed1 = 3*zf;
        else if (aa < 0.9f)
            leftWheelSpeed1 = 4*zf;
        else
            leftWheelSpeed1 = 5*zf;
    }
    /// <summary>
    /// 右轮的档位值
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="r"></param>
    public void TurnRightwheelSpeed(float x, float y, int r)
    {
        float a = (2 * y - x - Mathf.Abs(x))/(2*r);
        if (a < 0.1f && a > -0.1f)
        {
            rightWheelSpeed1 = 0;
            return;
        }
        int zf = a > 0 ? 1 : -1;
        float aa = Mathf.Abs(a);
        if (aa < 0.3f)
            rightWheelSpeed1 = 1*zf;
        else if (aa < 0.5f)
            rightWheelSpeed1 = 2*zf;
        else if (aa < 0.7f)
            rightWheelSpeed1 = 3*zf;
        else if (aa < 0.9f)
            rightWheelSpeed1 = 4*zf;
        else
            rightWheelSpeed1 = 5*zf;
    }

    public void SendWheelTurn()
    {
        turnOver = true;
        if (preLeftWheel.turnSpeed != leftWheel.turnSpeed || preLeftWheel.turnDirection != leftWheel.turnDirection 
            || preRightWheel.turnSpeed != rightWheel.turnSpeed || preRightWheel.turnDirection != rightWheel.turnDirection) //数据不一样时发送
        {
            if (joyData.motionType == CtrlMotionType.servo)
            {
                Dictionary<byte, TurnData> dict = new Dictionary<byte, TurnData>();
                if (joyData.type == JockstickData.JockType.twoServo)
                {
                    dict[joyData.leftUpID] = leftWheel;
                    dict[joyData.rightUpID] = rightWheel;
                }
                else if (joyData.type == JockstickData.JockType.fourServo)
                {
                    dict[joyData.leftUpID] = leftWheel;
                    dict[joyData.leftBottomID] = leftWheel;
                    dict[joyData.rightUpID] = rightWheel;
                    dict[joyData.rightBottomID] = rightWheel;
                }
                if (dict.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(dict);
                }
            } else if (joyData.motionType == CtrlMotionType.motor)
            {
                List<SendMotorData> list = new List<SendMotorData>();
                List<byte> stopList = new List<byte>();
                if (joyData.type == JockstickData.JockType.twoServo)
                {
                    if (leftWheel.turnDirection == TurnDirection.turnStop && leftWheel.turnDirection != preLeftWheel.turnDirection)
                    {
                        stopList.Add(joyData.leftUpID);
                    } else
                    {
                        SendMotorData leftData = new SendMotorData();
                        leftData.id = joyData.leftUpID;
                        leftData.direction = leftWheel.turnDirection;
                        leftData.speed = (short)leftWheel.turnSpeed;
                        leftData.time = 65535;
                        list.Add(leftData);
                    }
                    if (rightWheel.turnDirection == TurnDirection.turnStop && rightWheel.turnDirection != preRightWheel.turnDirection)
                    {
                        stopList.Add(joyData.rightUpID);
                    } else
                    {
                        SendMotorData rightData = new SendMotorData();
                        rightData.id = joyData.rightUpID;
                        rightData.direction = rightWheel.turnDirection;
                        rightData.speed = (short)rightWheel.turnSpeed;
                        rightData.time = 65535;
                        list.Add(rightData);
                    }
                } else if (joyData.type == JockstickData.JockType.fourServo)
                {
                    if (leftWheel.turnDirection == TurnDirection.turnStop && leftWheel.turnDirection != preLeftWheel.turnDirection)
                    {
                        stopList.Add(joyData.leftUpID);
                        stopList.Add(joyData.leftBottomID);
                    }
                    else
                    {
                        SendMotorData leftUpData = new SendMotorData();
                        leftUpData.id = joyData.leftUpID;
                        leftUpData.direction = leftWheel.turnDirection;
                        leftUpData.speed = (short)leftWheel.turnSpeed;
                        leftUpData.time = 65535;
                        list.Add(leftUpData);
                        SendMotorData leftBottomData = new SendMotorData();
                        leftBottomData.id = joyData.leftBottomID;
                        leftBottomData.direction = leftWheel.turnDirection;
                        leftBottomData.speed = (short)leftWheel.turnSpeed;
                        leftBottomData.time = 65535;
                        list.Add(leftBottomData);
                    }
                    if (rightWheel.turnDirection == TurnDirection.turnStop && rightWheel.turnDirection != preRightWheel.turnDirection)
                    {
                        stopList.Add(joyData.rightUpID);
                        stopList.Add(joyData.rightBottomID);
                    }
                    else
                    {
                        SendMotorData rightUpData = new SendMotorData();
                        rightUpData.id = joyData.rightUpID;
                        rightUpData.direction = rightWheel.turnDirection;
                        rightUpData.speed = (short)rightWheel.turnSpeed;
                        rightUpData.time = 65535;
                        list.Add(rightUpData);
                        SendMotorData rightBottomData = new SendMotorData();
                        rightBottomData.id = joyData.rightBottomID;
                        rightBottomData.direction = rightWheel.turnDirection;
                        rightBottomData.speed = (short)rightWheel.turnSpeed;
                        rightBottomData.time = 65535;
                        list.Add(rightBottomData);
                    }
                }
                if (stopList.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().StopMotor(stopList);
                }
                if (list.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().PlayMotor(list);
                }
            }
            preLeftWheel = leftWheel;
            preRightWheel = rightWheel;
        }
    }

    public bool leftOver;
    /// <summary>
    /// 左轮数据发送
    /// </summary>
    public void LeftWheelTurn()
    {
        leftOver = true;
        if (preLeftWheel.turnSpeed != leftWheel.turnSpeed || preLeftWheel.turnDirection != leftWheel.turnDirection) //数据不一样时发送
        {
            if (joyData.motionType == CtrlMotionType.servo)
            {
                Dictionary<byte, TurnData> dict = new Dictionary<byte, TurnData>();
                if (joyData.type == JockstickData.JockType.twoServo)
                {
                    dict[joyData.leftUpID] = leftWheel;
                }
                else if (joyData.type == JockstickData.JockType.fourServo)
                {
                    dict[joyData.leftUpID] = leftWheel;
                    dict[joyData.leftBottomID] = leftWheel;
                }
                if (dict.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(dict);
                }
            }
            else if (joyData.motionType == CtrlMotionType.motor)
            {
                List<SendMotorData> list = new List<SendMotorData>();
                List<byte> stopList = new List<byte>();
                if (joyData.type == JockstickData.JockType.twoServo)
                {
                    if (leftWheel.turnDirection == TurnDirection.turnStop)
                    {
                        stopList.Add(joyData.leftUpID);
                    }
                    else
                    {
                        SendMotorData leftData = new SendMotorData();
                        leftData.id = joyData.leftUpID;
                        leftData.direction = leftWheel.turnDirection;
                        leftData.speed = (short)leftWheel.turnSpeed;
                        leftData.time = 65535;
                        list.Add(leftData);
                    }
                }
                else if (joyData.type == JockstickData.JockType.fourServo)
                {
                    if (leftWheel.turnDirection == TurnDirection.turnStop)
                    {
                        stopList.Add(joyData.leftUpID);
                        stopList.Add(joyData.leftBottomID);
                    }
                    else
                    {
                        SendMotorData leftUpData = new SendMotorData();
                        leftUpData.id = joyData.leftUpID;
                        leftUpData.direction = leftWheel.turnDirection;
                        leftUpData.speed = (short)leftWheel.turnSpeed;
                        leftUpData.time = 65535;
                        list.Add(leftUpData);
                        SendMotorData leftBottomData = new SendMotorData();
                        leftBottomData.id = joyData.leftBottomID;
                        leftBottomData.direction = leftWheel.turnDirection;
                        leftBottomData.speed = (short)leftWheel.turnSpeed;
                        leftBottomData.time = 65535;
                        list.Add(leftBottomData);
                    }
                }
                if (stopList.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().StopMotor(stopList);
                }
                if (list.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().PlayMotor(list);
                }
            }
            preLeftWheel = leftWheel;
        }
    }
    public bool rightOver;
    /// <summary>
    /// 右轮数据发送
    /// </summary>
    public void RightWheelTurn()
    {
        rightOver = true;
        if (rightWheel.turnSpeed != preRightWheel.turnSpeed || rightWheel.turnDirection != preRightWheel.turnDirection) ////数据不一样时发送
        {
            if (joyData.motionType == CtrlMotionType.servo)
            {
                Dictionary<byte, TurnData> dict = new Dictionary<byte, TurnData>();
                if (joyData.type == JockstickData.JockType.twoServo)
                {
                    dict[joyData.rightUpID] = rightWheel;
                }
                else if (joyData.type == JockstickData.JockType.fourServo)
                {
                    dict[joyData.rightUpID] = rightWheel;
                    dict[joyData.rightBottomID] = rightWheel;
                }
                if (dict.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(dict);
                }
            }
            else if (joyData.motionType == CtrlMotionType.motor)
            {
                List<SendMotorData> list = new List<SendMotorData>();
                List<byte> stopList = new List<byte>();
                if (joyData.type == JockstickData.JockType.twoServo)
                {
                    if (rightWheel.turnDirection == TurnDirection.turnStop)
                    {
                        stopList.Add(joyData.rightUpID);
                    }
                    else
                    {
                        SendMotorData rightData = new SendMotorData();
                        rightData.id = joyData.rightUpID;
                        rightData.direction = rightWheel.turnDirection;
                        rightData.speed = (short)rightWheel.turnSpeed;
                        rightData.time = 65535;
                        list.Add(rightData);
                    }
                }
                else if (joyData.type == JockstickData.JockType.fourServo)
                {
                    if (rightWheel.turnDirection == TurnDirection.turnStop)
                    {
                        stopList.Add(joyData.rightUpID);
                        stopList.Add(joyData.rightBottomID);
                    }
                    else
                    {
                        SendMotorData rightUpData = new SendMotorData();
                        rightUpData.id = joyData.rightUpID;
                        rightUpData.direction = rightWheel.turnDirection;
                        rightUpData.speed = (short)rightWheel.turnSpeed;
                        rightUpData.time = 65535;
                        list.Add(rightUpData);
                        SendMotorData rightBottomData = new SendMotorData();
                        rightBottomData.id = joyData.rightBottomID;
                        rightBottomData.direction = rightWheel.turnDirection;
                        rightBottomData.speed = (short)rightWheel.turnSpeed;
                        rightBottomData.time = 65535;
                        list.Add(rightBottomData);
                    }
                }
                if (stopList.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().StopMotor(stopList);
                }
                if (list.Count > 0)
                {
                    RobotManager.GetInst().GetCurrentRobot().PlayMotor(list);
                }
            }
            preRightWheel = rightWheel;
        }
        
    }
}
/// <summary>
/// 横滑杆管理类
/// </summary>
public class HSliderControlManager
{
    List<HSliderControl> hSliderList;

    public HSliderControl GetHSliderByID(string id)
    {
        if (hSliderList != null)
        {
            return hSliderList.Find((x) => x.ID == id);
        }
        else
        {
            return null;
        }
    }

    public HSliderControlManager(List<HSliderWidgetData> hList)
    {
        hSliderList = new List<HSliderControl>(hList.Count);
        for (int i = 0; i < hList.Count; i++)
        {
            hSliderList.Add(new HSliderControl(hList[i]));
        }
    }

    /// <summary>
    /// 新增横滑杆控件
    /// </summary>
    /// <param name="data"></param>
    public void AddNewHStickcontrol(HSliderWidgetData data)
    {
        hSliderList.Add(new HSliderControl(data));
        // joystickList.Add(new JoyStickControl(data));
    }
    /// <summary>
    /// 删除横滑杆控件
    /// </summary>
    /// <param name="widgetid"></param>
    public void RemoveHStickcontrol(string widgetid)
    {
        hSliderList.Remove(GetHSliderByID(widgetid));
        // joystickList.Remove(GetJoystickByID(widgetid));
    }

    public void ClearUp()
    {
        hSliderList = new List<HSliderControl>();
    }

    /// <summary>
    /// 准备就绪
    /// </summary>
    public void ReadyHsliderControl()
    {
        if (hSliderList != null)
        {
            foreach (var tem in hSliderList)
            {
                if (tem.hsliderData != null)
                {
                    tem.isRightSetting = tem.hsliderData.isOK;
                }
                else
                    tem.isRightSetting = false;
            }
        }
    }
}
/// <summary>
/// 横滑杆控制类
/// </summary>
public class HSliderControl
{
    public bool isReady;  //控件是否可以控制 蓝牙ok，配置ok
    public string ID;
    public HSliderWidgetData hsliderData;
    private TurnData wheelData;
    private TurnData preWheelData;
    public static bool turnOver;
    public bool isRightSetting;
    public bool changeFlag;
    public bool changeOver;
    private int speedvalue; //速度档位值
    public int Speedvalue                          //档位值发生改变时 记录当前轮模式的数据改变
    {
        get
        {
            return speedvalue;
        }
        set
        {
            if (value != speedvalue && value != 0) //发生改变
            {
                int k = 1;
                /*if (hsliderData.directionDisclock)
                    k = -1;
                else
                    k = 1;*/
                if (value * k > 0)
                    wheelData.turnDirection = TurnDirection.turnByClock;
                else
                    wheelData.turnDirection = TurnDirection.turnByDisclock;
                wheelData.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(value)];
                changeFlag = true;  //标记着值发生改变
            }
            else if (value != 0)
            {

            }
            else
            {
                wheelData.turnDirection = TurnDirection.turnStop;
                wheelData.turnSpeed = 0;
                changeFlag = true;
            }
            speedvalue = value;
        }
    }

    public HSliderControl(HSliderWidgetData data)
    {
        hsliderData = data;
        this.ID = data.widgetId;
    }
    /// <summary>
    /// 坐标与速度的转换
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="r"></param>
    public void TurnWheelSpeed(float x, float y, int r)
    {
        float a = (2 * y - x - Mathf.Abs(x)) / (2 * r);
        if (a < 0.1f && a > -0.1f)
        {
            Speedvalue = 0;
            return;
        }
        int zf = a > 0 ? 1 : -1;
        float aa = Mathf.Abs(a);
        if (aa < 0.3f)
            Speedvalue = 1 * zf;
        else if (aa < 0.5f)
            Speedvalue = 2 * zf;
        else if (aa < 0.7f)
            Speedvalue = 3 * zf;
        else if (aa < 0.9f)
            Speedvalue = 4 * zf;
        else
            Speedvalue = 5 * zf;
    }

    /// <summary>
    /// 发送命令通知硬件
    /// </summary>
    public void WheelTurn()
    {
        changeOver = true;
        if (preWheelData.turnSpeed != wheelData.turnSpeed || preWheelData.turnDirection != wheelData.turnDirection) //数据不一样时发送
        {
            RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(hsliderData.servoID, wheelData);
            preWheelData = wheelData;
        }
    }
}

/// <summary>
/// 竖滑杆管理类
/// </summary>
public class VSliderControlManager
{
    List<VSliderControl> vSliderList;

    public VSliderControl GetVSliderByID(string id)
    {
        if (vSliderList != null)
        {
            return vSliderList.Find((x) => x.ID == id);
        }
        else
        {
            return null;
        }
    }

    public VSliderControlManager(List<SliderWidgetData> vList)
    {
        vSliderList = new List<VSliderControl>(vList.Count);
        for(int i = 0;i<vList.Count;i++)
        {
            vSliderList.Add(new VSliderControl(vList[i]));
        }
    }

    /// <summary>
    /// 新增摇杆控件
    /// </summary>
    /// <param name="data"></param>
    public void AddNewStickcontrol(SliderWidgetData data)
    {
        vSliderList.Add(new VSliderControl(data));
       // joystickList.Add(new JoyStickControl(data));
    }
    /// <summary>
    /// 删除摇杆控件
    /// </summary>
    /// <param name="widgetid"></param>
    public void RemoveStickcontrol(string widgetid)
    {
        vSliderList.Remove(GetVSliderByID(widgetid));
       // joystickList.Remove(GetJoystickByID(widgetid));
    }

    public void ClearUp()
    {
        vSliderList = new List<VSliderControl>();
    }

    /// <summary>
    /// 准备就绪
    /// </summary>
    public void ReadyVsliderControl()
    {
        if (vSliderList != null)
        {
            foreach (var tem in vSliderList)
            {
                if (tem.sliderData != null)
                {
                    tem.isRightSetting = tem.sliderData.isOK;
                }
                else
                    tem.isRightSetting = false;
            }
        }
    }
}
/// <summary>
/// 竖滑杆控制类
/// </summary>
public class VSliderControl
{
    public bool isReady;  //控件是否可以控制 蓝牙ok，配置ok
    public string ID;
    public SliderWidgetData sliderData;
    private TurnData wheelData;
    private TurnData preWheelData;
    public static bool turnOver;
    public bool isRightSetting;
    public bool changeFlag;
    public bool changeOver;
    private int speedvalue; //速度档位值
    public int Speedvalue                          //档位值发生改变时 记录当前轮模式的数据改变
    {
        get
        {
            return speedvalue;
        }
        set
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Speedvalue = " + Speedvalue);
            if (value != speedvalue && value != 0) //发生改变
            {
                int k = 1;
                if (sliderData.directionDisclock)
                    k = -1;
                else
                    k = 1;
                if (value * k > 0)
                    wheelData.turnDirection = TurnDirection.turnByClock;
                else
                    wheelData.turnDirection = TurnDirection.turnByDisclock;
                if (sliderData.motionType == CtrlMotionType.servo)
                {
                    wheelData.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(value)];
                } else if (sliderData.motionType == CtrlMotionType.motor)
                {
                    wheelData.turnSpeed = WidgetManager.motorSpeedArray[Mathf.Abs(value)];
                }
                changeFlag = true;  //标记着值发生改变
            }
            else if (value != 0)
            {

            }
            else
            {
                wheelData.turnDirection = TurnDirection.turnStop;
                wheelData.turnSpeed = 0;
                changeFlag = true;
            }
            speedvalue = value;
            if (isReady && changeFlag)
            {
                WheelTurn();
            }
        }
    }

    public VSliderControl(SliderWidgetData data)
    {
        sliderData = data;
        this.ID = data.widgetId;
    }
    /// <summary>
    /// 坐标与速度的转换
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="r"></param>
    public void TurnWheelSpeed(float x, float y, int r)
    {
        float a = (2 * y - x - Mathf.Abs(x)) / (2 * r);
        if (a < 0.1f && a > -0.1f)
        {
            Speedvalue = 0;
            return;
        }
        int zf = a > 0 ? 1 : -1;
        float aa = Mathf.Abs(a);
        if (aa < 0.3f)
            Speedvalue = 1 * zf;
        else if (aa < 0.5f)
            Speedvalue = 2 * zf;
        else if (aa < 0.7f)
            Speedvalue = 3 * zf;
        else if (aa < 0.9f)
            Speedvalue = 4 * zf;
        else
            Speedvalue = 5 * zf;
    }

    /// <summary>
    /// 发送命令通知硬件
    /// </summary>
    public void WheelTurn()
    {
        changeOver = true;
        if (preWheelData.turnSpeed != wheelData.turnSpeed || preWheelData.turnDirection != wheelData.turnDirection) //数据不一样时发送
        {
            if (sliderData.motionType == CtrlMotionType.servo)
            {
                RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(sliderData.servoID, wheelData);
            } else if (sliderData.motionType == CtrlMotionType.motor)
            {
                SendMotorData data = new SendMotorData();
                data.id = sliderData.servoID;
                data.direction = wheelData.turnDirection;
                data.speed = (short)wheelData.turnSpeed;
                data.time = 65535;
                if (wheelData.turnDirection == TurnDirection.turnStop)
                {
                    RobotManager.GetInst().GetCurrentRobot().StopMotor(sliderData.servoID);
                } else
                {
                    RobotManager.GetInst().GetCurrentRobot().PlayMotor(data);
                }
            }
            preWheelData = wheelData;
        }
    }
}

