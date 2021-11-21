/*===============================================================================
Copyright (c) 2015-2018 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    RawImage m_SpinnerImage;
    AsyncOperation m_AsyncOperation;
    bool m_SceneReadyToActivate;

    public static string SceneToLoad { get; set; }

    public static void Run()
    {
        SceneManager.LoadSceneAsync("2-Loading");
    }

    void Start()
    {
        m_SpinnerImage = GetComponentInChildren<RawImage>();
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        StartCoroutine(LoadNextSceneAsync());
    }

    void Update()
    {
        if (m_SpinnerImage)
        {
            if (!m_SceneReadyToActivate)
                m_SpinnerImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
            else
                m_SpinnerImage.enabled = false;
        }

        if (m_AsyncOperation != null)
        {
            if (m_AsyncOperation.progress < 0.9f)
                Debug.Log("Scene Loading Progress: " + m_AsyncOperation.progress * 100 + "%");
            else
            {
                m_SceneReadyToActivate = true;
                m_AsyncOperation.allowSceneActivation = true;
            }
        }
    }
    
    IEnumerator LoadNextSceneAsync()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (string.IsNullOrEmpty(SceneToLoad))
            m_AsyncOperation = SceneManager.LoadSceneAsync(nextSceneIndex);
        else
            m_AsyncOperation = SceneManager.LoadSceneAsync(SceneToLoad);

        m_AsyncOperation.allowSceneActivation = false;

        yield return m_AsyncOperation;
    }
}
