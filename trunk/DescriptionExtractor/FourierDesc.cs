using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;

namespace DescriptionExtractor
{
    public class FourierDesc
    {
        private Bitmap bitmap_;
        private int[,] bits_;
        private int maxcount_;
        private int start_x_;
        private int start_y_;
        private int[] cors_x_;
        private int[] cors_y_;
        private double boundary_sum_x_;
        private double boundary_sum_y_;
        private int[] boundary_x_;
        private int[] boundary_y_;
        private int boundary_count_;
        private Complex[] complex_;
        private double[] fourier_;
        private int coefficients_;
        
        // Constructor

        public FourierDesc(Bitmap bitmap)
        {
            bitmap_ = bitmap;
            bits_ = new int[bitmap.Width, bitmap.Height];
            maxcount_ = 5000;
            coefficients_ = 10;
            boundary_sum_x_ = 0D;
            boundary_sum_y_ = 0D;
            start_x_ = bitmap.Width / 2;
            boundary_x_ = new int[maxcount_];
            boundary_y_ = new int[maxcount_];
            cors_x_ = new int[8] { 1, 1, 1, 0, -1, -1, -1, 0 };
            cors_y_ = new int[8] { -1, 0, 1, 1, 1, 0, -1, -1 };
        }

        // Process bitmap

        public double[] Process()
        {
            FindBoundary();
            TraceBoundary();
            ComputeCentroidDistance();
            ComputeFourier();

            //toBitmap();

            double[] result;

            // Check if enough data present
            if (fourier_.Length > coefficients_)
            {
                result = new double[coefficients_];

                for (int i = 0; i < coefficients_; i++)
                {
                    result[i] = Math.Abs(fourier_[i + 1]) / Math.Abs(fourier_[0]);
                }
            }
            else
            {
                result = new double[0];
            }

            return result;
        }

        // Computes fourier descriptor
        
        private void ComputeFourier()
        {
            /*
            double[] data1 = new double[boundary_count_];
            double[] data2 = new double[boundary_count_];

            for (int i = 0; i < boundary_count_; i++)
            {
                data1[i] = Math.Sqrt(Math.Pow(complex_[i].real, 2) + Math.Pow(complex_[i].imag, 2));
            }

             */
            Fourier.FourierTransform.DFT(complex_, Fourier.FourierDirection.Forward);

            /*
            Complex[] newcomplex = new Complex[boundary_count_];

            for (int z = 0; z < boundary_count_; z++)
            {
                if (z < coefficients_)
                {
                    newcomplex[z] = complex_[z];
                }
                else
                {
                    newcomplex[z] = new Complex(0, 0);
                }
            }

            Fourier.FourierTransform.DFT(newcomplex, Fourier.FourierDirection.Backward);

            for (int i = 0; i < boundary_count_; i++)
            {
                data2[i] = Math.Sqrt(Math.Pow(newcomplex[i].real, 2) + Math.Pow(newcomplex[i].imag, 2));
            }
             */

            // Compute absolute value
            for (int i = 0; i < boundary_count_; i++)
            {
                fourier_[i] = Math.Sqrt(Math.Pow(complex_[i].real, 2) + Math.Pow(complex_[i].imag, 2));
            }

            
        }

        // Computes the center and centroid distance

        private void ComputeCentroidDistance()
        {
            complex_ = new Complex[boundary_count_];
            fourier_ = new double[boundary_count_];

            // Reposition center
            double center_x = boundary_sum_x_ / boundary_count_;
            double center_y = boundary_sum_y_ / boundary_count_;

            // Fill up complex array
            for (int i = 0; i < boundary_count_; i++)
            {
                complex_[i] = new Complex((double)(Math.Sqrt(Math.Pow(center_x - boundary_x_[i], 2) + Math.Pow(center_y - boundary_y_[i], 2))),0);
            }
        }        

        // Traces boundary of bitmap

        private void TraceBoundary()
        {
            // Bootstrap Boundary tracing
            bool finished = false;
            int prev_x = 0;
            int prev_y = 0;
            int current_x = start_x_;
            int current_y = start_y_;
            int[] next = new int[2];
            
            // Trace pixel
            for (int i = 0; i < maxcount_ && !finished; i++)
            {
                NextPixel(ref next, prev_x, prev_y, current_x, current_y);

                boundary_x_[i] = current_x;
                boundary_y_[i] = current_y;
                boundary_sum_x_ += current_x;
                boundary_sum_y_ += current_y;
                boundary_count_++;

                prev_x = current_x;
                prev_y = current_y;
                current_x = next[0];
                current_y = next[1];

                if (current_x == start_x_ && current_y == start_y_)
                {
                    finished = true;
                }
                if (current_x != 0 || current_y != 0)
                {
                    bits_[prev_x, prev_y] = 3;
                }
            }
        }

        // Find next pixel on boundary

