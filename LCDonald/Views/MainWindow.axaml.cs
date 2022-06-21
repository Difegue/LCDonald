using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using System.Diagnostics;

namespace LCDonald.Views
{
    public partial class MainWindow : Window
    {        
        public MainWindow()
        {
            InitializeComponent();
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
                PrimaryButtonText = "Close"
            };

            await dialog.ShowAsync();            
        }
    }
}
