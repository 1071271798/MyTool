using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CommonUtils
{
    /// <summary>
    /// 获取某个文件夹下面的所有文件
    /// </summary>
    /// <param name="path">绝对路径</param>
    /// <param name="fileList">文件列表</param>
    public static void GetFiles(string path, List<string> fileList)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            string[] dirs = Directory.GetDirectories(path);
            for (int i = dirs.Length - 1; i >= 0; --i)
            {
                GetFiles(dirs[i], fileList);
            }
            string[] files = Directory.GetFiles(path);
            for (int i = files.Length - 1; i >= 0; --i)
            {
                fileList.Add(ConvertSlashPath(files[i]));
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.Log(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 把文件名里的斜杠都改成反斜杠
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public static string ConvertSlashPath(string resName)
    {
        return resName.Replace('\\', '/');
    }
    /// <summary>
    /// 获取文件夹的名称
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFolderName(string path)
    {
        if (path.Contains("."))
        {
            path = Path.GetDirectoryName(path).Replace('\\', '/');
        } else
        {
            path = path.Replace('\\', '/');
        }
        return path.Substring(path.LastIndexOf("/") + 1);
    }
    /// <summary>
    /// 获取文件夹的路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFolderPath(string path)
    {
        if (path.Contains("."))
        {
            return Path.GetDirectoryName(path).Replace('\\', '/');
        } else
        {
            return path.Replace('\\', '/');
        }
    }

    /// <summary>
    /// 删除某个文件夹下的所有东西
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="delSelf">是否删除本身，false不删除</param>
    public static void DelDirector(string path, bool delSelf = false)
    {
        try
        {
            if (Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);
                for (int i = dirs.Length - 1; i >= 0; --i)
                {
                    DelDirector(dirs[i], true);
                }
                string[] files = Directory.GetFiles(path);
                for (int i = files.Length - 1; i >= 0; --i)
                {
                    File.Delete(files[i]);
                }
                if (delSelf)
                {
                    Directory.Delete(path);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.Log(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    /// <summary>
    /// 判断文件path（路径和文件名）是否在过滤列表中
    /// </summary>
    /// <param name="path">文件名</param>
    /// <param name="filterAry">过滤列表</param>
    /// <returns>true表示需要过滤</returns>
    public static bool IsFilter(string path, string[] filterAry)
    {
        int size = 0;
        if (null != filterAry)
        {
            size = filterAry.Length;
        }
        for (int i = 0; i < size; ++i)
        {
            if (path.Contains(filterAry[i]))
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 判断文件后缀是否在列表里面
    /// </summary>
    /// <param name="path">文件名</param>
    /// <param name="ends">后缀列表</param>
    /// <returns>true表示在</returns>
    public static bool EndWith(string path, string[] ends)
    {
        int size = 0;
        if (null != ends)
        {
            size = ends.Length;
        }
        for (int i = 0; i < size; ++i)
        {
            if (path.EndsWith(ends[i]))
            {
                return true;
            }
        }
        return false;
    }
}

