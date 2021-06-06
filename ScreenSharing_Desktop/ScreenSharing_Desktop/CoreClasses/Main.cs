using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

class Main
{

    public static char KEY;
    #region Parameters

    #endregion


    #region Variables

    public static int FPS
    {
        get
        {
            lock (Lck_FPS)
                return fps;
        }
        set
        {
            lock (Lck_FPS)
                fps = value;
        }
    }
    public static bool IsImageShowed
    {
        get
        {
            lock (Lck_IsImageShowed)
                return _isImageShowed;
        }
        set
        {
            lock (Lck_IsImageShowed)
                _isImageShowed = value;
        }
    }
    public static bool IsImageSent
    {
        get
        {
            lock (Lck_IsImageSent)
                return _isImageSent;
        }
        set
        {
            lock (Lck_IsImageSent)
                _isImageSent = value;
        }
    }
    public static Bitmap ScreenImage
    {
        get
        {
            lock (Lck_ScreenImage)
                return _screenImage;
        }
        set
        {
            lock (Lck_ScreenImage)
                _screenImage = value;
        }
    }
    public static double TransferSpeed
    {
        get
        {
            lock (Lck_TransferSpeed)
                return _transferSpeed;
        }
        set
        {
            lock (Lck_TransferSpeed)
                _transferSpeed = value;
        }
    }
    public static  bool IsControlsEnabled
    {
        get
        {
            lock (Lck_IsControlsEnabled)
                return _isControlsEnabled;
        }
        set
        {
            lock (Lck_IsControlsEnabled)
                _isControlsEnabled = value;
        }
    }
    public static CommunicationTypes CommunitionType
    {
        get
        {
            lock (Lck_CommunitionType)
                return _communitionType;
        }
        set
        {
            lock (Lck_CommunitionType)
                _communitionType = value;
        }
    }


    private static bool _isImageShowed = true;
    private static bool _isImageSent = true;
    private static bool _isControlsEnabled = false;
    private static Bitmap _screenImage;
    private static double _transferSpeed;
    private static CommunicationTypes _communitionType;
    private static int fps = 0;
    

    private static object Lck_FPS = new object();
    private static object Lck_ScreenImage = new object();
    private static object Lck_IsImageShowed = new object();
    private static object Lck_IsImageSent = new object();
    private static object Lck_TransferSpeed = new object();
    private static object Lck_IsControlsEnabled = new object();
    private static object Lck_CommunitionType = new object();
    
    private static Thread SenderThread;

    #endregion

    #region MQ Variables

    private static MQPublisher Publisher;
    private static MQSubscriber Subscriber;
    private static string Topic_Screen = "Screen";
    private static string Topic_Command = "Command";
    public static int Port_Screen = 4112;
    public static int Port_Command = 4113;
    public static string MyIP;
    public static string TargetIP;

    private static bool IsPublisherEnabled;
    private static Stopwatch SubStopwatch;
    private static int TotalBytesReceived = 0;
    private static int FpsCounter = 0;
    #endregion

    public enum CommunicationTypes
    {
        Sender,
        Receiver
    }

    /// <summary>
    /// Initializes a MQ Publisher with defined topic at given port
    /// </summary>
    public static void StartSharing()
    {
        MyIP = Client.GetDeviceIP();
        Publisher = new MQPublisher(Topic_Screen, MyIP, Port_Screen);
        ImageProcessing.StartScreenCapturer();
        IsPublisherEnabled = true;
        SenderThread = new Thread(PublisherCoreFcn);
        SenderThread.Start();
    }
    public static void StopSharing()
    {
        try
        {
            IsPublisherEnabled = false;
            SenderThread.Abort();
            ImageProcessing.StopScreenCapturer();
            FPS = 0;
            TransferSpeed = 0;
        }
        catch
        {
            Debug.WriteLine("Failed to Stop Publisher");
        }
    }
    private static void PublisherCoreFcn()
    {
 
        Stopwatch stopwatch = Stopwatch.StartNew();
        int totalBytesSent = 0;
        while (IsPublisherEnabled)
        {
            byte[] screenBytes= ImageProcessing.GetScreenBytes();
            if(screenBytes!=null && Publisher!=null)
            {
                Publisher.Publish(screenBytes);
                totalBytesSent += screenBytes.Length;
                FpsCounter++;
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    UpdateStats(totalBytesSent, stopwatch.Elapsed.TotalSeconds);
                    stopwatch.Restart();
                    totalBytesSent = 0;
                }
                //Thread.Sleep(10);
            }
            else
            {
                Debug.WriteLine("Capturer or Publisher was null. Transfer aborted!");
                break;
            }
        }
    }
    #region Subscriber Function

    public static void StartReceiving(string ip)
    {
        TargetIP = ip;
        Subscriber = new MQSubscriber(Topic_Screen, TargetIP, Port_Screen);
        Subscriber.OnDataReceived += Subscriber_OnDataReceived;
        SubStopwatch = Stopwatch.StartNew();
    }
    private static void Subscriber_OnDataReceived(byte[] data)
    {
        Stopwatch stp = Stopwatch.StartNew();
        if (data != null)
        {
            ScreenImage = ImageProcessing.ImageFromByteArray(data);
            TotalBytesReceived += data.Length;
            FpsCounter++;
            if (SubStopwatch.ElapsedMilliseconds >= 1000)
            {
                UpdateStats(TotalBytesReceived, SubStopwatch.Elapsed.TotalSeconds);
                TotalBytesReceived = 0;
                SubStopwatch.Restart();
            }
            IsImageShowed = true;
        }
        else
        {
            Debug.WriteLine("image data was null!");
        }
    }
    public static void StopReceiving()
    {
        Subscriber.Stop();
        FPS = 0;
        TransferSpeed = 0;
    }
    #endregion

    private static void UpdateStats(int totalbytes,double time)
    {
        double mb = 1024 * 1024;
        double totalMB = totalbytes / mb;
        if (FPS != 0)
            FPS = (int)(FPS * 0.9 + 0.1 * FpsCounter);
        else
            FPS = FpsCounter;
        FpsCounter = 0;
        if (TransferSpeed != 0)
            TransferSpeed = TransferSpeed * 0.9 + 0.1 * (totalMB / time);
        else
            TransferSpeed = totalMB / time;
    }
}
