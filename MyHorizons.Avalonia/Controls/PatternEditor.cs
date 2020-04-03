using Avalonia;
using Avalonia.Media;
using MyHorizons.Data.TownData;
using System;
using System.Collections.Generic;
using static MyHorizons.Avalonia.Utility.GridUtil;

namespace MyHorizons.Avalonia.Controls
{
    public sealed class PatternEditor : PatternVisualizer
    {
        private IReadOnlyList<Line>? lineCache;
        private static readonly Pen gridPen = new Pen(new SolidColorBrush(0xFF999999), 2, null, PenLineCap.Flat, PenLineJoin.Bevel);

        public PatternEditor(DesignPattern pattern, double width = 32, double height = 32) : base(pattern, width, height)
        {
            Resize(Width, Height);

            this.GetObservable(WidthProperty).Subscribe(newWidth => Resize(newWidth, Height));
            this.GetObservable(HeightProperty).Subscribe(newHeight => Resize(Width, newHeight));
        }

        private void Resize(double width, double height)
        {
            lineCache = GetGridCache(width + 1, height + 1, width / PATTERN_WIDTH, height / PATTERN_HEIGHT);
            InvalidateVisual();
        }

        public void SetDesign(DesignPattern? design) => Design = design;

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (lineCache != null)
                foreach (var line in lineCache)
                    context.DrawLine(gridPen, line.Point0, line.Point1);
        }
    }
}
