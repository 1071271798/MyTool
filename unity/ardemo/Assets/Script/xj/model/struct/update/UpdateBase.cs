using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateBase.cs
/// Description:升级的基类
/// Time:2016/11/23 14:18:16
/// </summary>
public class UpdateBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    protected byte[] mUpdateFile;
    protected ushort mSendFrameIndex = 0;
    protected ushort mFrameTotalNum = 0;
    protected string mFilePath = string.Empty;
    protected string mFileName = string.Empty;
    protected string mVersion = string.Empty;
    protected Robot mRobot;
    protected TopologyPartType mPartType = TopologyPartType.None;
    #endregion

    #region 公有函数

    public UpdateBase()
    {
        Init();
    }

    public UpdateBase(TopologyPartType partType)
    {
        mPartType = partType;
        Init();
    }
    /// <summary>
    /// 检查升级
    /// </summary>
    /// <param name="msg"></param>
    /// /// <returns></returns>
    public virtual ErrorCode CheckUpdate(ReadMotherboardDataMsgAck msg)
    {
        return ErrorCode.Result_OK;
    }
    /// <summary>
    /// 开始升级
    /// </summary>
    /// <param name="robot"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public virtual bool UpdateStart(Robot robot, byte arg)
    {
        mRobot = robot;
        InitUpdateFile();
        if (null == mUpdateFile)
        {
            return false;
        }
        return true;
    }

    public virtual void SendFrame()
    {
    }

    public bool IsWriteFinished()
    {
        if (mSendFrameIndex >= mFrameTotalNum)
        {
            return true;
        }
        return false;
    }
    public void UpdateProgress()
    {
        float per = mSendFrameIndex / (mFrameTotalNum + 0.0f) * 100;
        int perInt = (int)per;
        if (perInt <= 0) perInt = 0;
        else if (perInt >= 100) perInt = 99;
        EventMgr.Inst.Fire(EventID.Update_Progress, new EventArg(perInt));
    }

    public void CleanUp()
    {
        mUpdateFile = null;
        mSendFrameIndex = 0;
        mFrameTotalNum = 0;
        mFilePath = string.Empty;
        mVersion = string.Empty;
        mFileName = string.Empty;
    }
    #endregion

    #region 其他函数

    protected virtual void Init()
    {
        mVersion = UpdateManager.GetInst().GetHardwareVersion(mPartType);
        mFilePath = UpdateManager.GetInst().GetHardwarePath(mPartType);
        if (File.Exists(mFilePath))
        {
            mFileName = Path.GetFileNameWithoutExtension(mFilePath);
        }
        else
        {
            mFileName = mFilePath;
        }
    }

    /// <summary>
    /// 获取下一帧数据
    /// </summary>
    /// <returns></returns>
    protected byte[] GetNextFrame()
    {
        int len = 100;
        if (mSendFrameIndex < mFrameTotalNum)
        {
            if (mSendFrameIndex == mFrameTotalNum - 1)
            {
                len = mUpdateFile.Length - 100 * mSendFrameIndex;
            }
            byte[] bytes = new byte[len];
            Array.Copy(mUpdateFile, mSendFrameIndex * 100, bytes, 0, len);
            ++mSendFrameIndex;
            return bytes;
        }
        return null;
    }

    protected void InitUpdateFile()
    {
        mUpdateFile = ReadUpdateFile(mFilePath);
        if (null != mUpdateFile)
        {
            mFrameTotalNum = (ushort)(mUpdateFile.Length / 100);
            if (mUpdateFile.Length % 100 != 0)
            {
                mFrameTotalNum += 1;
            }
            mSendFrameIndex = 0;
        }
    }

    /// <summary>
    /// 读取升级文件内容
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    protected byte[] ReadUpdateFile(string path)
    {
        byte[] bytes = null;
        try
        {
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                bytes = new byte[fs.Length];
                try
                {
                    fs.Read(bytes, 0, bytes.Length);
                }
                catch (Exception e)
                {
                    MyLog.Log(string.Format("读取文件失败了，path = {0} Exception = {1}", path, e.ToString()));
                    bytes = null;
                }
                fs.Dispose();
                fs.Close();
            }
            else
            {
                TextAsset text = Resources.Load<TextAsset>(path);
                if (null != text)
                {
                    bytes = text.bytes;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return bytes;
    }
    #endregion
}