using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
//
/// <summary>
/// Author:xj
/// FileName:MyProjectBuild.cs
/// Description:
/// 
/// Time:2016/1/11 10:41:02
/// </summary>
public class MyProjectBuild
{
    #region 公有属性
    #endregion

    #region 其他属性
    static string[] sDefaultAry = new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/default/qiluogan" };
    #endregion

    #region 公有函数


    #endregion

    #region 其他函数


    //在这里找出你当前工程所有的场景文件，假设你只想把部分的scene文件打包 那么这里可以写你的条件判断 总之返回一个字符串数组。
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }
    
    [MenuItem("ProjectBuild/Android/导出社区fir_test")]
    public static void BuildForGoogle_fir_test()
    {
        BuildAndroidGoogle(AndroidChannel.fir_test);
    }

    [MenuItem("ProjectBuild/Android/导出社区google_play")]
    public static void BuildForGoogle_google_play()
    {
        BuildAndroidGoogle(AndroidChannel.google_play);
    }

    [MenuItem("ProjectBuild/Android/导出社区official_website")]
    public static void BuildForGoogle_official_website()
    {
        BuildAndroidGoogle(AndroidChannel.official_website);
    }

    [MenuItem("ProjectBuild/Android/导出社区fir_customers")]
    public static void BuildForGoogle_fir_customers()
    {
        BuildAndroidGoogle(AndroidChannel.fir_customers);
    }

    [MenuItem("ProjectBuild/Android/导出社区build_all")]
    public static void BuildForGoogle_build_all()
    {
        BuildAndroidGoogle(AndroidChannel.build_all);
    }

    public enum AndroidChannel
    {
        none,
        fir_test,
        google_play,
        official_website,
        fir_customers,
        build_all,
    }
    public static void BuildAndroidGoogle(AndroidChannel channel)
    {
        //打包之前先设置一下 预定义标签， 我建议大家最好 做一些  91 同步推 快用 PP助手一类的标签。 这样在代码中可以灵活的开启 或者关闭 一些代码。
        //因为 这里我是承接 上一篇文章， 我就以sharesdk做例子 ，这样方便大家学习 ，
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, "USE_SHARE");
        try
        {
            MyLog.Log("start BuildAndroidGoogle");
            if (SetCommunity(false, false, BuildTarget.Android))
            {
                SetAndroidSetting();
                string path = Application.dataPath.Replace("\\", "/");
                path = path.Substring(0, path.LastIndexOf("/Assets"));
                path = path.Substring(0, path.LastIndexOf("/"));
                path += "/RobotAndroid";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    PublicFunction.DelDirector(Path.Combine(path, PlayerSettings.productName));
                }
                ETC1SeperateRGBA.SetImageFormat(TextureImporterFormat.ETC_RGB4);
                ChangeTextureFormat.SetPartsTextureFormat(TextureImporterFormat.ETC_RGB4);
                //path = EditorUtility.SaveFilePanel("选择导出路径", path, "", "");
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
                //EditorUtility.RevealInFinder(Path.Combine(path, PlayerSettings.productName));
                AndroidMoveFloder(Path.Combine(path, PlayerSettings.productName + "/assets"), channel);
                string outPath = ProjectBuildConfig.GetInst().GetBuildOutPath();
                if (!ProjectBuildConfig.GetInst().IsAutoBuild() || !string.IsNullOrEmpty(outPath))
                {
                    PublicFunction.DelDirector(path, true);
                }
            }
        }
        catch (System.Exception ex)
        {
            MyLog.LogError(ex.ToString());
        }
        ProjectBuildConfig.GetInst().BuildFinished();
    }

    [MenuItem("ProjectBuild/Android/导出社区fir_test打日志版")]
    public static void BuildForGoogleTest()
    {
        if (SetCommunity(true,false, BuildTarget.Android))
        {
            SetAndroidSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/RobotAndroid";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(Path.Combine(path, PlayerSettings.productName));
            }
            //path = EditorUtility.SaveFilePanel("选择导出路径", path, "", "");
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
            //EditorUtility.RevealInFinder(Path.Combine(path, PlayerSettings.productName));
            AndroidMoveFloder(Path.Combine(path, PlayerSettings.productName + "/assets/bin"), AndroidChannel.fir_test);
            PublicFunction.DelDirector(path, true);
        }
        
    }

    [MenuItem("ProjectBuild/Android/导出unity正式版")]
    static void BuildForAndroid()
    {
        if (SetUnity(false, BuildTarget.Android))
        {
            SetAndroidSetting();
            string path = "C:/Users/Public/Desktop";
            path = EditorUtility.SaveFilePanel("选择导出路径", path, "Jimu", "apk");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.None);
                EditorUtility.RevealInFinder(path);
            }
        }
    }

    [MenuItem("ProjectBuild/Android/导出unity打日志版")]
    static void BuildForAndroidTest()
    {
        if (SetUnity(true, BuildTarget.Android))
        {
            SetAndroidSetting();
            string path = "C:/Users/Public/Desktop";
            path = EditorUtility.SaveFilePanel("选择导出路径", path, "Jimu", "apk");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.None);
                EditorUtility.RevealInFinder(path);
            }
        }
    }

    [MenuItem("ProjectBuild/IOS/导出社区正式版")]
    public static void BuildForIOS()
    {
        try
        {
            MyLog.Log("start BuildForIOS");
            if (SetCommunity(false, false, BuildTarget.iOS))
            {
                SetIphoneSetting();
                string path;
                string outPath = ProjectBuildConfig.GetInst().GetBuildOutPath();
                if (ProjectBuildConfig.GetInst().IsAutoBuild() && !string.IsNullOrEmpty(outPath))
                {
                    path = outPath;
                }
                else
                {
                    path = Application.dataPath.Replace("\\", "/");
                    path = path.Substring(0, path.LastIndexOf("/Assets"));
                    path = path.Substring(0, path.LastIndexOf("/"));
                    path += "/robot_ios/Roobt_ios_sq";
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    PublicFunction.DelDirector(path);
                }
                if (!ProjectBuildConfig.GetInst().IsAutoBuild())
                {
                    path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
                }
                if (!string.IsNullOrEmpty(path))
                {
                    ETC1SeperateRGBA.SetImageFormat(TextureImporterFormat.PVRTC_RGB4);
                    ChangeTextureFormat.SetPartsTextureFormat(TextureImporterFormat.PVRTC_RGBA4);
                    BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iOS, BuildOptions.None);
                    IosFinishedDealFiles(path);
                    //EditorUtility.RevealInFinder(path);
                }
            }
        }
        catch (System.Exception ex)
        {
            MyLog.LogError(ex.ToString());
        }
        ProjectBuildConfig.GetInst().BuildFinished();
    }

    /*[MenuItem("ProjectBuild/IOS/导出社区带个人模型版")]
    public static void BuildForIOSAndModel()
    {
        if (SetCommunity(false, true, BuildTarget.iPhone))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Roobt_ios_sq";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {

                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iPhone, BuildOptions.None);
                IosFinishedDealFiles(path);
                EditorUtility.RevealInFinder(path);
            }
            
        }
        
    }*/

    [MenuItem("ProjectBuild/IOS/导出社区打日志版")]
    public static void BuildForIOSTest()
    {
        if (SetCommunity(true, false, BuildTarget.iOS))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Roobt_ios_sq";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iOS, BuildOptions.None);
                IosFinishedDealFiles(path);
                EditorUtility.RevealInFinder(path);
            }
            
        }
        
    }

    [MenuItem("ProjectBuild/IOS/导出unity正式版")]
    static void BuildForUnityIOS()
    {
        if (SetUnity(false, BuildTarget.iOS))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Robot_ios_unity";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iOS, BuildOptions.None);
                EditorUtility.RevealInFinder(path);
            }

        }
    }

    [MenuItem("ProjectBuild/IOS/导出unity打日志版")]
    static void BuildForUnityIOSTest()
    {
        if (SetUnity(true, BuildTarget.iOS))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Robot_ios_unity";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iOS, BuildOptions.None);
                EditorUtility.RevealInFinder(path);
            }

        }
    }


    static void DelFolder(string path, string [] notDel, bool delSelf)
    {
        try
        {
            if (Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);
                for (int i = dirs.Length - 1; i >= 0; --i)
                {
                    string tmpPath = dirs[i].Replace('\\', '/');
                    if (null != notDel)
                    {
                        bool delFlag = true;
                        for (int notIndex = 0, notMax = notDel.Length; notIndex < notMax; ++notIndex)
                        {
                            if (tmpPath.Equals(notDel[notIndex]))
                            {
                                delFlag = false;
                            }
                        }
                        if (delFlag)
                        {
                            DelFolder(tmpPath, notDel, true);
                        }
                    }
                    else
                    {
                        DelFolder(tmpPath, notDel, true);
                    }
                }
                string[] files = Directory.GetFiles(path);
                for (int i = files.Length - 1; i >= 0; --i)
                {
                    File.Delete(files[i]);
                }
                if (delSelf)
                {
                    string tmpPath = path.Replace('\\', '/');
                    bool delFlag = true;
                    for (int notIndex = 0, notMax = notDel.Length; notIndex < notMax; ++notIndex)
                    {
                        if (notDel[notIndex].StartsWith(tmpPath))
                        {
                            delFlag = false;
                        }
                    }
                    if (delFlag)
                    {
                        Directory.Delete(path);
                    }
                    if (File.Exists(path + ".meta"))
                    {
                        File.Delete(path + ".meta");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {

        }
    }

    static bool SetCommunity(bool isTest, bool haveModel, BuildTarget target)
    {
        LoadStartScene();
        if (!CheckDefault.CheckLauguageConfig())
        {
            MyLog.LogError("多语言配置文件缺少部分翻译");
            //return false;
        }

        ClientMain.GetInst().useThirdAppFlag = true;
        ClientMain.GetInst().simulationUseThirdAppFlag = false;
        ClientMain.GetInst().debugLogFlag = isTest;
        ClientMain.GetInst().copyDefaultFlag = false;
        /*if (haveModel)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/default", sDefaultAry, true);
        }
        else
        {
            DelFolder(Application.streamingAssetsPath, sDefaultAry, true);
        }*/
        DelFolder(Application.streamingAssetsPath + "/defaultFiles", null, true);
        /*if (target == BuildTarget.Android)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/parts", new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/parts/android" }, true);
        }
        else if (target == BuildTarget.iPhone)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/parts", new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/parts/ios" }, true);
        }*/
        AssetDatabase.Refresh();
        //CheckDefault.CreateDefaultConfigNoDialog();
        
        //AssetDatabase.Refresh();
        return true;
    }

    static bool SetUnity(bool isTest, BuildTarget target)
    {
        LoadStartScene();
        if (!CheckDefault.CheckLauguageConfig())
        {
            MyLog.LogError("多语言配置文件缺少部分翻译");
            //return false;
        }
        ClientMain.GetInst().useThirdAppFlag = false;
        ClientMain.GetInst().simulationUseThirdAppFlag = false;
        ClientMain.GetInst().debugLogFlag = isTest;
        ClientMain.GetInst().copyDefaultFlag = true;

        DelFolder(Application.streamingAssetsPath + "/defaultFiles", null, true);
        /*if (target == BuildTarget.Android)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/parts", new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/parts/android" }, true);
        }
        else if (target == BuildTarget.iPhone)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/parts", new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/parts/ios" }, true);
        }*/
        AssetDatabase.Refresh();
        /*CheckDefault.CreateDefaultConfigNoDialog();
        AssetDatabase.Refresh();*/
        return true;
    }
    static void SetAndroidSetting()
    {
        PlayerSettings.Android.forceSDCardPermission = true;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;
        PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.useAnimatedAutorotation = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
    }

    static void SetIphoneSetting()
    {
#if UNITY_IPHONE
        PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.IL2CPP, BuildTarget.iPhone);
        PlayerSettings.SetPropertyInt("Architecture", 2, BuildTarget.iPhone);
        PlayerSettings.targetIOSGraphics = TargetIOSGraphics.OpenGLES_2_0;
#endif
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.useAnimatedAutorotation = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
    }

    /// <summary>
    /// ios打包完以后处理部分文件
    /// </summary>
    static void IosFinishedDealFiles(string path)
    {
        /*string path = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets"));
		path = path.Substring(0, path.LastIndexOf("/"));
		path += "/robot_ios/Roobt_ios_sq";*/
        string keyboardSrc = "Classes/Keyboard.mm";
        string keyboardDest = "Classes/UI/Keyboard.mm";
        string[] files = Directory.GetFiles(Path.Combine(path, "Libraries"));
        if (null != files)
        {
            for (int i = 0, imax = files.Length; i < imax; ++i)
            {
                if (files[i].EndsWith(".meta"))
                {
                    File.Delete(files[i]);
                }
            }
        }
        try
        {
            string str = Path.Combine(path, keyboardSrc).Replace("\\", "/");
            string dest = Path.Combine(path, keyboardDest).Replace("\\", "/");
            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
            File.Move(str, dest);
        }
        catch (System.Exception ex)
        {
            MyLog.Log("IosFinishedDealFiles error = " + ex.ToString());
        }
    }

    static void LoadStartScene()
    {
        string sceneName = "Assets/Scene/startScene.unity";
        if (!EditorApplication.currentScene.Equals("sceneName"))
        {
            EditorApplication.OpenScene(sceneName);
        }
    }

    static void AndroidMoveFloder(string srcPath, AndroidChannel channel)
    {
        string destPath = string.Empty;
        string outPath = ProjectBuildConfig.GetInst().GetBuildOutPath();
        if (ProjectBuildConfig.GetInst().IsAutoBuild() && !string.IsNullOrEmpty(outPath))
        {
            destPath = outPath;
        }
        else
        {
            if (srcPath.Contains("Jimu-Unity"))
            {
                destPath = srcPath.Substring(0, srcPath.IndexOf("Jimu-Unity"));
                destPath += "Jimu-Android";
            }
            else if (srcPath.Contains("Jimu-Education-Unity"))
            {
                destPath = srcPath.Substring(0, srcPath.IndexOf("Jimu-Education-Unity"));
                destPath += "Jimu-Education-Android";
            }
        }
        
        if (!Directory.Exists(destPath))
        {
            return;
        }
        string batPath = destPath;
        destPath += "/app/src/main/assets/bin";
        if (!ProjectBuildConfig.GetInst().IsAutoBuild())
        {
            if (Directory.Exists(destPath))
            {
                PublicFunction.DelDirector(destPath);
            }
            else
            {
                Directory.CreateDirectory(destPath);
            }
            PublicFunction.CopyDirectory(srcPath, destPath);
        }
        string v7aPath = destPath.Replace("/assets/bin", "/jniLibs/armeabi-v7a");
        string x86Path = destPath.Replace("/assets/bin", "/jniLibs/x86");
        string v7aSrcPath = srcPath.Replace("\\", "/").Replace("/assets", "/libs/armeabi-v7a");
        string x86SrcPath = srcPath.Replace("\\", "/").Replace("/assets", "/libs/x86");
        PublicFunction.CopyDirectory(v7aSrcPath, v7aPath);
        PublicFunction.CopyDirectory(x86SrcPath, x86Path);
        switch (channel)
        {
            case AndroidChannel.fir_test:
                PublicFunction.OpenProcess(batPath, "fir_test", "bat");
                break;
            case AndroidChannel.google_play:
                PublicFunction.OpenProcess(batPath, "google_play", "bat");
                break;
            case AndroidChannel.official_website:
                PublicFunction.OpenProcess(batPath, "official_website", "bat");
                break;
            case AndroidChannel.fir_customers:
                PublicFunction.OpenProcess(batPath, "fir_customers", "bat");
                break;
            case AndroidChannel.build_all:
                PublicFunction.OpenProcess(batPath, "build_all", "bat");
                break;
        }
        
    }
    /*[PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log(pathToBuiltProject);
    }*/
#endregion
}