using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class NetworkScanner
{
    #region Parameters

    private static readonly string ScannerTopic = "DeviceInfo";
    private static readonly int PublisherPort = 4119;

    #endregion


    public delegate void ScanCompleteDelegate();
    public static event ScanCompleteDelegate OnScanCompleted;
    public struct DeviceHandleTypeDef
    {
        public string Hostname;
        public string IP;
        public string Port;
    }

    public static List<DeviceHandleTypeDef> Devices = new List<DeviceHandleTypeDef>();
   
    private static int ConnectionTimeout;
    public static bool IsScanning
    {
        get
        {
            lock (Lck_IsScanning)
                return _isScanning;
        }
        private set
        {
            lock (Lck_IsScanning)
                _isScanning = value;
        }
    }
    public static int ScanPercentage
    {
        get
        {
            lock (Lck_ScanPercentage)
                return _scanPercentage;
        }
        private set
        {
            lock (Lck_ScanPercentage)
                _scanPercentage = value;
        }
    }

    private static string MyIP;
    private static string MyHostName;


    private static string IPHeader;
    public static bool IsPublishingEnabled
    {
        get
        {
            lock (Lck_IsDevicePublished)
                return _isPublishingEnabled;
        }
        set
        {
            lock (Lck_IsDevicePublished)
                _isPublishingEnabled = value;
        }
    }

    private static bool _isScanning = false;
    private static int _scanPercentage = 0;
    private static bool _isPublishingEnabled = false;

    private static object Lck_IsScanning = new object();
    private static object Lck_ScanPercentage = new object();
    private static object Lck_IsDevicePublished = new object();



    private static MQPublisher Publisher;

    public static void ScanAvailableDevices(int timeout = 35)
    {
        if (IsScanning)
            return;
        ConnectionTimeout = timeout;
        ScanPercentage = 0;
        string deviceIP, deviceHostname;
        GetDeviceAddress(out deviceIP, out deviceHostname);
        MyIP = deviceIP;
        Devices.Clear();
        char[] splitter = new char[] { '.' };
        var ipStack = deviceIP.Split(splitter);
        IPHeader = "";
        for (int i = 0; i < 3; i++)
        {
            IPHeader += ipStack[i] + ".";
        }
        IsScanning = true;
        Task.Run(() =>
        {
            
            for (int i = 0; i < 256; i++)
            {
                string ip = IPHeader + i.ToString();
                MQSubscriber subscriber = new MQSubscriber(ScannerTopic, ip, PublisherPort);
                subscriber.OnDataReceived += Subscriber_OnDataReceived;
                Thread.Sleep(ConnectionTimeout);
                subscriber.OnDataReceived -= Subscriber_OnDataReceived;
                subscriber.Stop();
                subscriber = null;
                ScanPercentage = (int)(i / 256.0 * 100)+1;
            }
            IsScanning = false;
            if(OnScanCompleted!=null)
            {
                OnScanCompleted();
            }
        });
      
    }

    private static void Subscriber_OnDataReceived(byte[] data)
    {
        if (data == null)
        {
            Debug.WriteLine("Data was null: ");
            return;
        }
        string msg = Encoding.UTF8.GetString(data);
        char[] splitter = new char[] { '&' };
        string[] msgParts = msg.Split(splitter);
        DeviceHandleTypeDef device;
        device.IP = msgParts[0].Substring(3);
        device.Port = msgParts[1].Substring(5);
        device.Hostname = msgParts[2].Substring(9);
        if(!Devices.Contains(device))
            Devices.Add(device);
        Debug.WriteLine("data: " + msg);
        for(int i=0;i<msgParts.Length;i++)
        {
            Debug.WriteLine("msgParts[" + i + "]=" + msgParts[i]);
        }
    }
    public static void PublishDevice()
    {
        IsPublishingEnabled = true;
        Publisher = new MQPublisher(ScannerTopic, Client.GetDeviceIP(), PublisherPort);
        Task.Run(() =>
        {
            GetDeviceAddress(out MyIP, out MyHostName);
            byte[] myID = Encoding.UTF8.GetBytes("IP:" + MyIP + "&Port:" + Main.Port_Screen + "&Hostname:" + MyHostName);
            while (IsPublishingEnabled)
            {
                Publisher.Publish(myID);
                Thread.Sleep(10);
            }
        });
    }
    public static void StopPublishing()
    {
        IsPublishingEnabled = false;
    }
    public static void GetDeviceAddress(out string deviceIP, out string deviceHostname)
    {
        IPAddress localAddr = null;
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localAddr = ip;
            }
        }
        deviceIP = localAddr.ToString();
        deviceHostname = host.HostName;
        MyIP = localAddr.ToString();
    }
}
