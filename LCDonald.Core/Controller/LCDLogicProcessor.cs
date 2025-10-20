using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace LCDonald.Core.Controller
{
    public class LCDLogicProcessor: IDisposable
    {
        private readonly ILCDGame _currentGame;
        private readonly ILCDView _currentView;
        private readonly IInteropService _interopService;

        private bool _isPaused = false;
        private bool _isStopped = true;
        private Timer? _gameTimer;

        private List<ISoundStream> _soundsPlaying;
        
        public LCDLogicProcessor(ILCDGame game, ILCDView view, IInteropService platformService)
        {
            _currentGame = game;
            _currentView = view;
            _interopService = platformService;
            
            _soundsPlaying = new List<ISoundStream>();

            _currentGame.Started += StartGame;
            _currentGame.Stopped += StopGame;
            _currentGame.Paused += PauseResumeGame;
            _currentGame.Resumed += PauseResumeGame;
        }

        public bool MuteSound { get; set; }

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
            _isPaused = false;
            
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
            FlushSounds();
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
                    // Inputs
                    var currentInputs = _currentView.GetPressedInputs();
                    if (!_currentGame.IsInputBlocked())
                        _currentGame.HandleInputs(currentInputs);
                    
                    // Display
                    _currentView.UpdateDisplay(_currentGame.GetVisibleGameElements());

                    // Sound
                    var soundsToPlay = _currentGame.GetSoundsToPlay();
                    if (!MuteSound)
                        PlaySounds(soundsToPlay);
                    _soundsPlaying.RemoveAll(ss => !ss.IsPlaying);
                }
                System.Threading.Thread.Sleep(10);
            }
            FlushSounds();
        }

        private void PlaySounds(List<LCDGameSound> gameSounds)
        {
            foreach (var sound in gameSounds)
            {
                if (sound == null) continue;
                
                var soundStream = _interopService.PlayAudio(_currentGame.ShortName, sound.AudioFileName, 0.5f);
                _soundsPlaying.Add(soundStream);
            }
        }

        private void FlushSounds()
        {
            List<ISoundStream> threadSafeCopy = [.. _soundsPlaying];
            foreach (var stream in threadSafeCopy)
            {
                stream.Stop();
                stream.Dispose();
            }
        }

        public void Dispose()
        {
            _gameTimer?.Stop();
            _gameTimer?.Dispose();

            FlushSounds();
            
            _currentGame.Started -= StartGame;
            _currentGame.Stopped -= StopGame;
            _currentGame.Paused -= PauseResumeGame;
        }

    }
}
