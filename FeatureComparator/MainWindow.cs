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
        SortedList<string, LightFieldSet> lfs;
        double[,] bins;
        int bins_amount;
        string[] args;

        public MainWindow(string[] arg)
        {
            args = arg;
            serializer = new XmlSerializer(typeof(LightFieldSet));
            lfs = new SortedList<string, LightFieldSet>();
            bins_amount = 10;
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
			// FeatureCompareState is a struct, hence passed by value by default
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

                Comparator cmp = new Comparator(lfd0, lfd1, 4.0f, 1.0f);
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
            int bins_disperse = (int)Math.Ceiling((double)(reader.dirs.Count) / bins_amount);
            bins = new double[reader.dirs.Count, bins_disperse];

            //////////////////////////////////////////////////////////////////////
            // Walk through every model in the directory and store streamfile
            //////////////////////////////////////////////////////////////////////

            foreach (KeyValuePair<string, string> kvp in reader.dirs)
            {
                if (!File.Exists(kvp.Key + "/features.xml"))
                {
                    MessageBox.Show("Error reading feature file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                TextReader r = new StreamReader(kvp.Key + "/features.xml");
                lfs.Add(kvp.Key, (LightFieldSet)serializer.Deserialize(r));
                r.Close();
            }

            //////////////////////////////////////////////////////////////////////
            // Compute distances
            //////////////////////////////////////////////////////////////////////

            foreach (KeyValuePair<string, string> kvp in reader.dirs)
            {
                // Load preview bmp
                try
                {
                    image = new Bitmap(kvp.Key + "/" + kvp.Value + "_LF0_IMG0.png");
                }
                catch (Exception)
                {
                    MessageBox.Show("Error reading image(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                // Distances
                Distance[] distances = new Distance[reader.dirs.Count];
                int iterator_distance = 0;

                //////////////////////////////////////////////////////////////////////
                // Then walk through all batches, split because max 64 events threads
                //////////////////////////////////////////////////////////////////////

                for (int z = 0; z < reader.batches.Length; z++)
                {
                    // Create thread pool
                    ThreadPool.SetMaxThreads(Environment.ProcessorCount + 1, Environment.ProcessorCount + 1);
                    ManualResetEvent[] events = new ManualResetEvent[reader.batches[z].Count];
                    int iterator_threads = 0;

                    // Walk through every dir
                    foreach (string dirname in reader.batches[z].Keys)
                    {
                        // Insert handle for thread
                        events[iterator_threads] = new ManualResetEvent(false);
                        distances[iterator_distance] = new Distance();
                        FeatureCompareState prstate = new FeatureCompareState(distances[iterator_distance], lfs[kvp.Key], lfs[dirname], reader.original[dirname], events[iterator_threads]);
                        WaitCallback async = new WaitCallback(this.ComputeDistance);
                        ThreadPool.QueueUserWorkItem(async, prstate);

                        iterator_threads++;
                        iterator_distance++;
                    }

                    // Thread barrier sync
                    WaitHandle.WaitAll(events);
                }

                //////////////////////////////////////////////////////////////////////
                // Sort distances
                //////////////////////////////////////////////////////////////////////

                SortedList<double, string> store = new SortedList<double, string>();
                TextWriter tw = new StreamWriter(kvp.Key + "/" + kvp.Value + ".dist");

                for (int z = 0; z < distances.Length; z++)
                {
                    if (!store.ContainsKey(distances[z].value))
                    {
                        tw.WriteLine(String.Format("{0:0.0000000000000}", distances[z].value) + "\t" + distances[z].name);

                        store.Add(distances[z].value, distances[z].name);
                    }
                }

                //////////////////////////////////////////////////////////////////////
                // Write distance to file
                //////////////////////////////////////////////////////////////////////

                foreach (KeyValuePair<double, string> str in store)
                {
                    // tw.WriteLine(String.Format("{0:0.0000000000000}",str.Key) + "\t" + str.Value);
                }

                tw.Close();

                //if (reader.categories[kvp.Key] == "AIRCRAFT")

                //////////////////////////////////////////////////////////////////////
                // Compute performance statistics
                //////////////////////////////////////////////////////////////////////

                double percentage = 100 / bins_amount;
                int g = 0;

                foreach (KeyValuePair<double, string> str in store)
                {
                    if (reader.categories[reader.reversed[str.Value]] == reader.categories[kvp.Key])
                    {
                        bins[progress, (int)Math.Floor((double)g / (double)bins_amount)] += percentage;
                    }

                    g++;
                }

                //////////////////////////////////////////////////////////////////////
                // Write performance stats to file
                //////////////////////////////////////////////////////////////////////

                TextWriter sw = new StreamWriter(reader.directoryname + "/performance.txt");
                int[] bins_total = new int[bins_disperse];

                for (int r = 0; r < reader.dirs.Count; r++)
                {
                    for (int t = 0; t < bins_disperse; t++)
                    {
                        if (bins[r, t] != 0)
                        {
                            bins_total[t] += (int)bins[r, t];
                        }
                        sw.Write(bins[r, t] + " ");
                    }

                    sw.Write(sw.NewLine);
                }

                for (int a = 0; a < bins_disperse; a++)
                {
                    sw.Write((int)(bins_total[a] / (progress + 1)) + " ");
                }

                sw.Close();

                //////////////////////////////////////////////////////////////////////
                // Progress reporting
                //////////////////////////////////////////////////////////////////////

                // Write double to file
                progress++;

                // YES SIR .. Reporting for progress!
                featureComparatorWorker.ReportProgress((int)(progress / ((double)reader.dirs.Count / 100)), image);
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
