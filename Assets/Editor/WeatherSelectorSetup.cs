using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Menu: Levada > Weather > Add Weather Selector to Open Menu Scene
///       Levada > Weather > Add Wind Selector to Open Menu Scene
///
/// For every working "Time Selector" in the open scene (e.g. "HMD or KAVE"), this creates a copy with the
/// same font, buttons and style, wired to WeatherSelector or WindSelector. Undo (Ctrl+Z) removes it again.
/// Move it if needed, then save the scene.
/// </summary>
public static class WeatherSelectorSetup
{
    private const float RowHeight = 120f; // vertical spacing between selectors

    [MenuItem("Levada/Weather/Add Weather Selector to Open Menu Scene")]
    private static void AddWeatherSelectors()
    {
        AddSelectors<WeatherSelector>("Weather Selector", "Weather",
            WeatherTypeInfo.DisplayName(WeatherType.Default),
            (time, siblings) => time + Vector2.down * RowHeight,
            (selector, left, right, text) =>
            {
                selector.leftButton = left;
                selector.rightButton = right;
                selector.weatherText = text;
            });
    }

    [MenuItem("Levada/Weather/Add Wind Selector to Open Menu Scene")]
    private static void AddWindSelectors()
    {
        AddSelectors<WindSelector>("Wind Selector", "Wind",
            WindLevelInfo.DisplayName(WindLevel.Default),
            WindPosition,
            (selector, left, right, text) =>
            {
                selector.leftButton = left;
                selector.rightButton = right;
                selector.windText = text;
            });
    }

    /// <summary>
    /// Under the Time and Weather selectors: centred below them if they sit side by side,
    /// otherwise one row below the lowest.
    /// </summary>
    private static Vector2 WindPosition(Vector2 time, List<Vector2> weatherSelectors)
    {
        if (weatherSelectors.Count == 0)
            return time + Vector2.down * RowHeight;

        Vector2 weather = weatherSelectors[0];
        if (Mathf.Abs(weather.y - time.y) < 1f)
            return new Vector2((time.x + weather.x) * 0.5f, time.y - RowHeight);

        return new Vector2(time.x, Mathf.Min(time.y, weather.y) - RowHeight);
    }

    // ------------------------------------------------------------------

    private static void AddSelectors<T>(string objectName, string label, string initialText,
                                        Func<Vector2, List<Vector2>, Vector2> position,
                                        Action<T, Button, Button, TMP_Text> wire) where T : MonoBehaviour
    {
        var created = new List<GameObject>();
        int alreadyThere = 0;

        foreach (TimeSelector time in Object.FindObjectsOfType<TimeSelector>(true))
        {
            // Skip incomplete copies of the script (e.g. the one on "User ID" with no buttons).
            if (time.leftButton == null || time.rightButton == null || time.numberText == null)
                continue;

            if (HasSiblingWith<T>(time.transform))
            {
                alreadyThere++;
                continue;
            }

            Vector2 timePos = AnchoredPosition(time.transform);
            var weatherPositions = new List<Vector2>();
            foreach (WeatherSelector weather in Object.FindObjectsOfType<WeatherSelector>(true))
            {
                if (weather.transform.parent == time.transform.parent)
                    weatherPositions.Add(AnchoredPosition(weather.transform));
            }

            GameObject copy = CreateFrom(time, objectName, label, initialText, position(timePos, weatherPositions), wire);
            created.Add(copy);
        }

        if (created.Count > 0)
        {
            Selection.objects = created.ToArray();
            Debug.Log($"[Weather] Added {created.Count} {objectName}(s). Move them if needed, then save the scene.");
        }
        else
        {
            string reason = alreadyThere > 0
                ? $"every Time Selector in this scene already has a {objectName} next to it."
                : "no Time Selector with buttons was found. Open the menu scene (e.g. 'HMD or KAVE') first.";
            EditorUtility.DisplayDialog(objectName, "Nothing added: " + reason, "OK");
        }
    }

