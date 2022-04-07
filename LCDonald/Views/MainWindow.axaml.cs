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
        private SKPictureControl svgElement;
        
        private Canvas canvas;
        private SvgDocument svgDocument;

        private MAMELayout gameLayout;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            svgDocument = SvgExtensions.Open("F:\\Projects\\LCDonald\\LCDonald\\Assets\\GameAssets\\tskyadventure\\tskyadventure.svg");

            svgDocument.ApplyRecursive(e => {
                if (e.ID == "life-1") 
                    e.Visibility = "hidden";
            });
            
            canvas = this.FindControl<Canvas>("CanvasTest");

            var parser = new MAMELayoutParser();
            gameLayout = parser.Parse("F:\\Projects\\LCDonald\\LCDonald\\Assets\\GameAssets\\tskyadventure\\tskyadventure.lay");
            DrawLayoutView("Front_Open");
            ClientSizeProperty.Changed.Subscribe(size => ScaleCanvas());
        }

        private void ScaleCanvas()
        {
            // Get the largest child of the canvas
            var largestChild = canvas.Children.OrderByDescending(c => c.Width * c.Height).First();

            // Scale the canvas so that it fits in the current window size
            var scale = Math.Min(this.Width / largestChild.Width, this.Height / largestChild.Height);
            canvas.RenderTransform = new ScaleTransform(scale, scale);
        }
        
        private void DrawLayoutView(string viewName)
        {
            var assetFolder = "F:\\Projects\\LCDonald\\LCDonald\\Assets\\GameAssets\\tskyadventure\\";
            canvas.Children.Clear();

            var view = gameLayout.Views.Where(v => v.Key == viewName)
                        .Select(v => v.Value).FirstOrDefault();
            if (view == null)
                return;

            foreach (var element in view.Elements)
            {
                var elementPicture = gameLayout.Elements[element.Ref].Image.File;

                // Define child Canvas element
                var imageControl = new Image
                {
                    Width = element.Width,
                    Height = element.Height,
                    Source = new Bitmap(assetFolder + elementPicture)
                };
                Canvas.SetTop(imageControl, element.Y);
                Canvas.SetLeft(imageControl, element.X);

                // Add child elements to the Canvas' Children collection
                canvas.Children.Add(imageControl);
            }

            // Draw screen
            if (view.ScreenHeight > -1)
            {
                svgElement = new SKPictureControl
                {
                    Width = view.ScreenWidth,
                    Height = view.ScreenHeight,
                    Picture = new SvgSource().FromSvgDocument(svgDocument)
                };
                Canvas.SetTop(svgElement, view.ScreenY);
                Canvas.SetLeft(svgElement, view.ScreenX);

                canvas.Children.Add(svgElement);
            }
        }
    }
}
