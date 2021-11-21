/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Vuforia;

public class GroundPlaneUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Text Title;
    [SerializeField] Text Instructions;
    [SerializeField] CanvasGroup ScreenReticle;
    [Header("UI Buttons")]
    [SerializeField] Toggle PlacementToggle;
    [SerializeField] Toggle GroundToggle;
    [SerializeField] Toggle MidAirToggle;
    
    const string TITLE_PRODUCT_PLACEMENT = "Product Placement";
    const string TITLE_GROUND_PLANE = "Ground Plane";
    const string TITLE_MID_AIR = "Mid-Air";
    
    const string ASTRONAUT_PLACEMENT_HINT = "Tap to place Astronaut";
    const string CHAIR_PLACEMENT_HINT = "Tap to place Chair";
    const string DRONE_PLACEMENT_HINT = "Tap to place Drone";
    const string CHAIR_CONTROLS_WITH_PINCH = "• Touch and drag to move Chair\n• Two fingers to rotate or pinch to scale";
    const string CHAIR_CONTROLS_WITHOUT_PINCH = "• Touch and drag to move Chair\n• Two fingers to rotate";
    const string POINT_DEVICE_TO_GROUND_HINT = "Point device towards ground";
    const string MOVE_TO_ANOTHER_PLACE_HINT = "Move to get better tracking for placing an anchor";
    const string BETTER_TRACKING_IS_NEEDED = "Need better tracking to place a Mid-Air Anchor";

    GraphicRaycaster mGraphicRayCaster;
    PointerEventData mPointerEventData;
    EventSystem mEventSystem;
    PlaneManager mPlaneManager;
    ProductPlacement mProductPlacement;
    TouchHandler mTouchHandler;

    void Start()
    {
        mPlaneManager = FindObjectOfType<PlaneManager>();
        mProductPlacement = FindObjectOfType<ProductPlacement>();
        mTouchHandler = FindObjectOfType<TouchHandler>();
        mGraphicRayCaster = FindObjectOfType<GraphicRaycaster>();
        mEventSystem = FindObjectOfType<EventSystem>();

        VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        
        UpdateUI();
    }

    void Update()
    {
        MidAirToggle.interactable = mPlaneManager.TrackingStatusIsTrackedAndNormal;
        GroundToggle.interactable = mPlaneManager.TrackingStatusIsTrackedAndNormal;
        PlacementToggle.interactable = mPlaneManager.TrackingStatusIsTrackedAndNormal;
    }

    void LateUpdate()
    {
        if (mPlaneManager.GroundPlaneHitReceived && mPlaneManager.TrackingStatusIsTrackedAndNormal)
        {
            // We got an automatic hit test this frame, hide the onscreen reticle when we get a hit test
            ScreenReticle.alpha = 0;
            Instructions.transform.parent.gameObject.SetActive(true);
            Instructions.enabled = true;

            if (mPlaneManager.CurrentPlaneMode == PlaneMode.GROUND)
            {
                Instructions.text = ASTRONAUT_PLACEMENT_HINT;
            }
            else if (mPlaneManager.CurrentPlaneMode == PlaneMode.PLACEMENT)
            {
                if (mProductPlacement.IsPlaced && mProductPlacement.IsAnchorTracking)
                    Instructions.text = mTouchHandler.EnablePinchScaling ? CHAIR_CONTROLS_WITH_PINCH : CHAIR_CONTROLS_WITHOUT_PINCH;
                else
                    Instructions.text = CHAIR_PLACEMENT_HINT;
            }
        }
        else
        {
            // No automatic hit test, so set alpha based on which plane mode is active
            if (!mPlaneManager.GroundPlaneHitReceived)
                ScreenReticle.alpha = mPlaneManager.CurrentPlaneMode != PlaneMode.MID_AIR ? 1 : 0;

            Instructions.transform.parent.gameObject.SetActive(true);
            Instructions.enabled = true;

            if (mPlaneManager.CurrentPlaneMode != PlaneMode.MID_AIR)
                Instructions.text = mPlaneManager.GroundPlaneHitReceived ? MOVE_TO_ANOTHER_PLACE_HINT : POINT_DEVICE_TO_GROUND_HINT;
            else
                Instructions.text = mPlaneManager.TrackingStatusIsTrackedAndNormal ? DRONE_PLACEMENT_HINT : BETTER_TRACKING_IS_NEEDED;
        }
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy() called.");
        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    public void UpdateUI()
    {
        PlacementToggle.isOn = mPlaneManager.CurrentPlaneMode == PlaneMode.PLACEMENT;
        GroundToggle.isOn = mPlaneManager.CurrentPlaneMode == PlaneMode.GROUND;
        MidAirToggle.isOn = mPlaneManager.CurrentPlaneMode == PlaneMode.MID_AIR;

        switch (mPlaneManager.CurrentPlaneMode)
        {
            case PlaneMode.GROUND:
                Title.text = TITLE_GROUND_PLANE;
                break;
            case PlaneMode.MID_AIR:
                Title.text = TITLE_MID_AIR;
                break;
            case PlaneMode.PLACEMENT:
                Title.text = TITLE_PRODUCT_PLACEMENT;
                break;
        }
    }

    public bool IsCanvasButtonPressed()
    {
        mPointerEventData = new PointerEventData(mEventSystem) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        mGraphicRayCaster.Raycast(mPointerEventData, results);
        var resultIsButton = false;
        foreach (var result in results)
        {
            if (result.gameObject.GetComponentInParent<Toggle>() != null ||
                result.gameObject.GetComponent<Button>() != null)
            {
                resultIsButton = true;
                break;
            }
        }
        return resultIsButton;
    }
    
    void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        var statusMessage = "";
        
        switch (targetStatus.StatusInfo)
        {
            case StatusInfo.NORMAL:
                statusMessage = "";
                break;
            case StatusInfo.UNKNOWN:
                statusMessage = "Limited Status";
                break;
            case StatusInfo.INITIALIZING:
                statusMessage = "Point your device to the floor and move to scan";
                break;
            case StatusInfo.EXCESSIVE_MOTION:
                statusMessage = "Move slower";
                break;
            case StatusInfo.INSUFFICIENT_FEATURES:
                statusMessage = "Not enough visual features in the scene";
                break;
            case StatusInfo.INSUFFICIENT_LIGHT:
                statusMessage = "Not enough light in the scene";
                break;
            case StatusInfo.RELOCALIZING:
                // Display a relocalization message in the UI if:
                // * No AnchorBehaviours are being tracked
                // * None of the active/tracked AnchorBehaviours are in TRACKED status

                // Set the status message now and clear it if none of the conditions are met.
                statusMessage = "Point back to previously seen area and rescan to relocalize.";

                var activeAnchors = FindObjectsOfType<AnchorBehaviour>();
                // Cycle through all of the active AnchorBehaviours.
                foreach (var anchor in activeAnchors)
                {
                    if (anchor.TargetStatus.Status == Status.TRACKED)
                    {
                        // If at least one of the AnchorBehaviours has Tracked status,
                        // then don't display the relocalization message.
                        statusMessage = "";
                        break;
                    }
                }
                break;
        }

        StatusMessage.Instance.Display(statusMessage);
    }
}
