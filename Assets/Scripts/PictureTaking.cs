using UnityEngine;
using UnityEngine.InputSystem;

public class PictureTaking : MonoBehaviour
{

    public InputActionProperty captureButton;
    public Camera xrCamera;

    void OnEnable()
    {
        captureButton.action.Enable();
    }

    void OnDisable()
    {
        captureButton.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if(captureButton.action.WasPressedThisFrame()){
            Debug.Log("capture button was pressed");
            TakeScreenshot();
            //StartCoroutine(CaptureXRCamera());
        }
    }

    void TakeScreenshot(){
        string screenshotPath = Application.persistentDataPath + "/Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        ScreenCapture.CaptureScreenshot(screenshotPath);
        Debug.Log("Screenshot saved to: " + screenshotPath);
    }

    private System.Collections.IEnumerator CaptureXRCamera()
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;
        RenderTexture rt = new RenderTexture(width, height, 24);
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        xrCamera.targetTexture = rt;
        xrCamera.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        xrCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        string screenshotPath = Application.persistentDataPath + "/Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        System.IO.File.WriteAllBytes(screenshotPath, screenshot.EncodeToPNG());
        Debug.Log("Screenshot saved to: " + screenshotPath);
    }
}
