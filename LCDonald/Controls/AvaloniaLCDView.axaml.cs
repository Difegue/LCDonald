using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Skia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using LCDonald.Core.Controller;
using LCDonald.Core.Layout;
using LCDonald.Core.Model;
using Svg;
using Svg.Model;
using System.Collections.Generic;
using System.Linq;

namespace LCDonald.Controls
{
    public partial class AvaloniaLCDView : UserControl, ILCDView
    {
        // View stuff
        private Canvas? _lcdCanvas;
        private SKPictureControl _svgElement;
        private SvgDocument? _svgDocument;

        // Game stuff
        private string? _gameAssetFolder;
        private MAMELayout? _gameLayout;
        private LCDLogicProcessor? _logicProcessor;

        private List<string> _gameElements;
        private List<string> _visibleGameElements;
        private readonly List<LCDGameInput> _inputBuffer;

        public static readonly DirectProperty<AvaloniaLCDView, ILCDGame> CurrentGameProperty =
            AvaloniaProperty.RegisterDirect<AvaloniaLCDView, ILCDGame>(
                nameof(CurrentGame),
                o => o.CurrentGame,
                (o, v) => o.CurrentGame = v);

        private ILCDGame _currentGame;
        public ILCDGame CurrentGame
        {
            get { return _currentGame; }
            set { SetAndRaise(CurrentGameProperty, ref _currentGame, value); if (value != null) LoadGame(); }
        }

        public static readonly DirectProperty<AvaloniaLCDView, MAMEView> CurrentViewProperty =
           AvaloniaProperty.RegisterDirect<AvaloniaLCDView, MAMEView>(
               nameof(CurrentView),
               o => o.CurrentView,
               (o, v) => o.CurrentView = v);

        private MAMEView _currentView;
        public MAMEView CurrentView
        {
            get { return _currentView; }
            set { SetAndRaise(CurrentViewProperty, ref _currentView, value); if (value != null) DrawLayoutView(_currentView); }
        }

        public AvaloniaLCDView()
        {
            InitializeComponent();
            _visibleGameElements = new List<string>();
            _inputBuffer = new List<LCDGameInput>();
            _lcdCanvas = this.FindControl<Canvas>("LCDCanvas");
            KeyDown += HandleInput;
            PointerPressed += (s, e) => Focus();
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
            var gameID = _currentGame.GetAssetFolderName();
            _gameAssetFolder = "F:\\Projects\\LCDonald\\LCDonald\\Assets\\GameAssets\\tskyadventure\\"; //TODO

            // Clear previous game if any
            _logicProcessor?.Dispose();

            // Load SVG 
            _svgDocument = SvgExtensions.Open($"{_gameAssetFolder}\\{gameID}.svg");

            // Load layout
            _gameLayout = new MAMELayoutParser().Parse($"{_gameAssetFolder}\\{gameID}.lay");
            _gameElements = _currentGame.GetAllGameElements();

            // Create logic processor
            _logicProcessor = new LCDLogicProcessor(_currentGame, this);

            // Draw first view by default
            CurrentView = _gameLayout.Views.Values.First();
            DrawLayoutView(CurrentView);
        }

        private void DrawLayoutView(MAMEView view)
        {
            _lcdCanvas?.Children.Clear();

            foreach (var element in view.Elements)
            {
                var elementPicture = _gameLayout?.Elements[element.Ref].Image.File;
                
                // Define child Canvas element
                var imageControl = new Image
                {
                    Width = element.Width,
                    Height = element.Height,
                    Source = new Bitmap(_gameAssetFolder + elementPicture)
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
        }
    }
}
