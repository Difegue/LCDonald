using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using LCDonald.Core.Layout;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDonald.Converters
{
    public class ViewIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Games, Closeup: ZoomIn / FullScreenMaximize Back: RotateCamera, Manual: PreviewLink
            var symbol = Symbol.Emoji;
            
            if (value is MAMEView v)
            {
                if (v.Name.Contains("Front"))
                    symbol = Symbol.Games;

                if (v.Name.Contains("Closeup"))
                    symbol = Symbol.FullScreen;

                if (v.Name.Contains("Back"))
                    symbol = Symbol.RotateCamera;

                if (v.Name.Contains("Instruction"))
                    symbol = Symbol.PreviewLink;
            }

            return new SymbolIconSource { Symbol = symbol };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
