using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterFlySpeed : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animation>()["Take 001"].speed = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
