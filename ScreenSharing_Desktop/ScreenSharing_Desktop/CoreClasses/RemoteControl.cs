using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


/// <summary>
/// This Class is used to control other device's keyboard and mouse remotely.
/// There is a publisher that publishes control commands and a subscriber which receives those comments and apply them to computer.
/// </summary>
class RemoteControl
{
    #region Parameters

    

    /// <summary>
    /// Port number where commands are transferred.
    /// </summary>
    public static int Port = 4113;

    /// <summary>
    /// Topic name of transfer
    /// </summary>
    private static string Topic = "Command";

    /// <summary>
    /// MQ Publisher and subscriber objects.
    /// </summary>
    private static MQPublisher Publisher;
    private static MQSubscriber Subscriber;

    /// <summary>
    /// Number of elements in keyboard array.
    /// </summary>
    private static int NumKeys = 173;  
    /// <summary>
    /// Number of Elements in Mouse data.
    /// </summary>
    private static int LenMouseData = 20;

    #endregion

    /// <summary>
    /// This structure is used to store all mouse related parameters in one variable.
    /// </summary>
    public struct MouseHandleTypeDef
    {
        public MouseButtonState LeftButton;
        public MouseButtonState RightButton;
        public MouseButtonState MiddleButton;
        public int ScrollDelta;
        public System.Drawing.Point Position;
        public bool DoubleClick;
    }


    #region Variables

    public static string MyIP;
    public static string TargetIP;
    public static bool IsPublisherEnabled;
    public static bool IsSubscriberEnabled;
    private static bool IsDataReceived = false;
    public static byte[] Keys = new byte[NumKeys];
    private static bool[] Keys_States = new bool[NumKeys];
    public static MouseHandleTypeDef VirtualMouse;
    private static bool[] MouseButtonStates = new bool[3];

    private static Thread Thread_Control;
    private static Thread Thread_Publisher;

    private static byte[] ReceivedData;
    private static object Lck_ReceivedData = new object();
    private static int TimeoutCounter = 0;

    public static bool IsDataUpdated { get; set; }

    /// <summary>
    /// Determines whether received commands will be applied.
    /// </summary>
    public static bool IsControlsEnabled { get; set; }

    private static double ControllerPeriod = 0.001;
    #endregion


    #region Publisher Functions


