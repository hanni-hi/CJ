using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTracker : MonoBehaviour
{
    private Button_Script bscript;

    public Image linkedUISprite;
    private Color buttonOwnerColor = Color.clear;

    private bool isTracked = false;



    void Start()
    {

        bscript = GetComponent<Button_Script>();

        if(bscript==null)
        {
            Debug.LogError("Button_Script를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if(bscript.isPushed&&!isTracked)
        {
            if (buttonOwnerColor != Color.clear)
            {
                linkedUISprite.color = buttonOwnerColor;
                isTracked = true;
            }
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
}
