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
    }
}
