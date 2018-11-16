using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Game;
using System.Data;
using MyMVC;
using System.Text;
using System.IO;

/// <summary>
/// Author:xj
/// FileName:LauguageConvert.cs
/// Description:
/// Time:2016/1/5 14:02:54
/// </summary>
public class LauguageConvert : EditorWindow
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    [MenuItem("MyTool/文本处理")]
    public static void MyLauguageConvert()
    {
        LauguageConvert windows = EditorWindow.GetWindow<LauguageConvert>(true, "LauguageConvert");
        windows.position = new Rect(400, 300, 700, 550);
    }


    LauguageType StringToLauguageType(string str)
    {
        int index = -1;
        for (int i = 0, imax = lauguageTypeStr.Length; i < imax; ++i)
        {
            if (str.Equals(lauguageTypeStr[i]))
            {
                index = i;
                break;
            }
        }
        if (-1 != index)
        {
            return lauguageTypes[index];
        }
        return lauguageTypes[0];
    }

    LauguageConvert()
    {
        Array ary = Enum.GetValues(typeof(LauguageType));
        lauguageTypeStr = new string[ary.Length - 1];
        lauguageTypes = new LauguageType[ary.Length - 1];
        for (int i = 0, imax = ary.Length; i < imax; ++i)
        {
            LauguageType tmp = (LauguageType)ary.GetValue(i);
            if (tmp != LauguageType.Ohter)
            {
                lauguageTypeStr[i] = tmp.ToString();
                lauguageTypes[i] = tmp;
            }
        }
    }
    #endregion

    #region 其他函数
    TextCell cell = new TextCell();
    string[] lauguageTypeStr = null;// new string[] { LauguageType.Chinese.ToString(), LauguageType.English.ToString(), LauguageType.Japanese.ToString(), LauguageType.German.ToString(), LauguageType.Italy.ToString(), LauguageType.French.ToString(), LauguageType.Spanish.ToString(), LauguageType.Portugal.ToString(), LauguageType.HokongChinese.ToString(), LauguageType.Arab.ToString() };
    LauguageType[] lauguageTypes = null;// new LauguageType[] { LauguageType.Chinese, LauguageType.English, LauguageType.Japanese, LauguageType.German, LauguageType.Italy, LauguageType.French, LauguageType.Spanish, LauguageType.Portugal, LauguageType.HokongChinese, LauguageType.Arab };
    //bool addLauguageTypeFlag = false;
    void OnGUI()
    {
        if (cell.lauguages.Count == 0)
        {
            for (int i = 0, imax = lauguageTypes.Length; i < imax; ++i)
            {
                Lauguage lg = new Lauguage();
                lg.lauType = lauguageTypes[i];
                cell.lauguages.Add(lg);
            }
        }
        LauguageCellGui(cell);
    }

    void UpdateCell(TextCell cell, TextCell newCell)
    {
        cell.key = newCell.key;
        for (int i = 0, imax = cell.lauguages.Count; i < imax; ++i)
        {
            for (int newIndex = 0, newMax = newCell.lauguages.Count; newIndex < newMax; ++newIndex)
            {
                if (cell.lauguages[i].lauType == newCell.lauguages[newIndex].lauType)
                {
                    cell.lauguages[i].value = newCell.lauguages[newIndex].value;
                }
            }
            
        }
    }
    bool mFindKeyFlag = false;
    bool mFindLauguageFlag = false;
    int mSelectedFindIndex = -1;
    TextCell[] mFindCell;
    string[] mFindCellText;
    void LauguageCellGui(TextCell cell)
    {
        GUILayout.BeginHorizontal();
        cell.key = EditorGUILayout.TextField("key", cell.key, GUILayout.Width(600));
        
        if (!string.IsNullOrEmpty(cell.key) && GUILayout.Button("搜索", GUILayout.Width(40), GUILayout.Height(20)))
        {
            mFindCell = LauguageTool.GetIns().FindAllForKey(cell.key);
            if (null != mFindCell)
            {
                mFindKeyFlag = true;
                mFindCellText = new string[mFindCell.Length];
                for (int i = 0, imax = mFindCell.Length; i < imax; ++i)
                {
                    mFindCellText[i] = mFindCell[i].key;
                }
                //UpdateCell(cell, findCell);
            }
        }
        GUILayout.EndHorizontal();
        if (mFindKeyFlag)
        {
            mSelectedFindIndex = GUILayout.SelectionGrid(mSelectedFindIndex, mFindCellText, 1);
            if (mSelectedFindIndex >= 0)
            {
                mFindKeyFlag = false;
                UpdateCell(cell, mFindCell[mSelectedFindIndex]);
                mSelectedFindIndex = -1;
                mFindCell = null;
                mFindCellText = null;
            }
        }
        
        for (int i = 0, imax = cell.lauguages.Count; i < imax; ++i)
        {
            GUILayout.BeginHorizontal();
            cell.lauguages[i].value = EditorGUILayout.TextField(cell.lauguages[i].lauType.ToString(), cell.lauguages[i].value, GUILayout.Width(600));
            if (!string.IsNullOrEmpty(cell.lauguages[i].value) && GUILayout.Button("搜索", GUILayout.Width(40), GUILayout.Height(20)))
            {
                mFindCell = LauguageTool.GetIns().FindAllForLauguage(cell.lauguages[i]);
                if (null != mFindCell)
                {
                    mFindLauguageFlag = true;
                    mFindCellText = new string[mFindCell.Length];
                    for (int j = 0, jmax = mFindCell.Length; j < jmax; ++j)
                    {
                        mFindCellText[j] = mFindCell[j].lauguages[i].value;
                    }
                    //UpdateCell(cell, findCell);
                }
            }
            GUILayout.EndHorizontal();
        }
        if (mFindLauguageFlag)
        {
            mSelectedFindIndex = GUILayout.SelectionGrid(mSelectedFindIndex, mFindCellText, 1);
            if (mSelectedFindIndex >= 0)
            {
                mFindLauguageFlag = false;
                UpdateCell(cell, mFindCell[mSelectedFindIndex]);
                mSelectedFindIndex = -1;
                mFindCell = null;
                mFindCellText = null;
            }
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("增加/修改", GUILayout.Width(100), GUILayout.Height(40)))
        {
            if (!string.IsNullOrEmpty(cell.key))
            {
                TextCell tmp = new TextCell();
                tmp.key = cell.key;
                for (int i = 0, imax = cell.lauguages.Count; i < imax; ++i)
                {
                    Lauguage lg = new Lauguage();
                    lg.lauType = cell.lauguages[i].lauType;
                    lg.value = cell.lauguages[i].value;
                    tmp.lauguages.Add(lg);
                }
                bool addFlag = LauguageTool.GetIns().AddLauguage(tmp);
                LauguageTool.GetIns().Save();
                cell.key = string.Empty;
                for (int i = 0, imax = cell.lauguages.Count; i < imax; ++i)
                {
                    cell.lauguages[i].value = string.Empty;
                }
                if (addFlag)
                {
                    EditorUtility.DisplayDialog("成功", "添加成功", "确定");
                }
                else
                {
                    EditorUtility.DisplayDialog("成功", "修改成功", "确定");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "key不能为空", "确定");
            }
            //addLauguageTypeFlag = true;
        }

        if (GUILayout.Button("删除", GUILayout.Width(100), GUILayout.Height(40)))
        {
            
            if (!string.IsNullOrEmpty(cell.key))
            {
                LauguageTool.GetIns().DelLauguage(cell);
                LauguageTool.GetIns().Save();
                cell.key = string.Empty;
                for (int i = 0, imax = cell.lauguages.Count; i < imax; ++i)
                {
                    cell.lauguages[i].value = string.Empty;
                }
                EditorUtility.DisplayDialog("成功", "删除成功", "确定");
            }
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("导出CSV", GUILayout.Width(100), GUILayout.Height(40)))
        {
            TextAsset text = Resources.Load("TextConfig", typeof(TextAsset)) as TextAsset;
            if (null != text)
            {   
                LauguageConfig lauguageConfig = XmlHelper.XmlDeserialize<LauguageConfig>(text.text.Trim(), Encoding.UTF8);
                if (null != lauguageConfig && lauguageConfig.TextCells.Count > 0)
                {
                    DataTable tblDatas = new DataTable();
                    DataColumn dc = null;
                    //赋值给dc，是便于对每一个datacolumn的操作
                    dc = tblDatas.Columns.Add("ID", Type.GetType("System.Int32"));
                    dc.AutoIncrement = true;//自动增加
                    dc.AutoIncrementSeed = 1;//起始为1
                    dc.AutoIncrementStep = 1;//步长为1
                    dc.AllowDBNull = false;//

                    dc = tblDatas.Columns.Add("Key", Type.GetType("System.String"));
                    for (int i = 0, imax = lauguageTypeStr.Length; i < imax; ++i)
                    {
                        dc = tblDatas.Columns.Add(lauguageTypeStr[i], Type.GetType("System.String"));
                    }

                    for (int i = 0, imax = lauguageConfig.TextCells.Count; i < imax; ++i)
                    {
                        TextCell tmp = lauguageConfig.TextCells[i];
                        DataRow newRow;
                        newRow = tblDatas.NewRow();
                        newRow["Key"] = tmp.key;
                        for (int j = 0, jmax = lauguageTypeStr.Length; j < jmax; ++j)
                        {
                            if (tmp.lauguages.Count > j)
                            {
                                newRow[lauguageTypeStr[j]] = tmp.lauguages[j].value;
                            }

                        }
                        tblDatas.Rows.Add(newRow);
                    }
                    CSVFileHelper.SaveCSV(tblDatas, Application.dataPath + "/Resources/TextConfig.csv");
                }
                
            }
            EditorUtility.RevealInFinder(Application.dataPath + "/Resources/TextConfig.csv");

        }
        if (GUILayout.Button("导入翻译", GUILayout.Width(100), GUILayout.Height(40)))
        {
            string folderPath = EditorUtility.OpenFilePanel("选择翻译文件", string.Empty, string.Empty);
            if (!string.IsNullOrEmpty(folderPath) && Path.GetExtension(folderPath).Equals(".txt"))
            {
                FileStream fs = new FileStream(folderPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs);
                string text = sr.ReadLine();
                string[] arys = text.Split('\t');
                int keyIndex = -1;
                for (int i = 0, imax = arys.Length; i < imax; ++i)
                {
                    if (arys[i].Equals("Key"))
                    {
                        keyIndex = i;
                        break;
                    }
                }
                if (-1 != keyIndex)
                {
                    LauguageTool.CleanUp();
                    while (!string.IsNullOrEmpty(text = sr.ReadLine()))
                    {
                        string[] lgstr = text.Split('\t');
                        if (null != lgstr && keyIndex < lgstr.Length)
                        {
                            TextCell tmp = new TextCell();
                            tmp.key = lgstr[keyIndex].Trim().TrimStart('"').TrimEnd('"');
                            if (string.IsNullOrEmpty(tmp.key))
                            {
                                continue;
                            }
                            for (int i = 0, imax = lgstr.Length; i < imax; ++i)
                            {
                                if (i != keyIndex && i < arys.Length && !string.IsNullOrEmpty(lgstr[i]))
                                {
                                    Lauguage lg = new Lauguage();
                                    lg.lauType = StringToLauguageType(arys[i]);
                                    lg.value = lgstr[i].Trim().TrimStart('"').TrimEnd('"');
                                    tmp.lauguages.Add(lg);
                                }
                            }
                            bool addFlag = LauguageTool.GetIns().AddLauguage(tmp);
                        }
                    }
                    LauguageTool.GetIns().Save();
                    EditorUtility.DisplayDialog("成功", "写入成功", "确定");
                }
                else
                {
                    EditorUtility.DisplayDialog("失败", "key不存在，请检查文本", "确定");
                }
            }
        }
        else if (GUILayout.Button("导入有key值的新增语言翻译", GUILayout.Width(100), GUILayout.Height(40)))
        {
            string folderPath = EditorUtility.OpenFilePanel("选择翻译文件", string.Empty, string.Empty);
            if (!string.IsNullOrEmpty(folderPath) && Path.GetExtension(folderPath).Equals(".txt"))
            {
                FileStream fs = new FileStream(folderPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs);
                string text = sr.ReadLine();
                string[] arys = text.Split('\t');
                int keyIndex = -1;
                for (int i = 0, imax = arys.Length; i < imax; ++i)
                {
                    if (arys[i].Equals("key"))
                    {
                        keyIndex = i;
                        break;
                    }
                }
                if (-1 != keyIndex)
                {
                    while (!string.IsNullOrEmpty(text = sr.ReadLine()))
                    {
                        string[] lgstr = text.Split('\t');
                        if (null != lgstr && keyIndex < lgstr.Length)
                        {
                            TextCell tmp = new TextCell();
                            tmp.key = lgstr[keyIndex].Trim().TrimStart('"').TrimEnd('"');
                            if (string.IsNullOrEmpty(tmp.key))
                            {
                                continue;
                            }
                            for (int i = 0, imax = lgstr.Length; i < imax; ++i)
                            {
                                if (i != keyIndex && i < arys.Length && !string.IsNullOrEmpty(lgstr[i]))
                                {
                                    Lauguage lg = new Lauguage();
                                    lg.lauType = StringToLauguageType(arys[i]);
                                    lg.value = lgstr[i].Trim().TrimStart('"').TrimEnd('"');
                                    tmp.lauguages.Add(lg);
                                }
                            }
                            bool addFlag = LauguageTool.GetIns().AddLauguage(tmp);
                        }
                    }
                    LauguageTool.GetIns().Save();
                    EditorUtility.DisplayDialog("成功", "写入成功", "确定");
                }
                else
                {
                    EditorUtility.DisplayDialog("失败", "key不存在，请检查文本", "确定");
                }
            }
        }
        else if (GUILayout.Button("导入新增语言翻译", GUILayout.Width(100), GUILayout.Height(40)))
        {
            string folderPath = EditorUtility.OpenFilePanel("选择翻译文件", string.Empty, string.Empty);
            if (!string.IsNullOrEmpty(folderPath) && Path.GetExtension(folderPath).Equals(".txt"))
            {
                FileStream fs = new FileStream(folderPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs);
                string text = sr.ReadLine();
                string[] arys = text.Split('\t');
                int keyIndex = -1;
                for (int i = 0, imax = arys.Length; i < imax; ++i)
                {
                    if (arys[i].Equals("Chinese"))
                    {
                        keyIndex = i;
                        break;
                    }
                }
                if (-1 != keyIndex)
                {
                    while (!string.IsNullOrEmpty(text = sr.ReadLine()))
                    {
                        string[] lgstr = text.Split('\t');
                        if (null != lgstr && keyIndex < lgstr.Length)
                        {
                            TextCell tmp = new TextCell();
                            Lauguage lg = new Lauguage();
                            lg.lauType = StringToLauguageType(arys[keyIndex]);
                            lg.value = lgstr[keyIndex].TrimStart('"').TrimEnd('"').Trim();
                            tmp.lauguages.Add(lg);
                            TextCell findTextCell = LauguageTool.GetIns().FindForLauguage(lg);
                            if (null != findTextCell)
                            {
                                tmp.key = findTextCell.key;
                                for (int i = 0, imax = lgstr.Length; i < imax; ++i)
                                {
                                    if (i != keyIndex && i < arys.Length && !string.IsNullOrEmpty(lgstr[i]))
                                    {
                                        Lauguage lg1 = new Lauguage();
                                        lg1.lauType = StringToLauguageType(arys[i]);
                                        lg1.value = lgstr[i].Trim().TrimStart('"').TrimEnd('"');
                                        tmp.lauguages.Add(lg1);
                                    }
                                }
                                LauguageTool.GetIns().AddLauguage(tmp);
                            }

                        }
                    }
                    LauguageTool.GetIns().Save();
                    EditorUtility.DisplayDialog("成功", "导入成功", "确定");
                }
                else
                {
                    EditorUtility.DisplayDialog("失败", "不存在中文，请检查文本", "确定");
                }
            }
        }
        /*if (addLauguageTypeFlag)
        {
            int selectid = EditorGUILayout.Popup(-1, lauguageTypeStr, GUILayout.Width(200));
            if (selectid >= 0)
            {
                addLauguageTypeFlag = false;
                Lauguage lg = new Lauguage();
                lg.lauType = lauguageTypes[selectid];
                cell.lauguages.Add(lg);
            }
        }*/
    }
    #endregion
}