//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class GameObjectManage : MonoBehaviour {
    public static bool flagger = false;   //防止每次回来一直创建
    public static GameObjectManage Ins;
    public GameObject TakePhotoObj;
    void Awake()
    {
        if (!flagger)
        {
            flagger = !flagger;
            DontDestroyOnLoad(gameObject); // 不销毁
            Ins = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //StepManager.GetIns().
    }

   /// <summary>
   /// 主页退出时 隐藏， 
   /// </summary>
    public void OnExit() 
    {
        if (TakePhotoObj != null)
        {
            TakePhotoObj.transform.GetChild(0).gameObject.GetComponent<BoxCollider>().enabled = false;
            TweenAlpha tw = GetTCompent.GetCompent<TweenAlpha>(TakePhotoObj.GetComponentInChildren<UITexture>().transform);
			tw.ResetToBeginning();
            tw.duration = 0.5f;
            tw.from = 1;
            tw.to = 0;
            tw.PlayForward();
        }
    }

    public void OnShow()
    {
        if (TakePhotoObj != null)
        {
            TakePhotoObj.SetActive(true);
            TakePhotoObj.transform.GetChild(0).gameObject.GetComponent<BoxCollider>().enabled = true;
            TweenAlpha tw = GetTCompent.GetCompent<TweenAlpha>(TakePhotoObj.GetComponentInChildren<UITexture>().transform);
			tw.ResetToBeginning();
            tw.duration = 0.5f;
            tw.from = 0;
            tw.to = 1;
            tw.PlayForward();
        }
    }

    /// <summary>
    /// 社区时会清空
    /// </summary>
    public void ClearData()
    {
        if (TakePhotoObj != null && TakePhotoObj.activeSelf)   //picdata
        {
            if (TakePhotoObj.GetComponentInChildren<UITexture>().mainTexture != null)
            {
                TakePhotoObj.GetComponentInChildren<UITexture>().mainTexture = null;
            }
        }
        //if (null != GameObject.Find("GuideUI Root"))  //guideUI
        //{
        //    Destroy(GameObject.Find("GuideUI Root"));
        //}
        //GuideViewBase.flagger = false;
        //GuideViewBase.OnceFlag = false;
        //StepManager.GetIns().ClearStepManager();
    }
}
