using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Countdown : MonoBehaviour
{
    public float countdownTime = 6f; // Time in seconds for the countdown
    private GameObject CountdownCanvas;
    private GameObject countdown;
    private TextMeshProUGUI countdownText; // Reference to the UI text element

    private float currentTime = 0f;
    private bool isCountdownStarted = false;
    private bool isFirstScene = true;

    private Manager Manager;
    GameObject CanvasPanel;
    private GameObject FOV;
    private Image FOV_Image;
    private GameObject mainCamera;

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

        currentTime = countdownTime;

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name != "Main_Menu_HMD")
        {
            countdown = GameObject.FindGameObjectWithTag("CountdownText");
            CountdownCanvas = GameObject.FindGameObjectWithTag("CountdownCanvas");
            CanvasPanel = GameObject.FindGameObjectWithTag("Panel");
            FOV = GameObject.Find("FOV");
            FOV_Image = FOV.GetComponentInChildren<Image>();
            FOV_Image.enabled = true;


            if (isFirstScene)
            {
                if (countdown != null)
                {
                    CountdownCanvas.GetComponent<Canvas>().enabled = true;
                    countdownText = countdown.GetComponent<TextMeshProUGUI>();
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

        if (isCountdownStarted)
        {
            currentTime -= Time.deltaTime;

            //Show Countdown value - starts at 5 seconds.
            if ( currentTime <= 5f ) { countdownText.text = currentTime.ToString("0"); }
            
            //When Countdown reaches 0 sec, show the Start text and activate XR Controller.
            if (currentTime < 1 && !Manager.isLastScene) 
            { 
                countdownText.text = "Start!";
            }
            
            //When everything is set to start
            if (currentTime <= 0 && !Manager.isLastScene)
            {
                CountdownCanvas.GetComponent<Canvas>().enabled = false;
                CanvasPanel.GetComponent<Image>().enabled = false;
                if(Manager != null)
                {
                    Manager.StartTimer();    
                }

                StopCountdown();
            }
            else if(currentTime <= 0 && Manager.isLastScene)
            {
                SceneManager.LoadScene("Main_Menu_HMD");
                FOV_Image.enabled = false;
                StopCountdown();
            }

        }
        if (Manager.isLastScene && !isCountdownStarted)
        {
            CountdownCanvas.GetComponent<Canvas>().enabled = true;
            CanvasPanel.GetComponent<Image>().enabled = true;
            countdownText = countdown.GetComponent<TextMeshProUGUI>();
            countdownText.enabled = true;
            countdownText.text = "Finishing in...";
            StartCountdown();
        }
    }

    public void StartCountdown()
    {
        ResetCountdown();
        isCountdownStarted = true;
    }

    public void StopCountdown()
    {
        Manager.isLastScene = false;
        isCountdownStarted = false;
        isFirstScene = false;
        countdownText.enabled = false;
    }

    public void ResetCountdown()
    {
        currentTime = countdownTime;
    }
}
