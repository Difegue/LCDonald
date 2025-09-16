using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using System;

namespace LCDonald.Views
{
    public partial class MainWindow : AppWindow
    {
        public MainView WindowContent { get; set; }

        public MainWindow(MainView view)
        {
            InitializeComponent();

#if BURGER
            Title = "Burger Bard Crystalectrics";

            var asset = AssetLoader.Open(new Uri("avares://LCDonald/Assets/burger-icon.ico"));
            Icon = new Bitmap(asset);
#endif

            WindowContent = view;
            ViewContainer.Children.Add(view);

            ExtendClientAreaChromeHints =
               Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome |
               Avalonia.Platform.ExtendClientAreaChromeHints.OSXThickTitleBar;

            if (OperatingSystem.IsMacOS())
            {
                if (Application.Current.RequestedThemeVariant == Avalonia.Styling.ThemeVariant.Dark) // Custom outer border to simulate macOS' dark theme window decoration
                    MacWindowBorder.IsVisible = true;
            }
        }

        #region Custom Drag area implementation
        private bool isPointerPressed = false;
        private PixelPoint startPosition = new PixelPoint(0, 0);
        private Point mouseOffsetToOrigin = new Point(0, 0);

        // TODO PointerPressed="BeginListenForDrag" PointerMoved="HandlePotentialDrag" PointerReleased="HandlePotentialDrop"
        private void HandlePotentialDrop(object sender, PointerReleasedEventArgs e)
        {
            var pos = e.GetPosition(this);
            startPosition = new PixelPoint((int)(startPosition.X + pos.X - mouseOffsetToOrigin.X), (int)(startPosition.Y + pos.Y - mouseOffsetToOrigin.Y));
            Position = startPosition;
            isPointerPressed = false;
        }

        private void HandlePotentialDrag(object sender, PointerEventArgs e)
        {
            if (isPointerPressed)
            {
                var pos = e.GetPosition(this);
                startPosition = new PixelPoint((int)(startPosition.X + pos.X - mouseOffsetToOrigin.X), (int)(startPosition.Y + pos.Y - mouseOffsetToOrigin.Y));
                Position = startPosition;
            }
        }

        private void BeginListenForDrag(object sender, PointerPressedEventArgs e)
        {
            startPosition = Position;
            mouseOffsetToOrigin = e.GetPosition(this);
            isPointerPressed = true;
        }

        #endregion
    }
}
