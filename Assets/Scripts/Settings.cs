using UnityEngine;

public static class Settings
{
    public static float SelectedTime = 7f;

    // Chosen in the menu by WeatherSelector, applied to the next scene by WeatherApplier.
    public static WeatherType SelectedWeather = WeatherType.Default;

    // Chosen in the menu by WindSelector, applied to the next scene by WindApplier.
    public static WindLevel SelectedWind = WindLevel.Default;
}
