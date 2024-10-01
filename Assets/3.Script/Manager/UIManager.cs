using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    public Image[] healthHearts;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    public GameObject homeUIPanel;
    public GameObject quitUIPanel;
    public GameObject pauseUIPanel;
    public GameObject M_pauseUIPanel;// 친구 한명이 나갔을때
   // public GameObject storeUIPanel;
    public GameObject rankingUIPanel;
    public GameObject aboutusUIPanel;
    public GameObject optionUIPanel;
    public GameObject warningMapselectUIPanel;
    public GameObject quitButton;
    public GameObject mapselectButton;
    public GameObject firstgameButton;
    public GameObject secondgameButton;
    public GameObject multigameButton;
    public GameObject selectButton;
    public GameObject gameOverUIPanel;
    public GameObject victoryUIPanel;
    public GameObject loginUIPanel;
    public GameObject lodingErrorUIPanel;

    public TextMeshProUGUI gameOverTimeText;
    public TextMeshProUGUI victoryTimeText;

    public Toggle start1;
    public Toggle start2;
    public Toggle start3;

    private GameTimer gameTimer;
    private PhotonManager photonmanager;
    private Timer timer;
    //private bool isPaused = false;
    private string selectedSceneName; //선택된 씬의 이름이 저장될 변수
    private GameObject selectedButton; //현재 선택된 맵 버튼을 저장함
    private Sprite originalSprite; //원래 스프라이트

    private string githubUrl = "https://github.com/hanni-hi";

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

          //  DontDestroyOnLoad(pauseUIPanel);
          //  DontDestroyOnLoad(gameOverUIPanel);
          //  DontDestroyOnLoad(victoryUIPanel);
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

    private void Start()
    {
        photonmanager = FindObjectOfType<PhotonManager>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
       // SaveReferencesToJSON();
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
        if (scene.name == "Demo Scene V1(Blue)") // 로비 씬의 이름을 넣으세요
        {
            homeUIPanel.SetActive(true);
            victoryUIPanel.SetActive(false);
            gameOverUIPanel.SetActive(false);
            pauseUIPanel.SetActive(false);
            M_pauseUIPanel.SetActive(false);
            //  // 로비 씬으로 돌아왔을 때 필요한 초기화 작업 수행
            //  InitializeLobbyUI();
            //  LoadReferencesFromJSON();
            GameManager.instance.ApplyLobbyPostProcessing();
            RemoveDuplicateObjects();
        }
        if(scene.name== "SciFi_Warehouse" || scene.name== "Example_01")
        {
            homeUIPanel.SetActive(false);
            StartCoroutine(CheckFirebaseInitializedAtStart());

            gameTimer = FindObjectOfType<GameTimer>();
            AssignHeartImages();
        }
        if (scene.name == "SciFi_Warehouse_M")
        {
            homeUIPanel.SetActive(false);
            StartCoroutine(CheckFirebaseInitializedAtStart());

           // timer = FindObjectOfType<Timer>();
            AssignHeartImages();
        }
    }

    private void RemoveDuplicateObjects()
    {
        GameObject[] dontDestroyObjects = { pauseUIPanel, gameOverUIPanel, victoryUIPanel };

        foreach(GameObject obj in dontDestroyObjects)
        {
            GameObject existingObj = GameObject.Find(obj.name);
            if(existingObj !=null && existingObj != obj)
            {
                Destroy(existingObj);
            }
        }
    }

    private IEnumerator CheckFirebaseInitializedAtStart()
    {
        float waitTime = 5f;
    while(!FirebaseAuthManager.instance||!FirebaseAuthManager.instance.IsInitialized())
        {
            Debug.Log("FirebaseAuthManager 초기화 대기 중...");
            yield return new WaitForSeconds(0.5f);
            waitTime -= 0.5f;

            if(waitTime<=0)
            {
                Debug.Log("파이어베이스 초기화 실패");
                yield break;
            }
        }
             
                Debug.Log("파이어베이스 초기화 완료");
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

    }

    public void OnLoginButtonClicked()
    {
        loginUIPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        Debug.Log("HidePauseMenu()");
        pauseUIPanel.SetActive(false);
        photonmanager.M_PauseUI.SetActive(false);
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

        if(SceneManager.GetActiveScene().name== "Example_01")
        {
            if(MemoUIManager.instance !=null)
            {
                MemoUIManager.instance.ClearInteractedObjects();
            }
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);// 현재 씬 다시 로드
    }

    public void LoadLobby()
    {
       Time.timeScale = 1f;

        if(SceneManager.GetActiveScene().name== "SciFi_Warehouse_M")
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom(false);
            }
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }
        }

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
       // storeUIPanel.SetActive(false);
        rankingUIPanel.SetActive(false);
        optionUIPanel.SetActive(false);
        mapselectButton.SetActive(false);
        pauseUIPanel.SetActive(false);
        loginUIPanel.SetActive(false);
        warningMapselectUIPanel.SetActive(false);
        aboutusUIPanel.SetActive(false);

    }

    public void MapisnotSelected()
    {

        warningMapselectUIPanel.SetActive(false);

    }

    public void OnRankingButtonClicked()
    {
        rankingUIPanel.SetActive(true);
    }

    public void OnASButtonClicked()
    {
        aboutusUIPanel.SetActive(true);
    }

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
        selectedSceneName = "Example_01";
    }
    public void Onmultigameselected()
    {

        selectedSceneName = "SciFi_Warehouse_M";
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

    public void OpenGitHubPage()
    {
        Application.OpenURL(githubUrl);
    }

    public void ShowPausePanel_M()
    {
        if(M_pauseUIPanel !=null)
        {
            M_pauseUIPanel.SetActive(true);
        }
    }

  //  public void OnResumeButtonClicked()
  //  {
  //      if (SceneManager.GetActiveScene().name== "SciFi_Warehouse_M" && Time.timeScale == 0 && UIManager.instance.pauseUIPanel.activeInHierarchy)
  //      {
  //          photonmanager.photonView.RPC("RPC_ResumeGame", RpcTarget.Others);
  //      }
  //  }

}
