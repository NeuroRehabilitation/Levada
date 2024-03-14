using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScaleManager : MonoBehaviour
{
    public static ScaleManager instance;

    [Header("Interval to Show Canvas (minutes)")]
    public float interval = 1;

    private Canvas scaleCanvas;
    private bool coroutineStarted = false;

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

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        scaleCanvas = gameObject.GetComponent<Canvas>();
    }

    private void Update()
    {
        if (Manager.isRunning && !coroutineStarted)
        {
            StartCoroutine(ShowScale());
            coroutineStarted = true;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main_Menu_HMD")
        {
            StartCoroutine(ShowScale());
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach(GameObject camera in cameras)
            {
                if(camera.GetComponent<Camera>().clearFlags == CameraClearFlags.Depth)
                {
                    gameObject.GetComponent<Canvas>().worldCamera = camera.GetComponent<Camera>();
                }
            }
        }
    }

    private IEnumerator ShowScale()
    {
        while (Manager.isRunning)
        {
            Debug.Log("ola");
            yield return new WaitForSeconds(5.0f);

            while (!SAM.submitButtonPressed)
            {
                Debug.Log("Waiting for Response");
                scaleCanvas.enabled = true;
                yield return new WaitUntil(() => SAM.submitButtonPressed);
            }

            scaleCanvas.enabled = false;
            SAM.submitButtonPressed = false;
            Debug.Log("Disappear Canvas");

        }
    }
}
