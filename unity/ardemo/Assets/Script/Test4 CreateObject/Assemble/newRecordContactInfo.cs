using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Xml.Linq;
//using System.Linq;
using System.Xml;
using System;
using System.IO;
using Game.Resource;
using Game.Platform;
using Game.Event;
using Game.Scene;

public class RecordContactInfo
{
    private string path;
    private static RecordContactInfo _instance;
    public RecordContactInfo() { openType = "default"; }
    static object sLock = new object();
    public static RecordContactInfo Instance
    {
        get
        {
            if (null == _instance)
            {
                lock (sLock)
                {
                    if (_instance == null)
                    {

                        _instance = new RecordContactInfo();
                    }
                }
            }

            return _instance;
        }
    }

    //生成物体的XML的名称
    private static XmlDocument xelell = new XmlDocument();
    private static XmlDocument xele=new XmlDocument();

    public string openType;//打开的robot数据属于哪种类型(自制，官方，下载)
    

    XmlNode xmlnode;
    XmlElement xmlelem;
    //public UILabel xmlNameLabel;

   FileInfo[] fis;
   string dir;
   string[] xmlN = new string[2];
   public string t = "tt";
   string goAngle;

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



    public void createPathInit(string filename,string rbtid,string datatype,string level)
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

                //给根节点添加一个子节点
                XmlElement xel = xele.CreateElement("RobotID");
                xel.InnerText = rbtid; //设置文本节点 机器人ID
                xmlelem.AppendChild(xel);

                XmlElement xell = xele.CreateElement("DataType");
                xell.InnerText = datatype; //设置文本节点 机器人ID
                xmlelem.AppendChild(xell);

