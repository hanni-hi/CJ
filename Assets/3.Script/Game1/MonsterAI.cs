using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public Transform player;
    public float chaseRadius = 4f;    // 플레이어를 쫓아갈 반경
    public float walkSpeed = 2f;
    public float runSpeed = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private BoxCollider col;

    public float rayLength = 5f; //레이케스트 길이
    public int rayCount = 8; //레이케스트를 발사할 개수

    public Collider rightHandCollider; //오른손 콜라이더
    public GameObject planeObject; //플레인 오브젝트(몬스터에 달려있음)

    private Coroutine chaseCoroutine;  // 코루틴을 제어하기 위한 변수

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider>();
        rightHandCollider.enabled = false;
        WalkTowardsPlayer(); // 게임 시작 시 플레이어를 쫓기 시작
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= chaseRadius && chaseCoroutine == null)
        {
                Debug.Log("플레이어가 반경 안이네?!");
              chaseCoroutine = StartCoroutine(ChasePlayer());
        }
        else if(distanceToPlayer>chaseRadius&&chaseCoroutine !=null)
        {
                Debug.Log("플레이어가 반경 밖이네?!");
                StopCoroutine(chaseCoroutine);
             chaseCoroutine = null;
             WalkTowardsPlayer();
        }

        if (chaseCoroutine == null)
        {
            agent.SetDestination(player.position);
        }
    }

  //  void OnCollisionEnter(Collision coll)
  //  {
  //      if(coll.gameObject.CompareTag("Door"))
  //      {
  //          CastRays();
  //      }
  //  }

    IEnumerator ChasePlayer()
    {
        Debug.Log("난 CHASE 이야");
        animator.SetInteger("Run", 2);  // 애니메이션 상태 변경
        agent.speed = runSpeed;

        rightHandCollider.enabled = true;

        while (Vector3.Distance(player.position, transform.position) <= chaseRadius)
        {
            agent.SetDestination(player.position);
            yield return null; // 매 프레임마다 업데이트
        }

        // 플레이어가 반경을 벗어나면 걷기 상태로 전환
        rightHandCollider.enabled = false;
        WalkTowardsPlayer();
        chaseCoroutine = null;
    }


    void WalkTowardsPlayer()
    {
        Debug.Log("난 WALK 이야");
        animator.SetInteger("Walk", 1);  // Walk 애니메이션 상태로 전환
        agent.speed = walkSpeed;

        agent.SetDestination(player.position); // 플레이어를 목표 지점으로 설정
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject==planeObject&& player != null && player.CompareTag("Player")) 
        {
                Debug.Log("여긴 온트리거엔터 야");
                FacePlayer(); // 플레이어 방향으로 몬스터 회전
                AttackPlayer(); // 공격 애니메이션 실행
            }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime*5f);
    }
        
    void AttackPlayer()
    {
        animator.SetTrigger("Attack");
        Debug.Log("어택애니메이션 트리거 야");
        StartCoroutine(ActivateCollider());
    }

    IEnumerator ActivateCollider()
    {
        yield return new WaitForSeconds(0.1f);

        rightHandCollider.enabled = true;

        yield return new WaitForSeconds(1.5f);

        rightHandCollider.enabled = false;

    }

    //  void CastRays()
    //  {
    //     
    //      //오브젝트 높이
    //      float height = GetComponent<Collider>().bounds.size.y;
    //
    //      //레이케스트를 발사할 시작 위치
    //      Vector3 rayStartPosition = transform.position + Vector3.up * height;
    //
    //      bool doorDetected = false;
    //      bool rayPassedThrough = false;
    //
    //      //시작지점에서 점진적으로 아래로 이동함
    //      for (int i=0;i<rayCount;i++)
    //      {
    //          Vector3 rayOrigin = rayStartPosition - Vector3.up * (i * (height / (rayCount - 1)));
    //
    //          RaycastHit hit;
    //          if(Physics.Raycast(rayOrigin, transform.forward,out hit, rayLength))
    //          {
    //              if(hit.collider.CompareTag("Door"))
    //              {
    //                  doorDetected = true;
    //              }
    //          }
    //          else
    //          {
    //              rayPassedThrough = true;
    //          }
    //      }
    //
    //      if(doorDetected&&rayPassedThrough)
    //      {
    //          StartCoroutine(SmallCollider());
    //          animator.SetInteger("Door", 4);
    //      }
    //  }
    //
    //  IEnumerator SmallCollider()
    //  {
    //      Vector3 originSize = col.size;
    //      col.size = originSize / 4f;
    //
    //      yield return new WaitForSeconds(3f);
    //
    //      col.size = originSize;
    //  }
}
