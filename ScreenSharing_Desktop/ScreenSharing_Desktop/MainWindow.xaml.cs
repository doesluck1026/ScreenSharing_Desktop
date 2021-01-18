using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScreenSharing_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Main main;
        private Timer uiUpdateTimer;
        private int UI_UpdateFrequency = 30;        /// Hz
        private int UI_UpdatePeriod;
        private bool ui_updateEnabled = false;
        private bool IsAutoShareEnabled = false;
        private BagFile OptionsFile;
        private string URL;
        private string FileName = "Options.dat";
        private bool IsConnectedToServer = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        static string NetworkGateway()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }

            return ip;
        }
        public void Ping_all()
        {

            string gate_ip = NetworkGateway();

            //Extracting and pinging all other ip's.
            string[] array = gate_ip.Split('.');

            for (int i = 2; i <= 255; i++)
            {

                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;

                //time in milliseconds           
                Ping(ping_var, 8, 4000);

            }

        }

        public void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                new Thread(delegate ()
                {
                    try
                    {
                        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        ping.SendAsync(host, timeout, host);
                    }
                    catch
                    {
                        // Do nothing and let it try again until the attempts are exausted.
                        // Exceptions are thrown for normal ping failurs like address lookup
                        // failed.  For this reason we are supressing errors.
                    }
                }).Start();
            }
        }
        List<string> list = new List<string>();
        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string hostname = GetHostName(ip);
                string macaddres = GetMacAddress(ip);
                string[] arr = new string[3];

                //store all three parameters to be shown on ListView
                arr[0] = ip;
                arr[1] = hostname;
                arr[2] = macaddres;
                if(!list.Contains(arr[0]))
                        list.Add(arr[0]);
            }
            else
            {
                // MessageBox.Show(e.Reply.Status.ToString());
            }
        }
        public string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
                // MessageBox.Show(e.Message.ToString());
            }

            return null;
        }


        //Get MAC address
        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "OWN Machine";
            }
        }
        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            main = new Main(Main.CommunicationTypes.Sender);
            main.StartSharingScreen();
            StartUiTimer();
        }

        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            if (!IsConnectedToServer)
            {
                string ip = txt_IP.Text;
                main = new Main(Main.CommunicationTypes.Receiver);
                main.StartReceiving(ip);
                StartUiTimer();
                IsConnectedToServer = main.IsConnectedToServer;
                if(IsConnectedToServer)
                    btn_Connect.Content = "Disconnect";
            }
            else
            {
                IsConnectedToServer = false;
                btn_Connect.Content = "Connect";
                main.StopReceiving();
            }
        }
        private void UpdateUITimer_Tick(object state)
        {
            if (uiUpdateTimer == null)
                return;
            uiUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);       /// Set timer period to infinity to avoid Server timer to call this function when this is not finished.
            var watch = Stopwatch.StartNew();                               /// Stopwatch to measure total time spent in this function.
            UpdateUI();
            uiUpdateTimer.Change((int) Math.Max(0, UI_UpdatePeriod - watch.ElapsedMilliseconds), (int) (UI_UpdatePeriod));
        }
        private void UpdateUI()
        {
            Dispatcher.Invoke(() =>
            {
                if (main.IsImageReceived || main.IsImageSent)
                {
                    if (main.ScreenImage != null)
                    {
                        imageBox.Source = BitmapSourceConvert.ToBitmapSource(main.ScreenImage);
                    }
                    lbl_FPS.Content = main.FPS.ToString();
                    lbl_Speed.Content = main.TransferSpeed.ToString("0.00") + " MB/s";
                    main.IsImageReceived = false;
                    main.IsImageSent = false;
                }
                if (main.CommunitionType == Main.CommunicationTypes.Sender)
                {
                    txt_IP.Text = main.HostName;
                    if (!main.IsConnectedToClient)
                        lbl_ConnectionStatus.Background = Brushes.Red;
                    else
                        lbl_ConnectionStatus.Background = Brushes.Lime;
                }
                else if (main.CommunitionType == Main.CommunicationTypes.Receiver)
                {
                    if (!main.IsConnectedToServer)
                        lbl_ConnectionStatus.Background = Brushes.Red;
                    else
                        lbl_ConnectionStatus.Background = Brushes.Lime;
                }
            });
        }
        private void StartUiTimer()
        {
            ui_updateEnabled = true;
            uiUpdateTimer = new Timer(UpdateUITimer_Tick,null,0, UI_UpdatePeriod);
        }
        private void StopUiTimer()
        {
            ui_updateEnabled = false;
            if (uiUpdateTimer != null)
            {
                uiUpdateTimer.Change(
                    Timeout.Infinite,
                    Timeout.Infinite);
                uiUpdateTimer.Dispose();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Debug.WriteLine("item : " + i + " : " + list[i]);
            }
            CloseApp();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadOptions();
                Dispatcher.Invoke(() =>
                {
                    chc_AutoShare.IsChecked = IsAutoShareEnabled;
                });
                if(IsAutoShareEnabled)
                {
                    btn_Share_Click(null, null);
                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    chc_AutoShare.IsChecked = IsAutoShareEnabled;
                });
            }
            Ping_all();
           
        }

        private void chc_AutoShare_Click(object sender, RoutedEventArgs e)
        {
            IsAutoShareEnabled = (bool)chc_AutoShare.IsChecked;
            SaveOptions();
        }
        private void LoadOptions()
        {
            UI_UpdatePeriod = (int)(1000.0 / UI_UpdateFrequency);
            OptionsFile = new BagFile();
            URL = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/JuniorVersusBug/ScreenSharingApp/";
            FileStream readerFileStream = new FileStream(URL+FileName, FileMode.Open, FileAccess.Read);
            // Reconstruct data
            BinaryFormatter formatter = new BinaryFormatter();
            OptionsFile = (BagFile)formatter.Deserialize(readerFileStream);
            readerFileStream.Close();
            IsAutoShareEnabled = OptionsFile.IsAutoShareEnabled;
        }
        private void SaveOptions()
        {
            try
            {
                OptionsFile.IsAutoShareEnabled = IsAutoShareEnabled;
                BagFile bagFile = OptionsFile;
                FileStream writerFileStream = new FileStream(URL + FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(writerFileStream, OptionsFile);
                writerFileStream.Close();
            }
            catch(Exception e)
            {
            }
        }
        private void CloseApp()
        {
            try
            {
                StopMainThreads();
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            catch
            {

            }
        }

        private void txt_IP_DropDownOpened(object sender, EventArgs e)
        {
            if (main == null)
            {
                main = new Main(Main.CommunicationTypes.Receiver);
                if (main.Comm.RecentServers.RecentServersList != null)
                {
                    for (int i = 0; i < main.Comm.RecentServers.RecentServersList.Count; i++)
                        txt_IP.Items.Add(main.Comm.RecentServers.RecentServersList[i]);
                }
            }
        }
        private void StopMainThreads()
        {
            StopUiTimer();
            if (main != null)
            {
                main.CancelSharing();
                main.StopReceiving();
            }
        }
        private void Reset()
        {
            StopMainThreads();
        }

    }
}
