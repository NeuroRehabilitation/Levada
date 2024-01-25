using System;
using System.Collections.Generic;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class _2mStepTest_Manager : MonoBehaviour
{
    public GameObject nonVR;
    public Transform cube;
    public Transform vrManager;

    //********
    public GameObject left_ball,
    right_ball;
    public bool isTrial = false;
    int start_counter = 4;
    public Text lblCountDown;
    public Material avatar_gesture_detection;
    public Transform []players;
    private Vector3 current_player;

    private string [] type_test = {"easy", "medium", "hard" };
    private string test_name;

    public GameObject testOverPanel;
    public GameObject[] modePanels;// by me
    public GameObject[] levelMode;// by me
    public bool isLongWalkTest = false;// by me
    public static int stepsCounter;
	// Kinect 
    private KinectSensor _kinectSensor;

    private BodyFrameReader _bodyFrameReader;
    private int _bodyCount;
    private Body[] _bodies;

    /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
    private List<GestureDetectors> _gestureDetectorList;

    // Test
    private int _subjectBodyIndex = -1;
    private float _2mTimer;
    //private const float TestDuration = 120f;
    public float TestDuration = 600;//120f;
    public BoxCollider TargetHeightCollider;
    public BoxCollider FloorLevelCollider;
    private TargetHeight _targetHeight;
    //private FloorLevel _floorLevel;
    private JointOrientationControl2M _subjectAvatarOrientationControl;
    public BodySourceViewer2M BodySourceViewer;
    private Test _test;
    public static float height;
    public static float maxHeight = 10;
    public static float minHeight = 0;


    public TargetFeedback FloorLevel;
    public TargetFeedback TargetLevel;
    public struct Test
    {
        public int RSteps;
        public bool LeftFootIsUp;
        public bool LeftFootIsDown;
        public bool LeftStepIsValid;
        public bool RightFootIsUp;
        public bool RightFootIsDown;
        public bool RightStepIsValid;
    }

    private _2mStepTest_Logger _logger = new _2mStepTest_Logger();

    // State
    private bool _gesturePause = true;
    private State _state = State.GetSubject;
    private SubState _subState = SubState.Start;
    private SubState _nextSubState = SubState.Start;
    private bool isCouroutineStatred = false; // by me
    private enum State
    {
        GetSubject,
        Instruct,
        Test,
        Result,
        Abort,
        Problem
    }

    private enum SubState
    {
        Start,
        Update,
        Idle,
        Next
    }

    // GUI
    public Button NextButton;
    public Button AbortButton;
    public Text GuitText;

    // FeedBack
    public AbstractFeedbackScale AbstractFeedbackLeft;
    public AbstractFeedbackScale AbstractFeedbackRight;
    IEnumerator countDown()
    {

        start_counter--;
        lblCountDown.gameObject.SetActive(true);

        yield return new WaitForSeconds(1);
        if (start_counter < 1)
        {
            lblCountDown.gameObject.SetActive(false);
            start_counter = 0;
            StopCoroutine("countDown");
        }
        else
        {

            if (start_counter > 1)
                lblCountDown.text = "Pronto";//Ready;
            else
                lblCountDown.text = "Comece";//Go
            StartCoroutine(countDown());

            if(GameBLESender.Instance!=null)
                GameBLESender.Instance.SendString("GameVariable,Start");
        }
        


    }
    void Start()
    {
        Cursor.visible = false;

        if(GameBLESender.Instance != null)
            GameBLESender.Instance.Init();

        avatar_gesture_detection.color = Color.red;

        if (ComplexityListener.hike_level < modePanels.Length && isLongWalkTest == true) // if it is a 6m step test with three levels
        {
            test_name = type_test[ComplexityListener.hike_level];
            // modePanels[ComplexityListener.hike_level].SetActive(true);
            if (!MainMenu.isnonVR)
                levelMode[ComplexityListener.hike_level].SetActive(true);
            else
                nonVR.SetActive(true);
            current_player = players[ComplexityListener.hike_level].position;
           
        }

        if (!isLongWalkTest) // if it is a two mintute step test
        {
            test_name = "2MStepTest";
            current_player = players[0].position;// if it is a two inute step test
        }

        if (isTrial == true)
        {
            test_name = "trial";
        }
           
        isCouroutineStatred = false;
        //TestDuration = 600;// ten minutes
        stepsCounter = 0;
		InitializeVariables();
        _targetHeight = TargetHeightCollider.GetComponent<TargetHeight>();
        //_floorLevel = FloorLevelCollider.GetComponent<FloorLevel>();

        // get the sensor object
        _kinectSensor = KinectSensor.GetDefault();

        if (_kinectSensor != null)
        {
            _bodyCount = _kinectSensor.BodyFrameSource.BodyCount;

            // body data
            _bodyFrameReader = _kinectSensor.BodyFrameSource.OpenReader();

            // body frame to use
            _bodies = new Body[_bodyCount];

            // initialize the gesture detection objects for our gestures
            _gestureDetectorList = new List<GestureDetectors>();
            for (int bodyIndex = 0; bodyIndex < _bodyCount; bodyIndex++)
            {
                _gestureDetectorList.Add(new GestureDetectors(_kinectSensor, SFTTest._2mST));
            }

            // start getting data from runtime
            _kinectSensor.Open();
        }
       
    }
     
     void Update()
     {
        if(GameBLESender.Instance != null)
            GameBLESender.Instance.SendString("GameVariable,Height,float," + height + "," + minHeight +","+maxHeight);

        _targetHeight.SetHeight(height);

       // Debug.Log("Height = " + height);

        if (!isLongWalkTest) // if it is a two mintute step test
        {
            current_player = players[0].position;// if it is a two inute step test
        }
        else
        {
            current_player = players[ComplexityListener.hike_level].position;

        }
        if (Input.GetButtonUp("Fire1")|| Input.GetKeyUp(KeyCode.A))
        {
            Time.timeScale = 1;

            OnNextButton();


        }
        if (Input.GetButtonUp ("Fire2")|| Input.GetKeyUp(KeyCode.S)) {

            Time.timeScale = 1;
            OnAbortButton();
        }
       
    }
    void FixedUpdate()
    {


        ManageGestureDetectors();

        switch (_state)
        {
            case State.GetSubject:
                GetSubject();
                break;

            case State.Instruct:
                if (!_bodies[_subjectBodyIndex].IsTracked)
                {
                    _state = State.Problem;
                    _subState = SubState.Start;
                    break;
                }

                Instruct();
                break;

            case State.Test:
                if (!_bodies[_subjectBodyIndex].IsTracked)
                {
                    _state = State.Problem;
                    _subState = SubState.Start;
                    break;
                }

                Testing();
                break;

            case State.Result:
                Result();
                break;

            case State.Abort:
                Abort();
                break;

            case State.Problem:
                Problem();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Problem()
    {
        switch (_subState)
        {
            case SubState.Start:
                _logger.Close();
                GuitText.text = "Subject tracking has been lost";
                AbortButton.onClick.RemoveAllListeners();
                AbortButton.onClick.AddListener(OnExitButton);
                AbortButton.GetComponentInChildren<Text>().text = "Get out";

                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(OnRestartButton);
                NextButton.interactable = true;
                NextButton.GetComponentInChildren<Text>().text = "Restart";

                _subState = SubState.Idle;
                avatar_gesture_detection.color = Color.red;

                break;

            case SubState.Update:
                break;

            case SubState.Idle:
                break;

            case SubState.Next:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Abort()
    {
        switch (_subState)
        {
            case SubState.Start:
                _logger.Close();
                GuitText.text = "Test canceled, restart or exit?";

                AbortButton.onClick.RemoveAllListeners();
                AbortButton.onClick.AddListener(OnExitButton);
                AbortButton.GetComponentInChildren<Text>().text = "Quit";

                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(OnRestartButton);
                NextButton.interactable = true;
                NextButton.GetComponentInChildren<Text>().text = "Restart";
                avatar_gesture_detection.color = Color.red;
                _subState = SubState.Idle;
                break;

            case SubState.Update:
                break;

            case SubState.Idle:
                break;

            case SubState.Next:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Result()
    {
        switch (_subState)
        {
            case SubState.Start:
                GuitText.text = String.Format("Número de passos: {0}", _test.RSteps);
                _subState = SubState.Idle;
                break;

            case SubState.Update:
                Debug.Log("Something went wrong, code should't go here");
                break;

            case SubState.Idle:
                //Just waits for next button
                _nextSubState = SubState.Next;
                break;

            case SubState.Next:
                SceneManager.LoadScene(0);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Testing()
    {
        switch (_subState)
        {
            case SubState.Start:

                if (start_counter < 1)
                {
                    AbstractFeedbackLeft.PlaySound(2);
                    _2mTimer = TestDuration;
                    GuitText.text = String.Format("Start taking steps\n");
                    _subState = SubState.Update;
                    _nextSubState = SubState.Update;
                    NextButton.interactable = false;
                    _gesturePause = false;
                }
                if (start_counter > 3)
                {
                    start_counter--;
                    StartCoroutine(countDown());
                }

                break;

            case SubState.Update:

                
                if (!isCouroutineStatred && isLongWalkTest)
                {
                    isCouroutineStatred = true;
                    isLongWalkTest = true;
                    StartCoroutine("upDateDifficulty", 1);

                }

               
                _logger.Log(test_name,_2mTimer, _test, FloorLevelCollider, TargetHeightCollider, _subjectAvatarOrientationControl, current_player);
                SetMarkers();
               // _2mTimer -= Time.fixedDeltaTime; //to keep loop infinite
                if (this._test.LeftFootIsDown || this._test.RightFootIsDown) { this.FloorLevel.SetMaterial(TargetFeedback.Colors.Green); }
                else { this.FloorLevel.SetMaterial(TargetFeedback.Colors.White); }
                if (this._test.LeftFootIsUp || this._test.RightFootIsUp) { this.TargetLevel.SetMaterial(TargetFeedback.Colors.Green); }
                else { this.TargetLevel.SetMaterial(TargetFeedback.Colors.White); }

                GuitText.text = String.Format("Comece a dar passos\n{0:f2} segundos\nNúmero de passos: {1}", _2mTimer,
                _test.RSteps);
                avatar_gesture_detection.color = Color.green;

                if (_2mTimer <= 0)
                {
                    avatar_gesture_detection.color = Color.red;

                    //AbstractFeedbackLeft.PlaySound(2);
                    _subState = SubState.Next;
                    NextButton.interactable = true;
                    NextButton.GetComponentInChildren<Text>().text = "Próximo";
                    testOverPanel.SetActive(true);
                }

                if (isTrial)
                {
                    if (_test.RSteps >= 10)
                    {
                        avatar_gesture_detection.color = Color.red;

                        //AbstractFeedbackLeft.PlaySound(2);
                        _subState = SubState.Next;
                        NextButton.interactable = true;
                        NextButton.GetComponentInChildren<Text>().text = "Próximo";
                        testOverPanel.SetActive(true);
                    }
                }
                break;

            case SubState.Idle:
                Debug.Log("Something went wrong, code should't go here");
                break;

            case SubState.Next:
                _logger.Close();
                _gesturePause = true;
                _state = State.Result;
                _subState = SubState.Start;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Instruct()
    {
        switch (_subState)
        {
            case SubState.Start:
                GuitText.text = "Instruções:\n\n" +
                                "Ao sinal sonoro comece a dar passos no lugar (sem correr)\n\n" +
                                "Levante os joelhos até a altura indicada de maneira alternada\n\n" +
                                "Tem 2 minutos para dar o maior número de passos.";
                _subState = SubState.Idle;
                break;

            case SubState.Update:
                Debug.Log("Something went wrong, code should't go here");
                break;

            case SubState.Idle:
                //Just waits for next button
                NextButton.GetComponentInChildren<Text>().text = "Começar";
                _nextSubState = SubState.Next;
                break;

            case SubState.Next:
                _state = State.Test;
                _subState = SubState.Start;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void GetSubject()
    {
        switch (_subState)
        {
            case SubState.Start:
                GuitText.text = "Sujeito: Assuma a pose em T.";
                _gesturePause = false;
                _subState = SubState.Idle;
                break;

            case SubState.Update:
                Debug.Log("Something went wrong, code should't go here");
                break;

            case SubState.Idle:
                GuitText.text = String.Format("Sujeito: Assuma a pose em T.\nSujeito: {0}", _subjectBodyIndex);
                if (_subjectBodyIndex < 0 || _subjectBodyIndex >= _bodyCount)
                {
                    NextButton.interactable = false;
                }
                else if (!_bodies[_subjectBodyIndex].IsTracked)
                {
                    NextButton.interactable = false;
                    _subjectBodyIndex = -1;
                }
                else
                {
                    NextButton.interactable = true;
                    avatar_gesture_detection.color = Color.yellow;
                }
                _nextSubState = SubState.Next;
                break;

            case SubState.Next:
                _gesturePause = true;
                if (!_bodies[_subjectBodyIndex].IsTracked)
                {
                    _state = State.Problem;
                    _subState = SubState.Start;
                    break;
                }
                BodySourceViewer.SetBodyVisible(_subjectBodyIndex);
                _state = State.Instruct;
                _subState = SubState.Start;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ManageGestureDetectors()
    {
        // ensure the readers are valid
        if (_bodyFrameReader != null)
        {
            // process bodies
            bool newBodyData = false;
            using (BodyFrame bodyFrame = _bodyFrameReader.AcquireLatestFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(_bodies);
                    newBodyData = true;
                }
            }

            if (newBodyData)
            {
                // update gesture detectors with the correct tracking id
                for (int bodyIndex = 0; bodyIndex < _bodyCount; bodyIndex++)
                {
                    var body = _bodies[bodyIndex];
                    if (body != null)
                    {
                        var trackingId = body.TrackingId;

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != _gestureDetectorList[bodyIndex].TrackingId)
                        {
                            _gestureDetectorList[bodyIndex].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            //_gestureDetectorList[bodyIndex].IsPaused = (trackingId == 0);
                            _gestureDetectorList[bodyIndex].OnDiscreteGestureDetected += CreateOnDiscreteGestureHandler(bodyIndex);
                        }
                        _gestureDetectorList[bodyIndex].IsPaused = _gesturePause || (trackingId == 0);
                        if (bodyIndex != _subjectBodyIndex && _state != State.GetSubject)
                        {
                            _gestureDetectorList[bodyIndex].IsPaused = true;
                        }
                    }
                }
            }
        }
    }

    private EventHandler<DiscreteGestureEventArgs> CreateOnDiscreteGestureHandler(int bodyIndex)
    {
        return (object sender, DiscreteGestureEventArgs e) => OnDiscreteGestureDetected(sender, e, bodyIndex);
    }

    private void OnDiscreteGestureDetected(object sender, DiscreteGestureEventArgs e, int bodyIndex)
    {
        var isDetected = e.IsBodyTrackingIdValid && e.IsGestureDetected;

        if (isDetected && e.GestureName == "poseT")
        {
            _subjectBodyIndex = bodyIndex;

            _subjectAvatarOrientationControl =
                BodySourceViewer.Avatars[_subjectBodyIndex].GetComponent<JointOrientationControl2M>();

            //if (!isLongWalkTest)
            {
                height = CalculateMidThighHeight();
                //height += height / 6; 
                //height = CalculateKneeHeight() + (3 * (CalculateHipHeight() - CalculateKneeHeight()) / 4);
                minHeight = CalculateKneeHeight() + (1 * (CalculateHipHeight() - CalculateKneeHeight()) / 4);
                maxHeight = CalculateKneeHeight() + (3 * (CalculateHipHeight() - CalculateKneeHeight()) / 4);
                //float height = CalculateMidThighHeight();
                _targetHeight.SetHeight(height);
                //Debug.Log(height);
            }

        }

        //StringBuilder text = new StringBuilder(string.Format("Body: {0}, {1}: Gesture Detected? {2}\n", bodyIndex, e.GestureName, isDetected));
        //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
        //Debug.Log(text);
    }
    int timeCounter = 0;

    private IEnumerator upDateDifficulty(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        /*
        if (ComplexityListener.hike_level == 0)
        {
            height = CalculateMidThighHeight();
            minHeight = CalculateKneeHeight();
            maxHeight = CalculateHipHeight();
            //float height = CalculateMidThighHeight();

            //  height = height + (height / 2);
            //height =  height - (height / 6);//4
            //_targetHeight.SetHeight(height);
        }

        else if (ComplexityListener.hike_level == 1)
        {

            height = CalculateMidThighHeight();
            minHeight = CalculateKneeHeight();
            maxHeight = CalculateHipHeight();
            //float height = CalculateMidThighHeight();
            //_targetHeight.SetHeight(height);
        }
        else if (ComplexityListener.hike_level == 2)
        {
            height = CalculateMidThighHeight();
            minHeight = CalculateKneeHeight();
            maxHeight = CalculateHipHeight();
            //float height = CalculateMidThighHeight();
            //height = height - (height / 2);
            //height = height + (height / 6);

            //_targetHeight.SetHeight(height);
        }*/

       // timeCounter++; // to keeep it in infinite
        if (timeCounter == 540)
        {
            if(!MainMenu.isnonVR)
            {
                levelMode[1].SetActive(false);
                levelMode[0].SetActive(true);
                vrManager.parent = cube;
                vrManager.position = new Vector3(cube.position.x, cube.position.y, cube.position.z);
                Quaternion target = Quaternion.Euler(0, 0, 0);

                vrManager.localRotation = target;
                cube.GetComponent<AI>().enabled = true;
            }
            

           
        }
       // StartCoroutine("upDateDifficulty", 1);
        if (timeCounter > TestDuration)
        {
            StopCoroutine("upDateDifficulty");
            GameBLESender.Instance.SendString("GameVariable,Stop");
            testOverPanel.SetActive(true);
        }
    }
   



    /*private IEnumerator upDateDifficulty(float waitTime)
    {
         yield return new WaitForSeconds(waitTime);

        if (timeCounter == 0)
        {
            modePanels[0].SetActive(true);

            float height = CalculateMidThighHeight();

          //  height = height + (height / 2);
            height = height - (height / 4 );
           _targetHeight.SetHeight(height);
        }

        else if (timeCounter == 120)
        {

            modePanels[1].SetActive(true);
            float height = CalculateMidThighHeight();
            _targetHeight.SetHeight(height);
        }
        else if (timeCounter == 240)
        {
            modePanels[2].SetActive(true);
            float height = CalculateMidThighHeight();
            //height = height - (height / 2);
            height = height + (height / 4);

            _targetHeight.SetHeight(height);
        }
            
        timeCounter++;

        StartCoroutine("upDateDifficulty", 1);
        if (timeCounter > 300)
        {
            StopCoroutine("upDateDifficulty");
            testOverPanel.SetActive(true);
        }
    } */
    public void OnNextButton()
    {
        if(!MainMenu.isnonVR)
            GameObject.FindGameObjectWithTag("cube").GetComponent<AI>().enabled=true;
        
        _subState = _nextSubState;
    }

    public void OnAbortButton()
    {
        _state = State.Abort;
        _subState = SubState.Start;
		OnExitButton ();
    }

    public void OnExitButton()
    {
        SceneManager.LoadScene(0);
    }

    public void OnRestartButton()
    {
        AbortButton.onClick.RemoveAllListeners();
        AbortButton.onClick.AddListener(OnAbortButton);
        AbortButton.GetComponentInChildren<Text>().text = "Abortar";

        NextButton.onClick.RemoveAllListeners();
        NextButton.onClick.AddListener(OnNextButton);
        NextButton.GetComponentInChildren<Text>().text = "Próximo";

        InitializeVariables();
    }

    private void InitializeVariables()
    {
        // Test
        _subjectBodyIndex = -1;
        _2mTimer = 0;
        _subjectAvatarOrientationControl = null;
        _test = new Test {RSteps = 0, LeftFootIsDown = true, LeftFootIsUp = false, LeftStepIsValid = true, RightFootIsDown = true, RightFootIsUp = false, RightStepIsValid = false};

        // State
        _gesturePause = true;
        _state = State.GetSubject;
        _subState = SubState.Start;
        _nextSubState = SubState.Start;

        // Feedback
        AbstractFeedbackLeft.SetTopMarker(.6f);
        AbstractFeedbackLeft.SetBottomMarker(0f);
        AbstractFeedbackLeft.TurnTopOff();
        AbstractFeedbackLeft.TurnBottomOn();
        
        AbstractFeedbackRight.SetTopMarker(.6f);
        AbstractFeedbackRight.SetBottomMarker(0f);
        AbstractFeedbackRight.TurnTopOff();
        AbstractFeedbackRight.TurnBottomOn();

        this.FloorLevel.SetMaterial(TargetFeedback.Colors.White);
        this.TargetLevel.SetMaterial(TargetFeedback.Colors.White);

        //BodySourceViewer.SetAllBodiesVisible();
    }

    private float CalculateMidThighHeight()
    {
        float height = 0f;

        //HipLeft 12
        //KneeLeft 13
        float leftMidThigh = _subjectAvatarOrientationControl.AvatarBones[13].transform.position.y +
                             (_subjectAvatarOrientationControl.AvatarBones[12].transform.position.y -
                              _subjectAvatarOrientationControl.AvatarBones[13].transform.position.y) / 2;
        //HipRight 16
        //KneeRight 17
        float rightMidThigh = _subjectAvatarOrientationControl.AvatarBones[17].transform.position.y +
                              (_subjectAvatarOrientationControl.AvatarBones[16].transform.position.y -
                               _subjectAvatarOrientationControl.AvatarBones[17].transform.position.y) / 2;
        height = leftMidThigh < rightMidThigh ? leftMidThigh : rightMidThigh;
        return height;
    }

    private float CalculateKneeHeight()
    {
        float Kneeheight = 0f;

        float leftKnee = _subjectAvatarOrientationControl.AvatarBones[13].transform.position.y;

        float rightKnee = _subjectAvatarOrientationControl.AvatarBones[17].transform.position.y;

        Kneeheight = leftKnee < rightKnee ? leftKnee : rightKnee;

        return Kneeheight;
    }

    private float CalculateHipHeight()
    {
        float Hipheight = 0f;

        float leftHip = _subjectAvatarOrientationControl.AvatarBones[12].transform.position.y;

        float rightHip = _subjectAvatarOrientationControl.AvatarBones[16].transform.position.y;

        Hipheight = leftHip > rightHip ? leftHip : rightHip;

        return Hipheight;
    }

    public void OnTargetHeightTriggerEnter(Collider collidingObject)
    {
        if (_state != State.Test || _subState != SubState.Update) return;

        //AnkleLeft 14
        if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[14])
        {
            AbstractFeedbackLeft.TurnTopOn();

            _test.LeftFootIsUp = true;
            if (_test.RightFootIsDown)
            {
                _test.LeftStepIsValid = true;
                if (TestDetails.TestDesc.Feedback != TestDetails.FeedbackType.Control)
                    ;// AbstractFeedbackLeft.PlaySound(1);
                stepsCounter++;

            }

        }
        //AnkleRight 18
        else if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[18])
        {
            AbstractFeedbackRight.TurnTopOn();

            _test.RightFootIsUp = true;
            if (_test.LeftFootIsDown)
            {
                _test.RightStepIsValid = true;
                if (TestDetails.TestDesc.Feedback != TestDetails.FeedbackType.Control)
                    ;//AbstractFeedbackRight.PlaySound(0);
            }

            if (_test.LeftStepIsValid && _test.RightStepIsValid && _test.LeftFootIsDown)
            {
                _test.LeftStepIsValid = false;
                _test.RightStepIsValid = false;
                _test.RSteps++;
                stepsCounter++;

            }

        }


    }

    public void OnTargetHeightTriggerExit(Collider collidingObject)
    {
        if (_state != State.Test || _subState != SubState.Update) return;

        //AnkleLeft 14
        if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[14])
        {
            AbstractFeedbackLeft.TurnTopOff();
            _test.LeftFootIsUp = false;
        }
        //AnkleRight 18
        else if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[18])
        {
            AbstractFeedbackRight.TurnTopOff();
            _test.RightFootIsUp = false;
        }
    }

    public void OnFloorLevelTriggerEnter(Collider collidingObject)
    {
        if (_state != State.Test || _subState != SubState.Update) return;

        //FootLeft 15
        if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[15])
        {
            AbstractFeedbackLeft.TurnBottomOn();
            _test.LeftFootIsDown = true;
        }
        //FootRight 19
        else if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[19])
        {
            AbstractFeedbackRight.TurnBottomOn();
            _test.RightFootIsDown = true;
        }
    }

    public void OnFloorLevelTriggerExit(Collider collidingObject)
    {
        if (_state != State.Test || _subState != SubState.Update) return;

        //FootLeft 15
        if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[15])
        {
            AbstractFeedbackLeft.TurnBottomOff();
            _test.LeftFootIsDown = false;
        }
        //FootRight 19
        else if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[19])
        {
            AbstractFeedbackRight.TurnBottomOff();
            _test.RightFootIsDown = false;
        }
    }

    private void SetMarkers()
    {
        float knee;
        float foot;

        //KneeLeft 13
        knee = 1 - 
               (TargetHeightCollider.transform.position.y -_subjectAvatarOrientationControl.AvatarBones[13].transform.position.y) /
               (TargetHeightCollider.transform.position.y - FloorLevelCollider.transform.position.y);

        //FootLeft 15
        foot = (FloorLevelCollider.transform.position.y -_subjectAvatarOrientationControl.AvatarBones[15].transform.position.y + .03f) /
               (FloorLevelCollider.transform.position.y - TargetHeightCollider.transform.position.y);

        AbstractFeedbackLeft.SetTopMarker(knee);
        AbstractFeedbackLeft.SetBottomMarker(foot);

        //KneeRight 17
        knee = 1 -
               (TargetHeightCollider.transform.position.y - _subjectAvatarOrientationControl.AvatarBones[17].transform.position.y) /
               (TargetHeightCollider.transform.position.y - FloorLevelCollider.transform.position.y);

        //FootRight 19
        foot = (FloorLevelCollider.transform.position.y - _subjectAvatarOrientationControl.AvatarBones[19].transform.position.y + .03f) /
               (FloorLevelCollider.transform.position.y - TargetHeightCollider.transform.position.y);

        AbstractFeedbackRight.SetTopMarker(knee);
        AbstractFeedbackRight.SetBottomMarker(foot);
        left_ball.transform.position = new Vector3(left_ball.transform.position.x, _subjectAvatarOrientationControl.AvatarBones[13].transform.position.y, left_ball.transform.position.z);
        right_ball.transform.position = new Vector3(right_ball.transform.position.x, _subjectAvatarOrientationControl.AvatarBones[17].transform.position.y, right_ball.transform.position.z);
    }

    void OnDestroy()
    {
        OnApplicationQuit();
    }

    void OnApplicationQuit()
    {
        if (_gestureDetectorList != null)
        {
            for (int bodyIndex = 0; bodyIndex < _bodyCount; bodyIndex++)
            {
                if (_gestureDetectorList[bodyIndex] != null)
                {
                    _gestureDetectorList[bodyIndex].OnDiscreteGestureDetected -= CreateOnDiscreteGestureHandler(bodyIndex);
                    _gestureDetectorList[bodyIndex].Dispose();
                    _gestureDetectorList[bodyIndex] = null;
                }
            }
        }

        if (_gestureDetectorList != null)
        {
            _gestureDetectorList.Clear();
            _gestureDetectorList = null;
        }

        if (_bodyFrameReader != null)
        {
            _bodyFrameReader.Dispose();
            _bodyFrameReader = null;
        }

        if (_kinectSensor != null)
        {
            if (_kinectSensor.IsOpen)
            {
                _kinectSensor.Close();
            }

            _kinectSensor = null;
        }
    }
}
