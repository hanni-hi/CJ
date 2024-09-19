using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FangBaby : MonoBehaviour
{
    public GameObject cube;
    public Transform Player;
    private NavMeshAgent agent;
    private Animator ani;

    public float checkRadius = 1.0f;

    void Start()
    {
        ani = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if(Player !=null)
        {
            NavMeshHit hit;
            if(NavMesh.SamplePosition(Player.position,out hit, checkRadius,NavMesh.AllAreas))
            {
                agent.SetDestination(Player.position);
            }
            else
            {
                agent.ResetPath();
            }
        }

    }

    public void OnPlayerDetected()
    {
            agent.isStopped = true;
            ani.SetBool("Attack",true);
    }

    public void OnPlayerLost()
    {
            agent.isStopped = false;
            ani.SetBool("Attack",false);
    }
}
