using UnityEngine;
using UnityEngine.Rendering;
using Vuforia;

public class VuforiaInitHandler : VuforiaMonoBehaviour
{
    public delegate void VuforiaInitErrorCallback(VuforiaUnity.InitError initResult);

    string mErrorText = string.Empty;
    VuforiaInitErrorCallback mInitErrorCallback;
    EventDelegate.Callback mStartedCallback;

    public void OnVuforiaInitializationError(VuforiaUnity.InitError initError)
    {
        if (initError != VuforiaUnity.InitError.INIT_SUCCESS)
        {
            SetErrorCode(initError);
        }
        if (null != mInitErrorCallback)
        {
            mInitErrorCallback(initError);
        }
    }

    public void SetInitErrorCallback(VuforiaInitErrorCallback callback)
    {
        mInitErrorCallback = callback;
    }

    public void SetVuforiaStartedCallback(EventDelegate.Callback callback)
    {
        mStartedCallback = callback;
    }

    void Awake()
    {
        // Check for an initialization error on start.
        VuforiaRuntime.Instance.RegisterVuforiaInitErrorCallback(OnVuforiaInitializationError);
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterVuforiaInitializedCallback(OnVuforiaInitialized);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPaused);
        GameObject backgroundPlane = GameObject.Find("ARCamera/BackgroundPlane");
        if (null != backgroundPlane)
        {
            PublicFunction.SetLayerRecursively(backgroundPlane, backgroundPlane.transform.parent.gameObject.layer);
            MeshRenderer mr = backgroundPlane.GetComponent<MeshRenderer>();
            if (null != mr)
            {
                mr.lightProbeUsage = LightProbeUsage.Off;
                mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
                mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                mr.allowOcclusionWhenDynamic = false;
            }
        }

    }

    void OnDestroy()
    {
        VuforiaRuntime.Instance.UnregisterVuforiaInitErrorCallback(OnVuforiaInitializationError);
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.UnregisterVuforiaInitializedCallback(OnVuforiaInitialized);
        VuforiaARController.Instance.UnregisterOnPauseCallback(OnPaused);
    }

    void SetErrorCode(VuforiaUnity.InitError errorCode)
    {
        switch (errorCode)
        {
            case VuforiaUnity.InitError.INIT_EXTERNAL_DEVICE_NOT_DETECTED:
                mErrorText =
                    "Failed to initialize Vuforia because this " +
                    "device is not docked with required external hardware.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_MISSING_KEY:
                mErrorText =
                    "Vuforia App key is missing. Please get a valid key " +
                    "by logging into your account at developer.vuforia.com " +
                    "and creating a new project.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_INVALID_KEY:
                mErrorText =
                    "Vuforia App key is invalid. " +
                    "Please get a valid key by logging into your account at " +
                    "developer.vuforia.com and creating a new project.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_NO_NETWORK_TRANSIENT:
                mErrorText = "Unable to contact server. Please try again later.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_NO_NETWORK_PERMANENT:
                mErrorText = "No network available. Please make sure you are connected to the Internet.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_CANCELED_KEY:
                mErrorText =
                    "This App license key has been cancelled and may no longer be used. " +
                    "Please get a new license key. \n\n";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_PRODUCT_TYPE_MISMATCH:
                mErrorText =
                    "Vuforia App key is not valid for this product. Please get a valid key " +
                    "by logging into your account at developer.vuforia.com and choosing the " +
                    "right product type during project creation. \n\n" +
                    "Note that Universal Windows Platform (UWP) apps require " +
                    "a license key created on or after August 9th, 2016.";
                break;
            case VuforiaUnity.InitError.INIT_NO_CAMERA_ACCESS:
                mErrorText =
                    "User denied Camera access to this app.\n" +
                    "To restore, enable Camera access in Settings:\n" +
                    "Settings > Privacy > Camera > " + Application.productName + "\n" +
                    "Also verify that the Camera is enabled in:\n" +
                    "Settings > General > Restrictions.";
                break;
            case VuforiaUnity.InitError.INIT_DEVICE_NOT_SUPPORTED:
                mErrorText = "Failed to initialize Vuforia because this device is not supported.";
                break;
            case VuforiaUnity.InitError.INIT_ERROR:
                mErrorText = "Failed to initialize Vuforia.";
                break;
        }
    }

    void OnVuforiaInitialized()
    {
        VuforiaConfiguration.Instance.DeviceTracker.FusionMode = FusionProviderType.OPTIMIZE_MODEL_TARGETS_AND_SMART_TERRAIN;
        var deviceTracker = TrackerManager.Instance.InitTracker<PositionalDeviceTracker>();
        deviceTracker.Start();
    }

    void OnVuforiaStarted()
    {
        //CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
        if (null != mStartedCallback)
        {
            mStartedCallback();
        }
    }

    void OnPaused(bool paused)
    {
        Debug.Log("VuforiaARController OnPaused paused = " + paused);
    }

    void BeforeDevicePoseUpdateCallback()
    {
        Debug.Log("DeviceTrackerARController BeforeDevicePoseUpdateCallback");
    }

    void DevicePoseStatusChangedCallback(TrackableBehaviour.Status status, TrackableBehaviour.StatusInfo statusInfo)
    {
        Debug.LogFormat("DeviceTrackerARController DevicePoseStatusChangedCallback status = {0} statusInfo = {1}", status, statusInfo);
    }

    void DevicePoseUpdatedCallback()
    {
        Debug.Log("DeviceTrackerARController DevicePoseUpdatedCallback");
    }

    void TrackerStartedCallback()
    {
        Debug.Log("DeviceTrackerARController TrackerStartedCallback");
    }
}