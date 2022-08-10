using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDonald.ViewModels
{
    public class Settings
    {
        public bool DarkenGameBackgrounds { get; set; }
        
        public bool DrawLCDShadows { get; set; }
    }

    public partial class SettingsViewModel: ObservableObject
    {
        private static string SETTINGS_FILE = "lcdconf.json";
        public static Settings CurrentSettings;

        public SettingsViewModel()
        {
            CurrentSettings = new Settings();
            LoadSettings();

            _darkenGameBackgrounds = CurrentSettings.DarkenGameBackgrounds;
            _drawLCDShadows = CurrentSettings.DrawLCDShadows;
        }

        [ObservableProperty]
        private bool _darkenGameBackgrounds;

        [ObservableProperty]
        private bool _drawLCDShadows;

        partial void OnDarkenGameBackgroundsChanged(bool value)
        {
            CurrentSettings.DarkenGameBackgrounds = value;
            SaveSettings();
        }

        partial void OnDrawLCDShadowsChanged(bool value)
        {
            CurrentSettings.DrawLCDShadows = value;
            SaveSettings();
        }

        private void SaveSettings()
        {
            using (var storage = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = storage.OpenFile(SETTINGS_FILE, FileMode.Create))
                {
                    JsonSerializer.Serialize(stream, CurrentSettings);
                }
            }
        }

        private void LoadSettings()
        {
            using (var storage = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists(SETTINGS_FILE))
                {
                    try
                    {
                        using (var stream = storage.OpenFile(SETTINGS_FILE, FileMode.Open))
                        {
                            CurrentSettings = JsonSerializer.Deserialize<Settings>(stream) ?? new Settings();
                        }
                    } 
                    catch
                    {
                        CurrentSettings = new Settings();
                    }
                    
                }
                else
                {
                    CurrentSettings = new Settings();
                }
            }
        }
    }
}
