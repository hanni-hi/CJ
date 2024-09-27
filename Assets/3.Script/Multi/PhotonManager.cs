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
    public Image[] canvasImages; // 18���� ĵ���� �̹���
    public Sprite originalSprite;

    // �÷��̾� �÷� �迭
    private Color[] playerColors = { Color.yellow, Color.blue,Color.red,Color.black,new Color(0.5f,0,0.5f) };

    //���� �Է�
    private readonly string version = "1.0f";

    //����� ���̵� �Է�
    private string userID = "Han";

    //UI���� ������
    public GameObject lobbyUI;
    public TextMeshProUGUI playerCountText;
    public Timer gameTimer;
    public Camera lobbyCamera;

    private int requiredPlayer = 2;
    private bool gameStarted = false;

    public GameObject[] playerPrefabs;
    private List<GameObject> availablePrefabs;
    private List<int> usedPrefab = new List<int>();
    private List<int> usedSpawnPoints = new List<int>();

    public ImageColorControl[] buttonImages;

    public Dictionary<int, int> playerPrefabIndexes = new Dictionary<int, int>();

    [PunRPC]
    private void RPC_UpdateUsedChoices(int prefabIndex, int spawnPointIndex)
    {
       if(!usedPrefab.Contains(prefabIndex))
        {
            usedPrefab.Add(prefabIndex);
        }
       
   if(!usedSpawnPoints.Contains(spawnPointIndex))
        {
            usedSpawnPoints.Add(spawnPointIndex);
        }
    }

    //������ Ŭ���̾�Ʈ�� �����Ѱ� �ٸ� �÷��̾�� ������
    [PunRPC]
    private void RPC_selectPrefab(int prefabIndex,int spawnPointIndex, int actorNum)
    {
        Debug.Log($"RPC_selectPrefab=Prefab index {prefabIndex} selected by actor {actorNum}");
        if (!usedPrefab.Contains(prefabIndex))
        {
            usedPrefab.Add(prefabIndex);

        }
        if(!usedSpawnPoints.Contains(spawnPointIndex))
        {
            usedSpawnPoints.Add(spawnPointIndex);
        }

        else
        {
            Debug.LogWarning($"Prefab index {prefabIndex} already used!");
        }

       photonView.RPC("RPC_UpdateUsedChoices",RpcTarget.AllBuffered,prefabIndex,spawnPointIndex);
    }

    [PunRPC]
    private void RPC_UpdateCanvasImage(int buttonIndex,bool isPressed, Color Pcolor, int actorNum)
    {
        //���� �÷��̾��� actornumber
        int localANum = PhotonNetwork.LocalPlayer.ActorNumber;
        //���� �÷��̾��� prefab �ε��� 
        int localPrefabIndex = playerPrefabIndexes.ContainsKey(localANum) ? playerPrefabIndexes[localANum] : -1;
        //��ư�� ���� �÷��̾��� prefab �ε��� 
        int pressedPrefabIndex = playerPrefabIndexes.ContainsKey(actorNum) ? playerPrefabIndexes[actorNum] : -1;


       if(isPressed)
        {
            if (localPrefabIndex == pressedPrefabIndex)
            {
                canvasImages[buttonIndex].color = Pcolor;
            }
            else
            {
                Color opponentColor = GetColorByPrefabIndex(pressedPrefabIndex);
                canvasImages[buttonIndex].color = opponentColor;
            }
       }
       else
        {
            canvasImages[buttonIndex].color = Color.gray;
        }
    }

    public Color GetColorByPrefabIndex(int prefabIndex)
    {
        switch(prefabIndex)
        {
            case 0: return Color.yellow;
            case 1: return Color.blue;
            case 2: return Color.red;
            case 3: return Color.black;
            case 4: return new Color(0.5f,0,0.5f); //�����
            default: return Color.gray;
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
        PhotonNetwork.ConnectUsingSettings();

        availablePrefabs = new List<GameObject>(playerPrefabs);
    
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

        //������ �뿡 �����ϰ� //���� ��ġ����ŷ ��� ����
        PhotonNetwork.JoinRandomRoom();

    }

    //������ �� ������ �������� ��� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"�����뿡 �����ϴµ� �����߽��ϴ� {returnCode} : {message}");

        //���� �Ӽ��� ����
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 2; //�ִ� �����ڼ�
        ro.IsOpen = true; //���� ���¿���
        ro.IsVisible = true; //�κ񿡼� �� ��Ͽ� ������ ����

        //�� ����
        PhotonNetwork.CreateRoom("My Rooom",ro);
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
        if(!gameStarted&&PhotonNetwork.CurrentRoom.PlayerCount==requiredPlayer)
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
        
        if(lobbyCamera !=null)
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
            selectedPrefab = availablePrefabs[prefabIdx];
                    usedPrefab.Add(prefabIdx);
                    usedSpawnPoints.Add(spawnPointIdx);

            playerPrefabIndexes[PhotonNetwork.LocalPlayer.ActorNumber] = prefabIdx;

            //���õ� �����հ� ���� ����Ʈ ������ ��� Ŭ���̾�Ʈ���� ����
                photonView.RPC("RPC_selectPrefab", RpcTarget.AllBuffered, prefabIdx,spawnPointIdx ,PhotonNetwork.LocalPlayer.ActorNumber);
        }
            else
            {
            // �� ��° �÷��̾ ���� ������������ ������ �� �ֵ��� ����
            while (prefabIdx == -1||spawnPointIdx==-1)
                {
                    yield return null;

                        List<int> remainingPrefabs = new List<int>();
                        List<int> remainingSpawnPoints = new List<int>();

                // ���� �������� �������� �߰�
                for (int i=0; i<availablePrefabs.Count;i++)
                        {
                            if(!usedPrefab.Contains(i))
                            {
                                remainingPrefabs.Add(i);
                            }
                        }

                // ���� ���� ����Ʈ�� �������� �߰�
                for (int i=1; i<points.Length;i++)
                    {
                        if(!usedSpawnPoints.Contains(i))
                        {
                            remainingSpawnPoints.Add(i);
                        }
                    }
                // ���� �����հ� ���� ����Ʈ���� �������� ����
                if (remainingPrefabs.Count > 0&& remainingSpawnPoints.Count > 0)
                        {
                            prefabIdx = remainingPrefabs[Random.Range(0, remainingPrefabs.Count)];
                        spawnPointIdx = remainingSpawnPoints[Random.Range(0, remainingSpawnPoints.Count)];
                    
           
                    selectedPrefab = availablePrefabs[prefabIdx];
            playerPrefabIndexes[PhotonNetwork.LocalPlayer.ActorNumber] = prefabIdx;

                 //   usedPrefab.Add(prefabIdx);
                 //   usedSpawnPoints.Add(spawnPointIdx);

                    // ���õ� �����հ� ���� ����Ʈ�� ��� Ŭ���̾�Ʈ���� ����
                    photonView.RPC("RPC_selectPrefab",RpcTarget.AllBuffered,prefabIdx,spawnPointIdx,PhotonNetwork.LocalPlayer.ActorNumber);
                }
                }

            }

            //ĳ���� ����
            PhotonNetwork.Instantiate(selectedPrefab.name, points[spawnPointIdx].position, points[spawnPointIdx].rotation, 0);

            foreach(var button in GameObject.FindGameObjectsWithTag("Button"))
        {
            ButtonTracker tracker = button.GetComponent<ButtonTracker>();
            if(tracker!=null)
            {
                tracker.SetPlayerColor(GetColorByPrefabIndex(prefabIdx));
            }
        }
        //Ÿ�̸�
        gameTimer.StartTimer();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach(var key in propertiesThatChanged.Keys)
        {
            if(key.ToString().StartsWith("Button")&&key.ToString().EndsWith("State"))
            {
                //��ư �ε��� ����
                int buttonIndex = int.Parse(key.ToString().Substring(6,key.ToString().Length-11));
                //��ư�� ���ȴ��� ���¸� ������
                bool ispressed = (bool)propertiesThatChanged[$"Button{buttonIndex}State"];
                //��ư�� ���� actor�� ��ȣ�� ������
                int actorNum = (int)PhotonNetwork.CurrentRoom.CustomProperties[$"Button{buttonIndex}Actor"];
                //actorNum�� ���� ���� ����
                int pressedPrefabIndex = playerPrefabIndexes.ContainsKey(actorNum) ? playerPrefabIndexes[actorNum] : -1;
                Color pColor = GetColorByPrefabIndex(pressedPrefabIndex);
            
            if(ispressed)
                {
                    canvasImages[buttonIndex].color = pColor;
                }
            else
                {
                    canvasImages[buttonIndex].color = Color.gray;
                }
            }
        }

       // for(int i=0; i<canvasImages.Length;i++)
       // {
       //     if (propertiesThatChanged.ContainsKey($"Button{i}State"))
       //     {
       //         bool isPressed = (bool)propertiesThatChanged[$"Button{i}State"];
       //         //canvasImages[i].sprite = isPressed ? newSprite : originalSprite;
       //         canvasImages[i].color = isPressed ? playerColors[i%playerColors.Length]:Color.gray;
       //     }
       // }
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
