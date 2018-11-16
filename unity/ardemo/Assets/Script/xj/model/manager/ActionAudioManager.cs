using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Resource;
using System.IO;
using Game.Platform;
using LitJson;
/// <summary>
/// Author:xj
/// FileName:ActionAudioManager.cs
/// Description:
/// Time:2017/3/28 13:57:10
/// </summary>
public class ActionAudioManager : SingletonObject<ActionAudioManager>
{
    #region 公有属性
    #endregion

    #region 其他属性
    enum ActionAudionState : byte
    {
        Play,
        Pause,
        Continue,
        Stop
    }
    Dictionary<string, Dictionary<string, string>> mActionAudioDic;
    #endregion

    #region 公有函数
    public ActionAudioManager()
    {

    }

    public void ReadRobotAudio(Robot robot)
    {
        try
        {
            if (null != mActionAudioDic)
            {
                mActionAudioDic.Clear();
            }
             
            string dirPath = string.Empty;
            if (ResourcesEx.GetRobotType(robot) == ResFileType.Type_default)
            {
                dirPath = ResourcesEx.GetRobotCommonPath(robot.Name) + "/sounds";
            }
            else
            {
                dirPath = ResourcesEx.GetRobotPath(robot.Name) + "/sounds";
            }
            if (Directory.Exists(dirPath))
            {
                string[] files = Directory.GetFiles(dirPath);
                if (null != files)
                {
                    for (int i = 0, imax = files.Length; i < imax; ++i)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(files[i]);
                        if (null == mActionAudioDic)
                        {
                            mActionAudioDic = new Dictionary<string, Dictionary<string, string>>();
                        }
                        if (!mActionAudioDic.ContainsKey(robot.ID))
                        {
                            Dictionary<string, string> dict = new Dictionary<string, string>();
                            mActionAudioDic[robot.ID] = dict;
                        }
                        mActionAudioDic[robot.ID][fileName] = PublicFunction.ConvertSlashPath(files[i]);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


    public string GetActionAudioPath(string robotId, string actionId)
    {
        if (null != mActionAudioDic)
        {
            if (mActionAudioDic.ContainsKey(robotId) && mActionAudioDic[robotId].ContainsKey(actionId))
            {
                return mActionAudioDic[robotId][actionId];
            }
        }
        return null;
    }

    public void PlayActionAudio(string robotId, string actionId)
    {
        try
        {
            if (null != mActionAudioDic && mActionAudioDic.ContainsKey(robotId) && mActionAudioDic[robotId].ContainsKey(actionId))
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["state"] = ActionAudionState.Play.ToString();
                dict["path"] = mActionAudioDic[robotId][actionId];//ResourcesEx.persistentDataPath + "/Normal_Btn.wav";//
                string jsonBill = Json.Serialize(dict);
                PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.playActionAudio, jsonBill);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    #endregion

    #region 其他函数
    #endregion
}