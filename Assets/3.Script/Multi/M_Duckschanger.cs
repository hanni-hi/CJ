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

    public int lastPlayerActorNum = -1;// 충돌한 마지막 플레이어의 num 저장 

    private void Start()
    {
        ptManager = FindObjectOfType<PhotonManager>();
        ptView = GetComponent<PhotonView>();
    }
}
