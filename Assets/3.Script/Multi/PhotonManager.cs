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
    private Color[] playerColors = { Color.white, Color.blue,Color.red,Color.black,new Color(0.5f,0,0.5f) };

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
        int idx = Random.Range(1,points.Length);
        GameObject selectedPrefab = null;

            int prefabIdx = -1;


            if (PhotonNetwork.IsMasterClient)
            {
        Debug.Log("���� ������ Ŭ���̾�Ʈ! ");
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

            //ĳ���� ����
            PhotonNetwork.Instantiate(selectedPrefab.name, points[idx].position, points[idx].rotation, 0);

            M_ButtonManager MBManager = FindObjectOfType<M_ButtonManager>();
            if(MBManager !=null)
            {
                //  buttonManager.SetPlayerColor(GetColorByPrefabIndex(prefabIndex));
                MBManager.SetPlayerColor(GetColorByPrefabIndex(prefabIdx));
            }
        //Ÿ�̸�
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
