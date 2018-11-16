#define USE_GZIP
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:CheckDefault.cs
/// Description:检查StreamingAssets下面的default文件夹，根据default生成配置文件
/// Time:2015/9/24 15:26:24
/// </summary>
public class CheckDefault
{
    #region 公有属性
    #endregion

    #region 其他属性
    static string Cfg_Name = "/default.cfg";
    #endregion

    #region 公有函数
    [MenuItem("MyTool/生成默认模型配置文件")]
#if USE_GZIP
    public static void CreateDefaultConfig()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        /*string path = streamingAssetsPath + "/default";
        string path1 = streamingAssetsPath + "/playerdata";*/
        string path = streamingAssetsPath + "/defaultFiles";
        if (Directory.Exists(path)/* || Directory.Exists(path1)*/)
        {
            string cfg = streamingAssetsPath + Cfg_Name;

            List<string> filesList = new List<string>();
            PublicFunction.GetFiles(path, filesList);
            //PublicFunction.GetFiles(path1, filesList);
            if (filesList.Count > 0)
            {
                //新建xml实例
                XmlDocument xmlDoc = new XmlDocument();
                //创建根节点，最上层节点
                XmlElement roots = xmlDoc.CreateElement("Roots");
                xmlDoc.AppendChild(roots);
                string outPath = path + ".zip";
                GZipHelper.CompressDirectory(path, outPath, null);
                XmlElement node = xmlDoc.CreateElement("Root");
                node.SetAttribute("file", Path.GetFileName(outPath));
                node.SetAttribute("md5", CreateMD5List.GetFileMd5(outPath));
                roots.AppendChild(node);
                xmlDoc.Save(cfg);

                EditorUtility.DisplayDialog("提示", "默认模型配置文件生成完毕", "确定");
            }
            else
            {
                if (File.Exists(cfg))
                {
                    File.Delete(cfg);
                }
                if (File.Exists(path + ".zip"))
                {
                    File.Delete(path + ".zip");
                }
                EditorUtility.DisplayDialog("提示", "没有默认模型", "确定");
            }
        }
        else
        {
            string cfg = Application.streamingAssetsPath + Cfg_Name;
            if (File.Exists(cfg))
            {
                File.Delete(cfg);
            }
            if (File.Exists(path + ".zip"))
            {
                File.Delete(path + ".zip");
            }
            EditorUtility.DisplayDialog("提示", "没有默认模型", "确定");
        }
    }
#else
    public static void CreateDefaultConfig()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        /*string path = streamingAssetsPath + "/default";
        string path1 = streamingAssetsPath + "/playerdata";*/
        string path = streamingAssetsPath + "/defaultFiles";
        if (Directory.Exists(path)/* || Directory.Exists(path1)*/)
        {
            string cfg = streamingAssetsPath + Cfg_Name;
            
            List<string> filesList = new List<string>();
            PublicFunction.GetFiles(path, filesList);
            //PublicFunction.GetFiles(path1, filesList);
            if (filesList.Count > 0)
            {
                //新建xml实例
                XmlDocument xmlDoc = new XmlDocument();
                //创建根节点，最上层节点
                XmlElement roots = xmlDoc.CreateElement("Roots");
                xmlDoc.AppendChild(roots);
                string dirRootName = "defaultFiles/";
                //添加内容
                for (int i = 0, icount = filesList.Count; i < icount; ++i)
                {
                    if (!filesList[i].EndsWith(".meta"))
                    {
                        XmlElement node = xmlDoc.CreateElement("Root");
                        if (filesList[i].Contains(dirRootName))
                        {
                            node.SetAttribute("file", filesList[i].Substring(filesList[i].IndexOf(dirRootName) + dirRootName.Length));
                        }
                        /*else if (filesList[i].Contains("playerdata"))
                        {
                            node.SetAttribute("file", filesList[i].Substring(filesList[i].IndexOf("playerdata")));
                        }*/
                        node.SetAttribute("md5", CreateMD5List.GetFileMd5(filesList[i]));
                        roots.AppendChild(node);
                    }
                }

                xmlDoc.Save(cfg);

                EditorUtility.DisplayDialog("提示", "默认模型配置文件生成完毕", "确定");
            }
            else
            {
                if (File.Exists(cfg))
                {
                    File.Delete(cfg);
                }
                EditorUtility.DisplayDialog("提示", "没有默认模型", "确定");
            }
        }
        else
        {
            string cfg = Application.streamingAssetsPath + Cfg_Name;
            if (File.Exists(cfg))
            {
                File.Delete(cfg);
            }
            EditorUtility.DisplayDialog("提示", "没有默认模型", "确定");
        }
    }
