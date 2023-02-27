using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ExCSS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDonald.Converters
{
    public class GameBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Completely arbitrary background assignment
            var url = value?.ToString() switch
            {
                "abanana" => "avares://LCDonald/Assets/Backgrounds/2003_series.jpg",
                "ksoccer" => "avares://LCDonald/Assets/Backgrounds/2003_series.jpg",
                "sagame" => "avares://LCDonald/Assets/Backgrounds/2003_series.jpg",
                "tskypatrol" => "avares://LCDonald/Assets/Backgrounds/2003_series.jpg",
                "sgrinder" => "avares://LCDonald/Assets/Backgrounds/2003_series.jpg",
                "sspeedway" => "avares://LCDonald/Assets/Backgrounds/2003_series.jpg",
                
                "artennis" => "avares://LCDonald/Assets/Backgrounds/2004_series.jpg",
                "cflower" => "avares://LCDonald/Assets/Backgrounds/2004_series.jpg",
                "kbaseball" => "avares://LCDonald/Assets/Backgrounds/2004_series.jpg",
                "sbasketball" => "avares://LCDonald/Assets/Backgrounds/2004_series.jpg",
                "sskateboard" => "avares://LCDonald/Assets/Backgrounds/2004_series.jpg",
                "tsoccer" => "avares://LCDonald/Assets/Backgrounds/2004_series.jpg",
                
                "arvolleyball" => "avares://LCDonald/Assets/Backgrounds/2005_series.jpg",
                "shockey" => "avares://LCDonald/Assets/Backgrounds/2005_series.jpg",
                "cflower2" => "avares://LCDonald/Assets/Backgrounds/2005_series.jpg",
                "tskyadventure" => "avares://LCDonald/Assets/Backgrounds/2005_series.jpg",
                
                "bfishing" => "avares://LCDonald/Assets/Backgrounds/2005_series_2.jpg",
                "bgiantegg" => "avares://LCDonald/Assets/Backgrounds/2005_series_2.jpg",
                "ktreasure" => "avares://LCDonald/Assets/Backgrounds/2005_series_2.jpg",
                "sxtremeboard" => "avares://LCDonald/Assets/Backgrounds/2005_series_2.jpg",
                
                _ => "avares://LCDonald/Assets/Backgrounds/2003_series.jpg",
            };

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var asset = assets.Open(new Uri(url));
            
            return new Bitmap(asset);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
