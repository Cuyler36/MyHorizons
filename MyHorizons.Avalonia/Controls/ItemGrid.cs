using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using MyHorizons.Data;
using MyHorizons.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static MyHorizons.Avalonia.Utility.GridUtil;

namespace MyHorizons.Avalonia.Controls
{
    class ItemGrid : Canvas
    {
        private static readonly Border ItemToolTip = new Border
        {
            BorderThickness = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            BorderBrush = Brushes.White,
            Child = new TextBlock
            {
                Foreground = Brushes.Black,
                Background = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                ZIndex = int.MaxValue
            }
        };

        private int itemsPerRow = 16;
        private int itemsPerCol = 16;
        private int itemSize = 32;
        private ItemCollection items;
        private IReadOnlyList<Line> lineCache;
        private readonly IList<RectangleGeometry> itemCache;
        private int x = -1;
        private int y = -1;
        private int currentIdx = -1;

        private bool mouseLeftDown = false;
        private bool mouseRightDown = false;
        private bool mouseMiddleDown = false;

        private const uint GridColor = 0xFF999999;
        private const uint HighlightColor = 0x7FFFFF00;
        private static readonly Pen gridPen = new Pen(new SolidColorBrush(GridColor), 2, null, PenLineCap.Flat, PenLineJoin.Bevel);
        private static readonly SolidColorBrush highlightBrush = new SolidColorBrush(HighlightColor);
        private static readonly SolidColorBrush itemBrush = new SolidColorBrush(0xBB00FF00);
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
                if (Background is ImageBrush brush)
                    brush.DestinationRect = new RelativeRect(0, 0, itemSize, itemSize, RelativeUnit.Absolute);
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

        private void SetItem()
        {
            var currentItem = items[currentIdx];
            if (currentItem != MainWindow.SelectedItem) // Poor hack
            {
                InvalidateVisual();
                items[currentIdx] = MainWindow.SelectedItem.Clone();
            }
        }

        private void ShowTip(PointerEventArgs e, bool updateText = false)
        {
            var grid = MainWindow.Singleton().FindControl<Grid>("MainContentGrid");
            var point = e.GetPosition(grid as IVisual);

            if (updateText && ItemToolTip.Child is TextBlock block)
                block.Text = ItemDatabaseLoader.GetNameForItem(items[currentIdx]);

            ItemToolTip.Margin = new Thickness(point.X + 15, point.Y + 10, 0, 0);
            if (ItemToolTip.Parent == null)
                grid.Children.Add(ItemToolTip);
        }

        private void HideTip()
        {
            if (ItemToolTip.Parent != null && ItemToolTip.Parent is Grid grid)
                grid.Children.Remove(ItemToolTip);
        }

        private void ItemGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
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

        private void ItemGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            switch (e.GetCurrentPoint(this).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    if (currentIdx > -1 && currentIdx < items.Count)
                    {
                        SetItem();
                        ShowTip(e, true);
                    }
                    mouseLeftDown = true;
                    break;
                case PointerUpdateKind.RightButtonPressed:
                    if (currentIdx > -1 && currentIdx < items.Count)
                        MainWindow.Singleton().SetItem(items[currentIdx]);
                    mouseRightDown = true;
                    break;
                case PointerUpdateKind.MiddleButtonPressed:
                    mouseMiddleDown = true;
                    break;
            }
        }

        private void ItemGrid_PointerLeave(object? sender, PointerEventArgs e)
        {
            if (x == -1 || y == -1) return;
            x = y = currentIdx = -1;
            HideTip();
            InvalidateVisual();
        }

        private void ItemGrid_PointerMoved(object? sender, PointerEventArgs e)
        {
            var point = e.GetPosition(sender as IVisual);
            if (point.X < 0 || point.Y < 0 || point.X >= Width || point.Y >= Height)
            {
                x = y = currentIdx = -1;
                mouseLeftDown = false;
                mouseRightDown = false;
                mouseMiddleDown = false;
                HideTip();
                InvalidateVisual();
                return;
            }

            var tX = (int)point.X - (int)point.X % itemSize;
            var tY = (int)point.Y - (int)point.Y % itemSize;
            var updateText = false;

            if (tX != x || tY != y)
            {
                x = tX;
                y = tY;

                var idx = (tY / itemSize) * itemsPerRow + tX / itemSize;
                if (idx != currentIdx)
                {
                    currentIdx = idx;
                    updateText = true;
                }

                InvalidateVisual();
            }

            if (currentIdx > -1 && currentIdx < items.Count)
            {
                if (mouseLeftDown)
                {
                    SetItem();
                    ShowTip(e, true);
                    return;
                }
                else if (mouseRightDown)
                    MainWindow.Singleton().SetItem(items[currentIdx]);
                ShowTip(e, updateText);
            }
        }

        private void Resize(int width, int height)
        {
            Width = width + 1;
            Height = height + 1;
            CreateAndCacheGridLines();
            CreateAndCacheItemRects();
        }

        private void CreateAndCacheGridLines() => lineCache = GetGridCache(Width, Height, itemSize, itemSize);

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
                        context.DrawGeometry(itemBrush, null, itemCache[i]);

            // Draw highlight.
            if (x > -1 && y > -1 && currentIdx > -1 && currentIdx < items.Count)
                context.FillRectangle(highlightBrush, new Rect(x, y, itemSize, itemSize));

            // Draw grid above items from gridline cache.
            foreach (var line in lineCache)
                context.DrawLine(gridPen, line.Point0, line.Point1);
        }
    }
}
