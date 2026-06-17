// ExistingFilePathToBoolConverter.cs
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.Helpers;
using Binding = System.Windows.Data.Binding;

namespace InventoryManagementApp.Utilities.Converters
{
    /// <summary>
    /// Returns <c>true</c> when the supplied string is a valid file path that exists.
    /// </summary>
    public class ExistingFilePathToBoolConverter : IValueConverter
    {
        private readonly ILogger<ExistingFilePathToBoolConverter> _logger;

        public ExistingFilePathToBoolConverter() : this(null) { }

        public ExistingFilePathToBoolConverter(ILogger<ExistingFilePathToBoolConverter>? logger = null)
            => _logger = logger ?? NullLogger<ExistingFilePathToBoolConverter>.Instance;

        /// <summary>
        /// Returns <c>true</c> if <paramref name="value"/> is a non-empty string and the file exists.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (Path.IsPathRooted(path))
                return File.Exists(path);

            var absPath = Helpers.PathHelper.GetAbsolutePath(path, false);
            return !string.IsNullOrWhiteSpace(absPath) && File.Exists(absPath);
        }

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

