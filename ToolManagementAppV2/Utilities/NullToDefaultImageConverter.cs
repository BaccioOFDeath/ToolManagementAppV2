// File: Utilities/NullToDefaultImageConverter.cs
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
                    if (!path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                        path = "file:///" + path.Replace("\\", "/");
                    return new BitmapImage(new Uri(path, UriKind.Absolute));
                }
                catch { }
            }
            if (_defaultImage == null)
            {
                try
                {
                    var resourceUri = new Uri("pack://application:,,,/Resources/DefaultToolImage.png", UriKind.Absolute);
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
