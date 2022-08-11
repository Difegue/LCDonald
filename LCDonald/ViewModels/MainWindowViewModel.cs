using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using LCDonald.Core.Controller;
using LCDonald.Core.Games;
using LCDonald.Core.Layout;
using LCDonald.Core.Model;
using LCDonald.Views;
using System;
using System.Collections.Generic;
using System.Linq;

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
                new KnucklesSoccer(),
                new ShadowGrinder(),
                new SonicSpeedway(),
                new AiAiBananaCatch(),
                new SonicExtremeBoarding(),
                new TailsSoccer(),
                new KnucklesBaseball(),
                new CreamFlowerCatch(),
                new SonicSkateboard(),
                new TailsSkyAdventure(),
                new AmyRougeTennis(),
                new CreamFlowerCatch2005(),
                new BillyGiantEgg()
            };

            // Pick a random game
            Game = _availableGames[new Random().Next(0, _availableGames.Count)];
        }

        [ObservableProperty]
        private List<ILCDGame> _availableGames;

        [ObservableProperty]
        private List<LCDGameInput> _currentInputs;

        [ObservableProperty]
        private List<MAMEView> _availableViews;

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

            _currentInputs = game.GetAvailableInputs();
        }

        partial void OnAvailableViewsChanged(List<MAMEView> views)
        {
            var viewList = views;
            if (viewList.Count > 0)
            {
                SelectedView = viewList[0];
            }
        }

        [ObservableProperty]
        private bool _isPaused;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PauseGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopGameCommand))]
        private bool _isGameRunning;

        [ObservableProperty]
        private MAMEView _selectedView;

        [RelayCommand]
        private void StartGame()
        {
            if (_isPaused)
            {
                Game.Resume();
            }
            else if (!_isGameRunning)
                Game.Start();
        }

        [RelayCommand(CanExecute = "IsGameRunning")] 
        private void PauseGame()
        {
            Game.Pause();
        }

        [RelayCommand(CanExecute = "IsGameRunning")]
        private void StopGame()
        {
            Game.Stop();
        }

        [RelayCommand]
        private void SetCurrentView(MAMEView v)
        {
            SelectedView = v;
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

    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        [Content]
        public IDataTemplate GameTemplate { get; set; }

        public IDataTemplate MacTemplate { get; set; }
        public IDataTemplate SettingsTemplate { get; set; }

        protected override IDataTemplate SelectTemplateCore(object item)
        {
            return item is LCDGameBase ? OperatingSystem.IsMacOS() ? MacTemplate : GameTemplate : SettingsTemplate;
        }
    }
}
