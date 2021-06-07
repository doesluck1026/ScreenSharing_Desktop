using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
    private static Key[] PrevKeyStates = new Key[256];
    private static Thread Thread_KeyBoard;
    private static bool isDataReceived = false;

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
        Thread_KeyBoard = new Thread(Keyboard_CoreFcn);
        Thread_KeyBoard.Start();
    }
    private static void Subscriber_OnDataReceived(byte[] data)
    {
        Stopwatch stp = Stopwatch.StartNew();
        if (data != null)
        {
            Keys = new byte[data.Length];
            data.CopyTo(Keys,0);
            isDataReceived = true;
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
    private static void Keyboard_CoreFcn()
    {
        Stopwatch stp = new Stopwatch();
        while (true)
        {
            if (isDataReceived)
            {
                isDataReceived = false;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    for (int i = 0; i < Keys.Length; i++)
                    {

                        if (PrevKeyStates[i] != (Key)Keys[i])
                        {
                            if (Keys[i] == 1)
                                Keyboard.Press((Key)i);
                            else if (Keys[i] == 0)
                                Keyboard.Release((Key)i);
                            PrevKeyStates[i] = (Key)Keys[i];
                        }
                        else
                        {
                            Keyboard.Press((Key)i);
                        }
                    }
                });
                stp.Restart();
            }
            else
            {
                if(stp.ElapsedMilliseconds>2000)
                {
                    for(int i=0;i<Keys.Length;i++)
                    {
                        Keys[i] = 2;
                    }
                }
            }
            Thread.Sleep(10);
        }
    }
}
