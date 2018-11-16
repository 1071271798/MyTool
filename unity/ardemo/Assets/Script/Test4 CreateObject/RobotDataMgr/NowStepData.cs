/*
 *功能：记录离开组装时，当时该机器人单步组装执行到的步数 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Xml.Linq;
//using System.Linq;
using System.Xml;
using System;
using System.IO;
using Game.Resource;

public class NowStepData
{
    
    private string path;

    private static NowStepData _instance;
    public NowStepData() { }
    static object sLock = new object();
    public static NowStepData Instance
    {
        get
        {
            if (null == _instance)
            {
                lock (sLock)
                {
                    if (_instance == null)
                    {

                        _instance = new NowStepData();
                    }
                }
            }

            return _instance;
        }
    }

    //生成物体的XML的名称
    //public string xmlName;
    //public string robotIDNum;

    public List<string> existXMLNams = new List<string>();

    // private static XElement xelell;
    //private static XElement xele;
    private static XmlDocument xelell = new XmlDocument();
    private static XmlDocument xele=new XmlDocument();

    XmlNode xmlnode;
    XmlElement xmlelem;
    //public UILabel xmlNameLabel;

   FileInfo[] fis;
   string dir;
   string[] xmlN = new string[2];
   public string t = "tt";
   string goAngle;
   string robotID;
   string[] xangles = new string[3];
   bool canProduce = false;
   string type2nd;
   string tempType;
   string tempid;
   public string robotname;
    public void ClearXele()
   {
      // xele.RemoveAll();
   }
    public void createPathInit(string filename,string rbtid)
    {
        if (filename != null & filename != " ")
        {
            robotname = filename;
            xele.Load(Path);
            if(xele !=null)
            {
                xele.RemoveAll();
                //加入一个根元素
                xmlelem = xele.CreateElement("Root");
                xele.AppendChild(xmlelem);//添加到文本中 

                xele.Save(Path);  
            }
        }
    }

    public void pathInit(string filename)
    {
        if (filename != null & filename != " ")
        {
            robotname = filename;
            xele.Load(Path); 
        }
    }

    public string Path
    {
        get
        {
            path = GetResourcesPath("nowstep");
            return path;
        }
    }

    public string GetResourcesPath(string xmlName)
    {
        string pathfileDefault = "";

        pathfileDefault = ResourcesEx.GetCommonPathForNoTypeName(xmlName);
        
        if (!Directory.Exists(pathfileDefault))
        {
            Directory.CreateDirectory(pathfileDefault);
        }
        return pathfileDefault + "/" + xmlName + ".xml";
    }

    public void CreateXElement()
    {
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


    #region 查找手机上已有的机器人
    //自制--查询准备新建的XML是否与已有文档重名
    public List<string> XMLExist()
    {
        List<string> existRobots = XMLExistNormal("playerdata");
        return existRobots;
    }

    //下载
    public List<string> XMLExistDownLoad()
    {
        List<string> existRobots = XMLExistNormal("download");
        return existRobots;
    }

    //默认
    public List<string> XMLExistDefault()
    {
        List<string> existRobots = XMLExistNormal("default");
        return existRobots;
    }

    DirectoryInfo di;

    //自制--查询准备新建的XML是否与已有文档重名
    public List<string> XMLExistNormal(string fileName)
    {
        string pathfile = string.Empty;
        if (fileName.Equals("default"))
        {
            pathfile = ResourcesEx.GetCommonRootPath();
        }
        else if (fileName.Equals("download"))
        {
            pathfile = PublicFunction.CombinePath(ResourcesEx.persistentDataPath, "download");
        }
        else
        {
            pathfile = ResourcesEx.GetUserRootPath();
        }
        if (!Directory.Exists(pathfile))
        {
            Directory.CreateDirectory(pathfile);
        }
        dir = pathfile;
        string[] filename = System.IO.Directory.GetDirectories(dir);
        if (filename != null)
        {
            foreach (string fi in filename)
            {
                //string FileName = dir + fi.Name;
                //Debuger.Log("filename:" + fi);
                di = new DirectoryInfo(fi);
                fis = di.GetFiles();

                if (fis != null)
                {
                    foreach (FileInfo fil in fis)
                    {
                        string FileName = dir + fil.Name;

                        if (System.IO.Path.GetExtension(FileName) == ".xml")
                        {
                            string name = DeleteNameExtension(fil.Name);
                            if (!existXMLNams.Contains(name))
                            {
                                existXMLNams.Add(name);
                            }

                        }
                    }
                }
            }
        }
        return existXMLNams;
    }
    #endregion

    #region 删除
    //删除名字的后缀名
    public string DeleteNameExtension(string xmlNam)
    {
        xmlN = xmlNam.Split('.');
        return xmlN[0];
    }
    #endregion


    #region 添加

    //添加新生成的物体
    public void AddGO(string id,string robotName)
    {

        XmlNode root = xele.SelectSingleNode("Root");
        XmlElement x = xele.CreateElement("GO");
        
        x.SetAttribute("step", id);
        x.SetAttribute("robotname",robotName);

        //注释
       // XElement notes = new XElement("添加鼠标选中的物体");
        if (root != null)
        {
            root.AppendChild(x);
        }

        xele.Save(Path);
    }
    #endregion

    #region 查找
    //查询鼠标选中物体Pos连接的物体
    public string FindPosContactInf(string robotName, string pos)
    {
        string x = "";     //x[0]=contactgo;x[1]=contactpoint
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.GetAttribute("robotname") == robotName)    //找到GO
                    {
                       x = xe.GetAttribute("step");       
                    }
                }

            }
        }
        return x;

    }
    #endregion

}