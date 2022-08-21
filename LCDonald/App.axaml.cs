using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.Styling;
using LCDonald.ViewModels;
using LCDonald.Views;
using System;

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
                var settings = new SettingsViewModel();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                    Settings = settings
                };

                var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                thm?.ForceWin32WindowToTheme(desktop.MainWindow); // Window is the Window object you want to force

                if (System.OperatingSystem.IsMacOS())   
                {
                    // Macify the styling a bit
                    Resources.Add("ControlContentThemeFontSize", (double)13);
                    Resources.Add("ContentControlThemeFontFamily", new FontFamily("avares://LCDonald/Assets#SF Pro Text"));
                    Resources.Add("ControlCornerRadius", new CornerRadius(6));
                    Resources.Add("NavigationViewContentGridCornerRadius", new CornerRadius(0));
                    Resources.Add("NavigationViewExpandedPaneBackground", Colors.Transparent);
                    Resources.Add("NavigationViewDefaultPaneBackground", Colors.Transparent);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void AboutMenuItem_OnClick(object sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is MainWindow win)
            {
                win.Open_Settings(sender, null);
            }
        }
    }
}
