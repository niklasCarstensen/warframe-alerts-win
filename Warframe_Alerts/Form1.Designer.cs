﻿using System.ComponentModel;
using System.Windows.Forms;

namespace Warframe_Alerts
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.AlertData = new System.Windows.Forms.DataGridView();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Faction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Time_Left = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.InvasionData = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.Notify_Icon = new System.Windows.Forms.NotifyIcon(this.components);
            this.Menu_Strip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Menu_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSetting = new System.Windows.Forms.Button();
            this.buttonSM = new System.Windows.Forms.Button();
            this.BtnLog = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.AlertData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InvasionData)).BeginInit();
            this.Menu_Strip.SuspendLayout();
            this.SuspendLayout();
            // 
            // AlertData
            // 
            this.AlertData.AllowUserToAddRows = false;
            this.AlertData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AlertData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Description,
            this.Title,
            this.Faction,
            this.Time_Left});
            this.AlertData.Location = new System.Drawing.Point(12, 25);
            this.AlertData.Name = "AlertData";
            this.AlertData.Size = new System.Drawing.Size(956, 225);
            this.AlertData.TabIndex = 0;
            // 
            // Description
            // 
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.Width = 200;
            // 
            // Title
            // 
            this.Title.HeaderText = "Title";
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            this.Title.Width = 400;
            // 
            // Faction
            // 
            this.Faction.HeaderText = "Faction";
            this.Faction.Name = "Faction";
            this.Faction.ReadOnly = true;
            // 
            // Time_Left
            // 
            this.Time_Left.HeaderText = "Time Left";
            this.Time_Left.Name = "Time_Left";
            this.Time_Left.ReadOnly = true;
            this.Time_Left.Width = 213;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Alerts";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 263);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Invasions";
            // 
            // InvasionData
            // 
            this.InvasionData.AllowUserToAddRows = false;
            this.InvasionData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.InvasionData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.InvasionData.Location = new System.Drawing.Point(12, 279);
            this.InvasionData.Name = "InvasionData";
            this.InvasionData.Size = new System.Drawing.Size(956, 225);
            this.InvasionData.TabIndex = 3;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.FillWeight = 197.1523F;
            this.dataGridViewTextBoxColumn2.HeaderText = "Title";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 600;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.FillWeight = 25.2336F;
            this.dataGridViewTextBoxColumn3.HeaderText = "Type";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 77;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.FillWeight = 77.61415F;
            this.dataGridViewTextBoxColumn4.HeaderText = "Start Time";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 236;
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(893, 510);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(75, 23);
            this.buttonExit.TabIndex = 4;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Location = new System.Drawing.Point(812, 510);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(75, 23);
            this.buttonUpdate.TabIndex = 5;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.Update_Click);
            // 
            // Notify_Icon
            // 
            this.Notify_Icon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.Notify_Icon.ContextMenuStrip = this.Menu_Strip;
            this.Notify_Icon.Icon = ((System.Drawing.Icon)(resources.GetObject("Notify_Icon.Icon")));
            this.Notify_Icon.Text = "Warframe Alerts";
            this.Notify_Icon.Visible = true;
            this.Notify_Icon.DoubleClick += new System.EventHandler(this.Notification_Icon_Double_Click);
            // 
            // Menu_Strip
            // 
            this.Menu_Strip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_Exit});
            this.Menu_Strip.Name = "Menu_Strip";
            this.Menu_Strip.Size = new System.Drawing.Size(93, 26);
            // 
            // Menu_Exit
            // 
            this.Menu_Exit.Name = "Menu_Exit";
            this.Menu_Exit.Size = new System.Drawing.Size(92, 22);
            this.Menu_Exit.Text = "Exit";
            this.Menu_Exit.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // buttonSetting
            // 
            this.buttonSetting.Location = new System.Drawing.Point(12, 510);
            this.buttonSetting.Name = "buttonSetting";
            this.buttonSetting.Size = new System.Drawing.Size(75, 23);
            this.buttonSetting.TabIndex = 6;
            this.buttonSetting.Text = "Settings";
            this.buttonSetting.UseVisualStyleBackColor = true;
            this.buttonSetting.Click += new System.EventHandler(this.Setting_Click);
            // 
            // buttonSM
            // 
            this.buttonSM.Location = new System.Drawing.Point(93, 510);
            this.buttonSM.Name = "buttonSM";
            this.buttonSM.Size = new System.Drawing.Size(133, 23);
            this.buttonSM.TabIndex = 7;
            this.buttonSM.Text = "Enable Start Minimized";
            this.buttonSM.UseVisualStyleBackColor = true;
            this.buttonSM.Click += new System.EventHandler(this.buttonSM_Click);
            // 
            // BtnLog
            // 
            this.BtnLog.Location = new System.Drawing.Point(233, 509);
            this.BtnLog.Name = "BtnLog";
            this.BtnLog.Size = new System.Drawing.Size(75, 23);
            this.BtnLog.TabIndex = 8;
            this.BtnLog.Text = "Enable Log";
            this.BtnLog.UseVisualStyleBackColor = true;
            this.BtnLog.Click += new System.EventHandler(this.BtnLog_Click);
            // 
            // Main_Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 541);
            this.Controls.Add(this.BtnLog);
            this.Controls.Add(this.buttonSM);
            this.Controls.Add(this.buttonSetting);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.InvasionData);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AlertData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Warframe Alerts";
            this.Resize += new System.EventHandler(this.Resize_Action);
            ((System.ComponentModel.ISupportInitialize)(this.AlertData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InvasionData)).EndInit();
            this.Menu_Strip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DataGridView AlertData;
        private Label label1;
        private Label label2;
        private DataGridView InvasionData;
        private Button buttonExit;
        private Button buttonUpdate;
        private NotifyIcon Notify_Icon;
        private ContextMenuStrip Menu_Strip;
        private ToolStripMenuItem Menu_Exit;
        private Button buttonSetting;
        private Button buttonSM;
        private DataGridViewTextBoxColumn Description;
        private DataGridViewTextBoxColumn Title;
        private DataGridViewTextBoxColumn Faction;
        private DataGridViewTextBoxColumn Time_Left;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private Button BtnLog;
    }
}

