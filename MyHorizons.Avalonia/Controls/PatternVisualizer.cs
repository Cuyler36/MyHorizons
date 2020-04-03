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

        private DesignPattern? _design;

        public DesignPattern? Design
        {
            get => _design;
            protected set
            {
                if (_design != value)
                {
                    _design = value;
                    UpdateBitmap();
                }
            }
        }

        private IBitmap? bitmap;

        public PatternVisualizer(DesignPattern design, double width = 32, double height = 32)
        {
            Width = width;
            Height = height;
            Design = design;
            ToolTip.SetTip(this, Design.Name);
        }

        ~PatternVisualizer()
        {
            bitmap?.Dispose();
        }

        protected unsafe void UpdateBitmap()
        {
            bitmap?.Dispose();

            if (Design != null)
            {
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

                    data[i] = Design.GetPixelArgb(x, y);
                }

                fixed (uint* p = data)
                    bitmap = new Bitmap(PixelFormat.Bgra8888, (IntPtr)p, new PixelSize(PATTERN_WIDTH, PATTERN_HEIGHT), new Vector(96, 96), sizeof(uint) * PATTERN_WIDTH);
                Background = new ImageBrush(bitmap);
            }
        }
    }
}
