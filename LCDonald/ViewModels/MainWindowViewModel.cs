using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCDonald.Core.Games;
using LCDonald.Core.Layout;
using LCDonald.Core.Model;
using System;
using System.Collections.Generic;

namespace LCDonald.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            _availableGames = new List<ILCDGame>
            {
                new SonicActionGame(),
                new TailsSkyPatrol(),
                new SonicSpeedway(),
                new TailsSkyAdventure()
            };

            // Pick a random game
            Game = _availableGames[new Random().Next(0, _availableGames.Count)];
        }

        [ObservableProperty]
        private List<ILCDGame> _availableGames;

        [ObservableProperty]
        private ILCDGame _game;

        partial void OnGameChanging(ILCDGame game)
        {
            if (Game != null)
            {
                Game.Stop();
                Game.Started -= Game_Started;
                Game.Paused -= Game_Paused;
                Game.Resumed -= Game_Resumed;
                Game.Stopped -= Game_Stopped;
            }

            game.Started += Game_Started;
            game.Paused += Game_Paused;
            game.Resumed += Game_Resumed;
            game.Stopped += Game_Stopped;
        }

        [ObservableProperty]
        private bool _isPaused;

        [ObservableProperty]
        private bool _isGameRunning;

        [ObservableProperty]
        private MAMEView _selectedView;
        
        [ICommand]
        private void StartGame()
        {
            if (_isPaused)
            {
                Game.Resume();
            }
            else if (!_isGameRunning)
                Game.Start();
        }

        [ICommand] // TODO (CanExecute = "IsGameRunning") doesn't work
        private void PauseGame()
        {  
            Game.Pause();
        }

        [ICommand] // TODO (CanExecute = "IsGameRunning") doesn't work
        private void StopGame()
        {
            Game.Stop();
        }

        private void Game_Started(object? sender, System.EventArgs e)
        {
            IsGameRunning = true;
        }
        private void Game_Paused(object? sender, EventArgs e)
        {
            IsPaused = true;
        }
        private void Game_Resumed(object? sender, EventArgs e)
        {
            IsPaused = false;
        }
        private void Game_Stopped(object? sender, EventArgs e)
        {
            IsGameRunning = false;
            IsPaused = false;
        }
    }
}
