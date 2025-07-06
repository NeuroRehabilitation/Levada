using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform handTransform;
    public GameObject cameraCube;
    public Camera miniCamera;

    void Update()
    {
        cameraCube.transform.position = handTransform.position  + new Vector3(0f, 0.1f, 0f); //tmep
        cameraCube.transform.rotation = handTransform.rotation;

        miniCamera.transform.position = cameraCube.transform.position;
        miniCamera.transform.rotation = cameraCube.transform.rotation;
    }
}

