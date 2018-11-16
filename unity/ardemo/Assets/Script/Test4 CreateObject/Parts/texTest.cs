//*************************
//文件名(File Name)：texTest.cs
//作者(Author):Eric Lee
//日期(Create Date):2016.07.21
//作用：对比官方模型需要的零件和用户购买的零件，当零件缺少时，给予提示。
//*************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;
using System.IO;
using Game.Resource;

public class texTest : MonoBehaviour {
    public delegate void DelegateMethod();
    public DelegateMethod delegateMethod;

    GameObject prefabgo;
    GameObject parentT;
    TextureMgr tMgr;
    string robotNameNoType;

    public void RunDelegateMethods()
    {

        if (delegateMethod != null)
        {
            delegateMethod.Invoke();
        }
    }

	// Use this for initialization
	public void CreateLackPicsList () {
        prefabgo = Resources.Load("Prefab/Test4/Test/samplebtn") as GameObject;//查找预设
        parentT = GameObject.Find("UIRootABL/Camera/Center/LackParts/Scroll View/UIGrid");  //查找父物体

        tMgr = new TextureMgr();
        
        Dictionary<string, object> partsOwned = ReadJsonFromTXT() as Dictionary<string,object>;

        if (partsOwned==null)
        {
            //if (JMSimulatorOnly.Instance.lackParts != null)
            //{
            //    JMSimulatorOnly.Instance.lackParts.SetActive(false);
            //}
            return;
        }
        string robotname = RobotMgr.Instance.rbtnametempt;
        robotNameNoType = RobotMgr.NameNoType(robotname);
        Dictionary<string, PartsCountPic> typesOwned = FindPartsTypeNames<object>(partsOwned, robotNameNoType);  //个人已有的零件

        Dictionary<string, string> picNames = GetpSpritesData.Ins.FindAllPicNamesT();
        Dictionary<string, PartsCountPic> types = FindPartsTypeNames<string>(picNames, robotNameNoType); //官方模型含有的零件名称
        Dictionary<string, string> wiresT = new Dictionary<string, string>();
        wiresT = NormalStringData.WiresName();

        bool closeNotice = true;

        if (types.Count <= typesOwned.Count)
        {
            foreach (string key in types.Keys)
            {
                if (typesOwned.ContainsKey(key)==false||((types[key].Count>typesOwned[key].Count)&&typesOwned.ContainsKey(key)) )
                {
                    
                    closeNotice = false;
              
                    break;
                }
            }

        }
        else
        {
            closeNotice = false;
 
        }

        if (closeNotice == true)   //关闭零件缺少提示框
        {
            if (JMSimulatorOnly.Instance.lackParts != null)
            {
                JMSimulatorOnly.Instance.lackParts.SetActive(false);
            }

        }
        else
        {
            //Debug.Log("dfsfdsf:");
            foreach (string key in types.Keys)
            {
                //Debug.Log("dfsfdsf:"+key+";count:"+types[key].Count);
                //1.玩家有搭建模型需要的零件并且零件数量充足
                if (typesOwned.ContainsKey(key) && (typesOwned[key].Count >= types[key].Count))
                {
                    //Debug.Log("dfsfdsf:" + key);


                }
                else
                {
                    List<string> innerSprites = GetInnerTexList.Instance.FindPicType();
                    GameObject childgo = Instantiate(prefabgo, Vector3.zero, Quaternion.identity) as GameObject;
                    childgo.transform.parent = parentT.transform;    //parent物体为UIWrap（含UICenter On Child,UIGrid脚本）
                    childgo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                    if (innerSprites.Contains(types[key].PicTemp) == false)
                    {
                        string pathTemp = ResourcesEx.persistentDataPath + "/partsPic/" + types[key].PicTemp + ".png";
                        Texture2D t = tMgr.LoadTextureByIO(pathTemp);//加载图片资源
                        childgo.GetComponent<UITexture>().mainTexture = t;
                        childgo.transform.Find("Sprite").GetComponent<UISprite>().spriteName = "Clean";


                    }
                    else
                    {

                        childgo.GetComponent<UITexture>().mainTexture = null;
                        childgo.transform.Find("Sprite").GetComponent<UISprite>().spriteName = types[key].PicTemp;
                    }


                    childgo.transform.name = key;

                    //2.玩家有搭建模型需要的零件，但零件数量不够
                    if (typesOwned.ContainsKey(key) && typesOwned[key].Count < types[key].Count)
                    {

                        int countP = types[key].Count - typesOwned[key].Count;
                        childgo.transform.Find("Label").GetComponent<UILabel>().text = key + "(-" + countP.ToString() + ")";
                    }
                    else  //3.玩家没有搭建模型需要的零件
                    {
                        childgo.transform.Find("Label").GetComponent<UILabel>().text = key + "(-" + types[key].Count.ToString() + ")";
                    }
                }


            }
        }
       
	}

