using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Windows.Kinect;

public class Logger : MonoBehaviour
{
    public GameObject player;
    public int logEveryNFrames = 1;
    [Header("Kinect logging")]
    public KinectBodySource2M kinectBodySource;
    public Transform kinectTransform;
    public bool kinectLoggingEnabled = true;
    public bool autoSelectFirstTrackedBody = true;
    [Range(0, 5)]
    public int kinectBodyIndex = 0;

    private List<LogEntry> logEntries = new List<LogEntry>();
    private string logFilePath;
    private string sceneName;
    private float sceneStartTime;
    private int _frameCounter = 0;
    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        sceneStartTime = Time.time;

        logEntries.Clear();

        string logDirectory = Application.persistentDataPath;
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        logFilePath = Path.Combine(logDirectory, $"log_{sceneName}_{timestamp}.json");

        Debug.Log("Log file path: " + logFilePath);
        Log($"Scene '{sceneName}' started", "Info");
    }

    void Update()
    {
        _frameCounter++;
        if (logEveryNFrames <= 1 || (_frameCounter % Mathf.Max(1, logEveryNFrames) == 0))
        {
            LogPlayerState();
        }
    }

    private void LogPlayerState()
    {
        Log("Auto-log player state", "PlayerState");
    }

    public void Log(string message, string logType)
    {
        LogEntry entry = new LogEntry
        {
            timestamp = ((System.DateTimeOffset)System.DateTime.Now).ToUnixTimeMilliseconds(),
            message = message,
            logType = logType,
            sceneName = SceneManager.GetActiveScene().name,
            timeInScene = Time.time - sceneStartTime,
            transforms = CaptureAllTransforms(),
            kinectJoints = kinectLoggingEnabled ? CaptureKinectJointPositions() : null
        };
        if (player.name == "Cube")
        {
            AI aiComponent = player.GetComponent<AI>();
            if (aiComponent != null)
            {
                entry.lastWaypoint = (aiComponent.next - 1).ToString();
            }
        }
        logEntries.Add(entry);
        SaveToJson();
    }

    private List<TransformData> CaptureAllTransforms()
    {
        List<TransformData> transforms = new List<TransformData>
        {
            new TransformData
            {
                name = player.name,
                position = player.transform.position,
                rotation = player.transform.rotation,
                localPosition = player.transform.localPosition,
                localRotation = player.transform.localRotation
            }
        };

        CaptureChildTransforms(player.transform, transforms);

        return transforms;
    }

    private void CaptureChildTransforms(Transform parent, List<TransformData> transforms)
    {
        foreach (Transform child in parent)
        {
            if (!child.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (child.name == "waypoints new")
            {
                continue;
            }

            transforms.Add(new TransformData
            {
                name = child.name,
                position = child.position,
                rotation = child.rotation,
                localPosition = child.localPosition,
                localRotation = child.localRotation
            });

            if (child.childCount > 0)
            {
                CaptureChildTransforms(child, transforms);
            }
        }
    }

    private void SaveToJson()
    {
        string json = JsonUtility.ToJson(new LogWrapper { logs = logEntries }, true);
        File.WriteAllText(logFilePath, json);
        Debug.Log("Log updated: " + logFilePath);
    }

    private List<JointData> CaptureKinectJointPositions()
    {
        if (kinectBodySource == null)
        {
            kinectBodySource = FindObjectOfType<KinectBodySource2M>();
        }
        if (kinectBodySource == null)
        {
            return null;
        }
        if (kinectTransform == null)
        {
            if (kinectBodySource.transform.parent != null)
            {
                kinectTransform = kinectBodySource.transform.parent;
            }
        }

        Body[] bodies = kinectBodySource.GetData();
        if (bodies == null || bodies.Length == 0)
        {
            return null;
        }

        Body body = null;
        if (autoSelectFirstTrackedBody)
        {
            foreach (var b in bodies)
            {
                if (b != null && b.IsTracked)
                {
                    body = b;
                    break;
                }
            }
        }
        else
        {
            int idx = Mathf.Clamp(kinectBodyIndex, 0, bodies.Length - 1);
            if (bodies[idx] != null && bodies[idx].IsTracked)
            {
                body = bodies[idx];
            }
        }

        if (body == null)
        {
            return null;
        }

        var list = new List<JointData>(25);

        System.Func<Vector3, Vector3> sensorToUnity = v => new Vector3(-v.x, v.y, v.z);

        foreach (JointType jt in System.Enum.GetValues(typeof(JointType)))
        {
            Windows.Kinect.Joint j = body.Joints[jt];
            Vector3 sensorPos = new Vector3(j.Position.X, j.Position.Y, j.Position.Z);

            Vector3 unityPos = sensorToUnity(sensorPos);
            Vector3 worldPos = (kinectTransform != null)
                ? kinectTransform.position + kinectTransform.rotation * unityPos
                : unityPos;

            list.Add(new JointData
            {
                joint = jt.ToString(),
                trackingState = j.TrackingState.ToString(),
                sensorPosition = sensorPos,
                worldPosition = worldPos
            });
        }

        return list;
    }

    [System.Serializable]
    private class LogWrapper
    {
        public List<LogEntry> logs;
    }

}

[System.Serializable]
public class LogEntry
{
    public long timestamp;
    public string message;
    public string logType;
    public string sceneName;
    public float timeInScene;
    public List<TransformData> transforms;
    public string lastWaypoint = "N/A";
    public List<JointData> kinectJoints;
}

[System.Serializable]
public class TransformData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localPosition;
    public Quaternion localRotation;
}

[System.Serializable]
public class JointData
{
    public string joint;
    public string trackingState;
    public Vector3 sensorPosition;
    public Vector3 worldPosition;
}