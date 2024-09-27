using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class M_ButtonManager : ButtonManager
{
    private int nextimgIndex = 0;
    private int playerButtonCount = 0;

    private Color playerColor = Color.white; // 기본 색상

    //private bool hasPlayerColor = false;

    public override void OnButtonPressed()
    {
        base.OnButtonPressed();

foreach(var button in buttons)
        {
            ButtonTracker tracker = button.GetComponent<ButtonTracker>();
            if(tracker !=null&&!tracker.IsButtonPressedByPlayer())
            {
                tracker.SetPlayerColor(playerColor);
                playerButtonCount++;
            }
        }
    }

    public override void OnButtonReleased()
    {
        base.OnButtonReleased();

        if (nextimgIndex > 0)
            nextimgIndex--;
        canvasImages[nextimgIndex].sprite = originalSprite;
        canvasImages[nextimgIndex].color = Color.white;

    }
}
