using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DJClass
{
    public Dictionary<string, Transform> trans;   //舵机内部模型的名称，Transform

    public Dictionary<string, DJLianDongs> djld = new Dictionary<string, DJLianDongs>();//联动舵机以及对应的角度差

    public Dictionary<string,string> djshape=new Dictionary<string,string>();//舵机插入时的花形

    public DJClass()
    {
        trans = new Dictionary<string, Transform>();
        
    }
}
public class DJLianDongs
{
    public Dictionary<string, DJLianDong> djlds = new Dictionary<string, DJLianDong>();//联动舵机的名称,DJLianDong
}

public class DJLianDong
{
    public int djid;//联动舵机ID
    public float difA;//被操作角度（difA +/-/*// defaultAngle）
    public string symbol;//加法或者减法(+ - * /)
    public float difB;//操作角度(defaultAngle +/-/*// difB)
}

public class LianDongGOs
{
    public Dictionary<int,ldgo> ldgos=new Dictionary<int,ldgo>();//(id,联动物体信息)
}
public class ldgo
{
    public float start;   //x轴大于start角度
    public float end;     //x轴小于end角度
    public Dictionary<string, Vector3> ldangle = new Dictionary<string, Vector3>();//(名称，角度)

    public Dictionary<string, Vector3> ldpos = new Dictionary<string, Vector3>();//(名称，坐标)
}