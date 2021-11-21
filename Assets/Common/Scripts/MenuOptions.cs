/*===============================================================================
Copyright (c) 2019 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class MenuOptions : MonoBehaviour
{
    public CameraSettings CameraSettings;
    public DeviceObserverSettings deviceObserverSettings;
    public Toggle DeviceObserverToggle;
    public Toggle AutofocusToggle;
    public Toggle FlashToggle;
    public Toggle StaticDTToggle;
    public OptionsConfig OptionsConfig;

    public bool IsDisplayed { get; private set; }
    
    protected virtual void Start()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized += AfterInitialization;

        if (FlashToggle != null)
            // Flash is not supported on ARCore Devices
            FlashToggle.interactable = !(
                Application.platform == RuntimePlatform.Android &&
                VuforiaRuntimeUtilities.GetActiveFusionProvider() == FusionProviderType.PLATFORM_SENSOR_FUSION
            );

        VuforiaApplication.Instance.OnVuforiaStarted += AppResumedUpdateUI;
    }

    public void ToggleAutofocus(bool enable)
    {
        if (CameraSettings)
            CameraSettings.SwitchAutofocus(enable);
    }

    void AfterInitialization(VuforiaInitError error)
    {
        if (VuforiaBehaviour.Instance != null)
            DeviceObserverToggle.isOn = VuforiaBehaviour.Instance.DevicePoseBehaviour.enabled;
        else
            DeviceObserverToggle.isOn = false;
    }

    void AppResumedUpdateUI()
    {
        UpdateUI();
    }

    public void ToggleTorch(bool enable)
    {
        if (FlashToggle && CameraSettings)
        {
            CameraSettings.SwitchFlashTorch(enable);
            // Update UI toggle status (ON/OFF) in case the flash switch failed
            FlashToggle.isOn = CameraSettings.IsFlashTorchEnabled();
        }
    }

    public void ToggleStaticDT(bool enable)
    {
        if (StaticDTToggle && deviceObserverSettings)
        {
            // we need to disable and re-enable all potentialy tracked targets so that the new device tracker instance
            // again knows about those tracked targets (information is propagated upon target activate)
            var targets = FindObjectsOfType<ScalableDataSetTrackableBehaviour>()
                .Where(m => m.enabled)
                .ToList();
            foreach (var target in targets)
            {
                target.enabled = false;
            }
            
            if (!deviceObserverSettings.ToggleStaticDeviceTracker(enable))
            {
                VLog.Log("red", "Unable to set the Device Tracker Static to " + enable);
            }
            
            foreach (var target in targets)
            {
                target.enabled = true;
            }
        }
    }

    public void ToggleExtendedTracking(bool enable)
    {
        if (deviceObserverSettings)
            deviceObserverSettings.ToggleDeviceObserver(enable);
    }
    
    public void UpdateUI()
    {
        if (DeviceObserverToggle && deviceObserverSettings)
            DeviceObserverToggle.isOn = deviceObserverSettings.IsDeviceObserverEnabled();

        if (FlashToggle && CameraSettings)
            FlashToggle.isOn = CameraSettings.IsFlashTorchEnabled();
        
        if (AutofocusToggle && CameraSettings)
            AutofocusToggle.isOn = CameraSettings.IsAutofocusEnabled();

        if (StaticDTToggle && deviceObserverSettings)
            StaticDTToggle.isOn = deviceObserverSettings.IsStaticDeviceTrackerEnabled();
    }

    public void ResetDeviceObserver()
    {
        VuforiaBehaviour.Instance.DevicePoseBehaviour.Reset();
    }
    
    public void ResetModelTarget()
    {
        foreach (var observerBehaviour in VuforiaBehaviour.Instance.World.GetObserverBehaviours())
            if (observerBehaviour is ModelTargetBehaviour modelTarget && observerBehaviour.enabled)
                modelTarget.Reset();
    }

    public void ShowOptionsMenu(bool show)    
    {
        if (OptionsConfig && OptionsConfig.AnyOptionsEnabled())
        {
            var canvasGroup = GetComponent<CanvasGroup>();

            if (show)
                UpdateUI();

            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;
            canvasGroup.alpha = show ? 1.0f : 0.0f;
            IsDisplayed = show;
        }
    }
    
    void OnDestroy()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized -= AfterInitialization;
    }
   
}
