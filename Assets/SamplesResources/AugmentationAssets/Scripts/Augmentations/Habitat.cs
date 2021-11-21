/*========================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.
 
Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/

public class Habitat : Augmentation
{
    const string ANIMATION_IS_HATCH_OPEN = "IsHatchOpen";
    const string ANIMATION_IS_DOOR_OPEN = "IsDoorOpen";
    
    public bool IsHatchOpen
    {
        get => mAnimator.GetBool(ANIMATION_IS_HATCH_OPEN);
        set => mAnimator.SetBool(ANIMATION_IS_HATCH_OPEN, value);
    }

    public bool IsDoorOpen
    {
        get => mAnimator.GetBool(ANIMATION_IS_DOOR_OPEN);
        set => mAnimator.SetBool(ANIMATION_IS_DOOR_OPEN, value);
    }

    public override void Restore()
    {
        base.Restore();
        IsDoorOpen = false;
        IsHatchOpen = false;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        OnEnterEvent.Invoke();
    }

    public void OpenDoor(bool value)
    {
        IsDoorOpen = value;
    }
}

