using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu control that works like TimeSelector / WeatherSelector:   &lt;   [ Strong (45 km/h) ]   &gt;
/// The chosen level is stored in Settings.SelectedWind and applied by WindApplier when the next scene loads.
///
/// Setup: "Levada > Weather > Add Wind Selector to Open Menu Scene", or put this component on a copy
/// of the "Time Selector" object and drag in its two buttons and display text.
/// </summary>
public class WindSelector : MonoBehaviour
{
    public Button leftButton;
    public Button rightButton;
    public TMP_Text windText;

    [Tooltip("Wind levels offered in the menu, in this order. Remove any you don't want to offer.")]
    [SerializeField]
    private WindLevel[] availableLevels =
    {
        WindLevel.Default,
        WindLevel.Calm,
        WindLevel.Light,
        WindLevel.Moderate,
        WindLevel.Strong,
        WindLevel.VeryStrong
    };

    private int currentIndex;

    private void Awake()
    {
        if (leftButton != null) leftButton.onClick.AddListener(Previous);
        if (rightButton != null) rightButton.onClick.AddListener(Next);
    }

    private void OnDestroy()
    {
        if (leftButton != null) leftButton.onClick.RemoveListener(Previous);
        if (rightButton != null) rightButton.onClick.RemoveListener(Next);
    }

    private void OnEnable()
    {
        // Keep showing the current choice when coming back to the menu.
        currentIndex = System.Array.IndexOf(availableLevels, Settings.SelectedWind);
        if (currentIndex < 0) currentIndex = 0;
        UpdateDisplay();
    }

    public void Previous()
    {
        if (availableLevels.Length == 0) return;
        currentIndex = (currentIndex - 1 + availableLevels.Length) % availableLevels.Length;
        UpdateDisplay();
    }

    public void Next()
    {
        if (availableLevels.Length == 0) return;
        currentIndex = (currentIndex + 1) % availableLevels.Length;
        UpdateDisplay();
    }

    /// <summary>Select a level directly, e.g. from a UI Button's OnClick (0 = Default, 1 = Calm ... 5 = Very Strong).</summary>
    public void SelectWind(int windLevel)
    {
        int index = System.Array.IndexOf(availableLevels, (WindLevel)windLevel);
        if (index < 0) return;
        currentIndex = index;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (availableLevels == null || availableLevels.Length == 0) return;

        Settings.SelectedWind = availableLevels[currentIndex];

        if (windText != null)
            windText.text = WindLevelInfo.DisplayName(Settings.SelectedWind);
    }
}
