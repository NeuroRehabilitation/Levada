using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
