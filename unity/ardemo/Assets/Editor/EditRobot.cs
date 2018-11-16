using Game.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:EditRobot.cs
/// Description:
/// Time:2016/4/5 13:16:47
/// </summary>
public class EditRobot
{
    #region 公有属性
    public string filePath;
    public string name;
    public string id;
    public ResFileType modelType;
    public Dictionary<string, int> duoJiDict;
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    public EditRobot(string path, string name)
    {
        this.name = name;
        this.filePath = path;
        duoJiDict = new Dictionary<string, int>();
        ReadXml();
    }

    public string GetDuoJiString()
    {
        List<int> list = new List<int>();
        if (null != duoJiDict)
        {
            foreach (KeyValuePair<string, int> kvp in duoJiDict)
            {
                list.Add(kvp.Value);
            }
        }
        list.Sort();
        return PublicFunction.ListToString<int>(list);
    }

    public List<int> GetServos()
    {
        List<int> list = new List<int>();
        if (null != duoJiDict)
        {
            foreach (KeyValuePair<string, int> kvp in duoJiDict)
            {
                list.Add(kvp.Value);
            }
        }
        list.Sort();
        return list;
    }

    public void ChangeModelID()
    {
        this.id = CreateID.CreateRobotID();
        SaveId();
    }
    #endregion

    #region 其他函数
    void ReadXml()
    {
        try
        {
            string path = Path.Combine(this.filePath, this.name + ".xml");
            if (!File.Exists(path))
            {//文件不存在
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            XmlNode node = xmlRoot.FirstChild;
            while (null != node)
            {
                if (!(node is XmlElement))
                {
                    continue;
                }
                switch (node.Name)
                {
                    case "RobotID":
                        this.id = node.InnerXml;
                        break;
                    case "DataType":
                        this.modelType = ResourcesEx.GetResFileType(node.InnerXml);
                        break;
                    case "GO":
                        XmlElement xe = (XmlElement)node;
                        string type = xe.GetAttribute("type");
                        if (!string.IsNullOrEmpty(type))
                        {
                            if (type.Equals(PublicFunction.Duoji_Type) || type.Equals(PublicFunction.Duoji_Type_Old))
                            {
                                string name = xe.GetAttribute("name");
                                string djid = xe.GetAttribute("djid");
                                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(djid) && PublicFunction.IsInteger(djid))
                                {
                                    duoJiDict[name] = int.Parse(djid);
                                }
                            }
                        }

                        break;
                }

                node = node.NextSibling;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("读取机器人数据失败" + ex.ToString());
        }
    }

    public void SaveId()
    {
        try
        {
            string path = Path.Combine(this.filePath, this.name + ".xml");
            if (!File.Exists(path))
            {//文件不存在
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            XmlNode node = xmlRoot.FirstChild;
            while (null != node)
            {
                if (!(node is XmlElement))
                {
                    continue;
                }
                switch (node.Name)
                {
                    case "RobotID":
                        node.InnerXml = this.id;
                        //this.id = node.InnerXml;
                        break;
                        /*case "DataType":
                            this.modelType = PublicFunction.ToResFileType(node.InnerXml);
                            break;
                        case "GO":
                            XmlElement xe = (XmlElement)node;
                            string type = xe.GetAttribute("type");
                            if (!string.IsNullOrEmpty(type))
                            {
                                if (type.Equals(PublicFunction.Duoji_Type) || type.Equals(PublicFunction.Duoji_Type_Old))
                                {
                                    string name = xe.GetAttribute("name");
                                    string djid = xe.GetAttribute("djid");
                                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(djid) && PublicFunction.IsInteger(djid))
                                    {
                                        duoJiDict[name] = int.Parse(djid);
                                    }
                                }
                            }

                            break;*/
                }

                node = node.NextSibling;
            }
            xmlDoc.Save(path);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("读取机器人数据失败" + ex.ToString());
        }
    }
    #endregion
}