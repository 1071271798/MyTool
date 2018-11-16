using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:SetServosConnection.cs
/// Description:设置舵机连接情况
/// Time:2016/4/5 10:37:53
/// </summary>
public class SetServosConnection : EditorWindow
{
    #region 公有属性
    #endregion

    #region 其他属性
    EditRobot mRobot = null;
    List<int> mServos = null;
    Dictionary<int, List<int>> mServosDict = null;
    int mNewPort;
    string mNewServos;
    ServosConnection mServosConnection;
    string mRobotFolderPath;
    #endregion

    #region 公有函数
    /*[MenuItem("MyTool/设置舵机连接情况", false, 1)]
    public static void OpenSetServosConnection()
    {
        SetServosConnection windows = EditorWindow.GetWindow<SetServosConnection>(true, "SetServosConnection");
        windows.position = new Rect(400, 300, 500, 550);


    }*/
    #endregion

    #region 其他函数
    void OnGUI()
    {
        string folderPath = string.Empty;
        if (GUILayout.Button("选择模型", GUILayout.Width(100), GUILayout.Height(20)))
        {
            folderPath = EditorUtility.OpenFolderPanel("选择模型", mRobotFolderPath, string.Empty);
        }
        if (!string.IsNullOrEmpty(folderPath))
        {//选中了模型
            if (PublicFunction.IsSameNameXml(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                if (null != files)
                {
                    for (int i = 0, imax = files.Length; i < imax; i++)
                    {
                        if (files[i].EndsWith(".xml"))
                        {
                            mRobotFolderPath = folderPath;
                            mRobot = new EditRobot(folderPath, Path.GetFileNameWithoutExtension(files[i]));
                            mServos = mRobot.GetServos();
                            mNewPort = 0;
                            mNewServos = string.Empty;
                            mServosConnection = new ServosConnection();
                            mServosConnection.Read(mRobotFolderPath);
                            mServosDict = new Dictionary<int, List<int>>();
                            List<int> ports = mServosConnection.GetPortList();
                            if (null != ports)
                            {
                                for (int portIndex = 0, portMax = ports.Count; portIndex < portMax; ++portIndex)
                                {
                                    List<int> tmpServos = mServosConnection.GetServosForPort(ports[portIndex]);
                                    if (null != tmpServos)
                                    {
                                        mServosDict[ports[portIndex]] = tmpServos;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "未发现模型", "确定");
            }
        }
        if (null != mServos)
        {
            GUILayout.Label("舵机ID:" + PublicFunction.ListToString<int>(mServos), GUILayout.Width(480));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("主板接口:", GUILayout.Width(60));
            mNewPort = EditorGUILayout.IntField(mNewPort, GUILayout.Width(20));
            GUILayout.Label("舵机ID:", GUILayout.Width(60));
            mNewServos = EditorGUILayout.TextField(mNewServos, GUILayout.Width(260));
            if (GUILayout.Button("增加", GUILayout.Width(60)))
            {
                if (mNewPort < 1 || mNewPort > 5)
                {
                    EditorUtility.DisplayDialog("错误", "错误的主板接口", "确定");
                }
                else
                {
                    List<int> tmpServos = StrToServos(mNewServos);
                    if (tmpServos.Count < 1)
                    {
                        EditorUtility.DisplayDialog("错误", "输入舵机为空", "确定");
                    }
                    else if (CheckServosID(tmpServos))
                    {
                        ErrorCode ret = mServosConnection.AddPortServos(mNewPort, tmpServos);
                        if (ErrorCode.Result_Port_Exist == ret)
                        {
                            EditorUtility.DisplayDialog("错误", "主板接口已使用", "确定");
                        }
                        else if (ErrorCode.Result_DJ_ID_Repeat == ret)
                        {
                            EditorUtility.DisplayDialog("错误", "重复的舵机ID", "确定");
                        }
                        else if (ErrorCode.Result_OK == ret)
                        {
                            mServosDict[mNewPort] = tmpServos;
                        }
                        mNewPort = 0;
                        mNewServos = string.Empty;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "输入了不存在的舵机ID", "确定");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (null != mServosDict)
            {
                List<int> delList = null;
                foreach (KeyValuePair<int, List<int>> kvp in mServosDict)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("主板接口:" + kvp.Key, GUILayout.Width(100));
                    GUILayout.Label("舵机ID:" + PublicFunction.ListToString<int>(kvp.Value));
                    if (GUILayout.Button("删除", GUILayout.Width(100)))
                    {
                        if (null == delList)
                        {
                            delList = new List<int>();
                        }
                        delList.Add(kvp.Key);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (null != delList)
                {
                    for (int i = 0, imax = delList.Count; i < imax; ++i)
                    {
                        mServosDict.Remove(delList[i]);
                        mServosConnection.DelPortServos(delList[i]);
                    }
                }
            }

            if (GUILayout.Button("完成"))
            {
                int useServosNum = 0;
                foreach (KeyValuePair<int, List<int>> kvp in mServosDict)
                {
                    for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
                    {
                        ++useServosNum;
                    }
                }
                if (useServosNum != mServos.Count)
                {
                    EditorUtility.DisplayDialog("错误", "有舵机未设置", "确定");
                    return;
                }
                mServosConnection.Save(mRobotFolderPath);
                mRobot = null;
                mServos = null;
                mRobotFolderPath = string.Empty;
                mNewPort = 0;
                mNewServos = string.Empty;
                mServosConnection = null;
                mServosDict = null;
            }
        }

        
    }

    List<int> StrToServos(string str)
    {
        List<int> list = new List<int>();
        string[] servos = null;
        if (str.Contains("-"))
        {
            servos = str.Split('-');
        }
        else if (str.Contains("."))
        {
            servos = str.Split('.');
        }
        else if (str.Contains(" "))
        {
            servos = str.Split(' ');
        }
        else
        {
            servos = new string[] { str };
        }
        if (null != servos && servos.Length > 0)
        {
            for (int i = 0, imax = servos.Length; i < imax; ++i)
            {
                if (PublicFunction.IsInteger(servos[i]))
                {
                    list.Add(int.Parse(servos[i]));
                }
            }
        }
        return list;
    }

    bool CheckServosID(List<int> list)
    {
        if (null == mServos)
        {
            return false;
        }
        for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            if (!mServos.Contains(list[i]))
            {
                return false;
            }
        }
        return true;
    }
    #endregion
}