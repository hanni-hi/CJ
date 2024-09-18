using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InSpace : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(10f, 20f, 15f); // ������Ʈ ��ü�� ȸ�� �ӵ�
    public float orbitSpeed = 20f; // �߽� ������Ʈ ������ ���� �ӵ� (����, degrees per second)
    public Transform centerObject; // �߽��� �� ������Ʈ (�ν����Ϳ��� ����)

    void Update()
    {
        // ������Ʈ ��ü�� ȸ�� (����)
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // �߽� ������Ʈ �ֺ��� ȸ�� (����)
        if (centerObject != null)
        {
            // �߽� ������Ʈ ������ Z�� �������� ����
            transform.RotateAround(centerObject.position, Vector3.forward, orbitSpeed * Time.deltaTime);
        }
    }
}
