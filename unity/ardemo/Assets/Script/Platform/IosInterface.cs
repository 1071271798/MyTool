using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Game.Resource;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:IosInterface.cs
/// Description:与ios通讯接口
/// Time:2015/9/11 13:39:12
/// </summary>
public class IosInterface : PlatformInterface
{

#if UNITY_IPHONE
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数

    public IosInterface()
    {

    }

    public override void PlatformInit()
    {
        try
        {
            IosPlatformInit(mGameObjectName);
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
            IosPlatformDestroy();
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
            IosOpenBluetooth(mGameObjectName);
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
            IosCloseBluetooth();
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
            return IosIsOpenBluetooth();
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
        try
        {
            IosStartScan();
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
            IosStopScan();
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
            IosConnenctBluetooth(mac);
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
            IosDisConnenctBuletooth();
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
            IosCannelConnectBluetooth();
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
            IosSendMsg(cmd, param, len);
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
            IosSaveModelOrActions(contents);
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
            IosDelModel(contents);
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "退出unity");
            IosBackThirdApp();
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
    public override void Photograph(string modelName, string path)
    {
        try
        {
            IosPhotograph(modelName, path);
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
            IosSaveModel(name, (int)type);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void PublishModel(string name)
    {
        try
        {
            IosPublishModel(name);
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
            IosActivationRobot(mcuId, sn);
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
            IosCallPlatformFunc(funcName, arg);
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
                IosSetSendXTState(1);
            }
            else
            {
                IosSetSendXTState(0);
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    
    public override string GetPlatformData(string dataType)
    {
        try
        {
            return IosGetUserData(dataType);
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
            return operateFile(modelId, (int)modelType, filePath, (int)operateType);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return base.OperateSyncFile(modelId, modelType, filePath, operateType);
    }
    public override bool IsConnectedSpeaker(string speaker)
    {
        try
        {
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
            connectSpeaker(speaker);
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
            IosLogEvent(text);
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
            IosLogInfo(text);
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
            IosLogDebug(text);
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
            return IosGetData(dataType, jsonString);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            MyLog.Log(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    return base.GetData(dataType, jsonString);
    }
    #endregion

    #region 其他函数
    [DllImport("__Internal")]
    private static extern void IosPlatformInit(string gameObjectName);
    [DllImport("__Internal")]
    private static extern void IosPlatformDestroy();
    [DllImport("__Internal")]
    private static extern void IosOpenBluetooth(string gameObjectName);
    [DllImport("__Internal")]
    private static extern void IosCloseBluetooth();
    [DllImport("__Internal")]
    private static extern bool IosIsOpenBluetooth();
    [DllImport("__Internal")]
    private static extern void IosStartScan();
    [DllImport("__Internal")]
    private static extern void IosStopScan();
    [DllImport("__Internal")]
    private static extern void IosConnenctBluetooth(string mac);
    [DllImport("__Internal")]
    private static extern void IosDisConnenctBuletooth();
    [DllImport("__Internal")]
    private static extern void IosCannelConnectBluetooth();
    [DllImport("__Internal")]
    private static extern void IosSendMsg(byte cmd, byte[] param, int len);
    [DllImport("__Internal")]
    private static extern void IosSaveModelOrActions(string contents);
    [DllImport("__Internal")]
    private static extern void IosDelModel(string contents);
    [DllImport("__Internal")]
    private static extern void IosBackThirdApp();
    [DllImport("__Internal")]
    private static extern void IosPhotograph(string name, string path);
    [DllImport("__Internal")]
    private static extern void IosSaveModel(string name, int type);
    [DllImport("__Internal")]
    private static extern void IosPublishModel(string name);
    [DllImport("__Internal")]
    private static extern void IosActivationRobot(string mcuId, string sn);
    [DllImport("__Internal")]
    private static extern void IosCallPlatformFunc(string funcName, string arg);
    [DllImport("__Internal")]
    private static extern void IosSetSendXTState(int state);
    [DllImport("__Internal")]
    private static extern string IosGetUserData(string dataType);
    [DllImport("__Internal")]
    private static extern void connectSpeaker(string speakerMac);
    [DllImport("__Internal")]
    private static extern bool operateFile(string modelId, int modelType, string filePath, int operateType);
    [DllImport("__Internal")]
    private static extern void IosLogEvent(string str);
    [DllImport("__Internal")]
    private static extern void IosLogInfo(string str);
    [DllImport("__Internal")]
    private static extern void IosLogDebug(string str);
    [DllImport("__Internal")]
    private static extern string IosGetData(string dataType, string jsonString);
    #endregion
#endif
}