using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

//    $ => string.Format()

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public Image[] canvasImages; // 18개의 캔버스 이미지
    public Sprite originalSprite;

    // 플레이어 컬러 배열
    private Color[] playerColors = { Color.white, Color.blue,Color.red,Color.black,new Color(0.5f,0,0.5f) };

    //버전 입력
    private readonly string version = "1.0f";

    //사용자 아이디 입력
    private string userID = "Han";

    //UI관련 변수들
    public GameObject lobbyUI;
    public TextMeshProUGUI playerCountText;
    public Timer gameTimer;
    public Camera lobbyCamera;

    private int requiredPlayer = 2;
    private bool gameStarted = false;

    public GameObject[] playerPrefabs;
    private List<GameObject> availablePrefabs;
    private List<int> usedPrefab = new List<int>();

    public ImageColorControl[] buttonImages;

    [PunRPC]
    private void RPC_selectPrefab(int prefabIndex, int actorNum)
    {
        Debug.Log($"Prefab index {prefabIndex} selected by actor {actorNum}");
        if (!usedPrefab.Contains(prefabIndex))
        {
            usedPrefab.Add(prefabIndex);

        }
        else
        {
            Debug.LogWarning($"Prefab index {prefabIndex} already used!");
        }
    }

    [PunRPC]
    private void RPC_UpdateCanvasImage(int buttonIndex,bool isPressed, Color Pcolor)
    {
        canvasImages[buttonIndex].color = isPressed ? Pcolor : Color.gray;
    }



    public Color GetColorByPrefabIndex(int prefabIndex)
    {
        switch(prefabIndex)
        {
            case 0: return Color.white;
            case 1: return Color.blue;
            case 2: return Color.red;
            case 3: return Color.black;
            case 4: return new Color(0.5f,0,0.5f); //보라색
            default: return Color.gray;
        }
    }

    void Awake()
    {
        //같은 룸의 유저들에게 자동으로 씬을 로딩
        PhotonNetwork.AutomaticallySyncScene = true;
        //같은 버전의 유저끼리 접속 허용
        PhotonNetwork.GameVersion = version;
        //유저 아이디 할당
        PhotonNetwork.NickName = userID;
      //  //포톤 서버와 통신 횟수 설정. 초당 30회
      //  Debug.Log(PhotonNetwork.SendRate);
        //서버 접속
        PhotonNetwork.ConnectUsingSettings();

        availablePrefabs = new List<GameObject>(playerPrefabs);
    
    }

    //포톤 서버에 접속 후 호출되는 콜백 함수
    public override void OnConnectedToMaster()
    {
      //  Debug.Log("마스터 서버에 들어왔어요! ");
      //  Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}"); //로비 입장여부 bool로 표시 아마도 false
        PhotonNetwork.JoinLobby();   //로비입장

    }

    //로비에 접속 후 호출되는 콜백 함수
    public override void OnJoinedLobby()
    { 
      //  Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}"); //아마도 true

        //랜덤한 룸에 접속하게 //랜덤 매치메이킹 기능 제공
        PhotonNetwork.JoinRandomRoom();

    }

    //랜덤한 룸 입장이 실패했을 경우 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"랜덤룸에 입장하는데 실패했습니다 {returnCode} : {message}");

        //룸의 속성을 정의
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 2; //최대 접속자수
        ro.IsOpen = true; //룸의 오픈여부
        ro.IsVisible = true; //로비에서 룸 목록에 보일지 여부

        //룸 생성
        PhotonNetwork.CreateRoom("My Rooom",ro);
    }

    //룸 생성이 완료된 후 호출되는 콜백 함수
    public override void OnCreatedRoom()
    {
        Debug.Log("룸이 생성되었습니다! ");
        Debug.Log($"룸 이름 = {PhotonNetwork.CurrentRoom.Name} ");

    }

    //룸에 입장한 후 호출되는 콜백 함수
    public override void OnJoinedRoom()
    {
        // //룸에 접속한 사용자 정보 확인     ActorNumber:플레이어고유값
        // foreach (var player in PhotonNetwork.CurrentRoom.Players)
        // {
        //     Debug.Log($"{player.Value.NickName}, {player.Value.ActorNumber}");
        // }

        UpdatePlayerCount();
        CheckAndStartGame();


    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerCount();
        CheckAndStartGame();
    }

    private void UpdatePlayerCount()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        playerCountText.text = $" {playerCount} / 2 ";
    }

    private void CheckAndStartGame()
    {
        if(!gameStarted&&PhotonNetwork.CurrentRoom.PlayerCount==requiredPlayer)
        {
            gameStarted = true;
            StartCoroutine(StartGame());
        }
    }

    private IEnumerator StartGame()
    {
        Debug.Log("여기는 StartGame()");
        yield return new WaitForSeconds(2f);

        lobbyUI.SetActive(false);
        
        if(lobbyCamera !=null)
        {
            lobbyCamera.gameObject.SetActive(false);
        }

        //캐릭터 출현 정보를 배열에 저장
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1,points.Length);
        GameObject selectedPrefab = null;

            int prefabIdx = -1;


            if (PhotonNetwork.IsMasterClient)
            {
        Debug.Log("나는 마스터 클라이언트! ");
                prefabIdx = Random.Range(0, availablePrefabs.Count);
                photonView.RPC("RPC_selectPrefab", RpcTarget.AllBuffered, prefabIdx, PhotonNetwork.LocalPlayer.ActorNumber);
            selectedPrefab = availablePrefabs[prefabIdx];
        }
            else
            {
                while (prefabIdx == -1)
                {
                    yield return null;
                    if (usedPrefab.Count > 0)
                    {
                        List<int> remainingPrefabs = new List<int>();

                        for(int i=0; i<availablePrefabs.Count;i++)
                        {
                            if(!usedPrefab.Contains(i))
                            {
                                remainingPrefabs.Add(i);
                            }

                        }
                        if (remainingPrefabs.Count > 0)
                        {
                            prefabIdx = remainingPrefabs[Random.Range(0, remainingPrefabs.Count)];
                            selectedPrefab = availablePrefabs[prefabIdx];
                        }
                    }
                }
            }

            //캐릭터 생성
            PhotonNetwork.Instantiate(selectedPrefab.name, points[idx].position, points[idx].rotation, 0);

            M_ButtonManager MBManager = FindObjectOfType<M_ButtonManager>();
            if(MBManager !=null)
            {
                //  buttonManager.SetPlayerColor(GetColorByPrefabIndex(prefabIndex));
                MBManager.SetPlayerColor(GetColorByPrefabIndex(prefabIdx));
            }
        //타이머
        gameTimer.StartTimer();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        for(int i=0; i<canvasImages.Length;i++)
        {
            if (propertiesThatChanged.ContainsKey("Button{i}State"))
            {
                bool isPressed = (bool)propertiesThatChanged[$"Button{i}State"];
                //canvasImages[i].sprite = isPressed ? newSprite : originalSprite;
                canvasImages[i].color = isPressed ? playerColors[i%playerColors.Length]:Color.gray;
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount==1)
        {
            if(UIManager.instance!=null)
            {
                UIManager.instance.ShowPausePanel_M();
            }
            Time.timeScale = 0;
        }
    }
}
