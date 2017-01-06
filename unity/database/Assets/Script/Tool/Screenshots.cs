using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:Screenshots.cs
/// Description:
/// Time:2016/12/29 10:16:16
/// </summary>
public class Screenshots : SingletonBehaviour<Screenshots>
{
    #region 公有属性
    #endregion

    #region 其他属性
    Vector3 v1;
    Vector3 v2;
    int w;
    int h;
    #endregion

    #region 公有函数

    public Screenshots()
    {

    }

    public void SaveScreenshots()
    {
        StartCoroutine(GetCapture());
    }
    #endregion

    #region 其他函数
    /*void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            v1 = Input.mousePosition;
            Debuger.Log("v1 = " + v1.ToString());
        }
        if (Input.GetMouseButtonUp(0))
        {
            v2 = Input.mousePosition;
            Debuger.Log("v2 = " + v2.ToString());
            w = int.Parse(Mathf.Abs(v1.x - v2.x).ToString());
            h = int.Parse(Mathf.Abs(v1.y - v2.y).ToString());
            Debuger.Log(string.Format("w = {0} h = {1}", w, h));
            StartCoroutine(GetCapture());
        }
    }*/
    IEnumerator GetCapture()
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        /*float vx = (v1.x > v2.x) ? v2.x : v1.x;
        float vy = (v1.y > v2.y) ? v2.y : v1.y;
        Debuger.Log(string.Format("vx = {0} vy = {1}", vx, vy));*/
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);

        byte[] imagebytes = tex.EncodeToJPG();
        tex.Compress(false);
        string dir = PublicFunction.CombinePath(Application.persistentDataPath, "Screenshots");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        System.IO.File.WriteAllBytes(dir + "/" + DateTime.Now.Ticks + ".jpg", imagebytes);
    }

    #endregion
}