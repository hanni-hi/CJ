using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class M_movement : MonoBehaviour
{
    private PhotonView pv;
    private CinemachineVirtualCamera vcamera;
    public AudioListener aulistener;

    private Animator anim;
    private CharacterController controller;

    private Vector3 moveDirection = Vector3.zero;
    public float speed = 6f;
    public float turnSpeed = 100f;
    public float gravity = 2f;
    private bool isTimelinePlaying = false;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        aulistener = GetComponent<AudioListener>();

        if(pv.IsMine)
        {
            aulistener.enabled = true;
        }
        else
        {
            aulistener.enabled = false;
        }

        vcamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
        if (vcamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera is not found in the scene.");
        }

        anim = gameObject.GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController is missing from the GameObject.");
            return; // 할당이 안 된 경우 업데이트를 하지 않도록 반환
        }
        //자신의 캐릭터일 경우 시네머신 카메라를 연결 
        if (pv.IsMine)
        {
            vcamera.Follow = transform;
            vcamera.LookAt = transform;
        }
    }

    void Update()
    {
        if (pv.IsMine)
        {
            Move();
        }
    }

    void Move()
    {
        if (isTimelinePlaying)
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

        if (controller.isGrounded)
        {
            moveDirection = transform.forward * Input.GetAxis("Vertical") * speed;
        }

        float turn = Input.GetAxis("Horizontal");
        transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
        controller.Move(moveDirection * Time.deltaTime);
        moveDirection.y -= gravity * Time.deltaTime;
    }

}
