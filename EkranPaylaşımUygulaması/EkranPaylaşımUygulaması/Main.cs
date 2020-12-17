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

    private bool _isSendingEnabled = true;
    private bool _isReceivingEnabled = true;
    private bool _isImageReceived = true;
    private Bitmap _screenImage;

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
    private  bool StartSharingScreen()
    {
        try
        {
            sendingThread = new Thread(SendingCoreFcn);                             /// Start Sending File
            sendingThread.Start();
            string Msg = "sWaitClient"; //resMng.GetString("sWaitClient", culInfo);  // "Wait for Client.";
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
        while (IsSendingEnabled)
        {
            string clientHostname = Comm.StartServer();            /// Wait for Client to connect and return the hostname of connected client.
            HostName = clientHostname;
            if (clientHostname == null && clientHostname == "")             /// if connection succeed
            {
                return;
            }
            stopwatch.Restart();
            while (Comm.isClientConnected)
            {
                /// get image Here
                /// 
                byte[] imageBytes= Screen.GetImageBytes();

                /// Send image to client   here
                /// 
                Comm.SendFilePacks(imageBytes, 0);

                /// Calculate FPS Rate here
                /// 
                fpsCounter++;
                if(stopwatch.Elapsed.TotalSeconds>=1)
                {
                    FPS = fpsCounter;
                    stopwatch.Restart();
                }




            }
        }
    }
    public void CancelSharing()
    {
        IsSendingEnabled = false;
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
        while (IsReceivingEnabled)
        {
            stopwatch.Restart();
            while (Comm.isConnectedToServer)
            {
                /// get image bytes
                byte[] ImageBytes = Comm.ReceiveFilePacks();

                /// Create image
                ScreenImage = Screen.GetImage(ImageBytes);
                IsImageReceived = true;
                /// Calculate FPS Rate here
                fpsCounter++;
                if (stopwatch.Elapsed.TotalSeconds >= 1)
                {
                    FPS = fpsCounter;
                    stopwatch.Restart();
                }
            }
        }
    }
}
