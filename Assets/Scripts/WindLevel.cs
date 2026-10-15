/// <summary>
/// Wind levels that can be picked in the main menu (WindSelector) and are applied to the next
/// scene by WindApplier. "Default" leaves the wind exactly as the scene / weather sets it.
/// </summary>
public enum WindLevel
{
    Default = 0,
    Calm,
    Light,
    Moderate,
    Strong,
    VeryStrong
}

/// <summary>What each wind level does. Change the numbers here to tune the levels.</summary>
public struct WindPreset
{
    public float speedKmh;       // NM_Wind "Wind Speed": how much the NatureManufacture plants and trees bend
    public float turbulence;     // NM_Wind "Turbulence" (0-2): how chaotic the movement is
    public float gustSpeedKmh;   // NM_Wind "Gust Speed"
    public float gustScale;      // NM_Wind "Gust Scale": how strong the gusts are
    public float windZoneScale;  // multiplies the scene's Unity Wind Zones (Unity trees, particles)
    public float lightSound;     // volume of Azure's light / medium / heavy wind loops (0-1)
    public float mediumSound;
    public float heavySound;

    public WindPreset(float speedKmh, float turbulence, float gustSpeedKmh, float gustScale,
                      float windZoneScale, float lightSound, float mediumSound, float heavySound)
    {
        this.speedKmh = speedKmh;
        this.turbulence = turbulence;
        this.gustSpeedKmh = gustSpeedKmh;
        this.gustScale = gustScale;
        this.windZoneScale = windZoneScale;
        this.lightSound = lightSound;
        this.mediumSound = mediumSound;
        this.heavySound = heavySound;
    }
}

public static class WindLevelInfo
{
    // For reference, S Lourenço's own NM_Wind is set to 25.4 km/h, turbulence 0.89, gusts 20 km/h x 0.35,
    // so "Moderate" looks like the scene does today.
    public static WindPreset Preset(WindLevel level)
    {
        switch (level)
        {
            //                                 km/h  turb  gust  gustScale zones  light  medium heavy
            case WindLevel.Calm:       return new WindPreset( 3f, 0.3f,  5f, 0.05f,  0.1f,  0.00f, 0.00f, 0.00f);
            case WindLevel.Light:      return new WindPreset(12f, 0.5f, 15f, 0.20f,  0.5f,  0.20f, 0.00f, 0.00f);
            case WindLevel.Moderate:   return new WindPreset(25f, 0.9f, 20f, 0.35f,  1.0f,  0.30f, 0.00f, 0.00f);
            case WindLevel.Strong:     return new WindPreset(45f, 1.0f, 40f, 0.60f,  1.8f,  0.15f, 0.35f, 0.00f);
            case WindLevel.VeryStrong: return new WindPreset(65f, 1.2f, 60f, 0.90f,  2.5f,  0.00f, 0.30f, 0.45f);
            default:                   return new WindPreset(25f, 0.9f, 20f, 0.35f,  1.0f,  0.30f, 0.00f, 0.00f);
        }
    }

    /// <summary>Text shown in the menu, e.g. "Strong (45 km/h)".</summary>
    public static string DisplayName(WindLevel level)
    {
        if (level == WindLevel.Default) return "Default";
        string name = level == WindLevel.VeryStrong ? "Very Strong" : level.ToString();
        return $"{name} ({Preset(level).speedKmh:0} km/h)";
    }
}
