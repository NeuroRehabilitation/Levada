using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class IdleManager : MonoBehaviour
{
    public GameObject kneeLeft;
    public GameObject kneeRight;
    public GameObject cube;
    public GameObject waypoints;
    public Transform player;

    private bool isIdle = false;
    private float currentTargetAlpha = -1f;
    private float currentKneeTargetAlpha = -1f;
    private Coroutine fadeCoroutine = null;
    private Dictionary<GameObject, Coroutine> objectFadeCoroutines = new Dictionary<GameObject, Coroutine>(); // Track one fade coroutine per object to avoid overlap
    Vector3 playerOldPosition;

    void Update()
    {
        //Toggle UI
        if (Input.GetKeyDown(KeyCode.L))
        {
            bool currentlyVisible = cube.activeSelf;
            if (currentlyVisible)
            {
                kneeLeft.SetActive(false);
                kneeRight.SetActive(false);
                cube.SetActive(false);
                waypoints.SetActive(false);
            }
            else
            {
                float alpha = currentTargetAlpha < 0f ? 0.25f : currentTargetAlpha;
                SetObjectActiveWithAlpha(kneeLeft, true, alpha);
                SetObjectActiveWithAlpha(kneeRight, true, alpha);
                SetObjectActiveWithAlpha(cube, true, alpha);
                waypoints.SetActive(true);
                foreach (Transform child in waypoints.transform)
                {
                    SetObjectActiveWithAlpha(child.gameObject, true, alpha);
                }
            }
        }

        checkMovement(player, playerOldPosition);

        float fadeDuration = 4f;
        float otherTargetAlpha = isIdle ? 0.25f : 0f;
        float kneeTargetAlpha = isIdle ? 0f : 0.25f;

        if (currentTargetAlpha != otherTargetAlpha || currentKneeTargetAlpha != kneeTargetAlpha)
        {
            currentTargetAlpha = otherTargetAlpha;
            currentKneeTargetAlpha = kneeTargetAlpha;
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(HandleIdleTransition(otherTargetAlpha, kneeTargetAlpha, fadeDuration));
        }

        playerOldPosition = player.transform.position;
    }

    private void SetObjectActiveWithAlpha(GameObject obj, bool active, float alpha)
    {
        obj.SetActive(active);
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;
            color.a = Mathf.Clamp01(alpha);
            renderer.material.color = color;
            renderer.enabled = alpha > 0f;
        }

        Image image = obj.GetComponent<Image>();
        if (image != null)
        {
            Color ic = image.color;
            ic.a = Mathf.Clamp01(alpha);
            image.color = ic;
            image.enabled = alpha > 0f;
        }
    }

    private IEnumerator FadeToOpacity(GameObject obj, float targetAlpha, float duration)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(includeInactive: true);
        Image[] images = obj.GetComponentsInChildren<Image>(includeInactive: true);

        if ((renderers == null || renderers.Length == 0) && (images == null || images.Length == 0))
            yield break;

        if (targetAlpha > 0f)
        {
            foreach (var r in renderers) if (r != null) r.enabled = true;
            foreach (var img in images) if (img != null) img.enabled = true;
        }

        Color[] startRendererColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) startRendererColors[i] = renderers[i].material.color;

        Color[] startImageColors = new Color[images.Length];
        for (int i = 0; i < images.Length; i++) startImageColors[i] = images[i].color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;
                Color c = startRendererColors[i];
                c.a = Mathf.Lerp(startRendererColors[i].a, targetAlpha, t);
                r.material.color = c;
            }

            for (int i = 0; i < images.Length; i++)
            {
                var img = images[i];
                if (img == null) continue;
                Color c = startImageColors[i];
                c.a = Mathf.Lerp(startImageColors[i].a, targetAlpha, t);
                img.color = c;
            }

            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;
            Color c = startRendererColors[i];
            c.a = targetAlpha;
            r.material.color = c;
            if (Mathf.Approximately(targetAlpha, 0f)) r.enabled = false;
        }

        for (int i = 0; i < images.Length; i++)
        {
            var img = images[i];
            if (img == null) continue;
            Color c = startImageColors[i];
            c.a = targetAlpha;
            img.color = c;
            if (Mathf.Approximately(targetAlpha, 0f)) img.enabled = false;
        }

        objectFadeCoroutines.Remove(obj);
    }

    private void checkMovement(Transform player, Vector3 oldPosition)
    {
        Vector3 playerPosition = player.transform.position;
        if (playerPosition != oldPosition)
        {
            isIdle = false;
            playerOldPosition = playerPosition;
        }
        else
        {
            isIdle = true;
        }

    }

    private IEnumerator HandleIdleTransition(float otherTargetAlpha, float kneeTargetAlpha, float fadeDuration)
    {
        var kneeObjects = new List<GameObject> { kneeLeft, kneeRight };
        foreach (var obj in kneeObjects)
        {
            if (obj == null) continue;
            if (objectFadeCoroutines.TryGetValue(obj, out var runningCoroutine) && runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }
            objectFadeCoroutines[obj] = StartCoroutine(FadeToOpacity(obj, kneeTargetAlpha, fadeDuration));
        }

        var otherObjects = new List<GameObject>();
        if (cube != null) otherObjects.Add(cube);
        if (waypoints != null)
        {
            foreach (Transform child in waypoints.transform)
            {
                otherObjects.Add(child.gameObject);
            }
        }

        foreach (var obj in otherObjects)
        {
            if (obj == null) continue;
            if (objectFadeCoroutines.TryGetValue(obj, out var runningCoroutine) && runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }
            objectFadeCoroutines[obj] = StartCoroutine(FadeToOpacity(obj, otherTargetAlpha, fadeDuration));
        }

        yield return new WaitForSeconds(fadeDuration);
        fadeCoroutine = null;
    }
}
