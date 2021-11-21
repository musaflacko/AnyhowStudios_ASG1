/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ModelTargetsUIManager : MonoBehaviour
{
    // For private serialized fields, we assign references in the Inspector, so disable assignment warnings.
    // Disable: CS0649: Field '' is never assigned to, and will always have its default value false
#pragma warning disable 649
    [Header("Symbolic Icon Canvas Groups")] 
    [SerializeField] CanvasGroup CanvasGroupAdvanced;
#pragma warning restore 649

    const float IMAGE_SWAP_FADE_RANGE_MAX = 0.001f;
    
    static readonly Color mWhiteTransparent = new Color(1f, 1f, 1f, 0f);
    Image[] mImageSequence; 
    Image[] mAdvancedImages;
    bool mIsUiEnabled;
    bool mIsImageSequencePaused;
    int mImageSequenceIndex;
    float mClock;
    float mFadeMeter;

    void Start()
    {
        InitSymbolicTargetIcons();
    }

    void Update()
    {
        UpdateSymbolicTargetIconFadeCycle();
    }
    
    public void SetUI(ModelTargetsManager.ModelTargetMode modelTargetMode, bool enable)
    {
        switch (modelTargetMode)
        {
            case ModelTargetsManager.ModelTargetMode.STANDARD:
                CanvasGroupAdvanced.alpha = 0;
                break;
            case ModelTargetsManager.ModelTargetMode.ADVANCED:
                mImageSequence = mAdvancedImages;
                CanvasGroupAdvanced.alpha = enable ? 1 : 0;
                break;
        }

        // If the UI enabled status has changed, update sequence variables
        if (mIsUiEnabled != enable)
        {
            ResetImageSequenceValues();
            mIsUiEnabled = enable;
        }
    }

    void InitSymbolicTargetIcons()
    {
        if (CanvasGroupAdvanced != null)
            mAdvancedImages = CanvasGroupAdvanced.GetComponentsInChildren<Image>();

        // Set all the symbolic icons to be transparent at start.
        foreach (var image in mAdvancedImages)
            image.color = mWhiteTransparent;
    }

    void UpdateSymbolicTargetIconFadeCycle()
    {
        if (!mIsUiEnabled) 
            return;
        
        mFadeMeter = Mathf.InverseLerp(-1f, 1f, Mathf.Sin(mClock += Time.deltaTime * 2));
        mFadeMeter = Mathf.SmoothStep(0, 1, mFadeMeter);

        if (mImageSequence == null) 
            return;
        
        if (mImageSequence.Length > 1)
        {
            if (mFadeMeter < IMAGE_SWAP_FADE_RANGE_MAX && !mIsImageSequencePaused)
            {
                mImageSequence[mImageSequenceIndex].color = Color.clear;
                mImageSequenceIndex = (mImageSequenceIndex + 1) % mImageSequence.Length;
                mImageSequence[mImageSequenceIndex].color = Color.white;
                mIsImageSequencePaused = true;
                StartCoroutine(ClearImageSequencePause());
            }
            mImageSequence[mImageSequenceIndex].color = Color.Lerp(mWhiteTransparent, Color.white, mFadeMeter);
        }
        else
            mImageSequence[0].color = Color.Lerp(mWhiteTransparent, Color.white, mFadeMeter);
    }

    void ResetImageSequenceValues()
    {
        mClock = 0f;
        mImageSequenceIndex = 0;
        
        if (mImageSequence != null)
            foreach (var image in mImageSequence)
                image.color = mWhiteTransparent;
    }

    IEnumerator ClearImageSequencePause()
    {
        // Wait until the fade meter exits the valid image swapping range before clearing sequence flag.
        // This enforces a maximum of one symbolic icon change per fade cycle.
        yield return new WaitUntil(() => mFadeMeter > IMAGE_SWAP_FADE_RANGE_MAX);
        mIsImageSequencePaused = false;
    }
}