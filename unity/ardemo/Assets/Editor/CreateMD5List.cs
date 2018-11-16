
#if UNITY_EDITOR
/// <summary>
/// 生成md5码
/// </summary>
using UnityEditor;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.IO;
using System.Xml;
public class CreateMD5List
{
    /// <summary>
    /// 需要跳过的文件
    /// </summary>
    private static string[] mFilterObjAry = { ".meta"};
    private static MD5CryptoServiceProvider mMd5Provider = new MD5CryptoServiceProvider();
    /// <summary>
    /// 获取文件md5列表
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetFilesMd5(string dirPath)
    {
        Dictionary<string, string> md5Dict = new Dictionary<string, string>();
        try
        {
            
            foreach (string filePath in Directory.GetFiles(dirPath))
            {
                if (PublicFunction.IsFilter(filePath, mFilterObjAry))
                {
                    continue;
                }
                string str = GetFileMd5(filePath);
                if (!string.IsNullOrEmpty(str))
                {
                    md5Dict.Add(filePath, str);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return md5Dict;
    }
    /// <summary>
    /// 获取单个文件的md5码
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFileMd5(string path)
    {
        try
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hash = mMd5Provider.ComputeHash(file);
            string strMD5 = System.BitConverter.ToString(hash);
            file.Close();
            file.Dispose();
            return strMD5;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return string.Empty;
    }
    /// <summary>
    /// 获取文件流的md5码
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string GetFileMd5(Stream stream)
    {
        try
        {
            byte[] hash = mMd5Provider.ComputeHash(stream);
            string strMD5 = System.BitConverter.ToString(hash);
            return strMD5;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return string.Empty;
    }
}
#endif