using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Game.Platform;
using Microsoft.International.Converters.PinYinConverter;
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
    public readonly static string Duoji_Start = "seivo-";
    public readonly static string Duoji_Type = "seivo";
    public readonly static string Duoji_Type_Old = "duoji";
    public readonly static string Hardware_Version_Name = "hardware_name_";
    public readonly static string Hardware_Version_Path = "hardware_path_";
    public readonly static string ID_Format = "ID-";
    public const int DuoJi_Start_Rota = 120;
    public const byte DuoJi_Min_Rota = 1;
    public const byte DuoJi_Max_Rota = 240;
    public const byte DuoJi_Min_Show_Rota = 2;
    public const byte DuoJi_Max_Show_Rota = 238;
    public const byte Robot_Power_Min = 70;

    public readonly static string ResultCode_Success = "1-1";
    public readonly static string ResultCode_Failure = "1-2";
    /// <summary>
    /// 更新主板程序的最低电量
    /// </summary>
    public const byte Update_System_Power_Min = 74;
    //public const byte Robot_Power_Max = 98;
    public const byte Robot_Power_Max = 84;
    public const byte Robot_Power_Empty = 65;
    public const byte Robot_New_Power_Empty = 68;//新主控锂电池的最低电压
    public const byte Robot_Power_Dry_Low = 47;//干电池低电量
    public const float Default_Screen_Width = 1334f;
    public const float Default_Screen_Height = 750f;
    public const byte DuoJi_Id_Min = 1;
    public const byte DuoJi_Id_Max = 32;
    public const byte Sensor_ID_Min = 1;
    public const byte Sensor_ID_Max = 8;
    public const char Separator_Comma = ',';
    public const char Separator_Or = '|';
    public const byte Show_Error_Time_Space = 10;

    public static Color32 GreyColor = new Color32(119, 123, 124, 255);
    public static Color PopWinColor = new Color(86 / 255f, 108 / 255f, 125 / 255f);
    public static Color PopWinColor_Default = new Color(86 / 255f, 108 / 255f, 125 / 255f);
    public static Color Connect_bg_Color = new Color32(249, 249, 249, 1);
    public const float PopWin_Alpha = 0.8f;
    public static Vector2 Back_Btn_Pos = new Vector2(14, 14);
    public static Vector2 Margin_Normal = new Vector2(36, 36);
    public static Vector2 Back_Btn_Pos_Iphonex = new Vector2(64, 14);
    public static Vector2 Iphonex_Add_Offset = new Vector2(50, 0);
    public const int Bottom_Margin_x = 50;
    public const int Title_Margin = 24;
    public const int Title_Margin_1 = 50;
    public const float Main_Scene_Right_Width = 0.408f;

    public static Color Button_Press_Color = new Color(238f / 255f, 238f / 255f, 238f / 255f, 0.8f);
    public static Color Button_Hover_Color = new Color(238f / 255f, 238f / 255f, 238f / 255f, 1f);
    public static Color Button_Disabled_Color = new Color(1, 1, 1, 0.6f);
    public static Color Blur_Effect_Texture_Color = new Color(150 / 255f, 150 / 255f, 150 / 255f, 1f);

    public static Vector2 Btn_Size = new Vector2(114, 114);
    public static Vector2 Btn_Padding = new Vector2(8, 0);
    public static Vector2 Connect_Icon_Size = new Vector2(76, 76);
    public static Vector2 Search_Model_Icon_Size = new Vector2(180, 180);
    /// <summary>
    /// 默认动作帧时间
    /// </summary>
    public const ushort Default_Actions_Time = 400;
    /// <summary>
    /// 默认的动作图标
    /// </summary>
    public const string Default_Actions_Icon_Name = "icon_coquetry";
    /// <summary>
    /// 默认动作的名字
    /// </summary>
    public const string Default_Actions_Name = "待命";
    /// <summary>
    /// 复位动作的中文名字
    /// </summary>
    public const string Default_Actions_Name_CN = "";

    /// <summary>
    /// 默认图标id
    /// </summary>
    public const string Default_Actions_Icon_ID = "icon_1";
    /// <summary>
    /// 上次连接的蓝牙
    /// </summary>
    public const string Last_Connected_Bluetooth = "LastConnectedBlue";
    /// <summary>
    /// 动作名字最大长度
    /// </summary>
    public const byte Action_Name_Lenght_Max = 16;
    /// <summary>
    /// 开放的传感器
    /// </summary>
    public static TopologyPartType[] Open_Topology_Part_Type = new TopologyPartType[] { 
        TopologyPartType.Infrared,
        TopologyPartType.Gyro, 
        TopologyPartType.Touch, 
        TopologyPartType.Light, 
        TopologyPartType.Gravity, 
        TopologyPartType.Ultrasonic, 
        TopologyPartType.DigitalTube, 
        TopologyPartType.Speaker, 
        TopologyPartType.EnLight,
        TopologyPartType.Atmosphere,
        TopologyPartType.Sound,
        TopologyPartType.Temperature,
        TopologyPartType.Color,
        TopologyPartType.RgbLight,
    };

    /// <summary>
    /// 需要读取数据的传感器
    /// </summary>

    public static TopologyPartType[] Read_All_Sensor_Type = new TopologyPartType[] { TopologyPartType.Infrared, TopologyPartType.Touch, TopologyPartType.Gyro, TopologyPartType.Ultrasonic, TopologyPartType.Color };

    public const ActionType Default_Action_Type = ActionType.Body;

    public const MoveSpeed Default_Move_Speed = MoveSpeed.M;

    public const MoveSpeed Default_Motor_Speed = MoveSpeed.VF;

    public const int Default_Action_Step = 1;

    public const float Default_Play_Speed = 1;
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

    public static ushort GetActionTime(int time, float speed)
    {
        time = PublicFunction.Rounding((time / speed));
        int mod = time % 20;
        if (mod > 10)
        {
            time += 20;
        }
        return (ushort)time;
    }
    /// <summary>
    /// 通过名字获取舵机id
    /// </summary>
    /// <param name="name">舵机名字</param>
    /// <returns></returns>
    public static int GetDuoJiId(string name)
    {
        try
        {
            return int.Parse(name.Substring(Duoji_Start.Length));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            return 0;
        }
    }
    /// <summary>
    /// 通过舵机id获取名字
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetDuoJiName(int id)
    {
        return Duoji_Start + (id);
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
        string pattern = @"^-{0,1}\d*$";
        return Regex.IsMatch(s, pattern);
    }

    public static bool IsFloat(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return false;
        }
        string pattern = @"^[+-]?[0-9]+.?[0-9]*$";
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

    public static string RGBToHexString(byte r, byte g, byte b)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("#");
        stringBuilder.Append(r.ToString("X2"));
        stringBuilder.Append(g.ToString("X2"));
        stringBuilder.Append(b.ToString("X2"));
        return stringBuilder.ToString();
    }

    public static bool IsBlackColor(string hexString)
    {
        if (string.IsNullOrEmpty(hexString))
        {
            return true;
        }
        if (hexString.StartsWith("#"))
        {
            hexString = hexString.Substring(1);
        }
        else if (hexString.StartsWith("0x") || hexString.StartsWith("0X"))
        {
            hexString = hexString.Substring(2);
        }
        if (hexString.Equals("00") || hexString.Equals("0000") || hexString.Equals("000000"))
        {
            return true;
        }
        return false;
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
            width = (int)(Default_Screen_Width * RootManualHeight / (Default_Screen_Height + 0.0f));
        }
        return width;
    }
    /// <summary>
    /// 获取显示高度
    /// </summary>
    /// <returns></returns>
    /// 
    public static bool SetHeightAndWidth(float screenHeight, float screenWidth)
    {
        float truewidth = Default_Screen_Height * screenWidth / screenHeight;
        if (truewidth >= Default_Screen_Width)
        {
            rootManualHeight = (int)(Default_Screen_Height);
        }
        else
        {
            rootManualHeight = (int)(Default_Screen_Width * screenHeight / screenWidth);
        }
        width = (int)(screenWidth * RootManualHeight / (screenHeight + 0.0f));
#if UNITY_IPHONE
        if (Screen.width == 2436 && Screen.height == 1125)
        //if (Screen.width == 996 && Screen.height == 460)
        {
            Back_Btn_Pos = Back_Btn_Pos_Iphonex;
        }
        else
        {
            Iphonex_Add_Offset = Vector2.zero;
        }
#else
        Iphonex_Add_Offset = Vector2.zero;
#endif
        return true;
    }

    public static void SetTextureFullScreen(UITexture texture)
    {
        SetTextureFullSize(texture, PublicFunction.GetWidth(), PublicFunction.GetHeight());
    }

    public static void SetTextureFullSize(UITexture texture, int showWidth, int showHeight)
    {
        if (null != texture && null != texture.mainTexture)
        {
            float oldWidth = texture.mainTexture.width;
            float oldHeight = texture.mainTexture.height;
            int height = (int)(showHeight * oldWidth / showWidth);
            if (height >= oldHeight)
            {
                texture.height = showHeight;
                texture.width = Mathf.CeilToInt(texture.height * oldWidth / oldHeight);
            }
            else
            {
                texture.width = showWidth ;
                texture.height = Mathf.CeilToInt(texture.width * oldHeight / oldWidth);
            }
        }
    }

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
    /// 判断舵机角度是否处于正常值
    /// </summary>
    /// <param name="rota"></param>
    /// <returns></returns>
    public static bool IsNormalRota(int rota)
    {
        if (rota < DuoJi_Min_Rota || rota > DuoJi_Max_Rota)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// 判断舵机角度是否处于显示的正常角度
    /// </summary>
    /// <param name="rota"></param>
    /// <returns></returns>
    public static bool IsShowNormalRota(int rota)
    {
        if (rota < DuoJi_Min_Show_Rota || rota > DuoJi_Max_Show_Rota)
        {
            return false;
        }
        return true;
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return str;
    }

    public static string ListToString<T>(List<T> list, char sep)
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
                        str += sep;
                    }
                    str += list[i];
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
    /// <summary>
    /// 计算偏移
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static Vector3 CalculateConstrainOffset(Vector2 min, Vector2 max, Vector4 rect)
    {
        Vector4 cr = rect;

        float offsetX = cr.z * 0.5f;
        float offsetY = cr.w * 0.5f;

        Vector2 minRect = new Vector2(min.x, min.y);
        Vector2 maxRect = new Vector2(max.x, max.y);
        Vector2 minArea = new Vector2(cr.x - offsetX, cr.y - offsetY);
        Vector2 maxArea = new Vector2(cr.x + offsetX, cr.y + offsetY);

        return NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
    }
    /// <summary>
    /// 计算居中的偏移
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static Vector3 CalculateCenterOffset(Vector2 min, Vector2 max, Vector4 rect)
    {
        Vector4 cr = rect;

        float offsetX = cr.z * 0.5f;
        float offsetY = cr.w * 0.5f;

        float width = max.x - min.x;
        float height = max.y - min.y;

        float minSpaceX = (rect.z - width) / 2;
        float minSpaceY = (rect.w - height) / 2;

        Vector2 targetMin = new Vector2(cr.x - offsetX + minSpaceX, cr.y - offsetY + minSpaceY);

        return new Vector3(targetMin.x - min.x, targetMin.y - min.y);
    }

    public static void MoveToCenter(Transform trans, Transform parentTrans, float scalingFactor, Vector4 rect, bool instant, EventDelegate.Callback callback)
    {
        trans.localScale = Vector3.one;
        Bounds bs = NGUIMath.CalculateRelativeWidgetBounds(parentTrans, trans);
        float width = rect.z * scalingFactor;
        float height = rect.w * scalingFactor;
        float x = bs.size.x / width;
        float y = bs.size.y / height;
        if (x > 1.0001f || y > 1.0001f)
        {
            float scale = 1 / Mathf.Max(x, y);
            //TweenScale.Begin(trans.gameObject, 0.3f, new Vector3(scale, scale, 1));
            trans.localScale = new Vector3(scale, scale, 1);
        }
        Bounds bs1 = NGUIMath.CalculateRelativeWidgetBounds(parentTrans, trans);
        if (instant)
        {
            trans.localPosition += PublicFunction.CalculateCenterOffset(bs1.min, bs1.max, rect);
            if (null != callback)
            {
                callback();
            }
        }
        else
        {
            Vector3 pos = trans.localPosition + PublicFunction.CalculateCenterOffset(bs1.min, bs1.max, rect);
            TweenPosition tw = TweenPosition.Begin(trans.gameObject, 0.3f, pos);
            if (null != tw && callback != null)
            {
                tw.SetOnFinished(callback);
            }
            else if (callback != null)
            {
                callback();
            }
        }
    }

    public static void MoveToCenter(Transform trans, Transform parentTrans, Vector4 rect, bool instant, EventDelegate.Callback callback)
    {
        trans.localScale = Vector3.one;
        Bounds bs1 = NGUIMath.CalculateRelativeWidgetBounds(parentTrans, trans);
        if (instant)
        {
            trans.localPosition += PublicFunction.CalculateCenterOffset(bs1.min, bs1.max, rect);
            if (null != callback)
            {
                callback();
            }
        }
        else
        {
            Vector3 pos = trans.localPosition + PublicFunction.CalculateCenterOffset(bs1.min, bs1.max, rect);
            TweenPosition tw = TweenPosition.Begin(trans.gameObject, 0.3f, pos);
            if (null != tw && callback != null)
            {
                tw.SetOnFinished(callback);
            } else if (callback != null)
            {
                callback();
            }
        }
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
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }
        foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
        {
            string destName = Path.Combine(destinationPath, fsi.Name);
            if (fsi is System.IO.FileInfo)
            {
                if (File.Exists(destName))
                {
                    File.Delete(destName);
                }
                File.Copy(fsi.FullName, destName);
            }
            else
            {
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return string.Empty;
    }

    /// <summary> 
    /// 汉字转化为拼音
    /// </summary> 
    /// <param name="str">汉字</param> 
    /// <returns>全拼</returns> 
    public static string GetPinyin(string str)
    {
        string r = string.Empty;
        foreach (char obj in str)
        {
            try
            {
                ChineseChar chineseChar = new ChineseChar(obj);
                string t = chineseChar.Pinyins[0].ToString();
                r += t.Substring(0, t.Length - 1);
            }
            catch
            {
                r += obj.ToString();
            }
        }
        return r;
    }
    /// <summary> 
    /// 汉字转化为拼音首字母
    /// </summary> 
    /// <param name="str">汉字</param> 
    /// <returns>首字母</returns> 
    public static string GetFirstPinyin(string str)
    {
        string r = string.Empty;
        foreach (char obj in str)
        {
            try
            {
                ChineseChar chineseChar = new ChineseChar(obj);
                string t = chineseChar.Pinyins[0].ToString();
                r += t.Substring(0, 1);
            }
            catch
            {
                r += obj.ToString();
            }
        }
        return r;
    }

    public static Vector2 TryParseVector2(string str)
    {
        try
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] tmp = GetVector(str);
                if (null != tmp && tmp.Length >= 2)
                {
                    Vector2 vector2 = Vector2.zero;
                    if (PublicFunction.IsFloat(tmp[0]))
                    {
                        vector2.x = float.Parse(tmp[0]);
                    }
                    if (PublicFunction.IsFloat(tmp[1]))
                    {
                        vector2.y = float.Parse(tmp[1]);
                    }
                    return vector2;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return Vector2.zero;
    }

    public static Vector3 TryParseVector3(string str)
    {
        try
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] tmp = GetVector(str);
                if (null != tmp && tmp.Length >= 3)
                {
                    Vector3 vector3 = Vector3.zero;
                    if (PublicFunction.IsFloat(tmp[0]))
                    {
                        vector3.x = float.Parse(tmp[0]);
                    }
                    if (PublicFunction.IsFloat(tmp[1]))
                    {
                        vector3.y = float.Parse(tmp[1]);
                    }
                    if (PublicFunction.IsFloat(tmp[2]))
                    {
                        vector3.z = float.Parse(tmp[2]);
                    }
                    return vector3;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "PublicFunction-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return Vector3.zero;
    }

    static string[] GetVector(string str)
    {
        str = str.Trim().TrimStart('(').TrimEnd(')').Replace(" ", "");
        return str.Split(',');
    }


    public static void SetCircularTexture(UITexture tex, Vector2 size)
    {
        if (tex.mainTexture.width < tex.mainTexture.height)
        {
            tex.height = (int)(tex.mainTexture.height * size.x / tex.mainTexture.width);
            tex.width = (int)size.x;
            if (null != tex.material)
            {
                tex.material.SetFloat("_TexWidth", 1);
                tex.material.SetFloat("_TexHeight", tex.mainTexture.height / (tex.mainTexture.width + 0.0f));
            }
        }
        else if (tex.mainTexture.width > tex.mainTexture.height)
        {
            tex.width = (int)(tex.mainTexture.width * size.y / tex.mainTexture.height);
            tex.height = (int)size.y;
            if (null != tex.material)
            {
                tex.material.SetFloat("_TexWidth", tex.mainTexture.width / (tex.mainTexture.height + 0.0f));
                tex.material.SetFloat("_TexHeight", 1);
            }
        } else
        {
            tex.width = (int)size.x;
            tex.height = (int)size.y;
            if (null != tex.material)
            {
                tex.material.SetFloat("_TexWidth", 1);
                tex.material.SetFloat("_TexHeight", 1);
            }
        }
    }

    public static void ResetCircularTexture(UITexture tex)
    {
        if (null != tex && null != tex.material)
        {
            tex.material.SetFloat("_TexWidth", 1);
            tex.material.SetFloat("_TexHeight", 1);
        }
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


    public static string FindRootPath(GameObject obj)
    {
        string path = obj.name;
        if (null != obj.transform.parent)
        {
            return FindRootPath(obj.transform.parent.gameObject) + "/" + path;
        }
        return path;
    }

    public static byte ListToByte(List<byte> ids)
    {
        byte id = 0;
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            id |= (byte)(1 << (ids[i] - 1));
        }
        return id;
    }

    public static byte ConverToByte(byte[] ids)
    {
        byte id = 0;
        for (int i = 0, imax = ids.Length; i < imax; ++i)
        {
            if (ids[i] > 0)
            {
                id |= (byte)(1 << (ids[i] - 1));
            }
        }
        return id;
    }

    public static UInt32 ConverToUInt32(byte[] ids)
    {
        UInt32 id = 0;
        for (int i = 0, imax = ids.Length; i < imax; ++i)
        {
            if (ids[i] > 0)
            {
                id += (UInt32)Math.Pow(2, ids[i] - 1);
            }
        }
        return id;
    }

    public static UInt32 ListToUInt32(List<byte> ids)
    {
        UInt32 id = 0;
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            id += (UInt32)Math.Pow(2, ids[i] - 1);
        }
        return id;
    }
    /// <summary>
    /// 传感器版本号转换成数组
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static byte[] SensorVersionToBytes(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            return new byte[] { 0, 0, 0, 0 };
        }
        byte[] bytes = new byte[4];
        bytes[0] = byte.Parse(version.Substring(0, 2));
        bytes[1] = byte.Parse(version.Substring(2, 2));
        bytes[2] = byte.Parse(version.Substring(4, 2));
        bytes[3] = byte.Parse(version.Substring(6, 2));
        return bytes;
    }

    public static float[] RGBToHSV(byte r, byte g, byte b)
    {
        float[] hsv = new float[3];
        byte max = (byte)Mathf.Max(r, g, b);
        byte min = (byte)Mathf.Min(r, g, b);
        byte V = max;
        float S = 0;
        if (max != 0)
        {
            S = (max - min) / (max + 0.0f) * 255;
        }
        float H = 0;
        if (min == max) H = 0;
        else if (r == max) H = (g - b) / (max - min + 0.0f) * 60.0f;
        else if (g == max) H = 120 + (b - r) / (max - min + 0.0f) * 60.0f;
        else if (b == max) H = 240 + (r - g) / (max - min + 0.0f) * 60.0f;
        if (H < 0) H = H + 360;
        hsv[0] = H;
        hsv[1] = S;
        hsv[2] = V;
        return hsv;
    }
    /// <summary>
    /// 红、橙、黄、绿、青、蓝、紫、白、黑、灰色
    /// 加入rgb的判断是因为颜色传感器识别误差较大，减少误识别
    /// </summary>
    public static string[] colorAry = new string[] { "#FF0000", "#FF8000", "#FFF000", "#00FF00", "#00FFFF", "#0000FF", "#800080", "#FFFFFF", "#000000", "#808080"};
    public static string ConverRgtToColor(byte r, byte g, byte b, bool collectFlag = true)
    {
        float[] hsv = PublicFunction.RGBToHSV(r, g, b);
        string color = string.Empty;
        if (hsv[1] >= 30 && hsv[2] >= 60 && (hsv[1] + hsv[2] >= 200 || hsv[1] >= 80 && hsv[2] >= 80 && hsv[0] >= 260 && hsv[0] <= 320))//260-320紫色放大区域
        {
            float offset1 = 10.0f * (255 - hsv[1]) / 55;
            if (offset1 > 10)
            {
                offset1 = 10;
            }
            float orange = 0;
            if (hsv[2] >= 250)
            {
                orange = 8.0f * (255 - hsv[1]) / 55;
            } else if (hsv[1] >= 250)
            {
                orange = 8.0f * (255 - hsv[2]) / 55;
            } else
            {
                orange = 4.0f * (255 - hsv[1]) / 55 + 4.0f * (255 - hsv[2]) / 55;
            }
            if (orange > 8)
            {
                orange = 8;
            }
            float blue = 0;
            if (hsv[1] <= 230)
            {
                blue = 10.0f * (230 - hsv[1]) / 55;
                if (blue > 10)
                {
                    blue = 10;
                }
            }
            float purple = 0;
            if (hsv[2] <= 200)
            {
                purple = 10 * (255 - hsv[1]) / 200.0f + 30 * (255 - hsv[2]) / 200.0f;
            }
            
            if (hsv[0] <= 16 - offset1 || hsv[0] > 320 + purple)
            {//红
                if (hsv[1] < 50)
                {//灰色
                    color = colorAry[9];
                }
                else if (hsv[2] >= 80 && r >= 140)
                {
                    color = colorAry[0];
                }
            }
            else if (hsv[0] <= 36 - orange)
            {//橙色
                if (hsv[2] >= 80 && b <= 140)
                {
                    color = colorAry[1];
                }
            }
            else if (hsv[0] <= 62 && hsv[2] >= 160)
            {//黄色
                if (b <= 140 || r >= 250 && g >= 250 && b <= 180)
                {
                    color = colorAry[2];
                }
            }
            else if (hsv[0] <= 144)
            {//绿色
                if (hsv[0] >= 50 && (g <= 120 && g - r >= 10 || g > 120))
                {
                    color = colorAry[3];
                }
            }
            else if (hsv[0] <= 200)
            {//青色
                color = colorAry[4];
            }
            else if (hsv[0] <= 256 - blue)
            {//蓝色
                if (hsv[1] <= 50)
                {//灰色
                    color = colorAry[9];
                } else if (b >= 110)
                {
                    color = colorAry[5];
                }
            }
            else if (hsv[0] <= 320 + purple)
            {//紫色
                if (b >= 50)
                {
                    color = colorAry[6];
                }
            }
        }
        else if (hsv[2] <= 40 || hsv[1] <= 40 && hsv[2] < 100)
        {//黑色
            color = colorAry[8];
        }
        else if (hsv[1] < 30 && hsv[2] >= 220)
        {//白色
            color = colorAry[7];
        } else
        {//灰色
            color = colorAry[9];
        }
        if (collectFlag)
        {
            SingletonObject<ColorCollectTool>.GetInst().AddColor(r, g, b, hsv, color);
        }
        return color;
    }

    public static int GetColorIndex(string color)
    {
        for (int i = 0, imax = colorAry.Length; i < imax; ++i)
        {
            if (colorAry[i].Equals(color))
            {
                return i;
            }
        }
        return -1;
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
