using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class _8ftUpGo_Logger
{
    private string _path, _filename;
    private TextWriter _fileLog;
    private readonly string _userId = TestDetails.TestDesc.UserId;
    private bool _initialized;

    // Use this for initialization
    private void Initialize(int trialNumber)
    {
        _path = Application.dataPath + "/Logs/" + _userId + "/";

        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        _filename = _userId + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_8ftUpGo_" + trialNumber + "_" + TestDetails.TestDesc.Feedback;
        _fileLog = new StreamWriter(_path + _filename + ".csv", false);

        var fileHeader = new List<string>
        {
            "Stopwatch",
            "DistanceValid",
            "UserIsSeated",
            "DistanceToMarker",
            "DistanceToChair"
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

    // Update log
    public void Log(_8ftUpGo_Manager.Test test, JointOrientationControl2M joints)
    {
        if (!_initialized)
            Initialize(test.TrialNumber);

        if (!_initialized) return;

        List<string> newline = new List<string>
        {
            test.Stopwatch.ToString(CultureInfo.InvariantCulture),
            test.DistanceValid.ToString(),
            test.UserIsSeated.ToString(),
            test.DistanceToMarker.ToString(CultureInfo.InvariantCulture),
            test.DistanceToChair.ToString(CultureInfo.InvariantCulture),
        };
        foreach (var jointsAvatarBone in joints.AvatarBones)
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
