using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _8ftPlane : MonoBehaviour
{
    public _8ftUpGo_Manager TestManager;

    void OnTriggerEnter(Collider other)
    {
        TestManager.On8ftPlaneTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        TestManager.On8ftPlaneTriggerExit(other);
    }
}
