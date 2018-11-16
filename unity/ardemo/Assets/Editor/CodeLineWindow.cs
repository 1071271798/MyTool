using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:CodeLineEditor.cs
/// Description:
/// Time:2016/5/31 17:44:25
/// </summary>
public class CodeLineWindow : EditorWindow
{
    [MenuItem("MyTool/获取代码行数", false, 2)]
    public static void CodeLineWindowEdit()
    {
        CodeLineWindow windows = EditorWindow.GetWindow<CodeLineWindow>(true, "获取代码行数");
        windows.position = new Rect(400, 300, 500, 550);
    }

    int GetFloderCount(string floderPath)
    {
        List<string> files = new List<string>();
        PublicFunction.GetFiles(floderPath, files);
        int lineCount = 0;
        for (int i = 0, imax = files.Count; i < imax; ++i)
        {
            string ex = Path.GetExtension(files[i]);
            if (ex.Equals(".cs") || ex.Equals(".java"))
            {
                try
                {
                    if (File.Exists(files[i]))
                    {
                        lineCount += LinesOfCode(files[i]);
                    }

                }
                catch (System.Exception)
                {
                }

            }
        }
        return lineCount;
    }

    int GetFilesCount(string floderPath)
    {
        List<string> files = new List<string>();
        PublicFunction.GetFiles(floderPath, files);
        int lineCount = 0;
        for (int i = 0, imax = files.Count; i < imax; ++i)
        {
            string ex = Path.GetExtension(files[i]);
            if (ex.Equals(".cs") || ex.Equals(".java"))
            {
                try
                {
                    if (File.Exists(files[i]))
                    {
                        lineCount += 1;
                    }

                }
                catch (System.Exception)
                {
                }

            }
        }
        return lineCount;
    }

    /// <summary>
    /// 检测一个C代码文件中的有效代码行数
    /// </summary>
    /// <param name="filename">文件名</param>
    /// <returns>代码行数</returns>
    public static int LinesOfCode(string filename)
    {
        System.IO.StreamReader sr = System.IO.File.OpenText(filename);
        string s = sr.ReadToEnd();
        sr.Close();
        bool isLine = false;  //一行中拥有有效字符时为true，该行可记入代码行数
        bool isCommitLf = false; //注释/*...*/中出现至少一个折行时为true
        int lines = 0;    //代码行数统计
        for (int i = 0; i < s.Length; i++)
        {
            //无效字符
            if (s[i] == ' ' || s[i] == '\r' || s[i] == '\t')
            {
                continue;
            }
            //搜索到换行，若该行有有效字符
            if (s[i] == '\n')
            {
                if (isLine)
                {
                    lines++;
                    isLine = false;
                }
                continue;
            }
            //字符串，占多少行按多少行算
            if (s[i] == '\"')
            {
                while (true)
                {
                    i++;
                    //如果文件遍历完毕则强行中止
                    if (i >= s.Length)
                    {
                        break;
                    }
                    //再次遇到字符'"'且前方没有或有偶数个'//'时，中止循环并退出
                    if (s[i] == '\"')
                    {
                        int sign = 0, counter = 0;
                        while (true)
                        {
                            sign++;
                            if (i - sign < 0)
                            {
                                break;
                            }
                            else if (s[i - sign] == '\\')
                            {
                                counter++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (counter % 2 == 0)
                        {
                            break;
                        }
                    }
                    //字符串中的换行，直接算作一行代码
                    if (s[i] == '\n')
                    {
                        lines++;
                        isLine = true;
                    }
                }
                isLine = true;
                continue;
            }
            //遇到形如 /*...*/ 的注释
            if (s[i] == '/' && i < s.Length - 1)
            {
                if (s[i + 1] == '*')
                {
                    i++;
                    while (true)
                    {
                        i++;
                        //如果文件遍历完毕则强行中止
                        if (i >= s.Length)
                        {
                            break;
                        }
                        if (s[i] == '\n')
                        {
                            if (isCommitLf == false)
                            {
                                if (isLine == true)
                                {
                                    lines++;
                                    isLine = false;
                                }
                                isCommitLf = true;
                            }
                        }
                        if (s[i] == '*' && i < s.Length - 1)
                        {
                            if (s[i + 1] == '/')
                            {
                                i++;
                                break;
                            }
                        }
                    }
                    isCommitLf = false;
                    continue;
                }
            }
            //遇到形如 // 的注释
            if (s[i] == '/' && i < s.Length - 1 && s[i + 1] == '/')
            {
                if (isLine == true)
                {
                    lines++;
                    isLine = false;
                }
                while (true)
                {
                    i++;
                    if (i >= s.Length || s[i] == '\n')
                    {
                        break;
                    }
                }
                continue;
            }
            //该行有了有效字符，算作一行
            isLine = true;
        }
        //最后一行可能没有字符'\n'结尾
        if (isLine)
        {
            lines++;
        }
        return lines;
    }
    List<FilesCodeData> mFileCodeList = new List<FilesCodeData>();
    Vector2 mScrollViewSize = Vector2.zero;
    string mFolderPath = Application.dataPath + "/Script";
    void OnGUI()
    {
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100), GUILayout.Height(20)))
        {
            mFolderPath = EditorUtility.OpenFolderPanel("选择文件夹", mFolderPath, string.Empty);
            if (!string.IsNullOrEmpty(mFolderPath))
            {
                string path = mFolderPath.Replace('\\', '/');
                if (path.StartsWith(Application.dataPath))
                {
                    path = path.Substring(Application.dataPath.Length);
                    if (path.StartsWith("/"))
                    {
                        path = path.Substring(1);
                    }
                }
                mFileCodeList.Add(new FilesCodeData(path, GetFloderCount(mFolderPath), GetFilesCount(mFolderPath)));
            }
        }
        if (mFileCodeList.Count > 0)
        {
            mScrollViewSize = EditorGUILayout.BeginScrollView(mScrollViewSize, GUILayout.Width(500), GUILayout.Height(500));

            for (int i = 0, imax = mFileCodeList.Count; i < imax; ++i)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(460), GUILayout.Height(30));
                EditorGUILayout.LabelField("路径:" + mFileCodeList[i].path, GUILayout.Width(300));
                EditorGUILayout.LabelField("行数:" + mFileCodeList[i].lineCount, GUILayout.Width(80));
                EditorGUILayout.LabelField("文件数:" + mFileCodeList[i].fileCount, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    class FilesCodeData
    {
        public string path;
        public int lineCount;
        public int fileCount;

        public FilesCodeData(string path, int line, int count)
        {
            this.path = path;
            this.lineCount = line;
            this.fileCount = count;
        }
    }
}