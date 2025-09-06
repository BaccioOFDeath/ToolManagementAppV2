// NameToInitialsConverter.cs
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeviceManagementApp.Utilities.Converters
{
    public class NameToInitialsConverter : IValueConverter
    {
        private readonly ILogger<NameToInitialsConverter> _logger;

        public NameToInitialsConverter() : this(null) { }

        public NameToInitialsConverter(ILogger<NameToInitialsConverter>? logger = null)
            => _logger = logger ?? NullLogger<NameToInitialsConverter>.Instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var name = (value as string)?.Trim();
                if (string.IsNullOrEmpty(name))
                    return string.Empty;

                var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return string.Empty;
                if (parts.Length == 1)
                    return parts[0][0].ToString().ToUpperInvariant();

                var first = parts[0][0];
                var last = parts[^1][0];
                return string.Concat(char.ToUpperInvariant(first), char.ToUpperInvariant(last));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert name to initials");
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
