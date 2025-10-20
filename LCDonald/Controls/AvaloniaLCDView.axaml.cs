using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Skia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using LCDonald.Core.Controller;
using LCDonald.Core.Layout;
using LCDonald.Core.Model;
using LCDonald.ViewModels;
using Svg;
using Svg.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LCDonald.Controls
{
    public partial class AvaloniaLCDView : UserControl, ILCDView
    {
        private IInteropService _interopService;

        // View stuff
        private SKPictureControl _svgElement;
        private SvgDocument? _svgDocument;

        // Game stuff
        private MAMELayout? _gameLayout;
        private LCDLogicProcessor? _logicProcessor;

        private List<string> _gameElements;
        private List<string> _visibleGameElements;
        private int _currentScreenIndex = -1;
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

            _interopService = Ioc.Default.GetRequiredService<IInteropService>();
            
            LCDCanvas.EffectiveViewportChanged += ComputeScale;
            ScaleSlider.PropertyChanged += ForceScale;
            ZoomBorder.ZoomChanged += HandleZoom;

            KeyDown += HandleInput;
            KeyUp += ClearInputs;
            PointerPressed += HandleMouseInput;
            LostFocus += (s, e) => CurrentGame?.Pause();         
        }

        private void HandleScrollImage(object? sender, PointerWheelEventArgs e)
        {
            // Intercept normal scroll events in case we're maxed out
            if (e.Delta.Y > 0 && ZoomBorder.ZoomX == ZoomBorder.MaxZoomX || e.Delta.Y < 0 && ZoomBorder.ZoomX == ZoomBorder.MinZoomX)
            {
                e.Handled = true;
                return;
            }
        }

        private void HandleScrollBar(object? sender, PointerWheelEventArgs e)
        {
            if (e.Delta.Y > 0 && ZoomBorder.ZoomX < ZoomBorder.MaxZoomX)
                ZoomBorder.ZoomIn();

            if (e.Delta.Y < 0 && ZoomBorder.ZoomX > ZoomBorder.MinZoomX)
                ZoomBorder.ZoomOut();
        }

        private void ZoomIn(object? sender, PointerPressedEventArgs e) => ZoomBorder.ZoomIn();
        private void ZoomOut(object? sender, PointerPressedEventArgs e) => ZoomBorder.ZoomOut();

        private void HandleZoom(object sender, ZoomChangedEventArgs e)
        {
            if (ZoomBorder.ZoomX != ScaleSlider.Value)
                ScaleSlider.Value = ZoomBorder.ZoomX;
        }

        private void ForceScale(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Value")
            {
                ZoomBorder.Zoom(ScaleSlider.Value, 0,0);
            }
        }
        private void ComputeScale(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            if (LCDCanvas.Children.Count == 0) return;

            // Get the largest child of the canvas
            var largestChild = LCDCanvas.Children.OrderByDescending(c => c.Width * c.Height).First();

            // Scale the canvas so that it fits in the current window size
            var scale = Math.Min(Bounds.Width / largestChild.Width, Bounds.Height / largestChild.Height);
            if (scale > 0)
            {
                ScaleSlider.Value = scale;
            }
        }

        private List<Key> _bufferedKeyCodes = new();
        private void HandleInput(object? sender, KeyEventArgs e)
        {
            // Prevent holding the key down from spamming the input buffer
            if (!_bufferedKeyCodes.Contains(e.Key))
                _currentGame.GetAvailableInputs().ForEach(input =>
                {
                    if (input.KeyCode == (int)e.Key)
                    {
                        _inputBuffer.Add(input);
                        _bufferedKeyCodes.Add(e.Key);
                    }
                });
            e.Handled = true;
        }

        private void HandleMouseInput(object? sender, PointerPressedEventArgs e)
        {
            Focus();

            // Check if the mouse is over a game element
            var mousePos = e.GetPosition(LCDCanvas);

            // Look in the gameElements if there's a button whose bounds contain this position
            var element = _currentView.Elements.FirstOrDefault(el => el.InputTag != null && 
                            el.X <= mousePos.X && el.Y <= mousePos.Y && el.X + el.Width >= mousePos.X && el.Y + el.Height >= mousePos.Y);

            if (element != null)
            {
                // Get the input associated with this element
                var input = _currentGame.GetAvailableInputs().FirstOrDefault(i => i.Name == element.InputTag);
                if (input != null)
                {
                    _inputBuffer.Add(input);
                }
            }
        }


        private void ClearInputs(object? sender, KeyEventArgs e)
        {
            _bufferedKeyCodes.Clear();
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
            if (_svgDocument == null || _visibleGameElements.SequenceEqual(visibleElements))
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

            // Clear previous game if any
            _logicProcessor?.Dispose();
            _visibleGameElements.Clear(); 
            _currentScreenIndex = 0;

            // Add handlers to focus the control when the game starts/resumes
            _currentGame.Started += (s, e) => Focus();
            _currentGame.Resumed += (s, e) => Focus();

            _svgDocument = SvgExtensions.Open(_interopService.GetGameAsset(gameID, $"{gameID}_0.svg"));

            // Load layout
            _gameLayout = new MAMELayoutParser().Parse(_interopService.GetGameAsset(gameID, $"{gameID}.lay"));

#if BURGER
            // Additional whitelabel elements available in all games
            // Adding them here ensures they'll be hidden alongside the rest when inactive.  
            _gameElements = ["win", "score-0", "score-1", .. _currentGame.GetAllGameElements() ];
#else
            _gameElements = _currentGame.GetAllGameElements();
#endif

            // Create logic processor
            _logicProcessor = new LCDLogicProcessor(_currentGame, this, _interopService);
            
            // Memes
            if (!SettingsViewModel.CurrentSettings.MuteSound && 
                (gameID == "artennis" || gameID == "cflower" || gameID == "kbaseball" || gameID == "sbasketball" || gameID == "sskateboard" || gameID == "tsoccer"))
                _interopService.PlayAudio("memes", gameID + ".ogg", 0.6f);

            // View selection is handled by the viewmodel hosting this control
            AvailableViews = _gameLayout.Views.Values.ToList();
        }

        private void DrawLayoutView(MAMEView view)
        {
            if (_logicProcessor != null)
                _logicProcessor.MuteSound = SettingsViewModel.CurrentSettings.MuteSound;
            
            LCDCanvas?.Children.Clear();

            // (Re)load SVG if a screen element is present
            var gameID = _currentGame.ShortName;
            if (view.ScreenIndex > -1 && view.ScreenIndex != _currentScreenIndex)
            {
                _currentScreenIndex = view.ScreenIndex;
                _visibleGameElements.Clear(); // Force a redraw
                _svgDocument = SvgExtensions.Open(_interopService.GetGameAsset(gameID, $"{gameID}_{view.ScreenIndex}.svg"));
            }

            foreach (var element in view.Elements)
            {
                if (_gameLayout == null) continue;
                if (!_gameLayout.Elements.ContainsKey(element.Ref)) continue;

                var elementPicture = _gameLayout.Elements[element.Ref].Image.File;
                if (elementPicture == null) continue;

                // This is jank af
                var darkenBackground = view.Name.ToLower().Contains("front") && element.Ref.ToLower().Contains("bg") && SettingsViewModel.CurrentSettings.DarkenGameBackgrounds;

                // Define child Canvas element
                var asset = _interopService.GetGameAsset(_currentGame.ShortName, elementPicture);
                var imageControl = new Image
                {
                    Width = element.Width,
                    Height = element.Height,
                    Opacity = darkenBackground ? 0.6 : 1,
                    Source = new Bitmap(asset)
                };
                Canvas.SetTop(imageControl, element.Y);
                Canvas.SetLeft(imageControl, element.X);

                // Add child elements to the Canvas' Children collection
                LCDCanvas?.Children.Add(imageControl);
            }

            // Draw screen
            if (view.ScreenHeight > -1)
            {
                // Draw a second SVG layer for the shadow if enabled
                if (SettingsViewModel.CurrentSettings.DrawLCDShadows)
                {
                    // TODO: This is surprisingly difficult to make unless https://github.com/wieslawsoltes/Svg.Skia/discussions/82 happens
                }

                _svgElement = new SKPictureControl
                {
                    Width = view.ScreenWidth,
                    Height = view.ScreenHeight,
                    Picture = new SvgSource().FromSvgDocument(_svgDocument)
                };
                
                Canvas.SetTop(_svgElement, view.ScreenY);
                Canvas.SetLeft(_svgElement, view.ScreenX);

                LCDCanvas?.Children.Add(_svgElement);
                UpdateDisplay(_currentGame.GetVisibleGameElements());
            }

            ComputeScale(this, new Avalonia.Layout.EffectiveViewportChangedEventArgs(new Rect()));
        }
    }
}
