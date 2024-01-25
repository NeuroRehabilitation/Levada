using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TestDetails
{
    public enum TestType
    {
        _8FtUpGo,
        _30SChair,
        _2MStep
    }

    public enum FeedbackType
    {
        Mirror,
        Abstract,
        Game,
        Control
    }

    public struct TestDescription
    {
        public string UserId;
        public TestType Test;
        public FeedbackType Feedback;
    }

    public struct KinectDescription
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public static TestDescription TestDesc;
    public static KinectDescription KinectDesc;
}
