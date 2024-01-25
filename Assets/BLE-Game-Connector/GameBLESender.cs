using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class GameBLESender : MonoBehaviour
{
    public static GameBLESender Instance { get; set; }
    public static string _ip;
    private const int Port = 1204;
    private IPEndPoint _remoteEndPoint;
    private UdpClient _client;

    public static bool IsGameSending;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        //_ip = LocalIpAdress();
        //Init();
    }
	        
    public void Init()
    {
        //Debug.Log("init");
        if(_ip != null)
        {
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(_ip), Port);
            _client = new UdpClient();
        }


        if(_remoteEndPoint != null)
        {
            SendString("Game,Connected to Game");
            Debug.Log("Game,Connected to Game");
        }
            
    }

	//Invoke this function whenever you want to send a GameVariable update to the BLE
	//Example: GameBLESender.Instance.SendString("GameVariable,BallSpeed,float," + speed + "," + "1,20");
	public void SendString(string message)
    {
        try
        {
            if (message != "")
            {
                var data = Encoding.UTF8.GetBytes(message);
                _client.Send(data, data.Length, _remoteEndPoint);
            }
        }

        catch (Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    private string LocalIpAdress()
    {
        if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        {
            return null;
        }

        var localIp = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            Debug.Log(ip.AddressFamily);
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIp = ip.ToString();
            }
        }
        return localIp;
    }

    private void OnApplicationQuit()
    {
        if (_client != null)
            _client.Close();
    }
}

