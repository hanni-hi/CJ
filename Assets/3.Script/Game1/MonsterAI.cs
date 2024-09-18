using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public Transform player;
    public float chaseRadius = 4f;    // �÷��̾ �Ѿư� �ݰ�
    public float walkSpeed = 2f;
    public float runSpeed = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private BoxCollider col;

    public float rayLength = 5f; //�����ɽ�Ʈ ����
    public int rayCount = 8; //�����ɽ�Ʈ�� �߻��� ����

    public Collider rightHandCollider; //������ �ݶ��̴�
    public GameObject planeObject; //�÷��� ������Ʈ(���Ϳ� �޷�����)

    private Coroutine chaseCoroutine;  // �ڷ�ƾ�� �����ϱ� ���� ����

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider>();
        rightHandCollider.enabled = false;
        WalkTowardsPlayer(); // ���� ���� �� �÷��̾ �ѱ� ����
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= chaseRadius && chaseCoroutine == null)
        {
                Debug.Log("�÷��̾ �ݰ� ���̳�?!");
              chaseCoroutine = StartCoroutine(ChasePlayer());
        }
        else if(distanceToPlayer>chaseRadius&&chaseCoroutine !=null)
        {
                Debug.Log("�÷��̾ �ݰ� ���̳�?!");
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
        Debug.Log("�� CHASE �̾�");
        animator.SetInteger("Run", 2);  // �ִϸ��̼� ���� ����
        agent.speed = runSpeed;

        rightHandCollider.enabled = true;

        while (Vector3.Distance(player.position, transform.position) <= chaseRadius)
        {
            agent.SetDestination(player.position);
            yield return null; // �� �����Ӹ��� ������Ʈ
        }

        // �÷��̾ �ݰ��� ����� �ȱ� ���·� ��ȯ
        rightHandCollider.enabled = false;
        WalkTowardsPlayer();
        chaseCoroutine = null;
    }


    void WalkTowardsPlayer()
    {
        Debug.Log("�� WALK �̾�");
        animator.SetInteger("Walk", 1);  // Walk �ִϸ��̼� ���·� ��ȯ
        agent.speed = walkSpeed;

        agent.SetDestination(player.position); // �÷��̾ ��ǥ �������� ����
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject==planeObject&& player != null && player.CompareTag("Player")) 
        {
                Debug.Log("���� ��Ʈ���ſ��� ��");
                FacePlayer(); // �÷��̾� �������� ���� ȸ��
                AttackPlayer(); // ���� �ִϸ��̼� ����
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
        Debug.Log("���þִϸ��̼� Ʈ���� ��");
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
    //      //������Ʈ ����
    //      float height = GetComponent<Collider>().bounds.size.y;
    //
    //      //�����ɽ�Ʈ�� �߻��� ���� ��ġ
    //      Vector3 rayStartPosition = transform.position + Vector3.up * height;
    //
    //      bool doorDetected = false;
    //      bool rayPassedThrough = false;
    //
    //      //������������ ���������� �Ʒ��� �̵���
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
