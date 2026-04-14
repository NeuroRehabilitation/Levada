using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DisableXRDisplay : MonoBehaviour
{
    public bool isHMD;

    private bool displayDisabled = false;
    private bool hmdPresent = true;

    void Awake()
    {
        FixDisplay();
    }

    void Update()
    {
        if (!isHMD)
        {
            hmdPresent = GetHmdPresent();
            if (!hmdPresent)
            {
                displayDisabled = false;
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.T) || (isHMD && displayDisabled) || (!isHMD && !displayDisabled))
        {
            FixDisplay();
        }
    }

    void FixDisplay()
    {
        if (isHMD && !GetHmdPresent())
        {
            displayDisabled = false;
            return;
        }

        var displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(displays);

        if (!isHMD)
        {
            foreach (var display in displays)
            {
                if (display.running)
                {
                    display.Stop();
                    displayDisabled = true;
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
                    displayDisabled = false;
                }
            }
        }
    }

    private bool GetHmdPresent()
    {
        var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (device.isValid && device.TryGetFeatureValue(CommonUsages.userPresence, out bool present))
            return present;

        return true;
    }
}
