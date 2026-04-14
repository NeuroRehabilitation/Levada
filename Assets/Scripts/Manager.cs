using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;

public class Manager : MonoBehaviour
{
    [Header("Scenes")]
    public List<int> Scenes;
    public List<string> currentScene = new List<string>();

    [Header("Selected Scene Index")]
    private int randomIndex;

    [Header("Number of Rounds")]
    private int NumberRounds = 1;
    private int currentRound = 1;

    [Header("Duration (minutes)")]
    public float duration = 0.2f; //duration of experiment in minutes.
    public static float elapsed_time = 0f;
    private float startTime;
    public static bool isRunning = false;
    private bool sceneChangeInProgress = false;

    [Header("CSV")]
    public CSV CSV_writer;

    [Header("SAM")]
    public Canvas SAM_Canvas;
    public string[] SAM_answers = new string[2];
    //public string[] VAS_answers = new string[4];
    public string[] DataToSave;

    [Header("User ID")]
    public TMP_InputField UserID;

    [Header("Start Button")]
    public Button StartButton_Desktop;
    public Button StartButton_HMD;

    [Header("LSL Streams")]
    public LSLStreamer SAM;
    public LSLStreamer Markers;

    private Vector3 lastWaypoint;
    private Vector3 XROrigin;
    GameObject[] waypoints;
    private GameObject mainCamera;

    [Header("Game Variable")]
    public GameObject FOV;
    private Image FOV_Image;
    public ImageScaler imageScaler;
    public float FOV_multiplier;

    private static Manager instance;
    private LSLInput LSLInput;
    private float lastGameVariable = 0.0f;

    public static bool isLastScene = false;
    private bool timerStarted = false;
    private bool startedLSL = false;

    private Logger logger;

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


        CreateList();

        CSV_writer = GetComponent<CSV>();

