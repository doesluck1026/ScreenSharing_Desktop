using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Main
{

    #region Parameters
    public static int PackSize = 1024 * 1024 * 3;            /// this represents the maximum length of bytes to be transfered to client in one package. default is 3 MB and should be smaller than 64 kB

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
    public static bool IsSendingEnabled
    {
        get
        {
            lock (Lck_IsSendingEnabled)
                return _isSendingEnabled;
        }
        set
        {
            lock (Lck_IsSendingEnabled)
                _isSendingEnabled = value;
        }
    }
    public static bool IsReceivingEnabled
    {
        get
        {
            lock (Lck_IsReceivingEnabled)
                return _isReceivingEnabled;
        }
        set
        {
            lock (Lck_IsReceivingEnabled)
                _isReceivingEnabled = value;
        }
    }
    public static bool IsImageReceived
    {
        get
        {
            lock (Lck_IsImageReceived)
                return _isImageReceived;
        }
        set
        {
            lock (Lck_IsImageReceived)
                _isImageReceived = value;
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
    public static bool IsConnectedToServer
    {
        get
        {
            lock (Lck_IsConnectedToServer)
                return _isConnectedToServer;
        }
        set
        {
            lock (Lck_IsConnectedToServer)
                _isConnectedToServer = value;
        }
    }
    public static bool IsConnectedToClient
    {
        get
        {
            lock (Lck_IsConnectedToClient)
                return _isConnectedToClient;
        }
        set
        {
            lock (Lck_IsConnectedToClient)
                _isConnectedToClient = value;
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

    private static bool _isSendingEnabled = true;
    private static bool _isReceivingEnabled = true;
    private static bool _isImageReceived = true;
    private static bool _isImageSent = true;
    private static bool _isControlsEnabled = false;
    private static bool _isConnectedToServer = false;
    private static bool _isConnectedToClient = false;
    private static Bitmap _screenImage;
    private static double _transferSpeed;
    private static CommunicationTypes _communitionType;

    private static Thread sendingThread;
    private static Thread receivingThread;

    private static string _HostName = "";
    private static int fps = 0;
    private static object HostName_Lock = new object();
    private static object Lck_IsSendingEnabled = new object();
    private static object Lck_IsReceivingEnabled = new object();
    private static object Lck_FPS = new object();
    private static object Lck_ScreenImage = new object();
    private static object Lck_IsImageReceived = new object();
    private static object Lck_IsImageSent = new object();
    private static object Lck_TransferSpeed = new object();
    private static object Lck_IsControlsEnabled = new object();
    private static object Lck_IsConnectedToServer = new object();
    private static object Lck_IsConnectedToClient = new object();
    private static object Lck_CommunitionType = new object();

    public static string HostName
    {

        get
        {
            lock (HostName_Lock)
            {
                return _HostName;
            }
        }
        set
        {
            lock (HostName_Lock)
            {
                _HostName = value;
            }
        }
    }

    #endregion
    public static Communication Comm;

    public enum CommunicationTypes
    {
        Sender,
        Receiver
    }

    public static void InitCommunication(CommunicationTypes communicationType)
    {
        Comm = new Communication();
        if (communicationType == CommunicationTypes.Sender)
        {
            HostName = Comm.CreateServer();
            Debug.WriteLine("Server IP: " + HostName);
        }
        CommunitionType = communicationType;
    }
    /// <summary>
    /// Starts sending slected file to client in another thread.
    /// </summary>
    /// <returns>returns true if transfer is started</returns>
    public static bool StartSharingScreen()
    {
        try
        {
            sendingThread = new Thread(SendingCoreFcn);                             /// Start Sending File
            sendingThread.Start();
            Debug.WriteLine("Wait for Client.");
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to start sending thread! \n " + e.ToString());
            return false;
        }
    }
    /// <summary>
    /// This function is used in a thread to send all file bytes to client.
    /// </summary>
    private static void SendingCoreFcn()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int fpsCounter = 0;
        int bytesSent = 0;
        int mb = 1024 * 1024;
        FPS = 30;
        while (IsSendingEnabled)
        {
            IsConnectedToClient = false;
            string clientHostname = Comm.StartServer();            /// Wait for Client to connect and return the hostname of connected client.
            ImageProcessing.StartGettingFrame();
            if (clientHostname == null && clientHostname == "")             /// if connection succeed
            {
                return;
            }
            Thread.Sleep(500);
            stopwatch.Restart();
            IsConnectedToClient = Comm.isClientConnected;
            while (Comm.isClientConnected)
            {
                try
                {
                    // double t1 = stopwatch.Elapsed.TotalMilliseconds;
                    /// get image Here
                    /// 
                    byte[] imageBytes = ImageProcessing.GetScreenBytes();

                    //double t2 = stopwatch.Elapsed.TotalMilliseconds;
                    /// Send image to client   here
                    /// 
                    Comm.SendFilePacks(imageBytes, 0);
                    //double t3 = stopwatch.Elapsed.TotalMilliseconds;

                    /// Get Response of client here
                    /// 
                    byte[] responseBytes = Comm.GetResponseFromClient();
                    if (responseBytes == null)
                    {
                        Comm.isClientConnected = false;
                        break;
                    }


                    /// Calculate FPS Rate here
                    /// 
                    fpsCounter++;
                    bytesSent += imageBytes.Length;
                    if (stopwatch.Elapsed.TotalSeconds >= 1)
                    {
                        FPS = (int)(FPS * 0.8 + 0.2 * fpsCounter);
                        ImageProcessing.FPS = FPS;
                        fpsCounter = 0;
                        TransferSpeed = (double)bytesSent / mb;
                        bytesSent = 0;
                        stopwatch.Restart();
                    }
                    IsImageSent = true;
                }
                catch (Exception e)
                {
                }
            }
        }
    }
    public static void CancelSharing()
    {
        IsSendingEnabled = false;
        ImageProcessing.StopGettingFrames();
        Comm.CloseServer();
        if (sendingThread != null)
        {
            if (sendingThread.IsAlive)
            {
                sendingThread.Abort();
                sendingThread = null;
            }
        }
    }
    public static void StartReceiving(string serverIp)
    {
        IsConnectedToServer=Comm.ConnectToServer(serverIp);

        try
        {
            receivingThread = new Thread(ReceivingCoreFcn);                             /// Start Sending File
            receivingThread.Start();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to start sending thread! \n " + e.ToString());
        }
    }
    private static void ReceivingCoreFcn()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int fpsCounter = 0;
        int bytesSent = 0;
        int mb = 1024 * 1024;
        FPS = 30;
        stopwatch.Restart();
        int errorCounter = 0;
        while (Comm.isConnectedToServer)
        {
            try
            {
                /// get image bytes
                byte[] ImageBytes = Comm.ReceiveFilePacks();
                /// Create image
                if (ImageBytes == null)
                {
                    errorCounter++;
                    if(errorCounter>3)
                    {
                        Comm.isConnectedToServer = false;
                        IsConnectedToServer = false;
                        break;
                    }
                    continue;
                }
                errorCounter = 0;
                ScreenImage = ImageProcessing.ImageFromByteArray(ImageBytes);
                int len = 5;
                byte[] data = new byte[len];

                data[0] = 0;

                Comm.SendResponseToServer(data);
                IsImageReceived = true;
                /// Calculate FPS Rate here
                fpsCounter++;
                bytesSent += ImageBytes.Length;
                if (stopwatch.Elapsed.TotalSeconds >= 1)
                {
                    FPS = (int)(FPS * 0.8 + 0.2 * fpsCounter);
                    ImageProcessing.FPS = FPS;
                    fpsCounter = 0;
                    TransferSpeed = (double)bytesSent / mb;
                    bytesSent = 0;
                    stopwatch.Restart();
                }
            }
            catch (Exception e)
            {
                IsConnectedToServer = Comm.isConnectedToServer;
            }
        }
        IsConnectedToServer = false;
    }
    public static void StopReceiving()
    {
        Comm.isConnectedToServer = false;
        Comm.CloseClient();
    }
}
