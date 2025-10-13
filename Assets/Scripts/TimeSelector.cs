using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeSelector : MonoBehaviour
{
    public Button leftButton;
    public Button rightButton;
    public TextMeshProUGUI numberText;

    [SerializeField] private float[] availableHours = { 7f, 10f, 13f, 16f };
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
        Settings.SelectedTime = availableHours[currentIndex];
        numberText.text = availableHours[currentIndex].ToString("00") + ":00h";
    }
}
