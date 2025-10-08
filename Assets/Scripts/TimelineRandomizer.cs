using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AzureSky;

public class TimelineRandomizer : MonoBehaviour
{
    public AzureSkyManager AzureSkyManager;
    void Start()
    {
        float[] times = { 7f, 10f, 13f, 16f };
        AzureSkyManager.timeController.timeline = times[Random.Range(0, times.Length)];
        GameObject.Find("Logger").GetComponent<Logger>().Log($"Timeline set to {AzureSkyManager.timeController.timeline}h", "Time" );
    }
}
