namespace FeatureComparator
{
    partial class MainWindow
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
            this.imageBox = new System.Windows.Forms.PictureBox();
            this.featureComparatorWorker = new System.ComponentModel.BackgroundWorker();
            this.processingProgressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // imageBox
            // 
            this.imageBox.Location = new System.Drawing.Point(0, 0);
            this.imageBox.Name = "imageBox";
            this.imageBox.Size = new System.Drawing.Size(256, 256);
            this.imageBox.TabIndex = 0;
            this.imageBox.TabStop = false;
            // 
            // imageProcessWorker
            // 
            this.featureComparatorWorker.WorkerReportsProgress = true;
            this.featureComparatorWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.featureComparatorWorker_DoWork);
            this.featureComparatorWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.featureComparatorWorker_ProgressChanged);
            // 
            // processingProgressBar
            // 
            this.processingProgressBar.Location = new System.Drawing.Point(12, 262);
            this.processingProgressBar.Name = "processingProgressBar";
            this.processingProgressBar.Size = new System.Drawing.Size(232, 23);
            this.processingProgressBar.TabIndex = 1;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(256, 290);
            this.Controls.Add(this.processingProgressBar);
            this.Controls.Add(this.imageBox);
            this.Name = "MainWindow";
            this.Text = "Feature Comparator";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox imageBox;
        private System.ComponentModel.BackgroundWorker featureComparatorWorker;
        private System.Windows.Forms.ProgressBar processingProgressBar;
    }
}

