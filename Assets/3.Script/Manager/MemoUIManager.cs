using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*

�޸� UI�� ���� ��(ShowMemo) isMemoClosed = false�� ����.
�޸� UI�� ���� ��(HideMemo) isMemoClosed = true�� ����.


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

    private HashSet<MemoData> interactedObjects = new HashSet<MemoData>(); // ��ȣ�ۿ��� ������Ʈ ����
    public bool isMemoClosed = true; // ó������ Memo UI�� ���� ����

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("MemoUIManager instance�� �ʱ�ȭ�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogError("MemoUIManager instance�� �̹� �����մϴ�. ���� ������Ʈ�� �ı��մϴ�.");
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

        if(!interactedObjects.Contains(memoData))
        {
            interactedObjects.Add(memoData);

            //  if(nextImageIndex<canvasImages.Length)
            //  {
            //      canvasImages[nextImageIndex].sprite = newSprite;
            //      nextImageIndex++;
            //  }

            Stage stage = FindObjectOfType<Stage>();
            if(stage != null)
            {
                stage.InteractWithMemo(memoData);
            }
        }

        // ó�� ��û�� ���� ���� memoUIInstance�� �ν��Ͻ�ȭ
        if (memoUIInstance == null)
        {
            memoUIInstance = Instantiate(memoUIPrefab, transform);

            Transform memoUITransform = memoUIInstance.transform.Find("MemoUI");

            if (memoUITransform != null)
            {
                // �ؽ�Ʈ ������Ʈ�� ã�� �����մϴ�.
                titleText = memoUITransform.Find("TitleText").GetComponent<TextMeshProUGUI>();
                contentText = memoUITransform.Find("ContentText").GetComponent<TextMeshProUGUI>();
            }
            if (titleText == null || contentText == null)
            {
                Debug.LogError("TitleText �Ǵ� ContentText�� null�Դϴ�. MemoUI �������� ������ Ȯ���ϼ���.");
                return;
            }

            memoUIInstance.SetActive(false);
        }
        // ������ ����
        titleText.text = memoData.title;
        contentText.text = memoData.content;
        // UI�� Ȱ��ȭ�մϴ�.
        memoUIInstance.SetActive(true);
    }

    public void HideMemo()
    {
        if(memoUIInstance !=null&&memoUIInstance.activeSelf)
        {
            memoUIInstance.SetActive(false);
        }
            isMemoClosed = true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && memoUIInstance.activeSelf)
        {
            HideMemo();
        }
    }
}
