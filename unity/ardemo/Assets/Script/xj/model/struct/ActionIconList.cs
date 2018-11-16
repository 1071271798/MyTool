using Game.Platform;
using MyMVC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ActionIconList.cs
/// Description:动作图标
/// Time:2015/12/7 11:12:06
/// </summary>

public class ActionIconList/* : ScriptableObject*/
{
    public List<ActionIcon> iconList;


    public void LoadXml()
    {
        try
        {
            TextAsset text = Resources.Load<TextAsset>("actionicon");
            iconList = new List<ActionIcon>();
            if (null != text)
            {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(text.text.Trim());
                XmlElement xmlRoot = xd.DocumentElement;
                XmlNode node = xmlRoot.FirstChild;
                while (null != node)
                {
                    if (!(node is XmlElement))
                    {
                        continue;
                    }
                    XmlElement xe = (XmlElement)node;
                    ActionIcon act = new ActionIcon(xe);
                    iconList.Add(act);
                    node = node.NextSibling;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    public void SaveXml()
    {
        try
        {
            if (null == iconList)
            {
                return;
            }
            //新建xml实例
            XmlDocument xmlDoc = new XmlDocument();
            //创建根节点，最上层节点
            XmlElement roots = xmlDoc.CreateElement("Roots");
            xmlDoc.AppendChild(roots);
            
            //添加内容
            for (int i = 0, icount = iconList.Count; i < icount; ++i)
            {
                XmlElement node = xmlDoc.CreateElement("Root");
                node = iconList[i].ToXmlElement(node);
                roots.AppendChild(node);
            }
            //将xml文件保存到本地
            xmlDoc.Save(Application.dataPath + "/Resources/actionicon.xml");
            /*string str = XmlHelper.XmlSerialize(this, Encoding.UTF8);
            FileStream fs = new FileStream(Application.dataPath + "/Resources/actionicon.cfg", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            fs.SetLength(0);//首先把文件清空了。
            sw.Write(str);//写你的字符串。
            sw.Close();*/
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

//[Serializable]
public class ActionIcon
{
    public string id;
    public string iconName;
    public int useCount;

    public ActionIcon(string id, string icon, int count)
    {
        this.id = id;
        this.iconName = icon;
        this.useCount = count;
    }

    public ActionIcon(XmlElement xe)
    {
        try
        {
            id = xe.GetAttribute("id");
            iconName = xe.GetAttribute("iconName");
            useCount = int.Parse(xe.GetAttribute("useCount"));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public XmlElement ToXmlElement(XmlElement xe)
    {
        xe.SetAttribute("id", id);
        xe.SetAttribute("iconName", iconName);
        xe.SetAttribute("useCount", useCount.ToString());
        return xe;
    }
}