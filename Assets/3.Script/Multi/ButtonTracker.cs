using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ButtonTracker : MonoBehaviour
{
    private Button_Script bscript;
    private PhotonManager PTmanager;
    private M_Duckschanger MDchanger;
    private DuckInteraction dinteraction;

    public Image linkedUISprite;
    private Color buttonOwnerColor = Color.gray;

    private bool isTracked = false;

    void Start()
    {
        bscript = GetComponent<Button_Script>();

        if (bscript == null)
        {
            Debug.LogError("Button_Script를 찾을 수 없습니다.");
        }

        PTmanager = FindObjectOfType<PhotonManager>();
        if (PTmanager == null)
        {
            Debug.LogError("PhotonManager를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if (bscript.isPushed && !isTracked)
        {
            linkedUISprite.color = buttonOwnerColor;

            isTracked = true;
        }
    }

    public void SetPlayerColor(Color color)
    {
        buttonOwnerColor = color;
    }

    public bool IsButtonPressedByPlayer()
    {
        return isTracked;
    }

    // 버튼이 눌렸을 때 호출되는 메서드
    public void OnButtonPushed(Collision other)
    {
        int buttonIndex = GetButtonIndex();
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;

        if (PTmanager != null)
        {
            UpdateButtonState(buttonIndex, true, actorNum);
        }
        else
        {
            Debug.LogError("PhotonManager가 존재하지 않습니다.");
        }

        UpdateButtonState(buttonIndex, true, actorNum);
    }

    // 버튼이 해제됐을 때 호출되는 메서드
    public void OnButtonReleased(Collision other)
    {
        int buttonIndex = GetButtonIndex();
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;

        if (PTmanager != null)
        {
            UpdateButtonState(buttonIndex, false, actorNum);
        }
        else
        {
            Debug.LogError("PhotonManager가 존재하지 않습니다.");
        }

        UpdateButtonState(buttonIndex, false, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private int GetButtonIndex()
    {
        Button_Script[] buttons = FindObjectsOfType<Button_Script>();
        return System.Array.IndexOf(buttons, bscript);
    }

    private void UpdateButtonState(int buttonIndex, bool isPressed, int actorNum)
    {
        if (PTmanager != null)
        {
            int pfIndex = PTmanager.playerPrefabIndexes[actorNum];
            Color pcolor = PTmanager.GetColorByPrefabIndex(pfIndex);

            ExitGames.Client.Photon.Hashtable buttonState = new ExitGames.Client.Photon.Hashtable();
            buttonState[$"Button{buttonIndex}State"] = isPressed;
            buttonState[$"Button{buttonIndex}Actor"] = actorNum;
            PhotonNetwork.CurrentRoom.SetCustomProperties(buttonState);

            PTmanager.photonView.RPC("RPC_UpdateCanvasImage", RpcTarget.All, buttonIndex, isPressed, pcolor, actorNum);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ducks"))
        {
            dinteraction = other.GetComponent<DuckInteraction>();

            if(dinteraction !=null)
            {
                dinteraction.HandleDuckOnButton();
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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ducks"))
        {
            Debug.Log("오리 ㅃㅇ");
            dinteraction = other.GetComponent<DuckInteraction>();

            if (dinteraction != null)
            {
                dinteraction.ResetDuckColor();
            }
            linkedUISprite.color = Color.gray;
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
