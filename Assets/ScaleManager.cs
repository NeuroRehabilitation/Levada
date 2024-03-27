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
    private GameObject UI_Controller;
    private GameObject Teleport_Controller;

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

            UI_Controller = GameObject.FindGameObjectWithTag("UI_Controller");
            Teleport_Controller = GameObject.FindGameObjectWithTag("Teleport_Controller");

            SetTeleportController();

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

    private void AdjustCameraSettings(GameObject mainCamera, Canvas scaleCanvas)
    {

        int offset = 10;

        Transform xr_origin = GameObject.Find("XR Origin").transform;

        //Adjust the rotation of the Canvas according to the XR Origin rotation
        Quaternion desiredRotation = xr_origin.rotation;

        //Get current XR Origin maincamera position 
        Vector3 xr_position = xr_origin.position;

        //Get the forward direction from the maincamera
        Vector3 xr_forward = xr_origin.forward.normalized * offset;
        
        //Calculate final position for the canvas
        Vector3 new_position = new Vector3(xr_position.x + xr_forward.x, xr_position.y + xr_forward.y, xr_position.z + xr_forward.z);
        


        int layerIndex = LayerMask.NameToLayer("UI");
        LayerMask layerMask = 1 << layerIndex;

        mainCamera.GetComponent<Camera>().cullingMask = layerMask;
        mainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        mainCamera.GetComponent<Camera>().backgroundColor = Color.black;

        scaleCanvas.transform.position = new_position;
        scaleCanvas.transform.rotation = desiredRotation;
    }

    private void ResetCameraSettings(GameObject mainCamera)
    {
        int layerIndex = LayerMask.NameToLayer("UI");

        mainCamera.GetComponent<Camera>().cullingMask = ~(1 << layerIndex);
        mainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
    }

    private IEnumerator ShowScale()
    {
        while (Manager.isRunning)
        {
            yield return new WaitForSeconds(interval*60);

            AdjustCameraSettings(mainCamera, scaleCanvas);

            while (!SAM.submitButtonPressed)
            {
                SetUIController();
                FOV_Image.enabled = false;
                scaleCanvas.enabled = true;
                
                yield return new WaitUntil(() => SAM.submitButtonPressed);
            }

            SetTeleportController();
            ResetCameraSettings(mainCamera);

            scaleCanvas.enabled = false;
            SAM.submitButtonPressed = false;
            FOV_Image.enabled = true;
        }
    }

    private void SetUIController()
    {
        if(UI_Controller != null)
            UI_Controller.SetActive(true);
        if(Teleport_Controller != null)
            Teleport_Controller.SetActive(false);
    }

    private void SetTeleportController()
    {
        if (UI_Controller != null)
            UI_Controller.SetActive(false);
        if (Teleport_Controller != null)
            Teleport_Controller.SetActive(true);
    }
}
