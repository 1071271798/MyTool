using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimData
{
    public Dictionary<string, AnimBase> anims;   //<id，anim类>

	
    public AnimData()
    {
        anims = new Dictionary<string, AnimBase>();
        
    }
}

public class AnimBase
{
    public string id;  //动画的id
    public string source;   //分割动画的原数据
    public string start;    //分割动画开始的帧
    public string end;      //分割动画结束的帧
    public string step;     //执行时显示的操作步数
    public string name;     //分割后动画的名称
    public string djid;     //舵机id
    public string parts;     //零件名称
    public string shape;     //装配位置的花型
    public string line;     //线的规格

    public string type;   //动画的类型，将一个动画分成几段进行打包，分成段后的名称

    public string goname;   //当前显示的模型的名称

    public string pic;//局部图

    public string firstPic;//每张局部图第一次出现时

    public string lvdaiNum;//履带拼接的数量

    public string sensorID;//传感器ID提示
}