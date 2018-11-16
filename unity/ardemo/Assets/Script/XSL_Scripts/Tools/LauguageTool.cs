//----------------------------------------------
//            积木2: xiongsonglin
//            语言配置工具
// Copyright © 2015 for Open
//----------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using MyMVC;
using System.Text;
using UnityEngine;
using System.IO;
using System;
using Game.Resource;
using Game.Platform;


public class LauguageTool
{
    private string path = "";
    private Dictionary<string,string> configText;
    private LauguageConfig lauguageConfig;
    private LauguageType _curLauguage;
    private string lastLanguage = string.Empty;
    public LauguageType CurLauguage
    {
        get { return _curLauguage; }
        set 
        {
            _curLauguage = value;
            GetDicByLauguage(value);
        }
    }

    private static LauguageTool Instance;
    public static LauguageTool GetIns()
    {
        if (Instance == null)
            Instance = new LauguageTool();
        return Instance;
    }
    private LauguageTool()
    {
        path += "/TextConfig.txt";
        LauguageConfigPath = ResourcesEx.persistentDataPath + "/LanguageCode";

        TextAsset text = Resources.Load("TextConfig", typeof(TextAsset)) as TextAsset;
        if (null != text)
        {
            lauguageConfig = XmlHelper.XmlDeserialize<LauguageConfig>(text.text.Trim(), Encoding.UTF8);
        }
        else //不存在则创建
        {
            lauguageConfig = new LauguageConfig();
            TextCell t1 = new TextCell();
            t1.key = "问候";
            Lauguage l1 = new Lauguage() { lauType = LauguageType.Chinese, value = "哈喽" };
            Lauguage l2 = new Lauguage() { lauType = LauguageType.English, value = "hello" };
            Lauguage l3 = new Lauguage() { lauType = LauguageType.Japanese, value = "我哈哟" };
            Lauguage l4 = new Lauguage() { lauType = LauguageType.Korean, value = "jfkjekjfk" };
            t1.lauguages.Add(l1);
            t1.lauguages.Add(l2);
            t1.lauguages.Add(l3);
            t1.lauguages.Add(l4);
            lauguageConfig.AddToTypes(t1);

            SaveObjToXml(lauguageConfig, path);
        }
        SetCurLauguage();
    }

    public bool IsExistForKey(string key)
    {
        if (null != configText && configText.ContainsKey(key))
        {
            return true;
        }
        return false;
    }

    public Dictionary<string, string> GetConfigText()
    {
        return configText;
    }

    public static void CleanUp()
    {
        Instance = null;
    }

    public void ClearAllLauguage()
    {
        lauguageConfig.ClearAllLauguage();
        if (null != configText)
        {
            configText.Clear();
        }
    }

    public bool AddLauguage(TextCell text)
    {
        return lauguageConfig.AddToTypes(text);
    }

    public void AddLanguageForChinese(TextCell text, string chineseValue)
    {
        lauguageConfig.AddForChinese(text, chineseValue);
    }

    public bool DelLauguage(TextCell text)
    {
        return lauguageConfig.DelLauguage(text);
    }

    public TextCell FindForKey(string key)
    {
        return lauguageConfig.FindForKey(key);
    }

    public TextCell[] FindAllForKey(string key)
    {
        return lauguageConfig.FindAllForKey(key);
    }

    public TextCell FindForLauguage(Lauguage lauguage)
    {
        return lauguageConfig.FindForLauguage(lauguage);
    }

    public TextCell[] FindAllForLauguage(Lauguage lauguage)
    {
        return lauguageConfig.FindAllForLauguage(lauguage);
    }

    public void Save()
    {
        SaveObjToXml(lauguageConfig, Application.dataPath + "/Resources" + path);
    }

    public void SetLauguage(LauguageType type)
    {
        CurLauguage = type;
    }

