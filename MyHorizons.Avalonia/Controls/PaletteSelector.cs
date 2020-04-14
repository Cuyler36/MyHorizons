using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MyHorizons.Data.TownData;

namespace MyHorizons.Avalonia.Controls
{
    public sealed class PaletteSelector : Canvas
    {
        private static readonly Pen gridPen = new Pen(new SolidColorBrush(0xFF777777), 2, null, PenLineCap.Flat, PenLineJoin.Bevel);

        private DesignPattern? _design;

        public new double Height
        {
            get => base.Height;
            private set => base.Height = value;
        }

        public new double Width
        {
            get => base.Width;
            set => Resize(value);
        }

        public PaletteSelector(DesignPattern pattern, double width = 16.0d)
        {
            _design = pattern;
            Resize(width);
        }

        public void SetDesign(DesignPattern? pattern)
        {
            _design = pattern;
            InvalidateVisual();
        }

        private void Resize(double w)
        {
            base.Width = w;
            base.Height = 16 * w;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (_design != null)
            {
                for (var i = 0; i < 16; i++)
                {
                    var rect = new Rect(0, i * Width, Width, Width);
                    if (i < 15)
                        context.FillRectangle(new SolidColorBrush(_design.Palette[i].ToArgb()), rect);
                    context.DrawRectangle(gridPen, rect);
                }
            }
        }
    }
}
