using UnityEngine;
using System.Collections;
using LitJson;
using System.IO;
//using UnityEditor;
using System.Collections.Generic;

public class GetRawJsonData : MonoBehaviour {


    //从TXT文本里都json
    public Dictionary<string, object> ReadJsonFromTXT(string path)
    {

        Dictionary<string, object> sm_textConfig = null;
        FileStream file = new FileStream(path, FileMode.Open);
        //新建字节型数组，数组的长度是file文件对象的长度（后面用于存放文件）
        byte[] heByte = new byte[file.Length];
        
        //通过fs对象的Read方法bt得到了fs对象流中的内容
        file.Read(heByte, 0, heByte.Length);

        //将heByte字节型数组中的数据由System.Text.Encoding.UTF8.GetString或者Encoding.Default.GetString(heByte)方法取出，交给myStr
        string myStr = System.Text.Encoding.UTF8.GetString(heByte);
   
         sm_textConfig = MiniJSON.Json.Deserialize(myStr) as Dictionary<string, object>;

         return sm_textConfig;
	}
}
