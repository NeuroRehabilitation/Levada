using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NarrativeUI : MonoBehaviour
{
    public AudioSource audioSource;
    public TextMeshProUGUI clipNameText;

    private string currentClipName;
    private AudioClip currentClip;

    void Update()
    {
        currentClip = audioSource.clip;
        if (currentClip != null)
        {
            currentClipName = currentClip.name;
        }
        else
        {
            currentClipName = "No narrative audio playing";
        }
        clipNameText.text = currentClipName;
    }
}
