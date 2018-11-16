#define USE_GZIP
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Game.Resource;
using System.IO;
using System.Text;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:DefaultModelCopy.cs
/// Description:默认模型复制
/// Time:2015/9/24 16:58:10
/// </summary>
public class DefaultModelCopy
{
    #region 公有属性
    public delegate void CopyFilesFinish(bool result);
    #endregion

    #region 其他属性
    Dictionary<string, string> streamVersion;
    Dictionary<string, string> perVersion;
    string Default_File = "default.cfg";
    int mNeedSaveCount = 0;
    int mSaveSuccessCount = 0;
    int mSaveCount = 0;
    CopyFilesFinish mFinish;
    List<string> defaultActionsList = null;
    #endregion

    #region 公有函数
    public DefaultModelCopy()
    {

    }

    public void CheckDefaultCopy(CopyFilesFinish result)
    {
        ReadDefault(result);
    }
    #endregion

    #region 其他函数
    void ReadDefault(CopyFilesFinish result)
    {
        mFinish = result;
        mSaveCount = 0;
        mSaveSuccessCount = 0;
        mNeedSaveCount = 0;
        LoadPerDefaultVersion();
        SingletonBehaviour<ClientMain>.GetInst().StartCoroutine(LoadDefault());
    }

    void LoadPerDefaultVersion()
    {
        try
        {
            string path = ResourcesEx.persistentDataPath + "/" + Default_File;
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path, Encoding.UTF8);
                perVersion = LoadXml(sr.ReadToEnd().Trim());
                sr.Close();
                sr.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    /// <summary>
    /// 解析配置文件数据
    /// </summary>
    /// <param name="contents"></param>
    /// <returns></returns>
    Dictionary<string, string> LoadXml(string contents)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(contents);
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            XmlNode node = xmlRoot.FirstChild;
            while (null != node)
            {
                if (!(node is XmlElement))
                {
                    continue;
                }
                XmlElement xe = (XmlElement)node;
                string file = xe.GetAttribute("file");
                string md5 = xe.GetAttribute("md5");
                dict[file] = md5;
                node = node.NextSibling;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return dict;
    }

    IEnumerator LoadDefault()
    {

#if UNITY_EDITOR
        WWW www = new WWW("file://" + Application.streamingAssetsPath + "/default.cfg");
#elif UNITY_ANDROID
        WWW www = new WWW(Application.streamingAssetsPath + "/default.cfg");
#else
        WWW www = new WWW("file://" + Application.streamingAssetsPath + "/default.cfg");
#endif
        yield return www;
        if (null != www.error)
        {
            Debuger.Log("LoadDefault:" + www.error);
            LoadDefaultComplete(null);
        }
        else
        {
            LoadDefaultComplete(www.text);
        }
        www.Dispose();
    }
#if USE_GZIP
    void LoadDefaultComplete(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            streamVersion = null;
        }
        else
        {
            streamVersion = LoadXml(text);
        }
        if (null != streamVersion)
        {
            mNeedSaveCount = 0;
            foreach (KeyValuePair<string, string> kvp in streamVersion)
            {
                /*bool defaultFlag = kvp.Key.StartsWith("default/");
                if (defaultFlag && kvp.Key.Contains("/actions/"))
                {//默认动作官方动作
                    string fileName = Path.GetFileNameWithoutExtension(kvp.Key);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (null == defaultActionsList)
                        {
                            defaultActionsList = new List<string>();
                        }
                        defaultActionsList.Add(fileName);
                    }
                }*/
                if (null == perVersion || !perVersion.ContainsKey(kvp.Key) || !perVersion[kvp.Key].Equals(kvp.Value))
                {//需要复制
                    /*if (!ClientMain.Copy_Default_Flag && defaultFlag)
                    {
                        continue;
                    }*/

                    mNeedSaveCount++;

                    //FilesCopy copy = new FilesCopy();
#if UNITY_EDITOR
                    string path = Path.Combine(ResourcesEx.streamingAssetsPath, kvp.Key);
                    GZipHelper.DecompressToDirectory(path, ResourcesEx.persistentDataPath, SaveFilesResult);
#elif UNITY_ANDROID
                    string path = Path.Combine(ResourcesEx.streamingAssetsPath, kvp.Key);
                    GZipHelper.WWWDecompressToDirectory(path, ResourcesEx.persistentDataPath, SaveFilesResult);
#else
					string path = "file://" + Path.Combine(ResourcesEx.streamingAssetsPath, kvp.Key);
                    GZipHelper.WWWDecompressToDirectory(path, ResourcesEx.persistentDataPath, SaveFilesResult);
#endif
                    //copy.CopyFile(path, ResourcesEx.persistentDataPath + "/" + kvp.Key, SaveFilesResult);
                }
            }
            if (mNeedSaveCount == 0)
            {
                CopyFinished(true);
            }
        }
        else
        {
            /*if (ClientMain.Copy_Default_Flag && !ClientMain.Use_Third_App_Flag)
            {
                PublicFunction.DelDirector(ResourcesEx.GetRootPath(ResFileType.Type_default));
            }*/
            string path = ResourcesEx.persistentDataPath + "/" + Default_File;
            perVersion = new Dictionary<string, string>();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            CopyFinished(true);
        }
    }
#else

