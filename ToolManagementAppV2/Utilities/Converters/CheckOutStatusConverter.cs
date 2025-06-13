// File: Utilities/CheckOutStatusConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace ToolManagementAppV2.Utilities.Converters
{
    public class CheckOutStatusConverter : IValueConverter
    {
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
                Console.WriteLine(ex);
            }
            return Binding.DoNothing;
        }
    }
}
