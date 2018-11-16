using UnityEngine;
using System.Collections;
using System.IO;

public class InitiateScript
{

    public static InitiateScript _instance;


    //场景内的预设
    private GameObject camRotateCenter;
    private Vector3 camRotateCenterPos;


    private GameObject oriGO;
    private Vector3 oriGOPos;

    private GameObject oriPos;
    private Vector3 oriPosPos;

    public GameObject center;
    private Vector3 centerPos;


    private GameObject UIRoot;
    private Vector3 UIRootPos;

    private GameObject others;
    private Vector3 othersPos;

    void Awake()
    {
        _instance = this;
        camRotateCenterPos = new Vector3(-6.37f, 1.252f, -0.29f);
        camRotateCenter = Resources.Load("Prefab/Test4/ScensePrefab/CamRotateCenter") as GameObject;
        camRotateCenter = GameObject.Instantiate(camRotateCenter, camRotateCenterPos, Quaternion.identity) as GameObject;
        camRotateCenter.name = "CamRotateCenter";
       
        oriGO = new GameObject();
        oriGO.name = "oriGO";
        oriGO.transform.position = new Vector3(-6.205f, 1.322f, -0.287f);
       // oriGO.AddComponent<NewRobotMotion>();
        

        oriPosPos = new Vector3(-1.251f, 2.25f, 1.589f);
        oriPos = Resources.Load("Prefab/Test4/ScensePrefab/OriPos") as GameObject;
        oriPos = GameObject.Instantiate(oriPos, oriPosPos, Quaternion.identity) as GameObject;
        oriPos.name = "OriPos";

        centerPos = new Vector3(-6.419f, 1.322f, -0.287f);
        center = Resources.Load("Prefab/Test4/ScensePrefab/Center") as GameObject;
        center = GameObject.Instantiate(center, centerPos, Quaternion.identity) as GameObject;
        center.name = "Center";
        center.transform.position = new Vector3(-6.413f, 1.336f, -0.287f);

        UIRootPos = new Vector3(11.77167f, -0.03749338f, 0.0f);
        UIRoot = Resources.Load("Prefab/Test4/ScensePrefab/UI Root") as GameObject;
        UIRoot = GameObject.Instantiate(UIRoot, UIRootPos, Quaternion.identity) as GameObject;
        UIRoot.name = "UI Root";
        foreach(Transform m in UIRoot.transform.GetComponentInChildren<Transform>())
        {
            if(m.name=="MainCamera")
            {
                m.parent = null;
                
            }
        }
        //GameObject cam = GameObject.Find("MainCamera/Camera");
        //oriGO.transform.GetComponent<NewRobotMotion>().mCam = cam.camera;

        GameObject.Find("MainCamera").transform.position = new Vector3(-6.413f, 1.336f, -2.003f);
        othersPos = new Vector3(-6.419f, 1.322f, -3.287f);
        others = Resources.Load("Prefab/Test4/ScensePrefab/Others") as GameObject;
        others = GameObject.Instantiate(others, othersPos, Quaternion.identity) as GameObject;
        others.name = "Others";
        init();
    }

    public void init()
    {
        string nameTemp = RobotMgr.Instance.rbtnametempt;
        if (nameTemp!=null&&RobotMgr.Instance.rbt.ContainsKey(nameTemp) == false)
        {
            string id = CreateID.CreateRobotID();
            string datatype = "playerdata";
            string level = "1";
            RobotMgr.Instance.CreateRobot(nameTemp, id, datatype,level);
        }
        
    }
}
