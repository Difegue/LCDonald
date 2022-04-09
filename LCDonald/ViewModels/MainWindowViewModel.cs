using CommunityToolkit.Mvvm.ComponentModel;
using LCDonald.Core.Controller;
using LCDonald.Core.Games;
using LCDonald.Core.Layout;
using LCDonald.Core.Model;
using System.Collections.Generic;
using System.ComponentModel;

namespace LCDonald.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {

        [ObservableProperty]
        private ILCDGame _game;

        [ObservableProperty]
        private List<MAMEView> _availableViews;

        [ObservableProperty]
        private MAMEView _selectedView;

        public MainWindowViewModel()
        {
            Game = new TailsSkyAdventure();
            AvailableViews = new List<MAMEView>();
        }

        public void StartGame()
        {
            Game.Start();
        }
    }
}
