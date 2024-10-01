using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;


//    $ => string.Format()

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager instance =null;


    public Image[] canvasImages; // 18개의 캔버스 이미지
    public Sprite originalSprite;

    // 플레이어 컬러 배열
    private Color[] playerColors = { Color.yellow, Color.blue, Color.red, Color.black, new Color(0.5f, 0, 0.5f) };

    //버전 입력
    private readonly string version = "1.0f";

    //사용자 아이디 입력
    private string userID = "Han";

    //UI관련 변수들
    public GameObject lobbyUI;
    public GameObject errorUI;
    public GameObject M_PauseUI;
    public TextMeshProUGUI playerCountText;
    public Timer gameTimer;
    public Camera lobbyCamera;

    private int requiredPlayer = 2;
    private bool gameStarted = false;
   // public bool shouldAutoJoinRoom = true;
    private int jointRoomAttempts = 0;
    private const int maxJoinRoomAttempts = 3;

    public GameObject[] playerPrefabs;
    private List<GameObject> availablePrefabs;
    private List<int> usedPrefab = new List<int>();
    private List<int> usedSpawnPoints = new List<int>();
    private List<RoomInfo> availableRooms = new List<RoomInfo>(); //방 목록을 저장할 리스트

    public ImageColorControl[] buttonImages;

    public Dictionary<int, int> playerPrefabIndexes = new Dictionary<int, int>();


    [PunRPC]
    public void RPC_PauseGame()
    {
        if (!UIManager.instance.pauseUIPanel.activeInHierarchy)
        {

            Debug.Log("M_PauseUI 활성화 및 게임 멈춤");
            M_PauseUI.SetActive(true);
            Time.timeScale = 0;
        }
    }

    [PunRPC]
    public void RPC_ResumeGame()
    {
        if(Time.timeScale==0)
        {
            Debug.Log("RPC_ResumeGame()");

            Time.timeScale = 1;
            UIManager.instance.HidePauseMenu();
            M_PauseUI.SetActive(false);
        }
    }    

    [PunRPC]
    private void RPC_UpdateUsedChoices(int prefabIndex, int spawnPointIndex)
    {
        if (!usedPrefab.Contains(prefabIndex))
        {
            usedPrefab.Add(prefabIndex);
        }

        if (!usedSpawnPoints.Contains(spawnPointIndex))
        {
            usedSpawnPoints.Add(spawnPointIndex);
        }
    }

    //마스터 클라이언트가 선택한걸 다른 플레이어에게 전달함
    [PunRPC]
    private void RPC_selectPrefab(int prefabIndex, int spawnPointIndex, int actorNum)
    {
        Debug.Log($"RPC_selectPrefab=Prefab index {prefabIndex} selected by actor {actorNum}");
        if (!usedPrefab.Contains(prefabIndex))
        {
            usedPrefab.Add(prefabIndex);

        }
        if (!usedSpawnPoints.Contains(spawnPointIndex))
        {
            usedSpawnPoints.Add(spawnPointIndex);
        }

        else
        {
            Debug.LogWarning($"Prefab index {prefabIndex} already used!");
        }

        playerPrefabIndexes[actorNum] = prefabIndex; //해당 플레이어의 프리팹 인덱스 저장
        photonView.RPC("RPC_UpdateUsedChoices", RpcTarget.All, prefabIndex, spawnPointIndex);
    }

    [PunRPC]
    private void RPC_UpdateCanvasImage(int buttonIndex, bool isPressed,int actorNum, Color playercolor) //컬러값을 포함해서 버튼 누를때마다 보내주는걸로 
    {
        if (buttonIndex < 0 || buttonIndex >= canvasImages.Length)
        {
            Debug.LogError($"Invalid buttonIndex: {buttonIndex}. It is out of range.");
            return;
        }

       // //현재 플레이어의 actornumber
       // int localANum = PhotonNetwork.LocalPlayer.ActorNumber;
       // //현재 플레이어의 prefab 인덱스 
       // int localPrefabIndex = playerPrefabIndexes.ContainsKey(localANum) ? playerPrefabIndexes[localANum] : -1;
       // //버튼을 누른 플레이어의 prefab 인덱스 
       // int pressedPrefabIndex = playerPrefabIndexes.ContainsKey(actorNum) ? playerPrefabIndexes[actorNum] : -1;
       //
       // Color playerColor = GetColorByPrefabIndex(playerPrefabIndexes[actorNum]); //각자 자기정보를 업데이트 중인거같다. 

      //  Color p1Color = GetColorByPrefabIndex(1);
      //  Color p2Color = GetColorByPrefabIndex(2);

        if (isPressed)
        {
            //if (localPrefabIndex == pressedPrefabIndex)
            //{
            //    canvasImages[buttonIndex].color = Pcolor;
            //}
            //else
            //{
            //    Color opponentColor = GetColorByPrefabIndex(pressedPrefabIndex);
            //    canvasImages[buttonIndex].color = opponentColor;
            //}

            // if(actorNum==1)
            // {
            //     canvasImages[buttonIndex].color = p1Color;
            // }
            // else if(actorNum==2)
            // {
            //     canvasImages[buttonIndex].color = p2Color;
            // }

            // 버튼이 눌렸을 때 해당 플레이어의 색상으로 변경
            canvasImages[buttonIndex].color = playercolor;
        }
        else
        {
            canvasImages[buttonIndex].color = Color.white;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
       // shouldAutoJoinRoom = true;
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        availableRooms.Clear(); //방 목록을 초기화
        foreach (RoomInfo room in roomList)
        {
            if (room.PlayerCount < room.MaxPlayers)
            {
                availableRooms.Add(room);
            }

        }
    }

    public Color GetColorByPrefabIndex(int prefabIndex)
    {
        switch (prefabIndex)
        {
            case 0: return Color.yellow;
            case 1: return Color.blue;
            case 2: return Color.red;
            case 3: return Color.black;
            case 4: return new Color(0.5f, 0, 0.5f); //보라색
            default: return Color.white;
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
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        availablePrefabs = new List<GameObject>(playerPrefabs);
        //Color 직렬화 및 역직렬화 등록
        PhotonPeer.RegisterType(typeof(Color), (byte)'C', SerializeColor, DeserializeColor);
    
    if(instance==null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    else
        {
            Destroy(gameObject);
            return;
        }
    
    
    }

    void Start()
    {

    }

    //Color 직렬화 함수
    public static short SerializeColor(StreamBuffer outStream, object customObject)
    {
        Color color = (Color)customObject; //직렬화할 컬러 객체
        float[] colorData = new float[4] { color.r, color.g, color.b, color.a }; //r, g, b, a 값들을 배열로 만듦
        for (int i = 0; i < colorData.Length; i++)
        {
            byte[] bytes = System.BitConverter.GetBytes(colorData[i]); //각 float 값을 바이트 배열로 변환
            outStream.Write(bytes, 0, bytes.Length); //변환된 바이트 배열을 streambuffer에 기록
        }

        return 16; // 4개의 float 값이 각각 4바이트이므로 총 16바이트를 반환
    }

    //Color 역직렬화 함수
    public static object DeserializeColor(StreamBuffer inStream, short length)
    {
        byte[] bytes = new byte[16];
        inStream.Read(bytes, 0, 16);

        float r = System.BitConverter.ToSingle(bytes, 0);
        float g = System.BitConverter.ToSingle(bytes, 4);
        float b = System.BitConverter.ToSingle(bytes, 8);
        float a = System.BitConverter.ToSingle(bytes, 12);

        return new Color(r, g, b, a);
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
        if (SceneManager.GetActiveScene().name == "SciFi_Warehouse_M")
        {
            //랜덤한 룸에 접속하게 //랜덤 매치메이킹 기능 제공
            PhotonNetwork.JoinRandomRoom();
        }
    }

    //랜덤한 룸 입장이 실패했을 경우 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"랜덤룸에 입장하는데 실패했습니다 {returnCode} : {message}");

        if (jointRoomAttempts < maxJoinRoomAttempts)
        {
            jointRoomAttempts++;
            Debug.Log($"방 입장 재시도중...{jointRoomAttempts / maxJoinRoomAttempts}");

            foreach (RoomInfo room in availableRooms)
            {
                if (room.PlayerCount == 1 && room.MaxPlayers == 2)
                {
                    Debug.Log("플레이어가 1명인 방이 있군요. 그 방에 입장할게요~");
                    PhotonNetwork.JoinRoom(room.Name);
                    return;
                }
            }
         //   if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
         //   {
                //룸의 속성을 정의
                RoomOptions ro = new RoomOptions();
                ro.MaxPlayers = 2; //최대 접속자수
                ro.IsOpen = true; //룸의 오픈여부
                ro.IsVisible = true; //로비에서 룸 목록에 보일지 여부

                //새로운 방 이름을 동적으로 생성
                string newRoomName = $"Room_{System.Guid.NewGuid()}";
                PhotonNetwork.CreateRoom(newRoomName, ro);
                Debug.Log($"새로운 방을 만들었습니다. 이름 : { newRoomName }");
           // }
        }
        else
        {
            Debug.LogError("방 입장 시도 횟수 3회 초과, 로비로 이동하겠습니다. ");
            StartCoroutine(ShowErrorAndReturnLobby());
        }
    }

    private IEnumerator ShowErrorAndReturnLobby()
    {
        errorUI.SetActive(true);
        yield return new WaitForSeconds(3f);
        errorUI.SetActive(false);

        PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene("Demo Scene V1(Blue)");
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
        if (!gameStarted && PhotonNetwork.CurrentRoom.PlayerCount == requiredPlayer)
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

        if (lobbyCamera != null)
        {
            lobbyCamera.gameObject.SetActive(false);
        }

        //캐릭터 출현 정보를 배열에 저장
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();

        int prefabIdx = -1;
        int spawnPointIdx = -1;
        GameObject selectedPrefab = null;

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("나는 마스터 클라이언트! ");
            prefabIdx = Random.Range(0, availablePrefabs.Count);
            spawnPointIdx = Random.Range(1, points.Length);
           // selectedPrefab = availablePrefabs[prefabIdx];

            //마스터 클라이언트가 선택한 프리팹을 딕셔너리에 저장
            playerPrefabIndexes[PhotonNetwork.LocalPlayer.ActorNumber] = prefabIdx;

            //선택된 프리팹과 스폰 포인트 정보를 모든 클라이언트에게 전송
            photonView.RPC("RPC_selectPrefab", RpcTarget.All, prefabIdx, spawnPointIdx, PhotonNetwork.LocalPlayer.ActorNumber);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            do
            {
                yield return new WaitForSeconds(1f);

                prefabIdx = Random.Range(0, availablePrefabs.Count);
                spawnPointIdx = Random.Range(1, points.Length);

                Debug.Log($"Client tries prefabIdx: {prefabIdx}, spawnPointIdx: {spawnPointIdx}");
            }
            while( prefabIdx==playerPrefabIndexes[PhotonNetwork.MasterClient.ActorNumber]
                    ||spawnPointIdx==playerPrefabIndexes[PhotonNetwork.MasterClient.ActorNumber]);

            playerPrefabIndexes[PhotonNetwork.LocalPlayer.ActorNumber] = prefabIdx;

            // 선택된 프리팹과 스폰 포인트를 모든 클라이언트에게 전송
            photonView.RPC("RPC_selectPrefab", RpcTarget.All, prefabIdx, spawnPointIdx, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        selectedPrefab = availablePrefabs[prefabIdx];
        //캐릭터 생성
        PhotonNetwork.Instantiate(selectedPrefab.name, points[spawnPointIdx].position, points[spawnPointIdx].rotation, 0);

        foreach (var button in GameObject.FindGameObjectsWithTag("Button"))
        {
            ButtonTracker tracker = button.GetComponent<ButtonTracker>();
            if (tracker != null)
            {
                Color p1color = GetColorByPrefabIndex(1);
                Color p2color = GetColorByPrefabIndex(2);

                tracker.SetPlayerColor(p1color, p2color);
            }
        }
        //타이머
        gameTimer.StartTimer();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach (var key in propertiesThatChanged.Keys)
        {
            if (key.ToString().StartsWith("Button") && key.ToString().EndsWith("State"))
            {
                //버튼 인덱스 추출
                int buttonIndex = int.Parse(key.ToString().Substring(6, key.ToString().Length - 11));
                //버튼이 눌렸는지 상태를 가져옴
                bool ispressed = (bool)propertiesThatChanged[$"Button{buttonIndex}State"];
                //버튼을 누른 actor의 번호를 가져옴
                int actorNum = (int)PhotonNetwork.CurrentRoom.CustomProperties[$"Button{buttonIndex}Actor"];
                //actorNum에 따른 색상 결정
                int pressedPrefabIndex = playerPrefabIndexes.ContainsKey(actorNum) ? playerPrefabIndexes[actorNum] : -1;
                Color pColor = GetColorByPrefabIndex(pressedPrefabIndex);

                if (ispressed)
                {
                    canvasImages[buttonIndex].color = pColor;
                }
                else
                {
                    canvasImages[buttonIndex].color = Color.white;
                }
            }
        }

        for (int i = 0; i < canvasImages.Length; i++)
        {
            if (propertiesThatChanged.ContainsKey($"Button{i}State"))
            {
                bool isPressed = (bool)propertiesThatChanged[$"Button{i}State"];
                //canvasImages[i].sprite = isPressed ? newSprite : originalSprite;
                canvasImages[i].color = isPressed ? playerColors[i % playerColors.Length] : Color.white;
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            if (UIManager.instance != null)
            {
                UIManager.instance.ShowPausePanel_M();
            }
            Time.timeScale = 0;
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방에서 나갔습니다.");
    }

    //네트워크 불안정으로 인해 연결이 끊어진 경우, 재접속과 방에 재입장할 수 있도록 처리
    //하지만 플레이어 한명이 나가는 순간 남은 플레이어에게 ui가 뜨고, 멈추고 로비로 나가게 할 것임으로
    // 이것만으로는 게임이 진행되게 하기 어려운 것 같아서 일단 보류 

    // public override void OnDisconnected(DisconnectCause cause)
    // {
    //     Debug.Log($"네트워크 연결이 끊어졌습니다. 원인 : {cause}");
    //
    //     if(cause != DisconnectCause.None)
    //     {
    //         Debug.Log("재접속을 시도합니다...");
    //         PhotonNetwork.ReconnectAndRejoin();
    //     }
    // }
}