                XmlElement xel2 = xele.CreateElement("Level");
                xel2.InnerText = level; //设置文本节点 机器人ID
                xmlelem.AppendChild(xel2);

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
            path = GetResourcesPath(robotname, openType);
            return path;
        }
    }

    public string GetResourcesPath(string xmlName,string opentype)
    {
        string pathfile = string.Empty;
        if (opentype.Equals("default"))
        {
            pathfile = ResourcesEx.GetCommonPathForNoTypeName(xmlName);
        }
        else if (opentype.Equals("playerdata"))
        {
            pathfile = ResourcesEx.GetRobotPathForNoTypeName(xmlName);
        }
        else
        {
            pathfile = ResourcesEx.persistentDataPath + "/download/" + xmlName;
        }

        
        
        if (!Directory.Exists(pathfile))
        {
            Directory.CreateDirectory(pathfile);
        }

        return pathfile + "/" + xmlName + ".xml";
    }

    public void CreateXElement(string nameTemp,string robotIDNum,string datatype,string level)
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
        xel.InnerText = robotIDNum; //设置文本节点 机器人ID
        xmlelem.AppendChild(xel);

        //给根节点添加一个子节点
        XmlElement xell = xele.CreateElement("DataType");
        xell.InnerText = datatype; //设置文本节点 机器人数据类型（玩家，默认，下载）
        xmlelem.AppendChild(xell);

        //给根节点添加一个子节点
        XmlElement xel2 = xele.CreateElement("Level");
        xel2.InnerText = level; //设置文本节点 机器人数据类型（玩家，默认，下载）
        xmlelem.AppendChild(xel2);
    }


    #region 查找手机上已有的机器人
    public UnityEngine.Object[] XmlSys()
    {
        UnityEngine.Object[] robotfiles = Resources.LoadAll("Prefab/Test4/DefaultRobot");
        return robotfiles;
    }

    public void pathInitSys(UnityEngine.Object temp)
    {
        TextAsset temptext = (TextAsset)temp;
        string t = temptext.ToString().Trim();
        // XmlDocument x = new XmlDocument();
        xele.LoadXml(t);
    }

    //自制--查询准备新建的XML是否与已有文档重名
    public List<string> XMLExist(string filetype)
    {
        List<string> existRobots = XMLExistNormal(filetype);
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

    
    /// <summary>
    /// 自制--查询准备新建的XML是否与已有文档重名,fileName是类型自制，默认，下载
    /// </summary>
    /// <param name="fileNameTemp"></param>
    /// <returns></returns>
    public List<string> XMLExistNormal(string fileNameTemp)
    {
        string pathfile = "";
        List<string> existXMLNams = new List<string>();
        if (fileNameTemp.Equals("playerdata"))
        {
            pathfile = ResourcesEx.GetUserRootPath() + "/" + fileNameTemp;
            if (!Directory.Exists(pathfile))
            {
                Directory.CreateDirectory(pathfile);
            }
            dir = pathfile;
        }
        else
        {
            pathfile = ResourcesEx.persistentDataPath + "/" + fileNameTemp;
            if (!Directory.Exists(pathfile))
            {
                Directory.CreateDirectory(pathfile);
            }
            dir = pathfile;
        }
        

        try
        {
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
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            Debug.Log("模型数据读取失败");
            RecordContactInfo.Instance.CloseRootABL();
            string notFind = LauguageTool.GetIns().GetText("模型数据读取失败");
            PromptMsg.ShowSinglePrompt(notFind);
        }
        return existXMLNams;
    }

    #endregion

    #region 删除
    /// <summary>
    /// 设置连接的物体的位置bool值为false
    /// </summary>
    /// <param name="pickGO"></param>
    /// <param name="preGOName"></param>
    /// <param name="preGOPos"></param>
    /// <param name="posGOName"></param>
    /// <param name="posGOPos"></param>
    public void DeleteGO(string pickGO, out string preGOName, out string preGOPos, out List<string> posGOName, out List<string> posGOPos)
    {
        List<string> posGOName1 = new List<string>();
        List<string> posGOPos1 = new List<string>();
        string preGOName1 = " ";
        string preGOPos1 = " ";

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
                        XmlNodeList childNodeList = xe.GetElementsByTagName("Pos");
                        if (childNodeList != null && childNodeList.Count > 0)
                        {
                            foreach (XmlNode x1n in childNodeList)
                            {
                                if (x1n != null)
                                {
                                    XmlElement x1e = (XmlElement)x1n;
                                    posGOName1.Add(x1e.GetAttribute("contactGO"));
                                    posGOPos1.Add(x1e.GetAttribute("contactPoint"));
                                }
                            }
                        }

                        XmlNodeList childNodeList1 = xe.GetElementsByTagName("PrePos");
                        if (childNodeList1 != null && childNodeList1.Count > 0)
                        {
                            foreach (XmlNode x2n in childNodeList1)
                            {
                                if (x2n != null)
                                {
                                    XmlElement x2e = (XmlElement)x2n;
                                    preGOName1 = x2e.GetAttribute("contactGO");
                                    preGOPos1 = x2e.GetAttribute("contactPoint");
                                }
                            }
                        }
                    }
                }

            }
        }

        posGOName = posGOName1;
        posGOPos = posGOPos1;
        preGOName = preGOName1;
        preGOPos = preGOPos1;
    }


    
    /// <summary>
    /// 删除XML文档
    /// </summary>
    /// <param name="xmlname"></param>
    /// <param name="datatype"></param>
    public void DeleteXmlFile(string xmlname,string datatype)
    {
        string pathfile = string.Empty;
        if (datatype == "default")
        {
            pathfile = PublicFunction.CombinePath(ResourcesEx.GetCommonPathForNoTypeName(xmlname), xmlname + ".xml");
        }
        else
        {
            pathfile = PublicFunction.CombinePath(ResourcesEx.GetRobotPathForNoTypeName(xmlname), xmlname + ".xml");
        }
        if (File.Exists(pathfile))
        {
            File.Delete(pathfile);
        }
    }

    /// <summary>
    /// 删除名字的后缀名
    /// </summary>
    /// <param name="xmlNam"></param>
    /// <returns></returns>
    public string DeleteNameExtension(string xmlNam)
    {
        xmlN = xmlNam.Split('.');
        return xmlN[0];
    }
    #endregion


    #region 添加
    /// <summary>
    /// 添加新生成的物体
    /// </summary>
    /// <param name="djid"></param>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <param name="pickGO"></param>
    /// <param name="isdp"></param>
    public void AddGO(string djid,string motorid,string id,string type,string pickGO,string isdp)
    {

        XmlNode root = xele.SelectSingleNode("Root");
        XmlElement x = xele.CreateElement("GO");
        
        x.SetAttribute("id", id);
        x.SetAttribute("type", type);
        x.SetAttribute("name", pickGO);
        x.SetAttribute("djid", djid);
        x.SetAttribute("motorID", motorid);

        if(type=="seivo"||type == "duoji")
        {
            x.SetAttribute("isDP", isdp);
        }

        //注释
       // XElement notes = new XElement("添加鼠标选中的物体");
        if (root != null)
        {
            root.AppendChild(x);
        }

        
    }

    public void Save(bool isNew)
    {
        xele.Save(Path);
        if (isNew)
        {
            PlatformMgr.Instance.OperateSyncFile(robotname, ResFileType.Type_playerdata, Path, OperateFileType.Operate_File_Add);
        }
        else
        {
            PlatformMgr.Instance.OperateSyncFile(robotname, ResFileType.Type_playerdata, Path, OperateFileType.Operate_File_Change);
        }
    }

    
    /// <summary>
    /// 给GO物体添加位置和角度属性
    /// </summary>
    /// <param name="pickGO"></param>
    /// <param name="goPos"></param>
    /// <param name="goAngle"></param>
    /// <param name="goScale"></param>
    /// <param name="startPos"></param>
    /// <param name="color"></param>
    /// <param name="dpshape"></param>
    public void AddGOPosAngle(string pickGO, string goPos, string goAngle,string goScale,string startPos,string color,string dpshape)
    {
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        if (nodeList != null && nodeList.Count>0)
       {
            foreach (XmlNode xn in nodeList)
            {
                if(xn!=null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.GetAttribute("name")==pickGO)
                    {
                        xe.SetAttribute("goPos", goPos);
                        xe.SetAttribute("goAngle", goAngle);
                        xe.SetAttribute("goScale", goScale);
                        xe.SetAttribute("startPos", startPos);
                        xe.SetAttribute("goColor", color);
                        xe.SetAttribute("dpShape", dpshape);
                    }
                }
               
            }
       }
    }
   

    /// <summary>
    ///添加物体GO的Pos即鼠标选中物体的位置
    ///pickGO：选中物体的名字；posID：是Pos还是PrePos；pos:鼠标选中物体的位置；
    /// </summary>
    /// <param name="pickGO"></param>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    public void AddPos(string pickGO,string id,string pos)
    {
        XmlElement x = xele.CreateElement(id);
        x.InnerText = pos;
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
                        xe.AppendChild(x);
                    }
                }

            }
        }

    }

    /// <summary>
    ///添加与鼠标选中物体连接的物体的名称和位置
    ///pickGO：选中物体的名字；posID：是Pos还是PrePos；pos:鼠标选中物体的位置；id：该位置相对其他位置生成物体的顺序
    ///contactGO:对应pos处连接的物体的名字；contactPoint：对应pos处连接物体的位置
    /// </summary>
    /// <param name="pickGO"></param>
    /// <param name="posID"></param>
    /// <param name="pos"></param>
    /// <param name="contactGO"></param>
    /// <param name="contactPoint"></param>
    public void AddContactInf(string pickGO,string posID,string pos, string contactGO, string contactPoint)
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
                        XmlNodeList childNodeList = xe.GetElementsByTagName(posID); 
                        if (childNodeList != null && childNodeList.Count > 0)
                        {
                            foreach (XmlNode xchild in childNodeList)
                            {
                                if(xchild!=null)
                                {
                                    XmlElement xechild = (XmlElement)xchild;
                                    if (xechild.InnerText == pos)         //找到连接的位置Pos 或者PrePos
                                    {
                                        xechild.SetAttribute("contactGO", contactGO);
                                        xechild.SetAttribute("contactPoint", contactPoint);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

    }
    #endregion

    #region 查找
    /// <summary>
    /// 查找联动物体信息
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, DJLianDongs> FindLDGOs(string robotNameTemp)
    {
        robotname = RobotMgr.NameNoType(robotNameTemp);

        xele.Load(Path);
        Dictionary<string, DJLianDongs> ldDJ = null;
        XmlNodeList nodeList = xele.GetElementsByTagName("rotateGOs");
        if (nodeList != null && nodeList.Count > 0)
        {
            ldDJ = new Dictionary<string, DJLianDongs>();
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    string djName = xe.GetAttribute("name");
                    DJLianDongs djdata = new DJLianDongs();
                    XmlNodeList childNodeList = xe.GetElementsByTagName("tGOs");
                    if (childNodeList != null && childNodeList.Count > 0)
                    {
                        foreach (XmlNode xnT in childNodeList)
                        {
                            XmlElement xeT = (XmlElement)xnT;
                            string ldDJName = xeT.GetAttribute("name");

                            if (djdata.djlds.ContainsKey(ldDJName) == false)
                            {
                                DJLianDong tDJ = new DJLianDong();
                                tDJ.djid = RobotMgr.Instance.rbt[robotNameTemp].gos[ldDJName].djID;

                                string difAT = xeT.GetAttribute("difA");
                                if (difAT != null)
                                {
                                    tDJ.difA = MathTool.StrToFloat(difAT);
                                }
                                else
                                {
                                    tDJ.difA = 0;
                                }

                                tDJ.symbol = xeT.GetAttribute("symbol");

                                string difBT = xeT.GetAttribute("difB");
                                if (difBT != null)
                                {
                                    tDJ.difB = MathTool.StrToFloat(difBT);
                                }
                                else
                                {
                                    tDJ.difB = 0;
                                }

                                djdata.djlds.Add(ldDJName, tDJ);
                            }
                        }
                    }

                    if (ldDJ.ContainsKey(djName) == false)
                    {
                        ldDJ.Add(djName, djdata);
                    }
                }
            }
        }
        return ldDJ;
    }

    /// <summary>
    /// 查找联动舵机信息
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, DJLianDongs> FindLDDJ(string robotNameTemp)
    {
        robotname = RobotMgr.NameNoType(robotNameTemp);

        xele.Load(Path);
        Dictionary<string, DJLianDongs> ldDJ = null;
        XmlNodeList nodeList = xele.GetElementsByTagName("rotateT");
        if (nodeList != null && nodeList.Count > 0)
        {
            ldDJ = new Dictionary<string, DJLianDongs>();
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    string djName = xe.GetAttribute("name");
                    DJLianDongs djdata = new DJLianDongs();
                    XmlNodeList childNodeList = xe.GetElementsByTagName("tGO");
                    if (childNodeList != null && childNodeList.Count > 0)
                    {
                        foreach (XmlNode xnT in childNodeList)
                        {
                            XmlElement xeT = (XmlElement)xnT;
                            string ldDJName = xeT.GetAttribute("name");

                            if(djdata.djlds.ContainsKey(ldDJName)==false)
                            {
                                DJLianDong tDJ = new DJLianDong();
                                tDJ.djid = RobotMgr.Instance.rbt[robotNameTemp].gos[ldDJName].djID;
                                
                                string difAT = xeT.GetAttribute("difA");
                                if (difAT != null)
                                {
                                    tDJ.difA = MathTool.StrToFloat(difAT);
                                }
                                else
                                {
                                    tDJ.difA = 0;
                                }

                                string symbol = xeT.GetAttribute("symbol");
                                if (symbol != null)
                                {
                                    tDJ.symbol = symbol;
                                }
                                else
                                {
                                    tDJ.symbol = "+";
                                }

                                string difBT = xeT.GetAttribute("difB");
                                if (difBT != null)
                                {
                                    tDJ.difB = MathTool.StrToFloat(difBT);
                                }
                                else
                                {
                                    tDJ.difB = 0;
                                }

                                djdata.djlds.Add(ldDJName,tDJ);
                            }
                        }
                    }

                    if (ldDJ.ContainsKey(djName) == false)
                    {
                        ldDJ.Add(djName,djdata);
                    }
                }
            }
        }
        return ldDJ;
    }


    /// <summary>
    /// 查找碰撞物体
    /// </summary>
    public Dictionary<string,string> FindColliderGO()
    {
        XmlNodeList nodeList = xele.GetElementsByTagName("ColliderGO");
        Dictionary<string,string> cGOs=new Dictionary<string,string>();

        if(nodeList!=null)
        {
            XmlElement xe = (XmlElement)nodeList[0];
            XmlNodeList nodeListl = xe.GetElementsByTagName("child");
                foreach (XmlNode xn in nodeListl)
                {
                    if (xn != null)
                    {
                        XmlElement xell = (XmlElement)xn;
                        string namT=xell.GetAttribute("name");
                        string scriptT=xell.GetAttribute("script");
                        if(cGOs.ContainsKey(namT)==false)
                        {
                            cGOs.Add(namT,scriptT);
                        }
                    }

                }
            }

        return cGOs;
    }

    /// <summary>
    /// 课程模式下完成提示信息
    /// </summary>
    public string FindLessonfsNotice()
    {
        XmlNodeList nodeList = xele.GetElementsByTagName("lessonfsNotice");
        string noticeT = null;
        if (nodeList != null)
        {
            XmlElement xe = (XmlElement)nodeList[0];
            if(xe!=null)
            {
                noticeT = xe.InnerText;
            }
            
        }

        return noticeT;
    }

    /// <summary>
    /// 查找预设动画名称
    /// </summary>
    public string FindAds()
    {
        XmlNodeList nodeList = xele.GetElementsByTagName("Ads");
        string ads =null;

        if (nodeList != null)
        {
            XmlElement xe = (XmlElement)nodeList[0];
     
            if(xe!=null&&xe.InnerText !=null)
            {
                ads = xe.InnerText;
            }
                       
        }

        return ads;
    }

    /// <summary>
    /// 查找是否有某节点
    /// </summary>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    public bool HasNode(string nodeName)
    {
        XmlNodeList nodeList = xele.GetElementsByTagName(nodeName);
        bool has = false;
        if (nodeList != null && nodeList.Count > 0)
        {
            has = true;
        }
        return has;
    }

    /// <summary>
    /// 查找节点的值
    /// </summary>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    public float FindOriValue(string nodeName)
    {
        XmlNodeList nodeList = xele.GetElementsByTagName(nodeName);
        float stepNum = 0;
        if (nodeList != null && nodeList.Count > 0)
        {
            XmlElement xe = (XmlElement)nodeList[0];
            stepNum = float.Parse(xe.InnerText);
        }
        return stepNum;
    }


    /// <summary>
    /// 给ChildGO添加元素
    /// </summary>
    /// <param name="pickGO"></param>
    /// <param name="childGO"></param>
    public void FindPosValues(string pickGO, List<string> childGO)
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
                        XmlNodeList childNodeList = xe.GetElementsByTagName("Pos");
                        if (childNodeList != null && childNodeList.Count > 0)
                        {
                            foreach (XmlNode xchild in childNodeList)
                            {
                                if (xchild != null)
                                {
                                    XmlElement xechild = (XmlElement)xchild;

                                    if (childGO.Count == 0)
                                    {
                                        childGO.Add(xechild.InnerText);
                                    }
                                    else
                                    {
                                        int num = 0;
                                        foreach (string ct in childGO)
                                        {
                                            if (xechild.InnerText != ct)
                                            {
                                                num++;
                                            }
                                        }
                                        if (num != childGO.Count - 1)
                                        {
                                            childGO.Add(xechild.InnerText);
                                        }
                                    }
                                }


                            }
                            foreach (XmlNode xchild in childNodeList)
                            {

                                FindPosValues(xchild.InnerText, childGO);
                            }
                        }
                    }
                }

            }

        }
    }
  

    /// <summary>
    /// 查询鼠标选中物体Pos连接的物体
    /// </summary>
    /// <param name="pickGO"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public string[] FindPosContactInf(string pickGO, string pos)
    {
        string[] x = new string[2];     //x[0]=contactgo;x[1]=contactpoint
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
                        XmlNodeList childNodeList = xe.GetElementsByTagName("Pos");
                        if (childNodeList != null && childNodeList.Count > 0)
                        {
                            foreach (XmlNode xchild in childNodeList)
                            {
                                if (xchild != null)
                                {
                                    XmlElement xechild = (XmlElement)xchild;
                                    if (xechild.InnerText == pos)         //找到连接的位置Pos 或者PrePos
                                    {
                                        x[0] = xechild.GetAttribute("contactGO");
                                        x[1] = xechild.GetAttribute("contactPoint");
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }
        return x;

    }

    /// <summary>
    /// 查找模型在编程界面的坐标和角度
    /// </summary>
    /// <param name="nameTemp"></param>
    /// <returns></returns>
    public string[] FindPosAction(string nameTemp)
    {
        robotname = nameTemp;
        xele.Load(Path); 
        string[] x = new string[2];     //x[0]=contactgo;x[1]=contactpoint
        XmlNodeList nodeList = xele.GetElementsByTagName("Action");
        if (nodeList != null && nodeList.Count > 0)
        {
            XmlElement xe = (XmlElement)nodeList[0];
           
            x[0] = xe.GetAttribute("Pos");
            x[1] = xe.GetAttribute("Angle");
           
        }
        else
        {
            x[0] = "(0,0,0)";
            x[1] = "(0,0,0)";
        }
        return x;
    }

    /// <summary>
    /// 查找主场景中模型的位置，角度，缩放
    /// </summary>
    /// <param name="nameTemp"></param>
    /// <returns></returns>
    public string[] FindPosModel(string nameTemp)
    {
        robotname = nameTemp;
        
        xele.Load(Path); 
        string[] x = new string[3];     //x[0]=contactgo;x[1]=contactpoint
        XmlNodeList nodeList = xele.GetElementsByTagName("Model");
        if (nodeList != null && nodeList.Count > 0)
        {
            XmlElement xe = (XmlElement)nodeList[0];

            if (xe.HasAttribute("Pos"))
            {
                 x[0] = xe.GetAttribute("Pos");
            }
            else
            {
                x[0] = "(0,0,0)";
            }

            if (xe.HasAttribute("Angle"))
            {
                x[1] = xe.GetAttribute("Angle");
            }
            else
            {
                x[1] = "(0,0,0)";
            }

            if (xe.HasAttribute("Scale"))
            {
                x[2] = xe.GetAttribute("Scale");
            }
            else
            {
                x[2] = "(1,1,1)";
            }

        }
        else
        {
            x[0] = "(0,0,0)";
            x[1] = "(0,0,0)";
            x[2] = "(1,1,1)";
        }
        return x;
    }


    
    /// <summary>
    /// 查找线的名称，及线子物体joint的名称后缀 <线的名称，joint后缀>
    /// </summary>
    /// <param name="nameTemp"></param>
    /// <returns></returns>
    public Dictionary<string,string> FindWiresData(string nameTemp)
    {
        robotname = nameTemp;
        Dictionary<string, string> wireJionts = new Dictionary<string, string>();     //<线的名称，joint后缀>
        if (!File.Exists(Path))
        {
            return wireJionts;
        }
        xele.Load(Path);
        XmlNodeList nodeList = xele.GetElementsByTagName("Wires");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;

                    string nameT = xe.GetAttribute("name");
                    string numT = xe.GetAttribute("num");

                    if (wireJionts.ContainsKey(nameT) == false)
                    {
                        wireJionts.Add(nameT, numT);
                    }
                }
            }
        }

        return wireJionts;
    }

    
	/// <summary>
    /// 查找动画场景中模型的位置，角度，缩放
	/// </summary>
	/// <param name="nameTemp"></param>
	/// <returns></returns>
    public string[] FindPosAnim(string nameTemp)
    {
        robotname = nameTemp;

        xele.Load(Path);
        string[] x = new string[3];     //x[0]=contactgo;x[1]=contactpoint
        XmlNodeList nodeList = xele.GetElementsByTagName("Anim");
        if (nodeList != null && nodeList.Count > 0)
        {
            XmlElement xe = (XmlElement)nodeList[0];
            if (xe.HasAttribute("Pos"))
            {
                x[0] = xe.GetAttribute("Pos");
            }
            else
            {
                x[0] = "(0,0,0)";
            }

            if (xe.HasAttribute("Angle"))
            {
                x[1] = xe.GetAttribute("Angle");
            }
            else
            {
                x[1] = "(0,0,0)";
            }
            if (xe.HasAttribute("Scale"))
            {
                x[2] = xe.GetAttribute("Scale");
            }
            else
            {
                x[2] = "(1,1,1)";
            }
        }
        else
        {
            x[0] = "(0,0,0)";
            x[1] = "(0,0,0)";
            x[2] = "(1,1,1)";
        }
        return x;
    }

    /// <summary>
    /// 查找模型的id
    /// </summary>
    /// <returns></returns>
    public string FindRobotID2nd()
    {
        string robotID="";

        XmlNodeList nodeList = xele.GetElementsByTagName("RobotID");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement x1n = (XmlElement)xn;
                    robotID = x1n.InnerText;
                }

            }
        }
        return robotID;
    }

    
    /// <summary>
    /// 找到机器人的数据类型
    /// </summary>
    /// <returns></returns>
    public string FindRobotDataType()
    {
        string dataType="";
        XmlNodeList nodeList = xele.GetElementsByTagName("DataType");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement x1n = (XmlElement)xn;
                    dataType = x1n.InnerText;
                }
            }
        }
        return dataType;
    }

    
    /// <summary>
    /// 找到机器人的等级
    /// </summary>
    /// <returns></returns>
    public string FindRobotLevel()
    {
        string level = "own";
        XmlNodeList nodeList = xele.GetElementsByTagName("Level");
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement x1n = (XmlElement)xn;
                    level = x1n.InnerText;
                }
            }
        }
        return level;
    }


    /// <summary>
    /// 查找duoji的名称和角度
    /// </summary>
    /// <param name="nameTemp"></param>
    /// <returns></returns>
    public Dictionary<string, int> FindDJData(string nameTemp)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        string namewithtype = RobotMgr.NameWithType(nameTemp, RecordContactInfo.Instance.openType);
        foreach (string child in RobotMgr.Instance.rbt[namewithtype].gos.Keys)
        {
            GO gotemp = RobotMgr.Instance.rbt[namewithtype].gos[child];
            if (child != null && gotemp != null && (gotemp.goType == "seivo" || gotemp.goType == "duoji") && gotemp.posAngle.angle != null)
            {
                xangles = RobotMgr.Instance.rbt[namewithtype].gos[child].posAngle.angle.Split(' ', '"', '(', ',', ',', ',', ')', '"');
                float rotax = float.Parse(xangles[1]);
                float rotay = float.Parse(xangles[2]);
                float rotaz = float.Parse(xangles[3]);
                if (rotax < 0)
                {
                    rotax += 360;
                }
                if (rotay < 0)
                {
                    rotay += 360;
                }
                if (rotaz < 0)
                {
                    rotaz += 360;
                }
                dict[child] = PublicFunction.Rounding(ConvertAngle.ConvertToRightAngle(new Vector3(rotax, rotay, rotaz)).x);
            }

        }
        return dict;
    }


    
    /// <summary>
    /// 查找物体的坐标和角度
    /// </summary>
    /// <param name="pickGO"></param>
    /// <returns></returns>
    public string[] FindGOPosAngle(string pickGO)
    {
        string[] x = new string[6];     //x[0]=goPos;x[1]=goAngle
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
                        if (xe.HasAttribute("goPos") && xe.GetAttribute("goPos") != null)
                        {
                            x[0] = xe.GetAttribute("goPos");
                        }
                        else
                        {
                            x[0] = "(0,0,0)";
                        }
                        if (xe.HasAttribute("goAngle") && xe.GetAttribute("goAngle") != null)
                        {
                            x[1] = xe.GetAttribute("goAngle");
                        }
                        else
                        {
                            x[1] = "(0,0,0)";
                        }
                        if (xe.HasAttribute("goScale") && xe.GetAttribute("goScale") != null)
                        {
                            x[2] = xe.GetAttribute("goScale");
                        }
                        else
                        {
                            x[2] = "(0,0,0)";
                        }
                        if (xe.HasAttribute("startPos") && xe.GetAttribute("startPos") != null)
                        {
                            x[3] = xe.GetAttribute("startPos");
                        }
                        else
                        {
                            x[3] = "(0,0,0)";
                        }
                        if (xe.HasAttribute("goColor") && xe.GetAttribute("goColor") != null)
                        {
                           x[4] = xe.GetAttribute("goColor");
                        }
                        else
                        {
                            x[4] ="(1,1,1,1)";
                        }
                        //Debug.Log("goColorRecord:"+x[4]);
                        if (xe.HasAttribute("dpShape") && xe.GetAttribute("dpShape") != null)
                        {
                            x[5] = xe.GetAttribute("dpShape");
                        }
                        else
                        {
                            x[5] = "Circle";
                        }
                        
                    }
                }

            }
        }
        return x;

    }


    string prePosValue;
    /// <summary>
    /// 查找物体的PrePos
    /// </summary>
    /// <param name="pickGO"></param>
    /// <returns></returns>
    public string FindPrePosValue(string pickGO)
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
                    if (xe.GetAttribute("name") == pickGO)    //找到GO
                    {
                        XmlNodeList childNodeList = xe.GetElementsByTagName("PrePos");
                        if (childNodeList != null && childNodeList.Count > 0)
                        {
                            foreach (XmlNode xchild in childNodeList)
                            {
                                if (xchild != null)
                                {
                                    XmlElement x1child = (XmlElement)xchild;
                                    prePosValue = x1child.InnerText;
                                }
                            }
                        }
                    }
                }

            }
        }
        return prePosValue;

    }

    
    /// <summary>
    /// 查询鼠标选中对应位置PrePos的物体信息。
    /// </summary>
    /// <param name="pickGO"></param>
    /// <returns></returns>
    public string[] FindPrePosContactInf(string pickGO)
    {
        string[] x = new string[2];
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
                            foreach (XmlNode xchild in childNodeList)
                            {
                                if (xchild != null)
                                {
                                    XmlElement x1child = (XmlElement)xchild;
                                    x[0] = x1child.GetAttribute("contactGO");
                                    x[1] = x1child.GetAttribute("contactPoint");
                                }
                            }
                        }
                    }
                }

            }
        }
        return x;

    }


    
    /// <summary>
    /// 查找XML文档中所有的物体的名字
    /// </summary>
    /// <param name="namewithtype"></param>
    /// <param name="filename"></param>
    public void FindAllGOData(string namewithtype,string filename)
    {
        try
        {
            robotname = filename;
            xele.Load(Path);

            string robotid = "";
            string datatype = "";
            string level = "";
            XmlNodeList nodeListTemp = xele.GetElementsByTagName("RobotID");
            if (nodeListTemp != null && nodeListTemp.Count > 0)
            {
                XmlElement x1n = (XmlElement)nodeListTemp[0];
                robotid = x1n.InnerText;
            }

            nodeListTemp = xele.GetElementsByTagName("DataType");
            if (nodeListTemp != null && nodeListTemp.Count > 0)
            {
                XmlElement x1n = (XmlElement)nodeListTemp[0];
                datatype = x1n.InnerText;
            }

            nodeListTemp = xele.GetElementsByTagName("Level");
            if (nodeListTemp != null && nodeListTemp.Count > 0)
            {
                XmlElement x1n = (XmlElement)nodeListTemp[0];
                level = x1n.InnerText;
            }


            if (RobotMgr.Instance.rbt.ContainsKey(namewithtype) == false)
            {

                RobotData robot1 = new RobotData();
                robot1.robotName = namewithtype;
                robot1.id = robotid;
                robot1.dataType = datatype;
                robot1.level = level;
                RobotMgr.Instance.rbt.Add(namewithtype, robot1);


                XmlNodeList nodeList = xele.GetElementsByTagName("GO");
                if (nodeList != null && nodeList.Count > 0)
                {
                    foreach (XmlNode xn in nodeList)
                    {
                        Dictionary<string, string> dataDetail = new Dictionary<string, string>();
                        if (xn != null)
                        {
                            XmlElement xe = (XmlElement)xn;
                            string nameTemp = xe.GetAttribute("name");

                            if (RobotMgr.Instance.rbt[namewithtype].gos.ContainsKey(nameTemp) == false)
                            {
                                GO go = new GO();
                                go.goID = xe.GetAttribute("id");
                                go.goType = xe.GetAttribute("type");

                                if (go.goType == "seivo" || go.goType == "duoji")
                                {
                                    string djIDTemp = xe.GetAttribute("djid");
                                    //Debug.Log("djIDTemp:" + djIDTemp + ":nameTemp:" + nameTemp);
                                    go.djID = int.Parse(djIDTemp);

                                    if (RecordContactInfo.Instance.openType == "default")
                                    {
                                        string isdptemp = xe.GetAttribute("isDP");
                                        go.isDP = int.Parse(isdptemp);
                                    }
                                }
                                if (go.goType == "motor" && xe.HasAttribute("motorID"))
                                {
                                    go.motorID = int.Parse(xe.GetAttribute("motorID"));
                                }

                                go.posAngle.pos = xe.GetAttribute("goPos");
                                go.posAngle.angle = xe.GetAttribute("goAngle");
                                go.posAngle.scale = xe.GetAttribute("goScale");
                                go.posAngle.startpos = xe.GetAttribute("startPos");
                                if (xe.HasAttribute("goColor"))
                                {
                                    go.color = xe.GetAttribute("goColor");
                                }
                                else
                                {
                                    go.color = "";
                                }
                                if (xe.HasAttribute("shape"))
                                {
                                    go.shape = xe.GetAttribute("shape");
                                }
                                else
                                {
                                    go.shape = "Circle";
                                }
                                if (xe.HasAttribute("hidego"))
                                {
                                    go.hidego = xe.GetAttribute("hidego");
                                }
                                else
                                {
                                    go.hidego = "false";
                                }

                                if (xe.HasAttribute("oriDJAngleX"))
                                {
                                    string angleXT = xe.GetAttribute("oriDJAngleX");
                                    float angleX = float.Parse(angleXT);
                                    go.oriDJAngleX = angleX;
                                }
                                else
                                {
                                    go.oriDJAngleX = 120.0f;
                                }

                                if (xe.HasAttribute("sensorID"))
                                {
                                    go.sensorID = xe.GetAttribute("sensorID");
                                }
                                else
                                {
                                    go.sensorID = null;
                                }

                                if (xe.HasAttribute("motorID"))
                                {
                                    go.motorID = int.Parse(xe.GetAttribute("motorID"));
                                }
                                

                                XmlNodeList childNodeList = xe.GetElementsByTagName("PrePos");
                                if (childNodeList != null && childNodeList.Count > 0)
                                {

                                    XmlElement x1child = (XmlElement)childNodeList[0];
                                    go.ppgp.contactgo = x1child.GetAttribute("contactGO");
                                    go.ppgp.contactpoint = x1child.GetAttribute("contactPoint");

                                }

                                RobotMgr.Instance.rbt[namewithtype].gos.Add(nameTemp, go);
                            }
                        }

                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            CloseRootABL();
            string notFind = LauguageTool.GetIns().GetText("模型数据读取失败");
            PromptMsg.ShowSinglePrompt(notFind);
        }
        
       
       
    }
   

    /// <summary>
    /// 查找XML文档中所有的物体的名字
    /// </summary>
    /// <param name="goName"></param>
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

    
    /// <summary>
    /// 查询鼠标选中物体的类型
    /// </summary>
    /// <param name="pickGO"></param>
    /// <returns></returns>
    public string FindPickGOType(string pickGO)
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
                    if (xe.GetAttribute("name") == pickGO)    //找到GO
                    {
                        if (xe.HasAttribute("type") && xe.GetAttribute("type") != null)
                        {
                            tempType = xe.GetAttribute("type");
                        }
                        else
                        {
                            tempType = "own";
                        }
                    }
                }

            }
        }
        return tempType;
    }


    /// <summary>
    /// 查找isDP  是否是舵机的舵盘旋转--1为是；0为否
    /// </summary>
    /// <param name="pickGO"></param>
    /// <returns></returns>
    public string FindisDP(string pickGO)
    {
        //XElement xele = XElement.Load(Path);
        string isdp="0";
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
                        if (xe.HasAttribute("isDP") && xe.GetAttribute("isDP") != null)
                        {
                            isdp = xe.GetAttribute("isDP");
                        }
                       
                        //Debug.Log("temptype:"+isdp);
                    }
                }

            }
        }
        return isdp;
    }

    #endregion

    #region 判断是否
    //修改机器人的名字
    public bool HasFile(string opentype, string filename)
    {
        bool hasFile=false;
        string path = string.Empty;
        if (opentype == "default")
        {
            path = PublicFunction.CombinePath(ResourcesEx.GetCommonPathForNoTypeName(filename), filename + ".xml");
        }
        else
        {
            path = PublicFunction.CombinePath(ResourcesEx.GetRobotPathForNoTypeName(filename), filename + ".xml");
        }

        if (System.IO.File.Exists(path))
        {
            hasFile = true;
        }
        return hasFile;
    }


    #endregion


    #region  修改
    //记录进度条轴的x坐标
    /// <summary>
    /// 记录退出搭建是，指针的坐标
    /// </summary>
    /// <param name="thumbx"></param>
    public void RecordThumbX(float thumbx)
    {
        XmlNodeList nodeList = xele.GetElementsByTagName("ThumbX");
        if (nodeList != null && nodeList.Count > 0)
        {
            XmlElement xe = (XmlElement)nodeList[0];
            xe.InnerText = thumbx.ToString();
        }
        else
        {
            XmlNode root = xele.SelectSingleNode("Root");
            XmlElement x = xele.CreateElement("ThumbX");

            x.InnerText = thumbx.ToString();
            //注释
            // XElement notes = new XElement("添加鼠标选中的物体");
            if (root != null)
            {
                root.AppendChild(x);
            }
        }
        xele.Save(Path);
    }

    /// <summary>
    /// 记录退出时的步数
    /// </summary>
    /// <param name="stepTemp"></param>
    public void RecordStepNum(float stepTemp)
    {
        XmlNodeList nodeList = xele.GetElementsByTagName("Step");
        if (nodeList != null && nodeList.Count > 0)
        {
            XmlElement xe = (XmlElement)nodeList[0];
            xe.InnerText=stepTemp.ToString();
        }
        else
        {
            XmlNode root = xele.SelectSingleNode("Root");
            XmlElement x = xele.CreateElement("Step");

            x.InnerText = stepTemp.ToString();
            //注释
            // XElement notes = new XElement("添加鼠标选中的物体");
            if (root != null)
            {
                root.AppendChild(x);
            }
        }
        xele.Save(Path);
    }

    /// <summary>
    /// 修改机器人的名字
    /// </summary>
    /// <param name="opentype"></param>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    public void ReviseFileNam(string opentype,string oldName,string newName)
    {
        string path = string.Empty;
        if (opentype == "default")
        {
            path = ResourcesEx.GetCommonRootPath();
        }
        else
        {
            path = ResourcesEx.GetUserRootPath();
        }
        string srcFileName = path+"/"+oldName+"/"+oldName+".xml";
		string destFileName = path+"/"+oldName+"/"+newName+".xml";

        string srcFolderPath = path + "/" + oldName;
        string destFolderPath = path + "/" + newName;
        if (System.IO.File.Exists(srcFileName))
        {
            System.IO.File.Move(srcFileName, destFileName);
        }
        if (System.IO.Directory.Exists(srcFolderPath))
        {
            System.IO.Directory.Move(srcFolderPath, destFolderPath);
        }
            
    }

    #endregion

   //当无读取权限时，关闭RootABL
    public void CloseRootABL() {
        if (GameObject.Find("UIRootABL") != null)
        {
            GameObject returnBtn = GameObject.Find("UIRootABL/Camera/Left/Return") as GameObject;
            UIEventListener.Get(returnBtn).onClick += ReturnScene;
            GameObject.Find("UIRootABL/Camera/Right").SetActive(false);
            GameObject.Find("UIRootABL/Camera/Bottom").SetActive(false);
            GameObject.Find("UIRootABL/Camera/Center").SetActive(false);
        }
    }

    void ReturnScene(GameObject go)
    {
        if (PlatformMgr.Instance.FromType == FromPlatform.Alpha_mini) //返回到mini App
        {
            PlatformMgr.Instance.BackToFromApp();
        }
        else
        {
            if (ClientMain.Use_Third_App_Flag)
            {

                if (JMSimulatorOnly.IsLessonBuild)
                {
                    PlatformMgr.Instance.BackCourseMenu(false);
                }
                else
                {
                    PlatformMgr.Instance.BackThirdApp();
                }
            }
            else
            {
                SceneMgr.EnterScene(SceneType.MenuScene);
            }
        }
    }

}