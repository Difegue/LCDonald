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

        private async void Open_About(object sender, PointerPressedEventArgs e)
        {
            // TODO slappy implementation, should go in settings later
            var dialog = new ContentDialog()
            {
                Title = "About McD's Sonic Simulator",
                Content = "A monument to the everlasting symbiotic relationship between Sonic the Hedgehog and ultracapitalist fast food joints. 🏛\n\nPowered by Avalonia and the LCDonald engine. \nFeel free to contribute to the project on GitHub!",
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "View Source Code"
            };

            var result = await dialog.ShowAsync();

            // Open URL if secondary
            if (result == ContentDialogResult.Secondary)
            {
                ProcessStartInfo psi = new ProcessStartInfo("https://github.com/Difegue/LCDonald");
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
        }
    }
}
