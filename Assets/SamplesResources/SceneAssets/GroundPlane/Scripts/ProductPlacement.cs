/*============================================================================== 
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/

using UnityEngine;
using Vuforia;
using Vuforia.UnityRuntimeCompiled;

public class ProductPlacement : MonoBehaviour
{
    public bool IsPlaced { get; private set; }
    public bool IsAnchorTracking { get; private set; }

    public Vector3 ProductScale
    {
        get
        {
            var augmentationScale = VuforiaRuntimeUtilities.IsPlayMode() ? 0.1f : ProductSize;
            return new Vector3(augmentationScale, augmentationScale, augmentationScale);
        }
    }

    [Header("Augmentation Objects")]
    [SerializeField] GameObject Chair = null;
    [SerializeField] GameObject ChairShadow = null;

    [Header("Control Indicators")]
    [SerializeField] GameObject TranslationIndicator = null;
    [SerializeField] GameObject RotationIndicator = null;

    [Header("Augmentation Size")]
    [Range(0.1f, 2.0f)]
    [SerializeField] float ProductSize = 0.65f;

    const string RESOURCES_CHAIR_BODY = "ChairBody";
    const string RESOURCES_CHAIR_FRAME = "ChairFrame";
    const string RESOURCES_CHAIR_SHADOW = "ChairShadow";
    const string RESOURCES_CHAIR_BODY_TRANSPARENT = "ChairBodyTransparent";
    const string RESOURCES_CHAIR_FRAME_TRANSPARENT = "ChairFrameTransparent";
    const string RESOURCES_CHAIR_SHADOW_TRANSPARENT = "ChairShadowTransparent";
    const string GROUND_PLANE_NAME = "Emulator Ground Plane";
    const string FLOOR_NAME = "Floor";

    MeshRenderer mChairRenderer;
    MeshRenderer mChairShadowRenderer;
    Material[] mChairMaterials, mChairMaterialsTransparent;
    Material mChairShadowMaterial, mChairShadowMaterialTransparent;
    PlaneManager mPlaneManager;
    GroundPlaneUI mGroundPlaneUI;
    Camera mMainCamera;
    string mFloorName;
    Vector3 mOriginalChairScale;

    // Property which returns whether chair visibility conditions are met
    bool IsChairVisible =>
        // The Chair should only be visible if the following conditions are met:
        // 1. Target Status is Tracked, Extended Tracked or Limited
        // 2. Ground Plane Hit was received for this frame
        // 3. The Plane Mode is equal to PLACEMENT
        mPlaneManager.TrackingStatusIsTrackedOrLimited && mPlaneManager.GroundPlaneHitReceived && 
        mPlaneManager.CurrentPlaneMode == PlaneMode.PLACEMENT;

    void Start()
    {
        mPlaneManager = FindObjectOfType<PlaneManager>();
        mMainCamera = VuforiaBehaviour.Instance.GetComponent<Camera>();
        mGroundPlaneUI = FindObjectOfType<GroundPlaneUI>();
        mChairRenderer = Chair.GetComponent<MeshRenderer>();
        mChairShadowRenderer = ChairShadow.GetComponent<MeshRenderer>();
        SetupMaterials();
        SetupFloor();
        
        mOriginalChairScale = Chair.transform.localScale;
        Reset();
    }


    void Update()
    {
        if (mPlaneManager.CurrentPlaneMode == PlaneMode.PLACEMENT)
        {
            EnablePreviewModeTransparency(!IsPlaced);
            if (!IsPlaced)
                GroundPlaneUtilities.RotateTowardsCamera(Chair);
        }

        if (mPlaneManager.CurrentPlaneMode == PlaneMode.PLACEMENT && IsPlaced)
        {
            RotationIndicator.SetActive(Input.touchCount == 2);

            TranslationIndicator.SetActive((TouchHandler.sIsSingleFingerDragging || TouchHandler.sIsSingleFingerStationary) 
                                            && !mGroundPlaneUI.IsCanvasButtonPressed());

            SnapProductToMousePosition();
        }
        else
        {
            RotationIndicator.SetActive(false);
            TranslationIndicator.SetActive(false);
        }
    }

    void SnapProductToMousePosition()
    {
        if (TouchHandler.sIsSingleFingerDragging || VuforiaRuntimeUtilities.IsPlayMode() && Input.GetMouseButton(0))
        {
            if (!UnityRuntimeCompiledFacade.Instance.IsUnityUICurrentlySelected())
            {
                var cameraToPlaneRay = mMainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(cameraToPlaneRay, out var cameraToPlaneHit) &&
                    cameraToPlaneHit.collider.gameObject.name == mFloorName)
                    Chair.transform.position = cameraToPlaneHit.point;
            }
        }
    }

    void LateUpdate()
    {
        if (!IsPlaced)
            SetVisible(IsChairVisible);
    }
    
    public void Reset()
    {
        Chair.transform.position = Vector3.zero;
        Chair.transform.localEulerAngles = Vector3.zero;
        Chair.transform.localScale = Vector3.Scale(mOriginalChairScale, ProductScale);
        
        IsPlaced = false;
    }

    // Called by Anchor_Placement's DefaultObserverEventHandler.OnTargetFound()
    public void OnAnchorFound()
    {
        IsAnchorTracking = true;
    }

    // Called by Anchor_Placement's DefaultObserverEventHandler.OnTargetLost()
    public void OnAnchorLost()
    {
        IsAnchorTracking = false;
    }

    public void PlaceProductAtAnchorAndSnapToMousePosition(Transform anchor)
    {
        PlaceProductAtAnchor(anchor);
        SnapProductToMousePosition();
    }
    
    void PlaceProductAtAnchor(Transform anchor)
    {
        Chair.transform.SetParent(anchor, true);
        Chair.transform.localPosition = Vector3.zero;
        IsPlaced = true;
    }

    public void PlaceProductAtAnchorFacingCamera(Transform anchor)
    {
        PlaceProductAtAnchor(anchor);
        GroundPlaneUtilities.RotateTowardsCamera(Chair);
    }

    public void DetachProductFromAnchor()
    {
        Chair.transform.SetParent(null);
        Reset();
    }
    
    void SetupMaterials()
    {
        mChairMaterials = new[]
                          {
                              Resources.Load<Material>(RESOURCES_CHAIR_BODY),
                              Resources.Load<Material>(RESOURCES_CHAIR_FRAME)
                          };

        mChairMaterialsTransparent = new[]
                                     {
                                         Resources.Load<Material>(RESOURCES_CHAIR_BODY_TRANSPARENT),
                                         Resources.Load<Material>(RESOURCES_CHAIR_FRAME_TRANSPARENT)
                                     };

        mChairShadowMaterial = Resources.Load<Material>(RESOURCES_CHAIR_SHADOW);
        mChairShadowMaterialTransparent = Resources.Load<Material>(RESOURCES_CHAIR_SHADOW_TRANSPARENT);
    }

    void SetupFloor()
    {
        if (VuforiaRuntimeUtilities.IsPlayMode())
            mFloorName = GROUND_PLANE_NAME;
        else
        {
            mFloorName = FLOOR_NAME;
            var floor = new GameObject(mFloorName, typeof(BoxCollider));
            floor.transform.SetParent(Chair.transform.parent);
            floor.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            floor.transform.localScale = Vector3.one;
            floor.GetComponent<BoxCollider>().size = new Vector3(100f, 0, 100f);
        }
    }

    /// <summary>
    /// This method is used prior to chair being placed. Once placed, chair visibility is controlled
    /// by the DefaultObserverEventHandler.
    /// </summary>
    /// <param name="visible">bool</param>
    void SetVisible(bool visible)
    {
        mChairRenderer.enabled = mChairShadowRenderer.enabled = visible;
    }

    void EnablePreviewModeTransparency(bool previewEnabled)
    {
        mChairRenderer.materials = previewEnabled ? mChairMaterialsTransparent : mChairMaterials;
        mChairShadowRenderer.material = previewEnabled ? mChairShadowMaterialTransparent : mChairShadowMaterial;
    }
}
