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
        private readonly List<string> idList = new List<string>();
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
            MaximumSize = new Size(1220, 530);
            MinimumSize = new Size(1020, 530);
            skinManager.ColorScheme = new ColorScheme((Primary)0x01C2F8, (Primary)0x039AC5, (Primary)0x4CD6FD, (Accent)0x039AC5, TextShade.WHITE);

            CBLog.Checked = config.Data.enableLog;
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
            List<Alert> alerts = new List<Alert>();
            List<Invasion> invasions = new List<Invasion>();
            List<Outbreak> outbreaks = new List<Outbreak>();

            string status = "";
            string xmlResponse = WarframeHandler.GetXml(ref status);

            if (status != "OK")
                return;

            string jsonResponse = WarframeHandler.GetJson(ref status);

            if (status != "OK")
                return;

            WarframeHandler.GetJsonObjects(jsonResponse);

            if (!config.Data.VoidTraderArrived && WarframeHandler.worldState.WS_VoidTrader.Inventory.Count != 0)
            {
                string items = "";
                foreach (VoidTraderItem item in WarframeHandler.worldState.WS_VoidTrader.Inventory)
                {
                    items += item.Item + " " + item.Credits + "c " + item.Ducats + "D\n";
                }
                Notify("Update", "Void trader arrived at " + WarframeHandler.worldState.WS_VoidTrader.Location + " with: \n" + items + "\nHe will leave again at " + WarframeHandler.worldState.WS_VoidTrader.EndTime, 1000);
            }
            config.Data.VoidTraderArrived = WarframeHandler.worldState.WS_VoidTrader.Inventory.Count != 0;
            
            stateLabel.InvokeIfRequired(() => { stateLabel.Text = "Worldstate:\n" +
                "Cetus Time: " + (WarframeHandler.worldState.WS_CetusCycle.IsDay ? "Day" : "Night") + " " + WarframeHandler.worldState.WS_CetusCycle.TimeLeft + "\n" +
                (config.Data.VoidTraderArrived ? WarframeHandler.worldState.WS_VoidTrader.Inventory.Select(x => x.Item + " " + x.Credits + "c " + x.Ducats + "D").Aggregate((x, y) => x + "\n" + y) + 
                "Trader leaves at: " + WarframeHandler.worldState.WS_VoidTrader.EndTime.AddHours(1) : 
                "Trader Arrival: " + (WarframeHandler.worldState.WS_VoidTrader.StartTime.AddHours(1) - DateTime.Now).ToString(@"dd\:hh\:mm\:ss")) + "\n" +
                "\nVoid Fissures: \n" +
                WarframeHandler.worldState.WS_Fissures.Select((x) => { return (x.EndTime.AddHours(1) - DateTime.Now).ToString(@"hh\:mm\:ss") + " " + x.Tier + " " + x.MissionType + "\n"; }).
                    Aggregate((x, y) => { return x + y; });
            });

            WarframeHandler.GetXMLObjects(xmlResponse, ref alerts, ref invasions, ref outbreaks);

            NotifyAlertsAndInvasions(ref alerts, ref invasions, ref outbreaks);

            Invoke(new Action(() =>
            {
                AlertData.Items.Clear();
                InvasionData.Items.Clear();
                idList.Clear();
            }));

            // Display alerts and invasions that are about to run out first
            alerts.Reverse();
            invasions.Reverse();

            for (var i = 0; i < alerts.Count; i++)
            {
                DateTime eTime = Convert.ToDateTime(alerts[i].ExpiryDate);

                string title = alerts[i].Title;
                string[] titleSp = title.Split('-');

                string tempTitle = titleSp[0];

                for (int j = 1; j < titleSp.Length - 1; j++)
                    tempTitle = tempTitle + "-" + titleSp[j];

                string description = alerts[i].Description;
                string faction = alerts[i].Faction;
                string aId = alerts[i].ID;
                
                TimeSpan aSpan = eTime.Subtract(DateTime.Now);
                string aLeft = "";

                if (aSpan.Days != 0)
                    aLeft = aLeft + aSpan.Days + " Days ";

                if (aSpan.Hours != 0)
                    aLeft = aLeft + aSpan.Hours + " Hours ";

                if (aSpan.Minutes != 0)
                    aLeft = aLeft + aSpan.Minutes + " Minutes ";

                aLeft = aLeft + aSpan.Seconds + " Seconds Left";

                idList.Add(aId);
                string[] row = { description, tempTitle, faction, aLeft };
                ListViewItem listViewItem = new ListViewItem(row);
                Invoke(new Action(() => AlertData.Items.Add(listViewItem)));
            }

            for (var i = 0; i < invasions.Count; i++)
            {
                var title = invasions[i].Title;
                var invId = invasions[i].ID;

                var sTime = Convert.ToDateTime(invasions[i].StartDate);
                var now = DateTime.Now;
                var span = now.Subtract(sTime);

                var time = "";

                if (span.Hours != 0)
                    time = time + span.Hours + " Hours ";

                time = time + span.Minutes + " Minutes Ago";

                idList.Add(invId);
                string[] row = { title, "Invasion", time };
                var listViewItem = new ListViewItem(row);
                Invoke(new Action(() => InvasionData.Items.Add(listViewItem)));
            }

            for (var i = 0; i < outbreaks.Count; i++)
            {
                var title = outbreaks[i].Title;
                var oId = outbreaks[i].ID;

                var sTime = Convert.ToDateTime(outbreaks[i].StartDate);
                var now = DateTime.Now;
                var oSpan = now.Subtract(sTime);

                var oTime = "";

                if (oSpan.Hours != 0)
                    oTime = oTime + oSpan.Hours + " Hours ";

                oTime = oTime + oSpan.Minutes + " Minutes Ago";

                idList.Add(oId);
                string[] row = { title, "Outbreak", oTime };
                var listViewItem = new ListViewItem(row);
                Invoke(new Action(() => InvasionData.Items.Add(listViewItem)));
            }
            
            Invoke(new Action(() =>
            {
                AlertData.Scrollable = AlertData.Items.Count != 3;
                InvasionData.Scrollable = InvasionData.Items.Count != 3;
                InvasionData.Columns[0].Width = InvasionData.Items.Count > 3 ? 627 : 644;
                AlertData.Columns[3].Width = AlertData.Items.Count > 3 ? 235 : 252;
            }));
        }
        public void NotifyAlertsAndInvasions(ref List<Alert> a, ref List<Invasion> i, ref List<Outbreak> o)
        {
            string notificationMessage = "";

            for (int j = 0; j < a.Count; j++)
            {
                if (idList.Contains(a[j].ID))
                    continue;

                if (config.Data.enableLog)
                    LogAlert(a[j].ID, a[j].Title);

                if (FilterAlerts(a[j].Title))
                {
                    DateTime exp = Convert.ToDateTime(a[j].ExpiryDate);
                    notificationMessage += a[j].Title + "\nExpires at " + exp.ToString(@"hh\:mm\:ss") + " so in " + (int)(exp - DateTime.Now).TotalMinutes +  "m\n";
                }
            }

            for (int j = 0; j < i.Count; j++)
            {
                if (idList.Contains(i[j].ID))
                    continue;

                if (config.Data.enableLog)
                    LogInvasion(i[j].ID, i[j].Title);

                if (FilterAlerts(i[j].Title))
                    notificationMessage += i[j].Title + "\n";
            }

            for (int j = 0; j < o.Count; j++)
            {
                if (idList.Contains(o[j].ID))
                    continue;

                if (config.Data.enableLog)
                    LogInvasion(o[j].ID, o[j].Title);

                if (FilterAlerts(o[j].Title))
                    notificationMessage += o[j].Title + "\n";
            }

            if (notificationMessage != "")
                Notify("Update", notificationMessage, 1000);
        }
        void Notify(string title, string text, int timeout)
        {
            if (title == "Update")
                client.setMessage(text + "|" + stateLabel.Text);

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
                            foreach (string s in notifyText.Split('\n'))
                                if (s.Contains("-"))
                                    Process.Start("https://warframe.fandom.com/wiki/Special:Search?query=" + WebUtility.UrlEncode(s.Split('-')[0].Trim(' ')));
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
        private bool FilterAlerts(string title)
        {
            if (config.Data.Filters.Count == 0)
                return true;
            return title.ContainsOneOf(Global.Filters) && !title.ContainsOneOf(Global.Ignorers);
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
        private void CBLog_CheckedChanged(object sender, EventArgs e)
        {
            config.Data.enableLog = CBLog.Checked;
            config.Save();
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

        // Log
        public void LogAlert(string id, string disc)
        {
            var flag = true;

            if (File.Exists("AlertLog.txt"))
            {
                var text = File.ReadAllText("AlertLog.txt");

                if (text.Contains(id))
                {
                    flag = false;
                }
            }

            if (flag)
            {
                File.AppendAllText("AlertLog.txt", id + '\t' + disc + Environment.NewLine);
            }
        }
        public void LogInvasion(string id, string disc)
        {
            var flag = true;

            if (File.Exists("InvasionLog.txt"))
            {
                var text = File.ReadAllText("InvasionLog.txt");

                if (text.Contains(id))
                {
                    flag = false;
                }
            }

            if (flag)
            {
                File.AppendAllText("InvasionLog.txt", id + '\t' + disc + Environment.NewLine);
            }
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
