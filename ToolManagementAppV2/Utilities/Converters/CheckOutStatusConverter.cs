// File: Utilities/CheckOutStatusConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.Utilities.Converters
{
    public class CheckOutStatusConverter : IValueConverter
    {
        private readonly ILogger<CheckOutStatusConverter> _logger;

        public CheckOutStatusConverter() : this(null) { }

        public CheckOutStatusConverter(ILogger<CheckOutStatusConverter>? logger = null)
            => _logger = logger ?? NullLogger<CheckOutStatusConverter>.Instance;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isCheckedOut && isCheckedOut ? "Check In" : "Check Out";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string status)
                {
                    if (status == "Check In")
                        return true;
                    if (status == "Check Out")
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConvertBack failed");
            }
            return System.Windows.Data.Binding.DoNothing;
        }
    }
}
