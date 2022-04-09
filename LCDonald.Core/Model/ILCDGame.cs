using System;
using System.Collections.Generic;
using System.Text;

namespace LCDonald.Core.Model
{
    public interface ILCDGame
    {
        public abstract string GetGameName();
        
        /// <summary>
        /// Get the Asset folder name of the game.
        /// </summary>
        /// <returns></returns>
        public abstract string GetAssetFolderName();

        /// <summary>
        /// Called every frame. 
        /// When the game wants to play a sound, it should return it through this method.
        /// </summary>
        /// <returns>A list of sounds to play</returns>
        public List<LCDGameSound> GetSoundsToPlay();

        /// <summary>
        /// Called every frame. Can update the game state.
        /// Inputs are only handled once -> Keeping a button pressed isn't recorded
        /// </summary>
        /// <param name="pressedInputs">Newly pressed inputs for this frame</param>
        public void HandleInputs(List<LCDGameInput> pressedInputs);

        /// <summary>
        /// Returns all the existing elements of the LCD game.
        /// </summary>
        /// <returns>A list of SVG group names matching the elements of the LCD</returns>
        public List<string> GetAllGameElements();        

        /// <summary>
        /// Called every frame. Returns the currently visible elements of the LCD game.
        /// </summary>
        /// <returns>A list of SVG groups to make visible when displaying the screen</returns>
        public List<string> GetVisibleGameElements();

        /// <summary>
        /// Compute and update the next game state.
        /// Is called every X milliseconds, X being the current game speed.
        /// </summary>
        public void UpdateGameState();

        /// <summary>
        /// Get the current rate at which UpdateGameState has to be called.
        /// </summary>
        /// <returns>The update rate</returns>
        public int GetGameSpeed();

        /// <summary>
        /// Get all the inputs this game uses.
        /// </summary>
        /// <returns>a list of inputs.</returns>
        public List<LCDGameInput> GetAvailableInputs();

        /// <summary>
        /// (Re)start the game, initializing the gamestate.
        /// </summary>
        public void Start();

        event EventHandler Started;
        event EventHandler Paused;
        event EventHandler Stopped;

    }

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
        public abstract void UpdateGameState();
        public abstract List<LCDGameInput> GetAvailableInputs();
        public abstract void HandleInputs(List<LCDGameInput> pressedInputs);
        public abstract List<string> GetAllGameElements();
        public abstract List<string> GetVisibleGameElements();

        protected int _gameSpeed = 100;
        public int GetGameSpeed() => _gameSpeed;

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

        public void Start()
        {
            InitializeGameState();
            Started?.Invoke(this, new EventArgs());
        }

        public void PauseResume()
        {
            Paused?.Invoke(this, new EventArgs());
        }

        public void Stop() 
        {
            Stopped?.Invoke(this, new EventArgs());
        }
    }
}