        private void NextPixel(ref int[] next, int old_x, int old_y, int current_x, int current_y)
        {
            int dir = 7;

            if (old_x + 1 == current_x && old_y == current_y)
            {
                dir = 0;
            }
            else if(old_x + 1 == current_x && old_y + 1 == current_y)
            {
                dir = 1;
            }
            else if (old_x == current_x && old_y + 1 == current_y)
            {
                dir = 2;
            }
            else if (old_x - 1 == current_x && old_y + 1 == current_y)
            {
                dir = 3;
            }
            else if (old_x - 1 == current_x && old_y == current_y)
            {
                dir = 4;
            }
            else if (old_x - 1 == current_x && old_y - 1 == current_y)
            {
                dir = 5;
            }
            else if (old_x == current_x && old_y - 1 == current_y)
            {
                dir = 6;
            }
            else if (old_x + 1 == current_x && old_y - 1 == current_y)
            {
                dir = 7;
            }

            AroundPixel(ref next, current_x, current_y, dir);
        }

        // Look around pixel for red color

        private void AroundPixel(ref int[] next, int x, int y, int dir)
        {
            int[] result = new int[2];
            int lookup = 0;

            for (int z = dir; z < dir + 8; z++)
            {
                lookup = z % 8;
                int lookup_x = x + cors_x_[lookup];
                int lookup_y = y + cors_y_[lookup];

                if ( ValidBit(lookup_x, lookup_y) )
                {
                    int bit = bits_[lookup_x, lookup_y];

                    if (bits_[lookup_x, lookup_y] == 2)
                    {
                        next[0] = lookup_x;
                        next[1] = lookup_y;
                        return;
                    }
                    else if (bits_[lookup_x, lookup_y] == 3 && result[0] == 0 && result[1] == 0)
                    {
                        result[0] = lookup_x;
                        result[1] = lookup_y;
                    }
                }
            }

            next = result;
        }

        // Find boundary of bitmap

        private void FindBoundary()
        {
            // Apply aforge filters
            FiltersSequence filter = new FiltersSequence();
            filter.Add(new Grayscale( 0.2125, 0.7154, 0.0721 ));
            filter.Add(new Threshold(125));
            filter.Add(new Closing());
            filter.Add(new Opening());
            filter.Add(new ConnectedComponentsLabeling());

            Bitmap bitmap = filter.Apply(bitmap_);

            // Get background color
            Color backgroundColor = bitmap.GetPixel(0, 0);
            Color shapeColor = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);

            // If the picked shape color is not backgroud)
            if (shapeColor != backgroundColor)
            {
                // Find starting point from center
                for (int y = bitmap.Height / 2; y > 0 && start_y_ == 0; y--)
                {
                    if (bitmap.GetPixel(start_x_, y) == backgroundColor)
                    {
                        start_y_ = y + 1;
                    }
                }
            }
            else
            {
                // The model has a hole, iterate till first discovery of object
                for (int y = bitmap.Height / 2; y > 0 && start_y_ == 0; y--)
                {
                    Color c = bitmap.GetPixel(start_x_, y);

                    if (c != backgroundColor && shapeColor == backgroundColor) 
                    {
                        shapeColor = c;
                    }
                    if (c == backgroundColor && shapeColor != backgroundColor)
                    {
                        start_y_ = y + 1;
                    }
                }
            }

            // Put in local memory
            for (int y = 0; y < bitmap.Height - 1; y++)
            {
                for (int x = 0; x < bitmap.Width - 1; x++)
                {
                    if (bitmap.GetPixel(x, y) == shapeColor)
                    {
                        bits_[x, y] = 1;
                    }
                    else
                    {
                        bits_[x, y] = 0;
                    }
                }
            }

            // Horizontal processing
            for (int y = -1; y < bitmap.Height; y++)
            {
                for (int x = -1; x < bitmap.Width; x++)
                {
                    ProcessPixel(x, y, x + 1, y);
                }
            }

            // Vertical processings
            for (int x = -1; x < bitmap.Width; x++)
            {
                for (int y = -1; y < bitmap.Height; y++)
                {
                    ProcessPixel(x, y, x, y + 1);
                }
            }
        }

        // Tries to find boundary pixels

        private void ProcessPixel(int x1, int y1, int x2, int y2)
        {
            int c1 = 0;
            int c2 = 0;

            // Checks if bits are valid
            if (ValidBit(x1, y1))
            {
                c1 = bits_[x1, y1];
            }
            if (ValidBit(x2, y2))
            {
                c2 = bits_[x2, y2];
            }

            // Draw borders
            if (c1 == 0 && (c2 == 1 || c2 == 2))
            {
                bits_[x2, y2] = 2;
            }
            else if ((c1 == 1 || c1 == 2) && c2 == 0)
            {
                bits_[x1, y1] = 2;
            }
        }

        // Helper function to check bitmap

        private bool ValidBit(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < bitmap_.Width && y < bitmap_.Height);
        }

        // Helper function to visualize bitmap

        private void toBitmap()
        {
            Bitmap asdf = new Bitmap(bitmap_.Width, bitmap_.Height);

            for (int x = 0; x < bitmap_.Width; x++)
            {
                for (int y = 0; y < bitmap_.Height; y++)
                {
                    if(bits_[x, y] == 0) {
                        asdf.SetPixel(x , y, Color.White);
                    }
                    else if (bits_[x, y] == 1)
                    {
                        asdf.SetPixel(x , y, Color.Black);
                    }
                    else if (bits_[x, y] == 2)
                    {
                        asdf.SetPixel(x, y, Color.Red);
                    }
                    else
                    {
                        asdf.SetPixel(x, y, Color.Green);
                    }
                    
                }
            }

            asdf.Save("C:/result1.bmp");
        }
    }
}
