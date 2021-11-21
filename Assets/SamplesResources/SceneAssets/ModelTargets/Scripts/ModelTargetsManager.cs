/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ModelTargetsManager : MonoBehaviour
{ 
    public enum ModelTargetMode
    {
        STANDARD,
        ADVANCED
    }
    
    // For private serialized fields, we assign references in the Inspector, so disable assignment warnings.
    // Disable: CS0649: Field '' is never assigned to, and will always have its default value false
#pragma warning disable 649
    [Header("Initial Model Target Mode")]
    [SerializeField] ModelTargetMode TargetMode = ModelTargetMode.ADVANCED;

    [Header("Model Target Shared Augmentation")]
    [SerializeField] GameObject LanderAugmentation;

    [Header("Model Target Behaviours")]
    [SerializeField] ModelTargetBehaviour StandardLanderTarget;
    [SerializeField] UnityEngine.UI.Image StandardButtonImage;
    [SerializeField] ModelTargetBehaviour AdvancedLanderTarget;
    [SerializeField] UnityEngine.UI.Image AdvancedButtonImage;

    [Header("Mobile")]
    [Tooltip("Guide Views Button only for Mobile")]
    [SerializeField] Button GuideViewsButton;
    [SerializeField] CanvasGroup ModelTargetsMenu;
#pragma warning restore 649
    
    ModelTargetsUIManager mModelTargetsUIManager;
    
    readonly List<ModelTargetBehaviour> mAdvancedTargetBehaviours = new List<ModelTargetBehaviour>();
    readonly Color32 COLOR_SELECTED = new Color32(98, 179, 2, 255);
    readonly Color32 COLOR_NOT_SELECTED = new Color32(99, 99, 99, 255);

    void Awake()
    {
        mModelTargetsUIManager = FindObjectOfType<ModelTargetsUIManager>();
    }

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
    }

    void LateUpdate()
    {
        if (TargetMode == ModelTargetMode.ADVANCED && mAdvancedTargetBehaviours.Count > 0)
        {
            var areAllAdvancedTargetsInitializing = true;
            foreach (var target in mAdvancedTargetBehaviours)
            {
                if (target && target.TargetStatus.StatusInfo != StatusInfo.INITIALIZING)
                {
                    areAllAdvancedTargetsInitializing = false;
                    break;
                }
            }
            EnableSymbolicTargetsUI(areAllAdvancedTargetsInitializing);
        }
    }

    void OnDestroy()
    {
        VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
    }

    void OnVuforiaStarted()
    {
        var modelTargetBehaviours = FindObjectsOfType<ModelTargetBehaviour>();
        foreach (var modelTargetBehaviour in modelTargetBehaviours)
            modelTargetBehaviour.enabled = false;

        switch (TargetMode)
        {
            case ModelTargetMode.STANDARD:
                EnableStandardModelTarget();
                break;
            case ModelTargetMode.ADVANCED:
                EnableAdvancedModelTarget();
                break;
        }
    }

    public void EnableSymbolicTargetsUI(bool enable)
    {
        mModelTargetsUIManager.SetUI(TargetMode, enable);
    }

    public void AddAdvancedModelTargetBehaviour(ModelTargetBehaviour modelTargetBehaviour)
    {
        Debug.Log("AddAdvancedModelTargetBehaviour() called.");
     
        if (modelTargetBehaviour != null)
            mAdvancedTargetBehaviours.Add(modelTargetBehaviour);

        if (TargetMode == ModelTargetMode.ADVANCED)
            modelTargetBehaviour.enabled = true;
        
        VLog.Log("cyan", "# of Advanced MTBs: " + mAdvancedTargetBehaviours.Count);
        EnableSymbolicTargetsUI(mAdvancedTargetBehaviours.Count == 0);
    }

    public void EnableStandardModelTarget()
    {
        foreach (var advancedTarget in mAdvancedTargetBehaviours)
            advancedTarget.enabled = false;
        
        AdvancedButtonImage.color = COLOR_NOT_SELECTED;
        
        StandardLanderTarget.enabled = true;
        StandardButtonImage.color = COLOR_SELECTED;
        
        TargetMode = ModelTargetMode.STANDARD;

        GuideViewsButton.interactable = true;
        EnableSymbolicTargetsUI(false);
        ResetAugmentationTransform(StandardLanderTarget.transform);
    }

    public void EnableAdvancedModelTarget()
    {
        StandardLanderTarget.enabled = false;
        StandardButtonImage.color = COLOR_NOT_SELECTED;
        
        foreach (var advancedTarget in mAdvancedTargetBehaviours)
            advancedTarget.enabled = true;
        AdvancedButtonImage.color = COLOR_SELECTED;
        
        TargetMode = ModelTargetMode.ADVANCED;
        
        EnableSymbolicTargetsUI(true);
        GuideViewsButton.interactable = false;
        ResetAugmentationTransform(AdvancedLanderTarget.transform);
    }

    void ResetAugmentationTransform(Transform targetTransform)
    {
        LanderAugmentation.transform.SetParent(targetTransform);
        LanderAugmentation.transform.localPosition = Vector3.zero;
        LanderAugmentation.transform.localRotation = Quaternion.identity;
        LanderAugmentation.transform.localScale = Vector3.one;
    }

    public void ShowTargetMenu(bool active)
    {
        if (ModelTargetsMenu)
        {
            ModelTargetsMenu.alpha = active ? 1 : 0;
            ModelTargetsMenu.interactable = active;
            ModelTargetsMenu.blocksRaycasts = active;
        }
    }

    /// <summary>
    /// Cycles through guide views for Standard Model Targets with multiple views.
    /// </summary>
    public void CycleGuideView()
    {
        VLog.Log("cyan", "CycleGuideView() called.");

        if (StandardLanderTarget == null) 
            return;
        
        var activeView = StandardLanderTarget.GetActiveGuideViewIndex();
        var totalViews = StandardLanderTarget.GetNumGuideViews();

        if (totalViews > 1 && activeView > -1)
        {
            var guideViewIndexToActivate = (activeView + 1) % totalViews;
            VLog.Log("yellow",
                     StandardLanderTarget.TargetName + ": Activating Guide View Index " +
                     guideViewIndexToActivate + " of " +
                     totalViews + " total Guide Views.");

            StandardLanderTarget.SetActiveGuideViewIndex(guideViewIndexToActivate);
        }
        else
        {
            VLog.Log("yellow",
                     "GuideView was not cycled." +
                     "\nActive Guide View Index = " + activeView +
                     "\nNumber of Guide Views = " + totalViews);
        }
    }
}