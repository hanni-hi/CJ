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
                Debug.Log($"오리가 플레이어 {lastPlayerActorNum}와 충돌했습니다.");
            }
        }
    }

    public void HandleDuckOnButton()
    {
        Debug.Log($"HandleDuckOnButton 호출됨 - lastPlayerActorNum: {lastPlayerActorNum}");
        if (lastPlayerActorNum != -1 && ptManager.playerPrefabIndexes.ContainsKey(lastPlayerActorNum))
        {
            int playerPrefabIndex = ptManager.playerPrefabIndexes[lastPlayerActorNum];
            Color playerColor = ptManager.GetColorByPrefabIndex(playerPrefabIndex);

            Debug.Log($"오리 색상 변경 시도: {playerColor}");

            ptview.RPC("RPC_ChangeDuckColor",RpcTarget.All,playerColor);

            if(!ptview.IsMine)
            {
                ptview.TransferOwnership(PhotonNetwork.LocalPlayer);
                Debug.Log($"이 오리는 이제 {PhotonNetwork.LocalPlayer.ActorNumber} 거임. ㅅㄱ");
            }

            ptManager.IncrementButtonCount(lastPlayerActorNum);
            
        }
        else
        {
            Debug.LogError("lastPlayerActorNum이 유효하지 않거나 ptManager에서 해당 플레이어를 찾을 수 없습니다.");
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
