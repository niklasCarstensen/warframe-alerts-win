﻿using System;
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
        bool windowVisible;

        public bool GameDetection = true;
        List<MaterialListView> Tabs = new List<MaterialListView>();
        List<Label> TabLabels = new List<Label>();
        public delegate void Updater(int index);
        List<Tuple<int, Updater>> TimerUpdates = new List<Tuple<int, Updater>>();

        // Startup
        public MainForm()
        {
            InitializeComponent();

            #region Tabs
            Tabs.Add(FissureData);
            TabLabels.Add(label1);
            TabLabels.Add(label2);
            TabLabels.Add(label3);
            TabLabels.Add(label4);
            while (TabLabels.Count > Tabs.Count)
            {
                MaterialListView view = new MaterialListView();
                for (int i = 0; i < FissureData.Columns.Count; i++)
                    view.Columns.Add((ColumnHeader)FissureData.Columns[i].Clone());
                view.SetBounds(FissureData.Location.X, FissureData.Location.Y, FissureData.Width, FissureData.Height);

                Controls.Add(view);
                Tabs.Add(view);
            }
            FissureData.BringToFront();
            for (int i = 0; i < TabLabels.Count; i++)
                TabLabels[i].Click += (object sender, EventArgs e) => {
                    this.InvokeIfRequired(() => {
                        try {
                            Tabs[TabLabels.IndexOf((Label)sender)].BringToFront();

                            foreach (Label x in TabLabels)
                                if (x == sender) x.ForeColor = Color.White;
                                else x.ForeColor = Color.DarkGray;
                        } catch { }
                    });
                };
            #endregion

            var views = Controls.OfType<MaterialListView>();
            foreach (MaterialListView view in views)
                view.ShowItemToolTips = true;

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

            IEnumerable<MaterialListView> views = Controls.OfType<MaterialListView>();
            foreach (MaterialListView view in views)
                view.ColumnWidthChanging += new ColumnWidthChangingEventHandler(StopColumnWidthChanging);
        }
        
        // Update
        public void WFupdate()
        {
            lock (this)
            {
                GUItimerUpdateTimer.Stop();
                string remotingMessage = "";

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
                    string interrestingItems = "";
                    foreach (VoidTraderItem item in WarframeHandler.worldState.WS_VoidTrader.Inventory)
                    {
                        if (FilterRewards(item.Item))
                            interrestingItems += item.Item + " - " + item.Credits + "c " + item.Ducats + "D\n";
                        items += item.Item + " _ " + item.Credits + "c " + item.Ducats + "D\n";
                    }
                    
                    Notify("Void-Trader", "Void trader arrived at " + WarframeHandler.worldState.WS_VoidTrader.Location + " with: \n" +
                        (interrestingItems == "" ? "Primed Disappointment" : interrestingItems) + "\nHe will leave again at " + WarframeHandler.worldState.WS_VoidTrader.EndTime, 1000);
#if !DEBUG
                    client.setMessage("Void-Traderↅ" + "Void trader arrived at " + WarframeHandler.worldState.WS_VoidTrader.Location + " with: \n" +
                        items + "\nHe will leave again at " + WarframeHandler.worldState.WS_VoidTrader.EndTime);
#endif
                }
                config.Data.VoidTraderArrived = WarframeHandler.worldState.WS_VoidTrader.Inventory.Count != 0;
                #endregion

                #region Notify Syndicate Rewards
                string notification = "";
                foreach (SyndicateMission mission in WarframeHandler.worldState.WS_SyndicateMissions)
                    for (int i = 0; i < mission.jobs.Count; i++)
                        if (mission.jobs[i].rewardPool != null && !config.Data.idList.Contains(mission.jobs[i].id))
                        {
                            config.Data.idList.Add(mission.jobs[i].id);
                            foreach (string reward in mission.jobs[i].rewardPool)
                            {
                                string message = reward + " currently available from the " + mission.Syndicate + "'s " + (i + 1) + ". bounty until " + mission.EndTime.ToLocalTime().ToLongTimeString() + "\n";
                                if (FilterRewards(reward))
                                    notification += message;
                                remotingMessage += message;
                            }
                        }
                Notify("Update", notification, 1000);
                #endregion

                #region Notify Alerts and Invasions
                notification = "";
                foreach (Alert a in WarframeHandler.worldState.WS_Alerts)
                    if (!config.Data.idList.Contains(a.Id))
                    {
                        config.Data.idList.Add(a.Id);
                        string message = a.ToTitle() + " - Expires at " + a.EndTime.ToLocalTime().ToLongTimeString() + ", so in " + (int)(a.EndTime.ToLocalTime() - DateTime.Now).TotalMinutes + " minutes\n";
                        if (FilterRewards(a.Mission.Reward.ToTitle()))
                            notification += message;
                        remotingMessage += message;
                    }
                foreach (Invasion i in WarframeHandler.worldState.WS_Invasions)
                    if (!config.Data.idList.Contains(i.Id) && !i.IsCompleted)
                    {
                        config.Data.idList.Add(i.Id);
                        if (FilterRewards(i.AttackerReward.ToTitle() + i.DefenderReward.ToTitle()))
                            notification += i.ToTitle() + "\n";
                        remotingMessage += i.ToTitle() + "\n";
                    }
                Notify("Update", notification, 1000);
                #endregion

#if !DEBUG
                if (!string.IsNullOrWhiteSpace(remotingMessage))
                    client.setMessage("Updateↅ" + remotingMessage);
                client.setMessage("StateↅUpdated at: " + DateTime.Now.ToLongTimeString() + "\n\n" +
                    "Cetus: " + WarframeHandler.worldState.WS_CetusCycle.TimeOfDay() + " " + (WarframeHandler.worldState.WS_CetusCycle.Expiry.ToLocalTime() - DateTime.Now).ToReadable() + "\n" +
                    "Fortuna: " + WarframeHandler.worldState.WS_FortunaCycle.Temerature() + " " + (WarframeHandler.worldState.WS_FortunaCycle.Expiry.ToLocalTime() - DateTime.Now).ToReadable() + "\n" +
                    "\nFissures:\n" +
                    WarframeHandler.worldState.WS_Fissures.OrderBy(x => x.TierNumber).Select(f => f.Tier + " - " + f.MissionType + " - " + (f.EndTime.ToLocalTime() - DateTime.Now).ToReadable())
                                                                                     .Aggregate((x, y) => x + "\n" + y));
#endif

                while (config.Data.idList.Count > 250)
                    config.Data.idList.RemoveAt(0);

                UpdateGUI();

                GUItimerUpdateTimer.Enabled = true;
                GUItimerUpdateTimer.Start();
            }
        }
        void Notify(string title, string text, int timeout)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(title) || timeout < 1)
                return;
            
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
        void UpdateGUI()
        {
            lock (TimerUpdates)
            {
                TimerUpdates.Clear();
            }

            this.InvokeIfRequired(() => {
                try {
                    var views = Controls.OfType<MaterialListView>();
                    foreach (MaterialListView view in views)
                        view.Items.Clear();

                    Alert[] alerts = WarframeHandler.worldState.WS_Alerts.OrderBy(x => (x.EndTime.ToLocalTime() - DateTime.Now)).ToArray();
                    foreach (Alert a in alerts)
                    {
                        AlertData.Items.Add(new ListViewItem(new string[] { a.Mission.Type, a.ToTitle(), a.Mission.Faction, (a.EndTime.ToLocalTime() - DateTime.Now).ToReadable() + " ▾" }));
                        TimerUpdates.Add(new Tuple<int, Updater>(AlertData.Items.Count - 1,
                            new Updater(i => AlertData.Items[i].SubItems[3].Text = (a.EndTime.ToLocalTime() - DateTime.Now).ToReadable() + " ▾")));
                    }

                    Invasion[] invasions = WarframeHandler.worldState.WS_Invasions.OrderBy(x => -Math.Abs(x.Completion - 50)).Where(x => !x.IsCompleted).ToArray();
                    foreach (Invasion inv in invasions)
                    {
                        InvasionData.Items.Add(new ListViewItem(new string[] { "", inv.ToTitle(), Math.Round(inv.Completion, 2) + "%",
                            (DateTime.Now - inv.StartTime.ToLocalTime()).ToReadable() + " ▴" }));
                        TimerUpdates.Add(new Tuple<int, Updater>(InvasionData.Items.Count - 1,
                            new Updater(j => InvasionData.Items[j].SubItems[3].Text = (DateTime.Now - inv.StartTime.ToLocalTime()).ToReadable() + " ▴")));
                    }

                    IOrderedEnumerable<Fissure> fissures = WarframeHandler.worldState.WS_Fissures.OrderBy(x => x.TierNumber);
                    foreach (Fissure f in fissures)
                    {
                        FissureData.Items.Add(new ListViewItem(new string[] { f.Tier + " - " + f.MissionType + " - " + (f.EndTime.ToLocalTime() - DateTime.Now).ToReadable() + " ▾" }));
                        TimerUpdates.Add(new Tuple<int, Updater>(FissureData.Items.Count - 1,
                            new Updater(i => FissureData.Items[i].SubItems[0].Text = f.Tier + " - " + f.MissionType + " - " + (f.EndTime.ToLocalTime() - DateTime.Now).ToReadable() + " ▾")));
                    }

                    // Void Trader
                    if (!config.Data.VoidTraderArrived)
                        Tabs[1].Items.Add("Baro will be back at " + WarframeHandler.worldState.WS_VoidTrader.StartTime.ToLocalTime());
                    else
                        Tabs[1].Items.Add("Baro is here until " + WarframeHandler.worldState.WS_VoidTrader.EndTime.ToLocalTime());
                    foreach (VoidTraderItem i in WarframeHandler.worldState.WS_VoidTrader.Inventory)
                        Tabs[1].Items.Add(new ListViewItem(new string[] { i.Item + "\t" + i.Credits + "c\t" + i.Ducats + "D" }));

                    // Cetus
                    SyndicateMission ostrons = WarframeHandler.worldState.WS_SyndicateMissions.Find(x => x.Syndicate == "Ostrons");
                    Tabs[2].Items.Add(new ListViewItem(new string[] { "" }));
                    TimerUpdates.Add(new Tuple<int, Updater>(0, new Updater(i => Tabs[2].Items[i].SubItems[0].Text = "Changes at " + ostrons.EndTime.ToLocalTime().ToLongTimeString() +
                        " - " + WarframeHandler.worldState.WS_CetusCycle.TimeOfDay() + " " + (WarframeHandler.worldState.WS_CetusCycle.Expiry.ToLocalTime() - DateTime.Now).ToReadable())));
                    for (int j = 0; j < ostrons.jobs.Count; j++)
                    {
                        SyndicateJob job = ostrons.jobs[j];
                        Tabs[2].Items.Add(new ListViewItem(new string[] { (j + 1) + ". Bounty: (" + job.standingStages.Sum() + " Standing)" }));
                        for (int i = 0; i < job.rewardPool.Count; i += 2)
                            Tabs[2].Items.Add(new ListViewItem(new string[] { job.rewardPool[i] + (job.rewardPool.Count > i + 1 ? ", " + job.rewardPool[i + 1] : "") }));
                        Tabs[2].Items.Add(new ListViewItem(new string[] { }));
                    }
                    Tabs[2].Items.RemoveAt(Tabs[2].Items.Count - 1);

                    // 4tuna
                    SyndicateMission tuna = WarframeHandler.worldState.WS_SyndicateMissions.Find(x => x.Syndicate == "Solaris United");
                    Tabs[3].Items.Add(new ListViewItem(new string[] { "" }));
                    TimerUpdates.Add(new Tuple<int, Updater>(0, new Updater(i => Tabs[3].Items[i].SubItems[0].Text = "Changes at " + tuna.EndTime.ToLocalTime().ToLongTimeString() +
                        " - " + WarframeHandler.worldState.WS_FortunaCycle.Temerature() + " " + (WarframeHandler.worldState.WS_FortunaCycle.Expiry.ToLocalTime() - DateTime.Now).ToReadable())));
                    for (int j = 0; j < tuna.jobs.Count; j++)
                    {
                        SyndicateJob job = tuna.jobs[j];
                        Tabs[3].Items.Add(new ListViewItem(new string[] { (j + 1) + ". Bounty: (" + job.standingStages.Sum() + " Standing)" }));
                        for (int i = 0; i < job.rewardPool.Count; i += 2)
                            Tabs[3].Items.Add(new ListViewItem(new string[] { job.rewardPool[i] + (job.rewardPool.Count > i + 1 ? ", " + job.rewardPool[i + 1] : "") }));
                        Tabs[3].Items.Add(new ListViewItem(new string[] { }));
                    }
                    Tabs[3].Items.RemoveAt(Tabs[3].Items.Count - 1);

                    foreach (MaterialListView view in views)
                        view.Fix();
                } catch { }
            });
        }
        void UpdateGUITimers()
        {
            if (windowVisible)
            {
                lock (TimerUpdates)
                {
                    this.InvokeIfRequired(() =>
                    {
                        foreach (Tuple<int, Updater> a in TimerUpdates)
                            try
                            {
                                a.Item2.Invoke(a.Item1);
                            }
                            catch { }
                    });
                }
            }
        }

        // Forms Events
        private void Update_Click(object sender, EventArgs e)
        {
            WFupdate();
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
        private void GUIupdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateGUITimers();
        }
        private void StopColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (sender.GetType() == typeof(MaterialListView))
            {
                e.Cancel = true;
                e.NewWidth = (sender as MaterialListView).Columns[e.ColumnIndex].Width;
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
            windowVisible = false;
        }
        private void ShowForm()
        {
            phaseShift = true;
            this.ForceShow();
            WindowState = FormWindowState.Normal;
            phaseShift = false;
            windowVisible = true;
        }
    }
}
