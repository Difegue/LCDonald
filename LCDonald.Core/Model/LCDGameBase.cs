using System;
using System.Collections.Generic;
using System.Timers;

namespace LCDonald.Core.Model
{
    public abstract class LCDGameBase : ILCDGame
    {
#pragma warning disable CS8618 
        public event EventHandler Started;
        public event EventHandler Paused;
        public event EventHandler Stopped;
#pragma warning restore CS8618

        public abstract string GetGameName();
        public abstract string GetAssetFolderName();
        public abstract void InitializeGameState();
        public abstract void Update();
        public abstract List<LCDGameInput> GetAvailableInputs();
        public abstract void HandleInputs(List<LCDGameInput> pressedInputs);
        public abstract List<string> GetAllGameElements();
        public abstract List<string> GetVisibleGameElements();

        protected int _customUpdateSpeed = 100;
        /// <summary>
        /// Called according to _customUpdateSpeed.
        /// Use to update enemy movement or other slower game logic.
        /// </summary>
        public abstract void CustomUpdate();

        private List<LCDGameSound> _gameSounds = new();
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

        private Timer? _customTimer;
        public void Start()
        {
            InitializeGameState();

            _customTimer?.Stop();
            _customTimer?.Dispose();

            _customTimer = new Timer(_customUpdateSpeed);
            _customTimer.Elapsed += UpdateGameState;
            _customTimer?.Start();

            Started?.Invoke(this, new EventArgs());
        }

        private bool _isPaused;
        public void PauseResume()
        {
            if (_isPaused)
            {
                _isPaused = false;
                _customTimer?.Start();
            }
            else
            {
                _isPaused = true;
                _customTimer?.Stop();
            }
            Paused?.Invoke(this, new EventArgs());
        }

        public void Stop()
        {
            _customTimer?.Stop();
            _customTimer?.Dispose();
            Stopped?.Invoke(this, new EventArgs());
        }

        private void UpdateGameState(object sender, ElapsedEventArgs e)
        {
            CustomUpdate();

            // Update speed in case the game sped it up
            ((Timer)sender).Interval = _customUpdateSpeed;
        }
    }
}
