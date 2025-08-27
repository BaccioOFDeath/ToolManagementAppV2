// NonEmptyStringToBoolConverter.cs
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Utilities.Converters
{
    public class NonEmptyStringToBoolConverter : IValueConverter
    {
        private readonly ILogger<NonEmptyStringToBoolConverter> _logger;

        public NonEmptyStringToBoolConverter() : this(null) { }

        public NonEmptyStringToBoolConverter(ILogger<NonEmptyStringToBoolConverter>? logger = null)
            => _logger = logger ?? NullLogger<NonEmptyStringToBoolConverter>.Instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

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
                _logger.LogError(ex, "ConvertBack failed");
            }
            return System.Windows.Data.Binding.DoNothing;
        }
    }
}
