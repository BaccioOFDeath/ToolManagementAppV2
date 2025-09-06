// File: Utilities/BooleanToAdminConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeviceManagementApp.Utilities.Converters
{
    public class BooleanToAdminConverter : IValueConverter
    {
        private readonly ILogger<BooleanToAdminConverter> _logger;

        public BooleanToAdminConverter() : this(null) { }

        public BooleanToAdminConverter(ILogger<BooleanToAdminConverter>? logger = null)
            => _logger = logger ?? NullLogger<BooleanToAdminConverter>.Instance;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool isAdmin)
                    return isAdmin ? "Admin" : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Convert failed");
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
                _logger.LogError(ex, "ConvertBack failed");
            }
            return value is string or bool ? false : System.Windows.Data.Binding.DoNothing;
        }
    }
}
