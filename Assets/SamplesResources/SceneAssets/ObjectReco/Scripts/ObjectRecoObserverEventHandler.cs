/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using UnityEngine;

public class ObjectRecoObserverEventHandler : DefaultObserverEventHandler
{
    public Animator HabitatAnimator;
    public Animator AstronautAnimator;

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        HabitatAnimator.SetBool("IsDoorOpen", true);
        AstronautAnimator.SetBool("IsWaving", true);
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        HabitatAnimator.SetBool("IsDoorOpen", false);
    }
}