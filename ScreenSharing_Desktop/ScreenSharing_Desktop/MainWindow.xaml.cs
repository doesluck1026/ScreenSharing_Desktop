using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Squirrel;

namespace ScreenSharing_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int MenuTimeout = 10;


        private Timer uiUpdateTimer;
        private int UI_UpdateFrequency = 40;        /// Hz
        private int UI_UpdatePeriod;
        private bool ui_updateEnabled = false;
        private bool IsConnectedToServer = false;
        private bool IsSharingStarted = false;
        private int SelectedIndex = -1;
        private bool IsControlsEnabled;
        private int MenuTimeoutCounter = 0;
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
                NetworkScanner.ScanAvailableDevices();
                Dispatcher.Invoke(() =>
                {
                    chc_AutoShare.IsChecked = Parameters.IsAutoShareEnabled;
                    chc_EnableControls.IsChecked = Parameters.IsControlsEnabled;
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
                                    var ipBlocks = localIP.ToString().Split(splitter);
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
                Debug.WriteLine("Failed when initializing ");
            }
        }
        /// <summary>
        /// Adds Current version number and app name at the top of main window 
        /// </summary>
        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Dispatcher.Invoke(() =>
            {
                this.Title += $" v.{versionInfo.FileVersion}";
            });
        }

        /// <summary>
        /// Checks for updates everytime this app is started.
        /// This method uses Squirrel nuget. Go to https://youtu.be/N_T_UOkr7ts to see my tutorial about this.
        /// </summary>
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
            if (!IsSharingStarted)
            {
                IsSharingStarted = true;
                Main.StartSharing();
                StartUiTimer();
                imageBox.Focus();
                btn_Share.Content = "Stop";
                txt_IP.Text = Main.MyIP;
            }
            else
            {
                btn_Share.Content = "Share";
                StopUiTimer();
                IsSharingStarted = false;
                Main.StopSharing();
                RemoteControl.StopReceiving();
            }
        }

        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            if (!IsConnectedToServer)
            {
                string ip = txt_IP.Text;
                if(SelectedIndex!=-1)
                    Main.StartReceiving(NetworkScanner.PublisherDevices[SelectedIndex].IP);
                else
                    Main.StartReceiving(txt_IP.Text);
                StartUiTimer();
                IsConnectedToServer = true;
                if(IsConnectedToServer)
                    btn_Connect.Content = "Disconnect";

                txt_IP.Focusable = false;
                txt_IP.IsEnabled = false;
            }
            else
            {
                IsConnectedToServer = false;
                btn_Connect.Content = "Connect";
                Main.StopReceiving();
                NetworkScanner.ScanAvailableDevices();
                txt_IP.Focusable = true;
                txt_IP.IsEnabled = true;
                SelectedIndex = -1;
            }
            imageBox.Focus();
        }

        private void UpdateUITimer_Tick(object state)
        {
            if (uiUpdateTimer == null)
                return;
            uiUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);       /// Set timer period to infinity to avoid Server timer to call this function when this is not finished.
            var watch = Stopwatch.StartNew();                               /// Stopwatch to measure total time spent in this function.
            UpdateUI();
            if (uiUpdateTimer == null)
                return;
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
                lbl_Ping.Content = ((int)Main.Ping).ToString()+" ms";
                lbl_Speed.Content = Main.TransferSpeed.ToString("0.00") + " MB/s";
                IsControlsEnabled = chc_EnableControls.IsChecked.Value;
                if (IsControlsEnabled)
                {
                    if(Main.CommunicationType==Main.CommunicationTypes.Receiver)
                    {
                        if (!RemoteControl.IsPublisherEnabled)
                            RemoteControl.StartSendingCommands();
                    }
                }
                if(stc_ControlBar.IsVisible && IsConnectedToServer)
                {
                    if (MenuTimeoutCounter < 100000)
                        MenuTimeoutCounter++;
                    if (MenuTimeoutCounter > MenuTimeout * UI_UpdateFrequency)
                    {
                        stc_ControlBar.Visibility = Visibility.Hidden;
                        MenuTimeoutCounter = 0;
                    }
                }
               

            });

            if (Main.CommunicationType == Main.CommunicationTypes.Sender)
            {
                RemoteControl.IsControlsEnabled = IsControlsEnabled;
                if (!RemoteControl.IsSubscriberEnabled)
                {

                    if (NetworkScanner.SubscriberDevices.Count > 0)
                    {
                        RemoteControl.StartReceiving(NetworkScanner.SubscriberDevices[0].IP);
                    }
                }
            }
            
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
            if (NetworkScanner.PublisherDevices != null)
            {
                for (int i = 0; i < NetworkScanner.PublisherDevices.Count; i++)
                    txt_IP.Items.Add(NetworkScanner.PublisherDevices[i].Hostname);
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

        private void txt_IP_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedIndex = txt_IP.SelectedIndex;
        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(IsControlsEnabled)
            {
                RemoteControl.Keys[(byte)e.Key] = e.IsDown ? (byte)1 : (byte)0;
                RemoteControl.IsDataUpdated = true;
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsControlsEnabled)
            {
                RemoteControl.Keys[(byte)e.Key] = e.IsDown ? (byte)1 : (byte)0;
                RemoteControl.IsDataUpdated = true;
            }
        }
        private void chc_EnableControls_Click(object sender, RoutedEventArgs e)
        {
            IsControlsEnabled = chc_EnableControls.IsChecked.Value;
            RemoteControl.IsControlsEnabled = IsControlsEnabled;
            Parameters.IsControlsEnabled = IsControlsEnabled;
            Parameters.Save();
        }

        private void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsControlsEnabled)
            {
                var pos = e.GetPosition(imageBox);
                double widthRatio = (double)Main.ScreenImage.Width / imageBox.ActualWidth;
                double heigthRatio = (double)Main.ScreenImage.Height / imageBox.ActualHeight;
                RemoteControl.VirtualMouse.Position = new System.Drawing.Point((int)(pos.X * widthRatio), (int)(pos.Y * heigthRatio));
                RemoteControl.IsDataUpdated = true;
            }
        }
        private void imageBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsControlsEnabled)
            {
                if (e.ChangedButton == MouseButton.Left)
                    RemoteControl.VirtualMouse.LeftButton = e.ButtonState;
                if (e.ChangedButton == MouseButton.Right)
                    RemoteControl.VirtualMouse.RightButton = e.ButtonState;
                if (e.ChangedButton == MouseButton.Middle)
                    RemoteControl.VirtualMouse.MiddleButton = e.ButtonState;
                RemoteControl.IsDataUpdated = true;
            }
        }

        private void imageBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsControlsEnabled)
            {
                if (e.ChangedButton == MouseButton.Left)
                    RemoteControl.VirtualMouse.LeftButton = e.ButtonState;
                if (e.ChangedButton == MouseButton.Right)
                    RemoteControl.VirtualMouse.RightButton = e.ButtonState;
                if (e.ChangedButton == MouseButton.Middle)
                    RemoteControl.VirtualMouse.MiddleButton = e.ButtonState;
                RemoteControl.IsDataUpdated = true;
            }
        }

        private void imageBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsControlsEnabled)
            {
                RemoteControl.VirtualMouse.ScrollDelta = e.Delta;
                RemoteControl.IsDataUpdated = true;
            }
        }
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsControlsEnabled)
            {
                RemoteControl.VirtualMouse.DoubleClick = true;
                RemoteControl.IsDataUpdated = true;
            }
        }
        private void imageBox_MouseLeave(object sender, MouseEventArgs e)
        {
            stc_ControlBar.Visibility = Visibility.Visible;
        }

        private void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            NetworkScanner.ScanAvailableDevices();
        }
    }
}
