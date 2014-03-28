using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TerrainImporter.Forms
{
    public partial class TerrainImporterProgress : Form
    {
        private DateTime startTime;
        private int lastUpdate;
        private int refreshRate;

        public TerrainImporterProgress()
        {
            InitializeComponent();
            this.startTime = DateTime.Now;
        }

        public int progress
        {
            get
            {
                return this.progressMeter.Value;
            }
            set
            {
                this.progressMeter.Value = value;
                this.updateTimeRemaining();
            }
        }

        public int Maximum
        {
            get
            {
                return this.progressMeter.Maximum;
            }
            set
            {
                this.progressMeter.Maximum = value;
                this.refreshRate = value / 100;
            }
        }

        public void increment(int incValue)
        {
            this.progressMeter.Increment(incValue);
            this.updateTimeRemaining();
        }

        private void updateTimeRemaining()
        {
            if (this.progressMeter.Value <= 0 || this.progressMeter.Value - this.lastUpdate <= this.refreshRate)
            {
                return;
            }
            Label label = this.timeRemainingLabel;
            string str1 = "Time Remaining: ";
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now.Subtract(this.startTime);
            timeSpan = new TimeSpan(Convert.ToInt64(Math.Round((double)Convert.ToSingle(timeSpan.Ticks) / ((double)Convert.ToSingle(this.progressMeter.Value) / (double)Convert.ToSingle(this.progressMeter.Maximum)))));
            TimeSpan local = @timeSpan;
            now = DateTime.Now;
            TimeSpan ts = now.Subtract(this.startTime);
            timeSpan = (local).Subtract(ts);
            string str2 = timeSpan.ToString();
            string str3 = str1 + str2;
            label.Text = str3;
            this.timeRemainingLabel.Refresh();
            this.lastUpdate = this.progressMeter.Value;
        }
    }
}
