using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SitAnimCode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Quaternion target = Quaternion.Euler(0, transform.localRotation.y, transform.localRotation.z);
        transform.rotation = target;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
