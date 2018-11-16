/*
 * 作用：场景运行状态下，记录选中物体的坐标，角度等
 *     注：1.在Hierarchy窗口先，按Shift可以多选物体；
 *         2.运行场景(如果编辑了脚本需要在Game场景中点击一下鼠标---这样就可以执行脚本里的程序了),然后按B键，就可以记录下物体的数据了
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using UnityEditor;

public class RecordGOData : MonoBehaviour 
{
    public static GameObject[] gos;
    public static  Dictionary<string, string> gopos = new Dictionary<string, string>();   //物体的位置
    public static Dictionary<string, string> goangl = new Dictionary<string, string>();   //物体的角度
    public static Dictionary<string, string> goscale = new Dictionary<string, string>();   //物体的缩放
   public static Dictionary<string, string> gocolorLL = new Dictionary<string, string>();   //物体的颜色

    public Dictionary<string, string> startpos = new Dictionary<string, string>();   //物体的开始坐标  即 从某位置移动到指定位置

//   [MenuItem("AssetBundleDemo/LinesData")]
//    static void RecordLinesData()
//    {
//        gopos = new Dictionary<string, string>();   //物体的位置
//        goangl = new Dictionary<string, string>();   //物体的角度
//        goscale = new Dictionary<string, string>();   //物体的缩放
//        gos = Selection.gameObjects;

//        #region 1.读取物体的坐标，角度，缩放
//        for (int i = 0; i < gos.Length; i++)
//        {
//            //Debug.Log("posd:" + gos[i].name);
//            if (gopos.ContainsKey(gos[i].name) == false)   //记录选中物体的坐标
//            {
//                Vector3 postemp = gos[i].transform.localPosition;
//                string pos = "(" + postemp.x.ToString() + "," + postemp.y.ToString() + "," + postemp.z.ToString() + ")";
//                gopos.Add(gos[i].name, pos);
//            }

//            if (goangl.ContainsKey(gos[i].name) == false)  //记录选中物体的角度
//            {
//                Vector3 angltemp = gos[i].transform.localEulerAngles;
//                string angl = "(" + angltemp.x.ToString() + "," + angltemp.y.ToString() + "," + angltemp.z.ToString() + ")";
//                goangl.Add(gos[i].name, angl);
//            }

//            if (goscale.ContainsKey(gos[i].name) == false)  //记录选中物体的缩放
//            {
//                Vector3 scaletemp = gos[i].transform.localScale;
//                string scale = "(" + scaletemp.x.ToString() + "," + scaletemp.y.ToString() + "," + scaletemp.z.ToString() + ")";
//                goscale.Add(gos[i].name, scale);
//            }

//        }
//        Debug.Log("gopos  count1:" + gopos.Count);
//        #endregion

//        string robotname = "K2";
//       string pathfile = "";

//        pathfile = Application.persistentDataPath + "/rawData/K2";
//        Debug.Log(Application.persistentDataPath);
//        if (Directory.Exists(pathfile)==false)
//        {
//            Debug.Log("gopos:");
//            RecordInfo.Instance.CreateXML(robotname);
//        }

//        if (gopos != null)
//        {
//            Debug.Log("gopos  count2:" + gopos.Count);
//            foreach (string nameTemp in gopos.Keys)
//            {
//                RecordInfo.Instance.AddPosAngleScale(nameTemp, gopos[nameTemp], goangl[nameTemp], goscale[nameTemp]);
//            }
//        }

//    }

//    [MenuItem("AssetBundleDemo/RecordModelData")]
//    static void RecordModelData()
//    {
//          gopos = new Dictionary<string, string>();   //物体的位置
//          goangl = new Dictionary<string, string>();   //物体的角度
//          goscale = new Dictionary<string, string>();   //物体的缩放
//          gocolorLL = new Dictionary<string, string>();   //物体的颜色
//        gos = Selection.gameObjects;

//#region 1.读取物体的坐标，角度，缩放
//        for (int i = 0; i < gos.Length; i++)
//        {
//            Debug.Log("posd:" + gos[i].name);
//            if (gopos.ContainsKey(gos[i].name) == false)   //记录选中物体的坐标
//            {
//                Vector3 postemp = gos[i].transform.position;
//                string pos = "(" + postemp.x.ToString() + "," + postemp.y.ToString() + "," + postemp.z.ToString() + ")";
//                gopos.Add(gos[i].name, pos);
//            }

//            if (goangl.ContainsKey(gos[i].name) == false)  //记录选中物体的角度
//            {
//                Vector3 angltemp = gos[i].transform.eulerAngles;
//                string angl = "(" + angltemp.x.ToString() + "," + angltemp.y.ToString() + "," + angltemp.z.ToString() + ")";
//                goangl.Add(gos[i].name, angl);
//            }

//            if (goscale.ContainsKey(gos[i].name) == false)  //记录选中物体的缩放
//            {
//                Vector3 scaletemp = gos[i].transform.localScale;
//                string scale = "(" + scaletemp.x.ToString() + "," + scaletemp.y.ToString() + "," + scaletemp.z.ToString() + ")";
//                goscale.Add(gos[i].name, scale);
//            }

//        }
//        Debug.Log("gopos  count1:" + gopos.Count);
//#endregion

//        #region 2.读取颜色

//        for (int i = 0; i < gos.Length; i++)
//        {

//            if (gocolorLL.ContainsKey(gos[i].name) == false)  //记录选中物体的颜色
//            {
//                Transform childtemp = null;
//                Transform grouptemp = null;
//                foreach (Transform child in gos[i].transform.GetComponentInChildren<Transform>())
//                {
//                    Debug.Log("gon:" + gos[i].name);
//                    if (child.name.Contains("Group"))
//                    {
//                        grouptemp = child;

//                        break;
//                    }
//                }
//                if (grouptemp != null)
//                {
//                    childtemp = grouptemp.GetChild(0);
//                    Color colortemp = childtemp.transform.renderer.material.color;
//                    string col = "(" + colortemp.r.ToString() + "," + colortemp.g.ToString() + "," + colortemp.b.ToString() + "," + colortemp.a.ToString() + ")";//(r,g,b,a)
//                    gocolorLL.Add(gos[i].name, col);

//                }
//                else
//                {
//                    Debug.Log("gon【i】:" + gos[i].name);
//                    Color colortemp = gos[i].transform.renderer.material.color;
//                    string col = "(" + colortemp.r.ToString() + "," + colortemp.g.ToString() + "," + colortemp.b.ToString() + "," + colortemp.a.ToString() + ")";//(r,g,b,a)
//                    gocolorLL.Add(gos[i].name, col);
//                    Debug.Log("col:" + col + "name:" + gos[i].name);
//                }
//            }
//        }
//        #endregion

//        #region 3.记录物体的坐标，角度，缩放,颜色，舵机ID
//        string robotname = "K2";
//        RecordInfo.Instance.CreateXML(robotname);

//        int idTemp = 0;
//        string gotype;
//        if (gopos != null)
//        {
//            Debug.Log("gopos  count2:" + gopos.Count);
//            foreach (string nameTemp in gopos.Keys)
//            {
                
//                bool haslow = nameTemp.Contains("_");
//                if (haslow == true)
//                {
//                    Debug.Log("___:" + nameTemp);

//                    string[] tP = nameTemp.Split('_');

//                    gotype = tP[0];

                   
//                    idTemp++;      //go的ID
//                    string id = idTemp.ToString();

//                    string djid = "0";
//                    if(nameTemp.Contains("seivo"))
//                    {
//                        if (nameTemp.Contains("_"))
//                        {
//                            string[] tPT = nameTemp.Split('_');
//                            string djidT=tPT[1];
//                            Debug.Log("djidT:" + djidT);
//                            djid = int.Parse(djidT).ToString();
//                        }
//                        else
//                        {
//                            djid = "1";
//                        }
//                    }

//                    RecordInfo.Instance.AddGO(nameTemp, id, gotype, gopos[nameTemp], goangl[nameTemp], goscale[nameTemp], djid, gocolorLL[nameTemp]);
                   
//                }
//                else
//                {
//                    Debug.Log("  :" + nameTemp);
//                    gotype = nameTemp;     //go的类型
//                    idTemp++;      //go的ID
//                    string id = idTemp.ToString();

//                    string djid = "0";
//                    if (nameTemp.Contains("seivo"))
//                    {
//                        if (nameTemp.Contains("_"))
//                        {
//                            string[] tPT = nameTemp.Split('_');
//                            string djidT = tPT[1];
//                            Debug.Log("djidT:"+djidT);
//                            djid = int.Parse(djidT).ToString();
//                        }
//                        else
//                        {
//                            djid = "1";
//                        }
//                    }
//                    RecordInfo.Instance.AddGO(nameTemp, id, gotype, gopos[nameTemp], goangl[nameTemp], goscale[nameTemp], djid, gocolorLL[nameTemp]);
//                }

//                RecordInfo.Instance.AddContactInf(nameTemp, "PrePos", "1", "duoji", "1");


//            }
//        }
//        #endregion

    //}

    //[MenuItem("AssetBundleDemo/RecordData")]
    //static void RecordData()
    //{
    //    //RecordContactInfo.Instance.CreateXML(robotname);
    //    RecordContactInfo.Instance.CreateXElement("K2");
    //    //获取要打包的对象（在Project视图中）
    //    Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
    //     //遍历选中的对象
    //    foreach (Object obj in selects)
    //    {
    //        string nameT = obj.name;
    //        RecordContactInfo.Instance.AddParts(nameT);
    //    }
    //}
}
