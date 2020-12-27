﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenSharing_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Main main;
        private Thread uiUpdateThread;
        private int UI_UpdateFrequency = 40;        /// Hz
        private double UI_UpdatePeriod;
        private bool ui_updateEnabled = false;
        private bool IsAutoShareEnabled = false;
        private BagFile OptionsFile;
        private string URL;
        private string FileName = "Options.dat";
        public MainWindow()
        {
            InitializeComponent();
            UI_UpdatePeriod = 1.0 / UI_UpdateFrequency;
            OptionsFile = new BagFile();
            URL = Environment.CurrentDirectory + "\\";
        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            main = new Main(Main.CommunicationTypes.Sender);
            main.StartSharingScreen();
            StartUiThread();
        }

        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            string ip = txt_IP.Text;
            main = new Main(Main.CommunicationTypes.Receiver);
            main.StartReceiving(ip);
            StartUiThread();
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
                        if(main.ScreenImage!=null)
                            imageBox.Source = BitmapSourceConvert.ToBitmapSource(main.ScreenImage);
                        lbl_FPS.Content = main.FPS.ToString();
                        lbl_Speed.Content = main.TransferSpeed.ToString("0.00") + " MB/s";
                        main.IsImageReceived = false;
                        main.IsImageSent = false;
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
            StopUiThread();
            if (main != null)
            {
                main.CancelSharing();
                main.StopReceiving();
            }
            Application.Current.Shutdown();
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

            }
        }

        private void chc_AutoShare_Click(object sender, RoutedEventArgs e)
        {
            IsAutoShareEnabled = (bool)chc_AutoShare.IsChecked;
            SaveOptions();
        }
        private void LoadOptions()
        {
            FileStream readerFileStream = new FileStream(URL+FileName, FileMode.Open, FileAccess.Read);
            // Reconstruct data
            BinaryFormatter formatter = new BinaryFormatter();
            OptionsFile = (BagFile)formatter.Deserialize(readerFileStream);
            IsAutoShareEnabled = OptionsFile.IsAutoShareEnabled;
        }
        private void SaveOptions()
        {
            OptionsFile.IsAutoShareEnabled = IsAutoShareEnabled;
            FileStream writerFileStream = new FileStream(URL+FileName, FileMode.Create, FileAccess.Write);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(writerFileStream, OptionsFile);
            writerFileStream.Close();
        }
    }
}