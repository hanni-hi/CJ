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


    public Image[] canvasImages; // 18���� ĵ���� �̹���
    public Sprite originalSprite;

    // �÷��̾� �÷� �迭
    private Color[] playerColors = { new Color(1f, 1f, 0.7f), new Color(0.2f, 0.4f, 0.6f), Color.red, Color.black, new Color(0.6f, 0.1f, 0.4f) };

    //���� �Է�
    private readonly string version = "1.0f";

    //����� ���̵� �Է�
    private string userID = "Han";

    //UI���� ������
    public GameObject lobbyUI;
    public GameObject errorUI;
    public GameObject M_PauseUI;
    public GameObject MVictoryUI;
    public GameObject MLoseUI;
    public GameObject MDrawUI;

    public TextMeshProUGUI playerCountText;
    public Timer gameTimer;
    public Camera lobbyCamera;

    private int requiredPlayer = 2;
    private bool gameStarted = false;
    private bool isCilentReady = false;

   // public bool shouldAutoJoinRoom = true;
    private int jointRoomAttempts = 0;
    private const int maxJoinRoomAttempts = 3;

    public GameObject[] playerPrefabs;
    private List<GameObject> availablePrefabs;
    private List<int> usedPrefab = new List<int>();
    private List<int> usedSpawnPoints = new List<int>();
    private List<RoomInfo> availableRooms = new List<RoomInfo>(); //�� ����� ������ ����Ʈ

    public Dictionary<int, int> playerPrefabIndexes = new Dictionary<int, int>();
    public Dictionary<int, int> playerButtonCount = new Dictionary<int, int>();

    [PunRPC]
    public void RPC_ClientReady()
    {
        isCilentReady = true;
    }


    [PunRPC]
    public void RPC_PauseGame()
    {
        if (!UIManager.instance.pauseUIPanel.activeInHierarchy)
        {

            Debug.Log("M_PauseUI Ȱ��ȭ �� ���� ����");
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

    //������ Ŭ���̾�Ʈ�� �����Ѱ� �ٸ� �÷��̾�� ������
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

        playerPrefabIndexes[actorNum] = prefabIndex; //�ش� �÷��̾��� ������ �ε��� ����
        photonView.RPC("RPC_UpdateUsedChoices", RpcTarget.All, prefabIndex, spawnPointIndex);
    }

    [PunRPC]
    private void RPC_UpdateCanvasImage(int buttonIndex, bool isPressed,int actorNum, Color playercolor) //�÷����� �����ؼ� ��ư ���������� �����ִ°ɷ� 
    {
        if (buttonIndex < 0 || buttonIndex >= canvasImages.Length)
        {
            Debug.LogError($"Invalid buttonIndex: {buttonIndex}. It is out of range.");
            return;
        }

        if (isPressed)
        {
            Debug.Log($"��������Ʈ �����մϴ�! {actorNum} : {playercolor}");
            canvasImages[buttonIndex].color = playercolor;

            ShowButtonCount();
        }
        else
        {
            Debug.Log("��������Ʈ�� �����մϴ�! ");
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
        availableRooms.Clear(); //�� ����� �ʱ�ȭ
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
            case 0: return new Color(1f, 1f, 0.7f);
            case 1: return new Color(0.2f, 0.4f, 0.6f);
            case 2: return Color.red;
            case 3: return Color.black;
            case 4: return new Color(0.6f, 0.1f, 0.4f); //�����
            default: return Color.white;
        }
    }

    void Awake()
    {
        //���� ���� �����鿡�� �ڵ����� ���� �ε�
        PhotonNetwork.AutomaticallySyncScene = true;
        //���� ������ �������� ���� ���
        PhotonNetwork.GameVersion = version;
        //���� ���̵� �Ҵ�
        PhotonNetwork.NickName = userID;
        //  //���� ������ ��� Ƚ�� ����. �ʴ� 30ȸ
        //  Debug.Log(PhotonNetwork.SendRate);
        //���� ����
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        availablePrefabs = new List<GameObject>(playerPrefabs);
        //Color ����ȭ �� ������ȭ ���
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

    //Color ����ȭ �Լ�
    public static short SerializeColor(StreamBuffer outStream, object customObject)
    {
        Color color = (Color)customObject; //����ȭ�� �÷� ��ü
        float[] colorData = new float[4] { color.r, color.g, color.b, color.a }; //r, g, b, a ������ �迭�� ����
        for (int i = 0; i < colorData.Length; i++)
        {
            byte[] bytes = System.BitConverter.GetBytes(colorData[i]); //�� float ���� ����Ʈ �迭�� ��ȯ
            outStream.Write(bytes, 0, bytes.Length); //��ȯ�� ����Ʈ �迭�� streambuffer�� ���
        }

        return 16; // 4���� float ���� ���� 4����Ʈ�̹Ƿ� �� 16����Ʈ�� ��ȯ
    }

    //Color ������ȭ �Լ�
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


    //���� ������ ���� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnConnectedToMaster()
    {
        //  Debug.Log("������ ������ ���Ծ��! ");
        //  Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}"); //�κ� ���忩�� bool�� ǥ�� �Ƹ��� false
        PhotonNetwork.JoinLobby();   //�κ�����

    }

    //�κ� ���� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinedLobby()
    {
        //  Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}"); //�Ƹ��� true
        if (SceneManager.GetActiveScene().name == "SciFi_Warehouse_M")
        {
            //������ �뿡 �����ϰ� //���� ��ġ����ŷ ��� ����
            PhotonNetwork.JoinRandomRoom();
        }
    }

    //������ �� ������ �������� ��� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"�����뿡 �����ϴµ� �����߽��ϴ� {returnCode} : {message}");

        if (jointRoomAttempts < maxJoinRoomAttempts)
        {
            jointRoomAttempts++;
            Debug.Log($"�� ���� ��õ���...{jointRoomAttempts / maxJoinRoomAttempts}");

            foreach (RoomInfo room in availableRooms)
            {
                if (room.PlayerCount == 1 && room.MaxPlayers == 2)
                {
                    Debug.Log("�÷��̾ 1���� ���� �ֱ���. �� �濡 �����ҰԿ�~");
                    PhotonNetwork.JoinRoom(room.Name);
                    return;
                }
            }
         //   if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
         //   {
                //���� �Ӽ��� ����
                RoomOptions ro = new RoomOptions();
                ro.MaxPlayers = 2; //�ִ� �����ڼ�
                ro.IsOpen = true; //���� ���¿���
                ro.IsVisible = true; //�κ񿡼� �� ��Ͽ� ������ ����

                //���ο� �� �̸��� �������� ����
                string newRoomName = $"Room_{System.Guid.NewGuid()}";
                PhotonNetwork.CreateRoom(newRoomName, ro);
                Debug.Log($"���ο� ���� ��������ϴ�. �̸� : { newRoomName }");
           // }
        }
        else
        {
            Debug.LogError("�� ���� �õ� Ƚ�� 3ȸ �ʰ�, �κ�� �̵��ϰڽ��ϴ�. ");
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

    //�� ������ �Ϸ�� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnCreatedRoom()
    {
        Debug.Log("���� �����Ǿ����ϴ�! ");
        Debug.Log($"�� �̸� = {PhotonNetwork.CurrentRoom.Name} ");

    }

    //�뿡 ������ �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinedRoom()
    {
        // //�뿡 ������ ����� ���� Ȯ��     ActorNumber:�÷��̾������
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
        Debug.Log("����� StartGame()");
        yield return new WaitForSeconds(2f);

        lobbyUI.SetActive(false);

        if (lobbyCamera != null)
        {
            lobbyCamera.gameObject.SetActive(false);
        }

        //ĳ���� ���� ������ �迭�� ����
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();

        int prefabIdx = -1;
        int spawnPointIdx = -1;
        GameObject selectedPrefab = null;

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("���� ������ Ŭ���̾�Ʈ! ");
            prefabIdx = Random.Range(0, availablePrefabs.Count);
            spawnPointIdx = Random.Range(1, points.Length);
           // selectedPrefab = availablePrefabs[prefabIdx];

            //������ Ŭ���̾�Ʈ�� ������ �������� ��ųʸ��� ����
            playerPrefabIndexes[PhotonNetwork.LocalPlayer.ActorNumber] = prefabIdx;

            //���õ� �����հ� ���� ����Ʈ ������ ��� Ŭ���̾�Ʈ���� ����
            photonView.RPC("RPC_selectPrefab", RpcTarget.All, prefabIdx, spawnPointIdx, PhotonNetwork.LocalPlayer.ActorNumber);
            yield return StartCoroutine(WaitForClient());
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

            // ���õ� �����հ� ���� ����Ʈ�� ��� Ŭ���̾�Ʈ���� ����
            photonView.RPC("RPC_selectPrefab", RpcTarget.All, prefabIdx, spawnPointIdx, PhotonNetwork.LocalPlayer.ActorNumber);

            ClientReady();
        }

        selectedPrefab = availablePrefabs[prefabIdx];
        //ĳ���� ����
        PhotonNetwork.Instantiate(selectedPrefab.name, points[spawnPointIdx].position, points[spawnPointIdx].rotation, 0);

        //Ÿ�̸�
        gameTimer.StartTimer();
    }

    private IEnumerator WaitForClient()
    {
        isCilentReady = false;
        float waitTime = 5f;
        float elapsedTime = 0f;

        while(!isCilentReady&&elapsedTime<waitTime)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        if(isCilentReady)
        {
            Debug.Log("Ŭ���̾�Ʈ �غ� �Ϸ�!");
        }
        else
        {
            Debug.LogWarning("���� Ŭ���̾�Ʈ�� �غ� �ȵƳ׿�.");
        }
    }

    public void ClientReady()
    {
        photonView.RPC("RPC_ClientReady",RpcTarget.MasterClient);
    }



    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach (var key in propertiesThatChanged.Keys)
        {
            if (key.ToString().StartsWith("Button") && key.ToString().EndsWith("State"))
            {
                //��ư �ε��� ����
                int buttonIndex = int.Parse(key.ToString().Substring(6, key.ToString().Length - 11));
                //��ư�� ���ȴ��� ���¸� ������
                bool ispressed = (bool)propertiesThatChanged[$"Button{buttonIndex}State"];
                //��ư�� ���� actor�� ��ȣ�� ������
                int actorNum = (int)PhotonNetwork.CurrentRoom.CustomProperties[$"Button{buttonIndex}Actor"];
                //actorNum�� ���� ���� ����
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
        Debug.Log("�濡�� �������ϴ�.");
    }

    public void IncrementButtonCount(int actorNum)
    {
if(!playerButtonCount.ContainsKey(actorNum))
        {
            playerButtonCount[actorNum] = 0;
        }
        playerButtonCount[actorNum]++;
    }

    public void DecrementButtonCount(int actorNum)
    {
        if(playerButtonCount.ContainsKey(actorNum)&&playerButtonCount[actorNum]>0)
        {
            playerButtonCount[actorNum]--;
        }
    }


    public void ShowButtonCount()
    {
        foreach(var player in playerButtonCount)
        {
            Debug.Log($"�÷��̾� {player.Key} �� {player.Value} �� ��ư�� �������ϴ�! ");
        }
    }

    public void EndMGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("�����Ϳ��� EndMGame ȣ���");

            int player1count = 0;
            int player2count = 0;

            ShowButtonCount();

            if(playerButtonCount.ContainsKey(1))
            {
                player1count = playerButtonCount[1];
            }
            if(playerButtonCount.ContainsKey(2))
            {
                player2count = playerButtonCount[2];
            }

            Debug.Log($"actor 1 : {player1count}  // actor 2 : {player2count} ");

            int winningPlayer = 0;

            if(player1count>player2count)
            {
                winningPlayer = 1;
            }
            else if(player2count>player1count)
            {
                winningPlayer = 2;
            }

            photonView.RPC("RPC_Endgame", RpcTarget.All, winningPlayer);

            // foreach (var image in canvasImages)
            // {
            //     if (image.color == GetColorByPrefabIndex(1))
            //     {
            //         player1count++;
            //     }
            //     else if (image.color == GetColorByPrefabIndex(2))
            //     {
            //         player2count++;
            //     }
            // }
            //
            // int winningPlayer = 0;
            //
            // Debug.Log($"actor 1 : {player1count}  // actor 2 : {player2count} ");
            //
            // if (player1count > player2count)
            // {
            //     winningPlayer = 1;
            // }
            // else if (player1count < player2count)
            // {
            //     winningPlayer = 2;
            // }
            //
            // photonView.RPC("RPC_Endgame",RpcTarget.All,winningPlayer);
        }
    }

    [PunRPC]
    public void RPC_Endgame(int winningPlayer)
    {
        Debug.Log($"Received winningPlayer: {winningPlayer}");

        int localPlayerActorNum = PhotonNetwork.LocalPlayer.ActorNumber;

        if (winningPlayer==localPlayerActorNum)
        {
            ShowVictoryUI(localPlayerActorNum);
        }
        else if(winningPlayer==0)
        {
            ShowDrawUI();
        }
        else
        {
            ShowLoseUI(localPlayerActorNum);
        }
    }

    private void ShowVictoryUI(int playerNum)
    {
        Debug.Log("Victory UI Ȱ��ȭ");
        MVictoryUI.SetActive(true);

        StartCoroutine(TransitionToLobby(MVictoryUI));
    }

    private void ShowLoseUI(int playerNum)
    {
        MLoseUI.SetActive(true);

        StartCoroutine(TransitionToLobby(MLoseUI));
    }

    private void ShowDrawUI()
    {
        MDrawUI.SetActive(true);

        StartCoroutine(TransitionToLobby(MDrawUI));
    }

    private IEnumerator TransitionToLobby(GameObject activeUI)
    {
        yield return new WaitForSeconds(10F);

        activeUI.SetActive(false);
     
        SceneManager.LoadScene("Demo Scene V1(Blue)");
    }

    //��Ʈ��ũ �Ҿ������� ���� ������ ������ ���, �����Ӱ� �濡 �������� �� �ֵ��� ó��
    //������ �÷��̾� �Ѹ��� ������ ���� ���� �÷��̾�� ui�� �߰�, ���߰� �κ�� ������ �� ��������
    // �̰͸����δ� ������ ����ǰ� �ϱ� ����� �� ���Ƽ� �ϴ� ���� 

    // public override void OnDisconnected(DisconnectCause cause)
    // {
    //     Debug.Log($"��Ʈ��ũ ������ ���������ϴ�. ���� : {cause}");
    //
    //     if(cause != DisconnectCause.None)
    //     {
    //         Debug.Log("�������� �õ��մϴ�...");
    //         PhotonNetwork.ReconnectAndRejoin();
    //     }
    // }
}
