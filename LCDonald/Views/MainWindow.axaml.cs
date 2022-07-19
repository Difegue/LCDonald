using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using System.Diagnostics;

namespace LCDonald.Views
{
    public partial class MainWindow : CoreWindow
    {        
        public MainWindow()
        {
            InitializeComponent();
            ExtendClientAreaChromeHints =
               Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome |
               Avalonia.Platform.ExtendClientAreaChromeHints.OSXThickTitleBar;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private async void Open_Settings(object sender, PointerPressedEventArgs e)
        {
            // Kinda unclean to do this in codebehind but this is a simple game, itll do
            var dialog = new ContentDialog()
            {
                Title = "Settings",
                Content = new SettingsPopup(),
                DataContext = new ViewModels.SettingsViewModel(),
                PrimaryButtonText = "Close",
                TitleTemplate = (Avalonia.Controls.Templates.IDataTemplate)Resources["DialogTitleTemplate"],
            };

            await dialog.ShowAsync();            
        }

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
    }
}
