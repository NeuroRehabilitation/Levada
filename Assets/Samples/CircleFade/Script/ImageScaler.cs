using UnityEngine;
using UnityEngine.UI;

public class ImageScaler : MonoBehaviour
{
    // Reference to the image component
    public Image image;

    [Range(0.1f,2)]
    public float FOV_Multiplier = 1;
    private float current_Multiplier;

    // Speed at which the image scales
    public float speed = 0.1f;

    // Maximum scale value
    public float _max = 1.9f;

    // Minimum scale value
    public float _min = 0.5f;

    // Variable to store the new scale
    Vector3 newScale = new Vector3();
    Vector3 start_scale = new Vector3();

    // Variable to track time elapsed
    private float timeElapsed;

    private void Start()
    {
        start_scale = image.transform.localScale;
        current_Multiplier = FOV_Multiplier;
    }

    private void Update()
    {
        if(current_Multiplier != FOV_Multiplier)
        {
            // Set the new scale
            newScale = new Vector3(image.transform.localScale.x * FOV_Multiplier, image.transform.localScale.y * FOV_Multiplier, image.transform.localScale.z * FOV_Multiplier);

            Debug.Log(newScale);

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


            // Calculate the difference in scale between current and target scale
            //Vector3 scale_Now = newScale - image.transform.localScale;

            // Normalize the scale difference vector
            //scale_Now.Normalize();

            // Apply the scaled difference to the image transform with speed
            image.transform.localScale = newScale; //* speed * Time.deltaTime;


            current_Multiplier = FOV_Multiplier;
        }

        
    }
}
