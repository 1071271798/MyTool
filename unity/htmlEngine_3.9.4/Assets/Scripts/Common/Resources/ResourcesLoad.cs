using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourcesLoad : SingletonBehaviour<ResourcesLoad>
{
    public delegate void LoadCompleted(params object[] arg);
    private Dictionary<LoadResType, Dictionary<string, ResData>> mResDict;

    public ResourcesLoad()
    {
        mResDict = new Dictionary<LoadResType, Dictionary<string, ResData>>();
    }


    public void Load(string path, LoadResType type, LoadCompleted callback)
    {
        if (!CheckLoadAsset(path, type, callback))
        {
            if (type == LoadResType.Load_Res_Texture)
            {
                SingletonBehaviour<MultiThreadLoadFiles>.GetInst().LoadFile(path, delegate (byte[] bytes)
                {
                    LoadTextureCompleted(path, bytes);
                });
            } else if (type == LoadResType.Load_Single_Asset_Bundle || type == LoadResType.Load_Prefab_Asset_Bundle)
            {
                SingletonBehaviour<MultiThreadLoadFiles>.GetInst().LoadFile(path, delegate (byte[] bytes)
                {
                    LoadAssetBundleCompleted(type, path, bytes);
                });
            }
            else
            {
                mResDict[type][path].CompletedCallback(Resources.Load(path));
            }
        }
    }

    public void LoadSingleThread(string path, LoadResType type, LoadCompleted callback)
    {
        if (!CheckLoadAsset(path, type, callback))
        {
            if (type == LoadResType.Load_Res_Texture)
            {
                byte[] bytes = LoadResByIO(path, callback);
                LoadTextureCompleted(path, bytes);
            } else if (type == LoadResType.Load_Single_Asset_Bundle || type == LoadResType.Load_Prefab_Asset_Bundle)
            {
                byte[] bytes = LoadResByIO(path, callback);
                LoadAssetBundleCompleted(type, path, bytes);
            }
            else
            {
                mResDict[type][path].CompletedCallback(Resources.Load(path));
            }
        }
    }

    public void WWWLoad(string path, LoadResType type, LoadCompleted callback)
    {
        if (!CheckLoadAsset(path, type, callback))
        {
            if (type == LoadResType.Load_Res_Texture)
            {
                StartCoroutine(WWWLoadTexture(path, callback));
            } else if (type == LoadResType.Load_Single_Asset_Bundle || type == LoadResType.Load_Prefab_Asset_Bundle)
            {
                StartCoroutine(WWWLoadAssetBundle(type, path, callback));
            }
            else
            {
                mResDict[type][path].CompletedCallback(Resources.Load(path));
            }
        }
    }

    public void UnLoad(LoadResType type)
    {
        try
        {
            if (mResDict.ContainsKey(type))
            {
                if (type != LoadResType.Load_Single_Asset_Bundle)
                {
                    foreach (var kvp in mResDict[type])
                    {
                        if (null != kvp.Value.res)
                        {
                            foreach (var asset in kvp.Value.res)
                            {
                                if (null != asset)
                                {
                                    Object.Destroy((UnityEngine.Object)asset);
                                }
                            }
                        }
                    }
                }

                mResDict.Remove(type);
                Resources.UnloadUnusedAssets();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public void UnLoad(LoadResType type, string path)
    {
        if (mResDict.ContainsKey(type))
        {
            mResDict[type].Remove(path);
            Resources.UnloadUnusedAssets();
        }
    }


    bool CheckLoadAsset(string path, LoadResType type, LoadCompleted callback)
    {
        if (mResDict.ContainsKey(type) && mResDict[type].ContainsKey(path))
        {
            if (mResDict[type][path].loadState == LoadState.LoadCompleted)
            {
                if (null != callback)
                {
                    callback(mResDict[type][path].res);
                }
            }
            else
            {
                mResDict[type][path].AddCallback(callback);
            }
            return true;
        }
        else
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
            mResDict[type][path].AddCallback(callback);
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    IEnumerator WWWLoadTexture(string path, LoadCompleted callback)
    {
        WWW www = new WWW("file:///" + path);
        yield return www;
        byte[] bytes = null;
        if (www.error == null && null != www.bytes)
        {
            bytes = www.bytes;
        }
        www.Dispose();
        LoadTextureCompleted(path, bytes);
    }

    IEnumerator WWWLoadAssetBundle(LoadResType type, string path, LoadCompleted callback)
    {
        WWW www = new WWW("file:///" + path);
        yield return www;
        byte[] bytes = null;
        if (www.error == null && null != www.bytes)
        {
            bytes = www.bytes;
        }
        www.Dispose();
        LoadAssetBundleCompleted(type, path, bytes);
    }

    /*IEnumerator LoadAssetsOutside(string path, LoadCompleted callback)
    {
        WWW www = new WWW("file:///" + path);
        yield return www;
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        if (www.error == null && null != www.bytes)
        {
            www.LoadImageIntoTexture(tex);
        }
        www.Dispose();
        LoadResType type = LoadResType.Load_Res_Texture;
        if (mResDict.ContainsKey(type) && mResDict[type].ContainsKey(path))
        {
            mResDict[type][path].CompletedCallback(tex);
        }
    }*/

    void LoadTextureCompleted(string path, byte[] bytes)
    {
        Texture2D tex = null;
        if (null != bytes)
        {
            try
            {
                //创建Texture
                tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.LoadImage(bytes);
            }
            catch (System.Exception ex)
            {
                Debug.Log("实例化图片出错:" + ex.ToString());
            }
        }
        LoadResType type = LoadResType.Load_Res_Texture;
        if (mResDict.ContainsKey(type) && mResDict[type].ContainsKey(path))
        {
            mResDict[type][path].CompletedCallback(tex);
        }
    }

    void LoadAssetBundleCompleted(LoadResType type, string path, byte[] bytes)
    {
        AssetBundle assetBundle = AssetBundle.LoadFromMemory(bytes);
        if (mResDict.ContainsKey(type) && mResDict[type].ContainsKey(path))
        {
            string[] assets = assetBundle.GetAllAssetNames();
            object[] objs = null;
            if (null != assets)
            {
                objs = new object[assets.Length];
                for (int i = 0, imax = assets.Length; i < imax; ++i)
                {
                    //Debug.Log(assets[i]);
                    Object loadObj = assetBundle.LoadAsset(assets[i]);
                    if (type == LoadResType.Load_Single_Asset_Bundle)
                    {
                        objs[i] = loadObj;//Object.Instantiate(loadObj);
                    } else
                    {
                        if (null != loadObj)
                        {
                            if (loadObj is GameObject)
                            {
                                objs[i] = Object.Instantiate(loadObj);
                            } else
                            {
                                objs[i] = loadObj;
                            }
                        }
                    }

                }
            }
            mResDict[type][path].CompletedCallback(objs);
        }
        if (type == LoadResType.Load_Single_Asset_Bundle)
        {
            assetBundle.Unload(false);
        }
        else
        {
            assetBundle.Unload(false);
        }
    }

    byte[] LoadResByIO(string path, LoadCompleted callback)
    {
        string loadPath = path;
        FileStream fileStream = null;
        byte[] bytes = null;
        try
        {
            //创建文件读取流
            fileStream = new FileStream(loadPath, FileMode.Open, FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            //创建文件长度缓冲区
            bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            //释放文件读取流
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;
        }
        catch (System.Exception ex)
        {
            if (null != fileStream)
            {
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;
            }
            Debug.Log("ResourcesLoad-LoadTexByIO:FileStream error:" + ex.ToString());
        }
        return bytes;
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
        public LoadResType type;
        public object[] res;
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

        public void CompletedCallback(params object[] res)
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
                        Debug.LogError("ResourcesLoad-CompletedCallback error:" + ex.ToString());
                    }
                }
                eventList.Clear();
                eventList = null;
            }
        }

    }

}

public enum LoadResType : byte
{
    Load_Res_Texture = 0,
    Load_Prefab_Asset_Bundle,
    Load_Single_Asset_Bundle,//加载图片，动画文件等无引用的可直接使用的单个资源
}