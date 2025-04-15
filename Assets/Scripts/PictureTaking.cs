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
        if(captureButton.action.WasPressedThisFrame()){
            Debug.Log("capture button was pressed");
            //TakeScreenshot();
            StartCoroutine(CaptureFromHandView());
        }
    }

    void TakeScreenshot(){
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

        string screenshotPath = Application.persistentDataPath + "/Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        System.IO.File.WriteAllBytes(screenshotPath, screenshot.EncodeToPNG());

        Debug.Log("Screenshot saved from hand view to: " + screenshotPath);
    }
}
