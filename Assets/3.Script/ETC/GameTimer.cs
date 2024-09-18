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

        // firebaseDatabase가 null인지 확인
        if (firebaseDatabase == null)
        {
            Debug.LogError("Firebase_Database를 찾을 수 없습니다.");
        }

        // 타이머 텍스트가 Inspector에서 할당되지 않았을 때
        if (timerText == null)
        {
            // 경로를 사용해서 Timer 오브젝트 찾기
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
            UpdateTimerDisplay(0f); // 초기화된 시간을 표시
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

    //타이머를 포맷에 맞춰 업데이트하는 함수
    void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        // mm:ss 형식으로 시간 표시
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    //타이머 시작
    public void StartTimer()
    {
        isRunning = true;
    }

    //타이머 멈춤
    public void StopTimer(string gameName)
    {
        isRunning = false;

        if(!FirebaseAuthManager.instance.IsInitialized())
        {
            Debug.Log("FirebaseAuthManager가 아직 초기화되지 않았습니다. 승리 시간을 저장할 수 없습니다.");
            return;
        }

        //string email = FirebaseAuth.DefaultInstance.CurrentUser != null ? FirebaseAuth.DefaultInstance.CurrentUser.Email : "Unknown";
        // 승리 시간 저장
        firebaseDatabase.SaveVictoryTime(elapsedTime,gameName);
    }

    //타이머 초기화
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
