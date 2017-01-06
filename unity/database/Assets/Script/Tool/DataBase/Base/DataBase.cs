using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
/// <summary>
/// Author:xj
/// FileName:DataBase.cs
/// Description:数据库基类
/// Time:2017/1/4 17:35:32
/// </summary>
public abstract class DataBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    public DataBase()
    {

    }

    public DataBase(string connectionString)
    {

    }

    public virtual void OpenDB(string connectionString)
    {
    }

    public virtual void CloseDB()
    {
    }

    public virtual object ExecuteQuery(string sqlQuery)
    {
        return null;
    }

    public object ReadFullTable(string tableName)
    {
        string query = "SELECT * FROM " + tableName;
        return ExecuteQuery(query);
    }

    public virtual List<string> GetAllTabelName()
    {
        return null;
    }

    public virtual List<string> GetAllColName(string tabelName)
    {
        return null;
    }

    public virtual List<string> GetAllColType(string tabelName)
    {
        return null;
    }

    public object InsertInto(string tableName, string[] values)
    {
        StringBuilder sb = new StringBuilder("INSERT INTO ");
        sb.Append(tableName);
        sb.Append(" VALUES (");
        sb.Append(values[0]);
        for (int i = 1; i < values.Length; ++i)
        {
            sb.Append(", ");
            sb.Append(values[i]);
        }
        sb.Append(")");
        return ExecuteQuery(sb.ToString());
    }

    public object UpdateInto(string tableName, string[] cols, string[] colsvalues, string selectkey, string selectvalue)
    {
        StringBuilder sb = new StringBuilder("UPDATE ");
        sb.Append(tableName);
        sb.Append(" SET ");
        sb.Append(cols[0]);
        sb.Append(" = ");
        sb.Append(colsvalues[0]);
        for (int i = 1; i < colsvalues.Length; ++i)
        {
            sb.Append(", ");
            sb.Append(cols[i]);
            sb.Append(" =");
            sb.Append(colsvalues[i]);
        }
        sb.Append(" WHERE ");
        sb.Append(selectkey);
        sb.Append(" = ");
        sb.Append(selectvalue);
        sb.Append(" ");
        return ExecuteQuery(sb.ToString());
    }

    public object Delete(string tableName, string[] cols, string[] colsvalues)
    {
        StringBuilder sb = new StringBuilder("DELETE FROM ");
        sb.Append(tableName);
        sb.Append(" WHERE ");
        sb.Append(cols[0]);
        sb.Append(" = ");
        sb.Append(colsvalues[0]);
        for (int i = 1; i < colsvalues.Length; ++i)
        {
            sb.Append(" or ");
            sb.Append(cols[i]);
            sb.Append(" = ");
            sb.Append(colsvalues[i]);
        }
        return ExecuteQuery(sb.ToString());
    }

    public object InsertIntoSpecific(string tableName, string[] cols, string[] values)
    {
        if (cols.Length != values.Length)
        {
            throw new Exception("columns.Length != values.Length");
        }
        StringBuilder sb = new StringBuilder("INSERT INTO ");
        sb.Append(tableName);
        sb.Append("(");
        sb.Append(cols[0]);
        for (int i = 1; i < cols.Length; ++i)
        {
            sb.Append(", ");
            sb.Append(cols[i]);
        }
        sb.Append(") VALUES (");
        sb.Append(values[0]);
        for (int i = 1; i < values.Length; ++i)
        {
            sb.Append(", ");
            sb.Append(values[i]);
        }
        sb.Append(")");
        return ExecuteQuery(sb.ToString());
    }

    public object DeleteContents(string tableName)
    {
        string query = "DELETE FROM " + tableName;
        return ExecuteQuery(query);
    }

    public object CreateTable(string name, string[] col, string[] colType)
    {
        if (col.Length != colType.Length)
        {
            throw new Exception("columns.Length != colType.Length");
        }
        StringBuilder sb = new StringBuilder("CREATE TABLE ");
        sb.Append(name);
        sb.Append(" (");
        sb.Append(col[0]);
        sb.Append(" ");
        sb.Append(colType[0]);
        for (int i = 1; i < col.Length; ++i)
        {
            sb.Append(", ");
            sb.Append(col[i]);
            sb.Append(" ");
            sb.Append(colType[i]);
        }
        sb.Append(")");
        return ExecuteQuery(sb.ToString());
    }

    public object SelectWhere(string tableName, string[] items, string[] col, string[] operation, string[] values)
    {
        if (col.Length != operation.Length || operation.Length != values.Length)
        {
            throw new Exception("col.Length != operation.Length != values.Length");
        }
        StringBuilder sb = new StringBuilder("SELECT ");
        sb.Append(items[0]);
        for (int i = 1; i < items.Length; ++i)
        {
            sb.Append(", ");
            sb.Append(items[i]);
        }
        sb.Append(" FROM ");
        sb.Append(tableName);
        sb.Append(" WHERE ");
        sb.Append(col[0]);
        sb.Append(operation[0]);
        sb.Append("'");
        sb.Append(values[0]);
        sb.Append("' ");
        for (int i = 1; i < col.Length; ++i)
        {
            sb.Append(" AND ");
            sb.Append(col[i]);
            sb.Append(operation[i]);
            sb.Append("'");
            sb.Append(values[0]);
            sb.Append("' ");
        }
        return ExecuteQuery(sb.ToString());
    }
    #endregion

    #region 其他函数
    protected object ReadAllTabelName()
    {
        string query = "SELECT NAME FROM SQLITE_MASTER WHERE TYPE='table'";
        return ExecuteQuery(query);
    }

    protected object ReadTableInfo(string tableName)
    {
        /*StringBuilder sb = new StringBuilder("SELECT * FROM ");
        sb.Append(tableName);
        sb.Append(" LIMIT 0");*/
        StringBuilder sb = new StringBuilder("PRAGMA table_info ([");
        sb.Append(tableName);
        sb.Append("])");
        return ExecuteQuery(sb.ToString());
    }
    #endregion
}