using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using System.Diagnostics;
using FluentAvalonia.Styling;
using Avalonia.Metadata;
using Avalonia.Controls.Templates;
using System;
using LCDonald.ViewModels;

namespace LCDonald.Views
{
    public partial class MainWindow : CoreWindow
    {
        public SettingsViewModel Settings { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            ExtendClientAreaChromeHints =
               Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome |
               Avalonia.Platform.ExtendClientAreaChromeHints.OSXThickTitleBar;

            if (OperatingSystem.IsMacOS())
            {
                // More Macification
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                NavView.OpenPaneLength = 248;
                NavView.IsPaneToggleButtonVisible = false;
                PaneBottomPadding.Height = 32;

                var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                if (thm?.RequestedTheme == "Dark")
                    MacWindowBorder.IsVisible = true;
            }
        }

        private async void Open_Settings(object sender, PointerPressedEventArgs e)
        {
            // Kinda unclean to do this in codebehind but this is a simple game, itll do
            var dialog = new ContentDialog()
            {
                Title = "Settings",
                Content = new SettingsPopup(),
                DataContext = Settings,
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = "Close",
                TitleTemplate = (IDataTemplate)Resources["DialogTitleTemplate"]
            };

            await dialog.ShowAsync();

            // Refresh Game View
            var lcdView = this.FindControl<Controls.AvaloniaLCDView>("LCDView");
            lcdView.CurrentView = lcdView.CurrentView; // heh
        }

        #region Custom Drag area implementation
        private bool isPointerPressed = false;
        private PixelPoint startPosition = new PixelPoint(0, 0);
        private Point mouseOffsetToOrigin = new Point(0, 0);

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
