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

        // connectedD가 null이 아닌 경우에만 초기 위치를 설정
        if (connectedD != null)
        {
            initialDPosition = connectedD.transform.position;
        }

    }

    void Update()
    {
        if(isTracked)
        {
            if (connectedD != null)  // connectedD가 있는 경우에만 오리 움직임 처리
            {
                Vector3 DtargetPosition = initialDPosition + new Vector3(0, upDistance, 0);
                connectedD.transform.position = Vector3.MoveTowards(connectedD.transform.position, DtargetPosition, moveSpeed * Time.deltaTime);

                Vector3 BtargetPosition = initialBPosition + new Vector3(0, -downDistance, 0);
                buttontop.localPosition = Vector3.MoveTowards(buttontop.localPosition, BtargetPosition, moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (connectedD != null)  // connectedD가 있는 경우에만 오리 움직임 처리
            {
                connectedD.transform.position = Vector3.MoveTowards(connectedD.transform.position, initialDPosition, moveSpeed * Time.deltaTime);
                buttontop.localPosition = Vector3.MoveTowards(buttontop.localPosition, initialBPosition, moveSpeed * Time.deltaTime);

                linkedUISprite.color = Color.white;
            }
            }
    }

   // public void SetPlayerColor(Color PColor1, Color PColor2)
   // {
   //     BOwnColor1 = PColor1;
   //     BOwnColor2 = PColor2;
   //
   //    // buttonOwnerColor = color;
   // }

    public bool IsButtonPressedByPlayer()
    {
        return isTracked;
    }

    private int GetButtonIndex()
    {
        ButtonTracker[] buttons = FindObjectsOfType<ButtonTracker>();
        return System.Array.IndexOf(buttons, this);
    }

    private void UpdateButtonState(bool isPressed, int actorNum)
    {
        if (PTmanager != null)
        {
            int buttonIndex = GetButtonIndex();
            Color pcolor = PTmanager.GetColorByPrefabIndex(PTmanager.playerPrefabIndexes[actorNum]);

             Debug.Log($"버튼 상태 업데이트 시도: Index {buttonIndex}, Pressed: {isPressed}, ActorNum: {actorNum}, Color: {pcolor}");

            PTmanager.photonView.RPC("RPC_UpdateCanvasImage", RpcTarget.All, buttonIndex, isPressed, actorNum,pcolor);
        }
        else
        {
            Debug.LogError("PhotonManager가 존재하지 않습니다.");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("버튼과 충돌 시도");
        if (other.gameObject.CompareTag("Ducks")&&!isTracked)
        {
            isTracked = true;
            dinteraction = other.gameObject.GetComponent<DuckInteraction>();

            if(dinteraction !=null)
            {
                dinteraction.HandleDuckOnButton();
                // OnButtonPushed(other);

                UpdateButtonState(true,PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ducks")&&isTracked)
        {
            isTracked = false;
            Debug.Log("오리 ㅃㅇ");
            dinteraction = other.gameObject.GetComponent<DuckInteraction>();

            if (dinteraction != null)
            {
                dinteraction.ResetDuckColor();
               // OnButtonReleased(null);
            }
            // linkedUISprite.color = Color.white;

            UpdateButtonState(false,PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

  //  public void PressButtonWithActorNum(int actorNum)
  //  {
  //      isTracked = true;
  //      UpdateButtonState(true,actorNum);
  //  }
  //
  //  public void ReleaseButtonWithActorNum(int actorNum)
  //  {
  //      isTracked = false;
  //      UpdateButtonState(false,actorNum);
  //  }
}
