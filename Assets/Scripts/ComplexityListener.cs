using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComplexityListener : MonoBehaviour
{
    public static int hike_level = 0;
    public GameObject waitPanel;
    // Start is called before the first frame update
    string env_selection ="";
    void Start()
    {
        env_selection  = PlayerPrefs.GetString("environment_selection", "");
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetButtonUp("Fire1") || Input.GetKeyUp(KeyCode.A))
        {
            hike_level = 0;
            Debug.Log("env selection"+ env_selection);
            SceneManager.LoadScene(env_selection);
            waitPanel.SetActive(true);
        }
        */

        if (Input.GetButtonUp("Fire2") || Input.GetKeyUp(KeyCode.S))
        {
            hike_level = 1;
            Debug.Log("env selection"+ env_selection);
            waitPanel.SetActive(true);

            SceneManager.LoadScene(env_selection);
        }

       /* else if (Input.GetButtonUp("Fire3") || Input.GetKeyUp(KeyCode.D))
        {

            hike_level = 2;
            Debug.Log("env selection"+ env_selection);
            waitPanel.SetActive(true);
            SceneManager.LoadScene(env_selection);

        }
        */
        else if (Input.GetButtonUp("Fire4") || Input.GetKeyUp(KeyCode.F))
        {


            waitPanel.SetActive(true);

            SceneManager.LoadScene(0);// main menu


        }
       
    }
}
