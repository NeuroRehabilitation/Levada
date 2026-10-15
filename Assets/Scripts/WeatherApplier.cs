using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AzureSky;
using UnityEngine.SceneManagement;

/// <summary>
/// Applies the weather chosen in the menu (Settings.SelectedWeather) to the Azure[Sky] Sky Manager
/// of every scene that is loaded - Sao_Lourenço_HMD, the Walk in Place scenes, the KAVE scenes, etc.
///
/// Nothing has to be added to the scenes: this script hooks itself into scene loading and adds
/// itself to the "Azure Sky Manager" object automatically.
///
/// You can still add it to a scene by hand (e.g. on the Azure Sky Manager) to:
///   - test a weather in the Editor without going through the menu (Override Menu Selection), or
///   - point a preset at a specific profile (Profile Overrides), e.g. the Tropical Forest profiles.
///
/// Weathers whose profile isn't in a scene's "Weather Profiles" list (e.g. Fog) are loaded from
/// Assets/Resources/Weather Profiles, so new weathers don't require editing every scene.
/// Fog also turns on Unity's scene fog, because that is the fog these scenes actually render.
/// </summary>
[DefaultExecutionOrder(1000)] // after AzureSkyManager, so its Start() can't undo the weather
public class WeatherApplier : MonoBehaviour
{
    [Serializable]
    public struct ProfileOverride
    {
        public WeatherType weather;
        public AzureSkyProfile profile;
    }

    [Header("Only needed when this component is added to a scene by hand")]
    [Tooltip("Editor only: ignore the menu and always use 'Weather In This Scene', so you can press Play " +
             "directly in a scene to test a weather. Has no effect in builds.")]
    public bool overrideMenuSelection = false;
    public WeatherType weatherInThisScene = WeatherType.Default;

    [Tooltip("Optional. Use these profiles instead of searching the Sky Manager's 'Weather Profiles' list by name.")]
    public List<ProfileOverride> profileOverrides = new List<ProfileOverride>();

    [Header("Rain / snow")]
    [Tooltip("Keep the Azure rain and snow particles above the player's camera, so precipitation falls around " +
             "the user wherever they are on the levada (the scenes don't do this on their own).")]
    public bool precipitationFollowsPlayer = true;

    [Header("Fog weather")]
    [Tooltip("Distance from the camera, in metres, where the fog begins. How far you can see is set in the Fog " +
             "profile: Fog Scattering > Global Fog Distance.")]
    public float fogStartDistance = 2f;

    [Range(0f, 1f)]
    [Tooltip("0 = the fog takes the colour of the sky light (follows the time of day), 1 = plain grey.")]
    public float fogGreyness = 0.5f;

    [Tooltip("Found automatically if left empty.")]
    public AzureSkyManager skyManager;

    // Folder inside Assets/Resources with weather profiles that every scene can use.
    private const string SharedProfilesFolder = "Weather Profiles";

    private bool m_applied;
    private bool m_sceneFog;
    private string m_appliedProfileName;
    private readonly List<Transform> m_precipitationRoots = new List<Transform>();
    private Transform m_followTarget;
    private float m_nextTargetSearch;

