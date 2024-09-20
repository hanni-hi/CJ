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

    private int maxRetryCount = 3;
    private int currentRetry = 0;
    private float retryDelay = 2.0f; //재시도 전 대기 시간

    private bool isFirebaseInitialized = false;

    void Start()
    {
        loadingUI.SetActive(true);

        StartCoroutine(WaitForFirebaseInitialization());
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        //10%에서 시작
        SetLoadingProgress(0.1f, "Initializing Firebase...");

        while (!FirebaseAuthManager.instance.IsInitialized())
        {
            yield return new WaitForSeconds(0.5f); //파이어베이스 초기화 작업이 끝날때까지 기다림
            Debug.Log("초기화 대기중");
        }
            Debug.Log("초기화 완료");
            SetLoadingProgress(1.0f, "Initialization completed!");

        yield return new WaitForSeconds(0.5f);
        loadingUI.SetActive(false);

        yield return LoadAdditionalData();

       //         if (dependencyTask.Result == DependencyStatus.Available)
       //         {
       //         Debug.Log("Firebase dependencies are available.");
       //
       //         FirebaseAuthManager.instance.auth = FirebaseAuth.DefaultInstance;
       //             isFirebaseInitialized = true;
       //
       //             SetLoadingProgress(0.5f, "Firebase is being initialized...");
       //
       //             yield return LoadAdditionalData();
       //             break;
       //         }
       //         else
       //     { 
       //         currentRetry++;
       //         Debug.Log("초기화에 실패했습니다...: " + dependencyTask.Result + "재시도 횟수: " + currentRetry);
       //
       //         if (currentRetry >= maxRetryCount)
       //         {
       //             SetLoadingProgress(1.0f, "Initialization failed. Retry limit exceeded...");
       //             yield break;
       //
       //         }
       //         else
       //         {
       //             SetLoadingProgress(0.1f, "Initialization failed. Retrying soon!");
       //             yield return new WaitForSeconds(retryDelay);
       //         }
       //     }
       // }
       //
       // //초기화 완료 후 UI 비활성화
       // if(isFirebaseInitialized)
       // {
       //     yield return new WaitForSeconds(0.5f);
       //     loadingUI.SetActive(false);
       // }

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
