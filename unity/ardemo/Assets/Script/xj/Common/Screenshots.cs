using Game.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.Platform;
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
    string mLastPicPath;
    #endregion

    #region 公有函数

    public Screenshots()
    {

    }

    public void SaveScreenshots(string picName = "", bool highQuality = false)
    {
        StartCoroutine(GetCapture(picName, highQuality));
        //GetCapture(null);
    }

    public void SaveEffect()
    {
        StartCoroutine(GetCapture());
    }

    public void SetScreenshots(UITexture uiTexture, EventDelegate.Callback callback)
    {
        StartCoroutine(GetCapture(uiTexture, callback));
    }
    #endregion

    #region 其他函数
    IEnumerator GetCapture(UITexture uiTexture, EventDelegate.Callback callback)
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = null;
        try
        {
            tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
            tex.Compress(true);
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "截图出错 : " + ex.ToString());
        }
        if (null != uiTexture)
        {
            if (null != uiTexture.mainTexture)
            {
                GameObject.Destroy(uiTexture.mainTexture);
            }
            if (null != tex)
            {
                uiTexture.mainTexture = GameObject.Instantiate(tex) as Texture2D;
            } else
            {
                uiTexture.mainTexture = Texture2D.whiteTexture;
            }
        }
        if (null != callback)
        {
            callback();
        }
    }
    IEnumerator GetCapture(string picName, bool highQuality)
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        tex.Compress(highQuality);
        byte[] imagebytes = tex.EncodeToJPG();
        string dir = PublicFunction.CombinePath(ResourcesEx.persistentDataPath, "Screenshots");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (string.IsNullOrEmpty(picName))
        {
            picName = DateTime.Now.Ticks.ToString();
        }
        mLastPicPath = dir + "/" + picName + ".jpg";
        System.IO.File.WriteAllBytes(mLastPicPath, imagebytes);
    }

    IEnumerator GetCapture()
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        tex.Compress(true);
        byte[] imagebytes = tex.EncodeToJPG();
        string dir = PublicFunction.CombinePath(ResourcesEx.persistentDataPath, "Screenshots");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        mLastPicPath = dir + "/" + DateTime.Now.Ticks.ToString() + ".jpg";
        System.IO.File.WriteAllBytes(mLastPicPath, imagebytes);        
    }

    #endregion
}