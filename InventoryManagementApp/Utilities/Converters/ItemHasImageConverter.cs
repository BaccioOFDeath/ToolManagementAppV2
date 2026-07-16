using System;
using System.Globalization;
using System.Windows.Data;

namespace InventoryManagementApp.Utilities.Converters
{
    /// <summary>
    /// Reports whether an item-like model resolves to a real image rather than the placeholder.
    /// </summary>
    public sealed class ItemHasImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => NullToDefaultImageConverter.HasCustomImage(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
