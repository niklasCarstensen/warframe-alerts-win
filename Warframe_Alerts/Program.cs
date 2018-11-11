using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Warframe_Alerts
{
    public struct Alert
    {
        public string ID;
        public string Title;
        public string Description;
        public string Start_Date;
        public string Faction;
        public string Expiry_Date;
    }

    public struct Invasion
    {
        public string ID;
        public string Title;
        public string Start_Date;
    }

    public struct Outbreak
    {
        public string ID;
        public string Title;
        public string Start_Date;
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region Check for other program instances
            try
            {
                foreach (Process p in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                    if (p.Id != Process.GetCurrentProcess().Id && p.MainModule.FileName == Process.GetCurrentProcess().MainModule.FileName)
                        return;
            } catch { return; }
            #endregion

            SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            Global.Main = new MainForm();
            Application.Run(Global.Main);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}