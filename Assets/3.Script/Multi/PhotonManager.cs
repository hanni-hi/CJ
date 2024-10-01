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
    private Color[] playerColors = { Color.yellow, Color.blue, Color.red, Color.black, new Color(0.5f, 0, 0.5f) };

    //���� �Է�
    private readonly string version = "1.0f";

    //����� ���̵� �Է�
    private string userID = "Han";

    //UI���� ������
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
    private List<RoomInfo> availableRooms = new List<RoomInfo>(); //�� ����� ������ ����Ʈ

    public ImageColorControl[] buttonImages;

    public Dictionary<int, int> playerPrefabIndexes = new Dictionary<int, int>();


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

       // //���� �÷��̾��� actornumber
       // int localANum = PhotonNetwork.LocalPlayer.ActorNumber;
       // //���� �÷��̾��� prefab �ε��� 
       // int localPrefabIndex = playerPrefabIndexes.ContainsKey(localANum) ? playerPrefabIndexes[localANum] : -1;
       // //��ư�� ���� �÷��̾��� prefab �ε��� 
       // int pressedPrefabIndex = playerPrefabIndexes.ContainsKey(actorNum) ? playerPrefabIndexes[actorNum] : -1;
       //
       // Color playerColor = GetColorByPrefabIndex(playerPrefabIndexes[actorNum]); //���� �ڱ������� ������Ʈ ���ΰŰ���. 

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

            // ��ư�� ������ �� �ش� �÷��̾��� �������� ����
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
            case 0: return Color.yellow;
            case 1: return Color.blue;
            case 2: return Color.red;
            case 3: return Color.black;
            case 4: return new Color(0.5f, 0, 0.5f); //�����
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

            // ���õ� �����հ� ���� ����Ʈ�� ��� Ŭ���̾�Ʈ���� ����
            photonView.RPC("RPC_selectPrefab", RpcTarget.All, prefabIdx, spawnPointIdx, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        selectedPrefab = availablePrefabs[prefabIdx];
        //ĳ���� ����
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
        //Ÿ�̸�
        gameTimer.StartTimer();
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
