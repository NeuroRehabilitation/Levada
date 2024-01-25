
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class AI : MonoBehaviour 
{
	private Animation anim;
    public Transform startMarker;
    public Transform[] endMarker;
    public float speed = 1.0F;
	//public EnemyShoter enemyShot;
	public Transform player;
	public bool target=false;
	bool strike=false;
    private float startTime;
    private float journeyLength;
    public int next = 0;
	int prevSteps=0;
    int currentSteps = 0;
    public GameObject way_points;
    public bool startWay = false;
    public  bool isReset = false;
    Vector3 cube_start_position;
    public float fracJourney = 0;
    private int maximumWaypoint=116;//135
    void Start() 
	{
        cube_start_position = transform.position;
        endMarker = way_points.GetComponentsInChildren<Transform>();
        maximumWaypoint = endMarker.Length - 2;
        //player = GameObject.FindWithTag ("Player").transform;
        startTime = Time.time;
        journeyLength = Vector3.Distance(startMarker.position, endMarker[Next()].position);
        

        anim = GetComponent<Animation> ();
		if(anim)
			anim.Play ();
		currentSteps = 0;
		prevSteps = currentSteps;
        Invoke("enableWay", 1);

    }
    private void enableWay()
    {
        startWay = true;
    }
	private bool checkSteps(){
        if(currentSteps > prevSteps) {
			prevSteps = currentSteps;
			return true;
		} else
			return false;
            
	}
    void Update() 
	{
		currentSteps = _2mStepTest_Manager.stepsCounter;

        if (startWay)
             Movement ();

    }

	void wayPointMovement ()
	{
        float distCovered = (Time.time - startTime) * speed;
         fracJourney = (float)distCovered / journeyLength;
        transform.position = Vector3.Lerp(startMarker.position, endMarker[next].position, fracJourney);
//******
        var targetRotation = Quaternion.LookRotation(endMarker[next+1].position - transform.position);

        // Smoothly rotate towards the target point.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 * Time.deltaTime);
//*******
        //transform.LookAt (endMarker [next].position);
      //  transform.localRotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up);
        //transform.localRotation = Quaternion.AngleAxis(endMarker [next].rotation.eulerAngles.y, Vector3.up);

        if (Vector3.Distance(this.transform.position, endMarker[next].position) <= 0f && checkSteps())
            {

                startTime = Time.time;
                startMarker = endMarker[next];
                journeyLength = Vector3.Distance(endMarker[next].position, endMarker[Next()].position);
            }
        
        
    }

    void Movement ()
	{
        if (next > maximumWaypoint)//135
        {
            next = 1;
            //fracJourney = 1f;
            isReset = true;
            gameObject.transform.position = new Vector3(endMarker[1].position.x, endMarker[1].position.y, endMarker[1].position.z);
            startMarker = endMarker[1];
            startTime = Time.time;
            journeyLength = Vector3.Distance(startMarker.position, endMarker[next].position);
            currentSteps = 0;
            prevSteps = currentSteps;
            isReset = false;

        }
        else {
            if (!isReset)
                wayPointMovement();
        }
        
                
			
	}
   
    //	void PlayerShoot ()
    //	{
    //		if(GetComponent<EnemyHealth> ().isDead!=true)
    //		GetComponentInChildren<AnimationController> ().Shot ();
    //	}

    //	void OnTriggerEnter(Collider other)
    //	{
    //		if (other.gameObject.CompareTag ("Player") && !GetComponent<EnemyHealth> ().isDead) 
    //		{
    //			if (!strike) {
    //				GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ().Check ();
    //				strike = true;
    //			}
    //		    target = true;
    //		}
    //	}
    int Next()
    {
        // if (next + 1 == endMarker.Length)
        //     SceneManager.LoadScene(0);
        next++;
        if (next > maximumWaypoint+1)//160
            next = 0;
        //next = (next + 1) % 5;//endMarker.Length;
       

        return next;	
	}
    
    private void OnTriggerEnter(Collider other)
    {
        //if (other.transform.tag.Equals("r1"))
        //{
        //    Debug.Log("entered ***");
        //    Quaternion target = Quaternion.Euler(0, 95, 0);

        //    gameObject.transform.localRotation = target;

        //}

        //if (other.transform.tag.Equals("r2"))
        //{
        //    Debug.Log("entered ***");
        //    Quaternion target = Quaternion.Euler(0, 119, 0);

        //    gameObject.transform.localRotation = target;

        //}
    }
}