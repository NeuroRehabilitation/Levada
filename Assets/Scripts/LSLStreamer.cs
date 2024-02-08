using LSL;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LSLStreamer : MonoBehaviour
{
    public string StreamName;
    public string StreamType;
    public string StreamID;
    // panquecas
    public int SampleRate;

    public TypeOfStream typeStream = new TypeOfStream();

    public List<string> Channels;


    private StreamOutlet Outlet;
    private bool Stream_On= true;

    private void Awake()
    {
        DontDestroyOnLoad(transform.parent);
    }

    public void StartStream()
    {
        StreamInfo streamInfo;
        var typestream = LSL.channel_format_t.cf_string;
        switch (typeStream) {
            case TypeOfStream.string1:
                typestream = LSL.channel_format_t.cf_string;
                break;
            case TypeOfStream.int64:
                typestream = LSL.channel_format_t.cf_int64;
                break;
            case TypeOfStream.float32:
                typestream = LSL.channel_format_t.cf_float32;
                break;
            case TypeOfStream.double64:
                typestream = LSL.channel_format_t.cf_double64;
                break;
            default:
                typestream = LSL.channel_format_t.cf_undefined;
                break;
        }

        streamInfo = new StreamInfo(StreamName, StreamType, Channels.Count, SampleRate, typestream);
        XMLElement chans = streamInfo.desc().append_child("channels");
        foreach (string channel in Channels)
        {
            chans.append_child("channel").append_child_value("label", channel);
        }
        Outlet = new StreamOutlet(streamInfo);
        Stream_On=true;
    }
    public void StreamData<T>(T[] sample)
    {

        if (Stream_On)
        {
            var streamsample = default(object); // Declare and initialize the variable outside the switch

            switch (typeStream)
            {
                case TypeOfStream.string1:
                    Outlet.push_sample(sample as string[]);
                    break;
                case TypeOfStream.int64:
                    Outlet.push_sample(sample as int[]);
                    break;
                case TypeOfStream.float32:
                    Outlet.push_sample(sample as float[]);
                    break;
                case TypeOfStream.double64:
                    Outlet.push_sample(sample as double[]);
                    break;
                default:
                    streamsample = sample as string[];
                    break;
            }

        // You can now use the `streamsample` variable outside the switch statement
            
            //Debug.Log(sample);
        }
        
        else
        {
            //Debug.Log("Stream hasn't been started");
        }
    }
    
    public void StopStream()
    {
        Outlet.Close();
        Stream_On = false;
    }

}
public enum TypeOfStream
{
    float32,
    string1,
    int64,
    double64,
    undefined

}
