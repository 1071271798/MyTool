using Game.Platform;
using Game.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:AndroidInterface.cs
/// Description:与安卓通讯接口
/// Time:2015/9/11 13:23:43
/// </summary>
public class AndroidInterface : PlatformInterface
{
#if UNITY_ANDROID
    #region 公有属性
    #endregion

    #region 其他属性

    AndroidJavaClass mJavaClass = null;
    AndroidJavaObject mJavaObj = null;

    #endregion

    #region 公有函数

    public AndroidInterface()
    {
        mJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        mJavaObj = mJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
    }
    public override void PlatformInit()
    {
        try
        {
            mJavaObj.Call("platformInit", mGameObjectName);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void PlatformDestroy()
    {
        try
        {
            mJavaObj.Call("platformDestroy");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 打开蓝牙
    /// </summary>
    public override void OpenBluetooth()
    {
        try
        {
            mJavaObj.Call("OpenBluetooth", mGameObjectName);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 关闭蓝牙 
    /// </summary>
    public override void CloseBluetooth()
    {
        try
        {
            mJavaObj.Call("Close");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 判断蓝牙是否打开
    /// </summary>
    /// <returns></returns>
    public override bool IsOpenBluetooth()
    {
        try
        {
            return mJavaObj.Call<bool>("IsOpenBluetooth");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return false;
    }

    public override void StartScan()
    {
        base.StartScan();
        try
        {
            mJavaObj.Call("startScan");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void StopScan()
    {
        base.StopScan();
        try
        {
            mJavaObj.Call("stopScan");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 连接蓝牙设备
    /// </summary>
    public override void ConnenctBluetooth(string mac)
    {
        try
        {
            mJavaObj.Call("ConnenctBluetooth", mac);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 断开蓝牙连接
    /// </summary>
    public override void DisConnenctBuletooth()
    {
        try
        {
            mJavaObj.Call("DisConnenct");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 取消连接
    /// </summary>
    public override void CancelConnectBluetooth()
    {
        try
        {
            mJavaObj.Call("cancelConnect");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="param"></param>
    /// <param name="len"></param>
    public override void SendMsg(byte cmd, byte[] param, int len)
    {
        try
        {
            mJavaObj.Call("SendMsg", new object[] { cmd, param, len });
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 保存/修改模型或者动作通知给应用
    /// </summary>
    /// <param name="contents"></param>
    public override void SaveModelOrActions(string contents)
    {
        try
        {
            mJavaObj.Call("SaveModelOrActions", contents);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 删除模型
    /// </summary>
    /// <param name="contents"></param>
    public override void DelModel(string contents)
    {
        try
        {
            mJavaObj.Call("DelModel", contents);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 返回应用
    /// </summary>
    public override void BackThirdApp()
    {
        try
        {
            mJavaObj.Call("ActivityFinish");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 拍照
    /// </summary>
    /// <param name="path"></param>
    public override void Photograph(string modleName, string path)
    {
        try
        {
            mJavaObj.Call("Photograph", modleName, path);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 保存模型
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="type">类型</param>
    public override void SaveModel(string name, ResFileType type = ResFileType.Type_playerdata)
    {
        try
        {
            mJavaObj.Call("SaveModel", name, (int)type);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 发布模型
    /// </summary>
    /// <param name="name"></param>
    public override void PublishModel(string name)
    {
        try
        {
            mJavaObj.Call("PublishModel", name);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 激活机器人
    /// </summary>
    /// <param name="mcuId"></param>
    /// <param name="sn"></param>
    public override void ActivationRobot(string mcuId, string sn)
    {
        try
        {
            mJavaObj.Call("ActivationRobot", mcuId, sn);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 通用接口
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="arg"></param>
    public override void CallPlatformFunc(string funcName, string arg)
    {
        try
        {
            mJavaObj.Call("CallPlatformFunc", funcName, arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 设置心跳包开关
    /// </summary>
    /// <param name="openFlag"></param>
    public override void SetSendXTState(bool openFlag)
    {
        try
        {
            if (openFlag)
            {
                mJavaObj.Call("SetSendXTState", 1);
            }
            else
            {
                mJavaObj.Call("SetSendXTState", 0);
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 获取平台数据
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public override string GetPlatformData(string dataType)
    {
        try
        {
            return mJavaObj.Call<string>("getUserData", dataType);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return base.GetPlatformData(dataType);
    }

    public override bool OperateSyncFile(string modelId, ResFileType modelType, string filePath, OperateFileType operateType)
    {
        try
        {
            return mJavaObj.Call<bool>("operateFile", modelId, (int)modelType, filePath, (int)operateType);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return base.OperateSyncFile(modelId, modelType, filePath, operateType);
    }

    /// <summary>
    /// 判断音响是否连接
    /// </summary>
    /// <param name="speaker"></param>
    /// <returns></returns>
    public override bool IsConnectedSpeaker(string speaker)
    {
        try
        {
            return mJavaObj.Call<bool>("isConnectedSpeaker");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return base.IsConnectedSpeaker(speaker);
    }

    public override void ConnectSpeaker(string speaker)
    {
        try
        {
            base.ConnectSpeaker(speaker);
            mJavaObj.Call("connectSpeaker", speaker);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void DisConnectSpeaker(string speaker)
    {
        try
        {
            base.DisConnectSpeaker(speaker);
            mJavaObj.Call("disConnectSpeaker", speaker);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// Event是重要日志
    /// </summary>
    /// <param name="text"></param>
    public override void LogEvent(string text)
    {
        try
        {
            Debug.Log(text);
            //mJavaObj.Call("unityLog", (int)MyLogType.LogTypeEvent, "Unity", text);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            MyLog.Log(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// Info是一般日志
    /// </summary>
    /// <param name="text"></param>
    public override void LogInfo(string text)
    {
        try
        {
            Debug.Log(text);
            //mJavaObj.Call("unityLog", (int)MyLogType.LogTypeInfo, "Unity", text);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            MyLog.Log(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// Debug是调试日志
    /// </summary>
    /// <param name="text"></param>
    public override void LogDebug(string text)
    {
        try
        {
            Debug.Log(text);
            //mJavaObj.Call("unityLog", (int)MyLogType.LogTypeDebug, "Unity", text);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            MyLog.Log(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public override string GetData(string dataType, string jsonString)
    {
        try
        {
            return mJavaObj.Call<string>("getData", dataType, jsonString);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return base.GetData(dataType, jsonString);
    }
    #endregion

    #region 其他函数
    #endregion
#endif
}