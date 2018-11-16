using UnityEngine;
using System.Collections;

public class UISize : MonoBehaviour {

	// Use this for initialization
	void Start () {
        UIRoot uiRoot = transform.GetComponent<UIRoot>();
        if (null != uiRoot)
        {
            uiRoot.scalingStyle = UIRoot.Scaling.FixedSize;
            uiRoot.manualHeight = PublicFunction.RootManualHeight;
            return;
        }
        GameObject rootObj1 = GameObject.Find("UI Root");
        if(rootObj1!=null)
        {
            UIRoot root = rootObj1.GetComponent<UIRoot>();
            if (null != root)
            {
                root.scalingStyle = UIRoot.Scaling.FixedSize;
                root.manualHeight = PublicFunction.RootManualHeight;
            }
        }

        GameObject rootObj2 = GameObject.Find("MainUIRoot");
        if (rootObj2 != null)
        {
            UIRoot root = rootObj2.GetComponent<UIRoot>();
            if (null == root)
            {
                root = rootObj2.AddComponent<UIRoot>();
            }
            /*root.scalingStyle = UIRoot.Scaling.PixelPerfect;
            root.manualHeight = 768;
            root.minimumHeight = 320;
            root.maximumHeight = root.activeHeight;*/
            root.scalingStyle = UIRoot.Scaling.FixedSize;
            root.manualHeight = PublicFunction.RootManualHeight;
        }
       
	}
	
}
