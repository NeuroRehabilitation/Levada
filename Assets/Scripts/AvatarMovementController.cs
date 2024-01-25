using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMovementController : MonoBehaviour
{
    public GameObject camObj;
    private int prev_step = 0;
    private int current_step = 0;
    int error_secs_counter = 0;
    private Camera cam;
    IEnumerator countDown()
    {

        if (prev_step<current_step)
        {
            
            prev_step = current_step;
            error_secs_counter--;
            if (error_secs_counter < 0)
                error_secs_counter = 0;
            if (error_secs_counter < 2)// 2 secs
                cam.enabled = true;
        }
        else
        {
            error_secs_counter++;
            if (error_secs_counter > 4)// 4 seconds
                error_secs_counter = 4;
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(countDown());
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = camObj.GetComponent<Camera>();
        prev_step = current_step;
        StartCoroutine(countDown());

    }

    // Update is called once per frame
    void Update()
    {
        current_step = _2mStepTest_Manager.stepsCounter;
        if (error_secs_counter > 3)// 3 seconds
            cam.enabled = true;

    }
}
