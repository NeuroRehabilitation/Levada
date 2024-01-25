using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SittoStandAnimationController : MonoBehaviour
{
    public float Fc;
    public _30sChairStandTest_Manager standTestManager;
	public GameObject person;
	public Animator anim;
    public float prev_progress=0;
    // Start is called before the first frame update

    public float _progress = 0;
    void Start()
    {
		anim.Play("sittostand");

        
    }

    // Update is called once per frame
    void Update()
    {
        _progress = standTestManager.getAvatarProgress();
        //  Debug.Log("progress" + _progress);
        if (_progress > 0.9f)
        {
            _progress = 0.9f;
        }

        if (_progress < 0)
        {
            _progress = 0;
        }

        //float alfa = (2f * (float)Math.PI * Time.deltaTime * Fc) / (2f * (float)Math.PI * Time.deltaTime * Fc + 1);

        //if (_progress > 0.2f && _progress < 0.5f)
        //{
        //    _progress = 0.49f;
        //}
        //else if (_progress > 0.49f && _progress < 0.7f)
        //{
        //    _progress = 0.69f;
        //}
        //else if (_progress > 0.69f)
        //{
        //    _progress = 1;
        //}
        //float filter_progress = _progress * alfa + prev_progress * (1 - alfa);

        anim.SetFloat ("progress", _progress);
       // prev_progress = filter_progress;
    }
}
