using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

//    $ => string.Format()

public class PhotonManager : MonoBehaviourPunCallbacks
{
    //���� �Է�
    private readonly string version = "1.0f";

    //����� ���̵� �Է�
    private string userID = "Han";

    void Awake()
    {
        //���� ���� �����鿡�� �ڵ����� ���� �ε�
        PhotonNetwork.AutomaticallySyncScene = true;
        //���� ������ �������� ���� ���
        PhotonNetwork.GameVersion = version;
        //���� ���̵� �Ҵ�
        PhotonNetwork.NickName = userID;
        //���� ������ ��� Ƚ�� ����. �ʴ� 30ȸ
        Debug.Log(PhotonNetwork.SendRate);
        //���� ����
        PhotonNetwork.ConnectUsingSettings();
    
    }

    //���� ������ ���� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ������ ���Ծ��! ");
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}"); //�κ� ���忩�� bool�� ǥ�� �Ƹ��� false
        PhotonNetwork.JoinLobby();   //�κ�����

    }

    //�κ� ���� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinedLobby()
    { 
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}"); //�Ƹ��� true

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
        Debug.Log($"PhotonNetwork.InRoom = {PhotonNetwork.InRoom}");
        Debug.Log($"���� �÷��̾� �� = {PhotonNetwork.CurrentRoom.PlayerCount}");

        //�뿡 ������ ����� ���� Ȯ��     ActorNumber:�÷��̾������
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{player.Value.NickName}, {player.Value.ActorNumber}");
        }

        //ĳ���� ���� ������ �迭�� ����
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1,points.Length);

        //ĳ���� ����
        PhotonNetwork.Instantiate("Stylized Astronaut",points[idx].position,points[idx].rotation,0);

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
