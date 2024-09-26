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

        PhotonView dptview = duckObject.GetComponent<PhotonView>(); // ���� ������Ʈ�� PhotonView�� ������
        if (dptview==null)
        {
            Debug.LogError("���� ������Ʈ�� PhotonView�� �����ϴ�!");
            return;
        }

        if(dptview.Owner!=PhotonNetwork.LocalPlayer) // ������ �������� ���� �÷��̾�� �ִ��� Ȯ��
        {
            dptview.RequestOwnership(); // ������ ��û
        }

        if (ptManager.playerPrefabIndexes.ContainsKey(actorNum))
            {
                int ppIndex = ptManager.playerPrefabIndexes[actorNum]; // �÷��̾��� ������ �ε����� ������

            // RPC ȣ��� ���� ���� ����
            ptView.RPC("RPC_ChangeDuck", RpcTarget.All, duckObject.GetComponent<PhotonView>().ViewID, ppIndex);
            }
    }
    public void RsetDuck(GameObject duckObject)
    {
        PhotonView duckPhotonView = duckObject.GetComponent<PhotonView>();
        if (duckPhotonView == null)
        {
            Debug.LogError("���� ������Ʈ�� PhotonView�� �����ϴ�!");
            return;
        }
        duckPhotonView.RPC("RPC_ChangeDuck", RpcTarget.All, duckPhotonView.ViewID, -1);
    }
  
    [PunRPC]
    private void RPC_ChangeDuck(int duckViewID, int prefIndex)
    {
        Debug.Log("RPC_ChangeDuck rpc ");

        GameObject duckobj = PhotonView.Find(duckViewID).gameObject; // ���� ������Ʈ�� ViewID�� ã��

        MeshRenderer drenderer = duckobj.GetComponent<MeshRenderer>();

        Color newColor = (prefIndex == -1) ? Color.gray : ptManager.GetColorByPrefabIndex(prefIndex);

            drenderer.material.color = newColor; // ���� ����
        Debug.Log($"���� ���� ���� �Ϸ�: {newColor}");
    }
}
