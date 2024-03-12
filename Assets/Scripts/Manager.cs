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
    public int randomIndex;

    [Header("Number of Rounds")]
    public int NumberRounds = 1;
    public int currentRound = 1;

    [Header("Duration (minutes)")]
    public float duration = 0.2f; //duration of experiment in minutes.

    [Header("CSV")]
    public CSV CSV_writer;

    [Header("Scales Answers")]
    public string[] SAM_answers = new string[2];
    public string[] VAS_answers = new string[4];
    public string[] DataToSave;

    [Header("User ID")]
    public TMP_InputField UserID;

    [Header("Start Button")]
    public Button StartButton;

    [Header("LSL Streams")]
    public LSLStreamer SAM;
    public LSLStreamer VAS;
    public LSLStreamer Markers;

    private Vector3 lastWaypoint;

    [Header("Game Variable")]
    private GameObject FOV;
    private Image FOV_Image;
    private float FOV_multiplier;

    private static Manager instance;
    private static LSLInput LSLInput;


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

        //CSV_writer = GetComponent<CSV>();

        if (StartButton != null)
            StartButton.interactable = false;
    }

    private void Start()
    {
        if (Scenes.Count > 0) { Shuffle(); }

        //CSV_writer.AddData("Scene", "Valence", "Arousal", "Anger", "Fear", "Joy", "Sad");

        SAM.StartStream();
        VAS.StartStream();
        Markers.StartStream();

        FOV = GameObject.Find("FOV");
        FOV_Image = FOV.GetComponentInChildren<Image>();
        FOV_multiplier = FOV.GetComponentInChildren<ImageScaler>().FOV_Multiplier;
        FOV_Image.enabled = false;

        StartCoroutine(CheckXRPosition());

    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "Main_Menu_HMD")
            UpdateGameVariable();

        if (SceneManager.GetActiveScene().name != "Main_Menu_HMD" && currentRound > NumberRounds)
        {
            SceneManager.LoadScene("Main_Menu_HMD");
            FOV_Image.enabled = false;
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "Main_Menu_HMD")
            {
                SceneManager.LoadScene("Main_Menu_HMD");
                FOV_Image.enabled = false;
            }
            else
                Quit();
        }

        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene("25_Fontes_HMD");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("Pico_Areeiro_Ruivo_HMD");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadScene("Sao_Lourenço_HMD");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.LoadScene("Caldeirao_Verde_HMD");
        }
    }

    private void UpdateGameVariable()
    {
        
        //float updatedGameVariable = LSLInput.GameVariable;
        float updatedGameVariable = FOV_multiplier + 0.0001f;

        if (FOV_multiplier != updatedGameVariable)
        {
            FOV_multiplier = updatedGameVariable;
        }
    }

    private IEnumerator CheckXRPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.0f);

            GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

            if (waypoints.Length > 0)
            {
                lastWaypoint = waypoints[waypoints.Length - 1].gameObject.transform.position;
                Debug.Log(lastWaypoint);
            }

            if(lastWaypoint != null)
            {
                if(GameObject.Find("XR Origin").transform.position == lastWaypoint)
                {
                    Debug.Log("End of Levada.");
                }
            }

        }
    }

    private void ActivateButton()
    {
        if (UserID.text != "")
            StartButton.interactable = true;
        else StartButton.interactable = false;
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
        Scenes.RemoveAt(randomIndex);
    }

    public void SelectScene(string SceneName)
    {
        //Destroy(gameObject);
        SceneManager.LoadScene(SceneName);
    }

    private void CreateList()
    {
        Scenes = Enumerable.Range(1, SceneManager.sceneCountInBuildSettings-1).ToList();
    }

    public void WriteData()
    {

        DataToSave = currentScene.Concat(SAM_answers.Concat(VAS_answers).ToArray()).ToArray();
        CSV_writer.AddData(DataToSave);
        currentScene.Clear();
    }

    public void ChangeScene()
    {
        if (Scenes.Count > 0)
        {
            Shuffle();
            LoadScene();

        }
        else
        {
            currentRound++;
            CreateList();
            Shuffle();
            LoadScene();
        }
    }

    public void Quit()
    {

        //Comment this line below when you build the project
        UnityEditor.EditorApplication.isPlaying = false;

        SAM.StopStream();
        VAS.StopStream();
        Markers.StopStream();

        Application.Quit();
    }
}
