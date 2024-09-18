using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Button_Script : MonoBehaviour
{
    //  public Action  onButtonPressed;
    //  public Action onButtonReleased;

    public bool isPushed = false;
    public float moveSpeed = 3f;
    public float downDistance = 0.3f;
    public float upDistance = 3.5f;

    public GameObject connectedDoor;
    public Transform buttonTop;

    private Vector3 initialButtonPosition;
    private Vector3 initialDoorPosition;

    private ButtonManager buttonManager;

    private void Start()
    {
        isPushed = false;
        initialButtonPosition = buttonTop.localPosition;
        initialDoorPosition = connectedDoor.transform.position;

        buttonManager = FindObjectOfType<ButtonManager>();
        if (buttonManager == null)
        {
            Debug.LogError("ButtonManager�� ã�� �� �����ϴ�. ButtonManager�� ���� �־�� �մϴ�.");
        }
    }

    private void Update()
    {
        if (isPushed)
        {

            //���� ��������
            Vector3 doorTargetPosition = initialDoorPosition + new Vector3(0, upDistance, 0);
            connectedDoor.transform.position = Vector3.MoveTowards(connectedDoor.transform.position, doorTargetPosition, moveSpeed * Time.deltaTime);

            //��ư�� �Ʒ��� 
            Vector3 buttonTargetPosition = initialButtonPosition + new Vector3(0, -downDistance, 0);
            buttonTop.localPosition = Vector3.MoveTowards(buttonTop.localPosition, buttonTargetPosition, moveSpeed * Time.deltaTime);

        }
        else
        {
            // ���� ��ư�� ���� ��ġ�� õõ�� ����
            connectedDoor.transform.position = Vector3.MoveTowards(connectedDoor.transform.position, initialDoorPosition, moveSpeed * Time.deltaTime);
            buttonTop.localPosition = Vector3.MoveTowards(buttonTop.localPosition, initialButtonPosition, moveSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if ((other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Ducks")) && gameObject.CompareTag("Button"))
        {
            if (!isPushed)
            {
                isPushed = true;
                // onButtonPressed?.Invoke();

                buttonManager.OnButtonPressed();
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if ((other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Ducks")) && gameObject.CompareTag("Button"))
        {
            if (isPushed)
            {
                isPushed = false;
                buttonManager.OnButtonReleased();
                // onButtonReleased?.Invoke();
            }
        }
    }
}






