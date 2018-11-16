using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:LogicLanguage.cs
/// Description:逻辑编程多语言文件
/// Time:2016/6/24 10:20:30
/// </summary>
public class LogicLanguage : SingletonObject<LogicLanguage>
{
    Dictionary<string, Dictionary<LauguageType, string>> languageDict;


    public LogicLanguage()
    {
        ReadConfigTxt();
    }


    public static string GetText(string key)
    {
        return LogicLanguage.GetInst().GetTextForKey(key, LauguageTool.GetIns().CurLauguage);
    }

    public static string GetText(string key, LauguageType type)
    {
        return LogicLanguage.GetInst().GetTextForKey(key, type);
    }


    public static LauguageType GetStringType(string type)
    {
        for (LauguageType i = LauguageType.Chinese; i < LauguageType.Ohter; ++i)
        {
            if (type.Equals(i.ToString()))
            {
                return i;
            }
        }
        return LauguageType.Ohter;
    }

    //////////////////////////////////////////////////////////////////////////

    string GetTextForKey(string key, LauguageType lgType)
    {
        if (null != languageDict)
        {
            if (languageDict.ContainsKey(key))
            {
                if (languageDict[key].ContainsKey(lgType))
                {
                    return languageDict[key][lgType];
                }
                else if (languageDict[key].ContainsKey(LauguageType.English))
                {
                    return languageDict[key][LauguageType.English];
                }
            }
        }
        return key;
    }
    void ReadConfigTxt()
    {
        TextAsset textAsset = Resources.Load("TextConfig", typeof(TextAsset)) as TextAsset;
        if (null != textAsset)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(textAsset.ToString().Trim());
            XmlNode firstNode = xmlDoc.DocumentElement;
            if (null != firstNode)
            {
                languageDict = new Dictionary<string, Dictionary<LauguageType, string>>();
                XmlNodeList node = firstNode.ChildNodes;
                for (int i = 0, imax = node.Count; i < imax; ++i)
                {
                    XmlElement xe = (XmlElement)node[i];
                    string key = xe.GetAttribute("key");
                    Dictionary<LauguageType, string> dict = new Dictionary<LauguageType, string>();
                    XmlNodeList childNode = node[i].ChildNodes;
                    for (int index = 0, indexMax = childNode.Count; index < indexMax; ++index)
                    {
                        XmlElement childXe = (XmlElement)childNode[index];
                        string lgType = childXe.GetAttribute("lauType");
                        string value = childNode[index].InnerXml;
                        dict[GetStringType(lgType)] = value;
                    }
                    
                    languageDict[key] = dict;
                }
            }
        }
    }
}