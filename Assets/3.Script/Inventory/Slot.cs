using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    /*
     �� Ŭ������ �κ��丮�� ������ ��Ÿ����, 
    �� ���Կ� �������� ��ġ�ϰ� �������� �̹����� �����ϴ� ������ �մϴ�.

     */

    [SerializeField] Image image; //���� ui�� ǥ���� �̹���

    private Item _item; //���Կ� ��ġ�� ������
    public Item item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item != null) //�������� ������ ��� �̹��� ǥ��
            {
                image.sprite = item.itemImage; 
                image.color = new Color(1, 1, 1, 1);
            }
            else //�������� ������ �̹��� ǥ��
            {
                image.color = new Color(1, 1, 1, 0);
            }
        }
    }
}
