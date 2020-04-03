using Avalonia;
using System.Collections.Generic;

namespace MyHorizons.Avalonia.Utility
{
    internal static class GridUtil
    {
        internal readonly struct Line
        {
            public readonly Point Point0;
            public readonly Point Point1;

            public Line(in Point p0, in Point p1)
            {
                Point0 = p0;
                Point1 = p1;
            }
        }

        public static IReadOnlyList<Line> GetGridCache(in double width, in double height, in double step)
        {
            var gridLineCache = new List<Line>((int)(width / step) + (int)(height / step) + 1);
            for (var x = 0d; x < width; x += step)
                gridLineCache.Add(new Line(new Point(x, -0.5), new Point(x, height)));
            for (var y = 0d; y < height; y += step)
                gridLineCache.Add(new Line(new Point(-0.5, y), new Point(width, y)));
            return gridLineCache;
        }
    }
}
