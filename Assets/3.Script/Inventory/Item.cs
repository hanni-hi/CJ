using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 아이템 데이터를 정의함
각 아이템은 이름과 이미지를 가짐
 */

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemImage;

}
