using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScaleManager : MonoBehaviour
{
    public static ScaleManager instance;

    [Header("Interval to Show Canvas (minutes)")]
    public float interval = 1;

    private Canvas scaleCanvas;
    private bool coroutineStarted = false;
    private GameObject FOV;
    private Image FOV_Image;
    private GameObject mainCamera;

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
        FOV = GameObject.Find("FOV");
        FOV_Image = FOV.GetComponentInChildren<Image>();
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
                if(camera.GetComponent<Camera>().clearFlags == CameraClearFlags.Skybox)
                {
                    mainCamera = camera;
                    gameObject.GetComponent<Canvas>().worldCamera = camera.GetComponent<Camera>();
                }
            }
        }
    }

    private IEnumerator ShowScale()
    {
        while (Manager.isRunning)
        {
            yield return new WaitForSeconds(interval*60);

            Vector3 main_camera_position = mainCamera.transform.position;
            Vector3 new_position = new Vector3(main_camera_position.x, main_camera_position.y+1, main_camera_position.z + 5);
            Quaternion desiredRotation = GameObject.Find("XR Origin").transform.rotation;

            scaleCanvas.transform.position = new_position;
            scaleCanvas.transform.rotation = desiredRotation;

            while (!SAM.submitButtonPressed)
            {
                FOV_Image.enabled = false;
                scaleCanvas.enabled = true;

                yield return new WaitUntil(() => SAM.submitButtonPressed);
            }

            scaleCanvas.enabled = false;
            SAM.submitButtonPressed = false;
            FOV_Image.enabled = true;
        }
    }
}
