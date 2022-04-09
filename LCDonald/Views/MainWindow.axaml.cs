using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Svg.Skia;
using Avalonia.Controls.Skia;
using Svg;
using Svg.Model;
using LCDonald.Core.Layout;
using System.Linq;
using Avalonia.Media.Imaging;
using System;
using Avalonia.Media;
using Avalonia;
using Avalonia.Platform;
using System.Timers;

namespace LCDonald.Views
{
    public partial class MainWindow : Window
    {        
        private Canvas canvas;
        
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

            //ClientSizeProperty.Changed.Subscribe(size => ScaleCanvas());
        }

        private void ScaleCanvas()
        {
            // Get the largest child of the canvas
            var largestChild = canvas.Children.OrderByDescending(c => c.Width * c.Height).First();

            // Scale the canvas so that it fits in the current window size
            var scale = Math.Min(this.Width / largestChild.Width, this.Height / largestChild.Height);
            canvas.RenderTransform = new ScaleTransform(scale, scale);
        }
        
        
    }
}
