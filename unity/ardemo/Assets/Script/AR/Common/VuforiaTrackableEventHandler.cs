using UnityEngine;

public class VuforiaTrackableEventHandler : DefaultTrackableEventHandler
{
    protected EventDelegate.Callback mTrackingFound;
    protected EventDelegate.Callback mTrackingLost;

    public void SetOnTrackingFound(EventDelegate.Callback callback)
    {
        mTrackingFound = callback;
    }

    public void SetOnTrackingLost(EventDelegate.Callback callback)
    {
        mTrackingLost = callback;
    }


    protected override void OnTrackingFound()
    {
        /*var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = true;

        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = true;*/

        if (null != mTrackingFound)
        {
            mTrackingFound();
        }
    }

    protected override void OnTrackingLost()
    {
        /*base.OnTrackingLost();*/
        if (null != mTrackingLost)
        {
            mTrackingLost();
        }
    }
}