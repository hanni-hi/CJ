using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

//    $ => string.Format()

public class PhotonManager : MonoBehaviourPunCallbacks
{
    //버전 입력
    private readonly string version = "1.0f";

    //사용자 아이디 입력
    private string userID = "Han";

    void Awake()
    {
        //같은 룸의 유저들에게 자동으로 씬을 로딩
        PhotonNetwork.AutomaticallySyncScene = true;
        //같은 버전의 유저끼리 접속 허용
        PhotonNetwork.GameVersion = version;
        //유저 아이디 할당
        PhotonNetwork.NickName = userID;
        //포톤 서버와 통신 횟수 설정. 초당 30회
        Debug.Log(PhotonNetwork.SendRate);
        //서버 접속
        PhotonNetwork.ConnectUsingSettings();
    
    }

    //포톤 서버에 접속 후 호출되는 콜백 함수
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버에 들어왔어요! ");
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}"); //로비 입장여부 bool로 표시 아마도 false
        PhotonNetwork.JoinLobby();   //로비입장

    }

    //로비에 접속 후 호출되는 콜백 함수
    public override void OnJoinedLobby()
    { 
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}"); //아마도 true

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
        Debug.Log($"PhotonNetwork.InRoom = {PhotonNetwork.InRoom}");
        Debug.Log($"현재 플레이어 수 = {PhotonNetwork.CurrentRoom.PlayerCount}");

        //룸에 접속한 사용자 정보 확인     ActorNumber:플레이어고유값
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{player.Value.NickName}, {player.Value.ActorNumber}");
        }

        //캐릭터 출현 정보를 배열에 저장
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1,points.Length);

        //캐릭터 생성
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
