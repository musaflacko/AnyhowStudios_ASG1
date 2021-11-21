/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using System.Timers;
using UnityEngine;
using Vuforia;

public class PlaneManager : MonoBehaviour
{
    [Header("Plane, Mid-Air, & Placement Augmentations")]
    [SerializeField] GameObject PlaneAugmentation;
    [SerializeField] GameObject MidAirAugmentation;
    [SerializeField] GameObject PlacementAugmentation;

    const int RELOCALIZATION_TIMER = 10000;
    const string UNSUPPORTED_DEVICE_TITLE = "Unsupported Device";
    const string UNSUPPORTED_DEVICE_BODY = "This device does not support anchors and hit test functionality. " +
                                           "Please check the list of supported Ground Plane devices on our site: " +
                                           "\n\nhttps://library.vuforia.com/articles/Solution/ground-plane-supported-devices.html";

    PlaneFinderBehaviour mPlaneFinder;
    MidAirPositionerBehaviour mMidAirPositioner;
    
    ContentPositioningBehaviour mContentPositioningBehaviour;
    TouchHandler mTouchHandler;
    ProductPlacement mProductPlacement;
    GroundPlaneUI mGroundPlaneUI;
    AnchorBehaviour mPlaneAnchor, mMidAirAnchor, mPlacementAnchor;
    Timer mTimer;
    int mAutomaticHitTestFrameCount;
    bool mTimerFinished;
    TargetStatus mCachedStatus = TargetStatus.NotObserved;
    Vector3 mOriginalPlaneScale;
    Vector3 mOriginalMidAirScale;

    public PlaneMode CurrentPlaneMode { get; private set; } = PlaneMode.PLACEMENT;
    
    public bool GroundPlaneHitReceived { get; private set; }

    /// <summary>
    /// More Strict: Property returns true when Status is Tracked and StatusInfo is Normal.
    /// </summary>
    public bool TrackingStatusIsTrackedAndNormal => (mCachedStatus.Status == Status.TRACKED || 
                                                     mCachedStatus.Status == Status.EXTENDED_TRACKED) &&
                                                     mCachedStatus.StatusInfo == StatusInfo.NORMAL;

    /// <summary>
    /// Less Strict: Property returns true when Status is Tracked/Normal or Limited/Unknown.
    /// </summary>
    public bool TrackingStatusIsTrackedOrLimited => (mCachedStatus.Status == Status.TRACKED ||
                                                     mCachedStatus.Status == Status.EXTENDED_TRACKED) &&
                                                     mCachedStatus.StatusInfo == StatusInfo.NORMAL ||
                                                     mCachedStatus.Status == Status.LIMITED && mCachedStatus.StatusInfo == StatusInfo.UNKNOWN;

    /// <summary>
    /// The Surface Indicator should only be visible if the following conditions are true:
    /// 1. Tracking Status is Tracked or Limited (sufficient for Hit Test Anchors)
    /// 2. Ground Plane Hit was received for this frame
    /// 3. The Plane Mode is equal to GROUND or PLACEMENT(see #4)
    /// 4. If the Plane Mode is equal to PLACEMENT and *there's no active touches
    /// </summary>
    bool SurfaceIndicatorVisibilityConditionsMet => TrackingStatusIsTrackedOrLimited &&
                                                    GroundPlaneHitReceived &&
                                                    (CurrentPlaneMode == PlaneMode.GROUND || 
                                                     CurrentPlaneMode == PlaneMode.PLACEMENT && Input.touchCount == 0);

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;
        VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;

        mPlaneFinder = FindObjectOfType<PlaneFinderBehaviour>();
        mMidAirPositioner = FindObjectOfType<MidAirPositionerBehaviour>();
        mContentPositioningBehaviour = mPlaneFinder.GetComponent<ContentPositioningBehaviour>();

        mPlaneFinder.HitTestMode = HitTestMode.AUTOMATIC;

        mProductPlacement = FindObjectOfType<ProductPlacement>();
        mTouchHandler = FindObjectOfType<TouchHandler>();
        mGroundPlaneUI = FindObjectOfType<GroundPlaneUI>();

