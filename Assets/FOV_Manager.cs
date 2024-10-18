using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FOV_Manager : MonoBehaviour
{
    private static FOV_Manager instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main_Menu_HMD")
        {
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (GameObject camera in cameras)
            {
                if (camera.GetComponent<Camera>().clearFlags == CameraClearFlags.Depth)
                {
                    gameObject.GetComponent<Canvas>().worldCamera = camera.GetComponent<Camera>();
                }
            }
        }
    }

}
