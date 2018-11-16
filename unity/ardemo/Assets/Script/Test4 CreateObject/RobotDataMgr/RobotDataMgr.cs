/*
 * 作用：将DataMgr的数据存入xml文档，或者将xml文档存入DataMgr中的变量中。
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotDataMgr
{
    private static RobotDataMgr _instance;

    static object sLock = new object();
    public static RobotDataMgr Instance      //单例
    {
        get
        {
            if (null == _instance)
            {
                lock (sLock)
                {
                    if (_instance == null)
                    {

                        _instance = new RobotDataMgr();
                    }
                }
            }
            return _instance;
        }
    }

    private RobotDataMgr()
    {

    }

    //修改duoji名字   重置duoji的ID即 XML文档中duiji的名字;orrid duoji原始名称；newid duoji新名字
    public void ReviseDJid(string nameTemp)
    {
        string nameNoType = RobotMgr.NameNoType(nameTemp);
        RecordContactInfo.Instance.createPathInit(nameNoType, RobotMgr.Instance.rbt[nameTemp].id, RobotMgr.Instance.rbt[nameTemp].dataType,RobotMgr.Instance.rbt[nameTemp].level);   //重新加载一下xml文档
        CreatGO(nameTemp);
        RobotMgr.Instance.djstartnum = RobotMgr.Instance.InintDJNum(nameTemp);
        RobotMgr.Instance.djstartid = RobotMgr.Instance.InintDJID(nameTemp);
        RecordContactInfo.Instance.Save(false);
    }

    //将数据写入XML文档
    public void CreatGO(string nameTemp)
    {

        foreach (string temp in RobotMgr.Instance.rbt[nameTemp].gos.Keys)
        {

            string id = RobotMgr.Instance.rbt[nameTemp].gos[temp].goID;
            string type = RobotMgr.Instance.rbt[nameTemp].gos[temp].goType;
            int djidtemp = RobotMgr.Instance.rbt[nameTemp].gos[temp].djID;
            string motorid = RobotMgr.Instance.rbt[nameTemp].gos[temp].motorID.ToString();
            string djid = djidtemp.ToString();
            int isdptemp = RobotMgr.Instance.rbt[nameTemp].gos[temp].isDP;
            string isdp = isdptemp.ToString();
            RecordContactInfo.Instance.AddGO(djid,motorid,id, type,temp,isdp);   //添加(goid,gotype,goname)

            string goPos = RobotMgr.Instance.rbt[nameTemp].gos[temp].posAngle.pos;
            string goAngle = RobotMgr.Instance.rbt[nameTemp].gos[temp].posAngle.angle;
            string startPos = RobotMgr.Instance.rbt[nameTemp].gos[temp].posAngle.startpos;
            string goScale= RobotMgr.Instance.rbt[nameTemp].gos[temp].posAngle.scale;
            string goColor = RobotMgr.Instance.rbt[nameTemp].gos[temp].color;
            string hidego = RobotMgr.Instance.rbt[nameTemp].gos[temp].hidego;
            RecordContactInfo.Instance.AddGOPosAngle(temp, goPos, goAngle, goScale, startPos, goColor, hidego);    //保存(gopos,goangle

            //保存prePos(contactgo,contactpoint)
            if (RobotMgr.Instance.rbt[nameTemp].gos[temp].ppgp.preposvalue != null)
            {
                string prevalue = RobotMgr.Instance.rbt[nameTemp].gos[temp].ppgp.preposvalue;
                string newgo = RobotMgr.Instance.rbt[nameTemp].gos[temp].ppgp.contactgo;
                string conpoint = RobotMgr.Instance.rbt[nameTemp].gos[temp].ppgp.contactpoint;
                if(newgo !=null)
                {
                    RecordContactInfo.Instance.AddPos(temp, "PrePos", prevalue);
                    RecordContactInfo.Instance.AddContactInf(temp, "PrePos", prevalue, newgo, conpoint);
                }
            }
        }
    }

    ////将旧的robotData写到新的robot中(RobotMgr.Instance.rbt)
    //public void CreatGOTOClass(string oldname,string newname)
    //{
    //    foreach (string temp in RobotMgr.Instance.rbt[oldname].gos.Keys)
    //    {
    //        string id = RobotMgr.Instance.rbt[oldname].gos[temp].goID;
    //        string type = RobotMgr.Instance.rbt[oldname].gos[temp].goType;

    //       // RecordContactInfo.Instance.AddGO(id, type, temp);   //添加(goid,gotype,goname)
    //        int djid = RobotMgr.Instance.rbt[oldname].gos[temp].djID;

    //        RobotMgr.Instance.CreateGO(newname,temp,id,type,djid);


    //        if (type == "seivo"||type == "duoji")
    //        {
    //            string isdptemp = RecordContactInfo.Instance.FindisDP(temp);
    //            int isdp = int.Parse(isdptemp);
    //            RobotMgr.Instance.rbt[newname].gos[temp].isDP = isdp;
    //        }

    //        string goPos = RobotMgr.Instance.rbt[oldname].gos[temp].posAngle.pos;
    //        string goAngle = RobotMgr.Instance.rbt[oldname].gos[temp].posAngle.angle;
    //        string goScale = RobotMgr.Instance.rbt[oldname].gos[temp].posAngle.scale;
    //        string startPos = RobotMgr.Instance.rbt[oldname].gos[temp].posAngle.startpos;
    //        string goColor = RobotMgr.Instance.rbt[oldname].gos[temp].color;
    //        string hidego = RobotMgr.Instance.rbt[oldname].gos[temp].hidego;
    //       // RecordContactInfo.Instance.AddGOPosAngle(temp, goPos, goAngle);    //保存(gopos,goangle
    //        RobotMgr.Instance.CreateGOPosAngle(newname, temp, goPos, goAngle,goScale,startPos,goColor,hidego);

    //        if (RobotMgr.Instance.rbt[oldname].gos[temp] != null)
    //        {
    //            //保存prePos(contactgo,contactpoint)
    //            if (RobotMgr.Instance.rbt[oldname].gos[temp].ppgp!=null&&RobotMgr.Instance.rbt[oldname].gos[temp].ppgp.preposvalue != null)
    //            {
    //                string prevalue = RobotMgr.Instance.rbt[oldname].gos[temp].ppgp.preposvalue;
    //                string newgo = RobotMgr.Instance.rbt[oldname].gos[temp].ppgp.contactgo;
    //                string conpoint = RobotMgr.Instance.rbt[oldname].gos[temp].ppgp.contactpoint;
                 
    //                RobotMgr.Instance.CreatePrePosGOPoint(newname, temp, prevalue, newgo, conpoint);
    //            }
    //        }
    //    }
    //}

    #region 无模型机器人
    //将旧的robotData写到新的robot中(RobotMgr.Instance.rbt)
    int gonum = 1;
    Vector3 objPos = Vector3.zero;
    Vector3 objAngle = Vector3.zero;
    Vector3 objScale;

    public void CreatRobotWithoutModel(string nameTemp, ReadMotherboardDataMsgAck data)
    {
       
        if (nameTemp != null && RobotMgr.Instance.rbt.ContainsKey(nameTemp) == false)
        {
            gonum = 1;
            string robotid = CreateID.CreateRobotID();
            string datatype="playerdata";
            string level = "level";
            RobotMgr.Instance.CreateRobot(nameTemp, robotid, datatype,level);

            Vector3 oriPos = new Vector3(-1.65f,0.8f,0);
            if (data.ids != null && data.ids.Count > 0) 
            { 
                foreach(byte tempid in data.ids)
                {
                    string id = gonum.ToString();
                    int djid = tempid;
                    string temp = "seivo-" + gonum;

                    objPos = oriPos+new Vector3((gonum % 5) * 0.45f ,-(gonum / 5) * 0.45f ,0.0f);

                    objAngle = new Vector3(90,90,0);
                    objScale = new Vector3(90, 90, 0);


                    string type = "seivo";
                    RobotMgr.Instance.CreateGO(nameTemp, temp, id, type, djid);

                    string post = "(" + objPos.x + "," + objPos.y + "," + objPos.z + ")";
                    string anglet = "(" + objAngle.x + "," + objAngle.y + "," + objAngle.z + ")";
                    string scale = "(" + objScale.x + "," + objScale.y + "," + objScale.z + ")";
                    RobotMgr.Instance.CreatePosAngNoModel(nameTemp, temp, post, anglet,scale);
                    gonum++;
                }
            }
            if (data.motors != null && data.motors.Count > 0)
            {
                foreach (byte tempid in data.motors)
                {
                    string id = gonum.ToString();
                    int motorID = tempid;
                    string temp = "motor-" + gonum;


                    objPos = oriPos + new Vector3((gonum % 5) * 0.45f, -(gonum / 5) * 0.45f, 0.0f);

                    objAngle = new Vector3(90, 90, 0);
                    objScale = new Vector3(90, 90, 0);


                    string type = "motor";
                    RobotMgr.Instance.CreateGO(nameTemp, temp, id, type, motorID);

                    string post = "(" + objPos.x + "," + objPos.y + "," + objPos.z + ")";
                    string anglet = "(" + objAngle.x + "," + objAngle.y + "," + objAngle.z + ")";
                    string scale = "(" + objScale.x + "," + objScale.y + "," + objScale.z + ")";
                    RobotMgr.Instance.CreatePosAngNoModel(nameTemp, temp, post, anglet, scale);
                    gonum++;
                }
            }
        }
    }

    public void SaveRobotMsg(string nameTemp)
    {
        #region 创建XML文档
        string nameNoType = RobotMgr.NameNoType(nameTemp);
        //目前禁止覆盖原有机器人，如果不需要原机器人信息，需再主目录中将机器人删除。
        if (RecordContactInfo.Instance.XMLExist(RecordContactInfo.Instance.openType).Contains(nameNoType) == false)
        {
            string id = RobotMgr.Instance.rbt[nameTemp].id;
            string datatype = RobotMgr.Instance.rbt[nameTemp].dataType;
            string level = RobotMgr.Instance.rbt[nameTemp].level;
            RecordContactInfo.Instance.CreateXElement(nameNoType, id, datatype,level);

            CreatGO(nameTemp);
            RecordContactInfo.Instance.Save(true);
        }
        #endregion
    }
    #endregion



    //读取机器人信息--将XML文档中的数据存入内存
    public void ReadMsg(string robotnametemp)
    {
        long tempi = System.DateTime.Now.Ticks;

        string nameNoType = RobotMgr.NameNoType(robotnametemp);

        RecordContactInfo.Instance.FindAllGOData(robotnametemp,nameNoType);

    }
    
    public void ReadMsgBase(string robotnametemp)
    {
        //string nameNoType = RobotMgr.NameNoType(robotnametemp);
        //RecordContactInfo.Instance.FindAllGOData(RobotMgr.Instance.rbt[r  obotnametemp].gos, nameNoType);

        //List<string> goName = new List<string>();
        //RecordContactInfo.Instance.FindAllGOName(goName);  //获取所有GO的名称
        ////Debug.Log("robotnametemp:" + robotnametemp);
        //if (goName != null && goName.Count > 0)
        //{
        //    foreach (string tempnam in goName)
        //    {
        //        if (tempnam != null)
        //        {
        //            string goid = RecordContactInfo.Instance.FindGOid(tempnam);
        //            string gotype = RecordContactInfo.Instance.FindPickGOType(tempnam);
        //            string djidtemp = RecordContactInfo.Instance.FindDJID(tempnam);
                    
        //            int djid = int.Parse(djidtemp);
                    
        //            RobotMgr.Instance.CreateGO(robotnametemp, tempnam, goid, gotype, djid);  //读取GO
        //           // Debug.Log("openttype:" + RecordContactInfo.Instance.openTypeTemp+";robotname:"+robotnametemp);
        //            if ((gotype == "seivo" || gotype == "duoji") && RecordContactInfo.Instance.openType == "default")
        //            {
        //                string isdptemp = RecordContactInfo.Instance.FindisDP(tempnam);
                        
        //                int isdp = int.Parse(isdptemp);
        //                RobotMgr.Instance.rbt[robotnametemp].gos[tempnam].isDP = isdp;
        //            }

        //            string[] x = new string[6];     //x[0]=goPos;x[1]=goAngle
        //            x = RecordContactInfo.Instance.FindGOPosAngle(tempnam);
        //            if (x[0] != null)
        //            {
        //                RobotMgr.Instance.CreateGOPosAngle(robotnametemp, tempnam, x[0], x[1],x[2],x[3],x[4],x[5]);  //读取gopos,goangle
        //            }

        //            if (RecordContactInfo.Instance.HasPrePos(tempnam))                                //读取 PrePos
        //            {
        //                string preposvalue;
        //                preposvalue = RecordContactInfo.Instance.FindPrePosValue(tempnam);

        //                string[] x1 = new string[2];
        //                x1 = RecordContactInfo.Instance.FindPrePosContactInf(tempnam);
        //                RobotMgr.Instance.CreatePrePosGOPoint(robotnametemp, tempnam, preposvalue, x1[0], x1[1]);
        //            }
        //        }
        //    }

        //    //Debug.Log("go:" + robotnametemp);
        //}
    }

    ////读取机器人信息--将XML文档中的数据存入内存
    //public void ReadMsgOld(string robotnametemp)
    //{
    //    long tempi = System.DateTime.Now.Ticks;
    //    Debug.Log("time0:" + System.DateTime.Now.Ticks);

    //    string nameNoType = RobotMgr.NameNoType(robotnametemp);
    //    RecordContactInfo.Instance.pathInit(nameNoType);

    //    string robotid = RecordContactInfo.Instance.FindRobotID2nd();
    //    string datatype = RecordContactInfo.Instance.FindRobotDataType();
    //    string level = RecordContactInfo.Instance.FindRobotLevel();
    //    RobotMgr.Instance.CreateRobot(robotnametemp, robotid, datatype, level);

    //    Debug.Log("time1:" + (System.DateTime.Now.Ticks - tempi));
    //    tempi = System.DateTime.Now.Ticks;

    //    ReadMsgBase(robotnametemp);

    //    Debug.Log("time2:" + (System.DateTime.Now.Ticks - tempi));
    //}

    //public void ReadMsgBaseOld(string robotnametemp)
    //{
    //    List<string> goName = new List<string>();
    //    RecordContactInfo.Instance.FindAllGOName(goName);  //获取所有GO的名称
    //    //Debug.Log("robotnametemp:" + robotnametemp);
    //    if (goName != null && goName.Count > 0)
    //    {
    //        foreach (string tempnam in goName)
    //        {
    //            if (tempnam != null)
    //            {
    //                string goid = RecordContactInfo.Instance.FindGOid(tempnam);
    //                string gotype = RecordContactInfo.Instance.FindPickGOType(tempnam);
    //                string djidtemp = RecordContactInfo.Instance.FindDJID(tempnam);

    //                int djid = int.Parse(djidtemp);

    //                RobotMgr.Instance.CreateGO(robotnametemp, tempnam, goid, gotype, djid);  //读取GO
    //                // Debug.Log("openttype:" + RecordContactInfo.Instance.openTypeTemp+";robotname:"+robotnametemp);
    //                if ((gotype == "seivo" || gotype == "duoji") && RecordContactInfo.Instance.openType == "default")
    //                {
    //                    string isdptemp = RecordContactInfo.Instance.FindisDP(tempnam);

    //                    int isdp = int.Parse(isdptemp);
    //                    RobotMgr.Instance.rbt[robotnametemp].gos[tempnam].isDP = isdp;
    //                }

    //                string[] x = new string[6];     //x[0]=goPos;x[1]=goAngle
    //                x = RecordContactInfo.Instance.FindGOPosAngle(tempnam);
    //                if (x[0] != null)
    //                {
    //                    RobotMgr.Instance.CreateGOPosAngle(robotnametemp, tempnam, x[0], x[1], x[2], x[3], x[4], x[5]);  //读取gopos,goangle
    //                }

    //                if (RecordContactInfo.Instance.HasPrePos(tempnam))                                //读取 PrePos
    //                {
    //                    string preposvalue;
    //                    preposvalue = RecordContactInfo.Instance.FindPrePosValue(tempnam);

    //                    string[] x1 = new string[2];
    //                    x1 = RecordContactInfo.Instance.FindPrePosContactInf(tempnam);
    //                    RobotMgr.Instance.CreatePrePosGOPoint(robotnametemp, tempnam, preposvalue, x1[0], x1[1]);
    //                }
    //            }
    //        }

    //        //Debug.Log("go:" + robotnametemp);
    //    }
    //}

}


