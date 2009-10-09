using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;

namespace DescriptionExtractor
{
    public partial class MainWindow : Form
    {
        Bitmap image;
        ZernikeDesc zernike;
        FeatureVector featureVector;

        public MainWindow()
        {
            InitializeComponent();            
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            /////////////////////////////////////////////////////////////////////////////////
            // Process input
            /////////////////////////////////////////////////////////////////////////////////
            String[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                MessageBox.Show("No input image(s) provided!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            //
            String filename = args[1];
            if(!File.Exists(filename))
            {
                MessageBox.Show("Input image(s) does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            //
            try
            {
                image = new Bitmap(filename);
                imageBox.Image = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reding image(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            /////////////////////////////////////////////////////////////////////////////////
            // Extract features
            /////////////////////////////////////////////////////////////////////////////////
            featureVector = new FeatureVector();
            zernike = new ZernikeDesc(image);
            featureVector.zernike = zernike.Process();


            /////////////////////////////////////////////////////////////////////////////////
            // Save to file
            /////////////////////////////////////////////////////////////////////////////////
            // Serialization
            XmlSerializer s = new XmlSerializer(typeof(FeatureVector));
            FileInfo fileinfo = new FileInfo(filename);
            TextWriter w = new StreamWriter(fileinfo.Directory + @"\features.xml");
            s.Serialize(w, featureVector);
            w.Close();
        }
    }
}
