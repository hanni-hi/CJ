using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System;

/*
 2����
- �� ���� ���
- �������� Ŭ���� üũ 
- Ÿ�Ӷ��� ���
- �޸� ������Ʈ�� ��ȣ�ۿ� 
 
 */
public class Stage : MonoBehaviour
{
    public Transform canvasParent;
    public string imagePrefix = "Mission";
    public Image[] canvasImages;

    public GameObject[] doors;
    public GameObject player;
    public float doorMoveDistance = 20;
    public float moveSpeed = 1f;

    private bool isFirstStageClear = false; // ù ���������� Ŭ����Ǿ����� ����
    private bool isSecondStageClear = false; // �� ��° �������� Ŭ���� ����
    private bool isThirdStageClear = false; // �� ��° �������� Ŭ���� ����
    private bool isSecondRoomTLPlayed = false; // �� ��° �� Ÿ�Ӷ����� ����Ǿ����� ����
    private bool shouldPlayFirstAfterSecond = false; // �� ��° Ÿ�Ӷ��� �� ù ��° Ÿ�Ӷ��� ��� ����
    public bool[] isDoorOpen = new bool[5]; // �� ���� ���ȴ��� ����
    private bool isSecondRoomTimelinePlaying = false; // �� ��° �� Ÿ�Ӷ����� ��� ������ ����
    private bool isFirstStageTimelineWaiting = false; // ù ��° �������� Ÿ�Ӷ��� ��� ����
    private bool isTimelinePlaying = false; //Ÿ�Ӷ����� ��� ������ ����

    private int nextImageIndex = 0;
    public Sprite newSprite;
    public Sprite defaultSprite;

    private HashSet<MemoData> interactedObjects = new HashSet<MemoData>();

    public GameObject cubesToNotDestroy;

    public GameObject objectToActivate;
    public GameObject[] objectToChangeMat;
    public Material[] newMaterial;
    public PlayableDirector FirstStageTLDirector;
    public PlayableDirector SecondRoomTLDirector;


    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        if(currentSceneName== "Example_01")
        {
            AssignMissionImages();
        }

        if(SecondRoomTLDirector !=null)
        {
            SecondRoomTLDirector.stopped += OnSecondRoomTimelineEnd;
        }

