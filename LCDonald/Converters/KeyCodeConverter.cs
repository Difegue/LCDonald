using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDonald.Converters
{
    public class KeyCodeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return ((int)value) switch
            {

                23 => "Left Arrow",
                24 => "Up Arrow",
                25 => "Right Arrow",
                26 => "Down Arrow",
                18 => "Space Bar",

                _ => "???"
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
