using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageScaler : MonoBehaviour
{
    // Reference to the image component
    [SerializeField]
    private Image image;

    public static ImageScaler instance;

    [Range(0.1f,1)]
    public float FOV_Multiplier = 1.0f;
    public float current_Multiplier;
    private float lastMultiplier = 0.0f;

    // Maximum scale value
    [Header("Maximum Scale")]
    public float _max = 2.3f;

    // Minimum scale value
    [Header("Minimum Scale")]
    public float _min = 0.62f;

    private float startTime;

    // Variable to store the new scale
    Vector3 newScale = new Vector3();
    Vector3 start_scale = new Vector3();

    private void Start()
    {
        start_scale = image.transform.localScale;
        current_Multiplier = FOV_Multiplier;
        lastMultiplier = current_Multiplier;

        image.transform.localScale = start_scale * current_Multiplier;

        image.enabled = false;

        startTime = Time.time;

        StartCoroutine(UpdateScale());
    }

    private IEnumerator UpdateScale()
    {
        while (true)
        {
            yield return new WaitUntil(() => current_Multiplier != lastMultiplier);

            // Set the new scale
            newScale = new Vector3(start_scale.x * current_Multiplier, start_scale.y * current_Multiplier, start_scale.z * current_Multiplier);

            // Ensure real scale stays within bounds
            if (newScale.x > _max)
            {
                newScale.x = _max;
                newScale.y = _max;
            }
            else if (newScale.x < _min)
            {
                newScale.x = _min;
                newScale.y = _min;
            }

            float t = (Time.time - startTime) / 0.5f;

            //Gradually change between FOV
            image.transform.localScale = Vector3.Lerp(image.transform.localScale, newScale, t);

            lastMultiplier = current_Multiplier;
        }
    }
}
