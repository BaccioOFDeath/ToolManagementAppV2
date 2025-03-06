// File: Utilities/BooleanToAdminConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace ToolManagementAppV2.Utilities
{
    public class BooleanToAdminConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAdmin && isAdmin)
                return "Admin";
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
