using System.Collections;
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
    private bool wasIdle = false;
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
                sphereLeft.SetActive(true);
                sphereRight.SetActive(true);
                cube.SetActive(true);
                waypoints.SetActive(true);
                foreach (Transform child in waypoints.transform)
                {
                    SetOpacity(child.gameObject, 0);
                }
                SetOpacity(sphereLeft, 0);
                SetOpacity(sphereRight, 0);
                SetOpacity(cube, 0);
            }
        }

        checkMovement(player, playerOldPosition);

        if (!isIdle && wasIdle)
        {
            float fadeDuration = 4f;
            float targetAlpha = 0f;
            StartCoroutine(FadeToOpacity(sphereLeft, targetAlpha, fadeDuration));
            StartCoroutine(FadeToOpacity(sphereRight, targetAlpha, fadeDuration));
            StartCoroutine(FadeToOpacity(cube, targetAlpha, fadeDuration));
            foreach (Transform child in waypoints.transform)
            {
                StartCoroutine(FadeToOpacity(child.gameObject, targetAlpha, fadeDuration));
            }
        }
        else if (isIdle && !wasIdle)
        {
            float fadeDuration = 4f;
            float targetAlpha = 0.5f;
            StartCoroutine(FadeToOpacity(sphereLeft, targetAlpha, fadeDuration));
            StartCoroutine(FadeToOpacity(sphereRight, targetAlpha, fadeDuration));
            StartCoroutine(FadeToOpacity(cube, targetAlpha, fadeDuration));
            foreach (Transform child in waypoints.transform)
            {
                StartCoroutine(FadeToOpacity(child.gameObject, targetAlpha, fadeDuration));
            }
        }

        wasIdle = isIdle;
        playerOldPosition = player.transform.position;
    }

    private void SetOpacity(GameObject obj, float opacity)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;
            color.a = Mathf.Clamp01(opacity);
            renderer.material.color = color;
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
}
