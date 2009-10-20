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
    public struct FeatureVector
    {
        public double[] zernike;
        public double[] fourier;
    }

	/// <summary>
	/// Lightfield desriptor, consited of view (images represented by their feature vectors)
	/// distributed on the vertices of regular dodecahedron. Stores 10 images for the 20 vertices
	/// </summary>
	public struct LightFieldDescriptor
	{
		public FeatureVector[] imageFeatures;

		public FeatureVector GetImageFeatures(int i)
		{
			int idx = 0;
			switch (idx)
			{
			case(0):
				idx = 0;
				break;
			case(1):
				idx = 0;
				break;
			case(2):
				idx = 0;
				break;
					// TODO ....
			}

			return imageFeatures[idx];
		}
	}

	/// <summary>
	/// Collection of randomly oriented rotations
	/// (pseudo random, same for all models, makes for an easier debugging)
	/// </summary>
	public struct LightFieldSet
	{
		public LightFieldDescriptor[] lightfields;
	}
}