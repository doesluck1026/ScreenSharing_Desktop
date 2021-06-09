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
    #region Parameters
    public static int Port = 4113;
    private static string Topic = "Command";
    private static MQPublisher Publisher;
    private static MQSubscriber Subscriber;
    private static int NumKeys = 173;
    private static int LenMouseData = 20;

    #endregion
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
    public static MouseHandleTypeDef PrevVirtualMouse;
    private static bool[] MouseButtonStates = new bool[3];

    private static Thread Thread_Control;

    private static byte[] ReceivedData;
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
    }
    public static void StopSendingCommands()
    {
        try
        {
            IsPublisherEnabled = false;
        }
        catch
        {
            Debug.WriteLine("Failed to Stop Publisher");
        }
    }
    public static void PublishCommands()
    {
        if (IsPublisherEnabled)
        {
            byte[] data = new byte[LenMouseData + NumKeys];
            Array.Copy(Keys, 0, data, 0, NumKeys);
            byte[] mouseData = PrepareMouseData();
            Array.Copy(mouseData, 0, data, NumKeys, LenMouseData);
            Publisher.Publish(data);
        }
    }
    public static void SetMousePosition(System.Drawing.Point mousePos)
    {
        VirtualMouse.Position.X = Math.Max(VirtualMouse.Position.X + (mousePos.X - PrevVirtualMouse.Position.X), 0);
        VirtualMouse.Position.Y = Math.Max(VirtualMouse.Position.Y + (mousePos.Y - PrevVirtualMouse.Position.Y), 0);
        PrevVirtualMouse.Position = mousePos;
    }
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
    private static void Control_CoreFcn()
    {
        while(IsSubscriberEnabled)
        {
            if(IsDataReceived)
            {
                Keys = new byte[NumKeys];
                byte[] mouseData = new byte[LenMouseData];
                IsDataReceived = false;
                Array.Copy(ReceivedData, 0, Keys, 0, Keys.Length);
                Array.Copy(ReceivedData, NumKeys, mouseData, 0, LenMouseData);
                HandleKeyBoard(Keys);
                HandleMouse(mouseData);
            }
            Thread.Sleep(5);
        }
    }
    private static void Subscriber_OnDataReceived(byte[] data)
    {
        if (data != null)
        {
            ReceivedData = new byte[data.Length];
            data.CopyTo(ReceivedData,0);
            IsDataReceived = true;
        }
    }
    public static void StopReceiving()
    {
        IsSubscriberEnabled = false;
        Subscriber.Stop();
    }

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
                {
                    if (Keys_States[i])
                    {
                        Keyboard.Release(key);
                    }
                }
            }
            else
            {
                if (Keys[i] == 1)
                {
                    Keyboard.Type(key);
                }
            }
        }
    }
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
