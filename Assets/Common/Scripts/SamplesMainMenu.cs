/*===============================================================================
Copyright (c) 2016-2019 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using TMPro;

public class SamplesMainMenu : MonoBehaviour
{
    bool isAboutScreenVisible
    {
        get { return this.aboutCanvas.sortingOrder > this.menuCanvas.sortingOrder; }
    }

    [SerializeField] Canvas menuCanvas = null;
    [SerializeField] Canvas aboutCanvas = null;
    [SerializeField] Text aboutTitle = null;
    [SerializeField] TextMeshProUGUI aboutDescription = null;

    AboutScreenInfo aboutScreenInfo;
    SafeAreaManager safeAreaManager;
    readonly Color lightGrey = new Color(220f / 255f, 220f / 255f, 220f / 255f);

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;

        safeAreaManager = FindObjectOfType<SafeAreaManager>();

        if (safeAreaManager)
        {
            safeAreaManager.SetAreaColors(lightGrey, Color.white);
            safeAreaManager.SetAreasEnabled(true, true);
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (isAboutScreenVisible)
                OnBackButton();
            else
                QuitApp();
        }
        else if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.JoystickButton0))
        {

            if (isAboutScreenVisible)
            {
                // In Unity 'Return' key same as clicking next button on About Screen
                // On ODG R7, JoystickButton0 is the Trackpad select button
                OnStartAR();
            }
        }
    }

    void OnVuforiaInitialized(VuforiaInitError error)
    {
        VuforiaApplication.Instance.OnVuforiaInitialized -= OnVuforiaInitialized;
        
        // initialize if null
        if (aboutScreenInfo == null)
            aboutScreenInfo = new AboutScreenInfo();
    }
    
    public void OnStartAR()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("2-Loading");
    }

    public void OnBackButton()
    {
        ShowAboutScreen(false);
    }

    public void OnMenuItemSelected(string selectedMenuItem)
    {
        if (selectedMenuItem != string.Empty)
        {
            // Set the scene to be loaded.
            LoadingScreen.SceneToLoad = "3-" + selectedMenuItem;

            // Populate the about screen info.
            aboutTitle.text = aboutScreenInfo.GetTitle(selectedMenuItem);
            aboutDescription.text = aboutScreenInfo.GetDescription(selectedMenuItem);

            // Display the about screen.
            ShowAboutScreen(true);
        }
    }

    void ShowAboutScreen(bool showAboutScreen)
    {
        if (showAboutScreen)
        {
            // Place About canvas in front of Menu canvas
            aboutCanvas.sortingOrder = menuCanvas.sortingOrder + 1;

            if (safeAreaManager)
            {
                safeAreaManager.SetAreaColors(lightGrey, Color.clear);
                safeAreaManager.SetAreasEnabled(true, false);
            }
        }
        else
        {
            // Place About canvas behind Menu canvas
            aboutCanvas.sortingOrder = menuCanvas.sortingOrder - 1;

            if (safeAreaManager)
            {
                safeAreaManager.SetAreaColors(lightGrey, Color.white);
                safeAreaManager.SetAreasEnabled(true, true);
            }
        }
    }

    void QuitApp()
    {
        if (Application.isEditor)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Application.Quit();
        }
    }
}
