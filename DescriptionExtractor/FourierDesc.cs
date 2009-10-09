using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DescriptionExtractor
{
    public class FourierDesc
    {
        private Bitmap bitmap_;
        private Bitmap newbitmap_;
        private Color shapeColor;
        private int start_x_;
        private int start_y_;
        private int[] xcors;
        private int[] ycors;

        // Constructor

        public FourierDesc(Bitmap bitmap)
        {
            shapeColor = Color.Black;
            bitmap_ = bitmap;
            newbitmap_ = new Bitmap(bitmap.Height, bitmap.Width);
            xcors = new int[8] { 1, 1, 1, 0, -1, -1, -1, 0 };
            ycors = new int[8] { -1, 0, 1, 1, 1, 0, -1, -1 };
        }

        // Process bitmap

        public void Process()
        {
            FindBoundary();
            TraceBoundary();
            bitmap_.Save("C:\\result.bmp");
            newbitmap_.Save("C:\\result2.bmp");
        }

        // Traces boundary of bitmap

        private void TraceBoundary()
        {
            // Bootstrap Boundary tracing
            int prev_x = 0;
            int prev_y = 0;
            bitmap_.SetPixel(start_x_, start_y_, Color.Green);
            newbitmap_.SetPixel(start_x_, start_y_, Color.Black);
            int current_x = start_x_;
            int current_y = start_y_;
            int[] next = new int[2];
            
            // Trace pixel
            for (int z = 0; z < 2000; z++)
            {
                next[0] = 0; next[1] = 0;
                NextPixel(ref next, prev_x, prev_y, current_x, current_y);
                prev_x = current_x;
                prev_y = current_y;
                current_x = next[0];
                current_y = next[1];

                if (current_x != 0 && current_y != 0)
                {
                    bitmap_.SetPixel(prev_x, prev_y, Color.Green);
                    newbitmap_.SetPixel(prev_x, prev_y, Color.Black);
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

            if (dir == 2)
            {
                int i = 0;
            }

            AroundPixel(ref next, current_x, current_y, 7);
        }

        // Look around pixel for red color

        private void AroundPixel(ref int[] next, int x, int y, int dir)
        {
            int[] result = new int[2];
            int lookup = 0;

            for (int z = dir; z < dir + 8; z++)
            {
                lookup = z % 8;

                if (IsColor(bitmap_.GetPixel(x + xcors[lookup], y + ycors[lookup]),Color.Red))
                {
                    next[0] = x + xcors[lookup];
                    next[1] = y + ycors[lookup];
                    return;
                }
                else if (IsColor(bitmap_.GetPixel(x + xcors[lookup], y + ycors[lookup]), Color.Green) &&
                         result[0] == 0 && result[1] == 0)
                {
                    result[0] = x + xcors[lookup];
                    result[1] = y + ycors[lookup];
                }
            }

            next = result;
        }

        // Find boundary of bitmap

        private void FindBoundary()
        {
            // Horizontal processing
            for (int y = 0; y < bitmap_.Height - 1; y++)
            {
                for (int x = 0; x < bitmap_.Width - 1; x++)
                {
                    ProcessPixel(x, y, x+1, y);
                }
            }
            
            // Vertical processing
            for (int y = 0; y < bitmap_.Width - 1; y++)
            {
                for (int x = 0; x < bitmap_.Height - 1; x++)
                {
                    ProcessPixel(y, x, y, x+1);
                }
            }
        }

        // Process pixel

        private void ProcessPixel(int x1, int y1, int x2, int y2)
        {
            Color c1 = bitmap_.GetPixel(x1, y1);
            Color c2 = bitmap_.GetPixel(x2, y2);

            if (!c1.Equals(c2))
            {
                // Get grayvalue of surface
                if (shapeColor.Equals(Color.Black))
                {
                    if (!IsColor(c1, Color.White))
                    {
                        start_x_ = x1;
                        start_y_ = y1;
                        shapeColor = c2;
                    } 
                    else if (!IsColor(c2, Color.White))
                    {
                        start_x_ = x2;
                        start_y_ = y2;
                        shapeColor = c2;
                    }
                }

                // Draw borders
                if (IsColor(c1, Color.White) && (IsColor(c2, shapeColor) || IsColor(c2, Color.Red)))
                {
                    bitmap_.SetPixel(x2, y2, Color.Red);
                }
                else if ((IsColor(c1, shapeColor) || IsColor(c2, Color.Red)) && IsColor(c2, Color.White))
                {
                    bitmap_.SetPixel(x1, y1, Color.Red);
                }
            }
        }

        private bool IsColor(Color c, Color d)
        {
            return (c.R == d.R && c.G == d.G && c.B == d.B);
        }  
    }
}
