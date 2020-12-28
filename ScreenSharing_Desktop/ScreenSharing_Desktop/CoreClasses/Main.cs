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
    public bool IsControlsEnabled
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
    public bool IsConnectedToServer
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
    public CommunicationTypes CommunitionType
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

    private bool _isSendingEnabled = true;
    private bool _isReceivingEnabled = true;
    private bool _isImageReceived = true;
    private bool _isImageSent = true;
    private bool _isControlsEnabled = false;
    private bool _isConnectedToServer = false;
    private Bitmap _screenImage;
    private double _transferSpeed;
    private CommunicationTypes _communitionType;
    
    private string _URL;                          /// File Path
    private Thread sendingThread;
    private Thread receivingThread;

    private string _HostName = "";
    private int fps = 0;
    private object HostName_Lock = new object();
    private object Lck_IsSendingEnabled = new object();
    private object Lck_IsReceivingEnabled = new object();
    private object Lck_FPS = new object();
    private object Lck_ScreenImage = new object();
    private object Lck_IsImageReceived = new object();
    private object Lck_IsImageSent = new object();
    private object Lck_TransferSpeed = new object();
    private object Lck_IsControlsEnabled = new object();
    private object Lck_IsConnectedToServer = new object();
    private object Lck_CommunitionType = new object();

    public string HostName
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


    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    //Mouse actions
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const int MOUSEEVENTF_RIGHTUP = 0x10;

    private bool wasLeftButtonClicked = false;
    private bool wasRightButtonClicked = false;
    
    public enum CommunicationTypes
    {
        Sender,
        Receiver
    }

    public Main(CommunicationTypes communicationType)
    {
        Comm = new Communication();
        if (communicationType == CommunicationTypes.Sender)
        {
            HostName= Comm.CreateServer();
            Debug.WriteLine("Server IP: " + HostName);
        }
        this.CommunitionType = communicationType;
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
        FPS = 30;
        while (IsSendingEnabled)
        {
            ImageProcessing.StartGettingFrame();
            string clientHostname = Comm.StartServer();            /// Wait for Client to connect and return the hostname of connected client.
            if (clientHostname == null && clientHostname == "")             /// if connection succeed
            {
                return;
            }
            
            Thread.Sleep(500);
            stopwatch.Restart();
            while (Comm.isClientConnected)
            {
                // double t1 = stopwatch.Elapsed.TotalMilliseconds;
                /// get image Here
                /// 
                byte[] imageBytes =  ImageProcessing.GetScreenBytes();

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
                //if (Comm.ReadBit(responseBytes[0], 0))
                //{
                //int cursor_x = responseBytes[1] | responseBytes[2] << 8;
                //int cursor_y = responseBytes[3] | responseBytes[4] << 8;
                //byte ControlByte = responseBytes[0];
                //bool leftClicked = Comm.ReadBit(ControlByte, 1);
                //bool rightClicked = Comm.ReadBit(ControlByte, 2);
                //if (leftClicked || rightClicked)
                //{
                //    if (leftClicked)
                //    {
                //        wasLeftButtonClicked = true;
                //        mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)cursor_x, (uint)cursor_y, 0, 0);
                //    }
                //    else
                //    {
                //        if (wasLeftButtonClicked)
                //        {
                //            mouse_event(MOUSEEVENTF_LEFTUP, (uint)cursor_x, (uint)cursor_y, 0, 0);
                //            wasLeftButtonClicked = false;
                //        }
                //    }
                //    if (rightClicked)
                //    {
                //        wasRightButtonClicked = true;
                //        mouse_event(MOUSEEVENTF_RIGHTDOWN, (uint)cursor_x, (uint)cursor_y, 0, 0);
                //    }
                //    else
                //    {
                //        if (wasRightButtonClicked)
                //        {
                //            mouse_event(MOUSEEVENTF_RIGHTUP, (uint)cursor_x, (uint)cursor_y, 0, 0);
                //            wasRightButtonClicked = false;
                //        }
                //    }
                //}
                //else
                //{
                //    System.Windows.Forms.Cursor.Position = new Point(cursor_x, cursor_y);
                //}
                //Debug.WriteLine("Control Byte: " + Convert.ToString(ControlByte, 2));

                //}
                //double t4 = stopwatch.Elapsed.TotalMilliseconds;
                // Debug.WriteLine("  imageTime: " + (t2 - t1) + " ms   sendingTime: " + (t3 - t2) + " ms  Response Time: " + (t4 - t3) + " ms");


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
        }
    }
    public void CancelSharing()
    {
        IsSendingEnabled = false;
        ImageProcessing.StopGettingFrames();
        if (sendingThread != null)
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
    private void ReceivingCoreFcn()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int fpsCounter = 0;
        int bytesSent = 0;
        int mb = 1024 * 1024;
        FPS = 30;
        while (IsReceivingEnabled)
        {
            stopwatch.Restart();
            while (Comm.isConnectedToServer)
            {
                /// get image bytes
                byte[] ImageBytes = Comm.ReceiveFilePacks();
                /// Create image
                if (ImageBytes == null)
                    continue;
                ScreenImage = ImageProcessing.ImageFromByteArray(ImageBytes);
                int len = 5;
                byte[] data = new byte[len];

                byte ControlByte = 0;
                if (IsControlsEnabled)
                {
                    //data[1] = (byte)((System.Windows.Forms.Cursor.Position.X) & 0xff);
                    //data[2] = (byte)((System.Windows.Forms.Cursor.Position.X >> 8) & 0xff);
                    //data[3] = (byte)((System.Windows.Forms.Cursor.Position.Y) & 0xff);
                    //data[4] = (byte)((System.Windows.Forms.Cursor.Position.Y >> 8) & 0xff);

                    //ControlByte = Comm.WriteToBit(ControlByte, 0, IsControlsEnabled);
                    //ControlByte = Comm.WriteToBit(ControlByte, 1, System.Windows.Forms.Control.MouseButtons == System.Windows.Forms.MouseButtons.Left);
                    //ControlByte = Comm.WriteToBit(ControlByte, 2, System.Windows.Forms.Control.MouseButtons == System.Windows.Forms.MouseButtons.Right);
                    //Debug.WriteLine("Control Byte: " + Convert.ToString(ControlByte, 2));
                }
                data[0] = ControlByte;

                Comm.SendResponseToServer(data);
                IsImageReceived = true;
                /// Calculate FPS Rate here
                fpsCounter++;
                bytesSent += ImageBytes.Length;
                if (stopwatch.Elapsed.TotalSeconds >= 1)
                {
                    FPS =(int) (FPS*0.8+0.2* fpsCounter);
                    ImageProcessing.FPS = FPS;
                    fpsCounter = 0;
                    TransferSpeed = (double)bytesSent / mb;
                    bytesSent = 0;
                    stopwatch.Restart();
                }
            }
            IsConnectedToServer = Comm.isConnectedToServer;
        }
    }
    public void StopReceiving()
    {
        Comm.isConnectedToServer = false;
        Comm.CloseClient();
    }
}
