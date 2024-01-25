/*
If you use or adapt this software in your research please consult
the author at afonso.goncalves@m-iti.org on how to cite it.

Copyright (C) 2017  Afonso Gonçalves 

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
 */

using UnityEngine;
using Windows.Kinect;

public class BodySourceViewer2M : MonoBehaviour
{
    public GameObject _avatar;

    private KinectBodySource2M _bodySource;
    private KinectSensor _sensor;
    public GameObject[] Avatars { get; private set; }
    private bool[] _activeAvatars;

    void Start()
    {
        _bodySource = gameObject.GetComponent<KinectBodySource2M>();
    }

    void Update()
    {

        if (_sensor == null)
        {
            _sensor = _bodySource.Sensor;
            if (_sensor == null) return;

            Avatars = new GameObject[_sensor.BodyFrameSource.BodyCount];
            _activeAvatars = new bool[_sensor.BodyFrameSource.BodyCount];
        }

        Body[] kinectBodies = _bodySource.GetData();
        if (kinectBodies == null)
        {
            //Destroy all avatars that exist
            for (int bodyIndex = 0; bodyIndex < _sensor.BodyFrameSource.BodyCount; bodyIndex++)
            {
                if (Avatars[bodyIndex] == null) continue;

                Destroy(Avatars[bodyIndex]);
                Avatars[bodyIndex] = null;
                _activeAvatars[bodyIndex] = false;
            }
            return;
        }

        for (int bodyIndex = 0; bodyIndex < _sensor.BodyFrameSource.BodyCount; bodyIndex++)
        {
            if (kinectBodies[bodyIndex] != null)
            {
                if (!_activeAvatars[bodyIndex] && kinectBodies[bodyIndex].IsTracked)
                {
                    Avatars[bodyIndex] = Instantiate(_avatar);
                    Avatars[bodyIndex].transform.SetParent(gameObject.transform);
                    Avatars[bodyIndex].GetComponent<JointPositionControl2M>().SetBodyIndex(bodyIndex);
                    Avatars[bodyIndex].GetComponent<JointOrientationControl2M>().SetBodyIndex(bodyIndex);
                    _activeAvatars[bodyIndex] = true;
                }
                else if (_activeAvatars[bodyIndex] && !kinectBodies[bodyIndex].IsTracked)
                {
                    //Destroy the avatar if it exists but the corresponding body is not being tracked
                    Destroy(Avatars[bodyIndex]);
                    Avatars[bodyIndex] = null;
                    _activeAvatars[bodyIndex] = false;
                }
            }
            else
            {
                //Destroy the avatar if the body is null
                Destroy(Avatars[bodyIndex]);
                Avatars[bodyIndex] = null;
                _activeAvatars[bodyIndex] = false;
            }
        }
    }

    public void SetBodiesInvisibleExcept(int body)
    {
        for (int bodyIndex = 0; bodyIndex < _sensor.BodyFrameSource.BodyCount; bodyIndex++)
        {
            if (Avatars[bodyIndex] == null) continue;
            if (bodyIndex == body) continue;
            var renders = Avatars[bodyIndex].GetComponentsInChildren<MeshRenderer>();
            foreach (var render in renders)
            {
                render.enabled = false;
            }
        }
    }

    public void SetBodyVisible(int body)
    {
        if (Avatars[body] == null) return;
        var renders = Avatars[body].GetComponentsInChildren<MeshRenderer>();
        foreach (var render in renders)
        {
            render.enabled = false;//true; by me*****
        }
    }


    /*  public void SetAllBodiesVisible()
      {
          if (_sensor == null) return;

          for (int bodyIndex = 0; bodyIndex < _sensor.BodyFrameSource.BodyCount; bodyIndex++)
          {
              if (Avatars[bodyIndex] == null) continue;
              var renders = Avatars[bodyIndex].GetComponentsInChildren<MeshRenderer>();
              foreach (var render in renders)
              {
                  render.enabled = true;
              }
          }
      }
      */
}
