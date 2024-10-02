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
        if (lastPlayerActorNum != -1 && ptManager.playerPrefabIndexes.ContainsKey(lastPlayerActorNum))
        {
            int playerPrefabIndex = ptManager.playerPrefabIndexes[lastPlayerActorNum];
            Color playerColor = ptManager.GetColorByPrefabIndex(playerPrefabIndex);

            Debug.Log($"���� ���� ���� �õ�: {playerColor}");

            ptview.RPC("RPC_ChangeDuckColor",RpcTarget.All,playerColor);
            
        }
        else
        {
            Debug.LogError("lastPlayerActorNum�� ��ȿ���� �ʰų� ptManager���� �ش� �÷��̾ ã�� �� �����ϴ�.");
        }
    }

    public void ResetDuckColor()
    {
        ptview.RPC("RPC_ChangeDuckColor",RpcTarget.All,Color.yellow);
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
}
