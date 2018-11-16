using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
/// <summary>
/// Author:xj
/// FileName:MyToolEditor.cs
/// Description:
/// Time:2016/10/22 17:19:37
/// </summary>
public class MyToolEditor : EditorWindow
{
    #region 公有属性
    public delegate void ReadTextCallBack(string text);
    #endregion

    #region 其他属性
    string mTextFilePath;
    List<MsgData> mMsgList;
    int mReceiveCount;
    Dictionary<CMDCode, Int2> mCostTimeDict;
    #endregion

    #region 公有函数
    [MenuItem("MyTool/工具集合")]
    public static void OpenCopyFilesEditor()
    {
        MyToolEditor windows = EditorWindow.GetWindow<MyToolEditor>(true, "MyToolEditor");
        windows.position = new Rect(400, 100, 700, 900);
    }

    public MyToolEditor()
    {
        
    }
    #endregion

    #region 其他函数
    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("-------------------------------命令分析-------------------------------", GUILayout.Width(500));
        if (GUILayout.Button("选择文件", GUILayout.Width(80)))
        {
            mTextFilePath = EditorUtility.OpenFilePanel("选择文件", mTextFilePath, "txt");
            if (!string.IsNullOrEmpty(mTextFilePath))
            {
                mMsgList = new List<MsgData>();
                mReceiveCount = 0;
                mCostTimeDict = new Dictionary<CMDCode, Int2>();
                MsgData msgData = null;
                ReadText(mTextFilePath, delegate (string text) {
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (text.Contains("|send time ="))
                        {//发送
                            if (null != msgData && msgData.cmd != CMDCode.Hand_Shake)
                            {
                                mMsgList.Add(msgData);
                            }
                            msgData = ToSendData(text);
                        } else if (text.Contains("AlphaInfoClientHandler:272|receive:"))
                        {//接收
                            MsgData tmpData = ToReceiveData(text);
                            if (null != tmpData && null != msgData && tmpData.cmd == msgData.cmd && msgData.cmd != CMDCode.Hand_Shake)
                            {
                                msgData.receiveTime = tmpData.receiveTime;
                                mMsgList.Add(msgData);
                                msgData = null;
                            }
                        }
                    }
                });
                for (int i = 0, imax = mMsgList.Count; i < imax; ++i)
                {
                    if (null != mMsgList[i].receiveTime)
                    {
                        mReceiveCount++;
                        if (mCostTimeDict.ContainsKey(mMsgList[i].cmd))
                        {
                            Int2 int2;
                            int2.num1 = mCostTimeDict[mMsgList[i].cmd].num1 + 1;
                            int2.num2 = mCostTimeDict[mMsgList[i].cmd].num2 + mMsgList[i].GetCostTime();
                            mCostTimeDict[mMsgList[i].cmd] = int2;
                        } else
                        {
                            Int2 int2;
                            int2.num1 = 1;
                            int2.num2 = mMsgList[i].GetCostTime();
                            mCostTimeDict[mMsgList[i].cmd] = int2;
                        }

                    }
                }
            }
        }
        if (null != mMsgList)
        {
            GUILayout.Label("发送命令总数：" + mMsgList.Count, GUILayout.Width(300));
            GUILayout.Label("接收命令总数：" + mReceiveCount, GUILayout.Width(300));
            if (null != mCostTimeDict)
            {
                foreach (var kvp in mCostTimeDict)
                {
                    GUILayout.Label(kvp.Key + "(" + PublicFunction.BytesToHexString(new byte[] { (byte)kvp.Key }) + ") 次数：" + kvp.Value.num1 + " 平均时间：" + (kvp.Value.num2 / kvp.Value.num1) / 10000.0f + "ms", GUILayout.Width(300));
                }
            }
        }
        VersionShow();
        HtmlCrawl();
        ColorAnalyze();
        GUILayout.EndVertical();
    }
    string htmlUrl = string.Empty;

    void HtmlCrawl()
    {
        GUILayout.Label("-------------------------------网页内容抓取-------------------------------", GUILayout.Width(500));
        GUILayout.BeginHorizontal();
        GUILayout.Label("网页URL：", GUILayout.Width(80));
        htmlUrl = EditorGUILayout.TextField(htmlUrl, GUILayout.Width(200));
        GUILayout.EndHorizontal();
        if (!string.IsNullOrEmpty(htmlUrl))
        {
            if (GUILayout.Button("抓取内容", GUILayout.Width(80)))
            {
                HtmlContentCrawl crawl = new HtmlContentCrawl(htmlUrl);
            }
        }
    }

    void VersionShow()
    {
        GUILayout.Label("-------------------------------版本号-------------------------------", GUILayout.Width(500));
        GUILayout.Label("version : " + SingletonObject<VersionTool>.GetInst().GetVersion());
        if (GUILayout.Button("高版本加1", GUILayout.Width(80)))
        {
            SingletonObject<VersionTool>.GetInst().AddHightVersion();
        }
        if (GUILayout.Button("中版本加1", GUILayout.Width(80)))
        {
            SingletonObject<VersionTool>.GetInst().AddMidVersion();
        }
        if (GUILayout.Button("低版本加1", GUILayout.Width(80)))
        {
            SingletonObject<VersionTool>.GetInst().AddLowVersion();
        }
    }

    List<string> mColorList = null;
    Dictionary<string, Int2> mColorResultDict = null;
    string[] colorAry = new string[] { "红", "橙", "黄", "绿", "青", "蓝", "紫", "白", "黑", "灰" };
    void ColorAnalyze()
    {
        GUILayout.Label("-------------------------------颜色分析-------------------------------", GUILayout.Width(500));
        if (GUILayout.Button("重新读取", GUILayout.Width(80)))
        {
            mColorList = null;
            mColorResultDict = null;
            SingletonObject<ColorCollectTool>.GetInst().RestoreReadText();
        }
        if (null == mColorList)
        {
            mColorList = SingletonObject<ColorCollectTool>.GetInst().GetColorList();
        }
        if (null == mColorResultDict)
        {
            mColorResultDict = SingletonObject<ColorCollectTool>.GetInst().GetColorResult();
        }
        if (null != mColorList)
        {
            string color = null;
            for (int i = 0, imax = mColorList.Count; i < imax; ++i)
            {
                color = mColorList[i];
                GUILayout.Label(string.Format(colorAry[PublicFunction.GetColorIndex(color)] + "  正确次数： " + mColorResultDict[color].num1 + " 总次数：" + mColorResultDict[color].num2 + " 正确率：" + mColorResultDict[color].num1 / (mColorResultDict[color].num2 + 0.0f)));
            }
            GUILayout.Label("====================success=======================");
            for (int i = 0, imax = mColorList.Count; i < imax; ++i)
            {
                color = mColorList[i];
                if (!SingletonObject<ColorCollectTool>.GetInst().HaveSuccess(color))
                {
                    continue;
                }
                GUILayout.BeginHorizontal();
                int colorIndex = PublicFunction.GetColorIndex(color);
                if (-1 != colorIndex)
                {
                    GUILayout.Label(colorAry[colorIndex] + ": ", GUILayout.Width(20));
                } else
                {
                    GUILayout.Label(color + "：", GUILayout.Width(20));
                }
                GUILayout.Label((SingletonObject<ColorCollectTool>.GetInst().GetNowSuccessColorIndex(color) + 1) + "/" + mColorResultDict[color].num1, GUILayout.Width(60));
                if (GUILayout.Button("上一步", GUILayout.Width(60)))
                {
                    SingletonObject<ColorCollectTool>.GetInst().MoveSuccessFront(color);
                }
                ColorCollectData data = SingletonObject<ColorCollectTool>.GetInst().GetNowSuccessColor(color);
                if (null != data)
                {
                    EditorGUILayout.ColorField(new Color32(data.r, data.g, data.b, 255), GUILayout.Width(200));
                    int index = PublicFunction.GetColorIndex(data.convertColor);
                    if (-1 != index)
                    {
                        GUILayout.Label("识别为：" + colorAry[index], GUILayout.Width(80));
                    } else
                    {
                        GUILayout.Label("未识别颜色", GUILayout.Width(80));
                    }
                    GUILayout.Label("目标颜色修改为：", GUILayout.Width(100));
                    int targetColorIndex = -1;
                    targetColorIndex = EditorGUILayout.Popup(targetColorIndex, colorAry, GUILayout.Width(60));
                    if (targetColorIndex != -1)
                    {
                        mColorList = null;
                        mColorResultDict = null;
                        SingletonObject<ColorCollectTool>.GetInst().ChangeNowSuccessTargetColor(color, PublicFunction.colorAry[targetColorIndex]);
                        break;
                    }
                }
                if (GUILayout.Button("下一步", GUILayout.Width(60)))
                {
                    SingletonObject<ColorCollectTool>.GetInst().MoveSuccessNext(color);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("====================failed=======================");
            if (null != mColorList)
            {
                for (int i = 0, imax = mColorList.Count; i < imax; ++i)
                {
                    color = mColorList[i];
                    if (!SingletonObject<ColorCollectTool>.GetInst().HaveFailed(color))
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal();
                    int colorIndex = PublicFunction.GetColorIndex(color);
                    if (-1 != colorIndex)
                    {
                        GUILayout.Label(colorAry[colorIndex] + ": ", GUILayout.Width(20));
                    }
                    else
                    {
                        GUILayout.Label(color + "：", GUILayout.Width(20));
                    }
                    GUILayout.Label((SingletonObject<ColorCollectTool>.GetInst().GetNowFailedColorIndex(color) + 1) + "/" + (mColorResultDict[color].num2 - mColorResultDict[color].num1), GUILayout.Width(60));
                    if (GUILayout.Button("上一步", GUILayout.Width(60)))
                    {
                        SingletonObject<ColorCollectTool>.GetInst().MoveFailedFront(color);
                    }
                    ColorCollectData data = SingletonObject<ColorCollectTool>.GetInst().GetNowFailedColor(color);
                    if (null != data)
                    {
                        EditorGUILayout.ColorField(new Color32(data.r, data.g, data.b, 255), GUILayout.Width(200));
                        int index = PublicFunction.GetColorIndex(data.convertColor);
                        if (-1 != index)
                        {
                            GUILayout.Label("识别为：" + colorAry[index], GUILayout.Width(80));
                        }
                        else
                        {
                            GUILayout.Label("未识别颜色", GUILayout.Width(80));
                        }
                        GUILayout.Label("目标颜色修改为：", GUILayout.Width(100));
                        int targetColorIndex = -1;
                        targetColorIndex = EditorGUILayout.Popup(targetColorIndex, colorAry, GUILayout.Width(60));
                        if (targetColorIndex != -1)
                        {
                            mColorList = null;
                            mColorResultDict = null;
                            SingletonObject<ColorCollectTool>.GetInst().ChangeNowFailedTargetColor(color, PublicFunction.colorAry[targetColorIndex]);
                            break;
                        }
                    }
                    if (GUILayout.Button("下一步", GUILayout.Width(60)))
                    {
                        SingletonObject<ColorCollectTool>.GetInst().MoveFailedNext(color);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            
            if (GUILayout.Button("重新识别", GUILayout.Width(60)))
            {
                mColorList = null;
                mColorResultDict = null;
                SingletonObject<ColorCollectTool>.GetInst().RestoreCalculation();
            }
            if (GUILayout.Button("重新保存", GUILayout.Width(60)))
            {
                SingletonObject<ColorCollectTool>.GetInst().ApplicationQuit();
            }
        }
        
    }







    void ReadText(string filePath, ReadTextCallBack callback)
    {
        try
        {
            if (File.Exists(filePath))
            {
                FileInfo file = new FileInfo(filePath);
                StreamReader sr = new StreamReader(file.Open(FileMode.Open));
                string readStr = null;
                while (null != (readStr = sr.ReadLine()))
                {
                    if (null != callback)
                    {
                        callback(readStr);
                    }
                }
                sr.Dispose();
                sr.Close();
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }





    public class MsgData
    {
        public DateTime sendTime;
        public CMDCode cmd;
        public DateTime receiveTime;

        public MsgData()
        {

        }

        public int GetCostTime()
        {
            if (null == sendTime || null == receiveTime)
            {
                return 0;
            }
            return (int)(receiveTime.Ticks - sendTime.Ticks);
        }
    }

    MsgData ToSendData(string text)
    {
        string[] args = text.Split('|');
        if (args.Length == 6)
        {
            MsgData data = new MsgData();
            data.sendTime = ToDate(args[0]);
            if (args[5].Contains("cmd="))
            {
                string cmdStr = args[5].Substring(args[5].IndexOf("cmd=") + 4);
                cmdStr = cmdStr.Substring(0, cmdStr.IndexOf(" "));
                if (Enum.IsDefined(typeof(CMDCode), cmdStr))
                {
                    data.cmd = (CMDCode)Enum.Parse(typeof(CMDCode), cmdStr);
                }
            }
            return data;
        }
        return null;
    }

    MsgData ToReceiveData(string text)
    {
        string[] args = text.Split('|');
        if (args.Length == 6)
        {
            MsgData data = new MsgData();
            data.receiveTime = ToDate(args[0]);
            if (args[5].Contains("receive:FB BF"))
            {
                string cmdStr = args[5].Substring("receive:FB BF ".Length + 3);
                cmdStr = cmdStr.Substring(0, cmdStr.IndexOf(" "));
                byte[] bytes = PublicFunction.HexStringToBytes(cmdStr);

                if (bytes != null && bytes.Length == 1)
                {
                    data.cmd = (CMDCode)bytes[0];
                }
            }
            return data;
        }
        return null;
    }

    DateTime ToDate(string text)
    {
        text = "2018-" + text;
        string dateStr = text.Substring(0, text.LastIndexOf(":"));
        DateTime date = Convert.ToDateTime(dateStr);
        string ms = text.Substring(text.LastIndexOf(":") + 1);
        if (PublicFunction.IsInteger(ms))
        {
            date = date.AddMilliseconds(int.Parse(ms));
        }
        return date;
    }

    
    #endregion

}