using Game.Scene;
using LitJson;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class GuideData
{
    public string guideSection;
    public SceneType unitySceneType;
    public string clientSceneType;
    public TriggerGuideType triggerGuideType;
    public string triggerEvent;
    public float delayTime = 0;
    public UIWidget.Pivot robotPivot;
    public Vector2 robotMargin;
    public Vector3 robotLocalEulerAngles;
    public string textKey;
    public Vector3 dialogOffsetPosition;
    public Vector3 dialogLocalEulerAngles;
    public List<DepthChangeData> changeList;

    public GuideData()
    {

    }

    public GuideData(XmlElement xe)
    {
        guideSection = xe.GetAttribute("guideSection");
        if (!string.IsNullOrEmpty(guideSection))
        {
            string[] ary = guideSection.Split(',');
            if (ary.Length > 0)
            {
                if (Enum.IsDefined(typeof(SceneType), ary[0]))
                {
                    unitySceneType = (SceneType)Enum.Parse(typeof(SceneType), ary[0]);
                } else
                {
                    clientSceneType = ary[0];
                }
            }
            if (ary.Length > 1)
            {
                triggerGuideType = TriggerGuideType.Trigger_Event;
                triggerEvent = ary[1];

            } else
            {
                triggerGuideType = TriggerGuideType.Trigger_Auto;
            }
        }
        string pivot = xe.GetAttribute("robotPivot");
        if (Enum.IsDefined(typeof(UIWidget.Pivot), pivot))
        {
            robotPivot = (UIWidget.Pivot)Enum.Parse(typeof(UIWidget.Pivot), pivot);
        }
        string delay = xe.GetAttribute("delayTime");
        if (!string.IsNullOrEmpty(delay) && PublicFunction.IsFloat(delay))
        {
            delayTime = float.Parse(delay);
        }
        robotMargin = PublicFunction.TryParseVector2(xe.GetAttribute("robotMargin"));
        robotLocalEulerAngles = PublicFunction.TryParseVector3(xe.GetAttribute("robotLocalEulerAngles"));
        textKey = xe.GetAttribute("textKey");
        dialogOffsetPosition = PublicFunction.TryParseVector3(xe.GetAttribute("dialogOffsetPosition"));
        dialogLocalEulerAngles = PublicFunction.TryParseVector3(xe.GetAttribute("dialogLocalEulerAngles"));
        string changeListStr = xe.GetAttribute("changeList");
        if (!string.IsNullOrEmpty(changeListStr))
        {
            changeList = new List<DepthChangeData>();
            JsonData data = new JsonData(Json.Deserialize(changeListStr.Replace("&quot;", "\"")));
            if (null != data)
            {
                for (int i = 0, imax = data.Count; i < imax; ++i)
                {
                    DepthChangeData depthData = new DepthChangeData((Dictionary<string, object>)data[i].Dictionary);
                    changeList.Add(depthData);
                }
            }
        }
    }


    public bool EqualsChangeList(GuideData data)
    {
        if (this.changeList == data.changeList)
        {
            return true;
        }
        if (this.changeList == null && data.changeList != null)
        {
            return false;
        }
        if (this.changeList != null && data.changeList == null)
        {
            return false;
        }
        if (this.changeList.Count != data.changeList.Count)
        {
            return false;
        }
        for (int i = 0, imax = this.changeList.Count; i < imax; ++i)
        {
            if (!this.changeList[i].EqualsData(data.changeList[i]))
            {
                return false;
            }
        }
        return true;
    }


    public override string ToString()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["guideSection"] = guideSection;
        dict["unitySceneType"] = unitySceneType;
        dict["clientSceneType"] = clientSceneType;
        dict["triggerGuideType"] = triggerGuideType;
        dict["triggerEvent"] = triggerEvent;
        dict["delayTime"] = delayTime;
        dict["robotPivot"] = robotPivot;
        dict["robotMargin"] = robotMargin;
        dict["robotLocalEulerAngles"] = robotLocalEulerAngles;
        dict["textKey"] = textKey;
        dict["dialogOffsetPosition"] = dialogOffsetPosition;
        dict["dialogLocalEulerAngles"] = dialogLocalEulerAngles;
        if (null != changeList)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            for (int i = 0, imax = changeList.Count; i < imax; ++i)
            {
                list.Add(changeList[i].ToDict());
            }
            dict["changeList"] = list;
        }
        return Json.Serialize(dict);
    }

    public XmlElement ToXmlElement(XmlDocument doc)
    {
        XmlElement element = doc.CreateElement("Guide");
        if (string.IsNullOrEmpty(guideSection))
        {
            if (unitySceneType != SceneType.EmptyScene && unitySceneType != SceneType.StartScene)
            {
                guideSection = unitySceneType.ToString();
            } else
            {
                guideSection = clientSceneType;
            }
            if (triggerGuideType == TriggerGuideType.Trigger_Event)
            {
                guideSection += "," + triggerEvent;
            }
        } else
        {
            element.SetAttribute("guideSection", guideSection);
        }
        //element.SetAttribute("unitySceneType", unitySceneType.ToString());
        //element.SetAttribute("clientSceneType", clientSceneType);
        //element.SetAttribute("triggerGuideType", triggerGuideType.ToString());
        //element.SetAttribute("triggerEvent", triggerEvent);
        element.SetAttribute("delayTime", delayTime.ToString());
        element.SetAttribute("robotPivot", robotPivot.ToString());
        element.SetAttribute("robotMargin", robotMargin.ToString());
        element.SetAttribute("robotLocalEulerAngles", robotLocalEulerAngles.ToString());
        element.SetAttribute("textKey", textKey);
        element.SetAttribute("dialogOffsetPosition", dialogOffsetPosition.ToString());
        element.SetAttribute("dialogLocalEulerAngles", dialogLocalEulerAngles.ToString());
        if (null != changeList)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            for (int i = 0, imax = changeList.Count; i < imax; ++i)
            {
                list.Add(changeList[i].ToDict());
            }
            element.SetAttribute("changeList", Json.Serialize(list));
        }
        return element;
    }

    public GuideData Copy()
    {
        GuideData data = new GuideData();
        data.guideSection = this.guideSection;
        data.unitySceneType = this.unitySceneType;
        data.clientSceneType = this.clientSceneType;
        data.triggerGuideType = this.triggerGuideType;
        data.triggerEvent = this.triggerEvent;
        data.delayTime = this.delayTime;
        data.robotPivot = this.robotPivot;
        data.robotMargin = this.robotMargin;
        data.robotLocalEulerAngles = this.robotLocalEulerAngles;
        data.textKey = this.textKey;
        data.dialogOffsetPosition = this.dialogOffsetPosition;
        data.dialogLocalEulerAngles = this.dialogLocalEulerAngles;
        data.changeList = this.changeList;
        return data;
    }
}

