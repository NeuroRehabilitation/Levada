using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
    [SerializeField] private AudioClip firstBlockInformative;
    [SerializeField] private AudioClip firstBlockRestorative;
    [SerializeField] private AudioClip secondBlockInformative;
    [SerializeField] private AudioClip secondBlockRestorative;
    [SerializeField] private AudioClip thirdBlockInformative;
    [SerializeField] private AudioClip thirdBlockRestorative;

    private AudioSource audioSource;

    private Dictionary<KeyCode, AudioClip> keyToClip;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        keyToClip = new Dictionary<KeyCode, AudioClip>
        {
            { KeyCode.Alpha1, firstBlockInformative },
            { KeyCode.Alpha2, firstBlockRestorative },
            { KeyCode.Alpha3, secondBlockInformative },
            { KeyCode.Alpha4, secondBlockRestorative },
            { KeyCode.Alpha5, thirdBlockInformative },
            { KeyCode.Alpha6, thirdBlockRestorative }
        };
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
    }
}