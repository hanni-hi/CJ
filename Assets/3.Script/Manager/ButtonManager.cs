using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class ButtonManager : MonoBehaviour
{
    public Button_Script[] buttons;
    public GameObject exitDoor;

    public PlayableDirector introTimeline; // ���� ���� Ÿ�Ӷ���
    public PlayableDirector outroTimeline; // ���� ���� Ÿ�Ӷ���

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

            // ���� ������ �������� ���¸� true�� ����
            if (Vector3.Distance(exitDoor.transform.position, exitDoorTargetPosition) < 0.01f)
            {
                outroTimeline.Play();  // ���� ���� Ÿ�Ӷ��� ���
                isExitOpen = true;  // ���� ������ ����
            }
        }
    }

    private void OnOutroTimelineFinished(PlayableDirector director)
    {
        UIManager.instance.ShowVictoryUI();
    }

    public virtual void OnButtonPressed()
    {
        // �̹����� ���� ��� ������� ���� ��쿡�� ó��
        if (nextImageIndex < canvasImages.Length)
        {
            canvasImages[nextImageIndex].sprite = newSprite; // �ش� �ε����� �̹����� ����
            nextImageIndex++; // ������ ������ �̹��� �ε��� ����
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