    public string LauguageConfigPath;
    public void SetCurLauguage()
    { 
        try
        {
            string language = PlatformMgr.Instance.GetUserData(UserDataType.language);
            if (string.IsNullOrEmpty(language))
            {
#if UNITY_EDITOR
                language = "China";
#else
                language = "English";
#endif

            }
            if (!language.Equals(lastLanguage))
            {
                lastLanguage = language;
                try
                {
                    switch (language)
                    {
                        case "China":
                            CurLauguage = LauguageType.Chinese;
                            break;
                        case "Japan":
                            CurLauguage = LauguageType.Japanese;
                            break;
                        case "Polski":
                            CurLauguage = LauguageType.Polaic;
                            break;
                        default:
                            CurLauguage = (LauguageType)Enum.Parse(typeof(LauguageType), language);
                            break;
                    }
                }
                catch (System.ArgumentException ex)
                {
                    lastLanguage = "English";
                    CurLauguage = LauguageType.English;
                }
                catch (System.Exception ex)
                {
                    lastLanguage = "English";
                    CurLauguage = LauguageType.English;
                }
            }
            
        }
        catch (System.Exception ex)
        {
            lastLanguage = "English";
            CurLauguage = LauguageType.English;
        }
        
    }

    public void SetCurLauguage(LauguageType lgType)
    {
        CurLauguage = lgType;
    }

    //生成字典
    private Dictionary<string,string> GetDicByLauguage(LauguageType type)
    {
        if (lauguageConfig == null)
            return null;
        configText = lauguageConfig.GetDicByType(type);
        return configText;
    }


