using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalFight : MonoBehaviour
{
    public float maxGause = 100f;
    public float currentGause = 0f;
    public float gaugeDecreaseRate = 10f;
    public float gaugeIncreaseRate = 80f;

    public GameObject gaugeUI;
    public Image gaugeImage;
    public GameObject door;
    public GameObject complImage;
    public Stage stage;

    private bool isDoorOpen = false;
    private bool iscomplImageShown = false;

    void Start()
    {
        currentGause = 0f;
        UpdateGaugeUI();
    }

    void Update()
    {
        if (currentGause > 0 && !isDoorOpen)
        {
            currentGause -= gaugeDecreaseRate * Time.deltaTime;
            currentGause = Mathf.Clamp(currentGause, 0, maxGause);
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !isDoorOpen)
        {
            currentGause += gaugeIncreaseRate * Time.deltaTime;
            currentGause = Mathf.Clamp(currentGause, 0, maxGause);
        }

        UpdateGaugeUI();

        if (currentGause >= maxGause && !isDoorOpen && !iscomplImageShown)
        {
            ShowComplImage();
        }

        if (iscomplImageShown && Input.anyKeyDown)
        {
            HideUI();
        }
    }

    void UpdateGaugeUI()
    {
        gaugeImage.fillAmount = currentGause / maxGause;
    }

    void ShowComplImage()
    {
        iscomplImageShown = true;
        complImage.SetActive(true);
    }

    void HideUI()
    {
        gaugeUI.SetActive(false);
        isDoorOpen = true;

        if (stage != null)
        {
            stage.isDoorOpen[4] = true;
        }
    }
}
