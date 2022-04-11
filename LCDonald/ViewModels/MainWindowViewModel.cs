using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCDonald.Core.Controller;
using LCDonald.Core.Games;
using LCDonald.Core.Layout;
using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LCDonald.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {

        [ObservableProperty]
        private ILCDGame _game;

        partial void OnGameChanging(ILCDGame game)
        {
            if (Game != null)
            {
                Game.Stop();
                Game.Started -= Game_Started;
                Game.Paused -= Game_Paused;
                Game.Stopped -= Game_Stopped;
            }

            game.Started += Game_Started;
            game.Paused += Game_Paused;
            game.Stopped += Game_Stopped;
        }

        [ObservableProperty]
        private bool _isPaused;
        
        [ObservableProperty]
        private bool _isGameRunning;
        
        [ObservableProperty]
        private List<MAMEView> _availableViews;

        [ObservableProperty]
        private MAMEView _selectedView;

        public MainWindowViewModel()
        {
            // TODO
            Game = new TailsSkyAdventure();
            AvailableViews = new List<MAMEView>();
        }

        [ICommand]
        private void StartGame()
        {
            if (_isPaused)
            {
                Game.PauseResume();
            }
            else if (!_isGameRunning)
                Game.Start();
        }

        [ICommand(CanExecute = "IsGameRunning")]
        private void PauseGame()
        {  
            Game.PauseResume();
        }

        [ICommand(CanExecute = "IsGameRunning")]
        private void StopGame()
        {
            Game.Stop();
        }

        private void Game_Started(object? sender, System.EventArgs e)
        {
            _isGameRunning = true;
        }
        private void Game_Paused(object? sender, EventArgs e)
        {
            _isPaused = !_isPaused;
        }
        private void Game_Stopped(object? sender, EventArgs e)
        {
            _isGameRunning = false;
        }
    }
}
