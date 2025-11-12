using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XRControllerLogger : MonoBehaviour
{
    [Header("Controller References")]
    public XRRayInteractor rayInteractor;
    [Header("Input Actions - Buttons")]
    public InputActionProperty gripButton;
    public InputActionProperty triggerButton;

    [Header("Input Actions - Tracking")]
    public InputActionProperty positionAction;
    public InputActionProperty rotationAction;

    [Header("Logging Settings")]
    public bool enableLogging = true;

    private float lastLogTime = 0f;
    private XRControllerState previousState;
    private Logger mainLogger;

    void Start()
    {
        mainLogger = FindObjectOfType<Logger>();
        if (mainLogger == null)
        {
            Debug.LogWarning("XRControllerLogger: No Logger component found in scene.");
        }

        if (rayInteractor == null)
            rayInteractor = GetComponent<XRRayInteractor>();

        previousState = new XRControllerState();

        EnableInputActions();
    }

    void OnEnable()
    {
        EnableInputActions();
    }

    void OnDisable()
    {
        DisableInputActions();
    }

    void Update()
    {
        if (!enableLogging) return;

        XRControllerState currentState = CaptureControllerState();
        LogControllerState(currentState);
        previousState = currentState;
        lastLogTime = Time.time;
    }

    private void EnableInputActions()
    {
        if (gripButton.action != null) gripButton.action.Enable();
        if (triggerButton.action != null) triggerButton.action.Enable();
        if (positionAction.action != null) positionAction.action.Enable();
        if (rotationAction.action != null) rotationAction.action.Enable();
    }

    private void DisableInputActions()
    {
        if (gripButton.action != null) gripButton.action.Disable();
        if (triggerButton.action != null) triggerButton.action.Disable();
        if (positionAction.action != null) positionAction.action.Disable();
        if (rotationAction.action != null) rotationAction.action.Disable();
    }

    private XRControllerState CaptureControllerState()
    {
        XRControllerState state = new XRControllerState();

        state.position = transform.position;
        state.rotation = transform.rotation;
        state.forward = transform.forward;
        state.right = transform.right;
        state.up = transform.up;

        state.gripValue = gripButton.action != null ? gripButton.action.ReadValue<float>() : 0f;
        state.triggerValue = triggerButton.action != null ? triggerButton.action.ReadValue<float>() : 0f;

        if (positionAction.action != null)
            state.trackedPosition = positionAction.action.ReadValue<Vector3>();
        
        if (rotationAction.action != null)
            state.trackedRotation = rotationAction.action.ReadValue<Quaternion>();

        if (rayInteractor != null)
        {
            state.rayOrigin = transform.position;
            state.rayDirection = transform.forward;
            
            // Check if ray is hitting something
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                state.rayHitPoint = hit.point;
                state.rayHitDistance = hit.distance;
                state.rayHitObject = hit.collider != null ? hit.collider.gameObject.name : "null";
                state.isRayHitting = true;
            }
            else
            {
                state.isRayHitting = false;
                state.rayHitObject = "none";
            }
        }

        return state;
    }

    private void LogControllerState(XRControllerState state)
    {
        string message = $"XR Controller State: " +
            $"Pos({state.position.x:F3}, {state.position.y:F3}, {state.position.z:F3}) " +
            $"Rot({state.rotation.x:F3}, {state.rotation.y:F3}, {state.rotation.z:F3}, {state.rotation.w:F3}) " +
            $"Grip:{state.gripValue:F2} Trigger:{state.triggerValue:F2}";

        if (state.isRayHitting)
        {
            message += $" | Ray Hit: {state.rayHitObject} at distance {state.rayHitDistance:F2}";
        }

        Debug.Log(message);

        if (mainLogger != null)
        {
            mainLogger.LogXRController(state);
        }
    }

    public XRControllerState GetCurrentState()
    {
        return CaptureControllerState();
    }
}
