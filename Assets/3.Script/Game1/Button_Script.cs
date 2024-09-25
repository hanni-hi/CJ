using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;


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
    private PhotonManager PTmanager;

    private ExitGames.Client.Photon.Hashtable customProperties;

    private void Start()
    {
        isPushed = false;
        if (buttonTop == null)
        {
            Debug.LogError("buttonTop is not assigned in the inspector.");
            return;
        }

        if (connectedDoor == null)
        {
            Debug.LogWarning("connectedDoor is not assigned, but proceeding without it.");
        }
        initialButtonPosition = buttonTop.localPosition;
        if (connectedDoor != null)
        {
            initialDoorPosition = connectedDoor.transform.position;
        }
        initialDoorPosition = connectedDoor.transform.position;

        buttonManager = FindObjectOfType<ButtonManager>();
        if (buttonManager == null)
        {
            Debug.LogError("ButtonManager를 찾을 수 없습니다. ButtonManager가 씬에 있어야 합니다.");
        }

        customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
    }

    private void Update()
    {
        if (isPushed)
        {
            if (connectedDoor != null)
            {
                //문을 윗쪽으로
                Vector3 doorTargetPosition = initialDoorPosition + new Vector3(0, upDistance, 0);
            connectedDoor.transform.position = Vector3.MoveTowards(connectedDoor.transform.position, doorTargetPosition, moveSpeed * Time.deltaTime);
            }

            //버튼을 아래로 
            Vector3 buttonTargetPosition = initialButtonPosition + new Vector3(0, -downDistance, 0);
            buttonTop.localPosition = Vector3.MoveTowards(buttonTop.localPosition, buttonTargetPosition, moveSpeed * Time.deltaTime);

        }
        else
        {
            if (connectedDoor != null)
            {
                // 문과 버튼을 원래 위치로 천천히 복원
                connectedDoor.transform.position = Vector3.MoveTowards(connectedDoor.transform.position, initialDoorPosition, moveSpeed * Time.deltaTime);
            }
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
                int buttonIndex = GetButtonIndex();
                UpdateButtonState(buttonIndex,true);

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
                int buttonIndex = GetButtonIndex();
                UpdateButtonState(buttonIndex,false);
            }
        }
    }

   // private void UpdateButtonCount(int buttonIndex,bool isPressed)
   // {
   //     if(PhotonNetwork.IsMasterClient)
   //     {
   //         ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
   //         //int currentCount = customProperties.ContainsKey("ButtonPressCount") ? (int)customProperties["ButtonPressCount"] : 0;
   //         properties[$"Button{buttonIndex}State"] = isPressed;
   //         PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
   //     }
   // }
    private int GetButtonIndex()
    {
        Button_Script[] buttons = FindObjectsOfType<Button_Script>();
            return System.Array.IndexOf(buttons,this);
    }

    private void UpdateButtonState(int buttonIndex,bool isPressed)
    {
        if(PTmanager !=null)
        {
            Color pcolor = PTmanager.GetColorByPrefabIndex(PhotonNetwork.LocalPlayer.ActorNumber-1);
            PTmanager.photonView.RPC("RPC_UpdateCanvasImage",RpcTarget.All,buttonIndex,isPressed,pcolor);
       
        }
    }
}






