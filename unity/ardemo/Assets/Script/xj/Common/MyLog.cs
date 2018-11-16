using System;
using UnityEngine;
using System.IO;
using Game.Resource;
using System.Text;
using System.Threading;

/// <summary>
/// Author:xj
/// FileName:MyLog.cs
/// Description:
/// Time:2015/12/29 18:50:51
/// </summary>
public class MyLog
{
    #region 公有属性
    #endregion

    #region 其他属性
    FileStream mFileStream;
    StreamWriter mStreamWriter;
    static MyLog mInst;

    #endregion

    #region 公有函数


    static public bool EnableLog = true;

    static public bool EnableLocalFile = true;



    public MyLog()
    {
        mInst = this;
        string path = ResourcesEx.persistentDataPath + "/log.txt";
        if (File.Exists(path))
        {
            mInst.mFileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }
        else
        {
            mInst.mFileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        }
        mInst.mStreamWriter = new StreamWriter(mInst.mFileStream);
    }

    public static void CloseMyLog()
    {
        if (null != mInst)
        {
            mInst.mStreamWriter.Dispose();
            mInst.mStreamWriter.Close();
            mInst.mFileStream.Dispose();
            mInst.mFileStream.Close();
            mInst = null;
        }
    }
    static public void Log(object message)
    {
        Log(message, null);
    }


    static public void Log(object message, UnityEngine.Object context)
    {
        if (EnableLog)
        {
            Debug.Log(message, context);
            if (EnableLocalFile)
            {
                MyLogWrite(message.ToString());
            }
        }
    }


    static public void LogError(object message)
    {
        LogError(message, null);
    }


    static public void LogError(object message, UnityEngine.Object context)
    {
        if (EnableLog)
        {
            Debug.LogError(message, context);
            if (EnableLocalFile)
            {
                MyLogWrite(message.ToString());
            }
        }
    }


    static public void LogWarning(object message)
    {
        LogWarning(message, null);
    }


    static public void LogWarning(object message, UnityEngine.Object context)
    {
        if (EnableLog)
        {
            Debug.LogWarning(message, context);
            if (EnableLocalFile)
            {
                MyLogWrite(message.ToString());
            }
        }
    }
    #endregion

    #region 其他函数

    static void MyLogWrite(string text)
    {
        text = DateTime.Now.ToLocalTime() + " " + text;
        if (null == mInst)
        {
            mInst = new MyLog();
        }
        mInst.mStreamWriter.WriteLine(text);
        mInst.mStreamWriter.Flush();
    }


    #endregion
}