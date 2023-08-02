using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using LCDonald.Core.Layout;
using LCDonald.ViewModels;
using LCDonald.Views;
using System;
using System.Linq;

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
            var thm = Application.Current?.Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault();
            var settings = new SettingsViewModel();
            
            if (SettingsViewModel.CurrentSettings.ApplicationTheme == "System" || SettingsViewModel.CurrentSettings.ApplicationTheme == "Default") 
            {
                RequestedThemeVariant = ThemeVariant.Default;
                thm.PreferSystemTheme = true;
            }
            else
            {
                RequestedThemeVariant = SettingsViewModel.CurrentSettings.ApplicationTheme == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;
            }

            var mainView = new MainView
            {
                DataContext = new MainWindowViewModel(),
                Settings = settings
            };

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow(mainView);

                if (OperatingSystem.IsMacOS())   
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
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = mainView;
            }
            
            base.OnFrameworkInitializationCompleted();
        }

        private void AboutMenuItem_OnClick(object sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is MainWindow win)
            {
                win.WindowContent.Open_Settings(sender, null);
            }
        }
    }
}
