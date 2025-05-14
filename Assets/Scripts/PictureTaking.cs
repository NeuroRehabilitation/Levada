using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PictureTaking : MonoBehaviour
{

    public InputActionProperty captureButton;
    public Transform handTransform;


    private Shader depthShader;


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

        //Depth map here
        depthShader = Shader.Find("Hidden/CustomDepth");
        if (depthShader == null)
        {
            Debug.LogError("Custom depth shader not found!");
            yield break;
        }

        RenderTexture depthRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D depthMap = new Texture2D(width, height, TextureFormat.RGB24, false);

        tempCam.targetTexture = depthRT;
        tempCam.clearFlags = CameraClearFlags.SolidColor;
        tempCam.backgroundColor = Color.white;
        tempCam.cullingMask = -1; 
        tempCam.RenderWithShader(depthShader, "");

        RenderTexture.active = depthRT;
        depthMap.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        depthMap.Apply();

        string depthPath = Application.persistentDataPath + "/" + baseFilename + "_depth.png";
        System.IO.File.WriteAllBytes(depthPath, depthMap.EncodeToPNG());

        RenderTexture.active = null;
        depthRT.Release();
        Destroy(depthRT);
        Destroy(depthMap);


        //Detect terrain here
        float detectionDistance = 10f;
        float coneAngle = 90f;
        List<DetectedObject> detectedNames = new List<DetectedObject>();

        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            TerrainData data = terrain.terrainData;
            Vector3 terrainPosition = terrain.transform.position;

            foreach (TreeInstance tree in data.treeInstances)
            {
                Vector3 worldTreePos = Vector3.Scale(tree.position, data.size) + terrainPosition;

                float distance = Vector3.Distance(handTransform.position, worldTreePos);
                if (distance <= detectionDistance)
                {
                    Vector3 directionToTree = worldTreePos - handTransform.position;
                    directionToTree.y = 0;
                    Vector3 forwardFlat = handTransform.forward;
                    forwardFlat.y = 0;

                    float angle = Vector3.Angle(forwardFlat.normalized, directionToTree.normalized);
                    if (angle <= coneAngle)
                    {
                        //Debug.DrawLine(handTransform.position, worldTreePos, Color.green, 2f);

                        string treeName = "Tree";
                        int prototypeIndex = tree.prototypeIndex;
                        if (prototypeIndex >= 0 && prototypeIndex < data.treePrototypes.Length)
                        {
                            var prefab = data.treePrototypes[prototypeIndex].prefab;
                            if (prefab != null) treeName = prefab.name;
                        }

                        Vector3 relativePosition = handTransform.InverseTransformPoint(worldTreePos);

                        detectedNames.Add(new DetectedObject
                        {
                            name = treeName,
                            relativePosition = relativePosition,
                            distance = distance
                        });
                    }
                }
            }
        }

        PictureMetadata metadata = new PictureMetadata
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            scene = SceneManager.GetActiveScene().name,
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
    public string scene;
    public Vector3 position;
    public Vector3 eulerRotation;
    public Vector3 forwardVector;
    public DetectedObject[] detectedObjects;
}

[System.Serializable]
public class DetectedObject
{
    public string name;
    public Vector3 relativePosition;
    public float distance;
}
