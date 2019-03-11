using Game;
using Game.Platform;
using Game.Resource;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARScene : BaseScene
{
    protected Transform mARTrans;
    protected Transform mARCameraTrans;
    protected Vector3 mARCameraLocalposition = Vector3.zero;
    protected Camera mARCamera;
    protected Transform mSceneRoot;
    protected UILabel mBottomLabel;
    protected GameObject mRecognitionObj;
    protected Transform mTargetObjTrans;
    protected ModelTrackerBehaviour mModelTracker;

    bool mCameraTrackingFlag = false;
    int mCameraTrackingCount = 0;

    long mRecognitionTimeIndex = -1;
    int mRecognitionFailedCount = 0;

    long mStartGameIndex = -1;

    bool mFirstTrackingFoundFlag = false;
    public ARScene()
    {
        mResPath = "ARGame/Scene/arRecognitionUI";
    }

    public override void LoadScene()
    {
        try
        {
            base.LoadScene();
            GameObject obj = ResourcesEx.Load<GameObject>("ARGame/Scene/ARScene") as GameObject;
            if (null != obj)
            {
                GameObject o = UnityEngine.Object.Instantiate(obj) as GameObject;
                o.name = obj.name;
                mARTrans = o.transform;
                mARTrans.parent = null;
                mARTrans.localPosition = Vector3.zero;
                mARTrans.localScale = Vector3.one;
                mARTrans.localEulerAngles = Vector3.zero;
                mARCameraTrans = mARTrans.Find("ARCamera");
                if (null != mARCameraTrans)
                {
                    mARCamera = mARCameraTrans.GetComponent<Camera>();
                }
                mTargetObjTrans = mARTrans.Find("ObjectTarget");
                mSceneRoot = mARTrans.Find("root");
                if (null != mSceneRoot)
                {
                    mModelTracker = mSceneRoot.GetComponent<ModelTrackerBehaviour>();
                }

                VuforiaInitHandler handler = GameHelper.FindChildComponent<VuforiaInitHandler>(mARTrans, "ARCamera");
                if (null != handler)
                {
                    handler.SetInitErrorCallback(OnVuforiaInitializationError);
                    handler.SetVuforiaStartedCallback(OnVuforiaStarted);
                }

                VuforiaTrackableEventHandler trackableHandler = GameHelper.FindChildComponent<VuforiaTrackableEventHandler>(mARTrans, "ObjectTarget");
                if (null != trackableHandler)
                {
                    trackableHandler.SetOnTrackingFound(OnTrackingFound);
                    trackableHandler.SetOnTrackingLost(OnTrackingLost);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            if (null != mTrans)
            {
                Transform recognition = mTrans.Find("center/recognition");
                if (null != recognition)
                {
                    mRecognitionObj = recognition.gameObject;
                }
                Transform topRight = mTrans.Find("topRight");
                if (null != topRight)
                {
                    GameHelper.SetPosition(topRight, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos);
                }
                Transform bottom = mTrans.Find("bottom");
                if (null != bottom)
                {
                    GameHelper.SetPosition(bottom, UIWidget.Pivot.Bottom, Vector2.zero);
                    UISprite bg = GameHelper.FindChildComponent<UISprite>(bottom, "bg");
                    if (null != bg)
                    {
                        bg.width = PublicFunction.GetExtendWidth();
                    }
                    mBottomLabel = GameHelper.FindChildComponent<UILabel>(bottom, "Label");
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void Close()
    {
        base.Close();
        if (null != mARTrans)
        {
            GameObject.DestroyImmediate(mARTrans.gameObject);
            mARTrans = null;
        }
        CancelStartGameTimer();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        if (null != mARCameraTrans)
        {
            if (mCameraTrackingFlag)
            {
                if (mARCameraTrans.localPosition.x == mARCameraLocalposition.x 
                    && mARCameraTrans.localPosition.y == mARCameraLocalposition.y 
                    && mARCameraTrans.localPosition.z == mARCameraLocalposition.z)
                {
                    ++mCameraTrackingCount;
                    if (mCameraTrackingCount > 15)
                    {//退出跟踪
                        OnCameraTrackingStop();
                        mCameraTrackingCount = 0;
                        mCameraTrackingFlag = false;
                    }
                } else
                {
                    mCameraTrackingCount = 0;
                }
            } else
            {
                if (mARCameraTrans.localPosition.x != mARCameraLocalposition.x
                    || mARCameraTrans.localPosition.y != mARCameraLocalposition.y
                    || mARCameraTrans.localPosition.z != mARCameraLocalposition.z)
                {
                    ++mCameraTrackingCount;
                    if (mCameraTrackingCount > 2)
                    {//进入跟踪状态
                        OnCameraTrackingStart();
                        mCameraTrackingCount = 0;
                        mCameraTrackingFlag = true;
                    }
                }
            }
            mARCameraLocalposition = mARCameraTrans.localPosition;
        }
    }

    protected override void OnButtonClick(GameObject obj)
    {
        try
        {
            base.OnButtonClick(obj);
            string name = obj.name;
            if (name.Equals("Btn_back"))
            {//返回到游戏菜单页
                BackGameMenu();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    
    protected virtual void OnVuforiaInitializationError(VuforiaUnity.InitError initError)
    {
        if (initError != VuforiaUnity.InitError.INIT_SUCCESS)
        {
            
            
        }
    }
    /// <summary>
    /// vuforia初始化完成
    /// </summary>
    protected virtual void OnVuforiaStarted()
    {
        SetBottomabel("请将摄像头对准独角兽");
        CheckRecognitionResult();
    }
    /// <summary>
    /// 识别到了模型
    /// </summary>
    protected virtual void OnTrackingFound()
    {
        CancelRecognitionCheck();
        mRecognitionFailedCount = 0;
        HideRecognitionObj();
        if (null != mModelTracker)
        {
            mModelTracker.OnTrackingFound();
        }
        if (!mFirstTrackingFoundFlag)
        {
            FirstTrackingFound();
            mFirstTrackingFoundFlag = true;
        }
    }
    /// <summary>
    /// 丢失模型
    /// </summary>
    protected virtual void OnTrackingLost()
    {
        if (null != mModelTracker)
        {
            mModelTracker.OnTrackingLost();
        }
    }
    /// <summary>
    /// 摄像头开始跟踪
    /// </summary>
    protected virtual void OnCameraTrackingStart()
    {
        Debug.Log("摄像头进入跟踪状态");
        if (null != mModelTracker)
        {
            mModelTracker.OnCameraTrackingStart();
        }
    }
    /// <summary>
    /// 摄像头停止追踪
    /// </summary>
    protected virtual void OnCameraTrackingStop()
    {
        Debug.Log("摄像头退出跟踪状态");
        if (null != mModelTracker)
        {
            mModelTracker.OnCameraTrackingStop();
        }
    }
    /// <summary>
    /// 第一次识别到模型，会调用ar3d场景加载函数
    /// </summary>
    protected virtual void FirstTrackingFound()
    {
        SetBottomabel("进入彩虹世界");
        LoadAR3DScene(mSceneRoot);
        CancelStartGameTimer();
        mStartGameIndex = Timer.Add(3, 1, 1, StartGame);
    }

    /// <summary>
    /// 返回游戏菜单页
    /// </summary>
    protected virtual void BackGameMenu()
    {

    }

    /// <summary>
    /// 加载ar3d场景
    /// </summary>
    /// <param name="sceneTrans">3d场景根节点</param>
    protected virtual void LoadAR3DScene(Transform sceneTrans)
    {

    }
    /// <summary>
    /// 加载3d场景完成
    /// </summary>
    protected virtual void LoadAR3DSceneComplete()
    {

    }
    /// <summary>
    /// 开始游戏
    /// </summary>
    protected virtual void StartGame()
    {
        HideRecognitionUI();
    }

    protected void SetBottomabel(string text)
    {
        if (null != mBottomLabel)
        {
            mBottomLabel.text = text;
        }
    }

    void ShowSceneRoot()
    {
        if (null != mSceneRoot)
        {
            var rendererComponents = mSceneRoot.GetComponentsInChildren<Renderer>(true);
            var colliderComponents = mSceneRoot.GetComponentsInChildren<Collider>(true);

            // Enable rendering:
            foreach (var component in rendererComponents)
                component.enabled = true;

            // Enable colliders:
            foreach (var component in colliderComponents)
                component.enabled = true;            
        }
    }

    void HideSceneRoot()
    {
        if (null != mSceneRoot)
        {
            var rendererComponents = mSceneRoot.GetComponentsInChildren<Renderer>(true);
            var colliderComponents = mSceneRoot.GetComponentsInChildren<Collider>(true);

            // Enable rendering:
            foreach (var component in rendererComponents)
                component.enabled = false;

            // Enable colliders:
            foreach (var component in colliderComponents)
                component.enabled = false;
        }
    }

    void CheckRecognitionResult()
    {
        CancelRecognitionCheck();
        mRecognitionTimeIndex = Timer.Add(10, 1, 1, RecognitionOutTime);
    }

    void RecognitionOutTime()
    {
        mRecognitionTimeIndex = -1;
        mRecognitionFailedCount++;
        //TrackerManager.Instance.GetTracker<ObjectTracker>().Stop();
        if (mRecognitionFailedCount == 1)
        {
            SetBottomabel("请把独角兽放在明亮、宽敞、平坦的地方");
        } else if (mRecognitionFailedCount == 2)
        {
            SetBottomabel("尝试从其他角度识别独角兽");
        } else if (mRecognitionFailedCount == 3)
        {
            SetBottomabel("请将摄像头对准独角兽");
            mRecognitionFailedCount = 0;
        }
        CheckRecognitionResult();
    }

    void CancelRecognitionCheck()
    {
        if (-1 != mRecognitionTimeIndex)
        {
            Timer.Cancel(mRecognitionTimeIndex);
            mRecognitionTimeIndex = -1;
        }
    }

    void CancelStartGameTimer()
    {
        if (-1 != mStartGameIndex)
        {
            Timer.Cancel(mStartGameIndex);
            mStartGameIndex = -1;
        }
    }

    void HideRecognitionObj()
    {
        if (null != mRecognitionObj)
        {
            mRecognitionObj.SetActive(false);
        }
    }

    void HideRecognitionUI()
    {
        if (null != mTrans)
        {
            mTrans.gameObject.SetActive(false);
        }
    }

    void ShowRecognitionUI()
    {
        if (null != mTrans)
        {
            mTrans.gameObject.SetActive(true);
        }
    }    
}
