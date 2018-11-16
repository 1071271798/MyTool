
using Game.Platform;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Game.Scene;
using Game;
using LitJson;

public class GuideManager : SingletonObject<GuideManager>
{
    Dictionary<string, List<GuideData>> mGuideDict = null;
    string mCurrentKey = null;
    int mCurrentIndex = -1;
    public GuideManager()
    {
        //ReadXml();
    }
    public void OpenGuideEdit()
    {
        ActiveGuide(GuideTriggerEvent.None);
        GuideData data = null;
        if (null != mCurrentKey && mCurrentIndex >= 0)
        {
            data = mGuideDict[mCurrentKey][mCurrentIndex];
            data = data.Copy();
        }
        GuideMsg.ShowMsg(data, true);
    }

    public void ActiveGuide(GuideTriggerEvent triggerEvent)
    {
        try
        {
            string key = GetNowKey(triggerEvent);
            if (null == key)
            {
                return;
            }
            if (null != mGuideDict && mGuideDict.ContainsKey(key) && null != mGuideDict[key] && mGuideDict[key].Count > 0 && IsGuide(key, mGuideDict[key][0]))
            {
                if (mGuideDict[key][0].triggerGuideType == TriggerGuideType.Trigger_Event && !string.Equals(mGuideDict[key][0].triggerEvent, triggerEvent.ToString()))
                {//未满足触发条件
                    mCurrentKey = null;
                    mCurrentIndex = -1;
                }
                else
                {
                    mCurrentKey = key;
                    mCurrentIndex = 0;
                }
            }
            else
            {
                mCurrentKey = null;
                mCurrentIndex = -1;
            }
            GuideData data = null;
            if (null != mCurrentKey && mCurrentIndex >= 0)
            {
                data = mGuideDict[mCurrentKey][mCurrentIndex];
                data = data.Copy();
                if (data.delayTime > 0)
                {
                    Timer.Add(data.delayTime, 1, 1, delegate ()
                    {
                        GuideMsg.ShowMsg(data, false);
                    });
                }
                else
                {
                    GuideMsg.ShowMsg(data, false);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public void GuideFinished()
    {
        if (null != mCurrentKey)
        {
            string saveKey = "Guide_" + mCurrentKey.ToString();
            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (mGuideDict.ContainsKey(mCurrentKey) && null != mGuideDict[mCurrentKey] && mGuideDict[mCurrentKey].Count > 0)
            {
                GuideData data = mGuideDict[mCurrentKey][0];
                if (null != data)
                {
                    if (data.unitySceneType == SceneType.Assemble)
                    {
                        dict["moduleType"] = "build";
                    }
                    else if (string.Equals("MainScene,Action_CloseWaitingPage", mCurrentKey))
                    {
                        dict["moduleType"] = "action";
                    } else if (string.Equals("MainScene,Program_CloseWaitingPage", mCurrentKey))
                    {
                        dict["moduleType"] = "program";
                    }
                }
                
            }
            
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.guideFinished, Json.Serialize(dict));
            mCurrentKey = null;
            mCurrentIndex = -1;
        }
    }


    public void SkipGuide()
    {
        if (null != mCurrentKey)
        {
#if UNITY_EDITOR
            string saveKey = "skipGuide";
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
#endif
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.skipGuide, string.Empty);
            mCurrentKey = null;
            mCurrentIndex = -1;
        }
    }
    public void ClearPlayerGuide()
    {
        string[] keys = { "Guide_Assemble,closeWaitingPage", "Guide_MainScene,Action_CloseWaitingPage", "Guide_MainScene,Program_CloseWaitingPage", "skipGuide" };
        for (int i = 0, imax = keys.Length; i < imax; ++i)
        {
            PlayerPrefs.DeleteKey(keys[i]);
        }
        PlayerPrefs.Save();
    }

    public GuideData GetCurrentData()
    {
        if (null != mGuideDict && null != mCurrentKey && mGuideDict.ContainsKey(mCurrentKey) && null != mGuideDict[mCurrentKey])
        {
            if (mCurrentIndex >= 0 && mCurrentIndex < mGuideDict[mCurrentKey].Count)
            {
                return mGuideDict[mCurrentKey][mCurrentIndex];
            }
        }
        return null;
    }


    public GuideData GetNextData()
    {
        if (null != mGuideDict && null != mCurrentKey && mGuideDict.ContainsKey(mCurrentKey) && null != mGuideDict[mCurrentKey])
        {
            if (mCurrentIndex >= 0 && mCurrentIndex + 1 < mGuideDict[mCurrentKey].Count)
            {
                return mGuideDict[mCurrentKey][mCurrentIndex + 1];
            }
        }
        return null;
    }

    public void MoveNextStep()
    {
        if (null != mGuideDict && null != mCurrentKey && mGuideDict.ContainsKey(mCurrentKey) && null != mGuideDict[mCurrentKey])
        {
            if (mCurrentIndex >= 0 && mCurrentIndex + 1 < mGuideDict[mCurrentKey].Count)
            {
                ++mCurrentIndex;
            }
            else
            {
                mCurrentKey = null;
                mCurrentIndex = -1;
            }
        }
    }

    public void MoveLastStep()
    {
        if (null != mGuideDict && null != mCurrentKey && mGuideDict.ContainsKey(mCurrentKey) && null != mGuideDict[mCurrentKey])
        {
            if (mCurrentIndex > 0)
            {
                --mCurrentIndex;
            } else
            {
                mCurrentKey = null;
                mCurrentIndex = -1;
            }
        }
    }

    public void ResetGuide()
    {
        if (null != mGuideDict)
        {
            mCurrentKey = GetNowKey(GuideTriggerEvent.None);
            if (mGuideDict.ContainsKey(mCurrentKey) && null != mGuideDict[mCurrentKey] && mGuideDict[mCurrentKey].Count > 0)
            {
                mCurrentIndex = 0;
            } else
            {
                mCurrentKey = null;
                mCurrentIndex = -1;
            }
        }
    }

    public void AddGuideData(GuideData data)
    {
        string key = data.guideSection;
        if (string.IsNullOrEmpty(key))
        {
            if (data.unitySceneType != SceneType.StartScene && data.unitySceneType != SceneType.EmptyScene)
            {
                key = data.unitySceneType.ToString();
            }
            else if (!string.IsNullOrEmpty(data.clientSceneType))
            {
                key = data.clientSceneType;
            }
        }
        if (key != null)
        {
            if (null == mGuideDict)
            {
                mGuideDict = new Dictionary<string, List<GuideData>>();
            }
            if (!mGuideDict.ContainsKey(key))
            {
                List<GuideData> list = new List<GuideData>();
                mGuideDict[key] = list;
            }
            mGuideDict[key].Add(data);
            mCurrentKey = key;
            mCurrentIndex = mGuideDict[key].Count - 1;
        }
    }

    public bool ModifyGuideData(GuideData data)
    {
        if (GetCurrentData() != null)
        {
            mGuideDict[mCurrentKey][mCurrentIndex] = data;
            return true;
        }
        return false;
    }


    public void RemoveCurrentStepData()
    {
        if (GetCurrentData() != null)
        {
            mGuideDict[mCurrentKey].RemoveAt(mCurrentIndex);
            if (mGuideDict[mCurrentKey].Count > 0)
            {
                --mCurrentIndex;
                if (mCurrentIndex < 0)
                {
                    mCurrentIndex = 0;
                }
            } else
            {
                mCurrentIndex = -1;
                mCurrentKey = null;
            }
        }
    }

    public void RemoveCurrentSceneData()
    {
        if (null != mGuideDict && mGuideDict.ContainsKey(mCurrentKey))
        {
            mGuideDict.Remove(mCurrentKey);
            mCurrentKey = null;
            mCurrentIndex = -1;
        }
    }

    public void SaveGuide()
    {
        SaveXml();
    }

    


    private void ReadXml()
    {
        try
        {
            TextAsset text = Resources.Load<TextAsset>("guideData");
            if (null != text && !string.IsNullOrEmpty(text.text))
            {
                XmlTool.ReadXmlByContent(text.text.Trim(), delegate (XmlElement xe)
                {
                    GuideData data = new GuideData(xe);
                    AddGuideData(data);
                });
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "GuideManager -" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    private void SaveXml()
    {
        try
        {
            string path = Application.dataPath.Replace("\\", "/") + "/Resources/guideData.xml";
            if (null != mGuideDict)
            {
                XmlTool.WriteXml(path, delegate (XmlDocument doc, XmlElement roots)
                {
                    foreach (var kvp in mGuideDict)
                    {
                        for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
                        {
                            roots.AppendChild(kvp.Value[i].ToXmlElement(doc));
                        }
                    }
                    return true;
                }, "Guides");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "GuideManager -" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    private string GetNowKey(GuideTriggerEvent triggerEvent)
    {
        string key = null;
        SceneType sceneType = SceneMgr.GetCurrentSceneType();
        if (sceneType != SceneType.StartScene && sceneType != SceneType.EmptyScene)
        {
            key = sceneType.ToString();
        }
        else if (SceneManager.GetInst().GetCurrentScene() != null)
        {
            key = SceneManager.GetInst().GetCurrentScene().GetType().ToString();
        }
        if (triggerEvent != GuideTriggerEvent.None)
        {
            key += "," + triggerEvent;
        }
        return key;
    }

    private bool IsGuide(string key, GuideData data)
    {
        return false;
#if UNITY_EDITOR
        if (PlayerPrefs.HasKey("skipGuide"))
        {
            return false;
        }
        string saveKey = "Guide_" + key.ToString();
        if (PlayerPrefs.HasKey(saveKey))
            return false;
        return true;
#else
        string moduleType = string.Empty;
        Dictionary<string, object> dict = new Dictionary<string, object>();
        if (data.unitySceneType == SceneType.Assemble)
        {
            if (!PlatformMgr.Instance.IsCourseFlag)
            {
                moduleType = "build";
            }
        }
        else if (string.Equals("MainScene,Action_CloseWaitingPage", key))
        {
            moduleType = "action";
        }
        else if (string.Equals("MainScene,Program_CloseWaitingPage", key))
        {
            moduleType = "program";
        }
        if (!string.IsNullOrEmpty(moduleType))
        {
            dict["moduleType"] = moduleType;
            string state = PlatformMgr.Instance.GetData(PlatformDataType.guideState, Json.Serialize(dict));
            if (string.IsNullOrEmpty(state))
            {
                return false;
            }
            Dictionary<string, object> stateDict = (Dictionary<string, object>)Json.Deserialize(state);
            if (stateDict.ContainsKey("isGuide") && PublicFunction.IsInteger(stateDict["isGuide"].ToString()))
            {
                return int.Parse(stateDict["isGuide"].ToString()) == 1 ? true : false;
            }
        }
        return false;
#endif
    }
}
