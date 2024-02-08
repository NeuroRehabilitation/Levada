using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    private float elapsedTime;
    public bool isRunning;
    public bool isFinished = false;
    public float duration = 0.2f; //duration of experiment in minutes.

    public Countdown Countdown;

    private void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            //Debug.Log(elapsedTime);
        }

        if (isRunning && elapsedTime >= duration * 60)
        {
            Countdown.countdownText.gameObject.SetActive(true);
            StopTimer();
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        Countdown.countdownText.text = "Finishing in...";
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