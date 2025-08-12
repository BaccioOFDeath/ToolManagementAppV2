// NullToBooleanConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace ToolManagementAppV2.Utilities.Converters
{
    /// <summary>
    /// Converts <c>null</c> values to <c>false</c> and non-<c>null</c> values to
    /// <c>true</c>, typically for bindings like <c>IsEnabled</c> that expect a
    /// boolean.
    /// </summary>
    public class NullToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Returns <c>true</c> if <paramref name="value"/> is not <c>null</c>;
        /// otherwise returns <c>false</c>.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null;

        /// <summary>
        /// Converts a boolean back to an object placeholder when <c>true</c>, or
        /// <c>null</c> when <c>false</c>. Returns <see cref="Binding.DoNothing"/>
        /// for non-boolean inputs.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool b)
                    return b ? new object() : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Binding.DoNothing;
        }
    }
}
