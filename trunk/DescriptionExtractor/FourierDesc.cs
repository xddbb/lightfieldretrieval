using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using AForge.Imaging.Filters;

namespace DescriptionExtractor
{
    public class FourierDesc
    {
        private Bitmap bitmap_;
        private Color shapeColor;
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
            shapeColor = Color.Black;
            bitmap_ = bitmap;
            maxcount_ = 5000;
            coefficients_ = 60;
            boundary_sum_x_ = 0D;
            boundary_sum_y_ = 0D;
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

            //bitmap_.Save("C:/result1.bmp");

            double[] result = new double[coefficients_];

            for (int i = 0; i < coefficients_; i++)
            {
                result[i] = Math.Abs(fourier_[i + 1]) / Math.Abs(fourier_[0]);
            }

            return result;
        }

        // Computes fourier descriptor

        private void ComputeFourier()
        {
            Fourier.FourierTransform.DFT(complex_, Fourier.FourierDirection.Forward);

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
                if (current_x != 0 && current_y != 0)
                {
                    bitmap_.SetPixel(prev_x, prev_y, Color.Red);
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

                Color c = bitmap_.GetPixel(x + cors_x_[lookup], y + cors_y_[lookup]);

                if (IsColor(c, shapeColor))
                {
                    next[0] = x + cors_x_[lookup];
                    next[1] = y + cors_y_[lookup];
                    return;
                }
                else if (IsColor(c, Color.Red) &&
                         result[0] == 0 && result[1] == 0)
                {
                    result[0] = x + cors_x_[lookup];
                    result[1] = y + cors_y_[lookup];
                }
            }

            next = result;
        }

        // Find boundary of bitmap

        private void FindBoundary()
        {
            FiltersSequence filter = new FiltersSequence();
            filter.Add(new Grayscale(0.2125, 0.7154, 0.0721));
            filter.Add(new Opening());
            filter.Add(new Edges());
            filter.Add(new Invert());

            Bitmap bitmap = filter.Apply(bitmap_);

            // Horizontal processing
            for (int y = 0; y < bitmap_.Height; y++)
            {
                for (int x = 0; x < bitmap_.Width; x++)
                {
                    // Dont store edges
                    if (x > 0 && y > 0 && x < bitmap_.Width - 1 && y < bitmap_.Height - 1)
                    {
                        if (IsColor(Color.White, bitmap.GetPixel(x, y)))
                        {
                            bitmap_.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            bitmap_.SetPixel(x, y, Color.Black);
                        }
                    }
                    else
                    {
                        bitmap_.SetPixel(x, y, Color.White);
                    }

                    // Find start
                    if (start_x_ == 0 && start_y_ == 0)
                    {
                        if (IsColor(bitmap_.GetPixel(x, y), shapeColor))
                        {
                            start_x_ = x;
                            start_y_ = y;
                        }
                    }
                }
            }
        }

        // Helper functioin compares colors

        private bool IsColor(Color c, Color d)
        {
            return (c.R == d.R && c.G == d.G && c.B == d.B);
        }  
    }
}
