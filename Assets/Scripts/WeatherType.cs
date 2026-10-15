/// <summary>
/// Weather presets that can be picked in the main menu before entering a scene.
/// Each preset is matched (by name) to one of the Azure[Sky] weather profiles of the scene
/// that is loaded next - see WeatherApplier. Weathers whose profile is not in a scene's list (like Fog)
/// are loaded from Assets/Resources/Weather Profiles instead, so the scenes don't need to be edited.
/// </summary>
public enum WeatherType
{
    Default = 0,   // keep the scene exactly as it was authored (its own day profile)
    ClearSky,
    Overcast,
    LightRain,
    MediumRain,
    Storm,
    Snow,
    Fog            // new weathers always go at the end, so the numbers Unity saved for the others don't change
}

public static class WeatherTypeInfo
{
    /// <summary>Text shown in the menu for each preset.</summary>
    public static string DisplayName(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.ClearSky:   return "Clear Sky";
            case WeatherType.Overcast:   return "Overcast";
            case WeatherType.LightRain:  return "Light Rain";
            case WeatherType.MediumRain: return "Medium Rain";
            case WeatherType.Storm:      return "Storm";
            case WeatherType.Snow:       return "Snow";
            case WeatherType.Fog:        return "Fog";
            default:                     return "Default";
        }
    }

    /// <summary>
    /// Keywords used to find the matching Azure[Sky] profile in a scene's "Weather Profiles" list.
    /// Profile names are compared in lower case with spaces, '_' and '-' removed, so
    /// "ClearSky", "Clear Sky" and "TropicalForestClearSky" all match ClearSky.
    /// </summary>
    public static string[] ProfileKeywords(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.ClearSky:   return new[] { "clearsky", "clear", "sunny" };
            case WeatherType.Overcast:   return new[] { "overcast", "cloudy" };
            case WeatherType.LightRain:  return new[] { "lightrain", "drizzle" };
            case WeatherType.MediumRain: return new[] { "mediumrain", "moderaterain" };
            case WeatherType.Storm:      return new[] { "storm", "thunder", "heavyrain" };
            case WeatherType.Snow:       return new[] { "snow" };
            case WeatherType.Fog:        return new[] { "fog", "mist" };
            default:                     return new string[0];
        }
    }
}
