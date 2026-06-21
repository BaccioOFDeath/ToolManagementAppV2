using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InventoryManagementApp.Utilities.Converters
{
    public class HexColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string text || string.IsNullOrWhiteSpace(text))
                return null;

            try
            {
                return ColorConverter.ConvertFromString(NormalizeHex(text));
            }
            catch (FormatException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                Color color => ToHex(color),
                _ => Binding.DoNothing
            };
        }

        private static string NormalizeHex(string text)
        {
            var hex = text.Trim();
            return hex.StartsWith("#", StringComparison.Ordinal) ? hex : $"#{hex}";
        }

        private static string ToHex(Color color)
        {
            return color.A == byte.MaxValue
                ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
                : $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
