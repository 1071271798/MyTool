using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Xml.Linq;
//using System.Linq;
using System.Xml;
using System;
using System.IO;
using Game.Resource;

public class RecordInfo
{
    private string path;
    private static RecordInfo _instance;
    public RecordInfo() { }
    static object sLock = new object();
    public static RecordInfo Instance
    {
        get
        {
            if (null == _instance)
            {
                lock (sLock)
                {
                    if (_instance == null)
                    {

                        _instance = new RecordInfo();
                    }
                }
            }

            return _instance;
        }
    }

    //生成物体的XML的名称
    private static XmlDocument xele=new XmlDocument();

    XmlElement xmlelem;

   public string robotname;

   #region 生成机器人ID
   public string Robot_Font = "robot_";
   public string CreateRobotID()
   {
       return Robot_Font + CreateGuid();
   }
   public string CreateGuid()
   {
       return System.Guid.NewGuid().ToString();
   }
   #endregion

    public string Path
    {
        get
        {
            path = GetResourcesPath(robotname);

            return path;
        }
    }

    public string GetResourcesPath(string xmlName)
    {
        string pathfile = "";

        pathfile = ResourcesEx.persistentDataPath + "/rawData/" + xmlName;
        //Debug.Log(Application.persistentDataPath);
        if (!Directory.Exists(pathfile))
        {
            Directory.CreateDirectory(pathfile);
        }

        return pathfile + "/" + xmlName + ".xml";
    }

    public void CreateXML(string nameTemp)
    {
        robotname = nameTemp;
        xele = new XmlDocument();

        //加入XML的声明段落,<?xml version="1.0" encoding="gb2312"?>
        XmlDeclaration xmldecl;
        xmldecl = xele.CreateXmlDeclaration("1.0", "utf-8", "yes");
        xele.AppendChild(xmldecl);

        //加入一个根元素
        xmlelem = xele.CreateElement("Root");
        xele.AppendChild(xmlelem);//添加到文本中 

        //给根节点添加一个子节点
        XmlElement xel = xele.CreateElement("RobotID");
        xel.InnerText = CreateRobotID(); //设置文本节点 机器人ID
        xmlelem.AppendChild(xel);

        //给根节点，模型的类型
        XmlElement xell = xele.CreateElement("DataType");
        xell.InnerText = "default"; //设置文本节点 机器人ID
        xmlelem.AppendChild(xell);

        //给根节点，模型的等级
        XmlElement xel2 = xele.CreateElement("Level");
        xel2.InnerText = "1"; //设置文本节点 机器人ID
        xmlelem.AppendChild(xel2);
        xele.Save(Path);  
        
    }


    #region 添加

    //添加新生成的物体
    public void AddGO(string pickGO, string id,string type, string pos, string angl,string scale,string djid,string gocolor)
    {
        XmlNode root = xele.SelectSingleNode("Root");
        XmlElement x = xele.CreateElement("GO");

        x.SetAttribute("name", pickGO);
        x.SetAttribute("id", id);
        x.SetAttribute("type", type);

        x.SetAttribute("goPos", pos);
        x.SetAttribute("goAngle", angl);
        x.SetAttribute("goScale", scale);

        x.SetAttribute("djid", djid);
        x.SetAttribute("goColor", gocolor);

        x.SetAttribute("isDP", "1");
        //注释
       // XElement notes = new XElement("添加鼠标选中的物体");
        if (root != null)
        {
            root.AppendChild(x);
        }

        xele.Save(Path);
    }

    //给GO物体添加开始位置
    public void AddPosAngleScale(string pickGO, string pos, string angl, string scale)
    {
        XmlNode root = xele.SelectSingleNode("Root");
        XmlElement x = xele.CreateElement("Line");

        x.SetAttribute("name", pickGO);

        x.SetAttribute("goPos", pos);
        x.SetAttribute("goAngle", angl);
        x.SetAttribute("goScale", scale);

        //注释
        // XElement notes = new XElement("添加鼠标选中的物体");
        if (root != null)
        {
            root.AppendChild(x);
        }

        xele.Save(Path);
    }

