using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Countdown : MonoBehaviour
{
    public float countdownTime = 6f; // Time in seconds for the countdown
    private TextMeshProUGUI countdownText; // Reference to the UI text element

    private float currentTime = 0f;
    private bool isCountdownStarted = false;
    private bool isFirstScene = true;

    private Manager Manager;
    GameObject CanvasPanel;
    private GameObject FOV;
    private Image FOV_Image;

    private static Countdown instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {Destroy(gameObject);}
            
        Manager = FindObjectOfType<Manager>();
    }
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if(Manager != null)
        {
            FOV = GameObject.Find("FOV");
            FOV_Image = FOV.GetComponentInChildren<Image>();
            FOV_Image.enabled = true;

        }

        currentTime = countdownTime;

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name != "Main_Menu_HMD")
        {
            countdownText = GameObject.FindObjectOfType<TextMeshProUGUI>();
            CanvasPanel = GameObject.FindGameObjectWithTag("Panel");
            if (isFirstScene)
            {
                if (countdownText != null)
                {
                    countdownText.enabled = true;
                    countdownText.text = "Prepare to Start!";
                }
                if (CanvasPanel != null)
                {
                    CanvasPanel.GetComponent<Image>().enabled = true;
                }
                StartCountdown();
            }
        }
    }

    private void Update()
    {

        if (isCountdownStarted && isFirstScene)
        {

            currentTime -= Time.deltaTime;


            //Show Countdown value - starts at 5 seconds.
            if ( currentTime <= 5f ) { countdownText.text = currentTime.ToString("0"); }
            
            //When Countdown reaches 0 sec, show the Start text and activate XR Controller.
            if (currentTime < 1) 
            { 
                countdownText.text = "Start!";
            }
            
            //When everything is set to start
            if (currentTime <= 0)
            {
                CanvasPanel.GetComponent<Image>().enabled = false;
                if(Manager != null)
                {
                    Manager.currentScene.Add("0");
                    Manager.Markers.StreamData(Manager.currentScene.ToArray());
                }

                StopCountdown();
            }
        }
    }

    public void StartCountdown()
    {
        //ResetCountdown();
        isCountdownStarted = true;
        isFirstScene = true;
    }

    public void StopCountdown()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        isCountdownStarted = false;
        isFirstScene = false;
        currentTime = 0;
        countdownText.gameObject.SetActive(false);
    }

    public void ResetCountdown()
    {
        currentTime = countdownTime;
    }
}
