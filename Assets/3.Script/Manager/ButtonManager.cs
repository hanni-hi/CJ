using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class ButtonManager : MonoBehaviour
{
    public Button_Script[] buttons;
    public GameObject exitDoor;

    public PlayableDirector introTimeline; // 게임 시작 타임라인
    public PlayableDirector outroTimeline; // 게임 종료 타임라인

    private Vector3 initialExitDoorPosition;
    public float moveSpeed = 3f;
    public float upDistance = 3.5f;
    private bool isExitOpen = false;

    public Image[] canvasImages;
    public Sprite newSprite;
    public Sprite originalSprite;
    private int nextImageIndex = 0;

   // private UIManager uimanager;

    //
    private void Start()
    {
        initialExitDoorPosition = exitDoor.transform.position;

        introTimeline.Play();

        outroTimeline.stopped += OnOutroTimelineFinished;
    }

    private void Update()
    {
        if(AreAllButtonPressed()&& !isExitOpen)
        {
            Vector3 exitDoorTargetPosition = initialExitDoorPosition + new Vector3(0,upDistance,0);
            exitDoor.transform.position = Vector3.MoveTowards(exitDoor.transform.position, exitDoorTargetPosition, moveSpeed * Time.deltaTime);

            // 문이 완전히 열렸으면 상태를 true로 변경
            if (Vector3.Distance(exitDoor.transform.position, exitDoorTargetPosition) < 0.01f)
            {
                outroTimeline.Play();  // 게임 종료 타임라인 재생
                isExitOpen = true;  // 문이 완전히 열림
            }
        }
    }

    private void OnOutroTimelineFinished(PlayableDirector director)
    {
        UIManager.instance.ShowVictoryUI();
    }

    public virtual void OnButtonPressed()
    {
        // 이미지가 아직 모두 변경되지 않은 경우에만 처리
        if (nextImageIndex < canvasImages.Length)
        {
            canvasImages[nextImageIndex].sprite = newSprite; // 해당 인덱스의 이미지를 변경
            nextImageIndex++; // 다음에 변경할 이미지 인덱스 증가
        }
    }

    public virtual void OnButtonReleased()
    {
        if(nextImageIndex>0)
        {
            nextImageIndex--;
            canvasImages[nextImageIndex].sprite = originalSprite;
        }
    }

    private bool AreAllButtonPressed()
    {
        foreach(Button_Script button in buttons)
        {
            if(!button.isPushed)
            {
                return false;
            }
        }
        return true;
    }
}
