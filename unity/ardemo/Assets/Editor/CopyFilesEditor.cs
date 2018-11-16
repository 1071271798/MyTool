using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
/// <summary>
/// Author:xj
/// FileName:CopyFilesEditor.cs
/// Description:
/// Time:2016/10/14 16:57:14
/// </summary>
public class CopyFilesEditor : EditorWindow
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    [MenuItem("MyTool/复制文件")]
    public static void OpenCopyFilesEditor()
    {
        CopyFilesEditor windows = EditorWindow.GetWindow<CopyFilesEditor>(true, "CopyFilesEditor");
        windows.position = new Rect(400, 300, 500, 550);
    }
    #endregion

    #region 其他函数
    string mSourceFolderPath = string.Empty;
    string mDestFolderPath = string.Empty;
    string mSourceFilePath = string.Empty;
    string mDestFilePath = string.Empty;
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择源文件夹", GUILayout.Width(80)))
        {
            mSourceFolderPath = EditorUtility.OpenFolderPanel("选择文件夹", mSourceFolderPath, string.Empty);
        }
        mSourceFolderPath = GUILayout.TextField(mSourceFolderPath, GUILayout.Width(350));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择目标文件夹", GUILayout.Width(80)))
        {
            mDestFolderPath = EditorUtility.OpenFolderPanel("选择文件路径", mDestFolderPath, string.Empty);
        }
        mDestFolderPath = GUILayout.TextField(mDestFolderPath, GUILayout.Width(350));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("复制", GUILayout.Width(80)))
        {
            if (!string.IsNullOrEmpty(mSourceFolderPath) && !string.IsNullOrEmpty(mDestFolderPath))
            {
                CopyFloder(mSourceFolderPath, mDestFolderPath);
                EditorUtility.DisplayDialog("提示", "复制完毕", "确定");
            }
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择源文件", GUILayout.Width(80)))
        {
            mSourceFilePath = EditorUtility.OpenFilePanel("选择文件", mSourceFilePath, string.Empty);
        }
        mSourceFilePath = GUILayout.TextField(mSourceFilePath, GUILayout.Width(350));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择目标路径", GUILayout.Width(80)))
        {
            mDestFilePath = EditorUtility.OpenFolderPanel("选择文件路径", mDestFilePath, string.Empty);
        }
        mDestFilePath = GUILayout.TextField(mDestFilePath, GUILayout.Width(350));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("复制", GUILayout.Width(80)))
        {
            if (!string.IsNullOrEmpty(mSourceFilePath) && !string.IsNullOrEmpty(mDestFilePath))
            {
                CopyFile(mSourceFilePath, mDestFilePath);
                EditorUtility.DisplayDialog("提示", "复制完毕", "确定");
            }
        }

    }

    void CopyFile(string src, string dest)
    {
        byte[] bytes = File.ReadAllBytes(src);
        string fileName = Path.GetFileNameWithoutExtension(src);
        if (null != bytes)
        {
            File.WriteAllBytes(dest + "/" + fileName + ".txt", bytes);
            /*Stream sw = new FileStream(dest + "/" + fileName + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
            sw.Write(bytes, 0, bytes.Length);
            sw.Dispose();
            sw.Close();*/
        }
    }

    void CopyFloder(string src, string dest)
    {
        string dirName = src.Substring(Path.GetDirectoryName(src).Length);
        if (!string.IsNullOrEmpty(dirName))
        {
            dest += dirName;
        }
        List<string> files = new List<string>();
        PublicFunction.GetFiles(src, files);
        src = PublicFunction.ConvertSlashPath(src);
        for (int i = 0, imax = files.Count; i < imax; ++i)
        {
            string srcPath = files[i].Substring(0, files[i].LastIndexOf('/'));
            srcPath = srcPath.Substring(src.Length);
            if (string.IsNullOrEmpty(srcPath))
            {
                CopyFile(files[i], dest);
            }
            else
            {
                string destPath = dest + srcPath;
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }
                CopyFile(files[i], destPath);
            }
        }
    }

    #endregion
}