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
        cameraCube.transform.position = handTransform.position;
        cameraCube.transform.rotation = handTransform.rotation;

        miniCamera.transform.position = cameraCube.transform.position;
        miniCamera.transform.rotation = cameraCube.transform.rotation;
    }
}

