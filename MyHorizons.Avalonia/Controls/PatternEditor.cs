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

        public PatternEditor(DesignPattern pattern) : base(pattern)
        {
            Resize(Width, Height);

            this.GetObservable(WidthProperty).Subscribe(width => Resize(width, Height));
            this.GetObservable(HeightProperty).Subscribe(height => Resize(Width, height));
        }

        private void Resize(double width, double height)
        {
            lineCache = GetGridCache(width, height, width / PATTERN_WIDTH, height / PATTERN_HEIGHT);
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (lineCache != null)
                foreach (var line in lineCache)
                    context.DrawLine(gridPen, line.Point0, line.Point1);
        }
    }
}
