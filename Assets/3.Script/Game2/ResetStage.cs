using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ResetStage : MonoBehaviour
{
    public Stage stageScript;
    public PlayableDirector timeline;
    public Inventory inven;

    public string requiredItemName = "Key";
    public int requiredKeyCount = 2;
    public GameObject cube;
   // private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
           // hasTriggered = true;
            if(stageScript!=null)
            {
                stageScript.ResetSprite();
                stageScript.CheckAndDestroyCube(cube);
            }
            if(CountItemsInInventory(requiredItemName)>=requiredKeyCount)
            {
                if(timeline !=null)
                {
                    timeline.Play();
                    timeline.stopped += OnTimelineEnd;
                }
            }
        }
    }
    private int CountItemsInInventory(string itemName)
    {
        int count = 0;
        foreach(Item item in inven.items)
        {
            if(item.itemName==itemName)
            {
                count++;
            }
        }
        return count;
    }

    private void OnTimelineEnd(PlayableDirector director)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowVictoryUI();
        }
        else
        {
            Debug.LogError("UIManager instance가 null입니다.");
        }
    }
}
