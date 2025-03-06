using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ToolManagementAppV2.Utilities
{
    public class NullToDefaultImageConverter : IValueConverter
    {
        private static BitmapImage _defaultImage;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    if (!path.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                    {
                        // Assume it's a file path.
                        return new BitmapImage(new Uri(path, UriKind.Absolute));
                    }
                    return new BitmapImage(new Uri(path, UriKind.Absolute));
                }
                catch { }
            }
            if (_defaultImage == null)
            {
                try
                {
                    // Use default image for user if parameter is "User"
                    string defaultFile = "DefaultUserPhoto.png";
                    if (parameter != null && parameter.ToString().Equals("Tool", StringComparison.OrdinalIgnoreCase))
                        defaultFile = "DefaultToolImage.png";
                    else if (parameter != null && parameter.ToString().Equals("Logo", StringComparison.OrdinalIgnoreCase))
                        defaultFile = "DefaultLogo.png";

                    var resourceUri = new Uri($"pack://application:,,,/Resources/{defaultFile}", UriKind.Absolute);
                    _defaultImage = new BitmapImage(resourceUri);
                }
                catch
                {
                    _defaultImage = new BitmapImage();
                }
            }
            return _defaultImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
