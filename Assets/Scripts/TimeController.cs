using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    private float elapsedTime;
    public bool isRunning;
    public bool isFinished = false;

    public float minutes = 0.2f;
    private float duration;

    public Countdown Countdown;
    private Manager Manager;

    private void Awake()
    {
        Manager = FindObjectOfType<Manager>();

        if (Manager != null)
        {
            duration = Manager.duration * 60;
        }
        else
            duration = minutes * 60;
    }

    private void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            //Debug.Log(elapsedTime);
        }

        if (isRunning && elapsedTime >= duration)
        {
            //Countdown.countdownText.gameObject.SetActive(true);
            StopTimer();
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        //Countdown.countdownText.text = "Finishing in...";
        isFinished = true;
        isRunning = false;
        
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}