    void LoadDefaultComplete(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            streamVersion = null;
        }
        else
        {
            streamVersion = LoadXml(text);
        }
        if (null != streamVersion)
        {
            if (ClientMain.Copy_Default_Flag && null != perVersion && !ClientMain.Use_Third_App_Flag)
            {//删除不需要了的文件
                List<string> delList = new List<string>();
                foreach (KeyValuePair<string, string> kvp in perVersion)
                {
                    if (!streamVersion.ContainsKey(kvp.Key) && kvp.Key.StartsWith("default/"))
                    {//默认文件才删除
                        delList.Add(kvp.Key);
                        string path = ResourcesEx.persistentDataPath + "/" + kvp.Key;
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
                for (int i = 0, imax = delList.Count; i < imax; ++i)
                {
                    perVersion.Remove(delList[i]);
                }
                SaveVesionXml();
            }
            mNeedSaveCount = 0;
            foreach (KeyValuePair<string, string> kvp in streamVersion)
            {
                bool defaultFlag = kvp.Key.StartsWith("default/");
                if (defaultFlag && kvp.Key.Contains("/actions/"))
                {//默认动作官方动作
                    string fileName = Path.GetFileNameWithoutExtension(kvp.Key);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (null == defaultActionsList)
                        {
                            defaultActionsList = new List<string>();
                        }
                        defaultActionsList.Add(fileName);
                    }
                }
                if (null == perVersion || !perVersion.ContainsKey(kvp.Key) || !perVersion[kvp.Key].Equals(kvp.Value))
                {//需要复制
                    /*if (!ClientMain.Copy_Default_Flag && defaultFlag)
                    {
                        continue;
                    }*/
                    mNeedSaveCount++;
                    FilesCopy copy = new FilesCopy();
#if UNITY_EDITOR
                    string path = "file://" + ResourcesEx.streamingAssetsPath + "/defaultFiles/" + kvp.Key;
#elif UNITY_ANDROID
                    string path = ResourcesEx.streamingAssetsPath + "/defaultFiles/" + kvp.Key;
#else
					string path = "file://" + ResourcesEx.streamingAssetsPath + "/defaultFiles/" + kvp.Key;
#endif
                    copy.CopyFile(path, ResourcesEx.persistentDataPath + "/" + kvp.Key, SaveFilesResult);
                }
            }
            if (mNeedSaveCount == 0)
            {
                CopyFinished(true);
            }
        }
        else
        {
            if (ClientMain.Copy_Default_Flag && !ClientMain.Use_Third_App_Flag)
            {
                PublicFunction.DelDirector(ResourcesEx.GetRootPath(ResFileType.Type_default));
            }
            string path = ResourcesEx.persistentDataPath + "/" + Default_File;
            perVersion = new Dictionary<string, string>();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            CopyFinished(true);
        }
    }
#endif

#if USE_GZIP
    void SaveFilesResult(string srcPath, bool result)
    {
        if (string.IsNullOrEmpty(srcPath))
        {
            if (result)
            {                
                if (null != streamVersion)
                {
                    if (null == perVersion)
                    {
                        perVersion = new Dictionary<string, string>();
                    }
                    foreach (KeyValuePair<string, string> kvp in streamVersion)
                    {
                        perVersion[kvp.Key] = kvp.Value;
                    }
                }
                SaveVesionXml();
            }
            CopyFinished(result);
        }
        else
        {
            srcPath = srcPath.Replace("\\", "/");
            bool defaultFlag = srcPath.StartsWith("default/");
            if (defaultFlag && srcPath.Contains("/actions/"))
            {//默认动作官方动作
                string fileName = Path.GetFileNameWithoutExtension(srcPath);
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (null == defaultActionsList)
                    {
                        defaultActionsList = new List<string>();
                    }
                    defaultActionsList.Add(fileName);
                }
            }
        }
    }
#else
    void SaveFilesResult(string srcPath, bool result)
    {
        if (result)
        {
            mSaveSuccessCount++;
            int index = srcPath.IndexOf("defaultFiles/");

            string fileName = srcPath.Substring(index + "defaultFiles/".Length);
            if (null == perVersion)
            {
                perVersion = new Dictionary<string, string>();
            }
            if (streamVersion.ContainsKey(fileName))
            {
                perVersion[fileName] = streamVersion[fileName];
            }
            SaveVesionXml();
        }
        mSaveCount++;
        if (mSaveCount == mNeedSaveCount)
        {
            if (mSaveSuccessCount == mNeedSaveCount)
            {
                CopyFinished(true);
            }
            else
            {
                CopyFinished(false);
            }
        }
    }
#endif

    void CopyFinished(bool result)
    {
        if (null != mFinish)
        {
            mFinish(result);
        }
        /*if (null != defaultActionsList)
        {
            for (int i = 0, imax = defaultActionsList.Count; i < imax; ++i)
            {
                ActionsManager.GetInst().AddOfficial(defaultActionsList[i]);
            }
            ActionsManager.GetInst().SaveOfficialActions();
        }*/
    }

    /// <summary>
    /// 保存版本信息
    /// </summary>
    private void SaveVesionXml()
    {
        if (null == perVersion)
        {
            return;
        }
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement xmlRoot = xmlDoc.CreateElement("Files");
        xmlDoc.AppendChild(xmlRoot);
        foreach (KeyValuePair<string, string> pair in perVersion)
        {
            XmlElement xe = xmlDoc.CreateElement("File");
            xmlRoot.AppendChild(xe);
            xe.SetAttribute("file", pair.Key);
            xe.SetAttribute("md5", pair.Value);
        }
        string path = System.IO.Path.Combine(ResourcesEx.persistentDataPath, Default_File);
        xmlDoc.Save(path);
        xmlDoc = null;
    }
#endregion
}