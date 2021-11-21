/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using UnityEngine;

public class ScreenAliveSettings : MonoBehaviour
{
    public SleepTimeOutSetting SleepSettings = SleepTimeOutSetting.Default;
    void Awake()
    {
        switch (SleepSettings)
        {
            case SleepTimeOutSetting.Default:
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
                break;
            case SleepTimeOutSetting.NeverSleep:
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                break;
        }
    }
}