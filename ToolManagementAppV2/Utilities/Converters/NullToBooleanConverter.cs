// NullToBooleanConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace ToolManagementAppV2.Utilities.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool b)
                    return b ? new object() : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Binding.DoNothing;
        }
    }
}
