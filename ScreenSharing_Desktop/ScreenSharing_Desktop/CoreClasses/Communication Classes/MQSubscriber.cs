using NetMQ;
using NetMQ.Sockets;

/// <summary>
/// This Class is used to subscribe a topic which is being published by using zero MQ.
/// You only need to provide ip, port and topic in order to start easy transfer.
/// Every time a data package is received,  OnDataReceived Event will be thrown. Just try to catch it....
/// For a better efficiency, Do NOT do any complicated task inside of this event or you will have a serious lag in your communication.
/// Trust me, the best way is to just get received byte array to a local array and let the event go. It will come back again if you miss it
/// 
/// And don't forget to pray the Lords of Communication for giving this beautiful protocol to us...
/// </summary>
class MQSubscriber
{
    /// <summary>
    ///  Topic name to be published with data.
    /// </summary>
    private string Topic;

    /// <summary>
    /// Publisher's IP Address.
    /// </summary>
    private string IP;

    /// <summary>
    /// The Port where data is being published.
    /// </summary>
    private int Port;

    /// <summary>
    /// MQ Subscriber Object
    /// </summary>
    private SubscriberSocket Subscriber;

    /// <summary>
    /// Poller Objects where the magic happens. This Object provides an event when data is received.
    /// </summary>
    private NetMQPoller Poller;

    /// <summary>
    /// Delegate to create an event to the classes which created an instance of this class
    /// </summary>
    /// <param name="data">This byte array will contain the received data from publisher</param>
    public delegate void DataEvent(byte[] data);

    /// <summary>
    /// This event will be thrown when a data with specified topic is received.
    /// </summary>
    public event DataEvent OnDataReceived;

    /// <summary>
    /// Initializes an MQ Subscriber Object.
    /// </summary>
    /// <param name="topic">Topic Name of communication</param>
    /// <param name="ip">Publisher's IP Address</param>
    /// <param name="port">The port that data is published</param>
    public MQSubscriber(string topic, string ip, int port)
    {

        this.Topic = topic;
        this.IP = ip;
        this.Port = port;
        Subscriber = new SubscriberSocket();
        Poller = new NetMQPoller() { Subscriber };
        Subscriber.Connect("tcp://" + IP + ":" + Port.ToString());
        Subscriber.Subscribe(Topic);
        Subscriber.ReceiveReady += Subscriber_ReceiveReady;
        Poller.RunAsync();
    }

    /// <summary>
    /// Cancels Subscription to the topic and closes connections.
    /// </summary>
    public void Stop()
    {
        Subscriber.ReceiveReady -= Subscriber_ReceiveReady;
        Poller.StopAsync();
        Subscriber.Unsubscribe(Topic);
        Subscriber.Disconnect("tcp://" + IP + ":" + Port.ToString());
        Poller.Dispose();
    }

    /// <summary>
    /// Function that is called by Poller Object when a data is received.
    /// Data Contains both topic and user data in the order .
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Subscriber_ReceiveReady(object sender, NetMQSocketEventArgs e)
    {
        var topic = Subscriber.ReceiveFrameString();
        var msg = Subscriber.ReceiveFrameBytes();
        if (OnDataReceived != null)
        {
            OnDataReceived(msg);
        }
    }
}

