
   
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class NetworkScanner
{
    public delegate void ScanCompleteDelegate();
    public static event ScanCompleteDelegate OnScanCompleted;
    public static event ScanCompleteDelegate OnClientConnected;
    private static int ConnectionTimeout;

    public struct DeviceHandleTypeDef
    {
        public string Hostname;
        public string IP;
        public string Port;
    }

    public static List<DeviceHandleTypeDef> PublisherDevices = new List<DeviceHandleTypeDef>();
    public static List<DeviceHandleTypeDef> SubscriberDevices = new List<DeviceHandleTypeDef>();
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

    private static int[] scanProgressArr;
    /// CDS Bot için bu satırı kullan diğerleri için alttakini
    public static string MyIP = "192.168.3.103";
    //private static string MyIP;
    public static string MyHostname;
    private static readonly int PublishPort = 4119;
    private static Server publisherServer;

    private static string IPHeader;
    public static bool IsDevicePublished = false;

    private static bool _isScanning = false;
    private static int _scanPercentage = 0;

    private static object Lck_IsScanning = new object();
    private static object Lck_ScanPercentage = new object();

    private static int ScanCounter = 0;
    public static void ScanAvailableDevices(int timeout = 200)
    {
        if (IsScanning)
            return;
        ConnectionTimeout = timeout;
        ScanPercentage = 0;
        GetDeviceAddress(out MyIP, out MyHostname);
        Debug.WriteLine("My IP: " + MyIP);
        PublisherDevices.Clear();
        char[] splitter = new char[] { '.' };
        var ipStack = MyIP.Split(splitter);
        IPHeader = "";
        for (int i = 0; i < 3; i++)
        {
            IPHeader += ipStack[i] + ".";
        }

        IsScanning = true;
        Task.Run(() =>
        {
            int numTasks = 8;
            int stackSize = 256 / numTasks;
            scanProgressArr = new int[numTasks];
            for (int i = 0; i < numTasks; i++)
            {
                ParallelScan(stackSize * i, stackSize * (i + 1), i);
            }
            Task.Run(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (true)
                {
                    int percentage = 0;
                    for (int i = 0; i < numTasks; i++)
                    {
                        percentage += scanProgressArr[i];
                    }
                    percentage /= numTasks;
                    //Debug.WriteLine("percentage: " + percentage);
                    ScanPercentage = percentage;
                    if (percentage >= 99 || stopwatch.Elapsed.TotalSeconds > 12)
                        break;
                    Thread.Sleep(50);
                }
                stopwatch.Stop();
                Debug.WriteLine("scan time: " + stopwatch.Elapsed.TotalSeconds + " s");
                if (OnScanCompleted != null)
                    OnScanCompleted();
                else
                {
                    ScanCounter++;
                    if (ScanCounter < 3 && PublisherDevices.Count < 1)
                        ScanAvailableDevices();
                    else
                        ScanCounter = 3;
                }
                IsScanning = false;
            });
        });

    }

    private static void ParallelScan(int startx, int endx, int progressIndex)
    {
        Task.Run(() =>
        {
            Stopwatch stp = Stopwatch.StartNew();
            int progress = 0;
            for (int i = startx; i < endx; i++)
            {
                try
                {
                    string targetIP = IPHeader + i.ToString();
                    if (targetIP == MyIP)
                        continue;
                    GetDeviceData(targetIP);
                    progress = (int)(((i - startx) / (double)(endx - startx - 1)) * 100.0);
                    scanProgressArr[progressIndex] = progress;
                }
                catch
                {

                }
            }

        });
    }
    private static void GetDeviceData(string IP)
    {
        //Stopwatch stp = Stopwatch.StartNew();
        var client = new Client(port: PublishPort, ip: IP);
        string clientIP = client.ConnectToServer(ConnectionTimeout);
        if (string.IsNullOrEmpty(clientIP))
        {
            //Debug.WriteLine("Connection Failed on: " + IP);
        }
        else
        {
            var data = client.GetData();
            if (data == null)
            {
                Debug.WriteLine("Data was null: " + IP);
                return;
            }
            var device = AnalyzeDeviceData(data);
            PublisherDevices.Add(device);
            client.SendDataServer(Encoding.UTF8.GetBytes("IP:" + MyIP + "&Port:" + RemoteControl.Port + "&DeviceName:" + MyHostname));
            client.DisconnectFromServer();
        }
    }
    public static void PublishDevice()
    {
        GetDeviceAddress(out MyIP, out MyHostname);
        publisherServer = new Server(port: PublishPort,ip: MyIP);
        publisherServer.SetupServer();
        publisherServer.StartListener();
        publisherServer.OnClientConnected += PublisherServer_OnClientConnected;
        Debug.WriteLine("Publisher started!");
        IsDevicePublished = true;
        SubscriberDevices.Clear();
    }

    private static void PublisherServer_OnClientConnected(string clientIP)
    {
        Debug.WriteLine("Client IP: " + clientIP);
        publisherServer.SendDataToClient(Encoding.UTF8.GetBytes("IP:" + MyIP + "&Port:" + Main.Port + "&DeviceName:" + MyHostname));

        byte[] data=publisherServer.GetData();
        var device = AnalyzeDeviceData(data);
        SubscriberDevices.Add(device);
        publisherServer.CloseServer();
        if(OnClientConnected!=null)
        {
            OnClientConnected();
        }
        PublishDevice();
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
        if (string.IsNullOrEmpty(MyIP))
            deviceIP = localAddr.ToString();
        else
            deviceIP = MyIP;
        deviceHostname = host.HostName;
    }
    private static DeviceHandleTypeDef AnalyzeDeviceData(byte[] data)
    {
        string msg = Encoding.UTF8.GetString(data);
        char[] splitter = new char[] { '&' };
        string[] msgParts = msg.Split(splitter);
        DeviceHandleTypeDef device;
        device.IP= msgParts[0].Substring(3);
        device.Port = msgParts[1].Substring(5);
        device.Hostname = msgParts[2].Substring(11);
        Debug.WriteLine(" device.IP: "  +device.IP + " device.Port: " + device.Port + " device.Hostname: " + device.Hostname);
        Debug.WriteLine("data: " + msg);
        return device;
    }
}

