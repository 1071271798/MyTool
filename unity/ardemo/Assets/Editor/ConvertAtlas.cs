using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ConvertAtlas.cs
/// Description:
/// Time:2015/8/27 15:19:40
/// </summary>
public class ConvertAtlas : EditorWindow
{
    #region 公有属性
    static UIAtlas sAtlas;
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    [MenuItem("MyTool/ConvertAtlas")]
    public static void MyConvertAtlas()
    {
        ConvertAtlas windows = EditorWindow.GetWindow<ConvertAtlas>(true, "ConvertAtlas");
        windows.position = new Rect(400, 300, 500, 550);

        
    }

    [MenuItem("MyTool/删除模型文件")]
    public static void DelFloderModelData()
    {
        string folderPath = EditorUtility.OpenFolderPanel("选择文件夹", string.Empty, string.Empty);
        if (!string.IsNullOrEmpty(folderPath))
        {
            string[] dirs = Directory.GetDirectories(folderPath);
            for (int i = dirs.Length - 1; i >= 0; --i)
            {
                DelAnModelData(dirs[i]);
            }
        }
        
    }

    public static void DelAnModelData(string path)
    {
        string[] dirs = Directory.GetDirectories(path);
        for (int i = dirs.Length - 1; i >= 0; --i)
        {
            if (!dirs[i].EndsWith("actions"))
            {
                PublicFunction.DelDirector(dirs[i], true);
            }
        }
        string[] files = Directory.GetFiles(path);
        for (int i = files.Length - 1; i >= 0; --i)
        {
            File.Delete(files[i]);
        }
    }
    #endregion

    #region 其他函数
    GameObject atlas = null;
    void OnGUI()
    {
        GameObject newObj = EditorGUILayout.ObjectField("model", atlas, typeof(GameObject), true) as GameObject;
        if (atlas != newObj)
        {
            atlas = newObj;
            sAtlas = atlas.GetComponent<UIAtlas>();
        }

        if (GUILayout.Button("替换", GUILayout.Width(100), GUILayout.Height(20)))
        {
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);
            if (null != objs)
            {
                for (int i = 0, imax = objs.Length; i < imax; ++i)
                {
                    //实例化预设，实例化以后才可以找到里面的组件
                    GameObject prefab1 = PrefabUtility.InstantiatePrefab(objs[i]) as GameObject;

                    UISprite[] sps = prefab1.GetComponentsInChildren<UISprite>(true);
                    if (null != sps)
                    {
                        for (int j = 0, jmax = sps.Length; j < jmax; ++j)
                        {
                            if (null != sAtlas)
                            {
                                sps[j].atlas = sAtlas;
                            }
                        }
                    }
                    //把去掉依赖的预设保存到本地
                    PrefabUtility.ReplacePrefab(prefab1, objs[i]);
                    UnityEngine.Object.DestroyImmediate(prefab1);
                    prefab1 = null;
                }
            }
        }
    }
    [MenuItem("MyTool/修改字体")]
    static public void yaheifont()
    {
        UIFont uf = AssetDatabase.LoadAssetAtPath("Assets/Font/Arial.prefab", typeof(UIFont)) as UIFont;
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        foreach (UnityEngine.Object _obj in objs)
        {
            string path = AssetDatabase.GetAssetPath(_obj);
            Debug.Log("objname:" + _obj.name);
            UnityEngine.Object[] arr = AssetDatabase.LoadAllAssetsAtPath(path);
            Debug.Log("PATH:" + path);
            foreach (UnityEngine.Object j in arr)
            {
                if (j != null && j.GetType() == typeof(UILabel))
                {
                    (j as UILabel).bitmapFont = uf;
                    Debug.Log("dfdfd:" + j.name + ",tyep:" + j.GetType());

                }

            }
            EditorUtility.SetDirty(_obj);
        }
        AssetDatabase.SaveAssets();
    }
    #endregion
}