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
        vcamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();

        anim = gameObject.GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        //자신의 캐릭터일 경우 시네머신 카메라를 연결 
    if(pv.IsMine)
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
