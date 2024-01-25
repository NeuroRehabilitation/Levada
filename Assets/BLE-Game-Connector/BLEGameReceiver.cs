using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class BLEGameReceiver : MonoBehaviour
{
    private Thread _receiveThread;
    private UdpClient _client;

    public static int Port = 2000;//port must be defined in BLE (for sending)
    public static bool IsConnected, CanConnect;
	public static float bottom_left=0, bottom_right=0, top_left=0, top_right=0, center_gravity_X=0, center_gravity_y=0;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
       // Debug.Log("Connected = "+ IsConnected);
        if (CanConnect && !IsConnected)
        {
            Init();
            CanConnect = false;
        }
        else if (CanConnect && IsConnected)
        {
            CloseConnection();
        }
    }

    private void Init()
    {
        _receiveThread = new Thread(ReceiveData) {IsBackground = true};
        _receiveThread.Start();
        IsConnected = true;
    }


    private void ReceiveData()
    {
        _client = new UdpClient(Port);
        //Debug.Log(_client);
        while (IsConnected)
        {
            //Debug.Log("ola");
            try
            {
                var ip = new IPEndPoint(IPAddress.Any, Port);
                var udpdata = _client.Receive(ref ip);
                var data = Encoding.UTF8.GetString(udpdata);

                Debug.Log(ip);

                TranslateData(data);
            }

            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

	//Create the variables of the objects that you want to update from the BLE and drag it in the inspector
    //Example: public PlayerController PlayerController;
    private void TranslateData(string data)
    {
        
        string[] separators = { "[$]", "[$$]", "[$$$]", ",", ";", " " };

        var words = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < words.Length; i++)
        {
            if(words[i] == "LocalIP")
            {
                GameBLESender._ip = words[i + 1];

                //Debug.Log(GameBLESender._ip);

                if (GameBLESender._ip != null)
                {
                    GameBLESender.Instance.Init();
                }                    
            }

            if (words[i] == "Height")
            {
                _2mStepTest_Manager.height = float.Parse(words[i + 1]);
            }

            if(words[i] == "LocalIP")
            {
                GameBLESender._ip = words[i + 1];
            }

			if (words [i].Equals ("Bottom_Left")) {
				bottom_left = float.Parse(words [i + 2]); 
			} else if (words [i].Equals ("Bottom_Right")) {
				bottom_right = float.Parse(words [i + 2]); 
			} else if (words [i].Equals ("Top_Left")) {
				top_left = float.Parse(words [i + 2]); 
			} else if (words [i].Equals ("Top_Right")) {
				top_right =float.Parse(words [i + 2]); 
			} else if (words [i].Equals ("CGravity_X")) {
				center_gravity_X =float.Parse(words [i + 2]); 
			} else if (words [i].Equals ("CGravity_Y")) {
				center_gravity_y = float.Parse(words [i + 2]); 
			}
			else
			{
				;// do nothing 
			}	
        }
    }

    private void CloseConnection()
    {
        if (IsConnected)
        {
            _receiveThread.Abort();
            if(_client != null)
                _client.Close();
            IsConnected = false;
            CanConnect = false;
        }
    }

    private void OnDisable()
    {
        CloseConnection();
    }

    private void OnApplicationQuit()
    {
        CloseConnection();
    }
}

