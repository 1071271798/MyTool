using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ProjectBuildConfig.cs
/// Description:
/// Time:2017/7/26 18:26:29
/// </summary>
public class ProjectBuildConfig : SingletonObject<ProjectBuildConfig>
{
    #region 公有属性
    #endregion

    #region 其他属性
    enum BuildState : byte
    {
        Build_None = 0,
        Build_Ready,
        Build_Ing,
        Build_Finished
    }

    enum BuildTarget : byte
    {
        Target_Ios = 1,
        Target_Android
    }
    string mConfigFilePath;
    BuildState mBuildState = BuildState.Build_None;
    string mBuildOutPath = string.Empty;
    BuildTarget mBuildTarget = BuildTarget.Target_Android;
    bool isAutoBuild = false;

    readonly string Build_State = "buildState";
    readonly string Out_Path = "outPath";
    readonly string Build_Target = "buildTarget";
    #endregion

    #region 公有函数
    public ProjectBuildConfig()
    {
        mConfigFilePath = Application.dataPath.Replace("\\", "/");
        mConfigFilePath = mConfigFilePath.Substring(0, mConfigFilePath.LastIndexOf("/Assets"));
        mConfigFilePath = mConfigFilePath.Substring(0, mConfigFilePath.LastIndexOf("/"));
        mConfigFilePath += "/build.txt";
        /*ReadConfig();*/
    }

    public bool NeedBuild()
    {
        if (mBuildState == BuildState.Build_Ready)
        {
            return true;
        }
        return false;
    }

    public bool IsAutoBuild()
    {
        return isAutoBuild;
    }

    public string GetBuildOutPath()
    {
        return mBuildOutPath;
    }

    public void BuildFinished()
    {
        if (mBuildState == BuildState.Build_Ing)
        {
            mBuildState = BuildState.Build_Finished;
            isAutoBuild = false;
            SaveConfig();
        }
    }

    public void AutoBuild()
    {
        if (NeedBuild())
        {
            switch (mBuildTarget)
            {
                case BuildTarget.Target_Android:
                    StartBuild();
                    MyProjectBuild.BuildAndroidGoogle(MyProjectBuild.AndroidChannel.none);
                    break;
                case BuildTarget.Target_Ios:
                    StartBuild();
                    MyProjectBuild.BuildForIOS();
                    break;
            }
        }
    }

    public void InitText(string text)
    {
        try
        {
            Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(text);
            if (null != data)
            {
                if (data.ContainsKey(Build_State))
                {
                    byte state = byte.Parse(data[Build_State].ToString());
                    mBuildState = (BuildState)state;
                }
                if (data.ContainsKey(Out_Path) && !string.IsNullOrEmpty(data[Out_Path].ToString()))
                {
                    mBuildOutPath = data[Out_Path].ToString();
                }
                if (data.ContainsKey(Build_Target) && !string.IsNullOrEmpty(data[Build_Target].ToString()))
                {
                    mBuildTarget = (BuildTarget)byte.Parse(data[Build_Target].ToString());
                }
                else
                {
                    mBuildState = BuildState.Build_None;
                }
            }
            MyLog.Log(string.Format("Build_State = {0} Build_Target = {1}", mBuildState, mBuildTarget));
        }
        catch (System.Exception ex)
        {
            mBuildState = BuildState.Build_None;
            MyLog.Log(ex.ToString());
        }
    }
    #endregion

    #region 其他函数
    void StartBuild()
    {
        isAutoBuild = true;
        mBuildState = BuildState.Build_Ing;
    }

    void ReadConfig()
    {
        try
        {
            string text = File.ReadAllText(mConfigFilePath);
            InitText(text);
        }
        catch (System.Exception ex)
        {
            mBuildState = BuildState.Build_None;
            MyLog.Log(ex.ToString());
        }
    }

    void SaveConfig()
    {
        try
        {
            /*Dictionary<string, object> dict = new Dictionary<string, object>();
            dict[Build_State] = (byte)mBuildState;
            dict[Out_Path] = mBuildOutPath;
            dict[Build_Target] = (byte)mBuildTarget;
            string text = Json.Serialize(dict);
            File.WriteAllText(mConfigFilePath, text);*/
            File.WriteAllText(mConfigFilePath.Replace("build.txt", "buildFinished.txt"), "1");
        }
        catch (System.Exception ex)
        {
            MyLog.LogError(ex.ToString());
        }
        
    }
    #endregion
}