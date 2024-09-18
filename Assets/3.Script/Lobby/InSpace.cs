using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InSpace : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(10f, 20f, 15f); // 오브젝트 자체의 회전 속도
    public float orbitSpeed = 20f; // 중심 오브젝트 주위를 도는 속도 (각도, degrees per second)
    public Transform centerObject; // 중심이 될 오브젝트 (인스펙터에서 참조)

    void Update()
    {
        // 오브젝트 자체의 회전 (자전)
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // 중심 오브젝트 주변을 회전 (공전)
        if (centerObject != null)
        {
            // 중심 오브젝트 주위를 Z축 기준으로 공전
            transform.RotateAround(centerObject.position, Vector3.forward, orbitSpeed * Time.deltaTime);
        }
    }
}
