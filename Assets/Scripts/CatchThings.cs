using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchThings : MonoBehaviour
{
	public GameObject _player;
	public GameObject VrManager;

	// Start is called before the first frame update
    void Start()
    {
        
    }

//    // Update is called once per frame
//    void Update()
//    {
//        
//    }

	void OnTriggerEnter(Collider other)
	{
		if (other.tag.Equals ("Player")) {
			if (gameObject.tag.Equals ("watch")) {
				VrManager.SetActive (false);
				_player.SetActive (true);
			}

			Destroy(gameObject);

		}
			
	}
}
