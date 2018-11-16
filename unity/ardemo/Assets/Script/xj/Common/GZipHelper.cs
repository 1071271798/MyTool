using Game.Platform;
using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:GZipHelper.cs
/// Description:压缩文件
/// Time:2016/3/2 14:27:25
/// </summary>
public class GZipHelper : SingletonObject<GZipHelper>
{
    #region 公有属性
    public delegate void ProgressDelegate(string sMessage, bool result);
    #endregion

    #region 其他属性

    #endregion

    #region 公有函数
    /// <summary>
    /// 压缩单个文件
    /// </summary>
    /// <param name="inFilePath">要压缩的文件完整路径</param>
    /// <param name="outFilePath">输出文件的完整路径</param>
    public static void CompressFile(string inFilePath, string outFilePath)
    {
        try
        {
            string fileName = Path.GetFileName(inFilePath);
            string filePath = inFilePath.Substring(0, inFilePath.LastIndexOf(fileName));
            /*FileStream outFile = new FileStream(filePath + "/" + fileName + ".zip", FileMode.Create, FileAccess.Write, FileShare.None);*/
            FileStream outFile = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            GZipOutputStream strem = new GZipOutputStream(outFile);
            CompressFile(filePath, fileName, strem);
            strem.Close();
            outFile.Close();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "GZipHelper-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="inDir">要压缩的文件夹完整路径</param>
    /// <param name="outFilePath">输出的压缩包完整路径</param>
    /// <param name="progress">进度回调</param>
    public static void CompressDirectory(string inDir, string outFilePath, ProgressDelegate progress)
    {
        string[] sFiles = Directory.GetFiles(inDir, "*.*", SearchOption.AllDirectories);
        int iDirLen = inDir[inDir.Length - 1] == Path.DirectorySeparatorChar ? inDir.Length : inDir.Length + 1;

        try
        {
            using (FileStream outFile = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (GZipOutputStream str = new GZipOutputStream(outFile))
                {
                    foreach (string sFilePath in sFiles)
                    {
                        if (sFilePath.EndsWith(".meta"))
                        {
                            continue;
                        }
                        string sRelativePath = sFilePath.Substring(iDirLen);
                        if (progress != null)
                            progress(sRelativePath, true);
                        CompressFile(inDir, sRelativePath, str);
                    }
                    str.Close();
                }
                outFile.Close();
            }
            if (null != progress)
            {
                progress(string.Empty, true);
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError("GZipHelper:" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            if (null != progress)
            {
                progress(string.Empty, false);
            }
        }
    }
    /// <summary>
    /// 解压单个文件
    /// </summary>
    /// <param name="inFilePath">完整的压缩文件路径</param>
    /// <param name="outDirPath">输出文件夹路径</param>
    public static void DecompressFile(string inFilePath, string outDirPath)
    {
        try
        {
            string fileName = Path.GetFileName(inFilePath);
            string filePath = inFilePath.Substring(0, inFilePath.LastIndexOf(fileName));

            FileStream inFile = new FileStream(inFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
            GZipInputStream strem = new GZipInputStream(inFile);
            DecompressFile(outDirPath, strem, null);
            strem.Close();
            inFile.Close();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "GZipHelper-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 用www解压单个文件
    /// </summary>
    /// <param name="inFilePath">压缩文件完整路径</param>
    /// <param name="outDirPath">输出目录</param>
    /// <param name="progress">解压结果</param>
    public static void WWWDecompressFile(string inFilePath, string outDirPath, ProgressDelegate progress)
    {
        SingletonBehaviour<ClientMain>.GetInst().StartCoroutine(WWWDecFile(inFilePath, outDirPath, progress));
    }


    /// <summary>
    /// 解压文件夹
    /// </summary>
    /// <param name="compressedFile">压缩文件路径</param>
    /// <param name="outDirPath">输出文件夹路径</param>
    /// <param name="progress">进度回调</param>
    public static void DecompressToDirectory(string compressedFile, string outDirPath, ProgressDelegate progress)
    {
        try
        {
            using (FileStream inFile = new FileStream(compressedFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (GZipInputStream zipStream = new GZipInputStream(inFile))
                {
                    while (DecompressFile(outDirPath, zipStream, progress)) ;
                    zipStream.Close();
                }
                inFile.Close();
            }
            if (null != progress)
            {
                progress(string.Empty, true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "GZipHelper-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            if (null != progress)
            {
                progress(string.Empty, false);
            }
        }
    }

    

    /// <summary>
    /// 通过www加载压缩文件解压出文件夹
    /// </summary>
    /// <param name="compressedFile">压缩文件完整路径</param>
    /// <param name="outDirPath">输出路径</param>
    /// <param name="progress">解压进度回调</param>
    public static void WWWDecompressToDirectory(string compressedFile, string outDirPath, ProgressDelegate progress)
    {
        SingletonBehaviour<ClientMain>.GetInst().StartCoroutine(/*GZipHelper.Inst.*/WWWDecompressToDir(compressedFile, outDirPath, progress));
    }

    #endregion

    #region 其他函数
    static void CompressFile(string sDir, string sRelativePath, GZipOutputStream zipStream)
    {
        //Compress file name
        char[] chars = sRelativePath.ToCharArray();
        zipStream.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
        foreach (char c in chars)
            zipStream.Write(BitConverter.GetBytes(c), 0, sizeof(char));

        //Compress file content
        byte[] bytes = File.ReadAllBytes(Path.Combine(sDir, sRelativePath));
        zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
        zipStream.Write(bytes, 0, bytes.Length);
    }

    static bool DecompressFile(string sDir, GZipInputStream zipStream, ProgressDelegate progress)
    {
        try
        {
            byte[] bytes = new byte[sizeof(int)];
            int Readed = zipStream.Read(bytes, 0, sizeof(int));
            if (Readed < sizeof(int))
                return false;

            int iNameLen = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[sizeof(char)];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < iNameLen; i++)
            {
                zipStream.Read(bytes, 0, sizeof(char));
                char c = BitConverter.ToChar(bytes, 0);
                sb.Append(c);
            }
            string sFileName = sb.ToString();
            if (progress != null)
                progress(sFileName, true);

            //Decompress file content
            bytes = new byte[sizeof(int)];
            zipStream.Read(bytes, 0, sizeof(int));
            int iFileLen = BitConverter.ToInt32(bytes, 0);

            bytes = new byte[iFileLen];
            zipStream.Read(bytes, 0, bytes.Length);

            string sFilePath = Path.Combine(sDir, sFileName).Replace("\\", "/");
            string sFinalDir = Path.GetDirectoryName(sFilePath).Replace("\\", "/");
            if (!Directory.Exists(sFinalDir))
                Directory.CreateDirectory(sFinalDir);

            using (FileStream outFile = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                outFile.Write(bytes, 0, iFileLen);
                outFile.Close();
            }
        }
        catch (System.Exception ex)
        {
            return false;
        }
        
            

        return true;
    }

    static IEnumerator WWWDecompressToDir(string compressedFile, string outDirPath, ProgressDelegate progress)
    {
        WWW www = new WWW(compressedFile);
        yield return www;
        if (null != www.error)
        {
            if (null != progress)
            {
                progress(string.Empty, false);
            }
        }
        else if (null != www.bytes)
        {
            MemoryStream inFile = new MemoryStream(www.bytes);
            using (GZipInputStream zipStream = new GZipInputStream(inFile))
            {
                while (DecompressFile(outDirPath, zipStream, progress));
                zipStream.Close();
            }
            inFile.Close();
            if (null != progress)
            {
                progress(string.Empty, true);
            }
        }
        else
        {
            if (null != progress)
            {
                progress(string.Empty, false);
            }
        }
    }

    /// <summary>
    /// 用www解压单个文件
    /// </summary>
    /// <param name="compressedFile">压缩文件路径</param>
    /// <param name="outDirPath">输出路径</param>
    /// <param name="progress">解压进度结果</param>
    /// <returns></returns>
    static IEnumerator WWWDecFile(string compressedFile, string outDirPath, ProgressDelegate progress)
    {
        WWW www = new WWW(compressedFile);
        yield return www;
        if (null != www.error)
        {
            if (null != progress)
            {
                progress(string.Empty, false);
            }
        }
        else if (null != www.bytes)
        {
            MemoryStream inFile = new MemoryStream(www.bytes);
            using (GZipInputStream zipStream = new GZipInputStream(inFile))
            {
                DecompressFile(outDirPath, zipStream, progress);
                zipStream.Close();
            }
            inFile.Close();
            if (null != progress)
            {
                progress(string.Empty, true);
            }
        }
        else
        {
            if (null != progress)
            {
                progress(compressedFile, false);
            }
        }
    }
    #endregion
}