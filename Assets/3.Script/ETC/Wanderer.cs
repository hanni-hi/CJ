using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wanderer : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator ani;
    public float walkRadius = 10f;
    public float minIdleTime = 4f;
    public float maxIdelTime = 8f;
    public float minWalkTime = 3f;
    public float maxWalkTime = 6f;

    private float idleTimer;
    private float walkTimer;
    private bool isWalking = false;


       void Start()
    {
        idleTimer = Random.Range(minIdleTime,maxIdelTime);
    }

       void Update()
    {
        if(isWalking)
        {
            walkTimer -= Time.deltaTime;

            if(walkTimer<=0f||agent.remainingDistance<=agent.stoppingDistance)
            {
                isWalking = false;
                idleTimer = Random.Range(minIdleTime,maxIdelTime);
                agent.ResetPath();
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if(idleTimer<=0f)
            {
                //°È±â ½ÃÀÛ
                isWalking = true;
                walkTimer = Random.Range(minWalkTime, maxWalkTime);
                agent.SetDestination(RandomNavMeshLocation());
            }
        }
        ani.SetFloat("Speed",agent.velocity.magnitude);
    }

    Vector3 RandomNavMeshLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomDirection,out hit,walkRadius,1))
        {
            return hit.position;
        }
        return transform.position;
    }

}
