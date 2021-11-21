/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

public class VideoObserverEventHandler : DefaultObserverEventHandler
{
    VideoController mVideoController;

    protected override void Start()
    {
        base.Start();

        if (mObserverBehaviour)
            mVideoController = mObserverBehaviour.GetComponentInChildren<VideoController>();
    }

    protected override void OnTrackingLost()
    {
        if(mVideoController)
            mVideoController.Pause();
        base.OnTrackingLost();
    }
}
