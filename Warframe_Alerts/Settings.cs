using System;
using System.Windows.Forms;

namespace Warframe_Alerts
{
    public partial class Settings : Form
    {
        private readonly MainForm parent;
        private string[] recommendedFilters = new string[] { "(Blueprint)", "Catalyst", "Reactor", "cr", "ENDO", "Nitain" };

        public Settings(MainForm mw)
        {
            parent = mw;
            InitializeComponent();
            textBoxInterval.Text = (config.Data.UpdateInterval / (60 * 1000)).ToString();
        }
        private void Settings_Load(object sender, EventArgs e)
        {
            UpdateFilters();
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            parent.InvokeIfRequired(() => { parent.WF_Update(); });
        }

        private void addFilter_Click(object sender, EventArgs e)
        {
            if (config.Data.Filters.Contains(comboBox1.Text))
            {
                MessageBox.Show("I cant add what I already have...");
            }
            else
            {
                config.Data.Filters.Add(comboBox1.Text);
                comboBox1.Text = "";
                config.Save();

                UpdateFilters();
            }
        }
        private void removeFilter_Click(object sender, EventArgs e)
        {
            if (config.Data.Filters.Contains(comboBox1.Text))
            {
                config.Data.Filters.Remove(comboBox1.Text);
                comboBox1.Text = "";
                config.Save();

                UpdateFilters();
            }
            else
            {
                MessageBox.Show("I cant remove what I dont have...");
            }
        }
        private void setButton_Click(object sender, EventArgs e)
        {
            string input = textBoxInterval.Text;

            int inputToInt;
            if (!Int32.TryParse(input, out inputToInt))
            {
                string message = "Please insert a valid input";
                string caption = "Invalid Input";
                MessageBox.Show(message, caption, MessageBoxButtons.OK);
                return;
            }

            config.Data.UpdateInterval = inputToInt * 60 * 1000;
            config.Save();
        }

        private void UpdateFilters()
        {
            comboBox1.Items.Clear();
            dataGridView1.Rows.Clear();
            foreach (string s in config.Data.Filters)
            {
                dataGridView1.Rows.Add(new object[] { s });
                comboBox1.Items.Add(s);
            }
            foreach (string s in recommendedFilters)
                if (!comboBox1.Items.Contains(s))
                    comboBox1.Items.Add(s);
        }
    }
}
