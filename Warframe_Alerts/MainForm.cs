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

namespace Warframe_Alerts
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public partial class MainWindow : MaterialForm
    {
        private readonly List<string> _idList = new List<string>();
        
        private readonly Timer updateTimer = new Timer();
        private bool _phaseShift;

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

        public MainWindow()
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
            WF_Update();

            updateTimer.Interval = config.Data.UpdateInterval;
            updateTimer.Tick += Update_Click;
            updateTimer.Start();
        }

        public void WF_Update()
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
                    Notify_Alerts_And_Invasions(ref alerts, ref invasions, ref outbreaks);
                }
            }
            else
            {
                Notify_Alerts_And_Invasions(ref alerts, ref invasions, ref outbreaks);
            }

            Invoke(new Action(() =>
            {
                AlertData.Items.Clear();
                InvasionData.Items.Clear();
                _idList.Clear();
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

                _idList.Add(aId);
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

                _idList.Add(invId);
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

                _idList.Add(oId);
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
        public void Notify_Alerts_And_Invasions(ref List<Alert> a, ref List<Invasion> I, ref List<Outbreak> o)
        {
            var notificationMessage = "";

            for (var i = 0; i < a.Count; i++)
            {
                var found = false;

                for (var j = 0; j < _idList.Count && !found; j++)
                {
                    if (a[i].ID == _idList[j])
                    {
                        found = true;
                    }
                }

                if (found) continue;
                if (config.Data.enableLog) Log_Alert(a[i].ID, a[i].Title);

                if (Filter_Alerts(a[i].Title))
                {
                    notificationMessage = notificationMessage + a[i].Title + '\n';
                }
            }

            for (var i = 0; i < I.Count; i++)
            {
                var found = false;

                for (var j = 0; j < _idList.Count && !found; j++)
                {
                    if (I[i].ID == _idList[j])
                    {
                        found = true;
                    }
                }

                if (found) continue;
                if (config.Data.enableLog) Log_Invasion(I[i].ID, I[i].Title);

                if (Filter_Alerts(I[i].Title))
                {
                    notificationMessage = notificationMessage + I[i].Title + '\n';
                }
            }

            for (var i = 0; i < o.Count; i++)
            {
                var found = false;

                for (var j = 0; j < _idList.Count && !found; j++)
                {
                    if (o[i].ID == _idList[j])
                    {
                        found = true;
                    }
                }

                if (found) continue;
                if (config.Data.enableLog) Log_Invasion(o[i].ID, o[i].Title);

                if (Filter_Alerts(o[i].Title))
                {
                    notificationMessage = notificationMessage + o[i].Title + '\n';
                }
            }

            if (notificationMessage == "") return;
            Notify_Icon.BalloonTipText = notificationMessage;
            Notify_Icon.BalloonTipTitle = @"Update";
            Notify_Icon.ShowBalloonTip(2000);
        }
        private bool Filter_Alerts(string title)
        {
            if (config.Data.Filters.Count == 0)
                return true;
            return title.ContainsOneOf(config.Data.Filters);
        }

        // Forms Events
        private void Update_Click(object sender, EventArgs e)
        {
            Action update = WF_Update;
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
            config.Data.startMinimized = CBLog.Checked;
            config.Save();
        }
        private void CBStartM_CheckedChanged(object sender, EventArgs e)
        {
            config.Data.startMinimized = CBStartM.Checked;
            config.Save();
        }
        private void Resize_Action(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized || _phaseShift) return;
            Hide();
            Opacity = 0;
            Notify_Icon.BalloonTipText = @"Warframe_Alerts is running in background";
            Notify_Icon.BalloonTipTitle = @"Update";
            if (config.Data.startMinimized) return;
            Notify_Icon.ShowBalloonTip(2000);
        }
        private void Notification_Icon_Double_Click(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized) return;
            _phaseShift = true;
            ShowInTaskbar = true;
            Opacity = 100;
            Show();
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            BringToFront();
            _phaseShift = false;
        }

        public void Log_Alert(string id, string disc)
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
        public void Log_Invasion(string id, string disc)
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
    }
}
