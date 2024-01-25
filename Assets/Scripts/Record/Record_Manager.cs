using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Windows.Kinect;

public class Record_Manager : MonoBehaviour {

    // Kinect 
    private KinectSensor _kinectSensor;
    private BodyFrameReader _bodyFrameReader;
    private int _bodyCount;
    private Body[] _bodies;

    /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
    private List<GestureDetectors> _gestureDetectorList;
    
    // Test
    private int _subjectBodyIndex = -1;
    private JointOrientationControl2M _subjectAvatarOrientationControl;
    public BodySourceViewer2M BodySourceViewer;

    private string _path, _filename;
    private TextWriter _fileLog;
    private readonly string _userId = TestDetails.TestDesc.UserId;
    private bool _initialized;
    private float _time;

    // GUI
    public Button NextButton;
    public Button AbortButton;
    public Text GuitText;
    private ColorBlock _defaultButtonColors;

    // State
    private bool _gesturePause = true;
    private State _state = State.GetSubject;
    private SubState _subState = SubState.Start;
    private SubState _nextSubState = SubState.Start;

    private enum State
    {
        GetSubject,
        Record,
        Stop,
        Problem
    }

    private enum SubState
    {
        Start,
        Update,
        Idle,
        Next
    }

    // Use this for initialization
    void Start()
    {
        _defaultButtonColors = this.NextButton.colors;


        InitializeVariables();
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

    // Update is called once per frame
    void FixedUpdate()
    {
        ManageGestureDetectors();

        switch (_state)
        {
            case State.GetSubject:
                GetSubject();
                break;

            case State.Record:
                if (!_bodies[_subjectBodyIndex].IsTracked)
                {
                    _state = State.Problem;
                    _subState = SubState.Start;
                    break;
                }

                Record();
                break;

            case State.Stop:
                Stop();
                break;

            case State.Problem:
                Problem();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void InitializeVariables()
    {
        // Test
        _subjectBodyIndex = -1;
        _subjectAvatarOrientationControl = null;

        // State
        _gesturePause = true;
        _state = State.GetSubject;
        _subState = SubState.Start;
        _nextSubState = SubState.Start;

        //Buttons
        AbortButton.onClick.RemoveAllListeners();
        AbortButton.onClick.AddListener(OnExitButton);
        AbortButton.interactable = true;
        AbortButton.GetComponentInChildren<Text>().text = "Sair";

        NextButton.onClick.RemoveAllListeners();
        NextButton.onClick.AddListener(this.OnNextButton);
        NextButton.interactable = true;
        NextButton.GetComponentInChildren<Text>().text = "Gravar";
        NextButton.colors = this._defaultButtonColors;
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
                _state = State.Record;
                _subState = SubState.Update;

                NextButton.GetComponentInChildren<Text>().text = "Parar";
                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(OnStopButton);
                NextButton.interactable = true;
                var buttonColors = this.NextButton.colors;
                buttonColors.normalColor = Color.red;
                buttonColors.highlightedColor = Color.red;
                this.NextButton.colors = buttonColors;

                AbortButton.interactable = false;

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Record()
    {
        switch (_subState)
        {
            case SubState.Start:
                Debug.Log("Something went wrong, code should't go here");
                break;

            case SubState.Update:
                _gesturePause = true;
                if (!_bodies[_subjectBodyIndex].IsTracked)
                {
                    _state = State.Problem;
                    _subState = SubState.Start;
                    break;
                }
                GuitText.text = String.Format("Gravando\nSujeito: {0}", _subjectBodyIndex);
                this.Log();
                break;

            case SubState.Idle:
                Debug.Log("Something went wrong, code should't go here");
                break;

            case SubState.Next:
                Debug.Log("Something went wrong, code should't go here");
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
                Close();
                GuitText.text = "O seguimento do sujeito perdeu-se";
                AbortButton.onClick.RemoveAllListeners();
                AbortButton.onClick.AddListener(OnExitButton);
                AbortButton.interactable = true;
                AbortButton.GetComponentInChildren<Text>().text = "Sair";

                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(OnRestartButton);
                NextButton.interactable = true;
                NextButton.GetComponentInChildren<Text>().text = "Reiniciar";
                NextButton.colors = this._defaultButtonColors;

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

    private void Stop()
    {
        switch (_subState)
        {
            case SubState.Start:
                Close();
                GuitText.text = "Gravação parada, reiniciar ou sair?";

                AbortButton.onClick.RemoveAllListeners();
                AbortButton.onClick.AddListener(OnExitButton);
                AbortButton.interactable = true;
                AbortButton.GetComponentInChildren<Text>().text = "Sair";

                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(OnRestartButton);
                NextButton.interactable = true;
                NextButton.GetComponentInChildren<Text>().text = "Reiniciar";
                NextButton.colors = this._defaultButtonColors;

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

    private void Log()
    {
        if (!_initialized) Initialize();

        if (!_initialized) return;

        this._time += Time.deltaTime;

        List<string> newline = new List<string>
        {
            _time.ToString(CultureInfo.InvariantCulture)
        };
        foreach (var jointsAvatarBone in _subjectAvatarOrientationControl.AvatarBones)
        {
            newline.Add(jointsAvatarBone.name);
            newline.Add(jointsAvatarBone.transform.position.x.ToString(CultureInfo.InvariantCulture));
            newline.Add(jointsAvatarBone.transform.position.y.ToString(CultureInfo.InvariantCulture));
            newline.Add(jointsAvatarBone.transform.position.z.ToString(CultureInfo.InvariantCulture));
            newline.Add(jointsAvatarBone.transform.rotation.x.ToString(CultureInfo.InvariantCulture));
            newline.Add(jointsAvatarBone.transform.rotation.y.ToString(CultureInfo.InvariantCulture));
            newline.Add(jointsAvatarBone.transform.rotation.z.ToString(CultureInfo.InvariantCulture));
            newline.Add(jointsAvatarBone.transform.rotation.w.ToString(CultureInfo.InvariantCulture));
        }

        var data = string.Join(",", newline.ToArray());
        _fileLog.WriteLine(data);
    }

    private void Initialize()
    {
        this._time = 0 - Time.deltaTime;
        _path = Application.dataPath + "/Logs/" + _userId + "/";

        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        _filename = _userId + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_") + TestDetails.TestDesc.Test;
        _fileLog = new StreamWriter(_path + _filename + ".csv", false);

        var fileHeader = new List<string>
        {
            "Time"
        };
        for (int i = 1; i <= 25; i++)
        {
            fileHeader.Add("JointType" + i);
            fileHeader.Add("PositionX" + i);
            fileHeader.Add("PositionY" + i);
            fileHeader.Add("PositionZ" + i);
            fileHeader.Add("OrientationX" + i);
            fileHeader.Add("OrientationY" + i);
            fileHeader.Add("OrientationZ" + i);
            fileHeader.Add("OrientationW" + i);
        }

        //builds the string that will be the _header of the csv _file
        var header = string.Join(",", fileHeader.ToArray());

        //writes the first line of the _file (_header)
        _fileLog.WriteLine(header);
        _initialized = true;
    }

    public void OnExitButton()
    {
        SceneManager.LoadScene(0);
    }

    public void OnRestartButton()
    {
        InitializeVariables();
    }

    public void OnNextButton()
    {
        _subState = _nextSubState;
    }

    public void OnStopButton()
    {
        _state = State.Stop;
        _subState = SubState.Start;
    }

    // Close the log
    public void Close()
    {
        if (!_initialized) return;

        _fileLog.Flush();
        _fileLog.Close();
        _fileLog.Dispose();
        _initialized = false;
    }
}
