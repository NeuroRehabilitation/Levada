using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorLevel : MonoBehaviour
{
    public _2mStepTest_Manager TestManager;

    public void SetHeight(float height)
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, height, gameObject.transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        TestManager.OnFloorLevelTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        TestManager.OnFloorLevelTriggerExit(other);
    }
}
