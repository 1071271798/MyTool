using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NormalStringData{

    //dj位置图标
    public static List<string> djPos()
    {
        List<string> posIndex = new List<string>();
        posIndex.Add("A");
        posIndex.Add("B");
        posIndex.Add("C");
        posIndex.Add("D");
        posIndex.Add("E");
        posIndex.Add("Star");
        posIndex.Add("Triangle");
        posIndex.Add("Square");
        posIndex.Add("Circle");

        return posIndex;
    }

    //主控盒数字编号
    public static List<string> zkhNumbers()
    {
        List<string> nums = new List<string>();
        nums.Add("Num1");
        nums.Add("Num2");
        nums.Add("Num3");
        nums.Add("Num4");
        nums.Add("Num5");
        nums.Add("Num6");
        nums.Add("Num7");
        nums.Add("Num8");

        return nums;
    }

    //线的对于关系
    public static Dictionary<string, string> WiresName()
    {
        Dictionary<string, string> wiresT = new Dictionary<string, string>();
        wiresT.Add("WIRE1", "W1-BLK");
        wiresT.Add("WIRE2", "W2-BLK");
        wiresT.Add("WIRE3", "W3-BLK");
        wiresT.Add("WIRE4", "W4-BLK");

        wiresT.Add("WIRE1-BLU", "W1-BLU");
        wiresT.Add("WIRE2-GRY", "W2-GRY");
        return wiresT;
    }

    //内置在app中的机器人id
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static List<string> DefaultRtID()
    {
        List<string> RtID = new List<string>();
        RtID.Add("robot_b053f2ac-e0cd-4427-8215-5a92bfc97d53");   //qiluogan
        return RtID;
    }

    /// <summary>
    /// 翻译数据
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string,string> TranslateData()
    {
        Dictionary<string, string> translateData = new Dictionary<string, string>();
        translateData.Add("not1", "取出ID为：");
        translateData.Add("not2","取出舵机注意");
        translateData.Add("not3","舵机位置注意");
        translateData.Add("not4","搭建完成!");
        translateData.Add("not5","完成什么模型");
        translateData.Add("not6","恭喜完成");
        translateData.Add("not7","继续搭建");
        translateData.Add("not8","确定");

        translateData.Add("not9","的发光位置进行装配");
        translateData.Add("not10","搭建到动作");
        translateData.Add("not11","搭建到遥控");
        translateData.Add("not12","关闭");
        translateData.Add("not13","主控盒插孔的发光位置");
        translateData.Add("not14","连接件");
        translateData.Add("not15","装饰件");
        translateData.Add("not16","连接线");
        translateData.Add("not17","控制器");
        translateData.Add("not18","舵机");
        translateData.Add("not19","零件缺少提示");
        translateData.Add("not20","购买零件提示");
        translateData.Add("not21","舵盘复位1");
        translateData.Add("not22", "舵盘复位2");   
        translateData.Add("not23", "确定");
        translateData.Add("not24", "搭建履带");
        translateData.Add("not25", "取出物体");
        translateData.Add("not26", "取出物体注意");
        translateData.Add("lessonFinish", "lessonfinishyuhangyuan");
        translateData.Add("not27", "祥云搭建完成");
        translateData.Add("not28", "回到悟空APP");
        return translateData;
    }
}