#endif
#if USE_GZIP
    public static void CreateDefaultConfigNoDialog()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string path = streamingAssetsPath + "/defaultFiles";
        if (Directory.Exists(path))
        {
            string cfg = streamingAssetsPath + Cfg_Name;
            List<string> filesList = new List<string>();
            PublicFunction.GetFiles(path, filesList);
            if (filesList.Count > 0)
            {
                //新建xml实例
                XmlDocument xmlDoc = new XmlDocument();
                //创建根节点，最上层节点
                XmlElement roots = xmlDoc.CreateElement("Roots");
                xmlDoc.AppendChild(roots);
                string outPath = path + ".zip";
                GZipHelper.CompressDirectory(path, outPath, null);
                XmlElement node = xmlDoc.CreateElement("Root");
                node.SetAttribute("file", Path.GetFileName(outPath));
                /*else if (filesList[i].Contains("playerdata"))
                {
                    node.SetAttribute("file", filesList[i].Substring(filesList[i].IndexOf("playerdata")));
                }*/
                node.SetAttribute("md5", CreateMD5List.GetFileMd5(outPath));
                roots.AppendChild(node);
                xmlDoc.Save(cfg);
            }
            else
            {
                if (File.Exists(cfg))
                {
                    File.Delete(cfg);
                }
                if (File.Exists(path + ".zip"))
                {
                    File.Delete(path + ".zip");
                }
            }
            PublicFunction.DelDirector(path, true);
            if (File.Exists(path + ".meta"))
            {
                File.Delete(path + ".meta");
            }
        }
        else
        {
            string cfg = Application.streamingAssetsPath + Cfg_Name;
            if (File.Exists(cfg))
            {
                File.Delete(cfg);
            }
            if (File.Exists(path + ".zip"))
            {
                File.Delete(path + ".zip");
            }
        }
    }
#else
    public static void CreateDefaultConfigNoDialog()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string path = streamingAssetsPath + "/defaultFiles";
        if (Directory.Exists(path))
        {
            string cfg = streamingAssetsPath + Cfg_Name;
            List<string> filesList = new List<string>();
            PublicFunction.GetFiles(path, filesList);
            if (filesList.Count > 0)
            {
                //新建xml实例
                XmlDocument xmlDoc = new XmlDocument();
                //创建根节点，最上层节点
                XmlElement roots = xmlDoc.CreateElement("Roots");
                xmlDoc.AppendChild(roots);
                string dirRootName = "defaultFiles/";
                //添加内容
                for (int i = 0, icount = filesList.Count; i < icount; ++i)
                {
                    if (!filesList[i].EndsWith(".meta"))
                    {
                        XmlElement node = xmlDoc.CreateElement("Root");
                        if (filesList[i].Contains(dirRootName))
                        {
                            node.SetAttribute("file", filesList[i].Substring(filesList[i].IndexOf(dirRootName) + dirRootName.Length));
                        }
                        /*else if (filesList[i].Contains("playerdata"))
                        {
                            node.SetAttribute("file", filesList[i].Substring(filesList[i].IndexOf("playerdata")));
                        }*/
                        node.SetAttribute("md5", CreateMD5List.GetFileMd5(filesList[i]));
                        roots.AppendChild(node);
                    }
                }
                xmlDoc.Save(cfg);
            }
            else
            {
                if (File.Exists(cfg))
                {
                    File.Delete(cfg);
                }
            }
        }
        else
        {
            string cfg = Application.streamingAssetsPath + Cfg_Name;
            if (File.Exists(cfg))
            {
                File.Delete(cfg);
            }
        }
    }
