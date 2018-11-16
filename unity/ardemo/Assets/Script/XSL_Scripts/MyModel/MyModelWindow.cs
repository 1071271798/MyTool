//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class MyModelWindow : MonoBehaviour {

    public GameObject CamBtn;
    public GameObject RightUI;
    public GameObject AlbumBtn;
    public UITexture ModelPic;

	// Use this for initialization
	void Start () {

        if (CamBtn != null)
        {
            UIEventListener.Get(CamBtn).onClick = OpenCamera;
        }
        if (AlbumBtn != null)
        {
            UIEventListener.Get(AlbumBtn).onClick = OpenAlbum;
        }
    //    if()

	}
	
	// Update is called once per frame
	void Update () {

    }

    #region //function open camera
    void OpenCamera(GameObject go)
    {
        //调用我们制作的Android插件打开手机摄像机
        /*AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        jo.Call("TakePhoto", "takePhoto");*/
    }

    void OpenAlbum(GameObject go)
    {
        //调用我们制作的Android插件打开手机相册
        /*AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        jo.Call("TakePhoto", "takeSave");*/
    }
    #endregion
}
