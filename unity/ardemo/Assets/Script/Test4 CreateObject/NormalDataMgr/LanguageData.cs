/*
 * 作用：获取翻译的数据信息
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class LanguageData
{
     XmlDocument x;
     public LanguageData()
    {
        Init();
    }
    void Init()
    {
        string t = GetXmlByPath().ToString().Trim();
        x = new XmlDocument();
        x.LoadXml(t);
       // xele = x.ToXDocument();
    }

    public TextAsset GetXmlByPath()
    {
        TextAsset tmp = Resources.Load("Script/Test4/Language/language") as TextAsset;

        return tmp;
    }

    public Dictionary<string, string> FindGOType(string langType)
    {
        XmlNodeList nodeList = x.GetElementsByTagName("Type");
        Dictionary<string, string> goType = new Dictionary<string, string>();
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;   //xe.InnerText:按钮的名称
                    //if (xe.InnerText==btnNam)    
                    //{
                    XmlNodeList nodeList1 = xe.GetElementsByTagName(langType);

                    if (nodeList1 != null && nodeList1.Count > 0)
                    {
                        foreach (XmlNode x1n in nodeList1)
                        {
                            if (x1n != null)
                            {
                                XmlElement x1e = (XmlElement)x1n;
                                goType.Add(xe.GetAttribute("type"),x1e.InnerText);
                            }
                            if(goType!=null&&goType.Count>0)
                            {
                                break;
                            }
                        }
                    }
                }

            }
        }
        return goType;
    }
}
