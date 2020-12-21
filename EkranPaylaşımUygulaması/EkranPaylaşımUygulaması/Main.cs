using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Main
{

    #region Parameters
    public static int PackSize = 1024 * 1024 * 3;            /// this represents the maximum length of bytes to be transfered to client in one package. default is 3 MB and should be smaller than 64 kB

    #endregion
    #region Variables

    public int FPS
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
    public bool IsSendingEnabled
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
    public bool IsReceivingEnabled
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
    public bool IsImageReceived
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
    public bool IsImageSent
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
    public Bitmap ScreenImage
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

    public double TransferSpeed
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

    private bool _isSendingEnabled = true;
    private bool _isReceivingEnabled = true;
    private bool _isImageReceived = true;
    private bool _isImageSent = true;
    private Bitmap _screenImage;
    private double _transferSpeed;

    private  string _URL;                          /// File Path
    private  Thread sendingThread;
    private  Thread receivingThread;

    private  string _HostName = "";
    private int fps = 0;
    private  object HostName_Lock = new object();
    private  object Lck_IsSendingEnabled = new object();
    private  object Lck_IsReceivingEnabled = new object();
    private  object Lck_FPS = new object();
    private  object Lck_ScreenImage = new object();
    private  object Lck_IsImageReceived = new object();
    private  object Lck_IsImageSent = new object();
    private  object Lck_TransferSpeed = new object();
    
    public  string HostName
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
    private Communication Comm;

    public  enum CommunicationTypes
    {
        Sender,
        Receiver
    }

    public  Main(CommunicationTypes communicationType)
    {
        Comm = new Communication();
        if (communicationType==CommunicationTypes.Sender)
        {
            string serverIP=Comm.CreateServer();
            Debug.WriteLine("Server IP: " + serverIP);            
        }
    }
    /// <summary>
    /// Starts sending slected file to client in another thread.
    /// </summary>
    /// <returns>returns true if transfer is started</returns>
    public bool StartSharingScreen()
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
            string Msg = "sFailedClient"; // "Failed to start sending thread!";
            Debug.WriteLine("Failed to start sending thread! \n " + e.ToString());
            return false;
        }
    }
    /// <summary>
    /// This function is used in a thread to send all file bytes to client.
    /// </summary>
    private void SendingCoreFcn()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int fpsCounter = 0;
        int bytesSent = 0;
        int mb = 1024 * 1024;
        while (IsSendingEnabled)
        {
            string clientHostname = Comm.StartServer();            /// Wait for Client to connect and return the hostname of connected client.
            HostName = clientHostname;
            if (clientHostname == null && clientHostname == "")             /// if connection succeed
            {
                return;
            }
            ImageProcessing.StartGettingFrame();
            Thread.Sleep(500);
            stopwatch.Restart();
            while (Comm.isClientConnected)
            {
               // double t1 = stopwatch.Elapsed.TotalMilliseconds;
                /// get image Here
                /// 
                byte[] imageBytes= ImageProcessing.GetScreenBytes();
                //double t2 = stopwatch.Elapsed.TotalMilliseconds;

                /// Send image to client   here
                /// 
                Comm.SendFilePacks(imageBytes, 0);
                //double t3 = stopwatch.Elapsed.TotalMilliseconds;

                /// Get Response of client here
                /// 
                byte[] responseBytes = Comm.GetResponseFromClient();
                //double t4 = stopwatch.Elapsed.TotalMilliseconds;
               // Debug.WriteLine("  imageTime: " + (t2 - t1) + " ms   sendingTime: " + (t3 - t2) + " ms  Response Time: " + (t4 - t3) + " ms");
                if (responseBytes==null)
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
                    FPS = fpsCounter;
                    fpsCounter = 0;
                    TransferSpeed = (double)bytesSent / mb;
                    bytesSent = 0;
                    stopwatch.Restart();
                }
                IsImageSent = true;
            }
        }
    }
    public void CancelSharing()
    {
        IsSendingEnabled = false;
        ImageProcessing.StopGettingFrames();
        if (sendingThread!=null)
        {
            if (sendingThread.IsAlive)
            {
                sendingThread.Abort();
                sendingThread = null;
                Comm.CloseServer();
            }
        }
    }
    public void StartReceiving(string serverIp)
    {
        Comm.ConnectToServer(serverIp);

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
    private void ReceivingCoreFcn()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int fpsCounter = 0;
        int bytesSent=0;
        int mb = 1024 * 1024;
        while (IsReceivingEnabled)
        {
            stopwatch.Restart();
            while (Comm.isConnectedToServer)
            {
                /// get image bytes
                byte[] ImageBytes = Comm.ReceiveFilePacks();
                Comm.SendResponseToServer();

                /// Create image
                ScreenImage = ImageProcessing.ImageFromByteArray(ImageBytes);
                IsImageReceived = true;
                /// Calculate FPS Rate here
                fpsCounter++;
                bytesSent += ImageBytes.Length;
                if (stopwatch.Elapsed.TotalSeconds >= 1)
                {
                    FPS = fpsCounter;
                    fpsCounter = 0;
                    TransferSpeed = (double)bytesSent / mb;
                    bytesSent = 0;
                    stopwatch.Restart();
                }
            }
        }
    }
    public void StopReceiving()
    {
        Comm.isConnectedToServer = false;
        Comm.CloseClient();
    }
}
