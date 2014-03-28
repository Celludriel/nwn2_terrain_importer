namespace TerrainImporter.Forms
{
    partial class TerrainImporterProgress
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.progressMeter = new System.Windows.Forms.ProgressBar();
            this.timeRemainingLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressMeter
            // 
            this.progressMeter.Location = new System.Drawing.Point(12, 25);
            this.progressMeter.Name = "progressMeter";
            this.progressMeter.Size = new System.Drawing.Size(268, 23);
            this.progressMeter.TabIndex = 0;
            // 
            // timeRemainingLabel
            // 
            this.timeRemainingLabel.AutoSize = true;
            this.timeRemainingLabel.Location = new System.Drawing.Point(12, 9);
            this.timeRemainingLabel.Name = "timeRemainingLabel";
            this.timeRemainingLabel.Size = new System.Drawing.Size(78, 13);
            this.timeRemainingLabel.TabIndex = 1;
            this.timeRemainingLabel.Text = "Time remaining";
            // 
            // progressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 70);
            this.Controls.Add(this.timeRemainingLabel);
            this.Controls.Add(this.progressMeter);
            this.Name = "progressDialog";
            this.Text = "Progress";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressMeter;
        private System.Windows.Forms.Label timeRemainingLabel;
    }
}