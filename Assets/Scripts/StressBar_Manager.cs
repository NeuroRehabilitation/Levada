using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StressBar_Manager : MonoBehaviour
{
    private static StressBar_Manager instance;
    private GameObject mainCamera;
    public float distance = 2.0f;
    public Vector3 offset;
    public float smoothSpeed = 5f; // Speed for smooth positioning.

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Update()
    {
        // Calculate target position directly in the center of the XR camera's forward view.
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * distance + offset;

        // Smoothly move the canvas to the target position.
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // Ensure the canvas is always facing the XR camera.
        transform.LookAt(mainCamera.transform);

        // Adjust rotation to face the user properly (if needed, depending on your canvas setup).
        transform.Rotate(0, 180, 0); // Rotates the canvas to face the user.
    }
}
