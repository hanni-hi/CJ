using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M_Cube : MonoBehaviour
{
    public FangBaby fangbaby;

    void OnTriggerEnter(Collider other)
    {
    if(other.CompareTag("Player"))
        {
            fangbaby.OnPlayerDetected();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            fangbaby.OnPlayerLost();
        }
    }
}
