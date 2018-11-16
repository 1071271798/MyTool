//----------------------------------------------
//            积木2: xiongsonglin
//            界面滑动开关
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class SliderToogle : UISlider {

    private bool isON = false;
    public delegate void VoidDelegate(bool flag);
    public VoidDelegate OnOffToogle;
    protected override void OnStart()
    {
        base.OnStart();
        //backgroundWidget.autoResizeBoxCollider = false;
        //backgroundWidget.GetComponent<BoxCollider>().size = new Vector3(backgroundWidget.width/2,backgroundWidget.height,0f);
        //backSize = backgroundWidget.GetComponent<BoxCollider>().size;
    }

    protected override void OnPressBackground(GameObject go, bool isPressed)
    {
        //base.OnPressBackground(go, isPressed);
    }

    protected override void OnPressForeground(GameObject go, bool isPressed)
    {
        //base.OnPressForeground(go, isPressed);
    }

    public override void ForceUpdate()
    {
        base.ForceUpdate();
        if (value < 0.45f)
        {
            value = 0.4f;
            isON = false;
            if (OnOffToogle != null)
                OnOffToogle(isON);
        }
        else
        {
            value = 0.62f;
            isON = true;
            if (OnOffToogle != null)
                OnOffToogle(isON);
        }
    }

}
