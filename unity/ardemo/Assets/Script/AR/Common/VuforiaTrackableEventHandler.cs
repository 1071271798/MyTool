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
        base.OnTrackingFound();
        if (null != mTrackingFound)
        {
            mTrackingFound();
        }
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        if (null != mTrackingLost)
        {
            mTrackingLost();
        }
    }
}