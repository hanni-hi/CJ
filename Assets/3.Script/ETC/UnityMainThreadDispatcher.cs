using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQ = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if(!instance)
        {
            instance = FindObjectOfType<UnityMainThreadDispatcher>();
            if(!instance)
            {
                var obj = new GameObject("UnityMainThreadDispatcher");
                instance = obj.AddComponent<UnityMainThreadDispatcher>();
            }
        }
        return instance;
    }
    public static UnityMainThreadDispatcher instance = null;


    void Update()
    {
        lock(executionQ)
        {
            while(executionQ.Count>0)
            {
                executionQ.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(IEnumerator action)
    {
        lock(executionQ)
        {
            executionQ.Enqueue(() => { StartCoroutine(action); });
        }
    }

    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    IEnumerator ActionWrapper(Action action)
    {
        action();
        yield return null;
    }
}
