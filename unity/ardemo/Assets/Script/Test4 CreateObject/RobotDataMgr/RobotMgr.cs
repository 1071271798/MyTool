/*
 * 作用：处理机器人类中的数据
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Game.Scene;
using Game.Platform;

public class RobotMgr
{
    #region 静态变量
    private static RobotMgr _instance;
	
    static object sLock = new object();
    public static RobotMgr Instance      //单例
    {
        get
        {
            if (null == _instance)
            {
                lock (sLock)
                {
                    if (_instance == null)
                    {

                        _instance = new RobotMgr();
                    }
                }
            }

            return _instance;
        }
    }

   public readonly static Color OriColor = new Color(145.0f/255, 145.0f / 255, 145.0f / 255, 255.0f/255);    //Grey

   public bool newRobot = false;
   public bool openActionList = false;

    private RobotMgr()
    {
        
    }
    #endregion

    #region 公共变量
    public Dictionary<string, RobotData> rbt = new Dictionary<string, RobotData>();   //<机器人名字，机器人类RobotData>
   public Dictionary<string, AnimData> rbtAnim = new Dictionary<string, AnimData>();   //<机器人名字，机器人类动画>

   public Dictionary<string, AnimationClip> anims = new Dictionary<string, AnimationClip>();  //存储动画片段
   public Dictionary<string, Texture> outPics = new Dictionary<string, Texture>();//从外部加载的图片资源
   public Dictionary<string, Texture> jubuPics = new Dictionary<string, Texture>();//外部加载的局部图资源
   //public Dictionary<string, GameObject> zkhNums = new Dictionary<string, GameObject>();   //主控盒数字标签
   public List<GameObject> hideGOs = new List<GameObject>();//生成时需要隐藏的模型

   public List<GameObject> AddedAllGOs = new List<GameObject>();//已生成的所有零件模型
   public Dictionary<string, DPBOX> alldpbox = new Dictionary<string, DPBOX>();//舵盘和舵机主体旋转时需要的父物体模型--只用舵机才使用该属性
   public List<GameObject> alldj = new List<GameObject>();  //所有的舵机物体-实物
   public Dictionary<string, Vector3> goPosAll = new Dictionary<string, Vector3>();  //生成模型的所有坐标
   public Dictionary<string, Vector3> goAngleAll = new Dictionary<string, Vector3>();  //生成模型的所有角度
   public Dictionary<string, Vector3> goScaleAll = new Dictionary<string, Vector3>();  //生成模型的所有尺寸
   public Dictionary<string, GameObject> prefabgos = new Dictionary<string, GameObject>();
   public Dictionary<string, RotateDJGO> djRotate = new Dictionary<string, RotateDJGO>();

   public int colliderCount = 0;//碰撞的次数
   public int finalCount = 2;//碰撞事件发生的次数
   public bool leave=true;

    public string rbtnametempt;
    public int nowCount;//选中物体上插入的物体的数量
    public int allCount;  //组成机器人的所有物体的数量
    public int djstartnum;
    public int djstartid;

    public bool startNum = true;

    public Dictionary<string, int> gonamnum = new Dictionary<string, int>();
    private List<string> typeIDTemp = new List<string>();
    private List<string> typeTemp = new List<string>();
   
    private int nameNum = 1;

    #endregion

    #region 创建
    //创建机器人  <机器人的名字，机器人ID,数据类型(玩家，默认，下载)>
    public void CreateRobot(string robotname,string robotoid,string datatype,string level)
    {
        if (rbt.ContainsKey(robotname)==false)
        {
           RobotData robot1=new RobotData();
           robot1.robotName = robotname;
           robot1.id = robotoid;
           robot1.dataType = datatype;
           robot1.level = level;
           rbt.Add(robotname, robot1);
        }

    }

    ////创建animation
    //public void CreateAnim(string robotid)
    //{
        
    //    if (rbtAnim.ContainsKey(robotid) == false)
    //    {
    //        AnimData anim = new AnimData();

    //        rbtAnim.Add(robotid, anim);

    //    }
    //}



    //创建GO
    public void CreateGO(string robotname,string goname,string goid,string gotype,int idTemp)
    {
        if (rbt[robotname].gos.ContainsKey(goname) == false)
        {
            GO go = new GO();
            go.goType = gotype;
            go.goID = goid;

            if (gotype == "seivo"||gotype=="duoji")
            {
                go.djID = idTemp;
            }

            if (gotype == "motor")
            {
                go.motorID = idTemp;
            }

            rbt[robotname].gos.Add(goname, go);
            
        }
        else
        {
            //Debug.Log("go:"+goname);
        }
    }

    //创建isdp
    public void CreateisDP(string robotname, string goname,int isdp)
    {
        if (rbt[robotname].gos.ContainsKey(goname) == false)
        {
            string gotype = rbt[robotname].gos[goname].goType;
            if (gotype == "seivo" || gotype == "duoji")
            {
                rbt[robotname].gos[goname].isDP = isdp;
            }
        }
    }


    //创建物体Pos和Angle
    public void CreateGOPosAngle(string robotname, string goname, string gopos, string goangle, string goscale, string startpos, string gocolor, string hidego)
    {
        rbt[robotname].gos[goname].posAngle.pos = gopos;
        rbt[robotname].gos[goname].posAngle.angle = goangle;
        rbt[robotname].gos[goname].posAngle.scale = goscale;
        rbt[robotname].gos[goname].posAngle.startpos =startpos;
        rbt[robotname].gos[goname].color = gocolor;
        rbt[robotname].gos[goname].hidego = hidego;
    }

    public void CreatePosAngNoModel(string robotname, string goname, string gopos, string goangle,string goscale)
    {
        rbt[robotname].gos[goname].posAngle.pos = gopos;
        rbt[robotname].gos[goname].posAngle.angle = goangle;
        rbt[robotname].gos[goname].posAngle.scale = goscale;
    }



    //创建物体PrePos的ContactGO和ContactPoint
    public void CreatePrePosGOPoint(string robotname, string goname, string preposvalue, string contactgo, string contactpoint)
    {
        if (rbt[robotname]!=null&&rbt[robotname].gos != null && rbt[robotname].gos[goname] != null )
        {
            if (rbt[robotname].gos[goname].ppgp == null)
            {
               rbt[robotname].gos[goname].ppgp =new PrePosGoPoint();
            }
            rbt[robotname].gos[goname].ppgp.preposvalue = preposvalue;
            rbt[robotname].gos[goname].ppgp.contactgo = contactgo;
            rbt[robotname].gos[goname].ppgp.contactpoint = contactpoint;
        }
    }
    #endregion

    #region 删除
    //删除Robot的所有信息
    public void DeleteRobotData(string robotname)
    {
        if (rbt.ContainsKey(robotname)==true &&rbt[robotname]!=null)
        {
          rbt.Remove(robotname);
        }
    }

    //删除GO的所有信息
    public void DeleteGOData(string robotname, string goname)
    {
        //1.找到goname的PrePos物体
        string prepostemp="";

        if (rbt[robotname].gos != null && rbt[robotname].gos.Count > 0&&goname!=null)
        {

            foreach (var j in rbt[robotname].gos)
            {
                if (rbt[robotname].gos[goname] != null && rbt[robotname].gos[goname].ppgp != null && rbt[robotname].gos[goname].ppgp.contactgo != null)
                {
                    if (rbt[robotname].gos[goname].ppgp.contactgo == j.Key)    //j.Key为GO的name
                    {
                        prepostemp = j.Key;
                    }
                }
            }

            //5.删除goname对应的GO物体
            rbt[robotname].gos.Remove(goname);
        }
    }

    #endregion


    #region  修改
    //更新生成id在删除go的id后面的go的id
    public void resetGoId(string robotname,string deleteid)
    {
        List<string> gokeys = new List<string>();
        foreach (string goname in rbt[robotname].gos.Keys) 
        {
            gokeys.Add(goname);
        }
        if (gokeys != null && gokeys.Count > 0)
        {
            for (int i = 0; i < gokeys.Count; i++)
            {
                int goid = int.Parse(rbt[robotname].gos[gokeys[i]].goID);
                if (goid > int.Parse(deleteid))
                {
                    rbt[robotname].gos[gokeys[i]].goID = (goid - 1).ToString();

                }

            }
        }
    }
    #endregion


    #region 查询
    //查找所有机器人的名字
    public List<string> ExistRobotsNam()
    {
        List<string> allnam = new List<string>();
        if (rbt != null && rbt.Count > 0)
        {
            foreach(string temp in rbt.Keys)
            {
                allnam.Add(temp);
            }
        }
        return allnam;
    }

    //查询机器人ID
    public string FindRobotID(string robotname)
    {
        string robotidtempt = "";
        if (rbt!= null && rbt.ContainsKey(robotname))
        {
            robotidtempt = rbt[robotname].id;

        }

        return robotidtempt;
    }

    //查询所有机器人ID
    public List<string> FindAllRobotID()
    {
        List<string> ids=new List<string>();
        if (rbt != null && rbt.Count>0)
        {
            foreach(string temp in rbt.Keys)
            {
                if (ids.Contains(rbt[temp].id)==false)
                {
                  ids.Add(rbt[temp].id);
                }
            }
        }
        return ids;
    }

    //查询机器人的数据类型   自制，默认，下载
    public string FindDataType(string robotname)
    {
        string datatypetemp = "";
        if (rbt != null && rbt.ContainsKey(robotname))
        {
            datatypetemp = rbt[robotname].dataType;
        }

        return datatypetemp;
    }

    //通过机器人名字和dataType找到机器人(默认的，自制的可以同名)
    public RobotData FindRobotByNamType(string robotname,string datatype)
    {
        RobotData robotdatatemp = new RobotData();
        if (rbt != null && rbt.Count > 0)
        {
            foreach (string temp in rbt.Keys)
            {
                if (temp==robotname&&rbt[temp].dataType ==datatype)
                {
                    robotdatatemp = rbt[temp];
                }
            }
        }
        return robotdatatemp;
    }

    //查询搭建机器人的物体的总数量
    public int FindAllGOCount(string robotname)
    {
        allCount = 0;
        if (rbt[robotname].gos != null && rbt[robotname].gos.Count >0)
        {         
           allCount = rbt[robotname].gos.Count;
         }
        
        return allCount;
    }

    //查询所有物体的名字
    public List<string> FindAllGOName(string robotname)
    {
        List<string> allgonames = new List<string>();

        for (int i = 1; i <= rbt[robotname].gos.Count;i++ )      //i表示GO  id
        {
            foreach (string j in rbt[robotname].gos.Keys)
            {
                if (rbt[robotname].gos[j].goID == i.ToString() && allgonames.Contains(j)==false)
                {
                    //////Debug.Log("goID:" + i + ";j:" + j + ";gocount:" + rbt[robotname].gos.Count);
                    allgonames.Add(j);
                }
            }
        }

        return allgonames;
    }

    //查询机器人内物体的所有类型
    public List<string> FindAllGOTypes(string robotname)
    {
        List<string> allgotypes = new List<string>();

        for (int i = 1; i <= rbt[robotname].gos.Count; i++)      //i表示GO  id
        {
            foreach (string j in rbt[robotname].gos.Keys)
            {
                string gotypeTemp = rbt[robotname].gos[j].goType;
                if (allgotypes.Contains(gotypeTemp) == false)
                {

                    //Debug.Log("produce go:" + gotypeTemp);
                    allgotypes.Add(gotypeTemp);
                }
            }
        }

        return allgotypes;
    }

    //查询机器人内物体的颜色
    public Dictionary<string, Color> FindAllGOColor(string robotname)
    {
        Dictionary<string, Color> allgoColor = new Dictionary<string, Color>();

        for (int i = 1; i <= rbt[robotname].gos.Count; i++)      //i表示GO  id
        {
            foreach (string nam in rbt[robotname].gos.Keys)
            {
                string gotypeTemp = rbt[robotname].gos[nam].color;
                Color tempcolor = SingletonObject<PublicTools>.GetInst().StringToColor(gotypeTemp);
                if (allgoColor.ContainsValue(tempcolor) == false)
                {
                    allgoColor.Add(nam, tempcolor);
                }
            }
        }

        return allgoColor;
    }

    //
    public void FindGONumbersStart(string robotName)
    {
        GameObject tempgo;
        //GOTypeData gt = new GOTypeData();
       // typeTemp = gt.FindGOType();
        typeTemp = RobotMgr.Instance.FindAllGOTypes(robotName);

        
        foreach (string temp in typeTemp)
        {
            tempgo = Resources.Load("Prefab/Test4/GOPrefabs/" + temp) as GameObject;
            //Debug.Log("tempgo:"+tempgo.name);
            if (gonamnum.ContainsKey(temp) == false)
            {

                gonamnum.Add(temp, nameNum);
            }
        }

        //GoColors = RobotMgr.Instance.FindAllGOColor(robotName);
    }
    //查询对应类型模型的数量
    public Dictionary<string, int> FindGONumbers(string robotname)
    {
        Dictionary<string, int> gonumber = new Dictionary<string, int>();

        
        return gonumber;
    }

    ////获取所有djname
    //public List<string> FindGONambyType(string robotname,string gotype)
    //{
    //    if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
    //    {
    //        RobotDataMgr.Instance.ReadMsg(robotname);
    //    }
    //    List<string> dict = new List<string>();
    //    foreach (string child in RobotMgr.Instance.rbt[robotname].gos.Keys)
    //    {
    //        if (RobotMgr.Instance.rbt[robotname].gos[child].goType == gotype)
    //        {
    //            dict.Add(child);
    //        }
    //    }
    //    return dict;
    //}

    //找到isDP  判断是否是舵盘旋转
    public int FindisDP(string robotname,string goname)
    {
        int isdp = RobotMgr.Instance.rbt[robotname].gos[goname].isDP;
        return isdp;
    }

    //找到duoji编号最大的名字  duoji名字初始化编号
    public int InintDJNum(string robotname)
    {
        int max = 1;
        if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
        {
            RobotDataMgr.Instance.ReadMsg(robotname);
        }

        List<string> DJNames = FindDJName(robotname);

        if (DJNames != null && DJNames.Count > 0)
        {
            List<int> num = new List<int>();
            foreach (string nam in DJNames)
            {
                int n = GetNamNum(nam);

                if (n != null)
                {
                    num.Add(n);
                }
            }

            if (num != null && num.Count > 0)
            {
                foreach (int m in num)
                {
                    if (m > max)
                    {
                        max = m;
                    }
                }
            }
        }
        return max;
    }


    //找到物体的角度
    public void FindGOAngle(string robotname, string goname,out string goangle)
    {
        goangle = "";
        if (rbt[robotname].gos != null && rbt[robotname].gos[goname] != null && rbt[robotname].gos[goname].posAngle != null)
        {
            goangle = rbt[robotname].gos[goname].posAngle.angle;
        }
        else
        {
            goangle = "";
        }
    }


    //找到物体的位置和角度
    public void FindGOPosAngle(string robotname, string goname,out string gopos,out string goangle,out string goscale)
    {
        gopos = "";
        goangle = "";
        goscale = "";
        if (rbt[robotname].gos != null && rbt[robotname].gos[goname] != null && rbt[robotname].gos[goname].posAngle != null)
        {
            gopos = rbt[robotname].gos[goname].posAngle.pos;
            goangle = rbt[robotname].gos[goname].posAngle.angle;
            goscale = rbt[robotname].gos[goname].posAngle.scale;
        }
    }

    //找到物体的startpos
    public void FindStartPos(string robotname, string goname, out string startpos)
    {
        startpos = "";
        //Debug.Log("go:"+goname);
        if (rbt[robotname].gos != null && rbt[robotname].gos[goname] != null && rbt[robotname].gos[goname].posAngle != null)
        {
            startpos = rbt[robotname].gos[goname].posAngle.startpos;
        }
    }

    //找到物体的PrePos的ContactGO
    public string FindPrePosCG(string robotname, string goname)
    {
      string prePosNam="";
        if (rbt[robotname].gos[goname] != null&&rbt[robotname].gos[goname].ppgp!=null)
        {
            if (rbt[robotname].gos[goname].ppgp.contactgo != null)
            {
                prePosNam = rbt[robotname].gos[goname].ppgp.contactgo;
            }
        }
        return prePosNam;
    }

    //*****查找duoji的名称和角度   
    public Dictionary<string, int> FindDJData(string robotname)
    {
        string[] xangles = new string[3];
        Dictionary<string, int> dict = new Dictionary<string, int>();
        Dictionary<string, GO> tempgo = new Dictionary<string, GO>();   //<go的名字,GO>
        if (rbt[robotname].gos != null)
        {
            foreach (string temp in rbt[robotname].gos.Keys)
            {
                string gotypetemp=rbt[robotname].gos[temp].goType;
                if (rbt[robotname].gos[temp]!=null&&(gotypetemp == "seivo"||gotypetemp=="duoji"))
                {
                    tempgo.Add(temp, rbt[robotname].gos[temp]);
                }
                //Debug.Log("tempgo:"+tempgo.Count);
            }

            if (tempgo != null && tempgo.Count > 0)
            {
                foreach (string child in tempgo.Keys)
                {
                    xangles = tempgo[child].posAngle.angle.Split(' ', '"', '(', ',', ',', ',', ')', '"');

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
        }
        return dict;
    }

    public List<int> FindMotorID(string robotname)
    {
        List<int> ids = null;
        if (rbt[robotname].gos != null)
        {
            foreach (string temp in rbt[robotname].gos.Keys)
            {
                if (rbt[robotname].gos[temp] != null && rbt[robotname].gos[temp].goType == "motor")
                {
                    if (null == ids)
                    {
                        ids = new List<int>();
                    }
                    ids.Add((byte)rbt[robotname].gos[temp].motorID);
                }
            }


        }
        return ids;
    }
    
    public List<byte> FindMotorIds(string robotname)
    {
        List<byte> ids = null;
        if (rbt[robotname].gos != null)
        {
            foreach (string temp in rbt[robotname].gos.Keys)
            {
                if (rbt[robotname].gos[temp] != null && rbt[robotname].gos[temp].goType == "motor")
                {
                    if (null == ids)
                    {
                        ids = new List<byte>();
                    }
                    ids.Add((byte)rbt[robotname].gos[temp].motorID);
                }
            }

            
        }
        return ids;
    }
    //找到所有舵机物体
    public List<GameObject> FindAllDJGO(List<GameObject> AddedGOs)
    {
        List<GameObject> djs = new List<GameObject>();
        //舵机
        for (int i = 0; i < AddedGOs.Count; i++)
        {
            string nameTemp = AddedGOs[i].name;
            string goType = GoType(nameTemp);


            if (goType == "seivo" || goType == "duoji")
            {
                djs.Add(AddedGOs[i]);
            }

        }


        return djs;
    }

    public  DJClass FindDJPosGO(Transform djgo)
    {
        
            DJClass djcTemp = new DJClass();
            List<string> posIndex = NormalStringData.djPos();
            foreach (Transform djchild in djgo.GetComponentInChildren<Transform>())
            {

                if (posIndex.Contains(djchild.name))
                {
                    if (djcTemp.trans.ContainsKey(djchild.name) == false)
                    {
                        //Debug.Log("roname:"+djgo.name+";duojichild:"+djchild.name);
                        djcTemp.trans.Add(djchild.name, djchild);
                    
                    }
                }
            }


            return djcTemp;
    }

    //找到dpgo 舵盘模型；boxgo舵机主体旋转时需要的父物体模型
    public void FindDPBOXGO(string robotname, GameObject go,out GameObject dpgo,out GameObject boxgo)
    {
        dpgo = null;
        boxgo = null;
        foreach(Transform child in go.GetComponentsInChildren<Transform>())
        {
            string number = GONumbType(go.name);
            if(child.name=="DP")
            {

                //Debug.Log("DP:"+ child.name);

                dpgo = child.gameObject;

                dpgo.name = "DP-" + number;//舵盘变换名字
            }
            else if(child.name=="dppos")
            {
                //Debug.Log("dppos:" + child.name);

                boxgo = child.gameObject;
                boxgo.name = "dppos-" + number;//舵机主体旋转父物体变换名字
            }
			else if(child.name=="DP-"+number)
			{
                //Debug.Log("DP  num:" + child.name);

                dpgo = child.gameObject;
			}
			else if(child.name=="dppos-"+number)
			{
               // Debug.Log("dppos  num:" + child.name);

                boxgo = child.gameObject;
			}
        }

    }
    #endregion


    #region 判断有无物体

    //判断物体的位置和角度是否存入
    public bool HasPosAndAngle(string robotname, string goname)
    {
        if (rbt[robotname].gos[goname] != null)
        {
            if (rbt[robotname].gos[goname].posAngle!=null&&rbt[robotname].gos[goname].posAngle.pos != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    //判断物体是否有PrePos
    public bool HasPrePos(string robotname, string goname)
    {
        if (rbt[robotname].gos[goname] != null)
        {
            if (rbt[robotname].gos[goname].ppgp!=null&&rbt[robotname].gos[goname].ppgp.preposvalue != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }



    //判断是否有duoji
    public bool HasDJ(string robotname)
    {
        bool hasdj = false;
        if (rbt[robotname]!=null&&rbt[robotname].gos != null && rbt[robotname].gos.Count>0)
        {
            foreach (string tempchild in rbt[robotname].gos.Keys)
            {
                string gotype = rbt[robotname].gos[tempchild].goType;
                if (gotype == "seivo" || gotype == "duoji")
                {
                    hasdj = true;
                }
            }
        }
        return hasdj;
    }

    //判断是否有zhukonghe
    public bool Haszkh(string robotname)
    {
        bool haszkh = false;
        if (rbt[robotname] != null && rbt[robotname].gos != null && rbt[robotname].gos.Count > 0)
        {
            foreach (string tempchild in rbt[robotname].gos.Keys)
            {
                string gotype = rbt[robotname].gos[tempchild].goType;
                if (gotype == "mc")
                {
                    haszkh = true;
                }
            }
        }
        return haszkh;
    }

    public bool AllDJ(string robotname)
    {
        bool alldj = true;
        if (rbt[robotname] != null && rbt[robotname].gos != null && rbt[robotname].gos.Count > 0)
        {
            foreach (string tempchild in rbt[robotname].gos.Keys)
            {
                string gotype = rbt[robotname].gos[tempchild].goType;
                if (gotype != "seivo" || gotype == "duoji")
                {
                    alldj = false;
                    break;
                }
            }
        }
        return alldj;
    }

    //判断是否是舵机主体转动 是为true，若为舵盘，为false
    //如何是舵盘旋转需要反向(按顺时针方向转动)
    public static bool isDJDP(string robotname,string djname)
    {
        int isDPTemp = RobotMgr.Instance.rbt[robotname].gos[djname].isDP;
        bool isDJDP = false;
        if(isDPTemp==1)
        {
            isDJDP = true;
        }
        else
        {
            isDJDP = false;
        }
        return isDJDP;
    }
    #endregion


    #region djID

    //获取所有djname
    public List<string> FindDJName(string nameTemp)
    {
        if (RobotMgr.Instance.rbt.ContainsKey(nameTemp) == false)
        {
            RobotDataMgr.Instance.ReadMsg(nameTemp);
        }
        List<string> dict = new List<string>();
        foreach (string child in RobotMgr.Instance.rbt[nameTemp].gos.Keys)
        {
            string gotypetemp=RobotMgr.Instance.rbt[nameTemp].gos[child].goType ;
            if (gotypetemp == "seivo"||gotypetemp == "duoji")
            {
                dict.Add(child);
            }
        }
        return dict;
    }

    //获取所有djid
    public List<int> FindDJID(string nameTemp)
    {
        if (RobotMgr.Instance.rbt.ContainsKey(nameTemp) == false)
        {
            RobotDataMgr.Instance.ReadMsg(nameTemp);
        }
        List<int> dict = new List<int>();
        foreach (string child in RobotMgr.Instance.rbt[nameTemp].gos.Keys)
        {
            string gotypetemp = RobotMgr.Instance.rbt[nameTemp].gos[child].goType;
            if (gotypetemp == "seivo" || gotypetemp == "duoji")
            {
                int djid1 = RobotMgr.Instance.rbt[nameTemp].gos[child].djID;
                dict.Add(djid1);
            }
        }
        return dict;
    }

    //找到duoji编号最大的名字  duoji名字初始化编号
    public int InintDJID(string robotname)
    {
        int max = 0;
        if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
        {
            RobotDataMgr.Instance.ReadMsg(robotname);
        }

        List<int> djIDs = FindDJID(robotname);

        if (djIDs != null && djIDs.Count > 0)
        {
            foreach (int n in djIDs)
            {
                if (n > max)
                {
                    max = n;

                }
            }
        }

        return max;
    }

    //*****通过名字找id
    public int FinddjIDBydjNam(string robotname, string djname)
    {
        int djid = 0;
        if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
        {
            RobotDataMgr.Instance.ReadMsg(robotname);
        }
        if (djname != null && djname!="")
        {

          djid = RobotMgr.Instance.rbt[robotname].gos[djname].djID;
          //Debug.Log("goname:"+djname+";djid:"+djid);
        }
        return djid;
    }


    //*****通过djid找名字
    public string FindDJBydjID(string robotname, int djidTemp)
    {
        string djnam = string.Empty;
        if (RobotMgr.Instance == null || RobotMgr.Instance.rbt == null)
        {
            return djnam;
        }
        if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
        {
            RobotDataMgr.Instance.ReadMsg(robotname);
        }
        if (RobotMgr.Instance.rbt.ContainsKey(robotname))
        {
            if (RobotMgr.Instance.rbt[robotname].gos != null && RobotMgr.Instance.rbt[robotname].gos.Count > 0)
            {
                foreach (string gonamTemp in RobotMgr.Instance.rbt[robotname].gos.Keys)
                {
                    string gotype = RobotMgr.Instance.rbt[robotname].gos[gonamTemp].goType;
                    if ((gotype == "seivo" || gotype == "duoji") && RobotMgr.Instance.rbt[robotname].gos[gonamTemp].djID == djidTemp)
                    {
                        djnam = gonamTemp;
                    }
                }
            }
        }
        return djnam;
    }

    //*****通过djid找名字
    public string FindMotorByMotorID(string robotname, int motorIDTemp)
    {
        string motorName = "";
        if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
        {
            RobotDataMgr.Instance.ReadMsg(robotname);
        }
        if (RobotMgr.Instance.rbt[robotname].gos != null && RobotMgr.Instance.rbt[robotname].gos.Count > 0)
        {
            foreach (string gonamTemp in RobotMgr.Instance.rbt[robotname].gos.Keys)
            {
                string gotype = RobotMgr.Instance.rbt[robotname].gos[gonamTemp].goType;
                if ((gotype == "motor") && RobotMgr.Instance.rbt[robotname].gos[gonamTemp].motorID == motorIDTemp)
                {
                    motorName = gonamTemp;
                }
            }
        }
        return motorName;
    }
    //修改duijiid
    public void ReviseDJID(string robotname, int oriid, int newid)
    {
        if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
        {
            RobotDataMgr.Instance.ReadMsg(robotname);
        }

        string djname = FindDJBydjID(robotname, oriid);
        List<int> dict = RobotMgr.Instance.FindDJID(robotname);

        if (djname != null && djname != "")
        {
            if (!dict.Contains(newid))
            {
                RobotMgr.Instance.rbt[robotname].gos[djname].djID = newid;
            }
            else
            {
                string tempdjname = FindDJBydjID(robotname, newid);
                RobotMgr.Instance.rbt[robotname].gos[djname].djID = newid;
                RobotMgr.Instance.rbt[robotname].gos[tempdjname].djID = oriid;
            }
        }
    }

    //修改马达ID
    public void ReviseMotorID(string robotname, int oriid, int newid)
    {
        if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false)
        {
            RobotDataMgr.Instance.ReadMsg(robotname);
        }

        string motorname = FindMotorByMotorID(robotname, oriid);
        List<int> dict = RobotMgr.Instance.FindMotorID(robotname);

        if (motorname != null && motorname != "")
        {
            if (!dict.Contains(newid) == false)
            {
                RobotMgr.Instance.rbt[robotname].gos[motorname].motorID = newid;
            }
            else
            {
                string tempdjname = FindMotorByMotorID(robotname, newid);
                RobotMgr.Instance.rbt[robotname].gos[motorname].motorID = newid;
                RobotMgr.Instance.rbt[robotname].gos[tempdjname].motorID = oriid;
            }
        }
    }

   

    #endregion


    #region 变换父子关系--旋转舵机时使用
    public void ChangeParent(string robotname,List<GameObject> djs, List<GameObject> AddedGOs, Dictionary<string, DPBOX> alldpbox)
    {

        #region 1.舵机的父物体不是另一个舵机的DP时
        for (int i = 0; i < djs.Count; i++)
        {
            string nameTemp = djs[i].name;
            string parentgo = rbt[robotname].gos[nameTemp].ppgp.contactgo;

            if (parentgo != "")
            {
                string parentType = GoType(parentgo);
                if (parentType != "DP")
                {
                    for (int j = 0; j < AddedGOs.Count; j++)
                    {
                        if (AddedGOs[j].name == parentgo)
                        {
                            
                            alldpbox[nameTemp].box.transform.parent = null;
                            djs[i].transform.parent = null;
                            
                            //Debug.Log("box:" + alldpbox[nameTemp].box.name);
                            djs[i].transform.parent = alldpbox[nameTemp].box.transform; //舵机主体变换为旋转父物体的子物体                            
                            alldpbox[nameTemp].box.transform.parent = AddedGOs[j].transform;  //舵机旋转父物体变换为舵机总父物体的子物体

                            alldpbox[nameTemp].dp.transform.parent = null;
                            alldpbox[nameTemp].dp.transform.parent = AddedGOs[j].transform;  //舵机dp变换为舵机总父物体的子物体
                            //Debug.Log("djN1:"+djs[i].name);
                            //djs.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
      #endregion

        #region 2.舵机的父物体是另一个舵机的DP时
        for (int i = 0; i < djs.Count; i++)
        {
            string nameTemp = djs[i].name;
            string parentgo = rbt[robotname].gos[nameTemp].ppgp.contactgo;
            if (parentgo != "")
            {
                string parentType = GoType(parentgo);
                if (parentType == "DP")
                {
                    foreach (string djTemp in alldpbox.Keys)
                    {
                        if (alldpbox[djTemp].dp.name == parentgo)
                        {
                            djs[i].transform.parent = null;
                            alldpbox[nameTemp].box.transform.parent = null;
                            djs[i].transform.parent = alldpbox[nameTemp].box.transform; //舵机主体变换为旋转父物体的子物体                            
                            alldpbox[nameTemp].box.transform.parent = alldpbox[djTemp].dp.transform;  //舵机旋转父物体变换为舵机总父物体的子物体

                            alldpbox[nameTemp].dp.transform.parent = null;
                            alldpbox[nameTemp].dp.transform.parent = alldpbox[djTemp].dp.transform;  //舵机dp变换为舵机总父物体的子物体
                            //Debug.Log("djN2:" + djs[i].name);
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region 3.非舵机
        for (int i = 0; i < AddedGOs.Count; i++)
        {
            if (djs.Contains(AddedGOs[i]) == false)
            {
                string nameTemp = AddedGOs[i].name;
				//Debug.Log("djtemp:"+nameTemp);
                string parentgo = rbt[robotname].gos[nameTemp].ppgp.contactgo;

                if (parentgo != "")
                {
                    string parentType = GoType(parentgo);
                    if (parentType != "DP")
                    {
                        for (int j = 0; j < AddedGOs.Count; j++)
                        {
                            if (AddedGOs[j].name == parentgo)
                            {
                                AddedGOs[i].transform.parent = null;
                                AddedGOs[i].transform.parent = AddedGOs[j].transform;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (string djTemp in alldpbox.Keys)
                        {
                            if (alldpbox[djTemp].dp.name == parentgo)
                            {
                                AddedGOs[i].transform.parent = null;
                                AddedGOs[i].transform.parent = alldpbox[djTemp].dp.transform;  //舵机旋转父物体变换为舵机总父物体的子物体
                                break;
                            }
                        }
                    }

                }
            }
        }
        #endregion
    }
    #endregion

    #region 恢复默认父子关系
    public void ResetParent(string robotname, GameObject oriGO)
    {

        for (int j = 0; j < alldj.Count; j++)
        {
            string nameTemp = alldj[j].name;
            
            alldj[j].transform.parent = null;
            alldpbox[nameTemp].dp.transform.parent = null;
            alldpbox[nameTemp].dp.transform.parent = alldj[j].transform;

            alldpbox[nameTemp].box.transform.parent = null;
            alldpbox[nameTemp].box.transform.parent = alldj[j].transform;

            alldj[j].transform.parent = oriGO.transform;
            
        }
        for (int i = 0; i < AddedAllGOs.Count; i++)
        {
            if (alldj.Contains(AddedAllGOs[i]) == false)
            {
                AddedAllGOs[i].transform.parent = null;
                AddedAllGOs[i].transform.parent = oriGO.transform;
            }

        }
 

    }

    //恢复舵机或者舵盘的坐标和角度
    public void ResetDJDPPAOld()
    {
        List<string> dpboxName = new List<string>();
        foreach (string key in alldpbox.Keys)
        {
            dpboxName.Add(key);
        }
        for (int i = dpboxName.Count - 1; i >= 0; i--)
        {
            alldpbox[dpboxName[i]].dp.transform.localPosition = alldpbox[dpboxName[i]].dplocalPos;
            alldpbox[dpboxName[i]].dp.transform.localEulerAngles = alldpbox[dpboxName[i]].dplocalAngle;

            alldpbox[dpboxName[i]].box.transform.localPosition = alldpbox[dpboxName[i]].boxlocalPos;
            alldpbox[dpboxName[i]].box.transform.localEulerAngles = alldpbox[dpboxName[i]].boxlocalAngle;

        }

        for (int i = 0; i < AddedAllGOs.Count; i++)
        {
            string goname = AddedAllGOs[i].name;

            if (goPosAll.ContainsKey(goname))
            {
                AddedAllGOs[i].transform.localPosition = goPosAll[goname];
                AddedAllGOs[i].transform.localEulerAngles = goAngleAll[goname];
            }
        }

    }
    #endregion


    #region 执行动作模式下，舵机初始角度
    public Dictionary<string, RotateDJGO> FindDJOriAngle(string robotName)
    {
        Dictionary<string, RotateDJGO> djRotate = new Dictionary<string, RotateDJGO>();
        foreach (GameObject temp in alldj)
        {
            string pickgo = temp.name;
           // Debug.Log("4343dfsfdfd:"+pickgo);
            RotateDJGO rdjg = new RotateDJGO();
      
            try
            {
                int isdp = RobotMgr.Instance.rbt[robotName].gos[pickgo].isDP;
                if (isdp == 1)
                {
                    rdjg.rotateGo = alldpbox[pickgo].dp;
                    rdjg.oriRotation = alldpbox[pickgo].dp.transform.eulerAngles;
                }
                else
                {
                    rdjg.rotateGo = alldpbox[pickgo].box;
                    rdjg.oriRotation = alldpbox[pickgo].box.transform.eulerAngles;
                }

                //GameObject origoT = GameObject.Find("oriGO");
                string namTt = rdjg.rotateGo.name;
                GameObject ttttt = GameObject.Find(namTt).gameObject;

               // Debug.Log("555dfsfdfd:" + rdjg.rotateGo + ";oriAngle:" + rdjg.oriRotation);
                djRotate.Add(pickgo, rdjg);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }
        return djRotate;
    }
    #endregion

    //设置模型的材质和颜色
    public static void SettingMatColor(GameObject go, Material mat,Color ori)
    {
        if (go != null)
        {

            Transform grouptemp = null;
            foreach (Transform child in go.transform.GetComponentInChildren<Transform>())
            {
                if (child.name == "Group")
                {
                    grouptemp = child;

                    break;
                }
            }
            if (grouptemp != null)
            {
                //Debug.Log("origo:" + ori);
                foreach (Transform childL in grouptemp.transform.GetComponentInChildren<Transform>())
                {
                    if (null != childL.transform.GetComponent<Renderer>() && null != childL.transform.GetComponent<Renderer>().material)
                    {
                        //childL.transform.renderer.sharedMaterial = mat;
                        //childL.transform.renderer.sharedMaterial.color = ori;
                        //childL.transform.renderer.sharedMaterial.SetColor("_SpecColor", ori);

                        childL.transform.GetComponent<Renderer>().material = mat;
                        childL.transform.GetComponent<Renderer>().material.color = ori;
                        childL.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", ori);
                    }
                }

            }
            else
            {
                // Debug.Log("origo:" + ori);
                if (null != go.transform.GetComponent<Renderer>() && null != go.transform.GetComponent<Renderer>().material)
                {
                    //go.transform.renderer.sharedMaterial = mat;
                    //go.transform.renderer.sharedMaterial.color = ori;
                    //go.transform.renderer.sharedMaterial.SetColor("_SpecColor", ori);


                    go.transform.GetComponent<Renderer>().material = mat;
                    go.transform.GetComponent<Renderer>().material.color = ori;
                    go.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", ori);
                }
            }
        }
    }

    //设置模型的颜色
    public static void SettingColor(GameObject go, Color ori)
    {
        if (go != null)
        {
            
            Transform grouptemp = null;
            foreach (Transform child in go.transform.GetComponentInChildren<Transform>())
            {
                if (child.name == "Group")
                {
                    grouptemp = child;
                    
                    break;
                }
            }
            if (grouptemp != null)
            {
                //Debug.Log("origo:" + ori);
                foreach (Transform childL in grouptemp.transform.GetComponentInChildren<Transform>())
                {
                    if (null != childL.transform.GetComponent<Renderer>() && null != childL.transform.GetComponent<Renderer>().material)
                        childL.transform.GetComponent<Renderer>().material.color=ori;
                    childL.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", ori);
                }

            }
            else
            {
             
                if (null != go.transform.GetComponent<Renderer>() && null != go.transform.GetComponent<Renderer>().material)
                {
                    go.transform.GetComponent<Renderer>().material.color = ori;
                    go.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", ori);
                }
                
            }
        }
    }

    public static void SettingLineColor(GameObject go)
    {
        if (go != null)
        {

            if (go.transform.childCount>0)
            {
                //Debug.Log("origo:" + ori);
                string nameT = go.name;
                if (nameT.Contains("line")||nameT.Contains("lnA")||nameT.Contains("lnB"))
                {
                    foreach (Transform childL in go.transform.GetComponentsInChildren<Transform>())
                    {
                        if (null != childL.transform.GetComponent<Renderer>() && null != childL.transform.GetComponent<Renderer>().material)
                        {
                            string nameChild = childL.name;
                            if (nameChild.Contains("Object"))
                            {
                                childL.transform.GetComponent<Renderer>().material.color = Color.white;
                                childL.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", Color.white);
                            }
                            else if (nameChild.Contains("Line"))
                            {
                                childL.transform.GetComponent<Renderer>().material.color = Color.black;
                                childL.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", Color.black);
                            }
                        }

                    }
                }
                else
                {
                    foreach (Transform childL in go.transform.GetComponentsInChildren<Transform>())
                    {
                        if (null != childL.transform.GetComponent<Renderer>() && null != childL.transform.GetComponent<Renderer>().material)
                        {
                            string nameChild = childL.name;
                            if (nameChild.Contains("Object"))
                            {
                                childL.transform.GetComponent<Renderer>().material.color = OriColor;
                                childL.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", OriColor);
                            }
                            else if (nameChild.Contains("Line"))
                            {
                                childL.transform.GetComponent<Renderer>().material.color = Color.black;
                                childL.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", Color.black);
                            }
                        }

                    }
                }


            }
            else
            {
                // Debug.Log("origo:" + ori);
                if (null != go.transform.GetComponent<Renderer>() && null != go.transform.GetComponent<Renderer>().material)
                {
                    go.transform.GetComponent<Renderer>().material.color = OriColor;
                    go.transform.GetComponent<Renderer>().material.SetColor("_SpecColor", OriColor);
                }
            }
        }
    }


    #region 选中物体的颜色

    //确认生成物体后，使模型的shader为Diffuse
    public static void ChangeDiffuse(GameObject go,Color ori)
    {
        if (go != null)
        {
            if(go.name !="DP")   //go不为舵盘
            {
                foreach (Transform goDiffuse in go.transform.GetComponentInChildren<Transform>())
                {
                    if (goDiffuse.name == "Box")
                    {
                        goDiffuse.GetComponent<Renderer>().material.shader = Shader.Find("Outlined/Silhouetted Bumped Diffuse");
                        goDiffuse.transform.GetComponent<Renderer>().material.color = ori;
                    }
                    else if (goDiffuse.name == "DP")
                    {
                        foreach (Transform goDif in goDiffuse.transform.GetComponentInChildren<Transform>())
                        {
                            goDif.GetComponent<Renderer>().material.shader = Shader.Find("Outlined/Silhouetted Bumped Diffuse");
                            goDif.transform.GetComponent<Renderer>().material.color = ori;
                        }
                    }
                    else if (goDiffuse.name == "Group")
                    {
                        foreach (Transform goDiffuseChild in goDiffuse.transform.GetComponentInChildren<Transform>())
                        {
                            if (goDiffuseChild.name != "DP")
                            {
                                goDiffuseChild.GetComponent<Renderer>().material.shader = Shader.Find("Outlined/Silhouetted Bumped Diffuse");
                                goDiffuseChild.transform.GetComponent<Renderer>().material.color = ori;
                            }
                            else
                            {
                                foreach (Transform goDif in goDiffuseChild.transform.GetComponentInChildren<Transform>())
                                {
                                    goDif.GetComponent<Renderer>().material.shader = Shader.Find("Outlined/Silhouetted Bumped Diffuse");
                                    goDif.transform.GetComponent<Renderer>().material.color = ori;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Transform goDiffuse in go.transform.GetComponentInChildren<Transform>())
                {
                    goDiffuse.GetComponent<Renderer>().material.shader = Shader.Find("Specular");
                    goDiffuse.transform.GetComponent<Renderer>().material.color = ori;
                }
            }
        }
    }

    //判断字符串是否为纯数字
    public static bool bolNum(string temp)
    {
        for (int i = 0; i < temp.Length; i++)
        {
            byte tempByte = Convert.ToByte(temp[i]);
            if ((tempByte < 48) || (tempByte > 57))
                return false;
        }

        return true;
    }

    //只获取duoji名字后的编号
    string[] nam;
    public int GetNamNum(string duojiname)
    {
        
        if(duojiname.Contains("-"))
        {
            nam = duojiname.Split('-');
        }
        else if (duojiname.Contains("_"))
        {
            nam = duojiname.Split('_');
        }
        int i = int.Parse(nam[1]);
        return i;
    }
    #endregion


    #region normal operation

    #region 机器人的名字加上数据类型的处理(自制，默认，下载)
    //返回带类型的名字
    public static string NameWithType(string namenotype,string datatype)
    {
        string namewithtype = namenotype + "_" + datatype;
        return namewithtype;
    }

    //返回不带类型的名字
    public static string NameNoType(string namewithtype)
    {
        if (string.IsNullOrEmpty(namewithtype))
        {
            return namewithtype;
        }
        int posnum=namewithtype.LastIndexOf("_");
        if (-1 == posnum)
        {
            return namewithtype;
        }
        string namenotype = namewithtype.Substring(0,posnum);  //如：12_playerdata   取:12
        return namenotype;
    }

    //找到机器人的类型(自制，默认，下载)
    public static string DataType(string namewithtype)
    {
        int posnum = namewithtype.LastIndexOf("_");
        int leng = namewithtype.Length - posnum;
        string namenotype = namewithtype.Substring(posnum + 1, leng - 1);  //如：12_playerdata   取:playerdata
        return namenotype;
    }

    //找到机器人的物体名称中用的符号
    public static string GOSymbolType(string namewithtype)
    {
        string type=null;
        if (namewithtype.Contains("-"))
        {
            type = "-";
        }
        else if (namewithtype.Contains("_"))
        {
            type = "_";
        }

        return type;
    }

    //找到机器人的物体的编号
    public static string GONumbType(string namewithtype)
    {
        int posnum=0;
        int leng = 0;
        string numberTemp = null; 
        if(namewithtype.Contains("-"))
        {
            posnum = namewithtype.LastIndexOf("-");
            leng = namewithtype.Length - posnum;
            numberTemp = namewithtype.Substring(posnum + 1, leng - 1);  //如：seivo-001   取:001
        }
        else if (namewithtype.Contains("_"))
        {
            posnum = namewithtype.LastIndexOf("_");
            leng = namewithtype.Length - posnum;
            numberTemp = namewithtype.Substring(posnum + 1, leng - 1);  //如：seivo-001   取:001
        }
        
        return numberTemp;
    }

	//查找物体的类型属性
    public static string GoRealType(string robotname,string goname)
    {
        string gotype = RobotMgr.Instance.rbt[robotname].gos[goname].goType;
        return gotype;
    }
	
    //找到机器人的物体的名称前缀
    public static string GoType(string namewithtype)
    {
        string gotype="";
        if (namewithtype.Contains("-"))
        {
            int posnum = namewithtype.LastIndexOf("-");
            gotype = namewithtype.Substring(0, posnum);  //如：seivo-001   取:seivo
        }
        else if (namewithtype.Contains("_"))
        {
            int posnum = namewithtype.LastIndexOf("_");
            gotype = namewithtype.Substring(0, posnum);  //如：seivo-001   取:seivo
        }
        else
        {
            
            gotype = namewithtype;
        }
        
        return gotype;
    }

    //找到机器人的物体的名称前缀
    public static string PartPicType(string namewithtype)
    {
        string gotype = namewithtype;
        if (namewithtype.Contains("-"))
        {
            int posnum = namewithtype.LastIndexOf("-");
            gotype = namewithtype.Substring(0, posnum);  //如：seivo-001   取:seivo
        }
        
        return gotype;
    }

    //找到舵盘花型   seivo-001:Star   --动画文件中
    public static string FindDJShape(string namewithtype,string index)
    {
        int posnum = namewithtype.LastIndexOf(index);
        int leng = namewithtype.Length - posnum;
        string numberTemp = namewithtype.Substring(posnum + 1, leng - 1);  //如：seivo-001:Star   取:Star
        return numberTemp;
    }

    //查找舵机名称--动画文件中
    public static string FindDJName(string namewithtype, string index)
    {
        string gotype = "";
        if (namewithtype.Contains(index))
        {
            int posnum = namewithtype.LastIndexOf(index);
            gotype = namewithtype.Substring(0, posnum);  //如：seivo-001:Star   取:seivo-001
        }
        else
        {
            gotype = namewithtype;
        }

        return gotype;
    }
    #endregion

    #endregion

    #region 修改带骨骼动画的线中的joint模型的名称
    public static void ChangeJointsName(GameObject linego)
    {
        string nameTemp = linego.name;
        string numType=GONumbType(nameTemp);   //名称需要加的编号
        string symbolType = GOSymbolType(nameTemp);  //名称需要加的符号"_"/"-"

        List<Transform> joints = new List<Transform>();
        
        foreach(Transform child in linego.transform.GetComponentsInChildren<Transform>())
        {
            if(child.name.Contains("joint"))
            {
                joints.Add(child);
            }
        }

        for (int i = joints.Count - 1; i >= 0;i--)
        {
            joints[i].name += symbolType + numType;
        }
    }
    #endregion

    #region 显示
    //显示舵机，传感器ID
    public void ShowID(GameObject goTemp,int idTemp,Dictionary<string, Texture> djIDTexture)
    {
        foreach (Transform child in goTemp.transform.GetComponentsInChildren<Transform>())
        {
            if(child.name=="Num1")
            {
                if (idTemp < 10)
                {
                    child.GetComponent<Renderer>().material.mainTexture=djIDTexture["0"];
                }
                else
                {
                    int idFirst = idTemp / 10;
                    child.GetComponent<Renderer>().material.mainTexture = djIDTexture[idFirst.ToString()];
                }
            }

            if (child.name == "Num2")
            {
                int idSec = idTemp % 10;
                child.GetComponent<Renderer>().material.mainTexture = djIDTexture[idSec.ToString()];
            }
        }
    }

    /// <summary>
    /// 关闭舵机上的位置标识
    /// </summary>
    /// <param name="AddedAllGOsT"></param>
    public static void CloseDJPosShow(List<GameObject> AddedAllGOsT)
    {
       // foreach (GameObject child in AddedAllGOs)
        for (int i = 0; i < AddedAllGOsT.Count;i++ )
        {
            string gotemp = RobotMgr.GoType(AddedAllGOsT[i].name);
            List<string> posIndex = NormalStringData.djPos();
            if (gotemp == "seivo")
            {
                List<Transform> djpos = new List<Transform>();

                foreach (Transform child in AddedAllGOsT[i].GetComponentInChildren<Transform>())
                {
                    if(posIndex.Contains(child.name))
                    {
                        djpos.Add(child);
                    }
                }

                for (int j = 0; j < djpos.Count;j++ )
                {
                    if (djpos[j].gameObject.activeInHierarchy == true)
                    {
                        djpos[j].gameObject.SetActive(false);
                    }
                }
     
            }

        }
    }

    /// <summary>
    /// 打开舵机位置上的标识
    /// </summary>
    /// <param name="AddedAllGOsT"></param>
    public static void OpenDJPosShow(List<GameObject> AddedAllGOsT)
    {
        // foreach (GameObject child in AddedAllGOs)
        for (int i = 0; i < AddedAllGOsT.Count; i++)
        {
            string gotemp = RobotMgr.GoType(AddedAllGOsT[i].name);
            List<string> posIndex = NormalStringData.djPos();
            if (gotemp == "seivo")
            {
                List<Transform> djpos = new List<Transform>();

                foreach (Transform child in AddedAllGOsT[i].GetComponentInChildren<Transform>())
                {
                    if (posIndex.Contains(child.name))
                    {
                        djpos.Add(child);
                    }
                }

                for (int j = 0; j < djpos.Count; j++)
                {
                    if(djpos[j].gameObject.activeInHierarchy==false)
                    {
                        djpos[j].gameObject.SetActive(true);
                    }
                    
                }

            }

        }
    }

    /// <summary>
    /// 打开舵机位置上的标识
    /// </summary>
    /// <param name="AddedAllGOsT"></param>
    public static void OpenDJPosShowSec(Dictionary<string, GameObject> AllGOs)
    {
        List<GameObject> AddedAllGOsT = new List<GameObject>();
        foreach(string key in AllGOs.Keys)
        {
            if(AddedAllGOsT.Contains(AllGOs[key])==false)
            {
                AddedAllGOsT.Add(AllGOs[key]);
            }
        }
        // foreach (GameObject child in AddedAllGOs)
        for (int i = 0; i < AddedAllGOsT.Count; i++)
        {
            string gotemp = RobotMgr.GoType(AddedAllGOsT[i].name);
            List<string> posIndex = NormalStringData.djPos();
            if (gotemp == "seivo")
            {
                List<Transform> djpos = new List<Transform>();

                foreach (Transform child in AddedAllGOsT[i].GetComponentInChildren<Transform>())
                {
                    if (posIndex.Contains(child.name))
                    {
                        djpos.Add(child);
                    }
                }

                for (int j = 0; j < djpos.Count; j++)
                {
                    if (djpos[j].gameObject.activeInHierarchy == false)
                    {
                        djpos[j].gameObject.SetActive(true);
                    }

                }

            }

        }
    }
    #endregion

    #region normal handle

    //查找子物体
    public GameObject FindChildGO(GameObject paGO,string nameT)
    {
        GameObject childGOT = null;
        childGOT = paGO.transform.Find(nameT).gameObject;
        return childGOT;
    }

    //查找对应翻译
    public string FindString(string notT)
    {
        string notFind = LauguageTool.GetIns().GetText(notT);
        return notFind;
    }

    //找到物体的UISprite组件
    public UISprite FindSpriteComponent(Transform m)
    {
        UISprite spriteTemp = null;
        spriteTemp = m.GetComponent<UISprite>();
        return spriteTemp;
    }
    #endregion

    //基本数据清空
    public void ClearBaseData()
    {
        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "ClearBaseData:");
         if (AddedAllGOs != null){  AddedAllGOs.Clear();}

        if (alldpbox != null) {  alldpbox.Clear();}

        if (alldj != null)  {  alldj.Clear();}

        if (goPosAll != null){  goPosAll.Clear();}

        if (goAngleAll != null){  goAngleAll.Clear();}

        if (goScaleAll != null){  goScaleAll.Clear();}

        if (hideGOs != null) { hideGOs.Clear(); }
    }

    //从Unity进入社区时，需要处理的数据
    public void GoToCommunity()
    {
        try
        {
            GameObject origo = GameObject.Find("oriGO");
            if (origo != null) { GameObject.Destroy(origo); }

            GameObject mVCenter = GameObject.Find("MVCenter");
            if (mVCenter != null) { GameObject.Destroy(mVCenter); };

            if (AddedAllGOs != null) { AddedAllGOs.Clear(); }

            if (alldpbox != null) { alldpbox.Clear(); }

            if (alldj != null) { alldj.Clear(); }

            if (goPosAll != null) { goPosAll.Clear(); }

            if (goAngleAll != null) { goAngleAll.Clear(); }

            if (goScaleAll != null) { goScaleAll.Clear(); }

            if (prefabgos != null) { prefabgos.Clear(); }


            if (rbtAnim != null) { rbtAnim.Clear(); }
            if (jubuPics != null) { jubuPics.Clear(); }
            if (anims != null) { anims.Clear(); }
            if (outPics != null) { outPics.Clear(); }
            if (hideGOs != null) { hideGOs.Clear(); }

            //if (AnimAssets.Instance != null) { GameObject.Destroy(AnimAssets.Instance); }
            SceneMgrTest.Instance.LastScene = SceneType.StartScene;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "GoToCommunity end");
    }
}

//舵机转动时，选择的物体和其初始角度
public class RotateDJGO
{
    public GameObject rotateGo { get;set;}  //舵机中转动的部分(舵盘或舵机主题)
    public Vector3 oriRotation { get; set; }//初始角度
}