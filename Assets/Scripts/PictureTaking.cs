using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PictureTaking : MonoBehaviour
{

    public InputActionProperty captureButton;
    public Transform handTransform;

    void OnEnable()
    {
        captureButton.action.Enable();
    }

    void OnDisable()
    {
        captureButton.action.Disable();
    }

    void Update()
    {
        if (captureButton.action.WasPressedThisFrame())
        {
            Debug.Log("capture button was pressed");
            //TakeScreenshot();
            StartCoroutine(CaptureFromHandView());
        }
    }

    void TakeScreenshot()
    {
        string screenshotPath = Application.persistentDataPath + "/Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        ScreenCapture.CaptureScreenshot(screenshotPath);
        Debug.Log("Screenshot saved to: " + screenshotPath);
    }

    private System.Collections.IEnumerator CaptureFromHandView()
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;

        GameObject tempCamGO = new GameObject("TempScreenshotCam");
        Camera tempCam = tempCamGO.AddComponent<Camera>();

        tempCamGO.transform.position = handTransform.position;
        tempCamGO.transform.rotation = handTransform.rotation;

        tempCam.fieldOfView = 60f; //FOV

        RenderTexture rt = new RenderTexture(width, height, 24);
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        tempCam.targetTexture = rt;
        tempCam.Render();

        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        tempCam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        Destroy(tempCamGO);

        string baseFilename = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string screenshotPath = Application.persistentDataPath + "/" + baseFilename + ".png";
        System.IO.File.WriteAllBytes(screenshotPath, screenshot.EncodeToPNG());

        Debug.Log("Screenshot saved from hand view to: " + screenshotPath);

        float detectionDistance = 30f;
        float coneAngle = 30f;
        List<string> detectedNames = new List<string>();
        Collider[] nearbyColliders = Physics.OverlapSphere(handTransform.position, detectionDistance);

        foreach (var col in nearbyColliders)
        {
            Vector3 directionToObject = (col.transform.position - handTransform.position).normalized;
            float angle = Vector3.Angle(handTransform.forward, directionToObject);

            if (angle < coneAngle)
            {
                detectedNames.Add(col.gameObject.name);
            }
        }

        Debug.DrawRay(handTransform.position, handTransform.forward * detectionDistance, Color.red, 2f);


        PictureMetadata metadata = new PictureMetadata
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            position = handTransform.position,
            eulerRotation = handTransform.rotation.eulerAngles,
            forwardVector = handTransform.forward,
            detectedObjects = detectedNames.ToArray()
        };


        string json = JsonUtility.ToJson(metadata, true);

        string metadataPath = Application.persistentDataPath + "/" + baseFilename + ".json";
        System.IO.File.WriteAllText(metadataPath, json);

        Debug.Log("Metadata saved to: " + metadataPath);
    }
}

[System.Serializable]
public class PictureMetadata
{
    public string timestamp;
    public Vector3 position;
    public Vector3 eulerRotation;
    public Vector3 forwardVector;
    public string[] detectedObjects;
}
