using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ModelDjData.cs
/// Description:模型的舵机数据
/// Time:2015/6/26 9:22:53
/// </summary>
public class ModelDjData
{
    Dictionary<byte, DuoJiData> mDjData = null;

    byte mMinId;
    public byte MinId
    {
        get { return mMinId; }
    }
    byte mMaxId;
    public byte MaxId
    {
        get { return mMaxId; }
    }
    public ModelDjData()
    {
        mDjData = new Dictionary<byte, DuoJiData>();
        mMinId = byte.MaxValue;
        mMaxId = byte.MinValue;
    }

    
    public void UpdateData(DuoJiData data)
    {
        if (null != data)
        {
            mDjData[data.id] = data;
            if (data.id < mMinId)
            {
                mMinId = data.id;
            }
            if (data.id > mMaxId)
            {
                mMaxId = data.id;
            }
        }
    }

    public bool RemoveData(byte id)
    {
        if (mDjData.ContainsKey(id))
        {
            mDjData.Remove(id);
            if (id == mMinId)
            {
                mMinId = byte.MaxValue;
                foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
                {
                    if (kvp.Key < mMinId)
                    {
                        mMinId = kvp.Key;
                    }
                }
            }
            else if (id == mMaxId)
            {
                mMaxId = byte.MinValue;
                foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
                {
                    if (kvp.Key > mMaxId)
                    {
                        mMaxId = kvp.Key;
                    }
                }
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// 更新舵机数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    public void UpdateData(int id, int rota)
    {
        byte btId = (byte)id;
        if (mDjData.ContainsKey(btId))
        {
            mDjData[btId].CloseTurnModel();
            mDjData[btId].rota = CorrectionRota(rota);
            if (btId < mMinId)
            {
                mMinId = btId;
            }
            if (btId > mMaxId)
            {
                mMaxId = btId;
            }
        }
    }
    /// <summary>
    /// 更新舵机模式
    /// </summary>
    /// <param name="id"></param>
    /// <param name="modelType"></param>
    public void UpdateServoModel(byte id, ServoModel modelType)
    {
        if (mDjData.ContainsKey(id))
        {
            mDjData[id].modelType = modelType;
        }
    }

    public void UpdateTurn(int id, TurnDirection dir, int speed)
    {
        byte btId = (byte)id;
        if (mDjData.ContainsKey(btId))
        {
            mDjData[btId].isTurn = true;
            mDjData[btId].turnData.turnDirection = dir;
            mDjData[btId].turnData.turnSpeed = (ushort)speed;
        }
    }
    /// <summary>
    /// 判断舵机是否是转动模式
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsTurn(int id)
    {
        byte btId = (byte)id;
        if (mDjData.ContainsKey(btId))
        {
            return mDjData[btId].isTurn;
        }
        return false;
    }
    /// <summary>
    /// 关闭转动模式
    /// </summary>
    /// <param name="id"></param>
    public void CloseTurn(int id)
    {
        byte btid = (byte)id;
        if (mDjData.ContainsKey(btid))
        {
            mDjData[btid].CloseTurnModel();
        }
    }
    /// <summary>
    /// 关闭所有轮转模式
    /// </summary>
    public void CloseAllTurn()
    {
        foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
        {
            kvp.Value.CloseTurnModel();
        }
    }
    /// <summary>
    /// 重置上次滚轮数据
    /// </summary>
    public void ResetAllLastTurn()
    {
        foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
        {
            kvp.Value.ResetLastTurn();
        }
    }

    public bool HaveServoTurn()
    {
        foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
        {
            if (kvp.Value.modelType == ServoModel.Servo_Model_Turn && kvp.Value.turnData.turnDirection != TurnDirection.turnStop)
            {
                return true;
            }
        }
        return false;
    }

    public WheelData GetDjTurnData(int id)
    {
        byte btId = (byte)id;
        WheelData data = new WheelData();
        data.wheelID = btId;
        if (mDjData.ContainsKey(btId))
        {
            data.isOpen = mDjData[btId].isTurn;
            data.direction = mDjData[btId].turnData.turnDirection;
            data.velocity = mDjData[btId].turnData.turnSpeed;
        }
        else
        {
            data.isOpen = false;
            data.direction = TurnDirection.turnStop;
            data.velocity = 0;
        }
        return data;
    }
    /// <summary>
    /// 舵机是否存在
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsExist(byte id)
    {
        if (mDjData.ContainsKey(id))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 获取舵机数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public DuoJiData GetDjData(byte id)
    {
        if (mDjData.ContainsKey(id))
        {
            return mDjData[id];
        }
        return null;
    }
    /// <summary>
    /// 获取所有舵机数据
    /// </summary>
    /// <returns></returns>
    public Dictionary<byte, DuoJiData> GetAllData()
    {
        return mDjData;
    }
    /// <summary>
    /// 获取舵机id列表
    /// </summary>
    /// <returns></returns>
    public List<byte> GetIDList()
    {
        List<byte> list = new List<byte>();
        foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
        {
            list.Add(kvp.Key);
        }
        list.Sort();
        return list;
    }
    /// <summary>
    /// 获取角模式的所有舵机
    /// </summary>
    /// <returns></returns>
    public List<byte> GetAngleList()
    {
        List<byte> list = new List<byte>();
        foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
        {
            if (kvp.Value.modelType == ServoModel.Servo_Model_Angle)
            {
                list.Add(kvp.Key);
            }
        }
        list.Sort();
        return list;
    }
    /// <summary>
    /// 获取轮模式的所有舵机
    /// </summary>
    /// <returns></returns>
    public List<byte> GetTurnList()
    {
        List<byte> list = new List<byte>();
        foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
        {
            if (kvp.Value.modelType == ServoModel.Servo_Model_Turn)
            {
                list.Add(kvp.Key);
            }
        }
        list.Sort();
        return list;
    }
    /// <summary>
    /// 校正角度
    /// </summary>
    /// <param name="rota"></param>
    /// <returns></returns>
    public short CorrectionRota(int rota)
    {
        int flag = rota > 0 ? 1 : -1;
        ushort num = (ushort)Mathf.Abs(rota);
        if (num < PublicFunction.DuoJi_Min_Rota)
        {
            num = PublicFunction.DuoJi_Min_Rota;
        }
        else if (num > PublicFunction.DuoJi_Max_Rota)
        {
            num = PublicFunction.DuoJi_Max_Rota;
        }
        return (short)(flag * num);
    }
    /// <summary>
    ///  获取舵机的数量
    /// </summary>
    public int Count
    {
        get { return mDjData.Count; }
    }

    /// <summary>
    /// 数据复位
    /// </summary>
    public void Reset()
    {
        foreach (KeyValuePair<byte, DuoJiData> kvp in mDjData)
        {
            kvp.Value.CloseTurnModel();
            kvp.Value.rota = kvp.Value.startRota;
        }
    }

    public void CleanUp()
    {
        mDjData.Clear();
    }
}


public class DuoJiData
{
    /// <summary>
    /// 舵机编号
    /// </summary>
    public byte id;
    /// <summary>
    /// 当前旋转角度
    /// </summary>
    public short rota;
    /// <summary>
    /// 目标角度
    /// </summary>
    public short targetRota;
    /// <summary>
    /// 最小角度
    /// </summary>
    public short minRota;
    /// <summary>
    /// 最大角度
    /// </summary>
    public short maxRota;
    /// <summary>
    /// 初始角度
    /// </summary>
    public short startRota;
    /// <summary>
    /// 记录实物机器人上次的角度
    /// </summary>
    public short lastRota;
    /// <summary>
    /// 是否处于连续转动模式，true表示处于连续转动模式
    /// </summary>
    public bool isTurn;
    /// <summary>
    /// isTurn为true才需要考虑
    /// </summary>
    public TurnData turnData;
    /// <summary>
    /// 上一次发送的连续转动数据
    /// </summary>
    public TurnData lastTurnData;
    /// <summary>
    /// 是否为掉电状态
    /// </summary>
    public bool isPowerDown = true;
    /// <summary>
    /// 舵机模式
    /// </summary>
    public ServoModel modelType = ServoModel.Servo_Model_Angle;
    public DuoJiData()
    {
        startRota = 120;
        isTurn = false;
    }

    public DuoJiData(byte id, short rota)
    {
        this.id         =   id;
        this.rota       =   rota;
        this.startRota  =   120;
        this.lastRota   =   (byte)rota;
        isTurn          =   false;
    }

    public DuoJiData(byte id, short rota, short targetRota, byte lastRota)
    {
        this.id         =   id;
        this.rota       =   rota;
        this.targetRota =   targetRota;
        this.lastRota   =   lastRota;
        this.startRota  =   120;
        isTurn          =   false;
    }
    /// <summary>
    /// 打开旋转模式
    /// </summary>
    /// <param name="turnDir"></param>
    /// <param name="speed"></param>
    public void OpenTurnModel(TurnDirection turnDir, int speed)
    {
        if (TurnDirection.turnStop == turnDir)
        {
            isTurn = false;
            turnData.turnDirection = TurnDirection.turnStop;
            turnData.turnSpeed = 0;
        }
        else
        {
            isTurn = true;
            turnData.turnDirection = turnDir;
            turnData.turnSpeed = (ushort)speed;
        }
    }
    /// <summary>
    /// 关闭旋转模式
    /// </summary>
    public void CloseTurnModel()
    {
        isTurn = false;
        lastTurnData.turnDirection = TurnDirection.turnStop;
        lastTurnData.turnSpeed = 0;
        turnData.turnDirection = TurnDirection.turnStop;
        turnData.turnSpeed = 0;
    }
    /// <summary>
    /// 清除上次滚轮信息
    /// </summary>
    public void ResetLastTurn()
    {
        lastTurnData.turnDirection = TurnDirection.turnStop;
        lastTurnData.turnSpeed = 0;
        turnData.turnDirection = TurnDirection.turnStop;
        turnData.turnSpeed = 0;
    }
    /// <summary>
    /// 是否需要发送连续转动数据
    /// </summary>
    /// <returns></returns>
    public bool NeedSendTurnMsg()
    {
        if (turnData.turnDirection != lastTurnData.turnDirection || turnData.turnSpeed != lastTurnData.turnSpeed)
        {
            return true;
        }
        return false;
    }
}

public struct TurnData
{
    /// <summary>
    /// 转动的模式，如何isTurn为false则不需要考虑
    /// </summary>
    public TurnDirection turnDirection;
    /// <summary>
    /// 转动的速度，多少度每秒
    /// </summary>
    public ushort turnSpeed;
}