#endif

    static void CreateEmptyCfg()
    {
        //新建xml实例
        XmlDocument xmlDoc = new XmlDocument();
        //创建根节点，最上层节点
        XmlElement roots = xmlDoc.CreateElement("Roots");
        xmlDoc.AppendChild(roots);
        string cfg = Application.streamingAssetsPath +Cfg_Name;
        xmlDoc.Save(cfg);
    }

    /*[MenuItem("MyTool/资源打包")]
    public static void BuildAsset()
    {
        UnityEngine.Object [] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        if (null != objs)
        {
            for (int i = 0, imax = objs.Length; i < imax; ++i)
            {
                string path = AssetDatabase.GetAssetPath(objs[i]);
                if (path.Contains("."))
                {
                    BuildPipeline.BuildAssetBundle(objs[i], null, Application.persistentDataPath + "/aaa.assetbundle", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows);
                }
            }
        }
    }*/

    [MenuItem("MyTool/检查多语言文件")]
    public static bool CheckLauguageConfig()
    {
        LauguageTool.CleanUp();
        string path = Path.Combine(Application.dataPath, "Script");
        List<string> files = new List<string>();
        PublicFunction.GetFiles(path, files);
        Dictionary<string, int> lauguageKey = new Dictionary<string, int>();
        for (int i = 0, imax = files.Count; i < imax; ++i)
        {
            string ex = Path.GetExtension(files[i]);
            if (ex.Equals(".cs"))
            {
                List<string> keys = GetFileLauguageKey(files[i]);
                if (null != keys && keys.Count > 0)
                {
                    for (int j = 0, jmax = keys.Count; j < jmax; ++j)
                    {
                        if (lauguageKey.ContainsKey(keys[j]))
                        {
                            ++lauguageKey[keys[j]];
                        }
                        else
                        {
                            lauguageKey.Add(keys[j], 1);
                        }
                    }
                }
            }
        }
        string notExist = string.Empty;
        foreach (KeyValuePair<string, int> kvp in lauguageKey)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && !kvp.Key.StartsWith("guideTip_") && !kvp.Key.StartsWith("Guidepage_") && !kvp.Key.StartsWith("lesson"))
            {
                if (!LauguageTool.GetIns().IsExistForKey(kvp.Key))
                {
                    notExist += kvp.Key + "\n";
                }
            }
        }

        string SurplusStr = string.Empty;
        Dictionary<string, string> configDict = LauguageTool.GetIns().GetConfigText();
        if (null != configDict)
        {
            foreach (KeyValuePair<string, string> kvp in configDict)
            {
                if (!lauguageKey.ContainsKey(kvp.Key) && !kvp.Key.StartsWith("guideTip_") && !kvp.Key.StartsWith("Guidepage_"))
                {
                    SurplusStr += kvp.Key + "\n";
                }
            }
        }
        if (!string.IsNullOrEmpty(SurplusStr))
        {
            Debug.LogError("存在多余的文本配置信息：\n" + SurplusStr);
        }

        if (!string.IsNullOrEmpty(notExist))
        {
            Debug.LogError("以下key值在文本配置文件中不存在：\n" + notExist);
            return false;
        }
        else
        {
            Debug.Log("无不存在的key值");
            return true;
        }
    }
    
    #endregion

    #region 其他函数

    static List<string> GetFileLauguageKey(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                StreamReader stream = new StreamReader(path);
                string text = stream.ReadToEnd();
                MatchCollection matchs = Regex.Matches(text, "(?<=LauguageTool.GetIns\\(\\).GetText\\s*\\(\\s*\"\\s*)[\\s\\S]*?(?=\\s*\")");
                if (null != matchs)
                {
                    List<string> list = new List<string>();
                    foreach (Match mc in matchs)
                    {
                        list.Add(mc.Value.Trim());
                    }
                    return list;
                }
                stream.Dispose();
                stream.Close();
            }
            
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        return null;
    }
#endregion
}