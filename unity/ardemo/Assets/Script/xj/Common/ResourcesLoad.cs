using Game.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesLoad : SingletonBehaviour<ResourcesLoad>
{
    public delegate void LoadCompleted(object arg);
    private Dictionary<Type, Dictionary<string, ResData>> mResDict;

    public ResourcesLoad()
    {
        mResDict = new Dictionary<Type, Dictionary<string, ResData>>();
    }


    public void Load(string path, Type type, LoadCompleted callback)
    {
        if (mResDict.ContainsKey(type) && mResDict[type].ContainsKey(path))
        {
            if (mResDict[type][path].loadState == LoadState.LoadCompleted)
            {
                if (null != callback)
                {
                    callback(mResDict[type][path].res);
                }
            } else
            {
                mResDict[type][path].AddCallback(callback);
            }
            
        } else
        {
            if (!mResDict.ContainsKey(type))
            {
                Dictionary<string, ResData> dict = new Dictionary<string, ResData>();
                mResDict[type] = dict;
            }
            ResData resData = new ResData();
            resData.type = type;
            resData.loadState = LoadState.Loding;
            mResDict[type][path] = resData;
            if (type == typeof(Texture))
            {
                StartCoroutine(LoadTex(path, callback));
            } else
            {
                resData.res = Resources.Load(path);
                resData.loadState = LoadState.LoadCompleted;
                if (null != callback)
                {
                    callback(resData.res);
                }
            }
        }
    }

    public void UnLoad(Type type)
    {
        try
        {
            if (mResDict.ContainsKey(type))
            {
                foreach (var kvp in mResDict[type])
                {
                    if (null != kvp.Value.res)
                    {
                        GameObject.Destroy((UnityEngine.Object)kvp.Value.res);
                    }
                }
                mResDict.Remove(type);
                Resources.UnloadUnusedAssets();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public void UnLoad(Type type, string path)
    {
        if (mResDict.ContainsKey(type))
        {
            mResDict[type].Remove(path);
            Resources.UnloadUnusedAssets();
        }
    }


    IEnumerator LoadTex(string path, LoadCompleted callback)
    {
        WWW www = new WWW(path);
        yield return www;
        Texture2D tex = null;
        if (www.isDone && null == www.error)
        {
            try
            {
                tex = www.texture;
            }
            catch (System.Exception ex)
            {
                Debuger.Log(ex.ToString());
            }
        }
        else
        {
            Debuger.Log("not done " + path);
        }
        Type type = typeof(Texture);
        if (mResDict.ContainsKey(type) && mResDict[type].ContainsKey(path))
        {
            mResDict[type][path].CompletedCallback(tex);
        }
        try
        {
            if (null != callback)
            {
                callback(tex);
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "ResourcesLoad-LoadTex:callback error:" + ex.ToString());
        }
        
        www.Dispose();
    }

    public enum LoadState : byte
    {
        None = 0,
        Loding,
        LoadCompleted,
    }

    public class ResData
    {
        public LoadState loadState;
        public Type type;
        public object res;
        private List<LoadCompleted> eventList;

        public ResData()
        {

        }

        public void AddCallback(LoadCompleted callback)
        {
            
            if (null != callback)
            {
                if (null == eventList)
                {
                    eventList = new List<LoadCompleted>();
                }
                eventList.Add(callback);
            }
        }

        public void DelCallback(LoadCompleted callback)
        {
            if (null != eventList)
            {
                eventList.Remove(callback);
            }
        }

        public void CompletedCallback(object res)
        {
            loadState = LoadState.LoadCompleted;
            this.res = res;
            if (null != eventList)
            {
                for (int i = 0, imax = eventList.Count; i < imax; ++i)
                {
                    try
                    {
                        eventList[i](res);
                    }
                    catch (System.Exception ex)
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "ResourcesLoad-CompletedCallback error:" + ex.ToString());
                    }
                }
                eventList.Clear();
                eventList = null;
            }
        }

    }
}
