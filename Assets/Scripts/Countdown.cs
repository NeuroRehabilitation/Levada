using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Countdown : MonoBehaviour
{
    public float countdownTime = 6f; // Time in seconds for the countdown
    public TextMeshProUGUI countdownText; // Reference to the UI text element

    private float currentTime = 0f;
    private bool isCountdownStarted = false;

    public TimeController TimeController;
    public GameObject RightHandController;
    private Manager Manager;
    GameObject CanvasPanel;
    private GameObject FOV;
    private Image FOV_Image;


    private void Awake()
    {
        countdownText.text = currentTime.ToString("Prepare to Start");
        Manager = FindObjectOfType<Manager>();
    }
    private void Start()
    {
        if(Manager != null)
        {
            FOV = GameObject.Find("FOV");
            FOV_Image = FOV.GetComponentInChildren<Image>();
            FOV_Image.enabled = true;

        }

        currentTime = countdownTime;
        RightHandController.SetActive(false);
        CanvasPanel = GameObject.FindGameObjectWithTag("Panel");

        if(!isCountdownStarted)
            StartCountdown();
    }

    private void Update()
    {
        if (isCountdownStarted)
        {

            currentTime -= Time.deltaTime;


            //Show Countdown value - starts at 5 seconds.
            if ( currentTime <= 5f ) { countdownText.text = currentTime.ToString("0"); }
            
            //When Countdown reaches 0 sec, show the Start text and activate XR Controller.
            if (currentTime < 1 && !TimeController.isFinished) 
            { 
                countdownText.text = "Start!";
                
                RightHandController.SetActive(true);
            }
            
            //When everything is set to start
            if (currentTime <= 0 && !TimeController.isFinished)
            {
                CanvasPanel.GetComponent<Image>().enabled = false;
                if(Manager != null)
                {
                    Manager.currentScene.Add("0");
                    Manager.Markers.StreamData(Manager.currentScene.ToArray());
                }

                StopCountdown();
                TimeController.StartTimer();
            }
             
            //When the duration of the experiment has reached the end.
            if (currentTime <= 0 && TimeController.isFinished)
            {
                if (Manager != null)
                {
                    Manager.currentScene.RemoveAt(Manager.currentScene.Count - 1);
                    Manager.currentScene.Add("1");
                    Manager.Markers.StreamData(Manager.currentScene.ToArray());
                    Manager.ChangeScene();
                }
                else
                {
                    SceneManager.LoadScene("Main_Menu_HMD");
                }


                StopCountdown();
               
            }
        }

        //if (!isCountdownStarted && TimeController.isFinished)
        //{
        //    StartCountdown();
        //    RightHandController.SetActive(false);
        //}
    }

    public void StartCountdown()
    {
        ResetCountdown();
        isCountdownStarted = true;
        
    }

    public void StopCountdown()
    {
        isCountdownStarted = false;
        currentTime = 0;
        countdownText.gameObject.SetActive(false);
    }

    public void ResetCountdown()
    {
        currentTime = countdownTime;
    }
}
