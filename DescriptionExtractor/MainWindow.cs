using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DescriptionExtractor
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Process input
            String[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                MessageBox.Show("No input image(s) provided!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            String filename = args[1];

            int g = 0;
        }
    }
}
