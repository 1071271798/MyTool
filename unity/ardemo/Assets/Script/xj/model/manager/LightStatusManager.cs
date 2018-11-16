using Game;
using Game.Platform;
using System;
using System.Collections.Generic;

public class LightStatusManager : SingletonObject<LightStatusManager>
{
    private Dictionary<TopologyPartType, Dictionary<byte,LightStatus>> mLightDict;
    private Dictionary<TopologyPartType, Queue<List<byte>>> mSendDict;
    private EventDelegate.Callback onAllClose;
    private UInt32[] EmojiShowTime = new UInt32[] { 2550, 4400, 2800, 1720, 2950, 800, 2050, 3400, 3000, 400, 300, 1000, 2400, 2040, 1500, 8400};
    public UInt32[] RgbLightEmojiShowTime = new UInt32[] { 12500, 2500, 4000, 3500, 2500, 7000};
    public LightStatusManager()
    {

    }

    public bool IsLight()
    {
        TopologyPartType[] lights = new TopologyPartType[] { TopologyPartType.Light , TopologyPartType.Ultrasonic, TopologyPartType.RgbLight};
        for (int i = 0, imax = lights.Length; i < imax; ++i)
        {
            if (null != mLightDict && mLightDict.ContainsKey(lights[i]))
            {
                Dictionary<byte, LightStatus> data = mLightDict[lights[i]];
                foreach (var kvp in data)
                {
                    if (kvp.Value.showType != LightShowType.Light_Close)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }


    public void SendCloseLight(Robot robot)
    {
        if (null != mLightDict)
        {
            Dictionary<TopologyPartType, List<byte>> dict = new Dictionary<TopologyPartType, List<byte>>();
            foreach (var kvp in mLightDict)
            {
                foreach (var item in kvp.Value)
                {
                    if (item.Value.showType != LightShowType.Light_Close)
                    {
                        if (!dict.ContainsKey(kvp.Key))
                        {
                            List<byte> list = new List<byte>();
                            dict[kvp.Key] = list;
                        }
                        dict[kvp.Key].Add(item.Key);
                    }
                }
            }
            if (dict.Count > 0)
            {
                foreach (var kvp in dict)
                {
                    if (kvp.Value.Count > 0)
                    {
                        switch (kvp.Key)
                        {
                            case TopologyPartType.Light:
                                {
                                    List<LightShowData> list = new List<LightShowData>();
                                    LightShowData data = new LightShowData();
                                    data.ids.Add(0);
                                    list.Add(data);
                                    robot.SendLight(kvp.Value, list, 0);
                                }
                                break;
                            case TopologyPartType.Ultrasonic:
                                {
                                    SendUltrasonicDataMsg msg = new SendUltrasonicDataMsg();
                                    msg.sensorData.sensorType = TopologyPartData.ConvertToSensorType(TopologyPartType.Ultrasonic);
                                    msg.sensorData.ids = kvp.Value;
                                    msg.rgb = string.Empty;
                                    msg.mode = 0;
                                    msg.time = 0;
                                    robot.SendUltrasonic(msg);
                                }
                                break;
                            case TopologyPartType.RgbLight:
                                {
                                    SendRgbLightDataMsg msg = new SendRgbLightDataMsg();
                                    msg.sensorData.ids = kvp.Value;
                                    msg.rgb = string.Empty;
                                    msg.time = 0;
                                    msg.flash = 0;
                                    msg.controlType = RgbLightControlType.Show_Color;
                                    robot.SendRgbLight_Color(msg);
                                }
                                break;
                        }
                    }
                }
            }    
        }
    }

    public void AddEmojiStatus(TopologyPartType partType, List<byte> ids, byte lightType, UInt16 times)
    {
        UInt32 showTime = 0;
        if (times < 0xffff)
        {
            switch (partType)
            {
                case TopologyPartType.Light:
                    {
                        if (lightType >= 0 && lightType < EmojiShowTime.Length)
                        {
                            showTime = EmojiShowTime[lightType] * times;
                        }
                        else
                        {
                            showTime = (UInt32)1000 * times;
                        }
                    }
                    break;
                case TopologyPartType.RgbLight:
                    {
                        if (lightType >= 0 && lightType < RgbLightEmojiShowTime.Length)
                        {
                            showTime = RgbLightEmojiShowTime[lightType] * times;
                        }
                        else
                        {
                            showTime = (UInt32)1000 * times;
                        }
                    }
                    break;
            }
            
        }
        AddLightShowStatus(partType, ids, LightShowType.Light_Show_Emoji, showTime);
    }
    /// <summary>
    /// 添加灯光状态，showTime=0表示常亮
    /// </summary>
    /// <param name="sensorType"></param>
    /// <param name="ids"></param>
    /// <param name="showType"></param>
    /// <param name="showTime"></param>
    public void AddLightShowStatus(TopologyPartType sensorType, List<byte> ids, LightShowType showType, UInt32 showTime)
    {
        CreateDict();
        if (!mLightDict.ContainsKey(sensorType))
        {
            Dictionary<byte, LightStatus> dict = new Dictionary<byte, LightStatus>();
            mLightDict[sensorType] = dict;
        }
        if (!mSendDict.ContainsKey(sensorType))
        {
            Queue<List<byte>> queue = new Queue<List<byte>>();
            mSendDict[sensorType] = queue;
        }
        mSendDict[sensorType].Enqueue(ids);
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            byte id = ids[i];
            if (!mLightDict[sensorType].ContainsKey(id))
            {
                LightStatus light = new LightStatus();
                mLightDict[sensorType][id] = light;
            }
            mLightDict[sensorType][id].sendShowType = showType;
            mLightDict[sensorType][id].showTime = showTime;
        }
    }

    public void ShowLight(TopologyPartType sensorType, List<byte> ids)
    {
        if (null != mLightDict && mLightDict.ContainsKey(sensorType))
        {
            for (int i = 0, imax = ids.Count; i < imax; ++i)
            {
                if (mLightDict[sensorType].ContainsKey(ids[i]))
                {
                    mLightDict[sensorType][ids[i]].Show(delegate() { CheckAllClose(); });
                }
            }
        }
    }

    public void CloseLight(TopologyPartType sensorType, List<byte> ids)
    {
        if (null != mLightDict && mLightDict.ContainsKey(sensorType))
        {
            for (int i = 0, imax = ids.Count; i < imax; ++i)
            {
                if (mLightDict[sensorType].ContainsKey(ids[i]))
                {
                    mLightDict[sensorType][ids[i]].Close();
                }
            }
            CheckAllClose();
        }
    }

    public void SendOutTime(TopologyPartType sensorType, List<byte> failIds = null)
    {
        List<byte> ids = DequeueSend(sensorType);
        if (null == failIds)
        {
            if (null != ids)
            {
                CloseLight(sensorType, ids);
            }
        } else
        {
            CloseLight(sensorType, failIds);
        }
    }

    public List<byte> DequeueSend(TopologyPartType partType)
    {
        if (null != mSendDict && mSendDict.ContainsKey(partType) && mSendDict[partType].Count > 0)
        {
            return mSendDict[partType].Dequeue();
        }
        return null;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        CloseAll();
        mLightDict = null;
        mSendDict = null;
    }

    private void CloseAll()
    {
        if (null != mLightDict)
        {
            foreach (var kvp in mLightDict)
            {
                foreach (var item in kvp.Value)
                {
                    item.Value.Close();
                }
            }
        }
    }

    private void CheckAllClose()
    {
        bool allClose = true;
        if (null != mLightDict)
        {
            foreach (var kvp in mLightDict)
            {
                foreach (var item in kvp.Value)
                {
                    if (item.Value.showType != LightShowType.Light_Close)
                    {
                        allClose = false;
                    }
                }
            }
        }
        if (allClose)
        {
            if (null != onAllClose)
            {
                onAllClose();
            }
        }
    }

    

    private void CreateDict()
    {
        if (null == mLightDict)
        {
            mLightDict = new Dictionary<TopologyPartType, Dictionary<byte, LightStatus>>();
        }
        if (null == mSendDict)
        {
            mSendDict = new Dictionary<TopologyPartType, Queue<List<byte>>>();
        }
    }

    
}


public class LightStatus
{
    public LightShowType sendShowType;
    public LightShowType showType;
    public UInt32 showTime;
    long changeIndex = -1;

    public LightStatus()
    {
        showType = LightShowType.Light_Close;
        showTime = 0;
    }

    public void Show(EventDelegate.Callback callback)
    {
        showType = sendShowType;
        if (showTime > 0 && showType != LightShowType.Light_Close)
        {
            if (-1 != changeIndex)
            {
                Timer.Cancel(changeIndex);
            }
            changeIndex = Timer.Add((showTime - 50) / 1000.0f, 1, 1, delegate () {
                showType = LightShowType.Light_Close;
                changeIndex = -1;
                if (null != callback)
                {
                    callback();
                }
            });
        } else
        {
            if (null != callback)
            {
                callback();
            }
        }
    }

    public void Close()
    {
        showType = LightShowType.Light_Close;
        if (-1 != changeIndex)
        {
            Timer.Cancel(changeIndex);
        }
        changeIndex = -1;
    }
}


public enum LightShowType : byte
{
    Light_Close = 0,
    Light_Show_Emoji,
    Light_Show_Color,
}

