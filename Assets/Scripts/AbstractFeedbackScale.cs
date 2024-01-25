using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractFeedbackScale : MonoBehaviour
{
    public GameObject ProgressMarker;
    public GameObject TopProgressMarker;
    public GameObject BottomProgressMarker;
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

    private void SetMarker()
    {
        ProgressMarker.transform.localPosition = new Vector3(
            (TopProgressMarker.transform.localPosition.x + BottomProgressMarker.transform.localPosition.x) / 2,
            (TopProgressMarker.transform.localPosition.y + BottomProgressMarker.transform.localPosition.y) / 2,
            (TopProgressMarker.transform.localPosition.z + BottomProgressMarker.transform.localPosition.z) / 2);

        ProgressMarker.transform.localScale = new Vector3(ProgressMarker.transform.localScale.x,
            (TopProgressMarker.transform.localPosition.y - BottomProgressMarker.transform.localPosition.y),
            ProgressMarker.transform.localScale.z);
    }

    public void SetTopMarker(float progress)
    {
        float trackProgress = 0f;

        trackProgress = progress - 0.5f;

        if (trackProgress > 0.5)
            trackProgress = 0.5f;
        else if (trackProgress < -.5f)
            trackProgress = -0.5f;

        TopProgressMarker.transform.localPosition = new Vector3(TopProgressMarker.transform.localPosition.x, trackProgress, TopProgressMarker.transform.localPosition.z);
        SetMarker();
    }

    public void SetBottomMarker(float progress)
    {
        float trackProgress = 0f;

        trackProgress = progress - 0.5f;

        if (trackProgress > 0.5)
            trackProgress = 0.5f;
        else if (trackProgress < -.5f)
            trackProgress = -0.5f;

        BottomProgressMarker.transform.localPosition = new Vector3(TopProgressMarker.transform.localPosition.x, trackProgress, TopProgressMarker.transform.localPosition.z);
        SetMarker();
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
