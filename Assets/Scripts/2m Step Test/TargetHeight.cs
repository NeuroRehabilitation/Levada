using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHeight : MonoBehaviour
{
    public _2mStepTest_Manager TestManager;

    public void SetHeight(float height)
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, height, gameObject.transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        TestManager.OnTargetHeightTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        TestManager.OnTargetHeightTriggerExit(other);
    }
}
