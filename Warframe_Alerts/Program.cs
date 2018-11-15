using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Warframe_Alerts
{
    public struct Alert
    {
        public string ID;
        public string Title;
        public string Description;
        public string StartDate;
        public string Faction;
        public string ExpiryDate;
    }

    public struct Invasion
    {
        public string ID;
        public string Title;
        public string StartDate;
    }

    public struct Outbreak
    {
        public string ID;
        public string Title;
        public string StartDate;
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
                    if (p.Id != Process.GetCurrentProcess().Id)
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