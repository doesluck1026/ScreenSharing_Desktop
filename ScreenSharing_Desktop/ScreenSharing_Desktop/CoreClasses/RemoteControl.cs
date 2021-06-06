using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

class RemoteControl
{

    public static int Port = 4113;
    private static string Topic = "Command";
    private static MQPublisher Publisher;
    private static MQSubscriber Subscriber;

    private static Thread SenderThread;

    public static string MyIP;
    public static string TargetIP;
    private static Stopwatch SubStopwatch;
    public static bool IsPublisherEnabled;

    public static byte[] Keys = new byte[256];

    /// <summary>
    /// Initializes a MQ Publisher with defined topic at given port
    /// </summary>
    public static void StartSendingCommands()
    {
        MyIP = Client.GetDeviceIP();
        Publisher = new MQPublisher(Topic, MyIP, Port);
        ImageProcessing.StartScreenCapturer();
        IsPublisherEnabled = true;
        SenderThread = new Thread(PublisherCoreFcn);
        SenderThread.Start();
    }
    public static void StopSendingCommands()
    {
        try
        {
            IsPublisherEnabled = false;
            SenderThread.Abort();
        }
        catch
        {
            Debug.WriteLine("Failed to Stop Publisher");
        }
    }
    private static void PublisherCoreFcn()
    {

        while (IsPublisherEnabled)
        {
            if (Publisher != null)
            {
                Publisher.Publish(Keys);
                Debug.WriteLine("key sent " + (char)Keys[0]);
                Thread.Sleep(20);
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
        Subscriber = new MQSubscriber(Topic, TargetIP, Port);
        Subscriber.OnDataReceived += Subscriber_OnDataReceived;
        SubStopwatch = Stopwatch.StartNew();
    }
    private static void Subscriber_OnDataReceived(byte[] data)
    {
        Stopwatch stp = Stopwatch.StartNew();
        if (data != null)
        {
            Keys = new byte[data.Length];
            data.CopyTo(Keys,0);
            SendKey((Key)Keys[0]);
            Debug.WriteLine("Key: "+(char)Keys[0]);
        }
        else
        {
            Debug.WriteLine("image data was null!");
        }
    }
    public static void StopReceiving()
    {
        Subscriber.Stop();
    }
    #endregion
    private static void SendKey(Key key)
    {
        if (Keyboard.PrimaryDevice != null)
        {
            if (Keyboard.PrimaryDevice.ActiveSource != null)
            {
                var e1 = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key) { RoutedEvent = Keyboard.KeyDownEvent };
                InputManager.Current.ProcessInput(e1);
            }
        }
    }
}
