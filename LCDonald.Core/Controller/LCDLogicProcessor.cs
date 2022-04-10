using LCDonald.Core.Model;
using SharpAudio;
using SharpAudio.Codec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace LCDonald.Core.Controller
{
    public class LCDLogicProcessor: IDisposable
    {
        private readonly ILCDGame _currentGame;
        private readonly ILCDView _currentView;
        private readonly AudioEngine _gameAudio;

        private bool _isPaused;
        private bool _isStopped;
        private Timer? _gameTimer;
        
        public LCDLogicProcessor(ILCDGame game, ILCDView view)
        {
            _currentGame = game;
            _currentView = view;
            _isPaused = false;

            _gameAudio = AudioEngine.CreateDefault();

            _currentGame.Started += StartGame;
            _currentGame.Stopped += StopGame;
            _currentGame.Paused += PauseResumeGame;
        }

        private void StartGame(object sender, EventArgs e)
        {
            _gameTimer?.Stop();
            _gameTimer?.Dispose();

            // Flush input buffer
            _currentView.GetPressedInputs();

            _gameTimer = new Timer(100);
            _gameTimer.Elapsed += (s, e) => _currentGame.Update();
            _gameTimer.Start();

            _isStopped = false;
            
            Task.Run(() => GameLoop());
        }


        private void StopGame(object sender, EventArgs e) => _isStopped = true;

        private void PauseResumeGame(object sender, EventArgs e)
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }            

        private void Pause()
        {
            _isPaused = true;
            _gameTimer?.Stop();
        }

        private void Resume()
        {
            _isPaused = false;
            _gameTimer?.Start();
        }

        private void GameLoop()
        {
            while (!_isStopped)
            {
                if (!_isPaused)
                {
                    var currentInputs = _currentView.GetPressedInputs();
                    _currentGame.HandleInputs(currentInputs);

                    _currentView.UpdateDisplay(_currentGame.GetVisibleGameElements());

                    PlaySounds(_currentGame.GetSoundsToPlay());
                }
                System.Threading.Thread.Sleep(10);
            }
        }

        private void PlaySounds(List<LCDGameSound> gameSounds)
        {
            foreach (var sound in gameSounds)
            {
                // TODO preload files into memory
                var soundFile = File.OpenRead(sound.AudioFileName);
                var soundStream = new SoundStream(soundFile, _gameAudio)
                {
                    Volume = 0.5f
                };
                soundStream.Play();
            }
        }

        public void Dispose()
        {
            _gameTimer?.Stop();
            _gameTimer?.Dispose();
            _gameAudio.Dispose();
            
            _currentGame.Started -= StartGame;
            _currentGame.Stopped -= StopGame;
            _currentGame.Paused -= PauseResumeGame;
        }

    }
}
