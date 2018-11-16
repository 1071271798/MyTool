using Game.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:FilesCopy.cs
/// Description:文件复制,可复制指定文件或者文件夹目录下的文件
/// Time:2015/9/24 10:00:57
/// </summary>
public class FilesCopy
{
    #region 公有属性
    /// <summary>
    /// 文件保存结果
    /// </summary>
    /// <param name="result"></param>
    public delegate void FileCopyResult(string srcPath, bool result);
    #endregion

    #region 其他属性

    #endregion

    #region 公有函数
    public FilesCopy()
    {

    }
    /// <summary>
    /// 复制单个文件
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="destPath"></param>
    /// <param name="result"></param>
    public void CopyFile(string srcPath, string destPath, FileCopyResult result = null)
    {
        try
        {
            string destDirPath = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }
            SingletonBehaviour<ClientMain>.GetInst().StartCoroutine(WWWCopyFile(srcPath, destPath, result));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            if (null != result)
            {
                result(srcPath, false);
            }
        }
        
    }
    /// <summary>
    /// 复制文件夹
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="destPath"></param>
    /// <param name="result"></param>
    public void CopyFolder(string srcPath, string destPath, FileCopyResult result = null)
    {
        try
        {
            string destDirPath = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            if (null != result)
            {
                result(srcPath, false);
            }
        }
    }
    #endregion

    #region 其他函数
    IEnumerator WWWCopyFile(string url, string destPath, FileCopyResult result)
    {
        WWW www = new WWW(url);
        yield return www;
        if (null != www.error)
        {
            Debuger.LogError("WWWLoadFile error =" + www.error + "\nurl=" + url + "\ndestPath" + destPath);
            if (null != result)
            {
                result(url, false);
            }
        }
        else if (null != www.bytes)
        {
            SaveResFile(url, destPath, www.bytes, result);
        }
        else
        {
            if (null != result)
            {
                result(url, false);
            }
        }
        www.Dispose();
    }

    /// <summary>
    /// 保存资源文件
    /// </summary>
    /// <param name="path">mFilesPath下的路径</param>
    /// <param name="info">文件信息</param>
    private void SaveResFile(string srcPath, string path, byte[] info, FileCopyResult result)
    {
        try
        {
            Stream sw = null;
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
            {//文件不存在则创建
                sw = file.Create();
            }
            else
            {
                sw = file.Open(FileMode.Truncate);
            }
            sw.Flush();
            sw.Write(info, 0, info.Length);
            sw.Close();
            sw.Dispose();
            if (null != result)
            {
                result(srcPath, true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            if (null != result)
            {
                result(srcPath, false);
            }
        }
        
    }
    #endregion
}