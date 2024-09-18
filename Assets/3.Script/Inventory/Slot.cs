using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    /*
     이 클래스는 인벤토리의 슬롯을 나타내며, 
    각 슬롯에 아이템을 배치하고 아이템의 이미지를 관리하는 역할을 합니다.

     */

    [SerializeField] Image image; //슬롯 ui에 표시할 이미지

    private Item _item; //슬롯에 배치될 아이템
    public Item item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item != null) //아이템이 존재할 경우 이미지 표시
            {
                image.sprite = item.itemImage; 
                image.color = new Color(1, 1, 1, 1);
            }
            else //아이템이 없을때 이미지 표시
            {
                image.color = new Color(1, 1, 1, 0);
            }
        }
    }
}
