using System;
using System.Globalization;
using System.Windows.Data;

namespace ToolManagementAppV2.Utilities.Converters
{
    public class ZeroToDisabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int qty && qty > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int i ? i : System.Windows.Data.Binding.DoNothing;
        }
    }
}
