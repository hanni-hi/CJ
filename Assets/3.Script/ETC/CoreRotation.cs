using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreRotation : MonoBehaviour
{
    public float rotationSpeed = 150f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);    
    }
}
