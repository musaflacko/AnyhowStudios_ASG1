/*===============================================================================
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class HyperlinkHandler : MonoBehaviour, IPointerClickHandler, IPointerUpHandler
{
    TextMeshProUGUI textMeshPro;
    Camera cam;
    
    void Start()
    {
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();

        // Get a reference to the camera if Canvas Render Mode is not ScreenSpace Overlay
        Canvas canvas = GetComponentInParent<Canvas>();
        cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ?
            null : canvas.worldCamera;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CheckIfLinkAndOpenURL();
    }
    
    void CheckIfLinkAndOpenURL()
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, cam);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}
