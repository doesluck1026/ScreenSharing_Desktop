using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;


class Communication
{
    private Server server;
    private Client client;
    private readonly byte StartByte = (byte)'J';
    private int Port = 42055;
    private int BufferSize = 1024 * 64;

    public bool isClientConnected
    {
        get
        {
            lock (Lck_isClientConnected)
                return _isClientConnected;
        }
        set
        {
            lock (Lck_isClientConnected)
                _isClientConnected = value;
        }
    }
    public bool isConnectedToServer
    {
        get
        {
            lock (Lck_isConnectedToServer)
                return _isConnectedToServer;
        }
        set
        {
            lock (Lck_isConnectedToServer)
                _isConnectedToServer = value;
        }
    }
    public long LastPackNumberReceived { get; private set; }
    public long LastPackNumberSent { get; private set; }
    public uint NumberOfPacks { get; private set; }

    public StringListBagFile RecentServers;
    private string URL = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/JuniorVersusBug/ScreenSharingApp/";
    private string FileName =  "ClientsList.dat";

    private bool _isClientConnected = false;
    private bool _isConnectedToServer = false;
    private object Lck_isClientConnected = new object();
    private object Lck_isConnectedToServer = new object();
    private enum Functions
    {
        SendingFile = 1,
    }

    public Communication()
    {
        LastPackNumberSent = 0;
        LastPackNumberReceived = -1;
        client = new Client();
        LoadRecentClientsList();
    }
    #region Server Functions
    /// <summary>
    /// Creates a server and starts listening to port. This is used to send file to another device.
    /// </summary>
    /// <returns></returns>
    public string CreateServer()
    {
        server = new Server(port:Port,bufferSize: BufferSize,StartByte:StartByte);                      /// Create server instance
        string serverIP = server.SetupServer();           /// Setup Server on default port. this Function will return device ip as string.
        return serverIP;
    }
    public void StartServer()
    {
        server.StartListener();      /// Start Listener for possible clients.
        isClientConnected = true;
    }
    public void CheckConnection()
    {
        isClientConnected = server.IsCLientConnected;
    }

    /// <summary>
    /// Sends File packs to the client and gets the acknowledge
    /// </summary>
    /// <param name="data">File Pack bytes (Max 32 Kb)</param>
    /// <param name="numPackage">Index of the data pack to be sent</param>
    /// <returns>Returns Acknowledge</returns>
    public void SendFilePacks(byte[] data)
    {
        byte[] HeaderBytes = PrepareDataHeader(Functions.SendingFile);      /// Prepare Data Header for given length. +4 is for to specify current package index.
        int HeaderLen = HeaderBytes.Length;
        byte[] DataToSend = new byte[data.Length + HeaderLen];                                  /// Create carrier data pack
        Array.Copy(HeaderBytes, 0, DataToSend, 0, HeaderLen);                                       /// Copy Header bytes to the carrier
        Array.Copy(data, 0, DataToSend, HeaderLen, data.Length);                                /// Copy given data bytes to carrier pack
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
    /// <param name="IP"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public bool ConnectToServer(string IP)
    {
        client = new Client(port: Port, ip:IP, bufferSize:BufferSize,StartByte:StartByte);
         string hostname=client.ConnectToServer();
        isConnectedToServer = client.IsConnectedToServer;
        if (hostname != null)
        {
            if (RecentServers != null)
            {
                if (RecentServers.RecentServersList.Contains(hostname) == false)
                {
                    RecentServers.RecentServersList.Add(hostname);
                    SaveRecentClientsList();
                }
            }
        }
        return isConnectedToServer;
    }

    /// <summary>
    /// Gets data from buffer and checks. returns file bytes only. you can directly write them to fileStream
    /// </summary>
    /// <returns>File Bytes as Byte array</returns>
    public byte[] ReceiveFilePacks()
    {
        byte[] receivedData = client.GetData();                                     /// Get Data From Buffer
        if (receivedData == null)
        {
            Debug.WriteLine("ReceiveFilePacks: Received data is null!");
            return null;
        }
        if (receivedData[0] == (byte)Functions.SendingFile)                        /// Check the function byte
        {
            byte[] dataPack = new byte[receivedData.Length - 1];                              /// Create data pack variable to store file bytes 
            Array.Copy(receivedData,1, dataPack, 0,dataPack.Length);      /// Copy array to data packs byte
            return dataPack;
            /// return data pack
        }
        else
        {
            Debug.WriteLine("ReceiveFilePacks function: Function Byte is wrong!: " + (Functions)receivedData[1]);
            return null;
        }
    }

    public void SendResponseToServer(byte[] data)
    {
        int len = data.Length;
        byte[] headerBytes = PrepareDataHeader(Functions.SendingFile);
        byte[] dataToSend = new byte[len + headerBytes.Length];
        headerBytes.CopyTo(dataToSend, 0);
        data.CopyTo(dataToSend, headerBytes.Length);
        if (client == null)
            return;
        client.SendDataServer(dataToSend);
    }
    public void CloseClient()
    {
        if (client == null)
            return;
        isConnectedToServer = !client.DisconnectFromServer();
        client = null;
    }
    private void LoadRecentClientsList()
    {
        try
        {

            FileStream readerFileStream = new FileStream(URL + FileName, FileMode.Open, FileAccess.Read);
            // Reconstruct data
            BinaryFormatter formatter = new BinaryFormatter();
            RecentServers = (StringListBagFile)formatter.Deserialize(readerFileStream);
            readerFileStream.Close();
            if(RecentServers.RecentServersList==null)
            {
                RecentServers.RecentServersList = new List<string>();
                SaveRecentClientsList();
            }
        }
        catch
        {
            RecentServers = new StringListBagFile();
            SaveRecentClientsList();
        }
    }
    private void SaveRecentClientsList()
    {
        var t = Task.Run(() =>
         {
             FileStream writerFileStream = new FileStream(URL + FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
             BinaryFormatter formatter = new BinaryFormatter();
             formatter.Serialize(writerFileStream, RecentServers);
             writerFileStream.Close();
         });
    }
    #endregion

    #region Common Functions
    private byte[] PrepareDataHeader(Functions func)
    {
        byte[] HeaderBytes = new byte[1];
        HeaderBytes[0] = (byte)func;
        return HeaderBytes;
    }
    private int CalculatePackageCount(double fileSize)
    {
        int packageCount = (int)Math.Ceiling(fileSize / (double)Main.PackSize);
        return packageCount;
    }
    public byte WriteToBit(byte value, byte index, bool data)
    {
        if (data)
            value = (byte)(value | (0x01 << index));
        else
            value = (byte)(value & ~(0x01 << index));
        return value;
    }
    public bool ReadBit(byte data, int index)
    {
        return ((data >> index) & 0x01) == 0x01;
    }
    #endregion
}
