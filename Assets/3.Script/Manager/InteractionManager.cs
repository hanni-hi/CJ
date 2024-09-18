using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionManager : MonoBehaviour
{
    public float rayDistance = 2f;
    public LayerMask interactableLayer;
    public TextMeshProUGUI interactionText; // 오브젝트 이름을 표시하는 Text
    public GameObject interactionUI; // 상호작용 UI (버튼 등 포함)
    public GameObject passwordUI; // 비밀번호 입력 UI
    public GameObject passwordWrongUI; // 틀렸음 UI
    public GameObject GaugeUI; // Fangbaby 옆에 pipe
    public GameObject button;
    public GameObject connectedDoor;
    public TMP_InputField passwordInputField; // 비밀번호 입력 필드
    public string correctPassword = "4321"; // 정답 비밀번호
    private GameObject currentInteractable; // 현재 상호작용 가능한 오브젝트
    private bool isUIActive = false; // UI가 활성화 되어 있는지 확인하는 변수
    private bool isDoorOpening = false;

    public Color highlightcolor = Color.red;
    private Color originalcolor;
    private Renderer objectRenderer;

    private Vector3 initialDoorPosition;

    void Start()
    {
        initialDoorPosition = connectedDoor.transform.position;
        passwordInputField.onEndEdit.AddListener(delegate { CheckPasswordOnEnter(); });
    }

    void Update()
    {
        Vector3 rayOrigin = new Vector3(transform.position.x,transform.position.y+1f,transform.position.z);
        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;
        
                Debug.DrawLine(ray.origin,ray.origin+ray.direction*rayDistance,Color.red);

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Interactable") || hit.collider.CompareTag("Story_Object") || hit.collider.CompareTag("Exit")) //레이가 인터랙태블 태그에 맞음
            {

                currentInteractable = hit.collider.gameObject;
                interactionText.text = currentInteractable.name;
                objectRenderer = currentInteractable.GetComponent<Renderer>();

                originalcolor = objectRenderer.material.color;

                objectRenderer.material.color = highlightcolor;
                interactionUI.SetActive(true);

                if (Input.GetKeyDown(KeyCode.F))
                {
                    InteractWithObject();
                }
            }

            if (hit.collider.CompareTag("Finish"))
            {
                GaugeUI.SetActive(true);
            }

        }
        else
        {
            if (objectRenderer != null)
            { 
                objectRenderer.material.color = Color.white;
        }

            interactionUI.SetActive(false);
            passwordUI.SetActive(false); // PasswordUI도 비활성화
            GaugeUI.SetActive(false);
            currentInteractable = null;

        }
        
        
        if(isDoorOpening)
        {
            Vector3 doorTargetPosition = initialDoorPosition = new Vector3(0, 3.5f, 0);
            connectedDoor.transform.position = Vector3.MoveTowards(connectedDoor.transform.position, doorTargetPosition, 2f * Time.deltaTime);

            // 목표 위치에 도달하면 문을 열기 종료
            if (connectedDoor.transform.position == doorTargetPosition)
            {
                isDoorOpening = false;
            }
        }
    }

    void InteractWithObject()
    {
       // if(currentInteractable != null)
       // {

            if(currentInteractable.name == "Door")
            {

            interactionUI.SetActive(false);
            passwordUI.SetActive(true);
            }

            else if (currentInteractable.CompareTag("Story_Object"))
            {
                InteractableObject interactable = currentInteractable.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }

          //  else if(currentInteractable.CompareTag("Exit"))
          //  
          //  {
          //
          //  }
        }
   // }

    void CheckPasswordOnEnter()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Enter 키 확인
        {
            CheckPassword();
        }
    }

    public void CheckPassword()
    {
        if(passwordInputField.text==correctPassword)
        {
            Debug.Log("비밀번호가 맞습니다. 문을 열게용~");
            OpenDoor();
            passwordUI.SetActive(false);

        }
        else
        {
            passwordWrongUI.SetActive(true);
        }
    }    

    void OpenDoor()
    {
        isDoorOpening = true; // 문 열기 시작
    }

    public void OnCloseButtonClick()
    {
        passwordWrongUI.SetActive(false);
    }
}
