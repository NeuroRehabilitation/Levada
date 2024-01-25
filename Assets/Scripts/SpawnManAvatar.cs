using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManAvatar : MonoBehaviour
{
    public GameObject man_avatar;
    public GameObject[] thighs_ankles;
    Transform left_thigh;
    Transform right_thigh;
    Transform left_knee;
    Transform right_knee;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(man_avatar, man_avatar.transform.position, man_avatar.transform.rotation);
        left_thigh =  GameObject.FindGameObjectWithTag("left_thigh").transform;
        right_thigh =  GameObject.FindGameObjectWithTag("right_thigh").transform;
        left_knee =  GameObject.FindGameObjectWithTag("left_knee").transform;
        right_knee =  GameObject.FindGameObjectWithTag("right_knee").transform;

    }

    // Update is called once per frame
    void Update()
    {
        //Quaternion target_thigh = Quaternion.Euler(thighs_ankles[0].transform.localRotation.z, transform.localRotation.y, transform.localRotation.z);
        //Quaternion target_knee = Quaternion.Euler(-0.05f, transform.localRotation.y, transform.localRotation.z);


        left_thigh.localEulerAngles = new Vector3(thighs_ankles[0].transform.localEulerAngles.z*-1,0, 0);//thighs_ankles[0].transform.localEulerAngles.z);
       right_thigh.localEulerAngles = new Vector3(thighs_ankles[0].transform.localEulerAngles.z * -1, 0, 0);//thighs_ankles[0].transform.localEulerAngles.z);
        left_knee.localEulerAngles = new Vector3(thighs_ankles[1].transform.localEulerAngles.z , 0, 0);//thighs_ankles[0].transform.localEulerAngles.z);
        right_knee.localEulerAngles = new Vector3(thighs_ankles[1].transform.localEulerAngles.z, 0, 0);//thighs_ankles[0].transform.localEulerAngles.z);

        //right_thigh.rotation = target_thigh;
        //left_knee.rotation = target_knee;
        //right_knee.rotation = target_knee;
        //Debug.Log("knee rotation******" + thighs_ankles[1].transform.localRotation.z);
    }
}
