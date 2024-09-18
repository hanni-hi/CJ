using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
게임 오브젝트로 존재하는 아이템을 표현 
Item으로부터 이미지를 설정하고 그게 게임내에서 보이게 함.
*/

public class ObjectItem : MonoBehaviour
{
    [Header("아이템")]
    public Item item;
    [Header("아이템 이미지")]
    public SpriteRenderer itemImage;

    void Start()
    {
        itemImage.sprite = item.itemImage;  
    }

}
