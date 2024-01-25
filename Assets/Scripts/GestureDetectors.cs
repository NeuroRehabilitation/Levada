using System;
using System.Collections.Generic;
using System.IO;
using Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using UnityEngine;
using System.Collections;


public enum SFTTest { _2mST, _8fUGT, _30sCST }

public class DiscreteGestureEventArgs : EventArgs
{
    
    public bool IsBodyTrackingIdValid { get; private set; }

    public bool IsGestureDetected { get; private set; }

    public float DetectionConfidence { get; private set; }

    public string GestureName { get; private set; }

    public DiscreteGestureEventArgs(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence, string gestureName)
    {
        this.IsBodyTrackingIdValid = isBodyTrackingIdValid;
        this.IsGestureDetected = isGestureDetected;
        this.DetectionConfidence = detectionConfidence;
        this.GestureName = gestureName;
    }
}

public class ContinuousGestureEventArgs : EventArgs
{
    public bool IsBodyTrackingIdValid { get; private set; }

    public float Progress { get; private set; }

    public string GestureName { get; private set; }

    public ContinuousGestureEventArgs(bool isBodyTrackingIdValid, float progress, string gestureName)
    {
        this.IsBodyTrackingIdValid = isBodyTrackingIdValid;
        this.Progress = progress;
        this.GestureName = gestureName;
    }
}

public class GestureDetectors : IDisposable
{
    
    /// <summary> Type of SFT </summary>
    private readonly SFTTest _SFTTestType;

    /// <summary> Path to the gesture databases that was trained with VGB </summary>
    private readonly List<string> _gestureDatabases = new List<string> { "GestureDB\\2mST.gbd", "GestureDB\\8fUGT.gbd", "GestureDB\\30sCST.gbd" };

    /// <summary> Name of the gestures in each of the databases that we want to track </summary>
    private readonly List<List<string>> _gestureNames =
        new List<List<string>>
        {
            new List<string> {"poseT"},
            new List<string> {"poseT", "sitStop"},
            new List<string> {"poseT", "crossArms", "sit_standProgress"}
        };

    /// <summary> Name of the discrete gesture in the database that we want to track </summary>
    //private readonly string seatedGestureName = "Seated";

    /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
    private VisualGestureBuilderFrameSource vgbFrameSource = null;

    /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
    private VisualGestureBuilderFrameReader vgbFrameReader = null;

    public event EventHandler<DiscreteGestureEventArgs> OnDiscreteGestureDetected;
    public event EventHandler<ContinuousGestureEventArgs> OnContinuousGestureDetected;

    /// <summary>
    /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
    /// </summary>
    /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
    /// <param name="sftTestType">Type of SFT being run, controls which gesture database to load</param>
    public GestureDetectors(KinectSensor kinectSensor, SFTTest sftTestType)
    {
        if (kinectSensor == null)
        {
            throw new ArgumentNullException("kinectSensor");
        }

        _SFTTestType = sftTestType;

        // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
        this.vgbFrameSource = VisualGestureBuilderFrameSource.Create(kinectSensor, 0);
        this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

        // open the reader for the vgb frames
        this.vgbFrameReader = this.vgbFrameSource.OpenReader();
        if (this.vgbFrameReader != null)
        {
            this.vgbFrameReader.IsPaused = true;
            this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
        }

        // load all the gestures that are on the list _gestureNames for the apropriate SFT from the gesture database
        var databasePath = Path.Combine(Application.streamingAssetsPath, this._gestureDatabases[(int)_SFTTestType]);
        using (VisualGestureBuilderDatabase database = VisualGestureBuilderDatabase.Create(databasePath))
        {
            var gestures = database.AvailableGestures;
            

            foreach (Gesture gesture in gestures)//database.AvailableGestures)
            {
                
                foreach (var gestureName in _gestureNames[(int)_SFTTestType])
                {
                    if (gesture.Name.Equals(gestureName))
                    {
                        if (gesture != null)
                            this.vgbFrameSource.AddGesture(gesture);
                        else
                            Debug.Log("gesture is null");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the body tracking ID associated with the current detector
    /// The tracking ID can change whenever a body comes in/out of scope
    /// </summary>
    public ulong TrackingId
    {
        get
        {
            return this.vgbFrameSource.TrackingId;
        }

        set
        {
            if (this.vgbFrameSource.TrackingId != value)
            {
                this.vgbFrameSource.TrackingId = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not the detector is currently paused
    /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
    /// </summary>
    public bool IsPaused
    {
        get
        {
            return this.vgbFrameReader.IsPaused;
        }

        set
        {
            if (this.vgbFrameReader.IsPaused != value)
            {
                this.vgbFrameReader.IsPaused = value;
            }
        }
    }

    /// <summary>
    /// Disposes all unmanaged resources for the class
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
    /// </summary>
    /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                this.vgbFrameReader.Dispose();
                this.vgbFrameReader = null;
            }

            if (this.vgbFrameSource != null)
            {
                this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                this.vgbFrameSource.Dispose();
                this.vgbFrameSource = null;
            }
        }
    }

    /// <summary>
    /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
    /// </summary>
    /// <param name="sender">object sending the event</param>
    /// <param name="e">event arguments</param>
    private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
    {
        VisualGestureBuilderFrameReference frameReference = e.FrameReference;
        using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
        {
            if (frame != null)
            {
                // get the discrete gesture results which arrived with the latest frame
                var discreteResults = frame.DiscreteGestureResults;
                var continousResults = frame.ContinuousGestureResults;

                if (discreteResults != null || continousResults != null)
                {
                    foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                    {
                        foreach (var gestureName in _gestureNames[(int)_SFTTestType])
                        {
                            if (gesture.Name.Equals(gestureName) &&
                                gesture.GestureType == GestureType.Discrete && discreteResults != null)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    if (this.OnDiscreteGestureDetected != null)
                                    {
                                        this.OnDiscreteGestureDetected(this,
                                            new DiscreteGestureEventArgs(true, result.Detected, result.Confidence, gesture.Name));
                                    }
                                }
                            }
                            else if (gesture.Name.Equals(gestureName) && gesture.GestureType == GestureType.Continuous && continousResults != null)
                            {
                                ContinuousGestureResult result = null;
                                continousResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    if (this.OnContinuousGestureDetected != null)
                                    {
                                        this.OnContinuousGestureDetected(this,
                                            new ContinuousGestureEventArgs(true, result.Progress, gesture.Name));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles the TrackingIdLost event for the VisualGestureBuilderSource object
    /// </summary>
    /// <param name="sender">object sending the event</param>
    /// <param name="e">event arguments</param>
    private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
    {
        if (this.OnDiscreteGestureDetected != null)
        {
            this.OnDiscreteGestureDetected(this, new DiscreteGestureEventArgs(false, false, 0.0f, ""));
        }

        if (this.OnContinuousGestureDetected != null)
        {
            this.OnContinuousGestureDetected(this, new ContinuousGestureEventArgs(false, 0.0f, ""));
        }
    }
}
