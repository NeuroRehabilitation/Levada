using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform handTransform;
    public GameObject cameraCube;
    public Camera miniCamera;

    void Update()
    {
        cameraCube.transform.position = handTransform.position + new Vector3(0f, 0.1f, 0f); //tmep
        cameraCube.transform.rotation = handTransform.rotation;

        miniCamera.transform.position = cameraCube.transform.position;
        miniCamera.transform.rotation = cameraCube.transform.rotation;

        CenterHandToDisplay();
    }

    private void CenterHandToDisplay()
    {
        XROrigin XrOrigin = handTransform.parent.parent.GetComponent<XROrigin>();
        Vector3 offsetTransform = handTransform.parent.transform.position;

        Camera mainCamera = null;
        foreach (Camera camera in Camera.allCameras)
        {
            if (camera.targetDisplay == 0) mainCamera = camera;
        }

        var ray = mainCamera.ScreenPointToRay(new Vector2( Screen.height / 2, Screen.width / 2));
        RaycastHit hitPoint;

        if (Physics.Raycast(ray, out hitPoint, 100.0f))
        {
            XrOrigin.CameraYOffset = hitPoint.point.y / 100; //Biggest issue, doesn't work on all scenes
            offsetTransform.x = hitPoint.point.x;
            offsetTransform.z = hitPoint.point.z;
        }
    }
}

