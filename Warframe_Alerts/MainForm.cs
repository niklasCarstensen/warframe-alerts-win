using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using RemotableObjects;
using WarframeNET;

namespace Warframe_Alerts
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public partial class MainForm : MaterialForm
    {
        private readonly System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
        private bool phaseShift;
        private string lastNotifyText;
        frmRCleint client;

        public bool GameDetection { get; set; } = true;
        public sealed override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set { base.MinimumSize = value; }
        }
        public sealed override Size MaximumSize
        {
            get { return base.MaximumSize; }
            set { base.MaximumSize = value; }
        }

        // Startup
        public MainForm()
        {
            InitializeComponent();

            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.DARK;
            FormBorderStyle = FormBorderStyle.None;
            skinManager.ColorScheme = new ColorScheme((Primary)0x01C2F8, (Primary)0x039AC5, (Primary)0x4CD6FD, (Accent)0x039AC5, TextShade.WHITE);
            
            CBStartM.Checked = config.Data.startMinimized;
            CBNoti.Checked = config.Data.desktopNotifications;
        }
        private void MainWindow_Load(object sender, EventArgs e)
        {
            client = new frmRCleint();

            Global.UpdateFilters();
            WFupdate();

            updateTimer.Interval = config.Data.UpdateInterval;
            updateTimer.Tick += Update_Click;
            updateTimer.Start();

            if (config.Data.startMinimized)
                Task.Factory.StartNew(() => { this.InvokeIfRequired(() => { HideForm(); }); });
        }
        
        // Update
        public void WFupdate()
        {
            #region GetJson
            string status = "";
            string jsonResponse = WarframeHandler.GetJson(ref status);
            if (status != "OK") return;
            WarframeHandler.GetJsonObjects(jsonResponse);
            #endregion

            #region Notify Void-Trader
            if (!config.Data.VoidTraderArrived && WarframeHandler.worldState.WS_VoidTrader.Inventory.Count != 0)
            {
                string items = "";
                foreach (VoidTraderItem item in WarframeHandler.worldState.WS_VoidTrader.Inventory)
                {
                    items += "[" + item.Item + "](" + "https://warframe.fandom.com/wiki/Special:Search?query=" + WebUtility.UrlEncode(item.Item) + ")" + " " + item.Credits + "c " + item.Ducats + "D\n";
                }
                Notify("Update", "Void trader arrived at " + WarframeHandler.worldState.WS_VoidTrader.Location + " with: \n" + items + "\nHe will leave again at " + WarframeHandler.worldState.WS_VoidTrader.EndTime, 1000);
            }
            config.Data.VoidTraderArrived = WarframeHandler.worldState.WS_VoidTrader.Inventory.Count != 0;
            #endregion

            #region Notify Alerts and Invasions
            string notification = "";
            foreach (Alert a in WarframeHandler.worldState.WS_Alerts)
                if (!config.Data.idList.Contains(a.Id))
                {
                    config.Data.idList.Add(a.Id);
                    if (FilterRewards(a.Mission.Reward.ToTitle()))
                        notification += a.ToTitle() + "\nExpires at " + a.EndTime.ToLocalTime().ToLongTimeString() + ", so in " + (int)(a.EndTime.ToLocalTime() - DateTime.Now).TotalMinutes + " minutes\n";
                }
            foreach (Invasion i in WarframeHandler.worldState.WS_Invasions)
                if (!config.Data.idList.Contains(i.Id))
                {
                    config.Data.idList.Add(i.Id);
                    if (!i.IsCompleted && FilterRewards(i.AttackerReward.ToTitle() + " " + i.DefenderReward.ToTitle()))
                        notification += i.ToTitle() + "\n";
                }
            Notify("Update", notification, 1000);
            #endregion

            //stateLabel.InvokeIfRequired(() => { stateLabel.Text = "Worldstate:\n" +
            //    "Cetus Time: " + (WarframeHandler.worldState.WS_CetusCycle.IsDay ? "Day" : "Night") + " " + WarframeHandler.worldState.WS_CetusCycle.TimeLeft + "\n" +
            //    (config.Data.VoidTraderArrived ? WarframeHandler.worldState.WS_VoidTrader.Inventory.Select(x => x.Item + " " + x.Credits + "c " + x.Ducats + "D").Aggregate((x, y) => x + "\n" + y) + 
            //    "Trader leaves at: " + WarframeHandler.worldState.WS_VoidTrader.EndTime.AddHours(1) : 
            //    "Trader Arrival: " + (WarframeHandler.worldState.WS_VoidTrader.StartTime.AddHours(1) - DateTime.Now).ToString(@"dd\:hh\:mm\:ss")) + "\n" +
            //    "\nVoid Fissures: \n" +
            //    WarframeHandler.worldState.WS_Fissures.Select((x) => { return (x.EndTime.AddHours(1) - DateTime.Now).ToString(@"hh\:mm\:ss") + " " + x.Tier + " " + x.MissionType + "\n"; }).
            //        Aggregate((x, y) => { return x + y; });
            //});

            #region Update GUI
            this.InvokeIfRequired(() =>
            {
                AlertData.Items.Clear();
                InvasionData.Items.Clear();
                FissureData.Items.Clear();

                IOrderedEnumerable<Alert> alerts = WarframeHandler.worldState.WS_Alerts.OrderBy(x => (x.EndTime.ToLocalTime() - DateTime.Now));
                foreach (Alert a in WarframeHandler.worldState.WS_Alerts)
                    AlertData.Items.Add(new ListViewItem(new string[] { a.Mission.Type, a.ToTitle(), a.Mission.Faction, (a.EndTime.ToLocalTime() - DateTime.Now).ToReadable() + " ▾" }));

                IOrderedEnumerable<Invasion> invasions = WarframeHandler.worldState.WS_Invasions.OrderBy(x => -x.StartTime.Ticks);
                foreach (Invasion inv in invasions)
                    if (!inv.IsCompleted)
                        InvasionData.Items.Add(new ListViewItem(new string[] { inv.ToTitle(), Math.Round(inv.Completion, 2) + "%",
                            (DateTime.Now - inv.StartTime.ToLocalTime()).ToString(@"hh\:mm\:ss") + " ▴" }));

                IOrderedEnumerable<Fissure> fissures = WarframeHandler.worldState.WS_Fissures.OrderBy(x => x.TierNumber);
                foreach (Fissure f in fissures)
                    FissureData.Items.Add(new ListViewItem(new string[] { f.Tier + " - " + f.MissionType + " - " + (f.EndTime.ToLocalTime() - DateTime.Now).ToReadable() + " ▾" }));

                AlertData.Fix();
                InvasionData.Fix();
                FissureData.Fix();
            });
            #endregion

            while (config.Data.idList.Count > 100)
                config.Data.idList.RemoveAt(0);
        }
        void Notify(string title, string text, int timeout)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(title) || timeout < 1)
                return;

            //client.setMessage(text);

            if (config.Data.desktopNotifications && !GameDetection ||
                config.Data.desktopNotifications && !DetectWarframe())
            {
                Notify_Icon.BalloonTipText = text;
                Notify_Icon.BalloonTipTitle = title;
                Notify_Icon.BalloonTipClicked += ((object sender, EventArgs e) => {
                    try
                    {
                        string notifyText = ((NotifyIcon)sender).BalloonTipText;
                        if (notifyText != lastNotifyText && MessageBox.Show("Show:\n" + notifyText + "\nin Browser?", "Show?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            ShowInBrowser(notifyText);
                        lastNotifyText = notifyText;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("UwU we made fucky wucky \n\n" + ex);
                    }
                });
                Notify_Icon.ShowBalloonTip(timeout);
            }
        }
        bool FilterRewards(string title)
        {
            if (config.Data.Filters.Count == 0)
                return true;
            return title.ContainsOneOf(Global.Filters) && !title.ContainsOneOf(Global.Ignorers);
        }
        void ShowInBrowser(string title)
        {
            foreach (string s in title.Split('\n'))
                if (s.Contains("-"))
                    Process.Start("https://warframe.fandom.com/wiki/Special:Search?query=" + WebUtility.UrlEncode(s.Split('-')[0].Trim(' ')));
        }

        // Forms Events
        private void Update_Click(object sender, EventArgs e)
        {
            Action update = WFupdate;
            update.BeginInvoke(ar => update.EndInvoke(ar), null);
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void Setting_Click(object sender, EventArgs e)
        {
            var sf = new Settings(this);
            sf.ShowDialog();
        }
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void Resize_Action(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && !phaseShift)
                HideForm();
        }
        private void Notification_Icon_Double_Click(object sender, EventArgs e)
        {
            this.InvokeIfRequired(() => { ShowForm(); });
        }
        private void minButton_Click(object sender, EventArgs e)
        {
            HideForm();
        }
        private void CBStartM_CheckedChanged(object sender, EventArgs e)
        {
            config.Data.startMinimized = CBStartM.Checked;
            config.Save();
        }
        private void CBNoti_CheckedChanged(object sender, EventArgs e)
        {
            config.Data.desktopNotifications = CBNoti.Checked;
            config.Save();
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            config.Save();
        }
        private void AlertData_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("Show:\n" + AlertData.SelectedItems[0].SubItems[1].Text + "\nin Browser?", "Show?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                ShowInBrowser(AlertData.SelectedItems[0].SubItems[1].Text);
        }

        // Other
        private static bool DetectWarframe()
        {
            var flag = false;
            var processlist = Process.GetProcesses();

            foreach (var process in processlist)
            {
                if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;
                if (process.ProcessName == "Warframe.x64" || process.ProcessName == "Warframe")
                {
                    flag = true;
                }
            }

            return flag;
        }
        private void HideForm()
        {
            phaseShift = true;
            this.ForceHide();
            phaseShift = false;
        }
        private void ShowForm()
        {
            phaseShift = true;
            this.ForceShow();
            WindowState = FormWindowState.Normal;
            phaseShift = false;
        }
    }
}
