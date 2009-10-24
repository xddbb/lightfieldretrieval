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

namespace DescriptionExtractor
{
    public partial class MainWindow : Form
    {
        Bitmap image;
		LightFieldDescriptor lfdsc;		
        DirectoryInfo directory;
        BaseReader reader;

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
           
            String basefilename = args[1];            

            // Check if file exists
            if (!File.Exists(basefilename))
            {
                MessageBox.Show("Basenames file does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Get list of all directory and model files
            reader = new BaseReader(basefilename);
                                        
            // Run workers
            imageProcessWorker.RunWorkerAsync();
        }

		class ImageProcessState
		{
			// FeatureVector is a struct, hence passed by value by default
			public ImageProcessState(FeatureVector fv, Bitmap bmp, FileInfo file, ManualResetEvent mevent)
			{
				this.fv = fv;
				this.image = bmp;
				this.file = file;
				this.mevent = mevent;
			}

			public FeatureVector fv;
			public Bitmap image;
			public FileInfo file;
			public ManualResetEvent mevent;		// Thread sync
		}

		private void ProcessImage(Object obj)
		{
			ImageProcessState prstate = (ImageProcessState)obj;

			/////////////////////////////////////////////////////////////////////////////////
			// Extract features
			/////////////////////////////////////////////////////////////////////////////////
			ZernikeDesc zernike = new ZernikeDesc(prstate.image);
			FourierDesc fourier = new FourierDesc(prstate.image);
			//
			#if DEBUG
				Console.WriteLine("Processing image " + prstate.file);
				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start();
			#endif
			//
			// Actual processing within these methods
			prstate.fv.zernike = zernike.Process();
			prstate.fv.fourier = fourier.Process();
			//
			#if DEBUG
				stopWatch.Stop();
				// Get the elapsed time as a TimeSpan value.
				TimeSpan ts = stopWatch.Elapsed;
				// Format and display the TimeSpan value.
				string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
				Console.WriteLine("Processed image " + prstate.file + " in " + elapsedTime);
			#endif
			prstate.mevent.Set();	// Thread done!
		}
		

        private void imageProcessWorker_DoWork(object sender, DoWorkEventArgs e)
        {           

            ICollection keyCol = reader.dirs.Keys;

            foreach (string dirname in keyCol)
            {
                if (Directory.Exists(dirname))
                {
                    /////////////////////////////////////////////////////////////////////////////////
                    // Process directory
                    /////////////////////////////////////////////////////////////////////////////////
                    directory = new DirectoryInfo(dirname);
					#if DEBUG
						Console.WriteLine("Processing of directory " + dirname + " started");
						Stopwatch stopWatch = new Stopwatch();
						stopWatch.Start();
					#endif
                    FileInfo[] files = directory.GetFiles("*.bmp");


                    if (files.Length > 0)
                    {
						// Each lightfield has 10 images and images are sorted first by lightfield
						// and then by the image number in that lightfield. 
						int lfdCount = files.Length / 10;									// Number of lightfields (rotations)
						LightFieldSet lightfieldSet = new LightFieldSet();				
						lightfieldSet.lightfields = new LightFieldDescriptor[lfdCount];		// Put them in a set (array in this case)
						//
						for (int i = 0; i < lfdCount; i++)					// All lightfields in a directory
						{
							lfdsc = new LightFieldDescriptor();
							lfdsc.imageFeatures = new FeatureVector[10];

							/// Mutlithreading tools ///////////////////////////////////////////////
							int cores = Environment.ProcessorCount;
							ThreadPool.SetMaxThreads(cores + 1, cores + 1);		// Number of cores + the current thread
							// Create eveths to wait for!
							ManualResetEvent[] events = new ManualResetEvent[10];
							//////////////////////////////////////////////////////////////////////

							for (int j = 0; j < 10; j++)					// All images in a lightfield
							{
								//////////////////////////////////////////////////////////////////////
								// Read file
								//////////////////////////////////////////////////////////////////////
								FileInfo file = files[i * 10 + j];
								try
								{
									image = new Bitmap(file.FullName);
								}
								catch (Exception)
								{
									MessageBox.Show("Error reading image(s)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
									Application.Exit();
								}

								// Report progess and clone image for display
								Bitmap clone = new Bitmap(image);
								imageProcessWorker.ReportProgress( ((i * 10 + j) * 100) / files.Length, clone);

								events[j] = new ManualResetEvent(false);			// release handle for the thread
								lfdsc.imageFeatures[j] = new FeatureVector();
								ImageProcessState prstate = new ImageProcessState(lfdsc.imageFeatures[j], image, file, events[j] );

								WaitCallback async = new WaitCallback(this.ProcessImage);								
								ThreadPool.QueueUserWorkItem(async, prstate);								

								//imageProcessWorker.ReportProgress( ((i * 10 + j) * 100) / files.Length, clone );
								int g = 0;																 
							}
							WaitHandle.WaitAll(events);		// Thread barieir sync
							lightfieldSet.lightfields[i] = lfdsc;
						}

						#if DEBUG
							stopWatch.Stop();
							// Get the elapsed time as a TimeSpan value.
							TimeSpan ts = stopWatch.Elapsed;
							// Format and display the TimeSpan value.
							string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
							Console.WriteLine("Processing of directory " + dirname + " done in " + elapsedTime);
						#endif	                        


                        /////////////////////////////////////////////////////////////////////////////////
                        // Save to file
                        /////////////////////////////////////////////////////////////////////////////////
                        // Serialization
                        try
                        {
                            XmlSerializer s = new XmlSerializer(typeof(LightFieldSet));
                            TextWriter w = new StreamWriter(directory.FullName + @"\features.xml");
                            s.Serialize(w, lightfieldSet);
                            w.Close();
                        }
                        catch (Exception)
						{
							MessageBox.Show("Error saving features to disk!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); 
						}						
                    }
				
                }
            }

            // Exit
            Application.Exit();
            return;
        }

        private void imageProcessWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {            
            processingProgressBar.Value = e.ProgressPercentage;
            imageBox.Image = (Image)e.UserState;
        }
    }
}
