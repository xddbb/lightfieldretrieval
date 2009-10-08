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
        private Color shapeColor;

        public FourierDesc(Bitmap bitmap)
        {
            shapeColor = Color.Black;
            bitmap_ = bitmap;

            findOutline();
        }

        public float[] Process()
        {
            return null;
        }

        private void findOutline()
        {
            // Horizontal processing
            for (int j = 0; j < bitmap_.Height - 1; j++)
            {
                // Bool that stores outside state
                bool outside = true;

                for (int i = 0; i < bitmap_.Width - 1; i++)
                {
                    Color c1 = bitmap_.GetPixel(i, j);
                    Color c2 = bitmap_.GetPixel(i + 1, j);

                   outside = processPixel(c1, c2, i, j, i+1, j, outside);
                }
            }

            
            // Vertical processing
            for (int i = 0; i < bitmap_.Width - 1; i++)
            {
                // Bool that stores outside state
                bool outside = true;

                for (int j = 0; j < bitmap_.Height - 1; j++)
                {
                    Color c1 = bitmap_.GetPixel(i, j);
                    Color c2 = bitmap_.GetPixel(i, j + 1);

                    outside = processPixel(c1, c2, i, j, i, j+1, outside);
                }
            }

            bitmap_.Save("C:\\result.bmp");
        }

        private bool processPixel(Color c1, Color c2, int i1, int j1, int i2, int j2, bool outside)
        {
            if (!c1.Equals(c2))
            {
                // Get grayvalue of surface
                if (shapeColor.Equals(Color.Black))
                {
                    if (!isColor(c1, Color.White))
                    {
                        shapeColor = c2;
                    } 
                    else if (!isColor(c2, Color.White))
                    {
                        shapeColor = c2;
                    }
                }

                // Draw borders
                if (outside)
                {
                    if ((isColor(c1, Color.White) || isColor(c1, Color.Blue)) && isColor(c2, shapeColor))
                    {
                        bitmap_.SetPixel(i1, j1, Color.Blue);
                        bitmap_.SetPixel(i2, j2, Color.Red);
                        outside = false;
                    }
                }
                else
                {
                    if ((isColor(c1, shapeColor) || isColor(c1, Color.Red)) && isColor(c2, Color.White))
                    {
                        bitmap_.SetPixel(i1, j1, Color.Red);
                        bitmap_.SetPixel(i2, j2, Color.Blue);
                        outside = true;
                    }
                }
            }

            return outside;
        }

        private bool isColor(Color c, Color d)
        {
            return (c.R == d.R && c.G == d.G && c.B == d.B);
        }
        
    }
}
