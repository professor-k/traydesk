using System;
using System.Collections.Generic;
using System.Drawing;

namespace TrayDesk
{
    public static class IconBuilder
    {
        private static readonly Dictionary<int, bool[,]> DigitPixels = new()
        {
            [0] = new [,]
            {
                {  true,  true,  true },
                {  true, false,  true },
                {  true, false,  true },
                {  true, false,  true },
                {  true,  true,  true }
            },
            [1] = new [,]
            {
                { false,  true,  true },
                { false, false,  true },
                { false, false,  true },
                { false, false,  true },
                { false, false,  true }
            },
            [2] = new [,]
            {
                {  true,  true,  true },
                { false, false,  true },
                {  true,  true,  true },
                {  true, false, false },
                {  true,  true,  true }
            },
            [3] = new [,]
            {
                {  true,  true,  true },
                { false, false,  true },
                { false,  true,  true },
                { false, false,  true },
                {  true,  true,  true }
            },
            [4] = new [,]
            {
                {  true, false,  true },
                {  true, false,  true },
                {  true,  true,  true },
                { false, false,  true },
                { false, false,  true }
            },
            [5] = new [,]
            {
                {  true,  true,  true },
                {  true, false, false },
                {  true,  true,  true },
                { false, false,  true },
                {  true,  true,  true }
            },
            [6] = new [,]
            {
                {  true,  true,  true },
                {  true, false, false },
                {  true,  true,  true },
                {  true, false,  true },
                {  true,  true,  true }
            },
            [7] = new [,]
            {
                {  true,  true,  true },
                { false, false,  true },
                { false,  true, false },
                { false,  true, false },
                { false,  true, false }
            },
            [8] = new [,]
            {
                {  true,  true,  true },
                {  true, false,  true },
                {  true,  true,  true },
                {  true, false,  true },
                {  true,  true,  true }
            },
            [9] = new [,]
            {
                {  true,  true,  true },
                {  true, false,  true },
                {  true,  true,  true },
                { false, false,  true },
                {  true,  true,  true }
            },
        };

        private static void DrawDigit(Bitmap bitmap, int x, int y, int digit, Color color)
        {
            var pixes = DigitPixels[digit];

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (pixes[i, j])
                    {
                        bitmap.SetPixel(x + j, y + i, color);
                    }
                }
            }
        }

        private static void CreateTimeSpan(Bitmap bitmap, TimeSpan span, int yOffset, Color color)
        {
            DrawDigit(bitmap,  0, yOffset, span.Hours / 10, color);
            DrawDigit(bitmap,  4, yOffset, span.Hours % 10, color);
            DrawDigit(bitmap,  9, yOffset, span.Minutes / 10, color);
            DrawDigit(bitmap, 13, yOffset, span.Minutes % 10, color);
        }

        public static Icon CreateIcon(TimeSpan up, TimeSpan down, bool warn)
        {
            var bitmap = new Bitmap(16, 16);
            if (!warn)
            {
                bitmap.MakeTransparent();
            }
            else
            {
                Graphics g = Graphics.FromImage(bitmap);
                g.Clear(Color.Yellow);
            }

            CreateTimeSpan(bitmap, up, 2, Color.Green);
            CreateTimeSpan(bitmap, down, 9, Color.Red);

            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);

            return icon;
        }
    }
}