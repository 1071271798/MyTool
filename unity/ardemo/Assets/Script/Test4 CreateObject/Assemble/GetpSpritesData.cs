
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;
using Game.Resource;

public class GetpSpritesData
{
    private static GetpSpritesData _instance;
    public static GetpSpritesData Ins
    {
        get { _instance = new GetpSpritesData(); return _instance; }
    }

    public GetpSpritesData()
    {
        xele.Load(Path); 
    }

    public void Dispose()
    {
        _instance = null;
    }

    public string robotid;
    string path;
    public string Path
    {
        get
        {
            path = GetResourcesPath("default");
            return path;
        }
        set
        {
            path= value;
        }
    }

    public TextAsset GetXmlByPath()
    {
        //path = "Script/Test4/partsData/partsData";
        path = PublicFunction.CombinePath(ResourcesEx.GetCommonRootPath(), "partsData");
      
        TextAsset tmp = Resources.Load(path) as TextAsset;

        return tmp;
    }

    public string GetResourcesPath(string opentype)
    {
        string nameTemp = RobotMgr.Instance.rbtnametempt;
        string nameNoType = RobotMgr.NameNoType(nameTemp);
        string pathfile = PublicFunction.CombinePath(ResourcesEx.GetRobotCommonPath(nameTemp), "partsData/partsData.xml");
        return pathfile;
        
    }

    private static XmlDocument xele = new XmlDocument();
   XmlNode xmlnode;
   XmlElement xmlelem;
    public void creatPath( )
    {
        xele.Load(Path);
        if (xele != null)
        {
            xele.RemoveAll();
            //加入一个根元素
            xmlelem = xele.CreateElement("Root");
            xele.AppendChild(xmlelem);//添加到文本中 

            xele.Save(Path);
        }
    }

    public void createElement(string id, string name, string pos)
    {

        XmlNode root = xele.SelectSingleNode("Root");
        XmlElement x = xele.CreateElement("GO");

        x.SetAttribute("id", id);
        x.SetAttribute("name", name);
        x.SetAttribute("pos", pos);
        x.SetAttribute("count", "2");
        x.SetAttribute("color", "white");

        root.AppendChild(x);

        xele.Save(Path);
    }

    
    /// <summary>
    /// 查找所用panel的名称
    /// </summary>
    /// <returns></returns>
    public List<string> FindPanels()
    {
        
        List<string> panelNames = new List<string>();
        XmlNodeList nodeList = xele.GetElementsByTagName("Panel");
        //Debug.Log("nodeListCount:" + nodeList.Count);
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                   if(xe.HasAttribute("name"))
                   {
                       string nameT=xe.GetAttribute("name");
                       if (panelNames.Contains(nameT)==false)
                       {
                           panelNames.Add(nameT);
                       }
                       
                   }
                }

            }
        }
        return panelNames;
    }

    
    /// <summary>
    /// 查找所有图片名称及数量(图片名称，数量)  如：("P34-GRY",30)
    /// </summary>
    /// <returns></returns>
    public Dictionary<string,string> FindAllPicNamesT()
    {

        Dictionary<string, string> picNames = new Dictionary<string, string>();
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        //Debug.Log("nodeListCount:" + nodeList.Count);
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.HasAttribute("name"))
                    {
                        string nameT = xe.GetAttribute("name");
                        if (picNames.ContainsKey(nameT) == false && nameT.Contains("label") == false && nameT != "" && nameT != "Clean")
                        {
                            string countT = xe.GetAttribute("count");
                            picNames.Add(nameT,countT);
                        }

                    }
                }

            }
        }
        return picNames;
    }

    
    /// <summary>
    /// 查找所有图片名称
    /// </summary>
    /// <returns></returns>
    public List<string> FindAllPicNames()
    {

        List<string> picNames = new List<string>();
        XmlNodeList nodeList = xele.GetElementsByTagName("GO");
        //Debug.Log("nodeListCount:" + nodeList.Count);
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.HasAttribute("name"))
                    {
                        string nameT = xe.GetAttribute("name");
                        if (picNames.Contains(nameT) == false && nameT.Contains("label") == false && nameT!= ""&&nameT!="Clean")
                        {
                            picNames.Add(nameT);

                            //Debug.Log("goname:"+nameT);
                        }

                    }
                }

            }
        }
        return picNames;
    }

    
    /// <summary>
    /// 查找每个panel中含有的gosprite
    /// </summary>
    /// <returns></returns>
    public Dictionary<string,SpriteGO> FindSpriteGO()
    {

        Dictionary<string, SpriteGO> panelDetails = new Dictionary<string,SpriteGO>();
        XmlNodeList nodeList = xele.GetElementsByTagName("Panel");
        //Debug.Log("nodeListCount:" + nodeList.Count);
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.HasAttribute("name"))
                    {
                        string nameT = xe.GetAttribute("name");
                        if (panelDetails.ContainsKey(nameT) == false)
                        {

                            SpriteGO spriteGOT = new SpriteGO();
                            int idTemp=0;

                            XmlNodeList childNodeList = xe.GetElementsByTagName("GO");
                            if (childNodeList != null && childNodeList.Count > 0)
                            {
                                foreach (XmlNode xchild in childNodeList)
                                {
                                    if (xchild != null)
                                    {
                                        XmlElement xechild = (XmlElement)xchild;
                                        
                                        if (xechild.HasAttribute("name"))         //找到连接的位置Pos 或者PrePos
                                        {
                                            if (spriteGOT.sptD.ContainsKey(idTemp) == false)
                                            {

                                                SpriteDetails spriteD = new  SpriteDetails();


                                                spriteD.id = idTemp;
                                                string nameT2 = xechild.GetAttribute("name");
                                                if (nameT2 != null && nameT2!="")
                                                {
                                                    //Debug.Log("nameT2:" + nameT2);
                                                    spriteD.name = nameT2;
                                                }
                                                else
                                                {
                                                    spriteD.name = "Clean";   //透明背景的贴图
                                                }

                                                //string idT = xechild.GetAttribute("id");
                                                //spriteD.id = int.Parse(idT);

                                                string posT = xechild.GetAttribute("pos");
                                                spriteD.pos = SingletonObject<PublicTools>.GetInst().StringToVector(posT);

                                                int countT = int.Parse(xechild.GetAttribute("count"));
                                                spriteD.count = countT;

                                                spriteGOT.sptD.Add(idTemp, spriteD);

                                                idTemp++;
                                            }
                                        }
                                        
                                    }
                                }
                            }

                            panelDetails.Add(nameT,spriteGOT);
                        }

                    }
                }

            }
        }

        return panelDetails;
    }
}
