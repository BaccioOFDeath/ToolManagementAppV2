// NonEmptyStringToBoolConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace ToolManagementAppV2.Utilities.Converters
{
    public class NonEmptyStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            !string.IsNullOrEmpty(value as string);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool b)
                    return b ? string.Empty : null;

                if (value is string s)
                    return s;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Binding.DoNothing;
        }
    }
}
