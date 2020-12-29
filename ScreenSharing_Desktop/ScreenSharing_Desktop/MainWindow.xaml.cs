using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace ScreenSharing_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Main main;
        private Thread uiUpdateThread;
        private int UI_UpdateFrequency = 30;        /// Hz
        private double UI_UpdatePeriod;
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

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            main = new Main(Main.CommunicationTypes.Sender);
            main.StartSharingScreen();
            StartUiThread();
        }

        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnectedToServer)
            {
                string ip = txt_IP.Text;
                main = new Main(Main.CommunicationTypes.Receiver);
                main.StartReceiving(ip);
                StartUiThread();
                IsConnectedToServer = true;
                btn_Connect.Content = "Disconnect";
            }
            else
            {
                IsConnectedToServer = false;
                btn_Connect.Content = "Connect";
                main.StopReceiving();
            }
        }
        private void UpdateUI()
        {
            Stopwatch stp = Stopwatch.StartNew();
            while (ui_updateEnabled)
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
                        if(!main.IsConnectedToClient)
                            lbl_ConnectionStatus.Background = Brushes.Red;
                        else
                            lbl_ConnectionStatus.Background = Brushes.Lime;
                    }
                    else if(main.CommunitionType == Main.CommunicationTypes.Receiver)
                    {
                        if (!main.IsConnectedToServer)
                            lbl_ConnectionStatus.Background = Brushes.Red;
                        else
                            lbl_ConnectionStatus.Background = Brushes.Lime;
                    }
                });
                while (stp.Elapsed.TotalSeconds <= UI_UpdatePeriod) ;
                stp.Restart();
            }
        }
        private void StartUiThread()
        {
            ui_updateEnabled = true;
            uiUpdateThread = new Thread(UpdateUI);
            uiUpdateThread.Start();
        }
        private void StopUiThread()
        {
            ui_updateEnabled = false;
            if (uiUpdateThread != null)
            {
                if (uiUpdateThread.IsAlive)
                {
                    try
                    {
                        uiUpdateThread.Abort();
                        uiUpdateThread = null;
                    }
                    catch
                    {

                    }
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
        }

        private void chc_AutoShare_Click(object sender, RoutedEventArgs e)
        {
            IsAutoShareEnabled = (bool)chc_AutoShare.IsChecked;
            SaveOptions();
        }
        private void LoadOptions()
        {
            UI_UpdatePeriod = 1.0 / UI_UpdateFrequency;
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
                Directory.GetAccessControl(URL + FileName);
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
                StopUiThread();
                if (main != null)
                {
                    main.CancelSharing();
                    main.StopReceiving();
                }
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            catch
            {

            }
        }
    }
}
