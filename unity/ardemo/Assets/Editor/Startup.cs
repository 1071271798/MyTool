using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:Startup.cs
/// Description:
/// Time:2016/9/6 10:03:54
/// </summary>
[InitializeOnLoad]
public class Startup
{
    static Startup()
    {
        ProjectBuildConfig.GetInst().AutoBuild();
        Debug.Log("Up and running");
    }
}