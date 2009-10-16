using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DescriptionExtractor
{
    public struct FeatureVector
    {
        public double[] zernike;
        public double[] fourier;
    }

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

	/*
    public struct FeatureCollection
    {
        public FeatureVector[] featureVectors;
    }
	*/
}