using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [Header("Scenes")]
    public List<int> Scenes;
    public List<string> currentScene = new List<string>();

    [Header("Selected Scene Index")]
    public int randomIndex;

    [Header("Number of Rounds")]
    public int NumberRounds = 2;
    public int currentRound = 1;

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

    void Awake()
    {
        DontDestroyOnLoad(this);
        //CreateList();

        //CSV_writer = GetComponent<CSV>();

        StartButton.interactable = false;
    }

    private void Start()
    {
        //if (Scenes.Count > 0) { Shuffle(); }

        //CSV_writer.AddData("Scene", "Valence", "Arousal", "Anger", "Fear", "Joy", "Sad");

        //SAM.StartStream();
        //VAS.StartStream();
        //Markers.StartStream();

    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape) || currentRound > NumberRounds)
        {
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

    public void ActivateButton()
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

    public void Shuffle()
    {
        System.Random random = new System.Random();
        randomIndex = random.Next(Scenes.Count);
    }

    private void LoadScene()
    {
      
        SceneManager.LoadScene(Scenes[randomIndex]);
        currentScene.Add(SceneManager.GetSceneByBuildIndex(Scenes[randomIndex]).name);
        Scenes.RemoveAt(randomIndex);
    }

    public void SelectScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void CreateList()
    {
        Scenes = Enumerable.Range(1, SceneManager.sceneCountInBuildSettings - 3).ToList();
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
        //CSV_writer.WriteToCSV();
        //CSV_writer.CloseCSV();

        //Comment this line below when you build the project
        //UnityEditor.EditorApplication.isPlaying = false;

        Application.Quit();
    }
}
