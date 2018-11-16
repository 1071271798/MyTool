//----------------------------------------------
//            积木2: xiongsonglin
//            轮模式功能
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;
using Game;
using Game.Event;

public class WheelModelWindow : BasePopWin {
    
    private string ConfirmBtn;
    private string CancelBtn;
    private UIToggle byClock;
    private UIToggle byDisclock;
    private UISlider speedBar;
    private WheelData CurData;

    /// <summary>
    /// 已是轮滑模式的构造
    /// </summary>
    /*public WheelModelWindow(WheelData wheelData)
    {
        mUIResPath = "Prefab/UI/WheelModelSetting";
        CurData = wheelData;
        len = speedArray[speedArray.Length - 1] - speedArray[0];
    }*/

    /// <summary>
    /// 非轮滑模式的构造
    /// </summary>
    /// <param name="ID"></param>
    public WheelModelWindow(int ID)
    {
        mUIResPath = "Prefab/UI/WheelModelSetting";
        CurData = new WheelData();
        CurData.wheelID = ID;
        CurData.direction = TurnDirection.turnByClock;
        CurData.velocity = speedArray[0];
        len = speedArray[speedArray.Length - 1] - speedArray[0];
    }

    /// <summary>
    /// 初始化的时候调用，只调用一次
    /// </summary>
    protected override void AddEvent()
    {
        base.AddEvent();
        byClock = GameHelper.FindChildComponent<UIToggle>(mTrans, "back/Direction/clockWise");
        byDisclock = GameHelper.FindChildComponent<UIToggle>(mTrans, "back/Direction/nishizhen");
        speedBar = GameHelper.FindChildComponent<UISlider>(mTrans, "back/Velocity/speedSlider");
        if (speedBar != null)
        {
            EventDelegate.Add(speedBar.onChange, ShowSpeed);
            speedBar.onDragFinished += GetFinalSpeed;
        }
      //  EventDelegate.Add(speedBar.onDragFinished, GetFinalSpeed);
        InitWindow(CurData);
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj == null)
            return;
        string btnName = obj.name;
        if (btnName == "cancelBtn")
        {
            DoCancel();
        }
        else if (btnName == "confrimBtn")
        {
            DoConfrim();
        }
        else if(btnName == "clockWise")
        {
            UIToggle tog = obj.GetComponent<UIToggle>();
            if (tog != null)
            {
                if (tog.value)
                {
                    GetDirection(TurnDirection.turnByClock);
                }
            }
        }
        else if (btnName == "nishizhen")
        {
            UIToggle tog = obj.GetComponent<UIToggle>();
            if (tog != null)
            {
                if (tog.value)
                {
                    GetDirection(TurnDirection.turnByDisclock);
                }
            }
        }
    }

    void DoCancel()
    {
        OnClose();
    }

    void DoConfrim()
    {
        EventMgr.Inst.Fire(EventID.OpenWheelModel, new EventArg(CurData));
        OnClose();
    }

    void GetDirection(TurnDirection dir)
    {
        CurData.direction = dir;
    }

    int[] speedArray = new int[]{0x0080, 0x00EA, 0x0154, 0x01BE, 0x0228, 0x0292};
    int len;
    void ShowSpeed()
    {
        if (speedBar != null)
        {
            int speed = (int)(speedBar.value * len) + speedArray[0];
            speedBar.GetComponentInChildren<UILabel>().text = speed.ToString();
        }
    }
    void GetFinalSpeed()
    {
        if (null != speedBar)
        {
            int speed = (int)(speedBar.value * len) + speedArray[0];
            for (int i = 0; i < speedArray.Length-1; i++)
            {
                if (speedArray[i] <= speed && speed < speedArray[i + 1])
                {
                    int a = speed - speedArray[i];
                    int b = speedArray[i + 1] - speed;
                    if (a >= b) { speed = speedArray[i + 1]; }
                    else speed = speedArray[i];
                    
                }
            }
            CurData.velocity = speed;
            Debuger.Log("speed = "+speed);
            speedBar.value = ((float)(speed - speedArray[0])) / len;
            speedBar.GetComponentInChildren<UILabel>().text = speed.ToString();
        }
    }

    protected override void Close()
    {
        if (speedBar != null)
        {
            EventDelegate.Remove(speedBar.onChange, ShowSpeed);
            speedBar.onDragFinished -= GetFinalSpeed;
            //EventDelegate.Remove(speedBar.onDragFinished, GetFinalSpeed);
        }
        base.Close();
    }

    void InitWindow(WheelData data)
    {
        if (data.direction == TurnDirection.turnByClock)
        {
            byClock.GetComponentInChildren<UIToggle>().value = true;
        }
        else if(data.direction == TurnDirection.turnByDisclock)
        {
            byDisclock.GetComponentInChildren<UIToggle>().value = true;
        }
        speedBar.value = ((float)(data.velocity - speedArray[0])) / len;
        speedBar.GetComponentInChildren<UILabel>().text = data.velocity.ToString();
    }
}

public struct WheelData
{
    public int wheelID;
    public bool isOpen;
    public int velocity;
    public int maxVelocity;
    public TurnDirection direction;
}
public enum TurnDirection : byte
{
    turnStop = 0x00,
    turnByClock = 0x01,
    turnByDisclock = 0x02 
}
