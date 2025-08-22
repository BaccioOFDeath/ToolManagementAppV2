using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Utilities.Converters
{
    public class DateTimeMinToEmptyStringConverter : IValueConverter
    {
        private readonly ILogger<DateTimeMinToEmptyStringConverter> _logger;

        public DateTimeMinToEmptyStringConverter() : this(null) { }

        public DateTimeMinToEmptyStringConverter(ILogger<DateTimeMinToEmptyStringConverter>? logger = null)
            => _logger = logger ?? NullLogger<DateTimeMinToEmptyStringConverter>.Instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is DateTime ndt)
                    return ndt == DateTime.MinValue ? null : ndt;

                if (value == null)
                    return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Convert failed");
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is DateTime dt)
                    return dt;

                if (value is string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return DateTime.MinValue;
                    if (DateTime.TryParse(s, culture, DateTimeStyles.None, out var parsed))
                        return parsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConvertBack failed");
            }

            return Binding.DoNothing;
        }
    }
}
