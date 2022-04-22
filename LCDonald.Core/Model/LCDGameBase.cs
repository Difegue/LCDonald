using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;

namespace LCDonald.Core.Model
{
    /// <summary>
    /// Base class for LCD Game simulation with a few common elements: Animations, blinking, custom timers, and a few other things.
    /// </summary>
    public abstract class LCDGameBase : ILCDGame
    {
#pragma warning disable CS8618 
        public event EventHandler Started;
        public event EventHandler Paused;
        public event EventHandler Resumed;
        public event EventHandler Stopped;
#pragma warning restore CS8618

        protected int _customUpdateSpeed = 100;
        protected bool _isInputBlocked;
        private bool _isPaused;
        private bool _isStopped = true;
        private Timer? _customTimer;
        private List<LCDGameSound> _gameSounds = new();
        private ConcurrentDictionary<string, float> _blinkingElements = new();
        private List<List<string>>? _currentAnimation;

        public abstract string Name { get; }
        public abstract string ShortName { get; }
        public abstract void InitializeGameState();
        public abstract List<LCDGameInput> GetAvailableInputs();
        public abstract void HandleInputs(List<LCDGameInput> pressedInputs);
        public abstract List<string> GetAllGameElements();

        /// <summary>
        /// Called according to _customUpdateSpeed.
        /// Use to update enemy movement or other slower game logic.
        /// </summary>
        public abstract void CustomUpdate();

        public bool IsInputBlocked() => _isInputBlocked;

        public List<LCDGameSound> GetSoundsToPlay()
        {
            // Copy _gameSounds, clear it and return the copy
            List<LCDGameSound> soundsToPlay = new List<LCDGameSound>(_gameSounds);
            _gameSounds.Clear();
            return soundsToPlay;
        }

        protected void QueueSound(LCDGameSound sound)
        {
            _gameSounds.Add(sound);
        }

        protected bool IsBlinking(string element) => _blinkingElements.ContainsKey(element);
        
        /// <summary>
        /// Set a given SVG element to blink X times.
        /// Blinking elements will draw regardless of what the game implementation sends.
        /// </summary>
        /// <param name="element">The SVG group name</param>
        /// <param name="times">Number of blink cycles</param>
        protected void BlinkElement(string element, int times)
        {
            // Reset blinkcount in case the key is still there
            _blinkingElements.AddOrUpdate(element, times, (key, oldValue) => times);
        }

        /// <summary>
        /// Sets a given sequence of visible elements to be played as an animation.
        /// Animations block input until they are finished.
        /// This method blocks until the animation is complete, so you should probably only call this in CustomUpdate blocks.
        /// </summary>
        /// <param name="animation">Sequence of visible elements to play.</param>
        protected void PlayAnimation(List<List<string>> animation)
        {
            _isInputBlocked = true;

            _currentAnimation = animation;

            // Wait for animation to be null again
            _customTimer?.Stop();
            while (_currentAnimation != null)
            {
                System.Threading.Thread.Sleep(100);
            }
            _customTimer?.Start();
            
            _isInputBlocked = false;
        }
        
        public void Start()
        {
            _isInputBlocked = false;
            InitializeGameState();

            _customTimer?.Stop();
            _customTimer?.Dispose();

            _customTimer = new Timer(_customUpdateSpeed);
            _customTimer.Elapsed += UpdateGameState;
            _customTimer?.Start();

            _isStopped = false;
            Started?.Invoke(this, new EventArgs());
        }

        public void Pause()
        {
            if (!_isPaused && !_isStopped)
            {
                _isPaused = true;
                _customTimer?.Stop();
                Paused?.Invoke(this, new EventArgs());
            }
        }

        public void Resume()
        {
            if (_isPaused && !_isStopped)
            {
                _isPaused = false;
                _customTimer?.Start();
                Resumed?.Invoke(this, new EventArgs());
            }
        }

        public void Stop()
        {
            _isStopped = true;
            _customTimer?.Stop();
            Stopped?.Invoke(this, new EventArgs());
        }

        public void Update()
        {
            UpdateCore();

            foreach (var element in _blinkingElements.Keys)
            {
                // Reduce count by 0.5 so we have two frames per blink cycle
                _blinkingElements[element] -= 0.5f;

                if (_blinkingElements[element] < 0)
                {
                    _blinkingElements.Remove(element, out _);
                }
            }

            // If there's an animation, go to its next frame
            _currentAnimation?.RemoveAt(0);
            if (_currentAnimation?.Count == 0)
                _currentAnimation = null;
        }

        public List<string> GetVisibleGameElements() 
        {
            // If there's an animation, disregard the game and play it directly
            if (_currentAnimation != null)
            {
                return _currentAnimation[0];
            }

            // Get the shown elements from the game implementation
            var visibleElements = GetVisibleElements();

            // Add elements that are blinking, remove the ones that aren't
            foreach (var element in _blinkingElements.Keys)
            {
                var isVisible = _blinkingElements[element] % 1 != 0.5;

                if (isVisible)
                    visibleElements.Add(element);
                else
                    visibleElements.Remove(element);
            }

            return visibleElements;
        }

        /// <summary>
        /// Called every frame. Returns the currently visible elements of the LCD game.
        /// Elements you return here will be overridden by the blinking elements.
        /// </summary>
        /// <returns>A list of SVG groups to make visible when displaying the screen</returns>
        protected abstract List<string> GetVisibleElements();

        /// <summary>
        /// Compute and update the next game state.
        /// Is called every 100 milliseconds.
        /// </summary>
        protected abstract void UpdateCore();
        
        private void UpdateGameState(object sender, ElapsedEventArgs e)
        {
            CustomUpdate();

            // Update speed in case the game sped it up
            ((Timer)sender).Interval = _customUpdateSpeed;
        }
    }
}
