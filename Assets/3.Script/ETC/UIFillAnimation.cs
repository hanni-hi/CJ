using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFillAnimation : MonoBehaviour
{
    public Image donutImage;
    public float speed = 1f;

    private bool filling = true;

    void Update()
    {
        // fillAmount 값을 한 방향으로만 증가시키고, 1에 도달하면 다시 0으로 돌아가게 설정
        donutImage.fillAmount += speed * Time.deltaTime;

        // fillAmount가 1을 넘으면 0으로 리셋
        if (donutImage.fillAmount >= 1f)    donutImage.fillAmount = 0f;
    }

}
