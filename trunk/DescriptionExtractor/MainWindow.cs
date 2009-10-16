#region GPL EULA
// Copyright (c) 2009, Bojan Endrovski, http://furiouspixels.blogspot.com/
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
#endregion

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
using System.Threading;
using System.Diagnostics;

namespace DescriptionExtractor
{
    public partial class MainWindow : Form
    {
        Bitmap image;
        /*
        ZernikeDesc zernike;
        FeatureVector featureVector;
        */ 
        FeatureCollection featureCollection;
        DirectoryInfo directory;

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
                MessageBox.Show("No input directory provided!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Process input directory
            String dirname = args[1];
            if(!Directory.Exists(dirname))
            {
                MessageBox.Show("Input directory does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            directory = new DirectoryInfo(dirname);
                                        
            // Run workers
            imageProcessWorker.RunWorkerAsync();
        }

        private void imageProcessWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            /////////////////////////////////////////////////////////////////////////////////
            // Process directory
            /////////////////////////////////////////////////////////////////////////////////
            FileInfo[] files = directory.GetFiles("*.bmp");
            featureCollection = new FeatureCollection();
            featureCollection.featureVectors = new FeatureVector[files.Length];
            ZernikeDesc zernike;
            FourierDesc fourier;

            int i = 0;
            foreach (FileInfo file in files)
            {
                try
                {
                    image = new Bitmap(file.FullName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error reading image(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                imageProcessWorker.ReportProgress((i * 100) / files.Length, new Bitmap(image));

                /////////////////////////////////////////////////////////////////////////////////
                // Extract features
                /////////////////////////////////////////////////////////////////////////////////
                zernike = new ZernikeDesc(image);
                fourier = new FourierDesc(image);
#if DEBUG
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
#endif
                featureCollection.featureVectors[i].zernike = zernike.Process();
                featureCollection.featureVectors[i].fourier = fourier.Process();
                i++;
#if DEBUG
                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds );
                Console.WriteLine("Processed image " + file + " in " + elapsedTime);
#endif                
            }
            imageProcessWorker.ReportProgress((i * 100) / files.Length, new Bitmap(image));

            /////////////////////////////////////////////////////////////////////////////////
            // Save to file
            /////////////////////////////////////////////////////////////////////////////////

            // Serialization
            XmlSerializer s = new XmlSerializer(typeof(FeatureCollection));
            TextWriter w = new StreamWriter(directory.FullName + @"\features.xml");
            s.Serialize(w, featureCollection);
            w.Close();

            /*
            // Deserialization
            ShoppingList newList;
            TextReader r = new StreamReader( "list.xml" );
            newList = (ShoppingList)s.Deserialize( r );
            r.Close();
            */
        }

        private void imageProcessWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {            
            processingProgressBar.Value = e.ProgressPercentage;
            imageBox.Image = (Image)e.UserState;
        }
    }
}
