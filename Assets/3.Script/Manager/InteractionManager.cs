using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionManager : MonoBehaviour
{
    public float rayDistance = 2f;
    public LayerMask interactableLayer;
    public TextMeshProUGUI interactionText; // ������Ʈ �̸��� ǥ���ϴ� Text
    public GameObject interactionUI; // ��ȣ�ۿ� UI (��ư �� ����)
    public GameObject passwordUI; // ��й�ȣ �Է� UI
    public GameObject passwordWrongUI; // Ʋ���� UI
    public GameObject GaugeUI; // Fangbaby ���� pipe
    public GameObject button;
    public GameObject connectedDoor;
    public TMP_InputField passwordInputField; // ��й�ȣ �Է� �ʵ�
    public string correctPassword = "4321"; // ���� ��й�ȣ
    private GameObject currentInteractable; // ���� ��ȣ�ۿ� ������ ������Ʈ
    private bool isUIActive = false; // UI�� Ȱ��ȭ �Ǿ� �ִ��� Ȯ���ϴ� ����
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
            if (hit.collider.CompareTag("Interactable") || hit.collider.CompareTag("Story_Object") || hit.collider.CompareTag("Exit")) //���̰� ���ͷ��º� �±׿� ����
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
            passwordUI.SetActive(false); // PasswordUI�� ��Ȱ��ȭ
            GaugeUI.SetActive(false);
            currentInteractable = null;

        }
        
        
        if(isDoorOpening)
        {
            Vector3 doorTargetPosition = initialDoorPosition = new Vector3(0, 3.5f, 0);
            connectedDoor.transform.position = Vector3.MoveTowards(connectedDoor.transform.position, doorTargetPosition, 2f * Time.deltaTime);

            // ��ǥ ��ġ�� �����ϸ� ���� ���� ����
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
        if (Input.GetKeyDown(KeyCode.Return)) // Enter Ű Ȯ��
        {
            CheckPassword();
        }
    }

    public void CheckPassword()
    {
        if(passwordInputField.text==correctPassword)
        {
            Debug.Log("��й�ȣ�� �½��ϴ�. ���� ���Կ�~");
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
        isDoorOpening = true; // �� ���� ����
    }

    public void OnCloseButtonClick()
    {
        passwordWrongUI.SetActive(false);
    }
}
