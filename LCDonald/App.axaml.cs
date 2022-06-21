using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