        if(FirstStageTLDirector !=null)
        {
            FirstStageTLDirector.stopped += OnFirstStageTimelineEnd;
        }
    }

    void OnFirstStageTimelineEnd(PlayableDirector director)
    {
        // ù��° �������� Ÿ�Ӷ����� �������� ����
    }

    void AssignMissionImages()
    {
        List<Image> imagesList = new List<Image>();

        foreach(Transform child in canvasParent)
        {
            if(child.name.StartsWith(imagePrefix))
            {
                Image img = child.GetComponent<Image>();
                if(img != null)
                {
                    imagesList.Add(img);
                }
            }
        }
        canvasImages = imagesList.ToArray();
    }

    //�޸�� ��ȣ�ۿ� ���� �� ȣ���ϴ� �Լ�
    public void InteractWithMemo(MemoData memoData)
    {


        if(memoData.name.Contains("MemoData2")|| memoData.name.Contains("MemoData8"))
        {
            return;
        }

        Debug.Log($"���� ��ȣ�ۿ� �ϴ� �޸����� {memoData.name}");
        if(!interactedObjects.Contains(memoData))
        {
            interactedObjects.Add(memoData);

            //�̹��� ����
            if(nextImageIndex<canvasImages.Length)
            {
                canvasImages[nextImageIndex].sprite = newSprite; //�̹��� ����
                nextImageIndex++;
            
                if(nextImageIndex==canvasImages.Length&& !isFirstStageClear)
                {
                    Debug.Log($"1�� Ŭ���� ! ");
                    StartCoroutine(WaitForMemoToClose(() =>
                    {
                        FirstStageTLDirector.Play();
                        isFirstStageClear = true;
                        isDoorOpen[0] = true;
                    }));
                }
                else if(nextImageIndex==canvasImages.Length&&isFirstStageClear&&!isSecondStageClear)
                {
                    StartCoroutine(WaitForMemoToClose(() =>
                    {
                        FirstStageTLDirector.Play();
                        isSecondStageClear = true;
                        isDoorOpen[2] = true;
                    }));
                }
                else if (nextImageIndex == canvasImages.Length &&isSecondStageClear&&!isThirdStageClear)
                {
                    StartCoroutine(WaitForMemoToClose(() =>
                    {
                       // FirstStageTLDirector.Play();
                        isThirdStageClear = true;
                        isDoorOpen[3] = true;
                    }));
                }
            }
        }

        if (memoData.name.Contains("MemoData7")&& isFirstStageClear) //Knock 2�� ��
        {
            StartCoroutine(WaitForMemoToClose(() => //�޸� ������� �����! 
            {
            if(!isSecondRoomTLPlayed) //���� 2���� Ÿ�Ӷ��� ��� �ȵǾ��ٸ�
            {
                if(nextImageIndex == canvasImages.Length) //�̼��� ��� �ϼ��Ǿ��ٸ�
                {
                    shouldPlayFirstAfterSecond = true; //2���� Ÿ�Ӷ��� ����ϰ� �״��� �������� Ŭ���������
                }
                else
                {
                    shouldPlayFirstAfterSecond = false;
                }
                SecondRoomTLDirector.Play();
                isSecondRoomTLPlayed = true;
            }
                    isDoorOpen[1] = true;

                if (objectToActivate != null)
                {
                    objectToActivate.SetActive(true);
                }

                for (int i = 0; i < objectToChangeMat.Length; i++)
                {
                    if (objectToChangeMat[i] != null && newMaterial.Length > i)
                    {
                        Renderer renderer = objectToChangeMat[i].GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material = newMaterial[i];
                        }
                    }
                }
            }));
        }
    }

    private IEnumerator WaitForMemoToClose(Action action)
    {
        while(!MemoUIManager.instance.isMemoClosed)
        {
            yield return null;
        }
        action?.Invoke();
    }

    void Update()
    {
        for(int i=0;i<doors.Length;i++)
        {
            if(isDoorOpen[i])
            {
                MoveDoor(doors[i]);
            }
        }
    }

    public void MoveDoor(GameObject door)
    {
            Vector3 targetPosition = new Vector3(door.transform.position.x, door.transform.position.y+doorMoveDistance,door.transform.position.z);
            door.transform.position = Vector3.MoveTowards(door.transform.position, targetPosition, moveSpeed * Time.deltaTime);
      
            if(Vector3.Distance(door.transform.position,targetPosition)<0.01f)
            {
            isDoorOpen[System.Array.IndexOf(doors, door)] = false;
            }
    }

    public void ResetSprite()
    {
            nextImageIndex = 0;

            for (int i = 0; i < canvasImages.Length; i++)
            {
                canvasImages[i].sprite = defaultSprite;
            }
    }

    public void CheckAndDestroyCube(GameObject cube)
    {
        if(cube != cubesToNotDestroy)
        {
            Destroy(cube);
        }
    }

    void OnSecondRoomTimelineEnd(PlayableDirector director)
    {
        isSecondRoomTimelinePlaying = false;
        if(isFirstStageTimelineWaiting)
        {
            FirstStageTLDirector.Play();
            isFirstStageTimelineWaiting = false;
        }

        if(!shouldPlayFirstAfterSecond)
        {
            return;
        }

        if(FirstStageTLDirector !=null)
        {
            FirstStageTLDirector.Play();
        }
    }


   // private IEnumerator Delay(PlayableDirector director, float delay)
   // {
   //     yield return new WaitForSeconds(delay);
   //
   //     if(director !=null)
   //     {
   //         director.Play();
   //     }
   //             
   // }
}
