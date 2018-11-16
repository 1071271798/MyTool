using System;
using System.Collections.Generic;
using UnityEngine;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
using System.IO;
using System.Text;
using UnityEditor;

/// <summary>
/// Author:xj
/// FileName:ZipTest.cs
/// Description:
/// Time:2016/3/2 10:25:54
/// </summary>
public class ZipTest
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    [MenuItem("MyTool/压缩文件")]
    public static void CompressAll()
    {
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        if (null != objs)
        {
            string rootPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
            for (int i = 0, imax = objs.Length; i < imax; ++i)
            {
                string path = rootPath + AssetDatabase.GetAssetPath(objs[i]);
                Debug.Log("path =" + path);
                if (Directory.Exists(path))
                {//文件夹
                    //string dirName = Path.GetDirectoryName(path);
                    //CompressDirectory(path, path + ".zip", null);
                    GZipHelper.CompressDirectory(path, path + ".zip", null);
                }
                else if (File.Exists(path))
                {
                    GZipHelper.CompressFile(path, path + ".zip");
                    /*string fileName = Path.GetFileName(path);
                    string filePath = path.Substring(0, path.LastIndexOf(fileName));
                    FileStream outFile = new FileStream(filePath + "/" + fileName + ".zip", FileMode.Create, FileAccess.Write, FileShare.None);
                    GZipOutputStream strem = new GZipOutputStream(outFile);
                    CompressFile(filePath, fileName, strem);
                    strem.Close();
                    outFile.Close();*/
                }
            }
        }
    }
    [MenuItem("MyTool/解压文件")]
    public static void DecompressAll()
    {
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        if (null != objs)
        {
            string rootPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
            for (int i = 0, imax = objs.Length; i < imax; ++i)
            {
                string path = rootPath + AssetDatabase.GetAssetPath(objs[i]);
                Debug.Log("path =" + path);
                
                if (path.EndsWith(".zip"))
                {
                    string oldPath = path.Replace(".zip", "");
                    if (!oldPath.Contains("."))
                    {//文件夹
                        GZipHelper.DecompressToDirectory(path, oldPath, null);
                        //GZipHelper.WWWDecompressToDirectory("file://" + path, oldPath, null);
                        //DecompressToDirectory(path, oldPath, null);
                    }
                    else if (File.Exists(path))
                    {
                        string fileName = Path.GetFileName(path);
                        string filePath = path.Substring(0, path.LastIndexOf(fileName));
                        GZipHelper.DecompressFile(path, filePath);
                        /*string fileName = Path.GetFileName(path);
                        string filePath = path.Substring(0, path.LastIndexOf(fileName));

                        FileStream inFile = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                        GZipInputStream strem = new GZipInputStream(inFile);
                        DecompressFile(filePath, strem, null);
                        strem.Close();
                        inFile.Close();*/
                    }
                }
                
            }
        }
    }

    #endregion

    #region 其他函数
    #endregion
}