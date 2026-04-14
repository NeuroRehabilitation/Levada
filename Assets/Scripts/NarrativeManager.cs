using System;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] Informative;
    [SerializeField] private AudioClip[] Restorative;
    private KeyCode[] keys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    private AudioSource audioSource;

    private Dictionary<KeyCode, AudioClip> keyToClip;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if(keyToClip == null)
        {
            keyToClip = new Dictionary<KeyCode, AudioClip>();
        }

        for(int i = 0; i < Informative.Length; i++)
        {
            keyToClip.Add(keys[i], Informative[i]);        
        }

        for(int j = 0; j < Restorative.Length; j++)
        {
            keyToClip.Add(keys[j + Informative.Length], Restorative[j]);
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    void Update()
    {
        if (keyToClip == null)
        {
            return;
        }

        foreach (KeyValuePair<KeyCode, AudioClip> entry in keyToClip)
        {
            if (Input.GetKeyDown(entry.Key))
            {
                PlayClip(entry.Value);
                break;
            }
        }

        if(audioSource.isPlaying == false)
        {
            audioSource.clip = null;
        }
    }
}