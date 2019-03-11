#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

public class RainbowGuideConfig : ScriptableObject
{
    public List<RainbowGuideData> guideList;
    

    private RainbowGuideConfig()
    {

    }
}

[System.Serializable]
public class RainbowGuideData
{
    public int id;
    public int skipId;
    public int nextId;
    public string text;
    public EGameGuideEventID activeEvent;
    public ERainbowGuideTriggerEvent triggerEvent;
    public ERainbowGuideTriggerTime triggerTime;
    public ERainbowGuideStyle style;
    public List<EColorType> color;

    

    public RainbowGuideData()
    {

    }
}