    // ------------------------------------------------------------------
    // Automatic hook-up: runs for every scene load, no scene changes needed
    // ------------------------------------------------------------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // sceneLoaded fires after the scene's Awake/OnEnable but before any Start,
        // so the weather is in place before the first frame is drawn.
        foreach (AzureSkyManager sky in FindObjectsOfType<AzureSkyManager>())
        {
            if (sky.gameObject.scene != scene || HasApplier(sky))
                continue;

            sky.gameObject.AddComponent<WeatherApplier>(); // Awake() finds the sky and applies the weather
        }
    }

    private static bool HasApplier(AzureSkyManager sky)
    {
        foreach (WeatherApplier applier in FindObjectsOfType<WeatherApplier>())
        {
            if (applier.skyManager == sky)
                return true;
        }
        return false;
    }

    // ------------------------------------------------------------------

    private void Awake()
    {
        if (skyManager == null) skyManager = GetComponent<AzureSkyManager>();
        if (skyManager == null) skyManager = FindObjectOfType<AzureSkyManager>();

        Apply();
    }

    private void Start()
    {
        // Apply again in case AzureSkyManager.Start() ran before us and reset the day profile.
        Apply();

        if (m_applied && precipitationFollowsPlayer)
            SetupPrecipitationFollow();

        StartCoroutine(LogAfterFirstFrame());
    }

    /// <summary>
    /// The weather this scene should use: the menu choice, or the inspector override.
    /// The override only works in the Editor, so a forgotten test setting can't end up in a build.
    /// </summary>
    public WeatherType RequestedWeather
    {
        get
        {
            return overrideMenuSelection && Application.isEditor ? weatherInThisScene : Settings.SelectedWeather;
        }
    }

    /// <summary>Sets the Sky Manager's profile to the requested weather, instantly (no transition).</summary>
    public void Apply()
    {
        m_applied = false;
        m_sceneFog = false;

        if (skyManager == null)
            return;

        WeatherType weather = RequestedWeather;
        if (weather == WeatherType.Default)
            return; // leave the scene exactly as authored

        AzureProfileController profiles = skyManager.profileController;

        int weatherIndex;
        AzureSkyProfile profile = FindProfile(profiles, weather, out weatherIndex);
        if (profile == null)
        {
            Debug.LogWarning($"[Weather] '{WeatherTypeInfo.DisplayName(weather)}' was selected but scene " +
                             $"'{gameObject.scene.name}' has no matching Azure profile. Available weather profiles: " +
                             $"{ListProfileNames(profiles)}, plus anything in Assets/Resources/{SharedProfilesFolder}. " +
                             "Keeping the scene's default weather.");
            return;
        }

        // Make the chosen profile the scene's day profile. AzureSkyManager.Start() always begins with
        // defaultProfileList[0], so this works whether or not Start() has already run. Only this scene
        // instance is changed at runtime; the scene file and the profile assets are untouched.
        profiles.defaultProfileList = new List<AzureSkyProfile> { profile };
        profiles.defaultProfile = profile;
        profiles.currentProfile = profile;
        profiles.targetProfile = profile;
        profiles.isWeatherTransition = false;
        profiles.transitionProgress = 0f;
        skyManager.weatherIndex = weatherIndex;

        m_applied = true;
        m_appliedProfileName = profile.name;
        m_sceneFog = weather == WeatherType.Fog;
    }

    // ------------------------------------------------------------------
    // Profile lookup
    // ------------------------------------------------------------------

    private AzureSkyProfile FindProfile(AzureProfileController profiles, WeatherType weather, out int weatherIndex)
    {
        weatherIndex = -1;

        // 1) Explicit overrides set in the inspector
        foreach (ProfileOverride entry in profileOverrides)
        {
            if (entry.weather == weather && entry.profile != null)
            {
                weatherIndex = IndexOf(profiles, entry.profile);
                return entry.profile;
            }
        }

        string[] keywords = WeatherTypeInfo.ProfileKeywords(weather);

        // 2) Match by name in the Sky Manager's "Weather Profiles" list.
        var sceneProfiles = new List<AzureSkyProfile>();
        foreach (WeatherProfile entry in profiles.weatherProfileList)
            sceneProfiles.Add(entry.profile);

        int match = MatchByName(sceneProfiles, keywords);
        if (match >= 0)
        {
            weatherIndex = match;
            return sceneProfiles[match];
        }

        // 3) Match by name in the profiles shared by all scenes (Assets/Resources/Weather Profiles), e.g. Fog.
        //    weatherIndex stays -1 because the profile isn't part of this scene's list.
        AzureSkyProfile[] shared = Resources.LoadAll<AzureSkyProfile>(SharedProfilesFolder);
        match = MatchByName(shared, keywords);
        return match >= 0 ? shared[match] : null;
    }

    /// <summary>
    /// Index of the first profile whose name matches one of the keywords, or -1.
    /// Pass 0 = exact name, 1 = name ends with keyword, 2 = name contains keyword.
    /// </summary>
    private static int MatchByName(IList<AzureSkyProfile> candidates, string[] keywords)
    {
        for (int pass = 0; pass < 3; pass++)
        {
            foreach (string keyword in keywords)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i] == null) continue;

                    string name = Normalize(candidates[i].name);
                    bool match = pass == 0 ? name == keyword
                               : pass == 1 ? name.EndsWith(keyword)
                               : name.Contains(keyword);
                    if (match)
                        return i;
                }
            }
        }
        return -1;
    }

    private static int IndexOf(AzureProfileController profiles, AzureSkyProfile profile)
    {
        for (int i = 0; i < profiles.weatherProfileList.Count; i++)
        {
            if (profiles.weatherProfileList[i].profile == profile)
                return i;
        }
        return -1;
    }

    private static string Normalize(string s)
    {
        return s.ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "").Replace(".", "");
    }

    private static string ListProfileNames(AzureProfileController profiles)
    {
        var names = new List<string>();
        foreach (WeatherProfile entry in profiles.weatherProfileList)
        {
            if (entry.profile != null) names.Add(entry.profile.name);
        }
        return names.Count > 0 ? string.Join(", ", names) : "(none)";
    }

    // ------------------------------------------------------------------
    // Rain and snow around the player
    // ------------------------------------------------------------------

    private void SetupPrecipitationFollow()
    {
        AzureSkyEffects effects = skyManager.GetComponentInChildren<AzureSkyEffects>(true);
        if (effects == null) effects = FindObjectOfType<AzureSkyEffects>();
        if (effects == null) return;

        // If the scene already makes the particles follow something, leave it alone.
        if (effects.particlesDriverList != null && effects.particlesDriverList.Count > 0) return;

        AddPrecipitationRoot(effects.rainLightParticle);
        AddPrecipitationRoot(effects.rainMediumParticle);
        AddPrecipitationRoot(effects.rainHeavyParticle);
        AddPrecipitationRoot(effects.snowParticle);
    }

    private void AddPrecipitationRoot(ParticleSystem particles)
    {
        if (particles == null) return;

        // In the Azure prefab the emitters sit 50 m above a shared "Particles" object: move that object.
        Transform root = particles.transform.parent != null ? particles.transform.parent : particles.transform;
        if (root == skyManager.transform) root = particles.transform; // never move the whole Sky Manager

        if (!m_precipitationRoots.Contains(root))
            m_precipitationRoots.Add(root);
    }

    private void LateUpdate()
    {
        UpdateSceneFog();

        if (m_precipitationRoots.Count == 0) return;

        if (m_followTarget == null || !m_followTarget.gameObject.activeInHierarchy)
        {
            if (Time.unscaledTime < m_nextTargetSearch) return;
            m_nextTargetSearch = Time.unscaledTime + 0.5f;
            m_followTarget = FindPlayerCamera();
            if (m_followTarget == null) return;
        }

        Vector3 position = m_followTarget.position;
        for (int i = 0; i < m_precipitationRoots.Count; i++)
        {
            if (m_precipitationRoots[i] != null)
                m_precipitationRoots[i].position = position;
        }
    }

    // ------------------------------------------------------------------
    // Fog
    // ------------------------------------------------------------------

    /// <summary>
    /// The scenes draw Unity's own fog (Lighting > Environment > Fog); Azure's fog effect isn't on their
    /// cameras. So for the Fog weather this sets Unity's fog from the Fog profile, every frame:
    /// visibility = the profile's Fog Scattering > Global Fog Distance, colour = the profile's sky light,
    /// so the fog gets darker or lighter with the time of day.
    /// </summary>
    private void UpdateSceneFog()
    {
        if (!m_sceneFog || skyManager == null) return;

        AzureSkySettings sky = skyManager.settings;
        float visibility = Mathf.Max(fogStartDistance + 1f, sky.GlobalFogDistance);

        Color skyLight = sky.AmbientEquatorColor;
        float grey = skyLight.grayscale;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear; // same mode the scenes already use, so it also works in builds
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = visibility;
        RenderSettings.fogColor = Color.Lerp(skyLight, new Color(grey, grey, grey, 1f), fogGreyness);
    }

    private static Transform FindPlayerCamera()
    {
        // HMD scenes: the headset camera inside the XR Origin (same object Manager.cs looks for)
        GameObject xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin != null)
        {
            Camera xrCamera = xrOrigin.GetComponentInChildren<Camera>();
            if (xrCamera != null && xrCamera.isActiveAndEnabled)
                return xrCamera.transform;
        }

        // KAVE / desktop scenes
        return Camera.main != null ? Camera.main.transform : null;
    }

    // ------------------------------------------------------------------
    // Logging (same Logger the TimelineSetter uses)
    // ------------------------------------------------------------------

    private IEnumerator LogAfterFirstFrame()
    {
        yield return null; // let Logger.Start() create its log file first

        string message = m_applied
            ? $"Weather set to {WeatherTypeInfo.DisplayName(RequestedWeather)} (profile '{m_appliedProfileName}')"
            : "Weather set to scene default";
        if (m_sceneFog)
            message += $", fog visibility {RenderSettings.fogEndDistance:0} m";

        Debug.Log($"[Weather] {message} in {gameObject.scene.path}");

        // A disabled Azure Sky Manager never updates the sky, so the weather can't be seen.
        if (m_applied && skyManager != null && !skyManager.isActiveAndEnabled)
        {
            Debug.LogWarning($"[Weather] The Azure Sky Manager in '{gameObject.scene.path}' is switched off, so the " +
                             "sky, light and rain won't change. If this is the old copy in Assets/ScenesHMD, untick it " +
                             "in Build Settings so the menu opens Assets/ScenesNew/HMD/Sao_Lourenço_HMD instead.");
        }

        GameObject loggerObject = GameObject.Find("Logger");
        global::Logger logger = loggerObject != null ? loggerObject.GetComponent<global::Logger>() : null;
        if (logger != null)
            logger.Log(message, "Weather");
    }
}
