using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using TMPro;

public class LSLInput : MonoBehaviour
{
    StreamInlet streamInlet;

    string[] channels;
    private int channelCount;
    private XMLElement channelgroup;

    // We need to find the stream somehow. You must provide a StreamName in editor or before this object is Started.
    public string StreamName;
    ContinuousResolver resolver;

    // We need buffers to pass to LSL when pulling data.
    private float[] data_buffer;

    public float GameVariable;
    private bool startedCoroutine = false;
    private static LSLInput instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!StreamName.Equals(""))
            resolver = new ContinuousResolver("name", StreamName);
        else
        {
            Debug.LogError("Object must specify a name for resolver to lookup a stream.");
            this.enabled = false;
            return;
        }
        StartCoroutine(ResolveExpectedStream());
    }

    IEnumerator ResolveExpectedStream()
    {
        var results = resolver.results();
        while (results.Length == 0)
        {
            yield return new WaitForSeconds(0.1f);
            results = resolver.results();
        }


        streamInlet = new StreamInlet(results[0]);
        channelCount = streamInlet.info().channel_count();
        channels = new string[channelCount];
        channelgroup = streamInlet.info().desc().child("channels").child("channel");

        for (int i = 0; i < channelCount; i++)
        {
            channels[i] = channelgroup.child_value("label");
            channelgroup = channelgroup.next_sibling();
        }

        data_buffer = new float[channelCount];
        StopCoroutine(ResolveExpectedStream());
    }

    IEnumerator PullSample()
    {
        while(streamInlet != null)
        {
            double timestamp = streamInlet.pull_sample(data_buffer,1.0);
            
            if(timestamp != 0.0)
            {
                GameVariable = data_buffer[0];
                //Debug.Log("GameVariable = " + GameVariable);
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    void Update()
    {
        if (streamInlet != null && !startedCoroutine)
        {
            StartCoroutine(PullSample());
            startedCoroutine = true;
        }
    }
}
