using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MyHorizons.Data;
using MyHorizons.Data.Save;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyHorizons.Avalonia
{
    public class MainWindow : Window
    {
        private MainSaveFile saveFile;
        private Player selectedPlayer;

        private Grid TitleBarGrid;

        private Grid CloseGrid;
        private Button CloseButton;

        private Grid ResizeGrid;
        private Button ResizeButton;

        private Grid MinimizeGrid;
        private Button MinimizeButton;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            TitleBarGrid = this.FindControl<Grid>("TitleBarGrid");
            CloseGrid = this.FindControl<Grid>("CloseGrid");
            CloseButton = this.FindControl<Button>("CloseButton");
            ResizeGrid = this.FindControl<Grid>("ResizeGrid");
            ResizeButton = this.FindControl<Button>("ResizeButton");
            MinimizeGrid = this.FindControl<Grid>("MinimizeGrid");
            MinimizeButton = this.FindControl<Button>("MinimizeButton");

            SetupSide("Left", StandardCursorType.LeftSide, WindowEdge.West);
            SetupSide("Right", StandardCursorType.RightSide, WindowEdge.East);
            SetupSide("Top", StandardCursorType.TopSide, WindowEdge.North);
            SetupSide("Bottom", StandardCursorType.BottomSide, WindowEdge.South);
            SetupSide("TopLeft", StandardCursorType.TopLeftCorner, WindowEdge.NorthWest);
            SetupSide("TopRight", StandardCursorType.TopRightCorner, WindowEdge.NorthEast);
            SetupSide("BottomLeft", StandardCursorType.BottomLeftCorner, WindowEdge.SouthWest);
            SetupSide("BottomRight", StandardCursorType.BottomRightCorner, WindowEdge.SouthEast);

            TitleBarGrid.PointerPressed += (i, e) => PlatformImpl?.BeginMoveDrag(e);

            CloseGrid.PointerEnter += CloseGrid_PointerEnter;
            CloseGrid.PointerLeave += CloseGrid_PointerLeave;

            ResizeGrid.PointerEnter += ResizeGrid_PointerEnter;
            ResizeGrid.PointerLeave += ResizeGrid_PointerLeave;

            MinimizeButton.PointerLeave += MinimizeButton_PointerLeave;
            MinimizeButton.PointerEnter += MinimizeButton_PointerEnter;

            CloseButton.Click += CloseButton_Click;
            ResizeButton.Click += ResizeButton_Click;
            MinimizeButton.Click += MinimizeButton_Click;

            PlatformImpl.WindowStateChanged = WindowStateChanged;

            var openBtn = this.FindControl<Button>("OpenSaveButton");
            openBtn.Click += OpenFileButton_Click;

            openBtn.IsVisible = true;
            this.FindControl<TabControl>("EditorTabControl").IsVisible = false;
            
        }

        private void SetupSide(string name, StandardCursorType cursor, WindowEdge edge)
        {
            var ctl = this.FindControl<Control>(name);
            ctl.Cursor = new Cursor(cursor);
            ctl.PointerPressed += (i, e) => PlatformImpl?.BeginResizeDrag(edge, e);
        }

        private void WindowStateChanged(WindowState state)
        {
            base.HandleWindowStateChanged(state);

            if (state != WindowState.Minimized)
            {
                var img = this.FindControl<Image>("ResizeImage");
                Bitmap bitmap;
                if (WindowState == WindowState.Normal)
                {
                    bitmap = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>().Open(new Uri("resm:MyHorizons.Avalonia.Resources.Maximize.png")));
                }
                else
                {
                    bitmap = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>().Open(new Uri("resm:MyHorizons.Avalonia.Resources.Restore.png")));
                }

                img.Source?.Dispose();
                img.Source = bitmap;
            }
        }

        private void AddPlayerImages()
        {
            var contentHolder = this.FindControl<StackPanel>("PlayerSelectorPanel");
            foreach (var playerSave in saveFile.GetPlayerSaves())
            {
                var img = new Image
                {
                    Width = 120,
                    Height = 120,
                    Source = LoadPlayerPhoto(playerSave.Index)
                };
                contentHolder.Children.Add(img);
            }
        }

        private void LoadPlayer(Player player)
        {
            selectedPlayer = player;
            this.FindControl<TextBox>("PlayerNameBox").Text = player.Name;
            this.FindControl<NumericUpDown>("WalletBox").Value = player.Wallet.Decrypt();
            this.FindControl<NumericUpDown>("NookMilesBox").Value = player.NookMiles.Decrypt();
        }

        private async void OpenFileButton_Click(object o, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter
                        {
                            Name = "New Horizons Save File",
                            Extensions = new List<string>
                            {
                                "dat"
                            }
                        },
                        new FileDialogFilter
                        {
                            Name = "All Files",
                            Extensions = new List<string>
                            {
                                "*"
                            }
                        }
                    }
            };

            var files = await openFileDialog.ShowAsync(this);
            if (files.Length > 0)
            {
                // Determine whether they selected the header file or the main file
                var file = files[0];
                string headerPath;
                string filePath;
                if (file.EndsWith("Header.dat"))
                {
                    headerPath = file;
                    filePath = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file).Replace("Header", "")}.dat");
                }
                else
                {
                    filePath = file;
                    headerPath = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}Header.dat");
                }

                if (File.Exists(headerPath) && File.Exists(filePath))
                {
                    saveFile = new MainSaveFile(headerPath, filePath);
                    if (saveFile.Loaded)
                    {
                        (o as Button).IsVisible = false;
                        this.FindControl<TabControl>("EditorTabControl").IsVisible = true;
                        AddPlayerImages();
                        LoadPlayer(saveFile.GetPlayerSaves()[0].Player);
                    }
                    else
                    {
                        saveFile = null;
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void CloseGrid_PointerEnter(object sender, PointerEventArgs e) => CloseGrid.Background = new SolidColorBrush(0xFF648589);

        private void CloseGrid_PointerLeave(object sender, PointerEventArgs e) => CloseGrid.Background = Brushes.Transparent;

        private void ResizeGrid_PointerEnter(object sender, PointerEventArgs e) => ResizeGrid.Background = new SolidColorBrush(0xFF648589);

        private void ResizeGrid_PointerLeave(object sender, PointerEventArgs e) => ResizeGrid.Background = Brushes.Transparent;

        private void MinimizeButton_PointerEnter(object sender, PointerEventArgs e) => MinimizeGrid.Background = new SolidColorBrush(0xFF648589);

        private void MinimizeButton_PointerLeave(object sender, PointerEventArgs e) => MinimizeGrid.Background = Brushes.Transparent;

        private Bitmap LoadPlayerPhoto(int index)
        {
            using var memStream = new MemoryStream(saveFile.GetPlayer(index).GetPhotoData());
            return new Bitmap(memStream);
        }
    }
}
