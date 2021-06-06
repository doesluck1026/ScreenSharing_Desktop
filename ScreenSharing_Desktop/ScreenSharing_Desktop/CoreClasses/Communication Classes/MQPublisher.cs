using NetMQ;
using NetMQ.Sockets;


class MQPublisher
{
    private string Topic;
    private string IP;
    private int Port;
    private PublisherSocket Publisher;
    public MQPublisher(string topic, string ip, int port)
    {
        this.Topic = topic;
        this.IP = ip;
        this.Port = port;
        Publisher = new PublisherSocket();
        Publisher.Bind("tcp://" + IP + ":" + Port.ToString());
        Publisher.Options.SendHighWatermark = 1;
    }

    public void Publish(byte[] data)
    {
        Publisher.SendMoreFrame(Topic).SendFrame(data);
    }
    public void Stop()
    {
        Publisher.Unbind("tcp://" + IP + ":" + Port.ToString());
        Publisher.Close();
        Publisher.Dispose();
    }
}

