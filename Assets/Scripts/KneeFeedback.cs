using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KneeFeedback : MonoBehaviour
{
    public Transform playerKnee;

    private float minimumY;
    private float maximumY;
    private float treshold = 0.4f;

    void Update()
    {
        if (playerKnee == null) return;

        if (minimumY > playerKnee.position.y)
        {
            minimumY = playerKnee.position.y;
            maximumY = minimumY + treshold;
        }

        GetComponent<Image>().fillAmount = Mathf.InverseLerp(minimumY, maximumY, playerKnee.position.y);
        //Debug.Log(GetComponent<Image>().fillAmount);

        if (GetComponent<Image>().fillAmount >= 0.5f)
        {
            ChangeColor(Color.green);
        }
        else
        {
            ChangeColor(Color.white);
        }
    }

    private void ChangeColor(Color value)
    {
        Color color = value;
        GetComponent<Image>().color = color;
    }
}
