using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Playables;

public class Player : MonoBehaviour
{
	[Header("인벤토리")]
	public Inventory inventory;

	private Animator anim;
	private CharacterController controller;

	public float speed = 600.0f;
	public float turnSpeed = 400.0f;
	private Vector3 moveDirection = Vector3.zero;
	public float gravity = 20.0f;
	public float pushForce = 20f;
	public float ObjpushForce = 2.0f;

	public GameObject pushCubePrefab;
	public Image skillImage;
	public float cubeLifeTime = 1f;

	private bool canUseSkill = true;
	private bool isInvincible = false;
	public float invincibleDuration = 2f;
	public float blinkInterval = 0.1f;

	private Renderer playerRenderer;
	private Color originalColor;
	private Color originalImageColor;
	public Color skillUnavailableColor = Color.gray;

	// 타임라인 추가
	public PlayableDirector timeline;
	private bool isTimelinePlaying = false;  // 타임라인 재생 여부 확인

	void Start()
	{
		controller = GetComponent<CharacterController>();
		anim = gameObject.GetComponentInChildren<Animator>();
		playerRenderer = GetComponentInChildren<Renderer>(); // 캐릭터의 자식 Renderer 컴포넌트 가져오기
		originalColor = playerRenderer.material.color; // 원래 색상 저장

        // UI 이미지 색상 설정
		if (skillImage != null)
		{
			originalImageColor = skillImage.color; // UI 이미지의 원래 색상 저장
		}

		if(timeline !=null)
        {
			timeline.played += OntimelineStart;
			timeline.stopped += OntimelineEnd;
        }
	}

	void Update()
	{
		if(isTimelinePlaying)
        {
			anim.SetInteger("AnimationPar", 0);
			return; //입력처리무시
		}

		if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow) || Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
		{
			anim.SetInteger("AnimationPar", 1);
		}
		else
		{
			anim.SetInteger("AnimationPar", 0);
		}
		if (Input.GetKeyDown(KeyCode.Space)&& canUseSkill)
		{
			CreatePushCube();
		}

		if (controller.isGrounded)
		{
			moveDirection = transform.forward * Input.GetAxis("Vertical") * speed;
		}

		float turn = Input.GetAxis("Horizontal");
		transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
		controller.Move(moveDirection * Time.deltaTime);
		moveDirection.y -= gravity * Time.deltaTime;

		// UI 이미지 색상 업데이트
		UpdateSkillImageColor();

	}

    void FixedUpdate()
    {
     if(Input.GetKeyDown(KeyCode.Alpha1))
        {
			UseItem(0);
        }
	 else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
			UseItem(1);
        }
	 else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
			UseItem(2);
        }
    }

	void UseItem(int slotIndex)
    {
		Item selectedItem = inventory.GetItemInSlot(slotIndex);

		if(selectedItem==null)
        {
			return;
        }

		if(selectedItem.itemName=="Heart")
        {
if(UIManager.instance.HasEmptyHeart())
            {
				UIManager.instance.FillEmptyHeart();
				inventory.RemoveItem(slotIndex);
				inventory.FreshSlot();
            }
        }
    }

    void CreatePushCube()
	{
		canUseSkill = false;

		Vector3 spawnPosition = transform.position + transform.forward * 1.5f;
		GameObject pushcube = Instantiate(pushCubePrefab, spawnPosition, transform.rotation);

		Rigidbody rb = pushcube.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.AddForce(transform.forward * pushForce, ForceMode.Impulse);
		}

		//Destroy(pushcube, cubeLifeTime);

		// 일정 시간 후 큐브 삭제 및 스킬 재사용 가능 설정
		StartCoroutine(DestroyCubeAfterTime(pushcube));
	}

	IEnumerator DestroyCubeAfterTime(GameObject cube)
    {
		yield return new WaitForSeconds(cubeLifeTime);
		Destroy(cube);
		canUseSkill = true;
    }

	void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.CompareTag("Monster")&&!isInvincible)
        {
			GameManager.instance.TakeDamage(1);
			StartCoroutine(BecomeInvincible());
		}

		Rigidbody body = hit.collider.attachedRigidbody;

		if (body != null && !body.isKinematic)
		{
			Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
			body.AddForce(pushDirection * ObjpushForce, ForceMode.Impulse);
		}
	}

	IEnumerator BecomeInvincible()
    {
		isInvincible = true;

		float elapsed = 0f;
		while(elapsed<invincibleDuration)
        {
			playerRenderer.material.color = Color.red;
			yield return new WaitForSeconds(blinkInterval);

			playerRenderer.material.color = originalColor;
			yield return new WaitForSeconds(blinkInterval);

			elapsed += blinkInterval * 2;
        }

		playerRenderer.material.color = originalColor;
		isInvincible = false;
    }

	void UpdateSkillImageColor()
    {
		if(skillImage !=null)
        {
			if(canUseSkill)
            {
				skillImage.color = originalImageColor;
            }
			else
            {
				skillImage.color = skillUnavailableColor;
            }
        }
    }

	void OntimelineStart(PlayableDirector pd)
    {
		isTimelinePlaying = true;
    }

		void OntimelineEnd(PlayableDirector pd)
    {
		isTimelinePlaying = false;
    }

    private void OnTriggerEnter(Collider other)
    {
		Debug.Log($"충돌함 + {other.name}");

        if(other.CompareTag("Item"))
        {
			ObjectItem objItem = other.GetComponent<ObjectItem>();
			if(objItem!=null && inventory !=null)
            {
				inventory.AddItem(objItem.item);
				Destroy(other.gameObject);
            }
        }
    }
}
