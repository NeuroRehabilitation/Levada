using System;
using System.Collections.Generic;
using System.Text;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class _8ftUpGo_Manager : MonoBehaviour
{

    // Kinect 
    private KinectSensor _kinectSensor;

    private BodyFrameReader _bodyFrameReader;
    private int _bodyCount;
    private Body[] _bodies;

    /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
    private List<GestureDetectors> _gestureDetectorList = null;

    // Test
    private int _subjectBodyIndex = -1;
    public _8ftTestSetup TestSetup;
    private JointOrientationControl2M _subjectAvatarOrientationControl;
    public BodySourceViewer2M BodySourceViewer;
    private Test _test;

    public struct Test
    {
        public List<float> Time;
        public bool DistanceValid;
        public float Stopwatch;
        public float CountDown;
        public int TrialNumber;
        public bool PlayStartFrame;
        public bool PlayHalfFrame;
        public bool PlayEndFrame;
        public bool UserIsSeated;
        public float DistanceToMarker;
        public float DistanceToChair;
    }

    private _8ftUpGo_Logger _logger = new _8ftUpGo_Logger();

    // State
    private bool _gesturePause = true;

    private State _state = State.GetSubject;
    private SubState _subState = SubState.Start;
    private SubState _nextSubState = SubState.Start;

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
    public AbstractFeedback AbstractFeedback;
    public TargetFeedback DistanceMarker;

    // Use this for initialization
    void Start()
    {
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
                _gestureDetectorList.Add(new GestureDetectors(_kinectSensor, SFTTest._8fUGT));
            }

            // start getting data from runtime
            _kinectSensor.Open();
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
                _subjectAvatarOrientationControl =
                    BodySourceViewer.Avatars[_subjectBodyIndex].GetComponent<JointOrientationControl2M>();
                BodySourceViewer.SetBodiesInvisibleExcept(_subjectBodyIndex);
               _state = State.Instruct;
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
                                "Levante-se da cadeira\n\n" +
                                "Caminhe o mais rápido que conseguir a distancia de 2,4 metros\n\n" +
                                "Vire-se, regresse e sente-se de novo na cadeira.\n\n" +
                                "\nAgora sente-se na cadeira e espere pelo sinal de começo.";
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
                _test.Stopwatch = 0f;
                _test.CountDown = Random.Range(2f, 4f);
                _test.PlayStartFrame = true;
                _test.PlayHalfFrame = true;
                _test.PlayEndFrame = true;
                _test.DistanceValid = false;
                GuitText.text = "Prepare-se\n";
                _subState = SubState.Idle;
                NextButton.interactable = true;
                _gesturePause = false;
                break;

            case SubState.Idle:
                //update start position while waits for next button
                var distance =
                    _subjectAvatarOrientationControl.AvatarBones[15].transform.position.z >
                    _subjectAvatarOrientationControl.AvatarBones[19].transform.position.z
                        ? _subjectAvatarOrientationControl.AvatarBones[15].transform.position.z
                        : _subjectAvatarOrientationControl.AvatarBones[19].transform.position.z;
                TestSetup.SetDistance(distance);
                _nextSubState = SubState.Update;
                NextButton.GetComponentInChildren<Text>().text = "Começar contagem decrescente";
                break;

            case SubState.Update:
                if (_test.CountDown > 0)
                {
                    NextButton.interactable = false;
                    GuitText.text = String.Format("Atenção");//"\nStart in {0:f2}", _test.CountDown);
                    _test.CountDown -= Time.fixedDeltaTime;

                    if (!_test.UserIsSeated)
                    {
                        _subState = SubState.Start;
                    }
                }
                else
                {
                    if (_test.PlayStartFrame)
                    {
                        _test.PlayStartFrame = false;
                        AbstractFeedback.PlaySound(2);
                    }
                    else if (_test.PlayHalfFrame && _test.DistanceValid)
                    {
                        _test.PlayHalfFrame = false;
                        if (TestDetails.TestDesc.Feedback != TestDetails.FeedbackType.Control)
                            AbstractFeedback.PlaySound(1);
                        AbstractFeedback.TurnTopOn();
                        AbstractFeedback.TurnBottomOff();
                        DistanceMarker.SetMaterial(TargetFeedback.Colors.Green);
                    }
                    else if (_test.PlayEndFrame && _test.DistanceValid && _test.UserIsSeated)
                    {
                        _test.PlayEndFrame = false;
                        if (TestDetails.TestDesc.Feedback != TestDetails.FeedbackType.Control)
                            AbstractFeedback.PlaySound(0);
                        AbstractFeedback.TurnTopOff();
                        AbstractFeedback.TurnBottomOn();
                        DistanceMarker.SetMaterial(TargetFeedback.Colors.White);
                    }

                    if (_test.DistanceValid && _test.UserIsSeated)
                    {
                        _test.Time[_test.TrialNumber] = _test.Stopwatch;
                        _test.TrialNumber++;
                        _subState = SubState.Next;
                        NextButton.interactable = true;
                        _gesturePause = true;
                    }
                    else
                    {
                        float marker = 0f;
                        if (!_test.DistanceValid)
                            marker = (2.4f - (_subjectAvatarOrientationControl.AvatarBones[0].transform.position.z - TestSetup.transform.position.z)) / 2.4f;
                        else
                            marker = (TestSetup.transform.position.z + 2.4f - _subjectAvatarOrientationControl.AvatarBones[0].transform.position.z) / 2.4f;

                        if (marker < 0)
                            marker = 0;
                        else if (marker > 1)
                            marker = 1;

                        AbstractFeedback.SetMarker(marker);

                        _test.DistanceToMarker = _subjectAvatarOrientationControl.AvatarBones[0].transform.position.z - TestSetup.transform.position.z;
                        _test.DistanceToChair = TestSetup.transform.position.z + 2.4f - _subjectAvatarOrientationControl.AvatarBones[0].transform.position.z;
                        _logger.Log(_test, _subjectAvatarOrientationControl);
                        _test.Stopwatch += Time.fixedDeltaTime;
                        NextButton.interactable = false;
                        GuitText.text = String.Format("Partida\n{0:f2} segundos\nDistância ao marcador {1:f2}\nDistância à cadeira {2:f2}", _test.Stopwatch, _test.DistanceToMarker, _test.DistanceToChair);
                    }
                }
                break;

            case SubState.Next:
                _logger.Close();
                if (_test.TrialNumber < 3)
                {
                    _state = State.Test;
                    _subState = SubState.Start;
                }
                else
                {
                    NextButton.GetComponentInChildren<Text>().text = "Próximo";
                    _state = State.Result;
                    _subState = SubState.Start;
                }
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
                GuitText.text = String.Format("Tempos: {0:f2} e {1:f2}", _test.Time[1], _test.Time[2]);
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

    private void Abort()
    {
        switch (_subState)
        {
            case SubState.Start:
                _logger.Close();
                GuitText.text = "Teste cancelado, reiniciar ou sair?";

                AbortButton.onClick.RemoveAllListeners();
                AbortButton.onClick.AddListener(OnExitButton);
                AbortButton.GetComponentInChildren<Text>().text = "Sair";

                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(OnRestartButton);
                NextButton.interactable = true;
                NextButton.GetComponentInChildren<Text>().text = "Recomeçar";

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

    private void Problem()
    {
        switch (_subState)
        {
            case SubState.Start:
                _logger.Close();
                GuitText.text = "O seguimento do sujeito perdeu-se";
                AbortButton.onClick.RemoveAllListeners();
                AbortButton.onClick.AddListener(OnExitButton);
                AbortButton.GetComponentInChildren<Text>().text = "Sair";

                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(OnRestartButton);
                NextButton.interactable = true;
                NextButton.GetComponentInChildren<Text>().text = "Reiniciar";

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
                            //this.gestureDetectorList[bodyIndex].IsPaused = (trackingId == 0);
                            _gestureDetectorList[bodyIndex].OnDiscreteGestureDetected +=
                                CreateOnDiscreteGestureHandler(bodyIndex);
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

        }

        if (bodyIndex != _subjectBodyIndex || !e.IsBodyTrackingIdValid || e.GestureName != "sitStop" ||
            _state != State.Test || (_subState != SubState.Update && _subState != SubState.Idle)) return;

        _test.UserIsSeated = isDetected;

        //StringBuilder text = new StringBuilder(string.Format("Body: {0}, {1}: Gesture Detected? {2}\n", bodyIndex, e.GestureName, isDetected));
        //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
        //Debug.Log(text);
    }

    public void OnNextButton()
    {
        _subState = _nextSubState;
    }

    public void OnAbortButton()
    {
        _state = State.Abort;
        _subState = SubState.Start;
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
        _subjectAvatarOrientationControl = null;
        _test = new Test
        {
            Time = new List<float>(3) {0f, 0f, 0f},
            DistanceValid = false,
            CountDown = 0,
            Stopwatch = 0,
            TrialNumber = 0,
            PlayStartFrame = false,
            PlayHalfFrame = false,
            PlayEndFrame = false,
            UserIsSeated = false,
            DistanceToChair = 0f,
            DistanceToMarker = 0f
        };

        // State
        _gesturePause = true;
        _state = State.GetSubject;
        _subState = SubState.Start;
        _nextSubState = SubState.Start;

        // Feedback
        AbstractFeedback.SetMarker(0f);
        AbstractFeedback.TurnTopOff();
        AbstractFeedback.TurnBottomOn();
        DistanceMarker.SetMaterial(TargetFeedback.Colors.White);

        //BodySourceViewer.SetAllBodiesVisible();
    }

    public void On8ftPlaneTriggerEnter(Collider collidingObject)
    {
        if (_state != State.Test || _subState != SubState.Update) return;

        if (collidingObject.gameObject.transform.parent.gameObject == _subjectAvatarOrientationControl.AvatarBones[0])
        {
            _test.DistanceValid = true;
        }
    }

    public void On8ftPlaneTriggerExit(Collider collidingObject)
    {

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
                    _gestureDetectorList[bodyIndex].OnDiscreteGestureDetected -=
                        CreateOnDiscreteGestureHandler(bodyIndex);
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
