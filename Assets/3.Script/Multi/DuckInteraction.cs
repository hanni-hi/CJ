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
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if(playerView !=null&&playerView.Owner !=null)
            {
                lastPlayerActorNum = playerView.Owner.ActorNumber;
                Debug.Log($"오리가 플레이어 {lastPlayerActorNum}와 충돌했습니다.");
            }

        }
    }

    public void HandleDuckOnButton()
    {
        if (lastPlayerActorNum != -1 && ptManager.playerPrefabIndexes.ContainsKey(lastPlayerActorNum))
        {
            int playerPrefabIndex = ptManager.playerPrefabIndexes[lastPlayerActorNum];
            Color playerColor = ptManager.GetColorByPrefabIndex(playerPrefabIndex);

            ptview.RPC("RPC_ChangeDuckColor",RpcTarget.All,playerColor.r,playerColor.g,playerColor.b);
        }
    }

    public void ResetDuckColor()
    {
        ptview.RPC("RPC_ChangeDuckColor",RpcTarget.All,Color.gray.r, Color.gray.g, Color.gray.b);
    }


    [PunRPC]
    private void RPC_ChangeDuckColor(float r, float g, float b)
    {
        Color newColor = new Color(r,g,b);
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if(renderer !=null)
        {
            renderer.material.color = newColor;
        }
    }
}
