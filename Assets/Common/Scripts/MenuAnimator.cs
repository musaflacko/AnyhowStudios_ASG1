/*===============================================================================
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.
 
Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
    Vector3 mVisiblePos = Vector3.zero;
    Vector3 mInvisiblePos = -Vector3.right * 2000;
    float mVisibility = 0;
    bool mVisible = false;
    Canvas mCanvas = null;
    MenuOptions mMenuOptions = null;
    
    [Range(0, 1)]
    public float SlidingTime = 0.3f;// seconds
    
    void Start()
    {
        mInvisiblePos = -Vector3.right * (2 * Screen.width);
        mVisibility = 0;
        mVisible = false;
        transform.position = mInvisiblePos;
        mCanvas = GetComponentsInChildren<Canvas>(true)[0];
        mMenuOptions = FindObjectOfType<MenuOptions>();
    }

    void Update()
    {
        mInvisiblePos = -Vector3.right * Screen.width * 2;

        if (mVisible)
        {
            // Switch ON the UI Canvas.
            mCanvas.gameObject.SetActive(true);
            if (!mCanvas.enabled)
                mCanvas.enabled = true;

            if (mVisibility < 1)
            {
                mVisibility += Time.deltaTime / SlidingTime;
                mVisibility = Mathf.Clamp01(mVisibility);
                transform.position = Vector3.Slerp(mInvisiblePos, mVisiblePos, mVisibility);
            }
        }
        else
        {
            if (mVisibility > 0)
            {
                mVisibility -= Time.deltaTime / SlidingTime;
                mVisibility = Mathf.Clamp01(mVisibility);
                transform.position = Vector3.Slerp(mInvisiblePos, mVisiblePos, mVisibility);

                // Switch OFF the UI Canvas when the transition is done.
                
                if (mVisibility < 0.01f)
                {
                    if (mCanvas.enabled)
                    {
                        mCanvas.gameObject.SetActive(false);
                        mCanvas.enabled = false;
                    }
                        
                }
            }
            else
                transform.position = mInvisiblePos;
        }
    }
    
    public void Show()
    {
        mVisible = true;
        if (mMenuOptions)
            mMenuOptions.UpdateUI();
    }

    public void Hide()
    {
        mVisible = false;
    }

    public bool IsVisible()
    {
        return mVisibility > 0.05f;
    }
}
