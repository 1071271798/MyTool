using UnityEngine;
using System.Collections;
using UnityEditor;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Text;

public class ClientMain : MonoBehaviour {

    string mDbName = string.Empty;
    string mTabelName = string.Empty;
    string mTabelCol = string.Empty;
    string mTabelType = string.Empty;

    string mAttributesValue = string.Empty;

    string mUpdateColName = string.Empty;
    string mUpdateColValue = string.Empty;
    string mUpdateKey = string.Empty;
    string mUpdateValue = string.Empty;

    string mDeleteColName = string.Empty;
    string mDeleteColValue = string.Empty;
    DataBase mDb = null;
    void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建/打开数据库"))
        {
            if (string.IsNullOrEmpty(mDbName))
            {
                EditorUtility.DisplayDialog("错误", "数据库名不能为空", "确定");
            }
            else if (null != mDb)
            {
                EditorUtility.DisplayDialog("错误", "请先关闭数据库", "确定");
            }
            else
            {
                mDb = new SqliteAccess(PublicFunction.CombinePath(Application.streamingAssetsPath, mDbName));
            }
        }
        mDbName = GUILayout.TextField(mDbName, GUILayout.Width(200));
        GUILayout.EndHorizontal();
    
        if (null != mDb)
        {
            if (GUILayout.Button("关闭数据库"))
            {
                mDb.CloseDB();
            }

            CreateTabel();
            if (!string.IsNullOrEmpty(mTabelName))
            {
                InsertValue();
                UpdateInfo();
                DeleteInfo();
            }
            if (GUILayout.Button("查找所有数据"))
            {
                if (string.IsNullOrEmpty(mTabelName))
                {
                    EditorUtility.DisplayDialog("错误", "表名不能为空", "确定");
                }
                else
                {
                    SqliteDataReader sqliteData = (SqliteDataReader)mDb.ReadFullTable(mTabelName);
                    List<string> colName = mDb.GetAllColName(mTabelName);
                    while (sqliteData.Read())
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0, imax = colName.Count; i < imax; ++i)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(',');
                            }
                            sb.Append(string.Format("{0} = {1}", colName[i], sqliteData[colName[i]].ToString()));
                        }
                        Debug.Log(sb.ToString());
                    }
                    
                }
            }
        }
        GUILayout.EndVertical();
    }

    void CreateTabel()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("表名:", GUILayout.Width(40));
        mTabelName = GUILayout.TextField(mTabelName, GUILayout.Width(60));
        GUILayout.Label("字段名(以-分开):", GUILayout.Width(100));
        mTabelCol = GUILayout.TextField(mTabelCol, GUILayout.Width(100));
        GUILayout.Label("类型(以-分开):", GUILayout.Width(100));
        mTabelType = GUILayout.TextField(mTabelType, GUILayout.Width(100));
        if (GUILayout.Button("创建表", GUILayout.Width(50)))
        {
            if (string.IsNullOrEmpty(mTabelName) || string.IsNullOrEmpty(mTabelCol) || string.IsNullOrEmpty(mTabelType))
            {
                EditorUtility.DisplayDialog("错误", "参数不能为空", "确定");
            }
            else
            {
                List<string> tabelList = mDb.GetAllTabelName();
                if (tabelList.Contains(mTabelName))
                {
                    EditorUtility.DisplayDialog("错误", "表名已存在", "确定");
                }
                else
                {
                    string[] colName = mTabelCol.Split('-');
                    string[] colType = mTabelType.Split('-');
                    if (colName.Length != colType.Length)
                    {
                        EditorUtility.DisplayDialog("错误", "字段名与字段类型个数不一致", "确定");
                    }
                    else
                    {
                        mDb.CreateTable(mTabelName, colName, colType);
                    }
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    void InsertValue()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("属性:", GUILayout.Width(40));
        mAttributesValue = GUILayout.TextField(mAttributesValue, GUILayout.Width(300));
        if (GUILayout.Button("插入数据", GUILayout.Width(60)))
        {
            if (string.IsNullOrEmpty(mAttributesValue))
            {
                EditorUtility.DisplayDialog("错误", "参数不能为空", "确定");
            }
            else
            {
                string[] attributes = mAttributesValue.Split('-');
                List<string> colName = mDb.GetAllColName(mTabelName);
                if (attributes.Length != colName.Count)
                {
                    EditorUtility.DisplayDialog("错误", "属性数量与表不一致", "确定");
                }
                else
                {
                    mDb.InsertInto(mTabelName, attributes);
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    void UpdateInfo()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("列名", GUILayout.Width(30));
        mUpdateColName = GUILayout.TextField(mUpdateColName, GUILayout.Width(100));
        GUILayout.Label("属性", GUILayout.Width(30));
        mUpdateColValue = GUILayout.TextField(mUpdateColValue, GUILayout.Width(100));
        GUILayout.Label("selectKey", GUILayout.Width(60));
        mUpdateKey = GUILayout.TextField(mUpdateKey, GUILayout.Width(40));
        GUILayout.Label("selectValue", GUILayout.Width(70));
        mUpdateValue = GUILayout.TextField(mUpdateValue, GUILayout.Width(40));
        if (GUILayout.Button("更新", GUILayout.Width(40)))
        {
            if (string.IsNullOrEmpty(mUpdateColName) || string.IsNullOrEmpty(mUpdateColValue) || string.IsNullOrEmpty(mUpdateKey) || string.IsNullOrEmpty(mUpdateValue))
            {
                EditorUtility.DisplayDialog("错误", "参数不能为空", "确定");
            }
            else
            {
                string[] updateColName = mUpdateColName.Split('-');
                string[] updateColValue = mUpdateColValue.Split('-');
                if (updateColName.Length != updateColValue.Length)
                {
                    EditorUtility.DisplayDialog("错误", "更新的列名与属性不一致", "确定");
                }
                else
                {
                    mDb.UpdateInto(mTabelName, updateColName, updateColValue, mUpdateKey, mUpdateValue);
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    void DeleteInfo()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("列名", GUILayout.Width(30));
        mDeleteColName = GUILayout.TextField(mDeleteColName, GUILayout.Width(100));
        GUILayout.Label("属性", GUILayout.Width(30));
        mDeleteColValue = GUILayout.TextField(mDeleteColValue, GUILayout.Width(100));
        if (GUILayout.Button("删除", GUILayout.Width(40)))
        {
            if (string.IsNullOrEmpty(mDeleteColName) || string.IsNullOrEmpty(mDeleteColValue))
            {
                EditorUtility.DisplayDialog("错误", "参数不能为空", "确定");
            }
            else
            {
                string[] deleteColName = mDeleteColName.Split('-');
                string[] deleteColValue = mDeleteColValue.Split('-');
                if (deleteColName.Length != deleteColValue.Length)
                {
                    EditorUtility.DisplayDialog("错误", "更新的列名与属性不一致", "确定");
                }
                else
                {
                    mDb.Delete(mTabelName, deleteColName, deleteColValue);
                }
            }
        }
        GUILayout.EndHorizontal();
    }
}
