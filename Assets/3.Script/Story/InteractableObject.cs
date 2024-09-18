using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public MemoData memoData;

    public void Interact()
    {
        Debug.Log("�̰� Interact��");

        MemoUIManager.instance.ShowMemo(memoData);
    }
}
