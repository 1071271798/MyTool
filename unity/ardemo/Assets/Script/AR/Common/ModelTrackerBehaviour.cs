using UnityEngine;
public class ModelTrackerBehaviour : MonoBehaviour
{
    public Transform target;
    public Transform arCameraTrans;
    public Camera arCamera;

    bool isShow = true;
    bool isTracking = false;
    Transform mTrans;
    TrackingCorrection mTrackingCorrection;
    int mFrameCount;

    public void OnTrackingFound()
    {
        if (!isShow)
        {
            ShowObject();
        }
        isTracking = true;
        mFrameCount = 0;
    }

    public void OnTrackingLost()
    {
        isTracking = false;
    }

    /// <summary>
    /// 摄像头开始跟踪
    /// </summary>
    public void OnCameraTrackingStart()
    {
        if (!isShow)
        {
            ShowObject();
        }
    }
    /// <summary>
    /// 摄像头停止追踪
    /// </summary>
    public void OnCameraTrackingStop()
    {
        if (isShow)
        {
            HideObject();
        }
    }

    void Awake()
    {
        mTrans = transform;
        mTrackingCorrection = new TrackingCorrection(OnChangeTransformDataCallback);
    }
    void OnEnable()
    {
        HideObject();
    }

    void OnDisable()
    {

    }

    void ShowObject()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = true;

        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = true;
        isShow = true;
    }

    void HideObject()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);

        // Disable rendering:
        foreach (var component in rendererComponents)
            component.enabled = false;

        // Disable colliders:
        foreach (var component in colliderComponents)
            component.enabled = false;
        isShow = false;
    }

    void LateUpdate()
    {
        if (mFrameCount < 10)
        {
            if (isTracking)
            {
                ++mFrameCount;
            }
            return;
        }
        if (isTracking && null != mTrans && null != target)
        {
            mTrackingCorrection.AddTargetTransformData(target.localPosition, target.localEulerAngles);
        }
        /*if (null != mTrans)
        {
            IsInView(mTrans.position);
        }*/
        
        /*if (isTracking && null != mTrans && null != mTarget && (Vector3.Distance(mTarget.localPosition, mTrans.localPosition) >= 5 || Vector3.Distance(mTarget.localEulerAngles, mTrans.localEulerAngles) >= 10))
        {
            mTrans.localPosition = mTarget.localPosition;
            mTrans.localEulerAngles = mTarget.localEulerAngles;
            isTracking = false;
        }*/
    }
    
    void OnChangeTransformDataCallback(Vector3 pos, Vector3 angle)
    {
        if (null != mTrans)
        {
            //MyLog.Log(string.Format("change Transform localPosition = {0},{1},{2} localEulerAngles = {3},{4},{5}", pos.x, pos.y, pos.z, angle.x, angle.y, angle.z));
            mTrans.localPosition = Vector3.Lerp(mTrans.localPosition, pos, 8);
            mTrans.localEulerAngles = Vector3.Lerp(mTrans.localEulerAngles, angle, 8);
            //mTrans.localPosition = pos;
            //mTrans.localEulerAngles = angle;
            //isTracking = false;
        }
    }
    bool IsInView(Vector3 worldPos)
    {
        if (null == arCameraTrans || null == arCamera)
        {
            return false;
        }
        Vector2 viewPos = arCamera.WorldToViewportPoint(worldPos);
        Vector3 dir = (worldPos - arCameraTrans.position).normalized;
        float dot = Vector3.Dot(arCameraTrans.forward, dir);//判断物体是否在相机前面
        float angle = Vector3.Angle(mTrans.position - arCameraTrans.position, arCameraTrans.forward);
        float dis = Mathf.Cos(angle / 180 * Mathf.PI) * Vector3.Distance(mTrans.position, arCameraTrans.position);
        Debug.Log("arCameraTrans pos = " + arCameraTrans.localPosition + " forward = " + arCameraTrans.forward + " model pos = " + mTrans.localPosition + " angle = " + angle + " dis = " + dis);
        if (dis > 0)
        {
            Rect rect = GetRect(dis);
            if (mTrans.position.x >= rect.x && mTrans.position.y >= rect.y && mTrans.position.x <= rect.x + rect.width && mTrans.position.y <= rect.y + rect.height)
            {
                Debug.Log("框内");
            } else
            {
                Debug.Log("框外");
            }
        }
        if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
        {
            /*if (viewPos.x < 0)
            {
                Debug.Log("左");
            }
            else if (viewPos.x > 1)
            {
                Debug.Log("右");
            }
            else if (viewPos.y < 0)
            {
                Debug.Log("下");
            }
            else if (viewPos.y > 1)
            {
                Debug.Log("上");
            }*/
            return false;
        }
    }

    Vector3[] GetCorners(float distance)
    {
        Vector3[] corners = new Vector3[4];

        float halfFOV = (arCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
        float aspect = arCamera.aspect;

        float height = distance * Mathf.Tan(halfFOV);
        float width = height * aspect;

        // UpperLeft
        corners[0] = arCameraTrans.position - (arCameraTrans.right * width);
        corners[0] += arCameraTrans.up * height;
        corners[0] += arCameraTrans.forward * distance;

        // UpperRight
        corners[1] = arCameraTrans.position + (arCameraTrans.right * width);
        corners[1] += arCameraTrans.up * height;
        corners[1] += arCameraTrans.forward * distance;

        // LowerLeft
        corners[2] = arCameraTrans.position - (arCameraTrans.right * width);
        corners[2] -= arCameraTrans.up * height;
        corners[2] += arCameraTrans.forward * distance;

        // LowerRight
        corners[3] = arCameraTrans.position + (arCameraTrans.right * width);
        corners[3] -= arCameraTrans.up * height;
        corners[3] += arCameraTrans.forward * distance;

        return corners;
    }

    Rect GetRect(float distance)
    {
        Vector3[] corners = new Vector3[4];

        float halfFOV = (arCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
        float aspect = arCamera.aspect;

        float height = distance * Mathf.Tan(halfFOV);
        float width = height * aspect;
        Vector3 leftPos = arCameraTrans.position - (arCameraTrans.right * width);
        leftPos += arCameraTrans.up * height;
        leftPos += arCameraTrans.forward * distance;
        Rect rect = new Rect(leftPos.x, leftPos.y, width, height);
        return rect;
    }

}