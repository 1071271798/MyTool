using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Platform;

public class BlurEffect : BasePopWin
{
    private UITexture mTexture;
    private UITexture setTexture;
    private List<EventDelegate.Callback> finishedList;
    private List<UITexture> needSetTextList;
    private Material CurMaterial;

    bool ScreenshotsFlag = false;

    private static BlurEffect sInst;
    public BlurEffect(UITexture uiTexture, EventDelegate.Callback onFinished)
    {
        mUIResPath = "Prefab/UI/BlurEffect";
        sInst = this;
        this.setTexture = uiTexture;
        finishedList = new List<EventDelegate.Callback>();
        if (null != onFinished)
        {
            finishedList.Add(onFinished);
        }
    }

    public static void ShowEffect(UITexture uiTexture, EventDelegate.Callback onFinished)
    {
        if (sInst == null)
        {
            object[] args = new object[2];
            args[0] = uiTexture;
            args[1] = onFinished;
            PopWinManager.GetInst().ShowPopWin(typeof(BlurEffect), args, 0, null, false);
        }
        else
        {
            if (!sInst.IsShow)
            {
                sInst.OnShow();
            }
            sInst.AddSetTexture(uiTexture, onFinished);
        }
    }

    public static void ShowEffect()
    {
        ShowTexture.ShowMsg(null, 0);
        ShowEffect(ShowTexture.GetUITexture(), null);
    }


    public override void Init()
    {
        base.Init();
        mCoverAlpha = 0;
        mAddBox = false;
        SetInitDepth(9999);
    }

    protected override void Close()
    {
        base.Close();
        sInst = null;
        if (null != needSetTextList && null != setTexture && setTexture.mainTexture != null)
        {
            for (int i = 0, imax = needSetTextList.Count; i < imax; ++i)
            {
                try
                {
                    needSetTextList[i].mainTexture = setTexture.mainTexture;
                }
                catch (System.Exception ex)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "设置高斯模糊图片出错:" + ex.ToString());
                }

            }
        }
        for (int i = 0, imax = finishedList.Count; i < imax; ++i)
        {
            try
            {
                finishedList[i]();
            }
            catch (System.Exception ex)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "执行高斯模糊回调出错:" + ex.ToString());
            }
        }
        Resources.UnloadUnusedAssets();
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        if (null != mTrans)
        {
            mTexture = GameHelper.FindChildComponent<UITexture>(mTrans, "Texture");
            if (null != mTexture)
            {
                mTexture.width = PublicFunction.GetWidth();
                mTexture.height = PublicFunction.GetHeight();
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (!ScreenshotsFlag)
        {
            SingletonBehaviour<Screenshots>.GetInst().SetScreenshots(setTexture, delegate ()
            {
                OnClose();
            });
            ScreenshotsFlag = true;
        }
    }

    private void AddSetTexture(UITexture uiTexture, EventDelegate.Callback onFinished)
    {
        
        if (null != uiTexture)
        {
            if (null == needSetTextList)
            {
                needSetTextList = new List<UITexture>();
            }
            needSetTextList.Add(uiTexture);
        }
        if (null != onFinished)
        {
            finishedList.Add(onFinished);
        }
    }


    /*public static void ShowBlurEffect()
    {

        UICameraAttb cameraAttb = UIManager.GetInst().GetCameraData(eUICameraType.OrthographicTwo);
        if (null != cameraAttb && null != cameraAttb.camera)
        {
            ShowTexture.ShowMsg(null);
            SetBlurEffect(ShowTexture.GetUITexture(), null);
        }
    }

    public static void SetBlurEffect(UITexture uiTexture, EventDelegate.Callback onFinished)
    {
        UICameraAttb cameraAttb = UIManager.GetInst().GetCameraData(eUICameraType.OrthographicTwo);
        if (null != cameraAttb && null != cameraAttb.camera)
        {
            SingletonBehaviour<Screenshots>.GetInst().SetScreenshots(uiTexture, delegate ()
            {

                BlurEffect effect = cameraAttb.camera.gameObject.GetComponent<BlurEffect>();
                if (null != effect)
                {
                    GameObject.DestroyImmediate(effect);
                }
                effect = cameraAttb.camera.gameObject.AddComponent<BlurEffect>();
                effect.mTexture = uiTexture;
                effect.onFinished = onFinished;
            });
        }
        else
        {
            if (null != onFinished)
            {
                onFinished();
            }
        }
    }
    */
}