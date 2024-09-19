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
        SetLoadingProgress(0.1f,"���̾�̽��� �ʱ�ȭ �ϰ� �־��...");

        //���̾�̽� �ʱ�ȭ
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(()=>dependencyTask.IsCompleted);
    
    if(dependencyTask.Result==DependencyStatus.Available)
        {
            FirebaseAuthManager.instance.auth = FirebaseAuth.DefaultInstance;
            isFirebaseInitialized = true;

            SetLoadingProgress(0.5f,"���̾�̽��� �ʱ�ȭ �ǰ� �־��...");

            yield return LoadAdditionalData();
        }
    else
        {
            Debug.Log("�ʱ�ȭ�� �����߽��ϴ�..."+dependencyTask.Result);
            SetLoadingProgress(1.0f,"�ʱ�ȭ ����...");
            yield break;

        }

    if(isFirebaseInitialized)
        {
            SetLoadingProgress(1.0f,"�ʱ�ȭ�� �Ϸ�Ǿ����ϴ�!");
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
        SetLoadingProgress(0.7f,"���� ������ �ε���");

        yield return new WaitForSeconds(2.0f);

        SetLoadingProgress(0.9f,"���� �������� �־��");
    }
}
