using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
���� ������Ʈ�� �����ϴ� �������� ǥ�� 
Item���κ��� �̹����� �����ϰ� �װ� ���ӳ����� ���̰� ��.
*/

public class ObjectItem : MonoBehaviour
{
    [Header("������")]
    public Item item;
    [Header("������ �̹���")]
    public SpriteRenderer itemImage;

    void Start()
    {
        itemImage.sprite = item.itemImage;  
    }

}
