/*
 * 作用：菜单页显示的机器人的名称及图片名称的管理
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RobotBtnData
{
     XmlDocument x;
     public RobotBtnData()
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
        TextAsset tmp = Resources.Load("Script/Test4/DefaultRobot/defaultrobot") as TextAsset;

        return tmp;
    }

    public Dictionary<string, string[]> FindBtnName()
    {
        XmlNodeList nodeList = x.GetElementsByTagName("GoType");
        Dictionary<string, string[]> names = new Dictionary<string, string[]>();
        string[] xl=new string[3];
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;   //xe.InnerText:按钮的名称
                    if (xe.GetAttribute("id") !=null)
                    {
                        xl = new string[3];

                        xl[0] = xe.GetAttribute("name");    //labelname
                        xl[1] = xe.InnerText;       //spritename
                        xl[2] = xe.GetAttribute("level"); 
                        string idxe = xe.GetAttribute("id");
                        names.Add(idxe, xl);
                    }
                }

            }
        }
        return names;
    }
}
