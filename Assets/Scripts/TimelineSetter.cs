using UnityEngine;
using UnityEngine.AzureSky;

public class TimelineSetter : MonoBehaviour
{
    public AzureSkyManager AzureSkyManager;
    void Start()
    {
        AzureSkyManager.timeController.timeline = Settings.SelectedTime;
        GameObject.Find("Logger").GetComponent<Logger>().Log($"Timeline set to {Settings.SelectedTime}h", "Time" );
    }
}
