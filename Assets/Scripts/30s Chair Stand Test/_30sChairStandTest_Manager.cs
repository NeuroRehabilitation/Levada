using System;
using System.Collections.Generic;
using System.Text;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System.Collections;

public class _30sChairStandTest_Manager : MonoBehaviour
{
    public Text lblCountDown;
    int start_counter = 4;
    public Material avatar_gesture_detection;
    // Kinect 
    private KinectSensor _kinectSensor;

    private BodyFrameReader _bodyFrameReader;
    private int _bodyCount;
    private Body[] _bodies;

    /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
    private List<GestureDetectors> _gestureDetectorList;

    // Test
    private int _subjectBodyIndex = -1;

    private float _30sTimer;
    private const float TestDuration = 30f;
    private JointOrientationControl2M _subjectAvatarOrientationControl;
    public BodySourceViewer2M BodySourceViewer;
    private Test _test;

    public TargetFeedback SitPose;
    public TargetFeedback StandPose;
	private float avatrProgress=0;
    public struct Test
    {
        public int Stands;
        public bool UserWasDown;
        public bool HalfStand;
        public float[] SitToStandProgess;
        public const float ProgressStandThreshold = 0.9f;
        public const float ProgressHalfThreshold = 0.5f;
        public const float ProgressSitThreshold = 0.1f;

    }

    private _30sChairStandTest_Logger _logger = new _30sChairStandTest_Logger();

    // State
    private bool _gesturePause = true;

    private State _state = State.GetSubject;
    private SubState _subState = SubState.Start;
    private SubState _nextSubState = SubState.Start;

