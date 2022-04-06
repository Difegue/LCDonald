using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Svg.Skia;
using Avalonia.Controls.Skia;
using Svg;
using Svg.Model;
using SS = Svg.Skia;
using SkiaSharp;

namespace LCDonald.Views
{
    public partial class MainWindow : Window
    {
        private SKPictureControl svgElement;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var svgDocument = SvgExtensions.Open("F:\\Projects\\LCDonald\\LCDonald\\Assets\\GameAssets\\tskyadventure\\tskyadventure.svg");


            svgDocument.ApplyRecursive(e => {
                if (e.ID == "life-1") 
                    e.Visibility = "hidden";
            });
            svgElement = this.FindControl<SKPictureControl>("SvgTest");
            
            var svgSource = new SvgSource();
            svgElement.Picture = svgSource.FromSvgDocument(svgDocument);
                
                  

        }
    }
}
