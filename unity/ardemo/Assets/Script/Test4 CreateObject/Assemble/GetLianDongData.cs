/*
 * 读取联动模型数据
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Scene;
using Game.Platform;
using Game.UI;
using Game.Event;
using System;
using System.Xml;
using Game.Resource;


public class GetLianDongData : SingletonBehaviour<GetLianDongData>
{

    public List<GameObject> contents = new List<GameObject>();

    private static XmlDocument xele = new XmlDocument();
    public string path;
    public string Path
    {
        get
        {
            path = GetPath();
            return path;
        }
    }

    public string GetPath()
    {
        string opentype = RecordContactInfo.Instance.openType;
        //Debug.Log("opentype:" + opentype + ";name:" + RecordContactInfo.Instance.robotname);
        string xmlName = RecordContactInfo.Instance.robotname;//RobotMgr.NameNoType(RecordContactInfo.Instance.robotname);
        
        string pathfile = ResourcesEx.persistentDataPath + "/" + opentype + "/" + xmlName + "/ldgos/ldgos.xml";

        return pathfile;
    }
    /// <summary>
    /// 联动物体的信息
    /// </summary>
    /// <param name="namewithtype"></param>
    /// <param name="filename"></param>
    public Dictionary<string,LianDongGOs> FindLDGOsData()
    {
        if (File.Exists(Path)==false)
        {
            return null;
        }
        xele.Load(Path);
        Dictionary<string,LianDongGOs> ldgos=null;
        XmlNodeList nodeList = xele.GetElementsByTagName("rotateGOs");
        if (nodeList != null && nodeList.Count > 0)
        {
            ldgos =new Dictionary<string,LianDongGOs>();
            
            foreach (XmlNode xn in nodeList)
            {
                LianDongGOs ldgosTemp=new LianDongGOs();
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    string nameTemp = xe.GetAttribute("name");

                    XmlNodeList nodeListT=xe.GetElementsByTagName("angles");
                    if(nodeListT !=null&&nodeListT.Count>0)
                    {
                        
                        foreach(XmlNode xnl1 in nodeListT)
                        {
                            XmlElement xel1=(XmlElement)xnl1;
                            string idT = xel1.GetAttribute("id");
                            int id = int.Parse(idT);

                            string startXT = xel1.GetAttribute("startX");
                            float startX = float.Parse(startXT);

                            string endXT = xel1.GetAttribute("endX");
                            float endX = float.Parse(endXT);                 

                            XmlNodeList nodeListT2 = xel1.GetElementsByTagName("tGOs");
                            Dictionary<string, Vector3> ldangle = null;
                            if (nodeListT2 != null && nodeListT2.Count > 0)
                            {
                                ldangle = new Dictionary<string, Vector3>();
                                foreach (XmlNode xnl2 in nodeListT2)
                                {
                                    XmlElement xel2 = (XmlElement)xnl2;
                                    string nameT2 = xel2.GetAttribute("name");
                                    if (ldangle.ContainsKey(nameT2) == false)
                                    {
                                        string angleT = xel2.GetAttribute("angle");
                                        Vector3 angle = SingletonObject<PublicTools>.GetInst().StringToVector(angleT);
                                        ldangle.Add(nameT2, angle);
                                    }
                                }
                            }

                            XmlNodeList nodeListT3 = xel1.GetElementsByTagName("pGOs");
                            Dictionary<string, Vector3> ldpos = null;
                            if (nodeListT3 != null && nodeListT3.Count > 0)
                            {
                                ldpos = new Dictionary<string, Vector3>();
                                foreach (XmlNode xnl3 in nodeListT3)
                                {
                                    XmlElement xel3 = (XmlElement)xnl3;
                                    string nameT3 = xel3.GetAttribute("name");
                                    if (ldangle.ContainsKey(nameT3) == false)
                                    {
                                        string posT = xel3.GetAttribute("pos");
                                        Vector3 pos = SingletonObject<PublicTools>.GetInst().StringToVector(posT);
                                        ldangle.Add(nameT3, pos);
                                    }
                                }
                            }
                            ldgo ldgoT = new ldgo();
                            ldgoT.start = startX;
                            ldgoT.end = endX;
                            ldgoT.ldangle = ldangle;
                            ldgoT.ldpos = ldpos;

                            if (ldgosTemp.ldgos.ContainsKey(id) == false)
                            {
                                ldgosTemp.ldgos.Add(id,ldgoT);
                            }
                        }

                    }

                    if(ldgos.ContainsKey(nameTemp)==false)
                    {
                        ldgos.Add(nameTemp, ldgosTemp);
                    }
                }
            }               
          }
        return ldgos;
       }
}
