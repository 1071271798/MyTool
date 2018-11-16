using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


//内置图片名称列表
public class GetInnerTexList : MonoBehaviour {
     XmlDocument x;
    private static GetInnerTexList _instance;

    static object sLock = new object();
    public static GetInnerTexList Instance      //单例
    {
        get
        {

            if (_instance == null)
            {

                _instance = new GetInnerTexList();
            }
            return _instance;
        }
    }

    public void Dispose()
    {
        _instance = null;
    }

    public GetInnerTexList()
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
        path = "Script/Test4/PartsList/PartsList";
      
        TextAsset tmp = Resources.Load(path) as TextAsset;

        return tmp;
    }

    public List<string> FindPicType()
    {
        XmlNodeList nodeList = x.GetElementsByTagName("Parts");
        List<string> pics = new List<string>();
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;
                    string parttype = xe.InnerText;
                    if(pics.Contains(parttype)==false)
                    {
                        pics.Add(parttype);
                    }
                }
            }
        }

        return pics;
    }
}