    //给GO物体添加开始位置
    public void AddStartPos(string pickGO, string startPos, string djid,string gocolor,string djshape)
    {
        robotname = "K2";
        xele.Load(Path); 
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.GetAttribute("name") == pickGO)
                    {
                        xe.SetAttribute("startPos", startPos);
                        xe.SetAttribute("djid", djid);
                        xe.SetAttribute("goColor", gocolor);
                        xe.SetAttribute("djShape", djshape);
                        xe.SetAttribute("isDP", "1");
                    }
                }

            }
        }
        xele.Save(Path);
    }
    //给GO物体添加颜色
    public void AddColor(string pickGO,string colort)
    {
        robotname = "K2";
        xele.Load(Path); 
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        //Debug.Log("count:" + nodeList.Count);
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.GetAttribute("name") == pickGO)
                    {
                        //Debug.Log("color:"+colort);
                        xe.SetAttribute("goColor", colort);
                    }
                    
                }

            }
        }
        xele.Save(Path);
    }

    //添加与鼠标选中物体连接的物体的名称和位置
    //pickGO：选中物体的名字；posID：是Pos还是PrePos；pos:鼠标选中物体的位置；id：该位置相对其他位置生成物体的顺序
    //contactGO:对应pos处连接的物体的名字；contactPoint：对应pos处连接物体的位置
    public void AddContactInf(string pickGO, string posID, string pos, string contactGO, string contactPoint)
    {
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.GetAttribute("name") == pickGO)    //找到GO
                    {
                        XmlElement x = xele.CreateElement(posID);
                        x.InnerText = pos;
                        x.SetAttribute("contactGO", contactGO);
                        x.SetAttribute("contactPoint", contactPoint);
                        xe.AppendChild(x);
                    }
                }

            }
        }
       // Debug.Log("output");

        xele.Save(Path);
    }



    #endregion

    #region 查找
    public string[] FindGOPosAngle(string pickGO)
    {
        string[] x = new string[3];     //x[0]=goPos;x[1]=goAngle
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.GetAttribute("name") == pickGO)    //找到GO
                    {
                        x[0] = xe.GetAttribute("goPos");
                        x[1] = xe.GetAttribute("goAngle");
                        if (xe.HasAttribute("startPos") && xe.GetAttribute("startPos") != null)
                        {
                            x[2] = xe.GetAttribute("startPos");
                        }
                        else
                        {
                            x[2] = "(0,0,0)";
                        }
                    }
                }

            }
        }
        return x;           

    }

    //查找XML文档中所有的物体的名字
    public void FindAllGOName(List<string> goName)
    {
        //XElement xele = XElement.Load(Path);
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    goName.Add(xe.GetAttribute("name"));
                }

            }
        }

    }
    #endregion

    #region 判断是否
    //判断物体是否有PrePos连接物
    public bool HasPrePos(string pickGO)
    {
        bool HasPrePos = false;
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.GetAttribute("name") == pickGO)    //找到GO
                    {
                        XmlNodeList childNodeList = xe.GetElementsByTagName("PrePos");
                        if (childNodeList != null && childNodeList.Count > 0)
                        {
                          HasPrePos = true;
                        }
                    }
                }

            }
        }
        return HasPrePos;
    }
    #endregion




#region 零件信息统计
    public void CreateXElement(string nameTemp)
    {
        
        robotname = nameTemp;
        xele = new XmlDocument();

        //加入XML的声明段落,<?xml version="1.0" encoding="gb2312"?>
        XmlDeclaration xmldecl;
        xmldecl = xele.CreateXmlDeclaration("1.0", "utf-8", "yes");
        xele.AppendChild(xmldecl);

        //加入一个根元素
        xmlelem = xele.CreateElement("Root");
        xele.AppendChild(xmlelem);//添加到文本中 

        xele.Save(Path);

    }
    //给GO物体添加开始位置
    public void AddParts(string partsName)
    {
        robotname = "K2";
        xele.Load(Path);

        XmlNode root = xele.SelectSingleNode("Root");
        XmlElement x = xele.CreateElement("Parts");
        x.InnerText = partsName;
        if (root != null)
        {
            root.AppendChild(x);
        }
        xele.Save(Path);
    }
#endregion
}