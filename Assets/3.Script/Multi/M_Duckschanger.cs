using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class M_Duckschanger : MonoBehaviour
{
    public GameObject[] ducksPrefabs;
    public GameObject defaultduckPrefab;

    private PhotonManager ptManager;
    private PhotonView ptView;
    private GameObject currentDuck;

    private void Start()
    {
        ptManager = FindObjectOfType<PhotonManager>();
        ptView = GetComponent<PhotonView>();
    }

    public void HandleDuckOnButton(GameObject duckObject)
    {
            int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;

        PhotonView dptview = duckObject.GetComponent<PhotonView>(); // 오리 오브젝트의 PhotonView를 가져옴
        if (dptview==null)
        {
            Debug.LogError("오리 오브젝트에 PhotonView가 없습니다!");
            return;
        }

        if(dptview.Owner!=PhotonNetwork.LocalPlayer) // 오리의 소유권이 현재 플레이어에게 있는지 확인
        {
            dptview.RequestOwnership(); // 소유권 요청
        }

        if (ptManager.playerPrefabIndexes.ContainsKey(actorNum))
            {
                int ppIndex = ptManager.playerPrefabIndexes[actorNum]; // 플레이어의 프리팹 인덱스를 가져옴

            // RPC 호출로 오리 색상 변경
            ptView.RPC("RPC_ChangeDuck", RpcTarget.All, duckObject.GetComponent<PhotonView>().ViewID, ppIndex);
            }
    }
    public void RsetDuck(GameObject duckObject)
    {
        PhotonView duckPhotonView = duckObject.GetComponent<PhotonView>();
        if (duckPhotonView == null)
        {
            Debug.LogError("오리 오브젝트에 PhotonView가 없습니다!");
            return;
        }
        duckPhotonView.RPC("RPC_ChangeDuck", RpcTarget.All, duckPhotonView.ViewID, -1);
    }
  
    [PunRPC]
    private void RPC_ChangeDuck(int duckViewID, int prefIndex)
    {
        Debug.Log("RPC_ChangeDuck rpc ");

        GameObject duckobj = PhotonView.Find(duckViewID).gameObject; // 오리 오브젝트를 ViewID로 찾음

        MeshRenderer drenderer = duckobj.GetComponent<MeshRenderer>();

        Color newColor = (prefIndex == -1) ? Color.gray : ptManager.GetColorByPrefabIndex(prefIndex);

            drenderer.material.color = newColor; // 색상 변경
        Debug.Log($"오리 색상 변경 완료: {newColor}");
    }
}
