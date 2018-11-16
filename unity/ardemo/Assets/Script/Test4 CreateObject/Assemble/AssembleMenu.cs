/*
 * 模型名称目录
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
using Game;


public class AssembleMenu : MonoBehaviour
{
    public static AssembleMenu _instance;

    public List<GameObject> items=new List<GameObject>();
    public List<GameObject> contents = new List<GameObject>();
    public List<string> createList = new List<string>();
    public List<string> downloadList=new List<string>();
    public List<string> defaultList=new List<string>();
    public List<GameObject> newBtns;    //每个类型（自制，默认，下载）生成的物体的名称,
    public List<GameObject> newbtnstemp;//所有生成的物体的名称
    public Dictionary<string,List<GameObject>> allnewbtns=new Dictionary<string,List<GameObject>>();//所有生成的物体的名称

    //public string openType;//打开的robot数据属于哪种类型(自制，官方，下载)

    private GameObject sampleBtn;
    private GameObject ownBtn;//个人模型btn预设
    public UILabel nameInputLabel;         //输入栏的内容
    public UILabel inputLabel;         //输入栏的内容
    public UILabel motorInputLabel;   //输入马达
    public UILabel inputNotice;
    public GameObject inputField;
    private Vector3 oriPos;

    private GameObject go;   //生成的按钮
    private GameObject newParent;
    private GameObject createParent;

    public List<string> existXMLNams = new List<string>();

    private int newNum = 0;
    private int createNum = 0;
    
    private GameObject menu;
    public GameObject saveMenu;
    public GameObject deleteMenu;

    private Dictionary<string, GameObject> bts = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> savebts = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> deletebts = new Dictionary<string, GameObject>();
    Dictionary<string, string[]> names = new Dictionary<string, string[]>();

    //删除
    bool isPressed = false;
    bool clickClose = false;
    GameObject deletebtn = null;

    public UILabel ownlabel;
    public UILabel defaultlabel;


    RobotBtnData rbd;

    void Awake()
    {        
        defaultList = RecordContactInfo.Instance.XMLExist("default"); 
        createList = RecordContactInfo.Instance.XMLExist("playerdata");
    }

    void Start()
    {
        _instance = this;
        items.Add(GameObject.Find("GOItem1"));    //自制栏的目录
        items.Add(GameObject.Find("GOItem2"));    //默认的目录
        items.Add(GameObject.Find("GOItem3"));    //下载的目录
        newBtns = new List<GameObject>();
        newbtnstemp = new List<GameObject>();
        allnewbtns.Add("playerdata",newbtnstemp);
        allnewbtns.Add("default", newbtnstemp);
        allnewbtns.Add("download", newbtnstemp);

        sampleBtn = Resources.Load("Prefab/Test4/Test/samplebtn") as GameObject;
        ownBtn = Resources.Load("Prefab/Test4/Test/ownbtn") as GameObject;
        nameInputLabel = GameObject.Find("UIRootMenu/SaveMenu/NameInputField/InputLabel").GetComponent<UILabel>();
        inputLabel = GameObject.Find("UIRootMenu/SaveMenu/InputField/InputLabel").GetComponent<UILabel>();
        motorInputLabel = GameObject.Find("UIRootMenu/SaveMenu/MotorInputField/InputLabel").GetComponent<UILabel>();
        inputNotice = GameObject.Find("UIRootMenu/SaveMenu/InputField/InputNotice").GetComponent<UILabel>();
        inputField = GameObject.Find("UIRootMenu/SaveMenu/InputField");
        saveMenu = GameObject.Find("UIRootMenu/SaveMenu");

        menu = GameObject.Find("UIRootMenu/StartMenu");
        deleteMenu = GameObject.Find("UIRootMenu/DeleteMenu");
        oriPos = new Vector3(-117.0f, -222.0f, 0);   

        //初始化时，自制菜单是默认启动的，所以需要初始化生成名称按钮
        CreateDefault();
        foreach (Transform child in menu.GetComponentInChildren<Transform>())
        {
            bts[child.name] = child.gameObject;
        }
        contents.Add(bts["Content1"]);
        contents.Add(bts["Content2"]);
        contents.Add(bts["Content3"]);

        foreach (Transform child in saveMenu.GetComponentInChildren<Transform>())
        {
            savebts.Add(child.name, child.gameObject);
        }

        foreach (Transform child in deleteMenu.GetComponentInChildren<Transform>())
        {
            deletebts.Add(child.name, child.gameObject);
        }
        deleteMenu.SetActive(false);
        saveMenu.SetActive(false);
        
        AddEvents();
        
        EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, On_Connenct_Result);

        GameObject uir = GameObject.Find("MainUIRoot");
        if (uir != null)
        {
            if (SceneMgr.mCurrentSceneType == SceneType.MenuScene)
            {
                Destroy(uir);
            }
        }
        
        ownlabel = FindBtnLabel(bts["Owned"]);
        defaultlabel = FindBtnLabel(bts["Default"]);


        //Debug.Log("opentype:" + RecordContactInfo.Instance.openTypeTemp);
        if (RecordContactInfo.Instance.openType == "playerdata")
        {
            ClickOwned(bts["Owned"]);
        }
        else if (RecordContactInfo.Instance.openType == "default")
        {
            ClickDefault(bts["Default"]);
        }


        ///修改机器人名字
        //RecordContactInfo.Instance.ReviseFileNam("playerdata", "qie", "tiane");   

    }


    float timetemp = 0;
    void Update()
    {
        if(isPressed)
        {
            if (Time.realtimeSinceStartup-timetemp > 1.5f)
            {
                deleteMenu.SetActive(true);
                clickClose = true;
            }
            else
            {
                clickClose = false;
                if (deleteMenu.activeInHierarchy)
                {
                    deleteMenu.SetActive(false);
                }
            }
        }
    }

    void AddEvents()
    {
        UIEventListener.Get(bts["Owned"]).onClick += ClickOwned;
        UIEventListener.Get(bts["Default"]).onClick += ClickDefault;

        UIEventListener.Get(savebts["Cancel"]).onClick += HideSaveMenu;
        UIEventListener.Get(savebts["Confirm"]).onClick +=ConfirmRobotName;

        UIEventListener.Get(deletebts["Confirm"]).onClick += DeleteBtnXML;
        UIEventListener.Get(deletebts["Cancel"]).onClick += CloseDelete;      
    }

    //删除按钮和XML文档
    public void DeleteBtnXML(GameObject tempgo)
    {
        string btnname = deletebtn.name;
        string datatypetemp = "";
        for (int i = allnewbtns["playerdata"].Count - 1; i >= 0; i--)
        {
            if (btnname == allnewbtns["playerdata"][i].name)
            {
                //删除对于名称的按钮
                Destroy(allnewbtns["playerdata"][i]);

                //删除相应列表中的对应名称，如果列表中有的话
                foreach (GameObject m in items)
                {
                    if (m.activeInHierarchy)
                    {
                        switch (m.name)
                        {
                            case "GOItem1":
                                datatypetemp = "playerdata";
                                if (createList.Contains(btnname)==true)
                                {
                                  createList.Remove(btnname);
                                }
                                break;
                            case "GOItem2":
                                datatypetemp = "default";
                                if (downloadList.Contains(btnname) == true)
                                {
                                    downloadList.Remove(btnname);
                                }
                                break;
                            case "GOItem3":
                                datatypetemp = "download";
                                if (defaultList.Contains(btnname) == true)
                                {
                                    defaultList.Remove(btnname);
                                }
                                break;
                        }

                       m.transform.GetComponent<UIGrid>().repositionNow = true;  //删除后，按钮重新排列
                    }
                }
                allnewbtns["playerdata"].RemoveAt(i);
                //删除XML文档
                string namenotype = RobotMgr.NameNoType(btnname);
                RecordContactInfo.Instance.DeleteXmlFile(namenotype, datatypetemp);
                if (RobotMgr.Instance.rbt.ContainsKey(btnname) == true)
                {
                    RobotMgr.Instance.rbt.Remove(btnname);
                    RobotManager.GetInst().DeleteRobot(btnname);
                }

                deleteMenu.SetActive(false);
                deletebtn = null;
                //ResetBtnsPos();
            }
        }       
    }

    //删除名字的后缀名
    string[] xmlN = new string[2];
    public string DeleteNameExtension(string xmlNam)
    {
        xmlN = xmlNam.Split('.');
        return xmlN[0];
    }

    public void ClickOwned(GameObject go)
    {
        CloseContent("Content1");
        ownlabel.color = new Color(0, 0, 0, 1.0f);
        defaultlabel.color = new Color(0f, 0f, 0f, 0.3f);
    }
    public void ClickDefault(GameObject go)
    {
        CloseContent("Content2");
        ownlabel.color = new Color(0f, 0f, 0f, 0.3f);
        defaultlabel.color = new Color(0f, 0f, 0f, 1.0f);
    }

    //字体颜色变色
    public UILabel FindBtnLabel(GameObject btn)
    {
        UILabel labelTemp=null;
        foreach(Transform temp in btn.GetComponentsInChildren<Transform>())
        {
            if(temp.name=="Label")
            {
                labelTemp=temp.GetComponent<UILabel>();
            }
        }

        return labelTemp;
    }


    //蓝牙连接结果
    void On_Connenct_Result(EventArg arg)
    {
        try
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                    
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
            
    }

    //新建物体即新建一个物体的名称按钮
    #region 新建物体
    public void ConfirmRobotName(GameObject go)
    {
        string nameTemp = nameInputLabel.text;
        string djID = inputLabel.text;
        string motorID = motorInputLabel.text;
        RecordContactInfo.Instance.openType = "playerdata";
        string namewithtype = RobotMgr.NameWithType(nameTemp,"playerdata");
        if (RobotMgr.Instance.rbt.ContainsKey(namewithtype) == false && nameTemp != "")
        {
            RobotManager.GetInst().IsCreateRobotFlag = true;
#if UNITY_EDITOR
            //if (PublicFunction.IsInteger(nameTemp))
            //{
                List<byte> djIDList = new List<byte>();
                List<byte> motorIDList = new List<byte>();
                int djIDCount = int.Parse(djID);
                int motorIDCount = int.Parse(motorID);
                for (byte i = 1; i <= djIDCount; ++i)
                {
                    djIDList.Add(i);
                }
                for (byte i = 1; i <= motorIDCount; ++i)
                {
                    motorIDList.Add(i);
                }
                RobotManager.GetInst().IsCreateRobotFlag = false;
                ReadMotherboardDataMsgAck msg = new ReadMotherboardDataMsgAck(0);
                msg.ids = djIDList;
                msg.motors = motorIDList;
                CreateGO(msg);
            //}
            //else
            //{
            //    HUDTextTips.ShowTextTip("请输入整数");
            //}

#else
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                PlatformMgr.Instance.DisConnenctBuletooth();
                On_Connenct_Result(new EventArg(false));
            }
            SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(RobotManager.GetInst().GetCurrentRobot());
#endif
        }
        else if (RobotMgr.Instance.rbt.ContainsKey(namewithtype) == false && nameTemp == "")
        {
            inputNotice.text = "Please input a robot's name.";
        }
        else if (RobotMgr.Instance.rbt.ContainsKey(namewithtype) == true)
        {
            inputNotice.text = "Name repetition!Please input new one.";
        }
    }

    public void CreateGO(ReadMotherboardDataMsgAck data)
    {
        List<int> djidTemp = new List<int>();
        if(data != null && data.ids.Count > 0 || data.motors != null && data.motors.Count > 0)
        {
                    
            string nameTemp = nameInputLabel.text;
            Console.WriteLine("new robot:" + nameTemp);

            RecordContactInfo.Instance.openType = "playerdata";
            string nameWithType = RobotMgr.NameWithType(nameTemp, "playerdata");
            RobotDataMgr.Instance.CreatRobotWithoutModel(nameWithType, data);   //EditAction
            RobotDataMgr.Instance.SaveRobotMsg(nameWithType);

            RobotManager.GetInst().CreateOrUpdateRobot(nameWithType, RobotMgr.Instance.rbt[nameWithType].id);
            RobotMgr.Instance.rbtnametempt = nameWithType;

            Robot robot = RobotManager.GetInst().GetRobotForID(RobotMgr.Instance.rbt[nameWithType].id);
            if (null != robot)
            {
                Robot createRobot = RobotManager.GetInst().GetCreateRobot();
                robot.ConnectRobotResult(createRobot.Mac, true);
                robot.MotherboardData = createRobot.MotherboardData;
                if (ClientMain.Use_Third_App_Flag)
                {
                    robot.ShowName = createRobot.ShowName;
                }
                RobotManager.GetInst().SetCurrentRobot(robot);
                //robot.ReadBack();
                ControlData.ClearData();
            }
            RobotMgr.Instance.newRobot = true;

            //SceneMgr.EnterScene(SceneType.MainWindow);
            MainScene.GotoScene(MainMenuType.Action_Menu);
        }
    }

    IEnumerator DelayTime(float t)
    {
        yield return new WaitForSeconds(t);
        inputNotice.text = "";
    }
	
    //根据每个目录中的文件名称生成相应的按钮

    public void CreateDefault()
    {
        foreach (GameObject m in items)
        {
                switch (m.name)
                {
                    case "GOItem1":
                        inputLabel.text = "";
                        newBtns.Clear();

                        if (createList != null)
                        {
                            CreateListItem1(m, createList, ref newBtns, "playerdata");
                        }
                        break;
                    case "GOItem2":
                        inputLabel.text = "";
                        newBtns.Clear();

                        if (defaultList != null)
                        {
                            CreateListN(m, defaultList, ref newBtns, "default");
                        }
                        break;
                    case "GOItem3":
                        inputLabel.text = "";
                        newBtns.Clear();
  
                        if (downloadList != null)
                        {
                           CreateListN(m, downloadList, ref newBtns, "download");
                        }
                        break;
                }
            }
    }

    public void CreateItems()
    {
        foreach (GameObject m in items)
        {
            if (m.activeInHierarchy)
            {
            switch (m.name)
            {
                case "GOItem1":
                    inputLabel.text = "";
                    newBtns.Clear();
                    CreateListItem1(m, createList, ref newBtns, "playerdata");
                    break;
                case "GOItem2":
                    inputLabel.text = "";
                    newBtns.Clear();
                    CreateListN(m, defaultList, ref newBtns, "default");
                    break;
                case "GOItem3":
                    inputLabel.text = "";
                    newBtns.Clear();
                    CreateListN(m, downloadList, ref newBtns, "download");
                    break;
            }
          }
        }
    }

    public void CloseContent(string namtemp)
    {
        for (int i = 0; i < contents.Count;i++ )
        {
            if (contents[i].name != namtemp)
            {
                contents[i].SetActive(false);
            }
            else
            {
                contents[i].SetActive(true);
            }
        }
    }

    //添加物体，排除相同物体
    public void AddGo(ref List<GameObject> list,GameObject obj)
    {
        if (list!=null&&list.Contains(obj)==false)
        {
            list.Add(obj);
        }
    }

    //删除菜单的显示与应酬
    public void CloseDelete(GameObject go)
    {
        deletebtn = null;
        clickClose = false;
        deleteMenu.SetActive(false);
    }

    //是否打开删除菜单
    public void OpenDelete(GameObject go,bool isPress)
    {
        if(isPress)
        {
            isPressed = true;
            timetemp = 0;
            timetemp += Time.realtimeSinceStartup;

            deletebtn = go;
            //Debug.Log("press:"+go.name);
        }
        else 
        {
            isPressed = false;
        }
        
    }

    //子物体中的sprite换UI--显示等级
    public void ChangeChildUI(Transform btntemp,string uiname)
    {
        foreach(Transform child in btntemp.GetComponentInChildren<Transform>())
        {
            if(child.name =="Sprite")
            {
                child.GetComponent<UISprite>().spriteName = uiname;
                break;
            }
        }
    }

    //根据给的List<string>数组生成相应的按钮
    GameObject temp;
    Texture textureOne;
    public void CreateListN(GameObject mchild,List<string> nameList,ref List<GameObject> newbs,string openType)
    {
        if(newbs!=null)
        {
            newbs.Clear();
        }
        createParent = mchild;
        
        Transform childs = mchild.GetComponentInChildren<Transform>();
        if (null == childs)
        {
            createNum = 0;
        }
        else
            createNum = childs.childCount;
        if (openType == "playerdata")   //"GOItem1"是激活状态
        {
            createNum = 0;
#region 玩家数据
            temp = CreateNewItem(ownBtn,"Add_playerdata", createNum++, createParent,0);
            if (newbs.Contains(temp) == false)
            {
                newbs.Add(temp);
            }
            if (createList != null)
            {

                for (int i = 1; i <= nameList.Count; i++)
                {

                    if (nameList[i - 1] != "")
                    {
                        if (CanCreate(nameList[i - 1], mchild.transform))
                        {
                            temp = CreateNewItem(ownBtn,(nameList[i - 1] + "_playerdata"), createNum++, createParent, i);
                            if (newbs.Contains(temp) == false)
                            {
                                newbs.Add(temp);
                            }
                            if (allnewbtns["playerdata"].Contains(temp) == false)
                            {
                                allnewbtns["playerdata"].Add(temp);
                            }
                        }
                    }
                }              

                for (int j = 0; j < newbs.Count; j++)
                {
                    if (j != 0)
                    {
                        //string jstr = (j % 8).ToString();
                        newbs[j].GetComponent<UISprite>().spriteName = "icon_star_sel";
                        
                        //ChangeChildUI(newbs[j].transform, names[jstr][2]);

                        UIEventListener.Get(newbs[j]).onPress = OpenDelete;
                        UIEventListener.Get(newbs[j]).onClick += SelectRobotPlayer;

                    }
                    else
                    {
                        
                        newbs[0].GetComponent<UISprite>().spriteName = "Add";
                        newbs[0].GetComponent<UISprite>().SetRect(0,0,50.0f,50.0f);
                        foreach(Transform child in newbs[0].GetComponent<Transform>())
                        {
                            if(child.name=="Sprite")
                            {
                                child.GetComponent<UISprite>().spriteName = "Button";
                            }
                        }
                        
                        UIEventListener.Get(newbs[j]).onClick += ShowSaveMenu;
                    }
                }
                UIGrid grid = items[0].GetComponent<UIGrid>();
                if (null != grid)
                {
                    grid.repositionNow = true;
                }
            }
#endregion
        }
        else if (openType == "default")
        {
            createNum = 0;
#region 官方数据
            if (defaultList != null)
            {
                textureOne = Resources.Load("prefab/Test4/UI/Meebot") as Texture;
                for (int i = 0; i < nameList.Count; i++)
                {        
                    if (nameList[i] != "")
                    {
                        if (CanCreate(nameList[i], mchild.transform))
                        {
                            temp = CreateNewItem(sampleBtn,(nameList[i] + "_default"), createNum++, createParent,i);
                            if (newbs.Contains(temp) == false)
                            {
                                newbs.Add(temp);
                            }
                            if (allnewbtns["default"].Contains(temp) == false)
                            {
                                allnewbtns["default"].Add(temp);
                            }
                            StartCoroutine(SetPic(nameList[i], temp));
                        }
                    }
                }
        
                for (int j = 0; j < newbs.Count;j++ )
                {
                    //string jstr = (j % 8).ToString();
                   

                    //ChangeChildUI(newbs[j].transform, names[jstr][2]);

                    //MainBtns.FindLabelChild(newbs[j], names[jstr][0]);
                    UIEventListener.Get(newbs[j]).onClick = SelectRobotDefault;
                }
            }
#endregion

            UIGrid grid = items[1].GetComponent<UIGrid>();
            if (null != grid)
            {
                grid.repositionNow = true;
            }
        }
        else if (openType == "download")
        {
            createNum = 0;
#region 下载数据
            if (downloadList != null)
            {
                for (int i = 0; i < nameList.Count; i++)
                {
                    if (nameList[i] != "")
                    {
                        if (CanCreate(nameList[i], mchild.transform))
                        {
                            temp = CreateNewItem(ownBtn,(nameList[i] + "_download"), createNum++, createParent, i);
                            if (newbs.Contains(temp) == false)
                            {
                                newbs.Add(temp);
                            }
                            if (allnewbtns["download"].Contains(temp) == false)
                            {
                                allnewbtns["download"].Add(temp);
                            }
                        }
                    }
                }

                for (int j = 0; j < newbs.Count; j++)
                {
                    UIEventListener.Get(newbs[j]).onClick = SelectRobotDownload;
                }
                UIGrid grid = items[2].GetComponent<UIGrid>();
                if (null != grid)
                {
                    grid.repositionNow = true;
                }
            }
#endregion
        }
    }


    IEnumerator SetPic(string picName,GameObject goT)
    {
            //零件图片
            string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "/default/" + picName + "/" + picName + ".png";
            //Debug.Log("dfsfdsf:" + pathTemp);
            WWW www = new WWW(pathTemp);

            Texture textureT = null;
            yield return www;
            if (www != null && string.IsNullOrEmpty(www.error))
            {
                //获取Texture
                textureT = www.texture;
                //更多操作...    
                if (textureT != null)
                {

                    //Debug.Log("dfsfdsf:"+pics[jubuPCount]);
                    goT.GetComponent<UITexture>().mainTexture = textureT;
                }
                

            }
            else
            {
               // Debug.Log("dfsfdsf:" + textureOne);
                goT.GetComponent<UITexture>().mainTexture = textureOne;
            }

        
    }
    //显示SaveMenu
    public void ShowSaveMenu(GameObject go)
    {
        saveMenu.SetActive(true);
    }
    //隐藏SaveMenu
    public void HideSaveMenu(GameObject go)
    {
        saveMenu.SetActive(false);
    }


    public void CreateListItem1(GameObject mchild, List<string> nameList, ref List<GameObject> newbs,string openType)
    {
        CreateListN(mchild,nameList,ref newbs,openType);
    }

    public void SelectRobotPlayer(GameObject btn)
    {
        if (clickClose == false)
        {
            
            RecordContactInfo.Instance.openType = "playerdata";

            if (RobotMgr.Instance.rbt.ContainsKey(btn.name) == false)
            {

                RobotDataMgr.Instance.ReadMsg(btn.name);
                RobotManager.GetInst().CreateOrUpdateRobot(btn.name);
            }
            SelectRobot(btn);


        }
    }

    public void SelectRobotDefault(GameObject btn)
    {
        RecordContactInfo.Instance.openType = "default";
        // Debug.Log("opentypedefault:" + RecordContactInfo.Instance.openTypeTemp);
        SceneMgrTest.Instance.LastScene = SceneType.StartScene;
        if (RobotMgr.Instance.rbt.ContainsKey(btn.name) == false)
        {
            
            RobotDataMgr.Instance.ReadMsg(btn.name);
            RobotManager.GetInst().CreateOrUpdateRobot(btn.name);
        }
        SelectRobot(btn);

        /*ReadMotherboardDataMsgAck msg = new ReadMotherboardDataMsgAck();
        msg.ids.Add(1);
        ServosConStateMsg.ShowMsg(msg);*/
    }

    public void SelectRobotDownload(GameObject btn)
    {
        
        RecordContactInfo.Instance.openType = "download";
        

        if (RobotMgr.Instance.rbt.ContainsKey(btn.name) == false)
        {
            RobotDataMgr.Instance.ReadMsg(btn.name);
        }

        SelectRobot(btn);
    }


    //确定选中名称
    public void SelectRobot(GameObject btn)    //
    {
        try
        {
            RobotMgr.Instance.rbtnametempt = btn.name;
           // Debug.Log("btnName:"+btn.name);
            RobotManager.GetInst().IsCreateRobotFlag = false;
            string robotname = RobotMgr.Instance.rbtnametempt;

            //Debug.Log("dfsdf:" + btn.name + ":gos:" + RobotMgr.Instance.rb      t[btn.name].gos.Count);
            RobotMgr.Instance.newRobot = false;
            if (RobotMgr.Instance.rbt.ContainsKey(RobotMgr.Instance.rbtnametempt))
            {

                Robot robot = RobotManager.GetInst().GetRobotForID(RobotMgr.Instance.rbt[RobotMgr.Instance.rbtnametempt].id);
                if (null != robot)
                {
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {//蓝牙是连接的
                        Robot oldRobot = RobotManager.GetInst().GetCurrentRobot();
                        if (null != oldRobot && oldRobot != robot)
                        {//换了模型要断开蓝牙
                            PlatformMgr.Instance.DisConnenctBuletooth();
                        }
                    }
                    RobotManager.GetInst().SetCurrentRobot(robot);
                }
            }
            ControlData.ClearData();
            EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, On_Connenct_Result);
            //   Debug.Log("come mainwindow");
            RobotMgr.Instance.openActionList = true;
            //SceneMgr.EnterScene(SceneType.MainWindow);
            MainScene.GotoScene(MainMenuType.Action_Menu);

            RobotMgr.Instance.startNum = true;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

#region
    //确定选中名称
    //public void ConfirmName()    //
    //{
    //    try
    //    {
    //        if (inputLabel.text != "" && inputLabel.text != "Please Select Robot!" && inputLabel.text != "Input a new robot's name" && RecordContactInfo.Instance.XMLExist().Contains(inputLabel.text) == true)
    //        {
    //            RobotMgr.Instance.rbtnametempt = inputLabel.text;
    //            string robotname = RobotMgr.Instance.rbtnametempt;
    //            if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
    //            {
    //                RobotDataMgr.Instance.ReadMsg(robotname);
    //            }
    //            RobotDataMgr.Instance.orirbtname = robotname;

    //            RobotMgr.Instance.newRobot = false;

    //            MainWindow.CertainSelect();
    //            RobotMgr.Instance.startNum = true;
        
    //        }
    //        else
    //        {
    //            inputNotice.text = "Please Select Robot！";
    //        }
    //    }
    //    catch (System.Exception ex)
    //    {
    //        if (ClientMain.Exception_Log_Flag)
    //        {
    //            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
    //            Debuger.Log(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
    //        }
    //    }
        

    //}
#endregion

	//生成按钮
	private GameObject CreateNewItem(GameObject prefab,string robotName,int posNum,GameObject parentTransform,int itemp) 
    {
        go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        go.name = robotName;

        
        foreach(Transform m in go.transform.GetComponentInChildren<Transform>())
        {
			if(m.name=="Label")
			{           
				string namenoType = RobotMgr.NameNoType(robotName);
                m.GetComponent<UILabel>().text = namenoType;
			}
        }
        go.transform.parent = parentTransform.transform;

        go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);   //使用的是NGUI自动的自动排序

        return go;
    }

    //是否可以生成按钮
    public bool CanCreate(string tempt,Transform t)
    {
        int count=0;
        foreach(Transform child in t.GetComponentInChildren<Transform>())
        {
            if(child.name == tempt)
            {
                count++;
            }
        }

        if (count == 0) return true;
        else return false;
    }

#endregion
}
