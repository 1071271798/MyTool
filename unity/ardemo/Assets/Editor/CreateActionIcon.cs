using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using MyMVC;
using System.Text;

/// <summary>
/// Author:xj
/// FileName:CreateActionIcon.cs
/// Description:生成动作图标列表
/// Time:2015/12/7 10:57:09
/// </summary>
public class CreateActionIcon : EditorWindow
{
    #region 公有属性
    public class TextureData
    {
        public string id;
        public Texture tex;
        public int count;

        public TextureData(string id, Texture tex, int num)
        {
            this.id = id;
            this.tex = tex;
            this.count = num;
        }
    }
    #endregion

    #region 其他属性
    ActionIconList mIconList;
    Dictionary<string, int> mIconIdDict;
    List<TextureData> mTextureDict;
    #endregion

    #region 公有函数
    [MenuItem("MyTool/生成动作图标配置文件")]
    public static void CreateActionIconList()
    {

        Rect rect = new Rect(0, 0, 400, 400);
        EditorWindow.GetWindowWithRect<CreateActionIcon>(rect, true, "CreateActionIconList", true);
        
        
    }



    #endregion

    #region 其他函数

    public CreateActionIcon()
    {
        mIconIdDict = new Dictionary<string, int>();
        mIconList = new ActionIconList();
        mIconList.LoadXml();
        //mIconList = (ActionIconList)AssetDatabase.LoadAssetAtPath("Assets/Resources/actionicon.asset", typeof(ActionIconList));
        mTextureDict = new List<TextureData>();
        if (null != mIconList && null != mIconList.iconList)
        {
            for (int i = 0, imax = mIconList.iconList.Count; i < imax; ++i)
            {
                mIconIdDict.Add(mIconList.iconList[i].id, 1);
                Texture tex = AssetDatabase.LoadAssetAtPath("Assets/Textures/actionIcon/" + mIconList.iconList[i].iconName + ".png", typeof(Texture)) as Texture;
                mTextureDict.Add(new TextureData(mIconList.iconList[i].id, tex, mIconList.iconList[i].useCount));
            }
        }
    }