    /// <summary>
    /// Initializes a MQ Publisher with defined topic at given port
    /// </summary>
    public static void StartSendingCommands()
    {
        MyIP = Client.GetDeviceIP();
        Publisher = new MQPublisher(Topic, MyIP, Port);
        IsPublisherEnabled = true;
        Thread_Publisher = new Thread(Publisher_CoreFcn);
        Thread_Publisher.Start();
    }
    /// <summary>
    /// Stops Publisher and kills the related thread.
    /// </summary>
    public static void StopSendingCommands()
    {
        try
        {
            IsPublisherEnabled = false;
            if(Thread_Publisher!=null)
            {
                if (Thread_Publisher.IsAlive)
                    Thread_Publisher.Abort();
            }
            if(Publisher!=null)
            {
                Publisher.Stop();
                Publisher = null;
            }
        }
        catch
        {
            Debug.WriteLine("Failed to Stop Publisher");
        }
    }
    /// <summary>
    /// This Function runs in Publisher thread and publishes data continueously.
    /// Calling "StopSendingCommands()" function will stop this thread.
    /// </summary>
    private static void Publisher_CoreFcn()
    {
        Stopwatch watch = Stopwatch.StartNew();
        Stopwatch TimeoutWatch = Stopwatch.StartNew();
        while(IsPublisherEnabled)
        {
            if (IsDataUpdated)
            {
                IsDataUpdated = false;
                byte[] data = new byte[LenMouseData + NumKeys];
                Array.Copy(Keys, 0, data, 0, NumKeys);
                byte[] mouseData = PrepareMouseData();
                Array.Copy(mouseData, 0, data, NumKeys, LenMouseData);
                Publisher.Publish(data);
                for (int i = 0; i < Keys.Length; i++)
                {
                    Key key = (Key)i;
                    if (key == Key.LeftAlt || key == Key.RightAlt || key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftShift || key == Key.RightShift)
                    {
                       
                    }
                    else
                    {
                        Keys[i] = 0;
                    }
                }
                TimeoutWatch.Restart();
            }
            else
            {
                if(TimeoutWatch.Elapsed.TotalSeconds>10)
                {
                    TimeoutWatch.Restart();
                    byte[] data = Encoding.ASCII.GetBytes("ImAlive");
                    Publisher.Publish(data);
                    Thread.Sleep(1);
                    Publisher.Publish(data);
                }
            }
            while (watch.Elapsed.TotalSeconds <= ControllerPeriod)
                Thread.Sleep(1);
        }
    }
    /// <summary>
    /// Prepares a byte array which contains basic mouse functions according to given parameters.
    /// </summary>
    /// <returns>Returns a byte array</returns>
    private static byte[] PrepareMouseData()
    {
        byte[] mouseData = new byte[LenMouseData];
        Array.Copy(BitConverter.GetBytes(VirtualMouse.Position.X), 0, mouseData, 0, sizeof(int));
        Array.Copy(BitConverter.GetBytes(VirtualMouse.Position.Y), 0, mouseData, 4, sizeof(int));
        mouseData[8] = (byte)VirtualMouse.LeftButton;
        mouseData[9] = (byte)VirtualMouse.RightButton;
        mouseData[10] = (byte)VirtualMouse.MiddleButton;
        Array.Copy(BitConverter.GetBytes(VirtualMouse.ScrollDelta), 0, mouseData, 11, sizeof(int));
        mouseData[15] = 0;
        mouseData[15] |= (VirtualMouse.DoubleClick? (byte)1 : (byte)0);

        VirtualMouse.ScrollDelta = 0;
        VirtualMouse.DoubleClick = false;
        return mouseData;
    }

    #endregion

    #region Subscriber Function

    /// <summary>
    /// Starts receiving commands from publisher with given IP address.
    /// After calling this function, an event will be thrown every time a data received.
    /// Process this data either in a thread or in the event itself(not recommended for big data)
    /// </summary>
    /// <param name="ip">Ip Address of Publisher</param>
    public static void StartReceiving(string ip)
    {
        TargetIP = ip;
        if (!IsSubscriberEnabled)
        {
            Subscriber = new MQSubscriber(Topic, TargetIP, Port);
            Subscriber.OnDataReceived += Subscriber_OnDataReceived;
            Thread_Control = new Thread(Control_CoreFcn);
            Thread_Control.Start();
        }
        IsSubscriberEnabled = true;
    }

    /// <summary>
    /// Control Thread's Core Function where received data is processed.
    /// </summary>
    private static void Control_CoreFcn()
    {
        Stopwatch watch = Stopwatch.StartNew();
        Stopwatch TimeoutWatch = Stopwatch.StartNew();

        while (IsSubscriberEnabled)
        {
            if (IsDataReceived && IsControlsEnabled)
            {
                byte[] receivedData;
                lock (Lck_ReceivedData)
                {
                    receivedData = new byte[ReceivedData.Length];
                    ReceivedData.CopyTo(receivedData, 0);
                }
                IsDataReceived = false;
                if (receivedData.Length <= 10)
                    continue;
                Keys = new byte[NumKeys];
                byte[] mouseData = new byte[LenMouseData];
                Array.Copy(receivedData, 0, Keys, 0, Keys.Length);
                Array.Copy(receivedData, NumKeys, mouseData, 0, LenMouseData);
                HandleKeyBoard(Keys);
                HandleMouse(mouseData);
                TimeoutWatch.Restart();
            }
            if (!IsDataReceived)
            {
                
                if (TimeoutWatch.Elapsed.TotalSeconds > 20)
                {
                    Main.IsSubscriberTimedOut = true;
                }
            }
            while (watch.Elapsed.TotalSeconds <= ControllerPeriod)
                Thread.Sleep(1);
        }
    }

