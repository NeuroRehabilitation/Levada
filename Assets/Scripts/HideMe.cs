using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideMe : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("hideIt", 1);
        
    }
    void hideIt()
    {
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        Invoke("hideIt", 1);
    }
}