        if (StartButton_Desktop != null)
            StartButton_Desktop.interactable = false;
        if (StartButton_HMD != null)
            StartButton_HMD.interactable = false;
    }

    private void Start()
    {

        logger = GameObject.Find("Logger").GetComponent<Logger>();

        if (logger == null)
        {
            Debug.Log("Manager: No Logger component found in scene.");
        }
        else
        {
            Debug.Log("Logger component found.");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (Scenes.Count > 0) { Shuffle(); }

        if (CSV_writer != null)
            CSV_writer.AddData("Scene", "Valence", "Arousal");

        if (SAM != null)
            SAM.StartStream();
        //VAS.StartStream();
        if (Markers != null)
            Markers.StartStream();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main_Menu_HMD")
        {
            StartCoroutine(CheckXRPosition());

            LSLInput = GameObject.FindObjectOfType<LSLInput>();

            if (FOV != null)
                FOV_Image = FOV.GetComponentInChildren<Image>();

            if (imageScaler != null)
                FOV_multiplier = imageScaler.current_Multiplier;

            waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

            if (waypoints.Length > 0)
            {
                lastWaypoint = waypoints[waypoints.Length - 1].gameObject.transform.position;
            }

            GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (GameObject camera in cameras)
            {
                if (camera.GetComponent<Camera>().clearFlags == CameraClearFlags.Skybox)
                {
                    mainCamera = camera;
                }
            }

            if (mainCamera != null)
            {
                LayerMask layerMask = -1; //Layer "Everything"
                mainCamera.GetComponent<Camera>().cullingMask = layerMask;
                mainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
            }
        }
    }


    private void Update()
    {

        //Debug.Log("FPS: " + (int)(1 / Time.unscaledDeltaTime));//fps

        if (logger == null)
        {
            logger = GameObject.Find("Logger").GetComponent<Logger>();
        }

        if (isRunning && !timerStarted)
        {
            StartCoroutine(TimerCoroutine());
            timerStarted = true;
        }

        if (SceneManager.GetActiveScene().name != "Main_Menu_HMD" && !startedLSL)
        {
            StartCoroutine(UpdateGameVariable());
            startedLSL = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "HMD or KAVE" && SceneManager.GetActiveScene().name != "Menu")
            {
                logger.Log("Exited scene", "Info");
                Cursor.visible = true;
                SceneManager.LoadScene("HMD or KAVE");
                bool sceneExists = SceneUtility.GetBuildIndexByScenePath("HMD or KAVE") != -1;
                if (sceneExists)
                    SceneManager.LoadScene("HMD or KAVE");
                else
                    SceneManager.LoadScene("Menu");
                //FOV_Image.enabled = false;
            }
            else
            {
                logger.Log("Quit application", "Info");
                Quit();
            }
        }

        //else if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    SceneManager.LoadScene("25_Fontes_HMD");
        //}

        //else if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    SceneManager.LoadScene("Pico_Areeiro_Ruivo_HMD");
        //}

        //else if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    SceneManager.LoadScene("Sao_Louren�o_HMD");
        //}

        //else if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    SceneManager.LoadScene("Caldeirao_Verde_HMD");
        //}
    }

    private IEnumerator UpdateGameVariable()
    {
        while (true)
        {
            if (LSLInput != null && imageScaler != null)
            {
                yield return new WaitUntil(() => LSLInput != null && LSLInput.GameVariable != lastGameVariable);

                imageScaler.current_Multiplier += LSLInput.GameVariable;
                lastGameVariable = LSLInput.GameVariable;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (isRunning && elapsed_time <= duration * 60)
        {
            elapsed_time = Time.realtimeSinceStartup - startTime;
            yield return null;
        }

        currentScene.Clear();
        currentScene.Add("end");
        currentScene.Add("0");
        Markers.StreamData(currentScene.ToArray());
        StopTimer();
    }

    private IEnumerator CheckXRPosition()
    {
        while (true)
        {
            yield return new WaitUntil(() =>
            {
                if (lastWaypoint != null)
                {
                    XROrigin = GameObject.Find("XR Origin").transform.position;
                }
                float distance = Vector3.Distance(XROrigin, lastWaypoint);

                return distance <= 1.5f && SAM_Canvas.enabled == false;

            });
            StopCoroutine(CheckXRPosition());
            currentScene.RemoveAt(currentScene.Count - 1);
            currentScene.Add("1");
            Markers.StreamData(currentScene.ToArray());
            ChangeScene();
            yield break;
        }
    }
    public void StartTimer()
    {
        isRunning = true;
        startTime = Time.realtimeSinceStartup;
    }

    public void StopTimer()
    {
        timerStarted = false;
        isRunning = false;
        isLastScene = true;
        ResetTimer();
    }

    private void ResetTimer()
    {
        elapsed_time = 0;
    }

    public void ActivateButton()
    {
        if (UserID.text != "")
        {
            StartButton_Desktop.interactable = true;
            StartButton_HMD.interactable = true;
        }

        else
        {
            StartButton_Desktop.interactable = false;
            StartButton_HMD.interactable = false;
        }
    }

    public void GetUserID()
    {
        CSV_writer.filename = UserID.text;
        CSV_writer.filePath = CSV_writer.directory + CSV_writer.filename + ".csv";
    }

    private void Shuffle()
    {
        System.Random random = new System.Random();
        randomIndex = random.Next(Scenes.Count);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(Scenes[randomIndex]);
        currentScene.Add(SceneManager.GetSceneByBuildIndex(Scenes[randomIndex]).name);
        currentScene.Add("0");
        Markers.StreamData(currentScene.ToArray());
        Scenes.RemoveAt(randomIndex);
    }

    public void SelectScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    private void CreateList()
    {
        Scenes = Enumerable.Range(1, SceneManager.sceneCountInBuildSettings - 1).ToList();
    }

    public void WriteData()
    {
        string[] row_data = new string[3];
        row_data[0] = currentScene[0];
        row_data[1] = SAM_answers[0];
        row_data[2] = SAM_answers[1];

        DataToSave = row_data;
        CSV_writer.AddData(DataToSave);
    }

    public void ChangeScene()
    {
        if (!isLastScene)
        {
            currentScene.Clear();

            if (Scenes.Count > 0)
            {
                Shuffle();
                LoadScene();
                int layerIndex = LayerMask.NameToLayer("Nothing");
                LayerMask layerMask = 1 << layerIndex;
                mainCamera.GetComponent<Camera>().cullingMask = layerMask;
                mainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
                mainCamera.GetComponent<Camera>().backgroundColor = Color.black;
            }
            else
            {
                currentRound++;
                CreateList();
                Shuffle();
                LoadScene();
            }
        }
    }

    public void Quit()
    {

        //Comment this line below when you build the project
        //UnityEditor.EditorApplication.isPlaying = false;
        StopAllCoroutines();
        /*CSV_writer.WriteToCSV();
        CSV_writer.CloseCSV();
        SAM.StopStream();
        //VAS.StopStream();
        Markers.StopStream();*/

        Application.Quit();
    }
}
