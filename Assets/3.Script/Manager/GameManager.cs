using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public int maxHealth = 3;  //최대 체력
    public int currentHealth;  //현재 체력
    public GameObject gameOverUI;  //게임 오버 UI
    private bool isPaused = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            DontDestroyOnLoad(gameOverUI);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth; // 게임 시작시에는 최대 체력
        UIManager.instance.UpdateHealthUI(currentHealth, maxHealth);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("피가 줄었어요...");
        UIManager.instance.UpdateHealthUI(currentHealth,maxHealth);

        if(currentHealth <=0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        UIManager.instance.ShowGameOverUI();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        gameOverUI.SetActive(false);
        currentHealth = maxHealth;
        UIManager.instance.UpdateHealthUI(currentHealth, maxHealth);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);// 현재 씬 다시 로드
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        UIManager.instance.ShowPauseMenu();
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        UIManager.instance.HidePauseMenu();
        isPaused = false;
    }
}
