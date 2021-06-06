using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Squirrel;

namespace ScreenSharing_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer uiUpdateTimer;
        private int UI_UpdateFrequency = 40;        /// Hz
        private int UI_UpdatePeriod;
        private bool ui_updateEnabled = false;
        private bool IsConnectedToServer = false;
        private int SelectedIndex = -1;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AddVersionNumber();
                CheckForUpdates();
                LoadOptions();
                NetworkScanner.PublishDevice();
                //NetworkScanner.ScanAvailableDevices();
                Dispatcher.Invoke(() =>
                {
                    chc_AutoShare.IsChecked = Parameters.IsAutoShareEnabled;
                });
                if (Parameters.IsAutoShareEnabled)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            bool noIP = true;
                            while (noIP)
                            {
                                var localIP = Client.GetDeviceIP();
                                if (localIP != null)
                                {
                                    char[] splitter = { '.' };
                                    var ipBlocks=localIP.ToString().Split(splitter);
                                    if (string.Equals(ipBlocks[0], "192") && string.Equals(ipBlocks[1], "168"))
                                    {
                                        noIP = false;
                                    }
                                }
                                Thread.Sleep(500);
                            }
                            btn_Share_Click(null, null);
                        }
                        catch
                        {

                        }
                    });

                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    chc_AutoShare.IsChecked = Parameters.IsAutoShareEnabled;
                });
            }
        }
        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Dispatcher.Invoke(() =>
            {
                this.Title += $" v.{versionInfo.FileVersion}";
            });
        }
        private async void CheckForUpdates()
        {
            try
            {
                using (var mgr= await UpdateManager.GitHubUpdateManager("https://github.com/doesluck1026/ScreenSharing_Desktop"))
                {
                    var release = await mgr.UpdateApp();
                }
            }
            catch ( Exception e)
            {
                Debug.WriteLine("Failed to check Updates: " + e.ToString());
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseApp();
        }
        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            Main.StartSharing();
            StartUiTimer();
        }

        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            if (!IsConnectedToServer)
            {
                string ip = txt_IP.Text;
                //Main.StartReceiving(NetworkScanner.Devices[SelectedIndex].IP);
                Main.StartReceiving(txt_IP.Text);
                StartUiTimer();
                IsConnectedToServer = true;
                if(IsConnectedToServer)
                    btn_Connect.Content = "Disconnect";
            }
            else
            {
                IsConnectedToServer = false;
                btn_Connect.Content = "Connect";
                Main.StopReceiving();
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
                    if (Main.ScreenImage != null)
                    {
                        imageBox.Source = BitmapSourceConvert.ToBitmapSource(Main.ScreenImage);
                    }
                    lbl_FPS.Content = Main.FPS.ToString();
                    lbl_Speed.Content = Main.TransferSpeed.ToString("0.00") + " MB/s";
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
                uiUpdateTimer = null;
            }
        }
       
        private void chc_AutoShare_Click(object sender, RoutedEventArgs e)
        {
            Parameters.IsAutoShareEnabled = (bool)chc_AutoShare.IsChecked;
            Parameters.Save();
        }
        private void LoadOptions()
        {
            UI_UpdatePeriod = (int)(1000.0 / UI_UpdateFrequency);
            Parameters.Init();
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
            txt_IP.Items.Clear();
            if (NetworkScanner.Devices != null)
            {
                for (int i = 0; i < NetworkScanner.Devices.Count; i++)
                    txt_IP.Items.Add(NetworkScanner.Devices[i].Hostname);
            }
        }
        private void StopMainThreads()
        {
            StopUiTimer();
            NetworkScanner.StopPublishing();
        }
        private void Reset()
        {
            StopMainThreads();
        }

        private void txt_IP_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedIndex = txt_IP.SelectedIndex;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
           
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }
}
