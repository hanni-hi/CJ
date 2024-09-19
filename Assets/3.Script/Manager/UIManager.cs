using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    public Image[] healthHearts;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    public GameObject quitUIPanel;
    public GameObject pauseUIPanel;
    public GameObject storeUIPanel;
    public GameObject rankingUIPanel;
   // public GameObject aboutusUIPanel;
    public GameObject optionUIPanel;
    public GameObject warningMapselectUIPanel;
    public GameObject quitButton;
    public GameObject mapselectButton;
    public GameObject firstgameButton;
    public GameObject secondgameButton;
    public GameObject selectButton;
    public GameObject gameOverUIPanel;
    public GameObject victoryUIPanel;
    public GameObject loginUIPanel;

    public TextMeshProUGUI gameOverTimeText;
    public TextMeshProUGUI victoryTimeText;

    public Toggle start1;
    public Toggle start2;
    public Toggle start3;

    private GameTimer gameTimer;
    //private bool isPaused = false;
    private string selectedSceneName; //선택된 씬의 이름이 저장될 변수
    private GameObject selectedButton; //현재 선택된 맵 버튼을 저장함
    private Sprite originalSprite; //원래 스프라이트

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            //     DontDestroyOnLoad(storeUIPanel);
            //     DontDestroyOnLoad(rankingUIPanel);
            //   //  DontDestroyOnLoad(aboutusUIPanel);
            //     DontDestroyOnLoad(optionUIPanel);
            //     DontDestroyOnLoad(warningMapselectUIPanel);
            //     DontDestroyOnLoad(quitUIPanel);
            //     DontDestroyOnLoad(quitButton);
            //     DontDestroyOnLoad(mapselectButton);
            //     DontDestroyOnLoad(firstgameButton);
            //     DontDestroyOnLoad(secondgameButton);
            //     DontDestroyOnLoad(selectButton);
            DontDestroyOnLoad(pauseUIPanel);
            DontDestroyOnLoad(gameOverUIPanel);
            DontDestroyOnLoad(victoryUIPanel);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (FindObjectsOfType<EventSystem>().Length > 1)
        {
            Destroy(gameObject); // 중복된 EventSystem을 파괴
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        for(int i=0; i<healthHearts.Length;i++)
        {
            if(i<currentHealth)
            {
                healthHearts[i].sprite = fullHeartSprite;
            }
            else
            {
                healthHearts[i].sprite = emptyHeartSprite;
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       // if (scene.name == "Demo Scene V1(Blue)") // 로비 씬의 이름을 넣으세요
       // {
       //     // 씬을 다시 로드하여 초기화
       //     SceneManager.LoadScene(scene.name);
       //
       //      // 로비 씬으로 돌아왔을 때 필요한 초기화 작업 수행
       //      InitializeLobbyUI();
       // }
        if(scene.name== "SciFi_Warehouse" || scene.name== "Example_01")
        {
            
            gameTimer = FindObjectOfType<GameTimer>();
            if (gameTimer == null)
            {
                Debug.LogError("GameTimer를 찾을 수 없습니다. 타이머가 초기화되지 않았습니다.");
            }
            AssignHeartImages();
        }
    }

    private void AssignHeartImages()
    {
        GameObject[] heartObjects = GameObject.FindGameObjectsWithTag("Heart");
        healthHearts = new Image[heartObjects.Length];
    
    for(int i=0; i<heartObjects.Length;i++)
        {
            healthHearts[i] = heartObjects[i].GetComponent<Image>();
        }
    }

    private void InitializeLobbyUI()
    {
        // 여기서 UI 상태 초기화
        quitUIPanel.SetActive(false);
        storeUIPanel.SetActive(false);
        rankingUIPanel.SetActive(false);
        optionUIPanel.SetActive(false);
        mapselectButton.SetActive(false);
        pauseUIPanel.SetActive(false);
       // aboutusUIPanel.SetActive(false);


        // 기타 필요한 초기화 작업 수행
    }

    public void OnLoginButtonClicked()
    {
        loginUIPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        pauseUIPanel.SetActive(false);
        Time.timeScale = 1f;
      //  isPaused = false;
    }

    public void ShowPauseMenu()
    {
        pauseUIPanel.SetActive(true);
        Time.timeScale = 0; //게임 시간 정지
      //  isPaused = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        pauseUIPanel.SetActive(false);
        victoryUIPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);// 현재 씬 다시 로드
    }

    public void LoadLobby()
    {
       Time.timeScale = 1f;
        SceneManager.LoadScene("Demo Scene V1(Blue)");
    }

    public void AdjustVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        mapselectButton.SetActive(true);
    }

    public void OnQuitButtonClicked()
    {
        quitUIPanel.SetActive(true);
    }

    public void OnYesButtonClicked()
    {
        Application.Quit();
    }

    public void OnCancelButtonClicked()
    {
        quitUIPanel.SetActive(false);
        storeUIPanel.SetActive(false);
        rankingUIPanel.SetActive(false);
        optionUIPanel.SetActive(false);
        mapselectButton.SetActive(false);
        pauseUIPanel.SetActive(false);
        loginUIPanel.SetActive(false);
        warningMapselectUIPanel.SetActive(false);

    }

    public void MapisnotSelected()
    {

        warningMapselectUIPanel.SetActive(false);

    }

    public void OnStoreButtonClicked()
    {
        storeUIPanel.SetActive(true);
    }

    public void OnRankingButtonClicked()
    {
        rankingUIPanel.SetActive(true);
    }

  //  public void OnASButtonClicked()
  //  {
  //      aboutusUIPanel.SetActive(true);
  //  }

    public void OnOptionClicked()
    {
        optionUIPanel.SetActive(true);
    }

    public void Onfirstgameselected()
    {
        //HandleMapSelected("Prototype+inven",firstgameButton);
        selectedSceneName = "SciFi_Warehouse";
    }

    public void Onsecondgameselected()
    {
        //  HandleMapSelected("",secondgameButton);  두번째 게임 신이 완성되면 이름 넣기
        selectedSceneName = "Example_01";
    }

    public void RealgameStart()
    {
        if (!string.IsNullOrEmpty(selectedSceneName))
        {
            mapselectButton.SetActive(false);
            SceneManager.LoadScene(selectedSceneName);
        }
        else
        {
            warningMapselectUIPanel.SetActive(true);
        }
    }

    public void ShowVictoryUI()
    {
        StartCoroutine(CheckFirebaseInitializedAndShowVictoryUI());

    }



    private IEnumerator CheckFirebaseInitializedAndShowVictoryUI()
    {
        float waitTime = 5f;
        while (!FirebaseAuthManager.instance || !FirebaseAuthManager.instance.IsInitialized())
        {
            Debug.Log("FirebaseAuthManager가 아직 초기화되지 않았습니다. 재시도 중...");
            yield return new WaitForSeconds(0.5f);
            waitTime -= 0.5f;
        
            if(waitTime<=0)
            {
                Debug.LogError("FirebaseAuthManager 초기화에 실패했습니다. Victory UI를 표시할 수 없습니다.");
                yield break;
            }
        }
            if (gameTimer != null)
        {
            string gameName = "";

            if(SceneManager.GetActiveScene().name== "SciFi_Warehouse")
            {
                gameName = "LEVEL MAP_01";
            }
            else if(SceneManager.GetActiveScene().name== "Example_01")
            {
                gameName = "LEVEL MAP_02";
            }
            gameTimer.StopTimer(gameName);

            string finalTime = gameTimer.GetFormattedTime(); //타이머 시간 가져옴
            victoryTimeText.text = finalTime;

            UpdateStartToggles(gameTimer.GetElapsedTime());
        }

        victoryUIPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ShowGameOverUI()
    {
        if(gameTimer != null)
        {
            gameTimer.StopTimer("Fail Game");

            string finalTime = gameTimer.GetFormattedTime();
            gameOverTimeText.text = finalTime;
        }
        gameOverUIPanel.SetActive(true);
        Time.timeScale = 0;
    }

    private void UpdateStartToggles(float elapsedTime)
    {
        //3분 이하
        if(elapsedTime<=180f)
        {
            start1.isOn = true;
            start2.isOn = true;
            start3.isOn = true;
        }
        else if(elapsedTime>180f&&elapsedTime<=240f)
        {
            start1.isOn = true;
            start2.isOn = true;
            start3.isOn = false;
        }
        else if(elapsedTime>240f&&elapsedTime<=300f)
        {
            start1.isOn = true;
            start2.isOn = false;
            start3.isOn = false;
        }
        else
        {
            start1.isOn = false;
            start2.isOn = false;
            start3.isOn = false;
        }
    }

    public bool HasEmptyHeart()
    {
        for(int i=0; i<healthHearts.Length;i++)
        {
            if(healthHearts[i].sprite==emptyHeartSprite)
            {
                return true;
            }
        }
        return false;
    }

    public void FillEmptyHeart()
    {
        for(int i=0; i< healthHearts.Length;i++)
        {
            if(healthHearts[i].sprite==emptyHeartSprite)
            {
                healthHearts[i].sprite = fullHeartSprite;
                break;
            }
        }
    }

}