    //获取详细数据
    //从TXT文本里都json
    public Dictionary<string, object> ReadJsonFromTXT()
    {

        Dictionary<string, object> recordData = null;

        
        string userID=JMSimulatorOnly.Instance.userID;
       //// Debug.Log("userID:"+userID);
       // userID = "local";
        string path = ResourcesEx.persistentDataPath + "/data/partsImport/" +userID + ".json";
        //Debug.Log("userID:" + userID);
        if(File.Exists(path))
        {
            recordData = new Dictionary<string, object>();
            GetRawJsonData grjd = new GetRawJsonData();

            Dictionary<string, object> sm_textConfig = grjd.ReadJsonFromTXT(path) as Dictionary<string, object>;

            foreach (string keyTemp in sm_textConfig.Keys)
            {
                List<object> picDataT = sm_textConfig[keyTemp] as List<object>;
                foreach (var i in picDataT)
                {
                    Dictionary<string, object> ttt = i as Dictionary<string, object>;
                    string nameT = ttt["name"].ToString();
                    string countT = ttt["num"].ToString();
                    if (recordData.ContainsKey(nameT) == false)
                    {
                        //Debug.Log("file  nameT:"+nameT+":count:"+countT);
                        recordData.Add(nameT, countT);
                    }

                }
            }
        }


        return recordData;
    }

    //每种类型的零件名称及数量
    public Dictionary<string, PartsCountPic> FindPartsTypeNames<T>(Dictionary<string, T> picNames, string robotNameNoType)
    {
        Dictionary<string, string> wiresT = NormalStringData.WiresName();
        // Dictionary<string, string> picNames = new Dictionary<string, string>();
        Dictionary<string, PartsCountPic> types = new Dictionary<string, PartsCountPic>();
        //picNames = FindAllPicNamesT();
        foreach (string temp in picNames.Keys)
        {
            if (temp != robotNameNoType)
            {
                string picType;
                if (temp.Contains("WIRE") == false)
                {
                    if (temp.Contains("-"))   //带"-"
                    {
                        int num = temp.LastIndexOf('-');
                        picType = temp.Substring(0, num);
                    }
                    else     //不带"-"
                    {
                        picType = temp;
                    }
                }
                else    //是线“WIRE”的要转换键值"W"
                {
                    
                    //picType = wiresT[temp];

                    if (wiresT[temp].Contains("-"))   //带"-"
                    {
                        int num = wiresT[temp].LastIndexOf('-');
                        picType = wiresT[temp].Substring(0, num);
                    }
                    else     //不带"-"
                    {
                        picType = wiresT[temp];
                    }
                }

                string valueT = picNames[temp].ToString();
      
                if (types.ContainsKey(picType) == false)   //types中没有的要添加
                {
                    int count = int.Parse(valueT);
                    PartsCountPic pcp = new PartsCountPic();
                    pcp.Count = count;
                    pcp.PicTemp = temp;

                    types.Add(picType, pcp);
                }
                else            //types中有的要增加value的值
                {
                    int countT = int.Parse(valueT);
                    countT += types[picType].Count;
                    types[picType].Count = countT;
                }
            }
        }
        return types;
    }


}

public class PartsCountPic
{
    private int count;    //零件的数量
    private string picTemp;//代替显示的图片的名称

    public int Count
    {
       get{return count;}
       set{count = value;}
    }

    public string PicTemp
    {
        get { return picTemp; }
        set { picTemp = value; }
    }
}
