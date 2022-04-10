using System;
using System.Collections.Generic;

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
        /// Is called every 100 milliseconds.
        /// </summary>
        public void Update();

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

}
