using LCDonald.Core.Model;
using System.Timers;

namespace LCDonald.Core.Controller
{
    public class LCDLogicProcessor
    {

        private ILCDGame _currentGame;
        private ILCDView _currentView;
        
        private bool _isPaused;
        private Timer? _gameTimer;

        public LCDLogicProcessor(ILCDGame game, ILCDView view)
        {
            _currentGame = game;
            _currentView = view;
            _isPaused = false;
        }

        public void StartGame()
        {
            _gameTimer?.Stop();
            _gameTimer?.Dispose();

            _currentGame.Start();
            _gameTimer = new Timer(_currentGame.GetGameSpeed());
            _gameTimer.Elapsed += UpdateGameState;
        }

        public void Pause()
        {
            _isPaused = true;
            _gameTimer?.Stop();
        }

        public void Resume()
        {
            _isPaused = false;
            _gameTimer?.Start();
        }

        public void GameLoop()
        {
            while (true)
            {
                if (!_isPaused)
                {
                    var currentInputs = _currentView.GetPressedInputs();
                    _currentGame.HandleInputs(currentInputs);

                    var soundsToPlay = _currentGame.GetSoundsToPlay();
                    _currentView.PlaySounds(soundsToPlay);
                }
            }
        }

        private void UpdateGameState(object sender, ElapsedEventArgs e)
        {
            _currentGame.UpdateGameState();

            // Update speed in case the game sped up
            ((Timer)sender).Interval = _currentGame.GetGameSpeed();
        }
    }
}
