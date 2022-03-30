using System;
using System.Collections.Generic;
using System.Text;

namespace LCDonald.Core.Model
{
    public interface ILCDGame
    {
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
        /// Called every frame. Returns the currently visible elements of the LCD game.
        /// </summary>
        /// <returns>A list of SVG groups to make visible when displaying the screen</returns>
        public List<LCDGameElement> GetVisibleGameElements();

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
        public List<LCDGameInput> GetInputs();

        /// <summary>
        /// (Re)start the game, initializing the gamestate.
        /// </summary>
        public void Start();

    }

    public class LCDGameBase : ILCDGame
    {
        public int GetGameSpeed()
        {
            throw new NotImplementedException();
        }

        public List<LCDGameInput> GetInputs()
        {
            throw new NotImplementedException();
        }

        public List<LCDGameSound> GetSoundsToPlay()
        {
            throw new NotImplementedException();
        }

        public List<LCDGameElement> GetVisibleGameElements()
        {
            throw new NotImplementedException();
        }

        public void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void UpdateGameState()
        {
            throw new NotImplementedException();
        }
    }
}
