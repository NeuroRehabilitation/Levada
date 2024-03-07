using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using TMPro;

public class LSLInput : MonoBehaviour
{
    StreamInfo[] streamInfos;
    StreamInlet streamInlet;

    string[] channels;
    private int channelCount;
    private XMLElement channelgroup;

    // We need to find the stream somehow. You must provide a StreamName in editor or before this object is Started.
    public string StreamName;
    ContinuousResolver resolver;

    // We need to keep track of the inlet once it is resolved.
    private StreamInlet inlet;

    // We need buffers to pass to LSL when pulling data.
    private float[] data_buffer;  

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
            yield return new WaitForSeconds(.1f);
            results = resolver.results();
        }

        inlet = new StreamInlet(results[0]);
        channelCount = streamInlet.info().channel_count();
        channels = new string[channelCount];
        channelgroup = streamInlet.info().desc().child("channels").child("channel");

        for (int i = 0; i < channelCount; i++)
        {
            channels[i] = channelgroup.child_value("label");
            channelgroup = channelgroup.next_sibling();
        }

        data_buffer = new float[channelCount];
    }

    void Update()
    {
        if (inlet != null)
        {
            double samples_returned = inlet.pull_sample(data_buffer);
            var sample = data_buffer[0];
            // Debug.Log("Samples returned: " + samples_returned);
        }
    }
}
