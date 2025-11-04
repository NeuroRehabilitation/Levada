using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class PictureTaking : MonoBehaviour
{

    public InputActionProperty showCameraButton;
    public InputActionProperty captureButton;
    public GameObject hand;
    public GameObject flashOverlay;


    private Shader depthShader;
    private bool canTakePicture = false;
    private bool isNonWalk = false;


    void OnEnable()
    {
        showCameraButton.action.Enable();
        captureButton.action.Enable();
    }

    void OnDisable()
    {
        showCameraButton.action.Disable();
        captureButton.action.Disable();
    }

    void Awake()
    {
        hand.transform.GetChild(0).gameObject.SetActive(false);
        if(hand.GetComponent<XRInteractorLineVisual>().enabled)
            isNonWalk = true;
    }

    void Update()
    {
        if (showCameraButton.action.WasPressedThisFrame())
        {
            hand.transform.GetChild(0).gameObject.SetActive(true);
            if (isNonWalk)
            {
                hand.GetComponent<XRRayInteractor>().enabled = false;
                hand.GetComponent<XRInteractorLineVisual>().enabled = false;
            }
            canTakePicture = true;
        }
        else if (showCameraButton.action.WasReleasedThisFrame())
        {
            hand.transform.GetChild(0).gameObject.SetActive(false);
            if (isNonWalk)
            {
                hand.GetComponent<XRRayInteractor>().enabled = true;
                hand.GetComponent<XRInteractorLineVisual>().enabled = true;
            }
            canTakePicture = false;
        }

        if (canTakePicture && captureButton.action.WasPressedThisFrame())
        {
            Debug.Log("capture button was pressed");
            StartCoroutine(CaptureFromHandView());
        }
    }

    private IEnumerator CaptureFromHandView()
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;

        GameObject tempCamGO = new GameObject("TempScreenshotCam");
        Camera tempCam = tempCamGO.AddComponent<Camera>();

        string baseFilename = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        TakePicture(width, height, tempCamGO, tempCam, baseFilename);
        CreateDepthMap(width, height, tempCamGO, tempCam, baseFilename);

        saveToJson(DetectObjects(), baseFilename);
    }

    private void TakePicture(int width, int height, GameObject tempCamGO, Camera tempCam, string baseFilename)
    {

        tempCamGO.transform.position = hand.transform.position;
        tempCamGO.transform.rotation = hand.transform.rotation;

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

        StartCoroutine(SimulateSnapEffect());

        string screenshotPath = Application.persistentDataPath + "/" + baseFilename + ".png";
        System.IO.File.WriteAllBytes(screenshotPath, screenshot.EncodeToPNG());

        Debug.Log("Screenshot saved from hand view to: " + screenshotPath);
    }

    private IEnumerator SimulateSnapEffect()
    {
        Renderer renderer = flashOverlay.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material overlayMat = renderer.material;

            Color originalColor = overlayMat.color;
            float flashAlpha = 1f;
            float fadeDuration = 0.2f;

            overlayMat.color = new Color(originalColor.r, originalColor.g, originalColor.b, flashAlpha);
            yield return new WaitForSeconds(0.05f);

            float timer = 0f;
            while (timer < fadeDuration)
            {
                float alpha = Mathf.Lerp(flashAlpha, 0f, timer / fadeDuration);
                overlayMat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }

            overlayMat.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }
    }

    private void CreateDepthMap(int width, int height, GameObject tempCamGO, Camera tempCam, string baseFilename)
    {
        depthShader = Shader.Find("Hidden/CustomDepth");
        if (depthShader == null)
        {
            Debug.LogError("Custom depth shader not found!");
            return;
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
    }

    private List<DetectedObject> DetectObjects()
    {
        float detectionDistance = 10f;
        float coneAngle = 65f;
        List<DetectedObject> detectedObjects = new List<DetectedObject>();

        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            TerrainData data = terrain.terrainData;
            Vector3 terrainPosition = terrain.transform.position;

            // Detect Trees
            foreach (TreeInstance tree in data.treeInstances)
            {
                Vector3 worldTreePos = Vector3.Scale(tree.position, data.size) + terrainPosition;
                float distance = Vector3.Distance(hand.transform.position, worldTreePos);

                if (distance <= detectionDistance)
                {
                    Vector3 directionToTree = worldTreePos - hand.transform.position;
                    directionToTree.y = 0;
                    Vector3 forwardFlat = hand.transform.forward;
                    forwardFlat.y = 0;

                    float angle = Vector3.Angle(forwardFlat.normalized, directionToTree.normalized);
                    if (angle <= coneAngle)
                    {
                        string treeName = "Tree";
                        int prototypeIndex = tree.prototypeIndex;
                        if (prototypeIndex >= 0 && prototypeIndex < data.treePrototypes.Length)
                        {
                            var prefab = data.treePrototypes[prototypeIndex].prefab;
                            if (prefab != null) treeName = prefab.name;
                        }

                        Vector3 relativePosition = hand.transform.InverseTransformPoint(worldTreePos);

                        detectedObjects.Add(new DetectedObject
                        {
                            name = treeName,
                            relativePosition = relativePosition,
                            worldPosition = worldTreePos,
                            distance = distance
                        });
                    }
                }
            }

            // Detect Details
            int detailWidth = data.detailWidth;
            int detailHeight = data.detailHeight;
            float cellSizeX = data.size.x / detailWidth;
            float cellSizeZ = data.size.z / detailHeight;

            for (int layer = 0; layer < data.detailPrototypes.Length; layer++)
            {
                int[,] layerMap = data.GetDetailLayer(0, 0, detailWidth, detailHeight, layer);
                string detailName = data.detailPrototypes[layer].prototype != null
                                    ? data.detailPrototypes[layer].prototype.name
                                    : $"Detail_{layer}";

                for (int x = 0; x < detailWidth; x++)
                {
                    for (int y = 0; y < detailHeight; y++)
                    {
                        if (layerMap[y, x] > 0)
                        {
                            Vector3 worldDetailPos = new Vector3(
                                x * cellSizeX + terrainPosition.x,
                                terrain.SampleHeight(new Vector3(x * cellSizeX, 0, y * cellSizeZ) + terrainPosition),
                                y * cellSizeZ + terrainPosition.z
                            );

                            float distance = Vector3.Distance(hand.transform.position, worldDetailPos);
                            if (distance <= detectionDistance)
                            {
                                Vector3 directionToDetail = worldDetailPos - hand.transform.position;
                                directionToDetail.y = 0;
                                Vector3 forwardFlat = hand.transform.forward;
                                forwardFlat.y = 0;

                                float angle = Vector3.Angle(forwardFlat.normalized, directionToDetail.normalized);
                                if (angle <= coneAngle)
                                {
                                    Vector3 relativePosition = hand.transform.InverseTransformPoint(worldDetailPos);

                                    detectedObjects.Add(new DetectedObject
                                    {
                                        name = detailName,
                                        relativePosition = relativePosition,
                                        worldPosition = worldDetailPos,
                                        distance = distance
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        return detectedObjects;
    }


    private void saveToJson(List<DetectedObject> detectedNames, string baseFilename)
    {
        PictureMetadata metadata = new PictureMetadata
        {
            timestamp = ((System.DateTimeOffset)System.DateTime.Now).ToUnixTimeMilliseconds(),
            scene = SceneManager.GetActiveScene().name,
            position = hand.transform.position,
            eulerRotation = hand.transform.rotation.eulerAngles,
            forwardVector = hand.transform.forward,
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
    public long timestamp;
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
    public Vector3 worldPosition;
    public float distance;
}
