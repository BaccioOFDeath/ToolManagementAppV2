// StringNotEmptyConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Binding = System.Windows.Data.Binding;

namespace InventoryManagementApp.Utilities.Converters
{
    /// <summary>
    /// Converts strings to <c>true</c> when they contain non-whitespace characters.
    /// </summary>
    public class StringNotEmptyConverter : IValueConverter
    {
        private readonly ILogger<StringNotEmptyConverter> _logger;

        public StringNotEmptyConverter() : this(null) { }

        public StringNotEmptyConverter(ILogger<StringNotEmptyConverter>? logger = null)
            => _logger = logger ?? NullLogger<StringNotEmptyConverter>.Instance;

        /// <summary>
        /// Returns <c>true</c> if <paramref name="value"/> is a non-empty string.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string s && !string.IsNullOrWhiteSpace(s);

        /// <summary>
        /// Converts a boolean back to either an empty string or <c>null</c>.
        /// Returns the original string if supplied.
        /// </summary>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

            return Binding.DoNothing;
        }
    }
}

