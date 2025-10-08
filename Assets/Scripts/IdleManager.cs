using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;


public class IdleManager : MonoBehaviour
{
    public GameObject sphereLeft;
    public GameObject sphereRight;
    public GameObject cube;
    public GameObject waypoints;
    public Transform player;

    private bool isIdle = false;
    private float currentTargetAlpha = -1f;
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
                sphereLeft.SetActive(false);
                sphereRight.SetActive(false);
                cube.SetActive(false);
                waypoints.SetActive(false);
            }
            else
            {
                float alpha = currentTargetAlpha < 0f ? 0.25f : currentTargetAlpha;
                SetObjectActiveWithAlpha(sphereLeft, true, alpha);
                SetObjectActiveWithAlpha(sphereRight, true, alpha);
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
        float targetAlpha = isIdle ? 0.25f : 0f;

        if (currentTargetAlpha != targetAlpha)
        {
            currentTargetAlpha = targetAlpha;
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(HandleIdleTransition(targetAlpha, fadeDuration));
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
    }

    private IEnumerator FadeToOpacity(GameObject obj, float targetAlpha, float duration)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) yield break;

        if (!renderer.enabled && targetAlpha > 0f)
            renderer.enabled = true;

        Color color = renderer.material.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            color.a = newAlpha;
            renderer.material.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        renderer.material.color = color;

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            renderer.enabled = false;
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

    private IEnumerator HandleIdleTransition(float targetAlpha, float fadeDuration)
    {
        var objects = new List<GameObject> { sphereLeft, sphereRight, cube };
        foreach (Transform child in waypoints.transform)
        {
            objects.Add(child.gameObject);
        }

        foreach (var obj in objects)
        {
            // Start or restart fade coroutine for each object
            if (objectFadeCoroutines.TryGetValue(obj, out var runningCoroutine) && runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }
            objectFadeCoroutines[obj] = StartCoroutine(FadeToOpacity(obj, targetAlpha, fadeDuration));
        }

        yield return new WaitForSeconds(fadeDuration);
        fadeCoroutine = null;
    }
}
