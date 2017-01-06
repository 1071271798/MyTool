using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using System.Diagnostics;
using UnityEditor;
#endif

/// <summary>
/// Author:xj
/// FileName:PublicFunction.cs
/// Description:
/// Time:2015/7/14 9:40:40
/// </summary>
public class PublicFunction
{
    #region 公有属性
    public const float Default_Screen_Width = 1334f;
    public const float Default_Screen_Height = 750f;
    
    public const char Separator_Comma = ',';
    public const char Separator_Or = '|';
    
    /// <summary>
    /// 获取uiroot的manualHeight用于适配
    /// </summary>
    public static int RootManualHeight
    {
        get 
        {
            if (rootManualHeight == 0)
            {
                float width = Default_Screen_Height * Screen.width / Screen.height;
                if (width >= Default_Screen_Width)
                {
                    rootManualHeight = (int)(Default_Screen_Height);
                }
                else
                {
                    rootManualHeight = (int)(Default_Screen_Width * Screen.height / Screen.width);
                }
            }
            return rootManualHeight;
        }
    }
    
#endregion

#region 私有属性
    static int rootManualHeight = 0;
#endregion

#region 公有函数
    /// <summary>
    /// 设置物体层级
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layer"></param>
    public static void SetLayerRecursively(GameObject obj, int layer)
    {
      
        if (obj != null)
        {
            obj.layer = layer;
            
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }

    /// <summary>
    /// 四舍五入，r为舍入的值，默认0.5即小于0.5舍了，大于入
    /// </summary>
    /// <param name="num">需要求舍入的值</param>
    /// <returns>list<int></returns>
    public static int Rounding(double num, double r = 0.5)
    {
        return (int)(num + r);
    }

    /// <summary>
    /// 判断一个字符串是否为合法整数(不限制长度)
    /// </summary>
    /// <param name="s">字符串</param>
    /// <returns>true：是整型</returns>
    public static bool IsInteger(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return false;
        }
        string pattern = @"^\d*$";
        return Regex.IsMatch(s, pattern);
    }
    /// <summary>
    /// 判断一个字符串是否有汉字
    /// </summary>
    /// <param name="s">字符串</param>
    /// <returns>true：有汉字</returns>
    public static bool CheckStrChinessReg(string text)
    {
        if (Regex.IsMatch(text, @"[\u4e00-\u9fbb]+"))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 把一个整数拆成按顺序排列的数组
    /// </summary>
    /// <param name="i">整数</param>
    /// <returns>list<int></returns>
    public static List<int> GetIntList(int i)
    {
        List<int> list = new List<int>();
        do
        {
            if (i < 0)
            {
                i = -i;
            }
            if (i >= 0 && i < 10)
            {
                list.Add(i);
                i = 0;
                break;
            }
            list.Add(i % 10);
            i /= 10;
        } while (0 != i);
        list.Reverse(0, list.Count);
        return list;
    }
    /// <summary>
    /// 把byte数组转换成十六进制显示的字符串
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string BytesToHexString(byte[] bytes)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (bytes == null || bytes.Length <= 0)
        {
            return string.Empty;
        }
        if (bytes != null)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                stringBuilder.Append(bytes[i].ToString("X2"));
            }
        }
        return stringBuilder.ToString(); 
    }
    /// <summary>
    /// 把十六进制的字符串转换成byte数组
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public static byte[] HexStringToBytes(string hexString)
    {
        if (string.IsNullOrEmpty(hexString))
        {
            return null;
        }
        hexString = hexString.ToUpper().Replace(" ", "");
        int length = hexString.Length / 2;
        char[] hexChars = hexString.ToCharArray();
        byte[] d = new byte[length];
        for (int i = 0; i < length; i++)
        {
            int pos = i * 2;
            d[i] = (byte)(charToByte(hexChars[pos]) << 4 | charToByte(hexChars[pos + 1]));
        }
        return d;
    }

    /// <summary>
    /// 把十六进制的字符串转换成rgb
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public static byte[] HexStringToRGB(string hexString)
    {
        byte[] rgb = new byte[3];
        if (!string.IsNullOrEmpty(hexString) && (hexString.StartsWith("#") || hexString.StartsWith("0x") || hexString.StartsWith("0X")))
        {
            if (hexString.StartsWith("#"))
            {
                hexString = hexString.Substring(1);
            }
            else if (hexString.StartsWith("0x") || hexString.StartsWith("0X"))
            {
                hexString = hexString.Substring(2);
            }
            if (hexString.Length > 6)
            {
                hexString = hexString.Substring(0, 6);
            }
            byte[] tmp = HexStringToBytes(hexString);
            if (null != tmp)
            {
                int index = 2;
                for (int i = tmp.Length - 1; i >= 0; --i)
                {
                    rgb[index] = tmp[i];
                    --index;
                }
            }
        }
        return rgb;
    }

    public static byte charToByte(char c)
    {
        return (byte)"0123456789ABCDEF".IndexOf(c);
    }
    static int width = 0;
    /// <summary>
    /// 获取显示宽度
    /// </summary>
    /// <returns></returns>
    public static int GetWidth()
    {
        if (0 == width)
        {
            width = (int)(Screen.width * RootManualHeight / (Screen.height + 0.0f));
        }
        return width;
    }
    /// <summary>
    /// 获取显示高度
    /// </summary>
    /// <returns></returns>
    public static int GetHeight()
    {
        return RootManualHeight;
    }

    public static int GetExtendWidth()
    {
        return GetWidth() + 4;
    }
    public static int GetExtendHeight()
    {
        return RootManualHeight + 4;
    }
    
    /// <summary>
    /// 把list转换成1,2,3这种字符串
    /// </summary>
    /// <param name="arg">list</param>
    /// <returns></returns>
    public static string ListToString<T>(List<T> list)
    {
        string str = string.Empty;
        try
        {
            if (null != list)
            {
                for (int i = 0, imax = list.Count; i < imax; ++i)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        str += Separator_Comma;
                    }
                    str += list[i];
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            UnityEngine.Debug.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
        return str;
    }

    public static List<int> StringToList(string str)
    {
        List<int> list = new List<int>();
        try
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] tmp = str.Split(Separator_Comma);
                if (null != tmp)
                {
                    for (int i = 0, imax = tmp.Length; i < imax; ++i)
                    {
                        list.Add(int.Parse(tmp[i]));
                    }
                }
                else
                {
                    list.Add(int.Parse(str));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            UnityEngine.Debug.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return list;
    }

    public static List<byte> StringToByteList(string str)
    {
        List<byte> list = new List<byte>();
        try
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] tmp = str.Split(Separator_Comma);
                if (null != tmp)
                {
                    for (int i = 0, imax = tmp.Length; i < imax; ++i)
                    {
                        list.Add(byte.Parse(tmp[i]));
                    }
                }
                else
                {
                    list.Add(byte.Parse(str));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            UnityEngine.Debug.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return list;
    }

    public static List<float> StringToFloatList(string str)
    {
        List<float> list = new List<float>();
        try
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] tmp = str.Split(Separator_Comma);
                if (null != tmp)
                {
                    for (int i = 0, imax = tmp.Length; i < imax; ++i)
                    {
                        list.Add(float.Parse(tmp[i]));
                    }
                }
                else
                {
                    list.Add(float.Parse(str));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            UnityEngine.Debug.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

        return list;
    }

    /// <summary>
    /// 获取某个文件夹下面的所有文件
    /// </summary>
    /// <param name="path">绝对路径</param>
    /// <param name="fileList">文件列表</param>
    public static void GetFiles(string path, List<string> fileList)
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
    
    //得到项目的名称
    public static string projectName
    {
        get
        {
            //在这里分析shell传入的参数， 还记得上面我们说的哪个 project-$1 这个参数吗？
            //这里遍历所有参数，找到 project开头的参数， 然后把-符号 后面的字符串返回，
            //这个字符串就是 91 了。。
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("project"))
                {
                    return arg.Split("-"[0])[1];
                }
            }
            return "test";
        }
    }

    public static void CopyDirectory(string sourcePath, string destinationPath)
    {
        DirectoryInfo info = new DirectoryInfo(sourcePath);
        Directory.CreateDirectory(destinationPath);
        foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
        {
            string destName = Path.Combine(destinationPath, fsi.Name);
            if (fsi is System.IO.FileInfo)
                File.Copy(fsi.FullName, destName);
            else
            {
                Directory.CreateDirectory(destName);
                CopyDirectory(fsi.FullName, destName);
            }
        }
    }
    /// <summary>
    /// 判断路径是否是模型文件路径
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    public static bool IsSameNameXml(string dirPath)
    {
        string dirName = dirPath.Substring(Path.GetDirectoryName(dirPath).Length + 1);
        string[] files = Directory.GetFiles(dirPath);
        if (null != files)
        {
            int count = 0;
            int index = -1;
            for (int i = 0, imax = files.Length; i < imax; i++)
            {
                if (files[i].EndsWith(".xml"))
                {
                    index = i;
                    ++count;
                }
            }
            if (-1 != index && count == 1)
            {
                return Path.GetFileNameWithoutExtension(files[index]).Equals(dirName);
            }
        }
        return false;
    }

    
    //获取字符串的CRC32校验值
    static public UInt32 GetCRC32Str(string sInputString)
    {
        //生成码表
        UInt32 Crc;
        UInt32[] Crc32Table = new UInt32[256];
        for (UInt32 i = 0; i < 256; i++)
        {
            Crc = (UInt32)i;
            for (UInt32 j = 0; j < 8; j++)
            {
                if ((Crc & 1) == 1)
                    Crc = (Crc >> 1) ^ 0xEDB88320;
                else
                    Crc >>= 1;
            }
            Crc32Table[i] = Crc;
        }
        byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sInputString);
        UInt32 value = 0xffffffff;
        int len = buffer.Length;
        for (UInt32 i = 0; i < len; i++)
        {
            value = (value >> 8) ^ Crc32Table[(value & 0xFF) ^ buffer[i]];
        }
        return value;
    }

    static public UInt32 GetCRC32Str(byte[] buffer)
    {
        //生成码表
        UInt32 Crc;
        UInt32[] Crc32Table = new UInt32[256];
        for (UInt32 i = 0; i < 256; i++)
        {
            Crc = (UInt32)i;
            for (UInt32 j = 0; j < 8; j++)
            {
                if ((Crc & 1) == 1)
                    Crc = (Crc >> 1) ^ 0xEDB88320;
                else
                    Crc >>= 1;
            }
            Crc32Table[i] = Crc;
        }
        /*byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sInputString); */
        UInt32 value = 0xffffffff;
        int len = buffer.Length;
        for (UInt32 i = 0; i < len; i++)
        {
            value = Crc32Table[(value ^ buffer[i]) & 0xFF] ^ (value >> 8);
        }
        return value;
    }
    /// <summary>
    /// 获取系统当前的unix时间戳的毫秒数
    /// </summary>
    /// <returns></returns>
    public static long GetNowMillisecond()
    {
        return DateTime.Now.Ticks / 10000;
    }

    /// <summary>  
    /// GET请求与获取结果  
    /// </summary>  
    public static string HttpGet(string Url, string postDataStr)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            UnityEngine.Debug.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return string.Empty;
    }

    /// </summary>  
    public static string HttpPost(string Url, string postDataStr)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataStr.Length;
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            writer.Write(postDataStr);
            writer.Flush();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码  
            }
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            return retString;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            UnityEngine.Debug.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return string.Empty;
    }

#if UNITY_EDITOR
    public static void OpenProcess(string path, string processName, string suffix)
    {
        Process[] process = Process.GetProcesses();//获取所有启动进程
        if (null != process)
        {
            for (int i = 0, size = process.Length; i < size; ++i)
            {
                try
                {
                    if (process[i].ProcessName.Equals(processName))
                    {
                        EditorUtility.DisplayDialog("提示", string.Format("{0}正在运行中！", processName), "OK");
                        return;
                    }
                }
                catch (System.Exception ex)
                {

                }
            }
        }
        try
        {
            Process.Start(Path.Combine(path, (processName + "." + suffix)));
        }
        catch (System.IO.FileNotFoundException)
        {//文件不存在
            EditorUtility.DisplayDialog("提示", string.Format("{0}不存在", processName), "OK");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            UnityEngine.Debug.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
#endif

    /// <summary>
    /// 拼接路径
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string CombinePath(params string [] args)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0, imax = args.Length; i < imax; ++i)
        {
            if (sb.Length > 0)
            {
                sb.Append('/');
            }
            sb.Append(args[i]);
        }
        return sb.ToString();
    }

#endregion
    #region 私有函数
    #endregion
}

public struct Int2
{
    public int num1;
    public int num2;
}