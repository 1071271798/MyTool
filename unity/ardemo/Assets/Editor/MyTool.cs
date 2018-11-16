using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:MyTool.cs
/// Description:
/// Time:2016/9/18 15:00:51
/// </summary>
public class MyTool
{
    #region 公有属性
    [MenuItem("MyTool/打开外部路径")]
    static void OpenExtendPath()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    #endregion

    #region 其他函数
    [MenuItem("MyTool/输出crc", false, 3)]
    static void OutCrc()
    {
        //以独占方式打开一个文件
        FileStream fs = new FileStream(Game.Resource.ResourcesEx.streamingAssetsPath + "/jimu2_app_41161301.bin", FileMode.Open, FileAccess.Read, FileShare.None);
        //创建一个Byte用来存放读取到的文件内容
        byte[] bytes = new byte[fs.Length];

        //定义变量存储当前数据剩余未读的长度
        int remaining = bytes.Length / 2;
        try
        {
            fs.Read(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            bytes = null;
        }
        fs.Dispose();
        fs.Close();

        byte[] bytes1 = null;
        TextAsset text = Resources.Load<TextAsset>("jimu2_app_41161301");
        if (null != text)
        {
            bytes1 = text.bytes;
        }



        byte[] mRobotUpdateData;
        //Encoding.ASCII.GetBytes(File.ReadAllText(Game.Resource.ResourcesEx.streamingAssetsPath + "/jimu2_app_41161301.bin", Encoding.ASCII));
        // byte[] bytes = File.ReadAllBytes(Game.Resource.ResourcesEx.streamingAssetsPath + "/jimu2_app_41161301.bin");
        //byte[] bytes = Encoding.ASCII.GetBytes(File.ReadAllText(Game.Resource.ResourcesEx.streamingAssetsPath + "/jimu2_app_41154901.bin", Encoding.ASCII));//File.ReadAllBytes(Game.Resource.ResourcesEx.streamingAssetsPath + "/jimu2_app_41161301.bin");
        /*StreamReader sr = new StreamReader(Game.Resource.ResourcesEx.streamingAssetsPath + "/jimu2_app_41161301.bin", Encoding.ASCII);
        string text = sr.ReadToEnd();
        byte[] bytes = Encoding.ASCII.GetBytes(text);*/
        int mod = bytes.Length % 64;
        if (mod != 0)
        {
            mRobotUpdateData = new byte[bytes.Length + 64 - mod];
            //mRobotUpdateData = new byte[(bytes.Length/64+1)*64];
        }
        else
        {
            mRobotUpdateData = new byte[bytes.Length];
        }
        Array.Copy(bytes, mRobotUpdateData, bytes.Length);
        Debug.Log(PublicFunction.GetCRC32Str(mRobotUpdateData));
        byte[] outAry = BitConverter.GetBytes(PublicFunction.GetCRC32Str(mRobotUpdateData));
        Debug.Log(PublicFunction.BytesToHexString(outAry));

        int mod1 = bytes1.Length % 64;
        if (mod1 != 0)
        {
            mRobotUpdateData = new byte[bytes1.Length + 64 - mod1];
            //mRobotUpdateData = new byte[(bytes.Length/64+1)*64];
        }
        else
        {
            mRobotUpdateData = new byte[bytes1.Length];
        }
        Array.Copy(bytes1, mRobotUpdateData, bytes1.Length);
        Debug.Log(PublicFunction.GetCRC32Str(mRobotUpdateData));
        byte[] outAry1 = BitConverter.GetBytes(PublicFunction.GetCRC32Str(mRobotUpdateData));
        Debug.Log(PublicFunction.BytesToHexString(outAry1));
    }

    [MenuItem("MyTool/复制连接图", false, 4)]
    static void MoveTopologyData()
    {
        string srcPath = Path.Combine(Application.persistentDataPath, "default");
        string[] destPath = { "E:/Jimu文档/07开发/官方模型/default", "E:/Jimu文档/07开发/草原漫步者模型/default", "E:/Jimu文档/07开发/6舵机版/default", "E:/Jimu文档/07开发/教育版模型/default", "E:/Jimu文档/07开发/Tankbot", "E:/Jimu文档/07开发/丛林飞车系列/default" };
        Dictionary<string, string> srcData = new Dictionary<string, string>();
        string[] srcDir = Directory.GetDirectories(srcPath);
        if (null != srcDir)
        {
            for (int i = 0, imax = srcDir.Length; i < imax; ++i)
            {
                string src = srcDir[i].Replace("\\", "/");
                string floderName = src.Substring(src.LastIndexOf('/') + 1);
                srcData[floderName] = src;
            }
        }
        if (srcData.Count > 0)
        {
            for (int i = 0, imax = destPath.Length; i < imax; ++i)
            {
                MoveFloderData(destPath[i], srcData);
            }
            EditorUtility.DisplayDialog("提示", "复制完毕", "确定");
        }
    }

    [MenuItem("MyTool/设置预设按钮为默认颜色", false, 5)]
    static void ChangeButtonColor()
    {
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        if (null == objs || objs.Length < 1)
        {
            EditorUtility.DisplayDialog("错误", "请选择需要查找的文件或文件夹", "确定");
        }
        else
        {
            for (int objIndex = 0, objMax = objs.Length; objIndex < objMax; ++objIndex)
            {
                PrefabType pType = PrefabUtility.GetPrefabType(objs[objIndex]);
                if (pType == PrefabType.Prefab)
                {
                    GameObject prefabGo = objs[objIndex] as GameObject;
                    if (prefabGo != null)
                    {
                        Component[] coms = prefabGo.GetComponentsInChildren(typeof(UIButtonColor), true);
                        if (null != coms && coms.Length > 0)
                        {
                            GameObject prefabAsset = GameObject.Instantiate(prefabGo) as GameObject;
                            coms = prefabAsset.GetComponentsInChildren(typeof(UIButtonColor), true);
                            for (int comIndex = 0, comMax = coms.Length; comIndex < comMax; ++comIndex)
                            {
                                UIButtonColor btn = coms[comIndex] as UIButtonColor;
                                btn.pressed = PublicFunction.Button_Press_Color;
                                btn.hover = PublicFunction.Button_Hover_Color;
                                btn.disabledColor = PublicFunction.Button_Disabled_Color;
                            }
                            Debug.Log(AssetDatabase.GetAssetPath(prefabGo));
                            PrefabUtility.ReplacePrefab(prefabAsset, prefabGo, ReplacePrefabOptions.ConnectToPrefab);
                            GameObject.DestroyImmediate(prefabAsset);
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("提示", "修改完毕", "确定");
        }
    }
    [MenuItem("MyTool/设置GameObject按钮为默认颜色", false, 6)]
    static void ChangeObjButtonColor()
    {
        GameObject selectGo = Selection.activeGameObject;
        if (null != selectGo)
        {
            Component[] coms = selectGo.GetComponentsInChildren(typeof(UIButtonColor), true);

            if (null != coms && coms.Length > 0)
            {
                for (int comIndex = 0, comMax = coms.Length; comIndex < comMax; ++comIndex)
                {
                    UIButtonColor btn = coms[comIndex] as UIButtonColor;
                    btn.pressed = PublicFunction.Button_Press_Color;
                    btn.hover = PublicFunction.Button_Hover_Color;
                    btn.disabledColor = PublicFunction.Button_Disabled_Color;
                }
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("提示", "修改完毕", "确定");
            }
            
        } else
        {
            EditorUtility.DisplayDialog("错误", "请选中GameObject", "确定");
        }
        
    }
    [MenuItem("MyTool/隐藏预设的ButtonScale", false, 7)]
    static void HideButtonScale()
    {
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        if (null == objs || objs.Length < 1)
        {
            EditorUtility.DisplayDialog("错误", "请选择需要查找的文件或文件夹", "确定");
        }
        else
        {
            for (int objIndex = 0, objMax = objs.Length; objIndex < objMax; ++objIndex)
            {
                PrefabType pType = PrefabUtility.GetPrefabType(objs[objIndex]);
                if (pType == PrefabType.Prefab)
                {
                    GameObject prefabGo = objs[objIndex] as GameObject;
                    if (prefabGo != null)
                    {
                        Component[] coms = prefabGo.GetComponentsInChildren(typeof(UIButtonScale), true);
                        if (null != coms && coms.Length > 0)
                        {
                            GameObject prefabAsset = GameObject.Instantiate(prefabGo) as GameObject;
                            coms = prefabAsset.GetComponentsInChildren(typeof(UIButtonScale), true);
                            for (int comIndex = 0, comMax = coms.Length; comIndex < comMax; ++comIndex)
                            {
                                UIButtonScale btn = coms[comIndex] as UIButtonScale;
                                btn.enabled = false;
                            }
                            Debug.Log(AssetDatabase.GetAssetPath(prefabGo));
                            PrefabUtility.ReplacePrefab(prefabAsset, prefabGo, ReplacePrefabOptions.ConnectToPrefab);
                            GameObject.DestroyImmediate(prefabAsset);
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("提示", "修改完毕", "确定");
        }
    }
    static void MoveFloderData(string dest, Dictionary<string, string> srcData)
    {
        string[] destDir = Directory.GetDirectories(dest);
        if (null != destDir)
        {
            for (int i = 0 ,imax = destDir.Length; i < imax; ++i)
            {
                string tmp = destDir[i].Replace("\\", "/");
                string floderName = tmp.Substring(tmp.LastIndexOf('/') + 1);
                if (srcData.ContainsKey(floderName))
                {
                    MoveOneModeServoData(srcData[floderName], tmp);
                }
            }
        }
    }
    static void MoveOneModeServoData(string src, string dest)
    {
        string servoSrcPath = Path.Combine(src, "servos/servos.txt");
        string servoDestPath = Path.Combine(dest, "servos/servos.txt");
        if (File.Exists(servoSrcPath))
        {
            string destDirPath = Path.GetDirectoryName(servoDestPath);
            if (File.Exists(servoDestPath))
            {
                File.Delete(servoDestPath);
            }
            else if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }
            File.Copy(servoSrcPath, servoDestPath);
        }
    }
    #endregion
}