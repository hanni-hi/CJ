using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ButtonTracker : MonoBehaviour
{
    private PhotonManager PTmanager;
    private M_Duckschanger MDchanger;
    private DuckInteraction dinteraction;

    public GameObject connectedD;
    public Transform buttontop;
    public float moveSpeed = 3f;
    public float downDistance = 0.3f;
    public float upDistance = 3.5f;

    private Vector3 initialBPosition;
    private Vector3 initialDPosition;


    public Image linkedUISprite;
    private Color buttonOwnerColor = Color.white;

    private Color BOwnColor1= Color.white;
    private Color BOwnColor2= Color.white;
    private int buttonIndex;

    private bool isTracked = false;

    void Start()
    {
        PTmanager = FindObjectOfType<PhotonManager>();
        if (PTmanager == null)
        {
            Debug.LogError("PhotonManager를 찾을 수 없습니다.");
        }

        initialBPosition = buttontop.localPosition;
        initialDPosition = connectedD.transform.position;

       // buttonIndex = GetButtonIndex();

    }

    void Update()
    {
       // if (!isTracked)
       // {
       //     linkedUISprite.color = buttonOwnerColor;
       //
       //     isTracked = true;
       // }

        if(isTracked)
        {
            Vector3 DtargetPosition = initialDPosition + new Vector3(0, upDistance, 0);
            connectedD.transform.position = Vector3.MoveTowards(connectedD.transform.position, DtargetPosition, moveSpeed * Time.deltaTime);

            Vector3 BtargetPosition = initialBPosition + new Vector3(0, -downDistance, 0);
            buttontop.localPosition = Vector3.MoveTowards(buttontop.localPosition, BtargetPosition, moveSpeed * Time.deltaTime);

        }
        else
        {
            connectedD.transform.position = Vector3.MoveTowards(connectedD.transform.position,initialDPosition,moveSpeed*Time.deltaTime);
            buttontop.localPosition = Vector3.MoveTowards(buttontop.localPosition, initialBPosition, moveSpeed * Time.deltaTime);

            linkedUISprite.color = Color.white;
        }
    }

    public void SetPlayerColor(Color PColor1, Color PColor2)
    {
        BOwnColor1 = PColor1;
        BOwnColor2 = PColor2;

       // buttonOwnerColor = color;
    }

    public bool IsButtonPressedByPlayer()
    {
        return isTracked;
    }

    // 버튼이 눌렸을 때 호출되는 메서드
   // public void OnButtonPushed(Collider other)
   // {
   //     int buttonIndex = GetButtonIndex();
   //     int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
   //
   //     if (PTmanager != null)
   //     {
   //         UpdateButtonState(true, actorNum);
   //     }
   //     else
   //     {
   //         Debug.LogError("PhotonManager가 존재하지 않습니다.");
   //     }
   //
   //     UpdateButtonState(true, actorNum);
   // }
   //
   // // 버튼이 해제됐을 때 호출되는 메서드
   // public void OnButtonReleased(Collision other)
   // {
   //     int buttonIndex = GetButtonIndex();
   //     int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
   //
   //     if (PTmanager != null)
   //     {
   //         UpdateButtonState(false, actorNum);
   //     }
   //     else
   //     {
   //         Debug.LogError("PhotonManager가 존재하지 않습니다.");
   //     }
   //
   //     UpdateButtonState(false, PhotonNetwork.LocalPlayer.ActorNumber);
   // }

    private int GetButtonIndex()
    {
        Button_Script[] buttons = FindObjectsOfType<Button_Script>();
        return System.Array.IndexOf(buttons, this);
    }

    private void UpdateButtonState(bool isPressed, int actorNum)
    {
        if (PTmanager != null)
        {
          //  int pfIndex = PTmanager.playerPrefabIndexes[actorNum];
             Color pcolor = PTmanager.GetColorByPrefabIndex(PTmanager.playerPrefabIndexes[actorNum]);


          //
             Debug.Log($"버튼 상태 업데이트 시도: Index {buttonIndex}, Pressed: {isPressed}, ActorNum: {actorNum}, Color: {pcolor}");
          //
          //  ExitGames.Client.Photon.Hashtable buttonState = new ExitGames.Client.Photon.Hashtable();
          //  buttonState[$"Button{buttonIndex}State"] = isPressed;
          //  buttonState[$"Button{buttonIndex}Actor"] = actorNum;
          //  PhotonNetwork.CurrentRoom.SetCustomProperties(buttonState);

            PTmanager.photonView.RPC("RPC_UpdateCanvasImage", RpcTarget.All, GetButtonIndex(), isPressed, actorNum,pcolor);
        }

        else
        {
            Debug.LogError("PhotonManager가 존재하지 않습니다.");
        }
    }

    private void OnCollisionEnter(Collider other)
    {
        Debug.Log("버튼과 충돌 시도");
        if (other.CompareTag("Ducks")&&!isTracked)
        {
            isTracked = true;
            dinteraction = other.GetComponent<DuckInteraction>();

            if(dinteraction !=null)
            {
                dinteraction.HandleDuckOnButton();
                // OnButtonPushed(other);

                UpdateButtonState(true,PhotonNetwork.LocalPlayer.ActorNumber);
            }

           // PhotonView dview = other.GetComponent<PhotonView>();
           // if (dview != null && dview.Owner != null)
           // {
           //     int actNum = dview.Owner.ActorNumber;
           //     Color pColor = PTmanager.GetColorByPrefabIndex(PTmanager.playerPrefabIndexes[actNum]);
           //
           //     linkedUISprite.color = pColor;
           //     DuckCChange(dview.gameObject, pColor);
           // }
        }
    }

    private void OnCollisionExit(Collider other)
    {
        if (other.CompareTag("Ducks")&&isTracked)
        {
            isTracked = false;
            Debug.Log("오리 ㅃㅇ");
            dinteraction = other.GetComponent<DuckInteraction>();

            if (dinteraction != null)
            {
                dinteraction.ResetDuckColor();
               // OnButtonReleased(null);
            }
            // linkedUISprite.color = Color.white;

            UpdateButtonState(false,PhotonNetwork.LocalPlayer.ActorNumber);
        }

       // PhotonView dview = other.GetComponent<PhotonView>();
       // if (dview != null)
       // {
       //     DuckCChange(dview.gameObject, Color.gray);
       // }
    }

   // private void DuckCChange(GameObject dobj,Color color)
   //     {
   //     PhotonView dptview = dobj.GetComponent<PhotonView>();
   //     if(dptview !=null)
   //     {
   //         PTmanager.photonView.RPC("RPC_ChangeDuckColor",RpcTarget.AllBuffered,dptview.ViewID,color.r,color.g,color.b);
   //     }
   //     }
}
