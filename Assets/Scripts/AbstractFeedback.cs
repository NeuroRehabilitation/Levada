using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractFeedback : MonoBehaviour
{
    public GameObject ProgressMarker;
    public GameObject TopGreenLight;
    public GameObject TopRedLight;
    public GameObject BottomGreenLight;
    public GameObject BottomRedLight;
    public Material Green;
    public Material DarkGreen;
    public Material Red;
    public Material DarkRed;

    private AudioSource[] _sounds = new AudioSource[3];

    // Use this for initialization
    void Start()
    {
        _sounds = gameObject.GetComponents<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(int sound)
    {
        if (0 <= sound && sound <= 2)
            _sounds[sound].Play();
    }

    public void SetMarker(float progress)
    {
        float trackProgress = 0f;

        trackProgress = progress - 0.5f;

        if (trackProgress > 0.5)
            trackProgress = 0.5f;
        else if (trackProgress < -.5f)
            trackProgress = -0.5f;

        ProgressMarker.transform.localPosition = new Vector3(ProgressMarker.transform.localPosition.x, trackProgress, ProgressMarker.transform.localPosition.z);
    }

    public void TurnTopOn()
    {
        TopGreenLight.GetComponent<Renderer>().material = Green;
        TopRedLight.GetComponent<Renderer>().material = DarkRed;
    }

    public void TurnTopOff()
    {
        TopGreenLight.GetComponent<Renderer>().material = DarkGreen;
        TopRedLight.GetComponent<Renderer>().material = Red;
    }

    public void TurnBottomOn()
    {
        BottomGreenLight.GetComponent<Renderer>().material = Green;
        BottomRedLight.GetComponent<Renderer>().material = DarkRed;
    }

    public void TurnBottomOff()
    {
        BottomGreenLight.GetComponent<Renderer>().material = DarkGreen;
        BottomRedLight.GetComponent<Renderer>().material = Red;
    }
}
