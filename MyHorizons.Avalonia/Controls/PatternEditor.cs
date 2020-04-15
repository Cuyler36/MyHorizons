using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using MyHorizons.Data.TownData;
using System;
using System.Collections.Generic;
using static MyHorizons.Avalonia.Utility.GridUtil;

namespace MyHorizons.Avalonia.Controls
{
    public sealed class PatternEditor : PatternVisualizer
    {
        private PaletteSelector _paletteSelector;
        private IReadOnlyList<Line>? lineCache;
        private int cellX = -1;
        private int cellY = -1;

        private double stepX;
        private double stepY;

        private bool leftDown = false;
        private bool rightDown = false;

        private static readonly Pen gridPen = new Pen(new SolidColorBrush(0xFF999999), 2, null, PenLineCap.Flat, PenLineJoin.Bevel);
        private static readonly Pen highlightPen = new Pen(new SolidColorBrush(0xFFFFFF00), 2, null, PenLineCap.Flat, PenLineJoin.Bevel);

        public PatternEditor(DesignPattern pattern, PaletteSelector selector, double width = 32, double height = 32) : base(pattern, width, height)
        {
            _paletteSelector = selector;
            Resize(Width, Height);
            ToolTip.SetTip(this, null);

            this.GetObservable(WidthProperty).Subscribe(newWidth => Resize(newWidth, Height));
            this.GetObservable(HeightProperty).Subscribe(newHeight => Resize(Width, newHeight));
            PointerMoved += OnPointerMoved;
            PointerLeave += (o, e) =>
            {
                cellX = cellY = -1;
                InvalidateVisual();
            };
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
        }

        private void Resize(double width, double height)
        {
            stepX = width / PATTERN_WIDTH;
            stepY = height / PATTERN_HEIGHT;
            lineCache = GetGridCache(width + 1, height + 1, stepX, stepY);
            InvalidateVisual();
        }

        public void SetDesign(DesignPattern? design)
        {
            Design = design;
            _paletteSelector.SetDesign(design);
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var point = e.GetPosition(sender as IVisual);
            if (point.X < 0 || point.Y < 0 || point.X >= Width || point.Y >= Height)
            {
                cellX = cellY = -1;
                InvalidateVisual();
            }
            else
            {
                var tX = (int)(point.X / (Width / PATTERN_WIDTH));
                var tY = (int)(point.Y / (Height / PATTERN_HEIGHT));
                if (tX < 0 || tX >= PATTERN_WIDTH || tY < 0 || tY >= PATTERN_HEIGHT)
                {
                    cellX = cellY = -1;
                    InvalidateVisual();
                }
                else if (tX != cellX || tY != cellY)
                {
                    cellX = tX;
                    cellY = tY;

                    if (leftDown)
                    {
                        if (Design?.GetPixel(cellX, cellY) != _paletteSelector.SelectedIndex)
                        {
                            Design?.SetPixel(cellX, cellY, (byte)_paletteSelector.SelectedIndex);
                            UpdateBitmap();
                        }
                    }
                    else if (rightDown)
                    {
                        _paletteSelector.SelectedIndex = Design?.GetPixel(cellX, cellY) ?? -1;
                    }

                    InvalidateVisual();
                }
            }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            switch (e.GetCurrentPoint(this).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    {
                        if (Design?.GetPixel(cellX, cellY) != _paletteSelector.SelectedIndex)
                        {
                            Design?.SetPixel(cellX, cellY, (byte)_paletteSelector.SelectedIndex);
                            UpdateBitmap();
                        }
                        leftDown = true;
                        break;
                    }
                case PointerUpdateKind.RightButtonPressed:
                    {
                        _paletteSelector.SelectedIndex = Design?.GetPixel(cellX, cellY) ?? -1;
                        rightDown = true;
                        break;
                    }
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            switch (e.GetCurrentPoint(this).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    {
                        leftDown = false;
                        break;
                    }
                case PointerUpdateKind.RightButtonReleased:
                    {
                        rightDown = false;
                        break;
                    }
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Draw grid
            if (lineCache != null)
                foreach (var line in lineCache)
                    context.DrawLine(gridPen, line.Point0, line.Point1);

            // Draw highlight
            if (cellX > -1 && cellY > -1)
                context.DrawRectangle(highlightPen, new Rect(cellX * stepX, cellY * stepY, stepX, stepY));
        }
    }
}
