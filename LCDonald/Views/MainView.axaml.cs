using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using LCDonald.ViewModels;
using System;

namespace LCDonald.Views
{
    public partial class MainView : UserControl
    {
        public SettingsViewModel Settings { get; set; }
        
        public MainView()
        {
            InitializeComponent();

            if (OperatingSystem.IsMacOS())
            {
                // More Macification
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                NavView.OpenPaneLength = 248;
                NavView.IsPaneToggleButtonVisible = false;
                PaneBottomPadding.Height = 32;
            }
        }

        public async void Open_Settings(object sender, PointerPressedEventArgs e)
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
    }
}
