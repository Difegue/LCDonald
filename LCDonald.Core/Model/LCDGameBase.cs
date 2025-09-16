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
        public event EventHandler Scored;
#pragma warning restore CS8618
        
        protected int _customUpdateSpeed = 100;
        protected int _blockedCustomUpdates;
        protected bool _isInputBlocked;
        protected bool _isEndlessMode;

#if BURGER
        protected bool _showHiddenGroup;
#endif

        protected Random _rng = new();
        
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
        /// This method blocks until the animation is complete, so you should probably only call this in CustomUpdate/UpdateCore blocks.
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

        /// <summary>
        /// Play the startup music for the game.
        /// This will block inputs for the given amount of cycles.
        /// </summary>
        /// <param name="musicFile">path to music file</param>
        /// <param name="blockInputCycles">How many cycles input should be blocked for. 1x custom loop is usually the correct speed.</param>
        protected void StartupMusic(string musicFile = "../common/game_start.ogg", int blockInputCycles = 1)
        {
            QueueSound(new LCDGameSound(musicFile));

            // Wait for sound to end before running custom updates
            _blockedCustomUpdates = blockInputCycles;
            _isInputBlocked = true;
        }

        /// <summary>
        /// Block custom updates for a given amount of cycles.
        /// </summary>
        /// <param name="blockedUpdates"></param>
        protected void BlockCustomUpdates(int blockedUpdates) => _blockedCustomUpdates = blockedUpdates;

        /// <summary>
        /// Play a generic game over animation: Slow 4x blink of the given elements.
        /// </summary>
        /// <param name="elementsToBlink">List of SVG elements to blink</param>
        protected void GenericGameOverAnimation(List<string> elementsToBlink, string animationSound = "../common/game_over.ogg")
        {
            QueueSound(new LCDGameSound(animationSound));

            var gameOverFrame1 = elementsToBlink;
            var gameOverFrame2 = new List<string>();

            // slow 4x blink
            var gameOverAnimation = new List<List<string>> { gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2};
            PlayAnimation(gameOverAnimation);
        }

        /// <summary>
        /// Play a generic victory animation: 3x blink of the given elements, then 10x blink of all elements.
        /// </summary>
        /// <param name="elementsToBlink">List of SVG elements to blink</param>
        protected void GenericVictoryAnimation(List<string> elementsToBlink)
        {
            QueueSound(new LCDGameSound("../common/game_win.ogg"));

            // 3x + 10x all
            var victoryFrame1 = elementsToBlink;
            var victoryFrame2 = new List<string>();
            var victoryFrame3 = GetAllGameElements();

#if BURGER
            // Add additional "win" images to display during a win
            victoryFrame3.Add("win");
#endif

            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2};
            PlayAnimation(victoryAnimation);
        }

        public void Start(bool isEndless = false)
        {
            _isInputBlocked = false;
            _isEndlessMode = isEndless;
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
            _isPaused = false;
            _customTimer?.Stop();
            _currentAnimation = null;

            Stopped?.Invoke(this, new EventArgs());
        }

        public void IncrementScore() => Scored?.Invoke(this, new EventArgs());

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
            // TODO: Hide all screen elements after 3 seconds to simulate LCD game init?
            if (_isStopped)
                return GetAllGameElements();

            // If there's an animation, disregard the game and play it directly
            if (_currentAnimation != null)
            {
                return _currentAnimation[0];
            }

            // Get the shown elements from the game implementation
            var visibleElements = GetVisibleElements();

#if BURGER
            if (_showHiddenGroup)
                visibleElements.Add("score-1");
#endif

            // Add elements that are blinking, remove the ones that aren't
            foreach (var element in _blinkingElements.Keys)
            {
                if (_blinkingElements.TryGetValue(element, out var value)) {
                    var isVisible = value % 1 != 0.5;

                    if (isVisible)
                        visibleElements.Add(element);
                    else
                        visibleElements.Remove(element);
                }
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
            if (_blockedCustomUpdates > 0)
            {
                _blockedCustomUpdates--;
            }
            else
            {
                _isInputBlocked = false;
                CustomUpdate();
            }

#if BURGER
            // 10% chance on every update to show the hidden group
            _showHiddenGroup = _rng.Next(1, 10) == 1;
#endif

            // Update speed in case the game sped it up
            ((Timer)sender).Interval = _customUpdateSpeed;
        }
    }
}
