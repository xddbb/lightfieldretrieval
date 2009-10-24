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
using System.Linq;
using System.Text;

namespace DescriptionExtractor
{
	/// <summary>
	/// A single feature vector intergrating both fourier and zerike coefficients
	/// thus combining silhouette and region
	/// </summary>
    public class FeatureVector
    {
        public double[] zernike;
        public double[] fourier;
    }

	/// <summary>
	/// Lightfield desriptor, consited of view (images represented by their feature vectors)
	/// distributed on the vertices of regular dodecahedron. Stores 10 images for the 20 vertices
	/// </summary>
	public class LightFieldDescriptor
	{
		public FeatureVector[] imageFeatures;

		/// <summary>
		/// Gets the image features for a given vertex on the
		/// dodecahedron.
		/// </summary>	
		public FeatureVector GetImageFeatures(int i)
		{
			int idx = -1;		// Fail if i not in range
			//
			switch (i)			// One long switch, scrolling time!
			{
			// Image 0
			case(0):
				idx = 0;
				break;
			case(1):
				idx = 0;
				break;

			// Image 1
			case (2):
				idx = 1;
				break;
			case (19):
				idx = 1;
				break;

			// Image 2
			case (3):
				idx = 2;
				break;
			case (18):
				idx = 2;
				break;

			// Image 3
			case (4):
				idx = 3;
				break;
			case (14):
				idx = 3;
				break;

			// Image 4
			case (5):
				idx = 4;
				break;
			case (13):
				idx = 4;
				break;

			// Image 5
			case (6):
				idx = 5;
				break;
			case (17):
				idx = 5;
				break;

			// Image 6
			case (7):
				idx = 6;
				break;
			case (16):
				idx = 6;
				break;

			// Image 7
			case (8):
				idx = 7;
				break;
			case (11):
				idx = 7;
				break;

			// Image 8
			case (9):
				idx = 8;
				break;
			case (10):
				idx = 8;
				break;

			// Image 9
			case (12):
				idx = 9;
				break;
			case (15):
				idx = 9;
				break;						
			}

			return imageFeatures[idx];
		}
	}

	/// <summary>
	/// Collection of randomly oriented rotations
	/// (pseudo random, same for all models, makes for an easier debugging)
	/// </summary>
	public class LightFieldSet
	{
		public LightFieldDescriptor[] lightfields;
	}
}