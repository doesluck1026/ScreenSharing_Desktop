using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Communication
{
    private Server server;
    private Client client;
    private readonly int HeaderLen = 7;
    private readonly byte StartByte = (byte)'J';
    private int Port = 42001;
    private int BufferSize = 1024 * 64;

    public bool isClientConnected = false;
    public bool isConnectedToServer = false;
    public long LastPackNumberReceived { get; private set; }
    public long LastPackNumberSent { get; private set; }
    public uint NumberOfPacks { get; private set; }
    private enum Functions
    {
        SendingFile = 1,
    }

    public Communication()
    {
        LastPackNumberSent = 0;
        LastPackNumberReceived = -1;
        client = new Client();
    }
    #region Server Functions
    /// <summary>
    /// Creates a server and starts listening to port. This is used to send file to another device.
    /// </summary>
    /// <returns></returns>
    public string CreateServer()
    {
        server = new Server(Port, BufferSize);                      /// Create server instance
        string serverIP = server.SetupServer();           /// Setup Server on default port. this Function will return device ip as string.
        return serverIP;
    }
    public string StartServer()
    {
        string hostname = server.StartListener();      /// Start Listener for possible clients.
        if (hostname != null)
        {
            isClientConnected = true;
            return hostname;                       /// return connection status
        }
        else
            return null;
    }

    /// <summary>
    /// Sends File packs to the client and gets the acknowledge
    /// </summary>
    /// <param name="data">File Pack bytes (Max 32 Kb)</param>
    /// <param name="numPackage">Index of the data pack to be sent</param>
    /// <returns>Returns Acknowledge</returns>
    public void SendFilePacks(byte[] data, long numPackage)
    {
        byte[] HeaderBytes = PrepareDataHeader(Functions.SendingFile, (uint)(data.Length + 4));     /// Prepare Data Header for given length. +4 is for to specify current package index.
        byte[] DataToSend = new byte[data.Length + 4 + HeaderLen];                                  /// Create carrier data pack
        Array.Copy(HeaderBytes, 0, DataToSend, 0, HeaderLen);                                       /// Copy Header bytes to the carrier
        Array.Copy(BitConverter.GetBytes(numPackage), 0, DataToSend, HeaderLen, sizeof(int));       /// Copy Index bytes to carrier pack
        Array.Copy(data, 0, DataToSend, HeaderLen + 4, data.Length);                                /// Copy given data bytes to carrier pack
        isClientConnected = server.SendDataToClient(DataToSend);                                    /// Send data to client.
    }
    public void CloseServer()
    {
        if (server == null)
            return;
        server.CloseServer();
        server = null;
    }
    public byte[] GetResponseFromClient()
    {
        byte[] receivedData = server.GetData();
        return receivedData;
    }
    #endregion

    #region Client Functions
    
    
    /// <summary>
    /// Connects to server with given ip at given port. This is used to receive file from another device.
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public  bool ConnectToServer(string ip)
    {
        client = new Client(Port, BufferSize);
        isConnectedToServer = client.ConnectToServer(ip);
        return isConnectedToServer;
    }
   
    /// <summary>
    /// Gets data from buffer and checks. returns file bytes only. you can directly write them to fileStream
    /// </summary>
    /// <returns>File Bytes as Byte array</returns>
    public  byte[] ReceiveFilePacks()
    {
        byte[] receivedData = client.GetData();                                     /// Get Data From Buffer
        if (receivedData == null)
        {
            Debug.WriteLine("ReceiveFilePacks: Received data is null!");
            return null;
        }
        if (receivedData[0] == StartByte)                                              /// check if start byte is correct
        {
            if (receivedData[1] == (byte)Functions.SendingFile)                        /// Check the function byte
            {
                uint dataLen = BitConverter.ToUInt32(receivedData, 3);              /// Get the length of the data bytes (index bytes are included to this number)
                uint packIndex = BitConverter.ToUInt32(receivedData, HeaderLen);    /// Get the index of data pack
                byte[] dataPack = new byte[dataLen - 4];                              /// Create data pack variable to store file bytes 
                Array.Copy(receivedData, HeaderLen + 4, dataPack, 0, dataLen - 4);      /// Copy array to data packs byte
                return dataPack;
                /// return data pack
            }
            else
            {
                Debug.WriteLine("ReceiveFilePacks function: Function Byte is wrong!: " + (Functions)receivedData[1]);
                return null;
            }
        }
        else
        {
            Debug.WriteLine("ReceiveFilePacks function: Start Byte is wrong!: " + receivedData[0]);
            return null;
        }
    }
    
    public void SendResponseToServer()
    {
        uint len = 1;
        byte[] dataToSend = new byte[len + HeaderLen];
        byte[] headerBytes=PrepareDataHeader(Functions.SendingFile, len);
        headerBytes.CopyTo(dataToSend, 0);
        dataToSend[HeaderLen] = 100;
        client.SendDataServer(dataToSend);
    }
    public  void CloseClient()
    {
        if (client == null)
            return;
        isConnectedToServer= !client.DisconnectFromServer();
        client = null;
    }

    #endregion

    #region Common Functions
    private  byte[] PrepareDataHeader(Functions func, uint len)
    {
        byte[] HeaderBytes = new byte[HeaderLen];
        HeaderBytes[0] = StartByte;
        HeaderBytes[1] = Convert.ToByte(func);
        byte[] lenBytes = BitConverter.GetBytes(len);
        Array.Copy(lenBytes, 0, HeaderBytes, 3, lenBytes.Length);
        return HeaderBytes;
    }
    private  uint CalculatePackageCount(double fileSize)
    {
        uint packageCount = (uint)Math.Ceiling(fileSize / (double)Main.PackSize);
        return packageCount;
    }
    #endregion
}
