
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class XmlTool
{
    public delegate void ReadXmlCallBack(XmlElement element);
    public delegate bool WriteXmlCallBack(XmlDocument doc, XmlElement roots);
    public static bool ReadXmlByPath(string path, ReadXmlCallBack callback)
    {
        if (!File.Exists(path))
        {//文件不存在
            return false;
        }
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        return ReadXml(xmlDoc, callback);
    }

    public static bool ReadXmlByContent(string text, ReadXmlCallBack callback)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(text);
        return ReadXml(xmlDoc, callback);
    }

    static bool ReadXml(XmlDocument xmlDoc, ReadXmlCallBack callback)
    {
        XmlElement xmlRoot = xmlDoc.DocumentElement;
        XmlNode node = xmlRoot.FirstChild;
        if (null == node)
        {
            return true;
        }
        while (null != node)
        {
            if (!(node is XmlElement))
            {
                continue;
            }
            XmlElement xe = (XmlElement)node;
            if (null != callback)
            {
                callback(xe);
            }
            node = node.NextSibling;
        }
        return true;
    }


    public static bool WriteXml(string path, WriteXmlCallBack callback, string rootName = "Roots")
    {
        //新建xml实例
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "no");
        //将新节点添加到文档中。  
        XmlElement root = xmlDoc.DocumentElement;
        xmlDoc.InsertBefore(xmldecl, root);
        XmlElement roots = xmlDoc.CreateElement(rootName);
        xmlDoc.AppendChild(roots);
        if (null != callback)
        {
            callback(xmlDoc, roots);
        }
        xmlDoc.Save(path);
        return true;
    }
}

