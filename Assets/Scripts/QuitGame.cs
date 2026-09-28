using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        // Stops play mode in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quits the built application
        Application.Quit();
#endif
    }

    // Example: call Quit() when Escape is pressed
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
    }
}