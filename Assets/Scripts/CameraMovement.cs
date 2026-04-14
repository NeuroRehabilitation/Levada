using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    public GameObject cameraCube;
    public Camera miniCamera;
    [Header("CAVE settings")]
    public bool centerHandToDisplay;
    public GameObject vRManager;
    public GameObject rayTarget;
    public float rayTargetDistance = 1f;

    private Vector3 _lastRayOrigin;
    private Vector3 _lastRayDirection;
    private bool _lastHit;
    private Vector3 _lastHitPoint;
    private int maxAlignments = 10;
    private int alignCount = 0;
    private InputAction resetButton;

    void Awake()
    {
        resetButton = new InputAction(
            name: "ResetButton",
            type: InputActionType.Button,
            binding: "<XRController>{RightHand}/secondaryButton"
        );
    }

    void Start()
    {
        if (resetButton != null)
        {
            resetButton.Enable();
        }
    }

    void Update()
    {
        cameraCube.transform.position = transform.position + new Vector3(0f, 0.1f, 0f);
        cameraCube.transform.rotation = transform.rotation;

        miniCamera.transform.position = cameraCube.transform.position;
        miniCamera.transform.rotation = cameraCube.transform.rotation;

        // Repeat code, should be refactored
        Camera selectedCamera = null;
        if (vRManager != null)
        {
            int surfaceCount = 0;
            foreach (Transform surface in vRManager.transform)
            {
                if (surface.name.StartsWith("Surface(Clone)"))
                {
                    surfaceCount++;
                    if (surfaceCount == 3)
                    {
                        foreach (Transform child in surface)
                        {
                            if (child.name.StartsWith("User View Camera(Clone)"))
                            {
                                selectedCamera = child.GetComponent<Camera>();
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }
        if (selectedCamera == null) selectedCamera = Camera.main;

        if (rayTarget != null && selectedCamera != null)
        {
            rayTarget.transform.position = selectedCamera.transform.position + selectedCamera.transform.forward * rayTargetDistance;
            rayTarget.transform.rotation = Quaternion.LookRotation(-selectedCamera.transform.forward, selectedCamera.transform.up);
        }

        if (Input.GetKeyDown(KeyCode.C) || (resetButton != null && resetButton.triggered))
        {
            alignCount = 0;
            rayTarget.GetComponent<BoxCollider>().enabled = true;
        }

        if (centerHandToDisplay && alignCount < maxAlignments)
        {
            CenterHandToDisplay();
            alignCount++;
        }
        else if (alignCount == maxAlignments)
        {
            Debug.Log("Max alignments reached.");
            rayTarget.GetComponent<BoxCollider>().enabled = false;
            alignCount++;
        }
    }

    private void CenterHandToDisplay()
    {
        if (transform.parent == null) return;
        Camera mainCamera = null;
        if (vRManager != null)
        {
            int surfaceCount = 0;
            foreach (Transform surface in vRManager.transform)
            {
                if (surface.name.StartsWith("Surface(Clone)"))
                {
                    surfaceCount++;
                    if (surfaceCount == 3) // should be the front camera
                    {
                        Debug.Log($"Selected third Surface(Clone): {surface.name}");
                        foreach (Transform child in surface)
                        {
                            if (child.name.StartsWith("User View Camera(Clone)"))
                            {
                                Debug.Log($"Found User View Camera(Clone) under {surface.name}");
                                mainCamera = child.GetComponent<Camera>();
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }
        if (mainCamera == null)
        {
            Debug.LogWarning("No matching User View Camera(Clone) found. Using Camera.main as fallback.");
            mainCamera = Camera.main;
        }
        if (mainCamera == null) return;

        var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hitPoint;

        _lastRayOrigin = ray.origin;
        _lastRayDirection = ray.direction;
        _lastHit = false;
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.cyan);

        if (Physics.Raycast(ray, out hitPoint, 100.0f))
        {
            _lastHit = true;
            _lastHitPoint = hitPoint.point;
            //Debug.Log($"Raycast hit: {hitPoint.collider?.name ?? "(no collider name)"} at {hitPoint.point}");
            Transform parent = transform.parent;

            parent.position = hitPoint.point - parent.rotation * transform.localPosition;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_lastRayOrigin, _lastRayOrigin + _lastRayDirection * 10f);
        if (_lastHit)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_lastHitPoint, 0.03f);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.03f);
    }
}