    /// <summary>
    /// This event will be thrown every time a data is received from publisher.
    /// </summary>
    /// <param name="data">Received Byte array</param>
    private static void Subscriber_OnDataReceived(byte[] data)
    {
        if (data != null)
        {
            lock(Lck_ReceivedData)
            {
                ReceivedData = new byte[data.Length];
                data.CopyTo(ReceivedData,0);
            }
            IsDataReceived = true;
            TimeoutCounter = 0;
        }
    }

    /// <summary>
    /// Stops Receiving command from publisher.
    /// </summary>
    public static void StopReceiving()
    {
        IsSubscriberEnabled = false;
        if (Subscriber != null)
            Subscriber.Stop();
        Subscriber = null;
    }

    /// <summary>
    /// Applies received commands to keyboard.
    /// This function wont be called unless IsControlsEnabled variable is true
    /// </summary>
    /// <param name="Keys">a byte array that contains key states for each key. if key state is 1: key is down, if it is zero, key is up</param>
    private static void HandleKeyBoard(byte[] Keys)
    {
        for (int i = 0; i < Keys.Length; i++)
        {
            Key key = (Key)i;
            if (key == Key.LeftAlt || key == Key.RightAlt || key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftShift || key == Key.RightShift)
            {
                if (Keys[i] == 1)
                {
                    Keyboard.Press(key);
                    Keys_States[i] = true;
                }
                else
                {   /// Calling Keyboard.Release Function when the key is not pressed, causes too many problems so better check if the key was pressed before.
                    if (Keys_States[i])
                    {
                        Keyboard.Release(key);
                    }
                }
            }
            else
            {
                /// if the related key is an ordinary key, then we can simulate a type event.
                if (Keys[i] == 1)
                {
                    Keyboard.Type(key);
                }
            }
        }
    }
    /// <summary>
    /// Applies received commands to mouse.
    /// This function wont be called unless IsControlsEnabled variable is true
    /// </summary>
    /// <param name="mouseData">a byte array that contains mouse commands</param>
    private static void HandleMouse(byte[] mouseData)
    {
        MouseHandleTypeDef mouse;
        mouse.Position = new System.Drawing.Point(BitConverter.ToInt32(mouseData, 0), BitConverter.ToInt32(mouseData, 4));
        mouse.LeftButton = (MouseButtonState)mouseData[8];
        mouse.RightButton = (MouseButtonState)mouseData[9];
        mouse.MiddleButton = (MouseButtonState)mouseData[10];
        mouse.ScrollDelta = BitConverter.ToInt32(mouseData, 11);
        mouse.DoubleClick = ((mouseData[15]) & 0x01) == 0x01;

        if (mouse.DoubleClick)
            Mouse.DoubleClick(MouseButton.Left);
        if (mouse.LeftButton == MouseButtonState.Pressed)
        {
            Mouse.Down(MouseButton.Left);
            MouseButtonStates[0] = true;
        }
        else
        {
            if(MouseButtonStates[0])
            {
                Mouse.Up(MouseButton.Left);
                MouseButtonStates[0] = false;
            }
        }

        if (mouse.RightButton == MouseButtonState.Pressed)
        {
            Mouse.Down(MouseButton.Right);
            MouseButtonStates[1] = true;
        }
        else
        {
            if (MouseButtonStates[1])
            {
                Mouse.Up(MouseButton.Right);
                MouseButtonStates[1] = false;
            }
        }

        if (mouse.MiddleButton == MouseButtonState.Pressed)
        {
            Mouse.Down(MouseButton.Middle);
            MouseButtonStates[2] = true;
        }
        else
        {
            if (MouseButtonStates[2])
            {
                Mouse.Up(MouseButton.Middle);
                MouseButtonStates[2] = false;
            }
        }
        Mouse.MoveTo(mouse.Position);
        if(mouse.ScrollDelta!=0)
            Mouse.Scroll(mouse.ScrollDelta);
    }
    #endregion
}
