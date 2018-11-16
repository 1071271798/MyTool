using UnityEngine;
using System.Collections;
using Game.Scene;

public class AdaptScreenSize : MonoBehaviour {

    UIRoot root;

    void Awake()
    {
        Adapt();
    }
    Resolution curRes;
	// Use this for initialization

    void Start()
    {
        curRes = Screen.currentResolution;
    }

    void OnEable()
    {
        Debug.Log("aaa: " + curRes.width + "  bbb: " + curRes.height);
        Screen.SetResolution(curRes.height, curRes.width, true);
    }

    void OnDisable()
    {
        Screen.SetResolution(curRes.width, curRes.height, true);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDestory()
    {
        Debug.Log("test screen size");
        Screen.SetResolution(curRes.width, curRes.height, true);
    }

    void Adapt()
    {
        AdaptiveUI();
        /*
        root = gameObject.GetComponent<UIRoot>();
        if (null != root)
        {
            float height = 1067f * Screen.height/Screen.width;
            if (height >= 1067)
            {
                root.manualHeight = 1067;
            }
            else
            {
                root.manualHeight = 640 * Screen.height / Screen.width;
            }
        }
         */
    }

	private void AdaptiveUI()
	{
		UIRoot uiRoot = transform.GetComponent<UIRoot>();
		if (uiRoot != null)
		{
            uiRoot.scalingStyle = UIRoot.Scaling.FixedSize;
            //float width = 960.0f * Screen.width / Screen.height;
            //if (width >= 960)
            //{
            //    uiRoot.manualHeight = 960;
            //    //uiRoot.manualHeight = 640;
            //}
            //else
            //{
            //    uiRoot.manualHeight = 960;
            //}
            if(Screen.height <= 750f)
            {
                uiRoot.manualHeight = 750;
            }
            else
            {
                uiRoot.manualHeight = 1000;
                //if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow)
                //{
                //    uiRoot.manualHeight = 850;
                //}
                //else
                //{
                //    uiRoot.manualHeight = 1000;
                //}
            }
		}
	}
}
