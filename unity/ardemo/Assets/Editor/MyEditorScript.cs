using System;
using UnityEngine;

public class MyEditorScript
{
    public static void MyMethod()
    {
        string jsonText = null;
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.Contains("buildState"))
            {
                jsonText = arg;
            }
            MyLog.Log(arg);
        }
        if (!string.IsNullOrEmpty(jsonText))
        {
            MyLog.Log("jsonText=" + jsonText);
            ProjectBuildConfig.GetInst().InitText(jsonText);
            ProjectBuildConfig.GetInst().AutoBuild();
        }
        
    }
}
