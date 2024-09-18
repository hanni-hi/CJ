using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
�κ��丮 �ý���, 
������ �߰�, ���Կ� ��ġ

*/

public class Inventory : MonoBehaviour
{
    public List<Item> items;

    [SerializeField]
    private Transform slotParent; //Bag ���� �� 
    [SerializeField]
    private Slot[] slots;

#if UNITY_EDITOR
    private void OnValidate()
    {
        slots = slotParent.GetComponentsInChildren<Slot>(); //���Ե� ��ũ��Ʈ�� �Ҵ�����
    }
#endif
    void Awake()
    {
        FreshSlot();  
    }

    //�������� �����ų� ������ slot �ٽ� �����ؼ� ȭ�鿡 ������
    public void FreshSlot()
    {
        int i = 0; //i�� �ٱ��� �����ؼ� �ΰ��� �������� ������ i�� �����
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
            Debug.Log("�κ��� ��� á���~");
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
