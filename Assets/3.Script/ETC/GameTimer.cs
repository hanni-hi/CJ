using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText=null;
    private float elapsedTime = 0;
    private bool isRunning = true;
    private Firebase_Database firebaseDatabase;

        void Start()
    {
        firebaseDatabase = FindObjectOfType<Firebase_Database>();

        // firebaseDatabase�� null���� Ȯ��
        if (firebaseDatabase == null)
        {
            Debug.LogError("Firebase_Database�� ã�� �� �����ϴ�.");
        }

        // Ÿ�̸� �ؽ�Ʈ�� Inspector���� �Ҵ���� �ʾ��� ��
        if (timerText == null)
        {
            // ��θ� ����ؼ� Timer ������Ʈ ã��
            GameObject timerObject = GameObject.Find("Canvas/Timer");
            if (timerObject != null)
            {
                timerText = timerObject.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogError("Timer object not found under Canvas!");
            }
        }

        if (timerText != null)
        {
            UpdateTimerDisplay(0f); // �ʱ�ȭ�� �ð��� ǥ��
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component for timer not found!");
        }
    }

    void Update()
    {
        if(isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay(elapsedTime);
        }
    }

    //Ÿ�̸Ӹ� ���˿� ���� ������Ʈ�ϴ� �Լ�
    void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        // mm:ss �������� �ð� ǥ��
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    //Ÿ�̸� ����
    public void StartTimer()
    {
        isRunning = true;
    }

    //Ÿ�̸� ����
    public void StopTimer(string gameName)
    {
        isRunning = false;

        if(!FirebaseAuthManager.instance.IsInitialized())
        {
            Debug.Log("FirebaseAuthManager�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�. �¸� �ð��� ������ �� �����ϴ�.");
            return;
        }

        //string email = FirebaseAuth.DefaultInstance.CurrentUser != null ? FirebaseAuth.DefaultInstance.CurrentUser.Email : "Unknown";
        // �¸� �ð� ����
        firebaseDatabase.SaveVictoryTime(elapsedTime,gameName);
    }

    //Ÿ�̸� �ʱ�ȭ
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay(elapsedTime);
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(elapsedTime/60f);
        int seconds = Mathf.FloorToInt(elapsedTime%60f);

        return string.Format("{0:00}:{1:00}",minutes,seconds);
    }    

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}
