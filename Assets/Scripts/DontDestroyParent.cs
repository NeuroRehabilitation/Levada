using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyParent : MonoBehaviour
{
    // Start is called before the first frame update
    private static DontDestroyParent instance;

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
