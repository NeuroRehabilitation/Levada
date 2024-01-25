using Windows.Kinect;
using UnityEngine;

public class KinectFloorSource : MonoBehaviour
{
    private KinectSensor Sensor { get; set; }
    private BodyFrameReader _reader;
    private Windows.Kinect.Vector4 _floor;
    private GameObject _kinect;

    void Start()
    {
        Sensor = KinectSensor.GetDefault();
        if (Sensor != null)
        {
            _reader = Sensor.BodyFrameSource.OpenReader();
            if (!Sensor.IsOpen)
                Sensor.Open();
        }

        _kinect = gameObject.transform.gameObject;
    }

    void Update()
    {
        if (Sensor == null)
            Sensor = KinectSensor.GetDefault();

        if (Sensor != null)
        {
            if (!Sensor.IsOpen)
                Sensor.Open();

            if (Sensor.IsOpen)
            {
                if (_reader == null)
                    _reader = Sensor.BodyFrameSource.OpenReader();
                else
                {
                    UpdateBodyData();
                    UpdateKinectHeightAndRotation();
                }
            }
        }
    }

    private void UpdateBodyData()
    {
        var frame = _reader.AcquireLatestFrame();
        if (frame != null)
        {
            _floor = frame.FloorClipPlane;
            frame.Dispose();
        }
    }

    private void UpdateKinectHeightAndRotation()
    {
        _kinect.transform.position = new Vector3(_kinect.transform.position.x, _floor.W, _kinect.transform.position.z);

        Vector3 floorNormal;
        floorNormal.x = -_floor.X;
        floorNormal.y = _floor.Y;
        floorNormal.z = _floor.Z;
        _kinect.transform.rotation = Quaternion.FromToRotation(floorNormal, Vector3.up);
    }

    void OnDestroy()
    {
        if (_reader != null)
        {
            _reader.Dispose();
            _reader = null;
        }

        if (Sensor == null) return;
        if (Sensor.IsOpen)
            Sensor.Close();
        Sensor = null;
    }
}
