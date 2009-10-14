using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace DescriptionExtractor
{
    public class ZernikeDesc
    {       
        //static Dictionary<Pair<int, int>, Polynomial> zernikePolynomials;
        static Dictionary<String, Polynomial> zernikePolynomials = new Dictionary<string,Polynomial>();
        protected int[,] bmap;  // Memory if fastest when word addressable anyhow...
        protected int N;

        public ZernikeDesc(Bitmap bitmap)
        {
            if(bitmap.Height != bitmap.Width)
                throw new InvalidOperationException("Bitmap not rectangular!");
            N = bitmap.Height;
            //
            bmap = new int[N, N];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int bit;
                    if (y < 0 || y > bitmap.Height || x < 0 || y > bitmap.Width)
                    {
                        bit = 0;
                    }
                    else
                    {
                        Color c = bitmap.GetPixel(x, y);
                        if ((c.A + c.R + c.G + c.B) == 1020)            // White is background
                            bit = 0;
                        else
                            bit = 1;
                    }
                    bmap[x, y] = bit;
                }
            }
        }

        private static double Factorial(int n)
        {
            double f = 1;
            for (int i = 2; i <= n; i++)
                f *= i;
            //
            return f;
        }

        private static int abs(int a)
        {
            if (a < 0)
                return -a;
            return a;
        }

        private int I(int x, int y)
        {
            return bmap[x, y];
        }

        private static Polynomial RadialPolynomial(int n, int m)
        {
            if (n == 0)
                return new Polynomial(new double[] { 1.0 });
            //
            double[] coeffs = new double[n + 1];
            //
            for (int s = 0; s <= (n - abs(m)) / 2; s++)
            {
                double nMinusCFact = Factorial(n - s);
                double sFact = Factorial(s);
                double term0 = Factorial((n + abs(m)) / 2 - s);
                double term1 = Factorial((n - abs(m)) / 2 - s);
                double c = nMinusCFact / (sFact * term0 * term1);
                c *= Math.Pow(-1, s);
                //
                coeffs[2 * s] = c;
                if (2 * s + 1 < n + 1)
                    coeffs[2 * s + 1] = 0.0;
            }
            Polynomial radialPoly = new Polynomial(coeffs);
            return radialPoly;
        }


        private static double RadialPolynomial(int n, int m, double rho)
        {
            String key = "" + m + n;
            Polynomial poly;
            if(zernikePolynomials.ContainsKey(key))
            {
                poly = zernikePolynomials[key];
            }
            else
            {
                poly = RadialPolynomial(n, m);
                zernikePolynomials[key] = poly;
#if DEBUG
                Console.WriteLine("Polynomial R(" + n + "," + m + "): " + poly + " added to cache.");
#endif
            }
            //
            return poly.Value(rho);
        }

		private Complex V(int n, int m, double rho, double theta)
		{
		}

        private Complex ZernikeMoment(int n, int m)
        {
            double zr = 0;                  // Real part
            double zi = 0;                  // Imaginary part
            double cnt = 0;                 // TODO: The normalization value is subject to change
            //
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    double xn = 2*x-N+1;        // Normalized x
                    double yn = N-1-2*y;        // Normalized y
                    double rho = Math.Sqrt( xn * xn + yn * yn ) / N;        // Go polar, Rho
                    if (rho <= 1.0)
                    {
                        double radial = RadialPolynomial(n, m, rho);
                        double theta = Math.Atan(yn / xn);                  // Go polar, Theta
                        zr += I(x, y) * radial * Math.Cos(m * theta);
                        zi += I(x, y) * radial * Math.Sin(m * theta);
                        cnt++;
                    }                    
                }
            }
            //
            Complex zm = new Complex(zr, zi);
            //zm = zm.Conjugate;
            return zm * (n + 1) / cnt;
            //return zm * (n + 1) / Math.PI;
        }

        private Bitmap Reconstruct(Complex[] Z)
        {
            Bitmap bmp = new Bitmap(N, N);

            double zr = 0;      // Real part
            double zi = 0;      // Imaginary part
            //
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    double xn = 2 * x - N + 1;                          // Normalized x
                    double yn = N - 1 - 2 * y;                          // Normalized y
                    double rho = Math.Sqrt(xn * xn + yn * yn) / N;      // Go polar, Rho
                    double theta = Math.Atan(yn / xn);                  // Go polar, Theta
                    if (rho <= 1.0)
                    {
                        int i = 0;
                        for (int n = 0; n <= 10; n++)
                        {
                            for (int m = -n; m <= n; m += 2)
                            {
                                double radial = RadialPolynomial(n, m, rho);
                                zr = radial * Math.Cos(m * theta);
                                zi = radial * Math.Sin(m * theta);
                                Complex vnm = new Complex(zr, zi);
                                Complex znm = Z[i++];
                                Complex result = znm * vnm;
                                //
                                Color c = Color.FromArgb(
                                    255,
                                    (int)(result.Modulus * 255 + 127),
                                    (int)(result.Real * 255 + 127),
                                    (int)(result.Imag * 255 + 127));                              
                                bmp.SetPixel(x, y, c);
                            }
                        }                        
                    }
                }
            }

            return bmp;
        }

        static int c = 0;
        public double[] Process()
        {                         
            double[] coef = new double[100];
            //
            Complex[] Z = new Complex[100];
            int i = 0;
            for (int n = 0; n <= 10; n++)
            {
                for (int m = -n; m <= n; m += 2)
                {                    
                    Z[i] = ZernikeMoment(n, m).Conjugate;
                    coef[i] = Z[i].Modulus;
                    i++;
                }
            }
            
            Bitmap bmp = Reconstruct(Z);
            bmp.Save("zernike" + c++ + ".png"); 
                       
            return coef;
        }
    }
}
