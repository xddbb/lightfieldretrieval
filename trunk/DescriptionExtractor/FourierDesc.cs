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

        public FourierDesc(Bitmap bitmap)
        {
            shapeColor = Color.Black;
            bitmap_ = bitmap;
            newbitmap_ = new Bitmap(bitmap.Height, bitmap.Width);

            findOutline();

            newbitmap_.Save("C:\\result.bmp");
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
                for (int i = 0; i < bitmap_.Width - 1; i++)
                {
                   processPixel(i, j, i+1, j);
                }
            }
            
            // Vertical processing
            for (int i = 0; i < bitmap_.Width - 1; i++)
            {
                for (int j = 0; j < bitmap_.Height - 1; j++)
                {
                    processPixel(i, j, i, j+1);
                }
            }
        }

        private void processPixel(int i1, int j1, int i2, int j2)
        {
            Color c1 = bitmap_.GetPixel(i1, j1);
            Color c2 = bitmap_.GetPixel(i2, j2);

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
                if (isColor(c1, Color.White) && isColor(c2, shapeColor))
                {
                    newbitmap_.SetPixel(i2, j2, Color.Black);
                }
                else if (isColor(c1, shapeColor) && isColor(c2, Color.White))
                {
                    newbitmap_.SetPixel(i1, j1, Color.Black);
                }
            }
        }

        private bool isColor(Color c, Color d)
        {
            return (c.R == d.R && c.G == d.G && c.B == d.B);
        }
        
    }
}
