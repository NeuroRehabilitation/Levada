using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeSelector : MonoBehaviour
{
    public Button leftButton;
    public Button rightButton;
    public TextMeshProUGUI numberText;

    [SerializeField] private float[] availableHours = { 7f, 8f, 13f, 16.5f };
    private int currentIndex = 0;

    void Start()
    {
        UpdateDisplay();

        leftButton.onClick.AddListener(PreviousNumber);
        rightButton.onClick.AddListener(NextNumber);
    }

    void PreviousNumber()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = availableHours.Length - 1;
        UpdateDisplay();
    }

    void NextNumber()
    {
        currentIndex++;
        if (currentIndex >= availableHours.Length)
            currentIndex = 0;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        float time = availableHours[currentIndex];
        Settings.SelectedTime = time;

        // Works for any value in the list, e.g. 16.5 -> "16:30h", 9.25 -> "09:15h"
        int hours = Mathf.FloorToInt(time);
        int minutes = Mathf.RoundToInt((time - hours) * 60f);
        if (minutes == 60) { hours++; minutes = 0; }
        numberText.text = hours.ToString("00") + ":" + minutes.ToString("00") + "h";
    }
}
