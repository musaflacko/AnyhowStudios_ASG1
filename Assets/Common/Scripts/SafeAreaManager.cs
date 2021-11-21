/*===============================================================================
Copyright (c) 2019 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System;
using UnityEngine;
using UnityEngine.UI;

public class SafeAreaManager : MonoBehaviour
{
    [System.Serializable]
    class SafeAreaRect
    {
        public RectTransform rectTransform = null;
        [Header("Apply Safe Area Constraints")]
        public bool top = false;
        public bool bottom = false;
    }

    [Header("Global Unsafe Area Settings (Per-Scene)")]
    [Tooltip("Unsafe Area Colors can be changed programmatically at runtime.")]
    [SerializeField] RectTransform topArea = null;
    [SerializeField] RectTransform bottomArea = null;
    [SerializeField] Color topAreaColor;
    [SerializeField] Color bottomAreaColor;
    [Tooltip("Safe Area Margin reduces the Safe Area by the specified amount at the Top/Bottom boundaries. " +
             "It is useful for testing Safe Area Behaviour in PlayMode.")]
    [Range(0,100)] // Max range value is arbitrary for example purposes
    [SerializeField] private int SafeAreaMargin = 0;

    [Header("Apply Safe Area Constraints to RectTransforms")]
    [SerializeField] SafeAreaRect[] safeAreaRects = null;
    
    ScreenOrientation lastOrientation;
    Rect lastSafeArea = new Rect(0, 0, 0, 0);
    Rect safeArea;
    Image topAreaImage = null;
    Image bottomAreaImage = null;
    bool colorsChanged => (topAreaColor != topAreaImage.color) || (bottomAreaColor != bottomAreaImage.color);

    void Awake()
    {
        if (!topArea || !bottomArea)
        {
            Debug.LogWarning("Either topArea or bottomArea is null. Programmatically getting the required references.");
            SetAreaRectTransforms();
        }
        
        // cache our unsafe area image components
        topAreaImage = topArea.GetComponent<Image>();
        bottomAreaImage = bottomArea.GetComponent<Image>();

        // Set the unsafe area colors using Inspector values
        SetAreaColors(topAreaColor, bottomAreaColor);
        
        safeArea = GetSafeArea();
    }

    void SetAreaRectTransforms()
    {
        var images = GetComponentsInChildren<Image>();
        if (images.Length != 2)
        {
            Debug.LogError($"SafeAreaManager must have exactly two children with Image components attached.");
            return;
        }

        topArea = images[0].rectTransform;
        bottomArea = images[1].rectTransform;
    }
    
    Rect GetSafeArea()
    {
        return new Rect(
            Screen.safeArea.x, 
            Screen.safeArea.y + SafeAreaMargin,
            Screen.safeArea.width, 
            Screen.safeArea.height - (SafeAreaMargin * 2));
    }

    void Start()
    {
        lastOrientation = Screen.orientation;

        Refresh();
    }

    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        safeArea = GetSafeArea();

        if ((safeArea != lastSafeArea) || (Screen.orientation != lastOrientation))
        {
            ApplySafeArea();
            UpdateUnsafeArea();
        }

        if (colorsChanged)
            SetAreaColors(topAreaColor, bottomAreaColor);
    }

    void ApplySafeArea()
    {
        lastSafeArea = safeArea;
        lastOrientation = Screen.orientation;

        foreach (SafeAreaRect areaRect in safeAreaRects)
        {
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y = areaRect.bottom ? anchorMin.y / Screen.height : 0;
            anchorMax.x /= Screen.width;
            anchorMax.y = areaRect.top ? anchorMax.y / Screen.height : 1;
            
            if (Screen.orientation == ScreenOrientation.LandscapeLeft ||
                Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 1;
            }
            
            areaRect.rectTransform.anchorMin = anchorMin;
            areaRect.rectTransform.anchorMax = anchorMax;
        }
    }
    
    void UpdateUnsafeArea()
    {
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y = anchorMin.y / Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y = anchorMax.y / Screen.height;

        SetUnsafeAreaSizes(anchorMin.y, anchorMax.y);
        
        SetAreaColors(topAreaColor, bottomAreaColor);
    }

    void SetUnsafeAreaSizes(float safeAreaAnchorMinY, float safeAreaAnchorMaxY)
    {
        topArea.anchorMin = new Vector2(0, safeAreaAnchorMaxY);
        topArea.anchorMax = Vector2.one;

        bottomArea.anchorMin = Vector2.zero;
        bottomArea.anchorMax = new Vector2(1, safeAreaAnchorMinY);
    }

    public void AddSafeAreaRect(RectTransform rect, bool applyTopConstraint, bool applyBottomConstraint)
    {
        Array.Resize(ref safeAreaRects, safeAreaRects.Length + 1);
        safeAreaRects[safeAreaRects.Length - 1] = new SafeAreaRect
        {
            rectTransform = rect,
            top = applyTopConstraint,
            bottom = applyBottomConstraint
        };

        ApplySafeArea();
    }

    public void SetAreasEnabled(bool topAreaEnabled, bool bottomAreaEnabled)
    {
        topAreaImage.enabled = topAreaEnabled;
        bottomAreaImage.enabled = bottomAreaEnabled;
    }

    /// <summary>
    /// Sets the area colors programmatically and updates Inspector colors.
    /// </summary>
    /// <param name="topColor">Top color.</param>
    /// <param name="bottomColor">Bottom color.</param>
    public void SetAreaColors(Color topColor, Color bottomColor)
    {
        // update Inspector-level colors to match programmatic ones
        topAreaColor = topColor;
        bottomAreaColor = bottomColor;

        // assign the colors
        topAreaImage.color = topAreaColor;
        bottomAreaImage.color = bottomAreaColor;
    }
}
