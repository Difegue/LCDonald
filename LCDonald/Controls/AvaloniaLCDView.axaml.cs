using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Skia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using LCDonald.Core.Controller;
using LCDonald.Core.Layout;
using LCDonald.Core.Model;
using Svg;
using Svg.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LCDonald.Controls
{
    public partial class AvaloniaLCDView : UserControl, ILCDView
    {
        // View stuff
        private Canvas? _lcdCanvas;
        private SKPictureControl _svgElement;
        private SvgDocument? _svgDocument;

        // Game stuff
        private string _gameAssetFolder;
        private MAMELayout? _gameLayout;
        private LCDLogicProcessor? _logicProcessor;

        private List<string> _gameElements;
        private List<string> _visibleGameElements;
        private readonly List<LCDGameInput> _inputBuffer;

        #region DependencyProperties
        public static readonly DirectProperty<AvaloniaLCDView, ILCDGame> CurrentGameProperty =
            AvaloniaProperty.RegisterDirect<AvaloniaLCDView, ILCDGame>(
                nameof(CurrentGame),
                o => o.CurrentGame,
                (o, v) => o.CurrentGame = v);

        private ILCDGame _currentGame;
        public ILCDGame CurrentGame
        {
            get { return _currentGame; }
            set { 
                SetAndRaise(CurrentGameProperty, ref _currentGame, value); 
                if (value != null) LoadGame(); 
            }
        }

        public static readonly DirectProperty<AvaloniaLCDView, List<MAMEView>> AvailableViewsProperty =
           AvaloniaProperty.RegisterDirect<AvaloniaLCDView, List<MAMEView>>(
               nameof(AvailableViews),
               o => o.AvailableViews,
               (o, v) => o.AvailableViews = v);
        
        private List<MAMEView> _gameViews;
        public List<MAMEView> AvailableViews
        {
            get { return _gameViews; }
            set { SetAndRaise(AvailableViewsProperty, ref _gameViews, value); }
        }


        public static readonly DirectProperty<AvaloniaLCDView, MAMEView> CurrentViewProperty =
           AvaloniaProperty.RegisterDirect<AvaloniaLCDView, MAMEView>(
               nameof(CurrentView),
               o => o.CurrentView,
               (o, v) => { if (v != null) o.CurrentView = v; });

        private MAMEView _currentView;
        public MAMEView CurrentView
        {
            get { return _currentView; }
            set { 
                SetAndRaise(CurrentViewProperty, ref _currentView, value); 
                if (value != null) DrawLayoutView(_currentView); 
            }
        }
        #endregion
        
        public AvaloniaLCDView()
        {
            InitializeComponent();
            _gameViews = new List<MAMEView>();
            _gameElements = new List<string>();
            _visibleGameElements = new List<string>();
            _inputBuffer = new List<LCDGameInput>();
            
            _lcdCanvas = this.FindControl<Canvas>("LCDCanvas");
            _lcdCanvas.EffectiveViewportChanged += ComputeScale;
            KeyDown += HandleInput;
            PointerPressed += (s, e) => Focus();
            LostFocus += (s, e) => CurrentGame?.Pause();         
        }

        private void ComputeScale(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            // Get the largest child of the canvas
            var largestChild = _lcdCanvas.Children.OrderByDescending(c => c.Width * c.Height).First();

            // Scale the canvas so that it fits in the current window size
            var scale = Math.Min(Bounds.Width / largestChild.Width, Bounds.Height / largestChild.Height);
            if (scale > 0)
                _lcdCanvas.RenderTransform = new ScaleTransform(scale, scale);
        }

        private void HandleInput(object? sender, KeyEventArgs e)
        {
            _currentGame.GetAvailableInputs().ForEach(input =>
            {
                if (input.KeyCode == (int)e.Key)
                {
                    _inputBuffer.Add(input);
                }
            });
            e.Handled = true;
        }

        public List<LCDGameInput> GetPressedInputs()
        {
            // Return inputbuffer and clear it
            var inputs = new List<LCDGameInput>(_inputBuffer);
            _inputBuffer.Clear();
            return inputs;
        }

        public void UpdateDisplay(List<string> visibleElements)
        {
            if (_visibleGameElements.SequenceEqual(visibleElements))
                return; // Nothing to update

            _svgDocument.ApplyRecursive(e => {
                if (_gameElements.Contains(e.ID))
                {
                    e.Visibility = visibleElements.Contains(e.ID) ? "visible" : "hidden";
                }
            });

            _visibleGameElements = visibleElements;
            Dispatcher.UIThread.Post(() => _svgElement.Picture = new SvgSource().FromSvgDocument(_svgDocument));
        }

        private void LoadGame()
        {
            var gameID = _currentGame.ShortName;
            var appFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _gameAssetFolder = Path.Combine(appFolder, "GameAssets", gameID);

            // Clear previous game if any
            _logicProcessor?.Dispose();

            // Add handlers to focus the control when the game starts/resumes
            _currentGame.Started += (s, e) => Focus();
            _currentGame.Resumed += (s, e) => Focus();

            // Load SVG 
            // TODO: Hide all screen elements after 3 seconds to simulate LCD game init?
            _svgDocument = SvgExtensions.Open($"{_gameAssetFolder}\\{gameID}.svg");

            // Load layout
            _gameLayout = new MAMELayoutParser().Parse($"{_gameAssetFolder}\\{gameID}.lay");
            _gameElements = _currentGame.GetAllGameElements();

            // Create logic processor
            _logicProcessor = new LCDLogicProcessor(_currentGame, this);

            // Draw first view by default
            AvailableViews = _gameLayout.Views.Values.ToList();
            CurrentView = AvailableViews.First(); // TODO we shouldn't have to do this, selecting currentview is the VM's job
        }

        private void DrawLayoutView(MAMEView view)
        {
            _lcdCanvas?.Children.Clear();

            foreach (var element in view.Elements)
            {
                var elementPicture = _gameLayout?.Elements[element.Ref].Image.File;
                if (elementPicture == null) continue;
                
                // Define child Canvas element
                var imageControl = new Image
                {
                    Width = element.Width,
                    Height = element.Height,
                    Source = new Bitmap(Path.Combine(_gameAssetFolder,elementPicture))
                };
                Canvas.SetTop(imageControl, element.Y);
                Canvas.SetLeft(imageControl, element.X);

                // Add child elements to the Canvas' Children collection
                _lcdCanvas?.Children.Add(imageControl);
            }

            // Draw screen
            if (view.ScreenHeight > -1)
            {
                _svgElement = new SKPictureControl
                {
                    Width = view.ScreenWidth,
                    Height = view.ScreenHeight,
                    Picture = new SvgSource().FromSvgDocument(_svgDocument)
                };
                
                Canvas.SetTop(_svgElement, view.ScreenY);
                Canvas.SetLeft(_svgElement, view.ScreenX);

                _lcdCanvas?.Children.Add(_svgElement);
            }

            ComputeScale(this, new Avalonia.Layout.EffectiveViewportChangedEventArgs(new Rect()));
        }
    }
}
