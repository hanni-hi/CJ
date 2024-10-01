using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public float totalTime = 120f;
    private float remainingTime;
    public TextMeshProUGUI timer;
    public GameObject resultUIPanel;

    private bool isGameRunning = false;

    void Start()
    {
        remainingTime = totalTime;
        UpdateTimerText(remainingTime);
    }

    void Update()
    {
    if(isGameRunning)
        {
            if(remainingTime>0)
            {
                remainingTime -= Time.deltaTime;
                UpdateTimerText(remainingTime);
            }
            else
            {
               EndGame();
            }
        }
    }

    void UpdateTimerText(float time)
    {
        int minutes = Mathf.FloorToInt(time/60);
        int seconds = Mathf.FloorToInt(time%60);
        timer.text = string.Format("{0:00}:{1:00}",minutes,seconds);
    }

    public void StartTimer()
    {
        isGameRunning = true;
    }

    void EndGame()
    {
        isGameRunning = false;
        Time.timeScale = 0;
        resultUIPanel.SetActive(true) ;

        if(PhotonManager.instance !=null)
        {
            PhotonManager.instance.EndMGame();
        }
    }
}
