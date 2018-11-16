//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class TestCamera : MonoBehaviour {

    public GameObject TestButton;

	// Use this for initialization
	void Start () {

        if (TestButton != null)
        {
            UIEventListener.Get(TestButton).onClick = DoTextCamera;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void DoTextCamera(GameObject go)
    {
        //调用我们制作的Android插件打开手机摄像机
        /*AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        jo.Call("TakePhoto", "takePhoto");*/
    }
}
