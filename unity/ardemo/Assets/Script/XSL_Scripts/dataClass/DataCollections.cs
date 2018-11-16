//----------------------------------------------
//            积木2: xiongsonglin
//            数据格式
// Copyright © 2015 for Open
//----------------------------------------------
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace MyData
{
    //模型动作序列化类
    
    public class ActionXmlData
    {
        public static int MaxActionAmount = 30;
        public string RobotID { get; set; }
        public string RobotName { get; set; }
        public List<string> ActionList { get; set; }
        public List<string> ActionCombine { get; set; }

        public ActionXmlData()
        {
            ActionList = new List<string>();
            ActionCombine = new List<string>();
            //for (int i = 0; i < 20; i++) //默认遥控界面最多能配置30个动作
            //{
            //    ActionList.Add("");
            //}
        }
    }
}
