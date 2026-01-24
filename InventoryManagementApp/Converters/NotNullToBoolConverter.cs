using System;
using System.Globalization;
using System.Windows.Data;

namespace InventoryManagementApp.Converters
{
	/// <summary>
	/// Converts non-null values to true and null to false for bindings.
	/// </summary>
	public class NotNullToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value != null;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
				return b ? new object() : null;
			return null;
		}
	}
}