        mPlaneAnchor = PlaneAugmentation.GetComponentInParent<AnchorBehaviour>();
        mMidAirAnchor = MidAirAugmentation.GetComponentInParent<AnchorBehaviour>();
        mPlacementAnchor = PlacementAugmentation.GetComponentInParent<AnchorBehaviour>();
        mOriginalPlaneScale = PlaneAugmentation.transform.localScale;
        mOriginalMidAirScale = MidAirAugmentation.transform.localScale;

        GroundPlaneUtilities.EnableRendererColliderCanvas(PlaneAugmentation, false);
        GroundPlaneUtilities.EnableRendererColliderCanvas(MidAirAugmentation, false);
        GroundPlaneUtilities.EnableRendererColliderCanvas(PlacementAugmentation, false);

        // Setup a timer to restart the DeviceTracker if tracking does not receive
        // status change from StatusInfo.RELOCALIZATION after 10 seconds.
        mTimer = new Timer(RELOCALIZATION_TIMER);
        mTimer.Elapsed += TimerFinished;
        mTimer.AutoReset = false;
    }

    void Update()
    {
        // The timer runs on a separate thread and we need to ResetTrackers on the main thread.
        if (mTimerFinished)
        {
            ResetDevicePoseBehaviour();
            ResetScene();
            mTimerFinished = false;
        }
    }

    void LateUpdate()
    {
        // The AutomaticHitTestFrameCount is assigned the Time.frameCount in the
        // HandleAutomaticHitTest() callback method. When the LateUpdate() method
        // is then called later in the same frame, it sets GroundPlaneHitReceived
        // to true if the frame number matches. For any code that needs to check
        // the current frame value of GroundPlaneHitReceived, it should do so
        // in a LateUpdate() method.
        GroundPlaneHitReceived = mAutomaticHitTestFrameCount == Time.frameCount;

        // Surface Indicator visibility conditions rely upon GroundPlaneHitReceived,
        // so we will move this method into LateUpdate() to ensure that it is called
        // after GroundPlaneHitReceived has been updated in Update().
        SetSurfaceIndicatorVisible(SurfaceIndicatorVisibilityConditionsMet);
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy() called.");
        if (VuforiaBehaviour.Instance != null) 
            VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    public void HandleAutomaticHitTest(HitTestResult result)
    {
        mAutomaticHitTestFrameCount = Time.frameCount;

        if (CurrentPlaneMode == PlaneMode.PLACEMENT && !mProductPlacement.IsPlaced)
        {
            mProductPlacement.DetachProductFromAnchor();
            PlacementAugmentation.transform.position = result.Position;
        }
    }
    
    public void HandleInteractiveHitTest(HitTestResult result)
    {
        if (result == null)
        {
            Debug.LogError("Invalid hit test result!");
            return;
        }

        if (!mGroundPlaneUI.IsCanvasButtonPressed())
        {
            Debug.Log("HandleInteractiveHitTest() called.");
            
            // If the PlaneFinderBehaviour's Mode is Automatic, then the Interactive HitTestResult will be centered.
            // PlaneMode.Ground and PlaneMode.Placement both use PlaneFinder's ContentPositioningBehaviour
            mContentPositioningBehaviour.DuplicateStage = false;

            // Place object based on Ground Plane mode
            switch (CurrentPlaneMode)
            {
                case PlaneMode.GROUND:

                    // With each tap, the Astronaut is moved to the position of the
                    // newly created anchor. Before we set any anchor, we first want
                    // to verify that the Status=TRACKED/EXTENDED_TRACKED and StatusInfo=NORMAL.
                    if (TrackingStatusIsTrackedAndNormal)
                    {
                        mContentPositioningBehaviour.AnchorStage = mPlaneAnchor;
                        mContentPositioningBehaviour.PositionContentAtPlaneAnchor(result);
                        GroundPlaneUtilities.EnableRendererColliderCanvas(PlaneAugmentation, true);

                        // Astronaut should rotate toward camera with each placement
                        PlaneAugmentation.transform.localPosition = Vector3.zero;
                        PlaneAugmentation.transform.localScale = Vector3.Scale(mOriginalPlaneScale, mProductPlacement.ProductScale);
                        GroundPlaneUtilities.RotateTowardsCamera(PlaneAugmentation);
                    }
                    break;

                case PlaneMode.PLACEMENT:

                    // With a tap a new anchor is created, so we first check that
                    // Status=TRACKED/EXTENDED_TRACKED and StatusInfo=NORMAL before proceeding.
                    if (TrackingStatusIsTrackedAndNormal)
                    {
                        // We assign our stage content, set an anchor and enable the content.
                        mContentPositioningBehaviour.AnchorStage = mPlacementAnchor;
                        mContentPositioningBehaviour.PositionContentAtPlaneAnchor(result);
                        GroundPlaneUtilities.EnableRendererColliderCanvas(PlacementAugmentation, true);
                        
                        // If the product has not been placed in the scene yet, we attach it to the anchor
                        // while rotating it to face the camera. Then we activate the content, also
                        // enabling rotation input detection.
                        // Otherwise, we simply attach the content to the new anchor, preserving its rotation.
                        // The placement methods will set the IsPlaced flag to true if the 
                        // transform argument is valid and to false if it is null.
                        if (!mProductPlacement.IsPlaced)
                        {
                            mProductPlacement.PlaceProductAtAnchorFacingCamera(mPlacementAnchor.transform);
                            mTouchHandler.EnableRotation = true;
                        }
                        else
                        {
                            mProductPlacement.PlaceProductAtAnchorAndSnapToMousePosition(mPlacementAnchor.transform);
                        }
                    }
                    break;
            }
        }
    }

    public void PlaceObjectInMidAir(Transform midAirTransform)
    {
        if (CurrentPlaneMode == PlaneMode.MID_AIR)
        {
            Debug.Log("PlaceObjectInMidAir() called.");

            // With each tap, the Drone is moved to the position of the
            // newly created anchor. Before we set any anchor, we first want
            // to verify that the Status=TRACKED/EXTENDED_TRACKED and StatusInfo=NORMAL.
            if (TrackingStatusIsTrackedAndNormal)
            {
                mContentPositioningBehaviour.AnchorStage = mMidAirAnchor;
                mContentPositioningBehaviour.PositionContentAtMidAirAnchor(midAirTransform);
                GroundPlaneUtilities.EnableRendererColliderCanvas(MidAirAugmentation, true);

                MidAirAugmentation.transform.localPosition = Vector3.zero;
                MidAirAugmentation.transform.localScale = Vector3.Scale(mOriginalMidAirScale, mProductPlacement.ProductScale);
                GroundPlaneUtilities.RotateTowardsCamera(MidAirAugmentation);
            }
        }
    }

    public void SetGroundMode(bool active)
    {
        if (active)
            SetMode(PlaneMode.GROUND);
    }

    public void SetMidAirMode(bool active)
    {
        if (active)
            SetMode(PlaneMode.MID_AIR);
    }

    public void SetPlacementMode(bool active)
    {
        if (active)
            SetMode(PlaneMode.PLACEMENT);
    }

    /// <summary>
    /// This method resets the augmentations and scene elements.
    /// It is called by the UI Reset Button and also by OnVuforiaPaused() callback.
    /// </summary>
    public void ResetScene()
    {
        Debug.Log("ResetScene() called.");

        // reset augmentations
        PlaneAugmentation.transform.position = Vector3.zero;
        PlaneAugmentation.transform.localEulerAngles = Vector3.zero;
        GroundPlaneUtilities.EnableRendererColliderCanvas(PlaneAugmentation, false);

        MidAirAugmentation.transform.position = Vector3.zero;
        MidAirAugmentation.transform.localEulerAngles = Vector3.zero;
        GroundPlaneUtilities.EnableRendererColliderCanvas(MidAirAugmentation, false);

        mProductPlacement.Reset();
        GroundPlaneUtilities.EnableRendererColliderCanvas(PlacementAugmentation, false);

        mProductPlacement.DetachProductFromAnchor();
        mGroundPlaneUI.UpdateUI();
        mTouchHandler.EnableRotation = false;
    }

    /// <summary>
    /// This method stops and restarts the DevicePoseBehaviour.
    /// It is called by the UI Reset Button and when RELOCALIZATION status has
    /// not changed for 10 seconds.
    /// </summary>
    public void ResetDevicePoseBehaviour()
    {
        Debug.Log("ResetDevicePoseBehaviour() called.");

        mPlacementAnchor.UnconfigureAnchor();
        mPlaneAnchor.UnconfigureAnchor();
        mMidAirAnchor.UnconfigureAnchor();
        VuforiaBehaviour.Instance.DevicePoseBehaviour.Reset();
    }

    /// <summary>
    /// This private method is called by the UI Button handler methods.
    /// </summary>
    /// <param name="mode">PlaneMode</param>
    void SetMode(PlaneMode mode)
    {
        CurrentPlaneMode = mode;
        mGroundPlaneUI.UpdateUI();
        mPlaneFinder.enabled = mode == PlaneMode.GROUND || mode == PlaneMode.PLACEMENT;
        mMidAirPositioner.enabled = mode == PlaneMode.MID_AIR;
        mTouchHandler.EnableRotation = mode == PlaneMode.PLACEMENT && PlacementAugmentation.activeInHierarchy;
    }

    /// <summary>
    /// This method can be used to set the Ground Plane surface indicator visibility.
    /// This sample will display it when the Status=TRACKED and StatusInfo=Normal.
    /// </summary>
    /// <param name="isVisible">bool</param>
    void SetSurfaceIndicatorVisible(bool isVisible)
    {
        var renderers = mPlaneFinder.PlaneIndicator.GetComponentsInChildren<Renderer>(true);
        var canvases = mPlaneFinder.PlaneIndicator.GetComponentsInChildren<Canvas>(true);

        foreach (var canvas in canvases)
            canvas.enabled = isVisible;

        foreach (var renderer in renderers)
            renderer.enabled = isVisible;
    }

    /// <summary>
    /// This is a C# delegate method for the Timer:
    /// ElapsedEventHandler(object sender, ElapsedEventArgs e)
    /// </summary>
    /// <param name="source">System.Object</param>
    /// <param name="e">ElapsedEventArgs</param>
    void TimerFinished(System.Object source, ElapsedEventArgs e)
    {
        mTimerFinished = true;
    }

    void OnVuforiaInitialized(VuforiaInitError initError)
    {
        if (initError != VuforiaInitError.NONE)
            return;
        
        Debug.Log("OnVuforiaInitialized() called.");

        if (VuforiaBehaviour.Instance.World.AnchorsSupported)
        {
            if (!VuforiaBehaviour.Instance.DevicePoseBehaviour.enabled)
            {
                Debug.LogError("The Ground Plane feature requires the Device Tracking to be started. " +
                               "Please enable it in the Vuforia Configuration or start it at runtime through the scripting API.");
                return;
            }
            
            Debug.Log("DevicePoseBehaviour is Active");
        }
        else
        {
            MessageBox.DisplayMessageBox(UNSUPPORTED_DEVICE_TITLE, UNSUPPORTED_DEVICE_BODY, false, null);
        }
    }

    void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        mCachedStatus = targetStatus;
        
        Debug.Log("PlaneManager.OnTargetStatusChanged(" + mCachedStatus.Status + ", " + mCachedStatus.StatusInfo + ")");

        if (mCachedStatus.StatusInfo != StatusInfo.RELOCALIZING)
        {
            // If the timer is running and the status is no longer Relocalizing, then stop the timer
            if (mTimer.Enabled)
                mTimer.Stop();
        }
        else
        {
            // Start a 10 second timer to Reset Device Tracker
            if (!mTimer.Enabled)
                mTimer.Start();
        }
    }
}
