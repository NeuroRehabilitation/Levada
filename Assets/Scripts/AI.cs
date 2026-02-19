
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class AI : MonoBehaviour 
{
	private Animation anim;
    public Transform startMarker;
    public Transform[] endMarker;
	//public EnemyShoter enemyShot;
	public Transform player;
	public bool target=false;
	bool strike=false;
    private float startTime;
    private float journeyLength;
    public int next = 0;
	//int prevSteps=0;
    //int currentSteps = 0;
    public GameObject way_points;
    public bool startWay = false;
    public  bool isReset = false;
    Vector3 cube_start_position;
    private int maximumWaypoint=116;//135
    private bool isMoving = false;
    public float stepDurationSeconds = 1.0f;
    private float stepActiveUntil = 0f;
    private float speed = 4.0f;
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
		//currentSteps = 0;
		//prevSteps = currentSteps;
        Invoke("enableWay", 1);

    }
    private void enableWay()
    {
        startWay = true;
    }
	private bool checkSteps(){
        if(_2mStepTest_Manager.stepsCounter == 1) {
			//prevSteps = currentSteps;
            _2mStepTest_Manager.stepsCounter = 0;
            float baseTime = Mathf.Max(stepActiveUntil, Time.time);
            stepActiveUntil = baseTime + Mathf.Max(0.0001f, stepDurationSeconds);
			return true;
		} else
			return false;
            
	}
    void Update() 
	{
		//currentSteps = _2mStepTest_Manager.stepsCounter;

        if (startWay)
             Movement ();

        //Debug.Log("Current Steps: " + currentSteps);
        //Debug.Log("Prev Steps: " + prevSteps);

    }

	/*void wayPointMovement ()
	{
        if (!isMoving && checkSteps())
        {
            isMoving = true;
            moveTimer = 0f;
        }

        if (!isMoving)
            return;

        moveTimer += Time.deltaTime;

        var targetPos = endMarker[next].position;
        var step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        if (next + 1 < endMarker.Length)
        {
            var targetRotation = Quaternion.LookRotation(endMarker[next + 1].position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPos) <= 0.001f)
        {
            startMarker = endMarker[next];
            journeyLength = Vector3.Distance(startMarker.position, endMarker[Next()].position);
        }

        if (moveTimer >= 1f)
        {
            isMoving = false;
        }
    }*/

    void wayPointMovement()
    {
        checkSteps();

        isMoving = Time.time < stepActiveUntil;

        if (!isMoving)
            return;

        float remainingMove = speed * Time.deltaTime;

        while (remainingMove > 0f)
        {
            Vector3 targetPos = endMarker[next].position;
            float distanceToTarget = Vector3.Distance(transform.position, targetPos);

            if (distanceToTarget <= remainingMove)
            {
                transform.position = targetPos;
                remainingMove -= distanceToTarget;

                startMarker = endMarker[next];
                journeyLength = Vector3.Distance(endMarker[next].position, endMarker[Next()].position);

                if (next > maximumWaypoint)
                    break;
            }
            else
            {
                Vector3 direction = (targetPos - transform.position).normalized;
                transform.position += direction * remainingMove;
                remainingMove = 0f;
            }

            int lookIndex = Mathf.Min(next + 1, endMarker.Length - 1);
            Vector3 lookTarget = endMarker[lookIndex].position;
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,Time.deltaTime);
        }
    }

    void Movement ()
	{
        if (next > maximumWaypoint)//135
        {
            /*next = 1;
            //fracJourney = 1f;
            isReset = true;
            gameObject.transform.position = new Vector3(endMarker[1].position.x, endMarker[1].position.y, endMarker[1].position.z);
            startMarker = endMarker[1];
            startTime = Time.time;
            journeyLength = Vector3.Distance(startMarker.position, endMarker[next].position);
            //currentSteps = 0;
            //prevSteps = currentSteps;
            isReset = false;*/

        }
        else {
            if (!isReset)
                wayPointMovement();
        }
        
        if(next == 1)
        {
            transform.position = endMarker[1].position;
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