public enum DepthChangeType : byte
{
    Change_Widget_Depth = 0,//
    Change_Camera_Depth,
    Change_Panel_Depth,
}

public enum FindTargetWay : byte
{
    Find_Abs_Path = 0,//绝对路径
    Find_Child,
}

public enum TriggerGuideType : byte
{
    Trigger_Auto = 0,//自动触发
    Trigger_Event,//触发了某事件再触发
}

public enum GuideTriggerEvent : byte
{
    None = 0,
    closeWaitingPage,
    Action_CloseWaitingPage,
    Program_CloseWaitingPage,
}

public class DepthChangeData
{
    public DepthChangeType changeType;
    public FindTargetWay findTargetWay;
    public string targetPath;
    public int childIndex;
    public int depth;

    public DepthChangeData()
    {

    }

    public bool EqualsData(DepthChangeData data)
    {
        if (this.changeType != data.changeType)
        {
            return false;
        }
        if (this.findTargetWay != data.findTargetWay)
        {
            return false;
        }
        if (!string.Equals(this.targetPath, data.targetPath))
        {
            return false;
        }
        if (this.childIndex != data.childIndex)
        {
            return false;
        }
        return true;
    }

    public DepthChangeData(Dictionary<string, object> dict)
    {
        if (null != dict)
        {
            if (dict.ContainsKey("changeType") && Enum.IsDefined(typeof(DepthChangeType), dict["changeType"]))
            {
                changeType = (DepthChangeType)Enum.Parse(typeof(DepthChangeType), dict["changeType"].ToString());
            }
            if (dict.ContainsKey("findTargetWay") && Enum.IsDefined(typeof(FindTargetWay), dict["findTargetWay"]))
            {
                findTargetWay = (FindTargetWay)Enum.Parse(typeof(FindTargetWay), dict["findTargetWay"].ToString());
            }
            if (dict.ContainsKey("targetPath"))
            {
                targetPath = dict["targetPath"].ToString();
            }
            if (dict.ContainsKey("childIndex") && PublicFunction.IsInteger(dict["childIndex"].ToString()))
            {
                childIndex = int.Parse(dict["childIndex"].ToString());
            }
            if (dict.ContainsKey("depth") && PublicFunction.IsInteger(dict["depth"].ToString()))
            {
                depth = int.Parse(dict["depth"].ToString());
            }
        }
    }

    public Dictionary<string, object> ToDict()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["changeType"] = changeType;
        dict["findTargetWay"] = findTargetWay;
        dict["targetPath"] = targetPath;
        dict["childIndex"] = childIndex;
        dict["depth"] = depth;
        return dict;
    }
}

