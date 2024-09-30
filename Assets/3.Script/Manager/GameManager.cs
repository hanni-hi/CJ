using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance = null;

    public int maxHealth = 3;  //�ִ� ü��
    public int currentHealth;  //���� ü��
    public GameObject gameOverUI;  //���� ���� UI
    private bool isPaused = false;

    // ����Ʈ ���μ��� ���� ����
    public PostProcessVolume postProcessVolume;  // ���� ������ PostProcessVolume
    private Bloom bloom;
    private ColorGrading colorGrading;
    private Vignette vignette;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

           // DontDestroyOnLoad(gameOverUI);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth; // ���� ���۽ÿ��� �ִ� ü��
        UIManager.instance.UpdateHealthUI(currentHealth, maxHealth);

        postProcessVolume.profile.TryGetSettings(out bloom);
        postProcessVolume.profile.TryGetSettings(out colorGrading);
        postProcessVolume.profile.TryGetSettings(out vignette);
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
        Debug.Log("�ǰ� �پ����...");
        UIManager.instance.UpdateHealthUI(currentHealth,maxHealth);

        if(currentHealth <=0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        UIManager.instance.ShowGameOverUI();
        ApplyGameOverPostProcessing();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        gameOverUI.SetActive(false);
        currentHealth = maxHealth;
        UIManager.instance.UpdateHealthUI(currentHealth, maxHealth);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);// ���� �� �ٽ� �ε�
        ResetPostProcessing();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        UIManager.instance.ShowPauseMenu();
        ApplyPausePostProcessing();
        isPaused = true;

        // Notify other players
        PhotonManager.instance.photonView.RPC("RPC_PauseGame", RpcTarget.Others);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        UIManager.instance.HidePauseMenu();
        ResetPostProcessing();
        isPaused = false;

        // Notify other players
        PhotonManager.instance.photonView.RPC("RPC_ResumeGame", RpcTarget.Others);
    }

    private void ApplyGameOverPostProcessing()
    {
        bloom.intensity.value = 5.0f; //��� ���ϰ� ����
        colorGrading.temperature.value = -40f; //������ ������ ����
        vignette.intensity.value = 0.6f; //�߾� ���ߵ�
    }

    private void ApplyPausePostProcessing()
    {
        bloom.intensity.value = 3.0f;
        colorGrading.saturation.value = -50f; //ä���� ������ ����
        vignette.intensity.value = 0.4f;
    }

    private void ResetPostProcessing()
    {
        bloom.intensity.value = 2.0f;
        colorGrading.temperature.value = 0f;
        vignette.intensity.value = 0.2f;
    }

    public void ApplyLobbyPostProcessing()
    {
        bloom.intensity.value = 6.0f;
        bloom.threshold.value = 1.1f;
        colorGrading.temperature.value = 20f;
        vignette.intensity.value = 0.3f;
    }
}
