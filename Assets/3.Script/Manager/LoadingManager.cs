using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;


public class LoadingManager : MonoBehaviour
{
    public GameObject loadingUI;
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;

    private bool isFirebaseInitialized = false;

    void Start()
    {
        loadingUI.SetActive(true);

        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        SetLoadingProgress(0.1f,"파이어베이스를 초기화 하고 있어요...");

        //파이어베이스 초기화
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(()=>dependencyTask.IsCompleted);
    
    if(dependencyTask.Result==DependencyStatus.Available)
        {
            FirebaseAuthManager.instance.auth = FirebaseAuth.DefaultInstance;
            isFirebaseInitialized = true;

            SetLoadingProgress(0.5f,"파이어베이스가 초기화 되고 있어요...");

            yield return LoadAdditionalData();
        }
    else
        {
            Debug.Log("초기화에 실패했습니다..."+dependencyTask.Result);
            SetLoadingProgress(1.0f,"초기화 실패...");
            yield break;

        }

    if(isFirebaseInitialized)
        {
            SetLoadingProgress(1.0f,"초기화가 완료되었습니다!");
            yield return new WaitForSeconds(0.5f);
            loadingUI.SetActive(false);
        }

    }

    private void SetLoadingProgress(float progress, string message)
    {
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
        SetLoadingProgress(0.7f,"게임 데이터 로딩중");

        yield return new WaitForSeconds(2.0f);

        SetLoadingProgress(0.9f,"거의 끝나가고 있어요");
    }
}
