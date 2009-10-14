using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DescriptionExtractor
{
    public struct FeatureVector
    {
        public double[] zernike;
        public float[] fourier;
    }

    public struct FeatureCollection
    {
        public FeatureVector[] featureVectors;
    }
}