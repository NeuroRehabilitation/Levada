using UnityEngine;
using UnityEngine.UI;

public class ImageScaler : MonoBehaviour
{
    // Reference to the image component
    [SerializeField]
    private Image image;

    [Range(0.1f,1)]
    public float FOV_Multiplier = 1;
    private float current_Multiplier;

    // Maximum scale value
    private float _max = 2.3f;

    // Minimum scale value
    private float _min = 0.5f;

    // Variable to store the new scale
    Vector3 newScale = new Vector3();
    Vector3 start_scale = new Vector3();

    private void Start()
    {
        start_scale = image.transform.localScale;
        current_Multiplier = FOV_Multiplier;

        image.transform.localScale = start_scale * current_Multiplier;
    }

    private void Update()
    {
        if(current_Multiplier != FOV_Multiplier)
        {
            // Set the new scale
            newScale = new Vector3(start_scale.x * FOV_Multiplier, start_scale.y * FOV_Multiplier, start_scale.z * FOV_Multiplier);

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

            // Apply the scaled difference to the image transform with speed
            image.transform.localScale = newScale;

            current_Multiplier = FOV_Multiplier;
        }
    }
}
