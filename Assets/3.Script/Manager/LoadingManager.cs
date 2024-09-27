using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingUI;
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;

    private bool isFirebaseInitialized = false;

    //
    private int maxRetryCount = 3;
    private int currentRetry = 0;
    private float retryDelay = 2.0f; //재시도 전 대기 시간
    //

    void Start()
    {
        loadingUI.SetActive(true);

        StartCoroutine(WaitForFirebaseInitialization());
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        //10%에서 시작
        SetLoadingProgress(0.1f, "Initializing Firebase...");

        while (!FirebaseAuthManager.instance.IsInitialized() && currentRetry < maxRetryCount)
        {
            currentRetry++;
            yield return new WaitForSeconds(0.5f); //파이어베이스 초기화 작업이 끝날때까지 기다림
            Debug.Log("초기화 대기중");
        }

        if(!FirebaseAuthManager.instance.IsInitialized())
        {
            Debug.LogError("초기화 실패했습니다 ㅠㅠ");
        SetLoadingProgress(0.0f, "Firebase Initialization Failed. Please try again...");
            if (UIManager.instance != null)
            {
               UIManager.instance.lodingErrorUIPanel.SetActive(true);  
            }

        }

        yield return LoadAdditionalData();

            SetLoadingProgress(1.0f, "Initialization completed!");

        yield return new WaitForSeconds(0.5f);
        loadingUI.SetActive(false);

    }

    private void SetLoadingProgress(float progress, string message)
    {
        if(progress>0.99f&& !isFirebaseInitialized)
        {
            progress = 0.99f;
        }

        if(loadingSlider !=null)
        {
            loadingSlider.value = progress;
        }
        if(loadingText !=null)
        {
            loadingText.text = message;
        }
    }

    private IEnumerator LoadAdditionalData()
    {
        SetLoadingProgress(0.7f, "Loading game data");

        yield return new WaitForSeconds(2.0f);

        SetLoadingProgress(0.9f, "Almost done");
    }
}
