using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorLevel : MonoBehaviour
{
    public _2mStepTest_Manager TestManager;

    void OnTriggerEnter(Collider other)
    {
        TestManager.OnFloorLevelTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        TestManager.OnFloorLevelTriggerExit(other);
    }
}