    private bool SaveObjToXml(LauguageConfig ll,string path)
    {
        if (ll == null)
            return false;

        string str = XmlHelper.XmlSerialize(lauguageConfig, Encoding.UTF8);
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);
        fs.SetLength(0);//首先把文件清空了。
        sw.Write(str);//写你的字符串。
        sw.Close();
        return true;
    }

    public string GetText(string key)
    {
        string str = key;
        if (configText.ContainsKey(key))
        {          
            if (!string.IsNullOrEmpty(configText[key]))
            {
                str = configText[key];
            }
        }
        return str;
    }

    public static void XmlToTxt(string filePath,char split)
    {
        StringBuilder temstr = new StringBuilder();
        LauguageConfig lauguageTemp = new LauguageConfig();
        TextAsset text = Resources.Load("TextConfig", typeof(TextAsset)) as TextAsset;
        if (null != text)
        {
            lauguageTemp = XmlHelper.XmlDeserialize<LauguageConfig>(text.text.Trim(), Encoding.UTF8);
        }
        if (lauguageTemp != null)
        {
            string hh = split.ToString();
            Debug.Log(hh);
            foreach (var tem in lauguageTemp.TextCells)
            {
                temstr.Append(tem.GetText(LauguageType.Chinese) + hh + tem.GetText(LauguageType.English) + hh + tem.GetText(LauguageType.Japanese) + hh + tem.key + "\n");
            }

            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs);
            fs.SetLength(0);//首先把文件清空了。
            sw.Write(temstr);//写你的字符串。
            sw.Close();
        }
    }

    public static void AddOnelaugageToXml(string filePath,char[] split,LauguageType type)
    {
        FileStream fss = new FileStream(filePath, FileMode.Open);
        StreamReader s = new StreamReader(fss);
        Dictionary<string, string> index_str = new Dictionary<string, string>();
        string[] str;
        while (!s.EndOfStream)
        { 
            str = s.ReadLine().Split(split);
            if (str.Length == 2 && str[1] != "")
            {
                try
                {
                    index_str.Add(str[1], str[0]);
                }
                catch
                {
                    Debug.Log(str[1].ToString());
                }
            }
        }
        s.Close();
        LauguageConfig lauTemp = new LauguageConfig();
        TextAsset text = Resources.Load("TextConfig", typeof(TextAsset)) as TextAsset;
        if (null != text)
        {
            lauTemp = XmlHelper.XmlDeserialize<LauguageConfig>(text.text.Trim(), Encoding.UTF8);
        }
        if (lauTemp != null)
        {
            int index = -1;
            for (int i = 0; i < lauTemp.TextCells[0].lauguages.Count; i++)  //找到要加入的语言对应的lauguages索引
            {
                if (lauTemp.TextCells[0].lauguages[i].lauType == type)
                    index = i;
            }
            if (index == -1)
                return;
            foreach (var tem in lauTemp.TextCells)
            {
                if (!index_str.ContainsKey(tem.key))
                {
                    Debug.Log("key不存在：" + tem.key);
                    continue;
                }
                tem.lauguages[index].value = index_str[tem.key];
            }
            string strr = XmlHelper.XmlSerialize(lauTemp, Encoding.UTF8);
            FileStream fs = new FileStream("d:/finalTTkk.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(strr);
            sw.Close();
            return;
        }
    }

    /// <summary>
    /// 将格式化的txt 转换成xml txt 格式为lau1#lua2#lua3#...#key  
    /// type 则为{lau1,lau2,lau3 ...}
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="split"></param>
    /// <param name="type"></param>
    public static void TexToXml(string filePath, char[] split, LauguageType[] type)
    {
        LauguageConfig lau = new LauguageConfig();
        FileStream fss = new FileStream(filePath, FileMode.Open);
        StreamReader s = new StreamReader(fss);//File.Open(filePath);
        string[] str;
        while (!s.EndOfStream)
        {
            str = s.ReadLine().Split(split);
            TextCell cell = new TextCell();
            cell.key = str[str.Length - 1];
            for (int i = 0; i < type.Length; i++)
            {
             //   Debug.Log(str.Length + "--"+ str[0]);
                Lauguage item = new Lauguage();
                item.lauType = type[i];
                
                item.value = str[i];
                cell.lauguages.Add(item);
            }
            lau.AddToTypes(cell);
        }
        s.Close();
        if (lau != null)
        {
            string strr = XmlHelper.XmlSerialize(lau, Encoding.UTF8);
            FileStream fs = new FileStream("d:/finalTT.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(strr);
            sw.Close();
            return;
        }
    }

    /// <summary>
    /// 将格式化的txt文件转化成xml，中文#英文#日文#tipindex
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="split"></param>
    public static void TxtToXml(string filePath, char[] split)
    {
        FileStream fss = new FileStream(filePath,FileMode.Open);
        StreamReader s = new StreamReader(fss);//File.Open(filePath);
        LauguageConfig lau = new LauguageConfig();
        string[] str;
        while (!s.EndOfStream)
        {
            str = s.ReadLine().Split(split);
            if (str.Length == 4 && str[3] != "")
            {
                TextCell cell = new TextCell();
                cell.key = str[3];
                Lauguage item1 = new Lauguage();
                Lauguage item2 = new Lauguage();
                Lauguage item3 = new Lauguage();
                item1.lauType = LauguageType.Chinese;
                item2.lauType = LauguageType.English;
                item3.lauType = LauguageType.Japanese;
                Lauguage item4 = new Lauguage();
                item4.lauType = LauguageType.Korean;
                item1.value = str[0];
                item2.value = str[1];
                item3.value = str[2];
                cell.lauguages.Add(item1);
                cell.lauguages.Add(item2);
                cell.lauguages.Add(item3);
                cell.lauguages.Add(item4);

                //if (str[3].Contains("guideTip"))
                //{
                //    if (str[1].Length > 52)
                //    {
                //        Debug.Log(str[3] + " # " + str[1]);
                //    }
                //}
                //else
                //{
                //    if (str[1].Length > 3 * str[0].Length && str[0].Length > 2)
                //    {
                //        Debug.Log(str[3] + " + " + str[1]);
                //    }
                //}

                lau.AddToTypes(cell);
            }
        }
        s.Close();
        if (lau != null)
        {
            string strr = XmlHelper.XmlSerialize(lau, Encoding.UTF8);
            FileStream fs = new FileStream("d:/finalTT.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(strr);
            sw.Close();
            return;
        }
    }

    public static void CheckLauguageLength()
    {
      //  string key;
      //  string 

    }

    /// <summary>
    /// 更新语言
    /// </summary>
    public static void UpdateNewlauguage()
    {
        LauguageConfig lauTemp = new LauguageConfig();
        TextAsset text = Resources.Load("TextConfig", typeof(TextAsset)) as TextAsset;
        if (null != text)
        {
            lauTemp = XmlHelper.XmlDeserialize<LauguageConfig>(text.text.Trim(), Encoding.UTF8);
        }
        if (lauTemp != null)
        {
            foreach (var tem in lauTemp.TextCells)
            {
                Lauguage german = new Lauguage();
                Lauguage italy = new Lauguage();
                german.lauType = LauguageType.German;
                italy.lauType = LauguageType.Italy;
                tem.lauguages.Add(german);
                tem.lauguages.Add(italy);
            }

            string strr = XmlHelper.XmlSerialize(lauTemp, Encoding.UTF8);
            FileStream fs = new FileStream("d:/finalTTp.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(strr);
            sw.Close();
            return;
        }
    }
    /// <summary>
    /// 判断是否是阿拉伯语
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsArab(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }
        for (int i = 0; i < str.Length; ++i)
        {
            char c = str[i];
            if (c >= 0x0600 && c <= 0x06FF)
            {//阿拉伯语
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 把阿拉伯语逆序
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ConvertArab(string str)
    {
        char[] chars = str.ToCharArray();
        if (null != chars)
        {
            List<List<char>> list = new List<List<char>>();
            int newState = 0;
            int oldState = 0;
            List<char> addList = null;
            for (int i = 0, imax = chars.Length; i < imax; ++i)
            {
                if (chars[i] > 127)
                {
                    newState = -1;
                }
                else
                {
                    newState = 1;
                }
                if (newState != oldState)
                {
                    if (null != addList)
                    {
                        list.Add(addList);
                    }
                    addList = new List<char>();
                    oldState = newState;
                }
                if (newState == -1 || chars[i] == ' ' || chars[i] == '.' || chars[i] == ',' || chars[i] == ';')
                {//阿拉伯语，需逆序||空格需逆序
                    if (addList.Count < 1)
                    {
                        addList.Add(chars[i]);
                    }
                    else
                    {
                        addList.Insert(0, chars[i]);
                    }
                }
                else
                {
                    addList.Add(chars[i]);
                }
            }
            if (null != addList)
            {
                list.Add(addList);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = list.Count - 1; i >= 0; --i)
            {//把list逆序
                sb.Append(list[i].ToArray());
            }
            return sb.ToString();
        }
        return string.Empty;
    }
}

[XmlType ("Text")]
public class LauguageConfig
{
    [XmlElement ("KEY")]
    public List<TextCell> TextCells;

    public LauguageConfig()
    {
        TextCells = new List<TextCell>();
    }

    public Dictionary<string,string> GetDicByType(LauguageType type)
    {
        Dictionary<string, string> keysValue = new Dictionary<string, string>();
        foreach (var tem in TextCells)
        {
            if(!keysValue.ContainsKey(tem.key))
                keysValue.Add(tem.key, tem.GetText(type));
        }
        return keysValue;
    }

    public bool AddToTypes(TextCell type) //避免重复的添加
    {
        for (int i = 0, imax = TextCells.Count; i < imax; ++i)
        {
            if (TextCells[i].key == type.key)
            {
                TextCells[i] = type;
                return false;
            }            
        }
        TextCells.Add(type);
        return true;
    }

    public void AddForChinese(TextCell language, string chineseValue)
    {
        for (int i = 0, imax = TextCells.Count; i < imax; ++i)
        {
            List<Lauguage> list = TextCells[i].lauguages;
            if (null != list)
            {
                for (int index = 0, indexMax = list.Count; index < indexMax; ++index)
                {
                    if (list[index].lauType == LauguageType.Chinese)
                    {
                        if (list[index].value.TrimEnd(' ').Replace("。", "").Replace(".", "").Replace("！", "").Replace("!", "").Replace("？", "").Replace("?", "") == chineseValue.TrimEnd(' ').Replace("。", "").Replace(".", "").Replace("！", "").Replace("!", "").Replace("？", "").Replace("?", ""))
                        {
                            TextCells[i] = language;
                        }
                        break;
                    }
                }
            }
            
        }
    }

    public bool DelLauguage(TextCell text)
    {
        for (int i = 0, imax = TextCells.Count; i < imax; ++i)
        {
            if (TextCells[i].key == text.key)
            {
                TextCells.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public TextCell FindForKey(string key)
    {
        for (int i = 0, imax = TextCells.Count; i < imax; ++i)
        {
            if (TextCells[i].key == key || TextCells[i].key.Contains(key))
            {
                return TextCells[i];
            }
        }
        return null;
    }

    public TextCell[] FindAllForKey(string key)
    {
        List<TextCell> list = null;
        for (int i = 0, imax = TextCells.Count; i < imax; ++i)
        {
            if (TextCells[i].key == key || TextCells[i].key.Contains(key))
            {
                if (null == list)
                {
                    list = new List<TextCell>();
                }
                list.Add(TextCells[i]);
            }
        }
        if (null != list)
        {
            return list.ToArray();
        }
        return null;
    }

    public TextCell FindForLauguage(Lauguage lauguage)
    {
        for (int i = 0, imax = TextCells.Count; i < imax; ++i)
        {
            string text = TextCells[i].GetText(lauguage.lauType);
            if (!string.IsNullOrEmpty(text))
            {
                if (lauguage.value.TrimEnd(' ').Replace("。", "").Replace(".", "").Replace("！", "").Replace("!", "").Replace("？", "").Replace("?", "") == text.TrimEnd(' ').Replace("。", "").Replace(".", "").Replace("！", "").Replace("!", "").Replace("？", "").Replace("?", ""))
                {
                    return TextCells[i];
                }
            }
        }
        return null;
    }

    public TextCell[] FindAllForLauguage(Lauguage lauguage)
    {
        List<TextCell> list = null;
        for (int i = 0, imax = TextCells.Count; i < imax; ++i)
        {
            string text = TextCells[i].GetText(lauguage.lauType);
            if (!string.IsNullOrEmpty(text) && (text.Equals(lauguage.value) || text.Contains(lauguage.value)))
            {
                if (null == list)
                {
                    list = new List<TextCell>();
                }
                list.Add(TextCells[i]);
            }
        }
        if (null != list)
        {
            return list.ToArray();
        }
        return null;
    }

    public void ClearAllLauguage()
    {
        TextCells.Clear();
    }
}

public class TextCell
{
    [XmlAttribute]
    public string key;
    [XmlElement ("lauguage")]
    public List<Lauguage> lauguages;

    public TextCell()
    {
        lauguages = new List<Lauguage>();
    }

    public string GetText(LauguageType type)
    {
        string eg = string.Empty;
        foreach (var tem in lauguages)
        {
            if (tem.lauType == LauguageType.English)
            {
                eg = tem.value;
            }
            if ((int)type == (int)tem.lauType)
            {
                if (type == LauguageType.Arab)
                {
                    if (LauguageTool.IsArab(tem.value))
                    {
                        return LauguageTool.ConvertArab(tem.value);
                    }
                    else
                    {
                        return tem.value;
                    }
                }
                else
                {
                    return tem.value;
                }
            }
                
        }
        return eg;
    }

    public TextCellEX TurnTo()
    {
        TextCellEX te = new TextCellEX();
        te.caochu = "";
        te.key = this.key;
        te.lauguages = this.lauguages;
        return te;
    }
}

public class LauguageConfigEX
{ 
    [XmlElement ("KEY")]
    public List<TextCellEX> TextCells;

    public LauguageConfigEX()
    {
        TextCells = new List<TextCellEX>();
    }

    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="lau"></param>
    /// <returns></returns>
    public static LauguageConfigEX Turnto(LauguageConfig lau)
    {
        LauguageConfigEX lauu = new LauguageConfigEX();
        foreach (var tem in lau.TextCells)
        {
            lauu.TextCells.Add(tem.TurnTo());
        }
        return lauu;
    }
}

public class TextCellEX
{ 
    [XmlAttribute]
    public string key;

    [XmlElement]
    public string caochu;

    [XmlElement ("lauguage")]
    public List<Lauguage> lauguages;

    public TextCellEX()
    {
        lauguages = new List<Lauguage>();
    }
}

public class Lauguage
{
    [XmlAttribute]
    public LauguageType lauType;
    [XmlText]
    public string value;
}

public enum LauguageType
{
    Chinese,//中文
    English,//英文
    Japanese,//日语
    Korean,//韩语
    German,//德语
    Italy,//意大利
    French,//法语
    Spanish,//西班牙语
    Portugal,//葡萄牙
    HokongChinese,//繁体
    Arab,//阿拉伯
    Russian,//俄语
    Polaic,//波兰
    Turkish,//土耳其语
    Danish,//丹麦语
    Thai,//泰语
    Indonesian,//印尼
    Ohter,
}

