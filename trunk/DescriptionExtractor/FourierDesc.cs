﻿using System;
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
        private Bitmap newbitmap_;
        private Color shapeColor;
        private Color boundaryColor;
        private Color backgroundColor;
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
            boundaryColor = Color.White;
            bitmap_ = bitmap;
            newbitmap_ = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            maxcount_ = 5000;
            coefficients_ = 15;
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

            //newbitmap_.Save("C:/result3.bmp");

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
                if (current_x != 0 || current_y != 0)
                {
                    newbitmap_.SetPixel(prev_x, prev_y, Color.Red);
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

                if ( isValidPixel(lookup_x, lookup_y) )
                {
                    Color c = newbitmap_.GetPixel(lookup_x, lookup_y);

                    if (IsColor(c, boundaryColor))
                    {
                        next[0] = lookup_x;
                        next[1] = lookup_y;
                        return;
                    }
                    else if (IsColor(c, Color.Red) &&
                             result[0] == 0 && result[1] == 0)
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
            FiltersSequence filter = new FiltersSequence();
            filter.Add(new Grayscale( 0.2125, 0.7154, 0.0721 ));
            filter.Add(new Threshold(125));
            filter.Add(new Closing());
            filter.Add(new Opening());
            filter.Add(new ConnectedComponentsLabeling());

            Bitmap bitmap = filter.Apply(bitmap_);

            //bitmap.Save("C:/result1.bmp");

            // We can assume that the center pixel is always filled
            shapeColor = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);

            // The background color
            backgroundColor = bitmap.GetPixel(0,0);

            // Find starting point from center
            for (int y = bitmap.Height / 2; y > 0 && start_y_ == 0; y--)
            {
                if (IsColor(bitmap.GetPixel( start_x_, y), backgroundColor))
                {
                    start_y_ = y + 1;
                }
            }

            // Horizontal processing
            for (int y = -1; y < bitmap.Height; y++)
            {
                for (int x = -1; x < bitmap.Width; x++)
                {
                    ProcessPixel(ref bitmap, x, y, x + 1, y);
                }
            }

            // Vertical processings
            for (int x = -1; x < bitmap.Width; x++)
            {
                for (int y = -1; y < bitmap.Height; y++)
                {
                    ProcessPixel(ref bitmap, x, y, x, y + 1);
                }
            }

            //newbitmap_.Save("C:/result2.bmp");
        }


        // Process pixel

        private void ProcessPixel(ref Bitmap bitmap, int x1, int y1, int x2, int y2)
        {
            Color c1 = backgroundColor;
            Color c2 = backgroundColor;

            if( isValidPixel( x1, y1) ) 
            {
                c1 = bitmap.GetPixel(x1, y1);
            } 
            if( isValidPixel( x2, y2) ) 
            {
                c2 = bitmap.GetPixel(x2, y2);
            }

            // Draw borders
            if ((!IsColor(c1, shapeColor)) && (IsColor(c2, shapeColor)))
            {
                newbitmap_.SetPixel(x2, y2, boundaryColor);
            }
            else if (IsColor(c1, shapeColor) && (!IsColor(c2, shapeColor)))
            {
                newbitmap_.SetPixel(x1, y1, boundaryColor);
            }
        }

        private bool isValidPixel(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < bitmap_.Width && y < bitmap_.Height);
        }

        // Helper functioin compares colors

        private bool IsColor(Color c, Color d)
        {
            return (c.R == d.R && c.G == d.G && c.B == d.B);
        }  
    }
}
