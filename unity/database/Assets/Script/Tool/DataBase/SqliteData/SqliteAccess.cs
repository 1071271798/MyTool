using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:SqliteAccess.cs
/// Description:sqlite数据库
/// Time:2017/1/4 14:19:01
/// </summary>
public class SqliteAccess : DataBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    SqliteConnection mDbConnection;
    SqliteCommand mDbCommand;
    SqliteDataReader mReader;
    #endregion

    #region 公有函数

    public SqliteAccess(string connectionString)
    {
        OpenDB(connectionString);
    }

    public SqliteAccess()
    {

    }

    public override void OpenDB(string connectionString)
    {
        try
        {
            if (!File.Exists(connectionString))
            {
                SqliteConnection.CreateFile(connectionString);
            }
            //string fileName = Path.GetFileName(connectionString);
#if UNITY_ANDROID
            mDbConnection = new SqliteConnection("URI=file:" + connectionString);
#else
            mDbConnection = new SqliteConnection("data source=" + connectionString);
#endif

            mDbConnection.Open();
            Debug.Log("Connected to db");
        }
        catch (Exception e)
        {
            string temp1 = e.ToString();
            Debug.Log(temp1);
        }
    }

    public override void CloseDB()
    {
        if (mDbCommand != null)
        {
            mDbCommand.Dispose();
        }
        mDbCommand = null;
        if (mReader != null)
        {
            mReader.Dispose();
        }
        mReader = null;
        if (mDbConnection != null)
        {
            mDbConnection.Close();
        }
        mDbConnection = null;
        Debug.Log("Disconnected from db.");
    }

    public override object ExecuteQuery(string sqlQuery)
    {
        Debug.Log("ExecuteQuery = " + sqlQuery);
        mDbCommand = mDbConnection.CreateCommand();
        mDbCommand.CommandText = sqlQuery;
        mReader = mDbCommand.ExecuteReader();
        return mReader;
    }

    public override List<string> GetAllTabelName()
    {
        List<string> tabelList = new List<string>();
        SqliteDataReader sqliteData = (SqliteDataReader)ReadAllTabelName();
        while (sqliteData.Read())
        {
            tabelList.Add(sqliteData.GetString(0));
        }
        return tabelList;
    }

    public override List<string> GetAllColName(string tabelName)
    {
        List<string> colList = new List<string>();
        SqliteDataReader sqliteData = (SqliteDataReader)ReadTableInfo(tabelName);
        while (sqliteData.Read())
        {
            colList.Add(sqliteData[1].ToString());
        }        
        return colList;
    }

    public override List<string> GetAllColType(string tabelName)
    {
        List<string> colList = new List<string>();
        SqliteDataReader sqliteData = (SqliteDataReader)ReadTableInfo(tabelName);
        while (sqliteData.Read())
        {
            colList.Add(sqliteData[2].ToString());
        }
        return colList;
    }
#endregion

#region 其他函数
#endregion
}