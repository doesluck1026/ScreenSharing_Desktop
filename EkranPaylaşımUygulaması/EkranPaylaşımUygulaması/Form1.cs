using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EkranPaylaşımUygulaması
{
    public partial class Form1 : Form
    {
        private Main main;
        private Thread uiUpdateThread;
        private int UI_UpdateFrequency = 40;        /// Hz
        private double UI_UpdatePeriod;
        private bool ui_updateEnabled = false;
        public Form1()
        {
            InitializeComponent();
            UI_UpdatePeriod = 1.0 / UI_UpdateFrequency;
        }

        private void btn_Share_Click(object sender, EventArgs e)
        {
            main = new Main(Main.CommunicationTypes.Sender);
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            string ip = txt_IP.Text;
            main = new Main(Main.CommunicationTypes.Receiver);
            StartUiThread();
        }
        private void UpdateUI()
        {
            Stopwatch stp = Stopwatch.StartNew();
            while(ui_updateEnabled)
            {
                picture_screen.Image = main.ScreenImage;
                lbl_FPS.Text = main.FPS.ToString();
                while (stp.Elapsed.TotalSeconds <= UI_UpdatePeriod) ;
                stp.Restart();
            }
        }
        private void StartUiThread()
        {
            ui_updateEnabled = true;
            uiUpdateThread = new Thread(UpdateUI);
        }
        private void StopUiThread()
        {
            ui_updateEnabled = false;
            if(uiUpdateThread!=null)
            {
                if(uiUpdateThread.IsAlive)
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
    }
}
