using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WarframeNET;

namespace Warframe_Alerts
{
    public static class Global
    {
        public static MainForm Main;
        public readonly static string CurrentExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static Random RDM = new Random();
        public static List<string> Filters;
        public static List<string> Ignorers;

        public static void UpdateFilters()
        {
            Filters = config.Data.Filters.Where(x => !x.StartsWith("-")).ToList();
            Ignorers = config.Data.Filters.Where(x => x.StartsWith("-")).Select(x => x.Remove(0, 1)).ToList();
        }

        public static void Hide(IntPtr WindowHandle) { ShowWindow(WindowHandle, 0); }
        public static void Minimize(IntPtr WindowHandle) { ShowWindow(WindowHandle, 2); }
        public static void Show(IntPtr WindowHandle) { ShowWindow(WindowHandle, 5); }

        // Imports
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Extensions
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        public static string GetEverythingBetween(this string str, string left, string right)
        {
            int leftIndex = str.IndexOf(left);
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex);

            if (leftIndex == -1 || rightIndex == -1 || leftIndex > rightIndex)
            {
                //throw new Exception("String doesnt contain left or right borders!");
                return "";
            }

            try
            {
                string re = str.Remove(0, leftIndex + left.Length);
                re = re.Remove(rightIndex - leftIndex - left.Length);
                return re;
            }
            catch
            {
                return "";
            }
        }
        public static bool StartsWith(this string str, string[] values)
        {
            foreach (string s in values)
                if (str.StartsWith(s))
                    return true;
            return false;
        }
        public static int LevenshteinDistance(this string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
        public static bool ContainsOneOf(this string str, IEnumerable<string> list)
        {
            foreach (string s in list)
                if (str.Contains(s))
                    return true;
            return false;
        }
        public static void InvokeIfRequired(this ISynchronizeInvoke obj, MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else
            {
                action();
            }
        }
        public static void ForceHide(this Form F)
        {
            Hide(F.Handle);
        }
        public static void ForceShow(this Form F)
        {
            Show(F.Handle);
        }
        public static string ToTitle(this Reward r)
        {
            List<string> inputs = new List<string> { (r.Items.Count == 0 ? "" : r.Items.Aggregate((x, y) => x + " " + y)),
                                                     (r.CountedItems.Count == 0 ? "" : r.CountedItems.Select(x => (x.Count > 1 ? x.Count + " " : "") + x.Type).Aggregate((x, y) => x + " " + y)),
                                                     (r.Credits == 0 ? "" : r.Credits + "c") };
            inputs.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            return inputs.Count == 0 ? "" : inputs.Aggregate((x, y) => (x + " - " + y));
        }
        public static string ToTitle(this Alert a)
        {
            return a.Mission.Reward.ToTitle() + " - " + a.Mission.Node;
        }
        public static string ToTitle(this Invasion inv)
        {
            return inv.AttackingFaction + "(" + inv.AttackerReward.ToTitle() + ") vs. " + inv.DefendingFaction + "(" + inv.DefenderReward.ToTitle() + ") - " + inv.Node;
        }
        public static string ToReadable(this TimeSpan t)
        {
            return string.Format("{0}{1}{2}{3}", t.Days > 0 ? t.Days + "d " : "",
                                                 t.Hours > 0 ? t.Hours + "h " : "",
                                                 t.Minutes > 0 ? t.Minutes + "m " : "",
                                                 t.Seconds > 0 ? t.Seconds + "s " : "0s ").Trim(' ');
        }
        public static void Fix(this MaterialListView v)
        {
            bool scrollbarVisible = false;
            for (int i = 0; i < v.Items.Count; i++)
                if (v.Items[i].Bounds.Bottom > v.Height)
                    scrollbarVisible = true;
            int widthSum = 0;
            for (int i = 0; i < v.Columns.Count - 1; i++)
                widthSum += v.Columns[i].Width;
            v.Columns[v.Columns.Count - 1].Width = v.Width - widthSum - (scrollbarVisible ? 17 : 0); // Scrollbar width is 16 px
        }
    }
}
