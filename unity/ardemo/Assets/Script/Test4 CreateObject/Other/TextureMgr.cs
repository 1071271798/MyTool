using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;

public class TextureMgr
{
 
   // public string uiname
     public TextureMgr() {}

     public Texture2D LoadTextureByIO(string path)
     {
         //创建文件读取流
         FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
         //Debug.Log(path);
         fileStream.Seek(0, SeekOrigin.Begin);
         //创建文件长度缓冲区
         byte[] bytes = new byte[fileStream.Length];
         //读取文件
         fileStream.Read(bytes, 0, (int)fileStream.Length);
         //释放文件读取流
         fileStream.Close();
         fileStream.Dispose();
         fileStream = null;

         //创建Texture
         int width = 800;
         int height = 640;
         Texture2D texture = new Texture2D(width, height);
         texture.LoadImage(bytes);
         return texture;
     }

    //取内部Texture资源
    public Texture GetTextureByPath(string pathTemp)
    {
        string path = pathTemp;
        Texture tmp = Resources.Load(path) as Texture;

        return tmp;
    }

    //舵机位置图标
    public Dictionary<string,Texture> FindPosPic()
    {
        List<string> djpos = NormalStringData.djPos();
        List<string> djposS=new List<string>();
        Dictionary<string, Texture> djposTexture = new Dictionary<string, Texture>();
        foreach(string key in djpos)
        {
            string nameTemp="Sec-"+key;
            if(djposS.Contains(key)==false)
            {
                djposS.Add(nameTemp);
            }
        }

        foreach(string key in djpos)
        {
            string path = "Prefab/Test4/UI/" + key;
            Texture texTemp = GetTextureByPath(path);
            if(djposTexture.ContainsKey(key)==false)
            {
                djposTexture.Add(key,texTemp);
            }
        }

        foreach (string key in djposS)
        {
            string path = "Prefab/Test4/UI/" + key;
            Texture texTemp = GetTextureByPath(path);
            if (djposTexture.ContainsKey(key) == false)
            {
                djposTexture.Add(key, texTemp);
            }
        }

        return djposTexture;
    }

    //舵机ID图标
    public Dictionary<string, Texture> FindIDPic()
    {
        Dictionary<string, Texture> djIDTexture = new Dictionary<string, Texture>();

        for (int i = 0; i < 10;i++ )
        {
            string key = i.ToString();
            string path = "Prefab/Test4/UI/" + key;
            Texture texTemp = GetTextureByPath(path);
            if (djIDTexture.ContainsKey(key) == false)
            {
                djIDTexture.Add(key, texTemp);
            }
        }

        return djIDTexture;
    }
}
