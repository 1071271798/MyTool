using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Resource;

#if UNITY_EDITOR
using MyTest;
#endif

/// <summary>
/// Author:xj
/// FileName:PlatformInterface.cs
/// Description:与平台通讯基类
/// Time:2015/9/11 13:12:16
/// 
/// </summary>
public class PlatformInterface
{
    #region 公有属性
    #endregion

    #region 其他属性
    protected string mGameObjectName = "MainClient";
    #endregion

    #region 公有函数

    public PlatformInterface()
    {

    }

    public virtual void PlatformInit()
    {

    }

    public virtual void PlatformDestroy()
    {

    }

    /// <summary>
    /// 打开蓝牙
    /// </summary>
    public virtual void OpenBluetooth()
    {

    }
    /// <summary>
    /// 关闭蓝牙 
    /// </summary>
    public virtual void CloseBluetooth()
    {

    }
    /// <summary>
    /// 打开蓝牙搜索
    /// </summary>
    public virtual void StartScan()
    {
#if UNITY_EDITOR
        SingletonObject<PlatformTestManager>.GetInst().StartScan();
#endif
    }
    /// <summary>
    /// 停止蓝牙搜索
    /// </summary>
    public virtual void StopScan()
    {
#if UNITY_EDITOR
        SingletonObject<PlatformTestManager>.GetInst().StopScan();
#endif
    }
    /// <summary>
    /// 判断蓝牙是否打开
    /// </summary>
    /// <returns></returns>
    public virtual bool IsOpenBluetooth()
    {
        return true;
    }
    /// <summary>
    /// 连接蓝牙设备
    /// </summary>
    public virtual void ConnenctBluetooth(string mac)
    {
#if UNITY_EDITOR
        SingletonObject<PlatformTestManager>.GetInst().ConnenctBluetooth(mac);
#endif
    }
    /// <summary>
    /// 断开蓝牙连接
    /// </summary>
    public virtual void DisConnenctBuletooth()
    {
#if UNITY_EDITOR
        SingletonObject<PlatformTestManager>.GetInst().DisConnenctBuletooth();
#endif
    }
    /// <summary>
    /// 取消连接
    /// </summary>
    public virtual void CancelConnectBluetooth()
    {

    }
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="param"></param>
    /// <param name="len"></param>
    public virtual void SendMsg(byte cmd, byte[] param, int len)
    {

    }
    /// <summary>
    /// 保存/修改模型或者动作通知给应用
    /// </summary>
    /// <param name="contents"></param>
    public virtual void SaveModelOrActions(string contents)
    {

    }
    /// <summary>
    /// 删除模型
    /// </summary>
    /// <param name="contents"></param>
    public virtual void DelModel(string contents)
    {

    }


    /// <summary>
    /// 返回应用
    /// </summary>
    public virtual void BackThirdApp()
    {

    }
    /// <summary>
    /// 拍照
    /// </summary>
    /// <param name="path"></param>
    public virtual void Photograph(string modelName, string path)
    {

    }
    /// <summary>
    /// 保存模型
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="type">类型</param>
    public virtual void SaveModel(string name, ResFileType type = ResFileType.Type_playerdata)
    {

    }
    /// <summary>
    /// 模型发布
    /// </summary>
    /// <param name="name"></param>
    public virtual void PublishModel(string name)
    {

    }
    /// <summary>
    /// 激活机器人
    /// </summary>
    /// <param name="mcuId"></param>
    /// <param name="sn"></param>
    public virtual void ActivationRobot(string mcuId, string sn)
    {

    }
    /// <summary>
    /// 通用接口
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="arg"></param>
    public virtual void CallPlatformFunc(string funcName, string arg)
    {
#if UNITY_EDITOR
        SingletonObject<PlatformTestManager>.GetInst().CallPlatformFunc(funcName, arg);
#endif
    }
    /// <summary>
    /// 设置心跳包开关
    /// </summary>
    /// <param name="openFlag"></param>
    public virtual void SetSendXTState(bool openFlag)
    {

    }
    /// <summary>
    /// 获取数据
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public virtual string GetPlatformData(string dataType)
    {
#if UNITY_EDITOR
        SingletonObject<PlatformTestManager>.GetInst().GetPlatformData(dataType);
#endif
        return string.Empty;
    }
    /// <summary>
    /// 判断音响是否连接
    /// </summary>
    /// <param name="speraker"></param>
    /// <returns></returns>
    public virtual bool IsConnectedSpeaker(string speaker)
    {
        return false;
    }

    /// <summary>
    /// 同步文件
    /// </summary>
    /// <param name="modelId">模型id</param>
    /// <param name="modelType">模型类型</param>
    /// <param name="filePath">模型路径</param>
    /// <param name="operateType">操作类型</param>
    /// <returns></returns>
    public virtual bool OperateSyncFile(string modelId, ResFileType modelType, string filePath, OperateFileType operateType)
    {
        return true;
    }

    /// <summary>
    /// 连接蓝牙音响
    /// </summary>
    /// <param name="speaker"></param>
    public virtual void ConnectSpeaker(string speaker)
    {


    }
    /// <summary>
    /// 断开蓝牙音响
    /// </summary>
    /// <param name="speaker"></param>
    public virtual void DisConnectSpeaker(string speaker)
    {

    }
    /// <summary>
    /// Event是重要日志
    /// </summary>
    /// <param name="text"></param>
    public virtual void LogEvent(string text)
    {
        Debuger.Log(text);
    }

    /// <summary>
    /// Info是一般日志
    /// </summary>
    /// <param name="text"></param>
    public virtual void LogInfo(string text)
    {
        Debuger.Log(text);
    }

    /// <summary>
    /// Debug是调试日志
    /// </summary>
    /// <param name="text"></param>
    public virtual void LogDebug(string text)
    {
        Debuger.Log(text);
    }

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public virtual string GetData(string dataType, string jsonString)
    {
#if UNITY_EDITOR
        SingletonObject<PlatformTestManager>.GetInst().GetData(dataType, jsonString);
#endif
        return string.Empty;
    }
    #endregion

    #region 其他函数
    #endregion
}