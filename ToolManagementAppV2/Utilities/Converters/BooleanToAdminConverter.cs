// File: Utilities/BooleanToAdminConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Extensions.Logging;

namespace ToolManagementAppV2.Utilities.Converters
{
    public class BooleanToAdminConverter : IValueConverter
    {
        private static readonly ILogger Logger = App.LoggerFactory.CreateLogger<BooleanToAdminConverter>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool isAdmin)
                    return isAdmin ? "Admin" : string.Empty;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Convert failed");
            }
            return System.Windows.Data.Binding.DoNothing;
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
                Logger.LogError(ex, "ConvertBack failed");
            }
            return value is string or bool ? false : System.Windows.Data.Binding.DoNothing;
        }
    }
}
