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
        // fillAmount ���� �� �������θ� ������Ű��, 1�� �����ϸ� �ٽ� 0���� ���ư��� ����
        donutImage.fillAmount += speed * Time.deltaTime;

        // fillAmount�� 1�� ������ 0���� ����
        if (donutImage.fillAmount >= 1f)    donutImage.fillAmount = 0f;
    }

}