    private enum State
    {
        GetSubject,
        Instruct,
        Trial,
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
    public AbstractFeedback AbstractFeedback;


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
            GetComponent<SittoStandAnimationController>().enabled = true;
        }
        else
        {
            if (start_counter > 1)
                lblCountDown.text = "Pronto";//Ready;
            else
                lblCountDown.text = "Comece";//Go
            StartCoroutine(countDown());
        }

    }


    // Use this for initialization
    void Start()
    {
        avatar_gesture_detection.color = Color.red;

        avatrProgress = 0;
		InitializeVariables();

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
                _gestureDetectorList.Add(new GestureDetectors(_kinectSensor, SFTTest._30sCST));
            }

            // start getting data from runtime
            _kinectSensor.Open();
        }
    }
    void Update() {

        if (Input.GetButtonUp("Fire1")|| Input.GetKeyUp(KeyCode.A)) 
		{
			OnNextButton ();
			Debug.Log("fire2 called");
		}else if (Input.GetButtonUp ("Fire2") || Input.GetKeyUp(KeyCode.S)) {
			OnAbortButton ();
			Debug.Log ("fire two called");
		}
	}

    // Update is called once per frame
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

            case State.Trial:
                if (!_bodies[_subjectBodyIndex].IsTracked)
                {
                    _state = State.Problem;
                    _subState = SubState.Start;
                    break;
                }

                Trial();
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
                AbortButton.GetComponentInChildren<Text>().text = "Quit";

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

    private void Result()
    {
        switch (_subState)
        {
            case SubState.Start:
                GuitText.text = String.Format("Número de levantamentos: {0}", _test.Stands);
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
                if (start_counter == 0)
                {
                    AbstractFeedback.PlaySound(2);
                    _test = new Test { Stands = 0, UserWasDown = true, HalfStand = false, SitToStandProgess = new[] { 0f, 0f } };
                    _30sTimer = TestDuration;
                    GuitText.text = String.Format("Start\n");
                    _subState = SubState.Update;
                    _nextSubState = SubState.Update;
                    NextButton.interactable = false;
                    NextButton.GetComponentInChildren<Text>().text = "Next";
                    _gesturePause = false;
                }

                if (start_counter > 3)
                {
                    GetComponent<SittoStandAnimationController>().enabled = false;
                    start_counter--;
                    StartCoroutine(countDown());
                }

                break;

            case SubState.Update:
                avatar_gesture_detection.color = Color.green;

                _logger.Log(_30sTimer, _test, _subjectAvatarOrientationControl);
                _30sTimer -= Time.fixedDeltaTime;
                GuitText.text = String.Format("Start\n{0:f2} seconds\nNumber of withdrawals : {1}\n",
                    _30sTimer, _test.Stands);
                if (_30sTimer <= 0)
                {
                    AbstractFeedback.PlaySound(2);
                    if (_test.HalfStand)
                    {
                        _test.Stands++;
                    }
                    _subState = SubState.Next;
                    NextButton.interactable = true;
                    avatar_gesture_detection.color = Color.red;
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
                avatar_gesture_detection.color = Color.red;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Trial()
    {
        switch (_subState)
        {
            case SubState.Start:
                
                
                _test = new Test { Stands = 0, UserWasDown = true, HalfStand = false, SitToStandProgess = new[] { 0f, 0f } };
                GuitText.text = String.Format("Try getting up 2 times.\n");
                _subState = SubState.Update;
                _nextSubState = SubState.Update;
                NextButton.interactable = false;
                _gesturePause = false;
                
                break;

            case SubState.Update:
                avatar_gesture_detection.color = Color.green;
                GuitText.text = String.Format("Try getting up 2 times.\nNumber of withdrawals: {0}\nProgress: {1:f3}", _test.Stands,
                    _test.SitToStandProgess[0]);
                if (_test.Stands >= 2)
                {
                    _subState = SubState.Idle;
                    NextButton.interactable = true;
                    NextButton.GetComponentInChildren<Text>().text = "Start";
                    avatar_gesture_detection.color = Color.yellow;
                }
                break;

            case SubState.Idle:
                // Feedback
                AbstractFeedback.SetMarker(0f);
                AbstractFeedback.TurnTopOff();
                AbstractFeedback.TurnBottomOn();
                this.StandPose.SetMaterial(TargetFeedback.Colors.White);
                this.SitPose.SetMaterial(TargetFeedback.Colors.White);

                GuitText.text = String.Format("Sit down and get ready to start the beep test.");
                _gesturePause = true;
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

    private void Instruct()
    {
        switch (_subState)
        {
            case SubState.Start:
                GuitText.text = "Instruções:\n\n" +
                                "Levante-se e sente-se com os braços cruzados\n\n" +
                                "Faça o maior número de levantamentos em 30 segundos\n\n" +
                                "Comece ao sinal sonoro.\n\n" +
                                "Sente-se e prepare-se para tentar 2 vezes";
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
                _state = State.Test;
                _subState = SubState.Start;
                // _state = State.Trial;
                // _subState = SubState.Start;
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
                BodySourceViewer.SetBodiesInvisibleExcept(_subjectBodyIndex);
                _subjectAvatarOrientationControl =
                    BodySourceViewer.Avatars[_subjectBodyIndex].GetComponent<JointOrientationControl2M>();
                _state = State.Instruct;
                _subState = SubState.Start;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnNextButton()
    {
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
        _30sTimer = 0;
        _test = new Test { Stands = 0, UserWasDown = true, HalfStand = false, SitToStandProgess = new[] { 0f, 0f } };

        // State
        _gesturePause = true;
        _state = State.GetSubject;
        _subState = SubState.Start;
        _nextSubState = SubState.Start;

        // Feedback
        AbstractFeedback.SetMarker(0f);
        AbstractFeedback.TurnTopOff();
        AbstractFeedback.TurnBottomOn();
        this.StandPose.SetMaterial(TargetFeedback.Colors.White);
        this.SitPose.SetMaterial(TargetFeedback.Colors.White);

       // BodySourceViewer.SetAllBodiesVisible();
    }

    void OnDisable()
    {
        Dispose();
    }

    void OnApplicationQuit()
    {
        Dispose();
    }

    private void Dispose()
    {
        if (_gestureDetectorList != null)
        {
            for (int bodyIndex = 0; bodyIndex < _bodyCount; bodyIndex++)
            {
                if (_gestureDetectorList[bodyIndex] != null)
                {
                    _gestureDetectorList[bodyIndex].OnDiscreteGestureDetected -= CreateOnDiscreteGestureHandler(bodyIndex);
                    _gestureDetectorList[bodyIndex].OnContinuousGestureDetected -= CreateOnContinuousGestureHandler(bodyIndex);
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
                            _gestureDetectorList[bodyIndex].OnContinuousGestureDetected += CreateOnContinuousGestureHandler(bodyIndex);
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

        if (isDetected && e.GestureName == "poseT" && _state == State.GetSubject && _subState == SubState.Idle)
        {
            avatar_gesture_detection.color = Color.yellow;

            _subjectBodyIndex = bodyIndex;
            //Debug.Log("Pose T");
        }
        else if (isDetected && e.GestureName == "crossArms")
        {
            //Debug.Log("cross arms");
        }

        //StringBuilder text = new StringBuilder(string.Format("Body: {0}, {1}: Gesture Detected? {2}\n", bodyIndex, e.GestureName, isDetected));
        //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
        //Debug.Log(text);
    }

    private EventHandler<ContinuousGestureEventArgs> CreateOnContinuousGestureHandler(int bodyIndex)
    {
        return (object sender, ContinuousGestureEventArgs e) => OnContinuousGestureDetected(sender, e, bodyIndex);
    }

    private void OnContinuousGestureDetected(object sender, ContinuousGestureEventArgs e, int bodyIndex)
    {
        if (bodyIndex != _subjectBodyIndex || !e.IsBodyTrackingIdValid || e.GestureName != "sit_standProgress" ||
            (_state != State.Test && _state != State.Trial) || _subState != SubState.Update) return;

        //StringBuilder text = new StringBuilder(string.Format("Body: {0}, {1}: Gesture Progress: {2:f3}\n", bodyIndex, e.GestureName, e.Progress));
        //Debug.Log(text);

        _test.SitToStandProgess[1] = _test.SitToStandProgess[0];
        _test.SitToStandProgess[0] = e.Progress;
		avatrProgress = e.Progress;

        if (_test.SitToStandProgess[0] <= Test.ProgressSitThreshold && _test.SitToStandProgess[1] <= Test.ProgressSitThreshold)
        {
            if (!_test.UserWasDown)
                if (TestDetails.TestDesc.Feedback != TestDetails.FeedbackType.Control)
                    AbstractFeedback.PlaySound(1);

            _test.UserWasDown = true;
            AbstractFeedback.TurnBottomOn();
            AbstractFeedback.TurnTopOff();
            this.StandPose.SetMaterial(TargetFeedback.Colors.White);
            this.SitPose.SetMaterial(TargetFeedback.Colors.Green);
        }
        else if (_test.SitToStandProgess[0] >= Test.ProgressStandThreshold && _test.SitToStandProgess[1] >= Test.ProgressStandThreshold)
        {
            if (_test.UserWasDown)
            {

                _test.Stands++;
                _test.UserWasDown = false;
                AbstractFeedback.TurnBottomOff();
                AbstractFeedback.TurnTopOn();
                this.StandPose.SetMaterial(TargetFeedback.Colors.Green);
                this.SitPose.SetMaterial(TargetFeedback.Colors.White);
				if (TestDetails.TestDesc.Feedback != TestDetails.FeedbackType.Control)
                    AbstractFeedback.PlaySound(0);
            }
        }
        else
        {
            this.StandPose.SetMaterial(TargetFeedback.Colors.White);
            this.SitPose.SetMaterial(TargetFeedback.Colors.White);
        }

        MoveMarker();

        _test.HalfStand = _test.SitToStandProgess[0] >= Test.ProgressHalfThreshold &&
                          _test.SitToStandProgess[1] >= Test.ProgressHalfThreshold;
    }

    private void MoveMarker()
    {
        float marker = (_test.SitToStandProgess[0] - Test.ProgressSitThreshold) / (Test.ProgressStandThreshold - Test.ProgressSitThreshold);

            if (marker < 0)
                marker = 0;
            else if (marker > 1)
                marker = 1;

        AbstractFeedback.SetMarker(marker);
    }

	public float getAvatarProgress()
	{

		return avatrProgress;
	}

}