using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MyHorizons.Data.TownData;

namespace MyHorizons.Avalonia.Controls
{
    public sealed class PatternVisualizer : Canvas
    {
        private const int PATTERN_WIDTH = 32;
        private const int PATTERN_HEIGHT = 32;

        public readonly DesignPattern Pattern;

        public PatternVisualizer(DesignPattern pattern)
        {
            Pattern = pattern;
            ToolTip.SetTip(this, Pattern.Name);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var scaleX = Width / PATTERN_WIDTH;
            var scaleY = Height / PATTERN_HEIGHT;

            for (var y = 0; y < PATTERN_HEIGHT; y++)
                for (var x = 0; x < PATTERN_WIDTH; x++)
                    context.FillRectangle(new SolidColorBrush(Pattern.GetPixelArgb(x, y)), new Rect(x * scaleX, y * scaleY, scaleX, scaleY));
        }
    }
}