    private static GameObject CreateFrom<T>(TimeSelector time, string objectName, string label, string initialText,
                                            Vector2 position, Action<T, Button, Button, TMP_Text> wire) where T : MonoBehaviour
    {
        Transform source = time.transform;

        GameObject copy = Object.Instantiate(time.gameObject, source.parent);
        Undo.RegisterCreatedObjectUndo(copy, "Add " + objectName);
        copy.name = objectName;
        copy.transform.SetSiblingIndex(source.parent != null ? source.parent.childCount - 1 : 0);

        if (copy.transform is RectTransform copyRect)
            copyRect.anchoredPosition = position;
        else
            copy.transform.localPosition = position;

        // Find the copied buttons / text (same place in the hierarchy as the originals)
        Button left = MapToCopy<Button>(source, copy.transform, time.leftButton);
        Button right = MapToCopy<Button>(source, copy.transform, time.rightButton);
        TMP_Text display = MapToCopy<TMP_Text>(source, copy.transform, time.numberText);

        if (left == null || right == null || display == null)
        {
            Debug.LogWarning($"[Weather] Couldn't find the buttons/text inside '{source.name}'. " +
                             $"Assign them by hand on the new {objectName}.", copy);
        }

        // Swap the script
        Object.DestroyImmediate(copy.GetComponent<TimeSelector>());
        T selector = copy.AddComponent<T>();
        wire(selector, left, right, display);

        ClearPersistentListeners(left);
        ClearPersistentListeners(right);

        // Value text: start at "Default" and let long names shrink to fit
        if (display != null)
        {
            display.text = initialText;
            display.enableWordWrapping = false;
            display.enableAutoSizing = true;
            display.fontSizeMax = display.fontSize;
            display.fontSizeMin = Mathf.Min(18f, display.fontSize);
            display.gameObject.name = label + " Display";
        }

        // Title: "Time of Day" -> label
        foreach (TMP_Text title in copy.GetComponentsInChildren<TMP_Text>(true))
        {
            if (title != display && ContainsTime(title.text))
            {
                title.text = label;
                title.gameObject.name = label + " Label";
            }
        }
        foreach (Text title in copy.GetComponentsInChildren<Text>(true))
        {
            if (ContainsTime(title.text))
            {
                title.text = label;
                title.gameObject.name = label + " Label";
            }
        }

        EditorSceneManager.MarkSceneDirty(copy.scene);
        return copy;
    }

    private static Vector2 AnchoredPosition(Transform t)
    {
        return t is RectTransform rect ? rect.anchoredPosition : (Vector2)t.localPosition;
    }

    private static bool HasSiblingWith<T>(Transform timeSelector) where T : MonoBehaviour
    {
        foreach (T other in Object.FindObjectsOfType<T>(true))
        {
            if (other.transform.parent == timeSelector.parent && other.gameObject.scene == timeSelector.gameObject.scene)
                return true;
        }
        return false;
    }

    /// <summary>Finds the object in the copy that sits at the same hierarchy position as 'original' does in the source.</summary>
    private static TComponent MapToCopy<TComponent>(Transform sourceRoot, Transform copyRoot, Component original) where TComponent : Component
    {
        if (original == null) return null;

        var path = new List<int>();
        for (Transform t = original.transform; t != sourceRoot; t = t.parent)
        {
            if (t == null) return null; // not inside the Time Selector
            path.Insert(0, t.GetSiblingIndex());
        }

        Transform target = copyRoot;
        foreach (int index in path)
        {
            if (index >= target.childCount) return null;
            target = target.GetChild(index);
        }
        return target.GetComponent<TComponent>();
    }

    private static void ClearPersistentListeners(Button button)
    {
        if (button == null) return;
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(button.onClick, i);
    }

    private static bool ContainsTime(string text)
    {
        return !string.IsNullOrEmpty(text) && text.IndexOf("time", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
