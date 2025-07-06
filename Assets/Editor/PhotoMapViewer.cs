using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PhotoMapViewer : EditorWindow
{
    private GameObject markerPrefab;
    private Camera mapCamera;
    private List<GameObject> spawnedMarkers = new List<GameObject>();
    private const int resolution = 1024;

    [MenuItem("Tools/Photo Map Viewer")]
    public static void ShowWindow()
    {
        GetWindow<PhotoMapViewer>("Photo Map Viewer");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Generate Top-Down Map"))
        {
            LoadPhotoPositions();
            CreateTempMapCamera();
            TakeTopDownScreenshot();
            Cleanup();
        }
    }

    void LoadPhotoPositions()
    {
        markerPrefab = Resources.Load<GameObject>("PhotoMarker");
        if (markerPrefab == null)
        {
            Debug.LogError("PhotoMarker prefab not found in Resources.");
            return;
        }

        string dataFolder = Application.dataPath + "/PhotoData";
        if (!Directory.Exists(dataFolder))
        {
            Debug.LogError("PhotoData folder not found: " + dataFolder);
            return;
        }

        string[] files = Directory.GetFiles(dataFolder, "*.json");
        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            PictureMetadata metadata = JsonUtility.FromJson<PictureMetadata>(json);

            GameObject marker = PrefabUtility.InstantiatePrefab(markerPrefab) as GameObject;
            Vector3 elevatedPosition = metadata.position + new Vector3(0f, 50f, 0f);
            marker.transform.position = elevatedPosition;
            marker.transform.rotation = Quaternion.LookRotation(metadata.forwardVector);
            marker.name = "PhotoMarker_" + metadata.timestamp.Replace(":", "-");
            spawnedMarkers.Add(marker);
        }

        Debug.Log("Loaded " + spawnedMarkers.Count + " photo positions.");
    }

    void CreateTempMapCamera()
    {
        GameObject camGO = new GameObject("TempMapCamera");
        mapCamera = camGO.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = 170f;
        mapCamera.transform.position = new Vector3(380f, 600f, 300f);  // for this scene (change for others)
        mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.backgroundColor = Color.gray;
        mapCamera.cullingMask = ~0;
    }

    void TakeTopDownScreenshot()
    {
        RenderTexture rt = new RenderTexture(resolution, resolution, 24);
        Texture2D screenshot = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);

        mapCamera.targetTexture = rt;
        mapCamera.Render();

        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        screenshot.Apply();

        string path = Application.dataPath + "/PhotoData/TopDownMap_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        File.WriteAllBytes(path, screenshot.EncodeToPNG());

        Debug.Log("Top-down map saved to: " + path);

        mapCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        DestroyImmediate(screenshot);
    }

    void Cleanup()
    {
        foreach (var marker in spawnedMarkers)
        {
            if (marker != null)
                DestroyImmediate(marker);
        }
        spawnedMarkers.Clear();

        if (mapCamera != null)
            DestroyImmediate(mapCamera.gameObject);
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
