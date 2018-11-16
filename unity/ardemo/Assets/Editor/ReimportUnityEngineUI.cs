using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ReimportUnityEngineUI.cs
/// Description:
/// Time:2016/5/18 9:21:05
/// </summary>
public class ReimportUnityEngineUI
{
    [MenuItem("Assets/Reimport UI Assemblies", false, 100)]
    public static void ReimportUI()
    {
        var path = EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/GUISystem/{0}/{1}";
        var version = Regex.Match(Application.unityVersion, @"^[0-9]+\.[0-9]+\.[0-9]+").Value;
/*
#if UNITY_4_6
        var path = EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/GUISystem/{0}/{1}";
        var version = Regex.Match(Application.unityVersion, @"^[0-9]+\.[0-9]+\.[0-9]+").Value;
#else
        var path = EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/GUISystem/{1}";
        var version = string.Empty;
#endif*/
        string engineDll = string.Format(path, version, "UnityEngine.UI.dll");
        string editorDll = string.Format(path, version, "Editor/UnityEditor.UI.dll");
        ReimportDll(engineDll);
        ReimportDll(editorDll);
    }
    static void ReimportDll(string path)
    {
        if (File.Exists(path))
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);
        else
            Debug.LogError(string.Format("DLL not found {0}", path));
    }
}