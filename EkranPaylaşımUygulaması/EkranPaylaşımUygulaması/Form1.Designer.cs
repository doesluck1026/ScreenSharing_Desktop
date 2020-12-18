
namespace EkranPaylaşımUygulaması
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picture_screen = new System.Windows.Forms.PictureBox();
            this.btn_Share = new System.Windows.Forms.Button();
            this.btn_Connect = new System.Windows.Forms.Button();
            this.txt_IP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_FPS = new System.Windows.Forms.Label();
            this.lbl_TransferSpeed = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picture_screen)).BeginInit();
            this.SuspendLayout();
            // 
            // picture_screen
            // 
            this.picture_screen.Location = new System.Drawing.Point(10, 11);
            this.picture_screen.Name = "picture_screen";
            this.picture_screen.Size = new System.Drawing.Size(997, 774);
            this.picture_screen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture_screen.TabIndex = 0;
            this.picture_screen.TabStop = false;
            // 
            // btn_Share
            // 
            this.btn_Share.Location = new System.Drawing.Point(1086, 128);
            this.btn_Share.Name = "btn_Share";
            this.btn_Share.Size = new System.Drawing.Size(92, 38);
            this.btn_Share.TabIndex = 1;
            this.btn_Share.Text = "Paylaş";
            this.btn_Share.UseVisualStyleBackColor = true;
            this.btn_Share.Click += new System.EventHandler(this.btn_Share_Click);
            // 
            // btn_Connect
            // 
            this.btn_Connect.Location = new System.Drawing.Point(1086, 84);
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(92, 38);
            this.btn_Connect.TabIndex = 2;
            this.btn_Connect.Text = "Bağlan";
            this.btn_Connect.UseVisualStyleBackColor = true;
            this.btn_Connect.Click += new System.EventHandler(this.btn_Connect_Click);
            // 
            // txt_IP
            // 
            this.txt_IP.Location = new System.Drawing.Point(1086, 58);
            this.txt_IP.Name = "txt_IP";
            this.txt_IP.Size = new System.Drawing.Size(92, 20);
            this.txt_IP.TabIndex = 3;
            this.txt_IP.Text = "192.168.1.37";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1060, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1085, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "FPS:";
            // 
            // lbl_FPS
            // 
            this.lbl_FPS.AutoSize = true;
            this.lbl_FPS.Location = new System.Drawing.Point(1119, 11);
            this.lbl_FPS.Name = "lbl_FPS";
            this.lbl_FPS.Size = new System.Drawing.Size(13, 13);
            this.lbl_FPS.TabIndex = 6;
            this.lbl_FPS.Text = "0";
            // 
            // lbl_TransferSpeed
            // 
            this.lbl_TransferSpeed.AutoSize = true;
            this.lbl_TransferSpeed.Location = new System.Drawing.Point(1121, 32);
            this.lbl_TransferSpeed.Name = "lbl_TransferSpeed";
            this.lbl_TransferSpeed.Size = new System.Drawing.Size(13, 13);
            this.lbl_TransferSpeed.TabIndex = 8;
            this.lbl_TransferSpeed.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1083, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "speed:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1155, 687);
            this.Controls.Add(this.lbl_TransferSpeed);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbl_FPS);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_IP);
            this.Controls.Add(this.btn_Connect);
            this.Controls.Add(this.btn_Share);
            this.Controls.Add(this.picture_screen);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.picture_screen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picture_screen;
        private System.Windows.Forms.Button btn_Share;
        private System.Windows.Forms.Button btn_Connect;
        private System.Windows.Forms.TextBox txt_IP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbl_FPS;
        private System.Windows.Forms.Label lbl_TransferSpeed;
        private System.Windows.Forms.Label label4;
    }
}

