using NetMQ;
using NetMQ.Sockets;


class MQSubscriber
{
    private string Topic;
    private string IP;
    private int Port;
    private SubscriberSocket Subscriber;
    private NetMQPoller Poller;

    public delegate void DataEvent(byte[] data);
    public event DataEvent OnDataReceived;
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
    public void Stop()
    {
        Poller.StopAsync();
        Subscriber.Unsubscribe(Topic);
        Subscriber.Disconnect("tcp://" + IP + ":" + Port.ToString());
        Subscriber.Dispose();
        Poller.Dispose();
    }
    private void Subscriber_ReceiveReady(object sender, NetMQSocketEventArgs e)
    {
        var topic = Subscriber.ReceiveFrameString();
        var msg = Subscriber.ReceiveFrameBytes();
        OnDataReceived(msg);
    }
}

