﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using FluentAvalonia.Styling;
using Avalonia.Styling;

namespace LCDonald.ViewModels
{
    public class Settings
    {
        public bool DarkenGameBackgrounds { get; set; } = true;
        public bool MuteSound { get; set; }
        public string ApplicationTheme { get; set; } = "System";
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
            _muteSound = CurrentSettings.MuteSound;
            _applicationTheme = CurrentSettings.ApplicationTheme switch
            {
                "System" => 0,
                "Light" => 1,
                "Dark" => 2,
                _ => 0
            };
        }

        [ObservableProperty]
        private bool _darkenGameBackgrounds;
        
        [ObservableProperty]
        private bool _muteSound;

        [ObservableProperty]
        private int _applicationTheme;

        [ObservableProperty]
        private bool _drawLCDShadows;

        partial void OnDarkenGameBackgroundsChanged(bool value)
        {
            CurrentSettings.DarkenGameBackgrounds = value;
            SaveSettings();
        }

        partial void OnMuteSoundChanged(bool value)
        {
            CurrentSettings.MuteSound = value;
            SaveSettings();
        }

        partial void OnApplicationThemeChanged(int value)
        {
            CurrentSettings.ApplicationTheme = value switch
            {
                0 => "Default",
                1 => "Light",
                2 => "Dark",
                _ => "Default",
            };
        
            SaveSettings();

            var thm = Application.Current?.Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault();

            if (CurrentSettings.ApplicationTheme == "System" || CurrentSettings.ApplicationTheme == "Default")
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Default;
                thm.PreferSystemTheme = true;
            }
            else
            {
                Application.Current.RequestedThemeVariant = CurrentSettings.ApplicationTheme == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;
            }

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
