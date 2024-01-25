using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _8ftTestSetup : MonoBehaviour {

    public void SetDistance(float distance)
    {
        distance = distance - 2.4f;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, distance);
    }
}
