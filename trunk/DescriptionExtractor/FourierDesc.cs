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

        public FourierDesc(Bitmap bitmap)
        {
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

                    processPixel(c1, c2, i, j, i+1, j, ref outside);
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

                    processPixel(c1, c2, i, j, i, j+1, ref outside);
                }
            }

            bitmap_.Save("C:\\result.bmp");
        }

        private void processPixel(Color c1, Color c2, int i, int j, int i2, int j2, ref bool outside)
        {
            if (!c1.Equals(c2))
            {
                if (outside)
                {
                    Color z = Color.White;

                    if ((isColor(c1, Color.White) || isColor(c1, Color.Blue)) && !isColor(c2, Color.White))
                    {
                        bitmap_.SetPixel(i, j, Color.Blue);
                        bitmap_.SetPixel(i2, j2, Color.Red);
                        outside = false;
                    }
                }
                else
                {
                    if (false && !isColor(c1, Color.White) && isColor(c2, Color.White))
                    {
                        bitmap_.SetPixel(i, j, Color.Red);
                        bitmap_.SetPixel(i2, j2, Color.Blue);
                        outside = true;
                    }
                }
            }
        }

        private bool isColor(Color c, Color d)
        {
            return (c.R == d.R && c.G == d.G && c.B == d.B);
        }
        
    }
}
