using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.ProBuilder.Shapes;

public class Logger : MonoBehaviour
{
    public GameObject player;

    private static List<LogEntry> logEntries = new List<LogEntry>();
    private static string logFilePath;
    private string sceneName;
    private float sceneStartTime;
    private static int logFileIndex;

    void Awake()
    {
        string logDirectory = Application.persistentDataPath;
        if (logFileIndex == 0) // Only set once per runtime
        {
            string[] logFiles = Directory.GetFiles(logDirectory, "log*.json");
            logFileIndex = logFiles.Length + 1;
        }
    }

    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        sceneStartTime = Time.time;
        if (string.IsNullOrEmpty(logFilePath))
        {
            logFilePath = Path.Combine(Application.persistentDataPath, $"log{logFileIndex:D3}.json");
        }
        Debug.Log("Log file path: " + logFilePath);
        Log($"Scene '{sceneName}' started", "Info");
    }

    public void Log(string message, string logType)
    {
        LogEntry entry = new LogEntry
        {
            timestamp = System.DateTime.Now.ToString("o"),
            message = message,
            logType = logType,
            sceneName = SceneManager.GetActiveScene().name,
            timeInScene = Time.time - sceneStartTime,
            totalTime = Time.timeSinceLevelLoad,
            playerPosition = player.transform.position,
            playerRotation = player.transform.rotation.eulerAngles
        };
        if (player.name == "Cube")
        {
            entry.lastWaypoint = (player.GetComponent<AI>().next - 1).ToString();
        }
        logEntries.Add(entry);
        SaveToJson();
    }

    private void SaveToJson()
    {
        string json = JsonUtility.ToJson(new LogWrapper { logs = logEntries }, true);
        File.WriteAllText(logFilePath, json);
        Debug.Log("Log updated: " + logFilePath);
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
    public string timestamp;
    public string message;
    public string logType;
    public string sceneName;
    public float timeInScene;
    public float totalTime;
    public Vector3 playerPosition;
    public Vector3 playerRotation;
    public string lastWaypoint = "N/A";
}