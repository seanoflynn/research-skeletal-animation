using System;
using System.Drawing;
using OpenTK;

namespace Graphics.Util
{
    public static class Bitmap
    {
        public static System.Drawing.Bitmap CreateBlank(int width, int height, Vector4 color)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(1, 1);
            Color c = Color.FromArgb(Convert.ToInt32(color.W * 255), Convert.ToInt32(color.X * 255), Convert.ToInt32(color.Y * 255), Convert.ToInt32(color.Z * 255));
            bmp.SetPixel(0, 0, c);
            return bmp;
        }
    }
}