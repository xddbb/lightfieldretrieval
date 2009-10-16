using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DescriptionExtractor
{
    class Fourier
    {
        /// <summary>
        /// Fourier transformation direction
        /// </summary>
        public enum FourierDirection
        {
                Forward = 1,
                Backward = -1
        };


        /// <summary>
        /// Fourier Transformation
        /// </summary>
        public class FourierTransform
        {
            // One dimensional Discrete Fourier Transform
            public static void DFT(Complex[] data, FourierDirection direction)
            {
                int n = data.Length;
                double arg, cos, sin;
                Complex[] dst = new Complex[n];

                // for each destination element
                for (int i = 0; i < n; i++)
                {
                    dst[i] = Complex.Zero;

                    arg = -(int)direction * 2.0 * System.Math.PI * (double)i / (double)n;

                    // sum source elements
                    for (int j = 0; j < n; j++)
                    {
                        cos = System.Math.Cos(j * arg);
                        sin = System.Math.Sin(j * arg);

                        dst[i].real += (double)(data[j].real * cos - data[j].imag * sin);
                        dst[i].imag += (double)(data[j].real * sin + data[j].imag * cos);
                    }
                }

                // copy elements
                if (direction == FourierDirection.Forward)
                {
                    // devide also for forward transform
                    for (int i = 0; i < n; i++)
                    {
                        data[i].real = dst[i].real / n;
                        data[i].imag = dst[i].imag / n;
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        data[i].real = dst[i].real;
                        data[i].imag = dst[i].imag;
                    }
                }
            }
        }
    }
}
