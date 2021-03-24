using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
        private int UI_UpdateFrequency = 30;        /// Hz
        private int UI_UpdatePeriod;
        private bool ui_updateEnabled = false;
        private bool IsConnectedToServer = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckForUpdates();
                AddVersionNumber();
                LoadOptions();
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
                            var localIP = Server.GetDeviceIP();
                            if(localIP!=null)
                            {
                                if(!string.Equals(localIP.ToString(), "127.0.0.0") && !string.Equals(localIP.ToString(), "127.0.0.1"))
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
        private async void CheckForUpdates()
        {
            try
            {
                using(var mgr=await UpdateManager.GitHubUpdateManager("https://github.com/doesluck1026/ScreenSharing_Desktop"))
                {
                    var release = await mgr.UpdateApp();
                }
            }
            catch
            {

            }
        }
        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Dispatcher.Invoke(() =>
            {
                this.Title += "v." + versionInfo.FileVersion;
            });
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseApp();
        }
        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            Main.InitCommunication(Main.CommunicationTypes.Sender);
            Main.StartSharingScreen();
            StartUiTimer();
        }

        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            if (!IsConnectedToServer)
            {
                string ip = txt_IP.Text;
                Main.InitCommunication(Main.CommunicationTypes.Receiver);
                Main.StartReceiving(ip);
                StartUiTimer();
                IsConnectedToServer = Main.IsConnectedToServer;
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
                if (Main.IsImageReceived || Main.IsImageSent)
                {
                    if (Main.ScreenImage != null)
                    {
                        imageBox.Source = BitmapSourceConvert.ToBitmapSource(Main.ScreenImage);
                    }
                    lbl_FPS.Content = Main.FPS.ToString();
                    lbl_Speed.Content = Main.TransferSpeed.ToString("0.00") + " MB/s";
                    Main.IsImageReceived = false;
                    Main.IsImageSent = false;
                }
                if (Main.CommunitionType == Main.CommunicationTypes.Sender)
                {
                    txt_IP.Text = Main.HostName;
                    if (!Main.IsConnectedToClient)
                        lbl_ConnectionStatus.Background = Brushes.Red;
                    else
                        lbl_ConnectionStatus.Background = Brushes.Lime;
                }
                else if (Main.CommunitionType == Main.CommunicationTypes.Receiver)
                {
                    if (!Main.IsConnectedToServer)
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
            if (Parameters.RecentServersList != null)
            {
                for (int i = 0; i < Parameters.RecentServersList.Count; i++)
                    txt_IP.Items.Add(Parameters.RecentServersList[i]);
            }
        }
        private void StopMainThreads()
        {
            StopUiTimer();
        }
        private void Reset()
        {
            StopMainThreads();
        }

    }
}
