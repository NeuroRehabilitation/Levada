using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

public class IdleManager : MonoBehaviour
{
    public GameObject sphereLeft;
    public GameObject sphereRight;
    public GameObject cube;
    public GameObject waypoints;

    private bool isIdle = false;
    private bool wasIdle = false;

    void Awake()
    {
        SetOpacity(sphereLeft, 0);
        SetOpacity(sphereRight, 0);
        SetOpacity(cube, 0);
    }

    void Update()
    {
        //Toggle UI
        if (Input.GetKeyDown(KeyCode.L))
        {
            bool currentlyVisible = waypoints.GetNamedChild("Sphere").gameObject.GetComponent<Renderer>().enabled;
            if (currentlyVisible)
            {
                sphereLeft.SetActive(false);
                sphereRight.SetActive(false);
                cube.SetActive(false);
                foreach (Transform child in waypoints.transform)
                {
                    child.gameObject.GetComponent<Renderer>().enabled = false;
                    SetOpacity(child.gameObject, 0f);
                }
            }
            else
            {
                sphereLeft.SetActive(true);
                sphereRight.SetActive(true);
                cube.SetActive(true);
                foreach (Transform child in waypoints.transform)
                {
                    child.gameObject.GetComponent<Renderer>().enabled = true;
                    SetOpacity(child.gameObject, 0.5f);
                }
                SetOpacity(sphereLeft, 0.5f);
                SetOpacity(sphereRight, 0.5f);
                SetOpacity(cube, 0.5f);
            }
        }

        if (isIdle && !wasIdle)
        {
            StartCoroutine(FadeToOpacity(sphereLeft, 0f, 2f));
            StartCoroutine(FadeToOpacity(sphereRight, 0f, 2f));
            StartCoroutine(FadeToOpacity(cube, 0f, 2f));
        }
        else if (!isIdle && wasIdle)
        {
            StartCoroutine(FadeToOpacity(sphereLeft, 0.5f, 2f));
            StartCoroutine(FadeToOpacity(sphereRight, 0.5f, 2f));
            StartCoroutine(FadeToOpacity(cube, 0.5f, 2f));
        }

        wasIdle = isIdle;
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

        SetMaterialToFade(renderer.material);

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

    private void SetMaterialToFade(Material mat)
    {
        mat.SetFloat("_Mode", 2);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
}
