using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Windows.Kinect;
using System.Reflection;
using System.Collections;
using System;

public class Logger : MonoBehaviour
{
    public GameObject player;
    [Header("Kinect logging")]
    public KinectBodySource2M kinectBodySource;
    public Transform kinectTransform;
    public bool kinectLoggingEnabled = true;
    public bool autoSelectFirstTrackedBody = true;
    [Range(0, 5)]
    public int kinectBodyIndex = 0;
    [Header("XR Controller logging")]
    public bool xrControllerLoggingEnabled = true;
    public GameObject rightHandControllerObject;
    [Header("Performance Settings")]
    public int logEveryNFrames = 1;
    public int saveEveryNFrames = 60;
    public int maxEntriesBeforeSave = 100;
    public bool useAsyncWrite = true;

    private List<LogEntry> logEntries = new List<LogEntry>();
    private List<string> exceptionTransforms = new List<string>
    {"waypoints new",
    "Canvas",
    "Frame",
    "Abstract Feedback Scale Left",
    "Abstract Feedback Scale Right",
    "[RightHand Controller] Model Parent",
    "[RightHand Controller] Attach",
    "[RightHand Controller] Ray Origin",
    "Trackables" };
    private string logFilePath;
    private string sceneName;
    private float sceneStartTime;
    private int _frameCounter = 0;
    private int _saveCounter = 0;
    private AI _cachedAIComponent;
    private Component _cachedControllerLogger;
    private MethodInfo _cachedGetStateMethod;
    private bool _hasSearchedForController = false;
    private StreamWriter _logStream;
    private bool _isFirstEntry = true;
    private bool _isWriting = false;
    private Queue<string> _writeQueue = new Queue<string>();
    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        sceneStartTime = Time.time;

        logEntries.Clear();

        string logDirectory = Application.persistentDataPath;
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        logFilePath = Path.Combine(logDirectory, $"log_{sceneName}_{timestamp}.json");

        Debug.Log("Log file path: " + logFilePath);

        InitializeLogFile();

        if (player != null && player.name == "Cube")
        {
            _cachedAIComponent = player.GetComponent<AI>();
        }

        CacheControllerReferences();

