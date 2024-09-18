using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System;

/*
 2게임
- 문 여는 기능
- 스테이지 클리어 체크 
- 타임라인 재생
- 메모 오브젝트와 상호작용 
 
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

    private bool isFirstStageClear = false; // 첫 스테이지가 클리어되었는지 여부
    private bool isSecondStageClear = false; // 두 번째 스테이지 클리어 여부
    private bool isThirdStageClear = false; // 세 번째 스테이지 클리어 여부
    private bool isSecondRoomTLPlayed = false; // 두 번째 방 타임라인이 재생되었는지 여부
    private bool shouldPlayFirstAfterSecond = false; // 두 번째 타임라인 후 첫 번째 타임라인 재생 여부
    public bool[] isDoorOpen = new bool[5]; // 각 문이 열렸는지 여부
    private bool isSecondRoomTimelinePlaying = false; // 두 번째 방 타임라인이 재생 중인지 여부
    private bool isFirstStageTimelineWaiting = false; // 첫 번째 스테이지 타임라인 대기 상태
    private bool isTimelinePlaying = false; //타임라인이 재생 중인지 여부

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
        // 첫전째 스테이지 타임라인이 끝났을때 동작
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

    //메모와 상호작용 했을 때 호출하는 함수
    public void InteractWithMemo(MemoData memoData)
    {


        if(memoData.name.Contains("MemoData2")|| memoData.name.Contains("MemoData8"))
        {
            return;
        }

        Debug.Log($"지금 상호작용 하는 메모데이터 {memoData.name}");
        if(!interactedObjects.Contains(memoData))
        {
            interactedObjects.Add(memoData);

            //이미지 변경
            if(nextImageIndex<canvasImages.Length)
            {
                canvasImages[nextImageIndex].sprite = newSprite; //이미지 변경
                nextImageIndex++;
            
                if(nextImageIndex==canvasImages.Length&& !isFirstStageClear)
                {
                    Debug.Log($"1층 클리어 ! ");
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

        if (memoData.name.Contains("MemoData7")&& isFirstStageClear) //Knock 2층 방
        {
            StartCoroutine(WaitForMemoToClose(() => //메모가 사라지면 재생해! 
            {
            if(!isSecondRoomTLPlayed) //아직 2층방 타임라인 재생 안되었다면
            {
                if(nextImageIndex == canvasImages.Length) //미션이 모두 완성되었다면
                {
                    shouldPlayFirstAfterSecond = true; //2층방 타임라인 재생하고 그다음 스테이지 클리어재생해
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
