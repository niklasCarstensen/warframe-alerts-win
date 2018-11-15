using System;
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

namespace Warframe_Alerts
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public partial class MainForm : MaterialForm
    {
        private readonly List<string> idList = new List<string>();
        private readonly System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
        private bool phaseShift;

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
            MaximumSize = new Size(1020, 530);
            MinimumSize = new Size(1020, 530);
            skinManager.ColorScheme = new ColorScheme((Primary)0x01C2F8, (Primary)0x039AC5, (Primary)0x4CD6FD, (Accent)0x039AC5, TextShade.WHITE);

            CBLog.Checked = config.Data.enableLog;
            CBStartM.Checked = config.Data.startMinimized;
        }
        private void MainWindow_Load(object sender, EventArgs e)
        {
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
            var wf = new WarframeHandler();

            var alerts = new List<Alert>();
            var invasions = new List<Invasion>();
            var outbreaks = new List<Outbreak>();

            var status = "";
            var response = wf.GetXml(ref status);

            if (status != "OK")
            {
                var message = "Network not responding" + '\n';
                message = message + response;

                Notify_Icon.BalloonTipText = message;
                Notify_Icon.BalloonTipTitle = @"Update Failed";
                Notify_Icon.ShowBalloonTip(2000);
                return;
            }

            wf.GetObjects(response, ref alerts, ref invasions, ref outbreaks);

            if (GameDetection)
            {
                if (!DetectWarframe())
                {
                    NotifyAlertsAndInvasions(ref alerts, ref invasions, ref outbreaks);
                }
            }
            else
            {
                NotifyAlertsAndInvasions(ref alerts, ref invasions, ref outbreaks);
            }

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
                var eTime = Convert.ToDateTime(alerts[i].Expiry_Date);

                var title = alerts[i].Title;
                var titleSp = title.Split('-');

                var tempTitle = titleSp[0];

                for (var j = 1; j < titleSp.Length - 1; j++)
                {
                    tempTitle = tempTitle + "-" + titleSp[j];
                }

                var description = alerts[i].Description;
                var faction = alerts[i].Faction;
                var aId = alerts[i].ID;

                //if (!Filter_Alerts(title)) continue;  Display all Alerts
                var aSpan = eTime.Subtract(DateTime.Now);
                var aLeft = "";

                if (aSpan.Days != 0)
                {
                    aLeft = aLeft + aSpan.Days + " Days ";
                }

                if (aSpan.Hours != 0)
                {
                    aLeft = aLeft + aSpan.Hours + " Hours ";
                }

                if (aSpan.Minutes != 0)
                {
                    aLeft = aLeft + aSpan.Minutes + " Minutes ";
                }

                aLeft = aLeft + aSpan.Seconds + " Seconds Left";

                idList.Add(aId);
                string[] row = { description, tempTitle, faction, aLeft };
                var listViewItem = new ListViewItem(row);
                Invoke(new Action(() => AlertData.Items.Add(listViewItem)));
            }

            for (var i = 0; i < invasions.Count; i++)
            {
                var title = invasions[i].Title;
                var invId = invasions[i].ID;

                var sTime = Convert.ToDateTime(invasions[i].Start_Date);
                var now = DateTime.Now;
                var span = now.Subtract(sTime);

                var time = "";

                if (span.Hours != 0)
                {
                    time = time + span.Hours + " Hours ";
                }

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

                var sTime = Convert.ToDateTime(outbreaks[i].Start_Date);
                var now = DateTime.Now;
                var oSpan = now.Subtract(sTime);

                var oTime = "";

                if (oSpan.Hours != 0)
                {
                    oTime = oTime + oSpan.Hours + " Hours ";
                }

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
                    return;

                if (config.Data.enableLog)
                    LogAlert(a[j].ID, a[j].Title);

                if (FilterAlerts(a[j].Title))
                    notificationMessage = notificationMessage + a[j].Title + '\n';
            }

            for (int j = 0; j < i.Count; j++)
            {
                if (idList.Contains(i[j].ID))
                    return;

                if (config.Data.enableLog)
                    LogInvasion(i[j].ID, i[j].Title);

                if (FilterAlerts(i[j].Title))
                    notificationMessage = notificationMessage + i[j].Title + '\n';
            }

            for (int j = 0; j < o.Count; j++)
            {
                if (idList.Contains(o[j].ID))
                    return;

                if (config.Data.enableLog)
                    LogInvasion(o[j].ID, o[j].Title);

                if (FilterAlerts(o[j].Title))
                    notificationMessage = notificationMessage + o[j].Title + '\n';
            }

            if (notificationMessage != "")
                Notify("Update", notificationMessage, 1000);
        }
        void Notify(string title, string text, int timeout)
        {
            Notify_Icon.BalloonTipText = text;
            Notify_Icon.BalloonTipTitle = title;
            Notify_Icon.ShowBalloonTip(timeout);
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

            Notify_Icon.BalloonTipText = @"Warframe_Alerts is running in background";
            Notify_Icon.BalloonTipTitle = @"Update";
            if (!config.Data.startMinimized)
                Notify_Icon.ShowBalloonTip(2000);
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