    string GetIconId()
    {
        int num = mIconIdDict.Count + 1;
        while (true)
        {
            if (!mIconIdDict.ContainsKey("icon_" + num))
            {
                return "icon_" + num;
            }
            ++num;
        }
    }
    Vector2 mScrollViewSize = Vector2.zero;
    void OnGUI()
    {
        if (GUILayout.Button("一键生成", GUILayout.Width(400)))
        {
            AutoCreateData();
        }
        if (GUILayout.Button("增加一张图片", GUILayout.Width(400)))
        {
            string id = GetIconId();
            Texture2D tex = new Texture2D(100, 100);
            mTextureDict.Add(new TextureData(id, tex, 1));
            mIconIdDict.Add(id, 0);
            /*if (null == mIconList)
            {
                mIconList = ScriptableObject.CreateInstance<ActionIconList>();
                mIconList.iconList = new List<ActionIcon>();
            }
            ActionIcon actIcon = new ActionIcon(id, string.Empty, 1);
            / *actIcon.id = id;
            actIcon.iconName = string.Empty;
            actIcon.useCount = 1;* /
            mIconList.iconList.Add(actIcon);*/
        }

        if (GUILayout.Button("清空图片", GUILayout.Width(400)))
        {
            mTextureDict.Clear();
            mIconIdDict.Clear();
            if (null != mIconList && null != mIconList.iconList)
            {
                mIconList.iconList.Clear();
            }
        }
        if (GUILayout.Button("生成数据", GUILayout.Width(200)))
        {
            CreateData();
        }

        //Texture tmpTex = null;
        if (null != mTextureDict)
        {
            Texture tmpTex = null;
            mScrollViewSize = EditorGUILayout.BeginScrollView(mScrollViewSize, GUILayout.Width(400), GUILayout.Height(300));

            int delIndex = -1;
            for (int i = 0, imax = mTextureDict.Count; i < imax; ++i)
            {
                if (i % 4 == 0)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(400), GUILayout.Height(100));
                }
                EditorGUILayout.BeginVertical();

                string str = string.Empty;
                str = EditorGUILayout.TextField(mTextureDict[i].id, GUILayout.Width(60));
                if (!string.IsNullOrEmpty(str) && !str.Equals(mTextureDict[i].id))
                {
                    for (int index = 0, indexMax = mTextureDict.Count; index < indexMax; ++index)
                    {
                        if (str.Equals(mTextureDict[index].id))
                        {
                            mTextureDict[index].id = mTextureDict[i].id;
                        }
                    }
                    mTextureDict[i].id = str;
                }

                
                mTextureDict[i].count = EditorGUILayout.IntField(mTextureDict[i].count, GUILayout.Width(60));
                tmpTex = EditorGUILayout.ObjectField(mTextureDict[i].tex, typeof(Texture), true, GUILayout.Width(60), GUILayout.Height(60)) as Texture;
                if (tmpTex != mTextureDict[i].tex)
                {
                    string path = AssetDatabase.GetAssetPath(tmpTex);
                    string name = Path.GetFileName(path);
                    if (name.StartsWith("icon_") && name.EndsWith(".png"))
                    {
                        mTextureDict[i].tex = tmpTex;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "请选择Textures/actionIcon下的图片", "确定");
                    }
                }
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    delIndex = i;
                    /*mTextureDict.RemoveAt(i);
                    --i;
                    --imax;*/
                }
                EditorGUILayout.EndVertical();
                if (i % 4 == 3)
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (-1 != delIndex)
            {
                for (int i = delIndex, imax = mTextureDict.Count; i < imax; ++i)
                {
                    if (i < imax - 1)
                    {
                        mTextureDict[i] = mTextureDict[i + 1];
                    }
                }
                mTextureDict.RemoveAt(mTextureDict.Count - 1);
            }
            EditorGUILayout.EndScrollView();
        }
        
    }

    void AutoCreateData()
    {
        string[] files = Directory.GetFiles(Application.dataPath + "/Textures/actionIcon");
        List<string> iconList = new List<string>();
        if (null != files)
        {
            for (int i = 0, imax = files.Length; i < imax; ++i)
            {
                string fileNmae = Path.GetFileName(files[i]);
                if (fileNmae.StartsWith("icon_") && files[i].EndsWith(".png"))
                {
                    iconList.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
            }
        }
        if (iconList.Count > 0)
        {
            for (int i = 0, imax = iconList.Count; i < imax; ++i)
            {
                if (!Exists(iconList[i]))
                {//不存在则添加进去
                    /*if (null == mIconList)
                    {
                        mIconList = ScriptableObject.CreateInstance<ActionIconList>();
                        mIconList.iconList = new List<ActionIcon>();
                    }*/
                    string id = GetIconId();
                    Texture tex = AssetDatabase.LoadAssetAtPath("Assets/Textures/actionIcon/" + iconList[i] + ".png", typeof(Texture)) as Texture;
                    mTextureDict.Add(new TextureData(id, tex, 1));
                    mIconIdDict.Add(id, 0);
                    /*ActionIcon actIcon = new ActionIcon(id, iconList[i], 1);
                    / *actIcon.id = id;
                    actIcon.iconName = iconList[i];
                    actIcon.useCount = 1;* /
                    mIconList.iconList.Add(actIcon);*/
                }
            }
        }
    }

    bool Exists(string name)
    {
        if (null != mIconList && null != mIconList.iconList)
        {
            for (int i = 0, imax = mIconList.iconList.Count; i < imax; ++i)
            {
                if (name.Equals(mIconList.iconList[i]))
                {
                    return true;
                }
            }
            
        }
        return false;
    }

    void CreateData()
    {
        if (null == mIconList)
        {
            mIconList = new ActionIconList();
            mIconList.iconList = new List<ActionIcon>();
        }
        mIconList.iconList.Clear();
        for (int i = 0, imax = mTextureDict.Count; i < imax; ++i)
        {
            if (null != mTextureDict[i].tex && !string.IsNullOrEmpty(mTextureDict[i].tex.name))
            {
                mIconList.iconList.Add(new ActionIcon(mTextureDict[i].id, mTextureDict[i].tex.name, mTextureDict[i].count));
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "图片不能为空", "确定");
                return;
            }
        }
        mIconList.iconList.Sort(
            delegate (ActionIcon a, ActionIcon b) 
            {
                int i = int.Parse(a.id.Substring("icon_".Length));
                int j = int.Parse(b.id.Substring("icon_".Length));
                return i - j;
            });
        mIconList.SaveXml();
       /* string path = "Assets/Resources/actionicon.asset";
        if (!File.Exists(path))
        {
            AssetDatabase.CreateAsset(mIconList, "Assets/Resources/actionicon.asset");
        }
        AssetDatabase.SaveAssets();*/
        AssetDatabase.Refresh();
        //EditorUtility.SetDirty(mIconList);
    }

    void OnDestroy()
    {
        mIconList = null;
        if (null != mTextureDict)
        {
            mTextureDict.Clear();
        }
        EditorUtility.UnloadUnusedAssets();
    }  
    
    #endregion
}