        Log($"Scene '{sceneName}' started", "Info");
    }

    void OnDestroy()
    {
        FinalizeLogFile();
    }

    void OnApplicationQuit()
    {
        FinalizeLogFile();
    }

    private void InitializeLogFile()
    {
        try
        {
            _logStream = new StreamWriter(logFilePath, false);
            _logStream.WriteLine("{\"logs\":[");
            _isFirstEntry = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize log file: {e.Message}");
        }
    }

    private void FinalizeLogFile()
    {
        if (_logStream != null)
        {
            try
            {
                StopAllCoroutines();

                if (logEntries.Count > 0)
                {
                    bool wasAsync = useAsyncWrite;
                    useAsyncWrite = false;
                    FlushBufferedEntries();
                    useAsyncWrite = wasAsync;
                }

                while (_writeQueue.Count > 0)
                {
                    _logStream.Write(_writeQueue.Dequeue());
                }

                _logStream.WriteLine();
                _logStream.WriteLine("]}");
                _logStream.Close();
                _logStream = null;
                Debug.Log($"Log file finalized: {logFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to finalize log file: {e.Message}");
            }
        }
    }

    void Update()
    {
        _frameCounter++;
        if( SceneManager.GetActiveScene().name == "HMD or KAVE") return;
        else if (logEveryNFrames <= 1 || (_frameCounter % Mathf.Max(1, logEveryNFrames) == 0))
        {
            LogPlayerState();
        }

        _saveCounter++;
        if (_saveCounter >= saveEveryNFrames || logEntries.Count >= maxEntriesBeforeSave)
        {
            FlushBufferedEntries();
            _saveCounter = 0;
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
            timestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds(),
            message = message,
            logType = logType,
            sceneName = sceneName,
            timeInScene = Time.time - sceneStartTime,
            transforms = CaptureAllTransforms(),
            kinectJoints = kinectLoggingEnabled ? CaptureKinectJointPositions() : null,
            xrControllerState = xrControllerLoggingEnabled ? CaptureXRControllerState() : null,
            playerDirection = CapturePlayerDirection()
        };

        if (_cachedAIComponent != null)
        {
            entry.lastWaypoint = (_cachedAIComponent.next - 1).ToString();
        }

        logEntries.Add(entry);
    }

    public void LogXRController(XRControllerState controllerState)
    {
        LogEntry entry = new LogEntry
        {
            timestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds(),
            message = "XR Controller State Update",
            logType = "XRController",
            sceneName = sceneName,
            timeInScene = Time.time - sceneStartTime,
            transforms = CaptureAllTransforms(),
            kinectJoints = kinectLoggingEnabled ? CaptureKinectJointPositions() : null,
            xrControllerState = controllerState,
            playerDirection = CapturePlayerDirection()
        };

        if (_cachedAIComponent != null)
        {
            entry.lastWaypoint = (_cachedAIComponent.next - 1).ToString();
        }

        logEntries.Add(entry);
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
                localPosition = player.transform.localPosition
                //localRotation = player.transform.localRotation,
                //forward = player.transform.forward,
                //right = player.transform.right,
                //up = player.transform.up
            }
        };

        CaptureChildTransforms(player.transform, transforms);

        return transforms;
    }

    private void CaptureChildTransforms(Transform parent, List<TransformData> transforms)
    {
        foreach (Transform child in parent)
        {
            if (!child.gameObject.activeInHierarchy || exceptionTransforms.Contains(child.name))
            {
                continue;
            }

            transforms.Add(new TransformData
            {
                name = child.name,
                position = child.position,
                rotation = child.rotation,
                localPosition = child.localPosition
                //localRotation = child.localRotation,
                //forward = child.forward,
                //right = child.right,
                //up = child.up
            });

            if (child.childCount > 0)
            {
                CaptureChildTransforms(child, transforms);
            }
        }
    }

    private void FlushBufferedEntries()
    {
        if (logEntries.Count == 0 || _logStream == null)
        {
            return;
        }

        try
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(logEntries.Count * 512);

            foreach (var entry in logEntries)
            {
                if (!_isFirstEntry)
                {
                    sb.Append(",\n");
                }
                else
                {
                    _isFirstEntry = false;
                }

                string json = JsonUtility.ToJson(entry, false);
                sb.Append(json);
            }

            string dataToWrite = sb.ToString();

            if (useAsyncWrite)
            {
                _writeQueue.Enqueue(dataToWrite);
                if (!_isWriting)
                {
                    StartCoroutine(AsyncWriteCoroutine());
                }
            }
            else
            {
                _logStream.Write(dataToWrite);
                _logStream.Flush();
            }

            logEntries.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to flush log entries: {e.Message}");
        }
    }

    private IEnumerator AsyncWriteCoroutine()
    {
        _isWriting = true;
        while (_writeQueue.Count > 0)
        {
            string data = _writeQueue.Dequeue();
            _logStream.Write(data);
            _logStream.Flush();
            yield return null;
        }

        _isWriting = false;
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

        Func<Vector3, Vector3> sensorToUnity = v => new Vector3(-v.x, v.y, v.z);

        foreach (JointType jt in Enum.GetValues(typeof(JointType)))
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

    private void CacheControllerReferences()
    {
        if (!_hasSearchedForController && rightHandControllerObject == null)
        {
            rightHandControllerObject = GameObject.Find("RightHand Controller");
            _hasSearchedForController = true;
        }

        if (rightHandControllerObject != null && _cachedControllerLogger == null)
        {
            _cachedControllerLogger = rightHandControllerObject.GetComponent("XRControllerLogger");

            if (_cachedControllerLogger != null && _cachedGetStateMethod == null)
            {
                _cachedGetStateMethod = _cachedControllerLogger.GetType().GetMethod("GetCurrentState");
            }
        }
    }

    private XRControllerState CaptureXRControllerState()
    {
        if (_cachedControllerLogger == null || _cachedGetStateMethod == null)
        {
            CacheControllerReferences();
        }

        if (_cachedGetStateMethod != null)
        {
            return _cachedGetStateMethod.Invoke(_cachedControllerLogger, null) as XRControllerState;
        }

        return null;
    }

    private PlayerDirectionData CapturePlayerDirection()
    {
        if (player == null)
        {
            return null;
        }

        return new PlayerDirectionData
        {
            forward = player.transform.forward,
            right = player.transform.right,
            up = player.transform.up
        };
    }

    [Serializable]
    private class LogWrapper
    {
        public List<LogEntry> logs;
    }

}

[Serializable]
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
    public XRControllerState xrControllerState;
    public PlayerDirectionData playerDirection;
}

[Serializable]
public class TransformData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localPosition;
    //public Quaternion localRotation;
    //public Vector3 forward;
    //public Vector3 right;
    //public Vector3 up;
}

[Serializable]
public class JointData
{
    public string joint;
    public string trackingState;
    public Vector3 sensorPosition;
    public Vector3 worldPosition;
}

[Serializable]
public class XRControllerState
{
    public Vector3 position;
    public Quaternion rotation;
    //public Vector3 forward;
    //public Vector3 right;
    //public Vector3 up;
    public float gripValue;
    public float triggerValue;
    public Vector3 trackedPosition;
    public Quaternion trackedRotation;
    public Vector3 rayOrigin;
    public Vector3 rayDirection;
    public bool isRayHitting;
    public Vector3 rayHitPoint;
    public float rayHitDistance;
    public string rayHitObject;
}

[Serializable]
public class PlayerDirectionData
{
    public Vector3 forward;
    public Vector3 right;
    public Vector3 up;
}