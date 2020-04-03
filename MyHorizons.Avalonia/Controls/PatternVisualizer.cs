using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MyHorizons.Data.TownData;
using System;

namespace MyHorizons.Avalonia.Controls
{
    public class PatternVisualizer : Canvas
    {
        protected const int PATTERN_WIDTH = 32;
        protected const int PATTERN_HEIGHT = 32;

        public readonly DesignPattern Pattern;

        private IBitmap? bitmap;

        public PatternVisualizer(DesignPattern pattern, double width = 32, double height = 32)
        {
            Width = width;
            Height = height;
            Pattern = pattern;
            UpdateBitmap();
            ToolTip.SetTip(this, Pattern.Name);
        }

        ~PatternVisualizer()
        {
            bitmap?.Dispose();
        }

        protected unsafe void UpdateBitmap()
        {
            bitmap?.Dispose();

            var data = new uint[PATTERN_WIDTH * PATTERN_HEIGHT];
            var x = 0;
            var y = 0;
            for (var i = 0; i < data.Length; i++, x++)
            {
                if (x == PATTERN_WIDTH)
                {
                    x = 0;
                    y++;
                }

                data[i] = Pattern.GetPixelArgb(x, y);
            }

            fixed(uint* p = data)
                bitmap = new Bitmap(PixelFormat.Bgra8888, (IntPtr)p, new PixelSize(PATTERN_WIDTH, PATTERN_HEIGHT), new Vector(96, 96), sizeof(uint) * PATTERN_WIDTH);
            Background = new ImageBrush(bitmap);
        }
    }
}
