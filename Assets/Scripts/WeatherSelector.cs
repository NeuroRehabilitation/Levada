using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu control that works like TimeSelector:   &lt;   [ Light Rain ]   &gt;
/// The chosen weather is stored in Settings.SelectedWeather and applied by WeatherApplier
/// as soon as the next scene (e.g. Sao_Lourenço_HMD) loads.
///
/// Setup: use the menu "Levada > Weather > Add Weather Selector to Open Menu Scene", or put this
/// component on a copy of the "Time Selector" object and drag in its two buttons and display text.
/// </summary>
public class WeatherSelector : MonoBehaviour
{
    public Button leftButton;
    public Button rightButton;
    public TMP_Text weatherText;

    // Called "availableWeathers" before Fog was added. The rename makes menus that were saved with the
    // old 7-item list start from this new default (with Fog) instead of keeping the old saved list.
    [Tooltip("Weathers offered in the menu, in this order. Remove any you don't want to offer.")]
    [SerializeField]
    private WeatherType[] weathersInMenu =
    {
        WeatherType.Default,
        WeatherType.ClearSky,
        WeatherType.Overcast,
        WeatherType.LightRain,
        WeatherType.MediumRain,
        WeatherType.Storm,
        WeatherType.Snow,
        WeatherType.Fog
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
        // Show the weather that is already selected, so the choice is kept when coming back to
        // the menu (Esc) and when switching between menu panels that each have a selector.
        currentIndex = System.Array.IndexOf(weathersInMenu, Settings.SelectedWeather);
        if (currentIndex < 0) currentIndex = 0;
        UpdateDisplay();
    }

    public void Previous()
    {
        if (weathersInMenu.Length == 0) return;
        currentIndex = (currentIndex - 1 + weathersInMenu.Length) % weathersInMenu.Length;
        UpdateDisplay();
    }

    public void Next()
    {
        if (weathersInMenu.Length == 0) return;
        currentIndex = (currentIndex + 1) % weathersInMenu.Length;
        UpdateDisplay();
    }

    /// <summary>Select a weather directly, e.g. from a UI Button's OnClick (0 = Default, 1 = Clear Sky ... 6 = Snow, 7 = Fog).</summary>
    public void SelectWeather(int weatherType)
    {
        int index = System.Array.IndexOf(weathersInMenu, (WeatherType)weatherType);
        if (index < 0) return;
        currentIndex = index;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (weathersInMenu == null || weathersInMenu.Length == 0) return;

        Settings.SelectedWeather = weathersInMenu[currentIndex];

        if (weatherText != null)
            weatherText.text = WeatherTypeInfo.DisplayName(Settings.SelectedWeather);
    }
}
