using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.Styling;
using LCDonald.ViewModels;
using LCDonald.Views;

namespace LCDonald
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                thm?.ForceWin32WindowToTheme(desktop.MainWindow); // Window is the Window object you want to force

                if (System.OperatingSystem.IsMacOS())
                {
                    // Macify the styling a bit
                    Resources.Add("ControlContentThemeFontSize", (double)13);
                    Resources.Add("ContentControlThemeFontFamily", new FontFamily("SF Pro Text"));
                    Resources.Add("ControlCornerRadius", new CornerRadius(6));
                    Resources.Add("NavigationViewContentGridCornerRadius", new CornerRadius(0));
                    Resources.Add("NavigationViewExpandedPaneBackground", Colors.Transparent);
                    Resources.Add("NavigationViewDefaultPaneBackground", Colors.Transparent);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
