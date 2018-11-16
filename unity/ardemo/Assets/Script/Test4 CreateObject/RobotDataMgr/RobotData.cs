/*
 * 作用:机器人的基本类
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotData
{
    public Dictionary<string, GO> gos;   //<GO名字，GO类>

    public string robotName;
    public string id;     //机器人的id
    public string level;     //机器人的等级
    public string dataType;   //是玩家数据("playerdata")，默认数据("default")，还是下载("download")的数据

    public RobotData()
    {
        Init();
    }

    void Init()
    {
        gos = new Dictionary<string, GO>();
    }
}

//生成的物体
public class GO
{
    public string goType;     //go的类型

    public string goID;        //go的类型

    public int djID;       //只使用与duoji
    public int isDP;   //判断选中舵机是否是舵盘旋转(0:不是；1:是)

    public string color;   //模型的颜色

    public string hidego;   //模型是否隐藏
    public string shape;//舵盘插入花形

    public float oriDJAngleX;  //舵机默认初始角度

    public GoPosAngle posAngle;    //go的pos和angle

    public PrePosGoPoint ppgp;     //go的PrePos

    public string sensorID;//传感器ID

    public int motorID;//传感器ID
    

    public GO()
    {
        Init();
    }
    void Init()
    {

        djID = 0;
        posAngle = new GoPosAngle();
        ppgp = new PrePosGoPoint();
    }

}

//go的pos和angle
public class GoPosAngle
{
    public string pos;
    public string angle;
    public string scale;
    public string startpos;
}

//prepos的contactgo和contactpoint
public class PrePosGoPoint
{
    public string preposvalue;
    public string contactgo;
    public string contactpoint;
}

//舵盘和舵机主体
public class DPBOX
{
    public GameObject dp;//舵机舵盘
    public GameObject box;//舵机主体旋转时需要的父物体模型

    public Vector3 dplocalPos;//舵机舵盘位置坐标
    public Vector3 dplocalAngle;//舵机舵盘角度坐标
    public Vector3 boxlocalPos;//舵机主体旋转时需要的父物体模型位置坐标
    public Vector3 boxlocalAngle;//舵机主体旋转时需要的父物体模型角度坐标
}
