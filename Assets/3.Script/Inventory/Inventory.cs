using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
인벤토리 시스템, 
아이템 추가, 슬롯에 배치

*/

public class Inventory : MonoBehaviour
{
    public List<Item> items;

    [SerializeField]
    private Transform slotParent; //Bag 담을 곳 
    [SerializeField]
    private Slot[] slots;

#if UNITY_EDITOR
    private void OnValidate()
    {
        slots = slotParent.GetComponentsInChildren<Slot>(); //슬롯들 스크립트에 할당해줌
    }
#endif
    void Awake()
    {
        FreshSlot();  
    }

    //아이템이 들어오거나 나가면 slot 다시 정리해서 화면에 보여줌
    public void FreshSlot()
    {
        int i = 0; //i를 바깥에 선언해서 두개의 포문에서 동일한 i를 사용함
        for(;i<items.Count&&i<slots.Length;i++)
        {
            slots[i].item = items[i];
        }
        for(;i<slots.Length;i++)
        {
            slots[i].item = null;
        }
    }

    public void AddItem(Item _item)
    {
        if(items.Count<slots.Length)
        {
            items.Add(_item);
            FreshSlot();
        }
        else
        {
            Debug.Log("인벤이 모두 찼어욤~");
        }
    }

    public Item GetItemInSlot(int slotIndex)
    {
        if(slotIndex<items.Count)
        {
            return items[slotIndex];
        }
        return null;
    }

    public void RemoveItem(int slotIndex)
    {
        if(slotIndex<items.Count)
        {
            items.RemoveAt(slotIndex);
        }
    }
}
