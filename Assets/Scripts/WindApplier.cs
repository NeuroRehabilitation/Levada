using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AzureSky;
using UnityEngine.SceneManagement;

/// <summary>
/// Applies the wind level chosen in the menu (Settings.SelectedWind) to every scene that loads:
///   - NM_Wind (NatureManufacture plants and trees, e.g. "WindControllers/Prefab_Wind Variant"):
///     wind speed, turbulence and gusts
///   - Unity Wind Zones (Unity trees, particles): scaled up or down
///   - Wind sound: plays Azure's wind loops at the level's volumes, replacing the weather's wind sound
///
/// Like WeatherApplier, nothing has to be added to the scenes. Add it to a scene by hand only to test a
/// level in the Editor (tick Override Menu Selection). The numbers for each level are in WindLevel.cs.
/// </summary>
[DefaultExecutionOrder(1000)] // LateUpdate after Azure has written its own wind values
public class WindApplier : MonoBehaviour
{
    [Header("Only needed when this component is added to a scene by hand")]
    [Tooltip("Editor only: ignore the menu and always use 'Wind In This Scene'. Has no effect in builds.")]
    public bool overrideMenuSelection = false;
    public WindLevel windInThisScene = WindLevel.Default;

    [Tooltip("Replace the weather's wind sound with one that matches the wind level.")]
    public bool controlWindSound = true;

    private WindLevel m_level;
    private WindPreset m_preset;
    private readonly List<WindZone> m_zones = new List<WindZone>();
    private readonly List<float> m_zoneBase = new List<float>();
    private AzureSkyEffects m_effects;
    private AudioSource m_lightSound, m_mediumSound, m_heavySound;
    private int m_plantWindCount;

    public WindLevel RequestedWind
    {
        get { return overrideMenuSelection && Application.isEditor ? windInThisScene : Settings.SelectedWind; }
    }

    // ------------------------------------------------------------------
    // Automatic hook-up
    // ------------------------------------------------------------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Settings.SelectedWind == WindLevel.Default) return;        // nothing to change
        if (!SceneHas<NM_Wind>(scene) && !SceneHas<WindZone>(scene)) return; // e.g. the menu
        if (SceneHas<WindApplier>(scene)) return;                      // one was added by hand

        var holder = new GameObject("Wind Applier (auto)");
        SceneManager.MoveGameObjectToScene(holder, scene);
        holder.AddComponent<WindApplier>();
    }

    private static bool SceneHas<T>(Scene scene) where T : Component
    {
        foreach (T component in FindObjectsOfType<T>())
        {
            if (component.gameObject.scene == scene) return true;
        }
        return false;
    }

    // ------------------------------------------------------------------

    private void Start()
    {
        m_level = RequestedWind;
        if (m_level == WindLevel.Default)
        {
            enabled = false;
            return;
        }

        m_preset = WindLevelInfo.Preset(m_level);

        // 1) NatureManufacture wind (moves the NM vegetation through global shader values every frame)
        foreach (NM_Wind plantWind in FindObjectsOfType<NM_Wind>())
        {
            if (plantWind.gameObject.scene != gameObject.scene) continue;
            plantWind.WindSpeed = m_preset.speedKmh;
            plantWind.Turbulence = m_preset.turbulence;
            plantWind.GustSpeed = m_preset.gustSpeedKmh;
            plantWind.GustScale = m_preset.gustScale;
            m_plantWindCount++;
        }

        // 2) Unity Wind Zones. Azure rewrites its own zone every frame from the weather (0 when clear,
        //    1 in rain), so for that one we scale its full-wind value of 1.
        m_effects = FindObjectOfType<AzureSkyEffects>();
        WindZone azureZone = m_effects != null ? m_effects.windZone : null;

        foreach (WindZone zone in FindObjectsOfType<WindZone>())
        {
            if (zone.gameObject.scene != gameObject.scene) continue;
            m_zones.Add(zone);
            m_zoneBase.Add(zone == azureZone ? 1f : zone.windMain);
        }

        // 3) Wind sound, using the same loops as Azure
        if (controlWindSound && m_effects != null)
        {
            m_lightSound = CopyWindSound(m_effects.windLight);
            m_mediumSound = CopyWindSound(m_effects.windMedium);
            m_heavySound = CopyWindSound(m_effects.windHeavy);
        }

        StartCoroutine(LogAfterFirstFrame());
    }

    private void LateUpdate()
    {
        for (int i = 0; i < m_zones.Count; i++)
        {
            if (m_zones[i] != null)
                m_zones[i].windMain = m_zoneBase[i] * m_preset.windZoneScale;
        }

        if (controlWindSound && m_effects != null)
        {
            // Mute the weather's wind loops (Azure keeps managing them) and play ours instead.
            Mute(m_effects.windLight);
            Mute(m_effects.windMedium);
            Mute(m_effects.windHeavy);

            SetVolume(m_lightSound, m_preset.lightSound);
            SetVolume(m_mediumSound, m_preset.mediumSound);
            SetVolume(m_heavySound, m_preset.heavySound);
        }
    }

    private AudioSource CopyWindSound(AudioSource source)
    {
        if (source == null || source.clip == null) return null;

        var holder = new GameObject("Wind Sound (" + source.name + ")");
        holder.transform.SetParent(transform, false);

        AudioSource copy = holder.AddComponent<AudioSource>();
        copy.clip = source.clip;
        copy.loop = true;
        copy.playOnAwake = false;
        copy.spatialBlend = source.spatialBlend;
        copy.outputAudioMixerGroup = source.outputAudioMixerGroup;
        copy.priority = source.priority;
        copy.volume = 0f;
        return copy;
    }

    private static void Mute(AudioSource source)
    {
        if (source != null) source.volume = 0f;
    }

    private static void SetVolume(AudioSource source, float volume)
    {
        if (source == null) return;
        source.volume = volume;
        if (volume > 0f && !source.isPlaying) source.Play();
        else if (volume <= 0f && source.isPlaying) source.Stop();
    }

    private IEnumerator LogAfterFirstFrame()
    {
        yield return null; // let Logger.Start() create its log file first

        string message = $"Wind set to {WindLevelInfo.DisplayName(m_level)}";
        Debug.Log($"[Wind] {message} in {gameObject.scene.path} " +
                  $"({m_plantWindCount} NM_Wind, {m_zones.Count} Wind Zones)");

        if (m_plantWindCount == 0 && m_zones.Count == 0)
            Debug.LogWarning("[Wind] This scene has no NM_Wind or Wind Zone, so the wind level has no visible effect.");

        GameObject loggerObject = GameObject.Find("Logger");
        global::Logger logger = loggerObject != null ? loggerObject.GetComponent<global::Logger>() : null;
        if (logger != null)
            logger.Log(message, "Wind");
    }
}
