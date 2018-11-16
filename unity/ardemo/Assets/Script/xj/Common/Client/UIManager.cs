using Game;
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:UIManager.cs
/// Description:
/// Time:2015/7/21 13:53:03
/// </summary>
public class UIManager : SingletonObject<UIManager>
{
    #region 公有属性
    #endregion

    #region 私有属性
    Dictionary<eUICameraType, UICameraAttb> mUICameraDict = new Dictionary<eUICameraType, UICameraAttb>();
    Dictionary<string, BaseUI> mBaseUIDict = new Dictionary<string, BaseUI>();
    List<BaseUI> mOpenUIList = new List<BaseUI>();
    int lastWidth;
    int lastHeight;
    #endregion

    #region 公有函数
    public void Init()
    {
        Create2DCamera(eUICameraType.OrthographicOne, GetCameraDepth(eUICameraType.OrthographicOne), (int)eLayer.OneUI);
        Create2DCamera(eUICameraType.OrthographicTwo, GetCameraDepth(eUICameraType.OrthographicTwo), (int)eLayer.PopUI);
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    public void CloseAllUI()
    {
        for (int i = 0, icount = mOpenUIList.Count; i < icount; ++i)
        {
            if (mOpenUIList[i].IsOpen)
            {
                mOpenUIList[i].OnClose();
            }
        }
        mOpenUIList.Clear();
    }

    public void OpenUI(BaseUI ui)
    {
        if (!mOpenUIList.Contains(ui))
        {
            mOpenUIList.Add(ui);
        }
    }

    public void CloseUI(BaseUI ui)
    {
        mOpenUIList.Remove(ui);
        ui.OnClose();
    }

    public Transform InitUI(UIAnchor.Side side, eUICameraType camType, Transform trans)
    {
        if (mUICameraDict.ContainsKey(camType))
        {
            CheckCameraDepth(camType);
            Transform parent = mUICameraDict[camType].root.transform.Find("UICamera/" + side.ToString());
            if (null != parent)
            {
                trans.parent = parent;
                Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(trans);
                Vector3 pos = Vector3.zero;
                if (side == UIAnchor.Side.TopLeft || side == UIAnchor.Side.Left || side == UIAnchor.Side.BottomLeft)
                {
                    pos.x += bound.size.x / 2;
                }
                else if (side == UIAnchor.Side.TopRight || side == UIAnchor.Side.Right || side == UIAnchor.Side.BottomRight)
                {
                    pos.x -= bound.size.x / 2;
                }
                if (side == UIAnchor.Side.TopLeft || side == UIAnchor.Side.Top || side == UIAnchor.Side.TopRight)
                {
                    pos.y -= bound.size.y / 2;
                }
                else if (side == UIAnchor.Side.BottomLeft || side == UIAnchor.Side.Bottom || side == UIAnchor.Side.BottomRight)
                {
                    pos.y += bound.size.y / 2;
                }
                trans.localPosition = pos;
                trans.localScale = Vector3.one;
                trans.localEulerAngles = Vector3.zero;
                PublicFunction.SetLayerRecursively(trans.gameObject, parent.gameObject.layer);
            }
            return parent;
        }
        return null;
        //AddButtonEvent(trans);
    }


    public void CheckScreenSize()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        int w = Screen.width;
        int h = Screen.height;

        if (w != lastWidth || h != lastHeight)
        {
            lastWidth = w;
            lastHeight = h;
            try
            {
                PublicFunction.SetHeightAndWidth(h, w);
                foreach (var kvp in mUICameraDict)
                {
                    if (null != kvp.Value.root)
                    {
                        UIRoot uiroot = kvp.Value.root.GetComponent<UIRoot>();
                        if (null != uiroot)
                        {
                            uiroot.manualHeight = PublicFunction.RootManualHeight;
                        }
                    }
                }
                for (int i = 0, imax = mOpenUIList.Count; i < imax; ++i)
                {
                    mOpenUIList[i].OnScreenResize();
                }
            }
            catch (System.Exception ex)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, ex.ToString());
            }
            
        }
    }
    public void Dispose()
    {
        //mUICameraDict.Clear();
        //mBaseUIDict.Clear();
        //mOpenUIList.Clear();
    }

    public void AddDestroyObj(GameObject obj)
    {
        AddDeletedObj(obj);
        /*UIPanel[] uiPanel = obj.GetComponentsInChildren<UIPanel>();
        if (null != uiPanel)
        {
            for (int i = 0, imax = uiPanel.Length; i < imax; ++i)
            {
                TweenAlpha.Begin(uiPanel[i].gameObject, 0.3f, 0.01f);
            }
        }
        Timer.Add(0.4f, 1, 1, AddDeletedObj, obj);*/
    }

    public Camera GetCamera(eUICameraType camType)
    {
        if (mUICameraDict.ContainsKey(camType))
        {
            return mUICameraDict[camType].camera;
        }
        return null;
    }

    public UICameraAttb GetCameraData(eUICameraType camType)
    {
        if (mUICameraDict.ContainsKey(camType))
        {
            return mUICameraDict[camType];
        }
        return null;
    }

    public static GameObject AddCover(Transform trans, float alpha, Color color, bool addCoverPanel = false, BaseUI baseUI = null, float duration = 0.3f)
    {
        GameObject obj = new GameObject("Cover");
        obj.transform.parent = trans;
        Camera cam = NGUITools.FindInParents<Camera>(trans);
        if (null != cam)
        {
            obj.transform.position = cam.transform.position;
        }
        else
        {
            obj.transform.localPosition = Vector3.zero;
        }
        obj.transform.localEulerAngles = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.layer = trans.gameObject.layer;

        if (addCoverPanel)
        {
            obj.AddComponent<UIPanel>();
        }
        GameObject text = NGUITools.AddChild(obj);
        text.name = "text";
        if (alpha == 1)
        {
            AddUITexture(text, 1, color, baseUI);
            /*TweenAlpha ta = TweenAlpha.Begin(obj, duration, alpha);
            ta.from = 0;*/

        } else
        {
//#if UNITY_ANDROID
            AddUITexture(text, alpha, color, baseUI);
            TweenAlpha ta = TweenAlpha.Begin(obj, duration, alpha);
            ta.from = 0;
/*#else
            AddUITexture(text, 1, color, baseUI, -1, true);
            TweenAlpha ta = TweenAlpha.Begin(obj, duration, 1);
            ta.from = 0;
#endif*/

        }
        return obj;
    }

    public static UITexture AddUITexture(GameObject obj, float alpha, Color color, BaseUI baseUI = null, int depth = -1, bool getTexFlag = false)
    {
        UITexture uiTexture = obj.AddComponent<UITexture>();
        uiTexture.depth = depth;
        if (getTexFlag)
        {
//#if UNITY_ANDROID
            uiTexture.mainTexture = Texture2D.whiteTexture;
            uiTexture.color = color;
            uiTexture.alpha = alpha;
/*#else
            uiTexture.mainTexture = ShowTexture.GetTexture2D();
            uiTexture.color = PublicFunction.Blur_Effect_Texture_Color;
#endif*/

        }
        else
        {
            uiTexture.mainTexture = Texture2D.whiteTexture;
            uiTexture.color = color;
            uiTexture.alpha = alpha;
        }
        
        
        uiTexture.shader = Shader.Find("Unlit/Transparent Colored");
        if (null != baseUI)
        {
            baseUI.AddScreenResizeEvent(obj.transform, delegate () {               
                uiTexture.width = PublicFunction.GetExtendWidth();
                uiTexture.height = PublicFunction.GetExtendHeight();
            });
        } else
        {
            uiTexture.width = PublicFunction.GetExtendWidth();
            uiTexture.height = PublicFunction.GetExtendHeight();
        }
        
        
        return uiTexture;
    }

    public static void AddBox(Transform trans, BaseUI baseUI)
    {
        if (null == trans)
        {
            return;
        }
        Transform cover = trans.Find("Cover");
        if (null == cover)
        {
            GameObject obj = new GameObject("Cover");
            cover = obj.transform;
            cover.parent = trans;
            cover.localPosition = Vector3.zero;
            cover.localEulerAngles = Vector3.zero;
            cover.localScale = Vector3.one;
            obj.layer = trans.gameObject.layer;
            
        }
        BoxCollider box = null;
        if (null != cover)
        {
            box = cover.gameObject.AddComponent<BoxCollider>();
            if (null != box)
            {
                box.center = Vector3.zero;
            }
        }

        if (null != baseUI)
        {
            baseUI.AddScreenResizeEvent(trans, delegate () {
                if (null != box)
                {
                    box.size = new Vector3(PublicFunction.GetExtendWidth(), PublicFunction.GetExtendHeight(), 0);
                }
            });
        }
        else
        {
            if (null != box)
            {
                box.size = new Vector3(PublicFunction.GetExtendWidth(), PublicFunction.GetExtendHeight(), 0);
            }
        }
        
    }

    public static void AddCoverBox(Transform trans, float alpha, Color color, bool addCoverPanel = false, BaseUI baseUI = null, float duration = 0.3f)
    {
        AddCover(trans, alpha, color, addCoverPanel, baseUI, duration);
        AddBox(trans, baseUI);
    }
    /// <summary>
    /// 设置按钮委托事件
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="del"></param>
    public static void SetButtonEventDelegate(Transform ui, ButtonDelegate del)
    {
        UIManager.AddButtonEvent(ui);
        ButtonEvent[] buttonEvents = ui.GetComponentsInChildren<ButtonEvent>(true);
        if (null != buttonEvents)
        {
            for (int i = 0, icount = buttonEvents.Length; i < icount; ++i)
            {
                buttonEvents[i].SetDelegate(del);
            }
        }
    }

    /// <summary>
    /// 获取Transform的Bounds
    /// </summary>
    /// <param name="trans"></param>
    /// <returns></returns>
    public static Bounds GetTransformBounds(Transform trans)
    {
        Bounds b1 = NGUIMath.CalculateRelativeWidgetBounds(trans);
        Vector3 scale = trans.lossyScale;
        b1.min = Vector3.Scale(b1.min, scale);
        b1.max = Vector3.Scale(b1.max, scale);
        return b1;
    }
    /// <summary>
    /// 针对整个游戏窗口适配
    /// </summary>
    /// <param name="obj">要求位置的GameObject</param>
    /// <param name="pivot">相对游戏窗口的位置</param>
    /// <param name="x">偏移</param>
    /// <param name="y"></param>
    /// <returns>世界坐标</returns>
    public static Vector3 GetWinPos(Transform obj, UIWidget.Pivot pivot, float x = 0, float y = 0)
    {
        if (null == obj)
        {
            return Vector3.zero;
        }
        Bounds b = NGUIMath.CalculateRelativeWidgetBounds(obj);
        Vector3 pivotPos = Vector3.zero;
        float winHeight = PublicFunction.GetHeight();
        float winWidth = PublicFunction.GetWidth();
        switch (pivot)
        {
            case UIWidget.Pivot.TopLeft:
                pivotPos = new Vector3(-winWidth / 2, winHeight / 2, 0);
                pivotPos = new Vector3(pivotPos.x + (b.size.x / 2 + x), pivotPos.y - (b.size.y / 2 + y), obj.localPosition.z);
                break;
            case UIWidget.Pivot.Top:
                pivotPos = new Vector3(0, winHeight / 2, 0);
                pivotPos = new Vector3(pivotPos.x + x, pivotPos.y - (b.size.y / 2 + y), obj.localPosition.z);
                break;
            case UIWidget.Pivot.TopRight:
                pivotPos = new Vector3(winWidth / 2, winHeight / 2, 0);
                return new Vector3(pivotPos.x - (b.size.x / 2 + x), pivotPos.y - (b.size.y / 2 + y), obj.localPosition.z);
            case UIWidget.Pivot.Left:
                pivotPos = new Vector3(-winWidth / 2, 0, 0);
                pivotPos = new Vector3(pivotPos.x + (b.size.x / 2 + x), pivotPos.y + y, obj.localPosition.z);
                break;
            case UIWidget.Pivot.Center:
                pivotPos = new Vector3(0, 0, 0);
                pivotPos = new Vector3(pivotPos.x + x, pivotPos.y + y, obj.localPosition.z);
                break;
            case UIWidget.Pivot.Right:
                pivotPos = new Vector3(winWidth / 2, 0, 0);
                pivotPos = new Vector3(pivotPos.x - (b.size.x / 2 + x), pivotPos.y + y, obj.localPosition.z);
                break;
            case UIWidget.Pivot.BottomLeft:
                pivotPos = new Vector3(-winWidth / 2, -winHeight / 2, 0);
                pivotPos = new Vector3(pivotPos.x + (b.size.x / 2 + x), pivotPos.y + (b.size.y / 2 + y), obj.localPosition.z);
                break;
            case UIWidget.Pivot.Bottom:
                pivotPos = new Vector3(0, -winHeight / 2, 0);
                pivotPos = new Vector3(pivotPos.x + x, pivotPos.y + (b.size.y / 2 + y), obj.localPosition.z);
                break;
            case UIWidget.Pivot.BottomRight:
                pivotPos = new Vector3(winWidth / 2, -winHeight / 2, 0);
                pivotPos = new Vector3(pivotPos.x - (b.size.x / 2 + x), pivotPos.y + (b.size.y / 2 + y), obj.localPosition.z);
                break;
        }
        //pivotPos = Vector3.Scale(pivotPos, obj.lossyScale);
        return pivotPos;
    }

    public void CheckCameraDepth(eUICameraType camType)
    {
        try
        {
            if (mUICameraDict.ContainsKey(camType) && mUICameraDict[camType] != null)
            {
                if (mUICameraDict[camType].camera.depth != GetCameraDepth(camType))
                {
                    mUICameraDict[camType].camera.depth = GetCameraDepth(camType);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public void OpenMultiTouch()
    {
        try
        {
            if (null != mUICameraDict)
            {
                foreach (var kvp in mUICameraDict)
                {
                    kvp.Value.uiCamera.allowMultiTouch = true;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public void CloseMultiTouch()
    {
        try
        {
            if (null != mUICameraDict)
            {
                foreach (var kvp in mUICameraDict)
                {
                    kvp.Value.uiCamera.allowMultiTouch = false;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 添加按钮事件
    /// </summary>
    /// <param name="trans"></param>
    static void AddButtonEvent(Transform trans)
    {
        BoxCollider[] boxs = trans.GetComponentsInChildren<BoxCollider>(true);
        if (null != boxs)
        {
            for (int i = 0, icount = boxs.Length; i < icount; ++i)
            {
                ButtonEvent btnEvent = boxs[i].transform.GetComponent<ButtonEvent>();
                if (null == btnEvent)
                {
                    btnEvent = boxs[i].transform.gameObject.AddComponent<ButtonEvent>();
                }
            }
        }
    }

#endregion

#region 私有函数
    private void Create2DCamera(eUICameraType camType, int depth, int layer)
    {
        int index = (int)camType;
        GameObject rootObj = new GameObject("UIRoot(2D)(" + index.ToString() + ")");
        UnityEngine.Object.DontDestroyOnLoad(rootObj);
        rootObj.layer = layer;
        rootObj.transform.localScale = new Vector3(0.00393f, 0.00393f, 0.00393f);
      //  rootObj.transform.localPosition = new Vector3(index * 1000, 0, 0);
        rootObj.transform.localPosition = Vector3.zero;
        
        //创建UIROOT
        UIRoot root = rootObj.AddComponent<UIRoot>();
        /*root.scalingStyle = UIRoot.Scaling.PixelPerfect;
        root.manualHeight = 768;
        root.minimumHeight = 320;
        root.maximumHeight = root.activeHeight;*/
        root.scalingStyle = UIRoot.Scaling.FixedSize;
        root.manualHeight = PublicFunction.RootManualHeight;
        
        //创建UI摄像机
        GameObject camObj = new GameObject("UICamera");
        camObj.transform.parent = rootObj.transform;
        camObj.transform.localPosition = Vector3.zero;
        camObj.transform.localScale = Vector3.one;
        camObj.layer = layer;

        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Depth;
        cam.orthographic = true;
        cam.orthographicSize = 1;
        cam.nearClipPlane = -1000;
        cam.farClipPlane = 1000;
        cam.cullingMask = 1 << layer;
        //cam.renderingPath = RenderingPath.Forward;
        cam.depth = depth;

        UICamera uiCamera = camObj.AddComponent<UICamera>();
        uiCamera.eventReceiverMask = 1 << layer;
        uiCamera.allowMultiTouch = false;

        for (int i = 0; i < 9; ++i)
        {
            UIAnchor.Side side = (UIAnchor.Side)i;
            CreateAnchor(side, camObj, layer);
        }

        UICameraAttb camAttb = new UICameraAttb();
        camAttb.depthIndex = index;
        camAttb.uiCamera = uiCamera;
        camAttb.camera = cam;
        camAttb.root = rootObj;
        camAttb.layer = layer;
        camAttb.depth = depth;
        mUICameraDict.Add(camType, camAttb);
    }

    private void CreateAnchor(UIAnchor.Side side, GameObject camObj, int layer)
    {
        GameObject anchorObj = new GameObject(side.ToString());
        anchorObj.transform.parent = camObj.transform;
        anchorObj.transform.localPosition = Vector3.zero;
        anchorObj.layer = layer;
        UIAnchor anchor = anchorObj.AddComponent<UIAnchor>();
        anchor.transform.localScale = Vector3.one;
        anchor.side = side;
    }

    private void AddDeletedObj(params object[] objs)
    {
        GameObject obj = objs[0] as GameObject;
        if (null != obj)
        {
            GameObject.Destroy(obj);
        }
    }

    private int GetCameraDepth(eUICameraType cameraType)
    {
        switch (cameraType)
        {
            case eUICameraType.OrthographicOne:
                return (int)eCameraDepth.Camera_Depth_One;
            case eUICameraType.OrthographicTwo:
                return (int)eCameraDepth.Camera_Depth_Two;
            default:
                return (int)eCameraDepth.Camera_Depth_Two;
        }
    }
#endregion
}

public class UICameraAttb
{
    public int depthIndex;
    public int depth;
    public UICamera uiCamera;
    public Camera camera;
    public GameObject root;
    public int layer;
}

public enum eUICameraType
{
    OrthographicOne = 1,
    OrthographicTwo
}

public enum eCameraDepth
{
    Camera_Depth_One = 11,
    Camera_Depth_Two = 15
}

public enum eLayer
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Water = 4,
    UI = 5,
    Ground = 8,
    Axis = 9,
    GameObject = 10,
    Arrow = 11,
    Robot = 12,
    OneUI = 13,
    PopUI = 14,

    AR = 18,
}