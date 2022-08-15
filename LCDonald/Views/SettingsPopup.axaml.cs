using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Threading;

namespace LCDonald.Views
{
    public partial class SettingsPopup : UserControl
    {
        public SettingsPopup()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            // Timer for carousel
            var carouselTimer = new System.Timers.Timer(5000);
            carouselTimer.Elapsed += (s,e) => {
                Dispatcher.UIThread.Post(() => this.FindControl<Carousel>("carousel").Next());
                carouselTimer.Stop();
                Thread.Sleep(5000);
                Dispatcher.UIThread.Post(() => this.FindControl<Carousel>("carousel").Previous());
                carouselTimer.Start();
            };

            carouselTimer.Start();
        }
    }
}
