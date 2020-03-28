using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using MyHorizons.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MyHorizons.Avalonia.Controls
{
    class ItemGrid : Canvas
    {
        private struct Line
        {
            public Point Point0;
            public Point Point1;

            public Line(Point p0, Point p1)
            {
                Point0 = p0;
                Point1 = p1;
            }
        }

        private int itemsPerRow = 16;
        private int itemsPerCol = 16;
        private int itemSize = 32;
        private ItemCollection items;
        private IList<Line> lineCache;
        private IList<RectangleGeometry> itemCache;
        private int x = -1;
        private int y = -1;

        private bool mouseLeftDown = false;
        private bool mouseRightDown = false;
        private bool mouseMiddleDown = false;

        private const uint GridColor = 0xFF999999;
        private const uint HighlightColor = 0x7FFFFF00;
        private static readonly Pen gridPen = new Pen(new SolidColorBrush(GridColor), 2, null, PenLineCap.Flat, PenLineJoin.Bevel);
        private static readonly SolidColorBrush HighlightBrush = new SolidColorBrush(HighlightColor);
        private static readonly Bitmap background = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>().Open(new Uri("resm:MyHorizons.Avalonia.Resources.ItemGridBackground.png")));

        public int ItemsPerRow
        {
            get => itemsPerRow;
            set
            {
                if (itemsPerRow == value) return;
                itemsPerRow = value;
                Resize(itemsPerRow * itemSize, itemsPerCol * itemSize);
            }
        }

        public int ItemsPerCol
        {
            get => itemsPerCol;
            set
            {
                if (itemsPerCol == value) return;
                itemsPerCol = value;
                Resize(itemsPerRow * itemSize, itemsPerCol * itemSize);
            }
        }

        public int ItemSize
        {
            get => itemSize;
            set
            {
                if (itemSize == value) return;
                itemSize = value;
                Resize(itemsPerRow * itemSize, itemsPerCol * itemSize);
                (Background as ImageBrush).DestinationRect = new RelativeRect(0, 0, itemSize, itemSize, RelativeUnit.Absolute);
            }
        }

        public ItemCollection Items
        {
            get => items;
            set
            {
                if (value == null) return;

                items = value;
                items.PropertyChanged += Items_PropertyChanged;
                InvalidateVisual(); // Invalidate the visual state so we re-render the image.
            }
        }

        private void Items_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Items")
                InvalidateVisual();
        }

        public ItemGrid(int numItems, int itemsPerRow, int itemsPerCol, int itemSize = 32)
        {
            this.itemsPerRow = itemsPerRow;
            this.itemsPerCol = itemsPerCol;
            this.itemSize = itemSize;

            var itms = new Item[numItems];
            for (var i = 0; i < numItems; i++)
                itms[i] = Item.NO_ITEM;
            items = new ItemCollection(itms);
            lineCache = new List<Line>();
            itemCache = new List<RectangleGeometry>();
            Resize(itemsPerRow * itemSize, itemsPerCol * itemSize);
            Background = new ImageBrush(background)
            {
                Stretch = Stretch.Uniform,
                TileMode = TileMode.Tile,
                SourceRect = new RelativeRect(0, 0, background.Size.Width, background.Size.Height, RelativeUnit.Absolute),
                DestinationRect = new RelativeRect(0, 0, itemSize, itemSize, RelativeUnit.Absolute),
            };

            PointerMoved += ItemGrid_PointerMoved;
            PointerLeave += ItemGrid_PointerLeave;
            PointerPressed += ItemGrid_PointerPressed;
            PointerReleased += ItemGrid_PointerReleased;
        }

        private void ItemGrid_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            switch (e.GetCurrentPoint(this).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    mouseLeftDown = false;
                    break;
                case PointerUpdateKind.RightButtonReleased:
                    mouseRightDown = false;
                    break;
                case PointerUpdateKind.MiddleButtonReleased:
                    mouseMiddleDown = false;
                    break;
            }
        }

        private void ItemGrid_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            switch (e.GetCurrentPoint(this).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    mouseLeftDown = true;
                    break;
                case PointerUpdateKind.RightButtonPressed:
                    mouseRightDown = true;
                    break;
                case PointerUpdateKind.MiddleButtonPressed:
                    mouseMiddleDown = true;
                    break;
            }
        }

        private void ItemGrid_PointerLeave(object sender, PointerEventArgs e)
        {
            if (x == -1 || y == -1) return;
            x = y = -1;
            InvalidateVisual();
        }

        private void ItemGrid_PointerMoved(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(sender as IVisual);
            if (point.X < 0 || point.Y < 0 || point.X >= Width || point.Y >= Height)
            {
                x = y = -1;
                mouseLeftDown = false;
                mouseRightDown = false;
                mouseMiddleDown = false;
                InvalidateVisual();
                return;
            }

            var tX = (int)point.X - (int)point.X % itemSize;
            var tY = (int)point.Y - (int)point.Y % itemSize;

            if (tX != x || tY != y)
            {
                x = tX;
                y = tY;
                InvalidateVisual();
            }

            if (mouseLeftDown)
            {

            }
        }

        private void Resize(int width, int height)
        {
            Width = width + 1;
            Height = height + 1;
            CreateAndCacheGridLines();
            CreateAndCacheItemRects();
        }

        private void CreateAndCacheGridLines()
        {
            // Clear previous entries.
            lineCache.Clear();
            for (var x = 0; x < Width; x += itemSize)
                lineCache.Add(new Line(new Point(x, -0.5), new Point(x, Height)));
            for (var y = 0; y < Height; y += itemSize)
                lineCache.Add(new Line(new Point(-0.5, y), new Point(Width, y)));
        }

        private void CreateAndCacheItemRects()
        {
            itemCache.Clear();
            for (var y = 0; y < ItemsPerCol; y++)
                for (var x = 0; x < ItemsPerRow; x++)
                    itemCache.Add(new RectangleGeometry(new Rect(x * itemSize, y * itemSize, itemSize, itemSize)));
        }

        private static int Clamp(int value, int min, int max) => value < min ? value : (value > max ? max : value);

        private int PositionToIdx(int x, int y) => ((Math.Max(0, y) / itemSize) * itemsPerRow) + (Math.Max(0, x) / itemSize) % itemsPerRow;

        private Item PositionToItem(int x, int y) => Items[PositionToIdx(x, y)];

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Draw items.
            // TODO?: Caching the brushes in ItemColorManager may increase performance.
            if (Items != null)
                for (var i = 0; i < Items.Count; i++)
                    if (items[i].ItemId != 0xFFFE)
                        context.DrawGeometry(new SolidColorBrush(0x7F00FF00), null, itemCache[i]);

            // Draw highlight.
            if (x > -1 && y > -1)
                context.DrawGeometry(HighlightBrush, null, new RectangleGeometry(new Rect(x, y, itemSize, itemSize)));

            // Draw grid above items from gridline cache.
            foreach (var line in lineCache)
                context.DrawLine(gridPen, line.Point0, line.Point1);
        }

        private void ValidateGridSize()
        {
            //if ()
        }
    }
}
