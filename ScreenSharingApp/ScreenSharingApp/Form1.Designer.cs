
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.picture_screen = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStrip_Connect = new System.Windows.Forms.ToolStripButton();
            this.txt_IP = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.lbl_FPS = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.lbl_TransferSpeed = new System.Windows.Forms.ToolStripLabel();
            this.toolStrip_Share = new System.Windows.Forms.ToolStripButton();
            this.tool_EnableControls = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.picture_screen)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picture_screen
            // 
            this.picture_screen.Location = new System.Drawing.Point(0, 28);
            this.picture_screen.Name = "picture_screen";
            this.picture_screen.Size = new System.Drawing.Size(1109, 532);
            this.picture_screen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture_screen.TabIndex = 0;
            this.picture_screen.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStrip_Share,
            this.toolStripSeparator1,
            this.toolStrip_Connect,
            this.txt_IP,
            this.toolStripLabel1,
            this.lbl_FPS,
            this.toolStripLabel3,
            this.lbl_TransferSpeed,
            this.tool_EnableControls});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1109, 25);
            this.toolStrip1.TabIndex = 11;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStrip_Connect
            // 
            this.toolStrip_Connect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStrip_Connect.Image = ((System.Drawing.Image)(resources.GetObject("toolStrip_Connect.Image")));
            this.toolStrip_Connect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStrip_Connect.Name = "toolStrip_Connect";
            this.toolStrip_Connect.Size = new System.Drawing.Size(56, 22);
            this.toolStrip_Connect.Text = "Connect";
            this.toolStrip_Connect.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.toolStrip_Connect.Click += new System.EventHandler(this.toolStrip_Connect_Click);
            // 
            // txt_IP
            // 
            this.txt_IP.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txt_IP.Name = "txt_IP";
            this.txt_IP.Size = new System.Drawing.Size(100, 25);
            this.txt_IP.Text = "192.168.1.36";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(26, 22);
            this.toolStripLabel1.Text = "FPS";
            // 
            // lbl_FPS
            // 
            this.lbl_FPS.Name = "lbl_FPS";
            this.lbl_FPS.Size = new System.Drawing.Size(13, 22);
            this.lbl_FPS.Text = "0";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(39, 22);
            this.toolStripLabel3.Text = "Speed";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // lbl_TransferSpeed
            // 
            this.lbl_TransferSpeed.Name = "lbl_TransferSpeed";
            this.lbl_TransferSpeed.Size = new System.Drawing.Size(13, 22);
            this.lbl_TransferSpeed.Text = "0";
            // 
            // toolStrip_Share
            // 
            this.toolStrip_Share.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStrip_Share.Image = ((System.Drawing.Image)(resources.GetObject("toolStrip_Share.Image")));
            this.toolStrip_Share.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStrip_Share.Name = "toolStrip_Share";
            this.toolStrip_Share.Size = new System.Drawing.Size(40, 22);
            this.toolStrip_Share.Text = "Share";
            this.toolStrip_Share.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.toolStrip_Share.Click += new System.EventHandler(this.toolStrip_Share_Click);
            // 
            // tool_EnableControls
            // 
            this.tool_EnableControls.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tool_EnableControls.Image = ((System.Drawing.Image)(resources.GetObject("tool_EnableControls.Image")));
            this.tool_EnableControls.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tool_EnableControls.Name = "tool_EnableControls";
            this.tool_EnableControls.Size = new System.Drawing.Size(91, 22);
            this.tool_EnableControls.Text = "EnableControls";
            this.tool_EnableControls.Click += new System.EventHandler(this.tool_EnableControls_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1109, 559);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.picture_screen);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.picture_screen)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picture_screen;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStrip_Connect;
        private System.Windows.Forms.ToolStripTextBox txt_IP;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel lbl_FPS;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel lbl_TransferSpeed;
        private System.Windows.Forms.ToolStripButton toolStrip_Share;
        private System.Windows.Forms.ToolStripButton tool_EnableControls;
    }
}

