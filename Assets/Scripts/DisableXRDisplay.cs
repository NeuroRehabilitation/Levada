using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DisableXRDisplay : MonoBehaviour
{
    public bool isHMD;

    void Awake()
    {
        var displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(displays);

        if (!isHMD)
        {
            foreach (var display in displays)
            {
                if (display.running)
                {
                    display.Stop();
                }
            }
        }
        else
        {
            foreach (var display in displays)
            {
                if (!display.running)
                {
                    display.Start();
                }
            }
        }
    }
}
