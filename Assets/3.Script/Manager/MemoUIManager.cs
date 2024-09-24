using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*

메모 UI가 열릴 때(ShowMemo) isMemoClosed = false로 설정.
메모 UI가 닫힐 때(HideMemo) isMemoClosed = true로 설정.


*/


public class MemoUIManager : MonoBehaviour
{
    public static MemoUIManager instance = null;

    public GameObject memoUIPrefab;
    private GameObject memoUIInstance;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    //  public Image[] canvasImages;
    //  public Sprite newSprite;
    //  private int nextImageIndex = 0;

    private HashSet<MemoData> interactedObjects = new HashSet<MemoData>(); // 상호작용한 오브젝트 추적
    public bool isMemoClosed = true; // 처음에는 Memo UI가 닫힌 상태

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // public void AssignCanvasImages(Image[] newCanvasImages)
    // {
    //     canvasImages = newCanvasImages;
    // }

    public void ShowMemo(MemoData memoData)
    {
        isMemoClosed = false;

        if (!interactedObjects.Contains(memoData))
        {
            interactedObjects.Add(memoData);

            //  if(nextImageIndex<canvasImages.Length)
            //  {
            //      canvasImages[nextImageIndex].sprite = newSprite;
            //      nextImageIndex++;
            //  }

            Stage stage = FindObjectOfType<Stage>();
            if (stage != null)
            {
                stage.InteractWithMemo(memoData);
            }
        }

        // 처음 요청이 있을 때만 memoUIInstance를 인스턴스화
        if (memoUIInstance == null)
        {
            memoUIInstance = Instantiate(memoUIPrefab, transform);

            Transform memoUITransform = memoUIInstance.transform.Find("MemoUI");

            if (memoUITransform != null)
            {
                // 텍스트 컴포넌트를 찾고 설정합니다.
                titleText = memoUITransform.Find("TitleText").GetComponent<TextMeshProUGUI>();
                contentText = memoUITransform.Find("ContentText").GetComponent<TextMeshProUGUI>();
            }
            if (titleText == null || contentText == null)
            {
                return;
            }

            memoUIInstance.SetActive(false);
        }
        // 데이터 설정
        titleText.text = memoData.title;
        contentText.text = memoData.content;
        // UI를 활성화합니다.
        memoUIInstance.SetActive(true);
    }

    public void HideMemo()
    {
        if (memoUIInstance != null && memoUIInstance.activeSelf)
        {
            memoUIInstance.SetActive(false);
        }
        isMemoClosed = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && memoUIInstance.activeSelf)
        {
            HideMemo();
        }
    }

    public void ClearInteractedObjects()
    {
        interactedObjects.Clear();
    }
}
