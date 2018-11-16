using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class PartsDataRead 
{
     XmlDocument x;
    private static PartsDataRead _instance;

    static object sLock = new object();
    public static PartsDataRead Instance      //单例
    {
        get
        {

            if (_instance == null)
            {

                _instance = new PartsDataRead();
            }
            return _instance;
        }
    }

    public void Dispose()
    {
        _instance = null;
    }

    public PartsDataRead()
    {
        Init();
    }
    string t;
    void Init()
    {

        x = new XmlDocument();
       
        t = GetXmlByPath().ToString().Trim();
        x.LoadXml(t);
    }
    string path;
    public TextAsset GetXmlByPath()
    {
        path = "Script/Test4/partsdata";
      
        TextAsset tmp = Resources.Load(path) as TextAsset;

        return tmp;
    }

    public List<string> FindPartsType()
    {
        XmlNodeList nodeList = x.GetElementsByTagName("part");
        List<string> parts = new List<string>();
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    string parttype = xe.InnerText;
                    if(parts.Contains(parttype)==false)
                    {
                        parts.Add(parttype);
                    }
                }
            }
        }

        return parts;
    }
}
