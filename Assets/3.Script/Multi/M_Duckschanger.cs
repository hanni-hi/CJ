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

    public int lastPlayerActorNum = -1;// �浹�� ������ �÷��̾��� num ���� 

    private void Start()
    {
        ptManager = FindObjectOfType<PhotonManager>();
        ptView = GetComponent<PhotonView>();
    }
}
