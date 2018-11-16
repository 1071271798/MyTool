using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

/// <summary>
/// Author:xj
/// FileName:TextureRenameEdit.cs
/// Description:
/// Time:2016/4/15 10:42:47
/// </summary>
public class TextureRenameEdit : EditorWindow
{
    #region 公有属性
    #endregion

    #region 其他属性
    string mFileFont = string.Empty;
    string mDelStr = string.Empty;
    #endregion

    #region 公有函数
    [MenuItem("MyTool/序列帧改名", false, 2)]
    public static void OpenTextureRenameEdit()
    {
        TextureRenameEdit windows = EditorWindow.GetWindow<TextureRenameEdit>(true, "TextureRenameEdit");
        windows.position = new Rect(400, 300, 500, 550);


    }

    #endregion

    #region 其他函数
    string mFolderPath = string.Empty;
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("修改前缀为:", GUILayout.Width(120));
        mFileFont = EditorGUILayout.TextField(mFileFont, GUILayout.Width(100));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("删除字符串:", GUILayout.Width(120));
        mDelStr = EditorGUILayout.TextField(mDelStr, GUILayout.Width(100));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100), GUILayout.Height(20)))
        {
            mFolderPath = EditorUtility.OpenFolderPanel("选择文件夹", mFolderPath, string.Empty);
        }
        if (GUILayout.Button("普通改名"))
        {
            if (!string.IsNullOrEmpty(mFolderPath))
            {
                string[] files = Directory.GetFiles(mFolderPath);
                if (null != files)
                {
                    for (int i = 0, imax = files.Length; i < imax; ++i)
                    {
                        string path = Path.GetDirectoryName(files[i]);
                        string ex = Path.GetExtension(files[i]);
                        string fileName = Path.GetFileNameWithoutExtension(files[i]);
                        string[] tmp = fileName.Split('_');
                        if (null != tmp && tmp.Length == 2 && PublicFunction.IsInteger(tmp[1]))
                        {
                            string font = mFileFont;
                            if (string.IsNullOrEmpty(font))
                            {
                                font = tmp[0];
                            }
                            int num = int.Parse(tmp[1]);
                            string newName = font + "_" + num;
                            File.Move(files[i], path + "/" + newName + ex);
                        }
                    }
                }
            }
        }
        if (GUILayout.Button("升序改名"))
        {
            SortFile(mFolderPath, true);
        }
        if (GUILayout.Button("降序改名"))
        {
            SortFile(mFolderPath, false);
        }
        if (GUILayout.Button("删除字符串"))
        {
            if (!string.IsNullOrEmpty(mFolderPath))
            {
                string[] files = Directory.GetFiles(mFolderPath);
                if (null != files)
                {
                    for (int i = 0, imax = files.Length; i < imax; ++i)
                    {
                        string path = Path.GetDirectoryName(files[i]);
                        string ex = Path.GetExtension(files[i]);
                        string fileName = Path.GetFileNameWithoutExtension(files[i]);
                        string newName = fileName.Replace(mDelStr, "");
                        File.Move(files[i], path + "/" + newName + ex);
                    }
                }
            }
        }
    }

    void SortFile(string folder, bool ascFlag)
    {
        if (!string.IsNullOrEmpty(folder))
        {
            string[] files = Directory.GetFiles(folder);
            if (null != files)
            {
                List<string> list = new List<string>();
                for (int i = 0, imax = files.Length; i < imax; ++i)
                {
                    list.Add(files[i]);
                }
                list.Sort(delegate (string a, string b)
                {
                    if (ascFlag)
                    {
                        return (GetNum(a) - GetNum(b));
                    }
                    else
                    {
                        return (GetNum(b) - GetNum(a));
                    }
                });
                for (int i = 0, imax = list.Count; i < imax; ++i)
                {
                    string path = Path.GetDirectoryName(list[i]);
                    string newPath = path + "/newFile";
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    string ex = Path.GetExtension(list[i]);
                    string fileName = Path.GetFileNameWithoutExtension(list[i]);
                    string[] tmp = fileName.Split('_');
                    if (null != tmp && tmp.Length == 2 && PublicFunction.IsInteger(tmp[1]))
                    {
                        string font = mFileFont;
                        if (string.IsNullOrEmpty(font))
                        {
                            font = tmp[0];
                        }
                        //int num = int.Parse(tmp[1]);
                        string newName = font + "_" + i;
                        File.Move(list[i], newPath + "/" + newName + ex);
                    }
                }
            }
        }
    }

    int GetNum(string str)
    {
        string fileName = Path.GetFileNameWithoutExtension(str);
        string[] tmp = fileName.Split('_');
        if (null != tmp && tmp.Length == 2 && PublicFunction.IsInteger(tmp[1]))
        {
            return int.Parse(tmp[1]);
        }
        return 0;
    }
    #endregion
}