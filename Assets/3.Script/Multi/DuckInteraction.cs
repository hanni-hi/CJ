using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DuckInteraction : MonoBehaviour
{
    private PhotonView ptview;
    private int lastPlayerActorNum = -1;

    private PhotonManager ptManager;

    void Start()
    {
        ptview = GetComponent<PhotonView>();
        ptManager=FindObjectOfType<PhotonManager>();
    }
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            PhotonView playerView = other.gameObject.GetComponent<PhotonView>();
            if(playerView !=null)
            {
                lastPlayerActorNum = playerView.Owner.ActorNumber;
                Debug.Log($"������ �÷��̾� {lastPlayerActorNum}�� �浹�߽��ϴ�.");
            }
        }
    }

    public void HandleDuckOnButton()
    {
        Debug.Log($"HandleDuckOnButton ȣ��� - lastPlayerActorNum: {lastPlayerActorNum}");
        if (lastPlayerActorNum != -1 && ptManager.playerPrefabIndexes.ContainsKey(lastPlayerActorNum))
        {
            int playerPrefabIndex = ptManager.playerPrefabIndexes[lastPlayerActorNum];
            Color playerColor = ptManager.GetColorByPrefabIndex(playerPrefabIndex);

            Debug.Log($"���� ���� ���� �õ�: {playerColor}");

            ptview.RPC("RPC_ChangeDuckColor",RpcTarget.All,playerColor);

            if(!ptview.IsMine)
            {
                ptview.TransferOwnership(PhotonNetwork.LocalPlayer);
                Debug.Log($"�� ������ ���� {PhotonNetwork.LocalPlayer.ActorNumber} ����. ����");
            }

            ptManager.IncrementButtonCount(lastPlayerActorNum);
            
        }
        else
        {
            Debug.LogError("lastPlayerActorNum�� ��ȿ���� �ʰų� ptManager���� �ش� �÷��̾ ã�� �� �����ϴ�.");
        }
    }

    public void ResetDuckColor()
    {
        ptview.RPC("RPC_ChangeDuckColor",RpcTarget.All,Color.yellow);
        ptManager.DecrementButtonCount(lastPlayerActorNum);
    }


    [PunRPC]
    private void RPC_ChangeDuckColor(Color newColor)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if(renderer !=null)
        {
            renderer.material.color = newColor;
        }
    }

    public int GetLastPlayerActorNum()
    {
        return lastPlayerActorNum;
    }
}
