// File: Utilities/BooleanToAdminConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace ToolManagementAppV2.Utilities.Converters
{
    public class BooleanToAdminConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool isAdmin)
                    return isAdmin ? "Admin" : string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool b)
                    return b;

                if (value is string s)
                    return string.Equals(s, "Admin", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return value is string or bool ? false : Binding.DoNothing;
        }
    }
}
