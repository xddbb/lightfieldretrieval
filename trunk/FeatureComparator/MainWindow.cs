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
using System.Collections;
using DescriptionExtractor;

namespace FeatureComparator
{
    public partial class MainWindow : Form
    {
        Bitmap image;
        BaseReader reader;
        XmlSerializer serializer;
        string[] args;

        public MainWindow(string[] arg)
        {
            args = arg;
            serializer = new XmlSerializer(typeof(LightFieldSet));
            InitializeComponent();            
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            if (args.Length < 1)
            {
                MessageBox.Show("No input directory provided!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            String basefilename = args[0];

            // Check if file exists
            if (!File.Exists(basefilename))
            {
                MessageBox.Show("Basenames file does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Get list of all directory and model files, batch number added
            reader = new BaseReader(basefilename, 32);

            // Run workers
            featureComparatorWorker.RunWorkerAsync();
        }

		class FeatureCompareState
		{
			// FeatureVector is a struct, hence passed by value by default
			public FeatureCompareState(Distance distance, LightFieldSet source, LightFieldSet target, string name, ManualResetEvent mevent)
			{
				this.distance = distance;
                this.source = source;
                this.target = target;
                this.name = name;
				this.mevent = mevent;
			}

			public Distance distance;
            public LightFieldSet source;
            public LightFieldSet target;
            public string name;
			public ManualResetEvent mevent;		// Thread sync
		}

		private void ComputeDistance(Object obj)
		{
            FeatureCompareState prstate = (FeatureCompareState)obj;

            //////////////////////////////////////////////////////////////////////
            // Actual compare. Search for the optimal roataion among the lightfields
            //////////////////////////////////////////////////////////////////////

            double min = Single.PositiveInfinity;
            for (int i = 0; i < prstate.source.lightfields.Length; i++)
            {
                LightFieldDescriptor lfd0 = prstate.source.lightfields[i];
                LightFieldDescriptor lfd1 = prstate.target.lightfields[i];
                Comparator cmp = new Comparator(lfd0, lfd1, 1.0f, 20.0f);
                double dist = cmp.Compare();
                if (dist < min)
                    min = dist;
            }

            prstate.distance.value = min;
            prstate.distance.name = prstate.name;
            prstate.mevent.Set();	// Thread done!
        }

        private void featureComparatorWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int progress = 0;

            // First walk through all dirs
            foreach (DictionaryEntry de in reader.dirs)
            {
                if (!File.Exists(de.Key + "/features.xml"))
                {
                    MessageBox.Show("Error reading feature file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                // Load first xml
                TextReader r = new StreamReader(de.Key + "/features.xml");
                LightFieldSet lfs1 = (LightFieldSet)serializer.Deserialize(r);
                r.Close();

                // Load preview bmp
                try
                {
                    image = new Bitmap(de.Key + "/" + de.Value + "_LF0_IMG0.bmp");
                }
                catch (Exception)
                {
                    MessageBox.Show("Error reading image(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                // Distances
                Distance[] distances = new Distance[reader.dirs.Count];
                int iterator_distance = 0;

                // Then walk through all batches, split because max 64 events
                for (int z = 0; z < reader.batches.Length; z++)
                {
                    // Create thread pool
                    ThreadPool.SetMaxThreads(Environment.ProcessorCount + 1, Environment.ProcessorCount + 1);
                    ManualResetEvent[] events = new ManualResetEvent[reader.batches[z].Count];
                    int iterator_threads = 0;

                    // Walk through every dir
                    foreach (string dirname in reader.batches[z].Keys)
                    {
                        if (!File.Exists(dirname + "/features.xml"))
                        {
                            MessageBox.Show("Error reading image(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                        }

                        // Load second xml
                        r = new StreamReader(dirname + "/features.xml");
                        LightFieldSet lfs2 = (LightFieldSet)serializer.Deserialize(r);
                        r.Close();

                        // Release handle for the thread
                        events[iterator_threads] = new ManualResetEvent(false);
                        distances[iterator_distance] = new Distance();
                        FeatureCompareState prstate = new FeatureCompareState(distances[iterator_distance], lfs1, lfs2, reader.original[dirname], events[iterator_threads]);
                        WaitCallback async = new WaitCallback(this.ComputeDistance);
                        ThreadPool.QueueUserWorkItem(async, prstate);

                        iterator_threads++;
                        iterator_distance++;
                    }

                    // Thread barrier sync
                    WaitHandle.WaitAll(events);
                }

                // Write double to file
                progress++;

                // YES SIR .. Reporting for progress!
                featureComparatorWorker.ReportProgress((int)(progress / ((double)reader.dirs.Count / 100)) , image);

                // Sort distances
                SortedList<double, string> store = new SortedList<double, string>();

                for (int z = 0; z < distances.Length; z++)
                {
                    store.Add(distances[z].value, distances[z].name);
                }

                // Write distance to file
                TextWriter tw = new StreamWriter(de.Key + "/" + de.Value + "_dist.txt");

                foreach (KeyValuePair<double, string> kvp in store)
                {
                    tw.WriteLine(kvp.Key + "\t" + kvp.Value);
                }

                tw.Close();
            }

            // Exit
            Application.Exit();
            return;
        }

        private void featureComparatorWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {            
            processingProgressBar.Value = e.ProgressPercentage;
            imageBox.Image = (Image)e.UserState;
        }
    }
}
